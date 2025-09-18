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
import { createMetadata } from '@/utils/seo';

export const metadata = createMetadata({
  title: 'Qalibrated Systems Limited - Professional Weighing & Automation Solutions in Kenya',
  description: 'Leading provider of innovative weighing, calibration, and automation systems in Kenya. Professional solutions for commercial weighing, building management, and intelligent transport systems.',
  keywords: 'weighing systems, calibration services, automation solutions, weighbridges, industrial scales, building management, transport systems, Kenya, Nairobi',
  path: '/',
  image: '/og-image.svg',
});

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