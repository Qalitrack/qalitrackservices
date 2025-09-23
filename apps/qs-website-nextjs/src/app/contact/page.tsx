'use client';

import { createMetadata } from '@/utils/seo';
import React, { useEffect, useState } from 'react';
import {
  MapPin, Phone, MailOpen, PhoneCall, Link, ArrowUp,
} from 'lucide-react';
import Image from 'next/image';

import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import ContactImage from '@/assets/axleweighers.png';
import placer from '@/assets/logoblack.svg';
import placer1 from '@/assets/portfolio-6.jpg';

import { MapContainer, TileLayer, Marker, Popup, Tooltip, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

const position: [number, number] = [-1.359227, 36.937984];

const redIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.7.1/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
  className: 'marker-bounce',
});

// Fix map disappearing on zoom
function FixMapSize() {
  const map = useMap();
  useEffect(() => {
    setTimeout(() => map.invalidateSize(), 100);
  }, [map]);
  return null;
}

// Custom Zoom buttons
function ZoomButtons() {
  const map = useMap();
  useEffect(() => {
    L.control.zoom({ position: 'bottomright' }).addTo(map);
  }, [map]);
  return null;
}

export default function Contact() {
  const [isMobile, setIsMobile] = useState(false);
  const [isBrowser, setIsBrowser] = useState(false); // detect client

  useEffect(() => {
    setIsBrowser(true); // safe to use window now
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
          <p className="text-lg md:text-xl">
            Home / <span className="text-amber-400">Contact</span>
          </p>
        </div>
      </section>

      {/* Main Contact Content */}
      <section className="container mx-auto px-4 my-16">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
          {/* Left Column */}
          <div className="bg-gray-50 p-8 rounded-lg shadow-md border border-gray-200 h-full">
            <h2 className="text-amber-500 text-lg font-semibold mb-6">Get In Touch</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-8">
              <div className="flex flex-col items-center text-center">
                <MapPin className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Address</h4>
                <a
                  href="https://www.google.com/maps/dir/?api=1&destination=-1.359227,36.937984"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-gray-600 text-sm hover:text-amber-500 transition"
                >
                  Qalibrated Systems Limited,<br />QSL Centre, Mombasa Road, Nairobi
                </a>
              </div>

              <div className="flex flex-col items-center text-center">
                <MailOpen className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Mail Us</h4>
                <a
                  href="mailto:info@qalibrated.co.ke"
                  className="text-gray-600 text-sm hover:text-amber-500 transition"
                >
                  info@qalibrated.co.ke
                </a>
              </div>

              <div className="flex flex-col items-center text-center">
                <PhoneCall className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Telephone</h4>
                <a
                  href="tel:+254714999996"
                  className="text-gray-600 text-sm hover:text-amber-500 transition"
                >
                  +254714999996
                </a>
              </div>

              <div className="flex flex-col items-center text-center">
                <Link className="h-12 w-12 text-amber-500 mb-3" />
                <h4 className="font-bold text-xl mb-1">Website</h4>
                <a
                  href="https://qalibrated.co.ke/"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-gray-600 text-sm hover:text-amber-500 transition"
                >
                  https://qalibrated.co.ke/
                </a>
              </div>

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
            <form
              action="mailto:info@qalibrated.co.ke"
              method="post"
              encType="text/plain"
              className="space-y-6"
            >
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <input
                  type="text"
                  name="name"
                  placeholder="Your Name"
                  className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400"
                />
                <input
                  type="email"
                  name="email"
                  placeholder="Your Email"
                  className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400"
                />
                <input
                  type="tel"
                  name="phone"
                  placeholder="Your Phone"
                  className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400"
                />
                <input
                  type="text"
                  name="project"
                  placeholder="Your Project"
                  className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400"
                />
              </div>
              <input
                type="text"
                name="subject"
                placeholder="Subject"
                className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400"
              />
              <textarea
                name="message"
                rows={6}
                placeholder="Message"
                className="shadow border rounded w-full py-3 px-4 text-gray-700 focus:outline-none focus:ring-2 focus:ring-amber-400 resize-y"
              />
              <button
                type="submit"
                className="bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition"
              >
                Send Message
              </button>
            </form>
          </div>
        </div>

        {/* Map Section */}
        <div className="mt-12">
          {isBrowser && (
            <div className="rounded-2xl shadow-lg overflow-hidden border-2 border-amber-400 h-[300px] md:h-[500px]">
              <MapContainer
                center={position}
                zoom={17}
                scrollWheelZoom={!isMobile}
                zoomControl={false}
                className="h-full w-full"
              >
                <FixMapSize />
                <TileLayer
                  url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; <a href="https://carto.com/">CARTO</a>'
                />
                <Marker position={position} icon={redIcon}>
                  <Tooltip permanent direction="top" offset={[0, -10]} opacity={1}>
                    <span className="font-semibold text-amber-600">
                      Qalibrated Systems Limited
                    </span>
                  </Tooltip>
                  <Popup>
                    <div className="p-3 rounded-lg shadow-md border border-gray-200 bg-white">
                      <h3 className="text-amber-500 font-bold mb-1">
                        📍 Qalibrated Systems Limited
                      </h3>
                      <p className="text-gray-700 text-sm mb-3">
                        QSL Centre, Mombasa Road, Nairobi
                      </p>
                      <a
                        href={`https://www.google.com/maps/dir/?api=1&destination=${position[0]},${position[1]}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-block bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold py-1 px-3 rounded shadow transition"
                      >
                        🚗 Navigate
                      </a>
                    </div>
                  </Popup>
                </Marker>
                <ZoomButtons />
              </MapContainer>
            </div>
          )}
        </div>
      </section>

      {/* Scroll to top button */}
      {isBrowser && (
        <button
          className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg transition"
          onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        >
          <ArrowUp className="w-6 h-6" />
        </button>
      )}

      <Footer />

      {/* CSS for pin bounce */}
      <style jsx>{`
        .marker-bounce {
          animation: bounce 1.5s infinite;
        }
        @keyframes bounce {
          0%, 20%, 50%, 80%, 100% { transform: translateY(0); }
          40% { transform: translateY(-8px); }
          60% { transform: translateY(-4px); }
        }
      `}</style>
    </div>
  );
}
