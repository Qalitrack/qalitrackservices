import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { fetchTransactions } from "../../store/weighingSlice";
import {
  Users, RefreshCw, Shield, Clock,
  ChevronRight, Truck, CheckCircle2, Hourglass, Weight,
  Activity, BarChart3, AlertTriangle, LayoutDashboard,
} from "lucide-react";
import PageHeader from "../../components/PageHeader.jsx";
import { useColorScheme } from "../../components/Context/ColorSchemeContext.jsx";
import { vibrantAccent } from "../../utils/schemeChartColor.js";
import CountUp from "react-countup";
import Chart from "react-apexcharts";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";
import { getTransactionStats } from "../../api/Transaction/Transaction.js";

const REFRESH_INTERVAL = 30000;

// ─── Helpers ───────────────────────────────────────────────────────────────────
const processRoles = (roles, users) => {
  if (!roles || !users) return [];
  const counts = roles.reduce((a, r) => ({ ...a, [r.name]: 0 }), {});
  users.forEach((u) => (u.roles || []).forEach((r) => { if (r in counts) counts[r]++; }));
  return roles.map((r) => ({ name: r.name, value: counts[r.name] || 0 }));
};

// ─── Chart configs ─────────────────────────────────────────────────────────────
const baseChart = (type) => ({
  type,
  toolbar: { show: false },
  background: "transparent",
  foreColor: "#6b7280",
  animations: { enabled: true, speed: 500 },
});

