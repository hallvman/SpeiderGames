using SpeiderGames.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Configuration.AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<MongoDBSettingsModel>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddOptions();

// Register MongoClient and IMongoDatabase
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettingsModel>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBSettingsModel>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<IGameService, MongoDBGetGameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=StartApp}/{action=Index}/{id?}");

app.Run();