/**
 * QaliTrack Business Flow Functions
 * Contains all the business flow step functions that interact with the APIs
 */

// Level 0: Foundation Data Creation Functions
async function createLocationTypes() {
    await workflowManager.executeStep('location-types', async () => {
        const results = [];
        for (const item of SampleData.locationTypes) {
            const result = await workflowManager.apiClient.createLocationTypes(item);
            results.push(result);
            if (!result.success) break;
        }
        return { success: results.every(r => r.success), data: results };
    });
}

async function loadLocationTypes() {
    await workflowManager.executeStep('location-types', async () => {
        return await workflowManager.apiClient.getLocationTypes();
    });
}

async function createZones() {
    await workflowManager.executeStep('zones', async () => {
        const results = [];
        for (const item of SampleData.zones) {
            const result = await workflowManager.apiClient.createZones(item);
            results.push(result);
            if (!result.success) break;
        }
        return { success: results.every(r => r.success), data: results };
    });
}

async function loadZones() {
    await workflowManager.executeStep('zones', async () => {
        return await workflowManager.apiClient.getZones();
    });
}

async function createProductCategories() {
    await workflowManager.executeStep('product-categories', async () => {
        return { 
            success: false, 
            data: 'Product Categories API endpoint not yet implemented in ProductCatalog controller' 
        };
    });
}

async function loadProductCategories() {
    await workflowManager.executeStep('product-categories', async () => {
        return { 
            success: false, 
            data: 'Product Categories API endpoint not yet implemented in ProductCatalog controller' 
        };
    });
}

async function createPackagingTypes() {
    await workflowManager.executeStep('packaging-types', async () => {
        return { 
            success: false, 
            data: 'Packaging Types API endpoint not yet implemented in ProductCatalog controller' 
        };
    });
}

async function loadPackagingTypes() {
    await workflowManager.executeStep('packaging-types', async () => {
        return { 
            success: false, 
            data: 'Packaging Types API endpoint not yet implemented in ProductCatalog controller' 
        };
    });
}

// Level 1: Business Configuration Functions
async function createSites() {
    await workflowManager.executeStep('sites', async () => {
        // Check dependencies first
        const dependencies = await workflowManager.checkDependencies({
            zones: () => workflowManager.apiClient.getZones(),
            locationTypes: () => workflowManager.apiClient.getLocationTypes()
        });

        if (!dependencies.zones || !dependencies.locationTypes) {
            return {
                success: false,
                data: 'Prerequisites not found: Create Zones and Location Types first'
            };
        }

        const zones = dependencies.zones.data || [];
        const locationTypes = dependencies.locationTypes.data || [];

        if (zones.length === 0 || locationTypes.length === 0) {
            return {
                success: false,
                data: 'No zones or location types available. Create them first.'
            };
        }

        // Create sites using sample data function
        const sitesData = SampleData.getSites(locationTypes, zones);
        const results = [];
        
        for (const item of sitesData) {
            const result = await workflowManager.apiClient.createSites(item);
            results.push(result);
            if (!result.success) break;
        }

        return { success: results.every(r => r.success), data: results };
    });
}

async function loadSites() {
    await workflowManager.executeStep('sites', async () => {
        return await workflowManager.apiClient.getSites();
    });
}

async function createBusinessEntities() {
    await workflowManager.executeStep('business-entities', async () => {
        const results = [];
        for (const item of SampleData.businessEntities) {
            const result = await workflowManager.apiClient.createBusinessEntity(item);
            results.push(result);
            if (!result.success) break;
        }
        return { success: results.every(r => r.success), data: results };
    });
}

async function loadBusinessEntities() {
    await workflowManager.executeStep('business-entities', async () => {
        return await workflowManager.apiClient.getBusinessEntities();
    });
}

async function createVehiclesDrivers() {
    await workflowManager.executeStep('vehicles-drivers', async () => {
        const results = { vehicles: [], drivers: [] };
        
        // Create vehicles
        for (const vehicle of SampleData.vehicles) {
            const result = await workflowManager.apiClient.createVehicle(vehicle);
            results.vehicles.push(result);
            if (!result.success) {
                return { success: false, data: result };
            }
        }

        // Create drivers
        for (const driver of SampleData.drivers) {
            const result = await workflowManager.apiClient.createDriver(driver);
            results.drivers.push(result);
            if (!result.success) {
                return { success: false, data: result };
            }
        }

        return { success: true, data: results };
    });
}