const barOpts = (cats, color = "#f59e0b") => ({
  chart: baseChart("bar"),
  xaxis: { categories: cats, labels: { style: { colors: "#9ca3af", fontSize: "9px" } }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { labels: { style: { colors: "#9ca3af", fontSize: "9px" }, formatter: (v) => Math.round(v) } },
  colors: [color],
  plotOptions: { bar: { borderRadius: 4, columnWidth: "58%" } },
  grid: { borderColor: "#f3f4f6", strokeDashArray: 3 },
  dataLabels: { enabled: false },
  tooltip: { theme: "light", style: { fontSize: "10px" } },
});

const hbarOpts = (cats, color = "#f59e0b") => ({
  chart: baseChart("bar"),
  xaxis: { labels: { style: { colors: "#9ca3af", fontSize: "9px" } }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { categories: cats, labels: { style: { colors: "#6b7280", fontSize: "9px" } } },
  colors: [color],
  plotOptions: { bar: { horizontal: true, borderRadius: 3, barHeight: "55%" } },
  grid: { borderColor: "#f3f4f6", strokeDashArray: 3 },
  dataLabels: { enabled: false },
  tooltip: { theme: "light", style: { fontSize: "10px" } },
});

// ─── UI Components ─────────────────────────────────────────────────────────────
const Panel = ({ title, subtitle, children, className = "", action }) => (
  <div className={`bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden ${className}`}>
    <div className="flex items-center justify-between px-4 pt-3 pb-1">
      <div>
        <p className="text-xs font-bold text-gray-800 uppercase tracking-wider">{title}</p>
        {subtitle && <p className="text-[11px] text-gray-500 mt-0.5">{subtitle}</p>}
      </div>
      {action}
    </div>
    <div className="px-4 pb-3">{children}</div>
  </div>
);

const KpiCard = ({ icon: Icon, label, value, sub, accent, loading, suffix }) => (
  <div className="relative bg-white rounded-xl border-2 border-gray-100 shadow-sm p-4 flex flex-col gap-3
    hover:border-amber-400 hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200 cursor-default">
    <span className="text-[11px] font-semibold text-gray-500 uppercase tracking-wide truncate pr-8">{label}</span>
    <div>
      <p className="text-3xl font-black tabular-nums text-gray-900 leading-none">
        {loading
          ? <span className="text-gray-200">—</span>
          : <><CountUp end={typeof value === "number" ? value : 0} duration={1} separator="," />{suffix && <span className="text-xs font-normal text-gray-400 ml-0.5">{suffix}</span>}</>}
      </p>
      {sub && <p className="text-[11px] text-gray-600 mt-1.5">{sub}</p>}
    </div>
    <div className="absolute bottom-3 right-3 p-2 rounded-lg" style={{ backgroundColor: accent + "20" }}>
      <Icon size={15} style={{ color: accent }} />
    </div>
  </div>
);

const MiniBar = ({ pct, color }) => (
  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
    <div className="h-full rounded-full transition-all duration-700" style={{ width: `${pct}%`, backgroundColor: color }} />
  </div>
);

const StatusBadge = ({ label, color }) => (
  <span className="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold border"
    style={{ backgroundColor: color + "12", color, borderColor: color + "30" }}>
    {label}
  </span>
);

const Spinner = () => (
  <div className="h-36 flex items-center justify-center">
    <div className="w-6 h-6 rounded-full border-2 border-amber-200 border-t-amber-500 animate-spin" />
  </div>
);

// ─── Main ──────────────────────────────────────────────────────────────────────
export default function AdminDashboard() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { colorScheme, COLOR_SCHEMES } = useColorScheme();
  const schemeColors = COLOR_SCHEMES[colorScheme];
  const chartAccent = useMemo(() => vibrantAccent(schemeColors.primary), [schemeColors.primary]);

  // Redux state — transactions (last 8, for the Recent Transactions table)
  const { transactions: txs, loading: txLoading } = useSelector((s) => s.weighing);

  // Local state for users/roles (no Redux slice for these)
  const [allUsers, setAllUsers] = useState([]);
  const [rolesData, setRolesData] = useState([]);
  const [usersLoading, setUsersLoading] = useState(true);
  const [stats, setStats] = useState(null);
  const [statsLoading, setStatsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastUpdated, setLastUpdated] = useState(null);

  const loading = txLoading || usersLoading || statsLoading;

  // Only the last 8 transactions are fetched here now (for the Recent
  // Transactions table) — counts/sums/breakdowns come from the stats endpoint
  // below instead of pulling the entire transaction table every 30s.
  const fetchAllData = useCallback(() => {
    dispatch(fetchTransactions({ pageSize: 8 }));
    setLastUpdated(new Date());
  }, [dispatch]);

  const loadStats = useCallback(async (signal) => {
    try {
      setStatsLoading(true);
      const res = await getTransactionStats(signal);
      setStats(res?.data ?? res);
      setError(null);
    } catch (e) {
      if (e.name !== "AbortError" && e.name !== "CanceledError") {
        setError("Failed to load dashboard stats.");
      }
    } finally {
      setStatsLoading(false);
    }
  }, []);

  // Fetch users + roles directly (no Redux slice)
  const loadUsers = useCallback(async (signal) => {
    try {
      setUsersLoading(true);
      const [usersData, rolesRes] = await Promise.all([
        fetchUsers(1, 1000, signal),
        fetchRoles(signal),
      ]);
      const users = usersData?.items || usersData?.data?.items || (Array.isArray(usersData) ? usersData : []);
      setAllUsers(users);
      setRolesData(processRoles(rolesRes, users));
      setError(null);
    } catch (e) {
      if (e.name !== "AbortError" && e.name !== "CanceledError") {
        setError("Failed to load user data.");
      }
    } finally {
      setUsersLoading(false);
    }
  }, []);

  useEffect(() => {
    const c = new AbortController();
    fetchAllData();
    loadStats(c.signal);
    loadUsers(c.signal);
    const t = setInterval(() => {
      fetchAllData();
      loadStats(c.signal);
      loadUsers(c.signal);
    }, REFRESH_INTERVAL);
    return () => { c.abort(); clearInterval(t); };
  }, [fetchAllData, loadStats, loadUsers]);

  const refresh = useCallback(() => {
    const c = new AbortController();
    fetchAllData();
    loadStats(c.signal);
    loadUsers(c.signal);
  }, [fetchAllData, loadStats, loadUsers]);

  // Derived stats
  const totalUsers   = allUsers.length;
  const activeUsers  = useMemo(() => allUsers.filter((u) => u.isActive).length, [allUsers]);
  const inactiveUsers = totalUsers - activeUsers;

  // Only the last 8 rows — used for the Recent Transactions table, nothing else
  const txList = useMemo(() => Array.isArray(txs) ? txs : [], [txs]);

  const totalCount          = stats?.totalCount ?? 0;
  const completedCount      = stats?.completedCount ?? 0;
  const activeCount         = stats?.activeCount ?? 0;
  const todayCount          = stats?.todayCount ?? 0;
  const todayCompletedCount = stats?.todayCompletedCount ?? 0;
  const thisWeekCount       = stats?.thisWeekCount ?? 0;
  const totalNet            = stats?.totalNetWeight ?? 0;
  const todayNet            = stats?.todayNetWeight ?? 0;
  const stuckCount          = stats?.stuckCount ?? 0;
  const oldestActiveAgeMinutes = stats?.oldestActiveAgeMinutes ?? null;

  const formatAge = (minutes) => {
    if (minutes == null) return null;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  };

  const weekTrend = useMemo(() => {
    const trend = stats?.weeklyTrend ?? [];
    return {
      labels: trend.map((d) => new Date(d.date).toLocaleDateString("en-US", { weekday: "short" })),
      counts: trend.map((d) => d.count),
    };
  }, [stats]);

  const topPlates = useMemo(
    () => (stats?.topVehicles ?? []).map((v) => [v.name, v.count]),
    [stats]
  );

  const commodityMix = useMemo(() => {
    const mix = stats?.commodityMix ?? [];
    return { labels: mix.map((c) => c.name), values: mix.map((c) => c.count) };
  }, [stats]);

  const recentTxs   = useMemo(() => [...txList].sort((a, b) => new Date(b.firstWeightDate || b.createdAt) - new Date(a.firstWeightDate || a.createdAt)).slice(0, 8), [txList]);

  // Chart options (memoized) — bar colors follow the active color scheme via
  // chartAccent (a vivid derivative of `primary`), not the muted preview swatch.
  const weekBarChart    = useMemo(() => barOpts(weekTrend.labels, chartAccent), [weekTrend.labels, chartAccent]);
  const platesHbarChart = useMemo(() => hbarOpts(topPlates.map((p) => p[0]), chartAccent), [topPlates, chartAccent]);
  const commodityChart  = useMemo(() => barOpts(commodityMix.labels, chartAccent), [commodityMix.labels, chartAccent]);

  const kpis = [
    { icon: Truck,        label: "Total Tickets",  value: totalCount,          sub: `${thisWeekCount} this week`,    accent: "#f59e0b" },
    { icon: CheckCircle2, label: "Completed",       value: completedCount,      sub: totalCount ? `${Math.round((completedCount / totalCount) * 100)}% rate` : "—", accent: "#10b981" },
    { icon: Hourglass,    label: "Pending W2",      value: activeCount,         sub: "Awaiting 2nd weight",            accent: "#ef4444" },
    { icon: Activity,     label: "Today",           value: todayCount,          sub: `${todayCompletedCount} completed`, accent: "#3b82f6" },
    { icon: Weight,       label: "Net Weight",      value: Math.round(totalNet / 1000), suffix: "T", sub: `${Math.round(todayNet / 1000)} T today`, accent: "#8b5cf6" },
    { icon: BarChart3,    label: "This Week",       value: thisWeekCount,       sub: "Tickets processed",              accent: "#f59e0b" },
    { icon: Users,        label: "Total Users",     value: totalUsers,          sub: `${activeUsers} active`,          accent: "#d97706" },
    { icon: Shield,       label: "Roles",           value: rolesData.length,    sub: "Permission groups",              accent: "#6b7280" },
  ];

  return (
    <div className="h-full flex flex-col rounded-lg shadow-md border border-gray-200 bg-white overflow-hidden">

      <PageHeader
        icon={LayoutDashboard}
        title="Qalitrack Dashboard"
        subtitle={
          <span className="flex items-center gap-1">
            <Clock size={10} />
            {lastUpdated
              ? `Updated ${lastUpdated.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}`
              : "Loading…"}
            {!loading && <span className="ml-1 inline-block w-1.5 h-1.5 rounded-full animate-pulse" style={{ background: "var(--cs-appbar-text)" }} />}
          </span>
        }
        flush
        className="border-b border-white/10"
        actions={
          <button onClick={refresh} disabled={loading}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold active:scale-95 transition-all disabled:opacity-50 border"
            style={{ background: "rgba(128,128,128,0.15)", color: "var(--cs-appbar-text)", borderColor: "rgba(128,128,128,0.2)" }}>
            <RefreshCw size={11} className={loading ? "animate-spin" : ""} /> Refresh
          </button>
        }
      />

      <div className="flex-1 overflow-y-auto p-5 space-y-4 min-w-0">

        {error && (
          <div className="px-3 py-2 bg-red-50 border border-red-200 text-red-600 rounded-lg text-xs">{error}</div>
        )}

        {!loading && stuckCount > 0 && (
          <div className="flex items-center gap-2 px-3 py-2 bg-red-50 border border-red-300 text-red-800 rounded-lg text-xs font-semibold">
            <AlertTriangle size={14} className="shrink-0" />
            {stuckCount} ticket{stuckCount !== 1 ? "s" : ""} pending 2nd weight for over 2 hours
            {oldestActiveAgeMinutes != null && ` — oldest: ${formatAge(oldestActiveAgeMinutes)}`}
          </div>
        )}

        {/* KPI Strip */}
        <div className="grid grid-cols-4 lg:grid-cols-8 gap-3">
          {kpis.map((k) => <KpiCard key={k.label} {...k} loading={loading} />)}
        </div>

        {/* Row 2: Weekly bar + Today summary */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3">

          <Panel title="Daily Tickets" subtitle="Last 7 days" className="lg:col-span-6">
            {loading ? <Spinner />
              : <Chart options={weekBarChart} series={[{ name: "Tickets", data: weekTrend.counts }]} type="bar" height={145} />}
          </Panel>

          <Panel title="Today's Summary" subtitle={new Date().toLocaleDateString("en-US", { weekday: "long", month: "short", day: "numeric" })} className="lg:col-span-6">
            <div className="space-y-3 mt-2">
              {[
                { label: "Tickets Today",  val: todayCount,             total: totalCount,  color: "#f59e0b" },
                { label: "Completed",      val: todayCompletedCount,    total: todayCount,  color: "#10b981" },
                { label: "Pending W2",     val: activeCount,            total: totalCount,  color: "#ef4444" },
                { label: "Today Net Wt",   val: `${Math.round(todayNet / 1000)} T`, total: null, color: "#8b5cf6" },
              ].map(({ label, val, total, color }) => (
                <div key={label}>
                  <div className="flex justify-between items-center mb-1">
                    <span className="text-[10px] text-gray-500 font-semibold uppercase tracking-wide">{label}</span>
                    <span className="text-xs font-bold" style={{ color }}>
                      {loading ? "—" : val}
                      {total !== null && !loading && <span className="text-gray-400 font-normal"> / {total}</span>}
                    </span>
                  </div>
                  {total !== null && !loading && (
                    <MiniBar pct={total > 0 ? Math.round((Number(val) / total) * 100) : 0} color={color} />
                  )}
                </div>
              ))}
            </div>
          </Panel>
        </div>

        {/* Row 3: Top vehicles + Commodity mix */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-3">

          <Panel title="Top Vehicles" subtitle="By number of tickets">
            {loading ? <Spinner />
              : topPlates.length > 0
                ? <Chart options={platesHbarChart} series={[{ name: "Tickets", data: topPlates.map(p => p[1]) }]} type="bar" height={145} />
                : <div className="h-36 flex items-center justify-center text-xs text-gray-400">No vehicle data yet</div>}
          </Panel>

          <Panel title="Commodity Mix" subtitle="Most weighed commodities">
            {loading ? <Spinner />
              : commodityMix.values.length > 0
                ? <Chart options={commodityChart} series={[{ name: "Tickets", data: commodityMix.values }]} type="bar" height={145} />
                : <div className="h-36 flex items-center justify-center text-xs text-gray-400">No commodity data yet</div>}
          </Panel>
        </div>

        {/* Row 4: Recent transactions + Role donut + Role breakdown */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3">

          <Panel title="Recent Transactions" subtitle="Latest weighbridge tickets" className="lg:col-span-7"
            action={<button onClick={() => navigate("../transactions")} className="text-[10px] text-amber-600 hover:text-amber-700 font-semibold flex items-center gap-0.5">View all <ChevronRight size={10} /></button>}>
            {loading && recentTxs.length === 0
              ? <div className="space-y-2 mt-1">{[...Array(5)].map((_, i) => <div key={i} className="h-6 bg-gray-100 rounded animate-pulse" />)}</div>
              : recentTxs.length === 0
                ? <p className="text-xs text-gray-400 py-6 text-center">No transactions found</p>
                : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-[10px] mt-1">
                      <thead>
                        <tr className="border-b border-gray-100">
                          {["Receipt", "Plate", "Transporter", "Net Wt", "Status", "Date"].map((h, i) => (
                            <th key={h} className={`py-1.5 pr-2 text-gray-400 font-semibold uppercase tracking-wide ${i > 2 ? "text-right" : "text-left"}`}>{h}</th>
                          ))}
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-50">
                        {recentTxs.map((tx) => (
                          <tr key={tx.ticketID || tx.id} className="hover:bg-amber-50 transition-colors">
                            <td className="py-1.5 pr-2 font-mono font-semibold text-amber-600">{tx.receiptNo || "—"}</td>
                            <td className="py-1.5 pr-2 font-bold text-gray-800">{tx.noPlate || "—"}</td>
                            <td className="py-1.5 pr-2 text-gray-500 truncate max-w-[80px]">{tx.transporterName || "—"}</td>
                            <td className="py-1.5 pr-2 text-right tabular-nums text-gray-700">{tx.netWeight ? `${parseFloat(tx.netWeight).toLocaleString()} kg` : "—"}</td>
                            <td className="py-1.5 pr-2 text-right">
                              <StatusBadge
                                label={tx.status || "—"}
                                color={tx.status === "Completed" ? "#10b981" : tx.status === "Active" ? "#f59e0b" : "#9ca3af"}
                              />
                            </td>
                            <td className="py-1.5 text-right text-gray-400">
                              {tx.firstWeightDate ? new Date(tx.firstWeightDate).toLocaleDateString("en-US", { month: "short", day: "numeric" }) : "—"}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
          </Panel>

          <div className="lg:col-span-5 flex flex-col gap-3">
            <Panel title="Users per Role" subtitle="Assignment breakdown">
              <div className="space-y-2.5 mt-1">
                {loading
                  ? [...Array(3)].map((_, i) => <div key={i} className="h-4 bg-gray-100 rounded animate-pulse" />)
                  : rolesData.map((r) => {
                    const pct = totalUsers > 0 ? Math.round((r.value / totalUsers) * 100) : 0;
                    return (
                      <div key={r.name}>
                        <div className="flex justify-between mb-1">
                          <span className="text-[10px] text-gray-600 font-medium">{r.name}</span>
                          <span className="text-[10px] font-bold text-amber-600">{r.value} <span className="text-gray-400 font-normal">({pct}%)</span></span>
                        </div>
                        <MiniBar pct={pct} color={chartAccent} />
                      </div>
                    );
                  })}
                <div className="grid grid-cols-3 gap-1 pt-2 border-t border-gray-100 mt-2">
                  {[{ label: "Total", val: totalUsers, color: "text-gray-900" }, { label: "Active", val: activeUsers, color: "text-emerald-600" }, { label: "Inactive", val: inactiveUsers, color: "text-gray-400" }].map(({ label, val, color }) => (
                    <div key={label} className="text-center">
                      <p className={`text-base font-black tabular-nums ${color}`}>{loading ? "—" : <CountUp end={val} duration={1} separator="," />}</p>
                      <p className="text-[9px] text-gray-400 uppercase tracking-wide">{label}</p>
                    </div>
                  ))}
                </div>
              </div>
            </Panel>
          </div>
        </div>

        <p className="text-center text-[10px] text-gray-400 pb-1">
          Powered by <span className="font-semibold text-gray-500">Qalibrated Systems</span> — Qalitrack v0.0.1
        </p>
      </div>
    </div>
  );
}
