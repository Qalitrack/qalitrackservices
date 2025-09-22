'use client';

import React, { useState } from 'react';
import {
  ArrowUp, Download, Factory, SlidersHorizontal, Cpu, Monitor, Camera, X
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import CatalogueImage from '@/assets/catalogbg.jpg';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';

// ---------------- MODELS ----------------
const models = [
  { code: "WB-TX-S-3X8-60T.6L", dimension: "3x8", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X10-60T.6L", dimension: "3x10", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X12-60T.6L", dimension: "3x12", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X14-60T.8L", dimension: "3x14", capacity: "60", division: "30" },
  { code: "WB-TX-M-3X16-80T.8L", dimension: "3x16", capacity: "80", division: "50" },
  { code: "WB-TX-L-3X18-100T.8L", dimension: "3x18", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X20-120T.8L", dimension: "3x20", capacity: "120", division: "50" },
];

// ---------------- ADVANCED TECH ----------------
const advancedTechData = [
  {
    icon: Factory,
    title: "CNC Controlled Production",
    description: "Robotic welding & CNC automation for zero-error manufacturing.",
    details: `
<h3>Key Benefits</h3>
<ul>
<li>Robotic welding ensures <strong>consistent, crack-free joints</strong>.</li>
<li>CNC-drilled connection points = <strong>precision fit</strong> and stronger structures.</li>
<li>Smooth finishing improves <strong>paint adhesion & corrosion resistance</strong>.</li>
<li>Homogeneous design eliminates weak points found in manual welds.</li>
<li>Longer service life even under <strong>extreme truck load stress</strong>.</li>
</ul>`,
    images: ["/images/cnc-machine.jpg"],
  },
  {
    icon: SlidersHorizontal,
    title: "LoadGuard Mounting Kits",
    description: "100% steel kits that guarantee long-term accuracy with zero creep.",
    details: `
<h3>Features</h3>
<ul>
<li>No rubber parts → won’t deform over time.</li>
<li>Works with Rocker Column load cells to compensate side loads.</li>
<li>Eliminates the need for check rods and bumper bolts.</li>
<li>Thermal expansion tolerance prevents seasonal inaccuracies.</li>
<li>Cuts service costs nearly to zero while maintaining accuracy.</li>
</ul>`,
    images: ["/images/mounting-kit.jpg"],
  },
  {
    icon: Cpu,
    title: "High Accuracy Load Cells",
    description: "IP68/IP69K stainless steel load cells with overload & lightning protection.",
    details: `
<h3>Features</h3>
<ul>
<li>Hermetically sealed stainless steel (IP68/IP69K).</li>
<li>Withstands immersion & high-pressure washdowns.</li>
<li>150% safe load, 300% ultimate load protection.</li>
<li>Rocker Column self-centering ensures stable readings.</li>
<li>Certified to OIML R60 Class C3 for trade use.</li>
<li>Built-in lightning & surge protection.</li>
</ul>`,
    images: ["/images/loadcell.jpg"],
  },
  {
    icon: Monitor,
    title: "Load Line 2 Management System",
    description: "Smart truck scale management software with detailed reporting.",
    details: `
<h3>Features</h3>
<ul>
<li>Windows-based WinSCALE software, easy to use.</li>
<li>Multilingual support (6+ languages).</li>
<li>Automatic date/time logging & vehicle IDs.</li>
<li>Detailed daily/monthly reports exportable to Excel.</li>
<li>Connects with printers, RFID, remote displays, cameras.</li>
<li>Preset tare memory for frequent vehicles.</li>
</ul>`,
    images: ["/images/software-ui.jpg"],
  },
  {
    icon: Camera,
    title: "Automation & Security Options",
    description: "Smart add-ons for safety, automation, and monitoring.",
    details: `
<h3>Options</h3>
<ul>
<li>Steel side rails protect truck tires, ensure safe entry/exit.</li>
<li>Barriers with RFID → operator-free weighing.</li>
<li>IP Cameras → capture numberplates & log images.</li>
<li>Remote LED displays show weights outside cabins.</li>
<li>Message terminals guide drivers visually.</li>
<li>Explosion-proof (ATEX) versions available.</li>
</ul>`,
    images: ["/images/security-options.jpg"],
  },
  {
    icon: Cpu,
    title: "Smart Diagnostics & Predictive Maintenance",
    description: "IoT sensors & analytics predict failures before they happen.",
    details: `
<h3>Features</h3>
<ul>
<li>IoT-enabled load cells send live health data to the cloud.</li>
<li>Real-time alerts for overloads, cable faults, drift issues.</li>
<li>Predictive analytics forecasts service intervals.</li>
<li>Mobile dashboard gives managers 24/7 access.</li>
<li>Reduced downtime → catch issues before failures.</li>
</ul>`,
    images: ["/images/iot-dashboard.jpg", "/images/predictive-sensors.jpg"],
  },
];

// ---------------- COMPONENT ----------------
export default function Catalogue() {
  const [showModal, setShowModal] = useState(false);
  const [selectedTech, setSelectedTech] = useState<typeof advancedTechData[0] | null>(null);
  const [lightboxImg, setLightboxImg] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  const filteredModels = models.filter(m =>
    m.code.toLowerCase().includes(search.toLowerCase()) ||
    m.dimension.includes(search) ||
    m.capacity.includes(search)
  );
  const totalPages = Math.ceil(filteredModels.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const paginatedModels = filteredModels.slice(startIndex, startIndex + itemsPerPage);

  const openTechModal = (tech: typeof advancedTechData[0]) => {
    setSelectedTech(tech);
    setShowModal(true);
  };
  const closeTechModal = () => {
    setShowModal(false);
    setSelectedTech(null);
  };

  return (
    <div className="min-h-screen bg-white text-gray-800">
      <Navbar />

      {/* Hero */}
      <section
        className="relative text-white py-20 md:py-32"
        style={{ backgroundImage: `url(${CatalogueImage.src})`, backgroundSize: 'cover' }}
      >
        <div className="absolute inset-0 bg-black/60"></div>
        <div className="relative container mx-auto px-4 text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Product Catalogue</h1>
          <p className="text-lg md:text-xl">Home / <span className="text-amber-400">Catalogue</span></p>
        </div>
      </section>

      {/* Intro */}
      <section className="py-16 text-center">
        <div className="container mx-auto px-4">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Our Products</h2>
          <h3 className="text-4xl font-bold mb-6">Comprehensive Weighing Solutions</h3>
          <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
            Explore our full range of weighbridges, load cells, automation, and accessories designed for
            industrial and commercial use.
          </p>
          <a
            href="/files/QslCatalogue.pdf"
            download="QSL-Catalogue.pdf"
            className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-3 px-8 rounded-lg shadow-lg inline-flex items-center gap-2"
          >
            <Download className="w-5 h-5" /> Download Full Catalogue (PDF)
          </a>
        </div>
      </section>

      {/* Models Table */}
      <section className="bg-gray-50 py-16">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-8">Weighbridge Models</h2>
          <div className="mb-6 flex justify-center">
            <input
              type="text"
              placeholder="Search by code, dimension, or capacity..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setCurrentPage(1); }}
              className="w-full md:w-1/2 px-4 py-2 border rounded-lg shadow-sm"
            />
          </div>
          <motion.div
            initial={{ opacity: 0, y: 40 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
            className="bg-white rounded-lg shadow-lg overflow-hidden"
          >
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-amber-500 text-white">
                  <tr>
                    <th className="px-6 py-4 text-left">Model Code</th>
                    <th className="px-6 py-4 text-left">Dimension (m)</th>
                    <th className="px-6 py-4 text-left">Capacity (T)</th>
                    <th className="px-6 py-4 text-left">Division (kg)</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedModels.map((m, i) => (
                    <tr key={i} className={i % 2 === 0 ? "bg-gray-50" : "bg-white"}>
                      <td className="px-6 py-4 font-mono">{m.code}</td>
                      <td className="px-6 py-4">{m.dimension}</td>
                      <td className="px-6 py-4">{m.capacity}</td>
                      <td className="px-6 py-4">{m.division}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </motion.div>
          {/* Pagination */}
          <div className="flex justify-center items-center mt-6 gap-2">
            <button
              disabled={currentPage === 1}
              onClick={() => setCurrentPage((p) => p - 1)}
              className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50"
            >
              Prev
            </button>
            <span>Page {currentPage} of {totalPages}</span>
            <button
              disabled={currentPage === totalPages}
              onClick={() => setCurrentPage((p) => p + 1)}
              className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      </section>

      {/* Advanced Tech Section */}
      <section className="py-16">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-6">Advanced Technology</h2>
          <p className="text-gray-600 text-center mb-12 max-w-2xl mx-auto">
            Click on a card to learn more — explore detailed descriptions and zoomable images.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {advancedTechData.map((tech, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                whileHover={{ scale: 1.05 }}
                viewport={{ once: true }}
                transition={{ duration: 0.5, delay: i * 0.1 }}
                className="p-6 bg-gray-50 rounded-lg shadow hover:shadow-xl cursor-pointer"
                onClick={() => openTechModal(tech)}
              >
                <tech.icon className="w-10 h-10 text-amber-500 mb-4" />
                <h4 className="font-bold text-lg mb-2">{tech.title}</h4>
                <p className="text-gray-600 text-sm">{tech.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Modal */}
      <AnimatePresence>
        {showModal && selectedTech && (
          <motion.div
            className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              initial={{ y: 50, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              exit={{ y: 50, opacity: 0 }}
              transition={{ duration: 0.4 }}
              className="bg-white rounded-lg shadow-xl max-w-3xl w-full p-6 relative overflow-y-auto max-h-[90vh]"
            >
              <button onClick={closeTechModal} className="absolute top-4 right-4 text-gray-500 hover:text-gray-700">
                <X className="w-6 h-6" />
              </button>
              <div className="flex items-center mb-4">
                <selectedTech.icon className="w-10 h-10 text-amber-500 mr-3" />
                <h3 className="text-2xl font-bold">{selectedTech.title}</h3>
              </div>
              <div
                className="prose max-w-none text-gray-700 mb-6"
                dangerouslySetInnerHTML={{ __html: selectedTech.details }}
              />
              {selectedTech.images && (
                <div className="grid grid-cols-2 gap-4">
                  {selectedTech.images.map((img, idx) => (
                    <img
                      key={idx}
                      src={img}
                      alt="tech detail"
                      className="rounded-lg cursor-pointer hover:opacity-80"
                      onClick={() => setLightboxImg(img)}
                    />
                  ))}
                </div>
              )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Lightbox */}
      <AnimatePresence>
        {lightboxImg && (
          <motion.div
            className="fixed inset-0 bg-black/80 flex items-center justify-center z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.img
              src={lightboxImg}
              alt="enlarged"
              initial={{ scale: 0.8, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.8, opacity: 0 }}
              transition={{ duration: 0.4 }}
              className="max-h-[90%] max-w-[90%] rounded-lg shadow-lg"
            />
            <button
              onClick={() => setLightboxImg(null)}
              className="absolute top-6 right-6 text-white hover:text-amber-400"
            >
              <X className="w-8 h-8" />
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Scroll To Top */}
      <motion.button
        className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg"
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        whileHover={{ scale: 1.1 }}
        whileTap={{ scale: 0.95 }}
        aria-label="Scroll to top"
      >
        <ArrowUp className="w-6 h-6" />
      </motion.button>

      <Footer />
    </div>
  );
}
