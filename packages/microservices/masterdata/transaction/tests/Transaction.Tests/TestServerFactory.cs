using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Transaction.Api.Middleware;
using Transaction.Core.Interfaces;
using Transaction.Core.Mappings;
using Transaction.Core.Services;
using Transaction.Infrastructure.Data;

namespace Transaction.Tests
{
    public class TestServerFactory : WebApplicationFactory<Program>
    {
        // Keep a reference to the service provider to control its lifetime
        private IServiceProvider? _serviceProvider;
        private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString();
        
        // Method to get the database context for testing
        public TransactionDbContext GetDbContext()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider is not initialized. Call CreateClient first.");
                
            var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        }
        
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Create a new host with test configuration
            var host = builder.Build();
            
            // Store the service provider
            _serviceProvider = host.Services;
            
            // Ensure the database is created (safe for InMemory)
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
                
                // Only call EnsureCreated for InMemory databases
                if (!db.Database.IsRelational())
                {
                    db.Database.EnsureCreated();
                }
            }

            host.Start();
            return host;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                // Add test configuration if needed
                config.AddInMemoryCollection();
            });

            // Set the environment to Test
            builder.UseEnvironment("Test");

            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TransactionDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database with a unique name for each test
                services.AddDbContext<TransactionDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: _dbName);
                    // Disable lazy loading in tests
                    options.UseLazyLoadingProxies(false);
                }, ServiceLifetime.Singleton);  // Use singleton to ensure the same instance is used throughout the test
                
                // Ensure the database is created immediately
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<TransactionDbContext>();
                    db.Database.EnsureCreated();
                }

                // Register AutoMapper with the TransactionProfile
                services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<TransactionProfile>();
                });
                
                // Mock ITimeService to return fixed times for consistent testing
                var mockTimeService = new Mock<ITimeService>();
                var testTime = new DateTime(2025, 11, 13, 16, 26, 33, DateTimeKind.Utc);
                var eatOffset = TimeSpan.FromHours(3);
                
                mockTimeService.Setup(t => t.Now).Returns(() => testTime.Add(eatOffset));
                mockTimeService.Setup(t => t.UtcNow).Returns(() => testTime);
                mockTimeService.Setup(t => t.TimeZone).Returns(TimeZoneInfo.CreateCustomTimeZone(
                    "EAT", 
                    eatOffset, 
                    "East Africa Time", 
                    "EAT"));
                mockTimeService.Setup(t => t.ConvertFromUtc(It.IsAny<DateTime>()))
                    .Returns<DateTime>(dt => dt.Add(eatOffset));
                mockTimeService.Setup(t => t.ConvertToUtc(It.IsAny<DateTime>()))
                    .Returns<DateTime>(dt => dt.Subtract(eatOffset));
                
                services.AddScoped<ITimeService>(_ => mockTimeService.Object);
            });
        }
    }
}