async function loadVehiclesDrivers() {
    await workflowManager.executeStep('vehicles-drivers', async () => {
        const vehicleResult = await workflowManager.apiClient.getVehicles();
        const driverResult = await workflowManager.apiClient.getDrivers();
        
        if (vehicleResult.success && driverResult.success) {
            return {
                success: true,
                data: { vehicles: vehicleResult.data, drivers: driverResult.data }
            };
        } else {
            return {
                success: false,
                data: 'Error loading vehicles or drivers'
            };
        }
    });
}

// Level 2: Operational Configuration Functions
async function createWeighbridges() {
    await workflowManager.executeStep('weighbridges', async () => {
        // Check if sites exist
        const sitesResult = await workflowManager.apiClient.getSites();
        if (!sitesResult.success) {
            return {
                success: false,
                data: 'Cannot create weighbridges: Sites not found. Create sites first.'
            };
        }

        const sites = sitesResult.data.data || [];
        if (sites.length === 0) {
            return {
                success: false,
                data: 'No sites available. Create sites first.'
            };
        }

        return {
            success: false,
            data: 'Weighbridge creation API endpoints available in HardwareManagement controller but need implementation'
        };
    });
}

async function loadWeighbridges() {
    await workflowManager.executeStep('weighbridges', async () => {
        return {
            success: false,
            data: 'Weighbridge loading API endpoints available in HardwareManagement controller but need implementation'
        };
    });
}

// Level 3: Business Orders & Transactions Functions
async function createCustomerOrders() {
    await workflowManager.executeStep('customer-orders', async () => {
        // Check dependencies
        const dependencies = await workflowManager.checkDependencies({
            businessEntities: () => workflowManager.apiClient.getBusinessEntities(),
            sites: () => workflowManager.apiClient.getSites()
        });

        if (!dependencies.businessEntities || !dependencies.sites) {
            return {
                success: false,
                data: 'Prerequisites not found: Create Business Entities and Sites first'
            };
        }

        const customers = (dependencies.businessEntities.data || []).filter(be => be.entityType === 'Customer');
        const sites = dependencies.sites.data || [];

        if (customers.length === 0 || sites.length === 0) {
            return {
                success: false,
                data: 'No customers or sites available. Create them first.'
            };
        }

        // Create orders using sample data function
        const ordersData = SampleData.getCustomerOrders(customers, sites);
        const results = [];
        
        for (const order of ordersData) {
            const result = await workflowManager.apiClient.createCustomerOrder(order);
            results.push(result);
            if (!result.success) break;
        }

        return { success: results.every(r => r.success), data: results };
    });
}

async function loadCustomerOrders() {
    await workflowManager.executeStep('customer-orders', async () => {
        return await workflowManager.apiClient.getCustomerOrders();
    });
}

async function createPurchaseOrders() {
    await workflowManager.executeStep('purchase-orders', async () => {
        // Check dependencies
        const dependencies = await workflowManager.checkDependencies({
            businessEntities: () => workflowManager.apiClient.getBusinessEntities(),
            sites: () => workflowManager.apiClient.getSites()
        });

        if (!dependencies.businessEntities || !dependencies.sites) {
            return {
                success: false,
                data: 'Prerequisites not found: Create Business Entities and Sites first'
            };
        }

        const suppliers = (dependencies.businessEntities.data || []).filter(be => be.entityType === 'Supplier');
        const sites = dependencies.sites.data || [];

        if (suppliers.length === 0 || sites.length === 0) {
            return {
                success: false,
                data: 'No suppliers or sites available. Create them first.'
            };
        }

        // Create orders using sample data function
        const ordersData = SampleData.getPurchaseOrders(suppliers, sites);
        const results = [];
        
        for (const order of ordersData) {
            const result = await workflowManager.apiClient.createPurchaseOrder(order);
            results.push(result);
            if (!result.success) break;
        }

        return { success: results.every(r => r.success), data: results };
    });
}

async function loadPurchaseOrders() {
    await workflowManager.executeStep('purchase-orders', async () => {
        return await workflowManager.apiClient.getPurchaseOrders();
    });
}

