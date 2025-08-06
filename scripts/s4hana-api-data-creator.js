const https = require('https');

/**
 * S/4HANA API Data Creator for Bamburi Cement
 * Modified workflow using existing trial infrastructure
 */

class S4HANAAPICreator {
    constructor() {
        this.config = {
            baseUrl: 'https://my305028-api.s4hana.ondemand.com',
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12',
            // Use existing trial infrastructure
            companyCode: '1710',
            plant: '1710',
            salesOrg: '1710',
            distributionChannel: '10',
            division: '00',
            currency: 'USD'
        };
        
        this.createdData = {
            businessPartners: [],
            materials: [],
            salesOrders: []
        };
        
        this.csrfToken = null;
        this.cookies = [];
    }

    // Authentication and CSRF token handling
    async authenticate() {
        console.log('🔐 Authenticating with S/4HANA API...');
        
        return new Promise((resolve, reject) => {
            const auth = Buffer.from(`${this.config.username}:${this.config.password}`).toString('base64');
            
            const options = {
                hostname: 'my305028-api.s4hana.ondemand.com',
                path: '/sap/opu/odata/sap/API_BUSINESS_PARTNER/',
                method: 'GET',
                headers: {
                    'Authorization': `Basic ${auth}`,
                    'X-CSRF-Token': 'Fetch',
                    'Accept': 'application/json'
                }
            };

            const req = https.request(options, (res) => {
                console.log(`Auth Status: ${res.statusCode}`);
                
                // Store CSRF token and cookies
                this.csrfToken = res.headers['x-csrf-token'];
                if (res.headers['set-cookie']) {
                    this.cookies = res.headers['set-cookie'];
                }
                
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => {
                    if (res.statusCode === 200) {
                        console.log('✅ Authentication successful');
                        console.log(`CSRF Token: ${this.csrfToken ? 'Obtained' : 'Not found'}`);
                        resolve(true);
                    } else {
                        console.log('Response:', data);
                        reject(new Error(`Authentication failed: ${res.statusCode}`));
                    }
                });
            });

            req.on('error', reject);
            req.end();
        });
    }

    // Generic API request method
    async makeAPIRequest(endpoint, method = 'GET', data = null) {
        return new Promise((resolve, reject) => {
            const auth = Buffer.from(`${this.config.username}:${this.config.password}`).toString('base64');
            
            const options = {
                hostname: 'my305028-api.s4hana.ondemand.com',
                path: endpoint,
                method: method,
                headers: {
                    'Authorization': `Basic ${auth}`,
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            };

            // Add CSRF token for write operations
            if (method !== 'GET' && this.csrfToken) {
                options.headers['X-CSRF-Token'] = this.csrfToken;
            }

            // Add cookies if available
            if (this.cookies.length > 0) {
                options.headers['Cookie'] = this.cookies.join('; ');
            }

            const req = https.request(options, (res) => {
                let responseData = '';
                res.on('data', chunk => responseData += chunk);
                res.on('end', () => {
                    try {
                        const result = {
                            statusCode: res.statusCode,
                            headers: res.headers,
                            data: responseData ? JSON.parse(responseData) : null
                        };
                        resolve(result);
                    } catch (e) {
                        resolve({
                            statusCode: res.statusCode,
                            headers: res.headers,
                            data: responseData
                        });
                    }
                });
            });

            req.on('error', reject);
            
            if (data) {
                req.write(JSON.stringify(data));
            }
            
            req.end();
        });
    }

    // Test API connectivity
    async testConnectivity() {
        console.log('🧪 Testing API connectivity...');
        
        try {
            // Test Business Partner API
            const bpResult = await this.makeAPIRequest('/sap/opu/odata/sap/API_BUSINESS_PARTNER/');
            console.log(`Business Partner API: ${bpResult.statusCode === 200 ? '✅' : '❌'} (${bpResult.statusCode})`);
            
            // Test Material API  
            const matResult = await this.makeAPIRequest('/sap/opu/odata/sap/API_MATERIAL_SRV/');
            console.log(`Material API: ${matResult.statusCode === 200 ? '✅' : '❌'} (${matResult.statusCode})`);
            
            // Test Sales Order API
            const soResult = await this.makeAPIRequest('/sap/opu/odata/sap/API_SALES_ORDER_SRV/');
            console.log(`Sales Order API: ${soResult.statusCode === 200 ? '✅' : '❌'} (${soResult.statusCode})`);
            
            return {
                businessPartner: bpResult.statusCode === 200,
                material: matResult.statusCode === 200,
                salesOrder: soResult.statusCode === 200
            };
            
        } catch (error) {
            console.error('❌ Connectivity test failed:', error.message);
            return { businessPartner: false, material: false, salesOrder: false };
        }
    }

    // Create Business Partner
    async createBusinessPartner(bpData) {
        console.log(`👥 Creating Business Partner: ${bpData.name}...`);
        
        const payload = {
            BusinessPartnerCategory: bpData.category, // "1" for Person, "2" for Organization
            OrganizationBPName1: bpData.name,
            SearchTerm1: bpData.searchTerm,
            BusinessPartnerGrouping: bpData.grouping || "BP02", // Standard grouping
            Country: "US", // Trial system country
            Region: "CA", // Available region in trial
            CityName: bpData.city,
            StreetName: bpData.street,
            PhoneNumber1: bpData.phone
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_BusinessPartner',
                'POST',
                payload
            );

            if (result.statusCode === 201) {
                const bpNumber = result.data.d.BusinessPartner;
                console.log(`✅ Created Business Partner: ${bpData.name} (${bpNumber})`);
                
                this.createdData.businessPartners.push({
                    name: bpData.name,
                    number: bpNumber,
                    type: bpData.type,
                    searchTerm: bpData.searchTerm
                });

                // Extend to Customer if needed
                if (bpData.type === 'Customer') {
                    await this.extendToCustomer(bpNumber, bpData);
                }
                
                // Extend to Vendor if needed
                if (bpData.type === 'Vendor') {
                    await this.extendToVendor(bpNumber, bpData);
                }

                return bpNumber;
            } else {
                console.error(`❌ Failed to create BP ${bpData.name}: ${result.statusCode}`);
                console.error('Response:', result.data);
                return null;
            }
        } catch (error) {
            console.error(`❌ Error creating BP ${bpData.name}:`, error.message);
            return null;
        }
    }

    // Extend Business Partner to Customer
    async extendToCustomer(bpNumber, bpData) {
        console.log(`🛍️ Extending ${bpNumber} to Customer...`);
        
        const customerPayload = {
            Customer: bpNumber,
            SalesOrganization: this.config.salesOrg,
            DistributionChannel: this.config.distributionChannel,
            Division: this.config.division,
            CustomerGroup: "01", // Standard customer group
            Currency: this.config.currency,
            PaymentTerms: "0001", // Standard payment terms
            CreditLimitAmount: bpData.creditLimit || "10000.00"
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_Customer',
                'POST',
                customerPayload
            );

            if (result.statusCode === 201) {
                console.log(`✅ Extended to Customer: ${bpNumber}`);
            } else {
                console.warn(`⚠️ Customer extension failed for ${bpNumber}: ${result.statusCode}`);
            }
        } catch (error) {
            console.warn(`⚠️ Customer extension error for ${bpNumber}:`, error.message);
        }
    }

    // Extend Business Partner to Vendor
    async extendToVendor(bpNumber, bpData) {
        console.log(`🏭 Extending ${bpNumber} to Vendor...`);
        
        const vendorPayload = {
            Supplier: bpNumber,
            CompanyCode: this.config.companyCode,
            SupplierGroup: "0001", // Standard supplier group
            Currency: this.config.currency,
            PaymentTerms: "0001"
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_Supplier',
                'POST',
                vendorPayload
            );

            if (result.statusCode === 201) {
                console.log(`✅ Extended to Vendor: ${bpNumber}`);
            } else {
                console.warn(`⚠️ Vendor extension failed for ${bpNumber}: ${result.statusCode}`);
            }
        } catch (error) {
            console.warn(`⚠️ Vendor extension error for ${bpNumber}:`, error.message);
        }
    }

    // Create Material
    async createMaterial(materialData) {
        console.log(`📦 Creating Material: ${materialData.description}...`);
        
        const payload = {
            Product: materialData.number || "", // Let system generate if empty
            ProductType: "FERT", // Finished product
            IndustrySector: "C", // Chemical
            ProductGroup: "CEMENT",
            BaseUnit: materialData.baseUnit,
            GrossWeight: materialData.weight.toString(),
            NetWeight: materialData.weight.toString(),
            WeightUnit: "KG",
            ProductDescription: [
                {
                    Language: "EN",
                    ProductDescription: materialData.description
                }
            ]
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_MATERIAL_SRV/A_Product',
                'POST',
                payload
            );

            if (result.statusCode === 201) {
                const materialNumber = result.data.d.Product;
                console.log(`✅ Created Material: ${materialData.description} (${materialNumber})`);
                
                this.createdData.materials.push({
                    number: materialNumber,
                    description: materialData.description,
                    price: materialData.price
                });

                // Add plant data
                await this.addPlantData(materialNumber, materialData);
                
                return materialNumber;
            } else {
                console.error(`❌ Failed to create material ${materialData.description}: ${result.statusCode}`);
                console.error('Response:', result.data);
                return null;
            }
        } catch (error) {
            console.error(`❌ Error creating material ${materialData.description}:`, error.message);
            return null;
        }
    }

    // Add Plant Data to Material
    async addPlantData(materialNumber, materialData) {
        console.log(`🏭 Adding plant data for ${materialNumber}...`);
        
        const plantPayload = {
            Product: materialNumber,
            Plant: this.config.plant,
            PlantSpecificStatus: "20", // Active
            MRPType: "PD", // Master production scheduling
            LotSizeKey: "EX" // Lot-for-lot
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_MATERIAL_SRV/A_ProductPlant',
                'POST',
                plantPayload
            );

            if (result.statusCode === 201) {
                console.log(`✅ Added plant data for ${materialNumber}`);
            } else {
                console.warn(`⚠️ Plant data creation failed for ${materialNumber}: ${result.statusCode}`);
            }
        } catch (error) {
            console.warn(`⚠️ Plant data error for ${materialNumber}:`, error.message);
        }
    }

    // Create Sales Order
    async createSalesOrder(orderData) {
        console.log(`🛒 Creating Sales Order for: ${orderData.customerName}...`);
        
        const payload = {
            SalesOrderType: "OR", // Standard order
            SalesOrganization: this.config.salesOrg,
            DistributionChannel: this.config.distributionChannel,
            OrganizationDivision: this.config.division,
            SoldToParty: orderData.customerBP,
            TransactionCurrency: this.config.currency,
            PurchaseOrderByCustomer: `PO-${Date.now()}`,
            to_Item: orderData.items.map((item, index) => ({
                SalesOrderItem: ((index + 1) * 10).toString(),
                Material: item.materialNumber,
                RequestedQuantity: item.quantity.toString(),
                RequestedQuantityUnit: item.unit,
                Plant: this.config.plant
            }))
        };

        try {
            const result = await this.makeAPIRequest(
                '/sap/opu/odata/sap/API_SALES_ORDER_SRV/A_SalesOrder',
                'POST',
                payload
            );

            if (result.statusCode === 201) {
                const orderNumber = result.data.d.SalesOrder;
                console.log(`✅ Created Sales Order: ${orderNumber} for ${orderData.customerName}`);
                
                this.createdData.salesOrders.push({
                    number: orderNumber,
                    customer: orderData.customerName,
                    value: orderData.totalValue
                });

                return orderNumber;
            } else {
                console.error(`❌ Failed to create sales order for ${orderData.customerName}: ${result.statusCode}`);
                console.error('Response:', result.data);
                return null;
            }
        } catch (error) {
            console.error(`❌ Error creating sales order for ${orderData.customerName}:`, error.message);
            return null;
        }
    }

    // Main execution method
    async createBamburiData() {
        console.log('🏭 Starting Bamburi Cement API Data Creation...\n');
        
        try {
            // Step 1: Authenticate
            await this.authenticate();
            
            // Step 2: Test connectivity
            const connectivity = await this.testConnectivity();
            if (!connectivity.businessPartner) {
                throw new Error('Business Partner API not accessible');
            }
            
            // Step 3: Create Business Partners
            console.log('\n👥 Creating Business Partners...');
            
            const businessPartners = [
                {
                    name: 'China Road & Bridge Corporation Kenya',
                    searchTerm: 'CRBC-KE',
                    type: 'Customer',
                    category: '2', // Organization
                    city: 'Nairobi',
                    street: 'Westlands Business Park',
                    phone: '+254-20-4444000',
                    creditLimit: '50000.00'
                },
                {
                    name: 'Nairobi Hardware Dealers Ltd',
                    searchTerm: 'NHWD',
                    type: 'Customer',
                    category: '2',
                    city: 'Nairobi',
                    street: 'Industrial Area',
                    phone: '+254-20-5551000',
                    creditLimit: '15000.00'
                },
                {
                    name: 'John Mwangi Construction',
                    searchTerm: 'JMWNG',
                    type: 'Customer',
                    category: '2',
                    city: 'Nairobi',
                    street: 'Kasarani',
                    phone: '+254-722-123456',
                    creditLimit: '5000.00'
                },
                {
                    name: 'Mombasa Transport SACCO Ltd',
                    searchTerm: 'MTSACCO',
                    type: 'Vendor',
                    category: '2',
                    city: 'Mombasa',
                    street: 'Transport Plaza',
                    phone: '+254-41-2222000'
                },
                {
                    name: 'Kenya Power & Lighting Co',
                    searchTerm: 'KPLC',
                    type: 'Vendor',
                    category: '2',
                    city: 'Nairobi',
                    street: 'Stima Plaza',
                    phone: '+254-20-3201000'
                }
            ];

            for (const bp of businessPartners) {
                await this.createBusinessPartner(bp);
                await this.sleep(2000); // Wait 2 seconds between requests
            }

            // Step 4: Create Materials (if Material API is available)
            if (connectivity.material) {
                console.log('\n📦 Creating Materials...');
                
                const materials = [
                    {
                        description: 'PowerMax Premium Cement 50kg',
                        baseUnit: 'BAG',
                        weight: 50,
                        price: 12.50
                    },
                    {
                        description: 'Nguvu General Purpose Cement 50kg',
                        baseUnit: 'BAG',
                        weight: 50,
                        price: 8.35
                    },
                    {
                        description: 'Fundi Affordable Cement 50kg',
                        baseUnit: 'BAG',
                        weight: 50,
                        price: 7.50
                    },
                    {
                        description: 'BamburiBlox Paving Blocks',
                        baseUnit: 'PC',
                        weight: 2.5,
                        price: 0.45
                    },
                    {
                        description: 'Readymix Concrete Grade M25',
                        baseUnit: 'M3',
                        weight: 2400,
                        price: 85.00
                    }
                ];

                for (const material of materials) {
                    await this.createMaterial(material);
                    await this.sleep(2000);
                }
            }

            // Step 5: Create Sales Orders (if we have customers and materials)
            if (connectivity.salesOrder && this.createdData.businessPartners.length > 0) {
                console.log('\n🛒 Creating Sales Orders...');
                
                // Find CRBC customer
                const crbcCustomer = this.createdData.businessPartners.find(bp => 
                    bp.searchTerm === 'CRBC-KE'
                );
                
                if (crbcCustomer && this.createdData.materials.length > 0) {
                    const sampleOrder = {
                        customerName: crbcCustomer.name,
                        customerBP: crbcCustomer.number,
                        totalValue: '16675.00',
                        items: [
                            {
                                materialNumber: this.createdData.materials[0]?.number || 'DUMMY',
                                quantity: '1000',
                                unit: 'BAG'
                            },
                            {
                                materialNumber: this.createdData.materials[1]?.number || 'DUMMY',
                                quantity: '500',
                                unit: 'BAG'
                            }
                        ]
                    };
                    
                    await this.createSalesOrder(sampleOrder);
                }
            }

            // Generate final report
            this.generateReport();
            
        } catch (error) {
            console.error('❌ Data creation failed:', error.message);
        }
    }

    // Helper method to pause execution
    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // Generate summary report
    generateReport() {
        console.log('\n📊 BAMBURI CEMENT API DATA CREATION REPORT');
        console.log('=' .repeat(60));
        
        console.log('\n👥 Business Partners Created:');
        this.createdData.businessPartners.forEach(bp => {
            console.log(`   ✅ ${bp.name} (${bp.number}) - ${bp.type}`);
        });
        
        console.log('\n📦 Materials Created:');
        this.createdData.materials.forEach(material => {
            console.log(`   ✅ ${material.description} (${material.number}) - $${material.price}`);
        });
        
        console.log('\n🛒 Sales Orders Created:');
        this.createdData.salesOrders.forEach(order => {
            console.log(`   ✅ ${order.number} - ${order.customer} ($${order.value})`);
        });
        
        console.log('\n🎉 API data creation completed!');
        console.log('\nNext Steps:');
        console.log('1. Verify data in S/4HANA trial system');
        console.log('2. Test additional API operations');
        console.log('3. Begin QaliTrack integration development');
        
        // Save data to file for reference
        const fs = require('fs');
        fs.writeFileSync(
            './bamburi-created-data.json',
            JSON.stringify(this.createdData, null, 2)
        );
        console.log('📄 Created data saved to: bamburi-created-data.json');
    }
}

// Export for module usage
module.exports = S4HANAAPICreator;

// Run if called directly
if (require.main === module) {
    const creator = new S4HANAAPICreator();
    creator.createBamburiData().catch(console.error);
}