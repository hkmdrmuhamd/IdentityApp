using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.UserName != null && model.Password != null)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        await _signInManager.SignOutAsync();

                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError("", "Lütfen e-posta adresinizi doğrulayın.");
                            return View(model);
                        }

                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);//true durumu kullanıcının hesabı kilitlenmişse kilidini açar
                        if (result.Succeeded)
                        {
                            await _userManager.ResetAccessFailedCountAsync(user);//kullanıcı başarılı giriş yaptığında hatalı giriş sayısını sıfırlar
                            await _userManager.SetLockoutEndDateAsync(user, null);//kullanıcı başarılı giriş yaptığında hesabın kilitlenme süresini sıfırlar
                            return RedirectToAction("Index", "Home");
                        }
                        else if (result.IsLockedOut)
                        {
                            var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                            var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                            ModelState.AddModelError("", $"Hesabınız {timeLeft.Minutes} dakika boyunca kilitlenmiştir.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Şifre hatalı");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Kullanıcı adı hatalı");
                    }
                }
            }
            return View(model);
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
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token });

                    TempData["message"] = "Email hesabınızdaki onay mailine tıklayınız";
                    return RedirectToAction("Login", "Account");
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

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData["message"] = "Geçersiz token bilgisi";
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız onaylandı";
                    return View("Login", "Account");
                }
            }
            TempData["message"] = "Kullanıcı Bulunamadı";
            return View();
        }

    }
}