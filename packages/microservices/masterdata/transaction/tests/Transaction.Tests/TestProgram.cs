using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Transaction.Infrastructure.Data;

namespace Transaction.Tests;

public class TestProgram
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TestStartup>();
            });
}

public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // This will be overridden in the test
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Minimal configuration needed for tests
        app.UseRouting();
        
        // Skip DataLeakPreventionMiddleware in tests
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
