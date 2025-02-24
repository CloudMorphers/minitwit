using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniTwit.Models;
using MiniTwit.Data; // Ensure you have a DbContext for the database
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MessagesController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ 1. GET /api/messages - Get all messages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageViewModel>>> GetMessages()
    {
        return await _context.Messages.ToListAsync();
    }

    // ✅ 2. GET /api/messages/{username} - Get messages for a specific user
    [HttpGet("{username}")]
    public async Task<ActionResult<IEnumerable<MessageViewModel>>> GetUserMessages(string username)
    {
        var userMessages = await _context.Messages
                            .Where(m => m.Username == username)
                            .ToListAsync();

        if (userMessages == null || userMessages.Count == 0)
        {
            return NotFound();
        }

        return userMessages;
    }

    // ✅ 3. POST /api/messages/{username} - Post a new message
    [HttpPost("{username}")]
    public async Task<IActionResult> PostMessage(string username, [FromBody] MessageViewModel newMessage)
    {
        if (string.IsNullOrEmpty(newMessage.Text))
        {
            return BadRequest("Message content cannot be empty.");
        }

        newMessage.Username = username;
        newMessage.PublishDate = DateTime.UtcNow;

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserMessages), new { username = newMessage.Username }, newMessage);
    }
}