import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { createMetadata } from "@/utils/seo";
import AIChatWidget from "@/components/AIChatWidget"; // make sure this path is correct

// ✅ Define fonts
const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  ...createMetadata({
    title: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
    description: 'Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.',
    keywords: 'weighing systems, calibration services, automation solutions, weighbridges, industrial scales, building management, transport systems, Kenya, Nairobi',
    path: '/',
  }),
  title: {
    default: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
    template: '%s | Qalibrated Systems Limited',
  },
  authors: [{ name: 'Qalibrated Systems Limited' }],
  creator: 'Qalibrated Systems Limited',
  publisher: 'Qalibrated Systems Limited',
  icons: {
    icon: '/favicon-32x32.png',
    shortcut: '/favicon-32x32.png',
    apple: '/apple-touch-icon.png',
  },
  manifest: '/site.webmanifest',
  verification: {
    google: process.env.NEXT_PUBLIC_GOOGLE_SITE_VERIFICATION || '',
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <head>
        <script src="/config.js" async></script>
      </head>
      <body className={`${geistSans.variable} ${geistMono.variable} antialiased`}>
        {children}
        <AIChatWidget /> {/* ✅ Global chatbot */}
      </body>
    </html>
  );
}
