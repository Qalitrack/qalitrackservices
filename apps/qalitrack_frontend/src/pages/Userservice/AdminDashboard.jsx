import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  fetchTransactions,
  fetchDrivers,
  fetchVehicles,
} from "../../store/weighingSlice";
import {
  Users, RefreshCw, Shield, Clock,
  ChevronRight, Circle, Truck, CheckCircle2, Hourglass, Weight,
  Activity, BarChart3, ArrowUpRight,
} from "lucide-react";
import CountUp from "react-countup";
import Chart from "react-apexcharts";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";

const REFRESH_INTERVAL = 30000;

// ─── Helpers ───────────────────────────────────────────────────────────────────
const isToday = (d) => {
  if (!d) return false;
  const t = new Date(d), n = new Date();
  return t.getDate() === n.getDate() && t.getMonth() === n.getMonth() && t.getFullYear() === n.getFullYear();
};
const isThisWeek = (d) => {
  if (!d) return false;
  const t = new Date(d), ago = new Date(); ago.setDate(ago.getDate() - 7);
  return t >= ago && t <= new Date();
};
const processUserGrowth = (users) => {
  if (!users?.length) return { dates: [], counts: [] };
  const sorted = [...users].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
  const map = {};
  sorted.forEach((u) => {
    const d = new Date(u.createdAt).toLocaleDateString("en-US", { month: "short", day: "numeric" });
    map[d] = (map[d] || 0) + 1;
  });
  const dates = Object.keys(map);
  let cum = 0;
  return { dates, counts: dates.map((d) => (cum += map[d])) };
};
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

