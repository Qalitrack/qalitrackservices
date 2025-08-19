import { useSelector, useDispatch } from 'react-redux';
import { setDeviceState, addAlert } from '../store/automationSlice';

export default function Automation() {
  const dispatch = useDispatch();
  const devices = useSelector(state => state.automation.devices);

  const handleToggle = (device) => {
    const newStatus = device.status === 'online' ? 'offline' : 'online';
    dispatch(setDeviceState({ deviceId: device.deviceId, status: newStatus, name: device.name }));
    dispatch(addAlert({
      message: `${device.name} turned ${newStatus}`,
      type: newStatus === 'online' ? 'success' : 'warning'
    }));
  };

  return (
    <div className="p-4 space-y-4">
      <h2 className="text-xl font-semibold">Automation Controls</h2>

      {devices.length === 0 ? (
        <p className="text-gray-500 italic">No devices found.</p>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {devices.map((device) => (
            <div key={device.deviceId} className="border rounded p-4 flex flex-col items-center bg-white shadow-sm">
              <h3 className="font-medium">{device.name}</h3>
              <p
                className={`mt-1 text-sm ${
                  device.status === 'online' ? 'text-green-600' : 'text-red-600'
                }`}
              >
                {device.status.toUpperCase()}
              </p>
              <button
                onClick={() => handleToggle(device)}
                className={`mt-3 px-4 py-2 rounded text-white ${
                  device.status === 'online' ? 'bg-red-500 hover:bg-red-600' : 'bg-green-500 hover:bg-green-600'
                }`}
              >
                {device.status === 'online' ? 'Turn Off' : 'Turn On'}
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
