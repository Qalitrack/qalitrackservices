import React, { useState, useEffect, useRef } from "react";
import {
  PlayCircle,
  Facebook,
  Linkedin,
  ChevronLeft,
  ChevronRight,
  X,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { Link } from "react-router-dom";

// Assets (images)
import IntelligentTransport from "../assets/hero/trans.jpg";
import ExpertCalibration from "../assets/hero/calibration.png";
import ResearchAndDevelopment from "../assets/hero/rnd34.jpeg";
import ElectronicCargoTracking from "../assets/hero/cargotracking.jpeg";
import AdvancedWeighbridge from "../assets/hero/weighing.png";

// Video (lazy load: put it in `public/videos/`)
const VIDEO_PATH = "/videos/qslvideo.mp4";

const slides = [
  {
    imageUrl: AdvancedWeighbridge,
    subheading: "Precision, Power, & Performance",
    heading: "Advanced Weighbridge Solutions",
  },
  {
    imageUrl: IntelligentTransport,
    subheading: "Optimizing Logistics",
    heading: "Intelligent Transport Systems",
    paragraph:
      "Our Intelligent Transport Systems provide advanced management and optimization of transport logistics and vehicle movement, enhancing overall efficiency.",
  },
  {
    imageUrl: ExpertCalibration,
    heading: "Expert Calibration Services",
    paragraph:
      "We offer expert calibration services to ensure the highest accuracy and compliance of all weighing instruments, providing you with reliable data.",
  },
  {
    imageUrl: ElectronicCargoTracking,
    subheading: "Securing Your Supply Chain",
    heading: "Electronic Cargo Tracking",
    paragraph:
      "Our Electronic Cargo Tracking System offers comprehensive real-time monitoring of transit cargo, securing your supply chain from start to finish.",
  },
  {
    imageUrl: ResearchAndDevelopment,
    subheading: "Innovation at Core",
    heading: "Research & Development",
    paragraph:
      "We are continuously innovating and developing new technologies to meet emerging industry demands, ensuring you stay ahead with cutting-edge solutions.",
  },
];

function HeroCarousel() {
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0);
  const [openVideo, setOpenVideo] = useState(false);
  const [videoSrc, setVideoSrc] = useState(null);
  const videoRef = useRef(null);

  const goToNextSlide = () =>
    setCurrentSlideIndex((prev) => (prev + 1) % slides.length);
  const goToPreviousSlide = () =>
    setCurrentSlideIndex((prev) => (prev - 1 + slides.length) % slides.length);

  // autoplay every 6s
  useEffect(() => {
    const interval = setInterval(goToNextSlide, 6000);
    return () => clearInterval(interval);
  }, []);

  // ✅ Preload all images on mount
  useEffect(() => {
    slides.forEach((slide) => {
      const img = new Image();
      img.src = slide.imageUrl;
    });
  }, []);

  const handleOpenVideo = () => {
    setVideoSrc(VIDEO_PATH);
    setOpenVideo(true);
  };

  const handleCloseVideo = () => {
    if (videoRef.current) {
      videoRef.current.pause();
      videoRef.current.currentTime = 0;
    }
    setOpenVideo(false);
    setVideoSrc(null);
  };

  const zoomDirections = ["in", "out", "in", "out", "in"];
  const currentZoom = zoomDirections[currentSlideIndex % zoomDirections.length];
  const currentSlide = slides[currentSlideIndex];

  return (
    <>
      {/* Hero Section */}
      <section className="relative min-h-[70vh] sm:min-h-[80vh] lg:min-h-screen flex items-center justify-center text-white overflow-hidden">
        {/* Background with crossfade + Ken Burns */}
        <div className="absolute inset-0">
          <AnimatePresence mode="wait">
            <motion.img
              key={currentSlide.imageUrl}
              src={currentSlide.imageUrl}
              alt={currentSlide.heading}
              className="w-full h-full object-cover absolute inset-0"
              initial={{ opacity: 0, scale: currentZoom === "in" ? 1.1 : 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: currentZoom === "in" ? 1.1 : 0.9 }}
              transition={{ duration: 1.2, ease: "easeInOut" }}
            />
          </AnimatePresence>

          {/* Overlay for constant contrast */}
          <div className="absolute inset-0 bg-black/50 pointer-events-none" />
        </div>

        {/* Arrows */}
        <button
          onClick={goToPreviousSlide}
          aria-label="Previous slide"
          className="absolute left-3 sm:left-6 top-1/2 -translate-y-1/2 bg-black/50 p-2 sm:p-3 rounded-full hover:bg-black/70 transition z-20"
        >
          <ChevronLeft size={22} />
        </button>
        <button
          onClick={goToNextSlide}
          aria-label="Next slide"
          className="absolute right-3 sm:right-6 top-1/2 -translate-y-1/2 bg-black/50 p-2 sm:p-3 rounded-full hover:bg-black/70 transition z-20"
        >
          <ChevronRight size={22} />
        </button>

        {/* Content */}
        <div className="relative z-10 max-w-4xl text-center px-4">
          {currentSlide.subheading && (
            <h3
              className="text-amber-400 font-semibold mb-2 text-xl sm:text-2xl md:text-3xl"
              dangerouslySetInnerHTML={{ __html: currentSlide.subheading }}
            />
          )}
          <h1
            className="text-2xl sm:text-4xl md:text-6xl font-bold leading-tight mb-4"
            dangerouslySetInnerHTML={{ __html: currentSlide.heading }}
          />
          {currentSlide.paragraph && (
            <p
              className="text-gray-200 mb-6 max-w-2xl mx-auto text-sm sm:text-base md:text-lg"
              dangerouslySetInnerHTML={{ __html: currentSlide.paragraph }}
            />
          )}

          {/* Buttons */}
          <div className="flex flex-col sm:flex-row justify-center gap-3 sm:gap-4 mb-6">
            <button
              onClick={handleOpenVideo}
              className="flex items-center justify-center gap-2 bg-white text-black font-medium px-5 sm:px-6 py-2 sm:py-3 rounded-full hover:bg-gray-200 transition text-sm sm:text-base"
            >
              <PlayCircle className="text-amber-400" size={18} /> Watch Video
            </button>
            <Link
              to="/services"
              className="bg-amber-400 text-black px-5 sm:px-6 py-2 sm:py-3 rounded-full font-medium hover:opacity-90 transition text-sm sm:text-base"
            >
              Learn More
            </Link>
          </div>

          {/* Social Links */}
          <div className="text-xs sm:text-sm font-semibold">Follow Us:</div>
          <div className="flex justify-center gap-3 mt-2 text-base sm:text-lg">
            <a
              href="https://www.facebook.com/profile.php?id=61573224881488"
              target="_blank"
              rel="noreferrer"
              className="p-2 bg-white text-black rounded-full hover:bg-gray-100"
            >
              <Facebook size={18} />
            </a>
            <a
              href="https://www.linkedin.com/company/qalibrated-systems-limited"
              target="_blank"
              rel="noreferrer"
              className="p-2 bg-white text-black rounded-full hover:bg-gray-100"
            >
              <Linkedin size={18} />
            </a>
          </div>
        </div>
      </section>

      {/* Video Modal */}
      <AnimatePresence>
        {openVideo && videoSrc && (
          <motion.div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 px-3"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleCloseVideo}
          >
            <motion.div
              className="relative bg-white rounded-2xl shadow-lg w-full max-w-[95%] sm:max-w-3xl"
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              transition={{ duration: 0.25 }}
              onClick={(e) => e.stopPropagation()}
            >
              <button
                onClick={handleCloseVideo}
                aria-label="Quit watching"
                className="absolute top-3 right-3 z-50 inline-flex items-center gap-2 bg-red-600 text-white px-3 py-1.5 sm:px-3 sm:py-2 rounded-full hover:bg-red-700 text-sm sm:text-base"
              >
                <X size={16} /> Quit
              </button>

              <video
                ref={videoRef}
                src={videoSrc}
                controls
                autoPlay
                controlsList="nodownload noplaybackrate"
                disablePictureInPicture
                onContextMenu={(e) => e.preventDefault()}
                onEnded={handleCloseVideo}
                className="w-full h-auto rounded-b-2xl"
              />
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}

export default HeroCarousel;
