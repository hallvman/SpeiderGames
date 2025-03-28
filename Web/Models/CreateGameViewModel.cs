using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SpeiderGames.Models
{
    [Serializable]
    public class Post
    {
        public string? PostName { get; set; }
        public string? Description {get; set;}
        public string? PostPin { get; set; }
        public double? PostPoints { get; set; }
    }
    [Serializable]
    public class Team
    {
        public string? TeamName { get; set; }
        public List<Post>? Posts { get; set; }
    }

    [Serializable]
    public class Game
    {
        public ObjectId Id { get; set; }
        public string? GameName { get; set; }
        public string? FullPhoneNumber { get; set; }
        public string? GameCode { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfTeams { get; set; }

        public List<Post>? Posts { get; set; }
        public List<Team>? Teams { get; set; }
        
        public List<Log>? Logs { get; set; }
    }

    public class Log
    {
        public DateTime RequestDate { get; set; }
        public string? TeamName { get; set; }
        public string? PostName { get; set; }
        public double Points { get; set; }
        public bool UpdateByAdmin { get; set; }
    }
    
    public class UpdatePointsViewModel
    {
        public SelectList? Games { get; set; }
        public SelectList? Teams { get; set; }
        public SelectList? Posts { get; set; }
        
        public Dictionary<string, string> PostDescriptions { get; set; } = new Dictionary<string, string>();
        public bool LogUpdate { get; set; }
        public string? GameName { get; set; }
        public string? GameCode { get; set; }
        public string? TeamName { get; set; }
        public string? PostName { get; set; }
        public string? PostPin { get; set; }
        public double Points { get; set; }
    }
}