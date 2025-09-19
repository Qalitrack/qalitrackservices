import React from 'react';

// --- ContactCallToAction Component ---
// This component features a call to action with two buttons and a stylized background.
const ContactCallToAction = () => {
  return (
    <section className="relative bg-black text-white py-20 px-4 overflow-hidden  shadow-lg ">
      {/* Background SVG pattern - simple representation of the lines */}
      <svg className="absolute top-0 left-0 w-full h-full z-0 opacity-20" viewBox="0 0 100 100" preserveAspectRatio="none">
        {/* Left lines */}
        <path d="M-10 10 L30 50 L-10 90" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <path d="M-15 15 L25 55 L-15 95" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <path d="M-20 20 L20 60 L-20 100" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <circle cx="20" cy="40" r="1.5" fill="#fcd34d" />
        <circle cx="15" cy="45" r="1.5" fill="#fcd34d" />
        <circle cx="10" cy="50" r="1.5" fill="#fcd34d" />

        {/* Right lines */}
        <path d="M110 10 L70 50 L110 90" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <path d="M115 15 L75 55 L115 95" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <path d="M120 20 L80 60 L120 100" stroke="#fcd34d" strokeWidth="0.5" fill="none" />
        <circle cx="80" cy="40" r="1.5" fill="#fcd34d" />
        <circle cx="85" cy="45" r="1.5" fill="#fcd34d" />
        <circle cx="90" cy="50" r="1.5" fill="#fcd34d" />
      </svg>

      <div className="container mx-auto relative z-10 text-center">
        <h2 className="text-3xl md:text-4xl font-bold mb-8">Can't find what you are looking for?</h2>
        <div className="flex flex-col sm:flex-row justify-center items-center gap-4">
  <a
    href="https://wa.me/254756999996" // Replace with your WhatsApp number
    target="_blank"
    rel="noopener noreferrer"
    className="bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200 text-center"
  >
    Start a live chat
  </a>
  {/* <a
    href="/contact" // Adjust if your route is different
    className="bg-transparent border-2 border-amber-500 text-amber-500 hover:bg-amber-500 hover:text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200 text-center"
  >
    Or contact us
  </a> */}
</div>

      </div>
    </section>
  );
};

export default ContactCallToAction;
