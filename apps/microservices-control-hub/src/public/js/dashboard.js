class MicroservicesDashboard {
    constructor() {
        this.refreshInterval = 30000; // 30 seconds
        this.refreshTimer = null;
        this.init();
    }

    async init() {
        await this.loadConfig();
        this.bindEvents();
        this.loadData();
        this.startAutoRefresh();
    }

    async loadConfig() {
        try {
            const response = await fetch('/api/config');
            const config = await response.json();
            
            // Update page title and main header
            const projectName = config.projectName || 'Microservices';
            const applicationName = config.applicationName || projectName;
            document.getElementById('pageTitle').textContent = `${applicationName} Control Hub`;
            document.getElementById('mainTitle').textContent = `🎛️ ${applicationName} Control Hub`;
            
            this.config = config;
        } catch (error) {
            console.warn('Failed to load config, using defaults:', error.message);
            this.config = { projectName: 'Microservices' };
        }
    }

    bindEvents() {
        // Refresh button
        document.getElementById('refreshBtn').addEventListener('click', () => {
            this.refresh();
        });

        // Filters
        document.getElementById('groupFilter').addEventListener('change', () => {
            this.applyFilters();
        });

        document.getElementById('statusFilter').addEventListener('change', () => {
            this.applyFilters();
        });

        // Modal close
        document.querySelector('.modal-close').addEventListener('click', () => {
            this.closeModal();
        });

        // Close modal on outside click
        document.getElementById('serviceModal').addEventListener('click', (e) => {
            if (e.target.id === 'serviceModal') {
                this.closeModal();
            }
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeModal();
            } else if (e.key === 'r' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                this.refresh();
            }
        });
    }

    async loadData() {
        try {
            await Promise.all([
                this.loadHealthData(),
                this.loadServicesData(),
                this.loadGroupsData(),
                this.loadDocsData()
            ]);
            this.updateLastUpdated();
        } catch (error) {
            console.error('Failed to load dashboard data:', error);
            this.showError('Failed to load dashboard data');
        }
    }

    async loadHealthData() {
        try {
            const response = await fetch('/api/health/stats/summary');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const data = await response.json();
            this.updateHealthStats(data);
        } catch (error) {
            console.error('Failed to load health data:', error);
        }
    }

    async loadServicesData() {
        try {
            const response = await fetch('/api/services');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const data = await response.json();
            this.servicesData = data.services;
            this.updateServicesDisplay();
            this.updateFilters();
        } catch (error) {
            console.error('Failed to load services data:', error);
            this.showServicesError();
        }
    }

    async loadGroupsData() {
        try {
            const response = await fetch('/api/services/groups');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const data = await response.json();
            this.updateGroupsDisplay(data.groups);
        } catch (error) {
            console.error('Failed to load groups data:', error);
        }
    }

    async loadDocsData() {
        try {
            const response = await fetch('/api/docs/summary');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const data = await response.json();
            this.updateDocsStats(data);
        } catch (error) {
            console.error('Failed to load documentation data:', error);
        }
    }

    updateHealthStats(data) {
        const overallStatus = document.getElementById('overallStatus');
        const overallStatusValue = document.getElementById('overallStatusValue');
        const healthyCount = document.getElementById('healthyCount');
        const healthyPercentage = document.getElementById('healthyPercentage');
        const totalServices = document.getElementById('totalServices');
        const avgResponseTime = document.getElementById('avgResponseTime');

        // Update overall status
        overallStatus.className = `stat-card overall-status ${data.overall}`;
        overallStatusValue.textContent = this.capitalizeFirst(data.overall);

        // Update healthy services
        healthyCount.textContent = `${data.healthy}/${data.total}`;
        healthyPercentage.textContent = `${data.healthPercentage}% healthy`;

        // Update total services
        totalServices.textContent = data.total;
        avgResponseTime.textContent = `${data.averageResponseTime}ms avg`;

        // Update status icon
        const statusIcon = overallStatus.querySelector('.stat-icon');
        statusIcon.textContent = this.getStatusIcon(data.overall);
    }

    updateDocsStats(data) {
        const totalEndpoints = document.getElementById('totalEndpoints');
        const docsCoverage = document.getElementById('docsCoverage');

        totalEndpoints.textContent = data.total.paths;
        docsCoverage.textContent = `${data.total.coverage}% documented`;
    }

    updateServicesDisplay() {
        const container = document.getElementById('servicesContainer');
        
        if (!this.servicesData || this.servicesData.length === 0) {
            container.innerHTML = '<div class="loading">No services found</div>';
            return;
        }

        const filteredServices = this.getFilteredServices();
        
        if (filteredServices.length === 0) {
            container.innerHTML = '<div class="loading">No services match the current filters</div>';
            return;
        }

        container.innerHTML = filteredServices.map(service => this.createServiceCard(service)).join('');

        // Add click handlers for service cards
        container.querySelectorAll('.service-card').forEach((card, index) => {
            card.addEventListener('click', () => {
                this.showServiceDetails(filteredServices[index]);
            });
        });
    }

    updateGroupsDisplay(groups) {
        const container = document.getElementById('groupsContainer');
        
        if (!groups || groups.length === 0) {
            container.innerHTML = '<div class="loading">No service groups found</div>';
            return;
        }

        container.innerHTML = groups.map(group => this.createGroupCard(group)).join('');
    }

    createServiceCard(service) {
        const responseTime = service.responseTime ? `${service.responseTime}ms` : 'N/A';
        const lastChecked = service.lastChecked ? 
            new Date(service.lastChecked).toLocaleTimeString() : 'Never';

        return `
            <div class="service-card" data-group="${service.group}" data-status="${service.health}">
                <div class="service-status ${service.health}"></div>
                <div class="service-info">
                    <h4>${service.name}</h4>
                    <div class="service-description">${service.description || 'No description available'}</div>
                </div>
                <div class="service-group">${service.group}</div>
                <div class="service-metrics">
                    <div class="response-time">${responseTime}</div>
                    <div class="last-checked">${lastChecked}</div>
                </div>
            </div>
        `;
    }

    createGroupCard(group) {
        const healthPercentage = group.total > 0 ? Math.round((group.healthy / group.total) * 100) : 0;
        
        return `
            <div class="group-card">
                <div class="group-header">
                    <div class="group-name">${group.name}</div>
                    <div class="group-stats">${group.healthy}/${group.total} healthy (${healthPercentage}%)</div>
                </div>
                <div class="group-services">
                    ${group.services.map(service => 
                        `<span class="service-pill ${service.status}">${service.name}</span>`
                    ).join('')}
                </div>
            </div>
        `;
    }

    getFilteredServices() {
        if (!this.servicesData) return [];

        const groupFilter = document.getElementById('groupFilter').value;
        const statusFilter = document.getElementById('statusFilter').value;

        return this.servicesData.filter(service => {
            const matchesGroup = !groupFilter || service.group === groupFilter;
            const matchesStatus = !statusFilter || service.health === statusFilter;
            return matchesGroup && matchesStatus;
        });
    }

    updateFilters() {
        if (!this.servicesData) return;

        // Update group filter
        const groupFilter = document.getElementById('groupFilter');
        const groups = [...new Set(this.servicesData.map(s => s.group))].sort();
        
        const currentGroupValue = groupFilter.value;
        groupFilter.innerHTML = '<option value="">All Groups</option>' +
            groups.map(group => `<option value="${group}">${this.capitalizeFirst(group)}</option>`).join('');
        groupFilter.value = currentGroupValue;
    }

    applyFilters() {
        this.updateServicesDisplay();
    }

    showServiceDetails(service) {
        const modal = document.getElementById('serviceModal');
        const modalServiceName = document.getElementById('modalServiceName');
        const modalBody = document.getElementById('modalBody');

        modalServiceName.textContent = service.name;
        modalBody.innerHTML = this.createServiceDetailsHTML(service);
        modal.style.display = 'block';
    }

    createServiceDetailsHTML(service) {
        return `
            <div style="display: grid; gap: 1rem;">
                <div>
                    <h4>Status Information</h4>
                    <p><strong>Current Status:</strong> <span class="service-pill ${service.health}">${this.capitalizeFirst(service.health)}</span></p>
                    <p><strong>Group:</strong> ${this.capitalizeFirst(service.group)}</p>
                    <p><strong>Response Time:</strong> ${service.responseTime || 'N/A'}ms</p>
                    <p><strong>Last Checked:</strong> ${service.lastChecked ? new Date(service.lastChecked).toLocaleString() : 'Never'}</p>
                </div>
                
                <div>
                    <h4>Service Details</h4>
                    <p><strong>Description:</strong> ${service.description || 'No description available'}</p>
                    <p><strong>API Root:</strong> ${service.apiRoot || 'N/A'}</p>
                    <p><strong>Documentation:</strong> ${service.hasDocumentation ? 'Available' : 'Not available'}</p>
                </div>
                
                <div>
                    <h4>Endpoints</h4>
                    <p><strong>Health Check:</strong> <a href="${service.endpoints?.health}" target="_blank">${service.endpoints?.health}</a></p>
                    ${service.endpoints?.swagger ? 
                        `<p><strong>Swagger:</strong> <a href="${service.endpoints.swagger}" target="_blank">${service.endpoints.swagger}</a></p>` : 
                        '<p><strong>Swagger:</strong> Not available</p>'
                    }
                </div>
            </div>
        `;
    }

    closeModal() {
        document.getElementById('serviceModal').style.display = 'none';
    }

    async refresh() {
        const refreshBtn = document.getElementById('refreshBtn');
        refreshBtn.classList.add('refreshing');
        
        try {
            await this.loadData();
        } finally {
            refreshBtn.classList.remove('refreshing');
        }
    }

    startAutoRefresh() {
        this.refreshTimer = setInterval(() => {
            this.loadData();
        }, this.refreshInterval);
    }

    stopAutoRefresh() {
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }
    }

    updateLastUpdated() {
        document.getElementById('lastUpdated').textContent = new Date().toLocaleTimeString();
    }

    showError(message) {
        console.error(message);
        // Could implement toast notifications here
    }

    showServicesError() {
        document.getElementById('servicesContainer').innerHTML = `
            <div class="error">
                <h3>Failed to Load Services</h3>
                <p>Unable to fetch services data. Please check that the control hub is properly configured.</p>
                <button onclick="window.dashboard.refresh()" style="margin-top: 1rem; padding: 0.5rem 1rem;">Retry</button>
            </div>
        `;
    }

    getStatusIcon(status) {
        const icons = {
            healthy: '💚',
            degraded: '🟡',
            unhealthy: '❌',
            down: '🔴',
            unknown: '❓'
        };
        return icons[status] || '❓';
    }

    capitalizeFirst(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
}

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.dashboard = new MicroservicesDashboard();
});

// Handle page visibility changes to pause/resume auto-refresh
document.addEventListener('visibilitychange', () => {
    if (window.dashboard) {
        if (document.hidden) {
            window.dashboard.stopAutoRefresh();
        } else {
            window.dashboard.startAutoRefresh();
        }
    }
});