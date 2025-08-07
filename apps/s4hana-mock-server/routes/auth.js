const express = require('express');
const router = express.Router();
const jwt = require('jsonwebtoken');

// Mock Communication Users (as defined in Issue #15)
const COMMUNICATION_USERS = {
    'QALI_API_USER': {
        username: 'QALI_API_USER',
        password: 'QaliTrack2025!',
        description: 'QaliTrack API Integration User',
        communicationSystem: 'QALITRACK_PROD',
        communicationArrangement: 'QALITRACK_SALES_ORDER_API',
        scenario: 'SAP_COM_0109',
        permissions: [
            'salesorder:read',
            'salesorder:create', 
            'salesorder:update',
            'businesspartner:read',
            'material:read',
            'plant:read'
        ],
        organizationalUnits: {
            companyCode: '1710',
            salesOrganization: '1710',
            distributionChannel: '10',
            division: '00'
        },
        authMethods: ['basic', 'oauth2'],
        active: true,
        created: '2025-08-06T00:00:00Z',
        lastLogin: null
    },
    'QALI_READ_USER': {
        username: 'QALI_READ_USER',
        password: 'ReadOnly2025!',
        description: 'QaliTrack Read-Only Integration User',
        communicationSystem: 'QALITRACK_PROD',
        communicationArrangement: 'QALITRACK_READ_API',
        scenario: 'SAP_COM_0109',
        permissions: [
            'salesorder:read',
            'businesspartner:read',
            'material:read',
            'plant:read'
        ],
        organizationalUnits: {
            companyCode: '1710',
            salesOrganization: '1710',
            distributionChannel: '10',
            division: '00'
        },
        authMethods: ['basic', 'oauth2', 'apikey'],
        active: true,
        created: '2025-08-06T00:00:00Z',
        lastLogin: null
    }
};

// JWT Secret for token generation
const JWT_SECRET = 'mock-s4hana-secret-2025-bamburi-cement';
const TOKEN_EXPIRY = '1h';

// Communication Arrangements Configuration
const COMMUNICATION_ARRANGEMENTS = {
    'QALITRACK_SALES_ORDER_API': {
        arrangementName: 'QALITRACK_SALES_ORDER_API',
        scenario: 'SAP_COM_0109',
        scenarioDescription: 'Sales Order Integration',
        communicationSystem: 'QALITRACK_PROD',
        communicationUser: 'QALI_API_USER',
        serviceUrl: '/sap/opu/odata4/sap/api_salesorder/srvd_a2x/sap/salesorder/0001/',
        authMethod: 'Basic Authentication',
        active: true,
        endpoints: [
            { path: '/api/sales-orders', method: 'GET', permission: 'salesorder:read' },
            { path: '/api/sales-orders', method: 'POST', permission: 'salesorder:create' },
            { path: '/api/sales-orders/*', method: 'PUT', permission: 'salesorder:update' },
            { path: '/api/business-partners', method: 'GET', permission: 'businesspartner:read' },
            { path: '/api/materials', method: 'GET', permission: 'material:read' },
            { path: '/api/plants', method: 'GET', permission: 'plant:read' }
        ]
    },
    'QALITRACK_READ_API': {
        arrangementName: 'QALITRACK_READ_API',
        scenario: 'SAP_COM_0109',
        scenarioDescription: 'Sales Order Integration (Read-Only)',
        communicationSystem: 'QALITRACK_PROD', 
        communicationUser: 'QALI_READ_USER',
        serviceUrl: '/sap/opu/odata4/sap/api_salesorder/srvd_a2x/sap/salesorder/0001/',
        authMethod: 'API Key / Basic Authentication',
        active: true,
        endpoints: [
            { path: '/api/sales-orders', method: 'GET', permission: 'salesorder:read' },
            { path: '/api/business-partners', method: 'GET', permission: 'businesspartner:read' },
            { path: '/api/materials', method: 'GET', permission: 'material:read' },
            { path: '/api/plants', method: 'GET', permission: 'plant:read' }
        ]
    }
};

