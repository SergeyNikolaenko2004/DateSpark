using DateSpark.API.Data;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIdeaGeneratorService, IdeaGeneratorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdventureService, AdventureService>();

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

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("DateSparkDB"));
}
else
{
    if (connectionString.Contains("postgresql://"))
    {
        try
        {
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
            }
        }
        catch
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("DateSparkDB"));
        }
    }

    if (!connectionString.Contains("InMemory"))
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
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
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
        try
        {
            dbContext.Database.Migrate();
        }
        catch
        {
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();