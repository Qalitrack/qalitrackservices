const express = require('express');
const DocsService = require('../services/docsService');

const router = express.Router();
const docsService = new DocsService();

// Get aggregated swagger documentation
router.get('/swagger.json', async (req, res) => {
  try {
    const aggregatedDocs = await docsService.getAggregatedDocs();
    res.json(aggregatedDocs);
  } catch (error) {
    console.error('Swagger aggregation error:', error);
    res.status(500).json({
      error: 'Failed to aggregate API documentation',
      message: error.message
    });
  }
});

// Get swagger documentation for a specific service
router.get('/services/:serviceName/swagger.json', async (req, res) => {
  try {
    const { serviceName } = req.params;
    const serviceDocs = await docsService.getServiceDocs(serviceName);
    
    if (!serviceDocs) {
      return res.status(404).json({
        error: 'Service documentation not found',
        message: `No swagger documentation found for service '${serviceName}'`
      });
    }

    res.json(serviceDocs);
  } catch (error) {
    console.error(`Swagger error for ${req.params.serviceName}:`, error);
    res.status(500).json({
      error: 'Failed to fetch service documentation',
      message: error.message
    });
  }
});

// Get list of services with documentation
router.get('/services', async (req, res) => {
  try {
    const services = await docsService.getDocumentedServices();
    res.json({
      timestamp: new Date().toISOString(),
      total: services.length,
      services: services
    });
  } catch (error) {
    console.error('Services list error:', error);
    res.status(500).json({
      error: 'Failed to get documented services',
      message: error.message
    });
  }
});

// Generate service documentation summary
router.get('/summary', async (req, res) => {
  try {
    const summary = await docsService.getDocumentationSummary();
    res.json({
      timestamp: new Date().toISOString(),
      ...summary
    });
  } catch (error) {
    console.error('Documentation summary error:', error);
    res.status(500).json({
      error: 'Failed to generate documentation summary',
      message: error.message
    });
  }
});

module.exports = router;