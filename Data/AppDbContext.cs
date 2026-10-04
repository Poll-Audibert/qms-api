using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches { get; set; }
    public DbSet<Counter> Counters { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<BranchService> BranchServices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserService> UserServices { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TellerSession> TellerSessions { get; set; }
    public DbSet<TellerStatusHistory> TellerStatusHistories { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Branch
        modelBuilder.Entity<Branch>()
            .Property(b => b.status)
            .HasConversion<string>();

        // Counter
        modelBuilder.Entity<Counter>()
            .Property(c => c.status)
            .HasConversion<string>();

        modelBuilder.Entity<Counter>()
            .Property(c => c.created_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Counter>()
            .HasOne(c => c.branch)
            .WithMany()
            .HasForeignKey(c => c.branch_id);

        // Service
        modelBuilder.Entity<Service>()
            .Property(s => s.status)
            .HasConversion<string>();

        modelBuilder.Entity<Service>()
            .Property(s => s.created_at)
            .HasDefaultValueSql("GETDATE()");

        // BranchService
        modelBuilder.Entity<BranchService>()
            .HasOne(bs => bs.branch)
            .WithMany()
            .HasForeignKey(bs => bs.branch_id);

        modelBuilder.Entity<BranchService>()
            .HasOne(bs => bs.service)
            .WithMany()
            .HasForeignKey(bs => bs.service_id);

        modelBuilder.Entity<BranchService>()
            .Property(bs => bs.created_at)
            .HasDefaultValueSql("GETDATE()");

        // User
        modelBuilder.Entity<User>()
            .Property(u => u.role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.status)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.created_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<User>()
            .HasOne(u => u.branch)
            .WithMany()
            .HasForeignKey(u => u.branch_id);

        // UserService
        modelBuilder.Entity<UserService>()
            .HasOne(us => us.user)
            .WithMany()
            .HasForeignKey(us => us.user_id);

        modelBuilder.Entity<UserService>()
            .HasOne(us => us.service)
            .WithMany()
            .HasForeignKey(us => us.service_id);

        modelBuilder.Entity<UserService>()
            .Property(us => us.created_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Device>()
            .Property(d => d.last_seen_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Device>()
            .Property(d => d.created_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Device>()
            .HasOne(d => d.branch)
            .WithMany()
            .HasForeignKey(d => d.branch_id);

        modelBuilder.Entity<Device>()
            .Property(d => d.status)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.service)
            .WithMany()
            .HasForeignKey(t => t.service_id);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.branch)
            .WithMany()
            .HasForeignKey(t => t.branch_id);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.user)
            .WithMany()
            .HasForeignKey(t => t.teller_id);


        modelBuilder.Entity<Ticket>()
            .Property(t => t.status)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(t => t.created_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Ticket>()
            .Property(t => t.called_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Ticket>()
            .Property(t => t.serving_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Ticket>()
            .Property(t => t.completed_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<TellerSession>()
            .Property(ts => ts.logged_in_at)
            .HasDefaultValueSql("GETDATE()");

         modelBuilder.Entity<TellerSession>()
            .Property(ts => ts.logged_out_at)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<TellerSession>()
            .Property(ts => ts.status)
            .HasConversion<string>();

        modelBuilder.Entity<TellerSession>()
            .HasOne(ts => ts.user)
            .WithMany()
            .HasForeignKey(ts => ts.user_id);

        modelBuilder.Entity<TellerStatusHistory>()
            .HasOne(h => h.user)
            .WithMany()
            .HasForeignKey(h => h.user_id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TellerStatusHistory>()
            .Property(h => h.status)
            .HasConversion<string>();

        modelBuilder.Entity<TellerStatusHistory>()
            .Property(h => h.changed_at)
            .HasDefaultValueSql("GETDATE()");


        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.user)
            .WithMany()
            .HasForeignKey(a => a.user_id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.branch)
            .WithMany()
            .HasForeignKey(a => a.branch_id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.created_at)
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

    }
}