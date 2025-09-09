import React from 'react';
import { Award, Users, Clock, ArrowRight, MapPin, MailOpen, PhoneCall } from 'lucide-react'; // Only import necessary icons
import aboutImage from '../assets/team.jpg'; // Ensure you have an image in the specified path

const AboutSnapshot = () => {
  return (
    <section className="container bg-white
     mx-auto px-4 my-16 text-center"> {/* Added text-center to center content */}
      <h2 className="text-4xl font-bold text-center mb-12 text-amber-400">About Us</h2> {/* Moved outside, changed to text-2xl, added mb-8 */}
      
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
        {/* Left Column - Image */}
        <div className="flex justify-center items-center">
          <img 
            src={aboutImage} 
            alt="About Us Image" 
            className="rounded-lg shadow-xl w-full h-auto object-cover max-h-[450px]" 
          />
          
        </div>

        {/* Right Column - Text Content */}
        <div className="text-center lg:text-left"> {/* Kept text-center for small screens, lg:text-left for larger screens */}
          <h3 className="text-4xl font-bold leading-tight mb-6">Meet Our Company and Discover Our Commitment to Precision</h3>
          
          <p className="text-gray-600 mb-6 leading-relaxed">
            <strong>Qalibrated Systems Limited (QSL) </strong>is a Kenyan-registered private limited company with 100% local shareholding. Originally established in 2009 as Resolution Electro-Technique, our company has grown into a market leader in advanced weighing systems, calibrations, and industrial automation. Since 2017, we've expanded into construction, roadworks, water and sewerage, and mechanical and electrical engineering.
          </p>
          <p className="text-gray-600 mb-8 leading-relaxed">
            We pride ourselves on our team of well-trained experts committed to delivering tailored solutions that directly address industry demands, ensuring accuracy, reliability, and local support.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <div className="bg-gray-100 p-4 rounded-md shadow-sm flex items-center space-x-3 text-left">
              <Award className="h-6 w-6 text-amber-500 flex-shrink-0" />
              <div>
                <h4 className="font-bold text-lg">Market Leader</h4>
                <p className="text-gray-700 text-sm">Leading in integrated weighing solutions.</p>
              </div>
            </div>
            <div className="bg-gray-100 p-4 rounded-md shadow-sm flex items-center space-x-3 text-left">
              <Users className="h-6 w-6 text-amber-500 flex-shrink-0" />
              <div>
                <h4 className="font-bold text-lg">Expert Team</h4>
                <p className="text-gray-700 text-sm">Well-trained professionals understanding market needs.</p>
              </div>
            </div>
            {/* <div className="bg-gray-100 p-4 rounded-md shadow-sm flex items-center space-x-3 text-left">
              <Clock className="h-6 w-6 text-amber-500 flex-shrink-0" />
              <div>
                <h4 className="font-bold text-lg">24/7 Support</h4>
                <p className="text-gray-700 text-sm">Always online to provide professional assistance.</p>
              </div>
            </div> */}
          </div>
        </div>
      </div>

      <a 
        href="/about" 
        className="inline-block bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200 mt-12" // Added mt-12 for spacing
      >
        Learn More About Us <ArrowRight className="h-4 w-4 ml- inline-block" />
      </a>
    </section>
  );
}

export default AboutSnapshot;
