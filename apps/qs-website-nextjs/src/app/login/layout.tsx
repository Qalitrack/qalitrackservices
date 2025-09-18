import { createMetadata } from '@/utils/seo';

export const metadata = {
  ...createMetadata({
    title: 'Login - Qalibrated Systems Limited | Access Your Dashboard',
    description: 'Login to your Qalibrated Systems Limited account to access your dashboard, manage products, view analytics, and monitor your weighing and automation systems.',
    keywords: 'login, dashboard access, user account, Qalibrated login, system management, weighing system dashboard',
    path: '/login',
    image: '/og-image.svg',
  }),
  robots: {
    index: false,
    follow: false,
    noindex: true,
    nofollow: true,
  },
};

export default function LoginLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}