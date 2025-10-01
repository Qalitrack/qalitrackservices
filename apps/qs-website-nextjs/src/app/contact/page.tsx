'use client';

import dynamic from 'next/dynamic';
import React, { useEffect, useState } from 'react';
import { MapPin, Phone, MailOpen, PhoneCall, Link, ArrowUp, Navigation } from 'lucide-react';
import Image from 'next/image';

import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import ContactImage from '@/assets/axleweighers.png';
import placer from '@/assets/logoblack.svg';
import placer1 from '@/assets/portfolio-6.jpg';

// Dynamically load map to prevent SSR issues
const ContactMap = dynamic(() => import('./ContactMap'), { ssr: false });

export default function Contact() {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768);
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
      <Navbar />

      {/* Hero Section */}
      <section
        className="relative bg-center text-white py-20 md:py-32"
        style={{
          backgroundImage: `url(${ContactImage.src})`,
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover',
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Contact Us</h1>
          {/* <p className="text-lg md:text-xl">
            Home / <span className="text-amber-400">Contact</span>
          </p> */}
        </div>
      </section>

      {/* Main Contact Content */}
      <section className="container mx-auto px-4 my-16">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
          {/* Left Column */}
          <div className="bg-gray-50 p-8 rounded-lg shadow-md border border-gray-200 h-full">
            <h2 className="text-amber-500 text-lg font-semibold mb-6">Get In Touch</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-8">
              {/* Address */}
              <div className="flex flex-col items-center text-center">
                <MapPin className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Address</h4>
                <a
                  href="https://maps.google.com/?cid=9409932413320131801&g_mp=Cidnb29nbGUubWFwcy5wbGFjZXMudjEuUGxhY2VzLlNlYXJjaFRleHQ"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-gray-600 text-sm hover:text-amber-500 transition"
                >
                  Qalibrated Systems Limited,<br />QSL Centre, Mombasa Road, Nairobi
                </a>
                <a
                  href="https://www.google.com/maps/dir//Qalibrated+Systems+Limited,+QSL+CENTER,+Mombasa+Road,+Nairobi,+Kenya/data=!4m9!4m8!1m0!1m5!1m1!19sChIJeXakEuMRLxgR2XRhiKzLloI!2m2!1d36.9208099!2d-1.3745112!3e0"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-2 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-amber-500 hover:bg-amber-600 transition"
                >
                  <Navigation className="mr-2 h-4 w-4" />
                  Navigate
                </a>
              </div>

              {/* Email */}
              <div className="flex flex-col items-center text-center">
                <MailOpen className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Mail Us</h4>
                <a href="mailto:info@qalibrated.co.ke" className="text-gray-600 text-sm hover:text-amber-500 transition">
                  info@qalibrated.co.ke
                </a>
              </div>

              {/* Phone */}
              <div className="flex flex-col items-center text-center">
                <PhoneCall className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Telephone</h4>
                <a href="tel:+254714999996" className="text-gray-600 text-sm hover:text-amber-500 transition">
                  +254714999996
                </a>
              </div>

              {/* Website */}
              <div className="flex flex-col items-center text-center">
                <Link className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Website</h4>
                <a href="https://qalibrated.co.ke/" target="_blank" rel="noopener noreferrer" className="text-gray-600 text-sm hover:text-amber-500 transition">
                  https://qalibrated.co.ke/
                </a>
              </div>

              {/* Images */}
              <div>
                <Image src={placer} alt="Advisor" className="rounded-lg h-full" />
              </div>
              <div>
                <Image src={placer1} alt="Advisor" className="rounded-lg" />
              </div>
            </div>
          </div>

          {/* Right Column */}
          <div className="bg-gray-50 p-8 rounded-lg shadow-md border border-gray-200 h-full">
            <h2 className="text-amber-500 text-lg font-semibold mb-2">Send Your Message</h2>
            <p className="text-gray-600 text-sm mb-6">Talk to us</p>
            <form action="mailto:info@qalibrated.co.ke" method="post" encType="text/plain" className="space-y-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <input type="text" name="name" placeholder="Your Name" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400" />
                <input type="email" name="email" placeholder="Your Email" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400" />
                <input type="tel" name="phone" placeholder="Your Phone" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400" />
                <input type="text" name="project" placeholder="Your Project" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400" />
              </div>
              <input type="text" name="subject" placeholder="Subject" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400" />
              <textarea name="message" rows={6} placeholder="Message" className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400 resize-y" />
              <button type="submit" className="bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition">
                Send Message
              </button>
            </form>
          </div>
        </div>

        {/* Map Section */}
        <div className="mt-12">
          <ContactMap isMobile={isMobile} />
        </div>
      </section>

      {/* Scroll to top button */}
      {/* <button className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg transition" onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}>
        <ArrowUp className="w-6 h-6" />
      </button> */}

      <Footer />
    </div>
  );
}