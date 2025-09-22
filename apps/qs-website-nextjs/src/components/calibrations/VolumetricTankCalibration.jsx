'use client';

import React, { useState, useEffect } from 'react';
import Navbar from '../Navbar'; // Using Navbar as per your latest instruction
import Footer from '../Footer';
// import Scales from '/public/calibration.jpg';





// Data for the sidebar navigation (consistent across calibration sub-pages)
const calibrationNavItems = [
  { name: 'Volumetric Tank Calibration', path: '/calibration/volumetric-tank' },
  { name: 'Multideck/Single Deck Calibration', path: '/calibration/multideck-singledeck' },
  { name: 'Flow Meters/Pressure Calibrations', path: '/calibration/flow-pressure' },
];

const VolumetricTankCalibrationPage = () => {
  const [activeNavItem, setActiveNavItem] = useState('/calibration/volumetric-tank');

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
          backgroundImage: "url('/calibration.jpeg')", // Placeholder image
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Volumetric Tank Calibration</h1>
          <p className="text-lg md:text-xl">Home / Calibration / <span className="text-amber-400">Volumetric Tank Calibration</span></p>
        </div>
      </section>

      {/* Main Content Area */}
      <main className="container mx-auto px-4 py-12 flex flex-col lg:flex-row gap-8">
        {/* Left Sidebar Navigation */}
        <aside className="w-full lg:w-1/4 bg-white rounded-lg shadow-md p-6 h-fit">
          <h3 className="text-xl font-bold text-gray-800 mb-4">CALIBRATION SERVICES</h3>
          <nav>
            <ul>
              {calibrationNavItems.map((item, index) => (
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
          

          {/* Description Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Volumetric tank calibration is essential for accurate measurement of liquid volumes in storage tanks. This process ensures that the tank's capacity is precisely known, which is critical for inventory management, legal-for-trade transactions, and regulatory compliance in industries such as oil & gas, chemicals, and food & beverage.
          </p>
          <p className="text-gray-700 leading-relaxed mb-8">
            Our calibration services utilize advanced techniques and certified equipment to provide highly accurate and reliable results, minimizing discrepancies and optimizing operational efficiency.
          </p>

          {/* Features Section */}
          <h2 className="text-3xl font-bold text-gray-800 mb-4">FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Accurate volume determination for various tank shapes</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Compliance with international standards</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Detailed calibration certificates provided</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>On-site calibration services available</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Reduces product loss and improves inventory control</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default VolumetricTankCalibrationPage;
