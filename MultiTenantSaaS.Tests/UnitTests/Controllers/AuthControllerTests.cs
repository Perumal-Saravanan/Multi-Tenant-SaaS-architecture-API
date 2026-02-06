using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTenantSaaS.API.Controllers;
using MultiTenantSaaS.Core.DTOs;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Infrastructure.Services.Interfaces;
using MultiTenantSaaS.Tests.Helpers;
using Xunit;
using Moq;

namespace MultiTenantSaaS.Tests.UnitTests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthController _controller;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ITenantService _tenantService;

    public AuthControllerTests()
    {
        _tenantService = new TenantService();
        _context = TestDbContextFactory.CreateInMemoryContext(_tenantService);

        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "YourSuperSecretKeyForJWTTokenGeneration12345!"},
            {"Jwt:Issuer", "MultiTenantSaaSAPI"},
            {"Jwt:Audience", "MultiTenantSaaSClient"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _jwtTokenService = new JwtTokenService(_configuration);
        var authService = new AuthService(_context, _jwtTokenService);
        
        // Create a mock logger
        var mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(authService, mockLogger.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "newuser@company.com",
            Password: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            CompanyName: "New Company",
            CompanyCode: "NEWCO"
        );

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Token.Should().NotBeNullOrEmpty();
        response.Email.Should().Be(request.Email);
        response.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var existingRequest = new RegisterRequest(
            Email: "duplicate@company.com",
            Password: "Password123!",
            FirstName: "First",
            LastName: "User",
            CompanyName: "Company A",
            CompanyCode: "COMPA"
        );

        await _controller.Register(existingRequest);

        var duplicateRequest = new RegisterRequest(
            Email: "duplicate@company.com",
            Password: "Password123!",
            FirstName: "Second",
            LastName: "User",
            CompanyName: "Company B",
            CompanyCode: "COMPB"
        );

        // Act
        var result = await _controller.Register(duplicateRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Email already registered");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "login@company.com",
            Password: "Password123!",
            FirstName: "Login",
            LastName: "Test",
            CompanyName: "Login Company",
            CompanyCode: "LOGIN"
        );

        await _controller.Register(registerRequest);

        // Get the tenant that was created during registration
        var tenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.CompanyCode == "LOGIN");
        tenant.Should().NotBeNull();
        
        // Set tenant context before login
        _tenantService.SetTenantContext(tenant!.Id, string.Empty);

        var loginRequest = new LoginRequest(
            Email: "login@company.com",
            Password: "Password123!"
        );

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Token.Should().NotBeNullOrEmpty();
        response.Email.Should().Be(loginRequest.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "valid@company.com",
            Password: "CorrectPassword123!",
            FirstName: "Valid",
            LastName: "User",
            CompanyName: "Valid Company",
            CompanyCode: "VALID"
        );

        await _controller.Register(registerRequest);

        // Get the tenant that was created during registration
        var tenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.CompanyCode == "VALID");
        tenant.Should().NotBeNull();
        
        // Set tenant context before login
        _tenantService.SetTenantContext(tenant!.Id, string.Empty);

        var loginRequest = new LoginRequest(
            Email: "valid@company.com",
            Password: "WrongPassword123!"
        );

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized!.Value.Should().Be("Invalid credentials or account is inactive");
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest(
            Email: "nonexistent@company.com",
            Password: "Password123!"
        );

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_CreatesUserWithAdminRole()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "admin@company.com",
            Password: "Password123!",
            FirstName: "Admin",
            LastName: "User",
            CompanyName: "Admin Company",
            CompanyCode: "ADMIN"
        );

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response!.Roles.Should().HaveCount(1);
        response.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Register_CreatesTenantAndUser()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "tenant@company.com",
            Password: "Password123!",
            FirstName: "Tenant",
            LastName: "Owner",
            CompanyName: "Tenant Company",
            CompanyCode: "TENANT"
        );

        // Act
        await _controller.Register(request);

        // Get the tenant without tenant context filter
        var tenant = _context.Tenants.Local.FirstOrDefault(t => t.CompanyCode == "TENANT") ?? 
                     await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.CompanyCode == "TENANT");
        tenant.Should().NotBeNull();
        tenant!.Name.Should().Be("Tenant Company");
        tenant.IsActive.Should().BeTrue();

        // Set tenant context to query for the user
        _tenantService.SetTenantContext(tenant.Id, string.Empty);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "tenant@company.com");
        user.Should().NotBeNull();
        user!.TenantId.Should().Be(tenant.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
