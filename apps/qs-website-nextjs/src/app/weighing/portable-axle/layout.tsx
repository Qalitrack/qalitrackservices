import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Portable & Axle Weighers - Qalibrated Systems Limited | Mobile Weighing Solutions',
  description: 'Portable weighing scales and axle weighers for mobile and temporary weighing applications. Accurate, durable, and easy-to-use weighing solutions for various industries.',
  keywords: 'portable weighers, axle weighers, mobile weighing, portable scales, temporary weighing, vehicle axle weighing, portable weighing systems',
  path: '/weighing/portable-axle',
  image: '/og-image.svg',
});
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