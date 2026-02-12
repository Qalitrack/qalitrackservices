import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, message, Radio, Drawer } from "antd";
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

dayjs.extend(relativeTime);
dayjs.extend(duration);

const { RangePicker } = DatePicker;

export default function Transactions() {
  const dispatch = useDispatch();
  const { transactions, loading, total } = useSelector(
    (state) => state.weighing
  );
  const [selectedRecord, setSelectedRecord] = useState(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [printMode, setPrintMode] = useState("color");
  const [isEditing, setIsEditing] = useState(false);
  const [editedRecord, setEditedRecord] = useState(null);
  const [saving, setSaving] = useState(false);
  const [showFilters, setShowFilters] = useState(false);

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
      const start = new Date(
        `${filters.startDate}T${filters.startTime || "00:00"}`
      );
      filtered = filtered.filter((t) => new Date(t.createdAt) >= start);
    }
    if (filters.endDate && !filters.dateRange) {
      const end = new Date(
        `${filters.endDate}T${filters.endTime || "23:59"}`
      );
      filtered = filtered.filter((t) => new Date(t.createdAt) <= end);
    }

    if (filters.vehicle) {
      const v = filters.vehicle.toLowerCase();
      filtered = filtered.filter((t) =>
        t.noPlate?.toLowerCase().includes(v)
      );
    }
    if (filters.driver) {
      const d = filters.driver.toLowerCase();
      filtered = filtered.filter((t) =>
        t.driverName?.toLowerCase().includes(d)
      );
    }
    if (filters.commodity) {
      const c = filters.commodity.toLowerCase();
      filtered = filtered.filter((t) =>
        t.commodityName?.toLowerCase().includes(c)
      );
    }
    if (filters.supplier) {
      const s = filters.supplier.toLowerCase();
      filtered = filtered.filter((t) =>
        t.supplierName?.toLowerCase().includes(s)
      );
    }
    if (filters.transporter) {
      const tr = filters.transporter.toLowerCase();
      filtered = filtered.filter((t) =>
        t.transporterName?.toLowerCase().includes(tr)
      );
    }
    if (filters.customer) {
      const cu = filters.customer.toLowerCase();
      filtered = filtered.filter((t) =>
        t.customerName?.toLowerCase().includes(cu)
      );
    }
    if (filters.operator) {
      const op = filters.operator.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.operatorName?.toLowerCase().includes(op) ||
          t.firstWeightOperator?.toLowerCase().includes(op)
      );
    }
    if (filters.origin) {
      const o = filters.origin.toLowerCase();
      filtered = filtered.filter((t) =>
        t.originName?.toLowerCase().includes(o)
      );
    }
    if (filters.destination) {
      const dest = filters.destination.toLowerCase();
      filtered = filtered.filter((t) =>
        t.destinationName?.toLowerCase().includes(dest)
      );
    }
    if (filters.weighbridge) {
      const wb = filters.weighbridge.toLowerCase();
      filtered = filtered.filter((t) =>
        t.weighBridgeName?.toLowerCase().includes(wb)
      );
    }
    if (filters.weighMode) {
      filtered = filtered.filter(
        (t) =>
          t.weighMode?.toLowerCase() === filters.weighMode.toLowerCase()
      );
    }

    return filtered;
  }, [transactions, filters]);

  useEffect(() => {
    const timeoutId = setTimeout(
      () => {
        loadTransactions();
      },
      filters.search ? 500 : 0
    );
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
        const operator = await dispatch(
          fetchUserById(record.operatorId)
        ).unwrap();
        enriched.operatorName =
          operator?.fullName || operator?.name || "N/A";
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

  const calculateTurnaroundTime = (firstWeightTime, secondWeightTime) => {
    if (!firstWeightTime || !secondWeightTime)
      return { display: "N/A", minutes: 0 };
    const first = dayjs(firstWeightTime);
    const second = dayjs(secondWeightTime);
    const diffMinutes = second.diff(first, "minute");
    if (diffMinutes < 1) return { display: "< 1m", minutes: 0 };
    if (diffMinutes < 60)
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return {
      display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`,
      minutes: diffMinutes,
    };
  };

  const calculateWaitTime = (createdAt) => {
    if (!createdAt) return { display: "-", minutes: 0 };
    const now = dayjs();
    const created = dayjs(createdAt);
    const diffMinutes = now.diff(created, "minute");
    if (diffMinutes < 1) return { display: "< 1m", minutes: 0 };
    if (diffMinutes < 60)
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return {
      display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`,
      minutes: diffMinutes,
    };
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

  const formatTurnaroundTimeSimple = (first, second) => {
    if (!first || !second) return "N/A";
    const diffMin = dayjs(second).diff(dayjs(first), "minute");
    return diffMin < 60
      ? `${diffMin}m`
      : `${Math.floor(diffMin / 60)}h ${diffMin % 60}m`;
  };

  const generatePDF = (record, isColor) => {
    const doc = new jsPDF();
    const colors = isColor
      ? {
          header: [70, 70, 70],
          text: [33, 33, 33],
          light: [245, 245, 245],
          accent: [220, 220, 220],
        }
      : {
          header: [0, 0, 0],
          text: [0, 0, 0],
          light: [255, 255, 255],
          accent: [240, 240, 240],
        };

    doc.setFontSize(14);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...colors.text);
    doc.text("QALIBRATED SYSTEMS LTD", 105, 15, { align: "center" });
    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text("PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996", 105, 20, {
      align: "center",
    });
    doc.setFontSize(16);
    doc.text("WEIGHING TICKET", 105, 35, { align: "center" });

    let y = 42;
    doc.setFillColor(...colors.header);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
    doc.text("TICKET DETAILS", 105, y + 5, { align: "center" });

    y += 7;
    doc.setTextColor(...colors.text);
    autoTable(doc, {
      startY: y,
      theme: "plain",
      styles: { fontSize: 8.5, cellPadding: 1.5, textColor: colors.text },
      columnStyles: {
        0: { fontStyle: "bold", cellWidth: 32 },
        1: { cellWidth: 58 },
        2: { fontStyle: "bold", cellWidth: 32 },
        3: { cellWidth: 58 },
      },
      body: [
        ["TICKET NO", `: ${record.receiptNo || "N/A"}`, "REGISTRATION", `: ${record.noPlate || "N/A"}`],
        ["AXLE TYPE", `: ${record.axleType || "N/A"}`, "COMMODITY", `: ${record.commodityName || "N/A"}`],
        ["TRANSPORTER", `: ${record.transporterName || "N/A"}`, "DRIVER", `: ${record.driverName || "N/A"}`],
        ["SUPPLIER", `: ${record.supplierName || "N/A"}`, "CUSTOMER", `: ${record.customerName || "N/A"}`],
        ["SOURCE", `: ${record.originName || "N/A"}`, "DESTINATION", `: ${record.destinationName || "N/A"}`],
        ["CONTAINER", `: ${record.containerNo || "N/A"}`, "SEAL NO", `: ${record.sealNo || "N/A"}`],
        ["WEIGH MODE", `: ${record.weighMode || "N/A"}`, "OPERATION", `: ${record.operation || "N/A"}`],
        ["WEIGHBRIDGE", `: ${record.weighBridgeName || "N/A"}`, "STATUS", `: ${record.status || "N/A"}`],
      ],
    });

    y = doc.lastAutoTable.finalY + 5;
    doc.setFillColor(...colors.header);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
    doc.text("WEIGHT SUMMARY", 105, y + 5, { align: "center" });

    y += 7;
    doc.setTextColor(...colors.text);
    autoTable(doc, {
      startY: y,
      theme: "grid",
      headStyles: {
        fillColor: colors.header,
        halign: "center",
        textColor: isColor ? [255, 255, 255] : [0, 0, 0],
      },
      styles: { halign: "center", fontSize: 9, textColor: colors.text },
      head: [["MEASUREMENT", "WEIGHT", "OPERATOR", "TIMESTAMP"]],
      body: [
        [
          "FIRST WEIGHT",
          `${record.firstWeight || 0} Kg`,
          record.firstWeightOperator || record.operatorName || "N/A",
          record.firstWeightTime
            ? dayjs(record.firstWeightTime).format("DD-MM-YY HH:mm")
            : "N/A",
        ],
        [
          "SECOND WEIGHT",
          `${record.secondWeight || 0} Kg`,
          record.secondWeightOperator || record.operatorName || "N/A",
          record.secondWeightTime
            ? dayjs(record.secondWeightTime).format("DD-MM-YY HH:mm")
            : "N/A",
        ],
        [
          { content: "NET WEIGHT", styles: { fillColor: colors.light, fontStyle: "bold", textColor: colors.text } },
          { content: `${record.netWeight || 0} Kg`, styles: { fillColor: colors.light, fontStyle: "bold", textColor: colors.text } },
          "",
          "",
        ],
        [
          { content: "TURNAROUND", styles: { fillColor: colors.accent, textColor: colors.text } },
          {
            content: formatTurnaroundTimeSimple(record.firstWeightTime, record.secondWeightTime),
            colSpan: 3,
            styles: { fillColor: colors.accent, textColor: colors.text },
          },
        ],
      ],
    });

    if (record.remarks || record.notes) {
      y = doc.lastAutoTable.finalY + 5;
      doc.setFillColor(...colors.header);
      doc.rect(14, y, 182, 7, "F");
      doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
      doc.text("REMARKS/NOTES", 105, y + 5, { align: "center" });
      y += 7;
      doc.setTextColor(...colors.text);
      doc.setFontSize(9);
      const remarkText = record.remarks || record.notes || "N/A";
      const splitRemarks = doc.splitTextToSize(remarkText, 170);
      doc.text(splitRemarks, 14, y + 3);
    }

    doc.save(`Ticket_${record.receiptNo}_${isColor ? "Color" : "BW"}.pdf`);
    message.success("PDF generated!");
  };

  const columns = [
    {
      title: "#",
      width: 40,
      fixed: "left",
      render: (_, __, index) => {
        const rowNumber =
          (filters.page - 1) * filters.pageSize + index + 1;
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
          <div className="font-semibold text-gray-800">
            {dayjs(d).format("DD-MMM")}
          </div>
          <div className="text-gray-500 font-medium">
            {dayjs(d).format("HH:mm")}
          </div>
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
      render: (t) => (
        <span className="text-[10px] text-gray-700 font-medium">{t || "-"}</span>
      ),
    },
    {
      title: "Commodity",
      dataIndex: "commodityName",
      width: 90,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Supplier",
      dataIndex: "supplierName",
      width: 85,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Transporter",
      dataIndex: "transporterName",
      width: 90,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Customer",
      dataIndex: "customerName",
      width: 85,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Origin",
      dataIndex: "originName",
      width: 80,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Destination",
      dataIndex: "destinationName",
      width: 90,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Weighbridge",
      dataIndex: "weighBridgeName",
      width: 95,
      render: (t) => (
        <span className="text-[10px] text-gray-600">{t || "-"}</span>
      ),
    },
    {
      title: "Mode",
      dataIndex: "weighMode",
      width: 70,
      render: (t) => (
        <span className="text-[10px] font-semibold text-gray-700">
          {t || "N/A"}
        </span>
      ),
    },
    {
      title: "Operator",
      dataIndex: "operatorName",
      width: 85,
      render: (text, record) => {
        const name = text || record.firstWeightOperator || "N/A";
        return (
          <span className="text-[10px] text-gray-700 font-medium">{name}</span>
        );
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
            <span className="text-[8px] text-amber-500 uppercase font-semibold">
              kg
            </span>
          )}
        </div>
      ),
    },
    {
      title: <ClockCircleOutlined style={{ fontSize: "10px" }} />,
      width: 55,
      render: (_, record) => {
        const hasSecondWeight =
          record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted =
          hasSecondWeight ||
          record.status === "Completed" ||
          record.status === "completed";

        const timeData = isCompleted
          ? calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime)
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
        const hasSecondWeight =
          record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted =
          hasSecondWeight || s === "Completed" || s === "completed";
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
              <svg
                className="w-4 h-4 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2.5}
                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
            <div>
              <div className="text-[11px] font-bold text-gray-900 leading-tight">
                Transactions
              </div>
              <div className="text-[9px] text-amber-700 font-medium leading-tight">
                <span className="font-semibold">
                  {filteredTransactions.length}
                </span>{" "}
                of <span className="font-semibold">{total || 0}</span>
              </div>
            </div>
          </div>

          <div className="flex gap-2 items-center">
            <Input
              allowClear
              placeholder="Search..."
              prefix={<Search size={10} className="text-gray-400" />}
              className="w-52 h-7 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
              value={filters.search}
              onChange={(e) =>
                setFilters({ ...filters, search: e.target.value, page: 1 })
              }
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
                  ? "bg-gradient-to-r from-amber-500 to-orange-600 text-white border-amber-500 hover:from-amber-600 hover:to-orange-700"
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
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Date Range
              </label>
              <RangePicker
                className="w-full h-6 text-[10px] border-amber-300 focus:border-amber-500"
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
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Start Time
              </label>
              <input
                type="time"
                value={filters.startTime}
                onChange={(e) =>
                  setFilters({ ...filters, startTime: e.target.value, page: 1 })
                }
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              />
            </div>

            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                End Time
              </label>
              <input
                type="time"
                value={filters.endTime}
                onChange={(e) =>
                  setFilters({ ...filters, endTime: e.target.value, page: 1 })
                }
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              />
            </div>

            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Status
              </label>
              <select
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                value={filters.status || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    status: e.target.value || null,
                    page: 1,
                  })
                }
              >
                <option value="">All</option>
                <option value="completed">Completed</option>
                <option value="inprogress">In Progress</option>
              </select>
            </div>

            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Vehicle
              </label>
              <Input
                placeholder="Vehicle..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.vehicle || ""}
                onChange={(e) =>
                  setFilters({ ...filters, vehicle: e.target.value, page: 1 })
                }
                allowClear
              />
            </div>
          </div>

          {/* Row 2 */}
          <div className="grid grid-cols-6 gap-2 mb-2">
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Driver
              </label>
              <Input
                placeholder="Driver..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.driver || ""}
                onChange={(e) =>
                  setFilters({ ...filters, driver: e.target.value, page: 1 })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Commodity
              </label>
              <Input
                placeholder="Commodity..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.commodity || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    commodity: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Supplier
              </label>
              <Input
                placeholder="Supplier..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.supplier || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    supplier: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Transporter
              </label>
              <Input
                placeholder="Transporter..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.transporter || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    transporter: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Customer
              </label>
              <Input
                placeholder="Customer..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.customer || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    customer: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Operator
              </label>
              <Input
                placeholder="Operator..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.operator || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    operator: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
          </div>

          {/* Row 3 */}
          <div className="grid grid-cols-6 gap-2">
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Origin
              </label>
              <Input
                placeholder="Origin..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.origin || ""}
                onChange={(e) =>
                  setFilters({ ...filters, origin: e.target.value, page: 1 })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Destination
              </label>
              <Input
                placeholder="Destination..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.destination || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    destination: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Weighbridge
              </label>
              <Input
                placeholder="Weighbridge..."
                className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                value={filters.weighbridge || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    weighbridge: e.target.value,
                    page: 1,
                  })
                }
                allowClear
              />
            </div>
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" />
                Mode
              </label>
              <select
                className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                value={filters.weighMode || ""}
                onChange={(e) =>
                  setFilters({
                    ...filters,
                    weighMode: e.target.value || null,
                    page: 1,
                  })
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

      {/* Compact Table */}
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
              onChange: (p, ps) =>
                setFilters({ ...filters, page: p, pageSize: ps }),
            }}
            rowClassName={(record) => {
              const hasSecondWeight =
                record.secondWeight && parseFloat(record.secondWeight) > 0;
              const isCompleted =
                hasSecondWeight ||
                record.status === "Completed" ||
                record.status === "completed";
              return isCompleted ? "completed-row" : "incomplete-row";
            }}
          />
        </div>
      </div>

      {/* Drawer - unchanged */}
      <Drawer
        title={
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
              <span className="text-white text-xs font-bold">📋</span>
            </div>
            <span className="text-sm font-bold text-gray-900">
              Ticket: {selectedRecord?.receiptNo}
            </span>
          </div>
        }
        placement="right"
        onClose={() => setIsDrawerOpen(false)}
        open={isDrawerOpen}
        width={450}
        footer={
          <div className="flex gap-2 justify-between items-center">
            <div className="flex gap-2 items-center">
              <span className="text-xs font-semibold text-gray-600">
                Print Mode:
              </span>
              <Radio.Group
                size="small"
                value={printMode}
                onChange={(e) => setPrintMode(e.target.value)}
              >
                <Radio.Button value="color" className="text-xs">
                  Color
                </Radio.Button>
                <Radio.Button value="bw" className="text-xs">
                  B&W
                </Radio.Button>
              </Radio.Group>
            </div>
            <div className="flex gap-2">
              {!isEditing ? (
                <>
                  <Button
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => setIsEditing(true)}
                    className="text-xs border-amber-300 text-amber-600 hover:border-amber-500 hover:text-amber-700"
                  >
                    Edit
                  </Button>
                  <Button
                    size="small"
                    type="primary"
                    icon={<Printer size={14} />}
                    onClick={() =>
                      generatePDF(selectedRecord, printMode === "color")
                    }
                    className="text-xs bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0"
                  >
                    Export PDF
                  </Button>
                </>
              ) : (
                <>
                  <Button
                    size="small"
                    icon={<CloseOutlined />}
                    onClick={() => {
                      setEditedRecord(selectedRecord);
                      setIsEditing(false);
                    }}
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
                    className="text-xs bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0"
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
                <span className="text-sm font-bold text-amber-900">
                  BASIC INFORMATION
                </span>
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
                  <div
                    key={field}
                    className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200"
                  >
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">
                      {label}
                    </div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) =>
                          setEditedRecord({
                            ...editedRecord,
                            [field]: e.target.value,
                          })
                        }
                        size="small"
                        className="text-xs border-amber-300 focus:border-amber-500"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">
                        {selectedRecord[field] || "N/A"}
                      </div>
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
                  <div
                    key={field}
                    className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200"
                  >
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">
                      {label}
                    </div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) =>
                          setEditedRecord({
                            ...editedRecord,
                            [field]: e.target.value,
                          })
                        }
                        size="small"
                        className="text-xs border-amber-300 focus:border-amber-500"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">
                        {selectedRecord[field] || "N/A"}
                      </div>
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
                <span className="text-sm font-bold text-amber-900">
                  LOCATIONS
                </span>
              </div>
              <div className="grid grid-cols-2 gap-2.5">
                {[
                  { label: "Origin", field: "originName", editable: true },
                  { label: "Destination", field: "destinationName", editable: true },
                  { label: "Weighbridge", field: "weighBridgeName", editable: false },
                  { label: "Operation", field: "operation", editable: false },
                ].map(({ label, field, editable }) => (
                  <div
                    key={field}
                    className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200"
                  >
                    <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">
                      {label}
                    </div>
                    {isEditing && editable ? (
                      <Input
                        value={editedRecord[field]}
                        onChange={(e) =>
                          setEditedRecord({
                            ...editedRecord,
                            [field]: e.target.value,
                          })
                        }
                        size="small"
                        className="text-xs border-amber-300 focus:border-amber-500"
                      />
                    ) : (
                      <div className="font-bold text-gray-900 text-xs">
                        {selectedRecord[field] || "N/A"}
                      </div>
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
                <span className="text-sm font-bold text-amber-900">
                  WEIGHT SUMMARY
                </span>
              </div>
              <div className="grid grid-cols-3 gap-2.5 mb-3">
                <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                  <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">
                    1st Weight
                  </div>
                  <div className="text-base font-extrabold text-orange-600">
                    {selectedRecord.firstWeight || 0}
                  </div>
                  <div className="text-[9px] text-amber-600 font-semibold">
                    KILOGRAMS
                  </div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.firstWeightTime
                      ? dayjs(selectedRecord.firstWeightTime).format(
                          "DD-MM-YY HH:mm"
                        )
                      : "N/A"}
                  </div>
                </div>
                <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                  <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">
                    2nd Weight
                  </div>
                  <div className="text-base font-extrabold text-green-600">
                    {selectedRecord.secondWeight || 0}
                  </div>
                  <div className="text-[9px] text-amber-600 font-semibold">
                    KILOGRAMS
                  </div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.secondWeightTime
                      ? dayjs(selectedRecord.secondWeightTime).format(
                          "DD-MM-YY HH:mm"
                        )
                      : "N/A"}
                  </div>
                </div>
                <div className="bg-gradient-to-br from-amber-200 via-amber-300 to-orange-300 rounded-lg p-2.5 border-2 border-amber-500 shadow-lg">
                  <div className="text-[10px] text-amber-900 font-extrabold uppercase mb-1">
                    Net Weight
                  </div>
                  <div className="text-lg font-black text-amber-950">
                    {selectedRecord.netWeight || 0}
                  </div>
                  <div className="text-[9px] text-amber-800 font-bold">
                    KILOGRAMS
                  </div>
                </div>
              </div>
              <div className="bg-white rounded-lg px-3 py-2.5 border-2 border-amber-300 flex items-center justify-between">
                <span className="text-[11px] text-amber-800 font-bold">
                  ⏱️ TURNAROUND TIME:
                </span>
                <span className="text-sm font-black text-amber-900 bg-amber-100 px-3 py-1 rounded-full">
                  {formatTurnaroundTimeSimple(
                    selectedRecord.firstWeightTime,
                    selectedRecord.secondWeightTime
                  )}
                </span>
              </div>
            </div>

            {/* Status & Remarks */}
            <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
              <div className="flex items-center gap-2 mb-3">
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                  <span className="text-white text-sm font-bold">📝</span>
                </div>
                <span className="text-sm font-bold text-amber-900">
                  STATUS & REMARKS
                </span>
              </div>
              <div className="mb-3">
                <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">
                  Status
                </div>
                <Tag
                  color={
                    (selectedRecord.secondWeight &&
                      parseFloat(selectedRecord.secondWeight) > 0) ||
                    selectedRecord.status === "Completed"
                      ? "success"
                      : "warning"
                  }
                  className="text-xs font-bold px-3 py-1 shadow-sm"
                >
                  {(selectedRecord.secondWeight &&
                    parseFloat(selectedRecord.secondWeight) > 0) ||
                  selectedRecord.status === "Completed"
                    ? "✅ COMPLETED"
                    : "⏳ IN PROGRESS"}
                </Tag>
              </div>
              <div>
                <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">
                  Remarks / Notes
                </div>
                {isEditing ? (
                  <Input.TextArea
                    value={editedRecord.remarks || editedRecord.notes}
                    onChange={(e) =>
                      setEditedRecord({
                        ...editedRecord,
                        remarks: e.target.value,
                      })
                    }
                    rows={3}
                    className="text-xs border-amber-300 focus:border-amber-500"
                  />
                ) : (
                  <div className="text-gray-900 bg-white p-2.5 rounded-lg border-2 border-amber-200 text-xs font-medium">
                    {selectedRecord.remarks ||
                      selectedRecord.notes ||
                      "💭 No remarks available"}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </Drawer>

      <style>{`
        .compact-table .ant-table { font-size: 10px; }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, #fffbeb, #fef3c7) !important;
          border-bottom: 1.5px solid #f59e0b !important;
          padding: 5px 8px !important;
          font-weight: 700 !important;
          font-size: 9px !important;
          color: #78350f !important;
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
          background: #fffbeb !important;
          box-shadow: inset 0 0 0 1px #fef3c7;
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
          transition: all 0.2s ease !important;
          margin: 0 2px !important;
        }
        .compact-table .ant-pagination-item-active {
          background: linear-gradient(135deg, #f59e0b, #f97316) !important;
          border-color: #f59e0b !important;
          box-shadow: 0 1px 3px rgba(245, 158, 11, 0.25) !important;
        }
        .compact-table .ant-pagination-item-active a {
          color: white !important; font-weight: 700 !important;
        }
        .compact-table .ant-pagination-item:hover { 
          border-color: #f59e0b !important; 
        }
        .compact-table .ant-select-selector { 
          height: 24px !important; 
          padding: 0 8px !important; 
        }
        .compact-table .ant-select-selection-item { 
          line-height: 22px !important; 
          font-size: 11px !important; 
        }
        .compact-table .ant-pagination-options {
          margin-left: 8px !important;
        }
        .compact-table .ant-pagination-total-text {
          font-size: 11px !important;
          line-height: 24px !important;
        }
      `}</style>
    </div>
  );
}