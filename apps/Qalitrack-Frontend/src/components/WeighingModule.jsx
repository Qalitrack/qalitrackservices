// App.js
import React, { useState, useEffect } from 'react';

// SVG for a loading spinner
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

// SVG for a truck icon
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

// SVG for a scale icon
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


const WeighingModule = () => {
  const [currentWeight, setCurrentWeight] = useState(0);
  const [weighbridgeStatus, setWeighbridgeStatus] = useState('ready'); // 'ready', 'busy', 'offline'
  const [loading, setLoading] = useState(true);
  const [step, setStep] = useState('idle'); // 'idle', 'first_weigh', 'second_weigh', 'complete'
  const [message, setMessage] = useState('');
  const [firstWeigh, setFirstWeigh] = useState(null);
  const [secondWeigh, setSecondWeigh] = useState(null);
  const [vehicle, setVehicle] = useState({
    id: 'KAA 123B',
    type: 'Truck',
    loadStatus: 'Pending First Weigh',
  });

  // Simulate real-time weight updates
  useEffect(() => {
    if (weighbridgeStatus === 'busy') {
      const interval = setInterval(() => {
        // Simulate weight fluctuating around a value
        setCurrentWeight(prevWeight => Math.max(0, Math.round((Math.random() * 500) + 12000)));
      }, 500);
      return () => clearInterval(interval);
    }
  }, [weighbridgeStatus]);

  // Simulate data fetching on initial load
  useEffect(() => {
    setTimeout(() => {
      setLoading(false);
    }, 1000);
  }, []);

  const handleStartWeigh = (weighType) => {
    setMessage(`Starting ${weighType}...`);
    setWeighbridgeStatus('busy');
    setStep(weighType);
    
    // Simulate a delay for the weighing process
    setTimeout(() => {
      const finalWeight = Math.round(Math.random() * 500) + (weighType === 'first_weigh' ? 12500 : 25000);
      setCurrentWeight(finalWeight);
      if (weighType === 'first_weigh') {
        setFirstWeigh(finalWeight);
        setVehicle(prev => ({ ...prev, loadStatus: 'Pending Second Weigh' }));
      } else {
        setSecondWeigh(finalWeight);
        setVehicle(prev => ({ ...prev, loadStatus: 'Complete' }));
      }
      setWeighbridgeStatus('ready');
      setMessage(`${weighType} completed.`);
    }, 3000);
  };
  
  const handleReset = () => {
    setCurrentWeight(0);
    setFirstWeigh(null);
    setSecondWeigh(null);
    setStep('idle');
    setWeighbridgeStatus('ready');
    setMessage('System ready.');
    setVehicle({
      id: 'KAA 123B',
      type: 'Truck',
      loadStatus: 'Pending First Weigh',
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'ready':
        return 'bg-green-500';
      case 'busy':
        return 'bg-yellow-500';
      case 'offline':
        return 'bg-red-500';
      default:
        return 'bg-gray-400';
    }
  };

  return (
    <div className="flex min-h-screen bg-gray-100 font-sans text-gray-800">
      {/* Sidebar - Reusing the dashboard sidebar for consistency */}
      <aside className="w-64 bg-gray-800 text-white p-4 hidden md:flex flex-col">
        <div className="flex items-center space-x-2 p-2">
          <ScaleIcon className="h-8 w-8 text-indigo-400" />
          <h1 className="text-2xl font-bold">Weighbridge</h1>
        </div>
        <nav className="mt-8">
          <ul>
            {/* Nav items for context, active item highlighted */}
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg text-gray-300 hover:bg-gray-700">
                <div className="h-6 w-6"><TruckIcon /></div>
                <span>Dashboard</span>
              </a>
            </li>
            <li className="mb-2">
              <a href="#" className="flex items-center space-x-3 p-3 rounded-lg bg-indigo-600 text-white shadow-lg">
                <div className="h-6 w-6"><ScaleIcon /></div>
                <span>Weighing Module</span>
              </a>
            </li>
          </ul>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 p-6 md:p-10 flex flex-col items-center">
        <header className="mb-6 w-full max-w-4xl">
          <h2 className="text-3xl font-bold text-gray-900">Weighing Module - WB-01</h2>
          <p className="text-gray-500">Location: Nairobi Grinding Station</p>
        </header>

        {loading ? (
          <div className="flex flex-col items-center justify-center min-h-[400px]">
            <LoadingSpinner className="h-12 w-12 animate-spin text-indigo-500" />
            <p className="mt-4 text-xl text-gray-600">Loading weighbridge data...</p>
          </div>
        ) : (
          <div className="w-full max-w-4xl bg-white p-8 rounded-xl shadow-lg flex flex-col lg:flex-row gap-8">
            {/* Weighing Status Panel */}
            <div className="flex-1 p-6 bg-gray-50 rounded-xl flex flex-col items-center justify-center space-y-6 text-center">
              <div className={`h-8 w-8 rounded-full ${getStatusColor(weighbridgeStatus)} animate-pulse`}></div>
              <p className="text-lg font-semibold capitalize">Status: {weighbridgeStatus}</p>
              <div className="text-8xl font-black text-gray-900 flex items-end">
                {weighbridgeStatus === 'offline' ? (
                  <span className="text-6xl text-red-500">OFFLINE</span>
                ) : (
                  <>
                    <span>{currentWeight}</span>
                    <span className="text-3xl font-normal ml-2 text-gray-500">kg</span>
                  </>
                )}
              </div>
              <p className={`text-md mt-4 p-2 rounded-lg ${
                message.includes('completed') ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'
              }`}>{message || 'Weighbridge is ready for operation.'}</p>
            </div>

            {/* Controls and Vehicle Info Panel */}
            <div className="flex-1 space-y-6">
              <div className="bg-gray-50 p-6 rounded-xl">
                <h3 className="text-xl font-bold mb-4">Vehicle Details</h3>
                <div className="space-y-2 text-gray-700">
                  <p><strong>Registration:</strong> {vehicle.id}</p>
                  <p><strong>Vehicle Type:</strong> {vehicle.type}</p>
                  <p><strong>Current Status:</strong> <span className={`font-semibold ${
                    vehicle.loadStatus.includes('Complete') ? 'text-green-600' : 'text-yellow-600'
                  }`}>{vehicle.loadStatus}</span></p>
                  <p><strong>First Weigh:</strong> {firstWeigh ? `${firstWeigh} kg` : 'N/A'}</p>
                  <p><strong>Second Weigh:</strong> {secondWeigh ? `${secondWeigh} kg` : 'N/A'}</p>
                  <p><strong>Net Weight:</strong> {firstWeigh && secondWeigh ? `${Math.abs(firstWeigh - secondWeigh)} kg` : 'N/A'}</p>
                </div>
              </div>

              <div className="p-6 bg-indigo-50 rounded-xl space-y-4">
                <h3 className="text-xl font-bold text-indigo-800">Weighing Controls</h3>
                <div className="flex flex-col space-y-4">
                  <button
                    onClick={() => handleStartWeigh('first_weigh')}
                    disabled={weighbridgeStatus !== 'ready' || step !== 'idle'}
                    className="w-full p-4 text-lg font-bold text-white bg-indigo-600 rounded-lg shadow-md hover:bg-indigo-700 transition duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Start First Weigh
                  </button>
                  <button
                    onClick={() => handleStartWeigh('second_weigh')}
                    disabled={weighbridgeStatus !== 'ready' || step !== 'first_weigh'}
                    className="w-full p-4 text-lg font-bold text-white bg-purple-600 rounded-lg shadow-md hover:bg-purple-700 transition duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Start Second Weigh
                  </button>
                  <button
                    onClick={handleReset}
                    className="w-full p-4 text-lg font-bold text-gray-700 bg-gray-200 rounded-lg shadow-md hover:bg-gray-300 transition duration-300"
                  >
                    Reset
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
};

// Main App component

