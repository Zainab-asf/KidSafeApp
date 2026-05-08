using KidSafe.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidSafe.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>             Users             => Set<User>();
    public DbSet<FlaggedMessage>   FlaggedMessages   => Set<FlaggedMessage>();
    public DbSet<Reward>           Rewards           => Set<Reward>();
    public DbSet<AbuseReport>      AbuseReports      => Set<AbuseReport>();
    public DbSet<Complaint>        Complaints        => Set<Complaint>();
    public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        // User unique email
        m.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // FlaggedMessage → Sender
        m.Entity<FlaggedMessage>()
            .HasOne(f => f.Sender)
            .WithMany(u => u.SentFlaggedMessages)
            .HasForeignKey(f => f.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reward → User (1-1)
        m.Entity<Reward>()
            .HasOne(r => r.User)
            .WithOne(u => u.Reward)
            .HasForeignKey<Reward>(r => r.UserId);

        // AbuseReport → Reporter
        m.Entity<AbuseReport>()
            .HasOne(a => a.Reporter)
            .WithMany(u => u.AbuseReports)
            .HasForeignKey(a => a.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Complaint → User
        m.Entity<Complaint>()
            .HasOne(c => c.User)
            .WithMany(u => u.Complaints)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ModerationAction → Actor and Target (no cascade to avoid cycles)
        m.Entity<ModerationAction>()
            .HasOne(ma => ma.Actor)
            .WithMany()
            .HasForeignKey(ma => ma.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        m.Entity<ModerationAction>()
            .HasOne(ma => ma.TargetUser)
            .WithMany()
            .HasForeignKey(ma => ma.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
