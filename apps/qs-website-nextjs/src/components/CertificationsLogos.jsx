'use client';

import React from 'react';
import { MessageSquare } from 'lucide-react'; 

// ✅ Manual imports — no curly braces in logos array
import Bamburi from '@/assets/partnerlogo/Bamburi_Cement_Logo.png';

import Camea from '@/assets/partnerlogo/camea_logo.jpeg';  
import Kenha from '@/assets/partnerlogo/kenha.png';

import Giropes from '@/assets/partnerlogo/giropes.jpg';  
import Nairobi from '@/assets/partnerlogo/nairobicountylogo.png';
import Kajiado from '@/assets/partnerlogo/kajiado_county.jpg';
import Kilifi from '@/assets/partnerlogo/kilificounty.png';
import Kisumu from '@/assets/partnerlogo/countygovernmntofkisumu.jpg';
import Sensocar from '@/assets/hero/sensocar.svg';

const CertificationsLogos = () => {
  const logos = [
    { name: 'Bamburi', imageUrl: Bamburi },
    
    { name: 'Camea', imageUrl: Camea },
    { name: 'KeNHA', imageUrl: Kenha },
    
    { name: 'Giropes', imageUrl: Giropes },
    { name: 'Nairobi County', imageUrl: Nairobi },
    { name: 'Kajiado County', imageUrl: Kajiado },
    { name: 'Kilifi County', imageUrl: Kilifi },
    { name: 'Kisumu County', imageUrl: Kisumu },
    { name: 'Sensocar', imageUrl: Sensocar },
  ];

  const duplicatedLogos = [...logos, ...logos]; // for infinite scroll effect

  return (
    <section className="bg-white py-8 px-4 shadow-inner overflow-hidden relative">
      <div className="relative w-full min-h-[80px] flex items-center">
        <div className="flex flex-nowrap items-center animate-scroll-logos">
          {duplicatedLogos.map((logo, index) => (
            <div key={index} className="flex-shrink-0 p-3 mx-4">
              <img 
                src={typeof logo.imageUrl === 'string' ? logo.imageUrl : logo.imageUrl.src || logo.imageUrl} 
                alt={logo.name} 
                className="h-12 object-contain filter hover:grayscale-0 transition-all duration-300" 
                onError={(e) => { 
                  e.target.onerror = null; 
                  e.target.src="https://placehold.co/100x50/CCCCCC/000000?text=Error"; 
                }} 
              />
            </div>
          ))}
        </div>
      </div>

      {/* WhatsApp Floating Icon */}
      

      {/* Animation */}
      <style>
        {`
        @keyframes scroll-logos {
          0% { transform: translateX(0); }
          100% { transform: translateX(-50%); }
        }
        .animate-scroll-logos {
          animation: scroll-logos 30s linear infinite;
          white-space: nowrap;
        }
        `}
      </style>
    </section>
  );
};

export default CertificationsLogos;
