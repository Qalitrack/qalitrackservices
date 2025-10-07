'use client';

import Link from 'next/link';
import {
  MapPin,
  Phone,
  Mail,
  UserPlus,
  Home,
  LogIn,
  LayoutDashboard,
  Menu,
  X,
  ChevronDown,
} from 'lucide-react';
import { useState } from 'react';
import logo from '@/assets/LOGO-COLORED.svg';

function Navbar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [weighingDropdownOpen, setWeighingDropdownOpen] = useState(false);
  const [calibrationDropdownOpen, setCalibrationDropdownOpen] = useState(false);
  const [automationDropdownOpen, setAutomationDropdownOpen] = useState(false);

  const closeAllDropdowns = () => {
    setWeighingDropdownOpen(false);
    setCalibrationDropdownOpen(false);
    setAutomationDropdownOpen(false);
  };

  const handleDropdownToggle = (setter, isOpen) => {
    closeAllDropdowns();
    setter(!isOpen);
  };

  const navItems = [
    { name: 'Home', path: '/' },
    { name: 'About', path: '/about' },
    {
      name: 'Weighing',
      path: '#',
      dropdown: [
        
        { name: 'Portable/Axle Weighers', path: '/weighing/portable-axle' },
        { name: 'Weighing Software', path: '/weighing/software' },
        { name: 'Unmanned/Automated Weighbridges', path: '/weighing/unmanned-automated' },
        { name: 'Retail Scales', path: '/weighing/retail-scales' },
        { name: 'Onboard Weighing', path: '/weighing/onboard-weighing' },
        { name: 'Weighbridge Accessories', path: '/weighing/accessories' },
        // { name: 'Multideck/Singledeck Weighbridges', path: '/weighing/multideck-singledeck' },
      ],
      dropdownState: weighingDropdownOpen,
      setDropdownState: setWeighingDropdownOpen,
    },
    {
      name: 'Calibration',
      path: '#',
      dropdown: [
        { name: 'Volumetric Tank Calibration', path: '/calibration/volumetric-tank' },
        { name: 'Multideck/Single Deck Calibration', path: '/calibration/multideck-singledeck' },
        { name: 'Flow Meters/Pressure Calibrations', path: '/calibration/flow-pressure' },
      ],
      dropdownState: calibrationDropdownOpen,
      setDropdownState: setCalibrationDropdownOpen,
    },
    {
      name: 'Automation',
      path: '#',
      dropdown: [
        { name: 'Building Management Systems', path: '/automation/building-management' },
        { name: 'Industrial Automation', path: '/automation/industrial' },
        { name: 'Intelligent Transport Systems', path: '/automation/intelligent-transport' },
      ],
      dropdownState: automationDropdownOpen,
      setDropdownState: setAutomationDropdownOpen,
    },
    { name: 'Catalogue', path: '/catalogue' },
    { name: 'Contact', path: '/contact' },
  ];

  return (
    <header className="fixed top-0 left-0 w-full z-50">
      {/* Top Info Bar */}
      <div className="bg-amber-400 text-xs text-black px-4 py-2 flex justify-between items-center overflow-x-auto">
        <div className="flex gap-4 items-center">
          <span className="flex items-center gap-1 min-w-fit"><MapPin size={14} /> QSL centre 1st Floor, Nairobi</span>
          <span className="flex items-center gap-1 min-w-fit"><Phone size={14} /> +254714999996/+254756999996</span>
          <span className="flex items-center gap-1 min-w-fit"><Mail size={14} /> info@qalibrated.co.ke</span>
          <span className="flex items-center gap-1 min-w-fit"><Home size={14} /> P.O BOX 34463-00100</span>
        </div>
        {/* <div className="hidden md:flex gap-4 items-center text-sm">
          <Link href="/login" className="flex items-center gap-1 hover:text-white"><LogIn size={14} /> Dashboard</Link>
        </div> */}
      </div>

      {/* Main Nav */}
      <nav className="bg-white shadow py-4 px-6 flex items-center justify-between relative z-40">
        <Link href="/" className="flex items-center gap-2">
          <img src={logo.src} alt="QSL Logo" className="h-10 w-auto" />
        </Link>

        {/* Desktop Navigation */}
        <div className="hidden md:flex gap-6 items-center font-bold tracking-wide text-black text-lg">
          {navItems.map((item, index) => (
            <div
              key={index}
              className="relative group"
              onMouseEnter={() => item.dropdown && handleDropdownToggle(item.setDropdownState, item.dropdownState)}
              onMouseLeave={closeAllDropdowns}
            >
              {item.dropdown ? (
                <button
                  className="hover:text-amber-400 hover:underline underline-offset-4 flex items-center font-bold"
                  onClick={(e) => {
                    e.preventDefault();
                    handleDropdownToggle(item.setDropdownState, item.dropdownState);
                  }}
                >
                  {item.name}
                  {item.dropdown && <ChevronDown className="h-4 w-4 ml-1 transition-transform duration-200 group-hover:rotate-180" />}
                </button>
              ) : (
                <Link
                  href={item.path}
                  className="hover:text-amber-400 hover:underline underline-offset-4 flex items-center font-bold"
                >
                  {item.name}
                </Link>
              )}
              {item.dropdown && item.dropdownState && (
                <div className="absolute left-0 mt-0 w-64 bg-white shadow-lg rounded-md overflow-hidden z-50">
                  {item.dropdown.map((subItem, subIndex) => (
                    <Link
                      key={subIndex}
                      href={subItem.path}
                      className="block px-4 py-3 text-gray-800 font-medium hover:bg-amber-100 hover:text-amber-600 transition-colors duration-200"
                      onClick={() => {
                        closeAllDropdowns();
                        setIsMobileMenuOpen(false);
                      }}
                    >
                      {subItem.name}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          ))}
          <Link href="/dashboard" className="ml-4 px-4 py-2 bg-amber-400 text-black rounded-full font-bold hover:opacity-90 transition">
            Get Started
          </Link>
        </div>

        {/* Hamburger Menu for Mobile */}
        <div className="md:hidden">
          <button onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)} className="text-black focus:outline-none">
            {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>

        {/* Mobile Navigation Menu */}
        {isMobileMenuOpen && (
          <div className="absolute top-full left-0 w-full bg-white shadow-lg md:hidden z-50">
            <div className="flex flex-col items-start p-6 space-y-4">
              {navItems.map((item, index) => (
                <div key={index} className="w-full">
                  <button
                    className="block w-full text-left py-2 text-black hover:text-amber-400 hover:underline underline-offset-4 font-bold tracking-wide flex items-center justify-between"
                    onClick={() => {
                      if (item.dropdown) {
                        handleDropdownToggle(item.setDropdownState, item.dropdownState);
                      } else {
                        setIsMobileMenuOpen(false);
                        window.location.href = item.path;
                      }
                    }}
                  >
                    {item.name}
                    {item.dropdown && <ChevronDown className={`h-4 w-4 transition-transform duration-200 ${item.dropdownState ? 'rotate-180' : ''}`} />}
                  </button>
                  {item.dropdown && item.dropdownState && (
                    <div className="bg-gray-50 mt-2 rounded-md overflow-hidden">
                      {item.dropdown.map((subItem, subIndex) => (
                        <Link
                          key={subIndex}
                          href={subItem.path}
                          className="block px-6 py-2 text-gray-700 font-medium hover:bg-gray-100 transition-colors duration-200"
                          onClick={() => setIsMobileMenuOpen(false)}
                        >
                          {subItem.name}
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
              ))}
              <Link
                href="/login"
                className="w-full text-center px-4 py-2 bg-amber-400 text-black rounded-full font-bold hover:opacity-90 transition mt-4"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Get Started
              </Link>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}

export default Navbar;
