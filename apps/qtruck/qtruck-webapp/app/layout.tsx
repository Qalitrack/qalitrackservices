import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import { AuthProvider } from '@/lib/auth/AuthContext'
import { createMetadata } from '@/lib/seo'

const inter = Inter({ subsets: ['latin'] })

export async function generateMetadata(): Promise<Metadata> {
  const baseMetadata = createMetadata({
    title: 'QTruck - Fleet Management Made Simple',
    description: 'Complete fleet management solution for modern logistics companies. Track vehicles, manage drivers, monitor expenses, and optimize routes with our comprehensive fleet management system.',
    keywords: 'fleet management, vehicle tracking, driver management, logistics, transportation, expense tracking, route optimization, truck management, Kenya',
    path: '/',
  });

  return {
    ...baseMetadata,
    title: {
      default: 'QTruck - Fleet Management Made Simple',
      template: '%s | QTruck',
    },
    authors: [{ name: 'QTruck Team' }],
    creator: 'QTruck',
    publisher: 'QTruck',
    icons: {
      icon: '/favicon.svg',
      shortcut: '/favicon-32x32.png',
      apple: '/favicon-32x32.png',
    },
    manifest: '/site.webmanifest',
    verification: {
      google: process.env.NEXT_PUBLIC_GOOGLE_SITE_VERIFICATION || '',
    },
  };
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <AuthProvider>
          {children}
        </AuthProvider>
      </body>
    </html>
  )
}