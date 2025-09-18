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
  Menu, // Import the Menu icon for the hamburger
  X, // Import the X icon for closing the menu
  ChevronDown, // Import ChevronDown for dropdown indicators
} from 'lucide-react';
import { useState } from 'react'; // Import useState hook
import logo from '@/assets/LOGO-COLORED.svg';

// IMPORTANT: routes.js defines the *routing logic*, Navbar defines the *navigation UI structure*.
// We don't directly import routes.js here to build navItems because routes.js is a flat list
// of path-to-component mappings and does not contain the hierarchical/dropdown metadata
// needed for the Navbar's visual structure.
// Instead, ensure the 'path' values in 'navItems' below are consistent with your routes.js.

function Navbar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false); // State to manage overall mobile menu open/close
  const [weighingDropdownOpen, setWeighingDropdownOpen] = useState(false);
  const [calibrationDropdownOpen, setCalibrationDropdownOpen] = useState(false);
  const [automationDropdownOpen, setAutomationDropdownOpen] = useState(false);

  // Helper to close all dropdowns
  const closeAllDropdowns = () => {
    setWeighingDropdownOpen(false);
    setCalibrationDropdownOpen(false);
    setAutomationDropdownOpen(false);
  };

  // Helper to handle dropdown toggle for both desktop and mobile
  const handleDropdownToggle = (setter, isOpen) => {
    closeAllDropdowns(); // Close other dropdowns before opening a new one
    setter(!isOpen);
  };

  // Define navigation items with their dropdowns.
  // The 'path' values here should match the paths defined in your routes.js.
  const navItems = [
    { name: 'Home', path: '/' },
    { name: 'About', path: '/about' },
    { 
      name: 'Weighing', 
      path: '#', // Use '#' or a relevant parent path if it's a clickable category page
      dropdown: [
        { name: 'Multideck/Singledeck Weighbridges', path: '/weighing/multideck-singledeck' },
        { name: 'Portable/Axle Weighers', path: '/weighing/portable-axle' },
        { name: 'Weighing Software', path: '/weighing/software' },
        { name: 'Unmanned/Automated Weighbridges', path: '/weighing/unmanned-automated' },
        { name: 'Retail Scales', path: '/weighing/retail-scales' },
        { name: 'Onboard Weighing', path: '/weighing/onboard-weighing' },
        { name: 'Weighbridge Accessories', path: '/weighing/accessories' },
      ],
      dropdownState: weighingDropdownOpen,
      setDropdownState: setWeighingDropdownOpen
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
      setDropdownState: setCalibrationDropdownOpen
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
      setDropdownState: setAutomationDropdownOpen
    },
    { name: 'Catalogue', path: '/catalogue' },
    { name: 'Contact', path: '/contact' },
  ];

  return (
    <header className="fixed top-0 left-0 w-full z-50"> {/* Applied fixed positioning here */}
      {/* Top Info Bar */}
      <div className="bg-amber-400 text-xs text-black px-4 py-2 flex justify-between items-center overflow-x-auto">
        <div className="flex gap-4 items-center">
          <span className="flex items-center gap-1 min-w-fit"><MapPin size={14} /> QSL centre 1st Floor, Nairobi</span>
          <span className="flex items-center gap-1 min-w-fit"><Phone size={14} /> +254714999996/+254756999996</span>
          <span className="flex items-center gap-1 min-w-fit"><Mail size={14} /> info@qalibrated.co.ke</span>
          <span className="flex items-center gap-1 min-w-fit"><Home size={14} /> P.O BOX 34463-00100</span>
        </div>
        <div className="hidden md:flex gap-4 items-center text-sm">
          {/* <Link to="/signup" className="flex items-center gap-1 hover:text-white"><UserPlus size={14} /> Register</Link> */}
          <Link href="/login" className="flex items-center gap-1 hover:text-white"><LogIn size={14} /> Dashboard</Link>
          {/* <Link to="/dashboard" className="flex items-center gap-1 hover:text-white"><LayoutDashboard size={14} /> My Dashboard</Link> */}
        </div>
      </div>

      {/* Main Nav */}
      <nav className="bg-white shadow py-4 px-6 flex items-center justify-between relative z-40"> {/* z-40 is fine here, z-50 on parent is dominant */}
        <Link href="/" className="flex items-center gap-2">
          <img src={logo.src} alt="QSL Logo" className="h-10 w-auto" />
        </Link>

        {/* Desktop Navigation */}
        <div className="hidden md:flex gap-6 items-center font-medium text-black">
          {navItems.map((item, index) => (
            <div 
              key={index} 
              className="relative group"
              onMouseEnter={() => item.dropdown && handleDropdownToggle(item.setDropdownState, item.dropdownState)}
              onMouseLeave={closeAllDropdowns} // Close all dropdowns when leaving this nav item's area
            >
              {item.dropdown ? (
                <button 
                  className="hover:text-amber-400 flex items-center"
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
                  className="hover:text-amber-400 flex items-center"
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
                      className="block px-4 py-3 text-gray-800 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                      onClick={() => {
                        closeAllDropdowns(); // Close dropdown on click
                        setIsMobileMenuOpen(false); // Ensure mobile menu is closed if it was open
                      }}
                    >
                      {subItem.name}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          ))}
          <Link href="/contact" className="ml-4 px-4 py-2 bg-amber-400 text-black rounded-full hover:opacity-90 transition">
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
                    className="block w-full text-left py-2 text-black hover:text-amber-400 font-medium flex items-center justify-between"
                    onClick={() => {
                      if (item.dropdown) {
                        handleDropdownToggle(item.setDropdownState, item.dropdownState);
                      } else {
                        setIsMobileMenuOpen(false); // Close mobile menu on non-dropdown link click
                        window.location.href = item.path; // Navigate directly for non-dropdowns
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
                          className="block px-6 py-2 text-gray-700 hover:bg-gray-100 transition-colors duration-200"
                          onClick={() => setIsMobileMenuOpen(false)} // Close mobile menu on sub-item click
                        >
                          {subItem.name}
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
              ))}
              <Link href="/contact" className="w-full text-center px-4 py-2 bg-amber-400 text-black rounded-full hover:opacity-90 transition mt-4" onClick={() => setIsMobileMenuOpen(false)}>
                Get Started
              </Link>
              {/* Mobile version of the top info bar links */}
              <div className="w-full pt-4 border-t border-gray-200 mt-4">
                <Link href="/register" className="flex items-center gap-1 hover:text-amber-400 w-full text-left py-2" onClick={() => setIsMobileMenuOpen(false)}><UserPlus size={16} /> Register</Link>
                <Link href="/login" className="flex items-center gap-1 hover:text-amber-400 w-full text-left py-2" onClick={() => setIsMobileMenuOpen(false)}><LogIn size={16} /> Login</Link>
                <Link href="/dashboard" className="flex items-center gap-1 hover:text-amber-400 w-full text-left py-2" onClick={() => setIsMobileMenuOpen(false)}><LayoutDashboard size={16} /> My Dashboard</Link>
              </div>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}

export default Navbar;
