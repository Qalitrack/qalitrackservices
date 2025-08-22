const fetch = require('node-fetch');
const net = require('net');
const ConfigService = require('./configService');

class HealthService {
  constructor() {
    this.configService = new ConfigService();
    this.timeout = 5000; // 5 second timeout
  }

  async checkAllServices() {
    const services = this.configService.getEnabledServices();
    const healthPromises = services.map(service => this.checkService(service.name));
    const results = await Promise.allSettled(healthPromises);
    
    const healthData = {};
    services.forEach((service, index) => {
      const result = results[index];
      if (result.status === 'fulfilled' && result.value) {
        healthData[service.name] = result.value;
      } else {
        healthData[service.name] = {
          status: 'unknown',
          error: result.reason?.message || 'Health check failed',
          timestamp: new Date().toISOString(),
          responseTime: null
        };
      }
    });

    return healthData;
  }

  async checkService(serviceName) {
    const service = this.configService.getService(serviceName);
    if (!service) {
      return null;
    }

    const startTime = Date.now();

    try {
      let status = 'unknown';
      
      // If service has a health endpoint, use HTTP check
      if (service.healthPath) {
        status = await this.httpHealthCheck(service);
      } else {
        // For infrastructure services, use TCP check
        status = await this.tcpHealthCheck(service);
      }

      const responseTime = Date.now() - startTime;

      const baseUrl = service.url ? service.url : `http://${service.host}:${service.port}`;
      
      return {
        status,
        timestamp: new Date().toISOString(),
        responseTime,
        endpoint: service.healthPath ? 
          `${baseUrl}${service.healthPath}` : 
          baseUrl,
        service: {
          name: service.name,
          group: service.group,
          description: service.description
        }
      };
    } catch (error) {
      const responseTime = Date.now() - startTime;
      return {
        status: 'unhealthy',
        error: error.message,
        timestamp: new Date().toISOString(),
        responseTime,
        service: {
          name: service.name,
          group: service.group,
          description: service.description
        }
      };
    }
  }

  async httpHealthCheck(service) {
    const baseUrl = this.configService.getServiceUrl(service);
    const url = `${baseUrl}${service.healthPath}`;
    
    // Validate URL before making request
    try {
      new URL(url);
    } catch (urlError) {
      throw new Error(`Invalid URL constructed: "${url}" - ${urlError.message}`);
    }
    
    try {
      const response = await fetch(url, {
        method: 'GET',
        timeout: this.timeout,
        headers: {
          'User-Agent': 'Microservices-Control-Hub/1.0'
        }
      });

      if (response.ok) {
        // Try to parse response for more detailed health info
        try {
          const healthData = await response.json();
          if (healthData.status) {
            return healthData.status.toLowerCase() === 'healthy' ? 'healthy' : 'degraded';
          }
        } catch (parseError) {
          // If can't parse JSON, but response is OK, consider healthy
        }
        return 'healthy';
      } else {
        return 'unhealthy';
      }
    } catch (error) {
      if (error.code === 'ECONNREFUSED' || error.code === 'ENOTFOUND') {
        return 'down';
      }
      throw error;
    }
  }

  async tcpHealthCheck(service) {
    return new Promise((resolve, reject) => {
      // For external URLs, extract host and port
      let host = service.host;
      let port = service.port;
      
      if (service.url) {
        try {
          const urlObj = new URL(service.url);
          host = urlObj.hostname;
          port = urlObj.port || (urlObj.protocol === 'https:' ? 443 : 80);
        } catch (error) {
          resolve('down');
          return;
        }
      }

      const socket = new net.Socket();
      const timeout = setTimeout(() => {
        socket.destroy();
        resolve('down');
      }, this.timeout);

      socket.connect(port, host, () => {
        clearTimeout(timeout);
        socket.end();
        resolve('healthy');
      });

      socket.on('error', (err) => {
        clearTimeout(timeout);
        socket.end();
        if (err.code === 'ECONNREFUSED' || err.code === 'ENOTFOUND') {
          resolve('down');
        } else {
          reject(err);
        }
      });
    });
  }

  getOverallStatus(healthData) {
    const statuses = Object.values(healthData).map(h => h.status);
    
    if (statuses.every(s => s === 'healthy')) {
      return 'healthy';
    } else if (statuses.some(s => s === 'healthy')) {
      return 'degraded';
    } else {
      return 'unhealthy';
    }
  }

  getHealthStats(healthData) {
    const services = Object.values(healthData);
    const total = services.length;
    const healthy = services.filter(s => s.status === 'healthy').length;
    const unhealthy = services.filter(s => s.status === 'unhealthy').length;
    const down = services.filter(s => s.status === 'down').length;
    const unknown = services.filter(s => s.status === 'unknown').length;
    const degraded = services.filter(s => s.status === 'degraded').length;

    const avgResponseTime = services
      .filter(s => s.responseTime !== null)
      .reduce((sum, s) => sum + s.responseTime, 0) / 
      services.filter(s => s.responseTime !== null).length || 0;

    return {
      total,
      healthy,
      unhealthy,
      down,
      unknown,
      degraded,
      healthPercentage: total > 0 ? Math.round((healthy / total) * 100) : 0,
      averageResponseTime: Math.round(avgResponseTime),
      overall: this.getOverallStatus(healthData)
    };
  }
}

module.exports = HealthService;