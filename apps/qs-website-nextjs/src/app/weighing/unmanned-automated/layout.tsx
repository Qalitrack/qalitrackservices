import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Unmanned & Automated Weighbridges - Qalibrated Systems Limited | Automated Weighing Systems',
  description: 'Unmanned and automated weighbridge systems with RFID technology, automated barriers, and remote monitoring. Efficient 24/7 weighing operations without manual intervention.',
  keywords: 'unmanned weighbridges, automated weighbridges, RFID weighing, automated weighing systems, remote weighing, unattended weighing',
  openGraph: {
    title: 'Unmanned & Automated Weighbridges - Qalibrated Systems Limited | Automated Weighing Systems',
    description: 'Unmanned and automated weighbridge systems with RFID technology, automated barriers, and remote monitoring. Efficient 24/7 weighing operations without manual intervention.',
    url: 'https://qalibrated.co.ke/weighing/unmanned-automated',
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
    title: 'Unmanned & Automated Weighbridges - Qalibrated Systems Limited | Automated Weighing Systems',
    description: 'Unmanned and automated weighbridge systems with RFID technology, automated barriers, and remote monitoring. Efficient 24/7 weighing operations without manual intervention.',
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

export default function UnmannedAutomatedLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}