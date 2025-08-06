const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

/**
 * S/4HANA Playwright Explorer
 * Automated browser-based exploration and data creation for S/4HANA trial system
 */
class S4HANAPlaywrightExplorer {
    constructor() {
        this.config = {
            baseUrl: 'https://my305028-api.s4hana.ondemand.com',
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12',
            headless: false, // Set to true for headless operation
            slowMo: 1000 // Slow down actions for better visibility
        };
        
        this.browser = null;
        this.page = null;
        this.context = null;
        
        this.discoveredPages = [];
        this.discoveredForms = [];
        this.navigationStructure = {};
        this.createdData = {
            businessPartners: [],
            materials: [],
            salesOrders: [],
            screenshots: []
        };
    }

    async initialize() {
        console.log('🚀 Initializing Playwright browser...');
        
        this.browser = await chromium.launch({
            headless: this.config.headless,
            slowMo: this.config.slowMo,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        
        this.context = await this.browser.newContext({
            viewport: { width: 1920, height: 1080 },
            userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        });
        
        this.page = await this.context.newPage();
        
        // Set up request/response logging
        this.page.on('request', request => {
            if (request.url().includes('odata') || request.url().includes('api')) {
                console.log(`📡 API Request: ${request.method()} ${request.url()}`);
            }
        });
        
        this.page.on('response', response => {
            if ((response.url().includes('odata') || response.url().includes('api')) && response.status() !== 200) {
                console.log(`⚠️ API Response: ${response.status()} ${response.url()}`);
            }
        });
    }

    async login() {
        console.log('🔐 Attempting to login to S/4HANA...');
        
        try {
            // Navigate to the main S/4HANA URL
            await this.page.goto(this.config.baseUrl, { waitUntil: 'networkidle' });
            await this.takeScreenshot('01-initial-page');
            
            // Wait for login form to load
            await this.page.waitForTimeout(3000);
            
            const currentUrl = this.page.url();
            console.log(`Current URL: ${currentUrl}`);
            
            // Look for SAP-specific login form elements
            const userInput = await this.page.$('input[placeholder="User"], input[name="sap-user"], input#j_username');
            const passwordInput = await this.page.$('input[placeholder="Password"], input[name="sap-password"], input#j_password');
            
            // Also try to find by visible text or general input types
            if (!userInput || !passwordInput) {
                console.log('🔍 Looking for alternative login form elements...');
                
                // Wait for any input fields to appear
                await this.page.waitForSelector('input', { timeout: 10000 });
                
                // Get all input fields and try to identify user/password fields
                const allInputs = await this.page.$$('input');
                console.log(`Found ${allInputs.length} input fields`);
                
                let foundUserInput = null;
                let foundPasswordInput = null;
                
                for (const input of allInputs) {
                    const type = await input.getAttribute('type');
                    const placeholder = await input.getAttribute('placeholder');
                    const name = await input.getAttribute('name');
                    
                    console.log(`Input - type: ${type}, placeholder: ${placeholder}, name: ${name}`);
                    
                    if (type === 'text' || type === 'email' || placeholder?.toLowerCase().includes('user')) {
                        foundUserInput = input;
                    } else if (type === 'password' || placeholder?.toLowerCase().includes('password')) {
                        foundPasswordInput = input;
                    }
                }
                
                if (foundUserInput && foundPasswordInput) {
                    console.log('📝 Found SAP login form, attempting to login...');
                    
                    await foundUserInput.fill(this.config.username);
                    await foundPasswordInput.fill(this.config.password);
                    await this.takeScreenshot('02-login-form-filled');
                    
                    // Look for the Log On button (as seen in screenshot)
                    const loginButton = await this.page.$('button:has-text("Log On"), input[value="Log On"], button[type="submit"]');
                    if (loginButton) {
                        console.log('🚀 Clicking Log On button...');
                        await loginButton.click();
                    } else {
                        console.log('⌨️ Pressing Enter on password field...');
                        await foundPasswordInput.press('Enter');
                    }
                    
                    console.log('⏳ Waiting for login to complete...');
                    await this.page.waitForTimeout(8000); // Give more time for SAP to process
                    await this.takeScreenshot('03-after-login');
                    
                } else {
                    console.log('❌ Could not identify username and password fields');
                    return false;
                }
            } else {
                console.log('📝 Found SAP login form elements directly...');
                
                await userInput.fill(this.config.username);
                await passwordInput.fill(this.config.password);
                await this.takeScreenshot('02-login-form-filled');
                
                const loginButton = await this.page.$('button:has-text("Log On"), input[value="Log On"], button[type="submit"]');
                if (loginButton) {
                    await loginButton.click();
                } else {
                    await passwordInput.press('Enter');
                }
                
                console.log('⏳ Waiting for login to complete...');
                await this.page.waitForTimeout(8000);
                await this.takeScreenshot('03-after-login');
            }
            
            // Wait for potential redirects and check final landing page
            await this.page.waitForTimeout(3000);
            const finalUrl = this.page.url();
            console.log(`Final URL after login: ${finalUrl}`);
            await this.takeScreenshot('05-final-landing-page');
            
            // Check if login was successful
            const isLoggedIn = await this.checkLoginSuccess();
            if (isLoggedIn) {
                console.log('✅ Login successful!');
                return true;
            } else {
                console.log('❌ Login may have failed or requires additional steps');
                // Take additional screenshot to see current state
                await this.takeScreenshot('06-login-failed-state');
                return false;
            }
            
        } catch (error) {
            console.error('❌ Login error:', error.message);
            await this.takeScreenshot('error-login');
            return false;
        }
    }

    async checkLoginSuccess() {
        console.log('🔍 Checking for login success indicators...');
        
        // Look for typical S/4HANA interface elements
        const indicators = [
            '.sapUiShell', // SAP UI5 shell
            '[data-sap-ui-type="sap.ui.unified.Shell"]',
            '.sapMShell',
            'div:has-text("Fiori Launchpad")',
            'div:has-text("SAP S/4HANA")',
            '.sapUshellShellHead',
            '.sapMTileContainer',
            '.sapUiUx3ShellHeader',
            '.sapUshellAppTitle', // App title in shell
            '.sapMPage', // General SAP UI5 page
            '.sapUiBody', // SAP UI5 body
            'div[data-sap-ui-root]', // SAP UI5 root element
            '.sapContrast' // SAP contrast theme
        ];
        
        for (const selector of indicators) {
            try {
                const element = await this.page.$(selector);
                if (element) {
                    console.log(`✅ Found login indicator: ${selector}`);
                    return true;
                }
            } catch (e) {
                // Continue checking other selectors
            }
        }
        
        // Check URL patterns
        const url = this.page.url();
        console.log(`Current URL for login check: ${url}`);
        
        if (url.includes('fiori') || url.includes('launchpad') || url.includes('shell') || url.includes('sap/bc/ui5')) {
            console.log('✅ Login successful based on URL pattern');
            return true;
        }
        
        // Check for absence of login form (might indicate successful login)
        const loginForm = await this.page.$('input[placeholder="User"], input[placeholder="Password"]');
        if (!loginForm && !url.includes('/ui')) {
            console.log('✅ Login form disappeared, likely successful login');
            return true;
        }
        
        // Check page title
        const title = await this.page.title();
        console.log(`Page title: ${title}`);
        if (title.includes('SAP') && !title.includes('Log on')) {
            console.log('✅ Login successful based on page title');
            return true;
        }
        
        return false;
    }

    async exploreInterface() {
        console.log('🔍 Exploring S/4HANA interface...');
        
        try {
            // Take screenshot of current state
            await this.takeScreenshot('06-interface-exploration-start');
            
            // Look for navigation elements
            await this.findNavigationElements();
            
            // Look for tiles/apps
            await this.findApplicationTiles();
            
            // Try to find specific business applications
            await this.findBusinessApplications();
            
            // Look for search functionality
            await this.findSearchFunctionality();
            
            return this.discoveredPages;
            
        } catch (error) {
            console.error('❌ Interface exploration error:', error.message);
            await this.takeScreenshot('error-exploration');
            return [];
        }
    }

    async findNavigationElements() {
        console.log('🧭 Looking for navigation elements...');
        
        const navSelectors = [
            '.sapMShellAppTitle', // App title
            '.sapUshellShellHeadTitle', // Shell title
            '.sapMButton', // General buttons
            '.sapMTile', // Tiles
            '.sapMStandardTile', // Standard tiles
            'nav', // Generic nav elements
            '[role="navigation"]',
            '.sapMList', // Lists that might contain navigation
            '.sapMListItem' // List items
        ];
        
        for (const selector of navSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`📍 Found ${elements.length} elements for selector: ${selector}`);
                    
                    // Get text content of first few elements
                    for (let i = 0; i < Math.min(3, elements.length); i++) {
                        const text = await elements[i].textContent();
                        if (text && text.trim()) {
                            console.log(`   - "${text.trim()}"`);
                        }
                    }
                }
            } catch (e) {
                // Continue with next selector
            }
        }
    }

    async findApplicationTiles() {
        console.log('🎯 Looking for application tiles...');
        
        const tileSelectors = [
            '.sapMTile',
            '.sapMStandardTile',
            '.sapMGenericTile',
            '[data-sap-ui-type*="Tile"]',
            '.sapUiUx3NavigationItem'
        ];
        
        const apps = [];
        
        for (const selector of tileSelectors) {
            try {
                const tiles = await this.page.$$(selector);
                console.log(`🔍 Found ${tiles.length} tiles with selector: ${selector}`);
                
                for (const tile of tiles) {
                    const text = await tile.textContent();
                    const isClickable = await tile.isEnabled();
                    
                    if (text && text.trim() && isClickable) {
                        apps.push({
                            selector,
                            text: text.trim(),
                            clickable: isClickable
                        });
                        
                        console.log(`   📱 App: "${text.trim()}"`);
                    }
                }
            } catch (e) {
                // Continue with next selector
            }
        }
        
        this.discoveredPages = apps;
        return apps;
    }

    async findBusinessApplications() {
        console.log('💼 Looking for business applications...');
        
        const businessKeywords = [
            'Master Data',
            'Business Partner',
            'Material',
            'Sales Order',
            'Purchase Order',
            'Customer',
            'Vendor',
            'Supplier',
            'Product',
            'Inventory',
            'Procurement',
            'Sales',
            'Finance',
            'Accounting'
        ];
        
        const foundApps = [];
        
        for (const keyword of businessKeywords) {
            try {
                // Look for elements containing the keyword
                const elements = await this.page.$$(`text=${keyword}`);
                if (elements.length > 0) {
                    console.log(`✅ Found "${keyword}" - ${elements.length} instances`);
                    foundApps.push(keyword);
                }
            } catch (e) {
                // Continue with next keyword
            }
        }
        
        return foundApps;
    }

    async findSearchFunctionality() {
        console.log('🔎 Looking for search functionality...');
        
        const searchSelectors = [
            'input[type="search"]',
            'input[placeholder*="search" i]',
            'input[placeholder*="Search" i]',
            '.sapMSearchField',
            '.sapUiSearchField',
            '[data-sap-ui-type*="SearchField"]'
        ];
        
        for (const selector of searchSelectors) {
            try {
                const searchField = await this.page.$(selector);
                if (searchField) {
                    console.log(`🔍 Found search field: ${selector}`);
                    const placeholder = await searchField.getAttribute('placeholder');
                    console.log(`   Placeholder: "${placeholder}"`);
                    return searchField;
                }
            } catch (e) {
                // Continue with next selector
            }
        }
        
        return null;
    }

    async tryAccessBusinessPartnerApp() {
        console.log('👥 Trying to access Business Partner application...');
        
        try {
            // Look for Business Partner related elements
            const bpSelectors = [
                'text=Business Partner',
                'text=Customer',
                'text=Vendor',
                'text=Supplier',
                '[title*="Business Partner" i]',
                '[aria-label*="Business Partner" i]'
            ];
            
            for (const selector of bpSelectors) {
                try {
                    const element = await this.page.$(selector);
                    if (element) {
                        console.log(`✅ Found Business Partner element: ${selector}`);
                        await element.click();
                        await this.page.waitForTimeout(3000);
                        await this.takeScreenshot('07-business-partner-app');
                        
                        // Look for forms or creation buttons
                        await this.lookForDataCreationForms('Business Partner');
                        return true;
                    }
                } catch (e) {
                    // Continue with next selector
                }
            }
            
            console.log('❌ Could not find Business Partner application');
            return false;
            
        } catch (error) {
            console.error('❌ Error accessing Business Partner app:', error.message);
            return false;
        }
    }

    async lookForDataCreationForms(context) {
        console.log(`📋 Looking for data creation forms in ${context}...`);
        
        const formSelectors = [
            'form',
            '.sapMForm',
            '.sapUiForm',
            '[data-sap-ui-type*="Form"]',
            'button:has-text("Create")',
            'button:has-text("New")',
            'button:has-text("Add")',
            'input[type="text"]',
            'input[type="email"]',
            'select',
            '.sapMSelect',
            '.sapMInput'
        ];
        
        const foundForms = [];
        
        for (const selector of formSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`📝 Found ${elements.length} form elements: ${selector}`);
                    foundForms.push({
                        context,
                        selector,
                        count: elements.length
                    });
                }
            } catch (e) {
                // Continue with next selector
            }
        }
        
        this.discoveredForms.push(...foundForms);
        return foundForms;
    }

    async takeScreenshot(name) {
        try {
            const screenshotPath = path.join(__dirname, 'screenshots', `${name}.png`);
            
            // Ensure screenshots directory exists
            if (!fs.existsSync(path.dirname(screenshotPath))) {
                fs.mkdirSync(path.dirname(screenshotPath), { recursive: true });
            }
            
            await this.page.screenshot({ 
                path: screenshotPath, 
                fullPage: true 
            });
            
            this.createdData.screenshots.push({
                name,
                path: screenshotPath,
                timestamp: new Date().toISOString()
            });
            
            console.log(`📸 Screenshot saved: ${screenshotPath}`);
        } catch (error) {
            console.error(`❌ Screenshot error for ${name}:`, error.message);
        }
    }

    async generateFullReport() {
        console.log('\n📊 GENERATING COMPREHENSIVE S/4HANA EXPLORATION REPORT');
        console.log('=' .repeat(80));
        
        const report = {
            timestamp: new Date().toISOString(),
            systemInfo: {
                baseUrl: this.config.baseUrl,
                finalUrl: this.page ? this.page.url() : 'Unknown'
            },
            discoveredPages: this.discoveredPages,
            discoveredForms: this.discoveredForms,
            screenshots: this.createdData.screenshots,
            recommendations: this.generateRecommendations()
        };
        
        // Save detailed report
        const reportPath = path.join(__dirname, 'reports', 's4hana-exploration-report.json');
        if (!fs.existsSync(path.dirname(reportPath))) {
            fs.mkdirSync(path.dirname(reportPath), { recursive: true });
        }
        
        fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));
        
        // Console output
        console.log('\n🔍 DISCOVERED INTERFACE ELEMENTS:');
        console.log(`   Total Pages/Apps Found: ${this.discoveredPages.length}`);
        console.log(`   Total Forms Found: ${this.discoveredForms.length}`);
        console.log(`   Screenshots Taken: ${this.createdData.screenshots.length}`);
        
        console.log('\n📱 DISCOVERED APPLICATIONS:');
        this.discoveredPages.forEach((app, index) => {
            console.log(`   ${index + 1}. "${app.text}" (${app.clickable ? 'Clickable' : 'Not Clickable'})`);
        });
        
        console.log('\n📋 DISCOVERED FORMS:');
        const formsByContext = {};
        this.discoveredForms.forEach(form => {
            if (!formsByContext[form.context]) {
                formsByContext[form.context] = [];
            }
            formsByContext[form.context].push(form);
        });
        
        Object.keys(formsByContext).forEach(context => {
            console.log(`   ${context}:`);
            formsByContext[context].forEach(form => {
                console.log(`     - ${form.selector} (${form.count} elements)`);
            });
        });
        
        console.log('\n💡 RECOMMENDATIONS:');
        report.recommendations.forEach((rec, index) => {
            console.log(`   ${index + 1}. ${rec}`);
        });
        
        console.log(`\n📄 Full report saved to: ${reportPath}`);
        console.log('📸 Screenshots saved to: ./screenshots/');
        
        return report;
    }

    generateRecommendations() {
        const recommendations = [];
        
        if (this.discoveredPages.length > 0) {
            recommendations.push('Explore clickable application tiles for data creation opportunities');
        }
        
        if (this.discoveredForms.length > 0) {
            recommendations.push('Investigate discovered forms for automated data entry');
        }
        
        recommendations.push('Try navigating through different menu structures');
        recommendations.push('Look for "Create" or "New" buttons in business applications');
        recommendations.push('Test search functionality to find specific business objects');
        recommendations.push('Consider using F12 developer tools to inspect API calls');
        
        return recommendations;
    }

    async cleanup() {
        console.log('🧹 Cleaning up browser resources...');
        
        if (this.page) {
            await this.page.close();
        }
        
        if (this.context) {
            await this.context.close();
        }
        
        if (this.browser) {
            await this.browser.close();
        }
    }

    // Main execution method
    async run() {
        console.log('🏭 Starting S/4HANA Playwright Exploration...\n');
        
        try {
            await this.initialize();
            
            const loginSuccess = await this.login();
            if (!loginSuccess) {
                console.log('⚠️ Login may not have been successful, but continuing exploration...');
            }
            
            await this.exploreInterface();
            
            // Try to access specific business applications
            await this.tryAccessBusinessPartnerApp();
            
            // Generate comprehensive report
            const report = await this.generateFullReport();
            
            console.log('\n🎉 Exploration completed successfully!');
            return report;
            
        } catch (error) {
            console.error('❌ Exploration failed:', error.message);
            await this.takeScreenshot('final-error');
            throw error;
        } finally {
            await this.cleanup();
        }
    }
}

// Export for module usage
module.exports = S4HANAPlaywrightExplorer;

// Run if called directly
if (require.main === module) {
    const explorer = new S4HANAPlaywrightExplorer();
    explorer.run().catch(console.error);
}