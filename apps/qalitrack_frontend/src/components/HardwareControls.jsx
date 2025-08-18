// src/components/HardwareControls.jsx
export default function HardwareControls() {
  return (
    <div className="border rounded bg-white shadow p-4 mt-4">
      <h3 className="text-lg font-semibold text-amber-600 mb-3">
        Hardware Controls
      </h3>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Vehicle Position</p>
          <p className="text-green-600 font-bold">Approaching</p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Gate Status</p>
          <p className="text-blue-600 font-bold">Operational</p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Traffic Lights</p>
          <p className="text-green-600 font-bold">Green</p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">System Status</p>
          <p className="text-green-600 font-bold">Online</p>
        </div>
      </div>
    </div>
  );
}
