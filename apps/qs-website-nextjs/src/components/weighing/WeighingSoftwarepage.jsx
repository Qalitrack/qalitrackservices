'use client';

import React, { useState, useEffect } from 'react';
import Navbar from '../Navbar'; // Using Navbar as per your latest instruction
import Footer from '../Footer';
import Scales from '@/assets/scales.jpg';

// Data for the sidebar navigation
const weighingNavItems = [
  { name: 'Multideck/Singledeck Weighbridges', path: '/weighing/multideck-singledeck' },
  { name: 'Portable/Axle Weighers', path: '/weighing/portable-axle' },
  { name: 'Weighing Software', path: '/weighing/software' },
  { name: 'Unmanned/Automated Weighbridges', path: '/weighing/unmanned-automated' },
  { name: 'Retail Scales', path: '/weighing/retail-scales' },
  { name: 'Onboard Weighing', path: '/weighing/onboard-weighing' },
  { name: 'Weighbridge Accessories', path: '/weighing/accessories' },
];

const MultideckSingledecksPage = () => {
  // State to manage the active sidebar item
  const [activeNavItem, setActiveNavItem] = useState('/weighing/multideck-singledeck');

  // Effect to set active nav item based on current path
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
          backgroundImage: `url(${Scales})`, // Placeholder image
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Multideck/ Singledecks</h1>
          <p className="text-lg md:text-xl">Home / Weighing / <span className="text-amber-400">Multideck/Singledecks</span></p>
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
            <a 
              href="../assets/qsl-catalogue (1).pdf" // Placeholder path
              target="_blank" 
              rel="noopener noreferrer" 
              className="block w-full bg-amber-500 text-white text-center py-3 rounded-lg hover:bg-amber-600 transition duration-200 mb-3"
            >
              DOWNLOAD BROCHURE
            </a>
            <a 
              href="../assets/QSL-CORPORATE-PPT-UPDATED2-2.pdf" // Placeholder path
              target="_blank" 
              rel="noopener noreferrer" 
              className="block w-full bg-gray-700 text-white text-center py-3 rounded-lg hover:bg-gray-800 transition duration-200"
            >
              DOWNLOAD BROCHURE
            </a>
          </div>
        </aside>

        {/* Right Content Area */}
        <div className="w-full lg:w-3/4 bg-white rounded-lg shadow-md p-8">
          {/* Main Image for the page */}
          {/* <img 
            src="https://images.unsplash.com/photo-1544620023-e692a71562b7?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" // Image from screenshot or similar
            alt="Multideck Weighbridge" 
            className="w-full h-auto rounded-lg mb-8"
          /> */}

          {/* Description Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Our Multidecks are typically designed to obtain the total vehicle gross weight and group axle weights. It
            consists of four platforms that are typically individual scales, each with four load cells at the corner. The
            load cells are connected together into a junction box and then fed to the indicator.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            It provides all the weights that are required for regulatory compliance and are legal for trading within the
            stipulated country. The first deck is for the steering axle, second deck for the drive axle, the third deck is for
            the first trailer axle and the fourth deck is for the second trailer axle. The sum of the four decks is the total
            gross weight of the vehicle.
          </p>

          {/* Features Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Regulatory Compliant according to Traffic Act and EAC VLC</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Uses both analogue & Digital Load Cells</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>The indicator weights can be broadcasted through various communication protocols i.e RS232, RS485, TCP/IP</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Calibrated with standard weights</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Installed with Remote Display Units</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default MultideckSingledecksPage;
