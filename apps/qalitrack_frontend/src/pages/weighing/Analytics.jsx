import dayjs from "dayjs"

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

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
        <StatCard title="Total Transactions" value={totalTransactions} />
        <StatCard title="Total Net Weight (kg)" value={totalNetWeight.toLocaleString()} />
        <StatCard title="Avg Net Weight (kg)" value={avgNetWeight.toLocaleString()} />
        <StatCard title="Today" value={todayCount} />
      </div>

      {/* Status Breakdown */}
      <div className="bg-white rounded shadow p-4">
        <h3 className="font-semibold mb-3">Transactions by Status</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left border-b">
              <th>Status</th>
              <th>Count</th>
            </tr>
          </thead>
          <tbody>
            {Object.entries(statusCounts).map(([status, count]) => (
              <tr key={status} className="border-b">
                <td className="py-2">{status}</td>
                <td>{count}</td>
              </tr>
            ))}
          </tbody>
        </table>
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
