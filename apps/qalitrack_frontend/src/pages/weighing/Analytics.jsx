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

export default function Analytics({ transactions = [] }) {
  // =========================
  // BASIC METRICS
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
  // STATUS METRICS
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
    ([status, count]) => ({
      name: status,
      value: count,
    })
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
  // TOP DRIVERS
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
      acc[commodity] = acc[commodity] || {
        name: commodity,
        weight: 0,
      }
      acc[commodity].weight += t.netWeight || 0
      return acc
    }, {})
  ).sort((a, b) => b.weight - a.weight)

  return (
    <div className="space-y-6">
      {/* KPI CARDS */}
      <div className="grid grid-cols-1 sm:grid-cols-6 gap-4">
        <StatCard title="Transactions" value={totalTransactions} />
        <StatCard
          title="Total Net Weight (kg)"
          value={totalNetWeight.toLocaleString()}
        />
        <StatCard
          title="Avg Net Weight"
          value={avgNetWeight.toLocaleString()}
        />
        <StatCard title="Today" value={todayCount} />
        <StatCard title="Completed" value={completedCount} />
        <StatCard title="Completion Rate" value={`${completionRate}%`} />
      </div>

      {/* CHARTS ROW 1 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Transactions per Day */}
        <ChartCard title="Transactions per Day">
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={transactionsByDay}>
              <XAxis dataKey="day" />
              <YAxis allowDecimals={false} />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        {/* Status Pie */}
        <ChartCard title="Transactions by Status">
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie
                data={statusChartData}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={95}
                label
              >
                {statusChartData.map((_, index) => (
                  <Cell
                    key={index}
                    fill={index % 2 === 0 ? YELLOW : YELLOW_LIGHT}
                  />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>

      {/* CHARTS ROW 2 */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Top Drivers */}
        <ChartCard title="Top Drivers by Trips">
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={topDrivers} layout="vertical">
              <XAxis type="number" allowDecimals={false} />
              <YAxis type="category" dataKey="name" width={120} />
              <Tooltip />
              <Bar dataKey="trips" fill={YELLOW} radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        {/* Commodity Weight */}
        <ChartCard title="Net Weight by Commodity">
          <ResponsiveContainer width="100%" height={280}>
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
   REUSABLE UI PIECES
========================= */

function StatCard({ title, value }) {
  return (
    <div className="bg-white rounded shadow p-4">
      <p className="text-xs text-gray-500">{title}</p>
      <p className="text-2xl font-semibold">{value}</p>
    </div>
  )
}

function ChartCard({ title, children }) {
  return (
    <div className="bg-white rounded shadow p-4">
      <h3 className="font-semibold mb-4">{title}</h3>
      {children}
    </div>
  )
}
