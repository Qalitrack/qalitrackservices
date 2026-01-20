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

  const statusCounts = transactions.reduce((acc, t) => {
    const key = t.status || "UNKNOWN"
    acc[key] = (acc[key] || 0) + 1
    return acc
  }, {})

  const statusChartData = Object.entries(statusCounts).map(
    ([status, count]) => ({
      name: status,
      value: count,
    })
  )

  const transactionsByDay = Object.values(
    transactions.reduce((acc, t) => {
      const day = dayjs(t.createdAt).format("DD MMM")
      acc[day] = acc[day] || { day, count: 0 }
      acc[day].count += 1
      return acc
    }, {})
  )

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
        <StatCard title="Total Transactions" value={totalTransactions} />
        <StatCard
          title="Total Net Weight (kg)"
          value={totalNetWeight.toLocaleString()}
        />
        <StatCard
          title="Avg Net Weight (kg)"
          value={avgNetWeight.toLocaleString()}
        />
        <StatCard title="Today" value={todayCount} />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Bar Chart */}
        <div className="bg-white rounded shadow p-4">
          <h3 className="font-semibold mb-4">
            Transactions per Day
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={transactionsByDay}>
              <XAxis dataKey="day" />
              <YAxis allowDecimals={false} />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Pie Chart */}
        <div className="bg-white rounded shadow p-4">
          <h3 className="font-semibold mb-4">
            Transactions by Status
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={statusChartData}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={100}
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
        </div>
      </div>
    </div>
  )
}

function StatCard({ title, value }) {
  return (
    <div className="bg-white rounded shadow p-4">
      <p className="text-xs text-gray-500">{title}</p>
      <p className="text-2xl font-semibold">{value}</p>
    </div>
  )
}
