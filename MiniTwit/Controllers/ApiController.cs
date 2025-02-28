using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MiniTwit.Data;
using MiniTwit.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTwit.Controllers
{
    [Route("api/")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiController(AppDbContext context)
        {
            _context = context;
        }

        private void UpdateLatest(HttpRequest request)
        {
            var latest = request.Query["latest"].FirstOrDefault();
            if (int.TryParse(latest, out int parsedCommandId) && parsedCommandId != -1)
            {
                System.IO.File.WriteAllText("./latest_processed_sim_action_id.txt", parsedCommandId.ToString());
            }
        }

        private IActionResult NotReqFromSimulator(HttpRequest request)
        {
            var fromSimulator = request.Headers["Authorization"].FirstOrDefault();
            // if (fromSimulator != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
            // {
            //     var error = "You are not authorized to use this resource!";
            //     return StatusCode(403, new { status = 403, error_msg = error });
            // }
            return null;
        }

        private async Task<int?> GetUserId(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == username);
            return user?.Id;
        }

        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            string filePath = "./latest_processed_sim_action_id.txt";
            int latestProcessedCommandId;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            try
            {
                var content = System.IO.File.ReadAllText(filePath);
                latestProcessedCommandId = int.Parse(content);
            }
            catch
            {
                latestProcessedCommandId = -1;
            }

            return Ok(new { latest = latestProcessedCommandId });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInputModel input)
        {
            UpdateLatest(Request);

            if (string.IsNullOrEmpty(input.Username))
            {
                return BadRequest(new { status = 400, error_msg = "You have to enter a username" });
            }

            if (string.IsNullOrEmpty(input.Email) || !input.Email.Contains("@"))
            {
                return BadRequest(new { status = 400, error_msg = "You have to enter a valid email address" });
            }

            if (string.IsNullOrEmpty(input.Pwd))
            {
                return BadRequest(new { status = 400, error_msg = "You have to enter a password" });
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.UserName == input.Username);
            if (existingUser != null)
            {
                return BadRequest(new { status = 400, error_msg = "The username is already taken" });
            }

            var user = new AppUser
            {
                UserName = input.Username,
                Email = input.Email,
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, input.Pwd)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("msgs")]
        public async Task<IActionResult> Messages([FromQuery] int no = 100)
        {
            UpdateLatest(Request);

            var notFromSimResponse = NotReqFromSimulator(Request);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }

            var messages = await _context.Messages
                .Where(m => m.Flagged == 0)
                .OrderByDescending(m => m.PubDate)
                .Take(no)
                .Join(_context.Users,
                      message => message.AuthorId,
                      user => user.Id,
                      (message, user) => new MessageViewModel
                      {
                          Email = user.Email,
                          Username = user.UserName,
                          Text = message.Text,
                          PublishDate = DateTimeOffset.FromUnixTimeSeconds(message.PubDate).UtcDateTime
                      })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("msgs/{username}")]
        [HttpPost("msgs/{username}")]
        public async Task<IActionResult> MessagesPerUser(string username, [FromQuery] int no = 100, [FromBody] MessageInputModel input = null)
        {
            UpdateLatest(Request);

            var notFromSimResponse = NotReqFromSimulator(Request);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }

            var userId = await GetUserId(username);
            if (userId == null)
            {
                return NotFound();
            }

            if (Request.Method == "GET")
            {
                var messages = await _context.Messages
                    .Where(m => m.Flagged == 0 && m.AuthorId == userId)
                    .OrderByDescending(m => m.PubDate)
                    .Take(no)
                    .Select(m => new MessageViewModel
                    {
                        Email = _context.Users.Where(u => u.Id == m.AuthorId).Select(u => u.Email).FirstOrDefault(),
                        Username = _context.Users.Where(u => u.Id == m.AuthorId).Select(u => u.UserName).FirstOrDefault(),
                        Text = m.Text,
                        PublishDate = DateTimeOffset.FromUnixTimeSeconds(m.PubDate).UtcDateTime
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            else if (Request.Method == "POST")
            {
                if (input == null || string.IsNullOrEmpty(input.Content))
                {
                    return BadRequest("Content is required.");
                }

                var message = new Message
                {
                    AuthorId = userId.Value,
                    Text = input.Content,
                    PubDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Flagged = 0
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return BadRequest("Unsupported request method.");
        }

        [HttpGet("fllws/{username}")]
        [HttpPost("fllws/{username}")]
        public async Task<IActionResult> Follow(string username, [FromQuery] int no = 100, [FromBody] FollowInputModel input = null)
        {
            UpdateLatest(Request);

            var notFromSimResponse = NotReqFromSimulator(Request);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }

            var userId = await GetUserId(username);
            if (userId == null)
            {
                return NotFound();
            }

            if (Request.Method == "GET")
            {
                var followers = await _context.Followers
                    .Where(f => f.WhoId == userId)
                    .Take(no)
                    .Join(_context.Users,
                          follower => follower.WhomId,
                          followedUser => followedUser.Id,
                          (follower, followedUser) => followedUser.UserName)
                    .ToListAsync();

                return Ok(new { follows = followers });
            }
            else if (Request.Method == "POST")
            {
                if (input == null)
                {
                    return BadRequest("Invalid input.");
                }

                if (input.Follow != null)
                {
                    var followsUserId = await GetUserId(input.Follow);
                    if (followsUserId == null)
                    {
                        return NotFound();
                    }

                    var existingFollower = await _context.Followers
                        .SingleOrDefaultAsync(f => f.WhoId == userId && f.WhomId == followsUserId);

                    if (existingFollower != null)
                    {
                        return BadRequest("You are already following this user.");
                    }

                    var follower = new Follower
                    {
                        WhoId = userId.Value,
                        WhomId = followsUserId.Value
                    };

                    _context.Followers.Add(follower);
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
                else if (input.Unfollow != null)
                {
                    var unfollowsUserId = await GetUserId(input.Unfollow);
                    if (unfollowsUserId == null)
                    {
                        return NotFound();
                    }

                    var follower = await _context.Followers
                        .SingleOrDefaultAsync(f => f.WhoId == userId && f.WhomId == unfollowsUserId);

                    if (follower != null)
                    {
                        _context.Followers.Remove(follower);
                        await _context.SaveChangesAsync();
                    }

                    return NoContent();
                }
            }

            return BadRequest("Unsupported request method.");
        }
    }
}