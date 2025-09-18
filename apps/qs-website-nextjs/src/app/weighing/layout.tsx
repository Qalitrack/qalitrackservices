import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Weighing Systems - Qalibrated Systems Limited | Weighbridges, Scales & Weighing Solutions',
  description: 'Professional weighing systems including multideck weighbridges, portable scales, axle weighers, retail scales, and weighing software. Quality solutions for commercial and industrial use.',
  keywords: 'weighing systems, weighbridges, truck scales, portable weighers, axle weighers, retail scales, weighing software, onboard weighing, Kenya',
  path: '/weighing',
});

export default function WeighingLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}