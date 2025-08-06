const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

/**
 * S/4HANA SAP ID Service Login and Data Creator
 * Handles the proper SAP ID Service authentication flow
 */
class S4HANASapIdExplorer {
    constructor() {
        this.config = {
            startUrl: 'https://my305028.s4hana.ondemand.com', // This redirects to SAP ID Service
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12',
            headless: false,
            slowMo: 1500
        };
        
        this.browser = null;
        this.page = null;
        this.context = null;
        
        this.discoveredApps = [];
        this.discoveredForms = [];
        this.screenshots = [];
        this.dataCreationOpportunities = [];
    }

    async initialize() {
        console.log('🚀 Initializing SAP ID Service Explorer...');
        
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
        
        // Enhanced request logging
        this.page.on('request', request => {
            const url = request.url();
            if (url.includes('sap') || url.includes('odata') || url.includes('api') || url.includes('fiori')) {
                console.log(`📡 ${request.method()} ${url.substring(0, 100)}...`);
            }
        });
        
        this.page.on('response', response => {
            const url = response.url();
            if (url.includes('sap') || url.includes('odata') || url.includes('api') || url.includes('fiori')) {
                const status = response.status();
                const icon = status >= 200 && status < 300 ? '✅' : status >= 400 ? '❌' : '⚠️';
                console.log(`${icon} ${status} ${url.substring(0, 100)}...`);
            }
        });
    }

    async loginViaSapId() {
        console.log('🔐 Starting SAP ID Service login flow...');
        
        try {
            // Navigate to the S/4HANA system (will redirect to SAP ID Service)
            console.log('🌐 Navigating to S/4HANA system...');
            await this.page.goto(this.config.startUrl, { waitUntil: 'networkidle', timeout: 30000 });
            await this.takeScreenshot('01-initial-redirect');
            
            // Wait a bit more for any additional redirects
            await this.page.waitForTimeout(3000);
            
            const currentUrl = this.page.url();
            const title = await this.page.title();
            console.log(`Current URL: ${currentUrl}`);
            console.log(`Page Title: ${title}`);
            
            // Check if we're on the SAP ID Service login page
            if (currentUrl.includes('accounts.sap.com')) {
                console.log('✅ Reached SAP ID Service login page');
                await this.handleSapIdLogin();
            } else {
                console.log('⚠️ Not on expected SAP ID Service page, checking for alternatives...');
                await this.handleAlternativeLogin();
            }
            
            return await this.checkLoginSuccess();
            
        } catch (error) {
            console.error('❌ Login error:', error.message);
            await this.takeScreenshot('error-login');
            return false;
        }
    }

    async handleSapIdLogin() {
        console.log('🔑 Handling SAP ID Service login...');
        
        // Look for the email/username field
        const usernameField = await this.page.$('input[name="j_username"], input[type="email"], input[placeholder*="Mail" i], input[placeholder*="Login" i]');
        
        if (usernameField) {
            console.log('📝 Found username field, entering credentials...');
            await usernameField.fill(this.config.username);
            
            // Look for continue button (SAP ID Service has a two-step process)
            const continueButton = await this.page.$('button:has-text("Continue"), input[value="Continue"], button[type="submit"]');
            if (continueButton) {
                console.log('🔄 Clicking Continue button...');
                await continueButton.click();
                await this.page.waitForTimeout(3000);
                await this.takeScreenshot('02-after-username');
            }
            
            // Now look for password field (might appear after continue)
            await this.page.waitForSelector('input[type="password"]', { timeout: 10000 });
            const passwordField = await this.page.$('input[type="password"]');
            
            if (passwordField) {
                console.log('🔒 Found password field, entering password...');
                await passwordField.fill(this.config.password);
                await this.takeScreenshot('03-credentials-filled');
                
                // Look for login/sign in button
                const loginButton = await this.page.$('button:has-text("Sign In"), button:has-text("Log On"), button:has-text("Continue"), button[type="submit"]');
                if (loginButton) {
                    console.log('🚀 Clicking login button...');
                    await loginButton.click();
                } else {
                    console.log('⌨️ Pressing Enter on password field...');
                    await passwordField.press('Enter');
                }
                
                console.log('⏳ Waiting for authentication to complete...');
                await this.page.waitForTimeout(10000); // Give more time for SAP authentication
                await this.takeScreenshot('04-after-login');
            }
        } else {
            console.log('❌ Could not find username field');
            return false;
        }
        
        return true;
    }

