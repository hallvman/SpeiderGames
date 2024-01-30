using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace SpeiderGames.Models
{
    public class GameViewModel
    {
        public ObjectId Id { get; set; }
        
        [Required(ErrorMessage = "Game Name is required")]
        public string GameName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of Posts/Questions must be greater than 0")]
        public int NumberOfPosts { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of Teams must be greater than 0")]
        public int NumberOfTeams { get; set; }
    }
}