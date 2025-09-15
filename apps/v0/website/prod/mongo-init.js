// MongoDB initialization script to create application user with proper permissions
print('Starting MongoDB initialization...');

// Switch to the application database
db = db.getSiblingDB('products_dashboard_db');

// Create application user with read/write access to the application database
db.createUser({
  user: process.env.MONGO_APP_USERNAME || 'app_user',
  pwd: process.env.MONGO_APP_PASSWORD || 'SecureAppUser456$%^',
  roles: [
    {
      role: 'readWrite',
      db: 'products_dashboard_db'
    }
  ]
});

print('Application user created successfully');

// Create indexes for better performance and security
db.users.createIndex({ email: 1 }, { unique: true });
db.users.createIndex({ createdAt: 1 });
db.products.createIndex({ createdAt: 1 });
db.products.createIndex({ category: 1 });

print('Database indexes created successfully');
print('MongoDB initialization completed');