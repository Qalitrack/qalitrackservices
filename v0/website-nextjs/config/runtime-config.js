// Runtime configuration for production deployment
// This file will be mounted as volume in Docker container
window.ENV = {
  API_BASE_URL: 'https://api.qalibrated.co.ke',
  SITE_URL: 'https://qalibrated.co.ke',
  ASSETS_URL: 'https://qalibrated.co.ke',
  TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
};