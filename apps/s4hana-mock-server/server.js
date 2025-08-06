const express = require('express');
const path = require('path');
const app = express();
const PORT = process.env.PORT || 5001;

// Health endpoint
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        timestamp: new Date().toISOString(),
        version: '1.0.0',
        service: 's4hana-mock-server',
        client: 'bamburi',
        apis: {
            salesOrder: 'active',
            businessPartner: 'active', 
            material: 'active',
            plant: 'active'
        },
        masterData: {
            plants: 3,
            customers: 3,
            vendors: 2,
            products: 5,
            sampleOrders: 3
        }
    });
});

// Serve static files
app.use(express.static('webapp'));

// Default route
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'webapp', 'index.html'));
});

app.listen(PORT, '0.0.0.0', () => {
    console.log(`🚀 S/4HANA Mock Server running on port ${PORT}`);
    console.log(`📊 Health check: http://localhost:${PORT}/health`);
    console.log(`🌐 Web interface: http://localhost:${PORT}/`);
    console.log(`🌍 External access: http://[your-ip]:${PORT}/`);
});

module.exports = app;