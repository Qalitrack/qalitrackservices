import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Enable standalone output for Docker
  output: 'standalone',
  
  // Allow images from external domains
  images: {
    domains: ['qalibrated.co.ke', 'localhost'],
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'qalibrated.co.ke',
        port: '',
        pathname: '/assets/**',
      },
      {
        protocol: 'http',
        hostname: 'localhost',
        port: '3001',
        pathname: '/**',
      },
    ],
  },

  // Environment variables that should be available in the browser
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
    NEXT_PUBLIC_SITE_URL: process.env.NEXT_PUBLIC_SITE_URL,
    NEXT_PUBLIC_ASSETS_URL: process.env.NEXT_PUBLIC_ASSETS_URL,
  },

  // Extend webpack to support SVGs as React components
  webpack(config) {
    // Safely find the default file-loader for SVGs
    const fileLoaderRule = config.module.rules.find((rule: any) => {
      return rule.test instanceof RegExp && rule.test.test('.svg');
    });

    // Exclude SVGs from the default loader
    if (fileLoaderRule) {
      fileLoaderRule.exclude = /\.svg$/i;
    }

    // Add SVGR loader to import SVGs as React components
    config.module.rules.push({
      test: /\.svg$/i,
      issuer: /\.[jt]sx?$/,
      use: ['@svgr/webpack'],
    });

    return config;
  },
};

export default nextConfig;
