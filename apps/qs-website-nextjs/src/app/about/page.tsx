'use client';

import React, { useState } from 'react';
import {
  ArrowUp,
  Scale,
  Building,
  Car,
  Settings,
  Factory,
  FlaskConical,
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import Link from 'next/link';
import Image from 'next/image';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import Newsletter from '@/components/ContactCallToAction';
import Partners from '@/components/CertificationsLogos';
import about1 from '@/assets/about.jpg';
import HeroAbout from '@/assets/hero/calibration.jpg';


const fadeUp = {
  hidden: { opacity: 0, y: 40 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.6 } },
};

const modalVariants = {
  hidden: { opacity: 0, scale: 0.9 },
  visible: { opacity: 1, scale: 1, transition: { duration: 0.3 } },
  exit: { opacity: 0, scale: 0.9, transition: { duration: 0.2 } },
};

const solutions = [
  {
    title: 'Commercial Weighing Solutions',
    short: 'Weighbridges, axle weighers, portable scales, and weighing software.',
    long: `We provide cutting-edge weighing technology tailored for logistics, 
    manufacturing, and transport sectors. Our solutions integrate seamlessly 
    with ERP and IoT systems for real-time monitoring and reporting.`,
    points: [
      'Truck weighbridges and axle weighers',
      'Digital portable scales',
      'Automated weighing software',
      'Integration with ERP systems',
    ],
    icon: Scale,
  },
  {
    title: 'Building & Construction',
    short: 'Residential, commercial, and industrial building with lifecycle support.',
    long: `Our construction services cover design, project management, 
    and execution across multiple sectors. We focus on sustainability, 
    efficiency, and cost-effectiveness.`,
    points: [
      'Turnkey residential & commercial projects',
      'Civil and structural works',
      'Electrical and mechanical installations',
      'Sustainable building solutions',
    ],
    icon: Building,
  },
  {
    title: 'Intelligent Transport Systems',
    short: 'Smart parking, traffic monitoring, and digital mobility solutions.',
    long: `We deliver smart transport solutions designed to improve 
    safety, efficiency, and environmental sustainability.`,
    points: [
      'Smart tolling and e-parking systems',
      'Fleet tracking and management',
      'Traffic data analytics',
      'IoT-enabled road safety monitoring',
    ],
    icon: Car,
  },
  {
    title: 'Calibrations',
    short: 'Calibration of tanks, meters, and weighing systems.',
    long: `We are ISO-compliant calibration experts ensuring accuracy 
    and compliance in all your measurement devices.`,
    points: [
      'Weighing machine calibration',
      'Tank and flow meter calibration',
      'ISO 17025 compliant',
      'Nationwide calibration services',
    ],
    icon: Settings,
  },
  {
    title: 'Industrial Automation',
    short: 'AI-powered conveyors, check weighers, and process control.',
    long: `We provide end-to-end industrial automation solutions that 
    leverage AI, robotics, and IoT for optimal efficiency.`,
    points: [
      'Automated conveyor systems',
      'Robotic packaging and check weighing',
      'Predictive maintenance solutions',
      'IoT and AI integration',
    ],
    icon: Factory,
  },
  {
    title: 'Research & Development',
    short: 'Innovations in measurement and infrastructure.',
    long: `Our R&D department pioneers new technologies for precision 
    measurement, sustainability, and smart infrastructure.`,
    points: [
      'Product prototyping',
      'Collaboration with universities',
      'Smart infrastructure solutions',
      'Sustainable engineering innovations',
    ],
    icon: FlaskConical,
  },
];

