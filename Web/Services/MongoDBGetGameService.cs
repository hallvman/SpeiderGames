using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using SpeiderGames.Models;

public interface IGameService
{
    Game GetGameByGameCode(string gameCode); 
    bool UpdatePoints(string gameName, string teamName, string postName, string postPin, int points);
    List<Team> GetTeamsForGame(string selectedGame);
    List<Post> GetPostsForGame(string selectedGame);

    List<Game> GetGames();
    bool ValidateGameCode(string gameCode);
    
    string GetPostPinForPostName(string gameCode, string postName);
    List<Log> GetDataFromMongoDB(string selectedGame);
    bool UpdatePointsInLogs(string gameName, string teamName, string postName, int points, bool updateByAdmin);
}

public class MongoDBGetGameService : IGameService
{
    private readonly IMongoCollection<Game> _gameCollection;
    private readonly IMongoCollection<UpdatePointsViewModel> _gamesCollection;
    private readonly MongoDbContext _dbContext;

    public MongoDBGetGameService(IMongoDatabase database)
    {
        _gameCollection = database.GetCollection<Game>("Games");
        _gamesCollection = database.GetCollection<UpdatePointsViewModel>("Games");

        // Create an index on the GameName field
        var indexKeys = Builders<Game>.IndexKeys.Ascending(g => g.GameName);
        var indexModel = new CreateIndexModel<Game>(indexKeys);
        _gameCollection.Indexes.CreateOne(indexModel);
    }
    
    public List<Log> GetDataFromMongoDB(string selectedGame)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, selectedGame);
        var projection = Builders<Game>.Projection.Include(g => g.Logs);

        var logs = _gameCollection.Find(filter).Project<Game>(projection).FirstOrDefault()?.Logs;

        return logs ?? new List<Log>();
    }
    
    public List<Team> GetTeamsForGame(string selectedGame)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, selectedGame);
        var projection = Builders<Game>.Projection.Include(g => g.Teams);

        var teams = _gameCollection.Find(filter).Project<Game>(projection).FirstOrDefault()?.Teams;

        return teams ?? new List<Team>();
    }

    public List<Post> GetPostsForGame(string selectedGame)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, selectedGame);
        var projection = Builders<Game>.Projection.Include(g => g.Posts);

        var posts = _gameCollection.Find(filter).Project<Game>(projection).FirstOrDefault()?.Posts;

        return posts ?? new List<Post>();
    }

    public Game GetGameByGameCode(string gameCode)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameCode, gameCode);
        var game = _gameCollection.Find(filter).FirstOrDefault();

        if (game == null)
        {
            return null;
        }

        return game;
    }
    
    public List<Game> GetGames()
    {
        try
        {
            var games = _gameCollection.Find(_ => true).ToList();
            return games;
        }
        catch (Exception ex)
        {
            // Handle the exception (log, throw, etc.)
            Console.WriteLine($"Error while fetching games: {ex.Message}");
            return new List<Game>();
        }
    }
    
    public bool UpdatePoints(string gameName, string teamName, string postName, string postPin, int points)
    {
        // No need to parse index from postName for filtering purposes.
        var filter = Builders<Game>.Filter.And(
            Builders<Game>.Filter.Eq(g => g.GameName, gameName),
            Builders<Game>.Filter.ElemMatch(g => g.Teams, team => team.TeamName == teamName)
        );

        var update = Builders<Game>.Update.Set($"Teams.$[team].Posts.$[post].PostPoints", points);
    
        // Use array filters to identify the correct team and post.
        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("team.TeamName", teamName)),
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument { { "post.PostName", postName }, { "post.PostPin", postPin } })
        };

        var options = new UpdateOptions { ArrayFilters = arrayFilters };

        var updateResult = _gameCollection.UpdateOne(filter, update, options);
   
        return updateResult.ModifiedCount > 0;
    }
    
    public bool UpdatePointsInLogs(string gameName, string teamName, string postName, int points, bool updateByAdmin)
    {
        DateTime now = DateTime.Now;
        var newLog = new Log
        {
            RequestDate = now,
            TeamName = teamName,
            PostName = postName,
            Points = points,
            UpdateByAdmin = updateByAdmin
        };

        // Define the update operation to add the new team
        var update = Builders<Game>.Update.Push(g => g.Logs, newLog);
        // Create a filter to find the game by its code
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, gameName);

        // Perform the update operation
        var result = _gameCollection.UpdateOne(filter, update);

        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    
    public string GetPostPinForPostName(string gameCode, string postName)
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("GameCode", gameCode)),
            new BsonDocument("$unwind", new BsonDocument("path", "$Teams")),
            new BsonDocument("$unwind", new BsonDocument("path", "$Teams.Posts")),
            new BsonDocument("$match", new BsonDocument("Teams.Posts.PostName", postName)),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "PostPin", "$Teams.Posts.PostPin" }
            }),
            new BsonDocument("$limit", 1)
        };
        
        string postPin = null;
    
        // Using async/await pattern correctly
        var cursor = _gamesCollection.AggregateAsync<BsonDocument>(pipeline).GetAwaiter().GetResult();
        while (cursor.MoveNextAsync().GetAwaiter().GetResult()) // Synchronously waiting on the async operation
        {
            foreach (var doc in cursor.Current)
            {
                postPin = doc["PostPin"].AsString;
                break; // Assuming uniqueness, breaking after the first match
            }
        }

        return postPin;
    }
    
    public bool ValidateGameCode(string gameCode)
    {
        // Retrieve the game based on selectedGame (ID or name)
        var game = GetGameByGameCode(gameCode);// logic to get game from database

        // Check if the game's code matches the provided code
        return game != null && game.GameCode.Equals(gameCode, StringComparison.OrdinalIgnoreCase);
    }
}