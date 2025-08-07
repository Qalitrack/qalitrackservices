const express = require('express');
const router = express.Router();
const path = require('path');
const fs = require('fs');
const { validateToken } = require('./auth');

// In-memory data stores (mock persistence)
let salesOrders = [];
let weightMeasurements = [];
let deliveries = [];
let plantTransfers = [];

// Load initial data
function loadInitialData() {
    try {
        const dataPath = path.join(__dirname, '../webapp/localService/data');
        
        // Load business partners
        const businessPartnersData = JSON.parse(fs.readFileSync(path.join(dataPath, 'BusinessPartners.json'), 'utf8'));
        
        // Load materials
        const materialsData = JSON.parse(fs.readFileSync(path.join(dataPath, 'Materials.json'), 'utf8'));
        
        // Load plants
        const plantsData = JSON.parse(fs.readFileSync(path.join(dataPath, 'Plants.json'), 'utf8'));
        
        // Load initial sales orders
        const salesOrdersData = JSON.parse(fs.readFileSync(path.join(dataPath, 'SalesOrders.json'), 'utf8'));
        salesOrders = salesOrdersData.value || salesOrdersData;

        // Initialize sample weight measurements
        initializeSampleWeightMeasurements();
        
        // Initialize sample deliveries
        initializeSampleDeliveries();
        
        console.log('✅ Initial data loaded successfully');
        
        return {
            businessPartners: businessPartnersData.value || businessPartnersData,
            materials: materialsData.value || materialsData,
            plants: plantsData.value || plantsData
        };
    } catch (error) {
        console.error('❌ Error loading initial data:', error.message);
        return {
            businessPartners: [],
            materials: [],
            plants: []
        };
    }
}

function initializeSampleWeightMeasurements() {
    weightMeasurements = [
        {
            MeasurementId: 'WM001',
            VehicleNumber: 'KCA 123A',
            DriverName: 'John Mwangi',
            PlantCode: '1000',
            WeighbridgeId: 'WB1',
            MeasurementType: 'Entry',
            Weight: 8250,
            TareWeight: 8250,
            NetWeight: 0,
            Timestamp: new Date('2025-08-06T08:00:00Z').toISOString(),
            SalesOrderNumber: 'SO001',
            DeliveryNumber: 'DEL001',
            Status: 'Pending_Exit',
            Operator: 'Mary Wanjiku'
        },
        {
            MeasurementId: 'WM002',
            VehicleNumber: 'KCA 123A',
            DriverName: 'John Mwangi',
            PlantCode: '1000',
            WeighbridgeId: 'WB1',
            MeasurementType: 'Exit',
            Weight: 33250,
            TareWeight: 8250,
            NetWeight: 25000,
            Timestamp: new Date('2025-08-06T10:30:00Z').toISOString(),
            SalesOrderNumber: 'SO001',
            DeliveryNumber: 'DEL001',
            Status: 'Completed',
            Operator: 'Mary Wanjiku'
        },
        {
            MeasurementId: 'WM003',
            VehicleNumber: 'KCB 456B',
            DriverName: 'Peter Kiprotich',
            PlantCode: '1100',
            WeighbridgeId: 'WB2',
            MeasurementType: 'Transfer_Out',
            Weight: 20000,
            TareWeight: 7500,
            NetWeight: 12500,
            Timestamp: new Date('2025-08-06T09:15:00Z').toISOString(),
            SalesOrderNumber: null,
            DeliveryNumber: null,
            Status: 'Completed',
            Operator: 'James Otieno'
        }
    ];
}

