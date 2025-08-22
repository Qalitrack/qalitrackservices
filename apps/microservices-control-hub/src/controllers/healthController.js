const express = require('express');
const HealthService = require('../services/healthService');

const router = express.Router();
const healthService = new HealthService();

// Get health status of all services
router.get('/', async (req, res) => {
  try {
    const healthData = await healthService.checkAllServices();
    res.json({
      timestamp: new Date().toISOString(),
      overall: healthService.getOverallStatus(healthData),
      services: healthData
    });
  } catch (error) {
    console.error('Health check error:', error);
    res.status(500).json({
      error: 'Failed to check service health',
      message: error.message
    });
  }
});

// Get health status of a specific service
router.get('/:serviceName', async (req, res) => {
  try {
    const { serviceName } = req.params;
    const healthData = await healthService.checkService(serviceName);
    
    if (!healthData) {
      return res.status(404).json({
        error: 'Service not found',
        message: `Service '${serviceName}' is not configured for monitoring`
      });
    }

    res.json({
      timestamp: new Date().toISOString(),
      service: serviceName,
      ...healthData
    });
  } catch (error) {
    console.error(`Health check error for ${req.params.serviceName}:`, error);
    res.status(500).json({
      error: 'Failed to check service health',
      message: error.message
    });
  }
});

// Get health summary statistics
router.get('/stats/summary', async (req, res) => {
  try {
    const healthData = await healthService.checkAllServices();
    const stats = healthService.getHealthStats(healthData);
    
    res.json({
      timestamp: new Date().toISOString(),
      ...stats
    });
  } catch (error) {
    console.error('Health stats error:', error);
    res.status(500).json({
      error: 'Failed to get health statistics',
      message: error.message
    });
  }
});

module.exports = router;