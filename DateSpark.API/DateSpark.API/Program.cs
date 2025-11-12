using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Services;
using DateSpark.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIdeaGeneratorService, IdeaGeneratorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "DateSpark",
            ValidAudience = "DateSparkUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your-super-secret-key-at-least-32-chars-long!"))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 🔥 УЛУЧШЕННАЯ КОНФИГУРАЦИЯ БАЗЫ ДАННЫХ ДЛЯ SUPABASE
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

// Для миграций используем appsettings.Development.json
if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("🛠️ Using Development connection string for migrations");
}

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback для случаев когда нет подключения
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("DateSparkDB"));
    Console.WriteLine("🔄 Using InMemory database (fallback)");
}
else
{
    // 🔥 ИСПРАВЛЕННЫЙ ПАРСИНГ ДЛЯ SUPABASE
    if (connectionString.Contains("postgresql://"))
    {
        try
        {
            // Убираем "postgresql://" и парсим вручную для совместимости
            var uriString = connectionString.Replace("postgresql://", "");
            var atIndex = uriString.IndexOf('@');
            var colonIndex = uriString.IndexOf(':');
            
            if (atIndex > 0 && colonIndex > 0)
            {
                var userInfo = uriString.Substring(0, atIndex);
                var hostAndDb = uriString.Substring(atIndex + 1);
                
                var userParts = userInfo.Split(':');
                var username = userParts[0];
                var password = userParts[1];
                
                var hostParts = hostAndDb.Split('/');
                var hostWithPort = hostParts[0];
                var database = hostParts[1];
                
                var host = hostWithPort.Split(':')[0];
                
                connectionString = $"Host={host};" +
                    $"Port=5432;" +
                    $"Database={database};" +
                    $"Username={username};" +
                    $"Password={password};" +
                    "SSL Mode=Require;Trust Server Certificate=true";

                Console.WriteLine($"✅ Using PostgreSQL on Supabase: {host}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
            Console.WriteLine($"🔍 Original string: {connectionString}");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("DateSparkDB"));
            Console.WriteLine("🔄 Fallback to InMemory database");
        }
    }

    // Используем PostgreSQL
    if (!connectionString.Contains("InMemory"))
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        Console.WriteLine("🗄️ Using PostgreSQL database");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://datespark-frontend.onrender.com",
            "https://sergeynikolaenko2004.github.io",
            "http://localhost:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// 🔥 АВТО-МИГРАЦИЯ И SEED ДАННЫЕ ТОЛЬКО ДЛЯ POSTGRESQL
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (dbContext.Database.IsRelational())
    {
        try
        {
            Console.WriteLine("🚀 Applying database migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("✅ Database migrations applied successfully!");
            
            // 🔥 ДОБАВЛЯЕМ ТЕСТОВЫЕ ДАННЫЕ ТОЛЬКО ЕСЛИ БАЗА ПУСТАЯ
            if (!dbContext.Ideas.Any())
            {
                Console.WriteLine("🌱 Adding test data to empty database...");
                
            // В методе seed данных замени:
            var testIdeas = new List<Idea>
            {
                new Idea { 
                    Title = "Романтический ужин при свечах", 
                    Description = "Приготовить ужин вместе при свечах с любимой музыкой", 
                    Category = "Романтическое", 
                    PriceCategory = PriceCategory.Medium, 
                    Location = "Дома", 
                    Mood = "Романтическое", 
                    Duration = "Вечер", 
                    Weather = "Любая",
                    IsActive = true
                },
                new Idea { 
                    Title = "Пикник в парке", 
                    Description = "Устроить пикник с пледом и вкусной едой", 
                    Category = "Активное", 
                    PriceCategory = PriceCategory.Low,
                    Location = "Природа", 
                    Mood = "Расслабленное", 
                    Duration = "Короткое", 
                    Weather = "Только ясно",
                    IsActive = true
                }
            };

                dbContext.Ideas.AddRange(testIdeas);
                dbContext.SaveChanges();
                Console.WriteLine($"✅ Added {testIdeas.Count} test ideas to database");
            }
            else
            {
                var ideaCount = dbContext.Ideas.Count();
                Console.WriteLine($"📊 Database already contains {ideaCount} ideas - skipping seed data");
            }
            
            // Проверяем подключение
            var canConnect = dbContext.Database.CanConnect();
            Console.WriteLine($"📊 Database connection: {canConnect}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed: {ex.Message}");
            Console.WriteLine($"🔍 Full error: {ex}");
        }
    }
    else
    {
        Console.WriteLine("🔄 InMemory database - skipping migrations and seed data");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();