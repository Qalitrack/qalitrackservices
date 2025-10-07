"use client";
import React, { useState } from "react";
import {
  Apple,
  Play,
  MapPin,
  Mail,
  Phone,
  Globe,
  ChevronDown,
} from "lucide-react";
import Link from "next/link";
import Image from "next/image";

const Footer = () => {
  const [openSection, setOpenSection] = useState(null);

  const toggleSection = (section) => {
    setOpenSection(openSection === section ? null : section);
  };

  return (
    <footer className="relative bg-gradient-to-br from-black via-gray-900 to-amber-950 text-gray-300 pt-16 pb-10 overflow-hidden">
      {/* Decorative background glow */}
      <div className="absolute inset-0">
        <div className="absolute -top-24 -left-24 w-72 h-72 bg-amber-500/20 rounded-full blur-3xl animate-pulse"></div>
        <div className="absolute bottom-0 right-0 w-96 h-96 bg-amber-700/10 rounded-full blur-3xl animate-pulse"></div>
      </div>

      {/* Main Footer Grid */}
      <div className="max-w-7xl mx-auto px-6 md:px-8 grid grid-cols-1 md:grid-cols-5 gap-12 relative z-10">
        {/* Company Info */}
        <div className="space-y-4">
          <Image
            src="/logorange.svg"
            alt="Qalibrated Logo"
            width={120}
            height={40}
            className="h-12 w-auto"
          />
          <p className="text-sm leading-relaxed">
            Qalibrated Systems Limited delivers cutting-edge weighing,
            calibration, and automation solutions that power Africa’s industries
            with accuracy and innovation.
          </p>
          <div className="flex gap-3 pt-4">
            <a
              href="#"
              className="bg-white text-black px-4 py-2 rounded-lg flex items-center shadow-lg hover:scale-105 hover:bg-amber-400 transition-all"
            >
              <Apple className="mr-2 w-5 h-5" /> App Store
            </a>
            <a
              href="#"
              className="bg-black/40 backdrop-blur-md border border-gray-600 text-white px-4 py-2 rounded-lg flex items-center hover:scale-105 hover:border-amber-400 transition-all"
            >
              <Play className="mr-2 w-5 h-5" /> Google Play
            </a>
          </div>
        </div>

        {/* Quick Links */}
        <div className="col-span-2">
          <h3 className="text-white font-bold text-lg mb-6 border-b border-gray-700 pb-2">
            Quick Links
          </h3>
          <ul className="space-y-4 text-sm">
            <li>
              <Link href="/" className="hover:text-amber-400 transition">
                Home
              </Link>
            </li>
            <li>
              <Link href="/about" className="hover:text-amber-400 transition">
                About
              </Link>
            </li>

            {/* Weighing Dropdown */}
            <li>
              <button
                onClick={() => toggleSection("weighing")}
                aria-expanded={openSection === "weighing"}
                className="flex justify-between items-center w-full text-left font-semibold text-white"
              >
                Weighing{" "}
                <ChevronDown
                  className={`w-4 h-4 transform transition-transform ${
                    openSection === "weighing" ? "rotate-180" : ""
                  }`}
                />
              </button>
              {openSection === "weighing" && (
                <ul className="ml-4 mt-2 space-y-2 text-sm">
                  <li>
                    <Link href="/weighing/portable-axle" className="hover:text-amber-400">
                      Portable/Axle Weighers
                    </Link>
                  </li>
                  <li>
                    <Link href="/weighing/software" className="hover:text-amber-400">
                      Weighing Software
                    </Link>
                  </li>
                  <li>
                    <Link href="/weighing/unmanned-automated" className="hover:text-amber-400">
                      Automated Weighbridges
                    </Link>
                  </li>
                  <li>
                    <Link href="/weighing/retail-scales" className="hover:text-amber-400">
                      Retail Scales
                    </Link>
                  </li>
                  <li>
                    <Link href="/weighing/onboard-weighing" className="hover:text-amber-400">
                      Onboard Weighing
                    </Link>
                  </li>
                  <li>
                    <Link href="/weighing/accessories" className="hover:text-amber-400">
                      Weighbridge Accessories
                    </Link>
                  </li>
                </ul>
              )}
            </li>

            {/* Calibration Dropdown */}
            <li>
              <button
                onClick={() => toggleSection("calibration")}
                aria-expanded={openSection === "calibration"}
                className="flex justify-between items-center w-full text-left font-semibold text-white"
              >
                Calibration{" "}
                <ChevronDown
                  className={`w-4 h-4 transform transition-transform ${
                    openSection === "calibration" ? "rotate-180" : ""
                  }`}
                />
              </button>
              {openSection === "calibration" && (
                <ul className="ml-4 mt-2 space-y-2 text-sm">
                  <li>
                    <Link href="/calibration/volumetric-tank" className="hover:text-amber-400">
                      Volumetric Tank Calibration
                    </Link>
                  </li>
                  <li>
                    <Link href="/calibration/multideck-singledeck" className="hover:text-amber-400">
                      Multideck/Single Deck Calibration
                    </Link>
                  </li>
                  <li>
                    <Link href="/calibration/flow-pressure" className="hover:text-amber-400">
                      Flow/Pressure Calibration
                    </Link>
                  </li>
                </ul>
              )}
            </li>

            {/* Automation Dropdown */}
            <li>
              <button
                onClick={() => toggleSection("automation")}
                aria-expanded={openSection === "automation"}
                className="flex justify-between items-center w-full text-left font-semibold text-white"
              >
                Automation{" "}
                <ChevronDown
                  className={`w-4 h-4 transform transition-transform ${
                    openSection === "automation" ? "rotate-180" : ""
                  }`}
                />
              </button>
              {openSection === "automation" && (
                <ul className="ml-4 mt-2 space-y-2 text-sm">
                  <li>
                    <Link href="/automation/building-management" className="hover:text-amber-400">
                      Building Management
                    </Link>
                  </li>
                  <li>
                    <Link href="/automation/industrial" className="hover:text-amber-400">
                      Industrial Automation
                    </Link>
                  </li>
                  <li>
                    <Link href="/automation/intelligent-transport" className="hover:text-amber-400">
                      Intelligent Transport
                    </Link>
                  </li>
                </ul>
              )}
            </li>

            <li>
              <Link href="/catalogue" className="hover:text-amber-400">
                Catalogue
              </Link>
            </li>
            <li>
              <Link href="/contact" className="hover:text-amber-400">
                Contact
              </Link>
            </li>
          </ul>
        </div>

        {/* Support */}
        <div>
          <h3 className="text-white font-bold text-lg mb-6 border-b border-gray-700 pb-2">
            Support
          </h3>
          <ul className="space-y-3 text-sm">
            <li><Link href="/support/privacy" className="hover:text-amber-400">Privacy Policy</Link></li>
            <li><Link href="/support/terms" className="hover:text-amber-400">Terms & Conditions</Link></li>
            <li><Link href="/support/disclaimer" className="hover:text-amber-400">Disclaimer</Link></li>
            {/* <li><Link href="/support/support" className="hover:text-amber-400">Support</Link></li> */}
            <li><Link href="/support/faq" className="hover:text-amber-400">FAQ</Link></li>
            {/* <li><Link href="/support/help" className="hover:text-amber-400">Help</Link></li> */}
          </ul>
        </div>

        {/* Contact Info */}
        <div>
          <h3 className="text-white font-bold text-lg mb-6 border-b border-gray-700 pb-2">
            Contact Info
          </h3>
          <ul className="space-y-4 text-sm">
            <li className="flex items-center gap-3">
              <MapPin className="text-amber-400 w-4 h-4" />
              QSL Centre, 1st Floor, Nairobi
            </li>
            <li className="flex items-center gap-3">
              <Mail className="text-amber-400 w-4 h-4" />
              info@qalibrated.co.ke
            </li>
            <li className="flex items-center gap-3">
              <Phone className="text-amber-400 w-4 h-4" />
              +254714999996 / +254756999996
            </li>
            <li className="flex items-center gap-3">
              <Globe className="text-amber-400 w-4 h-4" />
              www.qalibrated.co.ke
            </li>
          </ul>

          {/* Socials */}
          {/* <div className="flex gap-4 mt-6">
            {[Facebook, Twitter, Instagram, Linkedin].map((Icon, i) => (
              <a
                key={i}
                href="#"
                className="p-2 rounded-full bg-amber-400 text-black hover:scale-110 hover:rotate-3 transition-all"
              >
                <Icon size={16} />
              </a>
            ))}
          </div> */}
        </div>
      </div>

      {/* Footer Bottom */}
      <div className="border-t border-gray-700 mt-12 pt-6 text-center text-sm text-gray-400 relative z-10">
        <p>
          © {new Date().getFullYear()}{" "}
          <span className="text-white font-semibold">Qalibrated Systems Limited</span>.
          All rights reserved.
        </p>
        {/* <p className="mt-2">
          Crafted with <span className="animate-pulse text-amber-400">⚡</span> by{" "}
          <a
            href="https://www.linkedin.com/in/faith-zawadi"
            target="_blank"
            className="text-amber-400 hover:underline"
          >
            MissTechy
          </a>
        </p> */}
      </div>
    </footer>
  );
};

export default Footer;