function initializeSampleDeliveries() {
    deliveries = [
        {
            DeliveryNumber: 'DEL001',
            SalesOrderNumber: 'SO001',
            VehicleNumber: 'KCA 123A',
            DriverName: 'John Mwangi',
            SourcePlant: '1000',
            Destination: 'CRBC Construction Site - Nairobi',
            Status: 'Completed',
            PlannedWeight: 25000,
            ActualWeight: 25000,
            CreatedDate: new Date('2025-08-06T07:00:00Z').toISOString(),
            DeliveredDate: new Date('2025-08-06T14:30:00Z').toISOString(),
            WeightMeasurements: ['WM001', 'WM002']
        },
        {
            DeliveryNumber: 'DEL002',
            SalesOrderNumber: 'SO002',
            VehicleNumber: 'KCC 789C',
            DriverName: 'Grace Mutua',
            SourcePlant: '1100',
            Destination: 'NHWD Hardware - Mombasa',
            Status: 'In_Progress',
            PlannedWeight: 15000,
            ActualWeight: null,
            CreatedDate: new Date('2025-08-06T11:00:00Z').toISOString(),
            DeliveredDate: null,
            WeightMeasurements: []
        }
    ];
}

const initialData = loadInitialData();

// Utility functions
function generateId(prefix) {
    return `${prefix}${Date.now()}${Math.floor(Math.random() * 1000)}`;
}

function calculateNetWeight(weight, tareWeight) {
    return Math.max(0, weight - (tareWeight || 0));
}

// Apply authentication middleware to all API routes
router.use(validateToken);

// Permission check middleware
const requirePermission = (permission) => {
    return (req, res, next) => {
        if (!req.user.permissions.includes(permission)) {
            return res.status(403).json({
                error: 'INSUFFICIENT_PERMISSIONS',
                message: `Required permission: ${permission}`,
                userPermissions: req.user.permissions,
                communicationUser: req.user.sub,
                hint: 'Contact administrator to update Communication User permissions in Communication Arrangement'
            });
        }
        next();
    };
};

// Master Data Endpoints (Read-only)
router.get('/business-partners', requirePermission('businesspartner:read'), (req, res) => {
    res.json(initialData.businessPartners);
});

router.get('/business-partners/:id', requirePermission('businesspartner:read'), (req, res) => {
    const partner = initialData.businessPartners.find(bp => bp.BusinessPartner === req.params.id);
    if (!partner) {
        return res.status(404).json({ error: 'Business partner not found' });
    }
    res.json(partner);
});

router.get('/materials', requirePermission('material:read'), (req, res) => {
    res.json(initialData.materials);
});

router.get('/materials/:id', requirePermission('material:read'), (req, res) => {
    const material = initialData.materials.find(m => m.MaterialCode === req.params.id);
    if (!material) {
        return res.status(404).json({ error: 'Material not found' });
    }
    res.json(material);
});

router.get('/plants', requirePermission('plant:read'), (req, res) => {
    res.json(initialData.plants);
});

router.get('/plants/:id', requirePermission('plant:read'), (req, res) => {
    const plant = initialData.plants.find(p => p.PlantCode === req.params.id);
    if (!plant) {
        return res.status(404).json({ error: 'Plant not found' });
    }
    res.json(plant);
});

// Sales Orders (CRUD)
router.get('/sales-orders', requirePermission('salesorder:read'), (req, res) => {
    res.json(salesOrders);
});

router.get('/sales-orders/:orderId', requirePermission('salesorder:read'), (req, res) => {
    const order = salesOrders.find(so => so.SalesOrderNumber === req.params.orderId);
    if (!order) {
        return res.status(404).json({ error: 'Sales order not found' });
    }
    res.json(order);
});

