'use client';

import React, { useState, useEffect } from 'react';
import Navbar from '../Navbar'; // Using Navbar as per your latest instruction
import Footer from '../Footer';
// import Scales from '@/assets/weighing.png';

// Data for the sidebar navigation (consistent across weighing sub-pages)
const weighingNavItems = [
  // { name: 'Multideck/Singledeck Weighbridges', path: '/weighing/multideck-singledeck' },
  { name: 'Portable/Axle Weighers', path: '/weighing/portable-axle' },
  { name: 'Weighing Software', path: '/weighing/software' },
  { name: 'Unmanned/Automated Weighbridges', path: '/weighing/unmanned-automated' },
  { name: 'Retail Scales', path: '/weighing/retail-scales' },
  { name: 'Onboard Weighing', path: '/weighing/onboard-weighing' },
  { name: 'Weighbridge Accessories', path: '/weighing/accessories' },
];

const WeighbridgeAccessoriesPage = () => {
  const [activeNavItem, setActiveNavItem] = useState('/weighing/accessories');

  useEffect(() => {
    setActiveNavItem(window.location.pathname);
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 font-sans text-gray-800">
      <Navbar /> {/* Used Navbar directly */}

      {/* Hero Section */}
      <section 
        className="relative bg-cover bg-center text-white py-20 md:py-32 pt-32" // pt-32 to account for fixed Navbar
        style={{ 
          backgroundImage: "url('/weighing.png')", // Placeholder image
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Weighbridge Accessories</h1>
          <p className="text-lg md:text-xl">Home / Weighing / <span className="text-amber-400">Weighbridge Accessories</span></p>
        </div>
      </section>

      {/* Main Content Area */}
      <main className="container mx-auto px-4 py-12 flex flex-col lg:flex-row gap-8">
        {/* Left Sidebar Navigation */}
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
                        : 'text-gray-700 hover:bg-gray-100'}`
                    }
                    onClick={() => setActiveNavItem(item.path)}
                  >
                    {item.name}
                  </a>
                </li>
              ))}
            </ul>
          </nav>

          {/* Download Brochure Section */}
          <div className="mt-8 pt-6 border-t border-gray-200">
            <h4 className="text-lg font-bold text-gray-800 mb-3">Download Brochure</h4>
            <p className="text-gray-600 text-sm mb-4">Here is some useful documents</p>
            <a href="../assets/qsl-catalogue (1).pdf" target="_blank" rel="noopener noreferrer" className="block w-full bg-amber-500 text-white text-center py-3 rounded-lg hover:bg-amber-600 transition duration-200 mb-3">DOWNLOAD BROCHURE</a>
            <a href="../assets/QSL-CORPORATE-PPT-UPDATED2-2.pdf" target="_blank" rel="noopener noreferrer" className="block w-full bg-gray-700 text-white text-center py-3 rounded-lg hover:bg-gray-800 transition duration-200">DOWNLOAD BROCHURE</a>
          </div>
        </aside>

        {/* Right Content Area */}
        <div className="w-full lg:w-3/4 bg-white rounded-lg shadow-md p-8">
          {/* Main Image for the page */}
          {/* <img 
            src="https://images.unsplash.com/photo-1582213782179-e0d53f9ea933?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" // Placeholder image for accessories
            alt="Weighbridge Accessories" 
            className="w-full h-auto rounded-lg mb-8"
          /> */}

          {/* Description Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Weighbridge accessories are crucial components that enhance the functionality, accuracy, and longevity of weighing systems. This includes digital load cells, indicators, signal cables, junction boxes, and specialized mounting kits, all designed to ensure reliable performance in demanding industrial environments.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            These accessories play a vital role in data acquisition, signal processing, and overall system integrity, contributing to precise measurements and reduced maintenance needs.
          </p>

          {/* Features Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">KEY ACCESSORIES & FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>High Quality & Accuracy Load Cells: IP68/IP69K protection, OIML R60 Class C3 certified, 150% safe load, lightning protection.</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Junction Box: IP67 High Protection Class, includes lightning protection, maintenance-free.</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>LoadGuard Special Design Mounting Kits: 100% steel construction, compensates side loading, eliminates check rods.</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Digital Weighing Indicators: Provide clear weight readings and integrate with software.</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Signal Cables: Durable and shielded for reliable data transmission.</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Remote Displays: For external weight observation.</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default WeighbridgeAccessoriesPage;
