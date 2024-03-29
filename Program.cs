using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i =>
    new SmtpEmailSender(
        builder.Configuration["EmailSender:Host"],
        builder.Configuration.GetValue<int>("EmailSender:Port"),
        builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
        builder.Configuration["EmailSender:Username"],
        builder.Configuration["EmailSender:Password"]
    )
);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IdentityContext>(
    options => options.UseSqlite(builder.Configuration["ConnectionStrings:SQLite_Connection"]));

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

    options.Lockout.MaxFailedAccessAttempts = 5; //Kullanıcıya 5 defa yanlış giriş hakkı verildi
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //Kullanıcı giriş işlemi yaparken 5 defa yanlış giriş yaparsa 5 dakika süreyle kitlenir
    options.SignIn.RequireConfirmedEmail = true; //Kullanıcıların e-posta adreslerini doğrulamaları gerektiğini belirtir
});

builder.Services.ConfigureApplicationCookie(options => //Cookie ayarları
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; //Eğer yetkisi olmayan bir kullanıcı bir sayfaya erişmeye çalışırsa yönlendirileceği sayfa
    options.SlidingExpiration = true; //true olursa kullanıcı her istekte cookie süresi yenilenir
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); //Bu kullanımda ise bir süre belirtilir ve bu süre sonunda cookie silinir genellikle üstteki kullanım veya bu kullanım terch edilir
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

SeedData.IdentityTestUser(app);

app.Run();
