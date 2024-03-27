using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class UsersController : Controller
    {
        private UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_userManager.Users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                EditViewModel model = new EditViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName
                };
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var passwordCheckResult = await _userManager.CheckPasswordAsync(user, model.OldPassword);
                    if (!passwordCheckResult)
                    {
                        ModelState.AddModelError("", "Eski parola yanlış.");
                        return View(model);
                    }
                    else
                    {
                        user.UserName = model.UserName;
                        user.Email = model.Email;
                        user.FullName = model.FullName;

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded && !string.IsNullOrEmpty(model.Password))
                        {
                            await _userManager.RemovePasswordAsync(user);
                            await _userManager.AddPasswordAsync(user, model.Password);

                        }

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            foreach (IdentityError error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                }
            }
            return View(model);
        }
    }
}
