import { useSelector } from 'react-redux';
import KPIWidget from '../components/KPIWidget';
import LiveWeighbridgeStatus from '../components/LiveWeighbridgeStatus';
import ProcessFlow from '../components/ProcessFlow';
import AlertsPanel from '../components/AlertsPanel';
import { Truck, ClipboardList, Timer, AlertTriangle } from 'lucide-react';

export default function Dashboard() {
  const weighing = useSelector(state => state.weighing);
  const alerts = useSelector(state => state.automation.alerts || []);

  // Trucks in queue
  const trucksInQueue = weighing.transactions.filter(tx => tx.w1 && tx.w2 === null).length;

  // Active orders
  const activeOrders = new Set(
    weighing.transactions.filter(tx => tx.w2 === null).map(tx => tx.orderId)
  ).size;

  // Average TTAT Today
  const todayStr = new Date().toISOString().slice(0, 10);
  const todaysCompleted = weighing.transactions.filter(
    tx => tx.ttat && tx.date.startsWith(todayStr)
  );
  const avgTTAT = todaysCompleted.length
    ? formatSeconds(
        Math.round(todaysCompleted.reduce((sum, tx) => sum + tx.ttat, 0) / todaysCompleted.length)
      )
    : "00:00";

  // Alerts count
  const alertsCount = alerts.length;

  return (
    <div className="space-y-6 p-4 bg-gray-50 min-h-screen">
      {/* KPI Row */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <KPIWidget title="Trucks in Queue" value={trucksInQueue} icon={<Truck className="text-amber-500" size={28} />} />
        <KPIWidget title="Active Orders" value={activeOrders} icon={<ClipboardList className="text-amber-500" size={28} />} />
        <KPIWidget title="Average TTAT Today" value={avgTTAT} icon={<Timer className="text-amber-500" size={28} />} />
        <KPIWidget title="Alerts / Errors" value={alertsCount} icon={<AlertTriangle className="text-red-500" size={28} />} />
      </div>

      {/* Middle Row */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">Live Weighbridge Status</h3>
          <LiveWeighbridgeStatus />
        </div>

        
          <ProcessFlow />
        
      </div>

      {/* Alerts List */}
      <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-red-500">
        <h3 className="text-lg font-semibold mb-4 text-gray-700">Alerts & Errors</h3>
        <AlertsPanel alerts={alerts} />
      </div>
    </div>
  );
}

function formatSeconds(sec) {
  const m = Math.floor(sec / 60).toString().padStart(2, '0');
  const s = (sec % 60).toString().padStart(2, '0');
  return `${m}:${s}`;
}
