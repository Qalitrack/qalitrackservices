import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, FileDown } from "lucide-react";
import dayjs from "dayjs";

export default function ReportsTable({
  transactions = [],
  loading = false,
  currentPage = 1,
  pageSize = 4,
  totalRecords = 0,
  onPageChange = () => {},
  onPageSizeChange = () => {},
  showColumns = null,
}) {
  const [sortField, setSortField] = useState(null);
  const [sortOrder, setSortOrder] = useState(null);

  const totalPages = Math.ceil(totalRecords / pageSize);

  // =========================
  // Turnaround Time Calculation
  // =========================
  const calculateTurnaroundTime = (firstWeightTime, secondWeightTime) => {
    if (!firstWeightTime || !secondWeightTime) {
      return { display: "N/A", minutes: 0 };
    }
    
    const first = dayjs(firstWeightTime);
    const second = dayjs(secondWeightTime);
    const diffMinutes = second.diff(first, 'minute');
    
    if (diffMinutes < 1) {
      return { display: '< 1m', minutes: 0 };
    } else if (diffMinutes < 60) {
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    } else {
      const hours = Math.floor(diffMinutes / 60);
      const mins = diffMinutes % 60;
      return { 
        display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, 
        minutes: diffMinutes 
      };
    }
  };

  const calculateWaitTime = (createdAt) => {
    if (!createdAt) return { display: '-', minutes: 0 };
    
    const now = dayjs();
    const created = dayjs(createdAt);
    const diffMinutes = now.diff(created, 'minute');
    
    if (diffMinutes < 1) {
      return { display: '< 1m', minutes: 0 };
    } else if (diffMinutes < 60) {
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    } else {
      const hours = Math.floor(diffMinutes / 60);
      const mins = diffMinutes % 60;
      return { 
        display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, 
        minutes: diffMinutes 
      };
    }
  };

  // =========================
  // Sorting
  // =========================
  const handleSort = (field) => {
    if (sortField === field) {
      if (sortOrder === "asc") setSortOrder("desc");
      else if (sortOrder === "desc") {
        setSortField(null);
        setSortOrder(null);
      } else setSortOrder("asc");
    } else {
      setSortField(field);
      setSortOrder("asc");
    }
  };

  const getSortIcon = (field) => {
    if (sortField !== field)
      return <ArrowUpDown className="w-4 h-4 inline ml-1 opacity-30" />;

    return sortOrder === "asc" ? (
      <ArrowUp className="w-4 h-4 inline ml-1 text-amber-600" />
    ) : (
      <ArrowDown className="w-4 h-4 inline ml-1 text-amber-600" />
    );
  };

  // =========================
  // Sorting + Pagination
  // =========================
  const processedData = useMemo(() => {
    let data = [...transactions];

    if (sortField && sortOrder) {
      data.sort((a, b) => {
        const aVal = a?.[sortField] ?? "";
        const bVal = b?.[sortField] ?? "";

        if (typeof aVal === "number")
          return sortOrder === "asc" ? aVal - bVal : bVal - aVal;

        return sortOrder === "asc"
          ? String(aVal).localeCompare(String(bVal))
          : String(bVal).localeCompare(String(aVal));
      });
    }

    const start = (currentPage - 1) * pageSize;
    return data.slice(start, start + pageSize);
  }, [transactions, sortField, sortOrder, currentPage, pageSize]);

  // =========================
  // Badges
  // =========================
  const badge = (label, cls) => (
    <span className={`px-2 py-1 text-xs font-semibold rounded border ${cls}`}>
      {label || "N/A"}
    </span>
  );

  const statusBadge = (status, record) => {
    const hasSecondWeight = record?.secondWeight && parseFloat(record.secondWeight) > 0;
    const isCompleted = hasSecondWeight || status === 'Completed' || status === 'completed';
    
    return badge(
      isCompleted ? 'COMPLETED' : 'IN PROGRESS',
      isCompleted 
        ? "bg-green-100 text-green-800 border-green-300" 
        : "bg-amber-50 text-amber-800 border-amber-200"
    );
  };

  const weighModeBadge = (mode) => {
    return badge(mode?.toUpperCase(), "bg-amber-50 text-amber-800 border-amber-200");
  };

  const operationBadge = (operation) => {
    return badge(operation?.toUpperCase(), "bg-amber-100 text-amber-900 border-amber-300");
  };

  const turnaroundBadge = (record) => {
    const hasSecondWeight = record?.secondWeight && parseFloat(record.secondWeight) > 0;
    const isCompleted = hasSecondWeight || record?.status === 'Completed' || record?.status === 'completed';
    
    let timeData;
    if (isCompleted) {
      timeData = calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime);
    } else {
      timeData = calculateWaitTime(record.createdAt);
    }
    
    // Light amber shades only
    let colorClass;
    if (isCompleted) {
      if (timeData.minutes < 30) colorClass = "bg-amber-50 text-amber-700 border-amber-200";
      else if (timeData.minutes < 60) colorClass = "bg-amber-100 text-amber-800 border-amber-300";
      else if (timeData.minutes < 120) colorClass = "bg-amber-200 text-amber-900 border-amber-400";
      else colorClass = "bg-amber-300 text-amber-950 border-amber-500";
    } else {
      if (timeData.minutes < 30) colorClass = "bg-amber-50 text-amber-700 border-amber-200";
      else if (timeData.minutes < 60) colorClass = "bg-amber-200 text-amber-900 border-amber-400";
      else colorClass = "bg-amber-300 text-amber-950 border-amber-500";
    }
    
    return badge(timeData.display, colorClass);
  };

  // =========================
  // Loading & Empty states
  // =========================
  const renderLoading = () =>
    Array.from({ length: pageSize }).map((_, i) => (
      <tr key={i} className="animate-pulse">
        {Array.from({ length: showColumns?.length || 22 }).map((_, j) => (
          <td key={j} className="p-2 sm:p-3">
            <div className="h-4 bg-gray-200 rounded" />
          </td>
        ))}
      </tr>
    ));

  const renderEmpty = () => (
    <tr>
      <td colSpan={showColumns?.length || 22} className="h-64 text-center text-gray-500">
        <FileDown className="w-12 h-12 mx-auto mb-4 opacity-30" />
        <p className="text-lg font-semibold">No records found</p>
        <p className="text-sm mt-2">Adjust filters and try again</p>
      </td>
    </tr>
  );

  // =========================
  // All Available Columns
  // =========================
  const allColumns = [
    ["createdAt", "Date & Time"],
    ["receiptNo", "Receipt No."],
    ["noPlate", "Vehicle Reg."],
    ["driverName", "Driver"],
    ["commodityName", "Commodity"],
    ["supplierName", "Supplier"],
    ["transporterName", "Transporter"],
    ["customerName", "Customer"],
    ["originName", "Origin"],
    ["destinationName", "Destination"],
    ["weighBridgeName", "Weighbridge"],
    ["scaleName", "Scale"],
    ["weighMode", "Mode"],
    ["operation", "Operation"],
    ["operatorName", "Operator"],
    ["firstWeight", "First Weight (kg)"],
    ["secondWeight", "Second Weight (kg)"],
    ["netWeight", "Net Weight (kg)"],
    ["turnaround", "Turnaround"],
    ["status", "Status"],
    ["axleType", "Axle Type"],
    ["containerNo", "Container No."],
  ];

  const columnsToRender = showColumns
    ? showColumns.map((key) => {
        const found = allColumns.find(([k]) => k === key);
        return [key, found ? found[1] : key];
      })
    : allColumns;

  // =========================
  // Render Cell Value
  // =========================
  const renderCell = (key, record) => {
    const value = record[key];

    switch (key) {
      case "createdAt":
        return (
          <div className="leading-tight">
            <div className="text-[11px] font-semibold text-gray-800">
              {value ? dayjs(value).format('DD MMM YYYY') : '-'}
            </div>
            <div className="text-[10px] text-gray-500">
              {value ? dayjs(value).format('HH:mm:ss') : ''}
            </div>
          </div>
        );

      case "receiptNo":
        return <span className="text-[11px] font-mono font-semibold text-amber-700">{value || '-'}</span>;

      case "noPlate":
        return (
          <span className="text-[11px] font-bold tracking-wide text-gray-800">
            {value || '-'}
          </span>
        );

      case "firstWeight":
      case "secondWeight":
      case "netWeight":
        const colorClass = key === "netWeight" ? "text-amber-800 font-bold" : "text-amber-700";
        return (
          <div className="flex flex-col items-end leading-tight">
            <span className={`text-[11px] ${colorClass}`}>
              {value ? parseFloat(value).toLocaleString() : '-'}
            </span>
            <span className="text-[9px] text-gray-500 uppercase tracking-wide">kg</span>
          </div>
        );

      case "status":
        return statusBadge(value, record);

      case "weighMode":
        return weighModeBadge(value);

      case "operation":
        return operationBadge(value);

      case "turnaround":
        return turnaroundBadge(record);

      case "operatorName":
        const operatorName = value || record.firstWeightOperator || 'N/A';
        return (
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center text-white text-[10px] font-bold shadow-sm">
              {operatorName !== 'N/A' ? operatorName.charAt(0).toUpperCase() : '?'}
            </div>
            <span className="text-[11px] text-gray-700 font-medium">{operatorName}</span>
          </div>
        );

      default:
        return <span className="text-[11px] text-gray-700">{value || '-'}</span>;
    }
  };

  // =========================
  // Render
  // =========================
  return (
    <div className="bg-white rounded-lg border shadow-sm">
      <div className="p-3 border-b bg-amber-50">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-amber-100 flex items-center justify-center border border-amber-200">
            <svg className="w-4 h-4 text-amber-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h2 className="text-base font-bold text-gray-900">Transactions Report</h2>
            <p className="text-xs text-gray-600">
              Showing <span className="font-semibold text-amber-700">{processedData.length}</span> of <span className="font-semibold">{totalRecords}</span> records
            </p>
          </div>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[1800px] text-sm">
          <thead className="bg-amber-50 sticky top-0">
            <tr>
              {columnsToRender.map(([key, label]) => (
                <th
                  key={key}
                  onClick={() => handleSort(key)}
                  className={`p-2 cursor-pointer text-left whitespace-nowrap font-semibold text-[10px] text-gray-700 uppercase tracking-wider border-b border-amber-200 hover:bg-amber-100 transition-colors ${
                    key === "firstWeight" || key === "secondWeight" || key === "netWeight" ? "text-right" : ""
                  }`}
                >
                  {label} {getSortIcon(key)}
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {loading
              ? renderLoading()
              : processedData.length === 0
              ? renderEmpty()
              : processedData.map((t, idx) => (
                  <tr 
                    key={t.id} 
                    className={`border-b border-gray-100 hover:bg-amber-50/30 transition-colors ${
                      idx % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'
                    }`}
                  >
                    {columnsToRender.map(([key]) => (
                      <td 
                        key={key} 
                        className={`p-2 ${
                          key === "firstWeight" || key === "secondWeight" || key === "netWeight" ? "text-right" : ""
                        }`}
                      >
                        {renderCell(key, t)}
                      </td>
                    ))}
                  </tr>
                ))}
          </tbody>
        </table>
      </div>

      <div className="p-3 border-t bg-amber-50/50 flex flex-col sm:flex-row justify-between items-center gap-3">
        <div className="flex items-center gap-2">
          <span className="text-xs font-medium text-gray-700">Rows per page:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border border-amber-200 rounded-lg px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300 bg-white"
          >
            {[4, 10, 25, 50, 100].map((n) => (
              <option key={n} value={n}>
                {n}
              </option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="border border-amber-200 px-3 py-1 rounded-lg text-xs font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Previous
          </button>
          <span className="text-xs font-medium text-gray-700">
            Page <span className="font-bold text-amber-700">{currentPage}</span> of <span className="font-bold">{totalPages || 1}</span>
          </span>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages || totalPages === 0}
            className="border border-amber-200 px-3 py-1 rounded-lg text-xs font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}