/** @type {import('next').NextConfig} */
const nextConfig = {
  trailingSlash: true,
  images: {
    unoptimized: true
  },
  // Enable standalone output for Docker
  output: 'standalone',
  // Configure for dynamic pages
  experimental: {
    outputFileTracingRoot: undefined
  }
}

module.exports = nextConfig