import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Services - Qalibrated Systems Limited | Weighing, Calibration & Automation Solutions',
  description: 'Comprehensive weighing systems, calibration services, and automation solutions. Professional services including weighbridges, building management systems, and intelligent transport solutions.',
  keywords: 'weighing services, calibration services, automation solutions, weighbridges Kenya, building management systems, transport solutions, industrial automation',
  path: '/services',
});

export default function ServicesLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}