using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;

namespace MultiTenantSaaS.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext(ITenantService? tenantService = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var service = tenantService ?? new TenantService();
        var context = new ApplicationDbContext(options, service);

        // Seed default roles
        SeedDefaultData(context);

        return context;
    }

    private static void SeedDefaultData(ApplicationDbContext context)
    {
        // Seed roles if not already present
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Id = 1, Name = "Admin", Description = "Full system access" },
                new Role { Id = 2, Name = "Manager", Description = "Can manage team tasks" },
                new Role { Id = 3, Name = "User", Description = "Basic user access" }
            );
            context.SaveChanges();
        }
    }

    public static (Tenant tenant, User user) SeedTenantAndUser(
        ApplicationDbContext context,
        string email = "test@example.com",
        string companyName = "Test Company",
        string companyCode = "TEST",
        string roleName = "Admin")
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = companyName,
            CompanyCode = companyCode,
            IsActive = true
        };

        context.Tenants.Add(tenant);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = "hashedpassword",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        context.Users.Add(user);

        var role = context.Roles.First(r => r.Name == roleName);
        context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        context.SaveChanges();

        return (tenant, user);
    }
}
