using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MiniTwit.Data;
using MiniTwit.Models.Auth;

//using MiniTwit.Models;

namespace MiniTwit.Controllers
{
    // AuthController is responsible for handling user authentication (Login, Register, Logout).
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // This action displays the Register view.
        // It doesn't require any model data, it just returns an empty registration form.
        [HttpGet("/register")]
        public IActionResult Register()
        {
            return View();
        }


        // This POST method handles the submission of the registration form.
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check if the username is empty
            if (string.IsNullOrEmpty(model.Username))
            {
                TempData["Error"] = "You have to enter a username";
                return View(model);
            }

            // Check if the email is empty or invalid
            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
            {
                TempData["Error"] = "You have to enter a valid email address";
                return View(model);
            }

            // Check if password is empty
            if (string.IsNullOrEmpty(model.Password))
            {
                TempData["Error"] = "You have to enter a password";
                return View(model);
            }

            // Check if both passwords match
            if (model.Password != model.Password2)
            {
                TempData["Error"] = "The two passwords do not match";
                return View(model);
            }

            // Check if username already exists
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                TempData["Error"] = "The username is already taken";
                return View(model);
            }

            // Proceed with user registration
            var user = new AppUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                TempData["Success"] = "You were successfully registered and can now log in";
                return RedirectToAction("Login");
            }

            // If registration failed for any reason
            TempData["Error"] = "An error occurred while registering your account";
            return View(model);
        }



        // Login Action
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                TempData["Error"] = "Username is invalid";
                return View(model);  // Return the view with username error
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                TempData["Error"] = "Username is invalid"; // Username does not exist
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                TempData["Error"] = "Password is invalid";  // Password is empty
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Invalid password";  // If password doesn't match
                return View(model);
            }

            TempData["Success"] = "You were logged in";
            return RedirectToAction("Index", "Home");
        }

        // Logout Action
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}