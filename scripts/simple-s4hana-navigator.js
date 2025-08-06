const { chromium } = require('playwright');

/**
 * Simple S/4HANA Navigator for Manual Data Creation
 * Opens browser and guides through Bamburi data creation
 */

class SimpleS4HANANavigator {
    constructor() {
        this.browser = null;
        this.page = null;
        this.config = {
            url: 'https://my305028.s4hana.ondemand.com/ui?sap-language=EN&help-mixedLanguages=false&help-autoStartTour=PR_A8DA8C2F83492685#Shell-home',
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12'
        };
    }

    async launch() {
        console.log('🚀 Launching browser for S/4HANA data creation...');
        
        this.browser = await chromium.launch({
            headless: false, // Keep browser visible
            slowMo: 1000,    // Slow down for visibility
            args: ['--start-maximized']
        });
        
        const context = await this.browser.newContext({
            viewport: null // Use full screen
        });
        
        this.page = await context.newPage();
        
        // Set longer timeout for SAML authentication
        this.page.setDefaultTimeout(60000);
        
        console.log('✅ Browser launched successfully');
    }

    async navigateAndLogin() {
        console.log('🔐 Navigating to S/4HANA trial...');
        console.log(`URL: ${this.config.url}`);
        
        await this.page.goto(this.config.url);
        
        console.log('⏳ Waiting for login page to load...');
        console.log('');
        console.log('📋 MANUAL LOGIN REQUIRED:');
        console.log('========================');
        console.log(`Username: ${this.config.username}`);
        console.log(`Password: ${this.config.password}`);
        console.log('');
        console.log('Please complete the SAML login manually in the browser window.');
        console.log('The script will continue once you reach the Fiori Launchpad.');
        console.log('');
        
        // Wait for user to complete login and reach Fiori Launchpad
        try {
            await this.page.waitForURL('**/ui**', { timeout: 120000 }); // 2 minutes
            console.log('✅ Login completed - Fiori Launchpad detected');
        } catch (error) {
            console.log('⏳ Still waiting for login completion...');
            console.log('Press Ctrl+C to exit if login fails');
            
            // Wait indefinitely for manual login
            await this.page.waitForURL('**/ui**', { timeout: 0 });
            console.log('✅ Login completed');
        }
    }

