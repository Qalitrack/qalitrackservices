import { useSelector, useDispatch } from 'react-redux';
import { setDeviceState, addAlert } from '/src/store/automationSlice';
import { Zap, Power } from 'lucide-react';
import PageHeader from '../../components/PageHeader.jsx';

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
    <div className="h-full flex flex-col rounded-lg shadow-md border border-gray-200 bg-white overflow-hidden">
      <PageHeader
        icon={Zap}
        title="AUTOMATION"
        subtitle="Hardware device controls"
        flush
        className="border-b border-white/10"
      />

      <div className="flex-1 overflow-auto p-4 sm:p-6">
        {devices.length === 0 ? (
          <div className="text-center py-12">
            <Zap className="w-12 h-12 text-gray-300 mx-auto mb-2" />
            <p className="text-gray-500 text-sm font-medium">No devices found.</p>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {devices.map((device) => (
              <div
                key={device.deviceId}
                className="rounded-lg border border-gray-200 bg-white shadow-sm p-4 flex flex-col items-center gap-2"
              >
                <h3 className="font-semibold text-sm text-gray-900">{device.name}</h3>
                <span
                  className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                    device.status === 'online'
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  <span className={`w-1.5 h-1.5 rounded-full ${device.status === 'online' ? 'bg-green-500' : 'bg-red-500'}`} />
                  {device.status.toUpperCase()}
                </span>
                <button
                  onClick={() => handleToggle(device)}
                  className={`mt-2 inline-flex items-center gap-1.5 px-4 py-1.5 rounded-lg text-xs font-semibold text-white shadow-sm transition-all ${
                    device.status === 'online'
                      ? 'bg-red-500 hover:bg-red-600'
                      : 'bg-amber-500 hover:bg-amber-600'
                  }`}
                >
                  <Power className="w-3.5 h-3.5" />
                  {device.status === 'online' ? 'Turn Off' : 'Turn On'}
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
