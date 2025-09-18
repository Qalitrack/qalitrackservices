import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Weighing Software - Qalibrated Systems Limited | Digital Weighing Management Systems',
  description: 'Advanced weighing software for data management, reporting, and integration with ERP systems. Comprehensive digital solutions for weighbridge and scale management.',
  keywords: 'weighing software, weighbridge software, scale management software, weighing data management, digital weighing systems, ERP integration',
  openGraph: {
    title: 'Weighing Software - Qalibrated Systems Limited | Digital Weighing Management Systems',
    description: 'Advanced weighing software for data management, reporting, and integration with ERP systems. Comprehensive digital solutions for weighbridge and scale management.',
    url: 'https://qalibrated.co.ke/weighing/software',
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
    title: 'Weighing Software - Qalibrated Systems Limited | Digital Weighing Management Systems',
    description: 'Advanced weighing software for data management, reporting, and integration with ERP systems. Comprehensive digital solutions for weighbridge and scale management.',
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

export default function WeighingSoftwareLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}