using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Data;
public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public DbInitializer(ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public void Initialize()
    {
        try
        {
            if (_dbContext.Database.GetPendingMigrations().Count() > 0)
            {
                _dbContext.Database.Migrate();
            }
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();

                _userManager.CreateAsync(new ApplicationUser()
                {
                    UserName = "admin@wl.com",
                    Email = "admin@wl.com",
                    Name = "admin",
                    NormalizedUserName = "ADMIN@WL.COM",
                    NormalizedEmail = "ADMIN@WL.COM",
                    PhoneNumber = "123"
                }, "1qaz!QAZ").GetAwaiter().GetResult();

                ApplicationUser user = _dbContext.ApplicationUsers
                    .FirstOrDefault(a => a.Email == "admin@wl.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

        }
        catch (Exception e)
        {

            throw;
        }
    }
}
