using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.Entities;

namespace MultiTenantSaaS.Infrastructure.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for audit log queries
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.TenantId);
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.EntityName);
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.PerformedAt);
        modelBuilder.Entity<AuditLog>().HasIndex(a => new { a.TenantId, a.EntityName, a.PerformedAt });
    }
}
