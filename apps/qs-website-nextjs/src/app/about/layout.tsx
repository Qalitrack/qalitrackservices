import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'About Us - Qalibrated Systems Limited | Engineering Precision & Infrastructure Innovation',
  description: 'Learn about Qalibrated Systems Limited (QSL), a Kenyan company established in 2009, specializing in weighing systems, calibrations, industrial automation, and infrastructure development.',
  keywords: 'about Qalibrated, company history, weighing systems Kenya, calibration services, industrial automation, engineering company Nairobi',
  path: '/about',
});

export default function AboutLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}