const areaOpts = (cats, color = "#f59e0b") => ({
  chart: baseChart("area"),
  xaxis: { categories: cats, labels: { style: { colors: "#9ca3af", fontSize: "9px" }, rotate: -30, hideOverlappingLabels: true }, axisBorder: { show: false }, axisTicks: { show: false } },
  yaxis: { labels: { style: { colors: "#9ca3af", fontSize: "9px" }, formatter: (v) => Math.round(v) } },
  colors: [color], stroke: { curve: "smooth", width: 2 },
  fill: { type: "gradient", gradient: { shade: "light", opacityFrom: 0.25, opacityTo: 0.0 } },
  grid: { borderColor: "#f3f4f6", strokeDashArray: 3 },
  dataLabels: { enabled: false },
  tooltip: { theme: "light", style: { fontSize: "10px" } },
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

const donutOpts = (labels, colors) => ({
  chart: baseChart("donut"),
  labels, colors,
  legend: { position: "bottom", fontSize: "10px", fontWeight: 600, labels: { colors: "#6b7280" }, markers: { width: 8, height: 8, radius: 4 }, itemMargin: { horizontal: 4 } },
  plotOptions: { pie: { donut: { size: "65%", labels: { show: true, total: { show: true, label: "Total", fontSize: "10px", fontWeight: 700, color: "#d97706", formatter: (w) => w.globals.seriesTotals.reduce((a, b) => a + b, 0) }, value: { fontSize: "16px", fontWeight: 700, color: "#111827" } } } } },
  stroke: { width: 0 }, dataLabels: { enabled: false },
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
        {subtitle && <p className="text-[10px] text-gray-400 mt-0.5">{subtitle}</p>}
      </div>
      {action}
    </div>
    <div className="px-4 pb-3">{children}</div>
  </div>
);

const KpiCard = ({ icon: Icon, label, value, sub, accent, loading, suffix }) => (
  <div className="bg-white rounded-xl border-2 border-gray-100 shadow-sm p-3 flex flex-col gap-1
    hover:border-amber-400 hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200 cursor-default">
    <div className="flex items-center justify-between">
      <div className="p-1.5 rounded-lg" style={{ backgroundColor: accent + "20" }}>
        <Icon size={13} style={{ color: accent }} />
      </div>
      <ArrowUpRight size={11} className="text-gray-300" />
    </div>
    <p className="text-2xl font-black tabular-nums text-gray-900 leading-none">
      {loading
        ? <span className="text-gray-200">—</span>
        : <><CountUp end={typeof value === "number" ? value : 0} duration={1} separator="," />{suffix && <span className="text-xs font-normal text-gray-400 ml-0.5">{suffix}</span>}</>}
    </p>
    <p className="text-[10px] font-semibold text-gray-500 uppercase tracking-wide leading-none">{label}</p>
    {sub && <p className="text-[9px] text-gray-400 leading-none">{sub}</p>}
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

  // Redux state — transactions, drivers, vehicles
  const { transactions: txs, drivers, vehicles, loading: txLoading } = useSelector((s) => s.weighing);

  // Local state for users/roles (no Redux slice for these)
  const [allUsers, setAllUsers] = useState([]);
  const [rolesData, setRolesData] = useState([]);
  const [userGrowth, setUserGrowth] = useState({ dates: [], counts: [] });
  const [usersLoading, setUsersLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastUpdated, setLastUpdated] = useState(null);

  const loading = txLoading || usersLoading;

  // Fetch transactions, drivers, vehicles via Redux
  const fetchAllData = useCallback(() => {
    dispatch(fetchTransactions({ pageSize: 10000 }));
    dispatch(fetchDrivers());
    dispatch(fetchVehicles());
    setLastUpdated(new Date());
  }, [dispatch]);

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
      setUserGrowth(processUserGrowth(users));
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
    loadUsers(c.signal);
    const t = setInterval(() => {
      fetchAllData();
      loadUsers(c.signal);
    }, REFRESH_INTERVAL);
    return () => { c.abort(); clearInterval(t); };
  }, [fetchAllData, loadUsers]);

  const refresh = useCallback(() => {
    const c = new AbortController();
    fetchAllData();
    loadUsers(c.signal);
  }, [fetchAllData, loadUsers]);

  // Derived stats
  const totalUsers   = allUsers.length;
  const activeUsers  = useMemo(() => allUsers.filter((u) => u.isActive).length, [allUsers]);
  const inactiveUsers = totalUsers - activeUsers;

  const txList       = useMemo(() => Array.isArray(txs) ? txs : [], [txs]);
  const todayTxs     = useMemo(() => txList.filter((t) => isToday(t.firstWeightDate || t.createdAt)), [txList]);
  const completedTxs = useMemo(() => txList.filter((t) => t.status === "Completed"), [txList]);
  const pendingTxs   = useMemo(() => txList.filter((t) => t.status === "Active"), [txList]);
  const weekTxs      = useMemo(() => txList.filter((t) => isThisWeek(t.firstWeightDate || t.createdAt)), [txList]);
  const totalNet     = useMemo(() => txList.reduce((s, t) => s + (parseFloat(t.netWeight) || 0), 0), [txList]);
  const todayNet     = useMemo(() => todayTxs.reduce((s, t) => s + (parseFloat(t.netWeight) || 0), 0), [todayTxs]);

  const weekTrend = useMemo(() => {
    const days = Array.from({ length: 7 }, (_, i) => { const d = new Date(); d.setDate(d.getDate() - (6 - i)); return d; });
    return {
      labels: days.map((d) => d.toLocaleDateString("en-US", { weekday: "short" })),
      counts: days.map((d) => txList.filter((t) => {
        const x = new Date(t.firstWeightDate || t.createdAt);
        return x.getDate() === d.getDate() && x.getMonth() === d.getMonth() && x.getFullYear() === d.getFullYear();
      }).length),
    };
  }, [txList]);

  const topPlates = useMemo(() => {
    const map = {};
    txList.forEach((t) => { if (t.noPlate) map[t.noPlate] = (map[t.noPlate] || 0) + 1; });
    return Object.entries(map).sort((a, b) => b[1] - a[1]).slice(0, 5);
  }, [txList]);

  const commodityMix = useMemo(() => {
    const map = {};
    txList.forEach((t) => { if (t.commodityName) map[t.commodityName] = (map[t.commodityName] || 0) + 1; });
    const sorted = Object.entries(map).sort((a, b) => b[1] - a[1]).slice(0, 5);
    return { labels: sorted.map((x) => x[0]), values: sorted.map((x) => x[1]) };
  }, [txList]);

  const statusDonut = useMemo(() => ({
    labels: ["Completed", "Pending (W2)", "Other"],
    values: [completedTxs.length, pendingTxs.length, Math.max(0, txList.length - completedTxs.length - pendingTxs.length)],
  }), [txList, completedTxs, pendingTxs]);

  const recentTxs   = useMemo(() => [...txList].sort((a, b) => new Date(b.firstWeightDate || b.createdAt) - new Date(a.firstWeightDate || a.createdAt)).slice(0, 8), [txList]);
  const recentUsers = useMemo(() => [...allUsers].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 6), [allUsers]);

  // Chart options (memoized)
  const weekBarChart    = useMemo(() => barOpts(weekTrend.labels, "#f59e0b"), [weekTrend.labels]);
  const growthAreaChart = useMemo(() => areaOpts(userGrowth.dates, "#d97706"), [userGrowth.dates]);
  const statusDonutChart = useMemo(() => donutOpts(statusDonut.labels, ["#10b981", "#f59e0b", "#9ca3af"]), [statusDonut.labels]);
  const roleDonutChart  = useMemo(() => donutOpts(rolesData.map((r) => r.name), ["#f59e0b", "#d97706", "#fbbf24", "#fcd34d", "#92400e"]), [rolesData]);
  const platesHbarChart = useMemo(() => hbarOpts(topPlates.map((p) => p[0]), "#f59e0b"), [topPlates]);
  const commodityChart  = useMemo(() => barOpts(commodityMix.labels, "#d97706"), [commodityMix.labels]);

  const kpis = [
    { icon: Truck,        label: "Total Tickets",  value: txList.length,         sub: `${weekTxs.length} this week`,    accent: "#f59e0b" },
    { icon: CheckCircle2, label: "Completed",       value: completedTxs.length,   sub: txList.length ? `${Math.round((completedTxs.length / txList.length) * 100)}% rate` : "—", accent: "#10b981" },
    { icon: Hourglass,    label: "Pending W2",      value: pendingTxs.length,     sub: "Awaiting 2nd weight",            accent: "#ef4444" },
    { icon: Activity,     label: "Today",           value: todayTxs.length,       sub: `${todayTxs.filter(t => t.status === "Completed").length} completed`, accent: "#3b82f6" },
    { icon: Weight,       label: "Net Weight",      value: Math.round(totalNet / 1000), suffix: "T", sub: `${Math.round(todayNet / 1000)} T today`, accent: "#8b5cf6" },
    { icon: BarChart3,    label: "This Week",       value: weekTxs.length,        sub: "Tickets processed",              accent: "#f59e0b" },
    { icon: Users,        label: "Total Users",     value: totalUsers,            sub: `${activeUsers} active`,          accent: "#d97706" },
    { icon: Shield,       label: "Roles",           value: rolesData.length,      sub: "Permission groups",              accent: "#6b7280" },
  ];

  return (
    <div className="h-full overflow-y-auto bg-gray-50">

      {/* ── Amber accent header band ── */}
      <div className="bg-gradient-to-r from-amber-500 to-amber-400 px-5 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-sm font-black text-white tracking-tight">Qalitrack Dashboard</h1>
            <p className="text-[10px] text-amber-100 flex items-center gap-1 mt-0.5">
              <Clock size={10} />
              {lastUpdated
                ? `Updated ${lastUpdated.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}`
                : "Loading…"}
              {!loading && <span className="ml-1 inline-block w-1.5 h-1.5 rounded-full bg-white animate-pulse" />}
            </p>
          </div>
          <button onClick={refresh} disabled={loading}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-white/20 hover:bg-white/30 text-white rounded-lg text-xs font-bold active:scale-95 transition-all disabled:opacity-50 border border-white/30">
            <RefreshCw size={11} className={loading ? "animate-spin" : ""} /> Refresh
          </button>
        </div>

        {/* Inline mini stats in header */}
        <div className="flex gap-4 mt-3">
          {[
            { label: "Transactions", val: txList.length },
            { label: "Drivers", val: Array.isArray(drivers) ? drivers.length : 0 },
            { label: "Vehicles", val: Array.isArray(vehicles) ? vehicles.length : 0 },
            { label: "Users", val: totalUsers },
          ].map(({ label, val }) => (
            <div key={label} className="text-center">
              <p className="text-base font-black text-white tabular-nums leading-none">
                {loading ? "—" : <CountUp end={val} duration={1} separator="," />}
              </p>
              <p className="text-[9px] text-amber-100 uppercase tracking-wide mt-0.5">{label}</p>
            </div>
          ))}
        </div>
      </div>

      <div className="p-5 space-y-4 min-w-0">

        {error && (
          <div className="px-3 py-2 bg-red-50 border border-red-200 text-red-600 rounded-lg text-xs">{error}</div>
        )}

        {/* KPI Strip */}
        <div className="grid grid-cols-4 lg:grid-cols-8 gap-2">
          {kpis.map((k) => <KpiCard key={k.label} {...k} loading={loading} />)}
        </div>

        {/* Row 2: Weekly bar + Status donut + Today summary */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3">

          <Panel title="Daily Tickets" subtitle="Last 7 days" className="lg:col-span-4">
            {loading ? <Spinner />
              : <Chart options={weekBarChart} series={[{ name: "Tickets", data: weekTrend.counts }]} type="bar" height={145} />}
          </Panel>

          <Panel title="Ticket Status" subtitle="All time breakdown" className="lg:col-span-3">
            {loading ? <Spinner />
              : statusDonut.values.some((v) => v > 0)
                ? <Chart options={statusDonutChart} series={statusDonut.values} type="donut" height={145} />
                : <div className="h-36 flex items-center justify-center text-xs text-gray-400">No data yet</div>}
          </Panel>

          <Panel title="Today's Summary" subtitle={new Date().toLocaleDateString("en-US", { weekday: "long", month: "short", day: "numeric" })} className="lg:col-span-5">
            <div className="space-y-3 mt-2">
              {[
                { label: "Tickets Today",  val: todayTxs.length,                                        total: txList.length,    color: "#f59e0b" },
                { label: "Completed",      val: todayTxs.filter(t => t.status === "Completed").length,  total: todayTxs.length,  color: "#10b981" },
                { label: "Pending W2",     val: pendingTxs.length,                                      total: txList.length,    color: "#ef4444" },
                { label: "Today Net Wt",   val: `${Math.round(todayNet / 1000)} T`,                     total: null,             color: "#8b5cf6" },
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

        {/* Row 3: Top vehicles + Commodity mix + User growth */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">

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

          <Panel title="User Growth" subtitle="Cumulative registrations over time">
            {loading ? <Spinner />
              : userGrowth.dates.length > 0
                ? <Chart options={growthAreaChart} series={[{ name: "Users", data: userGrowth.counts }]} type="area" height={145} />
                : <div className="h-36 flex items-center justify-center text-xs text-gray-400">No data yet</div>}
          </Panel>
        </div>

        {/* Row 4: Recent transactions + Role donut + Role breakdown */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3">

          <Panel title="Recent Transactions" subtitle="Latest weighbridge tickets" className="lg:col-span-7"
            action={<button className="text-[10px] text-amber-600 hover:text-amber-700 font-semibold flex items-center gap-0.5">View all <ChevronRight size={10} /></button>}>
            {loading
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
            <Panel title="Role Distribution" subtitle={`${rolesData.length} roles configured`}>
              {loading ? <Spinner />
                : rolesData.length > 0 && rolesData.some((r) => r.value > 0)
                  ? <Chart options={roleDonutChart} series={rolesData.map((r) => r.value)} type="donut" height={135} />
                  : <div className="h-32 flex items-center justify-center text-xs text-gray-400">No data yet</div>}
            </Panel>

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
                        <MiniBar pct={pct} color="#f59e0b" />
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

        {/* Row 5: Recent users */}
        <Panel title="Recent Users" subtitle="Latest registrations"
          action={<button className="text-[10px] text-amber-600 hover:text-amber-700 font-semibold flex items-center gap-0.5">View all <ChevronRight size={10} /></button>}>
          {loading
            ? <div className="space-y-2 mt-1">{[...Array(4)].map((_, i) => <div key={i} className="h-6 bg-gray-100 rounded animate-pulse" />)}</div>
            : (
              <table className="w-full text-[10px] mt-1">
                <thead>
                  <tr className="border-b border-gray-100">
                    {["User", "Role", "Joined", "Status"].map((h, i) => (
                      <th key={h} className={`py-1.5 pr-3 text-gray-400 font-semibold uppercase tracking-wide ${i === 3 ? "text-right" : "text-left"}`}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {recentUsers.map((u) => (
                    <tr key={u.id} className="hover:bg-amber-50 transition-colors">
                      <td className="py-1.5 pr-3">
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded-full bg-amber-100 flex items-center justify-center flex-shrink-0">
                            <span className="text-amber-700 font-black text-[9px]">{(u.firstName?.[0] || u.username?.[0] || "?").toUpperCase()}</span>
                          </div>
                          <span className="font-medium text-gray-700 truncate max-w-[120px]">
                            {u.firstName && u.lastName ? `${u.firstName} ${u.lastName}` : u.username || "Unknown"}
                          </span>
                        </div>
                      </td>
                      <td className="py-1.5 pr-3">
                        <StatusBadge label={u.roles?.[0] || "—"} color="#f59e0b" />
                      </td>
                      <td className="py-1.5 pr-3 text-gray-400">
                        {u.createdAt ? new Date(u.createdAt).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "2-digit" }) : "—"}
                      </td>
                      <td className="py-1.5 text-right">
                        <span className={`inline-flex items-center gap-1 text-[10px] font-medium ${u.isActive ? "text-emerald-600" : "text-gray-400"}`}>
                          <Circle size={5} className={u.isActive ? "fill-emerald-500" : "fill-gray-400"} />
                          {u.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
        </Panel>

        <p className="text-center text-[10px] text-gray-400 pb-1">
          Powered by <span className="font-semibold text-gray-500">Qalibrated Systems</span> — Qalitrack v0.0.1
        </p>
      </div>
    </div>
  );
}
