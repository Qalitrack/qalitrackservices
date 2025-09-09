import React, { useState } from 'react';
import {
  MapPin, Phone, Mail, User, LogIn, LayoutDashboard, ChevronDown, // Top bar icons
  ArrowUp, // Scroll to top
  Blocks, Cog, Gauge, Camera, Monitor, Scan, HardHat, Shield, Flame, Barcode, // Feature icons
  Download, CheckCircle, Truck, Settings, Factory, ShieldCheck, Settings2, MessageSquare, Package, Lightbulb, // Specific feature icons
  X // Close icon for modal
} from 'lucide-react';

// Assuming your Navbar and Footer components are in src/components/
import Navbar from '../components/Navbar'; 
import Footer from '../components/Footer';
import Catalogoue from '../assets/catalogbg.jpg'

// Data for weighbridge models (complete from PDF)
const models = [
  { code: "WB-TX-S-3X8-60T.6L", dimension: "3X8", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X10-60T.6L", dimension: "3X10", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X12-60T.6L", dimension: "3X12", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X14-60T.8L", dimension: "3X14", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X16-60T.8L", dimension: "3X16", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X18-60T.8L", dimension: "3X18", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X20-60T.10L", dimension: "3X20", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X22-60T.10L", dimension: "3X22", capacity: "60", division: "30" },
  { code: "WB-TX-S-3X24-60T.10L", dimension: "3X24", capacity: "60", division: "30" },

  { code: "WB-TX-M-3X12-80T.6L", dimension: "3X12", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X14-80T.8L", dimension: "3X14", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X16-80T.8L", dimension: "3X16", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X18-80T.8L", dimension: "3X18", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X20-80T.10L", dimension: "3X20", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X22-80T.10L", dimension: "3X22", capacity: "80", division: "50" },
  { code: "WB-TX-M-3X24-80T.10L", dimension: "3X24", capacity: "80", division: "50" },

  { code: "WB-TX-L-3X16-100T.8L", dimension: "3X16", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X18-100T.8L", dimension: "3X18", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X20-100T.10L", dimension: "3X20", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X22-100T.10L", dimension: "3X22", capacity: "100", division: "50" },
  { code: "WB-TX-L-3X24-100T.10L", dimension: "3X24", capacity: "100", division: "50" },
];

// Data for Advanced Technology details
const advancedTechData = [
  {
    icon: Gauge,
    title: "High Quality & Accuracy Load Cells",
    detailedDescription: `
      Our truck scales operate in harsh conditions, so load cells are designed to withstand major forces.
      They work underwater and can be washed by pressurized water.
      Standardized load cell outputs guarantee exact measurements at every point, eliminating corner adjustments.
      They feature IP68/IP69K protection, hermetic sealing, and are OIML R60 Class C3 certified.
      Protected with 150% safe load and 300% ultimate load limits, ensuring long-term performance.
      Lightning protection components provide maximum protection from strikes and heavy surges.
      The "Rocker Column" self-centering design ensures high measurement accuracy.
      An interlock mechanism prevents rotation caused by forces during vehicle entry/exit, eliminating cable twisting and incorrect weighing.
    `
  },
  {
    icon: ShieldCheck,
    title: "Junction Box",
    detailedDescription: `
      Our junction boxes offer high quality and long-lasting performance with IP67 protection class.
      They include lightning protection structures and require no maintenance.
    `
  },
  {
    icon: Settings2,
    title: "LoadGuard Special Design Mounting Kits",
    detailedDescription: `
      Made of 100% steel, ensuring durability and no replacement costs, unlike rubber/caoutchouc kits.
      Works in full compatibility with load cells' "Rocker Column" feature to compensate side loading effects.
      Eliminates the need for check rods and bumper bolts, simplifying installation and maintenance.
      With low degrees of freedom, it reduces the resonance period of forces from different directions, allowing faster and more accurate weighing.
      Specially designed to allow oscillation within certain limits, providing perfect weighing performance by controlling forces from vehicle movement and thermal expansion.
    `
  }
];

// Data for Key Features
const keyFeaturesData = [
  { icon: Blocks, title: "Modular Design", description: "Low profile modular design for easy and quick installation and commissioning." },
  { icon: Cog, title: "CNC-Controlled Production", description: "High quality through CNC controlled production automation and robotic welding technology." },
  { icon: HardHat, title: "Highly Durable V-Beam Structure", description: "Superior construction design provides extraordinary durability and long lifetime." },
  { icon: Gauge, title: "High Accuracy Weighing", description: "Robust construction and high accuracy with multiple load cells design." },
  { icon: Settings, title: "Minimum Service & Maintenance", description: "Designed for long service life and reduced maintenance needs." },
  { icon: CheckCircle, title: "Legal Metrology Approved", description: "2014/31/EU type approval, OIML R76 and CE certified." },
  { icon: Truck, title: "Easy Truck & Container Loading", description: "Low profile platform design provides great convenience during entry and exit." },
  { icon: Factory, title: "Versatile Application", description: "Suitable for under silo filling, excavation, and all businesses needing truck scales." },
];

// Data for Optional Features
const optionalFeaturesData = [
  { icon: Scan, title: "Automatic Vehicle ID System", description: "RF transmitter on truck transfers info without operator, for quick & safe weighing." },
  { icon: Barcode, title: "Automatic Vehicle ID Terminal", description: "Mounted at entry/exit, reads card without driver exiting for operator-free weighing." },
  { icon: Monitor, title: "Remote Display", description: "Observes vehicle weight value from outside cabin with 6 digits red LED indicator." },
  { icon: MessageSquare, title: "Message Terminal", description: "Guides and informs driver visually at truck scales with automation applications." },
  { icon: Package, title: "Cover Sheet", description: "Optional cover plate to close space between platform blocks." },
  { icon: Camera, title: "IP Camera", description: "For weighing security or transferring photos to database, with optional number plate reading." },
  { icon: Shield, title: "Barrier", description: "Mounted at scale entry/exit to control vehicle access during weighing process." },
  { icon: Lightbulb, title: "Traffic Lights", description: "Organizes vehicle entry/exit traffic based on weight indicator commands." },
  { icon: Blocks, title: "Steel Side Rail System", description: "Provides safe truck entry/exit, prevents tire damage with cylindrical design." },
  { icon: Flame, title: "Ex-Proof Options", description: "ATEX approved truck scales for flammable or explosive environments." },
];


const Catalogue = () => {
  // State for Advanced Technology Modal
  const [showTechModal, setShowTechModal] = useState(false);
  const [selectedTechDetail, setSelectedTechDetail] = useState(null);

  // State for "Show More" functionality in sections
  const [showAllKeyFeatures, setShowAllKeyFeatures] = useState(false);
  const [showAllOptionalFeatures, setShowAllOptionalFeatures] = useState(false);
  const [showAllModels, setShowAllModels] = useState(false);

  const initialKeyFeaturesCount = 6; // Display first 6 key features
  const initialOptionalFeaturesCount = 4; // Display first 4 optional features
  const initialModelsCount = 5; // Display first 5 table rows

  const displayedKeyFeatures = showAllKeyFeatures ? keyFeaturesData : keyFeaturesData.slice(0, initialKeyFeaturesCount);
  const displayedOptionalFeatures = showAllOptionalFeatures ? optionalFeaturesData : optionalFeaturesData.slice(0, initialOptionalFeaturesCount);
  const displayedModels = showAllModels ? models : models.slice(0, initialModelsCount);

  const openTechModal = (detail) => {
    setSelectedTechDetail(detail);
    setShowTechModal(true);
  };

  const closeTechModal = () => {
    setShowTechModal(false);
    setSelectedTechDetail(null);
  };

  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
    

      {/* Navbar Component */}
      <section className='fixed top-0 left-0 relative z-50'>
      <Navbar />
      </section>

      {/* Hero Section - Weighbridge Catalogue */}
      <section 
        className="relative bg-cover bg-center text-white py-20 md:py-32" 
        style={{ 
          backgroundImage: `url(${Catalogoue})`, // Placeholder for weighbridge-related image
          backgroundAttachment: 'fixed',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
          backgroundSize: 'cover'
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="container mx-auto px-4 relative z-10 text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Weighbridge Catalogue</h1>
          <p className="text-lg md:text-xl">Home / <span className="text-amber-400">Catalogue</span></p>
        </div>
      </section>

      {/* Key Features Section */}
      <section className="container mx-auto px-4 my-16 text-center">
        <h2 className="text-amber-500 text-lg font-semibold mb-2">TX-BRIDGE Series</h2>
        <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">Discover Our Robust & Highly Accurate Weighbridges</h3>
        <p className="text-gray-600 mb-12 max-w-3xl mx-auto">
          Designed for seamless installation and years of uninterrupted operation, our TX-BRIDGE series offers unparalleled precision and durability for all your heavy-duty weighing needs.
        </p>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8 text-left">
          {displayedKeyFeatures.map((feature, index) => (
            <div key={index} className="bg-white p-6 rounded-lg shadow-md border border-gray-200 flex flex-col items-center text-center h-full transition duration-200 hover:shadow-xl hover:scale-[1.02]">
              <div className="bg-amber-100 rounded-full p-4 mb-4">
                <feature.icon className="h-8 w-8 text-amber-500" />
              </div>
              <h4 className="font-bold text-xl mb-1">{feature.title}</h4>
              <p className="text-gray-600 text-sm flex-grow">{feature.description}</p>
            </div>
          ))}
        </div>
        {keyFeaturesData.length > initialKeyFeaturesCount && (
          <button 
            onClick={() => setShowAllKeyFeatures(!showAllKeyFeatures)}
            className="mt-12 bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200"
          >
            {showAllKeyFeatures ? 'Show Less' : 'Show More Features'}
          </button>
        )}
      </section>

      {/* Advanced Technology Section */}
      <section className="bg-gray-50 py-16">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Core Components</h2>
          <h3 className="text-4xl font-bold leading-tight mb-12 max-w-2xl mx-auto">Advanced Technology for Unrivaled Performance</h3>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {advancedTechData.map((tech, index) => (
              <div 
                key={index} 
                className="bg-white p-6 rounded-lg shadow-md border border-gray-200 flex flex-col items-center text-center h-full cursor-pointer transition duration-200 hover:shadow-xl hover:scale-[1.02]"
                onClick={() => openTechModal(tech)}
              >
                <div className="bg-amber-100 rounded-full p-4 mb-4">
                  <tech.icon className="h-8 w-8 text-amber-500" />
                </div>
                <h4 className="font-bold text-xl mb-1 flex-grow">{tech.title}</h4>
                <button className="text-amber-500 font-semibold hover:underline mt-4">Learn More</button>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Load Line 2 Truck Scale Management System Section */}
      <section className="container mx-auto px-4 my-16">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          {/* Left Column - Image */}
          <div>
            <img src={Catalogoue} alt="Management System" className="rounded-lg shadow-xl w-full h-auto max-h-[500px] object-cover" />
          </div>
          {/* Right Column - Text Details */}
          <div>
            <h2 className="text-amber-500 text-lg font-semibold mb-2">WinSCALE Software</h2>
            <h3 className="text-4xl font-bold leading-tight mb-6">Load Line 2 Truck Scale Management System</h3>
            <p className="text-gray-600 mb-4 leading-relaxed">
              Load Line 2 "Truck Scale Management System" integrates the Weight Indicator, Computer, Windows-based WinSCALE program, and printer for a seamless, user-friendly weighing operation.
            </p>
            <p className="text-gray-600 mb-8 leading-relaxed">
              It offers multi-language support, flexible data management, and generates detailed reports (daily, monthly, custom dates) viewable on-screen or printable to MS Excel/TXT.
            </p>
            
            <h4 className="font-bold text-xl text-gray-800 mb-3">Key Standard Features:</h4>
            <ul className="list-disc list-inside space-y-2 text-gray-700 text-sm">
              <li>OIML R76, CE certified.</li>
              <li>Standard PC keyboard, 18.5" wide colorful LCD monitor.</li>
              <li>Automatic data time and serial number, high disc capacity.</li>
              <li>Available in 6 different languages.</li>
              <li>Network connection, separate printer identification.</li>
              <li>Programmable data fields, rapid coding usage, preset tare memory.</li>
              <li>Automation options (RFID, multi-scale integration).</li>
            </ul>
          </div>
        </div>
      </section>

      {/* Optional Features Section */}
      <section className="bg-gray-50 py-16">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-amber-500 text-lg font-semibold mb-2">Enhance Your System</h2>
          <h3 className="text-4xl font-bold leading-tight mb-12 max-w-2xl mx-auto">Available Optional Features</h3>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8 text-left">
            {displayedOptionalFeatures.map((feature, index) => (
              <div key={index} className="bg-white p-6 rounded-lg shadow-md border border-gray-200 flex flex-col items-center text-center h-full transition duration-200 hover:shadow-xl hover:scale-[1.02]">
                <div className="bg-amber-100 rounded-full p-4 mb-4">
                  <feature.icon className="h-8 w-8 text-amber-500" />
                </div>
                <h4 className="font-bold text-xl mb-1">{feature.title}</h4>
                <p className="text-gray-600 text-sm flex-grow">{feature.description}</p>
              </div>
            ))}
          </div>
          {optionalFeaturesData.length > initialOptionalFeaturesCount && (
            <button 
              onClick={() => setShowAllOptionalFeatures(!showAllOptionalFeatures)}
              className="mt-12 bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200"
            >
              {showAllOptionalFeatures ? 'Show Less' : 'Show More Features'}
            </button>
          )}
        </div>
      </section>

      {/* Technical Specifications Section */}
      <section className="container mx-auto px-4 my-16 text-center">
        <h2 className="text-amber-500 text-lg font-semibold mb-2">Specifications</h2>
        <h3 className="text-4xl font-bold leading-tight mb-12 max-w-2xl mx-auto">Detailed Technical Specifications</h3>
        <div className="overflow-x-auto bg-white rounded-lg shadow-md border border-gray-200">
          <table className="min-w-full border-collapse text-sm text-center">
            <thead className="bg-amber-500 text-white">
              <tr>
                <th className="p-4 border border-gray-300">Model Code</th>
                <th className="p-4 border border-gray-300">Dimensions (m)</th>
                <th className="p-4 border border-gray-300">Capacity (ton)</th>
                <th className="p-4 border border-gray-300">Division (kg)</th>
              </tr>
            </thead>
            <tbody>
              {displayedModels.map((item, idx) => (
                <tr key={idx} className="odd:bg-gray-50 hover:bg-gray-100 transition duration-150">
                  <td className="p-4 border border-gray-300">{item.code}</td>
                  <td className="p-4 border border-gray-300">{item.dimension}</td>
                  <td className="p-4 border border-gray-300">{item.capacity}</td>
                  <td className="p-4 border border-gray-300">{item.division}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {models.length > initialModelsCount && (
          <button 
            onClick={() => setShowAllModels(!showAllModels)}
            className="mt-12 bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition duration-200"
          >
            {showAllModels ? 'Show Less Models' : 'Show All Models'}
          </button>
        )}
        <p className="mt-4 text-gray-600 text-sm">Multi range weighing alternatives for all sizes and dimensions.</p>
      </section>

      {/* Download Catalogue Section */}
      <section className="bg-gray-50 py-16 text-center">
        <div className="container mx-auto px-4">
          <h3 className="text-4xl font-bold leading-tight mb-6 max-w-2xl mx-auto">Ready to Learn More?</h3>
          <p className="text-gray-600 mb-8 max-w-3xl mx-auto">
            Download our comprehensive catalogue for detailed information on all TX-BRIDGE models and features.
          </p>
          <a
            href="../assets/qsl-catalogue (1).pdf" // Assuming this path is correct in your project
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center bg-amber-500 text-white px-8 py-3 rounded-lg hover:bg-amber-600 font-bold shadow-lg transition duration-200"
          >
            <Download className="h-5 w-5 mr-2" /> Download Full Catalogue (PDF)
          </a>
          <p className="mt-4 text-sm text-gray-700">
            Need custom sizing or have specific requirements? <a href="/contact" className="text-amber-500 underline hover:text-amber-600 font-semibold">Contact our team</a>.
          </p>
        </div>
      </section>

      {/* Advanced Technology Detail Modal */}
      {showTechModal && selectedTechDetail && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 relative">
            <button 
              onClick={closeTechModal} 
              className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
            >
              <X className="h-6 w-6" />
            </button>
            <h3 className="text-3xl font-bold text-amber-600 mb-4">{selectedTechDetail.title}</h3>
            <p className="text-gray-700 leading-relaxed mb-4 whitespace-pre-line">{selectedTechDetail.detailedDescription}</p>
            <button 
              onClick={closeTechModal} 
              className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-2 px-4 rounded-lg transition duration-200"
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* Scroll to top button */}
      <button
        className="fixed bottom-8 right-8 bg-amber-500 hover:bg-amber-600 text-white p-3 rounded-full shadow-lg transition duration-300 focus:outline-none focus:ring-2 focus:ring-amber-400"
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
      >
        <ArrowUp className="w-6 h-6" />
      </button>

      {/* Footer Component */}
      <Footer />
    </div>
  );
}

export default Catalogue;
