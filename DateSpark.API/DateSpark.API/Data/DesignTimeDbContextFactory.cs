using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DateSpark.API.Models;

namespace DateSpark.API.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Host=localhost;Port=5432;Database=datespark_migrations;Username=postgres;Password=postgres;";

            var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                try
                {
                    var databaseUri = new Uri(envConnectionString);
                    var userInfo = databaseUri.UserInfo.Split(':');

                    connectionString = $"Host={databaseUri.Host};" +
                        $"Port=5432;" +
                        $"Database={databaseUri.LocalPath.TrimStart('/')};" +
                        $"Username={userInfo[0]};" +
                        $"Password={userInfo[1]};" +
                        "SSL Mode=Require;Trust Server Certificate=true";
                }
                catch
                {
                    // В релизной версии не логируем ошибки парсинга
                }
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}