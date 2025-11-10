using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddControllers();

// Add DbContext with PostgreSQL - ИСПРАВЛЕННАЯ ВЕРСИЯ
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    // Для локальной разработки - используем in-memory базу
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDB"));
    Console.WriteLine("Using InMemory database for development");
}
else
{
    // Для Render.com - правильное преобразование PostgreSQL URL
    try
    {
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var properConnectionString = $"Host={databaseUri.Host};" +
            $"Port={databaseUri.Port};" +
            $"Database={databaseUri.LocalPath.TrimStart('/')};" +
            $"Username={userInfo[0]};" +
            $"Password={userInfo[1]};" +
            "SSL Mode=Require;Trust Server Certificate=true";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(properConnectionString));
        
        Console.WriteLine("Using PostgreSQL database on Render");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        // Fallback to in-memory database
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDB"));
    }
}

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