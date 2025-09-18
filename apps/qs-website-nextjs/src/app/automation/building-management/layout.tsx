import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Building Management Systems - Qalibrated Systems Limited | Smart Building Automation',
  description: 'Advanced building management systems for efficient monitoring and control of HVAC, lighting, security, and building infrastructure. Smart automation solutions for modern buildings.',
  keywords: 'building management systems, BMS, smart buildings, building automation, HVAC control, lighting control, facility management, automated buildings',
  openGraph: {
    title: 'Building Management Systems - Qalibrated Systems Limited | Smart Building Automation',
    description: 'Advanced building management systems for efficient monitoring and control of HVAC, lighting, security, and building infrastructure. Smart automation solutions for modern buildings.',
    url: 'https://qalibrated.co.ke/automation/building-management',
    siteName: 'Qalibrated Systems Limited',
    images: [
      {
        url: 'https://qalibrated.co.ke/assets/LOGO-COLORED-CDvfuXKp.svg',
        width: 1200,
        height: 630,
        alt: 'Qalibrated Systems Limited Logo',
      }
    ],
    locale: 'en_US',
    type: 'website',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Building Management Systems - Qalibrated Systems Limited | Smart Building Automation',
    description: 'Advanced building management systems for efficient monitoring and control of HVAC, lighting, security, and building infrastructure. Smart automation solutions for modern buildings.',
    images: ['https://qalibrated.co.ke/assets/LOGO-COLORED-CDvfuXKp.svg'],
    creator: '@qalibrated',
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      'max-video-preview': -1,
      'max-image-preview': 'large',
      'max-snippet': -1,
    },
  },
};

export default function BuildingManagementLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}