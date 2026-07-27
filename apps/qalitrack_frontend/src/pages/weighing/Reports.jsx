import { useEffect, useMemo, useState } from "react";
import ReportsTable from "./ReportsTable";
import DriverReport from "./reportFiles/DriverReport";
import ReweighedTransactionsReport from "./reportFiles/ReweighedTransactionsReport";
import CustomerReport from "./reportFiles/CustomerReport";
import CommodityReport from "./reportFiles/CommodityReport";
import SupplierReport from "./reportFiles/SupplierReport";

// NEW ADVANCED REPORT COMPONENTS
import ReportAnalytics from "./ReportAnalytics";
import CustomReportBuilder from "./CustomReportBuilder";
// import ReportScheduler from "./ReportScheduler"; // TODO: backend not implemented yet
import CrossEntityComparison from "./CrossEntityComparison";

import { getTransactions } from "../../api/Transaction/Transaction.js";
import {
  RotateCcw, FileDown, FileSpreadsheet, Filter, X, FileText,
  BarChart3, Settings, Calendar, GitCompare,
  Receipt, User, Users, Package, Building2, Check, Hourglass
} from "lucide-react";
import dayjs from "dayjs";

import jsPDF from "jspdf";
import * as XLSX from "xlsx";
import logoSrc from "../../assets/logo.jpeg";
import { getTicketSettings, resolveReportColors } from "../../utils/ticketThemeConfig";
import PageHeader from "../../components/PageHeader.jsx";