router.post('/sales-orders', requirePermission('salesorder:create'), (req, res) => {
    try {
        const { SoldToParty, RequestedDeliveryDate, Plant, Items } = req.body;
        
        if (!SoldToParty || !Plant || !Items || Items.length === 0) {
            return res.status(400).json({ error: 'Missing required fields: SoldToParty, Plant, Items' });
        }

        const salesOrderNumber = generateId('SO');
        let totalValue = 0;

        // Calculate total value and prepare items
        const processedItems = Items.map((item, index) => {
            const material = initialData.materials.find(m => m.MaterialCode === item.MaterialCode);
            if (!material) {
                throw new Error(`Material ${item.MaterialCode} not found`);
            }

            const netPrice = material.StandardPrice;
            const netValue = netPrice * item.OrderQuantity;
            totalValue += netValue;

            return {
                ItemNumber: String((index + 1) * 10),
                MaterialCode: item.MaterialCode,
                OrderQuantity: item.OrderQuantity,
                Unit: material.BaseUnit,
                NetPrice: netPrice,
                NetValue: netValue,
                DeliveredQuantity: 0,
                RemainingQuantity: item.OrderQuantity
            };
        });

        const newOrder = {
            SalesOrderNumber: salesOrderNumber,
            SoldToParty,
            OrderDate: new Date().toISOString().split('T')[0],
            RequestedDeliveryDate: RequestedDeliveryDate || new Date(Date.now() + 7*24*60*60*1000).toISOString().split('T')[0],
            Plant,
            TotalValue: totalValue,
            Currency: 'KES',
            Status: 'Open',
            Items: processedItems,
            CreatedTimestamp: new Date().toISOString(),
            LastModified: new Date().toISOString()
        };

        salesOrders.push(newOrder);
        res.status(201).json(newOrder);
    } catch (error) {
        res.status(400).json({ error: error.message });
    }
});

router.put('/sales-orders/:orderId', requirePermission('salesorder:update'), (req, res) => {
    const orderIndex = salesOrders.findIndex(so => so.SalesOrderNumber === req.params.orderId);
    if (orderIndex === -1) {
        return res.status(404).json({ error: 'Sales order not found' });
    }

    const { Status, RequestedDeliveryDate } = req.body;
    
    if (Status) {
        salesOrders[orderIndex].Status = Status;
    }
    if (RequestedDeliveryDate) {
        salesOrders[orderIndex].RequestedDeliveryDate = RequestedDeliveryDate;
    }
    
    salesOrders[orderIndex].LastModified = new Date().toISOString();
    
    res.json(salesOrders[orderIndex]);
});

// Weight Measurements (CRUD)
router.get('/weight-measurements', (req, res) => {
    let filtered = weightMeasurements;
    
    if (req.query.vehicleNumber) {
        filtered = filtered.filter(wm => wm.VehicleNumber.toLowerCase().includes(req.query.vehicleNumber.toLowerCase()));
    }
    if (req.query.plantId) {
        filtered = filtered.filter(wm => wm.PlantCode === req.query.plantId);
    }
    if (req.query.status) {
        filtered = filtered.filter(wm => wm.Status === req.query.status);
    }
    
    res.json(filtered);
});

router.get('/weight-measurements/:measurementId', (req, res) => {
    const measurement = weightMeasurements.find(wm => wm.MeasurementId === req.params.measurementId);
    if (!measurement) {
        return res.status(404).json({ error: 'Weight measurement not found' });
    }
    res.json(measurement);
});

