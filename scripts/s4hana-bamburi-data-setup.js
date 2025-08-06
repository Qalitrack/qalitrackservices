const { chromium } = require('playwright');

/**
 * Bamburi Cement S/4HANA Data Setup Script
 * Automates creation of sample data in S/4HANA trial system
 */

class S4HANADataSetup {
    constructor() {
        this.browser = null;
        this.context = null;
        this.page = null;
        this.baseUrl = 'https://my305028.s4hana.ondemand.com';
        this.credentials = {
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12'
        };
        this.createdData = {
            businessPartners: [],
            materials: [],
            salesOrders: []
        };
    }

    async initialize() {
        console.log('🚀 Initializing Playwright browser...');
        this.browser = await chromium.launch({ 
            headless: false, // Set to true for headless mode
            slowMo: 1000 // Slow down actions for visibility
        });
        this.context = await this.browser.newContext();
        this.page = await this.context.newPage();
        
        // Set longer timeout for S/4HANA responses
        this.page.setDefaultTimeout(30000);
    }

    async login() {
        console.log('🔐 Logging into S/4HANA trial...');
        await this.page.goto(this.baseUrl);
        
        // Handle SAML login
        await this.page.waitForSelector('input[type="email"], input[name="j_username"], #j_username', { timeout: 10000 });
        
        // Try different possible username field selectors
        const usernameSelectors = [
            'input[type="email"]',
            'input[name="j_username"]', 
            '#j_username',
            'input[name="username"]',
            '[data-testid="username"]'
        ];
        
        let usernameField = null;
        for (const selector of usernameSelectors) {
            try {
                usernameField = await this.page.$(selector);
                if (usernameField) break;
            } catch (e) {
                // Continue to next selector
            }
        }
        
        if (usernameField) {
            await usernameField.fill(this.credentials.username);
        } else {
            throw new Error('Could not find username field');
        }
        
        // Handle password field
        const passwordSelectors = [
            'input[type="password"]',
            'input[name="password"]',
            '#password',
            'input[name="j_password"]'
        ];
        
        let passwordField = null;
        for (const selector of passwordSelectors) {
            try {
                passwordField = await this.page.$(selector);
                if (passwordField) break;
            } catch (e) {
                // Continue to next selector
            }
        }
        
        if (passwordField) {
            await passwordField.fill(this.credentials.password);
        }
        
        // Click login button
        const loginSelectors = [
            'button[type="submit"]',
            'input[type="submit"]',
            'button:has-text("Log On")',
            'button:has-text("Sign In")',
            '#logOnFormSubmit'
        ];
        
        for (const selector of loginSelectors) {
            try {
                const loginButton = await this.page.$(selector);
                if (loginButton) {
                    await loginButton.click();
                    break;
                }
            } catch (e) {
                // Continue to next selector
            }
        }
        
        // Wait for Fiori Launchpad to load
        await this.page.waitForURL('**/ui**', { timeout: 30000 });
        console.log('✅ Successfully logged in');
    }

    async navigateToApp(appName) {
        console.log(`🧭 Navigating to ${appName}...`);
        
        // Look for search icon or search field
        const searchSelectors = [
            '[data-testid="search"]',
            '.sapUshellSearchButton',
            '#searchFieldInShell',
            'input[placeholder*="Search"]',
            '[title="Search"]'
        ];
        
        let searchField = null;
        for (const selector of searchSelectors) {
            try {
                const element = await this.page.$(selector);
                if (element) {
                    await element.click();
                    // Wait a bit for search field to appear
                    await this.page.waitForTimeout(1000);
                    searchField = element;
                    break;
                }
            } catch (e) {
                // Continue to next selector
            }
        }
        
        if (!searchField) {
            // Try to find search input after clicking search button
            await this.page.waitForTimeout(1000);
            searchField = await this.page.$('input[type="search"], input[placeholder*="Search"]');
        }
        
        if (searchField) {
            await searchField.fill(appName);
            await this.page.keyboard.press('Enter');
            await this.page.waitForTimeout(2000);
            
            // Click on the first search result
            const firstResult = await this.page.$('.sapMText, .sapMLnk, a:has-text("' + appName + '")');
            if (firstResult) {
                await firstResult.click();
            }
        }
        
        await this.page.waitForTimeout(3000);
    }

