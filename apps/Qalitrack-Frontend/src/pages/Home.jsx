// App.js
import React, { useState, useEffect } from 'react';

// Replaced imports from 'react-icons' with inline SVG icons to make the code self-contained.
// These icons are from the Lucide library, a good open-source alternative.

// SVG for Dashboard icon
const DashboardIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect width="7" height="9" x="3" y="3" rx="1" />
    <rect width="7" height="5" x="14" y="3" rx="1" />
    <rect width="7" height="9" x="14" y="12" rx="1" />
    <rect width="7" height="5" x="3" y="16" rx="1" />
  </svg>
);

// SVG for Scale icon
const ScaleIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="m16 16.5-3.5-2.5-3.5 2.5V2L8 4l2 2 2-2 2 2 2-2 2-2V18h-2" />
    <path d="M7 18.5a.5.5 0 0 1-.5-.5v-2.5a.5.5 0 0 1 1 0v2.5a.5.5 0 0 1-.5.5Z" />
    <path d="M10 18.5a.5.5 0 0 1-.5-.5v-2.5a.5.5 0 0 1 1 0v2.5a.5.5 0 0 1-.5.5Z" />
    <path d="M13 18.5a.5.5 0 0 1-.5-.5v-2.5a.5.5 0 0 1 1 0v2.5a.5.5 0 0 1-.5.5Z" />
    <path d="M16 18.5a.5.5 0 0 1-.5-.5v-2.5a.5.5 0 0 1 1 0v2.5a.5.5 0 0 1-.5.5Z" />
    <path d="M19 18.5a.5.5 0 0 1-.5-.5v-2.5a.5.5 0 0 1 1 0v2.5a.5.5 0 0 1-.5.5Z" />
    <path d="M6.5 12h11" />
  </svg>
);

// SVG for List Checks icon
const ListChecksIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="m3 12 2 2 4-4" />
    <path d="M19 6H9" />
    <path d="M19 12H9" />
    <path d="M19 18H9" />
  </svg>
);

// SVG for Bar Chart icon
const BarChartIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="12" x2="12" y1="20" y2="10" />
    <line x1="18" x2="18" y1="20" y2="4" />
    <line x1="6" x2="6" y1="20" y2="16" />
    <line x1="12" x2="12" y1="20" y2="10" />
    <line x1="18" x2="18" y1="20" y2="4" />
    <line x1="6" x2="6" y1="20" y2="16" />
  </svg>
);

// SVG for Truck icon
const TruckIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M14 18V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v11a1 1 0 0 0 1 1h2" />
    <path d="M15 18h6a1 1 0 0 0 1-1v-4a1 1 0 0 0-1-1h-6v6Z" />
    <circle cx="7" cy="18" r="2" />
    <path d="M14 18h1" />
    <circle cx="17" cy="18" r="2" />
  </svg>
);

// SVG for Alert Circle icon
const AlertCircleIcon = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <circle cx="12" cy="12" r="10" />
    <line x1="12" x2="12" y1="8" y2="12" />
    <line x1="12" x2="12.01" y1="16" y2="16" />
  </svg>
);

// SVG for Loading Spinner
const LoadingSpinner = (props) => (
  <svg
    {...props}
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M21 12a9 9 0 1 1-6.219-8.56" />
  </svg>
);


