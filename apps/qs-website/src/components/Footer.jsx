// src/components/Footer.jsx
import React from "react";
import {
  Apple,
  Play,
  Facebook,
  Twitter,
  Instagram,
  Linkedin,
  MapPin,
  Mail,
  Phone,
  Globe,
} from "lucide-react";

import logo from "../assets/logorange.svg";
import FlyingBeeSwarm from "./Bee"; // <- new swarm with hive & bees

const Footer = () => {
  return (
    <footer className="relative bg-black text-gray-300 pt-12 overflow-hidden h-80">
      {/* Bees + Hive */}
      <FlyingBeeSwarm />

      <div className="max-w-7xl mx-auto px-6 md:px-8 grid grid-cols-1 md:grid-cols-4 gap-10 relative z-10">
        {/* Company Info */}
        <div>
          <img src={logo} alt="Qalibrated Logo" className="h-10 mb-4" />
          <p className="mb-4">
            Qalibrated Systems Limited provides innovative weighing and automation
            systems tailored for Africa’s industries.
          </p>
          <div className="flex gap-3">
            {/* <a
              href="#"
              className="bg-amber-400 text-black px-4 py-2 rounded flex items-center hover:bg-amber-500"
            >
              <Apple className="mr-2 w-5 h-5" /> App Store
            </a>
            <a
              href="#"
              className="bg-gray-800 text-white px-4 py-2 rounded flex items-center hover:bg-gray-700"
            >
              <Play className="mr-2 w-5 h-5" /> Google Play
            </a> */}
          </div>
        </div>

        {/* Quick Links */}
        <div>
          <h3 className="text-white font-semibold text-lg mb-4">Quick Links</h3>
          <ul className="space-y-2">
            <li><a href="/about" className="hover:text-amber-400">About Us</a></li>
            <li><a href="/services" className="hover:text-amber-400">Services</a></li>
            <li><a href="/catalogue" className="hover:text-amber-400">Catalogue</a></li>
            <li><a href="/contact" className="hover:text-amber-400">Contact</a></li>
          </ul>
        </div>

        {/* Support */}
        <div>
          <h3 className="text-white font-semibold text-lg mb-4">Support</h3>
          <ul className="space-y-2">
            <li><a href="/support/privacy" className="hover:text-amber-400">Privacy Policy</a></li>
            <li><a href="/support/terms" className="hover:text-amber-400">Terms & Conditions</a></li>
            {/* <li><a href="/support/disclaimer" className="hover:text-amber-400">Disclaimer</a></li>
            <li><a href="/support/support" className="hover:text-amber-400">Support</a></li> */}
            <li><a href="/support/faq" className="hover:text-amber-400">FAQ</a></li>
            <li><a href="#" className="hover:text-amber-400">Help</a></li>
          </ul>
        </div>

        {/* Contact Info */}
        <div>
          <h3 className="text-white font-semibold text-lg mb-4">Contact Info</h3>
          <ul className="space-y-3 text-sm">
            <li className="flex items-center gap-3">
              <MapPin className="text-amber-400 w-4 h-4" /> Nairobi, Kenya
            </li>
            <li className="flex items-center gap-3">
              <Mail className="text-amber-400 w-4 h-4" /> info@qalibrated.co.ke
            </li>
            <li className="flex items-center gap-3">
              <Phone className="text-amber-400 w-4 h-4" /> +254714999996
            </li>
            <li className="flex items-center gap-3">
              <Globe className="text-amber-400 w-4 h-4" /> www.qalibrated.co.ke
            </li>
          </ul>
          <div className="flex gap-4 mt-4">
            <a href="#" className="bg-amber-400 text-black p-2 rounded-full hover:bg-amber-500">
              <Facebook size={16} />
            </a>
            <a href="#" className="bg-amber-400 text-black p-2 rounded-full hover:bg-amber-500">
              <Twitter size={16} />
            </a>
            <a href="#" className="bg-amber-400 text-black p-2 rounded-full hover:bg-amber-500">
              <Instagram size={16} />
            </a>
            <a href="#" className="bg-amber-400 text-black p-2 rounded-full hover:bg-amber-500">
              <Linkedin size={16} />
            </a>
          </div>
          
        </div>
      </div>

      <div className="border-t border-gray-700 mt-10 py-4 px-6 text-center text-sm text-gray-400 relative z-10">
        <p>
          © {new Date().getFullYear()}{" "}
          <span className="text-white font-semibold">Qalibrated Systems Limited</span>. All rights reserved.
        </p>
        <p>
          Designed by{" "}
          <a
            href="https://www.linkedin.com/in/faith-zawadi"
            className="text-amber-400 hover:underline"
          >
            Faith Zawadi
          </a>
        </p>
      </div>
    </footer>
  );
};

export default Footer;
