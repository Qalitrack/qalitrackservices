// using Microsoft.EntityFrameworkCore;
// using ComplianceService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database configuration commented out for basic startup
// builder.Services.AddDbContext<ComplianceDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=compliance.db"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Services commented out for basic startup

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance Service V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// app.UseHttpsRedirection(); // Commented out as requested

app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();