// Basic Authentication endpoint (Communication User login)
router.post('/communicate/basic', (req, res) => {
    const authHeader = req.headers.authorization;
    
    if (!authHeader || !authHeader.startsWith('Basic ')) {
        return res.status(401).json({
            error: 'AUTHENTICATION_REQUIRED',
            message: 'Basic Authentication required for Communication User access',
            details: 'Use Communication User credentials in Authorization header'
        });
    }

    try {
        // Decode Basic Auth credentials
        const base64Credentials = authHeader.split(' ')[1];
        const credentials = Buffer.from(base64Credentials, 'base64').toString('ascii');
        const [username, password] = credentials.split(':');

        // Validate Communication User
        const user = COMMUNICATION_USERS[username];
        if (!user || user.password !== password || !user.active) {
            return res.status(401).json({
                error: 'INVALID_CREDENTIALS',
                message: 'Invalid Communication User credentials or user inactive',
                communicationSystem: 'S/4HANA Cloud',
                hint: 'Only Communication Users can access S/4HANA APIs - Business Users are not permitted'
            });
        }

        // Check if Basic Auth is allowed for this user
        if (!user.authMethods.includes('basic')) {
            return res.status(401).json({
                error: 'AUTH_METHOD_NOT_ALLOWED',
                message: 'Basic Authentication not configured for this Communication User',
                allowedMethods: user.authMethods
            });
        }

        // Update last login
        user.lastLogin = new Date().toISOString();

        // Generate access token
        const tokenPayload = {
            sub: user.username,
            communicationUser: true,
            communicationSystem: user.communicationSystem,
            communicationArrangement: user.communicationArrangement,
            scenario: user.scenario,
            permissions: user.permissions,
            organizationalUnits: user.organizationalUnits,
            iat: Math.floor(Date.now() / 1000),
            exp: Math.floor(Date.now() / 1000) + 3600 // 1 hour
        };

        const accessToken = jwt.sign(tokenPayload, JWT_SECRET);

        res.json({
            access_token: accessToken,
            token_type: 'Bearer',
            expires_in: 3600,
            scope: user.permissions.join(' '),
            communication_user: user.username,
            communication_arrangement: user.communicationArrangement,
            scenario: user.scenario,
            organizational_units: user.organizationalUnits,
            service_url: COMMUNICATION_ARRANGEMENTS[user.communicationArrangement]?.serviceUrl,
            authenticated_at: user.lastLogin
        });

    } catch (error) {
        res.status(400).json({
            error: 'INVALID_REQUEST',
            message: 'Invalid Basic Authentication format',
            details: 'Use format: Authorization: Basic <base64(username:password)>'
        });
    }
});

// OAuth 2.0 Token endpoint (Communication User OAuth)
router.post('/oauth2/token', (req, res) => {
    const { grant_type, username, password, client_id, client_secret } = req.body;

    // Only support password grant for Communication Users
    if (grant_type !== 'password') {
        return res.status(400).json({
            error: 'unsupported_grant_type',
            error_description: 'Only password grant type supported for Communication Users',
            supported_grant_types: ['password']
        });
    }

    // Validate Communication User
    const user = COMMUNICATION_USERS[username];
    if (!user || user.password !== password || !user.active) {
        return res.status(401).json({
            error: 'invalid_grant',
            error_description: 'Invalid Communication User credentials',
            hint: 'Ensure Communication User exists and is active in Communication Arrangement'
        });
    }

    // Check OAuth 2.0 support
    if (!user.authMethods.includes('oauth2')) {
        return res.status(400).json({
            error: 'unauthorized_client',
            error_description: 'OAuth 2.0 not configured for this Communication User',
            allowed_methods: user.authMethods
        });
    }

    // Update last login
    user.lastLogin = new Date().toISOString();

    // Generate tokens
    const tokenPayload = {
        sub: user.username,
        communicationUser: true,
        communicationSystem: user.communicationSystem,
        communicationArrangement: user.communicationArrangement,
        scenario: user.scenario,
        permissions: user.permissions,
        organizationalUnits: user.organizationalUnits,
        iat: Math.floor(Date.now() / 1000),
        exp: Math.floor(Date.now() / 1000) + 3600
    };

    const accessToken = jwt.sign(tokenPayload, JWT_SECRET);
    const refreshToken = jwt.sign({ 
        sub: user.username, 
        type: 'refresh',
        communicationUser: true 
    }, JWT_SECRET, { expiresIn: '24h' });

    res.json({
        access_token: accessToken,
        token_type: 'Bearer',
        expires_in: 3600,
        refresh_token: refreshToken,
        scope: user.permissions.join(' '),
        communication_user: user.username,
        communication_arrangement: user.communicationArrangement,
        communication_system: user.communicationSystem,
        scenario: user.scenario,
        service_endpoints: Object.keys(COMMUNICATION_ARRANGEMENTS).filter(arr => 
            COMMUNICATION_ARRANGEMENTS[arr].communicationUser === user.username
        ).map(arr => COMMUNICATION_ARRANGEMENTS[arr].serviceUrl)
    });
});