router.post('/weight-measurements', (req, res) => {
    try {
        const { VehicleNumber, DriverName, PlantCode, WeighbridgeId, MeasurementType, Weight, SalesOrderNumber, DeliveryNumber, Operator } = req.body;
        
        if (!VehicleNumber || !PlantCode || !MeasurementType || !Weight) {
            return res.status(400).json({ error: 'Missing required fields: VehicleNumber, PlantCode, MeasurementType, Weight' });
        }

        const measurementId = generateId('WM');
        let tareWeight = null;
        let netWeight = null;
        let status = 'Pending_Exit';

        // Business logic for weight measurements
        if (MeasurementType === 'Entry') {
            // For entry, the weight is typically the tare weight (empty truck)
            tareWeight = Weight;
            netWeight = 0;
            status = 'Pending_Exit';
        } else if (MeasurementType === 'Exit') {
            // For exit, find the corresponding entry measurement to calculate net weight
            const entryMeasurement = weightMeasurements.find(wm => 
                wm.VehicleNumber === VehicleNumber && 
                wm.MeasurementType === 'Entry' && 
                wm.Status === 'Pending_Exit' &&
                (!SalesOrderNumber || wm.SalesOrderNumber === SalesOrderNumber)
            );
            
            if (entryMeasurement) {
                tareWeight = entryMeasurement.Weight;
                netWeight = calculateNetWeight(Weight, tareWeight);
                status = 'Completed';
                
                // Update the entry measurement status
                entryMeasurement.Status = 'Completed';
            } else {
                // If no entry found, treat as standalone exit measurement
                tareWeight = Weight * 0.35; // Assume tare weight is ~35% of gross weight
                netWeight = calculateNetWeight(Weight, tareWeight);
                status = 'Completed';
            }
        } else if (MeasurementType === 'Transfer_Out' || MeasurementType === 'Transfer_In') {
            // For transfers, calculate based on vehicle capacity and load
            tareWeight = Weight * 0.35; // Estimate tare weight
            netWeight = calculateNetWeight(Weight, tareWeight);
            status = 'Completed';
        }

        const newMeasurement = {
            MeasurementId: measurementId,
            VehicleNumber,
            DriverName,
            PlantCode,
            WeighbridgeId: WeighbridgeId || 'WB1',
            MeasurementType,
            Weight,
            TareWeight: tareWeight,
            NetWeight: netWeight,
            Timestamp: new Date().toISOString(),
            SalesOrderNumber,
            DeliveryNumber,
            Status: status,
            Operator: Operator || 'System'
        };

        weightMeasurements.push(newMeasurement);
        
        // Update delivery status if applicable
        if (DeliveryNumber) {
            const delivery = deliveries.find(d => d.DeliveryNumber === DeliveryNumber);
            if (delivery) {
                if (!delivery.WeightMeasurements.includes(measurementId)) {
                    delivery.WeightMeasurements.push(measurementId);
                }
                
                if (MeasurementType === 'Entry') {
                    delivery.Status = 'In_Progress';
                } else if (MeasurementType === 'Exit' && netWeight > 0) {
                    delivery.Status = 'Loaded';
                    delivery.ActualWeight = netWeight;
                }
            }
        }

        res.status(201).json(newMeasurement);
    } catch (error) {
        res.status(400).json({ error: error.message });
    }
});

// Deliveries (CRUD)
router.get('/deliveries', (req, res) => {
    let filtered = deliveries;
    
    if (req.query.status) {
        filtered = filtered.filter(d => d.Status === req.query.status);
    }
    if (req.query.customerId) {
        filtered = filtered.filter(d => {
            const salesOrder = salesOrders.find(so => so.SalesOrderNumber === d.SalesOrderNumber);
            return salesOrder && salesOrder.SoldToParty === req.query.customerId;
        });
    }
    
    res.json(filtered);
});

router.get('/deliveries/:deliveryId', (req, res) => {
    const delivery = deliveries.find(d => d.DeliveryNumber === req.params.deliveryId);
    if (!delivery) {
        return res.status(404).json({ error: 'Delivery not found' });
    }
    
    // Include weight measurements details
    const deliveryWithMeasurements = {
        ...delivery,
        WeightMeasurements: delivery.WeightMeasurements.map(wmId => 
            weightMeasurements.find(wm => wm.MeasurementId === wmId)
        ).filter(Boolean)
    };
    
    res.json(deliveryWithMeasurements);
});

router.post('/deliveries', (req, res) => {
    try {
        const { SalesOrderNumber, VehicleNumber, DriverName, SourcePlant, Destination, PlannedWeight } = req.body;
        
        if (!SalesOrderNumber || !VehicleNumber || !SourcePlant) {
            return res.status(400).json({ error: 'Missing required fields: SalesOrderNumber, VehicleNumber, SourcePlant' });
        }

        const deliveryNumber = generateId('DEL');
        
        const newDelivery = {
            DeliveryNumber: deliveryNumber,
            SalesOrderNumber,
            VehicleNumber,
            DriverName,
            SourcePlant,
            Destination: Destination || 'Customer Location',
            Status: 'Planned',
            PlannedWeight: PlannedWeight || 25000,
            ActualWeight: null,
            CreatedDate: new Date().toISOString(),
            DeliveredDate: null,
            WeightMeasurements: []
        };

        deliveries.push(newDelivery);
        res.status(201).json(newDelivery);
    } catch (error) {
        res.status(400).json({ error: error.message });
    }
});

