const https = require('https');

/**
 * Simple S/4HANA API Connectivity Test
 * Tests basic authentication and API access
 */

class S4HANAAPITester {
    constructor() {
        this.config = {
            hostname: 'my305028-api.s4hana.ondemand.com',
            username: 'surgbc@gmail.com',
            password: 'qaliHANA12'
        };
        this.csrfToken = null;
    }

    async testBasicAuth() {
        console.log('🔐 Testing Basic Authentication...');
        
        return new Promise((resolve, reject) => {
            const auth = Buffer.from(`${this.config.username}:${this.config.password}`).toString('base64');
            
            const options = {
                hostname: this.config.hostname,
                path: '/sap/opu/odata/sap/API_BUSINESS_PARTNER/',
                method: 'GET',
                headers: {
                    'Authorization': `Basic ${auth}`,
                    'Accept': 'application/json',
                    'X-CSRF-Token': 'Fetch'
                }
            };

            const req = https.request(options, (res) => {
                console.log(`Status Code: ${res.statusCode}`);
                console.log('Headers:', Object.keys(res.headers));
                
                // Store CSRF token if present
                if (res.headers['x-csrf-token']) {
                    this.csrfToken = res.headers['x-csrf-token'];
                    console.log('✅ CSRF Token obtained');
                } else {
                    console.log('⚠️ No CSRF Token in response');
                }
                
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => {
                    if (res.statusCode === 200) {
                        console.log('✅ Basic Authentication successful');
                        try {
                            const jsonData = JSON.parse(data);
                            console.log('📄 Response type: JSON');
                            if (jsonData.d && jsonData.d.EntitySets) {
                                console.log('📋 Available EntitySets:');
                                jsonData.d.EntitySets.forEach(entity => {
                                    console.log(`   - ${entity.Name}`);
                                });
                            }
                        } catch (e) {
                            console.log('📄 Response type: XML/Other');
                            console.log('Response preview:', data.substring(0, 200) + '...');
                        }
                        resolve(true);
                    } else {
                        console.log('❌ Authentication failed');
                        console.log('Response:', data.substring(0, 500));
                        resolve(false);
                    }
                });
            });

            req.on('error', (error) => {
                console.error('❌ Request failed:', error.message);
                reject(error);
            });

            req.end();
        });
    }

    async testAPIEndpoints() {
        console.log('\n🧪 Testing API Endpoints...');
        
        const endpoints = [
            {
                name: 'Business Partner API',
                path: '/sap/opu/odata/sap/API_BUSINESS_PARTNER/'
            },
            {
                name: 'Material API',
                path: '/sap/opu/odata/sap/API_MATERIAL_SRV/'
            },
            {
                name: 'Sales Order API',
                path: '/sap/opu/odata/sap/API_SALES_ORDER_SRV/'
            },
            {
                name: 'Plant API',
                path: '/sap/opu/odata/sap/API_PLANT_SRV/'
            }
        ];

        const results = {};
        
        for (const endpoint of endpoints) {
            const result = await this.testEndpoint(endpoint.name, endpoint.path);
            results[endpoint.name] = result;
            await this.sleep(1000); // Wait 1 second between tests
        }
        
        return results;
    }

    async testEndpoint(name, path) {
        return new Promise((resolve) => {
            const auth = Buffer.from(`${this.config.username}:${this.config.password}`).toString('base64');
            
            const options = {
                hostname: this.config.hostname,
                path: path,
                method: 'GET',
                headers: {
                    'Authorization': `Basic ${auth}`,
                    'Accept': 'application/json'
                }
            };

            const req = https.request(options, (res) => {
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => {
                    const success = res.statusCode === 200;
                    console.log(`${name}: ${success ? '✅' : '❌'} (${res.statusCode})`);
                    
                    if (!success && res.statusCode !== 404) {
                        console.log(`   Error: ${data.substring(0, 100)}...`);
                    }
                    
                    resolve({
                        success,
                        statusCode: res.statusCode,
                        response: data.substring(0, 200)
                    });
                });
            });

            req.on('error', (error) => {
                console.log(`${name}: ❌ (Network Error)`);
                resolve({
                    success: false,
                    statusCode: 0,
                    error: error.message
                });
            });

            req.setTimeout(10000, () => {
                console.log(`${name}: ❌ (Timeout)`);
                req.destroy();
                resolve({
                    success: false,
                    statusCode: 0,
                    error: 'Timeout'
                });
            });

            req.end();
        });
    }

    async testBusinessPartnerQuery() {
        console.log('\n👥 Testing Business Partner Query...');
        
        return new Promise((resolve) => {
            const auth = Buffer.from(`${this.config.username}:${this.config.password}`).toString('base64');
            
            const options = {
                hostname: this.config.hostname,
                path: '/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_BusinessPartner?$top=5',
                method: 'GET',
                headers: {
                    'Authorization': `Basic ${auth}`,
                    'Accept': 'application/json'
                }
            };

            const req = https.request(options, (res) => {
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => {
                    if (res.statusCode === 200) {
                        try {
                            const jsonData = JSON.parse(data);
                            if (jsonData.d && jsonData.d.results) {
                                console.log(`✅ Found ${jsonData.d.results.length} Business Partners`);
                                jsonData.d.results.forEach((bp, index) => {
                                    console.log(`   ${index + 1}. ${bp.BusinessPartner} - ${bp.OrganizationBPName1 || bp.PersonFullName || 'Unknown'}`);
                                });
                            } else {
                                console.log('✅ Query successful but no results structure found');
                            }
                        } catch (e) {
                            console.log('✅ Query successful but response not JSON');
                        }
                    } else {
                        console.log(`❌ Query failed: ${res.statusCode}`);
                        console.log('Response:', data.substring(0, 300));
                    }
                    resolve(res.statusCode === 200);
                });
            });

            req.on('error', (error) => {
                console.error('❌ Query request failed:', error.message);
                resolve(false);
            });

            req.end();
        });
    }

    async sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async runFullTest() {
        console.log('🚀 Starting S/4HANA API Connectivity Test');
        console.log('=' .repeat(50));
        console.log(`Target System: ${this.config.hostname}`);
        console.log(`Username: ${this.config.username}`);
        console.log('=' .repeat(50));

        try {
            // Test 1: Basic Authentication
            const authSuccess = await this.testBasicAuth();
            
            if (!authSuccess) {
                console.log('\n❌ Basic authentication failed. Stopping tests.');
                return false;
            }

            // Test 2: API Endpoints
            const endpointResults = await this.testAPIEndpoints();
            
            // Test 3: Data Query
            if (endpointResults['Business Partner API']?.success) {
                await this.testBusinessPartnerQuery();
            }

            // Summary
            console.log('\n📊 TEST SUMMARY');
            console.log('=' .repeat(30));
            console.log(`Authentication: ${authSuccess ? '✅' : '❌'}`);
            console.log(`CSRF Token: ${this.csrfToken ? '✅' : '❌'}`);
            
            Object.entries(endpointResults).forEach(([name, result]) => {
                console.log(`${name}: ${result.success ? '✅' : '❌'}`);
            });

            const overallSuccess = authSuccess && Object.values(endpointResults).some(r => r.success);
            
            console.log('\n🎯 Next Steps:');
            if (overallSuccess) {
                console.log('✅ API connectivity confirmed!');
                console.log('   → Ready to create Bamburi Cement data');
                console.log('   → Run: node s4hana-api-data-creator.js');
            } else {
                console.log('❌ API connectivity issues detected');
                console.log('   → Check authentication credentials');
                console.log('   → Verify trial system is active');
                console.log('   → Consider using Playwright automation instead');
            }

            return overallSuccess;

        } catch (error) {
            console.error('❌ Test execution failed:', error.message);
            return false;
        }
    }
}

// Run if called directly
if (require.main === module) {
    const tester = new S4HANAAPITester();
    tester.runFullTest().catch(console.error);
}

module.exports = S4HANAAPITester;