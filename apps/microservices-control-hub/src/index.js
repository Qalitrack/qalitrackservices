const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const path = require('path');

const healthController = require('./controllers/healthController');
const docsController = require('./controllers/docsController');
const servicesController = require('./controllers/servicesController');

const app = express();
const port = process.env.PORT || 3000;
const projectName = process.env.PROJECT_NAME || 'Microservices';

// Security and middleware
const isProduction = process.env.NODE_ENV === 'production';

if (isProduction) {
  // Use strict CSP in production
  app.use(helmet({
    contentSecurityPolicy: {
      directives: {
        defaultSrc: ["'self'"],
        styleSrc: ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
        scriptSrc: ["'self'", "'unsafe-inline'"],
        fontSrc: ["'self'", "https://fonts.gstatic.com"],
        imgSrc: ["'self'", "data:", "https:"],
        upgradeInsecureRequests: [],
      },
    },
  }));
} else {
  // Disable CSP in development to avoid HTTP/HTTPS issues
  app.use(helmet({
    contentSecurityPolicy: false,
  }));
}

app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

// Rate limiting
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limit each IP to 100 requests per windowMs
  message: 'Too many requests from this IP, please try again later.'
});
app.use(limiter);

// Routes
app.use('/api/health', healthController);
app.use('/api/docs', docsController);
app.use('/api/services', servicesController);

// Project configuration endpoint
app.get('/api/config', (req, res) => {
  const ConfigService = require('./services/configService');
  const configService = new ConfigService();
  
  res.json({
    projectName: projectName,
    applicationName: configService.getApplicationName(),
    version: require('../package.json').version,
    environment: process.env.NODE_ENV || 'development',
    servers: configService.getServers()
  });
});

// Main dashboard
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'dashboard.html'));
});

// Swagger UI
app.get('/docs', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'swagger.html'));
});

// Health check for the control hub itself
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    version: require('../package.json').version,
    uptime: process.uptime()
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: 'The requested resource was not found',
    path: req.path
  });
});

// Error handler
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).json({
    error: 'Internal Server Error',
    message: process.env.NODE_ENV === 'development' ? err.message : 'Something went wrong'
  });
});

app.listen(port, () => {
  console.log(`🚀 ${projectName} Control Hub running at http://localhost:${port}`);
  console.log(`📊 Dashboard: http://localhost:${port}/`);
  console.log(`📋 API Docs: http://localhost:${port}/docs`);
  console.log(`💚 Health: http://localhost:${port}/health`);
});

module.exports = app;