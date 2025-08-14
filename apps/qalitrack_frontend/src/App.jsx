import { BrowserRouter, Routes, Route } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import Dashboard from './pages/Dashboard';
import Weighing from './pages/Weighing';
import Automation from './pages/Automation';
import Calibrations from './pages/Calibrations';
import Analytics from './pages/Analytics';
import Reports from './pages/Reports';
import System from './pages/System';

export default function App(){
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout/>}>
          <Route path="/" element={<Dashboard/>}/>
          <Route path="/weighing" element={<Weighing/>}/>
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
