export const metadata = {
  title: 'Login - Qalibrated Systems Limited | Access Your Dashboard',
  description: 'Login to your Qalibrated Systems Limited account to access your dashboard, manage products, view analytics, and monitor your weighing and automation systems.',
  keywords: 'login, dashboard access, user account, Qalibrated login, system management, weighing system dashboard',
  openGraph: {
    title: 'Login - Qalibrated Systems Limited | Access Your Dashboard',
    description: 'Login to your Qalibrated Systems Limited account to access your dashboard, manage products, view analytics, and monitor your weighing and automation systems.',
    url: 'https://qalibrated.co.ke/login',
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
    title: 'Login - Qalibrated Systems Limited | Access Your Dashboard',
    description: 'Login to your Qalibrated Systems Limited account to access your dashboard, manage products, view analytics, and monitor your weighing and automation systems.',
    images: ['https://qalibrated.co.ke/assets/LOGO-COLORED-CDvfuXKp.svg'],
    creator: '@qalibrated',
  },
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