using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Masterdata.Core.Entities;
using Masterdata.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Masterdata.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(MasterdataDbContext context)
    {
        try
        {
            // Seed all entities in order of dependencies
            await SeedAxleConfigurationsAsync(context);
            await SeedOwnersAsync(context);
            await SeedSuppliersAsync(context);
            await SeedCustomersAsync(context);
            await SeedTransportersAsync(context);
            await SeedDriversAsync(context);
            await SeedVehiclesAsync(context);
            await SeedProductsAsync(context);
            await SeedRoutesAsync(context);
            await SeedSaccosAsync(context);
            await SeedOrganisationsAsync(context);
            await SeedAffiliationsAsync(context);
            await SeedWeighbridgesAsync(context);
            await SeedAuditLogsAsync(context);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to seed database", ex);
        }
    }

    private static async Task SeedAxleConfigurationsAsync(MasterdataDbContext context)
    {
        // Use Find or direct check instead of complex LINQ
        var existingConfig = await context.AxleConfigurations
            .FirstOrDefaultAsync(a => a.Configuration == "4x2");
            
        if (existingConfig == null)
        {
            var configurations = new List<AxleConfiguration>
            {
                new AxleConfiguration
                {
                    Id = Guid.NewGuid().ToString(),
                    Configuration = "4x2",
                    Description = "Standard two-axle configuration with four wheels"
                },
                new AxleConfiguration
                {
                    Id = Guid.NewGuid().ToString(),
                    Configuration = "6x4",
                    Description = "Three-axle configuration with six wheels"
                }
            };
            
            await context.AxleConfigurations.AddRangeAsync(configurations);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedOwnersAsync(MasterdataDbContext context)
    {
        var existingOwner = await context.Owners
            .FirstOrDefaultAsync(o => o.Name == "John Smith Logistics");
            
        if (existingOwner == null)
        {
            var owners = new List<Owner>
            {
                new Owner
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "John Smith Logistics",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "john.smith@logistics.com", Phone = "+254123456789" }),
                    Type = "Company"
                },
                new Owner
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Jane Doe",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "jane.doe@gmail.com", Phone = "+254987654321" }),
                    Type = "Individual"
                }
            };
            
            await context.Owners.AddRangeAsync(owners);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSuppliersAsync(MasterdataDbContext context)
    {
        var existingSupplier = await context.Suppliers
            .FirstOrDefaultAsync(s => s.Name == "Acme Supplies Ltd");
            
        if (existingSupplier == null)
        {
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Acme Supplies Ltd",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "contact@acmesupplies.com", Phone = "+254111222333", Address = "Nairobi, Kenya" }),
                    Status = "active",
                    Logo = "https://example.com/logos/acme.png"
                },
                new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Global Goods Co",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "info@globalgoods.com", Phone = "+254444555666", Address = "Mombasa, Kenya" }),
                    Status = "active",
                    Logo = "https://example.com/logos/global.png"
                }
            };
            
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCustomersAsync(MasterdataDbContext context)
    {
        var existingCustomer = await context.Customers
            .FirstOrDefaultAsync(c => c.Name == "Nairobi Distributors");
            
        if (existingCustomer == null)
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nairobi Distributors",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "sales@nairobidis.co.ke", Phone = "+254700111222", Address = "Nairobi, Kenya" }),
                    Status = "active"
                },
                new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Coast Imports",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "info@coastimports.co.ke", Phone = "+254700333444", Address = "Mombasa, Kenya" }),
                    Status = "active"
                }
            };
            
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTransportersAsync(MasterdataDbContext context)
    {
        var existingTransporter = await context.Transporters
            .FirstOrDefaultAsync(t => t.Name == "Swift Trans Ltd");
            
        if (existingTransporter == null)
        {
            var transporters = new List<Transporter>
            {
                new Transporter
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Swift Trans Ltd",
                    ContactInfo = JsonSerializer.Serialize(new { 
                        Email = "ops@swifttrans.com", 
                        Phone = "+254555666777", 
                        Address = "Nairobi, Kenya",
                        LicenseNumber = "TRN-001"
                    }),
                    Status = "active",
                    Logo = "logos/swift_trans.png"
                },
                new Transporter
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Coastal Hauliers",
                    ContactInfo = JsonSerializer.Serialize(new { 
                        Email = "info@coastalhauliers.co.ke", 
                        Phone = "+254555888999", 
                        Address = "Mombasa, Kenya",
                        LicenseNumber = "TRN-002"
                    }),
                    Status = "active",
                    Logo = "logos/coastal_hauliers.png"
                }
            };
            
            await context.Transporters.AddRangeAsync(transporters);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDriversAsync(MasterdataDbContext context)
    {
        var existingDriver = await context.Drivers
            .FirstOrDefaultAsync(d => d.LicenseNumber == "DL123456");
            
        if (existingDriver == null)
        {
            var transporter = await context.Transporters.FirstOrDefaultAsync();
            var supplier = await context.Suppliers.FirstOrDefaultAsync();
            
            if (transporter != null)
            {
                var drivers = new List<Driver>
                {
                    new Driver
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Michael Otieno",
                        LicenseNumber = "DL123456",
                        LicenseExpiryDate = DateTime.UtcNow.AddYears(2),
                        Status = "active",
                        TransporterId = transporter.Id,
                        SupplierId = supplier?.Id
                    },
                    new Driver
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Sarah Wanjiku",
                        LicenseNumber = "DL789012",
                        LicenseExpiryDate = DateTime.UtcNow.AddYears(3),
                        Status = "active",
                        TransporterId = transporter.Id
                    }
                };
                
                await context.Drivers.AddRangeAsync(drivers);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedVehiclesAsync(MasterdataDbContext context)
    {
        var existingVehicle = await context.Vehicles
            .FirstOrDefaultAsync(v => v.RegistrationNumber == "KBC 123A");
            
        if (existingVehicle == null)
        {
            var owner = await context.Owners.FirstOrDefaultAsync();
            var supplier = await context.Suppliers.FirstOrDefaultAsync();
            var transporter = await context.Transporters.FirstOrDefaultAsync();
            var axleConfig = await context.AxleConfigurations.FirstOrDefaultAsync();

            if (owner != null && axleConfig != null)
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        Id = Guid.NewGuid().ToString(),
                        RegistrationNumber = "KBC 123A",
                        Type = "Truck",
                        Model = "Mercedes Actros 2020",
                        Color = "White",
                        Status = "active",
                        OwnerId = owner.Id,
                        SupplierId = supplier?.Id,
                        TransporterId = transporter?.Id,
                        AxleConfigurationId = axleConfig.Id
                    },
                    new Vehicle
                    {
                        Id = Guid.NewGuid().ToString(),
                        RegistrationNumber = "KBC 456B",
                        Type = "Truck",
                        Model = "Scania R500 2021",
                        Color = "Red",
                        Status = "active",
                        OwnerId = owner.Id,
                        SupplierId = supplier?.Id,
                        TransporterId = transporter?.Id,
                        AxleConfigurationId = axleConfig.Id
                    }
                };
                
                await context.Vehicles.AddRangeAsync(vehicles);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedProductsAsync(MasterdataDbContext context)
    {
        var existingProduct = await context.Products
            .FirstOrDefaultAsync(p => p.Code == "P001");
            
        if (existingProduct == null)
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = "P001",
                    Name = "Cement",
                    Description = "High-grade cement for construction",
                    Image = "https://example.com/images/cement.png"
                },
                new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = "P002",
                    Name = "Steel Bars",
                    Description = "Reinforcement steel bars",
                    Image = "https://example.com/images/steel.png"
                }
            };
            
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRoutesAsync(MasterdataDbContext context)
    {
        var existingRoute = await context.Routes
            .FirstOrDefaultAsync(r => r.Name == "Nairobi-Mombasa");
            
        if (existingRoute == null)
        {
            var routes = new List<Route>
            {
                new Route
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Nairobi-Mombasa",
                    StartPoint = "Nairobi",
                    EndPoint = "Mombasa",
                    Status = "active"
                },
                new Route
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Kisumu-Nakuru",
                    StartPoint = "Kisumu",
                    EndPoint = "Nakuru",
                    Status = "active"
                }
            };
            
            await context.Routes.AddRangeAsync(routes);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSaccosAsync(MasterdataDbContext context)
    {
        var existingSacco = await context.Saccos
            .FirstOrDefaultAsync(s => s.Name == "Unity Sacco");
            
        if (existingSacco == null)
        {
            var saccos = new List<Sacco>
            {
                new Sacco
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Unity Sacco",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "info@unitysacco.org", Phone = "+254111222333", Address = "Nairobi, Kenya" }),
                    OtherDetails = JsonSerializer.Serialize(new { RegistrationNumber = "SAC123", Members = 500 })
                },
                new Sacco
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Prosper Sacco",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "contact@prospersacco.org", Phone = "+254444555666", Address = "Eldoret, Kenya" }),
                    OtherDetails = JsonSerializer.Serialize(new { RegistrationNumber = "SAC456", Members = 300 })
                }
            };
            
            await context.Saccos.AddRangeAsync(saccos);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedOrganisationsAsync(MasterdataDbContext context)
    {
        var existingOrg = await context.Organisations
            .FirstOrDefaultAsync(o => o.Name == "Kenya Transport Association");
            
        if (existingOrg == null)
        {
            var organisations = new List<Organisation>
            {
                new Organisation
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Kenya Transport Association",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "info@kta.org", Phone = "+254777888999", Address = "Nairobi, Kenya" }),
                    Type = "Trade Association",
                    Status = "active"
                },
                new Organisation
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "East Africa Logistics Network",
                    ContactInfo = JsonSerializer.Serialize(new { Email = "contact@ealn.org", Phone = "+254222333444", Address = "Mombasa, Kenya" }),
                    Type = "Logistics Network",
                    Status = "active"
                }
            };
            
            await context.Organisations.AddRangeAsync(organisations);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAffiliationsAsync(MasterdataDbContext context)
    {
        var existingAffiliation = await context.Affiliations.FirstOrDefaultAsync();
            
        if (existingAffiliation == null)
        {
            var sacco = await context.Saccos.FirstOrDefaultAsync();
            var organisation = await context.Organisations.FirstOrDefaultAsync();

            if (sacco != null && organisation != null)
            {
                var affiliation = new Affiliation
                {
                    Id = Guid.NewGuid().ToString(),
                    SaccoId = sacco.Id,
                    OrganisationId = organisation.Id,
                    Type = "Membership",
                    Details = $"{sacco.Name} is a member of {organisation.Name}"
                };
                
                await context.Affiliations.AddAsync(affiliation);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedWeighbridgesAsync(MasterdataDbContext context)
    {
        var existingWeighbridge = await context.Weighbridges
            .FirstOrDefaultAsync(w => w.Location == "Athi River");
            
        if (existingWeighbridge == null)
        {
            var weighbridges = new List<Weighbridge>
            {
                new Weighbridge
                {
                    Id = Guid.NewGuid().ToString(),
                    Location = "Athi River",
                    Description = "Weighbridge at Athi River industrial area",
                    Status = "active"
                },
                new Weighbridge
                {
                    Id = Guid.NewGuid().ToString(),
                    Location = "Mlolongo",
                    Description = "Weighbridge on Mombasa Road",
                    Status = "active"
                }
            };
            
            await context.Weighbridges.AddRangeAsync(weighbridges);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAuditLogsAsync(MasterdataDbContext context)
    {
        var existingLog = await context.AuditLogs
            .FirstOrDefaultAsync(a => a.Action == "Initialize");
            
        if (existingLog == null)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync();
            var entityId = vehicle?.Id ?? "init";
            
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                EntityName = "Vehicle",
                EntityId = entityId,
                Action = "Initialize",
                NewValues = JsonSerializer.Serialize(new { Message = "Initial seeding of database" }),
                UserId = "system",
                UserName = "System",
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            
            await context.AuditLogs.AddAsync(auditLog);
            await context.SaveChangesAsync();
        }
    }
}