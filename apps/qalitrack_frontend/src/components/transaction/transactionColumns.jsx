import React from "react";
import { Tag, Button } from "antd";
import { ClockCircleOutlined, RetweetOutlined } from "@ant-design/icons";
import { Eye } from "lucide-react";
import dayjs from "dayjs";

export const calculateTurnaroundTime = (firstWeightDate, secondWeightDate, turnaroundTime) => {
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

export const calculateWaitTime = (createdAt) => {
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

export const getTimeColor = (minutes, isCompleted) => {
  if (isCompleted) {
    if (minutes < 30) return "green";
    if (minutes < 60) return "blue";
    if (minutes < 120) return "orange";
    return "red";
  }
  if (minutes < 30) return "green";
  if (minutes < 60) return "orange";
  return "red";
};

export const formatTurnaroundTimeSimple = (firstWeightDate, secondWeightDate, turnaroundTime) => {
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

export const getTransactionColumns = ({ filters, openViewDrawer, setReweighModal }) => [
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
      <span className="text-[10px] font-bold text-amber-600">
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
        {w && <span className="text-[8px] text-amber-500 uppercase font-semibold">kg</span>}
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
    width: 80,
    fixed: "right",
    render: (_, r) => {
      const status = r.status?.toLowerCase();
      const isReweighable =
        status === "completed" ||
        status === "reweighrequested" ||
        (r.secondWeight && parseFloat(r.secondWeight) > 0);
      return (
        <div className="flex items-center gap-1">
          <Button
            size="small"
            type="text"
            icon={<Eye size={12} />}
            onClick={() => openViewDrawer(r)}
            className="text-amber-600 hover:bg-amber-50 hover:text-amber-700 h-6 px-1.5 text-[10px] font-semibold transition-all"
          />
          {isReweighable && (
            <Button
              size="small"
              type="text"
              icon={<RetweetOutlined style={{ fontSize: 11 }} />}
              onClick={() => setReweighModal({ visible: true, transaction: r })}
              className="text-blue-500 hover:bg-blue-50 hover:text-blue-700 h-6 px-1.5 text-[10px] font-semibold transition-all"
              title="Reweigh"
            />
          )}
        </div>
      );
    },
  },
];
