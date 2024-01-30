﻿// MongoDbContext.cs
using MongoDB.Driver;
using SpeiderGames.Models;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string? connectionString, string? databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<GameViewModel> Games => _database.GetCollection<GameViewModel>("Games");
}