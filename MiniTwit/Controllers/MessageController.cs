using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MiniTwit.Models;

namespace MiniTwit.Controllers
{
    [ApiController]
    public class MessageController : Controller
    {
        private static List<Message> _messages = new List<Message>();

        // Post a message
        [HttpPost("/msgs/{username}")]
        public IActionResult CreateMessage(string username, [FromBody] MessageViewModel model)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(model.Content))
                return BadRequest(new { error = "Username and message content are required" });

            _messages.Add(new Message { User = username, Content = model.Content });

            return Ok(); // Returns HTTP 200 OK
        }

        // Get messages for a specific user
        [HttpGet("/msgs/{username}")]
        public IActionResult GetMessages(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { error = "Username is required" });

            var userMessages = _messages.Where(m => m.User == username).ToList();
            return Ok(userMessages); // Returns HTTP 200 OK with message list
        }

        // Get all messages
        [HttpGet("/msgs")]
        public IActionResult GetAllMessages()
        {
            return Ok(_messages); // Returns HTTP 200 OK with all messages
        }
    }
}
