'use client';

import React, { useState } from 'react';
import {
  ArrowUp,
  Download,
  Factory,
  SlidersHorizontal,
  Cpu,
  Monitor,
  Camera,
  X,
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import CatalogueImage from '@/assets/catalogbg.jpg';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import Newsletter from '@/components/ContactCallToAction';
import Partners from '@/components/CertificationsLogos';

// ---------------- PRODUCT SHOWCASE ----------------
const productShowcase = [
  {
    title: "Truck Scale",
    desc:
      "The TX-BRIDGE truck scale is engineered for industries that demand accuracy, durability, and seamless integration. Its modular design allows fast installation, while robotic welding ensures long service life. Ideal for heavy-duty transport hubs, mining, and manufacturing facilities.",
    img: "/truckscale.jpeg",
    details: `
      <p>The TX-BRIDGE truck scale is a flagship solution built for industries that demand strength and reliability. With heavy-duty construction and a modular platform design, these weighbridges are perfect for mines, transport hubs, and manufacturing plants where high traffic and extreme loads are routine.</p>
      <p>Each scale section is fabricated using CNC-controlled machinery and robotic welding, ensuring structural integrity with minimal human error. This high-precision build results in uniform quality, crack-free joints, and superior weight distribution across the platform.</p>
      <p>Beyond structural robustness, the TX-BRIDGE system offers compatibility with advanced automation tools. It integrates seamlessly with RFID vehicle recognition, surveillance cameras, and management software, transforming a traditional weighbridge into a fully automated logistics solution.</p>
    `,
    images: [
      "/truckscale1.jpg",
      "/truckscale2.jpg",
      "/truckscale3.jpg",
      "/truckscale4.jpg",
    ],
  },
  {
    title: "For Light Vehicles",
    desc:
      "Designed for vans, pickups, and smaller trucks, these weighbridges provide compact yet reliable weighing. Their portable nature allows flexible deployment, making them ideal for airports, depots, and commercial hubs where precision and speed matter most.",
    img: "/lightvehicle.jpeg",
    details: `
      <p>Our light vehicle weighbridges are engineered to deliver the same accuracy and reliability as their heavy-duty counterparts, but with a compact footprint. Ideal for logistics depots, airports, and commercial hubs, they provide fast and precise weighing without the space demands of a truck-scale system.</p>
      <p>Designed for vans, pickups, and small trucks, these platforms are made from durable steel but optimized to reduce overall weight, making them easy to install and relocate. The robust design ensures long service life even in environments with constant vehicle throughput.</p>
      <p>Operators benefit from a streamlined process thanks to intuitive software integration. These weighbridges can be paired with barcode or RFID systems, enabling swift vehicle identification and automated record keeping, ensuring accuracy and efficiency in busy facilities.</p>
    `,
    images: ["/light1.jpg", "/light2.jpg", "/light3.jpg", "/light4.jpg"],
  },
  {
    title: "Weighing Containers",
    desc:
      "Specially built for shipping and logistics, these scales handle standard container sizes while ensuring compliance with international SOLAS regulations. They guarantee safety, reduce overload risks, and integrate with customs and port management systems.",
    img: "/container.png",
    details: `
      <p>Container weighbridges are specialized systems designed to meet international shipping standards, including the Safety of Life at Sea (SOLAS) regulations. They ensure accurate verification of container weights, preventing overloads and ensuring safe transport by sea, rail, or road.</p>
      <p>Each system is tailored to handle standard shipping containers, with modular configurations that allow quick installation in ports, freight yards, or inland container depots. The robust steel structure is engineered to endure heavy loading cycles while maintaining consistent accuracy.</p>
      <p>With full integration capabilities, these container scales can connect to port management software, customs systems, and automation tools. This not only ensures compliance but also streamlines container handling, improving throughput in busy terminals.</p>
    `,
    images: [
      "/container1.jpg",
      "/container2.jpg",
      "/container3.jpg",
      "/container4.jpg",
    ],
  },
  {
    title: "Axle Weighing",
    desc:
      "Axle weighing systems deliver fast, accurate, and reliable axle load data for vehicles on the move. Installed flush on roads or depots, they improve compliance, extend fleet life, and reduce overloading risks in logistics and transport.",
    img: "/axle.jpeg",
    details: `
      <p>Axle weighing systems provide a fast and efficient way to measure vehicle loads by axle, helping prevent overloading and improving road safety. Installed flush with the ground, they allow vehicles to be weighed dynamically without interrupting traffic flow.</p>
      <p>These systems are built from high-grade steel and incorporate advanced load cell technology that delivers precise readings even in harsh outdoor conditions. Whether installed in highways, industrial plants, or transport depots, they ensure compliance with load regulations and extend vehicle lifespan by promoting proper load distribution.</p>
      <p>With data logging and integration options, axle weighbridges support traffic enforcement, fleet monitoring, and safety audits. Their rugged design, low maintenance requirements, and high throughput capacity make them indispensable tools for both government authorities and private operators.</p>
    `,
    images: ["/axle1.jpg", "/axle2.jpg", "/axle3.jpg", "/axle4.jpg"],
  },
];

// ---------------- ADVANCED TECH ----------------
const advancedTechData = [
  {
    icon: Factory,
    title: "CNC Controlled Production Automation",
    description: "Robotic welding & CNC machining ensure robust and error-free weighbridge structures.",
    details: `
      <ul>
        <li>Zero-error welding with automated CNC drilling.</li>
        <li>Homogeneous, crack-free construction eliminates weak points.</li>
        <li>Provides excellent paint adhesion and corrosion resistance.</li>
        <li>Durable V-beam structure ensures extraordinary load durability and long service life.</li>
      </ul>`,
    images: ["/cnc.webp"],
  },
  {
    icon: SlidersHorizontal,
    title: "LoadGuard Mounting Kits",
    description: "100% steel kits designed to eliminate creep, service costs, and weighing errors.",
    details: `
      <ul>
        <li>Works with Rocker Column load cells to neutralize side loads.</li>
        <li>Eliminates the need for check rods and bumper bolts.</li>
        <li>Thermal expansion tolerance ensures seasonal accuracy.</li>
        <li>Oscillation control improves weighing precision even with moving vehicles.</li>
      </ul>`,
    images: ["/loadingkit.webp"],
  },
  {
    icon: Cpu,
    title: "High Accuracy Stainless Steel Load Cells",
    description: "Hermetically sealed IP68/IP69K load cells with lightning and overload protection.",
    details: `
      <ul>
        <li>Withstands immersion & high-pressure washdowns.</li>
        <li>150% safe load, 300% ultimate load protection.</li>
        <li>Self-centering Rocker Column design guarantees stable readings.</li>
        <li>Certified to OIML R60 Class C3 for trade applications.</li>
      </ul>`,
    images: ["/loadcells.webp"],
  },
  // {
  //   icon: Monitor,
  //   title: "Load Line 2 Truck Scale Management System",
  //   description: "Smart software that simplifies operations with multi-language support & detailed reporting.",
  //   details: `
  //     <ul>
  //       <li>Windows-based WinSCALE software with clear menus.</li>
  //       <li>Supports 6+ languages.</li>
  //       <li>Generates detailed daily, monthly, or custom reports.</li>
  //       <li>Connects seamlessly to printers, RFID, cameras, and automation systems.</li>
  //     </ul>`,
  //   images: ["/load2cell.jpg"],
  // },
  // {
  //   icon: Camera,
  //   title: "Automation & Security Options",
  //   description: "Advanced add-ons for safety and automation.",
  //   details: `
  //     <ul>
  //       <li>Steel side rails protect trucks and ensure safe entry/exit.</li>
  //       <li>Automatic barriers with RFID allow operator-free weighing.</li>
  //       <li>IP Cameras capture number plates and integrate with databases.</li>
  //       <li>Remote LED displays and message terminals guide drivers visually.</li>
  //     </ul>`,
  //   images: ["/automation.jpeg"],
  // },
];

// ---------------- COMPONENT ----------------
export default function Catalogue() {
  const [selectedProduct, setSelectedProduct] = useState<
    (typeof productShowcase)[0] | null
  >(null);
  const [selectedTech, setSelectedTech] = useState<(typeof advancedTechData)[0] | null>(
    null
  );
  const [showTechModal, setShowTechModal] = useState(false);
  const [lightboxImg, setLightboxImg] = useState<string | null>(null);

  // Secure catalogue download
  const [showDownloadModal, setShowDownloadModal] = useState(false);
  const [form, setForm] = useState({ name: '', email: '' });
  const [accessGranted, setAccessGranted] = useState(false);

  const handleDownloadSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Replace this with actual lead capture logic (API call) if required.
    console.log('Lead captured:', form);
    setAccessGranted(true);
    setShowDownloadModal(false);
  };

  return (
    <div className="min-h-screen bg-white text-gray-800">
      <Navbar />

      {/* Hero */}
      <section
        className="relative text-white py-20 md:py-28"
        style={{
          backgroundImage: `url(${CatalogueImage.src})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
        }}
      >
        <div className="absolute inset-0 bg-black/60" />
        <div className="relative container mx-auto px-4 text-center">
          <h1 className="text-4xl md:text-6xl font-bold mb-4">Product Catalogue</h1>
          {/* <p className="text-md md:text-lg max-w-2xl mx-auto">
            Explore QSL’s complete line of industrial weighing solutions — from heavy-duty truck
            scales to container systems and axle weighbridges. Click any product to learn more or
            download the full catalogue.
          </p> */}
        </div>
      </section>

      {/* Intro + Secured Download */}
      <section className="py-12 text-center">
        <div className="container mx-auto px-4">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Our Products</h2>
          <h3 className="text-3xl md:text-4xl font-bold mb-4">Comprehensive Weighing Solutions</h3>
          <p className="text-gray-600 mb-8 max-w-3xl mx-auto">
            We deliver precision, durability, and automation-ready systems for industrial and
            commercial use. Get the official QSL catalogue by entering your email below.
          </p>

          {!accessGranted ? (
            <button
              onClick={() => setShowDownloadModal(true)}
              className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-3 px-6 rounded-lg shadow inline-flex items-center gap-2"
            >
              <Download className="w-5 h-5" /> Download Full Catalogue
            </button>
          ) : (
            <a
              href="/files/QSL-Catalogue.pdf"
              download="QSL-Catalogue.pdf"
              className="bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-6 rounded-lg shadow inline-flex items-center gap-2"
            >
              <Download className="w-5 h-5" /> Download Now
            </a>
          )}
        </div>
      </section>

      {/* Download Modal */}
      <AnimatePresence>
        {showDownloadModal && (
          <motion.div
            className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              initial={{ y: 30, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              exit={{ y: 30, opacity: 0 }}
              transition={{ duration: 0.25 }}
              className="bg-white rounded-lg shadow-xl max-w-md w-full p-6 relative"
            >
              <button
                onClick={() => setShowDownloadModal(false)}
                className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
              >
                <X className="w-6 h-6" />
              </button>
              <h3 className="text-2xl font-bold mb-2">Unlock the QSL Catalogue</h3>
              <p className="text-gray-600 mb-4">Enter your name and email to receive the download link.</p>

              <form onSubmit={handleDownloadSubmit} className="space-y-3">
                <input
                  type="text"
                  placeholder="Your name"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  required
                  className="w-full px-4 py-2 border rounded-lg"
                />
                <input
                  type="email"
                  placeholder="Your email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  required
                  className="w-full px-4 py-2 border rounded-lg"
                />
                <button
                  type="submit"
                  className="w-full bg-amber-500 hover:bg-amber-600 text-white font-semibold py-3 rounded-lg"
                >
                  Submit & Unlock
                </button>
              </form>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Product Showcase */}
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-4 space-y-12">
          {productShowcase.map((p, i) => (
            <div
              key={i}
              className={`grid md:grid-cols-2 gap-8 items-center ${i % 2 === 1 ? 'md:flex-row-reverse' : ''
                }`}
            >
              <img
                src={p.img}
                alt={p.title}
                className="w-full h-64 md:h-72 object-cover rounded-lg shadow-lg"
              />
              <div>
                <h3 className="text-2xl font-bold italic mb-3">{p.title}</h3>
                <p className="text-gray-600 mb-4">{p.desc}</p>

                {/* action buttons */}
                <div className="flex items-center gap-3">
                  <button
                    className="bg-black text-white px-5 py-2 rounded hover:bg-gray-800 transition"
                    onClick={() => setSelectedProduct(p)}
                  >
                    Learn More
                  </button>

                  {/* quick preview / anchor to download */}
                  <button
                    className="bg-transparent border border-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-100"
                    onClick={() => {
                      // small convenience: open download modal
                      setShowDownloadModal(true);
                    }}
                  >
                    Get Catalogue
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Product Modal with Sticky Header */}
      <AnimatePresence>
        {selectedProduct && (
          <motion.div
            className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              initial={{ y: 40, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              exit={{ y: 40, opacity: 0 }}
              transition={{ duration: 0.3 }}
              className="bg-white rounded-lg shadow-xl max-w-4xl w-full relative overflow-hidden flex flex-col max-h-[92vh]"
            >
              {/* Sticky Header */}
              <div className="sticky top-0 bg-white p-4 border-b flex justify-between items-center z-20">
                <div>
                  <h2 className="text-2xl font-semibold">{selectedProduct.title}</h2>
                  <p className="text-sm text-gray-500">{selectedProduct.desc}</p>
                </div>
                <button
                  onClick={() => setSelectedProduct(null)}
                  className="text-gray-500 hover:text-gray-700"
                  aria-label="Close"
                >
                  <X className="w-6 h-6" />
                </button>
              </div>

              {/* Scrollable Content */}
              <div className="overflow-y-auto p-6 space-y-6">
                <div
                  className="prose max-w-none text-gray-700"
                  dangerouslySetInnerHTML={{ __html: selectedProduct.details }}
                />

                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {selectedProduct.images.map((img, idx) => (
                    <img
                      key={idx}
                      src={img}
                      alt={`${selectedProduct.title} ${idx + 1}`}
                      className="rounded-lg shadow hover:opacity-80 cursor-pointer"
                      onClick={() => setLightboxImg(img)}
                    />
                  ))}
                </div>

                {/* CTA at bottom of modal */}
                <div className="pt-4 border-t mt-4 flex flex-col md:flex-row items-center justify-between gap-4">
                  <div>
                    <p className="text-sm text-gray-600">
                      Want the full specifications or a quote? Contact our sales team.
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <a
                      href="/contact"
                      className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded"
                    >
                      Contact Sales
                    </a>
                    <button
                      onClick={() => {
                        setSelectedProduct(null);
                        setShowDownloadModal(true);
                      }}
                      className="bg-transparent border border-gray-300 px-4 py-2 rounded"
                    >
                      Request Catalogue
                    </button>
                  </div>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Advanced Tech Section */}
      <section className="py-16">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-6">Advanced Technology</h2>
          <p className="text-gray-600 text-center mb-10 max-w-2xl mx-auto">
            Explore manufacturing and measurement innovations that make our systems accurate,
            durable, and low-maintenance.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {advancedTechData.map((tech, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 10 }}
                whileInView={{ opacity: 1, y: 0 }}
                whileHover={{ scale: 1.02 }}
                viewport={{ once: true }}
                transition={{ duration: 0.35, delay: i * 0.06 }}
                className="p-6 bg-gray-50 rounded-lg shadow hover:shadow-lg cursor-pointer"
                onClick={() => {
                  setSelectedTech(tech);
                  setShowTechModal(true);
                }}
              >
                <tech.icon className="w-10 h-10 text-amber-500 mb-4" />
                <h4 className="font-bold text-lg mb-2">{tech.title}</h4>
                <p className="text-gray-600 text-sm">{tech.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Tech Modal */}
      <AnimatePresence>
        {showTechModal && selectedTech && (
          <motion.div
            className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              initial={{ y: 30, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              exit={{ y: 30, opacity: 0 }}
              transition={{ duration: 0.25 }}
              className="bg-white rounded-lg shadow-xl max-w-3xl w-full p-6 relative overflow-y-auto max-h-[90vh]"
            >
              <button
                onClick={() => {
                  setShowTechModal(false);
                  setSelectedTech(null);
                }}
                className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
              >
                <X className="w-6 h-6" />
              </button>

              <div className="flex items-center gap-4 mb-4">
                <selectedTech.icon className="w-10 h-10 text-amber-500" />
                <h3 className="text-2xl font-bold">{selectedTech.title}</h3>
              </div>

              <div
                className="prose max-w-none mb-6 text-gray-700"
                dangerouslySetInnerHTML={{ __html: selectedTech.details }}
              />

              <div className="grid grid-cols-2 gap-4">
                {selectedTech.images.map((img, idx) => (
                  <img
                    key={idx}
                    src={img}
                    alt={`${selectedTech.title} ${idx + 1}`}
                    className="rounded-lg shadow hover:opacity-80 cursor-pointer"
                    onClick={() => setLightboxImg(img)}
                  />
                ))}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Lightbox */}
      <AnimatePresence>
        {lightboxImg && (
          <motion.div
            className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setLightboxImg(null)}
          >
            <img
              src={lightboxImg}
              alt="Lightbox"
              className="max-h-[90vh] max-w-[90vw] rounded shadow-lg"
            />
          </motion.div>
        )}
      </AnimatePresence>

      {/* Scroll To Top */}
      {/* <motion.button
        className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg"
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
        whileHover={{ scale: 1.08 }}
        whileTap={{ scale: 0.95 }}
        aria-label="Scroll to top"
      >
        <ArrowUp className="w-5 h-5" />
      </motion.button> */}

      <Newsletter />
      <Partners />
      <Footer />
    </div>
  );
}
