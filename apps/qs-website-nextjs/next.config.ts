import type { NextConfig } from "next";
import type { RuleSetRule } from "webpack"; // <-- Add this import

const nextConfig: NextConfig = {
  // ... (rest of your config unchanged)

  webpack(config) {
    // Safely find the default file-loader for SVGs
    const fileLoaderRule = config.module.rules.find((rule: RuleSetRule) => {
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