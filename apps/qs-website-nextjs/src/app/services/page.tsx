'use client';

import React, { useState } from 'react';
import {ArrowUp,
  Quote, Star, X
} from 'lucide-react';
// import Image from 'next/image';

import Hero from '@/assets/portrait-engineers-work-hours-job-site.jpg';
import Footer from '@/components/Footer';
import Navbar from '@/components/Navbar';
import Newsletter from '@/components/ContactCallToAction';
import Partners from '@/components/CertificationsLogos';

// Data for services, including detailed descriptions
const servicesData = [
  {
    title: "Commercial Weighing Solutions",
    description: "Comprehensive weighing systems for various commercial applications, ensuring accuracy and reliability.",
    detailedDescription: "Qalibrated Systems offers a wide range of commercial weighing solutions, from retail scales to heavy-duty industrial weighbridges. Our solutions are designed for precision, durability, and compliance with industry standards, ensuring accurate measurements for your business operations. We provide installation, calibration, and ongoing maintenance.",
  },
  {
    title: "Building Management System",
    description: "Integrated solutions for efficient monitoring and control of building infrastructure and operations.",
    detailedDescription: "Our Building Management Systems (BMS) provide centralized control over your building's HVAC, lighting, security, and energy consumption. This leads to optimized operational efficiency, reduced energy costs, and enhanced comfort and safety for occupants. We design and implement custom BMS solutions for commercial and industrial buildings.",
  },
  {
    title: "Intelligent Transport System",
    description: "Advanced systems for managing and optimizing transport logistics and vehicle movement.",
    detailedDescription: "QSL's Intelligent Transport Systems (ITS) leverage cutting-edge technology for traffic management, vehicle tracking, and electronic toll collection. Our solutions improve road safety, reduce congestion, and enhance the overall efficiency of transportation networks, crucial for logistics and public infrastructure.",
  },
  {
    title: "Calibrations",
    description: "Expert calibration services to ensure the highest accuracy and compliance of weighing instruments.",
    detailedDescription: "Accurate calibration is fundamental for reliable weighing. Qalibrated Systems provides accredited calibration services for all types of weighing equipment, ensuring your instruments meet national and international standards. Regular calibration minimizes errors, ensures legal compliance, and maintains the integrity of your measurements.",
  },
  {
    title: "Industrial Automation",
    description: "Automated solutions to streamline industrial processes, enhancing efficiency and productivity.",
    detailedDescription: "Our industrial automation services focus on designing and implementing automated control systems for manufacturing and processing plants. This includes PLC programming, SCADA systems, robotics integration, and process optimization, leading to increased efficiency, reduced operational costs, and improved product quality.",
  },
  {
    title: "Research and Development",
    description: "Continuous innovation and development of new technologies to meet emerging industry demands.",
    detailedDescription: "Qalibrated Systems is committed to continuous innovation. Our R&D department explores new technologies and develops bespoke solutions to address unique industry challenges. We invest in research to stay at the forefront of weighing, automation, and transport technologies, offering future-proof solutions to our clients.",
  }
];

