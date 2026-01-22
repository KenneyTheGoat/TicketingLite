using Microsoft.AspNetCore.Identity;

public static class SeedRolesAndUsers
{
    public static async Task RunAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Always safe: ensure roles exist
        string[] roles = ["Admin", "Agent", "Client"];
        foreach (var r in roles)
        {
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));
        }

        // ONLY seed demo users in Development (or if explicitly enabled)
        var seedDemoUsers = env.IsDevelopment() || config.GetValue<bool>("SeedDemoUsers");

        if (!seedDemoUsers)
            return;

        await EnsureUser(userMgr, "admin@demo.com", "Admin123!", "Admin");
        await EnsureUser(userMgr, "agent@demo.com", "Agent123!", "Agent");
        await EnsureUser(userMgr, "client@demo.com", "Client123!", "Client");
    }

    private static async Task EnsureUser(UserManager<IdentityUser> userMgr, string email, string password, string role)
    {
        var user = await userMgr.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var res = await userMgr.CreateAsync(user, password);
            if (!res.Succeeded)
                throw new Exception(string.Join("; ", res.Errors.Select(e => e.Description)));
        }

        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }
}
