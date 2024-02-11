using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using MongoDB.Bson;
using SpeiderGames.Models;

public interface IGameService
{
    Game GetGameByName(string gameId);
    bool UpdatePoints(string gameName, string teamName, string postName, int points);
    List<Team> GetTeamsForGame(string selectedGame);
    List<Post> GetPostsForGame(string selectedGame);

    List<Game> GetGames();
    bool ValidateGameCode(string selectedGame, string gameCode);
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

    public Game GetGameByName(string gameName)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, gameName);
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
    
    public bool UpdatePoints(string gameName, string teamName, string postName, int points)
    {
        var index = 0;

        // Remove the "PostName" prefix
        string postNumberString = postName.Replace("Post", "");

        int.TryParse(postNumberString, out index);
        
        var filter = Builders<Game>.Filter.And(
            Builders<Game>.Filter.Eq(g => g.GameName, gameName),
            Builders<Game>.Filter.ElemMatch(g => g.Teams, team => team.TeamName == teamName),
            Builders<Game>.Filter.ElemMatch(g => g.Teams[index-1].Posts, post => post.PostName == postName)
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
    
    public bool ValidateGameCode(string selectedGame, string gameCode)
    {
        // Retrieve the game based on selectedGame (ID or name)
        var game = GetGameByName(selectedGame);// logic to get game from database

        // Check if the game's code matches the provided code
        return game != null && game.GameCode.Equals(gameCode, StringComparison.OrdinalIgnoreCase);
    }
    
    public (List<Team> Teams, List<Post> Posts) GetTeamsAndPosts(string gameName)
    {
        var filter = Builders<Game>.Filter.Eq(g => g.GameName, gameName);
        var game = _gameCollection.Find(filter).FirstOrDefault();

        if (game != null)
        {
            return (game.Teams, game.Posts);
        }

        return (new List<Team>(), new List<Post>());
    }
}