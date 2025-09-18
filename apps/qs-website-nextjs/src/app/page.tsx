import React from 'react';
import Navbar from '@/components/Navbar';
import HeroCarousel from '@/components/HeroCarousel';
import AboutSnapshot from '@/components/AboutSnapshot';
import ServicesGrid from '@/components/ServicesGrid';
import StatsCounter from '@/components/StatsCounter';
import FAQAccordion from '@/components/FAQAccordion';
import Partners from '@/components/CertificationsLogos';
import Newsletter from '@/components/ContactCallToAction';
import ProductCards from '@/components/ProductsGrid';
import Footer from '@/components/Footer';

export const metadata = {
  title: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
  description: 'Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.',
  keywords: 'weighing systems, calibration services, automation solutions, weighbridges, industrial scales, building management, transport systems, Kenya, Nairobi',
  openGraph: {
    title: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
    description: 'Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.',
    url: 'https://qalibrated.co.ke',
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
    title: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
    description: 'Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.',
    images: ['https://qalibrated.co.ke/assets/LOGO-COLORED-CDvfuXKp.svg'],
    creator: '@qalibrated',
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      'max-video-preview': -1,
      'max-image-preview': 'large',
      'max-snippet': -1,
    },
  },
};

export default function Home() {
  return (
    <>
      <section id="navbar">
        <Navbar />
      </section>

      {/* Hero Carousel Section */}
      <section id="hero">
        <HeroCarousel />
      </section>
      {/* About Snapshot Section */}
      <section id="about">
        <AboutSnapshot />
      </section>
      <section id="services">
        <ProductCards />
      </section>

      {/* Services Section */}
      <section id="services">
        <ServicesGrid />
      </section>
     
      {/* Stats/Counter Section */}
      <section id="stats">
        <StatsCounter />
      </section>
     
      {/* FAQs Section */}
      <section id="faq">
        <FAQAccordion />
      </section>
       {/* Newsletter Section */}
      <section id="faq">
        <Newsletter />
      </section>

          {/* Partners Section */}
      <section id="faq">
        <Partners />
      </section>
      <section id="footer">
        <Footer />
      </section>
    </>
  );
}