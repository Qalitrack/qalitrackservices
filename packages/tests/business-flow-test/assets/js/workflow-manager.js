/**
 * QaliTrack Workflow Manager
 * Manages the business flow testing workflow and UI interactions
 */

class WorkflowManager {
    constructor() {
        this.apiClient = new QaliTrackAPIClient();
        this.totalSteps = 10;
        this.completedSteps = 0;
        this.stepStatus = {};
        this.sampleDataCache = {};
        
        this.initializeUI();
        this.loadConfiguration();
    }

    initializeUI() {
        // Initialize progress bar
        this.updateProgress();
        
        // Test initial connections
        setTimeout(() => {
            this.testConnection('masterData');
            this.testConnection('dataManager');
        }, 1000);
    }

    loadConfiguration() {
        // Load saved configuration from localStorage
        const savedConfig = localStorage.getItem('qalitrack-api-config');
        if (savedConfig) {
            try {
                const config = JSON.parse(savedConfig);
                this.apiClient.config = { ...this.apiClient.config, ...config };
                this.updateConfigUI();
            } catch (e) {
                console.warn('Failed to load saved configuration');
            }
        }
    }

    saveConfiguration() {
        localStorage.setItem('qalitrack-api-config', JSON.stringify(this.apiClient.config));
    }

    updateConfigUI() {
        document.getElementById('masterDataUrl').textContent = this.apiClient.config.masterData.replace('/api', '');
        document.getElementById('dataManagerUrl').textContent = this.apiClient.config.dataManager.replace('/api', '');
    }

    updateProgress() {
        const percentage = (this.completedSteps / this.totalSteps) * 100;
        const progressBar = document.getElementById('progressBar');
        if (progressBar) {
            progressBar.style.width = percentage + '%';
        }
    }

    setStepStatus(stepId, status) {
        const statusElement = document.getElementById(`status-${stepId}`);
        if (statusElement) {
            statusElement.className = `status-indicator status-${status}`;
        }
        
        // Update completed steps counter
        if (status === 'success' && this.stepStatus[stepId] !== 'success') {
            this.completedSteps++;
            this.updateProgress();
        } else if (status !== 'success' && this.stepStatus[stepId] === 'success') {
            this.completedSteps--;
            this.updateProgress();
        }
        
        this.stepStatus[stepId] = status;
    }

    showResponse(elementId, data, success = true) {
        const responseArea = document.getElementById(`response-${elementId}`);
        if (!responseArea) return;

        responseArea.style.display = 'block';
        
        const timestamp = new Date().toLocaleTimeString();
        const formattedData = typeof data === 'string' ? data : JSON.stringify(data, null, 2);
        
        responseArea.innerHTML = success ? 
            `✅ Success (${timestamp}):\n${formattedData}` :
            `❌ Error (${timestamp}):\n${formattedData}`;
        
        // Auto-scroll to bottom
        responseArea.scrollTop = responseArea.scrollHeight;
    }

    async testConnection(apiType) {
        const statusElement = document.getElementById(`${apiType}Status`);
        if (statusElement) {
            statusElement.className = 'status-indicator status-pending loading';
        }

        const result = await this.apiClient.testConnection(apiType);
        
        if (statusElement) {
            statusElement.className = `status-indicator ${result.success ? 'status-success' : 'status-error'}`;
        }

        return result;
    }

    // Generic method to handle step execution with error handling
    async executeStep(stepId, operation, successMessage = 'Operation completed successfully') {
        try {
            this.setStepStatus(stepId, 'pending');
            
            const result = await operation();
            
            if (result.success || (Array.isArray(result) && result.every(r => r.success))) {
                this.setStepStatus(stepId, 'success');
                this.showResponse(stepId, result.data || result, true);
            } else {
                this.setStepStatus(stepId, 'error');
                this.showResponse(stepId, result.data || result.error || 'Unknown error', false);
            }
            
            return result;
        } catch (error) {
            this.setStepStatus(stepId, 'error');
            this.showResponse(stepId, error.message, false);
            return { success: false, error: error.message };
        }
    }

    // Configuration Management
    updateApiEndpoint(apiType, newUrl) {
        // Ensure URL ends with /api for consistency
        const baseUrl = newUrl.replace(/\/api$/, '');
        this.apiClient.config[apiType] = `${baseUrl}/api`;
        this.updateConfigUI();
        this.saveConfiguration();
        
        // Re-test connection
        this.testConnection(apiType);
    }

