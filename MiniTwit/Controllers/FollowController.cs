using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MiniTwit.Models;

namespace MiniTwit.Controllers
{
    [ApiController]
    public class FollowController : Controller
    {
        private static Dictionary<string, List<string>> _follows = new Dictionary<string, List<string>>();

        // Follow or unfollow a user
        [HttpPost("/fllws/{username}")]
        public IActionResult Follow(string username, [FromBody] FollowViewModel model)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { error = "Username is required" });

            if (!_follows.ContainsKey(username))
                _follows[username] = new List<string>();

            if (!string.IsNullOrEmpty(model.Follow))
            {
                if (!_follows[username].Contains(model.Follow))
                    _follows[username].Add(model.Follow);
            }
            else if (!string.IsNullOrEmpty(model.Unfollow))
            {
                _follows[username].Remove(model.Unfollow);
            }
            else
            {
                return BadRequest(new { error = "You must specify a user to follow or unfollow" });
            }

            return Ok(); // Returns HTTP 200 OK
        }

        // Get the list of users a given user is following
        [HttpGet("/fllws/{username}")]
        public IActionResult GetFollows(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { error = "Username is required" });

            var follows = _follows.ContainsKey(username) ? _follows[username] : new List<string>();
            return Ok(new { follows }); // Returns HTTP 200 OK with follow list
        }
    }
}
