/**
 * QaliTrack API Client
 * Handles all API communications for the business flow test interface
 */

class QaliTrackAPIClient {
    constructor() {
        this.config = {
            masterData: 'http://localhost:5004/api',
            dataManager: 'http://localhost:5005/api'
        };
        this.requestTimeout = 10000; // 10 seconds
    }

    /**
     * Make an API call with error handling and timeout
     */
    async apiCall(endpoint, method = 'GET', data = null) {
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.requestTimeout);

            const options = {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                signal: controller.signal
            };

            if (data) {
                options.body = JSON.stringify(data);
            }

            const response = await fetch(endpoint, options);
            clearTimeout(timeoutId);

            let result;
            const contentType = response.headers.get('content-type');
            
            if (contentType && contentType.includes('application/json')) {
                result = await response.json();
            } else {
                result = await response.text();
            }

            return {
                success: response.ok,
                status: response.status,
                data: result,
                headers: Object.fromEntries(response.headers.entries())
            };
        } catch (error) {
            if (error.name === 'AbortError') {
                return {
                    success: false,
                    status: 0,
                    data: `Request timeout after ${this.requestTimeout / 1000} seconds`,
                    error: 'TIMEOUT'
                };
            }
            return {
                success: false,
                status: 0,
                data: error.message,
                error: error.name
            };
        }
    }

    /**
     * Test API connection
     */
    async testConnection(apiType) {
        const endpoints = {
            masterData: `${this.config.masterData}/SiteManagement/zones?page=1&pageSize=1`,
            dataManager: `${this.config.dataManager}/Transactions?page=1&pageSize=1`
        };

        const endpoint = endpoints[apiType];
        if (!endpoint) {
            return { success: false, data: 'Invalid API type' };
        }

        return await this.apiCall(endpoint);
    }

    // MasterData API Methods
    async createLocationTypes(data) {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/location-types`, 'POST', data);
    }

    async getLocationTypes() {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/location-types`);
    }

    async createZones(data) {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/zones`, 'POST', data);
    }

    async getZones() {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/zones`);
    }

    async createSites(data) {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/sites`, 'POST', data);
    }

    async getSites() {
        return await this.apiCall(`${this.config.masterData}/SiteManagement/sites`);
    }

    async createBusinessEntity(data) {
        return await this.apiCall(`${this.config.masterData}/BusinessEntity`, 'POST', data);
    }

    async getBusinessEntities() {
        return await this.apiCall(`${this.config.masterData}/BusinessEntity`);
    }

    async createVehicle(data) {
        return await this.apiCall(`${this.config.masterData}/Vehicle`, 'POST', data);
    }

    async getVehicles() {
        return await this.apiCall(`${this.config.masterData}/Vehicle`);
    }

    async createDriver(data) {
        return await this.apiCall(`${this.config.masterData}/Driver`, 'POST', data);
    }

    async getDrivers() {
        return await this.apiCall(`${this.config.masterData}/Driver`);
    }

    // DataManager API Methods
    async createCustomerOrder(data) {
        return await this.apiCall(`${this.config.dataManager}/Orders/customer-orders`, 'POST', data);
    }

    async getCustomerOrders() {
        return await this.apiCall(`${this.config.dataManager}/Orders/customer-orders`);
    }

    async createPurchaseOrder(data) {
        return await this.apiCall(`${this.config.dataManager}/Orders/purchase-orders`, 'POST', data);
    }

    async getPurchaseOrders() {
        return await this.apiCall(`${this.config.dataManager}/Orders/purchase-orders`);
    }

    async createWeighingTransaction(data) {
        return await this.apiCall(`${this.config.dataManager}/Transactions`, 'POST', data);
    }

    async getWeighingTransactions() {
        return await this.apiCall(`${this.config.dataManager}/Transactions`);
    }

    async createQualityTestResult(data) {
        return await this.apiCall(`${this.config.dataManager}/Quality/test-results`, 'POST', data);
    }

    async getQualityTestResults() {
        return await this.apiCall(`${this.config.dataManager}/Quality/test-results`);
    }

    // Helper method to handle bulk operations
    async bulkCreate(endpoint, dataArray) {
        const results = [];
        for (const item of dataArray) {
            const result = await this.apiCall(endpoint, 'POST', item);
            results.push(result);
            if (!result.success) {
                break; // Stop on first error
            }
            // Small delay between requests to avoid overwhelming the server
            await new Promise(resolve => setTimeout(resolve, 200));
        }
        return results;
    }

    // Utility method to format API responses for display
    formatResponse(response) {
        return {
            success: response.success,
            status: response.status,
            timestamp: new Date().toISOString(),
            data: response.data,
            ...(response.error && { error: response.error })
        };
    }
}