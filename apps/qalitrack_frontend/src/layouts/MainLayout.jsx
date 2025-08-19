import { Outlet } from 'react-router-dom';
import Sidebar from '../components/Sidebar';
import Topbar from '../components/Topbar';

export default function MainLayout(){
  return (
    <div className="flex h-screen">
      <Sidebar/>
      <div className="flex flex-col flex-1">
        <Topbar/>
        <main className="flex-1 p-4 overflow-auto bg-gray-100">
          <Outlet/>
        </main>
      </div>
    </div>
  );
}
