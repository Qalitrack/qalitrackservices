import { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import DriverReport from "./reportFiles/DriverReport";
import CustomerReport from "./reportFiles/CustomerReport";
import CommodityReport from "./reportFiles/CommodityReport";
import SupplierReport from "./reportFiles/SupplierReport";

// NEW ADVANCED REPORT COMPONENTS
import ReportAnalytics from "./ReportAnalytics";
import CustomReportBuilder from "./CustomReportBuilder";
// import ReportScheduler from "./ReportScheduler"; // TODO: backend not implemented yet
import CrossEntityComparison from "./CrossEntityComparison";

import { fetchTransactions } from "../../store/weighingSlice";
import {
  RotateCcw, FileDown, FileSpreadsheet, Filter, X,
  BarChart3, Settings, Calendar, GitCompare
} from "lucide-react";
import dayjs from "dayjs";

import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";

export default function Reports() {
  const dispatch = useDispatch();
  const { transactions, loading } = useSelector((state) => state.weighing);

  const REPORT_TABS = [
    { id: "transactions", label: "Transactions", icon: null },
    { id: "drivers", label: "Drivers", icon: null },
    { id: "customers", label: "Customers", icon: null },
    { id: "commodities", label: "Commodities", icon: null },
    { id: "suppliers", label: "Suppliers", icon: null },
    { id: "report-analytics", label: "Report Analytics", icon: <BarChart3 size={14} />, badge: "NEW" },
    { id: "comparison", label: "Comparison", icon: <GitCompare size={14} />, badge: "NEW" },
    { id: "custom", label: "Custom Builder", icon: <Settings size={14} />, badge: "NEW" },
    // { id: "scheduler", label: "Scheduler", icon: <Calendar size={14} />, badge: "NEW" }, // TODO: backend not implemented yet
  ];

  const [activeTab, setActiveTab] = useState("transactions");

  // Pagination (TABLE ONLY) — default 6 per page
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(6);

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: "",
    search: "",
  });

  // Filter panel visibility
  const [showFilters, setShowFilters] = useState(false);

  // Export preview
  const [showExportPreview, setShowExportPreview] = useState(false);
  const [exportType, setExportType] = useState(null);

  useEffect(() => {
    dispatch(fetchTransactions({ pageSize: 10000 }));
  }, [dispatch]);

  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // =========================
  // Turnaround Calculation
  // =========================
  const calculateTurnaroundTime = (firstWeightDate, secondWeightDate, turnaroundTime) => {
    if (turnaroundTime) {
      const parts = turnaroundTime.split(":");
      if (parts.length >= 2) {
        const h = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10);
        const total = h * 60 + m;
        if (total < 1) return "< 1m";
        return h > 0 ? (m ? `${h}h ${m}m` : `${h}h`) : `${m}m`;
      }
    }
    if (!firstWeightDate || !secondWeightDate) return "N/A";
    const diffMinutes = dayjs(secondWeightDate).diff(dayjs(firstWeightDate), "minute");
    if (diffMinutes < 1) return "< 1m";
    if (diffMinutes < 60) return `${diffMinutes}m`;
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  };

  // =========================
  // FILTERED DATA
  // =========================
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      const start = new Date(`${filters.startDate}T${filters.startTime || "00:00"}`);
      data = data.filter((t) => new Date(t.createdAt) >= start);
    }
    if (filters.endDate) {
      const end = new Date(`${filters.endDate}T${filters.endTime || "23:59"}`);
      data = data.filter((t) => new Date(t.createdAt) <= end);
    }
    if (filters.status) {
      if (filters.status.toLowerCase() === "completed") {
        data = data.filter(
          (t) =>
            (t.secondWeight && parseFloat(t.secondWeight) > 0) ||
            t.status === "Completed" ||
            t.status === "completed"
        );
      } else if (filters.status.toLowerCase() === "in progress") {
        data = data.filter(
          (t) =>
            (!t.secondWeight || parseFloat(t.secondWeight) === 0) &&
            t.status !== "Completed" &&
            t.status !== "completed"
        );
      }
    }
    if (filters.search) {
      const q = filters.search.toLowerCase();
      data = data.filter(
        (t) =>
          t.receiptNo?.toLowerCase().includes(q) ||
          t.noPlate?.toLowerCase().includes(q) ||
          t.driverName?.toLowerCase().includes(q) ||
          t.commodityName?.toLowerCase().includes(q) ||
          t.supplierName?.toLowerCase().includes(q) ||
          t.transporterName?.toLowerCase().includes(q) ||
          t.customerName?.toLowerCase().includes(q)
      );
    }
    return data;
  }, [transactions, filters]);

  const totalRecords = filteredTransactions.length;

  const totals = useMemo(() => {
    return filteredTransactions.reduce(
      (acc, t) => {
        acc.count += 1;
        acc.net += Number(t.netWeight || 0);
        acc.first += Number(t.firstWeight || 0);
        acc.second += Number(t.secondWeight || 0);
        return acc;
      },
      { count: 0, net: 0, first: 0, second: 0 }
    );
  }, [filteredTransactions]);

  // Count active filters
  const activeFilterCount = Object.entries(filters).filter(
    ([key, value]) => value && !["page", "pageSize"].includes(key)
  ).length;

  // =========================
  // EXPORTS
  // =========================
  const exportPDF = () => {
    const doc = new jsPDF("landscape", "mm", "a4");
    const pageWidth = doc.internal.pageSize.getWidth();

    doc.setFillColor(245, 158, 11);
    doc.rect(0, 0, pageWidth, 20, "F");
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(16);
    doc.setFont("helvetica", "bold");
    doc.text("TRANSACTIONS REPORT", pageWidth / 2, 10, { align: "center" });
    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    doc.text(
      `Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`,
      pageWidth / 2,
      15,
      { align: "center" }
    );

    doc.setTextColor(0, 0, 0);
    doc.setFontSize(9);
    doc.text(`Total Records: ${filteredTransactions.length}`, 14, 25);
    doc.text(`Total Net Weight: ${totals.net.toLocaleString()} kg`, 14, 30);

    autoTable(doc, {
      startY: 35,
      head: [[
        "#", "Date & Time", "Receipt", "Vehicle", "Driver", "Commodity",
        "Supplier", "Transporter", "Customer", "Origin", "Destination",
        "Weighbridge", "Scale", "Mode", "Operation", "Operator",
        "First Wt (kg)", "Second Wt (kg)", "Net Wt (kg)", "TAT", "Status",
      ]],
      body: filteredTransactions.map((t, idx) => {
        const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
        const isCompleted =
          hasSecondWeight || t.status === "Completed" || t.status === "completed";
        return [
          idx + 1,
          dayjs(t.createdAt).format("DD MMM YY HH:mm"),
          t.receiptNo || "-",
          t.noPlate || "-",
          t.driverName || "-",
          t.commodityName || "-",
          t.supplierName || "-",
          t.transporterName || "-",
          t.customerName || "-",
          t.originName || "-",
          t.destinationName || "-",
          t.weighBridgeName || "-",
          t.scaleName || "-",
          t.weighMode || "-",
          t.operation || "-",
          t.operatorName || t.firstWeightOperator || "-",
          t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : "-",
          t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : "-",
          t.netWeight ? parseFloat(t.netWeight).toLocaleString() : "-",
          calculateTurnaroundTime(t.firstWeightDate, t.secondWeightDate, t.turnaroundTime),
          isCompleted ? "COMPLETED" : "IN PROGRESS",
        ];
      }),
      styles: { fontSize: 7, cellPadding: 1.5 },
      headStyles: {
        fillColor: [245, 158, 11],
        textColor: [0, 0, 0],
        fontStyle: "bold",
        fontSize: 7,
      },
      alternateRowStyles: { fillColor: [250, 250, 250] },
      columnStyles: {
        16: { halign: "right" },
        17: { halign: "right" },
        18: { halign: "right" },
      },
    });

    doc.save(`transaction-report-${dayjs().format("YYYY-MM-DD")}.pdf`);
  };

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map((t, idx) => {
        const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
        const isCompleted =
          hasSecondWeight || t.status === "Completed" || t.status === "completed";
        return {
          "#": idx + 1,
          Date: dayjs(t.createdAt).format("DD MMM YYYY"),
          Time: dayjs(t.createdAt).format("HH:mm:ss"),
          "Receipt No": t.receiptNo || "-",
          "Vehicle Registration": t.noPlate || "-",
          "Driver Name": t.driverName || "-",
          Commodity: t.commodityName || "-",
          Supplier: t.supplierName || "-",
          Transporter: t.transporterName || "-",
          Customer: t.customerName || "-",
          Origin: t.originName || "-",
          Destination: t.destinationName || "-",
          Weighbridge: t.weighBridgeName || "-",
          Scale: t.scaleName || "-",
          "Weigh Mode": t.weighMode || "-",
          Operation: t.operation || "-",
          Operator: t.operatorName || t.firstWeightOperator || "-",
          "Axle Type": t.axleType || "-",
          "Container No": t.containerNo || "-",
          "Seal No": t.sealNo || "-",
          "First Weight (kg)": t.firstWeight ? parseFloat(t.firstWeight) : 0,
          "Second Weight (kg)": t.secondWeight ? parseFloat(t.secondWeight) : 0,
          "Net Weight (kg)": t.netWeight ? parseFloat(t.netWeight) : 0,
          "First Weight Time": t.firstWeightDate
            ? dayjs(t.firstWeightDate).format("DD MMM YYYY HH:mm:ss")
            : "-",
          "Second Weight Time": t.secondWeightDate
            ? dayjs(t.secondWeightDate).format("DD MMM YYYY HH:mm:ss")
            : "-",
          "Turnaround Time": calculateTurnaroundTime(
            t.firstWeightDate,
            t.secondWeightDate,
            t.turnaroundTime
          ),
          Status: isCompleted ? "COMPLETED" : "IN PROGRESS",
          Remarks: t.remarks || t.notes || "-",
        };
      })
    );

    const colWidths = Object.keys(filteredTransactions[0] || {}).map(() => ({
      wch: 15,
    }));
    ws["!cols"] = colWidths;

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(wb, `transaction-report-${dayjs().format("YYYY-MM-DD")}.xlsx`);
  };

  const clearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      startTime: "",
      endTime: "",
      status: "",
      search: "",
    });
    setCurrentPage(1);
  };

  const renderActiveReport = () => {
    // NEW ADVANCED REPORTS
    if (activeTab === "report-analytics") {
      return <ReportAnalytics transactions={filteredTransactions} />;
    }
    
    if (activeTab === "comparison") {
      return <CrossEntityComparison transactions={filteredTransactions} />;
    }
    
    if (activeTab === "custom") {
      return <CustomReportBuilder transactions={filteredTransactions} />;
    }
    
    // if (activeTab === "scheduler") { // TODO: backend not implemented yet
    //   return <ReportScheduler transactions={filteredTransactions} />;
    // }

    // EXISTING REPORTS
    if (activeTab !== "transactions") {
      const ComponentMap = {
        drivers: DriverReport,
        customers: CustomerReport,
        commodities: CommodityReport,
        suppliers: SupplierReport,
      };
      const Component = ComponentMap[activeTab];
      return <Component transactions={filteredTransactions} loading={loading} />;
    }

    return (
      <>
        {/* Summary Cards */}
        <div className="mb-4 grid grid-cols-2 sm:grid-cols-4 gap-3">
          <div className="bg-gradient-to-br from-amber-50 to-amber-100 rounded-lg p-3 border border-amber-200 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-800">
              Total Transactions
            </div>
            <div className="text-xl font-bold mt-1 text-amber-900">
              {totals.count.toLocaleString()}
            </div>
          </div>
          <div className="bg-gradient-to-br from-amber-100 to-orange-100 rounded-lg p-3 border border-amber-300 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-900">
              Total Net Weight
            </div>
            <div className="text-xl font-bold mt-1 text-amber-950">
              {totals.net.toLocaleString()}
            </div>
            <div className="text-[10px] text-amber-800">kg</div>
          </div>
          <div className="bg-gradient-to-br from-amber-50 to-amber-100 rounded-lg p-3 border border-amber-200 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-800">
              First Weight Total
            </div>
            <div className="text-xl font-bold mt-1 text-orange-600">
              {totals.first.toLocaleString()}
            </div>
            <div className="text-[10px] text-amber-700">kg</div>
          </div>
          <div className="bg-gradient-to-br from-amber-100 to-orange-100 rounded-lg p-3 border border-amber-300 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-900">
              Second Weight Total
            </div>
            <div className="text-xl font-bold mt-1 text-green-600">
              {totals.second.toLocaleString()}
            </div>
            <div className="text-[10px] text-amber-800">kg</div>
          </div>
        </div>

        <ReportsTable
          transactions={filteredTransactions}
          loading={loading}
          currentPage={currentPage}
          pageSize={pageSize}
          totalRecords={totalRecords}
          onPageChange={setCurrentPage}
          onPageSizeChange={(size) => { setPageSize(size); setCurrentPage(1); }}
        />
      </>
    );
  };

  // Don't show filters for advanced tabs
  const showFiltersPanel = [
    "transactions", "drivers", "customers", "commodities", "suppliers"
  ].includes(activeTab);

  return (
    <div className="h-screen bg-gradient-to-br from-gray-50 to-gray-100 overflow-hidden flex flex-col">
      {/* Header */}
      <div className="mb-4 px-4 sm:px-6 pt-4 sm:pt-6 shrink-0">
        <div className="flex items-center gap-2 mb-2">
          <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
            <svg
              className="w-6 h-6 text-white"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">REPORTS</h1>
            <p className="text-xs text-amber-700 font-medium">
              Operational and analytical system reports
            </p>
          </div>
        </div>
      </div>

      {/* TABS */}
      <div className="flex gap-2 mb-4 flex-wrap px-4 sm:px-6 shrink-0">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`relative px-4 py-2 rounded-lg text-sm font-medium capitalize transition-all flex items-center gap-1.5 ${
              activeTab === tab.id
                ? "bg-gradient-to-r from-amber-500 to-orange-600 text-white border border-amber-500 shadow-sm"
                : "bg-white text-gray-700 hover:bg-amber-50 border border-gray-200"
            }`}
          >
            {tab.icon}
            {tab.label}
            {tab.badge && (
              <span className="absolute -top-1 -right-1 px-1.5 py-0.5 bg-green-500 text-white text-[9px] font-bold rounded-full">
                {tab.badge}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* MAIN CONTENT AREA - SCROLLABLE */}
      <div className="flex-1 overflow-y-auto px-4 sm:px-6 pb-6">
        {/* FILTERS — only for basic report tabs */}
        {showFiltersPanel && (
        <div className="bg-white border border-amber-200 rounded-lg shadow-sm mb-4">
          {/* Filter bar header */}
          <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 flex items-center justify-between rounded-t-lg">
            <div className="flex items-center gap-2">
              {/* Search — always visible */}
              <div className="relative">
                <svg
                  className="absolute left-2 top-1/2 -translate-y-1/2 w-3 h-3 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z"
                  />
                </svg>
                <input
                  type="text"
                  placeholder="Search..."
                  value={filters.search}
                  onChange={(e) =>
                    setFilters({ ...filters, search: e.target.value })
                  }
                  className="pl-7 pr-3 h-7 w-52 border border-gray-300 rounded-md text-[11px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
                />
              </div>

              {activeFilterCount > 0 && (
                <span className="text-[9px] text-amber-900 font-bold bg-gradient-to-r from-amber-100 to-amber-200 px-2 py-0.5 rounded-full border border-amber-300 shadow-sm">
                  🎯 {activeFilterCount} active
                </span>
              )}
            </div>

            <div className="flex items-center gap-2">
              {activeFilterCount > 0 && (
                <button
                  onClick={clearFilters}
                  className="flex items-center gap-1 h-7 px-2.5 border border-red-300 rounded-md text-[10px] font-semibold text-red-700 bg-red-50 hover:bg-red-100 transition-colors shadow-sm"
                >
                  <X size={11} />
                  Clear
                </button>
              )}

              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center gap-1.5 h-7 px-3 rounded-md text-[11px] font-medium border transition-all ${
                  showFilters
                    ? "bg-gradient-to-r from-amber-500 to-orange-600 text-white border-amber-500 shadow-sm"
                    : "border-gray-300 text-gray-700 hover:border-amber-400 hover:text-amber-700"
                }`}
              >
                <Filter size={13} />
                Filters
              </button>

              {activeTab === "transactions" && (
                <div className="flex gap-1.5 ml-1">
                  <button
                    onClick={() => {
                      setExportType("pdf");
                      setShowExportPreview(true);
                    }}
                    className="flex items-center gap-1.5 h-7 px-3 bg-amber-100 text-amber-900 border border-amber-300 rounded-md text-[10px] font-medium hover:bg-amber-200 transition-all"
                  >
                    <FileDown size={13} />
                    PDF
                  </button>
                  <button
                    onClick={() => {
                      setExportType("excel");
                      setShowExportPreview(true);
                    }}
                    className="flex items-center gap-1.5 h-7 px-3 border border-gray-300 bg-white rounded-md text-[10px] font-medium hover:bg-gray-50 transition-colors"
                  >
                    <FileSpreadsheet size={13} />
                    Excel
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Collapsible filter panel */}
          {showFilters && (
            <div className="px-3 py-3 bg-gradient-to-br from-gray-50 via-amber-50/30 to-orange-50/20">
              <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                {/* Start Date */}
                <div className="space-y-1">
                  <label className="flex items-center gap-1 text-[9px] font-semibold text-gray-700 uppercase tracking-wider">
                    <span className="w-1 h-1 bg-amber-500 rounded-full" />
                    Start Date
                  </label>
                  <input
                    type="date"
                    value={filters.startDate}
                    onChange={(e) =>
                      setFilters({ ...filters, startDate: e.target.value })
                    }
                    className="w-full border border-amber-300 px-2 py-1.5 rounded-md text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-400 h-6"
                  />
                </div>

                {/* Start Time */}
                <div className="space-y-1">
                  <label className="flex items-center gap-1 text-[9px] font-semibold text-gray-700 uppercase tracking-wider">
                    <span className="w-1 h-1 bg-amber-500 rounded-full" />
                    Start Time
                  </label>
                  <input
                    type="time"
                    value={filters.startTime}
                    onChange={(e) =>
                      setFilters({ ...filters, startTime: e.target.value })
                    }
                    className="w-full border border-amber-300 px-2 py-1.5 rounded-md text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-400 h-6"
                  />
                </div>

                {/* End Date */}
                <div className="space-y-1">
                  <label className="flex items-center gap-1 text-[9px] font-semibold text-gray-700 uppercase tracking-wider">
                    <span className="w-1 h-1 bg-amber-500 rounded-full" />
                    End Date
                  </label>
                  <input
                    type="date"
                    value={filters.endDate}
                    onChange={(e) =>
                      setFilters({ ...filters, endDate: e.target.value })
                    }
                    className="w-full border border-amber-300 px-2 py-1.5 rounded-md text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-400 h-6"
                  />
                </div>

                {/* End Time */}
                <div className="space-y-1">
                  <label className="flex items-center gap-1 text-[9px] font-semibold text-gray-700 uppercase tracking-wider">
                    <span className="w-1 h-1 bg-amber-500 rounded-full" />
                    End Time
                  </label>
                  <input
                    type="time"
                    value={filters.endTime}
                    onChange={(e) =>
                      setFilters({ ...filters, endTime: e.target.value })
                    }
                    className="w-full border border-amber-300 px-2 py-1.5 rounded-md text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-400 h-6"
                  />
                </div>

                {/* Status */}
                <div className="space-y-1">
                  <label className="flex items-center gap-1 text-[9px] font-semibold text-gray-700 uppercase tracking-wider">
                    <span className="w-1 h-1 bg-amber-500 rounded-full" />
                    Status
                  </label>
                  <select
                    value={filters.status}
                    onChange={(e) =>
                      setFilters({ ...filters, status: e.target.value })
                    }
                    className="w-full border border-amber-300 px-2 rounded-md text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-400 bg-white h-6"
                  >
                    <option value="">All Status</option>
                    <option value="Completed">Completed</option>
                    <option value="In Progress">In Progress</option>
                  </select>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {renderActiveReport()}

      {/* EXPORT PREVIEW MODAL - only for transactions */}
      {showExportPreview && activeTab === "transactions" && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg w-full max-w-7xl max-h-[90vh] flex flex-col shadow-2xl">
            {/* Modal Header */}
            <div className="p-4 border-b bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-900">
                    Export Preview
                  </h2>
                  <p className="text-xs text-gray-600 mt-1">
                    Showing{" "}
                    <span className="font-semibold text-amber-700">
                      {filteredTransactions.length}
                    </span>{" "}
                    filtered records
                  </p>
                </div>
                <button
                  onClick={() => setShowExportPreview(false)}
                  className="text-gray-400 hover:text-gray-600 transition-colors"
                >
                  <X size={20} />
                </button>
              </div>
            </div>

            {/* Modal Content */}
            <div className="flex-1 overflow-auto p-4">
              <div className="border rounded-lg overflow-auto">
                <table className="w-full text-xs min-w-[2000px]">
                  <thead className="bg-gradient-to-b from-amber-50 to-amber-100/50 sticky top-0">
                    <tr>
                      {[
                        "#", "Date", "Time", "Receipt", "Vehicle", "Driver",
                        "Commodity", "Supplier", "Transporter", "Customer",
                        "Origin", "Destination", "Weighbridge", "Operator",
                        "First Wt", "Second Wt", "Net Wt", "TAT", "Status",
                      ].map((h) => (
                        <th
                          key={h}
                          className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase tracking-wider border-b-2 border-amber-200"
                        >
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {filteredTransactions.map((t, idx) => {
                      const hasSecondWeight =
                        t.secondWeight && parseFloat(t.secondWeight) > 0;
                      const isCompleted =
                        hasSecondWeight ||
                        t.status === "Completed" ||
                        t.status === "completed";
                      return (
                        <tr
                          key={t.id}
                          className={`border-t border-gray-100 ${
                            isCompleted
                              ? "bg-green-50/40"
                              : idx % 2 === 0
                              ? "bg-white"
                              : "bg-gray-50/50"
                          }`}
                        >
                          <td className="p-2">
                            <span className="inline-flex items-center justify-center w-6 h-6 rounded-full bg-gradient-to-br from-amber-100 to-amber-200 text-[10px] font-extrabold text-amber-900 border border-amber-300 shadow-sm">
                              {idx + 1}
                            </span>
                          </td>
                          <td className="p-2">{dayjs(t.createdAt).format("DD MMM YYYY")}</td>
                          <td className="p-2">{dayjs(t.createdAt).format("HH:mm:ss")}</td>
                          <td className="p-2 font-mono font-bold text-amber-600 bg-amber-50 rounded px-1.5">
                            {t.receiptNo || "-"}
                          </td>
                          <td className="p-2">
                            <div className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[10px] font-bold">
                              {t.noPlate || "-"}
                            </div>
                          </td>
                          <td className="p-2 font-medium text-gray-700">{t.driverName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.commodityName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.supplierName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.transporterName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.customerName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.originName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.destinationName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.weighBridgeName || "-"}</td>
                          <td className="p-2 font-medium text-gray-700">
                            {t.operatorName || t.firstWeightOperator || "-"}
                          </td>
                          <td className="p-2 text-right font-bold text-orange-600">
                            {t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : "-"}
                          </td>
                          <td className="p-2 text-right font-bold text-green-600">
                            {t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : "-"}
                          </td>
                          <td className="p-2 text-right font-extrabold text-amber-600">
                            {t.netWeight ? parseFloat(t.netWeight).toLocaleString() : "-"}
                          </td>
                          <td className="p-2 font-semibold text-gray-700">
                            {calculateTurnaroundTime(t.firstWeightDate, t.secondWeightDate, t.turnaroundTime)}
                          </td>
                          <td className="p-2">
                            <span
                              className={`px-2 py-0.5 rounded text-[10px] font-bold border ${
                                isCompleted
                                  ? "bg-green-100 text-green-800 border-green-300"
                                  : "bg-amber-50 text-amber-800 border-amber-200"
                              }`}
                            >
                              {isCompleted ? "✓ COMPLETED" : "⏳ IN PROGRESS"}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="p-4 border-t bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 flex justify-end gap-2">
              <button
                onClick={() => setShowExportPreview(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  exportType === "pdf" ? exportPDF() : exportExcel();
                  setShowExportPreview(false);
                }}
                className="px-4 py-2 bg-gradient-to-r from-amber-500 to-orange-600 text-white rounded-lg text-xs font-semibold hover:from-amber-600 hover:to-orange-700 transition-all shadow-sm"
              >
                Download {exportType?.toUpperCase()}
              </button>
            </div>
          </div>
        </div>
      )}
      </div>
    </div>
  );
}