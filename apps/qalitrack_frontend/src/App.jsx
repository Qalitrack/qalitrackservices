import { BrowserRouter, Routes, Route } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import Dashboard from './pages/Dashboard';
import FactoryWeighing from './pages/Weighing';
import Automation from './pages/Automation';
import Calibrations from './pages/Calibrations';
import Analytics from './pages/Analytics';
import Reports from './pages/Reports';
import System from './pages/System';
import Vehicle from './components/weighing/Vehicles';
import Drivers from './components/weighing/Drivers';

export default function App(){
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout/>}>
          <Route path="/" element={<Dashboard/>}/>
          <Route path="/weighing/factory" element={<FactoryWeighing/>}/>
          <Route path="/weighing/vehicle" element={<Vehicle/>}/>
          <Route path="/weighing/drivers" element={<Drivers/>}/>
          <Route path="/automation" element={<Automation/>}/>
          <Route path="/calibrations" element={<Calibrations/>}/>
          <Route path="/analytics" element={<Analytics/>}/>
          <Route path="/reports" element={<Reports/>}/>
          <Route path="/system" element={<System/>}/>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
