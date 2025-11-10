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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Дополнительные настройки моделей (опционально)
            modelBuilder.Entity<Idea>()
                .Property(i => i.Price)
                .HasPrecision(10, 2); // Для денежных значений
        }
    }
}