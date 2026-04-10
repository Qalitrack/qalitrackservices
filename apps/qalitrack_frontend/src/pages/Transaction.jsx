import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, message, Radio, Drawer, Modal } from "antd";
import {
  ReloadOutlined,
  EditOutlined,
  SaveOutlined,
  CloseOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import duration from "dayjs/plugin/duration";
import {
  fetchTransactions,
  fetchUserById,
  updateTransactionApi,
} from "../store/weighingSlice";
import { Printer, Eye, Search, Filter, X } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import logoSrc from "../assets/logo.jpeg";

// ─── Theme utilities ──────────────────────────────────────────────────────────
import {
  getTicketSettings,
  resolvePdfTheme,
  resolveFontSize,
  resolveReportColors,
  TICKET_THEMES,
} from "../utils/ticketThemeConfig"; // adjust path to match your project structure

dayjs.extend(relativeTime);
dayjs.extend(duration);

const { RangePicker } = DatePicker;

// ─── Theme-aware PDF generator ────────────────────────────────────────────────
const generateThemedPDF = async (record, ticketSettings, formatTurnaroundTimeSimple, previewOnly = false) => {
  const doc      = new jsPDF({ unit: "mm", format: "a4" });
  const W        = 210;
  const L        = 14;          // left margin
  const R        = 196;         // right edge  (W - 14)
  const TW       = R - L;       // table / section width = 182
  const palette  = resolvePdfTheme(ticketSettings);
  const fontSize = resolveFontSize(ticketSettings.ticketFontSize);

  const companyName    = ticketSettings.companyName    || "QALIBRATED SYSTEMS LTD";
  const companyAddress = ticketSettings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996";

  // ── Fixed accent / status colours ────────────────────────────────────────
  const white     = [255, 255, 255];
  const black     = [0,   0,   0];
  const gray      = [107, 114, 128];
  const lightGray = [243, 244, 246];
  const borderCol = [209, 213, 219];
  const cream     = [254, 252, 232];
  const { primary: amberFill, primaryDark: amberBdr } = resolveReportColors(ticketSettings);
  const green     = [21,  128,  61];
  const red       = [185,  28,  28];

  // ── Status ───────────────────────────────────────────────────────────────
  const rawStatus  = (record.overallStatus || record.legalStatus || record.status || "N/A").toUpperCase();
  const isLegal    = rawStatus === "LEGAL" || rawStatus === "COMPLETED";
  const isOverload = rawStatus.includes("OVER");
  const statusText = isLegal ? "LEGAL" : isOverload ? "OVERLOAD" : rawStatus;
  const statusBg   = isLegal ? green : isOverload ? red : palette.accentBg;

  const ticketDate = dayjs(record.firstWeightDate || record.createdAt || new Date()).format("MMM D, YYYY HH:mm");
  const timestamp  = dayjs(record.firstWeightDate || record.createdAt || new Date()).format("DD-MM-YY hh:mm A");

  // ── Pre-load logo ─────────────────────────────────────────────────────────
  let logoImg = null;
  try {
    logoImg = await new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = ticketSettings.companyLogo || logoSrc;
    });
  } catch (_) { /* logo unavailable – skip */ }

  // ── HEADER ───────────────────────────────────────────────────────────────
  // Logo image (left)
  if (logoImg) {
    doc.addImage(logoImg, "JPEG", L, 7, 18, 18);
  }

  // Company name – centred
  doc.setFontSize(fontSize.title);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.bodyText);
  doc.text(companyName, W / 2, 13, { align: "center" });

  // Address – centred
  doc.setFontSize(fontSize.sub - 1);
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...gray);
  doc.text(companyAddress, W / 2, 19, { align: "center" });

  // Date – top-right
  doc.setFontSize(fontSize.sub);
  doc.setTextColor(...palette.bodyText);
  doc.text(ticketDate, R, 10, { align: "right" });

  // Status badge – top-right
  const badgeW = 24;
  doc.setFillColor(...statusBg);
  doc.roundedRect(R - badgeW, 13, badgeW, 7, 1.5, 1.5, "F");
  doc.setFontSize(fontSize.sub - 0.5);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...white);
  doc.text(statusText, R - badgeW / 2, 17.5, { align: "center" });

  // Divider
  doc.setDrawColor(...borderCol);
  doc.setLineWidth(0.3);
  doc.line(L, 29, R, 29);

  // "WEIGHING TICKET" badge – centred
  const tBadgeW = 52;
  doc.setFillColor(...amberFill);
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.roundedRect((W - tBadgeW) / 2, 32, tBadgeW, 8, 2, 2, "FD");
  doc.setFontSize(fontSize.heading);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...black);
  doc.text("WEIGHING TICKET", W / 2, 37.5, { align: "center" });

  // ── TICKET DETAILS ───────────────────────────────────────────────────────
  let y = 47;

  doc.setFillColor(...palette.headerBg);
  doc.roundedRect(L, y, TW, 7, 2, 2, "F");
  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.headerText);
  doc.text("TICKET DETAILS", W / 2, y + 5, { align: "center" });
  y += 9; // header height + small gap

  const detailsY = y;
  autoTable(doc, {
    startY: detailsY,
    margin: { left: L, right: L },
    theme: "plain",
    styles: {
      fontSize: fontSize.body,
      cellPadding: { top: 1.8, right: 2, bottom: 1.8, left: 3 },
      textColor: black,
      lineColor: palette.border,
      lineWidth: 0.2,
      fillColor: [252, 252, 252],
    },
    columnStyles: {
      0: { fontStyle: "bold", cellWidth: 26 },
      1: { cellWidth: 62 },
      2: { fontStyle: "bold", cellWidth: 28 },
      3: { cellWidth: 66 },
    },
    body: [
      ["TICKET NO",   `: ${record.receiptNo        || "N/A"}`, "REGISTRATION", `: ${record.noPlate          || "N/A"}`],
      ["AXLE TYPE",   `: ${record.axleType          || "N/A"}`, "COMMODITY",    `: ${record.commodityName    || "N/A"}`],
      ["TRANSPORTER", `: ${record.transporterName   || "N/A"}`, "TIMESTAMP",    `: ${timestamp}`],
      ["SOURCE",      `: ${record.originName        || "N/A"}`, "DESTINATION",  `: ${record.destinationName || "N/A"}`],
      ["OPERATOR",    `: ${record.operatorName      || "N/A"}`, "DRIVER",       `: ${record.driverName      || "N/A"}`],
      ["SUPPLIER",    `: ${record.supplierName      || "N/A"}`, "CUSTOMER",     `: ${record.customerName    || "N/A"}`],
      ["CONTAINER",   `: ${record.containerNo       || "N/A"}`, "SEAL NO",      `: ${record.sealNo          || "N/A"}`],
      ["WEIGHBRIDGE", `: ${record.weighBridgeName   || "N/A"}`, "WEIGH MODE",   `: ${record.weighMode       || "N/A"}`],
    ],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.roundedRect(L, detailsY, TW, doc.lastAutoTable.finalY - detailsY, 3, 3, "S");

  // ── WEIGHT MEASUREMENTS ──────────────────────────────────────────────────
  y = doc.lastAutoTable.finalY + 4;

  const grossDate = record.firstWeightDate  ? dayjs(record.firstWeightDate).format("DD-MM-YY hh:mm A")  : "—";
  const tareDate  = record.secondWeightDate ? dayjs(record.secondWeightDate).format("DD-MM-YY hh:mm A") : "—";
  const operator  = record.operatorName  || "—";
  const scale     = record.scaleName     || "—";
  const bridge    = record.weighBridgeName || "—";

  const wmY = y;
  autoTable(doc, {
    startY: wmY,
    margin: { left: L, right: L },
    theme: "grid",
    headStyles: {
      fillColor: lightGray,
      textColor: black,
      fontStyle: "bold",
      fontSize: fontSize.body,
      halign: "center",
      lineColor: borderCol,
    },
    styles: {
      fontSize: fontSize.body,
      textColor: black,
      lineColor: borderCol,
      cellPadding: { top: 2, bottom: 2, left: 3, right: 2 },
    },
    columnStyles: {
      0: { fontStyle: "bold", cellWidth: 32 },
      1: { cellWidth: 26 },
      2: { cellWidth: 34 },
      3: { cellWidth: 30 },
      4: { cellWidth: 22 },
    },
    head: [["MEASUREMENT", "WEIGHT (kg)", "DATE", "OPERATOR", "SCALE", "WEIGHBRIDGE"]],
    body: [
      ["GROSS WEIGHT", record.firstWeight  ? `${record.firstWeight} kg`  : "—", grossDate, operator, scale, bridge],
      ["TARE WEIGHT",  record.secondWeight ? `${record.secondWeight} kg` : "—", tareDate,  operator, scale, bridge],
      ["NET WEIGHT",   record.netWeight    ? `${record.netWeight} kg`    : "—", grossDate, operator, scale, bridge],
    ],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.roundedRect(L, wmY, TW, doc.lastAutoTable.finalY - wmY, 3, 3, "S");

  // ── AXLE WEIGHT ANALYSIS ─────────────────────────────────────────────────
  y = doc.lastAutoTable.finalY + 4;

  doc.setFillColor(...palette.headerBg);
  doc.roundedRect(L, y, TW, 8, 2, 2, "F");
  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.headerText);
  doc.text("AXLE WEIGHT ANALYSIS", W / 2, y + 5.5, { align: "center" });
  y += 10; // header height + small gap

  const axleY = y;
  autoTable(doc, {
    startY: axleY,
    margin: { left: L, right: L },
    theme: "grid",
    headStyles: {
      fillColor: lightGray,
      textColor: black,
      fontStyle: "bold",
      fontSize: fontSize.body,
      halign: "center",
      lineColor: borderCol,
    },
    styles: {
      fontSize: fontSize.body,
      halign: "center",
      textColor: black,
      lineColor: borderCol,
      cellPadding: 2,
    },
    columnStyles: {
      0: { fontStyle: "bold", halign: "left", cellWidth: 28 },
    },
    head: [["ITEMS", "GROUP 1", "GROUP 2", "GROUP 3", "GROUP 4", "GVW"]],
    body: [
      ["ACTUAL WT",  "N/A", "N/A", "N/A", "N/A", `${record.firstWeight || 0} KG`],
      ["PDF",        "N/A", "N/A", "N/A", "N/A", "N/A"],
      ["ALLOWED",    "N/A", "N/A", "N/A", "N/A", "N/A"],
      ["ALLOWED+5%", "N/A", "N/A", "N/A", "N/A", "N/A"],
      ["EXCESS",     "N/A", "N/A", "N/A", "N/A", "N/A"],
      [
        { content: "RESULT", styles: { fontStyle: "bold" } },
        "N/A", "N/A", "N/A", "N/A",
        { content: statusText, styles: { fillColor: statusBg, textColor: white, fontStyle: "bold" } },
      ],
    ],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.roundedRect(L, axleY, TW, doc.lastAutoTable.finalY - axleY, 3, 3, "S");

  // ── WEIGHT SUMMARY ───────────────────────────────────────────────────────
  y = doc.lastAutoTable.finalY + 4;

  const summaryY = y;
  autoTable(doc, {
    startY: summaryY,
    margin: { left: L, right: L },
    theme: "grid",
    styles: {
      lineColor: borderCol,
      lineWidth: 0.4,
      cellPadding: { top: 3, bottom: 3, left: 2, right: 2 },
    },
    headStyles: {
      fillColor: lightGray,
      textColor: black,
      fontStyle: "bold",
      fontSize: fontSize.sub,
      halign: "center",
    },
    head: [["FIRST WEIGHT", "SECOND WEIGHT", "NET WEIGHT"]],
    body: [[
      { content: `${record.firstWeight  || 0} Kg`, styles: { fontSize: 13, fontStyle: "bold", halign: "center" } },
      { content: `${record.secondWeight || 0} Kg`, styles: { fontSize: 13, fontStyle: "bold", halign: "center" } },
      {
        content: `${record.netWeight || 0} Kg`,
        styles: { fontSize: 13, fontStyle: "bold", halign: "center", fillColor: palette.netWeightBg },
      },
    ]],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.roundedRect(L, summaryY, TW, doc.lastAutoTable.finalY - summaryY, 3, 3, "S");

  // ── VEHICLE SNAPSHOT ─────────────────────────────────────────────────────
  y = doc.lastAutoTable.finalY + 4;

  doc.setFillColor(...palette.headerBg);
  doc.roundedRect(L, y, TW, 8, 2, 2, "F");
  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.headerText);
  doc.text("VEHICLE SNAPSHOT", W / 2, y + 5.5, { align: "center" });
  y += 8;

  const snapH = 45;
  doc.setFillColor(30, 30, 35);
  doc.roundedRect(L, y, TW, snapH, 3, 3, "F");
  doc.setFontSize(fontSize.body);
  doc.setFont("helvetica", "italic");
  doc.setTextColor(160, 160, 160);
  doc.text(`[ Vehicle Image: ${record.noPlate || "N/A"} ]`, W / 2, y + snapH / 2, { align: "center" });

  doc.setFontSize(fontSize.body - 1);
  doc.setFont("helvetica", "italic");
  doc.setTextColor(...gray);
  doc.text(
    `Captured: ${dayjs(record.firstWeightDate || new Date()).format("DD/MM/YYYY HH:mm:ss")}`,
    W / 2, y + snapH + 5, { align: "center" }
  );

  // Advance y past snapshot + caption
  y += snapH + 10;

  // ── REMARKS (optional) ───────────────────────────────────────────────────
  if (record.remarks || record.notes) {
    y += 2;
    doc.setFillColor(...palette.headerBg);
    doc.roundedRect(L, y, TW, 7, 2, 2, "F");
    doc.setFontSize(fontSize.sub);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...palette.headerText);
    doc.text("REMARKS / NOTES", W / 2, y + 5, { align: "center" });
    y += 10;
    doc.setTextColor(...palette.bodyText);
    doc.setFontSize(fontSize.body);
    doc.setFont("helvetica", "normal");
    const remarkLines = doc.splitTextToSize(record.remarks || record.notes, TW - 4);
    doc.text(remarkLines, L + 2, y);
    y += remarkLines.length * (fontSize.body * 0.35) + 4;
  }

  // ── FOOTER ───────────────────────────────────────────────────────────────
  y += 4;

  doc.setFillColor(248, 249, 250);
  doc.setDrawColor(...borderCol);
  doc.setLineWidth(0.3);
  doc.roundedRect(L, y, TW, 15, 3, 3, "FD");

  // Logo image (left inside footer)
  if (logoImg) {
    doc.addImage(logoImg, "JPEG", L + 2, y + 2, 11, 11);
  }

  // Tagline (centre)
  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.bodyText);
  doc.text("Powered by Qalibrated Systems", W / 2, y + 7, { align: "center" });
  doc.setFontSize(fontSize.body - 1);
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...gray);
  doc.text("www.qalibrated.co.ke", W / 2, y + 11, { align: "center" });

  // "Inventing and Making Happen" badge (right, inside footer)
  const tagW = 46;
  const tagX = R - tagW - 2;
  doc.setFillColor(...amberFill);
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.3);
  doc.roundedRect(tagX, y + 4, tagW, 7, 2, 2, "FD");
  doc.setFontSize(6.5);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...black);
  doc.text("Inventing and Making Happen", tagX + tagW / 2, y + 8.5, { align: "center" });

  if (previewOnly) {
    return URL.createObjectURL(doc.output("blob"));
  }

  doc.save(`Ticket_${record.receiptNo || "unknown"}_${TICKET_THEMES[ticketSettings.ticketTheme]?.name || "PDF"}.pdf`);
  message.success(`Weighing ticket exported (${TICKET_THEMES[ticketSettings.ticketTheme]?.name || "Default"} theme)!`);
};

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function Transactions() {
  const dispatch = useDispatch();
  const { transactions, loading, total } = useSelector(
    (state) => state.weighing
  );
  const [selectedRecord, setSelectedRecord] = useState(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [editedRecord, setEditedRecord] = useState(null);
  const [saving, setSaving] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [pdfTheme, setPdfTheme] = useState(() => getTicketSettings().ticketTheme || "modern");
  const [isExportPreviewOpen, setIsExportPreviewOpen] = useState(false);
  const [previewBlobUrl, setPreviewBlobUrl] = useState(null);
  const [previewLoading, setPreviewLoading] = useState(false);

  const isCompletedRecord = selectedRecord
    ? (selectedRecord.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) ||
      selectedRecord.status === "Completed" ||
      selectedRecord.status === "completed"
    : false;

  // ── Live ticket settings (synced from SystemSettings) ─────────────────────
  const [ticketSettings, setTicketSettings] = useState(getTicketSettings);

  // Listen for changes broadcast by SystemSettings
  useEffect(() => {
    const handler = (e) => setTicketSettings((prev) => ({ ...prev, ...e.detail }));
    window.addEventListener("ticketSettingsChanged", handler);
    return () => window.removeEventListener("ticketSettingsChanged", handler);
  }, []);

  const [filters, setFilters] = useState({
    search: "",
    dateRange: null,
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: null,
    vehicle: "",
    driver: "",
    commodity: "",
    supplier: "",
    transporter: "",
    customer: "",
    operator: "",
    origin: "",
    destination: "",
    weighbridge: "",
    weighMode: null,
    page: 1,
    pageSize: 10,
  });

  const loadTransactions = useCallback(() => {
    const params = {
      pageNumber: filters.page,
      pageSize: filters.pageSize,
    };

    if (filters.search) params.search = filters.search;

    if (filters.dateRange && filters.dateRange[0] && filters.dateRange[1]) {
      params.startDate = filters.dateRange[0].format("YYYY-MM-DD");
      params.endDate = filters.dateRange[1].format("YYYY-MM-DD");
    } else {
      if (filters.startDate) params.startDate = filters.startDate;
      if (filters.endDate) params.endDate = filters.endDate;
    }

    if (filters.startTime) params.startTime = filters.startTime;
    if (filters.endTime) params.endTime = filters.endTime;
    if (filters.status) params.status = filters.status;

    dispatch(fetchTransactions(params));
  }, [
    dispatch,
    filters.page,
    filters.pageSize,
    filters.search,
    filters.dateRange,
    filters.startDate,
    filters.endDate,
    filters.startTime,
    filters.endTime,
    filters.status,
  ]);

  const filteredTransactions = React.useMemo(() => {
    let filtered = transactions || [];

    if (filters.status) {
      if (filters.status === "completed") {
        filtered = filtered.filter(
          (t) =>
            (t.secondWeight && parseFloat(t.secondWeight) > 0) ||
            t.status === "Completed" ||
            t.status === "completed"
        );
      } else if (filters.status === "inprogress") {
        filtered = filtered.filter(
          (t) =>
            (!t.secondWeight || parseFloat(t.secondWeight) === 0) &&
            t.status !== "Completed" &&
            t.status !== "completed"
        );
      }
    }

    if (filters.startDate && !filters.dateRange) {
      const start = new Date(`${filters.startDate}T${filters.startTime || "00:00"}`);
      filtered = filtered.filter((t) => new Date(t.createdAt) >= start);
    }
    if (filters.endDate && !filters.dateRange) {
      const end = new Date(`${filters.endDate}T${filters.endTime || "23:59"}`);
      filtered = filtered.filter((t) => new Date(t.createdAt) <= end);
    }

    const textFilters = [
      ["vehicle",     "noPlate"],
      ["driver",      "driverName"],
      ["commodity",   "commodityName"],
      ["supplier",    "supplierName"],
      ["transporter", "transporterName"],
      ["customer",    "customerName"],
      ["origin",      "originName"],
      ["destination", "destinationName"],
      ["weighbridge", "weighBridgeName"],
    ];

    textFilters.forEach(([filterKey, dataKey]) => {
      if (filters[filterKey]) {
        const val = filters[filterKey].toLowerCase();
        filtered = filtered.filter((t) => t[dataKey]?.toLowerCase().includes(val));
      }
    });

    if (filters.operator) {
      const op = filters.operator.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.operatorName?.toLowerCase().includes(op) ||
          t.firstWeightOperator?.toLowerCase().includes(op)
      );
    }
    if (filters.weighMode) {
      filtered = filtered.filter(
        (t) => t.weighMode?.toLowerCase() === filters.weighMode.toLowerCase()
      );
    }

    return filtered;
  }, [transactions, filters]);

  useEffect(() => {
    const timeoutId = setTimeout(() => loadTransactions(), filters.search ? 500 : 0);
    return () => clearTimeout(timeoutId);
  }, [loadTransactions]);

  const activeFilterCount = Object.entries(filters).filter(([key, value]) => {
    if (["page", "pageSize"].includes(key)) return false;
    if (key === "dateRange") return value !== null;
    return value !== "" && value !== null;
  }).length;

  const clearFilters = () => {
    setFilters({
      search: "",
      dateRange: null,
      startDate: "",
      endDate: "",
      startTime: "",
      endTime: "",
      status: null,
      vehicle: "",
      driver: "",
      commodity: "",
      supplier: "",
      transporter: "",
      customer: "",
      operator: "",
      origin: "",
      destination: "",
      weighbridge: "",
      weighMode: null,
      page: 1,
      pageSize: filters.pageSize,
    });
  };

  const openViewDrawer = async (record) => {
    let enriched = { ...record };
    if (record.operatorId) {
      try {
        const operator = await dispatch(fetchUserById(record.operatorId)).unwrap();
        enriched.operatorName = operator?.fullName || operator?.name || "N/A";
      } catch {
        enriched.operatorName = "Unknown Operator";
      }
    }
    setSelectedRecord(enriched);
    setEditedRecord(enriched);
    setIsEditing(false);
    setIsDrawerOpen(true);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      await dispatch(
        updateTransactionApi({
          ticketId: editedRecord.ticketID || editedRecord.id,
          data: {
            receiptNo: editedRecord.receiptNo,
            noPlate: editedRecord.noPlate,
            driverName: editedRecord.driverName,
            commodityName: editedRecord.commodityName,
            transporterName: editedRecord.transporterName,
            supplierName: editedRecord.supplierName,
            customerName: editedRecord.customerName,
            originName: editedRecord.originName,
            destinationName: editedRecord.destinationName,
            weighMode: editedRecord.weighMode,
            status: editedRecord.status,
            axleType: editedRecord.axleType,
            containerNo: editedRecord.containerNo,
            sealNo: editedRecord.sealNo,
            remarks: editedRecord.remarks,
            notes: editedRecord.notes,
          },
        })
      ).unwrap();
      message.success("Updated successfully!");
      setSelectedRecord(editedRecord);
      setIsEditing(false);
      loadTransactions();
    } catch (error) {
      message.error("Failed to update: " + (error.message || "Unknown error"));
    } finally {
      setSaving(false);
    }
  };

  const calculateTurnaroundTime = (firstWeightDate, secondWeightDate, turnaroundTime) => {
    if (turnaroundTime) {
      const parts = turnaroundTime.split(":");
      if (parts.length >= 2) {
        const h = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10);
        const total = h * 60 + m;
        if (total < 1) return { display: "< 1m", minutes: 0 };
        return { display: h > 0 ? (m ? `${h}h ${m}m` : `${h}h`) : `${m}m`, minutes: total };
      }
    }
    if (!firstWeightDate || !secondWeightDate) return { display: "N/A", minutes: 0 };
    const diffMinutes = dayjs(secondWeightDate).diff(dayjs(firstWeightDate), "minute");
    if (diffMinutes < 1) return { display: "< 1m", minutes: 0 };
    if (diffMinutes < 60) return { display: `${diffMinutes}m`, minutes: diffMinutes };
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return { display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, minutes: diffMinutes };
  };

  const calculateWaitTime = (createdAt) => {
    if (!createdAt) return { display: "-", minutes: 0 };
    const now = dayjs();
    const created = dayjs(createdAt);
    const diffMinutes = now.diff(created, "minute");
    if (diffMinutes < 1) return { display: "< 1m", minutes: 0 };
    if (diffMinutes < 60) return { display: `${diffMinutes}m`, minutes: diffMinutes };
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return { display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, minutes: diffMinutes };
  };

  const getTimeColor = (minutes, isCompleted) => {
    if (isCompleted) {
      if (minutes < 30) return "green";
      if (minutes < 60) return "blue";
      if (minutes < 120) return "orange";
      return "red";
    } else {
      if (minutes < 30) return "green";
      if (minutes < 60) return "orange";
      return "red";
    }
  };

  const formatTurnaroundTimeSimple = (firstWeightDate, secondWeightDate, turnaroundTime) => {
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
    const diffMin = dayjs(secondWeightDate).diff(dayjs(firstWeightDate), "minute");
    if (diffMin < 1) return "< 1m";
    return diffMin < 60 ? `${diffMin}m` : `${Math.floor(diffMin / 60)}h ${diffMin % 60}m`;
  };

  // ── Current theme for badge display ───────────────────────────────────────
  const currentThemeMeta = TICKET_THEMES[ticketSettings.ticketTheme] || TICKET_THEMES.modern;
  const themePreviewColor = currentThemeMeta.preview.header;
  // Selected PDF theme (controls drawer title pill + Export PDF button)
  const selectedThemeMeta = TICKET_THEMES[pdfTheme] || TICKET_THEMES.modern;
  const selectedThemeColor = selectedThemeMeta.preview.header;

  const columns = [
    {
      title: "#",
      width: 40,
      fixed: "left",
      render: (_, __, index) => {
        const rowNumber = (filters.page - 1) * filters.pageSize + index + 1;
        return (
          <div className="flex items-center justify-center">
            <span className="inline-flex items-center justify-center w-5 h-5 rounded-full bg-gradient-to-br from-amber-100 to-amber-200 text-[10px] font-extrabold text-amber-900 border border-amber-300 shadow-sm">
              {rowNumber}
            </span>
          </div>
        );
      },
    },
    {
      title: "Date/Time",
      dataIndex: "createdAt",
      width: 75,
      render: (d) => (
        <div className="text-[10px] leading-tight">
          <div className="font-semibold text-gray-800">{dayjs(d).format("DD-MMM")}</div>
          <div className="text-gray-500 font-medium">{dayjs(d).format("HH:mm")}</div>
        </div>
      ),
    },
    {
      title: "Receipt",
      dataIndex: "receiptNo",
      width: 70,
      render: (t) => (
        <span className="text-[10px] font-mono font-bold text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">
          {t || "-"}
        </span>
      ),
    },
    {
      title: "Vehicle",
      dataIndex: "noPlate",
      width: 70,
      render: (t) => (
        <div className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[10px] font-bold">
          {t || "-"}
        </div>
      ),
    },
    {
      title: "Driver",
      dataIndex: "driverName",
      width: 85,
      render: (t) => <span className="text-[10px] text-gray-700 font-medium">{t || "-"}</span>,
    },
    {
      title: "Commodity",
      dataIndex: "commodityName",
      width: 90,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Supplier",
      dataIndex: "supplierName",
      width: 85,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Transporter",
      dataIndex: "transporterName",
      width: 90,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Customer",
      dataIndex: "customerName",
      width: 85,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Origin",
      dataIndex: "originName",
      width: 80,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Destination",
      dataIndex: "destinationName",
      width: 90,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Weighbridge",
      dataIndex: "weighBridgeName",
      width: 95,
      render: (t) => <span className="text-[10px] text-gray-600">{t || "-"}</span>,
    },
    {
      title: "Mode",
      dataIndex: "weighMode",
      width: 70,
      render: (t) => (
        <span className="text-[10px] font-semibold text-gray-700">{t || "N/A"}</span>
      ),
    },
    {
      title: "Operator",
      dataIndex: "operatorName",
      width: 85,
      render: (text, record) => {
        const name = text || record.firstWeightOperator || "N/A";
        return <span className="text-[10px] text-gray-700 font-medium">{name}</span>;
      },
    },
    {
      title: "1st",
      dataIndex: "firstWeight",
      width: 65,
      align: "right",
      render: (w) => (
        <span className="text-[10px] font-bold text-orange-600">
          {w ? `${parseFloat(w).toLocaleString()}` : "-"}
        </span>
      ),
    },
    {
      title: "2nd",
      dataIndex: "secondWeight",
      width: 65,
      align: "right",
      render: (w) => (
        <span className="text-[10px] font-bold text-green-600">
          {w ? `${parseFloat(w).toLocaleString()}` : "-"}
        </span>
      ),
    },
    {
      title: "Net",
      dataIndex: "netWeight",
      width: 70,
      align: "right",
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[11px] font-extrabold text-amber-600">
            {w ? `${parseFloat(w).toLocaleString()}` : "-"}
          </span>
          {w && (
            <span className="text-[8px] text-amber-500 uppercase font-semibold">kg</span>
          )}
        </div>
      ),
    },
    {
      title: <ClockCircleOutlined style={{ fontSize: "10px" }} />,
      width: 55,
      render: (_, record) => {
        const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted =
          hasSecondWeight || record.status === "Completed" || record.status === "completed";
        const timeData = isCompleted
          ? calculateTurnaroundTime(record.firstWeightDate, record.secondWeightDate, record.turnaroundTime)
          : calculateWaitTime(record.createdAt);
        const color = getTimeColor(timeData.minutes, isCompleted);
        const title = isCompleted
          ? `Turnaround Time: ${timeData.display}`
          : `Waiting Time: ${timeData.display}`;
        return (
          <Tag
            color={color}
            className="text-[9px] font-bold px-2 py-0 m-0 rounded-full shadow-sm leading-tight"
            title={title}
          >
            {timeData.display}
          </Tag>
        );
      },
    },
    {
      title: "Status",
      dataIndex: "status",
      width: 65,
      fixed: "right",
      render: (s, record) => {
        const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted = hasSecondWeight || s === "Completed" || s === "completed";
        return (
          <Tag
            color={isCompleted ? "success" : "warning"}
            className="text-[9px] font-bold px-2 py-0.5 m-0 uppercase rounded-full shadow-sm leading-tight"
          >
            {isCompleted ? "✓" : "⏳"}
          </Tag>
        );
      },
    },
    {
      title: "",
      width: 50,
      fixed: "right",
      render: (_, r) => (
        <Button
          size="small"
          type="text"
          icon={<Eye size={12} />}
          onClick={() => openViewDrawer(r)}
          className="text-amber-600 hover:bg-amber-50 hover:text-amber-700 h-6 px-1.5 text-[10px] font-semibold transition-all"
        />
      ),
    },
  ];

  return (
    <div className="h-screen flex flex-col bg-white">
      {/* Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 shrink-0">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5}
                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
            <div>
              <div className="text-[11px] font-bold text-gray-900 leading-tight">Transactions</div>
              <div className="text-[9px] text-amber-700 font-medium leading-tight">
                <span className="font-semibold">{filteredTransactions.length}</span> of{" "}
                <span className="font-semibold">{total || 0}</span>
              </div>
            </div>

            {/* Live theme indicator */}
            <div
              className="flex items-center gap-1.5 ml-2 px-2 py-0.5 rounded-full border text-[9px] font-bold"
              style={{
                borderColor: themePreviewColor,
                color: themePreviewColor,
                backgroundColor: `${themePreviewColor}15`,
              }}
              title="Active ticket theme — change in System Settings → Tickets & Printing"
            >
              <span
                className="w-2 h-2 rounded-full"
                style={{ backgroundColor: themePreviewColor }}
              />
              {currentThemeMeta.name} theme
            </div>
          </div>

          <div className="flex gap-2 items-center">
            <Input
              allowClear
              placeholder="Search..."
              prefix={<Search size={10} className="text-gray-400" />}
              className="w-52 h-7 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })}
            />

            {activeFilterCount > 0 && (
              <span className="text-[9px] text-amber-900 font-bold bg-gradient-to-r from-amber-100 to-amber-200 px-2 py-0.5 rounded-full border border-amber-300 shadow-sm">
                🎯 {activeFilterCount} active
              </span>
            )}

            {activeFilterCount > 0 && (
              <Button
                size="small"
                danger
                icon={<X size={12} />}
                onClick={clearFilters}
                className="h-7 text-[10px] font-semibold shadow-sm rounded bg-red-50 border-red-300 text-red-700 hover:bg-red-100"
              >
                Clear
              </Button>
            )}

            <Button
              icon={<Filter size={14} />}
              className={`h-7 text-[11px] font-medium ${
                showFilters
                  ? "bg-gradient-to-r from-amber-500 to-orange-600 text-white border-amber-500"
                  : "border-gray-300 hover:border-amber-500 hover:text-amber-600"
              }`}
              onClick={() => setShowFilters(!showFilters)}
            >
              Filters
            </Button>

            <Button
              type="primary"
              icon={<ReloadOutlined />}
              className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0 h-7 text-[10px] font-semibold text-white shadow-sm"
              onClick={loadTransactions}
              loading={loading}
            >
              Refresh
            </Button>
          </div>
        </div>
      </div>

      {/* Filters Panel */}
      {showFilters && (
        <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-orange-50/20 border-b border-amber-200 px-3 py-2 shrink-0">
          {/* Row 1 */}
          <div className="grid grid-cols-5 gap-2 mb-2">
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Date Range
              </label>
              <RangePicker
                className="w-full h-6 text-[10px] border-amber-300"
                value={filters.dateRange}
                onChange={(d) =>
                  setFilters({ ...filters, dateRange: d, startDate: "", endDate: "", page: 1 })
                }
                format="DD-MM-YY"
                placeholder={["Start", "End"]}
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Start Time
              </label>
              <input
                type="time"
                value={filters.startTime}
                onChange={(e) => setFilters({ ...filters, startTime: e.target.value, page: 1 })}
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2"
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> End Time
              </label>
              <input
                type="time"
                value={filters.endTime}
                onChange={(e) => setFilters({ ...filters, endTime: e.target.value, page: 1 })}
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2"
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Status
              </label>
              <select
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2"
                value={filters.status || ""}
                onChange={(e) => setFilters({ ...filters, status: e.target.value || null, page: 1 })}
              >
                <option value="">All</option>
                <option value="completed">Completed</option>
                <option value="inprogress">In Progress</option>
              </select>
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Vehicle
              </label>
              <Input
                placeholder="Vehicle..."
                className="h-6 text-[10px] border-amber-300"
                value={filters.vehicle || ""}
                onChange={(e) => setFilters({ ...filters, vehicle: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>

          {/* Row 2 */}
          <div className="grid grid-cols-6 gap-2 mb-2">
            {[
              ["driver", "Driver"],
              ["commodity", "Commodity"],
              ["supplier", "Supplier"],
              ["transporter", "Transporter"],
              ["customer", "Customer"],
              ["operator", "Operator"],
            ].map(([key, label]) => (
              <div key={key}>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                  <span className="w-1 h-1 bg-amber-500 rounded-full" /> {label}
                </label>
                <Input
                  placeholder={`${label}...`}
                  className="h-6 text-[10px] border-amber-300"
                  value={filters[key] || ""}
                  onChange={(e) => setFilters({ ...filters, [key]: e.target.value, page: 1 })}
                  allowClear
                />
              </div>
            ))}
          </div>

          {/* Row 3 */}
          <div className="grid grid-cols-6 gap-2">
            {[
              ["origin", "Origin"],
              ["destination", "Destination"],
              ["weighbridge", "Weighbridge"],
            ].map(([key, label]) => (
              <div key={key}>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                  <span className="w-1 h-1 bg-amber-500 rounded-full" /> {label}
                </label>
                <Input
                  placeholder={`${label}...`}
                  className="h-6 text-[10px] border-amber-300"
                  value={filters[key] || ""}
                  onChange={(e) => setFilters({ ...filters, [key]: e.target.value, page: 1 })}
                  allowClear
                />
              </div>
            ))}
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Mode
              </label>
              <select
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2"
                value={filters.weighMode || ""}
                onChange={(e) =>
                  setFilters({ ...filters, weighMode: e.target.value || null, page: 1 })
                }
              >
                <option value="">All</option>
                <option value="single">Single</option>
                <option value="double">Double</option>
                <option value="auto">Auto</option>
              </select>
            </div>
            <div className="col-span-2" />
          </div>
        </div>
      )}

      {/* Table */}
      <div className="flex-1 overflow-hidden">
        <div className="h-full bg-white overflow-hidden">
          <Table
            columns={columns}
            dataSource={filteredTransactions}
            rowKey="id"
            loading={loading}
            size="small"
            className="compact-table"
            scroll={{ y: "calc(100vh - 120px)", x: 1650 }}
            pagination={{
              current: filters.page,
              pageSize: filters.pageSize,
              total: filteredTransactions.length,
              showSizeChanger: true,
              showTotal: (total) => `${total} records`,
              size: "small",
              pageSizeOptions: ["10", "20", "50", "100"],
              onChange: (p, ps) => setFilters({ ...filters, page: p, pageSize: ps }),
            }}
            rowClassName={(record) => {
              const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
              const isCompleted =
                hasSecondWeight || record.status === "Completed" || record.status === "completed";
              return isCompleted ? "completed-row" : "incomplete-row";
            }}
          />
        </div>
      </div>

      {/* Drawer */}
      <Drawer
        title={
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
              <span className="text-white text-xs font-bold">📋</span>
            </div>
            <span className="text-sm font-bold text-gray-900">
              Ticket: {selectedRecord?.receiptNo}
            </span>
            {/* PDF theme pill in drawer header — reflects current selection */}
            <span
              className="ml-auto text-[9px] font-bold px-2 py-0.5 rounded-full border"
              style={{
                color: selectedThemeColor,
                borderColor: selectedThemeColor,
                backgroundColor: `${selectedThemeColor}18`,
              }}
            >
              {selectedThemeMeta.name}
            </span>
          </div>
        }
        placement="right"
        onClose={() => setIsDrawerOpen(false)}
        open={isDrawerOpen}
        width={450}
        footer={
          <div className="flex gap-2 justify-between items-center">
            {/* PDF Theme toggle */}
            <div className="flex items-center gap-1">
              <span className="text-[9px] font-semibold text-gray-500 mr-1">PDF:</span>
              <button
                onClick={() => setPdfTheme("modern")}
                className={`h-6 px-2 text-[9px] font-bold rounded-l border transition-all ${
                  pdfTheme === "modern"
                    ? "bg-amber-500 text-white border-amber-500"
                    : "bg-white text-gray-500 border-gray-300 hover:border-amber-400 hover:text-amber-600"
                }`}
              >
                Modern
              </button>
              <button
                onClick={() => setPdfTheme("classic")}
                className={`h-6 px-2 text-[9px] font-bold rounded-r border-t border-b border-r transition-all ${
                  pdfTheme === "classic"
                    ? "bg-gray-800 text-white border-gray-800"
                    : "bg-white text-gray-500 border-gray-300 hover:border-gray-500 hover:text-gray-700"
                }`}
              >
                B&W
              </button>
            </div>
            <div className="flex gap-2">
              {!isEditing ? (
                <>
                  {!isCompletedRecord && (
                    <Button
                      size="small"
                      icon={<EditOutlined />}
                      onClick={() => setIsEditing(true)}
                      className="text-xs border-amber-300 text-amber-600 hover:border-amber-500"
                    >
                      Edit
                    </Button>
                  )}
                  <Button
                    size="small"
                    type="primary"
                    icon={<Printer size={14} />}
                    onClick={async () => {
                      setIsExportPreviewOpen(true);
                      setPreviewLoading(true);
                      const url = await generateThemedPDF(selectedRecord, { ...ticketSettings, ticketTheme: pdfTheme }, formatTurnaroundTimeSimple, true);
                      setPreviewBlobUrl(url);
                      setPreviewLoading(false);
                    }}
                    className="text-xs border-0"
                    style={{
                      background: pdfTheme === "modern"
                        ? "linear-gradient(135deg, var(--cs-500), var(--cs-600))"
                        : "linear-gradient(135deg, #374151, #111827)",
                    }}
                  >
                    Export PDF
                  </Button>
                </>
              ) : (
                <>
                  <Button
                    size="small"
                    icon={<CloseOutlined />}
                    onClick={() => { setEditedRecord(selectedRecord); setIsEditing(false); }}
                    className="text-xs"
                  >
                    Cancel
                  </Button>
                  <Button
                    size="small"
                    type="primary"
                    icon={<SaveOutlined />}
                    loading={saving}
                    onClick={handleSave}
                    className="text-xs bg-gradient-to-r from-amber-500 to-amber-600 border-0"
                  >
                    Save
                  </Button>
                </>
              )}
            </div>
          </div>
        }
      >
        {selectedRecord && (
          <div className="space-y-3 text-xs">
            {/* Basic Info */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">📋</span>
                </div>
                <span className="text-sm font-bold text-amber-900">BASIC INFORMATION</span>
              </div>
              <div className="grid grid-cols-2 gap-2.5">
                {[
                  { label: "Receipt", field: "receiptNo", editable: true },
                  { label: "Vehicle", field: "noPlate", editable: true },
                  { label: "Driver", field: "driverName", editable: true },
                  { label: "Commodity", field: "commodityName", editable: true },
                  { label: "Axle Type", field: "axleType", editable: true },
                  { label: "Weigh Mode", field: "weighMode", editable: true },
                  { label: "Container", field: "containerNo", editable: true },
                  { label: "Seal No", field: "sealNo", editable: true },
                ].map(({ label, field, editable }) => (
                  <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                        size="small"
                        className="text-xs border-amber-300 focus:border-amber-500"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">{selectedRecord[field] || "N/A"}</div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Parties */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">🏢</span>
                </div>
                <span className="text-sm font-bold text-amber-900">PARTIES</span>
              </div>
              <div className="grid grid-cols-2 gap-2.5">
                {[
                  { label: "Supplier", field: "supplierName", editable: true },
                  { label: "Customer", field: "customerName", editable: true },
                  { label: "Transporter", field: "transporterName", editable: true },
                  { label: "Operator", field: "operatorName", editable: false },
                ].map(({ label, field, editable }) => (
                  <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                        size="small"
                        className="text-xs border-amber-300"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">{selectedRecord[field] || "N/A"}</div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Locations */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">📍</span>
                </div>
                <span className="text-sm font-bold text-amber-900">LOCATIONS</span>
              </div>
              <div className="grid grid-cols-2 gap-2.5">
                {[
                  { label: "Origin", field: "originName", editable: true },
                  { label: "Destination", field: "destinationName", editable: true },
                  { label: "Weighbridge", field: "weighBridgeName", editable: false },
                  { label: "Operation", field: "operation", editable: false },
                ].map(({ label, field, editable }) => (
                  <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                        size="small"
                        className="text-xs border-amber-300"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">{selectedRecord[field] || "N/A"}</div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Weight Summary */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">⚖️</span>
                </div>
                <span className="text-sm font-bold text-amber-900">WEIGHT SUMMARY</span>
              </div>
              <div className="grid grid-cols-3 gap-2.5 mb-3">
                <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                  <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">1st Weight</div>
                  <div className="text-base font-extrabold text-orange-600">{selectedRecord.firstWeight || 0}</div>
                  <div className="text-[9px] text-amber-600 font-semibold">KILOGRAMS</div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.firstWeightDate ? dayjs(selectedRecord.firstWeightDate).format("DD-MM-YY HH:mm") : "N/A"}
                  </div>
                </div>
                <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                  <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">2nd Weight</div>
                  <div className="text-base font-extrabold text-green-600">{selectedRecord.secondWeight || 0}</div>
                  <div className="text-[9px] text-amber-600 font-semibold">KILOGRAMS</div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.secondWeightDate ? dayjs(selectedRecord.secondWeightDate).format("DD-MM-YY HH:mm") : "N/A"}
                  </div>
                </div>
                <div className="bg-gradient-to-br from-amber-200 via-amber-300 to-orange-300 rounded-lg p-2.5 border-2 border-amber-500 shadow-lg">
                  <div className="text-[10px] text-amber-900 font-extrabold uppercase mb-1">Net Weight</div>
                  <div className="text-lg font-black text-amber-950">{selectedRecord.netWeight || 0}</div>
                  <div className="text-[9px] text-amber-800 font-bold">KILOGRAMS</div>
                </div>
              </div>
              <div className="bg-white rounded-lg px-3 py-2.5 border-2 border-amber-300 flex items-center justify-between">
                <span className="text-[11px] text-amber-800 font-bold">⏱️ TURNAROUND TIME:</span>
                <span className="text-sm font-black text-amber-900 bg-amber-100 px-3 py-1 rounded-full">
                  {formatTurnaroundTimeSimple(selectedRecord.firstWeightDate, selectedRecord.secondWeightDate, selectedRecord.turnaroundTime)}
                </span>
              </div>
            </div>

            {/* Status & Remarks */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">📝</span>
                </div>
                <span className="text-sm font-bold text-amber-900">STATUS & REMARKS</span>
              </div>
              <div className="mb-3">
                <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">Status</div>
                <Tag
                  color={
                    (selectedRecord.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) ||
                    selectedRecord.status === "Completed"
                      ? "success"
                      : "warning"
                  }
                  className="text-xs font-bold px-3 py-1 shadow-sm"
                >
                  {(selectedRecord.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) ||
                  selectedRecord.status === "Completed"
                    ? "✅ COMPLETED"
                    : "⏳ IN PROGRESS"}
                </Tag>
              </div>
              <div>
                <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">Remarks / Notes</div>
                {isEditing ? (
                  <Input.TextArea
                    value={editedRecord.remarks || editedRecord.notes}
                    onChange={(e) => setEditedRecord({ ...editedRecord, remarks: e.target.value })}
                    rows={3}
                    className="text-xs border-amber-300 focus:border-amber-500"
                  />
                ) : (
                  <div className="text-gray-900 bg-white p-2.5 rounded-lg border-2 border-amber-200 text-xs font-medium">
                    {selectedRecord.remarks || selectedRecord.notes || "💭 No remarks available"}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </Drawer>

      {/* Export Preview Modal */}
      {selectedRecord && (
        <Modal
          open={isExportPreviewOpen}
          onCancel={() => { setIsExportPreviewOpen(false); setPreviewBlobUrl(null); }}
          width={980}
          title={
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                <Printer size={13} className="text-white" />
              </div>
              <span className="text-sm font-bold text-gray-900">
                Ticket Preview — {selectedRecord.receiptNo}
              </span>
            </div>
          }
          footer={null}
          destroyOnClose
        >
          <div className="flex gap-4" style={{ height: 680 }}>
            {/* ── Left: Exact PDF Preview via iframe ── */}
            <div className="flex-1 bg-gray-100 rounded-lg overflow-hidden border border-gray-200 flex items-center justify-center">
              {previewLoading || !previewBlobUrl ? (
                <div className="text-gray-400 text-sm font-medium">Generating preview…</div>
              ) : (
                <iframe
                  src={previewBlobUrl}
                  title="Ticket Preview"
                  className="w-full h-full rounded-lg"
                  style={{ border: "none" }}
                />
              )}
            </div>

            {/* ── Right: Export Options ── */}
            <div className="w-52 shrink-0 flex flex-col gap-3">
              <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
                <div className="text-[10px] font-bold text-gray-600 uppercase mb-2">Print Style</div>
                <div className="flex gap-1">
                  <button
                    onClick={async () => {
                      setPdfTheme("modern");
                      setPreviewLoading(true);
                      const url = await generateThemedPDF(selectedRecord, { ...ticketSettings, ticketTheme: "modern" }, formatTurnaroundTimeSimple, true);
                      setPreviewBlobUrl(url);
                      setPreviewLoading(false);
                    }}
                    className={`flex-1 py-2 text-[10px] font-bold rounded-l border transition-all ${
                      pdfTheme === "modern"
                        ? "bg-amber-500 text-white border-amber-500 shadow-sm"
                        : "bg-white text-gray-500 border-gray-300 hover:border-amber-400 hover:text-amber-600"
                    }`}
                  >
                    Modern
                  </button>
                  <button
                    onClick={async () => {
                      setPdfTheme("classic");
                      setPreviewLoading(true);
                      const url = await generateThemedPDF(selectedRecord, { ...ticketSettings, ticketTheme: "classic" }, formatTurnaroundTimeSimple, true);
                      setPreviewBlobUrl(url);
                      setPreviewLoading(false);
                    }}
                    className={`flex-1 py-2 text-[10px] font-bold rounded-r border-t border-b border-r transition-all ${
                      pdfTheme === "classic"
                        ? "bg-gray-800 text-white border-gray-800 shadow-sm"
                        : "bg-white text-gray-500 border-gray-300 hover:border-gray-500 hover:text-gray-700"
                    }`}
                  >
                    B&amp;W
                  </button>
                </div>
              </div>

              <Button
                type="primary"
                block
                icon={<Printer size={14} />}
                onClick={() => {
                  generateThemedPDF(selectedRecord, { ...ticketSettings, ticketTheme: pdfTheme }, formatTurnaroundTimeSimple);
                  setIsExportPreviewOpen(false);
                  setPreviewBlobUrl(null);
                }}
                style={{
                  background: pdfTheme === "modern"
                    ? "linear-gradient(135deg, var(--cs-500), var(--cs-600))"
                    : "linear-gradient(135deg, #374151, #111827)",
                  border: "none",
                  fontWeight: 700,
                }}
              >
                Download PDF
              </Button>

              <Button block onClick={() => { setIsExportPreviewOpen(false); setPreviewBlobUrl(null); }}>
                Cancel
              </Button>

              <div className="text-[10px] text-gray-400 text-center mt-auto pt-2 border-t border-gray-100">
                <div className="font-medium">{selectedRecord.receiptNo}</div>
                <div>{selectedRecord.noPlate}</div>
                <div className="mt-1" style={{ color: selectedThemeColor }}>
                  {selectedThemeMeta.name} theme
                </div>
              </div>
            </div>
          </div>
        </Modal>
      )}

      <style>{`
        .compact-table .ant-table { font-size: 10px; }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, var(--cs-50), var(--cs-100)) !important;
          border-bottom: 1.5px solid var(--cs-500) !important;
          padding: 5px 8px !important;
          font-weight: 700 !important;
          font-size: 9px !important;
          color: var(--cs-900) !important;
          text-transform: uppercase;
          letter-spacing: 0.3px;
          line-height: 1.2;
        }
        .compact-table .ant-table-tbody > tr > td {
          padding: 2px 6px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          transition: all 0.12s ease;
          line-height: 1.15;
        }
        .compact-table .ant-table-tbody > tr.completed-row > td {
          background: rgba(236, 253, 245, 0.4) !important;
        }
        .compact-table .ant-table-tbody > tr.completed-row:hover > td {
          background: rgba(236, 253, 245, 0.8) !important;
        }
        .compact-table .ant-table-tbody > tr.incomplete-row > td {
          background: white !important;
        }
        .compact-table .ant-table-tbody > tr.incomplete-row:hover > td {
          background: var(--cs-50) !important;
        }
        .compact-table .ant-pagination {
          margin: 6px 0 !important;
          padding: 0 8px !important;
        }
        .compact-table .ant-pagination-item,
        .compact-table .ant-pagination-prev,
        .compact-table .ant-pagination-next {
          min-width: 24px !important;
          height: 24px !important;
          line-height: 22px !important;
          font-size: 11px !important;
          border-radius: 4px !important;
          margin: 0 2px !important;
        }
        .compact-table .ant-pagination-item-active {
          background: linear-gradient(135deg, var(--cs-500), var(--cs-600)) !important;
          border-color: var(--cs-500) !important;
        }
        .compact-table .ant-pagination-item-active a {
          color: white !important;
          font-weight: 700 !important;
        }
        .compact-table .ant-select-selector {
          height: 24px !important;
          padding: 0 8px !important;
        }
        .compact-table .ant-select-selection-item {
          line-height: 22px !important;
          font-size: 11px !important;
        }
        .compact-table .ant-pagination-options { margin-left: 8px !important; }
        .compact-table .ant-pagination-total-text {
          font-size: 11px !important;
          line-height: 24px !important;
        }
      `}</style>
    </div>
  );
}