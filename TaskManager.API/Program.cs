using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TaskManager.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console().WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
// Removed Swagger/OpenAPI to bypass the .NET 10 SDK bug

// 2. Entity Framework Setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. JWT Authentication Setup
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretFallbackKeyForDevelopment123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagerAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagerClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

app.UseHttpsRedirection();

// Auth middleware must remain here
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();