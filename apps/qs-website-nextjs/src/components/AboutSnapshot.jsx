'use client';

import React from 'react';
import Image from 'next/image';
import { motion } from 'framer-motion';
import { Award, Users, ArrowRight } from 'lucide-react';
import aboutImage from '../assets/team.jpg';

const AboutSnapshot = () => {
  return (
    <section className="container bg-white mx-auto px-4 my-16 text-center">
      <motion.h2
        className="text-4xl font-bold text-center mb-12 text-amber-400"
        initial={{ opacity: 0, y: -30 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true }}
        transition={{ duration: 0.6 }}
      >
        About Us
      </motion.h2>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
        {/* Left Column - Image */}
        <motion.div
          className="flex justify-center items-center"
          initial={{ opacity: 0, x: -50 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.8 }}
        >
          <Image
            src={aboutImage}
            alt="Qalibrated Systems Limited team"
            className="rounded-lg shadow-xl object-cover max-h-[450px] w-full h-auto"
            priority
          />
        </motion.div>

        {/* Right Column - Text Content */}
        <motion.div
          className="text-center lg:text-left"
          initial={{ opacity: 0, x: 50 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.8, delay: 0.2 }}
        >
          <h3 className="text-4xl font-bold leading-tight mb-6">
            Discover Our Commitment to Precision
          </h3>

          <p className="text-gray-600 mb-6 leading-relaxed">
            <strong>Qalibrated Systems Limited (QSL)</strong> is a Kenyan-registered
            private limited company with 100% local shareholding. Originally established
            in 2009 as <em>Resolution Electro-Technique</em>, our company has grown into
            a market leader in advanced weighing systems, calibration, and industrial
            automation. Since 2017, we have expanded into construction, roadworks,
            water and sewerage, as well as mechanical and electrical engineering.
          </p>

          <p className="text-gray-600 mb-8 leading-relaxed">
            We take pride in our team of highly trained professionals who are dedicated
            to delivering tailored solutions that meet industry needs with accuracy,
            reliability, and trusted local support.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <motion.div
              className="bg-gray-100 p-4 rounded-md shadow-sm flex items-center space-x-3 text-left"
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6, delay: 0.3 }}
            >
              <Award className="h-6 w-6 text-amber-500 flex-shrink-0" />
              <div>
                <h4 className="font-bold text-lg">Market Leader</h4>
                <p className="text-gray-700 text-sm">
                  Pioneering integrated weighing and automation solutions.
                </p>
              </div>
            </motion.div>

            <motion.div
              className="bg-gray-100 p-4 rounded-md shadow-sm flex items-center space-x-3 text-left"
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6, delay: 0.5 }}
            >
              <Users className="h-6 w-6 text-amber-500 flex-shrink-0" />
              <div>
                <h4 className="font-bold text-lg">Expert Team</h4>
                <p className="text-gray-700 text-sm">
                  Skilled professionals with deep industry knowledge.
                </p>
              </div>
            </motion.div>
          </div>

          {/* CTA Button */}
          <motion.a
            href="/about"
            className="inline-flex items-center bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.7 }}
          >
            Learn More About Us
            <ArrowRight className="h-4 w-4 ml-2 inline-block" />
          </motion.a>
        </motion.div>
      </div>
    </section>
  );
};

export default AboutSnapshot;
