import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Product Catalogue - Qalibrated Systems Limited | Weighing & Automation Products',
  description: 'Explore our comprehensive product catalogue featuring weighing systems, calibration equipment, automation solutions, and accessories. Quality products for industrial and commercial use.',
  keywords: 'product catalogue, weighing equipment, calibration tools, automation products, industrial scales, weighbridge accessories, Kenya',
  path: '/catalogue',
});

export default function CatalogueLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}