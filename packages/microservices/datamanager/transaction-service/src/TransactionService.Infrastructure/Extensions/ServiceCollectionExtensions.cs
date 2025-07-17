using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Core.Interfaces;
using TransactionService.Core.Mappings;
using TransactionService.Core.Services;
using TransactionService.Core.Validators;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.ExternalServices;
using TransactionService.Infrastructure.Repositories;

namespace TransactionService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<TransactionDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                 "Data Source=transaction_service.db";
            options.UseSqlite(connectionString);
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(TransactionMappingProfile));

        // Add Services
        services.AddScoped<ITransactionService, Core.Services.TransactionService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IChargeService, ChargeService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IOrchestrationService, OrchestrationService>();

        // Add Validators
        services.AddValidatorsFromAssemblyContaining<CreateTransactionRequestValidator>();

        // Add HTTP Client for external services
        services.AddHttpClient<IMasterDataIntegrationService, MasterDataIntegrationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "TransactionService/1.0");
        });

        // Add External Services
        services.AddScoped<IMasterDataIntegrationService, MasterDataIntegrationService>();

        // Add Repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IChargeRepository, ChargeRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }

    public static async Task<IServiceProvider> MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log migration error
            Console.WriteLine($"Migration failed: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }
}