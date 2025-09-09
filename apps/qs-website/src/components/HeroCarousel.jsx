import React, { useState, useEffect, useRef } from 'react';
import {
  PlayCircle,
  Facebook,
  Linkedin,
  ChevronLeft,
  ChevronRight,
  X
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import IntelligentTransport from '../assets/hero/trans.jpg';
import ExpertCalibration from '../assets/hero/calibration.png';
import ResearchAndDevelopment from '../assets/hero/rnd34.jpeg';
import ElectronicCargoTracking from '../assets/hero/cargotracking.jpeg';
import AdvancedWeighbridge from '../assets/hero/weighing.png';
import { Link } from 'react-router-dom';
import AboutVideo from '../assets/hero/qslvideo.mp4';

const slides = [
  {
    imageUrl: `${AdvancedWeighbridge}`,
    subheading: "Precision, Power, & Performance",
    heading: "Advanced Weighbridge Solutions ",
  },
  {
    imageUrl: `${IntelligentTransport}`,
    subheading: "Optimizing Logistics",
    heading: "Intelligent Transport Systems ",
    paragraph:
      "Our Intelligent Transport Systems provide advanced management and optimization of transport logistics and vehicle movement, enhancing overall efficiency.",
  },
  {
    imageUrl: `${ExpertCalibration}`,
    heading: "Expert Calibration Services ",
    paragraph:
      "We offer expert calibration services to ensure the highest accuracy and compliance of all weighing instruments, providing you with reliable data.",
  },
  {
    imageUrl: `${ElectronicCargoTracking}`,
    subheading: "Securing Your Supply Chain",
    heading: "Electronic Cargo Tracking ",
    paragraph:
      "Our Electronic Cargo Tracking System offers comprehensive real-time monitoring of transit cargo, securing your supply chain from start to finish.",
  },
  {
    imageUrl: `${ResearchAndDevelopment}`,
    subheading: "Innovation at Core",
    heading: "Research & Development ",
    paragraph:
      "We are continuously innovating and developing new technologies to meet emerging industry demands, ensuring you stay ahead with cutting-edge solutions.",
  },
];

function HeroCarousel() {
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0);
  const [openVideo, setOpenVideo] = useState(false);
  const videoRef = useRef(null);

  const goToNextSlide = () => {
    setCurrentSlideIndex((prevIndex) => (prevIndex + 1) % slides.length);
  };

  const goToPreviousSlide = () => {
    setCurrentSlideIndex((prevIndex) => (prevIndex - 1 + slides.length) % slides.length);
  };

  // carousel auto-cycle (keeps previous behaviour)
  useEffect(() => {
    const interval = setInterval(() => {
      goToNextSlide();
    }, 5000);
    return () => clearInterval(interval);
  }, [currentSlideIndex]);

  // try autoplay when modal opens (may be blocked by browser autoplay policies)
  useEffect(() => {
    if (openVideo && videoRef.current) {
      videoRef.current.play().catch(() => {
        /* autoplay blocked — user can press play */
      });
    }
  }, [openVideo]);

  // central close handler: pause + reset then close
  const handleCloseVideo = () => {
    if (videoRef.current) {
      try {
        videoRef.current.pause();
        videoRef.current.currentTime = 0;
      } catch (err) {
        // ignore any errors
      }
    }
    setOpenVideo(false);
  };

  // close on Escape key
  useEffect(() => {
    const onKey = (e) => {
      if (e.key === 'Escape' && openVideo) handleCloseVideo();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [openVideo]);

  const currentSlide = slides[currentSlideIndex];

  return (
    <>
      <section
        className="relative h-[100vh] bg-cover bg-center flex items-center justify-center text-white transition-all duration-500 ease-in-out my-10"
        style={{ backgroundImage: `url(${currentSlide.imageUrl})` }}
      >
        <div className="absolute inset-0 bg-black opacity-50" />

        {/* Left Arrow */}
        <button
          className="absolute left-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 p-3 rounded-full hover:bg-opacity-75 transition z-20"
          onClick={goToPreviousSlide}
          aria-label="Previous slide"
        >
          <ChevronLeft size={24} />
        </button>

        {/* Right Arrow */}
        <button
          className="absolute right-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 p-3 rounded-full hover:bg-opacity-75 transition z-20"
          onClick={goToNextSlide}
          aria-label="Next slide"
        >
          <ChevronRight size={24} />
        </button>

        <div className="relative z-10 max-w-4xl text-center px-4">
          {currentSlide.subheading && (
            <h3 className="text-amber-400 font-semibold mb-2 text-3xl"
              dangerouslySetInnerHTML={{ __html: currentSlide.subheading }} />
          )}
          <h1 className="text-4xl md:text-6xl font-bold leading-tight mb-4"
            dangerouslySetInnerHTML={{ __html: currentSlide.heading }} />
          {currentSlide.paragraph && (
            <p className="text-gray-200 mb-6" dangerouslySetInnerHTML={{ __html: currentSlide.paragraph }} />
          )}

          <div className="flex flex-col sm:flex-row justify-center gap-4 mb-6">
            <button
              onClick={() => setOpenVideo(true)}
              className="flex items-center gap-2 bg-white text-black font-medium px-6 py-3 rounded-full hover:bg-gray-200 transition"
            >
              <PlayCircle className="text-amber-400" size={18} /> Watch Video
            </button>

            <Link to="/services" className="bg-amber-400 text-black px-6 py-3 rounded-full font-medium hover:opacity-90 transition">
              Learn More
            </Link>
          </div>

          <div className="text-sm font-semibold">Follow Us:</div>
          <div className="flex justify-center gap-4 mt-2 text-lg">
            <a href="https://www.facebook.com/profile.php?id=61573224881488" target="_blank" rel="noreferrer" className="p-2 bg-white text-black rounded-full hover:bg-gray-100">
              <Facebook size={18} />
            </a>
            <a href="https://www.linkedin.com/company/qalibrated-systems-limited" target="_blank" rel="noreferrer" className="p-2 bg-white text-black rounded-full hover:bg-gray-100">
              <Linkedin size={18} />
            </a>
          </div>
        </div>
      </section>

      {/* Video Modal (animated) */}
      <AnimatePresence>
        {openVideo && (
          <motion.div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleCloseVideo} // close when clicking outside inner content
          >
            <motion.div
              className="relative bg-white rounded-2xl shadow-lg w-full max-w-3xl"
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              transition={{ duration: 0.25 }}
              onClick={(e) => e.stopPropagation()} // prevent overlay click from closing when clicking inside modal
            >
              {/* Quit Watching button — placed above everything with high z */}
              <button
                onClick={handleCloseVideo}
                aria-label="Quit watching"
                className="absolute top-3 right-3 z-50 inline-flex items-center gap-2 bg-red-600 text-white px-3 py-2 rounded-full hover:bg-red-700 focus:outline-none"
              >
                <X size={16} /> Quit
              </button>

              {/* Video */}
              <div className="w-full">
                <video
                  ref={videoRef}
                  src={AboutVideo}
                  controls
                  autoPlay
                  controlsList="nodownload noplaybackrate"
                  disablePictureInPicture
                  onContextMenu={(e) => e.preventDefault()}
                  onEnded={handleCloseVideo}
                  className="w-full h-auto rounded-b-2xl"
                />
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}

export default HeroCarousel;
