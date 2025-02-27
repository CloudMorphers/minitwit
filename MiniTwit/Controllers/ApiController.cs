using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Data;
using MiniTwit.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTwit.Controllers
{
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("msgs")]
        public async Task<IActionResult> Messages([FromQuery] int no = 100)
        {
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
            // Update latest (implement this method if needed)
            // update_latest(request);

            // Check if the request is from a simulator (implement this method if needed)
            // var notFromSimResponse = not_req_from_simulator(request);
            // if (notFromSimResponse != null)
            // {
            //     return notFromSimResponse;
            // }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound();
            }

            if (Request.Method == "GET")
            {
                var messages = await _context.Messages
                    .Where(m => m.Flagged == 0 && m.AuthorId == user.Id)
                    .OrderByDescending(m => m.PubDate)
                    .Take(no)
                    .Select(m => new MessageViewModel
                    {
                        Email = user.Email,
                        Username = user.UserName,
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
                    AuthorId = user.Id,
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

    }
}