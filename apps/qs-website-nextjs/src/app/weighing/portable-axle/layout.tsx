import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Portable & Axle Weighers - Qalibrated Systems Limited | Mobile Weighing Solutions',
  description: 'Portable weighing scales and axle weighers for mobile and temporary weighing applications. Accurate, durable, and easy-to-use weighing solutions for various industries.',
  keywords: 'portable weighers, axle weighers, mobile weighing, portable scales, temporary weighing, vehicle axle weighing, portable weighing systems',
  openGraph: {
    title: 'Portable & Axle Weighers - Qalibrated Systems Limited | Mobile Weighing Solutions',
    description: 'Portable weighing scales and axle weighers for mobile and temporary weighing applications. Accurate, durable, and easy-to-use weighing solutions for various industries.',
    url: 'https://qalibrated.co.ke/weighing/portable-axle',
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
    title: 'Portable & Axle Weighers - Qalibrated Systems Limited | Mobile Weighing Solutions',
    description: 'Portable weighing scales and axle weighers for mobile and temporary weighing applications. Accurate, durable, and easy-to-use weighing solutions for various industries.',
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

export default function PortableAxleLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}