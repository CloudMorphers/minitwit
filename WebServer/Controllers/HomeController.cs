using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Data;
using MiniTwit.Models;

namespace MiniTwit.Controllers;

public class HomeController : Controller
{
    private const int ItemsPerPage = 30;

    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public HomeController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(PublicTimeline));
        }
        var signedInUser = await _userManager.GetUserAsync(User);
        var followedUserIds = await _context
            .UserFollows.Where(userFollow => userFollow.FollowerId == signedInUser!.Id)
            .Select(userFollow => userFollow.FollowingId)
            .ToListAsync();
        var messages = await GetMessagesAsync(
            1,
            message => message.AuthorId == signedInUser!.Id || followedUserIds.Contains(message.AuthorId)
        );
        var model = new MyTimelineViewModel { Messages = messages };
        ViewData["Title"] = "My Timeline";
        return View("MyTimeline", model);
    }

    [HttpGet("public")]
    public async Task<IActionResult> PublicTimeline()
    {
        var messages = await GetMessagesAsync(1);
        var model = new TimelineViewModel { Messages = messages };
        ViewData["Title"] = "Public Timeline";
        return View("Timeline", model);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> UserTimeline(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        var messages = await GetMessagesAsync(1, message => message.AuthorId == user.Id);
        var followsUser = false;
        if (_signInManager.IsSignedIn(User))
        {
            var signedInUser = await _userManager.GetUserAsync(User);
            followsUser = await _context.UserFollows.AnyAsync(userFollow =>
                userFollow.FollowerId == signedInUser!.Id && userFollow.FollowingId == user.Id
            );
        }
        var model = new TimelineViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            FollowsUser = followsUser,
            Messages = messages,
        };
        ViewData["Title"] = $"{user.UserName}'s Timeline";
        return View("Timeline", model);
    }

    [HttpPost("add_message")]
    public async Task<IActionResult> PostMessage(MyTimelineViewModel model)
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(AuthController.Login), "Auth");
        }
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState
                .Values.SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault();
            TempData["Error"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }
        var signedInUser = await _userManager.GetUserAsync(User);
        var message = new Message
        {
            Text = model.Text,
            PublishDate = DateTime.UtcNow,
            AuthorId = signedInUser!.Id,
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Your message was recorded";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("follow/{username}")]
    public async Task<IActionResult> FollowUser(string username)
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(AuthController.Login), "Auth");
        }
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        var signedInUser = await _userManager.GetUserAsync(User);
        var userFollow = new UserFollow { FollowerId = signedInUser!.Id, FollowingId = user.Id };
        _context.UserFollows.Add(userFollow);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"You are now following {username}";
        return RedirectToAction(nameof(UserTimeline), new { username });
    }

    [HttpGet("unfollow/{username}")]
    public async Task<IActionResult> UnfollowUser(string username)
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(AuthController.Login), "Auth");
        }
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        var signedInUser = await _userManager.GetUserAsync(User);
        var userFollow = await _context.UserFollows.FirstOrDefaultAsync(userFollow =>
            userFollow.FollowerId == signedInUser!.Id && userFollow.FollowingId == user.Id
        );
        if (userFollow == null)
        {
            return RedirectToAction(nameof(UserTimeline), new { username });
        }
        _context.UserFollows.Remove(userFollow);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"You are no longer following {username}";
        return RedirectToAction(nameof(UserTimeline), new { username });
    }

    private Task<List<MessageViewModel>> GetMessagesAsync(
        int page,
        Expression<Func<Message, bool>>? predicate = null,
        bool isFlagged = false
    )
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "The value must be greater than zero.");
        }
        var queryable = _context
            .Messages.Include(message => message.Author)
            .Where(message => message.IsFlagged == isFlagged);
        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }
        return queryable
            .OrderByDescending(message => message.PublishDate)
            .Select(message => new MessageViewModel
            {
                Email = message.Author!.Email!,
                Username = message.Author!.UserName!,
                Text = message.Text,
                PublishDate = message.PublishDate,
            })
            .Skip((page - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToListAsync();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
