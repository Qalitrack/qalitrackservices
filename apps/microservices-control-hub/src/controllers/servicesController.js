const express = require('express');
const ConfigService = require('../services/configService');
const HealthService = require('../services/healthService');
const DocsService = require('../services/docsService');

const router = express.Router();
const configService = new ConfigService();
const healthService = new HealthService();
const docsService = new DocsService();

// Get all configured services with their status
router.get('/', async (req, res) => {
  try {
    const services = configService.getAllServices();
    const healthData = await healthService.checkAllServices();
    const documentedServices = await docsService.getDocumentedServices();
    
    const enrichedServices = services.map(service => {
      const health = healthData[service.name] || { status: 'unknown' };
      const hasDocumentation = documentedServices.some(doc => doc.name === service.name);
      
      return {
        ...service,
        health: health.status,
        responseTime: health.responseTime,
        lastChecked: health.timestamp,
        hasDocumentation,
        endpoints: {
          health: `http://${service.host}:${service.port}${service.healthPath || '/health'}`,
          swagger: service.swaggerPath ? `http://${service.host}:${service.port}${service.swaggerPath}` : null
        }
      };
    });

    res.json({
      timestamp: new Date().toISOString(),
      total: enrichedServices.length,
      healthy: enrichedServices.filter(s => s.health === 'healthy').length,
      services: enrichedServices
    });
  } catch (error) {
    console.error('Services listing error:', error);
    res.status(500).json({
      error: 'Failed to get services information',
      message: error.message
    });
  }
});

// Get service discovery configuration
router.get('/config', (req, res) => {
  try {
    const config = configService.getConfig();
    res.json({
      timestamp: new Date().toISOString(),
      environment: config.environment || 'development',
      totalServices: config.services?.length || 0,
      config: config
    });
  } catch (error) {
    console.error('Config retrieval error:', error);
    res.status(500).json({
      error: 'Failed to get configuration',
      message: error.message
    });
  }
});

// Get service groups (infrastructure, application, etc.)
router.get('/groups', async (req, res) => {
  try {
    const services = configService.getAllServices();
    const healthData = await healthService.checkAllServices();
    
    const groups = services.reduce((acc, service) => {
      const group = service.group || 'application';
      if (!acc[group]) {
        acc[group] = {
          name: group,
          services: [],
          healthy: 0,
          total: 0
        };
      }
      
      const health = healthData[service.name] || { status: 'unknown' };
      acc[group].services.push({
        name: service.name,
        status: health.status,
        responseTime: health.responseTime
      });
      acc[group].total++;
      if (health.status === 'healthy') {
        acc[group].healthy++;
      }
      
      return acc;
    }, {});

    res.json({
      timestamp: new Date().toISOString(),
      groups: Object.values(groups)
    });
  } catch (error) {
    console.error('Service groups error:', error);
    res.status(500).json({
      error: 'Failed to get service groups',
      message: error.message
    });
  }
});

// Refresh service discovery (reload config)
router.post('/refresh', (req, res) => {
  try {
    configService.reloadConfig();
    res.json({
      message: 'Service configuration refreshed successfully',
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    console.error('Config refresh error:', error);
    res.status(500).json({
      error: 'Failed to refresh configuration',
      message: error.message
    });
  }
});

module.exports = router;