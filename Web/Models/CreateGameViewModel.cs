using MongoDB.Bson;

namespace SpeiderGames.Models
{
    public class Post
    {
        public string? Name { get; set; }
        public int Points { get; set; }
    }

    public class Team
    {
        public string? Name { get; set; }
        public int Points { get; set; }
    }

    public class Game
    {
        public ObjectId Id { get; set; }
        public string? GameName { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfTeams { get; set; }

        public List<Post>? Posts { get; set; }
        public List<Team>? Teams { get; set; }
    }
}