import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Automation Solutions - Qalibrated Systems Limited | Industrial & Building Automation',
  description: 'Advanced automation solutions including building management systems, industrial automation, and intelligent transport systems. Smart solutions for modern infrastructure needs.',
  keywords: 'automation solutions, building management systems, industrial automation, intelligent transport systems, smart buildings, automated systems, Kenya',
  path: '/automation',
});

export default function AutomationLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}