export default function LiveWeighbridgeStatus() {
  const weighbridgeStatus = [
    { id: 1, name: 'Weighbridge 1', status: 'Active', lastUpdated: '2 mins ago' },
    { id: 2, name: 'Weighbridge 2', status: 'Idle', lastUpdated: '5 mins ago' },
    { id: 3, name: 'Weighbridge 3', status: 'Error', lastUpdated: '1 min ago' },
  ];

  return (
    <div className="space-y-3">
      {weighbridgeStatus.map((wb) => (
        <div
          key={wb.id}
          className={`p-4 rounded-lg flex justify-between items-center shadow-sm border 
            ${wb.status === 'Active' ? 'border-green-400 bg-green-50' : 
              wb.status === 'Error' ? 'border-red-400 bg-red-50' : 
              'border-gray-300 bg-gray-50'}`}
        >
          <div>
            <p className="font-semibold text-gray-700">{wb.name}</p>
            <p className="text-xs text-gray-500">Last updated {wb.lastUpdated}</p>
          </div>
          <span
            className={`px-3 py-1 rounded-full text-sm font-medium
              ${wb.status === 'Active' ? 'bg-green-100 text-green-700' :
                wb.status === 'Error' ? 'bg-red-100 text-red-700' :
                'bg-gray-200 text-gray-700'}`}
          >
            {wb.status}
          </span>
        </div>
      ))}
    </div>
  );
}
