using Bookshop.Models;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email };
                var res = await _userManager.CreateAsync(user, model.Password);
                if (res.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

            }
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Normalize email to ensure consistency
                var normalizedEmail = model.Email.ToUpper();

                // Find user by email
                var user = await _userManager.FindByEmailAsync(normalizedEmail);
                if (user != null)
                {
                    var res = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (res.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (res.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "User account locked out.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No user found with this email.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
             await _signInManager.SignOutAsync();
             return RedirectToAction("Index", "Home");
    
        }

    }
}
