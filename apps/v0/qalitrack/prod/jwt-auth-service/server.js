const express = require('express');
const jwt = require('jsonwebtoken');

const app = express();
const PORT = process.env.PORT || 3000;

// JWT Configuration - same as your UserService
const JWT_SECRET = process.env.JWT_SECRET_KEY || 'mqtNGlrJcW/j+7u73scKbGYVkfAM1qUsfRET77ss5yStSusjT7zAw62WGUZff6Rh';
const JWT_ISSUER = process.env.JWT_ISSUER || 'UserService';
const JWT_AUDIENCE = process.env.JWT_AUDIENCE || 'UserService';

app.use(express.json());

// Health check endpoint
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'healthy', service: 'jwt-auth-service' });
});

// JWT validation endpoint for Traefik ForwardAuth
app.use('/auth', (req, res) => {
    try {
        // Get token from Authorization header
        const authHeader = req.headers.authorization;
        
        if (!authHeader) {
            console.log('No authorization header provided');
            return res.status(401).json({ error: 'No authorization header' });
        }

        if (!authHeader.startsWith('Bearer ')) {
            console.log('Invalid authorization header format');
            return res.status(401).json({ error: 'Invalid authorization header format' });
        }

        const token = authHeader.substring(7); // Remove 'Bearer ' prefix

        if (!token) {
            console.log('No token provided');
            return res.status(401).json({ error: 'No token provided' });
        }

        // Verify JWT token
        const decoded = jwt.verify(token, JWT_SECRET, {
            issuer: JWT_ISSUER,
            audience: JWT_AUDIENCE,
            algorithms: ['HS256']
        });

        console.log(`Valid token for user: ${decoded.email || decoded.sub}`);
        
        // Token is valid - return 200 to allow access
        res.status(200).json({ 
            valid: true, 
            user: decoded.email || decoded.sub,
            userId: decoded.sub 
        });

    } catch (error) {
        console.log('JWT validation failed:', error.message);
        
        if (error.name === 'TokenExpiredError') {
            return res.status(401).json({ error: 'Token expired' });
        } else if (error.name === 'JsonWebTokenError') {
            return res.status(401).json({ error: 'Invalid token' });
        } else {
            return res.status(500).json({ error: 'Token validation error' });
        }
    }
});

app.listen(PORT, '0.0.0.0', () => {
    console.log(`JWT Auth Service running on port ${PORT}`);
    console.log(`Health check: http://localhost:${PORT}/health`);
    console.log(`Auth endpoint: http://localhost:${PORT}/auth`);
});