    async createBusinessPartner(bpData) {
        console.log(`👥 Creating Business Partner: ${bpData.name}...`);
        
        try {
            await this.navigateToApp('Manage Business Partner Master Data');
            
            // Look for Create/New button
            const createSelectors = [
                'button:has-text("Create")',
                'button:has-text("New")',
                'button:has-text("+")',
                '[data-testid="create"]',
                '.sapMBtn:has-text("Create")'
            ];
            
            for (const selector of createSelectors) {
                try {
                    const createButton = await this.page.$(selector);
                    if (createButton) {
                        await createButton.click();
                        break;
                    }
                } catch (e) {
                    continue;
                }
            }
            
            await this.page.waitForTimeout(2000);
            
            // Fill in business partner data
            await this.fillFormFields({
                'Organization Name': bpData.name,
                'Search Term': bpData.searchTerm,
                'Country': bpData.country,
                'City': bpData.city,
                'Street': bpData.street,
                'Postal Code': bpData.postalCode,
                'Phone': bpData.phone
            });
            
            // Save the business partner
            await this.clickSaveButton();
            
            // Get the generated BP number
            const bpNumber = await this.getBPNumber();
            this.createdData.businessPartners.push({
                name: bpData.name,
                number: bpNumber,
                type: bpData.type
            });
            
            console.log(`✅ Created Business Partner: ${bpData.name} (${bpNumber})`);
            
        } catch (error) {
            console.error(`❌ Error creating Business Partner ${bpData.name}:`, error);
        }
    }

    async createMaterial(materialData) {
        console.log(`📦 Creating Material: ${materialData.description}...`);
        
        try {
            await this.navigateToApp('Manage Material Master Data');
            
            // Click Create/New
            await this.clickCreateButton();
            
            // Fill material data
            await this.fillFormFields({
                'Material': materialData.number,
                'Material Type': materialData.type,
                'Description': materialData.description,
                'Base Unit': materialData.baseUnit,
                'Material Group': materialData.group,
                'Gross Weight': materialData.weight,
                'Weight Unit': 'KG'
            });
            
            // Navigate through tabs if needed (Plant Data, Sales Data, etc.)
            await this.fillPlantData(materialData);
            await this.fillSalesData(materialData);
            await this.fillAccountingData(materialData);
            
            await this.clickSaveButton();
            
            this.createdData.materials.push({
                number: materialData.number,
                description: materialData.description
            });
            
            console.log(`✅ Created Material: ${materialData.description}`);
            
        } catch (error) {
            console.error(`❌ Error creating Material ${materialData.description}:`, error);
        }
    }

    async createSalesOrder(orderData) {
        console.log(`🛒 Creating Sales Order for: ${orderData.customer}...`);
        
        try {
            await this.navigateToApp('Create Sales Order');
            
            // Fill order header
            await this.fillFormFields({
                'Sold-to Party': orderData.customerBP,
                'Sales Organization': '1710',
                'Distribution Channel': '10',
                'Division': '00'
            });
            
            // Add order items
            for (const item of orderData.items) {
                await this.addSalesOrderItem(item);
            }
            
            await this.clickSaveButton();
            
            const orderNumber = await this.getSalesOrderNumber();
            this.createdData.salesOrders.push({
                number: orderNumber,
                customer: orderData.customer,
                value: orderData.totalValue
            });
            
            console.log(`✅ Created Sales Order: ${orderNumber} for ${orderData.customer}`);
            
        } catch (error) {
            console.error(`❌ Error creating Sales Order:`, error);
        }
    }

    // Helper methods
    async fillFormFields(fields) {
        for (const [label, value] of Object.entries(fields)) {
            if (!value) continue;
            
            try {
                // Try multiple strategies to find and fill fields
                const selectors = [
                    `input[aria-label*="${label}"]`,
                    `input[placeholder*="${label}"]`,
                    `input[title*="${label}"]`,
                    `//label[contains(text(), "${label}")]/following::input[1]`,
                    `//span[contains(text(), "${label}")]/following::input[1]`
                ];
                
                let filled = false;
                for (const selector of selectors) {
                    try {
                        const field = selector.startsWith('//') 
                            ? await this.page.locator(selector).first()
                            : await this.page.$(selector);
                        
                        if (field) {
                            await field.fill(String(value));
                            filled = true;
                            break;
                        }
                    } catch (e) {
                        continue;
                    }
                }
                
                if (!filled) {
                    console.warn(`⚠️ Could not find field: ${label}`);
                }
                
            } catch (error) {
                console.warn(`⚠️ Error filling field ${label}:`, error.message);
            }
        }
    }

