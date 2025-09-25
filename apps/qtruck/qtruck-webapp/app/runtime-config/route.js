import { NextResponse } from 'next/server'

export async function GET() {
  // Extract base domain from API_URL if it includes /api
  const apiUrl = process.env.API_URL || '/api'
  const baseDomain = apiUrl.replace(/\/api$/, '') || ''
  
  return NextResponse.json({
    apiUrl: baseDomain, // This will be the base domain for building URLs
    siteUrl: process.env.SITE_URL || process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000',
    appName: process.env.APP_NAME || process.env.NEXT_PUBLIC_APP_NAME || 'QTruck'
  })
}