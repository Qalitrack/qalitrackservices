using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ArchiveService.Core.Interfaces;
using ArchiveService.Core.Services;
using ArchiveService.Infrastructure.Data;
using ArchiveService.Infrastructure.Repositories;

namespace ArchiveService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ArchiveDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=archive.db"));

            // Add Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IArchiveMetadataRepository, ArchiveMetadataRepository>();
            services.AddScoped<IRetentionPolicyRepository, RetentionPolicyRepository>();
            services.AddScoped<IDataMigrationRepository, DataMigrationRepository>();

            // Add Core Services
            services.AddScoped<IArchivalEngine, ArchivalEngine>();
            services.AddScoped<IRetentionPolicyEngine, RetentionPolicyEngine>();
            services.AddScoped<IDataCompressionService, DataCompressionService>();
            services.AddScoped<IArchiveSearchService, ArchiveSearchService>();

            return services;
        }
    }
}