export default function About() {
  const [activeSolution, setActiveSolution] = useState<typeof solutions[0] | null>(null);

  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
      <Navbar />

      {/* Hero Section */}
      <section
        className="relative bg-cover bg-center text-white py-20 md:py-32 rounded-b-lg shadow-lg"
        style={{ backgroundImage: `url(${HeroAbout.src})` }}
      >
        <div className="absolute inset-0 bg-black opacity-50 rounded-b-lg"></div>
        <div className="relative z-10 max-w-screen-xl mx-auto px-4 text-center">
          <motion.h1
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            className="text-3xl sm:text-5xl md:text-6xl font-bold mb-4"
          >
            About Us
          </motion.h1>
          {/* <motion.p
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            className="text-base md:text-xl"
          >
            Home / <span className="text-amber-400">About</span>
          </motion.p> */}
        </div>
      </section>

      {/* About Content */}
      <section className="max-w-screen-xl mx-auto px-4 my-16">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          <motion.div
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            viewport={{ once: true }}
          >
            <h3 className="text-2xl sm:text-4xl font-bold mb-6">
              Engineering Precision & Infrastructure Innovation
            </h3>
            <p className="text-gray-600 mb-4 leading-relaxed">
              <strong>Qalibrated Systems Limited (QSL)</strong> is a Kenyan-registered
              private limited company with 100% local shareholding. Established in
              2009 as Resolution Electro-Technique, our company has grown into a
              market leader in advanced weighing systems, calibrations, and
              industrial automation. Since 2017, we&apos;ve expanded into construction,
              water & sewerage, and mechanical & electrical engineering.
            </p>
            <p className="text-gray-600 mb-8 leading-relaxed">
              We operate from Nairobi with branches in Mombasa and Kisumu, providing
              nationwide service. Our client-first approach and strong technical team
              have built lasting partnerships across industries.
            </p>
          </motion.div>

          <motion.div
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            viewport={{ once: true }}
            className="flex justify-center"
          >
            <Image
              src={about1}
              alt="About Us"
              className="w-full h-auto rounded-lg shadow-xl max-w-full"
            />
          </motion.div>
        </div>
      </section>

      {/* Solutions Section */}
      <section className="bg-gray-50 py-16 rounded-t-lg shadow-inner">
        <div className="max-w-screen-xl mx-auto px-4 text-center">
          <motion.h2
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            className="text-amber-500 text-2xl sm:text-3xl font-bold mb-2"
          >
            What We Offer
          </motion.h2>
          <motion.h3
            initial="hidden"
            whileInView="visible"
            variants={fadeUp}
            className="text-2xl sm:text-4xl font-bold mb-12 max-w-2xl mx-auto"
          >
            Delivering integrated solutions across weighing, engineering, and
            infrastructure.
          </motion.h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 sm:gap-8">
            {solutions.map((sol, i) => (
              <motion.div
                key={i}
                className="bg-white p-6 rounded-lg shadow-md border flex flex-col items-center text-center hover:shadow-2xl hover:scale-105 transition duration-300"
                initial="hidden"
                whileInView="visible"
                variants={fadeUp}
              >
                <div className="bg-amber-100 rounded-full p-4 mb-4">
                  <sol.icon
                    className="h-10 w-10 text-amber-500"
                    aria-hidden="true"
                  />
                </div>
                <h4 className="font-bold text-lg sm:text-xl mb-2">{sol.title}</h4>
                <p className="text-gray-600 text-sm mb-4">{sol.short}</p>
                <button
                  className="text-amber-500 font-semibold hover:underline text-sm"
                  onClick={() => setActiveSolution(sol)}
                >
                  Learn More →
                </button>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Modal for Learn More */}
      <AnimatePresence>
        {activeSolution && (
          <motion.div
            className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50 p-4"
            initial="hidden"
            animate="visible"
            exit="exit"
            variants={modalVariants}
          >
            <div className="bg-white rounded-2xl shadow-lg max-w-2xl w-full p-8 relative overflow-y-auto max-h-[90vh]">
              {/* Close Button */}
              <button
                onClick={() => setActiveSolution(null)}
                className="absolute top-3 right-3 text-gray-500 hover:text-black text-xl"
              >
                ✕
              </button>

              {/* Icon + Title */}
              <div className="flex items-center gap-3 mb-4">
                <div className="bg-amber-100 p-3 rounded-full">
                  <activeSolution.icon className="h-8 w-8 text-amber-500" />
                </div>
                <h3 className="text-2xl font-bold">{activeSolution.title}</h3>
              </div>

              {/* Long Description */}
              <p className="text-gray-700 mb-4">{activeSolution.long}</p>

              {/* Bullet Points */}
              <ul className="list-disc pl-6 text-gray-700 space-y-2 mb-6">
                {activeSolution.points.map((point, idx) => (
                  <li key={idx}>{point}</li>
                ))}
              </ul>

              {/* CTA button */}
              <Link
                href="/contact"
                className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-2 px-6 rounded-lg shadow-md transition duration-300 inline-block text-center"
              >
                Contact Us to Learn More
              </Link>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Scroll to Top */}
      {/* <button
        className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg transition duration-300"
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        aria-label="Scroll to top"
      >
        <ArrowUp className="w-6 h-6" aria-hidden="true" />
      </button> */}
      <Newsletter/>
      <Partners/>
      <Footer />
    </div>
  );
}