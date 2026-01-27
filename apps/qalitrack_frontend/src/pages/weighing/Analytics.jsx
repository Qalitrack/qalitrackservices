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
  LineChart,
  Line,
  Legend,
} from "recharts"

const YELLOW = "#facc15"
const YELLOW_LIGHT = "#fde68a"
const YELLOW_BG = "#fffbeb"
const GRAY = "#9ca3af"

export default function Analytics({ transactions = [] }) {
  /* =========================
     CORE METRICS
  ========================= */
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

  /* =========================
     STATUS
  ========================= */
  const statusCounts = transactions.reduce((acc, t) => {
    const key = t.status || "UNKNOWN"
    acc[key] = (acc[key] || 0) + 1
    return acc
  }, {})

  const completedCount = statusCounts["Completed"] || 0
  const completionRate =
    totalTransactions > 0
      ? Math.round((completedCount / totalTransactions) * 100)
      : 0

  const statusChartData = Object.entries(statusCounts).map(
    ([name, value]) => ({ name, value })
  )

  /* =========================
     TRANSACTIONS BY DAY
  ========================= */
  const transactionsByDay = Object.values(
    transactions.reduce((acc, t) => {
      const day = dayjs(t.createdAt).format("DD MMM")
      acc[day] = acc[day] || { day, count: 0, weight: 0 }
      acc[day].count += 1
      acc[day].weight += t.netWeight || 0
      return acc
    }, {})
  )

  /* =========================
     TRANSACTIONS BY HOUR
  ========================= */
  const transactionsByHour = Array.from({ length: 24 }, (_, h) => ({
    hour: `${h}:00`,
    count: transactions.filter(
      (t) => dayjs(t.createdAt).hour() === h
    ).length,
  }))

  /* =========================
     CUMULATIVE TRANSACTIONS
  ========================= */
  let runningTotal = 0
  const cumulativeData = transactionsByDay.map((d) => {
    runningTotal += d.count
    return { day: d.day, total: runningTotal }
  })

  /* =========================
     STATUS TREND
  ========================= */
  const statusTrend = Object.values(
    transactions.reduce((acc, t) => {
      const day = dayjs(t.createdAt).format("DD MMM")
      acc[day] = acc[day] || { day, Completed: 0, Pending: 0 }

      if (t.status === "Completed") acc[day].Completed += 1
      else acc[day].Pending += 1

      return acc
    }, {})
  )

  /* =========================
     TOP DRIVERS
  ========================= */
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

  /* =========================
     NET WEIGHT BY COMMODITY
  ========================= */
  const weightByCommodity = Object.values(
    transactions.reduce((acc, t) => {
      const commodity = t.commodityName || "Unknown"
      acc[commodity] = acc[commodity] || { name: commodity, weight: 0 }
      acc[commodity].weight += t.netWeight || 0
      return acc
    }, {})
  ).sort((a, b) => b.weight - a.weight)

  return (
    <div className="h-full overflow-y-auto pr-2 space-y-8">
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
      <Section title="Daily Overview">
        <ChartCard title="Transactions per Day">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={transactionsByDay}>
              <XAxis dataKey="day" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Transactions by Status">
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={statusChartData} dataKey="value" label outerRadius={90}>
                {statusChartData.map((_, i) => (
                  <Cell key={i} fill={i % 2 === 0 ? YELLOW : YELLOW_LIGHT} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </Section>

      {/* ROW 2 */}
      <Section title="Time & Behaviour">
        <ChartCard title="Transactions by Hour">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={transactionsByHour}>
              <XAxis dataKey="hour" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW_LIGHT} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Cumulative Transactions">
          <ResponsiveContainer width="100%" height={260}>
            <LineChart data={cumulativeData}>
              <XAxis dataKey="day" />
              <YAxis />
              <Tooltip />
              <Line dataKey="total" stroke={YELLOW} strokeWidth={3} />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>
      </Section>

      {/* ROW 3 */}
      <Section title="Performance Breakdown">
        <ChartCard title="Status Trend">
          <ResponsiveContainer width="100%" height={260}>
            <LineChart data={statusTrend}>
              <XAxis dataKey="day" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line dataKey="Completed" stroke={YELLOW} />
              <Line dataKey="Pending" stroke={GRAY} />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Top Drivers">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={topDrivers} layout="vertical">
              <XAxis type="number" />
              <YAxis type="category" dataKey="name" width={120} />
              <Tooltip />
              <Bar dataKey="trips" fill={YELLOW} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>
      </Section>
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

function Section({ title, children }) {
  return (
    <div className="space-y-3">
      <h2 className="text-sm font-semibold text-gray-600">{title}</h2>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        {children}
      </div>
    </div>
  )
}
