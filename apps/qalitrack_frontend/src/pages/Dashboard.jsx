import { useSelector } from "react-redux";
import KPIWidget from "../components/KPIWidget";
import LiveWeighbridgeStatus from "../components/LiveWeighbridgeStatus";
import ProcessFlow from "../components/ProcessFlow";
import AlertsPanel from "../components/AlertsPanel";
import { Truck, ClipboardList, Timer, AlertTriangle } from "lucide-react";
import { Line, Bar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  LineElement,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  Tooltip,
  Legend,
} from "chart.js";

ChartJS.register(
  LineElement,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  Tooltip,
  Legend
);

export default function Dashboard() {
  const weighing = useSelector((state) => state.weighing);
  const alerts = useSelector((state) => state.automation.alerts || []);

  // 🚛 Trucks currently in queue
  const trucksInQueue = weighing.transactions.filter(
    (tx) => tx.w1 && tx.w2 === null
  ).length;

  // 📋 Active orders
  const activeOrders = new Set(
    weighing.transactions.filter((tx) => tx.w2 === null).map((tx) => tx.orderId)
  ).size;

  // ⏱ Average Turnaround Time Today
  const todayStr = new Date().toISOString().slice(0, 10);
  const todaysCompleted = weighing.transactions.filter(
    (tx) => tx.ttat && tx.date.startsWith(todayStr)
  );
  const avgTTAT = todaysCompleted.length
    ? formatSeconds(
        Math.round(
          todaysCompleted.reduce((sum, tx) => sum + tx.ttat, 0) /
            todaysCompleted.length
        )
      )
    : "00:00";

  // ⚠️ Alerts
  const alertsCount = alerts.length;

  // 📊 Chart Data
  const weightTrends = {
    labels: weighing.transactions.slice(-5).map((tx) =>
      new Date(tx.date).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
    ),
    datasets: [
      {
        label: "Weight 1",
        data: weighing.transactions.slice(-5).map((tx) => tx.w1),
        borderColor: "#f59e0b",
        backgroundColor: "rgba(245, 158, 11, 0.2)",
      },
      {
        label: "Weight 2",
        data: weighing.transactions.slice(-5).map((tx) => tx.w2 || 0),
        borderColor: "#10b981",
        backgroundColor: "rgba(16, 185, 129, 0.2)",
      },
    ],
  };

  const queueTrends = {
    labels: ["Mon", "Tue", "Wed", "Thu", "Fri"],
    datasets: [
      {
        label: "Trucks in Queue",
        data: [4, 6, 3, 8, trucksInQueue],
        backgroundColor: "#3b82f6",
      },
    ],
  };

  return (
    <div className="space-y-6 p-4 bg-gray-50 min-h-screen">
      {/* ===== KPI ROW ===== */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <KPIWidget
          title="Trucks in Queue"
          value={trucksInQueue}
          icon={<Truck className="text-amber-500" size={28} />}
        />
        <KPIWidget
          title="Active Orders"
          value={activeOrders}
          icon={<ClipboardList className="text-amber-500" size={28} />}
        />
        <KPIWidget
          title="Average TTAT Today"
          value={avgTTAT}
          icon={<Timer className="text-amber-500" size={28} />}
        />
        <KPIWidget
          title="Alerts / Errors"
          value={alertsCount}
          icon={<AlertTriangle className="text-red-500" size={28} />}
        />
      </div>

      {/* ===== MAIN PANELS ===== */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Live Weighbridge Panel */}
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">
            Live Weighbridge Status
          </h3>
          <LiveWeighbridgeStatus />
        </div>

        {/* Process Flow */}
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">
            Process Flow
          </h3>
          <ProcessFlow />
        </div>
      </div>

      {/* ===== CHARTS ===== */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Weight Trends */}
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-blue-500">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">
            Weight Trends (Last 5)
          </h3>
          <Line data={weightTrends} />
        </div>

        {/* Queue Trends */}
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-green-500">
          <h3 className="text-lg font-semibold mb-4 text-gray-700">
            Trucks in Queue (Weekly)
          </h3>
          <Bar data={queueTrends} />
        </div>
      </div>

      {/* ===== ALERTS LIST ===== */}
      <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-red-500">
        <h3 className="text-lg font-semibold mb-4 text-gray-700">
          Alerts & Errors
        </h3>
        <AlertsPanel alerts={alerts} />
      </div>
    </div>
  );
}

function formatSeconds(sec) {
  const m = Math.floor(sec / 60).toString().padStart(2, "0");
  const s = (sec % 60).toString().padStart(2, "0");
  return `${m}:${s}`;
}
