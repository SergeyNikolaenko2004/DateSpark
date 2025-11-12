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
        public DbSet<IdeaVote> IdeaVotes { get; set; }
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

            // Связь многие-ко-многим User-Couple
            modelBuilder.Entity<UserCouple>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UserCouple>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCouple>()
                .HasOne(uc => uc.Couple)
                .WithMany(c => c.UserCouples)
                .HasForeignKey(uc => uc.CoupleId);

            // Уникальная пара пользователь-пара
            modelBuilder.Entity<UserCouple>()
                .HasIndex(uc => new { uc.UserId, uc.CoupleId })
                .IsUnique();

            // 🔥 ДОБАВЬ КОНФИГУРАЦИЮ ДЛЯ IDEA VOTES (этого не хватает!)
            modelBuilder.Entity<IdeaVote>(entity =>
            {
                entity.HasKey(iv => iv.Id);
                
                // Связь с User
                entity.HasOne(iv => iv.User)
                    .WithMany()
                    .HasForeignKey(iv => iv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Связь с Idea  
                entity.HasOne(iv => iv.Idea)
                    .WithMany()
                    .HasForeignKey(iv => iv.IdeaId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Уникальность пары UserId + IdeaId (один пользователь - один голос за идею)
                entity.HasIndex(iv => new { iv.UserId, iv.IdeaId })
                    .IsUnique();
            });
        }
    }
}