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
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // This POST method handles the submission of the registration form.
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // Login Action
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }

        // Logout Action
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}