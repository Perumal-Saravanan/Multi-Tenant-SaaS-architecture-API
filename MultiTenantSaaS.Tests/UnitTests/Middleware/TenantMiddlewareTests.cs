using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MultiTenantSaaS.Infrastructure.Middleware;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Tests.Helpers;
using System.Security.Claims;
using Xunit;

namespace MultiTenantSaaS.Tests.UnitTests.Middleware;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ExtractsTenantIdFromJwt()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = JwtTokenHelper.GenerateTestToken(userId, tenantId);
        var principal = JwtTokenHelper.GetPrincipalFromToken(token);

        var tenantService = new TenantService();
        var middleware = new TenantMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context, tenantService);

        // Assert
        tenantService.GetCurrentTenantId().Should().Be(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_SetsTenantContext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = JwtTokenHelper.GenerateTestToken(userId, tenantId, "test@example.com");
        var principal = JwtTokenHelper.GetPrincipalFromToken(token);

        var tenantService = new TenantService();
        var middleware = new TenantMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context, tenantService);

        // Assert
        tenantService.GetCurrentTenantId().Should().Be(tenantId);
        tenantService.GetCurrentUserId().Should().Be(userId.ToString());
    }

    [Fact]
    public async Task InvokeAsync_HandlesInvalidToken()
    {
        // Arrange
        var tenantService = new TenantService();
        var middleware = new TenantMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(); // No claims

        // Act
        await middleware.InvokeAsync(context, tenantService);

        // Assert - Should not throw, tenant service should have empty values
        tenantService.GetCurrentTenantId().Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (innerHttpContext) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = JwtTokenHelper.GenerateTestToken(userId, tenantId);
        var principal = JwtTokenHelper.GetPrincipalFromToken(token);

        var tenantService = new TenantService();
        var middleware = new TenantMiddleware(next);

        var context = new DefaultHttpContext();
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context, tenantService);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
