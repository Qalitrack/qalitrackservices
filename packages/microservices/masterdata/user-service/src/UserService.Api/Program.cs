using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using UserService.Core.Services;
using UserService.Core.Mappings;
using Microsoft.OpenApi.Models;
using UserService.Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Log debug messages at different points in the program startup
Log.Information("Starting application...");

// Configure to listen on port 8080
builder.WebHost.UseUrls("http://localhost:8080");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/user-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Log that Serilog has been set up
Log.Information("Serilog has been configured.");

// Add services to the container
builder.Services.AddControllers();
Log.Information("Controllers have been added to the DI container.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPermissionsService, PermissionsService>();
builder.Services.AddScoped<IUserService, UserService.Core.Services.UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

Log.Information("Services and repositories have been registered.");

// Add DbContext
builder.Services.AddDbContext<UserServiceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                      "Data Source=user-service.db", 
        b => b.MigrationsAssembly("UserService.Infrastructure")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "your-super-secret-key-here-minimum-32-characters");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "UserService",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "UserService",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
Log.Information("JWT Authentication has been configured.");

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "UserService API", 
        Version = "v1"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    Log.Information("Swagger has been configured with JWT authentication.");
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile));
Log.Information("AutoMapper has been configured.");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
Log.Information("CORS has been configured.");

builder.Services.AddHealthChecks();
Log.Information("Health checks have been configured.");

var app = builder.Build();

// Run migrations and seed the database
 using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    try
    {
        // Apply migrations
        dbContext.Database.Migrate();
        Log.Information("Migrations applied successfully.");
        
        // Seed the data (you can replace this with your custom seed method if needed)
        await PrepDb.PrepPopulation(app, isProduction: false);
        Log.Information("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying migrations or seeding the database.");
        throw;
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});
Log.Information("Swagger UI has been set up.");

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSerilogRequestLogging();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and health check endpoints
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Application is now running...");

Log.Information("UserService starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }