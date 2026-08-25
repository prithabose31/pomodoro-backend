using Microsoft.EntityFrameworkCore;
using PomodoroApp.Core.Entities;

namespace PomodoroApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<Subtask> Subtasks { get; set; } = null!;

        public DbSet<PomodoroSession> PomodoroSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User Table ──────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(500);

                entity.Property(u => u.AuthProvider)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(u => u.ProviderId)
                    .HasMaxLength(255);

                entity.Property(u => u.CreatedAt)
                    .IsRequired();

                entity.Property(u => u.UpdatedAt)
                    .IsRequired();
            });

            // ── RefreshToken Table ───────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.TokenHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(rt => rt.ExpiresAt)
                    .IsRequired();

                entity.Property(rt => rt.CreatedAt)
                    .IsRequired();

                entity.Property(rt => rt.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Relationship — RefreshToken belongs to User
                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshToken)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Category Table ───────────────────────────────────
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                entity.Property(c => c.Emoji)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(c => c.Color)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(c => c.CreatedAt).IsRequired();
                entity.Property(c => c.UpdatedAt).IsRequired();

                // One user has many categories
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── TaskItem Table ───────────────────────────────────
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(t => t.WeeklyGoalMinutes)
                    .HasDefaultValue(0);

                entity.Property(t => t.TimeSpentMinutes)
                    .HasDefaultValue(0);

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.NoAction); 


                entity.HasOne(t => t.Category)
                    .WithMany(c => c.Tasks)  // ← change WithMany() to WithMany(c => c.Tasks)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ── Subtask Table ─────────────────────────────────────
            modelBuilder.Entity<Subtask>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(s => s.Task)
                    .WithMany(t => t.Subtasks)
                    .HasForeignKey(s => s.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PomodoroSession>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Task)
                    .WithMany()
                    .HasForeignKey(p => p.TaskId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}