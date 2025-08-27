using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace QaliTrack.MasterData.Api.Infrastructure;

public static class ModuleConfiguration
{
    public static void ConfigureModules(this IServiceCollection services)
    {
        // Load enabled modules from environment (Django INSTALLED_APPS equivalent)
        var enabledModulesString = Environment.GetEnvironmentVariable("ENABLED_MODULES") 
            ?? "Organization,Driver,Vehicle,BusinessEntities,SACCO,Product,Route,Weighbridge";
        
        var enabledModules = enabledModulesString
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new ModuleControllerConvention(enabledModules));
        });

        // Register module-specific services based on enabled modules
        RegisterModuleServices(services, enabledModules);
    }

    private static void RegisterModuleServices(IServiceCollection services, HashSet<string> enabledModules)
    {
        // Register services for each enabled module
        if (enabledModules.Contains("Organization"))
        {
            // Register Organization module services
            // services.AddScoped<IOrganizationService, OrganizationService>();
        }

        if (enabledModules.Contains("Driver"))
        {
            // Register Driver module services
            // services.AddScoped<IDriverService, DriverService>();
        }

        if (enabledModules.Contains("Vehicle"))
        {
            // Register Vehicle module services
            // services.AddScoped<IVehicleService, VehicleService>();
        }

        // Add more modules as needed...
    }
}

public class ModuleControllerConvention : IApplicationModelConvention
{
    private readonly HashSet<string> _enabledModules;

    public ModuleControllerConvention(HashSet<string> enabledModules)
    {
        _enabledModules = enabledModules;
    }

    public void Apply(ApplicationModel application)
    {
        var controllersToRemove = new List<ControllerModel>();

        foreach (var controller in application.Controllers)
        {
            var controllerName = controller.ControllerName;
            
            // Check if this controller's module is enabled
            if (!IsModuleEnabled(controllerName))
            {
                controllersToRemove.Add(controller);
            }
        }

        // Remove controllers for disabled modules
        foreach (var controller in controllersToRemove)
        {
            application.Controllers.Remove(controller);
        }
    }

    private bool IsModuleEnabled(string controllerName)
    {
        // Map controller names to module names
        var moduleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Organization", "Organization" },
            { "Driver", "Driver" },
            { "Vehicle", "Vehicle" },
            { "Customer", "BusinessEntities" },
            { "Supplier", "BusinessEntities" },
            { "Transporter", "BusinessEntities" },
            { "BusinessEntity", "BusinessEntities" },
            { "SACCO", "SACCO" },
            { "Product", "Product" },
            { "Route", "Route" },
            { "Weighbridge", "Weighbridge" },
            { "User", "User" },
            { "Report", "Report" }
        };

        if (moduleMapping.TryGetValue(controllerName, out var moduleName))
        {
            return _enabledModules.Contains(moduleName);
        }

        // If not mapped, assume it's enabled (for backwards compatibility)
        return true;
    }
}