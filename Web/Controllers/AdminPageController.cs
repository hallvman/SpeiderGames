using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeiderGames.Models;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace SpeiderGames.Controllers
{
    public class AdminPageController : Controller
    {
        private readonly ILogger<AdminPageController> _logger;
        private readonly IGameService _gameService;
        private readonly MongoDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IGameController _gameController;

        public AdminPageController(ILogger<AdminPageController> logger, IGameService gameService, IConfiguration configuration, IGameController gameController)
        {
            _logger = logger;
            _gameService = gameService;
            _gameController = gameController;
            _configuration = configuration;
            string dbName = _configuration["MongoDBSettings:DatabaseName"];
            string connectionString = _configuration["MongoDBSettings:ConnectionString"];
            _dbContext = new MongoDbContext(connectionString, dbName);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue("AdminAccessGranted", out string accessGranted) && accessGranted == "true")
            {
                Request.Cookies.TryGetValue("GameCode", out string? gameCode);
                var game = _gameService.GetGameByGameCode(gameCode);
                bool isValidGamecode = _gameService.ValidateGameCode(gameCode);

                if (isValidGamecode)
                {
                    var posts = _gameService.GetPostsForGame(game.GameName);
                    ViewData["LogoutType"] = "AdminAccessGranted";
                    return View("/Views/AdminPage/Index.cshtml", game);
                }
                return View("/Views/AdminPage/AdminPage.cshtml");
            }
            return View("/Views/AdminPage/AdminPage.cshtml");
        }
        
        [HttpPost]
        public IActionResult Index(string gameCode)
        {
            var game = _gameService.GetGameByGameCode(gameCode);
            bool isValidGamecode = _gameService.ValidateGameCode(gameCode);
            
            if (isValidGamecode)
            {
                // Create a cookie with a long expiration time after successful game code verification
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7), // Expires in 7 days
                    HttpOnly = true, // Enhances security
                    Secure = true // Ensures cookie is sent over HTTPS
                };

                // Set a cookie indicating admin access is granted
                Response.Cookies.Append("AdminAccessGranted", "true", cookieOptions);
                Response.Cookies.Append("GameCode", gameCode, cookieOptions);
                ViewData["LogoutType"] = "AdminAccessGranted";
                return View("/Views/AdminPage/Index.cshtml", game);
            }
            
            return View("Error");
        }

        [HttpPost]
        public ActionResult SeePostCodes(Game game)
        {
            var postList = _gameService.GetPostsForGame(game.GameName);
            
            var model = new Game
            {
                GameName = game.GameName,
                Posts = postList
            };
            ViewData["LogoutType"] = "AdminAccessGranted";
            return View("AdminCodePage", model);
        }
        
        [HttpPost]
        public ActionResult CreatePdf(string gameName)
        {
            var data = _gameService.GetDataFromMongoDB(gameName);
            GeneratePdf(data);

            var now = DateTime.Now.ToString();

            // Optionally return the PDF file as a download
            return File(System.IO.File.ReadAllBytes("output.pdf"), "application/pdf", $"{now}_logg.pdf");
        }
        
        public void GeneratePdf(List<Log> data)
        {
            using (var stream = new FileStream("output.pdf", FileMode.Create))
            {
                var document = new Document();
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Adding a title
                var titleFont = new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD);
                document.Add(new Paragraph("Innsendte Poeng", titleFont));
                document.Add(new Paragraph($"Generert den: {DateTime.Now.ToString()}"));
                document.Add(new Paragraph("Dette er en oversikt over alle innsendte poeng:\n\n"));

                // Adding a table
                PdfPTable table = new PdfPTable(5);
                table.AddCell("Tidspunkt:");
                table.AddCell("Lag:");
                table.AddCell("Post:");
                table.AddCell("Poeng:");
                table.AddCell("Oppdatert av admin:");
                
                foreach (var item in data)
                {
                    string requestDate = item.RequestDate.ToString();
                    table.AddCell(requestDate); // Current time for each entry
                    table.AddCell(item.TeamName);
                    table.AddCell(item.PostName);
                    table.AddCell(item.Points.ToString());
                    string boolString = "";
                    if (item.UpdateByAdmin)
                    {
                        boolString = "Ja";
                    }
                    else
                    {
                        boolString = "Nei";
                    }
                    table.AddCell(boolString); 
                }

                document.Add(table);
                document.Close();
            }
        }
        
        [HttpPost]
        public ActionResult ChangeTeamPoints(UpdatePointsViewModel model)
        {
            bool isValidGamecode = _gameService.ValidateGameCode(model.GameCode);
            
            if (!isValidGamecode)
            {
                return View("Error");
            }
            // Perform any necessary logic based on the selected game
            // For example, you might want to fetch Teams and Posts for the selected game
            var teams = _gameService.GetTeamsForGame(model.GameName);
            var posts = _gameService.GetPostsForGame(model.GameName);
            
            // This assumes that you have properties Teams and Posts in your UpdatePointsViewModel
            model.Teams = new SelectList(teams, "TeamName", "TeamName");
            model.Posts = new SelectList(posts, "PostName", "PostName");
            ViewData["LogoutType"] = "AdminAccessGranted";
            return View("AdminChangePoints", model);
        }
        
        [HttpPost]
        public ActionResult AdminUpdatePoints(UpdatePointsViewModel model)
        {
            var postPin = _gameService.GetPostPinForPostName(model.GameCode, model.PostName);
            
            var updated = _gameService.UpdatePoints(model.GameName, model.TeamName, model.PostName, postPin, model.Points);

            if (updated)
            {
                var adminUpdated = _gameService.UpdatePointsInLogs(model.GameName, model.TeamName, model.PostName, model.Points, true);
                var updateComplete = new UpdatePointsViewModel
                {
                    GameName = model.GameName,
                    TeamName = model.TeamName,
                    PostName = model.PostName,
                    Points = model.Points,
                    LogUpdate = adminUpdated,
                };
                return RedirectToAction("Index", "AdminPage", updateComplete);
            }
            else
            {
                return View("Error");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> AddPost(string gameCode)
        {
            string postPin = _gameController.GeneratePostPin();
            var game = _gameService.GetGameByGameCode(gameCode);
            int postNumber = game.Posts != null ? game.Posts.Count : 0;
            string postName = $"Post{postNumber + 1}"; // +1 because the count is 0-based, but we want to start naming from 1
            var filter = Builders<Game>.Filter.Eq(g => g.GameCode, gameCode);
            var newPost = new Post { PostName = postName, PostPin = postPin, PostPoints = 0 };
            var update = Builders<Game>.Update.Push<Post>("Posts", newPost);
            
            try
            {
                await _dbContext.Games.UpdateOneAsync(filter, update);
                
                // Assuming you have logic to update the NumberOfPosts in the Game
                var updateNumberOfPosts = Builders<Game>.Update.Inc(g => g.NumberOfPosts, 1);
                await _dbContext.Games.UpdateOneAsync(filter, updateNumberOfPosts);
                
                // Assuming the structure of your game document includes an array of teams
                foreach (var team in game.Teams) // Assuming you have a list of teams in your game object
                {
                    // Construct a filter to target the specific team within the specific game.
                    // This assumes teams are uniquely identifiable within the game by a TeamId or similar.
                    var teamFilter = Builders<Game>.Filter.And(
                        Builders<Game>.Filter.Eq(g => g.GameCode, gameCode),
                        Builders<Game>.Filter.ElemMatch(g => g.Teams, t => t.TeamName == team.TeamName));

                    // Define the update operation to add the new post to the team.
                    // Adjust "Teams.$.Posts" according to your actual document structure.
                    var teamUpdate = Builders<Game>.Update.Push($"Teams.$.Posts", newPost);

                    // Perform the update operation.
                    await _dbContext.Games.UpdateOneAsync(teamFilter, teamUpdate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            var updatedGame = _gameService.GetGameByGameCode(gameCode);
            
            return View("/Views/AdminPage/Index.cshtml", updatedGame);
        }
        
        [HttpPost]
        public async Task<ActionResult> DeletePost(string gameCode)
        {
            // Retrieve the game document
            var game = _gameService.GetGameByGameCode(gameCode);
            if (game != null && game.Posts != null && game.Posts.Count > 0)
            {
                // Remove the last post from the list
                game.Posts.RemoveAt(game.Posts.Count - 1);
        
                // Update the document with the modified posts list
                var update = Builders<Game>.Update.Set("Posts", game.Posts);
                await _dbContext.Games.UpdateOneAsync(Builders<Game>.Filter.Eq(g => g.GameCode, game.GameCode), update);

                // Assuming you have logic to update the NumberOfPosts in the Game
                var updateNumberOfPosts = Builders<Game>.Update.Inc(g => g.NumberOfPosts, -1);
                await _dbContext.Games.UpdateOneAsync(Builders<Game>.Filter.Eq(g => g.GameCode, game.GameCode), updateNumberOfPosts);
                
                // Now, iterate through each team to remove the last post from their list
                foreach (var team in game.Teams)
                {
                    if (team.Posts != null && team.Posts.Count > 0)
                    {
                        // Assuming the structure allows direct manipulation, remove the last post
                        team.Posts.RemoveAt(team.Posts.Count - 1);

                        // Update each team document individually
                        // This assumes there's a direct way to reference and update a specific team's Posts within a Game document, adjust accordingly
                        var teamUpdate = Builders<Game>.Update.Set($"Teams.$.Posts", team.Posts);
                        var teamFilter = Builders<Game>.Filter.And(
                            Builders<Game>.Filter.Eq("GameCode", game.GameCode),
                            Builders<Game>.Filter.ElemMatch(g => g.Teams, t => t.TeamName == team.TeamName)
                        );

                        await _dbContext.Games.UpdateOneAsync(teamFilter, teamUpdate);
                    }
                }
            }
            var updatedGame = _gameService.GetGameByGameCode(gameCode);
            
            return View("/Views/AdminPage/Index.cshtml", updatedGame);
        }
        
        [HttpPost]
        public async Task<ActionResult> DeleteTeam(string gameCode, string teamName)
        {
            // Retrieve the game document
            var game = _gameService.GetGameByGameCode(gameCode);
            
            // Find the team by name.
            var teamToRemove = game.Teams.FirstOrDefault(t => t.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase));
            
            // Remove the team from the game's team list.
            game.Teams.Remove(teamToRemove);

            // Update the game document in the database.
            var filter = Builders<Game>.Filter.Eq(g => g.GameCode, gameCode);
            var update = Builders<Game>.Update.Set(g => g.Teams, game.Teams);
            await _dbContext.Games.UpdateOneAsync(filter, update);
            
            var updatedGame = _gameService.GetGameByGameCode(gameCode);
            
            return View("/Views/AdminPage/Index.cshtml", updatedGame);
        }
        
        [HttpPost]
        public async Task<ActionResult> AddTeam(string gameCode, string teamName)
        {
            // Retrieve the game document
            var game = _gameService.GetGameByGameCode(gameCode);
            
            // Initialize the Posts list for the new team with a default post for each existing game post
            var newTeamPosts = _gameService.GetPostsForGame(game.GameName);
            
            // Create a new team object
            var newTeam = new Team
            {
                TeamName = teamName,
                Posts = newTeamPosts
            };

            // Define the update operation to add the new team
            var update = Builders<Game>.Update.Push(g => g.Teams, newTeam);
            // Create a filter to find the game by its code
            var filter = Builders<Game>.Filter.Eq(g => g.GameCode, gameCode);

            // Perform the update operation
            await _dbContext.Games.UpdateOneAsync(filter, update);
            
            var updatedGame = _gameService.GetGameByGameCode(gameCode);
            
            return View("/Views/AdminPage/Index.cshtml", updatedGame);
        }

        [HttpPost]
        public async Task<ActionResult> AddDescriptionForPost(string gameCode, string postName, string description)
        {
            var game = _gameService.ValidateGameCode(gameCode);

            var filter = Builders<Game>.Filter.And(
                Builders<Game>.Filter.Eq(g => g.GameCode, gameCode),
                Builders<Game>.Filter.ElemMatch(g => g.Posts, Builders<Post>.Filter.Eq(p => p.PostName, postName))
            );
            
            var indexPost = Regex.Replace(postName, "Post", "", RegexOptions.IgnoreCase);
            int.TryParse(indexPost, out int index);
            
            var update = Builders<Game>.Update.Set(g => g.Posts[index-1].Description, description);
            
            await _dbContext.Games.UpdateOneAsync(filter, update);
            
            var updatedGame = _gameService.GetGameByGameCode(gameCode);
            
            return View("/Views/AdminPage/Index.cshtml", updatedGame);
        }
    }
}