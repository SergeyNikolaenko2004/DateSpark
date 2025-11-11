using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIdeaGeneratorService, IdeaGeneratorService>();
builder.Services.AddControllers();

// 🔥 ИСПРАВЛЕННАЯ КОНФИГУРАЦИЯ БАЗЫ ДАННЫХ
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
    // ПАРСИНГ ДЛЯ RENDER.COM И ЛОКАЛЬНОЙ РАЗРАБОТКИ
    if (connectionString.Contains("postgresql://"))
    {
        // Формат Render.com: postgresql://user:pass@host/dbname
        try
        {
            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(':');

            connectionString = $"Host={databaseUri.Host};" +
                $"Port=5432;" +
                $"Database={databaseUri.LocalPath.TrimStart('/')};" +
                $"Username={userInfo[0]};" +
                $"Password={userInfo[1]};" +
                "SSL Mode=Require;Trust Server Certificate=true";

            Console.WriteLine($"✅ Using PostgreSQL on Render: {databaseUri.Host}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
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
            "https://sergeynikolaenko2004.github.io",
            "http://localhost:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// 🔥 АВТО-МИГРАЦИЯ ТОЛЬКО ДЛЯ POSTGRESQL
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
            
            // Проверяем подключение
            var canConnect = dbContext.Database.CanConnect();
            Console.WriteLine($"📊 Database connection: {canConnect}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("🔄 InMemory database - skipping migrations");
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();