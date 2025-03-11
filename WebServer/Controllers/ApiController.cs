using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Core;
using MiniTwit.Data;
using MiniTwit.Models.Api;

namespace MiniTwit.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly AtomicIntegerFile _latestProcessedCommandId = new("latest_processed_sim_action_id.txt", -1);
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ApiController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task UpdateLatestAsync()
    {
        var latest = Request.Query["latest"].FirstOrDefault();
        if (int.TryParse(latest, out var parsedCommandId))
        {
            await _latestProcessedCommandId.SetAsync(parsedCommandId);
        }
    }

    private IActionResult? NotRequestFromSimulator()
    {
#if DEBUG
        return null;
#else
        var fromSimulator = Request.Headers.Authorization.FirstOrDefault();
        if (fromSimulator != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
        {
            var error = "You are not authorized to use this resource!";
            return StatusCode(403, new { status = 403, error_msg = error });
        }
        return null;
#endif
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var latest = await _latestProcessedCommandId.GetAsync();
        return Ok(new { latest });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterInputModel model)
    {
        await UpdateLatestAsync();
        var existingUser = await _userManager.FindByNameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest(new { status = 400, error_msg = "The username is already taken" });
        }
        var user = new AppUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { status = 400, error_msg = "An error occurred while registering your account" });
        }
        return NoContent();
    }

    [HttpGet("msgs/{username?}")]
    public async Task<IActionResult> Messages(string? username = null, [FromQuery(Name = "no")] int count = 100)
    {
        await UpdateLatestAsync();
        var forbiddenResponse = NotRequestFromSimulator();
        if (forbiddenResponse != null)
        {
            return forbiddenResponse;
        }
        var user = username != null ? await _userManager.FindByNameAsync(username) : null;
        if (username != null && user == null)
        {
            return NotFound();
        }
        var messages = await _context
            .Messages.Include(message => message.Author)
            .Where(message => (username == null || message.AuthorId == user!.Id) && !message.IsFlagged)
            .OrderByDescending(message => message.PublishDate)
            .Select(message => new
            {
                content = message.Text,
                pub_date = message.PublishDate,
                user = message.Author!.UserName,
            })
            .Take(count)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessage(string username, [FromBody] MessageInputModel model)
    {
        await UpdateLatestAsync();
        var forbiddenResponse = NotRequestFromSimulator();
        if (forbiddenResponse != null)
        {
            return forbiddenResponse;
        }
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        if (string.IsNullOrEmpty(model.Content))
        {
            return BadRequest("Content is required.");
        }
        var message = new Message
        {
            Text = model.Content,
            PublishDate = DateTime.UtcNow,
            AuthorId = user.Id,
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> UserFollows(string username, [FromQuery(Name = "no")] int count = 100)
    {
        await UpdateLatestAsync();
        var forbiddenResponse = NotRequestFromSimulator();
        if (forbiddenResponse != null)
        {
            return forbiddenResponse;
        }
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        var followers = await _context
            .UserFollows.Include(userFollow => userFollow.Following)
            .Where(userFollow => userFollow.FollowerId == user.Id)
            .Select(userFollow => userFollow.Following!.UserName)
            .Take(count)
            .ToListAsync();
        return Ok(new { follows = followers });
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> FollowOrUnfollowUser(string username, [FromBody] FollowInputModel model)
    {
        await UpdateLatestAsync();
        var forbiddenResponse = NotRequestFromSimulator();
        if (forbiddenResponse != null)
        {
            return forbiddenResponse;
        }
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }
        if (string.IsNullOrEmpty(model.Follow) && string.IsNullOrEmpty(model.Unfollow))
        {
            return BadRequest("Target username is required.");
        }
        var otherUser = await _userManager.FindByNameAsync(
            string.IsNullOrEmpty(model.Follow) ? model.Unfollow! : model.Follow
        );
        if (otherUser == null)
        {
            return NotFound();
        }
        var userFollow = await _context.UserFollows.FirstOrDefaultAsync(userFollow =>
            userFollow.FollowerId == user.Id && userFollow.FollowingId == otherUser.Id
        );
        if (!string.IsNullOrEmpty(model.Follow))
        {
            if (userFollow != null)
            {
                return NoContent();
            }
            userFollow = new UserFollow { FollowerId = user.Id, FollowingId = otherUser.Id };
            _context.UserFollows.Add(userFollow);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (userFollow == null)
            {
                return NoContent();
            }
            _context.UserFollows.Remove(userFollow);
            await _context.SaveChangesAsync();
        }
        return NoContent();
    }
}
