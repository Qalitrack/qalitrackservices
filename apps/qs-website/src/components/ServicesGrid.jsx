import React from 'react';
import { Scale, Building, Car, ArrowRight } from 'lucide-react'; // Only import necessary icons

const ServicesGrid = () => { // Renamed component from HomeServices to ServicesGrid
  // Select a few key services to highlight on the homepage
  const featuredServices = [
    {
      icon: Scale,
      title: "Commercial Weighing Solutions",
      description: "Comprehensive weighing systems ensuring accuracy and reliability for various commercial applications.",
      link: "/services" // Link to the full services page
    },
    {
      icon: Building,
      title: "Building Management System",
      description: "Integrated solutions for efficient monitoring and control of building infrastructure and operations.",
      link: "/services"
    },
    {
      icon: Car,
      title: "Intelligent Transport System",
      description: "Advanced systems for managing and optimizing transport logistics and vehicle movement.",
      link: "/services"
    },
  ];

  return (
    <section className="bg-gray-50 py-16">
      <div className="container mx-auto px-4 text-center">
        <h2 className="text-amber-500 text-lg font-semibold mb-2">What We Offer</h2>
        <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">Our Core Services</h3>
        <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
          Qalibrated Systems Limited provides advanced integrated solutions in weighing, automation, and transport systems, tailored to meet diverse industry needs.
        </p>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {featuredServices.map((service, index) => (
            <div 
              key={index} 
              className="bg-white p-8 rounded-lg shadow-md border border-gray-200 flex flex-col items-center text-center transition duration-200 hover:shadow-xl hover:scale-[1.02]"
            >
              <div className="bg-amber-100 rounded-full p-4 mb-6">
                <service.icon className="h-12 w-12 text-amber-500" />
              </div>
              <h4 className="font-bold text-xl mb-2">{service.title}</h4>
              <p className="text-gray-600 text-sm mb-6 flex-grow">{service.description}</p>
              <a 
                href={service.link} 
                className="text-amber-500 font-semibold hover:underline flex items-center"
              >
                Learn More <ArrowRight className="h-4 w-4 ml-1" />
              </a>
            </div>
          ))}
        </div>

        <div className="mt-12">
          <a 
            href="/services" 
            className="inline-block bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200"
          >
            View All Services
          </a>
        </div>
      </div>
    </section>
  );
}

export default ServicesGrid; // Exported with the new name
