using AspireNext.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace AspireNext.Server.Data;

public static class AdminSeeder
{
    private const string AdminEmail = "p.charest46@gmail.com";

    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (await roleManager.RoleExistsAsync("Admin"))
        {
            return;
        }

        await roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is not null)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
