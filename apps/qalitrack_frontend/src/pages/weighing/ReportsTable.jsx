import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, FileDown } from "lucide-react";
import dayjs from "dayjs";

export default function ReportsTable({
  transactions = [],
  loading = false,
  currentPage = 1,
  pageSize = 6,
  totalRecords = 0,
  onPageChange = () => {},
  onPageSizeChange = () => {},
  showColumns = null,
}) {
  const [sortField, setSortField] = useState(null);
  const [sortOrder, setSortOrder] = useState(null);

  const totalPages = Math.ceil(totalRecords / pageSize);

  // ── Time helpers ───────────────────────────────────────────────────────────
  const calcTAT = (a, b, turnaroundTime) => {
    // Use pre-calculated turnaroundTime from API if available (format: "HH:MM:SS.fffffff")
    if (turnaroundTime) {
      const parts = turnaroundTime.split(":");
      if (parts.length >= 2) {
        const h = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10);
        const totalMinutes = h * 60 + m;
        if (totalMinutes < 1) return { display: "< 1m", minutes: 0 };
        if (h > 0) return { display: m ? `${h}h ${m}m` : `${h}h`, minutes: totalMinutes };
        return { display: `${m}m`, minutes: totalMinutes };
      }
    }
    // Fallback: calculate from timestamps using API field names
    const start = a || null;
    const end = b || null;
    if (!start || !end) return { display: "N/A", minutes: 0 };
    const diff = dayjs(end).diff(dayjs(start), "minute");
    if (diff < 1) return { display: "< 1m", minutes: 0 };
    if (diff < 60) return { display: `${diff}m`, minutes: diff };
    const h = Math.floor(diff / 60), m = diff % 60;
    return { display: m ? `${h}h ${m}m` : `${h}h`, minutes: diff };
  };

  const calcWait = (created) => {
    if (!created) return { display: "-", minutes: 0 };
    const diff = dayjs().diff(dayjs(created), "minute");
    if (diff < 1) return { display: "< 1m", minutes: 0 };
    if (diff < 60) return { display: `${diff}m`, minutes: diff };
    const h = Math.floor(diff / 60), m = diff % 60;
    return { display: m ? `${h}h ${m}m` : `${h}h`, minutes: diff };
  };

  // ── Sorting ────────────────────────────────────────────────────────────────
  const handleSort = (field) => {
    if (field === "turnaround" || field === "status") return;
    if (sortField === field) {
      if (sortOrder === "asc") setSortOrder("desc");
      else if (sortOrder === "desc") { setSortField(null); setSortOrder(null); }
      else setSortOrder("asc");
    } else { setSortField(field); setSortOrder("asc"); }
  };

  const getSortIcon = (field) => {
    if (field === "turnaround" || field === "status") return null;
    if (sortField !== field)
      return <ArrowUpDown className="w-2.5 h-2.5 inline ml-0.5 opacity-30 shrink-0" />;
    return sortOrder === "asc"
      ? <ArrowUp   className="w-2.5 h-2.5 inline ml-0.5 text-amber-600 shrink-0" />
      : <ArrowDown className="w-2.5 h-2.5 inline ml-0.5 text-amber-600 shrink-0" />;
  };

  // ── Data processing ────────────────────────────────────────────────────────
  const sortedFull = useMemo(() => {
    let data = [...transactions];
    if (sortField && sortOrder) {
      data.sort((a, b) => {
        const av = a?.[sortField] ?? "", bv = b?.[sortField] ?? "";
        if (typeof av === "number") return sortOrder === "asc" ? av - bv : bv - av;
        return sortOrder === "asc"
          ? String(av).localeCompare(String(bv))
          : String(bv).localeCompare(String(av));
      });
    }
    return data;
  }, [transactions, sortField, sortOrder]);

  const processedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return sortedFull.slice(start, start + pageSize);
  }, [sortedFull, currentPage, pageSize]);

  const getRowNumber = (record) => sortedFull.findIndex(r => r.id === record.id) + 1;

  const isCompleted = (r) =>
    (r?.secondWeight && parseFloat(r.secondWeight) > 0) ||
    r?.status === "Completed" || r?.status === "completed";

  // ── Column definitions ─────────────────────────────────────────────────────
  // Widths expressed as percentages — browser fills available space.
  // Text columns truncate with title tooltip so nothing is hidden permanently.
  const allColumns = [
    { key: "createdAt",       label: "Date",     w: "5%",   align: "left"   },
    { key: "receiptNo",       label: "Receipt",  w: "6%",   align: "left"   },
    { key: "noPlate",         label: "Vehicle",  w: "5.5%", align: "center" },
    { key: "driverName",      label: "Driver",   w: "6%",   align: "left"   },
    { key: "commodityName",   label: "Commodity",w: "6%",   align: "left"   },
    { key: "supplierName",    label: "Supplier", w: "6%",   align: "left"   },
    { key: "transporterName", label: "Trans.",   w: "6%",   align: "left"   },
    { key: "customerName",    label: "Customer", w: "6%",   align: "left"   },
    { key: "originName",      label: "Origin",   w: "5%",   align: "left"   },
    { key: "destinationName", label: "Dest.",    w: "5%",   align: "left"   },
    { key: "weighBridgeName", label: "W.Bridge", w: "5.5%", align: "left"   },
    { key: "weighMode",       label: "Mode",     w: "4%",   align: "center" },
    { key: "operation",       label: "Op.",      w: "4%",   align: "center" },
    { key: "operatorName",    label: "Operator", w: "5%",   align: "left"   },
    { key: "firstWeight",     label: "1st",      w: "4.5%", align: "right"  },
    { key: "secondWeight",    label: "2nd",      w: "4.5%", align: "right"  },
    { key: "netWeight",       label: "Net",      w: "4.5%", align: "right"  },
    { key: "turnaround",      label: "⏱",       w: "4%",   align: "center" },
    { key: "status",          label: "St.",      w: "3.5%", align: "center" },
  ];

  const columnsToRender = showColumns
    ? showColumns.map(key => allColumns.find(c => c.key === key) || { key, label: key, w: "5%", align: "left" })
    : allColumns;

  // ── Cell renderer ──────────────────────────────────────────────────────────
  const renderCell = (col, record, rowNum) => {
    const { key } = col;
    const value = record[key];

    if (key === "__rowNum__") {
      return (
        <span className="inline-flex items-center justify-center w-5 h-5 rounded-full bg-gradient-to-br from-amber-100 to-amber-200 text-[9px] font-extrabold text-amber-900 border border-amber-300 shadow-sm">
          {rowNum}
        </span>
      );
    }

    switch (key) {
      case "createdAt":
        return (
          <div className="leading-tight">
            <div className="font-semibold text-gray-800 text-[9px]">
              {value ? dayjs(value).format("DD-MMM") : "-"}
            </div>
            <div className="text-gray-500 text-[8px]">
              {value ? dayjs(value).format("HH:mm") : ""}
            </div>
          </div>
        );

      case "receiptNo":
        return (
          <span
            title={value || ""}
            className="font-mono font-bold text-amber-600 bg-amber-50 px-1 py-0.5 rounded text-[9px] block truncate"
          >
            {value || "-"}
          </span>
        );

      case "noPlate":
        return (
          <span
            title={value || ""}
            className="block truncate bg-gray-900 text-white px-1 py-0.5 rounded text-[9px] font-bold text-center"
          >
            {value || "-"}
          </span>
        );

      case "firstWeight":
        return (
          <div className="flex flex-col items-end leading-tight">
            <span className="text-[10px] font-bold text-orange-600">
              {value ? parseFloat(value).toLocaleString() : "-"}
            </span>
            {value && <span className="text-[8px] text-amber-500 font-semibold">kg</span>}
          </div>
        );

      case "secondWeight":
        return (
          <div className="flex flex-col items-end leading-tight">
            <span className="text-[10px] font-bold text-green-600">
              {value ? parseFloat(value).toLocaleString() : "-"}
            </span>
            {value && <span className="text-[8px] text-green-500 font-semibold">kg</span>}
          </div>
        );

      case "netWeight":
        return (
          <div className="flex flex-col items-end leading-tight">
            <span className="text-[10px] font-extrabold text-amber-600">
              {value ? parseFloat(value).toLocaleString() : "-"}
            </span>
            {value && <span className="text-[8px] text-amber-500 font-semibold">kg</span>}
          </div>
        );

      case "status": {
        const done = isCompleted(record);
        return (
          <span className={`inline-flex items-center justify-center w-5 h-5 rounded-full text-[9px] font-bold border shadow-sm mx-auto ${
            done
              ? "bg-green-100 text-green-800 border-green-300"
              : "bg-amber-50 text-amber-800 border-amber-200"
          }`}>
            {done ? "✓" : "⏳"}
          </span>
        );
      }

      case "weighMode":
        return (
          <span
            title={value || ""}
            className="block truncate px-1 py-0.5 text-[8px] font-semibold rounded border bg-amber-50 text-amber-800 border-amber-200 uppercase text-center"
          >
            {value || "N/A"}
          </span>
        );

      case "operation":
        return (
          <span
            title={value || ""}
            className="block truncate px-1 py-0.5 text-[8px] font-semibold rounded border bg-amber-100 text-amber-900 border-amber-300 uppercase text-center"
          >
            {value || "N/A"}
          </span>
        );

      case "turnaround": {
        const done = isCompleted(record);
        const td = done
          ? calcTAT(record.firstWeightDate, record.secondWeightDate, record.turnaroundTime)
          : calcWait(record.createdAt);
        let cls;
        if (done) {
          if (td.minutes < 30)  cls = "bg-green-100 text-green-700 border-green-300";
          else if (td.minutes < 60)  cls = "bg-blue-100 text-blue-700 border-blue-200";
          else if (td.minutes < 120) cls = "bg-orange-100 text-orange-700 border-orange-300";
          else cls = "bg-red-100 text-red-700 border-red-300";
        } else {
          if (td.minutes < 30) cls = "bg-green-100 text-green-700 border-green-300";
          else if (td.minutes < 60) cls = "bg-orange-100 text-orange-700 border-orange-300";
          else cls = "bg-red-100 text-red-700 border-red-300";
        }
        return (
          <span className={`block truncate px-1 py-0.5 text-[8px] font-bold rounded border shadow-sm text-center ${cls}`}>
            {td.display}
          </span>
        );
      }

      case "operatorName":
        return (
          <span
            title={value || record.firstWeightOperator || ""}
            className="block truncate text-[9px] text-gray-700 font-medium"
          >
            {value || record.firstWeightOperator || "N/A"}
          </span>
        );

      default:
        return (
          <span
            title={String(value || "")}
            className="block truncate text-[9px] text-gray-600"
          >
            {value || "-"}
          </span>
        );
    }
  };

  // ── Loading / empty ────────────────────────────────────────────────────────
  const colCount = columnsToRender.length + 1;

  const renderLoading = () =>
    Array.from({ length: pageSize }).map((_, i) => (
      <tr key={i} className="animate-pulse">
        {Array.from({ length: colCount }).map((_, j) => (
          <td key={j} className="px-1.5 py-2">
            <div className="h-3 bg-gray-200 rounded" />
          </td>
        ))}
      </tr>
    ));

  const renderEmpty = () => (
    <tr key="empty">
      <td colSpan={colCount} className="h-48 text-center text-gray-500">
        <FileDown className="w-10 h-10 mx-auto mb-3 opacity-30" />
        <p className="text-sm font-semibold">No records found</p>
        <p className="text-xs mt-1">Adjust filters and try again</p>
      </td>
    </tr>
  );

  // ── Render ─────────────────────────────────────────────────────────────────
  return (
    <div className="bg-white rounded-lg border border-amber-200 shadow-sm overflow-hidden flex flex-col">

      {/* Header bar */}
      <div className="px-3 py-2 border-b bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b-amber-200 shrink-0">
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm shrink-0">
            <svg className="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h2 className="text-[11px] font-bold text-gray-900 leading-tight">Transactions Report</h2>
            <p className="text-[9px] text-amber-700 font-medium leading-tight">
              Showing <span className="font-semibold">{processedData.length}</span> of{" "}
              <span className="font-semibold">{totalRecords}</span> records
            </p>
          </div>
        </div>
      </div>

      {/* Table — table-fixed + w-full fills available space exactly, no overflow */}
      <div className="w-full overflow-hidden">
        <table
          className="w-full table-fixed border-collapse"
          style={{ fontSize: "0.7rem" }}
        >
          <colgroup>
            <col style={{ width: "2.5%" }} />
            {columnsToRender.map((col) => (
              <col key={col.key} style={{ width: col.w }} />
            ))}
          </colgroup>

          <thead className="bg-gradient-to-b from-amber-50 to-amber-100/50">
            <tr>
              <th className="px-1 py-1.5 text-center font-bold text-[8px] text-amber-900 uppercase tracking-wide border-b-2 border-amber-200 overflow-hidden">
                #
              </th>
              {columnsToRender.map((col) => (
                <th
                  key={col.key}
                  onClick={() => handleSort(col.key)}
                  style={{ textAlign: col.align === "right" ? "right" : col.align === "center" ? "center" : "left" }}
                  className={`px-1 py-1.5 font-bold text-[8px] text-amber-900 uppercase tracking-wide border-b-2 border-amber-200 overflow-hidden transition-colors hover:bg-amber-100 ${
                    col.key === "turnaround" || col.key === "status" ? "cursor-default" : "cursor-pointer"
                  }`}
                >
                  <span className="flex items-center gap-0.5 overflow-hidden"
                    style={{ justifyContent: col.align === "right" ? "flex-end" : col.align === "center" ? "center" : "flex-start" }}
                  >
                    <span className="truncate">{col.label}</span>
                    {getSortIcon(col.key)}
                  </span>
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {loading
              ? renderLoading()
              : processedData.length === 0
              ? renderEmpty()
              : processedData.map((t) => {
                  const done = isCompleted(t);
                  const rowNum = getRowNumber(t);
                  return (
                    <tr
                      key={t.ticketID || t.id}
                      className={`border-b border-gray-100 transition-colors ${
                        done
                          ? "bg-green-50/30 hover:bg-green-50/60"
                          : "bg-white hover:bg-amber-50/40"
                      }`}
                    >
                      <td className="px-1 py-1.5 text-center overflow-hidden">
                        {renderCell({ key: "__rowNum__" }, t, rowNum)}
                      </td>
                      {columnsToRender.map((col) => (
                        <td
                          key={col.key}
                          className="px-1 py-1.5 overflow-hidden"
                          style={{ textAlign: col.align === "right" ? "right" : col.align === "center" ? "center" : "left" }}
                        >
                          {renderCell(col, t, rowNum)}
                        </td>
                      ))}
                    </tr>
                  );
                })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="px-3 py-2 border-t bg-amber-50/50 flex flex-col sm:flex-row justify-between items-center gap-2 shrink-0">
        <div className="flex items-center gap-2">
          <span className="text-[10px] font-medium text-gray-700">Rows per page:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border border-amber-200 rounded-md px-1.5 text-[10px] focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300 bg-white h-6"
          >
            {[6, 10, 25, 50, 100].map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="border border-amber-200 px-3 h-6 rounded-md text-[10px] font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors bg-white"
          >
            Previous
          </button>
          <span className="text-[10px] font-medium text-gray-700">
            Page <span className="font-bold text-amber-700">{currentPage}</span> of{" "}
            <span className="font-bold">{totalPages || 1}</span>
          </span>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages || totalPages === 0}
            className="border border-amber-200 px-3 h-6 rounded-md text-[10px] font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors bg-white"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}