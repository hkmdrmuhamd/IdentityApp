using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    public static class SeedData
    {
        private const string adminUser = "admin";
        private const string adminPassword = "Admin_123";

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
            if (context.Database.GetAppliedMigrations().Any())
            {
                context.Database.Migrate();
            }

            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(adminUser);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = adminUser,
                    FullName = "Muhammed Hükümdar",
                    Email = "admin@mhkmdr.com",
                    PhoneNumber = "1234567890",

                };
                await userManager.CreateAsync(user, adminPassword); //adminPassword'ü hashleyip veritabanına kaydeder. Arka planda Identity bu işlemi yapar.
            }
        }
    }
}
