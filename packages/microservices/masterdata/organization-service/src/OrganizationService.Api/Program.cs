using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Interfaces;
using OrganizationService.Core.Mappings;
using OrganizationService.Infrastructure.Data;
using OrganizationService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(OrganizationProfile));

// Add Entity Framework
builder.Services.AddDbContext<OrganizationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=organization.db"));

// Add repositories
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrganizationUserRepository, OrganizationUserRepository>();
builder.Services.AddScoped<IOrganizationSettingsRepository, OrganizationSettingsRepository>();
builder.Services.AddScoped<IOrganizationDepartmentRepository, OrganizationDepartmentRepository>();
builder.Services.AddScoped<IOrganizationLocationRepository, OrganizationLocationRepository>();

// Add services
builder.Services.AddScoped<IOrganizationService, OrganizationService.Core.Services.OrganizationService>();

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrganizationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
