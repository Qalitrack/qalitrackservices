'use client';

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
import Link from "next/link";

// Images
import AdvancedWeighbridge from "@/assets/hero/weighing.png";
import IntelligentTransport from "@/assets/hero/trans.jpg";
import ExpertCalibration from "@/assets/hero/calibration.png";
import ResearchAndDevelopment from "@/assets/hero/rnd34.jpeg";
import ElectronicCargoTracking from "@/assets/hero/cargotracking.jpeg";

const slides = [
  {
    imageUrl: AdvancedWeighbridge,
    placeholderUrl: "/placeholders/weighing-small.jpg",
    heading: "Advanced Weighbridge Solutions",
    subheading: "Precision, Power, & Performance",
  },
  {
    imageUrl: IntelligentTransport,
    placeholderUrl: "/placeholders/trans-small.jpg",
    heading: "Intelligent Transport Systems",
    subheading: "Optimizing Logistics",
    paragraph:
      "Our Intelligent Transport Systems provide advanced management and optimization of transport logistics and vehicle movement, enhancing overall efficiency.",
  },
  {
    imageUrl: ExpertCalibration,
    placeholderUrl: "/placeholders/calibration-small.jpg",
    heading: "Expert Calibration Services",
    paragraph:
      "We offer expert calibration services to ensure the highest accuracy and compliance of all weighing instruments, providing you with reliable data.",
  },
  {
    imageUrl: ElectronicCargoTracking,
    placeholderUrl: "/placeholders/cargotracking-small.jpg",
    heading: "Electronic Cargo Tracking",
    subheading: "Securing Your Supply Chain",
    paragraph:
      "Our Electronic Cargo Tracking System offers comprehensive real-time monitoring of transit cargo, securing your supply chain from start to finish.",
  },
  {
    imageUrl: ResearchAndDevelopment,
    placeholderUrl: "/placeholders/rnd34-small.jpg",
    heading: "Research & Development",
    subheading: "Innovation at Core",
    paragraph:
      "We are continuously innovating and developing new technologies to meet emerging industry demands, ensuring you stay ahead with cutting-edge solutions.",
  },
];

function HeroCarousel() {
  const [currentSlideIndex, setCurrentSlideIndex] = useState(0);
  const [openVideo, setOpenVideo] = useState(false);
  const videoRef = useRef(null);

  const currentSlide = slides[currentSlideIndex];

  // Auto cycle slides
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlideIndex((prev) => (prev + 1) % slides.length);
    }, 6000);
    return () => clearInterval(interval);
  }, []);

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
      {/* Hero Section */}
      <section
        className="relative min-h-screen flex items-center justify-center overflow-hidden"
        role="region"
        aria-label="Hero Carousel"
      >
        {/* Background images with smooth fade */}
        <div className="absolute inset-0">
          {/* Placeholder blurred background */}
          <img
            src={currentSlide.placeholderUrl}
            alt={`Placeholder for ${currentSlide.heading}`}
            className="absolute inset-0 w-full h-full object-cover blur-xl scale-105"
          />

          {/* Full image crossfade */}
          <motion.img
            key={currentSlide.imageUrl.src}
            src={currentSlide.imageUrl.src}
            alt={currentSlide.heading}
            className="absolute inset-0 w-full h-full object-cover"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 1.2, ease: "easeInOut" }}
          />

          {/* Overlay tint */}
          <div className="absolute inset-0 bg-black/50" />
        </div>

        {/* Navigation arrows */}
        <button
          className="absolute left-3 sm:left-6 top-1/2 -translate-y-1/2 bg-black/50 p-3 rounded-full hover:bg-black/70 transition z-20"
          onClick={() =>
            setCurrentSlideIndex(
              (prev) => (prev - 1 + slides.length) % slides.length
            )
          }
          aria-label="Previous slide"
        >
          <ChevronLeft size={22} />
        </button>
        <button
          className="absolute right-3 sm:right-6 top-1/2 -translate-y-1/2 bg-black/50 p-3 rounded-full hover:bg-black/70 transition z-20"
          onClick={() =>
            setCurrentSlideIndex((prev) => (prev + 1) % slides.length)
          }
          aria-label="Next slide"
        >
          <ChevronRight size={22} />
        </button>

        {/* Slide content */}
        <div className="relative z-10 max-w-3xl md:max-w-4xl text-center px-4 text-white">
          <AnimatePresence mode="wait">
            <motion.div
              key={currentSlideIndex}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              transition={{ duration: 0.8, ease: "easeInOut" }}
            >
              {currentSlide.subheading && (
                <h2 className="text-amber-400 font-semibold mb-2 text-2xl sm:text-3xl">
                  {currentSlide.subheading}
                </h2>
              )}
              <h1 className="text-3xl sm:text-5xl md:text-6xl font-bold mb-4 leading-snug">
                {currentSlide.heading}
              </h1>
              {currentSlide.paragraph && (
                <p className="text-gray-200 mb-6 text-base sm:text-lg leading-relaxed">
                  {currentSlide.paragraph}
                </p>
              )}

              <div className="flex flex-col sm:flex-row justify-center gap-4 mb-6">
                <button
                  onClick={() => setOpenVideo(true)}
                  className="flex items-center gap-2 bg-white text-black font-medium px-5 py-3 rounded-full hover:bg-gray-200 transition"
                >
                  <PlayCircle className="text-amber-400" size={18} /> Watch Video
                </button>

                <Link
                  href="/services"
                  className="bg-amber-400 text-black px-6 py-3 rounded-full font-medium hover:opacity-90 transition"
                >
                  Learn More
                </Link>
              </div>

              {/* Socials */}
              <div className="text-sm font-semibold">Follow Us:</div>
              <div className="flex justify-center gap-3 mt-2 text-lg">
                <a
                  href="https://www.facebook.com/profile.php?id=61573224881488"
                  target="_blank"
                  rel="noreferrer"
                  className="p-2 bg-white text-black rounded-full hover:bg-gray-100"
                  aria-label="Visit our Facebook page"
                >
                  <Facebook size={18} />
                </a>
                <a
                  href="https://www.linkedin.com/company/qalibrated-systems-limited"
                  target="_blank"
                  rel="noreferrer"
                  className="p-2 bg-white text-black rounded-full hover:bg-gray-100"
                  aria-label="Visit our LinkedIn page"
                >
                  <Linkedin size={18} />
                </a>
              </div>
            </motion.div>
          </AnimatePresence>
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
                  src="#" // Replace with {AboutVideo}
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
