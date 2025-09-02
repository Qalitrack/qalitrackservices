// src/pages/Dashboard.jsx
import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import KPIWidget from '/src/components/KPIWidget.jsx';
import LiveWeighbridgeStatus from '/src/components/LiveWeighbridgeStatus.jsx';
import ProcessFlow from '/src/components/ProcessFlow';
import AlertsPanel from '/src/components/AlertsPanel';
import {
  Truck,
  ClipboardList,
  Timer,
  AlertTriangle,
  Scale,
  Activity,
} from 'lucide-react';

export default function Dashboard() {
  const weighing = useSelector((state) => state.weighing || { transactions: [] });
  const alerts = useSelector((state) => state.automation?.alerts || []);

  const txs = Array.isArray(weighing.transactions) ? weighing.transactions : [];

  const todayStr = new Date().toISOString().slice(0, 10);

  // ---- Helpers ----
  const getType = (tx) => (tx.type || tx.transactionType || '').toLowerCase(); // support both keys
  const isCompleted = (tx) => tx.w1 != null && tx.w2 != null && !tx.deactivated;
  const isInQueue = (tx) => tx.w1 != null && tx.w2 == null && !tx.deactivated;

  const secFmt = (sec) => {
    const m = Math.floor((sec || 0) / 60).toString().padStart(2, '0');
    const s = ((sec || 0) % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  // ---- KPIs & Derived Data ----
  const {
    trucksInQueue,
    activeOrders,
    avgTTAT,
    completedToday,
    avgNetToday,
    anomalies,
    recent,
  } = useMemo(() => {
    const trucksInQueue = txs.filter(isInQueue).length;

    const activeOrders = new Set(
      txs.filter((tx) => tx.w2 == null).map((tx) => tx.orderId)
    ).size;

    const todaysCompleted = txs.filter(
      (tx) => isCompleted(tx) && (tx.date || '').startsWith(todayStr)
    );

    const avgTTAT = todaysCompleted.length
      ? secFmt(
          Math.round(
            todaysCompleted.reduce((sum, tx) => sum + (tx.ttat || 0), 0) /
              todaysCompleted.length
          )
        )
      : '00:00';

    const completedToday = todaysCompleted.length;

    const avgNetToday = (() => {
      const nets = todaysCompleted
        .map((tx) => (tx.w1 != null && tx.w2 != null ? Math.abs(tx.w1 - tx.w2) : null))
        .filter((v) => typeof v === 'number');
      if (!nets.length) return 0;
      return Math.round(nets.reduce((a, b) => a + b, 0) / nets.length);
    })();

    // Inbound/Outbound anomaly rules
    // inbound: W2 should be < W1
    // outbound: W2 should be > W1
    const anomalies = txs
      .filter((tx) => tx.w1 != null && tx.w2 != null && !tx.deactivated)
      .filter((tx) => {
        const t = getType(tx);
        if (t === 'inbound') return !(tx.w2 < tx.w1);
        if (t === 'outbound') return !(tx.w2 > tx.w1);
        return false; // unknown type doesn't flag
      })
      .slice(0, 5); // keep it light

    const recent = [...txs]
      .sort((a, b) => new Date(b.date) - new Date(a.date))
      .slice(0, 8);

    return {
      trucksInQueue,
      activeOrders,
      avgTTAT,
      completedToday,
      avgNetToday,
      anomalies,
      recent,
    };
  }, [txs, todayStr]);

  const alertsCount = alerts.length;

  return (
    <div className="space-y-6 p-4 bg-gray-50 min-h-screen">
      {/* KPI Row */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
        <KPIWidget
          title="Trucks in Queue"
          value={trucksInQueue}
          icon={<Truck className="text-amber-500" size={26} />}
        />
        <KPIWidget
          title="Active Orders"
          value={activeOrders}
          icon={<ClipboardList className="text-amber-500" size={26} />}
        />
        <KPIWidget
          title="Avg TTAT Today"
          value={avgTTAT}
          icon={<Timer className="text-amber-500" size={26} />}
        />
        <KPIWidget
          title="Completed Today"
          value={completedToday}
          icon={<Activity className="text-amber-500" size={26} />}
        />
        <KPIWidget
          title="Avg Net (Today)"
          value={`${avgNetToday || 0} kg`}
          icon={<Scale className="text-amber-500" size={26} />}
        />
      </div>

      {/* Middle Row */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* Live Weighbridge */}
        <div className="xl:col-span-2 bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-lg font-semibold text-gray-700">
              Live Weighbridge Status
            </h3>
          </div>
          <LiveWeighbridgeStatus />
        </div>

        {/* Weighing Insights (Anomalies) */}
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500/70">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-lg font-semibold text-gray-700">
              Weighing Insights
            </h3>
            <div className="inline-flex items-center gap-2 text-sm">
              <AlertTriangle className={`w-4 h-4 ${anomalies.length ? 'text-red-500' : 'text-gray-400'}`} />
              <span className={`${anomalies.length ? 'text-red-600' : 'text-gray-500'}`}>
                {anomalies.length} anomaly{anomalies.length === 1 ? '' : 'ies'}
              </span>
            </div>
          </div>

          {anomalies.length === 0 ? (
            <p className="text-sm text-gray-500">No anomalies detected today.</p>
          ) : (
            <ul className="divide-y border rounded">
              {anomalies.map((tx) => {
                const t = getType(tx);
                const rule =
                  t === 'inbound' ? 'W2 should be less than W1' : 'W2 should be greater than W1';
                return (
                  <li key={tx.id} className="p-3 text-sm">
                    <div className="flex items-center justify-between">
                      <div>
                        <div className="font-medium text-gray-800">
                          {tx.plate} • {t || 'unknown'}
                        </div>
                        <div className="text-gray-600">
                          W1: <b>{tx.w1}</b> kg &nbsp; W2:{' '}
                          <b className="text-red-600">{tx.w2}</b> kg &nbsp; • Rule: {rule}
                        </div>
                        <div className="text-xs text-gray-500">
                          {new Date(tx.date).toLocaleString()} • Order {tx.orderId || '—'}
                        </div>
                      </div>
                      <span className="text-xs bg-red-100 text-red-700 px-2 py-1 rounded">
                        Check at Weighing
                      </span>
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </div>

      {/* Process Flow + Recent Transactions */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500 xl:col-span-1">
          <h3 className="text-lg font-semibold mb-3 text-gray-700">Process Flow</h3>
          <ProcessFlow />
        </div>

        <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-amber-500 xl:col-span-2">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-lg font-semibold text-gray-700">Recent Transactions</h3>
            <span className="text-xs text-gray-500">Last 8 records</span>
          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full text-sm border">
              <thead className="bg-gray-100 text-left">
                <tr>
                  <th className="px-3 py-2 border">Plate</th>
                  <th className="px-3 py-2 border">Type</th>
                  <th className="px-3 py-2 border">W1</th>
                  <th className="px-3 py-2 border">W2</th>
                  <th className="px-3 py-2 border">Net</th>
                  <th className="px-3 py-2 border">Status</th>
                  <th className="px-3 py-2 border">Date</th>
                </tr>
              </thead>
              <tbody>
                {recent.map((tx) => {
                  const t = getType(tx) || '—';
                  const net =
                    tx.w1 != null && tx.w2 != null ? Math.abs(tx.w1 - tx.w2) : null;
                  const status = tx.deactivated
                    ? 'Deactivated'
                    : tx.w2 != null
                    ? 'Completed'
                    : 'In Queue';

                  return (
                    <tr key={tx.id} className="hover:bg-gray-50">
                      <td className="px-3 py-2 border">{tx.plate || '—'}</td>
                      <td className="px-3 py-2 border capitalize">{t}</td>
                      <td className="px-3 py-2 border">{tx.w1 ?? '—'}</td>
                      <td className="px-3 py-2 border">{tx.w2 ?? '—'}</td>
                      <td className="px-3 py-2 border">{net ?? '—'}</td>
                      <td className="px-3 py-2 border">
                        <span
                          className={[
                            'px-2 py-0.5 rounded text-xs',
                            status === 'Completed' && 'bg-green-100 text-green-700',
                            status === 'In Queue' && 'bg-amber-100 text-amber-700',
                            status === 'Deactivated' && 'bg-gray-100 text-gray-700',
                          ]
                            .filter(Boolean)
                            .join(' ')}
                        >
                          {status}
                        </span>
                      </td>
                      <td className="px-3 py-2 border">
                        {tx.date ? new Date(tx.date).toLocaleString() : '—'}
                      </td>
                    </tr>
                  );
                })}
                {recent.length === 0 && (
                  <tr>
                    <td colSpan="7" className="px-3 py-6 text-center text-gray-500">
                      No transactions yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Alerts */}
      <div className="bg-white rounded-lg shadow-md p-4 border-t-4 border-red-500">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-lg font-semibold text-gray-700">Alerts & Errors</h3>
          <span className="inline-flex items-center gap-2 text-sm text-gray-600">
            <AlertTriangle className="w-4 h-4 text-red-500" />
            {alertsCount} total
          </span>
        </div>
        <AlertsPanel alerts={alerts} />
      </div>
    </div>
  );
}
