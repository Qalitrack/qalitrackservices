// Configuration that can be passed as volume to Docker
// First check for runtime config (Docker volume mounted), then env vars, then defaults
const getRuntimeConfig = () => {
  if (typeof window !== 'undefined' && window.ENV) {
    return window.ENV;
  }
  return {};
};

const runtimeConfig = getRuntimeConfig();

const config = {
  API_BASE_URL: (runtimeConfig.API_BASE_URL && runtimeConfig.API_BASE_URL !== '__NEXT_PUBLIC_API_BASE_URL__') ? 
                runtimeConfig.API_BASE_URL : 
                process.env.NEXT_PUBLIC_API_BASE_URL || 
                'http://localhost:5000/api',
  SITE_URL: (runtimeConfig.SITE_URL && runtimeConfig.SITE_URL !== '__NEXT_PUBLIC_SITE_URL__') ? 
            runtimeConfig.SITE_URL : 
            process.env.NEXT_PUBLIC_SITE_URL || 
            'http://localhost:3001',
  ASSETS_URL: (runtimeConfig.ASSETS_URL && runtimeConfig.ASSETS_URL !== '__NEXT_PUBLIC_ASSETS_URL__') ? 
              runtimeConfig.ASSETS_URL : 
              process.env.NEXT_PUBLIC_ASSETS_URL || 
              'http://localhost:3001',
  // Add other configuration values that might be needed
  TIMEOUT: runtimeConfig.TIMEOUT || 10000,
  RETRY_ATTEMPTS: runtimeConfig.RETRY_ATTEMPTS || 3,
};

export default config;