using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DateSpark.API.Models;

namespace DateSpark.API.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Если нет connection string, используем InMemory для дизайна
            if (string.IsNullOrEmpty(connectionString))
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseInMemoryDatabase("DesignTimeDB");
                return new AppDbContext(optionsBuilder.Options);
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}