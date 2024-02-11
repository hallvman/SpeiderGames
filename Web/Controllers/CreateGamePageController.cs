﻿using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using System.Diagnostics;

namespace SpeiderGames.Controllers
{
    public class CreateGamePageController : Controller
    {
        private readonly ILogger<CreateGamePageController> _logger;

        public CreateGamePageController(ILogger<CreateGamePageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/Pages/CreateGamePage.cshtml");
        }
    }
}