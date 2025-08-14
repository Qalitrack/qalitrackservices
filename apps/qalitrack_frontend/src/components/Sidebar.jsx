import { NavLink } from 'react-router-dom';

const links = [
  { to: '/', label: 'Dashboard' },
  { to: '/weighing', label: 'Weighing' },
  { to: '/automation', label: 'Automation' },
  { to: '/calibrations', label: 'Calibrations' },
  { to: '/analytics', label: 'Analytics' },
  { to: '/reports', label: 'Reports' },
  { to: '/system', label: 'System' },
];

export default function Sidebar(){
  return (
    <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
      <div className="px-4 py-4 font-bold text-xl">Bamburi</div>
      <nav className="flex-1 px-2 space-y-1">
        {links.map(l => (
          <NavLink key={l.to} to={l.to} end
            className={({isActive}) => `block px-3 py-2 rounded hover:bg-gray-100 ${isActive ? 'bg-gray-200 font-medium' : ''}`}>
            {l.label}
          </NavLink>
        ))}
      </nav>
      <div className="p-3 text-sm text-gray-500 border-t">v0.1</div>
    </aside>
  );
}
