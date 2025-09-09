import React from 'react';

//Import auth pages
import Login from '../pages/auth/Login.jsx';
import Signup from '../pages/auth/Signup.jsx';
// Import general pages from the 'pages' directory
import Home from '../pages/Home.jsx';
import About from '../pages/About.jsx';
import Services from '../pages/Services.jsx'; // Assuming you have a Services.jsx page
import Catalogue from '../pages/Catalogue.jsx';
import Contact from '../pages/Contact.jsx';
import Dashboard from '../pages/Dashboard.jsx';
 //support pages
  import { Disclaimer } from '../components/support/Disclaimer.jsx';
  import { Support } from '../components/support/Support.jsx';
  import { FAQ } from '../components/support/FAQ.jsx';
  import { TermsConditions } from '../components/support/Terms.jsx';
  import { PrivacyPolicy } from '../components/support/PrivacyPolicy.jsx';

// Import all your Weighing pages 0722450779
import MultideckSingledecksPage from '../components/weighing/WeighingSoftware.jsx';
import PortableAxleWeighersPage from '../components/weighing/PortableAxle.jsx';
import WeighingSoftwarePage from '../components/weighing/WeighingSoftwarepage.jsx';
import UnmannedAutomatedWeighbridgesPage from '../components/weighing/UnmannedAutomatedWeighbridges.jsx';
import RetailScalesPage from '../components/weighing/RetailScales.jsx';
import OnboardWeighingPage from '../components/weighing/OnboardWeighing.jsx';
import WeighbridgeAccessoriesPage from '../components/weighing/WeighbridgeAccessories.jsx';

// Import all your Calibration pages
import VolumetricTankCalibrationPage from '../components/calibrations/VolumetricTankCalibration.jsx';
import MultideckSingleDeckCalibrationPage from '../components/calibrations/MultideckSingleDeckCalibration.jsx';
import FlowMetersPressureCalibrationsPage from '../components/calibrations/FlowPressure.jsx';

// Import all your Automation pages
import BuildingManagementSystemsPage from '../components/automations/BuildingManagement.jsx'; // Corrected folder from 'automations' to 'automation'
import IndustrialAutomationPage from '../components/automations/IndustrialAutomation.jsx';
import IntelligentTransportSystemsPage from '../components/automations/IntelligentTransport.jsx';



 
const routes = [
   // Auth Pages
  { path: 'login', element: <Login/> },
  { path: 'signup', element: <Signup/> },

  // General Pages
  { path: '/', element: <Home /> },
  { path: '/about', element: <About /> },
  { path: '/services', element: <Services /> },
  { path: '/catalogue', element: <Catalogue /> },
  { path: '/contact', element: <Contact /> },
  { path: '/dashboard', element: <Dashboard /> },
  // Support Pages
  {path: '/support/disclaimer', element: <Disclaimer /> }, 
  {path: '/support/support', element: <Support /> },
  {path: '/support/faq', element: <FAQ /> },
  {path: '/support/terms', element: <TermsConditions /> },
  {path: '/support/privacy', element: <PrivacyPolicy /> },
  // Weighing Routes
  { path: '/weighing/multideck-singledeck', element: <MultideckSingledecksPage /> },
  { path: '/weighing/portable-axle', element: <PortableAxleWeighersPage /> },
  { path: '/weighing/software', element: <WeighingSoftwarePage /> },
  { path: '/weighing/unmanned-automated', element: <UnmannedAutomatedWeighbridgesPage /> },
  { path: '/weighing/retail-scales', element: <RetailScalesPage /> },
  { path: '/weighing/onboard-weighing', element: <OnboardWeighingPage /> },
  { path: '/weighing/accessories', element: <WeighbridgeAccessoriesPage /> },

  // Calibration Routes
  { path: '/calibration/volumetric-tank', element: <VolumetricTankCalibrationPage /> },
  { path: '/calibration/multideck-singledeck', element: <MultideckSingleDeckCalibrationPage /> },
  { path: '/calibration/flow-pressure', element: <FlowMetersPressureCalibrationsPage /> },

  // Automation Routes
  { path: '/automation/building-management', element: <BuildingManagementSystemsPage /> },
  { path: '/automation/industrial', element: <IndustrialAutomationPage /> },
  { path: '/automation/intelligent-transport', element: <IntelligentTransportSystemsPage /> },

  // Add routes for Register, Login, Dashboard if they are full pages
  // { path: '/register', element: <RegisterPage /> },
  // { path: '/login', element: <LoginPage /> },
  // { path: '/dashboard', element: <DashboardPage /> },
];

export default routes;