export default function Reports() {
  // Reports.jsx fetches its own data independently instead of reading/writing
  // the shared `weighing.transactions` Redux slice — that slice is also used
  // by the Dashboard/Analytics/Transactions pages, and the Reweighed
  // Transactions tab below needs the true, always-unfiltered transaction set
  // regardless of whatever date/status filter is currently applied here, so
  // sharing state with pages that filter/replace that slice risked corrupting
  // it for everyone else.
  const [allTransactions, setAllTransactions] = useState([]);
  const [scopedTransactions, setScopedTransactions] = useState(null); // non-null once a server-side date/status filter is active
  const [loading, setLoading] = useState(true);

  const REPORT_TABS = [
    { id: "transactions", label: "Transactions", icon: <Receipt size={14} /> },
    { id: "reweighed", label: "Reweighed Transactions", icon: <RotateCcw size={14} /> },
    { id: "drivers", label: "Drivers", icon: <User size={14} /> },
    { id: "customers", label: "Customers", icon: <Users size={14} /> },
    { id: "commodities", label: "Commodities", icon: <Package size={14} /> },
    { id: "suppliers", label: "Suppliers", icon: <Building2 size={14} /> },
    { id: "report-analytics", label: "Report Analytics", icon: <BarChart3 size={14} /> },
    { id: "comparison", label: "Comparison", icon: <GitCompare size={14} /> },
    { id: "custom", label: "Custom Builder", icon: <Settings size={14} /> },
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

  const extractItems = (res) => res?.data?.items || res?.items || (Array.isArray(res?.data) ? res.data : []) || [];

  // The true, always-unfiltered set — fetched once per page visit (this page
  // has no 30-second poller, unlike Dashboard/Analytics, so a single bulk
  // load here isn't a growing/recurring cost). Powers the Reweighed
  // Transactions tab and is the fallback source everywhere else when no
  // date/status filter is active.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const res = await getTransactions({ pageSize: 10000 });
        if (!cancelled) setAllTransactions(extractItems(res));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  // When the user picks a date range or status, ask the server for just that
  // slice instead of re-filtering the already-fetched 10,000 rows in JS —
  // this is the part that previously always operated on the full pull.
  useEffect(() => {
    const hasServerFilter = filters.startDate || filters.endDate || filters.status;
    if (!hasServerFilter) {
      setScopedTransactions(null);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const params = { pageSize: 10000 };
        if (filters.startDate) params.startDate = `${filters.startDate}T${filters.startTime || "00:00"}`;
        if (filters.endDate) params.endDate = `${filters.endDate}T${filters.endTime || "23:59"}`;
        if (filters.status.toLowerCase() === "completed") params.isCompleted = true;
        if (filters.status.toLowerCase() === "in progress") params.isCompleted = false;
        const res = await getTransactions(params);
        if (!cancelled) setScopedTransactions(extractItems(res));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [filters.startDate, filters.endDate, filters.startTime, filters.endTime, filters.status]);

  // Server-scoped when a date/status filter is active, else the full set —
  // date/status filtering itself now happens server-side; only free-text
  // search (which the backend doesn't support as a single multi-field OR) is
  // still applied client-side below.
  const transactions = scopedTransactions ?? allTransactions;

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

    // Date range and status are now applied server-side (see the fetch
    // effects above) — `transactions` already reflects them when active.
    // Only free-text search still needs a client-side pass, since the
    // backend doesn't support a single multi-field OR search.
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
  const exportPDF = async () => {
    const settings     = getTicketSettings();
    const companyName  = settings.companyName    || "QALIBRATED SYSTEMS LTD";
    const companyAddr  = settings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996";

    const doc = new jsPDF("landscape", "mm", "a4");
    const PW  = doc.internal.pageSize.getWidth();   // 297
    const L   = 14;
    const R   = PW - 14;
    const TW  = R - L;

    // ── Palette ───────────────────────────────────────────────────────────
    const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings);
    const black      = [0,   0,   0];
    const gray       = [107, 114, 128];
    const borderCol  = [229, 231, 235];
    const green      = [21,  128, 61];

    // ── Pre-load logo — scaled to fit, never cropped ───────────────────────
    // Previously clipped to a circle via ctx.arc()+ctx.clip(), which cut off
    // any part of the (non-circular) logo that fell outside that circle.
    let circularLogo = null;
    try {
      const img = await new Promise((resolve, reject) => {
        const i = new Image();
        i.onload = () => resolve(i);
        i.onerror = reject;
        i.src = settings.companyLogo || logoSrc;
      });
      const sz  = 200;
      const pad = sz * 0.06;
      const cv = document.createElement("canvas");
      cv.width = sz; cv.height = sz;
      const ctx = cv.getContext("2d");
      const avail  = sz - pad * 2;
      const aspect = img.naturalWidth / img.naturalHeight;
      const drawW  = aspect >= 1 ? avail : avail * aspect;
      const drawH  = aspect >= 1 ? avail / aspect : avail;
      ctx.drawImage(img, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH);
      const imgData = ctx.getImageData(0, 0, sz, sz);
      const px = imgData.data;
      for (let p = 0; p < px.length; p += 4) {
        if (px[p] > 240 && px[p + 1] > 240 && px[p + 2] > 240) px[p + 3] = 0;
      }
      ctx.putImageData(imgData, 0, 0);
      circularLogo = cv.toDataURL("image/png");
    } catch (_) { /* logo unavailable */ }

    // ── HEADER ────────────────────────────────────────────────────────────
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17);

    doc.setFontSize(14);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...black);
    doc.text(companyName, PW / 2, 11, { align: "center" });

    doc.setFontSize(7.5);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(companyAddr, PW / 2, 16, { align: "center" });

    // Report badge (right)
    const badgeW = 52;
    doc.setFillColor(...accent);
    doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, "F");
    doc.setFontSize(8);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...accentHeaderText);
    doc.text("TRANSACTIONS REPORT", R - badgeW / 2, 9.5, { align: "center" });

    doc.setFontSize(7);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(`Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`, R, 16, { align: "right" });

    // Amber divider
    doc.setDrawColor(...accent);
    doc.setLineWidth(0.8);
    doc.line(L, 23, R, 23);

    // ── SUMMARY STATS ─────────────────────────────────────────────────────
    let y = 27;
    const statW = (TW - 8) / 3;
    const stats = [
      { label: "TOTAL RECORDS",    value: `${filteredTransactions.length}` },
      { label: "TOTAL NET WEIGHT", value: `${totals.net.toLocaleString()} kg` },
      { label: "REPORT DATE",      value: dayjs().format("DD MMM YYYY") },
    ];

    stats.forEach((s, i) => {
      const bx = L + i * (statW + 4);
      doc.setFillColor(...accentLight);
      doc.setDrawColor(...accentDark);
      doc.setLineWidth(0.3);
      doc.roundedRect(bx, y, statW, 10, 2, 2, "FD");
      doc.setFontSize(6.5);
      doc.setFont("helvetica", "normal");
      doc.setTextColor(...gray);
      doc.text(s.label, bx + statW / 2, y + 3.8, { align: "center" });
      doc.setFontSize(9);
      doc.setFont("helvetica", "bold");
      doc.setTextColor(...black);
      doc.text(s.value, bx + statW / 2, y + 8.2, { align: "center" });
    });

    y += 14;

    // ── DATA — one record card per transaction, every field, never truncated ──
    // A flat table with all ~17 non-identity fields (269mm / 17 ≈ 16mm per
    // column) still forces truncation on names/dates no matter how the widths
    // are tuned. Each transaction gets its own bordered card with a 3-column
    // label:value grid instead — every field gets ~64mm of value space, more
    // than enough for anything in this data set. Trade-off: ~3-4 cards fit per
    // landscape page instead of ~20 table rows, so this report runs longer.
    const PH      = doc.internal.pageSize.getHeight();
    const colW    = TW / 3;
    const rowH    = 5.2;
    const headerH = 7;
    const bodyRows = 5; // 15 label:value slots (14 fields + 1 spare)
    const cardH   = headerH + bodyRows * rowH + 8; // +8 for the weight row band
    const cardGap = 3;
    const bottomReserve = 12; // keep clear of the page edge; footer only draws once, after the last card

    const drawContinuationHeader = () => {
      doc.setFontSize(9);
      doc.setFont("helvetica", "bold");
      doc.setTextColor(...black);
      doc.text(companyName, L, 10);
      doc.setFontSize(7.5);
      doc.setFont("helvetica", "normal");
      doc.setTextColor(...gray);
      doc.text("Transactions Report (continued)", R, 10, { align: "right" });
      doc.setDrawColor(...accent);
      doc.setLineWidth(0.5);
      doc.line(L, 13, R, 13);
      return 18;
    };

    const drawField = (label, value, cx, cy, colWidth) => {
      if (!label) return;
      doc.setFontSize(7.5);
      doc.setFont("helvetica", "bold");
      doc.setTextColor(...gray);
      doc.text(`${label}:`, cx, cy);
      const labelW = doc.getTextWidth(`${label}: `);
      doc.setFont("helvetica", "normal");
      doc.setTextColor(...black);
      const maxValW = colWidth - labelW - 4;
      const valText = doc.splitTextToSize(String(value ?? "-") || "-", maxValW)[0];
      doc.text(valText, cx + labelW, cy);
    };

    const drawTransactionCard = (t, idx, yStart) => {
      const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
      const isCompleted =
        hasSecondWeight || t.status === "Completed" || t.status === "completed";
      const isOutbound = t.operation === "Outbound Product Dispatch";
      const party = isOutbound ? t.customerName : t.supplierName;
      const partyLabel = isOutbound ? "Customer" : "Supplier";

      doc.setFillColor(255, 255, 255);
      doc.setDrawColor(...accentDark);
      doc.setLineWidth(0.3);
      doc.rect(L, yStart, TW, cardH, "FD");

      // Header band: # / receipt / date+time / status badge
      doc.setFillColor(...accent);
      doc.rect(L, yStart, TW, headerH, "F");
      doc.setFontSize(8);
      doc.setFont("helvetica", "bold");
      doc.setTextColor(...accentHeaderText);
      doc.text(`#${idx + 1}`, L + 3, yStart + headerH / 2 + 1.3);
      doc.text(t.receiptNo || "-", L + 16, yStart + headerH / 2 + 1.3);
      doc.text(dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"), L + TW / 2, yStart + headerH / 2 + 1.3, { align: "center" });

      const badgeW = 30;
      doc.setFillColor(...(isCompleted ? green : accentDark));
      doc.roundedRect(R - badgeW - 2, yStart + 1.2, badgeW, headerH - 2.4, 1.2, 1.2, "F");
      doc.setFontSize(7);
      doc.setTextColor(255, 255, 255);
      doc.text(isCompleted ? "COMPLETED" : "IN PROGRESS", R - badgeW / 2 - 2, yStart + headerH / 2 + 1, { align: "center" });

      // Body grid — every field the original report had, none dropped
      const fields = [
        ["Vehicle", t.noPlate], ["Driver", t.driverName], ["Commodity", t.commodityName],
        ["Transporter", t.transporterName], [partyLabel, party], ["Origin", t.originName],
        ["Destination", t.destinationName], ["Weighbridge", t.weighBridgeName], ["Scale", t.scaleName],
        ["Mode", t.weighMode], ["Operation", t.operation], ["Operator", t.operatorName || t.firstWeightOperator],
        ["TAT", calculateTurnaroundTime(t.firstWeightDate, t.secondWeightDate, t.turnaroundTime)], [null, null], [null, null],
      ];
      let fy = yStart + headerH + 3.8;
      for (let r = 0; r < bodyRows; r++) {
        for (let c = 0; c < 3; c++) {
          const [label, value] = fields[r * 3 + c];
          drawField(label, value, L + c * colW + 2.5, fy, colW);
        }
        fy += rowH;
      }

      // Weight row — visually separated + bolded, mirrors the summary table
      const wy = yStart + headerH + bodyRows * rowH + 5.5;
      doc.setDrawColor(...borderCol);
      doc.setLineWidth(0.2);
      doc.line(L + 2, wy - 3.6, R - 2, wy - 3.6);
      const weightFields = [
        ["Gross Wt", t.firstWeight  ? `${parseFloat(t.firstWeight).toLocaleString()} kg`  : "-"],
        ["Second Wt", t.secondWeight ? `${parseFloat(t.secondWeight).toLocaleString()} kg` : "-"],
        ["Net Wt", t.netWeight    ? `${parseFloat(t.netWeight).toLocaleString()} kg`    : "-"],
      ];
      weightFields.forEach(([label, value], c) => {
        const cx = L + c * colW + 2.5;
        doc.setFontSize(7.5);
        doc.setFont("helvetica", "bold");
        doc.setTextColor(...gray);
        doc.text(`${label}:`, cx, wy);
        const labelW = doc.getTextWidth(`${label}: `);
        doc.setFontSize(9);
        doc.setTextColor(...(c === 2 ? accentDark : black));
        doc.text(value, cx + labelW, wy);
      });

      return yStart + cardH;
    };

    filteredTransactions.forEach((t, idx) => {
      if (y + cardH > PH - bottomReserve) {
        doc.addPage();
        y = drawContinuationHeader();
      }
      y = drawTransactionCard(t, idx, y) + cardGap;
    });

    // ── FOOTER ────────────────────────────────────────────────────────────
    if (y + 2 + 10 > PH - 5) {
      doc.addPage();
      y = drawContinuationHeader();
    }
    const footerY = y + 2;
    doc.setFillColor(...accentLight);
    doc.setDrawColor(...accentDark);
    doc.setLineWidth(0.3);
    doc.roundedRect(L, footerY, TW, 10, 2, 2, "FD");

    if (circularLogo) doc.addImage(circularLogo, "PNG", L + 2, footerY + 1, 8, 8);

    doc.setFontSize(7.5);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...black);
    doc.text("Powered by Qalibrated Systems  |  www.qalibrated.co.ke", PW / 2, footerY + 5, { align: "center" });
    doc.setFontSize(6.5);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text("Inventing and Making Happen", PW / 2, footerY + 8.5, { align: "center" });

    // Watermark on all pages
    if (circularLogo) {
      try {
        const wmSize = 90;
        const wmCanvas = document.createElement("canvas");
        wmCanvas.width = 200; wmCanvas.height = 200;
        const wmCtx = wmCanvas.getContext("2d");
        const wmImg = await new Promise((resolve, reject) => {
          const i = new Image(); i.onload = () => resolve(i); i.onerror = reject;
          i.src = circularLogo;
        });
        wmCtx.globalAlpha = 0.07;
        wmCtx.drawImage(wmImg, 0, 0, 200, 200);
        const wmData = wmCanvas.toDataURL("image/png");
        const totalPages = doc.internal.getNumberOfPages();
        for (let p = 1; p <= totalPages; p++) {
          doc.setPage(p);
          doc.addImage(wmData, "PNG", PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
        }
      } catch (_) {}
    }

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

    // REWEIGHED TRANSACTIONS REPORT — pass the true unfiltered set so the
    // date/status filters above (now applied server-side) can't accidentally
    // exclude ReweighRequested records
    if (activeTab === "reweighed") {
      return <ReweighedTransactionsReport transactions={allTransactions} loading={loading} />;
    }

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
          <div className="bg-white border border-amber-200 rounded-lg p-3 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-gray-600">
              Total Transactions
            </div>
            <div className="text-xl font-bold mt-1 text-gray-900">
              {totals.count.toLocaleString()}
            </div>
          </div>
          <div className="bg-white border border-amber-200 rounded-lg p-3 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-gray-600">
              Total Net Weight
            </div>
            <div className="text-xl font-bold mt-1 text-gray-900">
              {totals.net.toLocaleString()}
            </div>
            <div className="text-[10px] text-gray-500">kg</div>
          </div>
          <div className="bg-white border border-amber-200 rounded-lg p-3 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-gray-600">
              First Weight Total
            </div>
            <div className="text-xl font-bold mt-1 text-gray-900">
              {totals.first.toLocaleString()}
            </div>
            <div className="text-[10px] text-gray-500">kg</div>
          </div>
          <div className="bg-white border border-amber-200 rounded-lg p-3 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-gray-600">
              Second Weight Total
            </div>
            <div className="text-xl font-bold mt-1 text-gray-900">
              {totals.second.toLocaleString()}
            </div>
            <div className="text-[10px] text-gray-500">kg</div>
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

  // Don't show filters for advanced tabs or reweighed (has its own filter UI)
  const showFiltersPanel = [
    "transactions", "drivers", "customers", "commodities", "suppliers"
  ].includes(activeTab);

  return (
    <div className="h-full bg-gradient-to-br from-gray-50 to-gray-100 overflow-hidden flex flex-col">
      <PageHeader icon={FileText} title="REPORTS" subtitle="Operational and analytical system reports" />

      {/* TABS */}
      <div className="flex items-center gap-5 mb-4 flex-wrap px-4 sm:px-6 border-b border-gray-200 shrink-0">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`flex items-center gap-1.5 pb-2 -mb-px text-sm font-medium capitalize border-b-2 transition-colors ${
              activeTab === tab.id
                ? "border-amber-500 text-amber-600"
                : "border-transparent text-gray-500 hover:text-gray-700"
            }`}
          >
            {tab.icon}
            {tab.label}
            {tab.badge && (
              <span className="px-1.5 py-0.5 bg-green-500 text-white text-[9px] font-bold rounded-full leading-none">
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
          <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50 border-b border-amber-200 flex items-center justify-between rounded-t-lg">
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

              {/* Status filter — always visible */}
              <select
                value={filters.status}
                onChange={(e) => setFilters({ ...filters, status: e.target.value })}
                className="h-7 px-2 border border-gray-300 rounded-md text-[11px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300 bg-white"
              >
                <option value="">All Status</option>
                <option value="Completed">Completed</option>
                <option value="In Progress">In Progress</option>
              </select>

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
                    ? "bg-amber-500 text-white border-amber-500 shadow-sm"
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
            <div className="px-3 py-3 bg-gradient-to-br from-gray-50 via-amber-50/30 to-amber-50/20">
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
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
            <div className="p-4 border-b bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50">
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
                          key={t.ticketID || t.id}
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
                          <td className="p-2 text-right font-bold text-amber-600">
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
                              className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold border ${
                                isCompleted
                                  ? "bg-green-100 text-green-800 border-green-300"
                                  : "bg-amber-50 text-amber-800 border-amber-200"
                              }`}
                            >
                              {isCompleted
                                ? <><Check className="w-3 h-3" strokeWidth={3} /> COMPLETED</>
                                : <><Hourglass className="w-3 h-3" strokeWidth={2.5} /> IN PROGRESS</>}
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
            <div className="p-4 border-t bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50 flex justify-end gap-2">
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
                className="px-4 py-2 bg-gradient-to-r from-amber-500 to-amber-600 text-white rounded-lg text-xs font-semibold hover:from-amber-600 hover:to-amber-700 transition-all shadow-sm"
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