using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Interfaces;
using SaccoService.Core.Mappings;
using SaccoService.Infrastructure.Data;
using SaccoService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(SaccoProfile));

// Add Entity Framework
builder.Services.AddDbContext<SaccoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sacco.db"));

// Add repositories
builder.Services.AddScoped<ISaccoRepository, SaccoRepository>();
builder.Services.AddScoped<ISaccoMemberRepository, SaccoMemberRepository>();
builder.Services.AddScoped<ISaccoCommitteeRepository, SaccoCommitteeRepository>();
builder.Services.AddScoped<ISaccoMeetingRepository, SaccoMeetingRepository>();
builder.Services.AddScoped<ISaccoFinancialRepository, SaccoFinancialRepository>();
builder.Services.AddScoped<ISaccoShareRepository, SaccoShareRepository>();
builder.Services.AddScoped<ISaccoLoanRepository, SaccoLoanRepository>();

// Add services
builder.Services.AddScoped<ISaccoService, SaccoService.Core.Services.SaccoService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SaccoDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sacco Service API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

// Comment out HTTPS redirect as requested
// app.UseHttpsRedirection();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
