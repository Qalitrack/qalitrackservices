using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Infrastructure.Data;
using QaliTrack.MasterData.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Configure lowercase routing for Django-style URLs
    options.Conventions.Add(new Microsoft.AspNetCore.Mvc.ApplicationModels.RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
    
    // Add Django-style query parameter binding
    options.ModelBinderProviders.Insert(0, new QueryParametersModelBinderProvider());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// Configure Django-style modules (INSTALLED_APPS equivalent)
builder.Services.ConfigureModules();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "QaliTrack Master Data API", 
        Version = "v1",
        Description = "Consolidated Master Data Service with Django-style modular architecture"
    });
});

// Configure Database (Django-style with .env support)
builder.Services.ConfigureDatabase(builder.Configuration);

// Configure AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure FluentValidation (will add later)
// builder.Services.AddFluentValidationAutoValidation();
// builder.Services.AddFluentValidationClientsideAdapters();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QaliTrack Master Data API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at root
});

// Serve static files including documentation
app.UseStaticFiles();

// Configure documentation serving
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs")),
    RequestPath = "/docs"
});

// Documentation default route
app.MapGet("/docs", () => Results.Redirect("/docs/index.html"));
app.MapFallback("/docs/{**path}", async context =>
{
    var path = context.Request.Path.Value?.Replace("/docs/", "") ?? "index.html";
    if (string.IsNullOrEmpty(path) || path == "/")
        path = "index.html";
    
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs", path);
    if (File.Exists(filePath))
    {
        // Set proper Content-Type based on file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".yml" or ".yaml" => "text/yaml; charset=utf-8",
            _ => "application/octet-stream"
        };
        
        context.Response.ContentType = contentType;
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Documentation file not found");
    }
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MasterDataDbContext>();
    context.Database.EnsureCreated();
}

// app.UseHttpsRedirection(); // Removed to avoid HTTPS redirect warnings
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();