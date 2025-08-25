using Microsoft.EntityFrameworkCore;
using RouteService.Core.Interfaces;
using RouteService.Core.Services;
using RouteService.Infrastructure.Data;
using RouteService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(RouteService.Core.Mappings.RouteProfile));

// Add Entity Framework
builder.Services.AddDbContext<RouteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=route.db"));

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteWaypointRepository, RouteWaypointRepository>();
builder.Services.AddScoped<IRouteRestrictionRepository, RouteRestrictionRepository>();
builder.Services.AddScoped<IRouteConditionRepository, RouteConditionRepository>();

// Placeholder repository registrations for the remaining repositories
// In a full implementation, these would be actual repository implementations with full functionality
builder.Services.AddScoped<IRouteTollRepository, RouteTollRepository>();
builder.Services.AddScoped<IRoutePerformanceRepository, RoutePerformanceRepository>();
builder.Services.AddScoped<IRouteHazmatRepository, RouteHazmatRepository>();
builder.Services.AddScoped<IRouteScheduleRepository, RouteScheduleRepository>();

// Add services
builder.Services.AddScoped<IRouteService, RouteService.Core.Services.RouteService>();

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

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Route Service API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RouteDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();