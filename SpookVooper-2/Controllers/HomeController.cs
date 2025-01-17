﻿using Microsoft.AspNetCore.Mvc;
using SV2.Helpers;
using SV2.Models;
using System.Diagnostics;

namespace SV2.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : SVController {
    private readonly ILogger<HomeController> _logger;
    
    [TempData]
    public string StatusMessage { get; set; }

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}