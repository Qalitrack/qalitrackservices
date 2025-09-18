// Docker volume-mountable configuration
// This file can be mounted as /app/public/config.js in Docker
window.ENV = {
  API_BASE_URL: 'https://your-api-domain.com/api',
  TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
};