using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class AccountController(IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    public IActionResult Login(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        LoginViewModel loginVM = new()
        {
            RedirectUrl = returnUrl
        };
        return View(loginVM);
    }

    public IActionResult Register(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
            roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();
        }

        RegisterViewModel registerVM = new()
        {
            RoleList = roleManager.Roles.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Name
            }),
            RedirectUrl = returnUrl
        };

        return View(registerVM);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerVM)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser user = new()
            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                PhoneNumber = registerVM.PhoneNumber,
                NormalizedEmail = registerVM.Email.ToUpper(),
                EmailConfirmed = true,
                UserName = registerVM.Email,
                CreatedAt = DateTime.Now

            };

            var result = await userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(registerVM.Role))
                    await userManager.AddToRoleAsync(user, registerVM.Role);
                else
                    await userManager.AddToRoleAsync(user, SD.Role_Customer);

                await signInManager.SignInAsync(user, isPersistent: false);

                if (string.IsNullOrEmpty(registerVM.RedirectUrl))
                    return RedirectToAction("Index", "Home");
                else
                    return LocalRedirect(registerVM.RedirectUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        registerVM.RoleList = roleManager.Roles
            .Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Name
            });

        return View(registerVM);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginVM)
    {
        if (ModelState.IsValid)
        {
            var result = await signInManager
                .PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(loginVM.Email);
                if (await userManager.IsInRoleAsync(user, SD.Role_Admin))
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    if (string.IsNullOrEmpty(loginVM.RedirectUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return LocalRedirect(loginVM.RedirectUrl);
                }
            }
            else
                ModelState.AddModelError("", "Invalid login attempt");
        }
        return View(loginVM);
    }

    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
