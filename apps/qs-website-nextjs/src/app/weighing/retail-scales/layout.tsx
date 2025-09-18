import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Retail Scales - Qalibrated Systems Limited | Commercial Retail Weighing Solutions',
  description: 'Professional retail scales for shops, markets, and commercial establishments. Accurate and reliable weighing solutions for retail and commercial applications.',
  keywords: 'retail scales, commercial scales, shop scales, market scales, retail weighing, commercial weighing solutions',
  openGraph: {
    title: 'Retail Scales - Qalibrated Systems Limited | Commercial Retail Weighing Solutions',
    description: 'Professional retail scales for shops, markets, and commercial establishments. Accurate and reliable weighing solutions for retail and commercial applications.',
    url: 'https://qalibrated.co.ke/weighing/retail-scales',
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
    title: 'Retail Scales - Qalibrated Systems Limited | Commercial Retail Weighing Solutions',
    description: 'Professional retail scales for shops, markets, and commercial establishments. Accurate and reliable weighing solutions for retail and commercial applications.',
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

export default function RetailScalesLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}