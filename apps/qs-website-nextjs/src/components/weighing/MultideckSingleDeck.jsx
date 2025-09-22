'use client';

import React, { useState, useEffect } from 'react';
import Navbar from '../Navbar';
import Footer from '../Footer';


// Sidebar navigation items (consistent across weighing sub-pages)
const weighingNavItems = [
  // { name: 'Multideck/Singledeck Weighbridges', path: '/weighing/multideck-singledeck' },
  { name: 'Portable/Axle Weighers', path: '/weighing/portable-axle' },
  { name: 'Weighing Software', path: '/weighing/software' },
  { name: 'Unmanned/Automated Weighbridges', path: '/weighing/unmanned-automated' },
  { name: 'Retail Scales', path: '/weighing/retail-scales' },
  { name: 'Onboard Weighing', path: '/weighing/onboard-weighing' },
  { name: 'Weighbridge Accessories', path: '/weighing/accessories' },
];

const MultideckSingledecksPage = () => {
  const [activeNavItem, setActiveNavItem] = useState('/weighing/multideck-singledeck');

  useEffect(() => {
    setActiveNavItem(window.location.pathname);
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 font-sans text-gray-800">
      <Navbar />

      {/* Hero Section */}
      <section
        className="relative bg-cover bg-center text-white py-20 md:py-32 pt-32"
        style={{
          backgroundImage: "url('/weighing.png')",
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover',
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Multideck / Singledeck</h1>
          <p className="text-lg md:text-xl">
            Home / Weighing / <span className="text-amber-400">Multideck / Singledeck</span>
          </p>
        </div>
      </section>

      {/* Main Content Area */}
      <main className="container mx-auto px-4 py-12 flex flex-col lg:flex-row gap-8">
        {/* Sidebar */}
        <aside className="w-full lg:w-1/4 bg-white rounded-lg shadow-md p-6 h-fit">
          <h3 className="text-xl font-bold text-gray-800 mb-4">WEIGHING SOLUTIONS</h3>
          <nav>
            <ul>
              {weighingNavItems.map((item, index) => (
                <li key={index} className="mb-2">
                  <a
                    href={item.path}
                    className={`block py-3 px-4 rounded-md transition-colors duration-200 
                      ${activeNavItem === item.path
                        ? 'bg-amber-500 text-white font-semibold'
                        : 'text-gray-700 hover:bg-gray-100'}`}
                    onClick={() => setActiveNavItem(item.path)}
                  >
                    {item.name}
                  </a>
                </li>
              ))}
            </ul>
          </nav>

          {/* Download Brochure */}
          <div className="mt-8 pt-6 border-t border-gray-200">
            <h4 className="text-lg font-bold text-gray-800 mb-3">Download Brochure</h4>
            <p className="text-gray-600 text-sm mb-4">Here are some useful documents</p>
            <a
              href="/assets/qsl-catalogue (1).pdf"
              target="_blank"
              rel="noopener noreferrer"
              className="block w-full bg-amber-500 text-white text-center py-3 rounded-lg hover:bg-amber-600 transition duration-200 mb-3"
            >
              CATALOGUE PDF
            </a>
            <a
              href="/assets/QSL-CORPORATE-PPT-UPDATED2-2.pdf"
              target="_blank"
              rel="noopener noreferrer"
              className="block w-full bg-gray-700 text-white text-center py-3 rounded-lg hover:bg-gray-800 transition duration-200"
            >
              CORPORATE PROFILE
            </a>
          </div>
        </aside>

        {/* Right Content */}
        <div className="w-full h-auto lg:w-3/4 bg-white rounded-lg shadow-md p-8">
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Our Multidecks are typically designed to obtain the total vehicle gross weight and group axle weights. 
            They consist of four platforms that are individual scales, each with four load cells at the corners. 
            The load cells connect into a junction box, then feed to the indicator.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            It provides all the weights required for regulatory compliance and are legal for trading. 
            The first deck is for the steering axle, the second for the drive axle, the third for the first trailer axle, 
            and the fourth for the second trailer axle. The sum of all decks equals the total gross weight.
          </p>

          <h2 className="text-3xl font-bold text-gray-800 mb-4">FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Regulatory compliant according to Traffic Act and EAC VLC</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Supports both analogue & digital load cells</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Indicator weights broadcasted via RS232, RS485, TCP/IP</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Calibrated with standard weights</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Installed with remote display units</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default MultideckSingledecksPage;
