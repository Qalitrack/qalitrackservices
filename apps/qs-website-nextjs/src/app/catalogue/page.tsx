'use client';

import React, { useState } from 'react';
import {
  ArrowUp, Download, CheckCircle, Truck, Settings, Factory, ShieldCheck, X
} from 'lucide-react';
import Image from 'next/image';

import Navbar from '@/components/Navbar'; 
import Footer from '@/components/Footer';
import CatalogueImage from '@/assets/catalogbg.jpg';

// Data for weighbridge models (simplified)
const models = [
  { code: "WB-TX-S-3X8-60T.6L", dimension: "3X8", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X10-60T.6L", dimension: "3X10", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X12-60T.6L", dimension: "3X12", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X14-60T.8L", dimension: "3X14", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X16-60T.8L", dimension: "3X16", capacity: "60", division: "30" },
  { code: "WB-TX-M-3X12-80T.6L", dimension: "3X12", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X14-80T.8L", dimension: "3X14", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X16-80T.8L", dimension: "3X16", capacity: "80", division: "50" },
  { code: "WB-TX-L-3X16-100T.8L", dimension: "3X16", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X18-100T.8L", dimension: "3X18", capacity: "100", division: "50" },
];

// Data for Advanced Technology details
const advancedTechData = [
  {
    icon: CheckCircle,
    title: "High Quality & Accuracy Load Cells",
    description: "Our truck scales operate in harsh conditions with load cells designed to withstand major forces and provide exact measurements.",
  },
  {
    icon: Truck,
    title: "Superior Scale Performance",
    description: "Built for durability and precision in all weather conditions with standardized outputs for consistent accuracy.",
  },
  {
    icon: Settings,
    title: "Advanced Weighing Technology",
    description: "State-of-the-art digital technology ensures reliable performance and easy maintenance for long-term operation.",
  },
  {
    icon: Factory,
    title: "Industrial Grade Construction",
    description: "Robust construction designed for heavy industrial use with reinforced steel and protective coatings.",
  },
  {
    icon: ShieldCheck,
    title: "Certified Compliance",
    description: "All systems meet international standards and regulations for commercial weighing applications.",
  },
];

export default function Catalogue() {
  const [showModal, setShowModal] = useState(false);
  const [selectedTech, setSelectedTech] = useState<typeof advancedTechData[0] | null>(null);

  const openTechModal = (tech: typeof advancedTechData[0]) => {
    setSelectedTech(tech);
    setShowModal(true);
  };

  const closeTechModal = () => {
    setShowModal(false);
    setSelectedTech(null);
  };

  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
      <Navbar />

      {/* Hero Section */}
      <section 
        className="relative bg-center text-white py-20 md:py-32" 
        style={{ 
          backgroundImage: `url(${CatalogueImage.src})`,
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Product Catalogue</h1>
          <p className="text-lg md:text-xl">Home / <span className="text-amber-400">Catalogue</span></p>
        </div>
      </section>

      {/* Catalogue Introduction */}
      <section className="bg-white py-16">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Our Products</h2>
          <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">
            Comprehensive Weighing Solutions
          </h3>
          <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
            Explore our extensive range of high-quality weighing equipment and solutions designed for various industrial and commercial applications.
          </p>
          <button className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-3 px-8 rounded-lg shadow-lg transition duration-200 inline-flex items-center gap-2">
            <Download className="w-5 h-5" />
            Download Full Catalogue
          </button>
        </div>
      </section>

      {/* Weighbridge Models Table */}
      <section className="bg-gray-50 py-16">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-12">Weighbridge Models</h2>
          <div className="bg-white rounded-lg shadow-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-amber-500 text-white">
                  <tr>
                    <th className="px-6 py-4 text-left font-semibold">Model Code</th>
                    <th className="px-6 py-4 text-left font-semibold">Dimension (m)</th>
                    <th className="px-6 py-4 text-left font-semibold">Capacity (T)</th>
                    <th className="px-6 py-4 text-left font-semibold">Division (kg)</th>
                  </tr>
                </thead>
                <tbody>
                  {models.map((model, index) => (
                    <tr key={index} className={index % 2 === 0 ? "bg-gray-50" : "bg-white"}>
                      <td className="px-6 py-4 font-mono text-sm">{model.code}</td>
                      <td className="px-6 py-4">{model.dimension}</td>
                      <td className="px-6 py-4">{model.capacity}</td>
                      <td className="px-6 py-4">{model.division}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </section>

      {/* Advanced Technology Section */}
      <section className="bg-white py-16">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-2">Advanced Technology</h2>
          <p className="text-gray-600 text-center mb-12 max-w-2xl mx-auto">
            Our weighing solutions incorporate cutting-edge technology for superior performance and reliability.
          </p>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {advancedTechData.map((tech, index) => (
              <div 
                key={index}
                className="bg-gray-50 p-6 rounded-lg shadow-md border border-gray-200 cursor-pointer hover:shadow-xl hover:scale-105 transition duration-300"
                onClick={() => openTechModal(tech)}
              >
                <div className="flex items-center mb-4">
                  <div className="bg-amber-100 p-3 rounded-full mr-4">
                    <tech.icon className="h-8 w-8 text-amber-500" />
                  </div>
                  <h4 className="font-bold text-lg">{tech.title}</h4>
                </div>
                <p className="text-gray-600 text-sm mb-4">{tech.description}</p>
                <button className="text-amber-500 font-semibold hover:underline text-sm">
                  Learn More →
                </button>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Call to Action */}
      <section className="bg-amber-500 py-16">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-white text-3xl font-bold mb-4">
            Need a Custom Solution?
          </h2>
          <p className="text-white mb-8 max-w-2xl mx-auto">
            Contact our experts to discuss your specific weighing requirements and get a tailored solution for your business.
          </p>
          <button className="bg-white text-amber-500 font-semibold py-3 px-8 rounded-lg shadow-lg hover:bg-gray-100 transition duration-200">
            Contact Our Experts
          </button>
        </div>
      </section>

      {/* Technology Detail Modal */}
      {showModal && selectedTech && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 relative">
            <button 
              onClick={closeTechModal} 
              className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
            >
              <X className="h-6 w-6" />
            </button>
            
            <div className="flex items-center mb-4">
              <div className="bg-amber-100 p-3 rounded-full mr-4">
                <selectedTech.icon className="h-8 w-8 text-amber-500" />
              </div>
              <h3 className="text-2xl font-bold">{selectedTech.title}</h3>
            </div>
            
            <p className="text-gray-700 leading-relaxed mb-6">{selectedTech.description}</p>
            
            <button 
              onClick={closeTechModal} 
              className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-2 px-4 rounded-lg transition duration-200"
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* Scroll to top button */}
      <button
        className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg transition duration-300 focus:outline-none focus:ring-2 focus:ring-amber-400"
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
      >
        <ArrowUp className="w-6 h-6" />
      </button>

      <Footer />
    </div>
  );
}