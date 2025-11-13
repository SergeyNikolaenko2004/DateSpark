using Microsoft.EntityFrameworkCore;
using DateSpark.API.Models;

namespace DateSpark.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Idea> Ideas { get; set; }
        // 🔥 УДАЛИ СТРОКУ: public DbSet<IdeaVote> IdeaVotes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Couple> Couples { get; set; }
        public DbSet<UserCouple> UserCouples { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Идеи
            modelBuilder.Entity<Idea>()
                .Property(i => i.PriceCategory)
                .HasConversion<int>();

            // Уникальный email пользователя
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Уникальный код приглашения
            modelBuilder.Entity<Couple>()
                .HasIndex(c => c.JoinCode)
                .IsUnique();

            // 🔥 ИСПРАВЛЕННАЯ СВЯЗЬ User-UserCouple
            modelBuilder.Entity<UserCouple>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UserCouple>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCouples)  // ← ИСПРАВЛЕНО: указать навигационное свойство
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCouple>()
                .HasOne(uc => uc.Couple)
                .WithMany(c => c.UserCouples)
                .HasForeignKey(uc => uc.CoupleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Уникальная пара пользователь-пара
            modelBuilder.Entity<UserCouple>()
                .HasIndex(uc => new { uc.UserId, uc.CoupleId })
                .IsUnique();
        }
    }
}