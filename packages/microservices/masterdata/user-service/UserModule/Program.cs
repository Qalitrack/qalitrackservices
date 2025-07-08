using System.Text;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using UserModule.Data;
using UserModule.Middleware;
using UserModule.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using UserModule.Authentication;
using UserModule.Authorization;
using UserModule.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (the 'Bearer ' prefix will be added automatically)"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register dependencies
builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("UserDatabase"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IUserShiftService, UserShiftService>();
// Configure Sanctum Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (jwtSecret == null)
{
    throw new ArgumentNullException($"Jwt:Secret configuration is missing.");
}
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Sanctum";
    options.DefaultChallengeScheme = "Sanctum";
})
.AddScheme<AuthenticationSchemeOptions, SanctumAuthenticationHandler>("Sanctum", options => { });

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "users.read", "users.create", "users.update", "users.delete",
        "users.assign-roles", "users.remove-roles", "users.read-permissions",
        "roles.read", "roles.create", "roles.update", "roles.delete", "roles.assign-permissions",
        "permissions.read", "permissions.create", "permissions.update", "permissions.delete",
        "shifts.read", "shifts.create", "shifts.update", "shifts.delete", "shifts.assign-user", "shifts.remove-user","shifts.mode.read","shifts.mode.update","shifts.read-user-shifts","Shifts.create-user-shifts","Shifts.update-user-shifts","Shifts.delete-user-shifts","Shifts.assign-user-shifts","Shifts.remove-user-shifts",
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy($"RequirePermission:{permission}", policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

// Uncomment the following lines to enable CORS for microservices  that are hosted on different domains
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowMicroservices", policy =>
//     {
//         policy.WithOrigins("http://other-microservice-domain:port") // Specify allowed origins
//             .AllowAnyMethod()
//             .AllowAnyHeader()
//             .AllowCredentials();
//     });
// });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseRouting();
// Uncomment the following line to enable CORS for microservices that are hosted on different domains
// app.UseCors("AllowMicroservices");
app.UseShiftEndLogout();
app.UseMiddleware<SanctumAuthenticationMiddleware>();
app.UseAuthentication();
app.UseShiftEndLogout(); // Add this line
app.UseAuthorization();
app.MapControllers();
PrepDb.PrepPopulation(app, app.Environment);
app.Run();