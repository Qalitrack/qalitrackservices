const fs = require('fs');
const path = require('path');

class ConfigService {
  constructor() {
    this.configPath = process.env.CONFIG_PATH || './config/services.json';
    this.config = null;
    this.loadConfig();
  }

  loadConfig() {
    try {
      if (fs.existsSync(this.configPath)) {
        const configData = fs.readFileSync(this.configPath, 'utf8');
        this.config = JSON.parse(configData);
        console.log(`✅ Loaded configuration from ${this.configPath}`);
        console.log(`📊 Found ${this.config.services?.length || 0} services`);
      } else {
        console.warn(`⚠️  Configuration file not found at ${this.configPath}`);
        this.config = this.getDefaultConfig();
      }
    } catch (error) {
      console.error(`❌ Failed to load configuration: ${error.message}`);
      this.config = this.getDefaultConfig();
    }
  }

  reloadConfig() {
    this.loadConfig();
  }

  getConfig() {
    return this.config;
  }

  getAllServices() {
    return this.config.services || [];
  }

  getService(serviceName) {
    return this.config.services?.find(service => service.name === serviceName);
  }

  getServicesByGroup(group) {
    return this.config.services?.filter(service => service.group === group) || [];
  }

  getEnabledServices() {
    return this.config.services?.filter(service => service.enabled !== false) || [];
  }

  getServicesWithDocumentation() {
    return this.config.services?.filter(service => 
      service.enabled !== false && service.swaggerPath
    ) || [];
  }

  getDefaultConfig() {
    return {
      environment: process.env.NODE_ENV || 'development',
      version: '1.0.0',
      external: {
        host: process.env.EXTERNAL_HOST || 'localhost',
        protocol: process.env.EXTERNAL_PROTOCOL || 'http',
        gatewayPort: process.env.GATEWAY_PORT || '7000'
      },
      services: [
        {
          name: 'user-service',
          host: 'user-service',
          port: 80,
          group: 'application',
          enabled: true,
          healthPath: '/health',
          swaggerPath: '/swagger/v1/swagger.json',
          apiRoot: '/api/users',
          description: 'User management and authentication service'
        },
        {
          name: 'api-gateway',
          host: 'api-gateway',
          port: 80,
          group: 'gateway',
          enabled: true,
          healthPath: '/health',
          swaggerPath: '/swagger/v1/swagger.json',
          apiRoot: '',
          description: 'API Gateway routing and aggregation'
        },
        {
          name: 'postgres',
          host: 'postgres',
          port: 5432,
          group: 'infrastructure',
          enabled: true,
          healthPath: null,
          swaggerPath: null,
          description: 'PostgreSQL database'
        },
        {
          name: 'redis',
          host: 'redis',
          port: 6379,
          group: 'infrastructure',
          enabled: true,
          healthPath: null,
          swaggerPath: null,
          description: 'Redis cache and session store'
        }
      ]
    };
  }

  getExternalServerUrl() {
    const external = this.config.external || {};
    const protocol = external.protocol || 'http';
    const host = external.host || 'localhost';
    const port = external.gatewayPort || '7000';
    
    return `${protocol}://${host}:${port}`;
  }

  getServers() {
    // Return configured servers or fallback to external config
    if (this.config.servers && Array.isArray(this.config.servers)) {
      return this.config.servers;
    }
    
    // Fallback to legacy external config
    return [{
      url: this.getExternalServerUrl(),
      description: `${this.config.environment || 'development'} Environment`
    }];
  }

  getApplicationName() {
    return this.config.applicationName || 'Microservices';
  }

  // Helper to build service URL (supports both host:port and full URL)
  getServiceUrl(service) {
    if (service.url) {
      // External URL format: https://service.com or http://service.com:8080
      return service.url;
    } else if (service.host) {
      // Legacy host:port format: service-name:80
      const protocol = service.protocol || 'http';
      return `${protocol}://${service.host}:${service.port}`;
    }
    throw new Error(`Service ${service.name} must have either 'url' or 'host' + 'port'`);
  }
}

module.exports = ConfigService;