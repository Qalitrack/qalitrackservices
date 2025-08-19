export default function ProcessFlow() {
  const steps = [
    { id: 1, title: 'Truck Arrival', status: 'completed' },
    { id: 2, title: 'Weighing (W1)', status: 'completed' },
    { id: 3, title: 'Loading / Unloading', status: 'active' },
    { id: 4, title: 'Weighing (W2)', status: 'pending' },
    { id: 5, title: 'Exit', status: 'pending' },
  ];

  return (
    <div className="flex flex-col space-y-4">
      {steps.map((step, index) => (
        <div key={step.id} className="flex items-center space-x-4">
          {/* Step Circle */}
          <div
            className={`w-10 h-10 flex items-center justify-center rounded-full font-bold 
              ${step.status === 'completed' ? 'bg-green-500 text-white' :
                step.status === 'active' ? 'bg-amber-500 text-white' :
                'bg-gray-300 text-gray-600'}`}
          >
            {index + 1}
          </div>

          {/* Step Details */}
          <div className="flex-1">
            <p className="font-medium text-gray-800">{step.title}</p>
            <div className="h-2 w-full bg-gray-200 rounded">
              <div
                className={`h-2 rounded transition-all duration-500 
                  ${step.status === 'completed' ? 'bg-green-500 w-full' :
                    step.status === 'active' ? 'bg-amber-500 w-1/2' :
                    'bg-gray-300 w-0'}`}
              ></div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
