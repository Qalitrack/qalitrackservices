import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  
  // Parse allowed hosts from environment variable
  const getAllowedHosts = () => {
    const hostsEnv = env.VITE_ALLOWED_HOSTS || env.ALLOWED_HOSTS;
    if (hostsEnv) {
      // Split by comma and trim whitespace
      return hostsEnv.split(',').map(host => host.trim());
    }
    // Default fallback
    return ['localhost'];
  };
  
  return {
    plugins: [react()],
    define: {
      __APP_ENV__: JSON.stringify(env.APP_ENV),
    },
    server: {
      host: '0.0.0.0',
      port: 3000,
      allowedHosts: getAllowedHosts()
    }
  }
})