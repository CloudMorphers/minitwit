using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniTwit.Data;
using MiniTwit.Models.Auth;

namespace MiniTwit.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthController> _logger; // Add logger

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger; // Initialize logger
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        _logger.LogInformation("Register page requested");
        ViewData["Title"] = "Sign Up";
        return View();
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        _logger.LogInformation("Register attempt for username {Username}", model.Username);
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState
                .Values.SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault();
            TempData["Error"] = errorMessage;
            return View(model);
        }
        var existingUser = await _userManager.FindByNameAsync(model.Username);
        if (existingUser != null)
        {
            TempData["Error"] = "The username is already taken";
            return View(model);
        }
        var user = new AppUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            TempData["Error"] = "An error occurred while registering your account";
            return View(model);
        }
        TempData["Success"] = "You are successfully registered and can now log in";
        return RedirectToAction("Login");
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        _logger.LogInformation("Login page requested");
        ViewData["Title"] = "Sign In";
        return View();
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        _logger.LogInformation("Login attempt for username {Username}", model.Username);
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState
                .Values.SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault();
            TempData["Error"] = errorMessage;
            return View(model);
        }
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
        if (!result.Succeeded)
        {
            TempData["Error"] = "Username or password is invalid";
            return View(model);
        }
        TempData["Success"] = "You are now logged in";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("Logout request");
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
