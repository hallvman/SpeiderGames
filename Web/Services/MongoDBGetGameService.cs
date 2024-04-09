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
}

public class MongoDBGetGameService : IGameService
{
    private readonly IMongoCollection<Game> _gameCollection;
    private readonly IMongoCollection<UpdatePointsViewModel> _gamesCollection;

    public MongoDBGetGameService(IMongoDatabase database)
    {
        _gameCollection = database.GetCollection<Game>("Games");
        _gamesCollection = database.GetCollection<UpdatePointsViewModel>("Games");

        // Create an index on the GameName field
        var indexKeys = Builders<Game>.IndexKeys.Ascending(g => g.GameName);
        var indexModel = new CreateIndexModel<Game>(indexKeys);
        _gameCollection.Indexes.CreateOne(indexModel);
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
        var index = 0;

        // Remove the "PostName" prefix
        string postNumberString = postName.Replace("Post", "");

        int.TryParse(postNumberString, out index);
        
        var filter = Builders<Game>.Filter.And(
            Builders<Game>.Filter.Eq(g => g.GameName, gameName),
            Builders<Game>.Filter.ElemMatch(g => g.Teams, team => team.TeamName == teamName),
            Builders<Game>.Filter.ElemMatch(g => g.Teams[index-1].Posts, post => post.PostName == postName && post.PostPin == postPin)
        );

        var update = Builders<Game>.Update.Set($"Teams.$[team].Posts.$[post].PostPoints", points);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(BsonDocument.Parse($"{{ 'team.TeamName': '{teamName}' }}")),
            new BsonDocumentArrayFilterDefinition<BsonDocument>(BsonDocument.Parse($"{{ 'post.PostName': '{postName}' }}"))
        };

        var options = new UpdateOptions { ArrayFilters = arrayFilters };

        var updateResult = _gameCollection.UpdateOne(filter, update, options);

        return updateResult.ModifiedCount > 0;
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