var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://SergeyNibolaenko2004.github.io",  // 👈 ТВОЙ ФРОНТЕНД
            "http://localhost:3000"                    // Для разработки
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.MapControllers();
app.Run();