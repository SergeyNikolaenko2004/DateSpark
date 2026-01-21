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
    public DbSet<User> Users { get; set; }
    public DbSet<Couple> Couples { get; set; }
    public DbSet<UserCouple> UserCouples { get; set; }

    public DbSet<AdventureCard> AdventureCards { get; set; }

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
            .WithMany(u => u.UserCouples)
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
        
        // 🔥 НОВАЯ КОНФИГУРАЦИЯ: AdventureCard
        modelBuilder.Entity<AdventureCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Связь с Idea (необязательная)
            entity.HasOne(e => e.Idea)
                  .WithMany()
                  .HasForeignKey(e => e.IdeaId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Связь с Couple (обязательная)
            entity.HasOne(e => e.Couple)
                  .WithMany(c => c.AdventureCards)
                  .HasForeignKey(e => e.CoupleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с User (кто создал)
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Индексы для производительности
            entity.HasIndex(e => e.CoupleId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PlannedDate);
            
            // Ограничение длины
            entity.Property(e => e.Title)
                  .HasMaxLength(100);
        });
    }
}
}