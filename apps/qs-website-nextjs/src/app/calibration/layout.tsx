import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Calibration Services - Qalibrated Systems Limited | Professional Equipment Calibration',
  description: 'Professional calibration services for volumetric tanks, weighing systems, flow meters, and pressure equipment. ISO 17025 compliant calibration services across Kenya.',
  keywords: 'calibration services, tank calibration, weighing calibration, flow meter calibration, pressure calibration, ISO 17025, Kenya calibration',
  path: '/calibration',
});

export default function CalibrationLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}