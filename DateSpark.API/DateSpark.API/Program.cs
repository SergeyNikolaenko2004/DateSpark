var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS - РАЗРЕШАЕМ ЗАПРОСЫ ОТ ФРОНТЕНДА
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://sergeynikolaenko2004.github.io",  // 👈 ТВОЙ ФРОНТЕНД
            "http://localhost:3000"                    // Для локальной разработки
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// ИСПОЛЬЗУЕМ CORS - ЭТО ВАЖНО!
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();