    async handleAlternativeLogin() {
        console.log('🔍 Checking for alternative login methods...');
        
        // Look for standard login fields
        const usernameField = await this.page.$('input[type="text"], input[type="email"], input[placeholder*="User" i]');
        const passwordField = await this.page.$('input[type="password"]');
        
        if (usernameField && passwordField) {
            console.log('📝 Found standard login form...');
            await usernameField.fill(this.config.username);
            await passwordField.fill(this.config.password);
            
            const loginButton = await this.page.$('button:has-text("Log"), button[type="submit"], input[type="submit"]');
            if (loginButton) {
                await loginButton.click();
            } else {
                await passwordField.press('Enter');
            }
            
            await this.page.waitForTimeout(8000);
        }
    }

    async checkLoginSuccess() {
        console.log('🔍 Checking login success...');
        
        const currentUrl = this.page.url();
        const title = await this.page.title();
        
        console.log(`Post-login URL: ${currentUrl}`);
        console.log(`Post-login Title: ${title}`);
        
        await this.takeScreenshot('05-login-check');
        
        // Success indicators
        const successIndicators = [
            // URL patterns
            currentUrl.includes('fiori'),
            currentUrl.includes('launchpad'),
            currentUrl.includes('shell'),
            currentUrl.includes('s4hana') && !currentUrl.includes('accounts.sap.com'),
            
            // Title patterns
            title.includes('Fiori'),
            title.includes('Launchpad'),
            title.includes('S/4HANA'),
            title.includes('SAP') && !title.includes('Sign In') && !title.includes('Log'),
            
            // Not on error/login pages
            !currentUrl.includes('error'),
            !currentUrl.includes('login'),
            !currentUrl.includes('accounts.sap.com')
        ];
        
        const successCount = successIndicators.filter(Boolean).length;
        console.log(`Success indicators: ${successCount}/${successIndicators.length}`);
        
        if (successCount >= 3) {
            console.log('✅ Login appears successful!');
            return true;
        } else {
            console.log('❌ Login may have failed');
            return false;
        }
    }

    async exploreInterface() {
        console.log('🔍 Exploring S/4HANA interface...');
        
        await this.takeScreenshot('06-interface-start');
        
        // Look for Fiori tiles/apps
        await this.findFioriTiles();
        
        // Look for navigation menus
        await this.findNavigationMenus();
        
        // Look for business applications
        await this.findBusinessApplications();
        
        // Look for create/new buttons
        await this.findDataCreationOpportunities();
    }

    async findFioriTiles() {
        console.log('🎨 Looking for Fiori tiles...');
        
        const tileSelectors = [
            '.sapMTile',
            '.sapMStandardTile',
            '.sapMGenericTile',
            '.sapUshellTile',
            '[data-sap-ui-type*="Tile"]',
            'div[role="button"]',
            '.sapMButton'
        ];
        
        for (const selector of tileSelectors) {
            try {
                const tiles = await this.page.$$(selector);
                if (tiles.length > 0) {
                    console.log(`🎯 Found ${tiles.length} tiles: ${selector}`);
                    
                    for (let i = 0; i < Math.min(10, tiles.length); i++) {
                        const text = await tiles[i].textContent();
                        if (text && text.trim()) {
                            this.discoveredApps.push({
                                type: 'tile',
                                selector,
                                text: text.trim(),
                                index: i
                            });
                            console.log(`   📱 "${text.trim()}"`);
                        }
                    }
                }
            } catch (e) {
                // Continue with next selector
            }
        }
    }

    async findNavigationMenus() {
        console.log('🧭 Looking for navigation menus...');
        
        const menuSelectors = [
            '.sapMMenu',
            '.sapMList',
            '.sapUiMnuItm',
            '.sapMListItem',
            'nav',
            '[role="navigation"]',
            '.sapUshellShellHeader'
        ];
        
        for (const selector of menuSelectors) {
            try {
                const menus = await this.page.$$(selector);
                if (menus.length > 0) {
                    console.log(`🗂️ Found ${menus.length} navigation elements: ${selector}`);
                }
            } catch (e) {
                // Continue
            }
        }
    }

    async findBusinessApplications() {
        console.log('💼 Looking for business applications...');
        
        // Business keywords to search for
        const businessKeywords = [
            'Master Data', 'Business Partner', 'Customer', 'Vendor', 'Supplier',
            'Material', 'Product', 'Sales Order', 'Purchase Order', 'Invoice',
            'Finance', 'Accounting', 'Procurement', 'Inventory', 'Warehouse'
        ];
        
        const pageText = await this.page.textContent('body');
        const foundKeywords = [];
        
        for (const keyword of businessKeywords) {
            if (pageText && pageText.toLowerCase().includes(keyword.toLowerCase())) {
                foundKeywords.push(keyword);
                console.log(`✅ Found business area: ${keyword}`);
            }
        }
        
        return foundKeywords;
    }