export default function Services() {
  const [showModal, setShowModal] = useState(false);
  const [selectedService, setSelectedService] = useState<typeof servicesData[0] | null>(null);

  const openServiceModal = (service: typeof servicesData[0]) => {
    setSelectedService(service);
    setShowModal(true);
  };

  const closeServiceModal = () => {
    setShowModal(false);
    setSelectedService(null);
  };

  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
      <Navbar />

      {/* Hero Section */}
      <section className="relative bg-cover bg-center text-white py-20 md:py-32" style={{ backgroundImage: `url(${Hero.src})`, backgroundAttachment: 'fixed' }}>
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Our Services</h1>
          <p className="text-lg md:text-xl">Home / <span className="text-amber-400">Services</span></p>
        </div>
      </section>

      {/* Our Services Section */}
      <section className="bg-white py-16">
        <div className="container mx-auto px-4 text-center">
          <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">We provide the best solutions for your business.</h3>
          <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
            Qalibrated Systems Limited offers a diverse portfolio of integrated weighing, automation, and transport system solutions. Our expertise ensures precision, efficiency, and reliability across various industrial and commercial sectors.
          </p>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
            {servicesData.map((service, index) => (
              <div 
                key={index} 
                className="bg-white rounded-lg shadow-md border border-gray-200 flex flex-col items-center text-center cursor-pointer transition duration-200 hover:shadow-xl hover:scale-[1.02]"
                onClick={() => openServiceModal(service)}
              >
                <div className="p-6">
                  <h4 className="font-bold text-xl mb-2">{service.title}</h4>
                  <p className="text-gray-600 text-sm mb-6">{service.description}</p>
                  <button className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-2 px-6 rounded-lg transition duration-200">Learn More</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Testimonial Section */}
      <section className="bg-gray-100 py-16">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Testimonial</h2>
          <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">Our Clients Reviews</h3>
          <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
            Hear directly from our satisfied clients about how Qalibrated Systems Limited has delivered exceptional value and precision solutions for their businesses.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {/* Testimonial Card 1 */}
            <div className="bg-white p-8 rounded-lg shadow-md flex flex-col items-center text-center relative">
              <Quote className="absolute top-4 left-4 h-10 w-10 text-amber-400 opacity-20" />
              <img src="https://placehold.co/80x80/E0F2F7/2D3748?text=Client+A" alt="Client A" className="rounded-full w-20 h-20 object-cover mb-4 border-2 border-amber-400" />
              <p className="text-gray-700 text-sm mb-4 italic">
                &quot;Qalibrated Systems provided us with an incredibly accurate weighing solution. Their team was professional and the installation was seamless. Highly recommend!&quot;
              </p>
              <div className="flex justify-center mb-2">
                {[...Array(5)].map((_, i) => (
                  <Star key={i} className="h-5 w-5 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <h4 className="font-bold text-lg mb-1">Jane Doe</h4>
              <p className="text-gray-600 text-sm">Logistics Manager</p>
            </div>

            {/* Testimonial Card 2 */}
            <div className="bg-white p-8 rounded-lg shadow-md flex flex-col items-center text-center relative">
              <Quote className="absolute top-4 left-4 h-10 w-10 text-amber-400 opacity-20" />
              <img src="https://placehold.co/80x80/E0F2F7/2D3748?text=Client+B" alt="Client B" className="rounded-full w-20 h-20 object-cover mb-4 border-2 border-amber-400" />
              <p className="text-gray-700 text-sm mb-4 italic">
                &quot;Their industrial automation expertise transformed our production line. We&apos;ve seen significant improvements in efficiency and reduced downtime.&quot;
              </p>
              <div className="flex justify-center mb-2">
                {[...Array(5)].map((_, i) => (
                  <Star key={i} className="h-5 w-5 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <h4 className="font-bold text-lg mb-1">John Smith</h4>
              <p className="text-gray-600 text-sm">Factory Owner</p>
            </div>

            {/* Testimonial Card 3 */}
            <div className="bg-white p-8 rounded-lg shadow-md flex flex-col items-center text-center relative">
              <Quote className="absolute top-4 left-4 h-10 w-10 text-amber-400 opacity-20" />
              <img src="https://placehold.co/80x80/E0F2F7/2D3748?text=Client+C" alt="Client C" className="rounded-full w-20 h-20 object-cover mb-4 border-2 border-amber-400" />
              <p className="text-gray-700 text-sm mb-4 italic">
                &quot;The 24/7 support from Qalibrated Systems is unmatched. Any issue, big or small, is addressed promptly and professionally. Truly a reliable partner.&quot;
              </p>
              <div className="flex justify-center mb-2">
                {[...Array(5)].map((_, i) => (
                  <Star key={i} className="h-5 w-5 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <h4 className="font-bold text-lg mb-1">Emily White</h4>
              <p className="text-gray-600 text-sm">Procurement Head</p>
            </div>
          </div>
        </div>
      </section>

      {/* Service Detail Modal */}
      {showModal && selectedService && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 relative">
            <button 
              onClick={closeServiceModal} 
              className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
            >
              <X className="h-6 w-6" />
            </button>
            <h3 className="text-3xl font-bold text-amber-600 mb-4">{selectedService.title}</h3>
            <p className="text-gray-700 leading-relaxed mb-4">{selectedService.detailedDescription}</p>
            <button 
              onClick={closeServiceModal} 
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

        <Newsletter/>
      
        <Partners />

      <Footer/>
    </div>
  );
}