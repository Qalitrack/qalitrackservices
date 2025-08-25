using TransactionService.Api.Extensions;
using TransactionService.Api.Middleware;
using TransactionService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Add API services
builder.Services.AddApiServices();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Migrate database
await app.Services.MigrateDatabase();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction Service API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

app.UseCors("AllowAll");
app.UseResponseCaching();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }