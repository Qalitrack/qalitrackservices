const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

/**
 * S/4HANA Fiori Launchpad Explorer
 * Try multiple entry points and authentication methods for S/4HANA trial system
 */
class S4HANAFioriExplorer {
    constructor() {
        this.config = {
            baseUrls: [
                'https://my305028-api.s4hana.ondemand.com',
                'https://my305028-api.s4hana.ondemand.com/sap/bc/ui5_ui5/ui2/ushell/shells/abap/FioriLaunchpad.html',
                'https://my305028-api.s4hana.ondemand.com/sap/bc/ui2/flp',
                'https://my305028-api.s4hana.ondemand.com/sap/bc/ui5_ui5/sap/arsrvc_upb_admn/main.html',
                'https://my305028.s4hana.ondemand.com', // Alternative subdomain
            ],
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12',
            headless: false,
            slowMo: 1500
        };
        
        this.browser = null;
        this.page = null;
        this.context = null;
        this.currentUrl = '';
        
        this.discoveredApps = [];
        this.discoveredForms = [];
        this.accessibleUrls = [];
        this.createdData = {
            screenshots: [],
            loginAttempts: [],
            discoveredEndpoints: []
        };
    }

    async initialize() {
        console.log('🚀 Initializing Fiori Explorer...');
        
        this.browser = await chromium.launch({
            headless: this.config.headless,
            slowMo: this.config.slowMo,
            args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-web-security']
        });
        
        this.context = await this.browser.newContext({
            viewport: { width: 1920, height: 1080 },
            userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
            acceptDownloads: true,
            permissions: ['geolocation']
        });
        
        this.page = await this.context.newPage();
        
        // Enhanced logging
        this.page.on('request', request => {
            if (request.url().includes('sap') || request.url().includes('odata') || request.url().includes('api')) {
                console.log(`📡 Request: ${request.method()} ${request.url()}`);
            }
        });
        
        this.page.on('response', response => {
            if (response.url().includes('sap') || response.url().includes('odata') || response.url().includes('api')) {
                const status = response.status();
                const icon = status >= 200 && status < 300 ? '✅' : status >= 400 ? '❌' : '⚠️';
                console.log(`${icon} Response: ${status} ${response.url()}`);
            }
        });
        
        this.page.on('console', msg => {
            if (msg.type() === 'error') {
                console.log(`🔴 Browser Error: ${msg.text()}`);
            }
        });
    }

    async tryMultipleEntryPoints() {
        console.log('🌐 Trying multiple S/4HANA entry points...');
        
        for (const [index, url] of this.config.baseUrls.entries()) {
            console.log(`\n🔍 Attempt ${index + 1}: ${url}`);
            
            try {
                const result = await this.tryUrl(url, `attempt-${index + 1}`);
                this.accessibleUrls.push({
                    url,
                    accessible: result.success,
                    status: result.status,
                    requiresAuth: result.requiresAuth,
                    hasLoginForm: result.hasLoginForm
                });
                
                if (result.success && result.hasContent) {
                    console.log(`✅ Successfully accessed: ${url}`);
                    this.currentUrl = url;
                    return true;
                }
                
            } catch (error) {
                console.log(`❌ Failed to access ${url}: ${error.message}`);
                this.accessibleUrls.push({
                    url,
                    accessible: false,
                    error: error.message
                });
            }
            
            // Wait between attempts
            await this.page.waitForTimeout(2000);
        }
        
        return false;
    }

    async tryUrl(url, screenshotPrefix) {
        console.log(`🌐 Navigating to: ${url}`);
        
        try {
            const response = await this.page.goto(url, { 
                waitUntil: 'networkidle',
                timeout: 30000 
            });
            
            await this.page.waitForTimeout(3000);
            await this.takeScreenshot(`${screenshotPrefix}-initial`);
            
            const status = response ? response.status() : 'unknown';
            console.log(`   Status: ${status}`);
            
            // Check what we got
            const title = await this.page.title();
            const currentUrl = this.page.url();
            
            console.log(`   Title: ${title}`);
            console.log(`   Final URL: ${currentUrl}`);
            
            // Check for login requirements
            const hasLoginForm = await this.hasLoginForm();
            const requiresAuth = currentUrl.includes('login') || currentUrl.includes('logon') || hasLoginForm;
            
            if (hasLoginForm) {
                console.log('🔐 Login form detected, attempting authentication...');
                const loginResult = await this.attemptLogin(screenshotPrefix);
                
                if (loginResult) {
                    await this.takeScreenshot(`${screenshotPrefix}-post-login`);
                    const hasContent = await this.checkForContent();
                    return { 
                        success: true, 
                        status, 
                        requiresAuth: true, 
                        hasLoginForm: true, 
                        hasContent 
                    };
                }
            }
            
            // Even without login, check if there's useful content
            const hasContent = await this.checkForContent();
            
            return { 
                success: response && response.ok(), 
                status, 
                requiresAuth, 
                hasLoginForm, 
                hasContent 
            };
            
        } catch (error) {
            console.log(`❌ Error accessing ${url}: ${error.message}`);
            await this.takeScreenshot(`${screenshotPrefix}-error`);
            return { success: false, status: 'error', error: error.message };
        }
    }

    async hasLoginForm() {
        const loginSelectors = [
            'input[placeholder="User"]',
            'input[placeholder="Password"]', 
            'input[name="j_username"]',
            'input[name="j_password"]',
            'input[type="password"]',
            'form[action*="login"]',
            'form[action*="logon"]',
            '.sapMDialog', // SAP dialog might contain login
            '.sapUiCore' // General SAP UI indicator
        ];
        
        for (const selector of loginSelectors) {
            try {
                const element = await this.page.$(selector);
                if (element) {
                    console.log(`   🔍 Found login element: ${selector}`);
                    return true;
                }
            } catch (e) {
                // Continue checking
            }
        }
        
        return false;
    }

    async attemptLogin(screenshotPrefix) {
        console.log('🔐 Attempting to login...');
        
        try {
            // Find username field
            const usernameSelectors = [
                'input[placeholder="User"]',
                'input[name="j_username"]',
                'input[type="email"]',
                'input[type="text"]'
            ];
            
            let usernameField = null;
            for (const selector of usernameSelectors) {
                usernameField = await this.page.$(selector);
                if (usernameField) {
                    console.log(`   Found username field: ${selector}`);
                    break;
                }
            }
            
            // Find password field
            const passwordField = await this.page.$('input[type="password"], input[placeholder="Password"], input[name="j_password"]');
            
            if (usernameField && passwordField) {
                await usernameField.fill(this.config.username);
                await passwordField.fill(this.config.password);
                
                await this.takeScreenshot(`${screenshotPrefix}-credentials-filled`);
                
                // Find and click login button
                const loginButtonSelectors = [
                    'button:has-text("Log On")',
                    'button:has-text("Login")',
                    'button:has-text("Sign In")',
                    'input[type="submit"]',
                    'button[type="submit"]'
                ];
                
                let loginButton = null;
                for (const selector of loginButtonSelectors) {
                    try {
                        loginButton = await this.page.$(selector);
                        if (loginButton) {
                            console.log(`   Found login button: ${selector}`);
                            break;
                        }
                    } catch (e) {
                        // Continue
                    }
                }
                
                if (loginButton) {
                    await loginButton.click();
                } else {
                    console.log('   No login button found, pressing Enter');
                    await passwordField.press('Enter');
                }
                
                // Wait for login to process
                console.log('   ⏳ Waiting for login to complete...');
                await this.page.waitForTimeout(8000);
                
                const newUrl = this.page.url();
                const newTitle = await this.page.title();
                
                console.log(`   Post-login URL: ${newUrl}`);
                console.log(`   Post-login Title: ${newTitle}`);
                
                // Check if login was successful
                const stillHasLoginForm = await this.hasLoginForm();
                if (!stillHasLoginForm && !newUrl.includes('error')) {
                    console.log('✅ Login appears successful!');
                    return true;
                } else {
                    console.log('❌ Login may have failed');
                    return false;
                }
                
            } else {
                console.log('❌ Could not find username/password fields');
                return false;
            }
            
        } catch (error) {
            console.log(`❌ Login error: ${error.message}`);
            return false;
        }
    }

    async checkForContent() {
        console.log('🔍 Checking for useful content...');
        
        // Look for SAP UI5/Fiori elements
        const contentSelectors = [
            '.sapUiBody',
            '.sapMTile',
            '.sapMPage',
            '.sapUiCore',
            '[data-sap-ui]',
            '.sapMShell',
            '.sapMList',
            '.sapMButton',
            'div[id*="sap"]',
            '.sapContrast'
        ];
        
        let foundElements = 0;
        for (const selector of contentSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`   ✅ Found ${elements.length} elements: ${selector}`);
                    foundElements += elements.length;
                }
            } catch (e) {
                // Continue
            }
        }
        
        // Look for specific business content
        const businessKeywords = [
            'Fiori', 'Launchpad', 'Business Partner', 'Material', 'Sales', 
            'Purchase', 'Finance', 'Master Data', 'Transaction'
        ];
        
        const pageText = await this.page.textContent('body');
        const foundKeywords = businessKeywords.filter(keyword => 
            pageText && pageText.toLowerCase().includes(keyword.toLowerCase())
        );
        
        if (foundKeywords.length > 0) {
            console.log(`   ✅ Found business keywords: ${foundKeywords.join(', ')}`);
        }
        
        return foundElements > 0 || foundKeywords.length > 0;
    }

    async exploreCurrentPage() {
        console.log('🔍 Exploring current page for apps and forms...');
        
        await this.takeScreenshot('current-page-exploration');
        
        // Look for clickable tiles/apps
        const tileSelectors = [
            '.sapMTile',
            '.sapMStandardTile', 
            '.sapMGenericTile',
            'a[href*="app"]',
            'div[role="button"]',
            '.sapMButton',
            '.sapMListItem'
        ];
        
        for (const selector of tileSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`📱 Found ${elements.length} potential apps: ${selector}`);
                    
                    for (let i = 0; i < Math.min(5, elements.length); i++) {
                        const text = await elements[i].textContent();
                        if (text && text.trim()) {
                            this.discoveredApps.push({
                                selector,
                                text: text.trim(),
                                index: i
                            });
                            console.log(`   - "${text.trim()}"`);
                        }
                    }
                }
            } catch (e) {
                // Continue
            }
        }
        
        // Look for forms
        const formSelectors = [
            'form',
            'input[type="text"]',
            'input[type="email"]',
            'select',
            'textarea',
            '.sapMInput',
            '.sapMSelect'
        ];
        
        for (const selector of formSelectors) {
            try {
                const elements = await this.page.$$(selector);
                if (elements.length > 0) {
                    console.log(`📝 Found ${elements.length} form elements: ${selector}`);
                    this.discoveredForms.push({
                        selector,
                        count: elements.length
                    });
                }
            } catch (e) {
                // Continue
            }
        }
    }

    async tryClickFirstApp() {
        if (this.discoveredApps.length > 0) {
            console.log('🖱️ Trying to click first discovered app...');
            
            const firstApp = this.discoveredApps[0];
            try {
                const elements = await this.page.$$(firstApp.selector);
                if (elements[firstApp.index]) {
                    await elements[firstApp.index].click();
                    await this.page.waitForTimeout(5000);
                    await this.takeScreenshot('first-app-clicked');
                    
                    console.log(`✅ Clicked on: ${firstApp.text}`);
                    return true;
                }
            } catch (error) {
                console.log(`❌ Error clicking app: ${error.message}`);
            }
        }
        
        return false;
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
            
            this.createdData.screenshots.push({
                name,
                path: screenshotPath,
                timestamp: new Date().toISOString()
            });
            
            console.log(`📸 Screenshot: ${screenshotPath}`);
        } catch (error) {
            console.error(`❌ Screenshot error: ${error.message}`);
        }
    }

    async generateReport() {
        console.log('\n📊 GENERATING S/4HANA FIORI EXPLORATION REPORT');
        console.log('=' .repeat(80));
        
        const report = {
            timestamp: new Date().toISOString(),
            accessibleUrls: this.accessibleUrls,
            discoveredApps: this.discoveredApps,
            discoveredForms: this.discoveredForms,
            screenshots: this.createdData.screenshots,
            recommendations: []
        };
        
        // Generate recommendations
        if (this.accessibleUrls.some(u => u.accessible)) {
            report.recommendations.push('Successfully accessed S/4HANA system - continue with app exploration');
        } else {
            report.recommendations.push('No URLs accessible - check credentials or trial system status');
        }
        
        if (this.discoveredApps.length > 0) {
            report.recommendations.push(`Found ${this.discoveredApps.length} apps - try clicking on business-related apps`);
        }
        
        if (this.discoveredForms.length > 0) {
            report.recommendations.push(`Found form elements - investigate for data creation opportunities`);
        }
        
        // Save report
        const reportPath = path.join(__dirname, 'reports', 'fiori-exploration-report.json');
        if (!fs.existsSync(path.dirname(reportPath))) {
            fs.mkdirSync(path.dirname(reportPath), { recursive: true });
        }
        
        fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));
        
        // Console output
        console.log('\n🌐 URL ACCESS RESULTS:');
        this.accessibleUrls.forEach((url, index) => {
            const status = url.accessible ? '✅' : '❌';
            console.log(`   ${status} ${url.url} (${url.status || 'unknown'})`);
            if (url.requiresAuth) console.log(`      🔐 Requires authentication`);
            if (url.hasLoginForm) console.log(`      📝 Has login form`);
        });
        
        console.log('\n📱 DISCOVERED APPS:');
        this.discoveredApps.forEach((app, index) => {
            console.log(`   ${index + 1}. "${app.text}"`);
        });
        
        console.log('\n📋 DISCOVERED FORMS:');
        this.discoveredForms.forEach((form, index) => {
            console.log(`   ${index + 1}. ${form.selector} (${form.count} elements)`);
        });
        
        console.log(`\n📄 Report saved: ${reportPath}`);
        console.log(`📸 Screenshots: ${this.createdData.screenshots.length} saved`);
        
        return report;
    }

    async cleanup() {
        console.log('🧹 Cleaning up...');
        if (this.page) await this.page.close();
        if (this.context) await this.context.close();
        if (this.browser) await this.browser.close();
    }

    async run() {
        console.log('🏭 Starting S/4HANA Fiori Explorer...\n');
        
        try {
            await this.initialize();
            
            // Try multiple entry points
            const accessSuccess = await this.tryMultipleEntryPoints();
            
            if (accessSuccess) {
                // Explore the current page
                await this.exploreCurrentPage();
                
                // Try to click on first app if any found
                await this.tryClickFirstApp();
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
module.exports = S4HANAFioriExplorer;

// Run if called directly
if (require.main === module) {
    const explorer = new S4HANAFioriExplorer();
    explorer.run().catch(console.error);
}