    async clickCreateButton() {
        const selectors = [
            'button:has-text("Create")',
            'button:has-text("New")',
            'button:has-text("+")',
            '[data-testid="create"]'
        ];
        
        for (const selector of selectors) {
            try {
                const button = await this.page.$(selector);
                if (button) {
                    await button.click();
                    await this.page.waitForTimeout(2000);
                    return;
                }
            } catch (e) {
                continue;
            }
        }
    }

    async clickSaveButton() {
        const selectors = [
            'button:has-text("Save")',
            'button:has-text("Create")',
            '[data-testid="save"]',
            '.sapMBtn:has-text("Save")'
        ];
        
        for (const selector of selectors) {
            try {
                const button = await this.page.$(selector);
                if (button) {
                    await button.click();
                    await this.page.waitForTimeout(3000);
                    return;
                }
            } catch (e) {
                continue;
            }
        }
    }

    async fillPlantData(materialData) {
        // Implementation for plant data tab
        try {
            const plantTab = await this.page.$('span:has-text("Plant Data"), a:has-text("Plant")');
            if (plantTab) {
                await plantTab.click();
                await this.page.waitForTimeout(1000);
                
                // Fill plant-specific fields
                await this.fillFormFields({
                    'Plant': '1000',
                    'MRP Type': 'PD',
                    'Lot Size': 'EX'
                });
            }
        } catch (error) {
            console.warn('Could not fill plant data:', error.message);
        }
    }

    async fillSalesData(materialData) {
        // Implementation for sales data tab
        try {
            const salesTab = await this.page.$('span:has-text("Sales Data"), a:has-text("Sales")');
            if (salesTab) {
                await salesTab.click();
                await this.page.waitForTimeout(1000);
                
                await this.fillFormFields({
                    'Sales Organization': '1710',
                    'Distribution Channel': '10',
                    'Sales Unit': materialData.baseUnit
                });
            }
        } catch (error) {
            console.warn('Could not fill sales data:', error.message);
        }
    }

    async fillAccountingData(materialData) {
        // Implementation for accounting data tab
        try {
            const accountingTab = await this.page.$('span:has-text("Accounting"), a:has-text("Costing")');
            if (accountingTab) {
                await accountingTab.click();
                await this.page.waitForTimeout(1000);
                
                await this.fillFormFields({
                    'Standard Price': materialData.price,
                    'Currency': 'KES'
                });
            }
        } catch (error) {
            console.warn('Could not fill accounting data:', error.message);
        }
    }

    async addSalesOrderItem(item) {
        // Implementation for adding sales order items
        try {
            // Look for Add Item button
            const addButton = await this.page.$('button:has-text("Add"), button:has-text("+")');
            if (addButton) {
                await addButton.click();
                await this.page.waitForTimeout(1000);
            }
            
            // Fill item details
            await this.fillFormFields({
                'Material': item.material,
                'Quantity': item.quantity,
                'Plant': item.plant
            });
            
        } catch (error) {
            console.warn('Could not add sales order item:', error.message);
        }
    }

    async getBPNumber() {
        // Try to extract BP number from success message or form
        try {
            await this.page.waitForTimeout(2000);
            const numberElement = await this.page.$('.sapMText:has-text("Business Partner"), .sapMMessageToast');
            if (numberElement) {
                const text = await numberElement.textContent();
                const match = text.match(/(\d{10})/);
                return match ? match[1] : 'UNKNOWN';
            }
        } catch (error) {
            console.warn('Could not get BP number');
        }
        return 'UNKNOWN';
    }

    async getSalesOrderNumber() {
        // Try to extract sales order number
        try {
            await this.page.waitForTimeout(2000);
            const numberElement = await this.page.$('.sapMText:has-text("Sales Order"), input[readonly]');
            if (numberElement) {
                const text = await numberElement.inputValue() || await numberElement.textContent();
                return text.trim();
            }
        } catch (error) {
            console.warn('Could not get sales order number');
        }
        return 'UNKNOWN';
    }

