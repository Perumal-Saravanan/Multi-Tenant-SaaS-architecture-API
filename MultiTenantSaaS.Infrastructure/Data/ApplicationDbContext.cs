using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Core.Interfaces;
using MultiTenantSaaS.Infrastructure.Services;

namespace MultiTenantSaaS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite key for UserRole
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Configure relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // CRITICAL: Global Query Filter for Tenant Isolation
        // This automatically filters all queries by the current tenant
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
        modelBuilder.Entity<TaskItem>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Full system access" },
            new Role { Id = 2, Name = "Manager", Description = "Can manage team tasks" },
            new Role { Id = 3, Name = "User", Description = "Basic user access" }
        );

        // Configure indexes for performance
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.TenantId);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.TenantId);
        modelBuilder.Entity<Category>().HasIndex(c => c.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-populate audit fields
        var entries = ChangeTracker.Entries<IAuditable>();
        var currentUser = _tenantService.GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = currentUser;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
