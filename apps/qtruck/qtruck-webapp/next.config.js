/** @type {import('next').NextConfig} */
const nextConfig = {
  trailingSlash: true,
  images: {
    unoptimized: true
  },
  // Enable standalone output for Docker
  output: 'standalone',
  // Skip ESLint during build (we run it separately in CI)
  eslint: {
    ignoreDuringBuilds: true
  },
  // Skip TypeScript checking during build (we run it separately in CI)
  typescript: {
    ignoreBuildErrors: true
  }
}

module.exports = nextConfig