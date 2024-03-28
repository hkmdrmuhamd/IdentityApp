using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Controllers
{
    public class UsersController : Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;

        public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                ViewBag.Roles = await _roleManager.Roles.Select(i => i.Name).ToListAsync();
                return View(new EditViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    SelectedRoles = await _userManager.GetRolesAsync(user)
                });
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
                    if (model.OldPassword != null)
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
                                await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user)); //Kullanıcının tüm rollerini sil 
                                if (model.SelectedRoles != null)
                                {
                                    await _userManager.AddToRolesAsync(user, model.SelectedRoles); //Yeni rolleri ekle
                                }
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
                    else
                    {
                        user.UserName = model.UserName;
                        user.Email = model.Email;
                        user.FullName = model.FullName;

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                            if (model.SelectedRoles != null)
                            {
                                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                            }
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

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

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
            return View(id);
        }
    }
}
