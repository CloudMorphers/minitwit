using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MiniTwit.Models.Auth;
using MiniTwit.Data;

namespace MiniTwit.Controllers
{
    [ApiController]
    public class AuthController : Controller
    {
        private static List<AppUser> _users = new List<AppUser>();

        // Register a new user
        [HttpPost("/register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new { error = "All fields are required" });

            if (_users.Any(u => u.UserName == model.Username))  // FIX: Use UserName instead of Username
                return BadRequest(new { error = "Username is already taken" });

            var user = new AppUser
            {
                UserName = model.Username, // FIX: Use UserName
                Email = model.Email
            };
            _users.Add(user);

            return Ok(); // Returns HTTP 200 OK
        }

        [HttpPost("/login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new { error = "Username and password are required" });

            var user = _users.FirstOrDefault(u => u.UserName == model.Username); // FIX: Use UserName
            if (user == null)
                return Unauthorized(new { error = "Invalid username" });

            return Ok(); // Returns HTTP 200 OK
        }

    }
}
