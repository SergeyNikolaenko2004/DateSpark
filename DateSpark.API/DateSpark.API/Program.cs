using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIdeaGeneratorService, IdeaGeneratorService>();
builder.Services.AddControllers();

// Add DbContext with PostgreSQL - ИСПРАВЛЕННАЯ ВЕРСИЯ
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    // Для локальной разработки - используем in-memory базу
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("DateSparkDB"));

    Console.WriteLine("🔄 Using InMemory database for all operations");
}
else

/* {
    // Для Render.com - НОВЫЙ ПАРСИНГ БЕЗ ПОРТА
    try
    {
        // Новый формат: postgresql://user:pass@host/dbname (без порта)
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');

        // Используем стандартный порт PostgreSQL 5432
        var properConnectionString = $"Host={databaseUri.Host};" +
            $"Port=5432;" +  // 👈 ЯВНО УКАЗЫВАЕМ ПОРТ 5432
            $"Database={databaseUri.LocalPath.TrimStart('/')};" +
            $"Username={userInfo[0]};" +
            $"Password={userInfo[1]};" +
            "SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(properConnectionString));

        Console.WriteLine("✅ Using PostgreSQL database on Render");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
        // Fallback to in-memory database
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDB"));
        Console.WriteLine("🔄 Fallback to InMemory database");
    }
} */

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://sergeynikolaenko2004.github.io",
            "http://localhost:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Используем Swagger ВСЕГДА (не только для разработки)
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();