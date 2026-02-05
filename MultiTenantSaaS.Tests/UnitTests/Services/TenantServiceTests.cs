using FluentAssertions;
using MultiTenantSaaS.Infrastructure.Services;
using Xunit;

namespace MultiTenantSaaS.Tests.UnitTests.Services;

public class TenantServiceTests
{
    [Fact]
    public void SetTenantContext_StoresTenantIdAndUserId()
    {
        // Arrange
        var service = new TenantService();
        var tenantId = Guid.NewGuid();
        var userId = "test-user-123";

        // Act
        service.SetTenantContext(tenantId, userId);

        // Assert
        service.GetCurrentTenantId().Should().Be(tenantId);
        service.GetCurrentUserId().Should().Be(userId);
    }

    [Fact]
    public void GetCurrentTenantId_ReturnsSetTenantId()
    {
        // Arrange
        var service = new TenantService();
        var expectedTenantId = Guid.NewGuid();
        service.SetTenantContext(expectedTenantId, "user-id");

        // Act
        var actualTenantId = service.GetCurrentTenantId();

        // Assert
        actualTenantId.Should().Be(expectedTenantId);
    }

    [Fact]
    public void GetCurrentUserId_ReturnsSetUserId()
    {
        // Arrange
        var service = new TenantService();
        var expectedUserId = "test-user-456";
        service.SetTenantContext(Guid.NewGuid(), expectedUserId);

        // Act
        var actualUserId = service.GetCurrentUserId();

        // Assert
        actualUserId.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetCurrentTenantId_BeforeSet_ReturnsEmptyGuid()
    {
        // Arrange
        var service = new TenantService();

        // Act
        var tenantId = service.GetCurrentTenantId();

        // Assert
        tenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetCurrentUserId_BeforeSet_ReturnsEmptyString()
    {
        // Arrange
        var service = new TenantService();

        // Act
        var userId = service.GetCurrentUserId();

        // Assert
        userId.Should().BeEmpty();
    }
}
