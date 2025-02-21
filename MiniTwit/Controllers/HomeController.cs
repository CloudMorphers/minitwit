using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Data;
using MiniTwit.Models;

namespace MiniTwit.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Timeline()
    {
        var allMessages = await _context.Messages.Include(message=>message.Author).ToListAsync();
        var messageViewModels = allMessages.Select(message => new MessageViewModel
        {
            Email = message.Author!.Email!,
            Username = message.Author!.UserName!,
            Text = message.Text,
            PublishDate = message.PublishDate
        }).ToList();
        var timelineViewModel = new TimelineViewModel
        {
            Messages = messageViewModels,
        };
        return View(timelineViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Login()
    {
        return Redirect("/Identity/Account/Login");
    }

    public IActionResult Register()
    {
        return Redirect("/Identity/Account/Register");
    }
}
