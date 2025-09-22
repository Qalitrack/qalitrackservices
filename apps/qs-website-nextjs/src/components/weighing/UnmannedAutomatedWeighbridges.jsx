'use client';

import React, { useState, useEffect } from 'react';
import Header from '../Navbar';
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

const UnmannedAutomatedWeighbridgesPage = () => {
  const [activeNavItem, setActiveNavItem] = useState('/weighing/unmanned-automated');

  useEffect(() => {
    setActiveNavItem(window.location.pathname);
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 font-sans text-gray-800">
      <Header /> {/* Fixed Header */}

      {/* Hero Section */}
      <section 
        className="relative bg-cover bg-center text-white py-20 md:py-32 pt-32" // pt-32 to account for fixed header
        style={{ 
          backgroundImage: "url('/weighing.png')", // Placeholder image for automation
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Unmanned/Automated Weighbridges</h1>
          <p className="text-lg md:text-xl">Home / Weighing / <span className="text-amber-400">Unmanned/Automated Weighbridges</span></p>
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
            src="https://images.unsplash.com/photo-1596526131078-0c381c521133?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" // Image from PPT Page 5 (Static Weighbridges)
            alt="Unmanned/Automated Weighbridges" 
            className="w-full h-auto rounded-lg mb-8"
          /> */}

          {/* Description Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Unmanned and automated weighbridges offer a highly efficient and secure solution for vehicle weighing without the need for an operator. These systems leverage advanced technologies like RFID, cameras, barriers, and traffic lights to fully automate the weighing process, from vehicle identification to data recording.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            This automation minimizes human error, speeds up operations, and provides seamless integration into existing management systems, making them ideal for high-traffic environments and 24/7 operations.
          </p>

          {/* Features Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Possibility of unattended operation</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Automatic Vehicle Identification (RFID/Contactless Mifare card)</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Integrated with cameras for security and number plate reading</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Automated barriers and traffic lights for vehicle control</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Real-time data transfer to management systems</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Reduced operational costs and increased efficiency</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Elimination of human error in data entry</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default UnmannedAutomatedWeighbridgesPage;