    async showGuidance() {
        console.log('');
        console.log('🎯 BAMBURI CEMENT DATA CREATION GUIDE');
        console.log('=====================================');
        console.log('');
        console.log('Now that you\'re logged in, follow these steps to create the Bamburi data:');
        console.log('');
        
        console.log('📋 STEP 1: CREATE BUSINESS PARTNERS');
        console.log('-----------------------------------');
        console.log('1. Search for "Business Partner" in the global search');
        console.log('2. Click on "Manage Business Partner Master Data"');
        console.log('3. Create these customers:');
        console.log('');
        console.log('   Customer 1: China Road & Bridge Corporation Kenya');
        console.log('   - Search Term: CRBC-KE');
        console.log('   - Country: US (trial limitation)');
        console.log('   - City: Nairobi');
        console.log('   - Street: Westlands Business Park');
        console.log('   - Phone: +254-20-4444000');
        console.log('   - Credit Limit: $50,000');
        console.log('');
        console.log('   Customer 2: Nairobi Hardware Dealers Ltd');
        console.log('   - Search Term: NHWD');
        console.log('   - City: Nairobi');
        console.log('   - Street: Industrial Area');
        console.log('   - Credit Limit: $15,000');
        console.log('');
        console.log('   Customer 3: John Mwangi Construction');
        console.log('   - Search Term: JMWNG');
        console.log('   - City: Nairobi');
        console.log('   - Credit Limit: $5,000');
        console.log('');
        console.log('   Vendor 1: Mombasa Transport SACCO Ltd');
        console.log('   - Search Term: MTSACCO');
        console.log('   - Type: Vendor');
        console.log('   - City: Mombasa');
        console.log('');
        console.log('   Vendor 2: Kenya Power & Lighting Co');
        console.log('   - Search Term: KPLC');
        console.log('   - Type: Vendor');
        console.log('   - City: Nairobi');
        console.log('');
        
        console.log('📦 STEP 2: CREATE MATERIALS');
        console.log('---------------------------');
        console.log('1. Search for "Material" or "Manage Material Master Data"');
        console.log('2. Create these cement products:');
        console.log('');
        console.log('   Material 1: PowerMax Premium Cement 50kg');
        console.log('   - Material Type: FERT');
        console.log('   - Base Unit: BAG');
        console.log('   - Weight: 50 KG');
        console.log('   - Price: $12.50');
        console.log('');
        console.log('   Material 2: Nguvu General Purpose Cement 50kg');
        console.log('   - Base Unit: BAG');
        console.log('   - Weight: 50 KG');
        console.log('   - Price: $8.35');
        console.log('');
        console.log('   Material 3: Fundi Affordable Cement 50kg');
        console.log('   - Base Unit: BAG');
        console.log('   - Weight: 50 KG');
        console.log('   - Price: $7.50');
        console.log('');
        console.log('   Material 4: BamburiBlox Paving Blocks');
        console.log('   - Base Unit: PC (Pieces)');
        console.log('   - Weight: 2.5 KG');
        console.log('   - Price: $0.45');
        console.log('');
        console.log('   Material 5: Readymix Concrete Grade M25');
        console.log('   - Base Unit: M3');
        console.log('   - Weight: 2400 KG');
        console.log('   - Price: $85.00');
        console.log('');
        
        console.log('🛒 STEP 3: CREATE SALES ORDERS');
        console.log('------------------------------');
        console.log('1. Search for "Sales Order" or "Create Sales Order"');
        console.log('2. Create sample orders using the business partners and materials above');
        console.log('');
        console.log('   Order 1: Large Infrastructure Project');
        console.log('   - Customer: China Road & Bridge Corporation');
        console.log('   - Item 1: 1,000 bags PowerMax Cement');
        console.log('   - Item 2: 500 bags Nguvu Cement');
        console.log('   - Total Value: ~$16,675');
        console.log('');
        console.log('   Order 2: Regional Dealer Order');
        console.log('   - Customer: Nairobi Hardware Dealers');
        console.log('   - Item 1: 100 bags Nguvu Cement');
        console.log('   - Item 2: 200 bags Fundi Cement');
        console.log('   - Item 3: 500 pieces BamburiBlox');
        console.log('   - Total Value: ~$2,560');
        console.log('');
        
        console.log('💡 IMPORTANT NOTES:');
        console.log('-------------------');
        console.log('• Use existing Company Code: 1710');
        console.log('• Use existing Plant: 1710');
        console.log('• Use Sales Organization: 1710');
        console.log('• Use Distribution Channel: 10');
        console.log('• Use Division: 00');
        console.log('• Currency: USD (trial system)');
        console.log('• Note down all generated BP numbers and Material numbers');
        console.log('');
        
        console.log('🎯 After creating the data:');
        console.log('1. Document all generated IDs');
        console.log('2. Test the data by viewing Business Partners, Materials, and Sales Orders');
        console.log('3. Return to this console and press Enter when complete');
        console.log('');
    }

    async waitForCompletion() {
        console.log('⏳ Press Enter when you have finished creating all the data...');
        
        return new Promise((resolve) => {
            process.stdin.resume();
            process.stdin.setEncoding('utf8');
            process.stdin.once('data', () => {
                resolve();
            });
        });
    }

    async showCompletionSummary() {
        console.log('');
        console.log('🎉 BAMBURI CEMENT DATA CREATION COMPLETED!');
        console.log('=========================================');
        console.log('');
        console.log('You should now have created:');
        console.log('✅ 5 Business Partners (3 customers + 2 vendors)');
        console.log('✅ 5 Materials (cement products)');
        console.log('✅ 2-3 Sample Sales Orders');
        console.log('');
        console.log('📝 Next Steps:');
        console.log('1. Keep the browser open to verify your data');
        console.log('2. Document the generated BP and Material numbers');
        console.log('3. Test API access to the created data');
        console.log('4. Begin QaliTrack integration development');
        console.log('');
        console.log('🔗 Integration Ready:');
        console.log('• Customer master data for weighbridge validation');
        console.log('• Product catalog for load verification');
        console.log('• Sales orders for transaction processing');
        console.log('• Vendor relationships for SACCO management');
        console.log('');
        console.log('The browser will remain open for your continued use.');
        console.log('Close this terminal when you\'re done.');
    }

    async run() {
        try {
            await this.launch();
            await this.navigateAndLogin();
            await this.showGuidance();
            await this.waitForCompletion();
            await this.showCompletionSummary();
            
            // Keep browser open
            console.log('');
            console.log('Browser will remain open. Close manually when finished.');
            
            // Wait indefinitely
            return new Promise(() => {});
            
        } catch (error) {
            console.error('❌ Script failed:', error.message);
            if (this.browser) {
                await this.browser.close();
            }
        }
    }
}

// Run the navigator
const navigator = new SimpleS4HANANavigator();
navigator.run().catch(console.error);