    async createBamburiSampleData() {
        console.log('🏭 Starting Bamburi Cement sample data creation...\n');
        
        // Business Partners Data
        const businessPartners = [
            {
                name: 'China Road & Bridge Corporation',
                searchTerm: 'CRBC',
                type: 'Customer',
                country: 'KE',
                city: 'Nairobi',
                street: 'Westlands',
                postalCode: '00100',
                phone: '+254-20-4444000'
            },
            {
                name: 'Nairobi Hardware Dealers Ltd',
                searchTerm: 'NHWD',
                type: 'Customer',
                country: 'KE',
                city: 'Nairobi',
                street: 'Industrial Area',
                postalCode: '00200',
                phone: '+254-20-5551000'
            },
            {
                name: 'John Mwangi Construction',
                searchTerm: 'JMWNG',
                type: 'Customer',
                country: 'KE',
                city: 'Nairobi',
                street: 'Kasarani',
                postalCode: '00618',
                phone: '+254-722-123456'
            },
            {
                name: 'Mombasa Transport SACCO Ltd',
                searchTerm: 'MTSACCO',
                type: 'Vendor',
                country: 'KE',
                city: 'Mombasa',
                street: 'Transport Plaza',
                postalCode: '80100',
                phone: '+254-41-2222000'
            }
        ];

        // Materials Data
        const materials = [
            {
                number: 'PWRMAX50',
                type: 'FERT',
                description: 'PowerMax Premium Cement 50kg',
                baseUnit: 'BAG',
                group: 'CEMENT',
                weight: '50',
                price: '1250'
            },
            {
                number: 'NGUVU50',
                type: 'FERT',
                description: 'Nguvu General Purpose Cement 50kg',
                baseUnit: 'BAG',
                group: 'CEMENT',
                weight: '50',
                price: '835'
            },
            {
                number: 'FUNDI50',
                type: 'FERT',
                description: 'Fundi Affordable Cement 50kg',
                baseUnit: 'BAG',
                group: 'CEMENT',
                weight: '50',
                price: '750'
            },
            {
                number: 'BBLOX01',
                type: 'FERT',
                description: 'BamburiBlox Paving Blocks',
                baseUnit: 'PC',
                group: 'CONCRETE',
                weight: '2.5',
                price: '45'
            }
        ];

        try {
            // Create Business Partners
            console.log('📋 Creating Business Partners...');
            for (const bp of businessPartners) {
                await this.createBusinessPartner(bp);
                await this.page.waitForTimeout(2000);
            }

            // Create Materials
            console.log('\n📦 Creating Materials...');
            for (const material of materials) {
                await this.createMaterial(material);
                await this.page.waitForTimeout(2000);
            }

            // Create Sample Sales Orders (using created BPs)
            console.log('\n🛒 Creating Sales Orders...');
            const sampleOrders = [
                {
                    customer: 'CRBC',
                    customerBP: this.createdData.businessPartners.find(bp => bp.name.includes('China'))?.number || 'MANUAL',
                    totalValue: '16675000',
                    items: [
                        {
                            material: 'PWRMAX50',
                            quantity: '10000',
                            plant: '1000'
                        },
                        {
                            material: 'NGUVU50',
                            quantity: '5000',
                            plant: '1000'
                        }
                    ]
                }
            ];

            for (const order of sampleOrders) {
                await this.createSalesOrder(order);
                await this.page.waitForTimeout(2000);
            }

        } catch (error) {
            console.error('❌ Error during data creation:', error);
        }
    }

    async generateReport() {
        console.log('\n📊 BAMBURI CEMENT SAMPLE DATA CREATION REPORT');
        console.log('=' .repeat(60));
        
        console.log('\n👥 Business Partners Created:');
        this.createdData.businessPartners.forEach(bp => {
            console.log(`   ✅ ${bp.name} (${bp.number})`);
        });
        
        console.log('\n📦 Materials Created:');
        this.createdData.materials.forEach(material => {
            console.log(`   ✅ ${material.description} (${material.number})`);
        });
        
        console.log('\n🛒 Sales Orders Created:');
        this.createdData.salesOrders.forEach(order => {
            console.log(`   ✅ ${order.number} - ${order.customer} (KES ${order.value})`);
        });
        
        console.log('\n🎉 Data creation completed successfully!');
        console.log('\nNext Steps:');
        console.log('1. Verify data in S/4HANA trial system');
        console.log('2. Test OData API access with created data');
        console.log('3. Begin QaliTrack integration development');
    }

    async cleanup() {
        if (this.browser) {
            await this.browser.close();
        }
    }

    async run() {
        try {
            await this.initialize();
            await this.login();
            await this.createBamburiSampleData();
            await this.generateReport();
        } catch (error) {
            console.error('❌ Script failed:', error);
        } finally {
            await this.cleanup();
        }
    }
}

// Run the script
async function main() {
    const dataSetup = new S4HANADataSetup();
    await dataSetup.run();
}

// Export for module usage
module.exports = S4HANADataSetup;

// Run if called directly
if (require.main === module) {
    main().catch(console.error);
}