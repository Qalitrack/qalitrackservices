'use client';

import React, { useState, useEffect } from 'react';
import Navbar from '../Navbar'; // Using Navbar as per your latest instruction
import Footer from '../Footer';
// import Scales from '@/assets/team.jpg';


// Data for the sidebar navigation (consistent across automation sub-pages)
const automationNavItems = [
  { name: 'Building Management Systems', path: '/automation/building-management' },
  { name: 'Industrial Automation', path: '/automation/industrial' },
  { name: 'Intelligent Transport Systems', path: '/automation/intelligent-transport' },
];

const BuildingManagementSystemsPage = () => {
  const [activeNavItem, setActiveNavItem] = useState('/automation/building-management');

  useEffect(() => {
    setActiveNavItem(window.location.pathname);
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 font-sans text-gray-800">
      <Navbar /> {/* Fixed Navbar */}

      {/* Hero Section */}
      <section 
        className="relative bg-cover bg-center text-white py-20 md:py-32 pt-32" // pt-32 to account for fixed Navbar
        style={{ 
          backgroundImage: "url('/automation.jpeg')", // Placeholder image for BMS
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Building Management Systems</h1>
          <p className="text-lg md:text-xl">Home / Automation / <span className="text-amber-400">Building Management Systems</span></p>
        </div>
      </section>

      {/* Main Content Area */}
      <main className="container mx-auto px-4 py-12 flex flex-col lg:flex-row gap-8">
        {/* Left Sidebar Navigation */}
        <aside className="w-full lg:w-1/4 bg-white rounded-lg shadow-md p-6 h-fit">
          <h3 className="text-xl font-bold text-gray-800 mb-4">AUTOMATION SOLUTIONS</h3>
          <nav>
            <ul>
              {automationNavItems.map((item, index) => (
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
        <div className="w-full h-auto lg:w3/4 bg-white rounded-lg shadow-md p-8">
          {/* Main Image for the page */}
          {/* <img 
            src={BM} // Image from PPT Page 14 (Building Management Systems)
            alt="Building Management Systems" 
            className="w-full h-auto rounded-lg mb-8"
          /> */}

          {/* Description Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Building Management Systems (BMS) provide centralized control and monitoring of a building's mechanical and electrical equipment, including HVAC, lighting, security, fire systems, and access control. These integrated systems aim to optimize building performance, enhance occupant comfort, and reduce operational costs.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            Our BMS solutions offer intelligent automation, real-time data analysis, and remote management capabilities, ensuring efficient energy consumption, improved safety, and streamlined facility operations.
          </p>

          {/* Features Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Centralized control and monitoring of building systems</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>HVAC optimization for energy efficiency</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Integrated lighting control and scheduling</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Advanced security and access control</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Fire detection and alarm system integration</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Real-time data analytics and reporting</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Remote management and mobile access</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Improved occupant comfort and productivity</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default BuildingManagementSystemsPage;
