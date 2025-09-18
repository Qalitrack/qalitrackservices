import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Multideck & Singledeck Weighbridges - Qalibrated Systems Limited | Commercial Truck Scales',
  description: 'Professional multideck and singledeck weighbridges for accurate truck weighing. High-capacity commercial weighing solutions with digital indicators and software integration.',
  keywords: 'multideck weighbridges, singledeck weighbridges, truck scales, commercial weighing, vehicle weighing, weighbridge Kenya, truck weighing systems',
  path: '/weighing/multideck-singledeck',
});

export default function MultideckSingledeckLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}