    showConfigModal() {
        const modal = document.createElement('div');
        modal.className = 'config-modal';
        modal.innerHTML = `
            <div class="config-modal-content">
                <div class="config-modal-header">
                    <h3>🔧 API Configuration</h3>
                    <button class="close-modal" onclick="this.closest('.config-modal').remove()">×</button>
                </div>
                <div class="config-modal-body">
                    <div class="config-field">
                        <label>MasterData API Base URL:</label>
                        <input type="url" id="masterDataConfig" value="${this.apiClient.config.masterData.replace('/api', '')}" 
                               placeholder="http://localhost:5004">
                    </div>
                    <div class="config-field">
                        <label>DataManager API Base URL:</label>
                        <input type="url" id="dataManagerConfig" value="${this.apiClient.config.dataManager.replace('/api', '')}" 
                               placeholder="http://localhost:5005">
                    </div>
                </div>
                <div class="config-modal-footer">
                    <button class="btn btn-primary" onclick="workflowManager.saveApiConfig()">Save Configuration</button>
                    <button class="btn" onclick="this.closest('.config-modal').remove()">Cancel</button>
                </div>
            </div>
        `;
        
        // Add modal styles
        const style = document.createElement('style');
        style.textContent = `
            .config-modal {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                display: flex;
                justify-content: center;
                align-items: center;
                z-index: 1000;
            }
            .config-modal-content {
                background: white;
                border-radius: 12px;
                width: 500px;
                max-width: 90vw;
                box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
            }
            .config-modal-header {
                padding: 20px;
                border-bottom: 1px solid #eee;
                display: flex;
                justify-content: space-between;
                align-items: center;
            }
            .config-modal-header h3 {
                margin: 0;
                color: #2c3e50;
            }
            .close-modal {
                background: none;
                border: none;
                font-size: 24px;
                cursor: pointer;
                color: #666;
                padding: 0;
                width: 30px;
                height: 30px;
            }
            .config-modal-body {
                padding: 20px;
            }
            .config-field {
                margin-bottom: 15px;
            }
            .config-field label {
                display: block;
                margin-bottom: 5px;
                font-weight: 500;
                color: #2c3e50;
            }
            .config-field input {
                width: 100%;
                padding: 10px;
                border: 1px solid #ddd;
                border-radius: 6px;
                font-size: 14px;
            }
            .config-modal-footer {
                padding: 20px;
                border-top: 1px solid #eee;
                display: flex;
                gap: 10px;
                justify-content: flex-end;
            }
        `;
        
        document.head.appendChild(style);
        document.body.appendChild(modal);
    }

    saveApiConfig() {
        const masterDataUrl = document.getElementById('masterDataConfig').value;
        const dataManagerUrl = document.getElementById('dataManagerConfig').value;
        
        this.updateApiEndpoint('masterData', masterDataUrl);
        this.updateApiEndpoint('dataManager', dataManagerUrl);
        
        document.querySelector('.config-modal').remove();
        
        // Show success message
        this.showNotification('Configuration saved successfully!', 'success');
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        
        const style = document.createElement('style');
        style.textContent = `
            .notification {
                position: fixed;
                top: 20px;
                right: 20px;
                padding: 12px 20px;
                border-radius: 8px;
                color: white;
                font-weight: 500;
                z-index: 1001;
                opacity: 0;
                transform: translateY(-10px);
                transition: all 0.3s ease;
            }
            .notification-success { background: #27ae60; }
            .notification-error { background: #e74c3c; }
            .notification-info { background: #3498db; }
            .notification.show {
                opacity: 1;
                transform: translateY(0);
            }
        `;
        
        if (!document.head.querySelector('.notification-styles')) {
            style.className = 'notification-styles';
            document.head.appendChild(style);
        }
        
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => notification.classList.add('show'), 100);
        
        // Remove after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Utility method for handling dependencies
    async checkDependencies(dependencies) {
        const results = {};
        for (const [key, getter] of Object.entries(dependencies)) {
            const result = await getter();
            results[key] = result.success ? (result.data?.data || result.data) : null;
        }
        return results;
    }
}

// Global workflow manager instance
let workflowManager;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    workflowManager = new WorkflowManager();
});