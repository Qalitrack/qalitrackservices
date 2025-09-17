// Helper script to setup test database
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

async function setupTestDatabase() {
  console.log('🗄️  Setting up test database...');
  
  try {
    // Check if we're accidentally using production database
    if (fs.existsSync('db.sqlite3') && !fs.existsSync('.env.testing')) {
      console.log('⚠️  Production database detected. Creating isolated test environment...');
    }
    
    // Create test marker file
    fs.writeFileSync('.env.testing', `TESTING=true\nSTARTED=${new Date().toISOString()}\n`);
    console.log('🧪 Created test environment marker');
    
    // Delete test database if it exists (safe - only test DB)
    const testDbPath = 'test_db.sqlite3';
    if (fs.existsSync(testDbPath)) {
      fs.unlinkSync(testDbPath);
      console.log('🗑️  Removed existing test database');
    }
    
    // Also clean up any cached database files
    const cacheFiles = ['test_db.sqlite3-wal', 'test_db.sqlite3-shm'];
    cacheFiles.forEach(file => {
      if (fs.existsSync(file)) {
        fs.unlinkSync(file);
        console.log(`🗑️  Removed ${file}`);
      }
    });
    
    // Create test database with environment variable
    execSync('source venv/bin/activate && DATABASE_URL="sqlite:///test_db.sqlite3" python manage.py migrate', { 
      shell: '/bin/bash',
      cwd: process.cwd(),
      stdio: 'inherit',
      env: {
        ...process.env,
        DATABASE_URL: 'sqlite:///test_db.sqlite3',
        DEBUG: 'True',
        SECRET_KEY: 'test-secret-key-for-testing-only'
      }
    });
    
    // Flush all data to ensure clean start
    execSync('source venv/bin/activate && DATABASE_URL="sqlite:///test_db.sqlite3" python manage.py flush --noinput', { 
      shell: '/bin/bash',
      cwd: process.cwd(),
      stdio: 'inherit',
      env: {
        ...process.env,
        DATABASE_URL: 'sqlite:///test_db.sqlite3',
        DEBUG: 'True',
        SECRET_KEY: 'test-secret-key-for-testing-only'
      }
    });
    
    console.log('✅ Test database setup complete');
    console.log('📝 Using test database: test_db.sqlite3');
    console.log('🔒 Production database (db.sqlite3) untouched');
    
  } catch (error) {
    console.error('❌ Test database setup failed:', error.message);
    
    // Clean up on failure
    if (fs.existsSync('.env.testing')) {
      fs.unlinkSync('.env.testing');
    }
    
    process.exit(1);
  }
}

function cleanupTestEnvironment() {
  console.log('🧹 Cleaning up test environment...');
  
  try {
    // Remove test marker
    if (fs.existsSync('.env.testing')) {
      fs.unlinkSync('.env.testing');
      console.log('🗑️  Removed test environment marker');
    }
    
    // Optional: Remove test database (comment out to keep for debugging)
    if (fs.existsSync('test_db.sqlite3')) {
      fs.unlinkSync('test_db.sqlite3');
      console.log('🗑️  Removed test database');
    }
    
    // Clean up test media files
    if (fs.existsSync('test_media')) {
      execSync('rm -rf test_media', { stdio: 'inherit' });
      console.log('🗑️  Removed test media files');
    }
    
    console.log('✅ Test environment cleaned up');
  } catch (error) {
    console.error('❌ Failed to cleanup test environment:', error.message);
  }
}

if (require.main === module) {
  const command = process.argv[2];
  
  if (command === 'cleanup') {
    cleanupTestEnvironment();
  } else {
    setupTestDatabase();
  }
}

module.exports = { setupTestDatabase, cleanupTestEnvironment };