    async findDataCreationOpportunities() {
        console.log('➕ Looking for data creation opportunities...');
        
        const creationSelectors = [
            'button:has-text("Create")',
            'button:has-text("New")',
            'button:has-text("Add")',
            'a:has-text("Create")',
            'a:has-text("New")',
            '.sapMButton:has-text("Create")',
            '.sapMButton:has-text("New")',
            '[title*="Create" i]',
            '[aria-label*="Create" i]'
        ];
        
        for (const selector of creationSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`➕ Found ${elements.length} creation buttons: ${selector}`);
                    
                    for (const element of elements) {
                        const text = await element.textContent();
                        if (text && text.trim()) {
                            this.dataCreationOpportunities.push({
                                selector,
                                text: text.trim()
                            });
                            console.log(`   🔧 "${text.trim()}"`);
                        }
                    }
                }
            } catch (e) {
                // Continue
            }
        }
    }

    async tryClickBusinessApp(appName) {
        console.log(`🖱️ Trying to click on ${appName} app...`);
        
        // Look for the app by name
        const appSelectors = [
            `*:has-text("${appName}")`,
            `[title*="${appName}" i]`,
            `[aria-label*="${appName}" i]`
        ];
        
        for (const selector of appSelectors) {
            try {
                const element = await this.page.$(selector);
                if (element) {
                    console.log(`✅ Found ${appName} element: ${selector}`);
                    await element.click();
                    await this.page.waitForTimeout(5000);
                    await this.takeScreenshot(`07-${appName.toLowerCase().replace(/\s+/g, '-')}-app`);
                    
                    // Look for forms in the opened app
                    await this.lookForFormsInCurrentView();
                    return true;
                }
            } catch (e) {
                // Continue with next selector
            }
        }
        
        console.log(`❌ Could not find ${appName} app`);
        return false;
    }

    async lookForFormsInCurrentView() {
        console.log('📋 Looking for forms in current view...');
        
        const formSelectors = [
            'form',
            'input[type="text"]',
            'input[type="email"]',
            'input[type="number"]',
            'select',
            'textarea',
            '.sapMInput',
            '.sapMSelect',
            '.sapMComboBox',
            '.sapMDatePicker'
        ];
        
        for (const selector of formSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`📝 Found ${elements.length} form elements: ${selector}`);
                    this.discoveredForms.push({
                        selector,
                        count: elements.length,
                        context: 'business-app'
                    });
                }
            } catch (e) {
                // Continue
            }
        }
    }

    async takeScreenshot(name) {
        try {
            const screenshotPath = path.join(__dirname, 'screenshots', `${name}.png`);
            
            if (!fs.existsSync(path.dirname(screenshotPath))) {
                fs.mkdirSync(path.dirname(screenshotPath), { recursive: true });
            }
            
            await this.page.screenshot({ 
                path: screenshotPath, 
                fullPage: true 
            });
            
            this.screenshots.push({
                name,
                path: screenshotPath,
                timestamp: new Date().toISOString()
            });
            
            console.log(`📸 Screenshot: ${name}.png`);
        } catch (error) {
            console.error(`❌ Screenshot error: ${error.message}`);
        }
    }

    async generateDataCreationScript() {
        console.log('🔧 Generating data creation script based on discoveries...');
        
        let script = `// Auto-generated S/4HANA Data Creation Script
// Based on interface exploration results

const { chromium } = require('playwright');

class S4HANADataCreator {
    constructor() {
        this.config = {
            baseUrl: '${this.config.startUrl}',
            username: '${this.config.username}',
            password: '${this.config.password}',
            headless: false,
            slowMo: 1000
        };
        
        this.createdRecords = [];
    }
    
    async initialize() {
        this.browser = await chromium.launch({
            headless: this.config.headless,
            slowMo: this.config.slowMo
        });
        
        this.context = await this.browser.newContext({
            viewport: { width: 1920, height: 1080 }
        });
        
        this.page = await this.context.newPage();
    }
    
    async login() {
        // Login implementation based on successful flow
        await this.page.goto(this.config.baseUrl);
        
        // Wait for SAP ID Service redirect
        await this.page.waitForSelector('input[name="j_username"]', { timeout: 10000 });
        
        // Enter username
        const usernameField = await this.page.$('input[name="j_username"]');
        await usernameField.fill(this.config.username);
        
        // Click continue
        const continueBtn = await this.page.$('button:has-text("Continue")');
        await continueBtn.click();
        
        // Enter password
        await this.page.waitForSelector('input[type="password"]', { timeout: 10000 });
        const passwordField = await this.page.$('input[type="password"]');
        await passwordField.fill(this.config.password);
        
        // Submit login
        const loginBtn = await this.page.$('button:has-text("Sign In")');
        await loginBtn.click();
        
        // Wait for redirect to S/4HANA
        await this.page.waitForTimeout(10000);
    }
    
    // Data creation methods based on discovered opportunities
`;

        // Add discovered apps as methods
        this.discoveredApps.forEach((app, index) => {
            const methodName = app.text.toLowerCase().replace(/[^a-z0-9]/g, '');
            script += `
    async create${methodName.charAt(0).toUpperCase() + methodName.slice(1)}Data() {
        console.log('Creating data in ${app.text}...');
        
        // Click on ${app.text} tile/app
        const appElement = await this.page.$('${app.selector}');
        if (appElement) {
            await appElement.click();
            await this.page.waitForTimeout(3000);
            
            // Look for create/new buttons
            const createBtn = await this.page.$('button:has-text("Create"), button:has-text("New"), .sapMButton:has-text("Create")');
            if (createBtn) {
                await createBtn.click();
                await this.page.waitForTimeout(2000);
                
                // TODO: Fill form fields based on discovered forms
                // Add specific field filling logic here
                
                console.log('${app.text} data creation completed');
            }
        }
    }
`;
        });

        script += `
    async cleanup() {
        if (this.browser) {
            await this.browser.close();
        }
    }
    
    async run() {
        try {
            await this.initialize();
            await this.login();
            
            // Execute data creation methods
            ${this.discoveredApps.map((app, index) => {
                const methodName = app.text.toLowerCase().replace(/[^a-z0-9]/g, '');
                return `await this.create${methodName.charAt(0).toUpperCase() + methodName.slice(1)}Data();`;
            }).join('\n            ')}
            
            console.log('Data creation completed!');
            
        } catch (error) {
            console.error('Error:', error.message);
        } finally {
            await this.cleanup();
        }
    }
}

// Run if called directly
if (require.main === module) {
    const creator = new S4HANADataCreator();
    creator.run().catch(console.error);
}

module.exports = S4HANADataCreator;
`;

        // Save the generated script
        const scriptPath = path.join(__dirname, 'generated-s4hana-data-creator.js');
        fs.writeFileSync(scriptPath, script);
        console.log(`📄 Generated data creation script: ${scriptPath}`);
        
        return scriptPath;
    }

    async generateReport() {
        console.log('\n📊 GENERATING COMPREHENSIVE EXPLORATION REPORT');
        console.log('=' .repeat(80));
        
        const report = {
            timestamp: new Date().toISOString(),
            loginMethod: 'SAP ID Service',
            finalUrl: this.page ? this.page.url() : 'Unknown',
            discoveredApps: this.discoveredApps,
            discoveredForms: this.discoveredForms,
            dataCreationOpportunities: this.dataCreationOpportunities,
            screenshots: this.screenshots,
            recommendations: []
        };
        
        // Generate recommendations
        if (this.discoveredApps.length > 0) {
            report.recommendations.push(`Found ${this.discoveredApps.length} applications - focus on business-related tiles`);
        }
        
        if (this.dataCreationOpportunities.length > 0) {
            report.recommendations.push(`Found ${this.dataCreationOpportunities.length} creation opportunities`);
        }
        
        if (this.discoveredForms.length > 0) {
            report.recommendations.push(`Found form elements in ${this.discoveredForms.length} contexts`);
        }
        
        // Save report
        const reportPath = path.join(__dirname, 'reports', 'sap-id-exploration-report.json');
        if (!fs.existsSync(path.dirname(reportPath))) {
            fs.mkdirSync(path.dirname(reportPath), { recursive: true });
        }
        
        fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));
        
        // Console output
        console.log(`\n🔗 Final URL: ${report.finalUrl}`);
        
        console.log('\n📱 DISCOVERED APPLICATIONS:');
        this.discoveredApps.forEach((app, index) => {
            console.log(`   ${index + 1}. "${app.text}" (${app.type})`);
        });
        
        console.log('\n➕ DATA CREATION OPPORTUNITIES:');
        this.dataCreationOpportunities.forEach((opp, index) => {
            console.log(`   ${index + 1}. "${opp.text}"`);
        });
        
        console.log('\n📋 DISCOVERED FORMS:');
        this.discoveredForms.forEach((form, index) => {
            console.log(`   ${index + 1}. ${form.selector} (${form.count} elements) - ${form.context}`);
        });
        
        console.log(`\n📄 Report saved: ${reportPath}`);
        console.log(`📸 Screenshots: ${this.screenshots.length} saved`);
        
        return report;
    }

    async cleanup() {
        console.log('🧹 Cleaning up...');
        if (this.page) await this.page.close();
        if (this.context) await this.context.close();
        if (this.browser) await this.browser.close();
    }

    async run() {
        console.log('🏭 Starting S/4HANA SAP ID Service Explorer...\n');
        
        try {
            await this.initialize();
            
            // Attempt login via SAP ID Service
            const loginSuccess = await this.loginViaSapId();
            
            if (loginSuccess) {
                console.log('✅ Login successful, exploring interface...');
                
                // Explore the interface
                await this.exploreInterface();
                
                // Try to access specific business apps
                const businessApps = ['Business Partner', 'Material', 'Customer', 'Sales Order'];
                for (const app of businessApps) {
                    await this.tryClickBusinessApp(app);
                    await this.page.waitForTimeout(2000);
                }
                
                // Generate data creation script
                await this.generateDataCreationScript();
                
            } else {
                console.log('❌ Login failed, but continuing with available exploration...');
            }
            
            // Generate comprehensive report
            const report = await this.generateReport();
            
            console.log('\n🎉 Exploration completed!');
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
module.exports = S4HANASapIdExplorer;

// Run if called directly
if (require.main === module) {
    const explorer = new S4HANASapIdExplorer();
    explorer.run().catch(console.error);
}