async function createWeighingTransactions() {
    await workflowManager.executeStep('weighing-transactions', async () => {
        // Check all dependencies
        const dependencies = await workflowManager.checkDependencies({
            customerOrders: () => workflowManager.apiClient.getCustomerOrders(),
            sites: () => workflowManager.apiClient.getSites(),
            vehicles: () => workflowManager.apiClient.getVehicles(),
            drivers: () => workflowManager.apiClient.getDrivers()
        });

        // Validate all dependencies exist
        const missingDeps = [];
        if (!dependencies.customerOrders) missingDeps.push('Customer Orders');
        if (!dependencies.sites) missingDeps.push('Sites');
        if (!dependencies.vehicles) missingDeps.push('Vehicles');
        if (!dependencies.drivers) missingDeps.push('Drivers');

        if (missingDeps.length > 0) {
            return {
                success: false,
                data: `Prerequisites not found: Create ${missingDeps.join(', ')} first`
            };
        }

        // Extract data arrays
        const orders = dependencies.customerOrders.data || [];
        const sites = dependencies.sites.data || [];
        const vehicles = dependencies.vehicles.data || [];
        const drivers = dependencies.drivers.data || [];

        if (orders.length === 0 || sites.length === 0 || vehicles.length === 0 || drivers.length === 0) {
            return {
                success: false,
                data: 'Insufficient data available. Ensure all prerequisites are created with data.'
            };
        }

        // Create sample transactions
        const results = [];
        const sampleOrder = orders[0];
        const sampleSite = sites[0];
        const sampleVehicle = vehicles[0];
        const sampleDriver = drivers[0];

        // For now, we'll assume weighbridgeId needs to be provided or mocked
        const transactionData = SampleData.generateWeighingTransaction(
            sampleOrder.id,
            sampleSite.id,
            'mock-weighbridge-id', // This would need actual weighbridge data
            sampleVehicle.id,
            sampleDriver.id
        );

        const result = await workflowManager.apiClient.createWeighingTransaction(transactionData);
        results.push(result);

        return { success: result.success, data: results };
    });
}

async function loadWeighingTransactions() {
    await workflowManager.executeStep('weighing-transactions', async () => {
        return await workflowManager.apiClient.getWeighingTransactions();
    });
}

// Workflow Action Functions
async function createAllSampleData() {
    workflowManager.showNotification('Starting complete workflow data creation...', 'info');
    
    const steps = [
        { name: 'Location Types', fn: createLocationTypes },
        { name: 'Zones', fn: createZones },
        { name: 'Product Categories', fn: createProductCategories },
        { name: 'Packaging Types', fn: createPackagingTypes },
        { name: 'Sites', fn: createSites },
        { name: 'Business Entities', fn: createBusinessEntities },
        { name: 'Vehicles & Drivers', fn: createVehiclesDrivers },
        { name: 'Weighbridges', fn: createWeighbridges },
        { name: 'Customer Orders', fn: createCustomerOrders },
        { name: 'Purchase Orders', fn: createPurchaseOrders }
    ];

    for (const step of steps) {
        workflowManager.showNotification(`Creating ${step.name}...`, 'info');
        await step.fn();
        // Small delay between steps to avoid overwhelming the server
        await new Promise(resolve => setTimeout(resolve, 1000));
    }

    workflowManager.showNotification('Workflow data creation completed!', 'success');
}

async function loadAllData() {
    workflowManager.showNotification('Loading all existing data...', 'info');
    
    const steps = [
        { name: 'Location Types', fn: loadLocationTypes },
        { name: 'Zones', fn: loadZones },
        { name: 'Product Categories', fn: loadProductCategories },
        { name: 'Packaging Types', fn: loadPackagingTypes },
        { name: 'Sites', fn: loadSites },
        { name: 'Business Entities', fn: loadBusinessEntities },
        { name: 'Vehicles & Drivers', fn: loadVehiclesDrivers },
        { name: 'Weighbridges', fn: loadWeighbridges },
        { name: 'Customer Orders', fn: loadCustomerOrders },
        { name: 'Purchase Orders', fn: loadPurchaseOrders },
        { name: 'Weighing Transactions', fn: loadWeighingTransactions }
    ];

    for (const step of steps) {
        await step.fn();
        // Shorter delay for loading
        await new Promise(resolve => setTimeout(resolve, 500));
    }

    workflowManager.showNotification('Data loading completed!', 'success');
}

async function clearAllData() {
    if (!confirm('This will attempt to clear all test data from both APIs. This action cannot be undone. Continue?')) {
        return;
    }

    workflowManager.showNotification('Clear All Data functionality would require DELETE endpoints to be implemented', 'info');
    
    // Note: This would require implementing DELETE endpoints in the APIs
    // For now, we just show a notification about the limitation
}