// API Key authentication (for read-only access)
router.post('/authenticate/apikey', (req, res) => {
    const { api_key } = req.body;
    const apiKeyHeader = req.headers['x-api-key'] || api_key;

    if (!apiKeyHeader) {
        return res.status(400).json({
            error: 'MISSING_API_KEY',
            message: 'API key required in X-API-Key header or request body',
            usage: 'X-API-Key: QALI_READ_<base64_encoded_credentials>'
        });
    }

    try {
        // API Key format: QALI_READ_<base64(username:timestamp)>
        if (!apiKeyHeader.startsWith('QALI_READ_')) {
            throw new Error('Invalid API key format');
        }

        const keyData = apiKeyHeader.substring(10); // Remove 'QALI_READ_' prefix
        const decoded = Buffer.from(keyData, 'base64').toString('ascii');
        const [username, timestamp] = decoded.split(':');

        // Validate user and API key support
        const user = COMMUNICATION_USERS[username];
        if (!user || !user.active || !user.authMethods.includes('apikey')) {
            throw new Error('Invalid API key or user');
        }

        // API keys are for read-only users only
        if (!user.permissions.every(p => p.includes(':read'))) {
            return res.status(403).json({
                error: 'INSUFFICIENT_PERMISSIONS',
                message: 'API key authentication only supports read-only operations',
                hint: 'Use OAuth 2.0 or Basic Authentication for write operations'
            });
        }

        // Update last login
        user.lastLogin = new Date().toISOString();

        // Generate read-only access token
        const tokenPayload = {
            sub: user.username,
            communicationUser: true,
            authMethod: 'apikey',
            communicationSystem: user.communicationSystem,
            communicationArrangement: user.communicationArrangement,
            scenario: user.scenario,
            permissions: user.permissions,
            organizationalUnits: user.organizationalUnits,
            iat: Math.floor(Date.now() / 1000),
            exp: Math.floor(Date.now() / 1000) + 1800 // 30 minutes for API key auth
        };

        const accessToken = jwt.sign(tokenPayload, JWT_SECRET);

        res.json({
            access_token: accessToken,
            token_type: 'Bearer',
            expires_in: 1800,
            auth_method: 'API Key',
            scope: 'read-only',
            permissions: user.permissions,
            communication_user: user.username,
            communication_arrangement: user.communicationArrangement,
            limitations: 'Read-only access - Create/Update/Delete operations not permitted'
        });

    } catch (error) {
        res.status(401).json({
            error: 'INVALID_API_KEY',
            message: 'Invalid API key format or expired key',
            format: 'QALI_READ_<base64(username:timestamp)>',
            example: 'QALI_READ_UUFMSV9SRUFEX1VTRVI6MTY0MTQwNDgwMA=='
        });
    }
});

// Token validation middleware (for protecting API endpoints)
const validateToken = (req, res, next) => {
    const authHeader = req.headers.authorization;
    
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
        return res.status(401).json({
            error: 'ACCESS_TOKEN_REQUIRED',
            message: 'Valid access token required for S/4HANA API access',
            authentication_options: [
                'POST /auth/communicate/basic - Communication User Basic Auth',
                'POST /auth/oauth2/token - Communication User OAuth 2.0', 
                'POST /auth/authenticate/apikey - API Key (Read-only)'
            ]
        });
    }

    try {
        const token = authHeader.split(' ')[1];
        const decoded = jwt.verify(token, JWT_SECRET);
        
        // Ensure it's a Communication User token
        if (!decoded.communicationUser) {
            return res.status(403).json({
                error: 'INVALID_USER_TYPE',
                message: 'Only Communication Users can access S/4HANA APIs',
                hint: 'Business Users cannot be used for API integration - use Communication User authentication'
            });
        }

        // Attach user info to request
        req.user = decoded;
        req.communicationArrangement = COMMUNICATION_ARRANGEMENTS[decoded.communicationArrangement];
        
        next();
    } catch (error) {
        if (error.name === 'TokenExpiredError') {
            return res.status(401).json({
                error: 'TOKEN_EXPIRED',
                message: 'Access token has expired',
                hint: 'Refresh token or re-authenticate with Communication User credentials'
            });
        }

        res.status(401).json({
            error: 'INVALID_TOKEN',
            message: 'Invalid or malformed access token'
        });
    }
};

// Communication Arrangements management endpoints
router.get('/communication/arrangements', (req, res) => {
    res.json({
        communicationArrangements: Object.values(COMMUNICATION_ARRANGEMENTS),
        total: Object.keys(COMMUNICATION_ARRANGEMENTS).length,
        note: 'Mock implementation of SAP Communication Arrangements as described in Issue #15'
    });
});

router.get('/communication/arrangements/:arrangementId', (req, res) => {
    const arrangement = COMMUNICATION_ARRANGEMENTS[req.params.arrangementId];
    
    if (!arrangement) {
        return res.status(404).json({
            error: 'ARRANGEMENT_NOT_FOUND',
            message: `Communication Arrangement ${req.params.arrangementId} not found`,
            availableArrangements: Object.keys(COMMUNICATION_ARRANGEMENTS)
        });
    }

    res.json(arrangement);
});

router.get('/communication/users', (req, res) => {
    // Return Communication Users without passwords
    const users = Object.keys(COMMUNICATION_USERS).map(username => {
        const user = { ...COMMUNICATION_USERS[username] };
        delete user.password; // Remove password from response
        return user;
    });

    res.json({
        communicationUsers: users,
        total: users.length,
        note: 'Communication Users are system users designed for API integration - Business Users cannot access APIs'
    });
});

// Export middleware for protecting API routes
module.exports = {
    router,
    validateToken,
    COMMUNICATION_USERS,
    COMMUNICATION_ARRANGEMENTS
};