const Dashboard = () => {
  const [loading, setLoading] = useState(true);
  const [metrics, setMetrics] = useState({
    trucksInQueue: 0,
    totalThroughput: 0,
    activeWeighbridges: 0,
    alerts: 0,
  });

  const [weighbridgeStatus, setWeighbridgeStatus] = useState([
    { id: 'WB-01', location: 'Nairobi Grinding', status: 'Active', color: 'bg-green-500', isAvailable: true },
    { id: 'WB-02', location: 'Mombasa Plant', status: 'Idle', color: 'bg-gray-400', isAvailable: false },
    { id: 'WB-03', location: 'Kitui Road', status: 'Active', color: 'bg-green-500', isAvailable: true },
    { id: 'WB-04', location: 'Mbaraki', status: 'Maintenance', color: 'bg-yellow-500', isAvailable: false },
  ]);

  // Simulate data fetching
  useEffect(() => {
    setTimeout(() => {
      setMetrics({
        trucksInQueue: 7,
        totalThroughput: 124,
        activeWeighbridges: 2,
        alerts: 3,
      });
      setLoading(false);
    }, 1500);
  }, []);

  const Card = ({ title, value, icon, loading }) => (
    <div className="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow duration-300 flex flex-col justify-between">
      <div className="flex justify-between items-start">
        <h3 className="text-xl font-semibold text-gray-800">{title}</h3>
        <div className="p-2 bg-indigo-100 text-indigo-600 rounded-full">
          {icon}
        </div>
      </div>
      <div className="mt-4 text-4xl font-bold text-gray-900">
        {loading ? (
          <LoadingSpinner className="animate-spin text-indigo-500" />
        ) : (
          <span>{value}</span>
        )}
      </div>
    </div>
  );

  return (
    <div className="flex min-h-screen bg-gray-100 font-sans text-gray-800">
      {/* Sidebar */}
      <aside className="w-64 bg-gray-800 text-white p-4 hidden md:flex flex-col">
        <div className="flex items-center space-x-2 p-2">
          <DashboardIcon className="h-8 w-8 text-indigo-400" />
          <h1 className="text-2xl font-bold">Bamburi Dispatch</h1>
        </div>
        <nav className="mt-8">
          <ul>
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg bg-indigo-600 text-white shadow-lg">
                <DashboardIcon className="h-6 w-6" />
                <span>Dashboard</span>
              </a>
            </li>
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg text-gray-300 hover:bg-gray-700">
                <ScaleIcon className="h-6 w-6" />
                <span>Weighing Module</span>
              </a>
            </li>
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg text-gray-300 hover:bg-gray-700">
                <BarChartIcon className="h-6 w-6" />
                <span>Analytics</span>
              </a>
            </li>
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg text-gray-300 hover:bg-gray-700">
                <ListChecksIcon className="h-6 w-6" />
                <span>Reports</span>
              </a>
            </li>
          </ul>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 p-6 md:p-10">
        <header className="mb-6 flex justify-between items-center">
          <h2 className="text-3xl font-bold text-gray-900">Dashboard Overview</h2>
        </header>

        {/* KPI Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <Card 
            title="Trucks in Queue" 
            value={metrics.trucksInQueue} 
            icon={<TruckIcon className="h-6 w-6" />}
            loading={loading}
          />
          <Card 
            title="Total Daily Throughput" 
            value={metrics.totalThroughput} 
            icon={<ScaleIcon className="h-6 w-6" />}
            loading={loading}
          />
          <Card 
            title="Active Weighbridges" 
            value={metrics.activeWeighbridges} 
            icon={<DashboardIcon className="h-6 w-6" />}
            loading={loading}
          />
          <Card 
            title="System Alerts" 
            value={metrics.alerts} 
            icon={<AlertCircleIcon className="h-6 w-6" />}
            loading={loading}
          />
        </div>

        {/* Real-time Status and Analytics */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Weighbridge Status Panel */}
          <div className="lg:col-span-1 bg-white p-6 rounded-xl shadow-lg">
            <h3 className="text-xl font-semibold mb-4">Weighbridge Status</h3>
            <ul className="space-y-4">
              {weighbridgeStatus.map((wb) => (
                <li key={wb.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                  <div className="flex items-center space-x-3">
                    <span className={`h-3 w-3 rounded-full ${wb.color}`}></span>
                    <div>
                      <p className="font-bold">{wb.id}</p>
                      <p className="text-sm text-gray-500">{wb.location}</p>
                    </div>
                  </div>
                  <span className={`text-sm font-semibold px-3 py-1 rounded-full ${
                    wb.isAvailable ? 'bg-green-100 text-green-700' : 'bg-gray-200 text-gray-700'
                  }`}>
                    {wb.status}
                  </span>
                </li>
              ))}
            </ul>
          </div>

          {/* TTAT Analytics Panel */}
          <div className="lg:col-span-2 bg-white p-6 rounded-xl shadow-lg">
            <h3 className="text-xl font-semibold mb-4">TTAT Analytics (Last 7 Days)</h3>
            <div className="h-64 bg-gray-100 rounded-lg flex items-center justify-center text-gray-500">
              <p>Placeholder for chart visualization</p>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

// Main App component

