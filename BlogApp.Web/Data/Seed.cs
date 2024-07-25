using BlogApp.Web.Enums;
using BlogApp.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Web.Data
{
    public sealed class Seed
    {
        private readonly AppDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public Seed(AppDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedRolesAsync()
        {
            if (_roleManager.Roles.Any()) return;

            await _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
            await _roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await _roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
        }

        public async Task SeedUsersAsync()
        {
            if (_userManager.Users.Any()) return;

            var superadmin = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "superadmin@gmail.com",
                UserName = "superadmin",
            };

            var admin = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@gmail.com",
                UserName = "admin",
            };

            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "user@gmail.com",
                UserName = "user",
            };

            var superadminResult = await _userManager.CreateAsync(superadmin, "super123");
            if (superadminResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(superadmin, Roles.SuperAdmin.ToString());
                if (!roleResult.Succeeded)
                {
                    // Handle role assignment failure
                    throw new Exception($"Failed to assign role '{Roles.SuperAdmin}' to '{superadmin.UserName}'.");
                }
            }
            else
            {
                // Handle user creation failure
                throw new Exception($"Failed to create superadmin user: {string.Join(", ", superadminResult.Errors.Select(e => e.Description))}");
            }

            var adminResult = await _userManager.CreateAsync(admin, "admin123");
            if (adminResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(admin, Roles.Admin.ToString());
                if (!roleResult.Succeeded)
                {
                    // Handle role assignment failure
                    throw new Exception($"Failed to assign role '{Roles.Admin}' to '{admin.UserName}'.");
                }
            }
            else
            {
                // Handle user creation failure
                throw new Exception($"Failed to create admin user: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
            }

            var userResult = await _userManager.CreateAsync(user, "user123");
            if (userResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                if (!roleResult.Succeeded)
                    // Handle role assignment failure
                {
                    throw new Exception($"Failed to assign role '{Roles.User}' to '{user.UserName}'.");
                }
            }
            else
            {
                // Handle user creation failure
                throw new Exception($"Failed to create user: {string.Join(", ", superadminResult.Errors.Select(e => e.Description))}");
            }


        }
    }
}
