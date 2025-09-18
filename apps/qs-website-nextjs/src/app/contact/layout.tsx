import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Contact Us - Qalibrated Systems Limited | Get in Touch for Weighing & Automation Solutions',
  description: 'Contact Qalibrated Systems Limited for professional weighing, calibration, and automation solutions. Located in Nairobi with branches in Mombasa and Kisumu. Call +254714999996.',
  keywords: 'contact Qalibrated, weighing solutions Kenya, Nairobi automation company, calibration services contact, QSL contact information',
  path: '/contact',
});

export default function ContactLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}