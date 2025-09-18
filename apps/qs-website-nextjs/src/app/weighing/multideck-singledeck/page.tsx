'use client';

import React from 'react';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';

export default function MultideckSingledeckWeighbridges() {
  return (
    <div className="min-h-screen bg-gray-100 font-sans text-gray-800">
      <Navbar />
      
      {/* Hero Section */}
      <section className="relative bg-cover bg-center text-white py-20 md:py-32 pt-32">
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-2">Multideck/Singledeck Weighbridges</h1>
          <p className="text-lg md:text-xl">Home / Weighing / <span className="text-amber-400">Multideck/Singledeck Weighbridges</span></p>
        </div>
      </section>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-12">
        <div className="bg-white rounded-lg shadow-md p-8">
          <h2 className="text-3xl font-bold text-gray-800 mb-4">DESCRIPTION</h2>
          <p className="text-gray-700 leading-relaxed mb-6">
            Our multideck and singledeck weighbridges provide accurate and reliable weighing solutions for various industrial applications. 
            These systems are designed to handle heavy-duty operations while ensuring precision and durability.
          </p>
          
          <h2 className="text-3xl font-bold text-gray-800 mb-4">KEY FEATURES</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>High capacity weighing systems for industrial use</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Robust construction for long-term reliability</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Advanced load cell technology for accuracy</p>
            </div>
            <div className="flex items-start">
              <span className="text-amber-500 mr-2 text-xl">&raquo;</span>
              <p>Weather-resistant design for outdoor use</p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}