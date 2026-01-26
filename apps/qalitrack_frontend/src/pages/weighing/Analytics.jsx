import dayjs from "dayjs"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts"

const YELLOW = "#facc15"
const YELLOW_LIGHT = "#fde68a"
const YELLOW_BG = "#fffbeb"

export default function Analytics({ transactions = [] }) {
  // =========================
  // CORE METRICS (shared truth)
  // =========================
  const totalTransactions = transactions.length

  const totalNetWeight = transactions.reduce(
    (sum, t) => sum + (t.netWeight || 0),
    0
  )

  const avgNetWeight =
    totalTransactions > 0
      ? Math.round(totalNetWeight / totalTransactions)
      : 0

  const todayCount = transactions.filter((t) =>
    dayjs(t.createdAt).isSame(dayjs(), "day")
  ).length

  // =========================
  // STATUS (same as reports)
  // =========================
  const statusCounts = transactions.reduce((acc, t) => {
    const key = t.status || "UNKNOWN"
    acc[key] = (acc[key] || 0) + 1
    return acc
  }, {})

  const completedCount = statusCounts["Completed"] || 0
  const pendingCount = totalTransactions - completedCount

  const completionRate =
    totalTransactions > 0
      ? Math.round((completedCount / totalTransactions) * 100)
      : 0

  const statusChartData = Object.entries(statusCounts).map(
    ([name, value]) => ({ name, value })
  )

  // =========================
  // TRANSACTIONS BY DAY
  // =========================
  const transactionsByDay = Object.values(
    transactions.reduce((acc, t) => {
      const day = dayjs(t.createdAt).format("DD MMM")
      acc[day] = acc[day] || { day, count: 0 }
      acc[day].count += 1
      return acc
    }, {})
  )

  // =========================
  // TOP DRIVERS (same logic as DriverReport)
  // =========================
  const topDrivers = Object.values(
    transactions.reduce((acc, t) => {
      const driver = t.driverName || "Unknown"
      acc[driver] = acc[driver] || { name: driver, trips: 0 }
      acc[driver].trips += 1
      return acc
    }, {})
  )
    .sort((a, b) => b.trips - a.trips)
    .slice(0, 5)

  // =========================
  // NET WEIGHT BY COMMODITY
  // =========================
  const weightByCommodity = Object.values(
    transactions.reduce((acc, t) => {
      const commodity = t.commodityName || "Unknown"
      acc[commodity] = acc[commodity] || { name: commodity, weight: 0 }
      acc[commodity].weight += t.netWeight || 0
      return acc
    }, {})
  ).sort((a, b) => b.weight - a.weight)

  return (
    <div className="space-y-5">
      {/* KPI STRIP */}
      <div className="grid grid-cols-2 sm:grid-cols-6 gap-3">
        <KPI label="Transactions" value={totalTransactions} />
        <KPI label="Total Weight" value={totalNetWeight.toLocaleString()} />
        <KPI label="Avg Weight" value={avgNetWeight.toLocaleString()} />
        <KPI label="Today" value={todayCount} />
        <KPI label="Completed" value={completedCount} />
        <KPI label="Completion %" value={`${completionRate}%`} />
      </div>

      {/* ROW 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <ChartCard title="Transactions per Day">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={transactionsByDay}>
              <XAxis dataKey="day" />
              <YAxis allowDecimals={false} />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Transactions by Status">
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie
                data={statusChartData}
                dataKey="value"
                nameKey="name"
                outerRadius={90}
                label
              >
                {statusChartData.map((_, i) => (
                  <Cell
                    key={i}
                    fill={i % 2 === 0 ? YELLOW : YELLOW_LIGHT}
                  />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>

      {/* ROW 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <ChartCard title="Top Drivers">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={topDrivers} layout="vertical">
              <XAxis type="number" allowDecimals={false} />
              <YAxis type="category" dataKey="name" width={110} />
              <Tooltip />
              <Bar dataKey="trips" fill={YELLOW} radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Net Weight by Commodity">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={weightByCommodity}>
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="weight" fill={YELLOW} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>
    </div>
  )
}

/* =========================
   UI ATOMS
========================= */

function KPI({ label, value }) {
  return (
    <div className="bg-yellow-50 border rounded px-3 py-2">
      <p className="text-[11px] text-gray-500 uppercase">{label}</p>
      <p className="text-lg font-semibold">{value}</p>
    </div>
  )
}

function ChartCard({ title, children }) {
  return (
    <div className="bg-white border rounded p-4">
      <h3 className="text-sm font-semibold mb-3">{title}</h3>
      <div className="bg-[#fffbeb] rounded p-2">{children}</div>
    </div>
  )
}
