using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.DTOs;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;

namespace MultiTenantSaaS.Infrastructure.Services;

using MultiTenantSaaS.Infrastructure.Services.Interfaces;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(ApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email))
        {
            return null;
        }

        // Create new tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName,
            CompanyCode = request.CompanyCode,
            IsActive = true
        };

        _context.Tenants.Add(tenant);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        _context.Users.Add(user);

        // Assign Admin role to first user
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        
        if (adminRole == null)
        {
            adminRole = new Role 
            { 
                Name = "Admin", 
                Description = "Full system access" 
            };
            _context.Roles.Add(adminRole);
        }

        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            Role = adminRole // Use navigation property to handle both existing and new roles correctly
        });

        await _context.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user, new[] { "Admin" }, tenant.Id);

        return new AuthResponse(
            token,
            user.Email,
            user.FirstName,
            user.LastName,
            tenant.Id,
            new[] { "Admin" }
        );
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var token = _jwtTokenService.GenerateToken(user, roles, user.TenantId);

        return new AuthResponse(
            token,
            user.Email,
            user.FirstName,
            user.LastName,
            user.TenantId,
            roles
        );
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}