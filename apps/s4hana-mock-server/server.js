const express = require('express');
const path = require('path');
const swaggerUi = require('swagger-ui-express');
const redoc = require('redoc-express');
const swaggerDocument = require('./swagger.json');
const apiRoutes = require('./routes/api');
const { router: authRoutes } = require('./routes/auth');
const app = express();
const PORT = process.env.PORT || 5001;

// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// API Documentation - Swagger UI
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument, {
    customCss: '.swagger-ui .topbar { display: none }',
    customSiteTitle: 'S/4HANA Public Edition Integration API',
    customfavIcon: '/favicon.ico',
    swaggerOptions: {
        persistAuthorization: true,
        displayRequestDuration: true
    }
}));

// API Documentation - ReDoc
app.get('/redoc', redoc({
    title: 'S/4HANA Public Edition Integration API - ReDoc',
    specUrl: '/swagger.json'
}));

// Serve the swagger.json file directly
app.get('/swagger.json', (req, res) => {
    res.json(swaggerDocument);
});

// Authentication Routes (public - no auth required)
app.use('/auth', authRoutes);

// API Routes (protected - authentication required)
app.use('/api', apiRoutes);

// Health endpoint
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        timestamp: new Date().toISOString(),
        version: '1.0.0',
        service: 's4hana-public-edition-integration',
        client: 'development',
        apis: {
            salesOrder: 'active',
            businessPartner: 'active',
            material: 'active',
            plant: 'active',
            weightMeasurement: 'active',
            delivery: 'active',
            plantTransfer: 'active'
        },
        masterData: {
            plants: 3,
            customers: 3,
            vendors: 2,
            products: 5,
            sampleOrders: 3
        },
        endpoints: {
            documentation: '/api-docs',
            redoc: '/redoc',
            swagger: '/swagger.json',
            health: '/health',
            api: '/api',
            authentication: '/auth'
        },
        authentication: {
            communicationUsers: {
                total: 2,
                endpoints: [
                    'POST /auth/communicate/basic - Communication User Basic Authentication',
                    'POST /auth/oauth2/token - Communication User OAuth 2.0',
                    'POST /auth/authenticate/apikey - API Key Authentication (Read-only)'
                ]
            },
            note: 'Business Users cannot access APIs - only Communication Users as per SAP S/4HANA Cloud requirements'
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
    console.log(`🚀 S/4HANA Public Edition Integration running on port ${PORT}`);
    console.log(`📊 Health check: http://localhost:${PORT}/health`);
    console.log(`🌐 Web interface: http://localhost:${PORT}/`);
    console.log(`🌍 External access: http://[your-ip]:${PORT}/`);
});

module.exports = app;