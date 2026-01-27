import { useMemo, useState } from "react"
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

/* =========================
   COLORS
========================= */
const YELLOW = "#facc15"
const YELLOW_LIGHT = "#fde68a"
const YELLOW_BG = "#fffbeb"
const GRAY = "#9ca3af"

/* =========================
   MAIN COMPONENT
========================= */
export default function Analytics({ transactions = [] }) {
  /* =========================
     FILTER STATES
  ========================== */
  const [status, setStatus] = useState("ALL")
  const [fromDate, setFromDate] = useState("")
  const [toDate, setToDate] = useState("")

  /* =========================
     FILTERED TRANSACTIONS
  ========================== */
  const filteredTransactions = useMemo(() => {
    return transactions.filter((t) => {
      const created = dayjs(t.createdAt)
      if (status !== "ALL" && t.status !== status) return false
      if (fromDate && created.isBefore(dayjs(fromDate), "day")) return false
      if (toDate && created.isAfter(dayjs(toDate), "day")) return false
      return true
    })
  }, [transactions, status, fromDate, toDate])

  /* =========================
     KPI METRICS
  ========================== */
  const totalTransactions = filteredTransactions.length
  const totalNetWeight = filteredTransactions.reduce(
    (sum, t) => sum + (t.netWeight || 0),
    0
  )
  const avgWeight =
    totalTransactions > 0 ? Math.round(totalNetWeight / totalTransactions) : 0
  const completedCount = filteredTransactions.filter(
    (t) => t.status === "Completed"
  ).length
  const completionRate =
    totalTransactions > 0
      ? Math.round((completedCount / totalTransactions) * 100)
      : 0

  /* =========================
     DAILY DATA
  ========================== */
  const dailyData = useMemo(() => {
    const map = {}
    filteredTransactions.forEach((t) => {
      const day = dayjs(t.createdAt).format("DD MMM")
      map[day] = map[day] || { day, count: 0, weight: 0 }
      map[day].count++
      map[day].weight += t.netWeight || 0
    })
    return Object.values(map)
  }, [filteredTransactions])

  /* =========================
     HOURLY DATA
  ========================== */
  const hourlyData = useMemo(() => {
    return Array.from({ length: 24 }, (_, h) => ({
      hour: `${h}:00`,
      count: filteredTransactions.filter(
        (t) => dayjs(t.createdAt).hour() === h
      ).length,
    }))
  }, [filteredTransactions])

  /* =========================
     STATUS DISTRIBUTION
  ========================== */
  const statusData = useMemo(() => {
    const map = {}
    filteredTransactions.forEach((t) => {
      const key = t.status || "Unknown"
      map[key] = (map[key] || 0) + 1
    })
    return Object.entries(map).map(([name, value]) => ({ name, value }))
  }, [filteredTransactions])

  /* =========================
     COMMODITY BREAKDOWN
  ========================== */
  const commodityData = useMemo(() => {
    const map = {}
    filteredTransactions.forEach((t) => {
      const key = t.commodityName || "Unknown"
      map[key] = map[key] || { name: key, weight: 0 }
      map[key].weight += t.netWeight || 0
    })
    return Object.values(map).sort((a, b) => b.weight - a.weight)
  }, [filteredTransactions])

  /* =========================
     PREVIOUS PERIOD COMPARISON
  ========================== */
  const comparison = useMemo(() => {
    if (!fromDate || !toDate) return null

    const range = dayjs(toDate).diff(dayjs(fromDate), "day") + 1
    const prevFrom = dayjs(fromDate).subtract(range, "day")
    const prevTo = dayjs(toDate).subtract(range, "day")

    const prevData = transactions.filter((t) => {
      const d = dayjs(t.createdAt)
      return d.isAfter(prevFrom) && d.isBefore(prevTo)
    })
    const prevWeight = prevData.reduce((s, t) => s + (t.netWeight || 0), 0)

    return {
      weightChange:
        prevWeight > 0
          ? Math.round(((totalNetWeight - prevWeight) / prevWeight) * 100)
          : 0,
    }
  }, [fromDate, toDate, totalNetWeight, transactions])

  /* =========================
     EXPORT PREVIEW
  ========================== */
  const exportPayload = {
    filters: { status, fromDate, toDate },
    summary: { totalTransactions, totalNetWeight, avgWeight, completionRate },
    data: filteredTransactions,
  }

  return (
    <div className="h-full overflow-y-auto space-y-8 pr-2">
      {/* FILTERS */}
      <div className="flex flex-wrap gap-3">
        <select
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          className="border rounded px-2 py-1 text-sm"
        >
          <option value="ALL">All Status</option>
          <option value="Completed">Completed</option>
          <option value="Pending">Pending</option>
        </select>

        <input
          type="date"
          value={fromDate}
          onChange={(e) => setFromDate(e.target.value)}
          className="border rounded px-2 py-1 text-sm"
        />

        <input
          type="date"
          value={toDate}
          onChange={(e) => setToDate(e.target.value)}
          className="border rounded px-2 py-1 text-sm"
        />
      </div>

      {/* KPI STRIP */}
      <div className="grid grid-cols-2 sm:grid-cols-6 gap-3">
        <KPI label="Transactions" value={totalTransactions} />
        <KPI label="Total Weight" value={totalNetWeight.toLocaleString()} />
        <KPI label="Avg Weight" value={avgWeight.toLocaleString()} />
        <KPI label="Completed %" value={`${completionRate}%`} />
        {comparison && (
          <KPI label="Weight Change" value={`${comparison.weightChange}%`} />
        )}
      </div>

      {/* VOLUME + STATUS */}
      <Section title="Volume Overview">
        <ChartCard title="Transactions per Day">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={dailyData}>
              <XAxis dataKey="day" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Status Distribution">
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie data={statusData} dataKey="value" label outerRadius={90}>
                {statusData.map((_, i) => (
                  <Cell key={i} fill={i % 2 ? YELLOW : YELLOW_LIGHT} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </Section>

      {/* TIME + COMMODITY */}
      <Section title="Time & Commodity">
        <ChartCard title="Hourly Transactions">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={hourlyData}>
              <XAxis dataKey="hour" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="count" fill={YELLOW_LIGHT} />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Commodity Contribution">
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={commodityData}>
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="weight" fill={YELLOW} />
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
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">{children}</div>
    </div>
  )
}