router.put('/deliveries/:deliveryId', (req, res) => {
    const deliveryIndex = deliveries.findIndex(d => d.DeliveryNumber === req.params.deliveryId);
    if (deliveryIndex === -1) {
        return res.status(404).json({ error: 'Delivery not found' });
    }

    const { Status, ActualWeight, DeliveredDate } = req.body;
    
    if (Status) {
        deliveries[deliveryIndex].Status = Status;
    }
    if (ActualWeight !== undefined) {
        deliveries[deliveryIndex].ActualWeight = ActualWeight;
    }
    if (DeliveredDate) {
        deliveries[deliveryIndex].DeliveredDate = DeliveredDate;
    }
    if (Status === 'Delivered' && !deliveries[deliveryIndex].DeliveredDate) {
        deliveries[deliveryIndex].DeliveredDate = new Date().toISOString();
    }
    
    res.json(deliveries[deliveryIndex]);
});

// Plant Transfers (CRUD)
router.get('/plant-transfers', (req, res) => {
    res.json(plantTransfers);
});

router.post('/plant-transfers', (req, res) => {
    try {
        const { SourcePlant, DestinationPlant, VehicleNumber, MaterialCode, TransferQuantity, Unit } = req.body;
        
        if (!SourcePlant || !DestinationPlant || !MaterialCode || !TransferQuantity) {
            return res.status(400).json({ error: 'Missing required fields: SourcePlant, DestinationPlant, MaterialCode, TransferQuantity' });
        }

        const transferNumber = generateId('TRF');
        
        const newTransfer = {
            TransferNumber: transferNumber,
            SourcePlant,
            DestinationPlant,
            VehicleNumber,
            MaterialCode,
            TransferQuantity,
            Unit: Unit || 'BAG',
            Status: 'Planned',
            CreatedDate: new Date().toISOString(),
            CompletedDate: null
        };

        plantTransfers.push(newTransfer);
        res.status(201).json(newTransfer);
    } catch (error) {
        res.status(400).json({ error: error.message });
    }
});

// Additional endpoint for weight measurement summary by vehicle
router.get('/weight-measurements/vehicle/:vehicleNumber/summary', (req, res) => {
    const vehicleMeasurements = weightMeasurements.filter(wm => 
        wm.VehicleNumber === req.params.vehicleNumber
    );
    
    const summary = {
        vehicleNumber: req.params.vehicleNumber,
        totalMeasurements: vehicleMeasurements.length,
        pendingExit: vehicleMeasurements.filter(wm => wm.Status === 'Pending_Exit').length,
        completed: vehicleMeasurements.filter(wm => wm.Status === 'Completed').length,
        lastMeasurement: vehicleMeasurements.length > 0 ? vehicleMeasurements[vehicleMeasurements.length - 1] : null,
        totalNetWeight: vehicleMeasurements.reduce((sum, wm) => sum + (wm.NetWeight || 0), 0)
    };
    
    res.json(summary);
});

// Endpoint for weighbridge statistics
router.get('/statistics/weighbridge', (req, res) => {
    const { plantCode, date } = req.query;
    
    let filtered = weightMeasurements;
    
    if (plantCode) {
        filtered = filtered.filter(wm => wm.PlantCode === plantCode);
    }
    
    if (date) {
        const targetDate = new Date(date).toISOString().split('T')[0];
        filtered = filtered.filter(wm => wm.Timestamp.startsWith(targetDate));
    }
    
    const stats = {
        totalMeasurements: filtered.length,
        entryMeasurements: filtered.filter(wm => wm.MeasurementType === 'Entry').length,
        exitMeasurements: filtered.filter(wm => wm.MeasurementType === 'Exit').length,
        transferMeasurements: filtered.filter(wm => wm.MeasurementType.includes('Transfer')).length,
        totalNetWeight: filtered.reduce((sum, wm) => sum + (wm.NetWeight || 0), 0),
        averageNetWeight: filtered.length > 0 ? filtered.reduce((sum, wm) => sum + (wm.NetWeight || 0), 0) / filtered.filter(wm => wm.NetWeight > 0).length : 0,
        uniqueVehicles: [...new Set(filtered.map(wm => wm.VehicleNumber))].length
    };
    
    res.json(stats);
});

module.exports = router;