import { createMetadata } from '@/utils/seo';
export const metadata = {
  title: 'Volumetric Tank Calibration - Qalibrated Systems Limited | ISO 17025 Tank Calibration Services',
  description: 'Professional volumetric tank calibration services compliant with ISO 17025 standards. Accurate calibration for fuel tanks, water tanks, and industrial storage vessels.',
  keywords: 'volumetric tank calibration, tank calibration, fuel tank calibration, ISO 17025 calibration, tank measurement, storage tank calibration',
  openGraph: {
    title: 'Volumetric Tank Calibration - Qalibrated Systems Limited | ISO 17025 Tank Calibration Services',
    description: 'Professional volumetric tank calibration services compliant with ISO 17025 standards. Accurate calibration for fuel tanks, water tanks, and industrial storage vessels.',
    url: 'https://qalibrated.co.ke/calibration/volumetric-tank',
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
    title: 'Volumetric Tank Calibration - Qalibrated Systems Limited | ISO 17025 Tank Calibration Services',
    description: 'Professional volumetric tank calibration services compliant with ISO 17025 standards. Accurate calibration for fuel tanks, water tanks, and industrial storage vessels.',
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

export default function VolumetricTankLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}