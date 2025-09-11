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
import AboutVideo from "../assets/hero/qslvideo.mp4";

// Full + Placeholder images
import AdvancedWeighbridge from "../assets/hero/weighing.png";
import IntelligentTransport from "../assets/hero/trans.jpg";
import ExpertCalibration from "../assets/hero/calibration.png";
import ResearchAndDevelopment from "../assets/hero/rnd34.jpeg";
import ElectronicCargoTracking from "../assets/hero/cargotracking.jpeg";

const slides = [
  {
    imageUrl: AdvancedWeighbridge,
    placeholderUrl: "/placeholders/weighing-small.jpg",
    heading: "Advanced Weighbridge Solutions ",
    subheading: "Precision, Power, & Performance",
  },
  {
    imageUrl: IntelligentTransport,
    placeholderUrl: "/placeholders/trans-small.jpg",
    heading: "Intelligent Transport Systems ",
    subheading: "Optimizing Logistics",
    paragraph:
      "Our Intelligent Transport Systems provide advanced management and optimization of transport logistics and vehicle movement, enhancing overall efficiency.",
  },
  {
    imageUrl: ExpertCalibration,
    placeholderUrl: "/placeholders/calibration-small.jpg",
    heading: "Expert Calibration Services ",
    paragraph:
      "We offer expert calibration services to ensure the highest accuracy and compliance of all weighing instruments, providing you with reliable data.",
  },
  {
    imageUrl: ElectronicCargoTracking,
    placeholderUrl: "/placeholders/cargotracking-small.jpg",
    heading: "Electronic Cargo Tracking ",
    subheading: "Securing Your Supply Chain",
    paragraph:
      "Our Electronic Cargo Tracking System offers comprehensive real-time monitoring of transit cargo, securing your supply chain from start to finish.",
  },
  {
    imageUrl: ResearchAndDevelopment,
    placeholderUrl: "/placeholders/rnd34-small.jpg",
    heading: "Research & Development ",
    subheading: "Innovation at Core",
    paragraph:
      "We are continuously innovating and developing new technologies to meet emerging industry demands, ensuring you stay ahead with cutting-edge solutions.",
  },
];

function HeroCarousel() {
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0);
  const [openVideo, setOpenVideo] = useState(false);
  const [loadedImages, setLoadedImages] = useState({});
  const videoRef = useRef(null);

  const currentSlide = slides[currentSlideIndex];

  // Cycle slides automatically
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlideIndex((prev) => (prev + 1) % slides.length);
    }, 5000);
    return () => clearInterval(interval);
  }, [currentSlideIndex]);

  // Preload next image
  useEffect(() => {
    const nextIndex = (currentSlideIndex + 1) % slides.length;
    const nextImg = new Image();
    nextImg.src = slides[nextIndex].imageUrl;
  }, [currentSlideIndex]);

  const handleImageLoad = (url) => {
    setLoadedImages((prev) => ({ ...prev, [url]: true }));
  };

  const handleCloseVideo = () => {
    if (videoRef.current) {
      try {
        videoRef.current.pause();
        videoRef.current.currentTime = 0;
      } catch {}
    }
    setOpenVideo(false);
  };

  useEffect(() => {
    const onKey = (e) => {
      if (e.key === "Escape" && openVideo) handleCloseVideo();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [openVideo]);

  return (
    <>
      <section className="relative h-[100vh] flex items-center justify-center overflow-hidden my-10">
        {/* Background images */}
        <div className="absolute inset-0">
          {/* Placeholder (blurred, loads instantly) */}
          <img
            src={currentSlide.placeholderUrl}
            alt="blurred background"
            className="absolute inset-0 w-full h-full object-cover blur-xl scale-105"
          />
          {/* Full image crossfades when loaded */}
          <motion.img
            key={currentSlide.imageUrl}
            src={currentSlide.imageUrl}
            alt={currentSlide.heading}
            className="absolute inset-0 w-full h-full object-cover"
            initial={{ opacity: 0 }}
            animate={{ opacity: loadedImages[currentSlide.imageUrl] ? 1 : 0 }}
            transition={{ duration: 1 }}
            onLoad={() => handleImageLoad(currentSlide.imageUrl)}
          />
          <div className="absolute inset-0 bg-black/50" />
        </div>

        {/* Navigation arrows */}
        <button
          className="absolute left-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 p-3 rounded-full hover:bg-opacity-75 transition z-20"
          onClick={() =>
            setCurrentSlideIndex(
              (prev) => (prev - 1 + slides.length) % slides.length
            )
          }
        >
          <ChevronLeft size={24} />
        </button>
        <button
          className="absolute right-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 p-3 rounded-full hover:bg-opacity-75 transition z-20"
          onClick={() =>
            setCurrentSlideIndex((prev) => (prev + 1) % slides.length)
          }
        >
          <ChevronRight size={24} />
        </button>

        {/* Slide content */}
        <div className="relative z-10 max-w-4xl text-center px-4 text-white">
          {currentSlide.subheading && (
            <h3 className="text-amber-400 font-semibold mb-2 text-3xl">
              {currentSlide.subheading}
            </h3>
          )}
          <h1 className="text-4xl md:text-6xl font-bold mb-4">
            {currentSlide.heading}
          </h1>
          {currentSlide.paragraph && (
            <p className="text-gray-200 mb-6">{currentSlide.paragraph}</p>
          )}

          <div className="flex flex-col sm:flex-row justify-center gap-4 mb-6">
            <button
              onClick={() => setOpenVideo(true)}
              className="flex items-center gap-2 bg-white text-black font-medium px-6 py-3 rounded-full hover:bg-gray-200 transition"
            >
              <PlayCircle className="text-amber-400" size={18} /> Watch Video
            </button>

            <Link
              to="/services"
              className="bg-amber-400 text-black px-6 py-3 rounded-full font-medium hover:opacity-90 transition"
            >
              Learn More
            </Link>
          </div>

          <div className="text-sm font-semibold">Follow Us:</div>
          <div className="flex justify-center gap-4 mt-2 text-lg">
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
        {openVideo && (
          <motion.div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleCloseVideo}
          >
            <motion.div
              className="relative bg-white rounded-2xl shadow-lg w-full max-w-3xl"
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              transition={{ duration: 0.25 }}
              onClick={(e) => e.stopPropagation()}
            >
              <button
                onClick={handleCloseVideo}
                className="absolute top-3 right-3 z-50 inline-flex items-center gap-2 bg-red-600 text-white px-3 py-2 rounded-full hover:bg-red-700"
              >
                <X size={16} /> Quit
              </button>
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
