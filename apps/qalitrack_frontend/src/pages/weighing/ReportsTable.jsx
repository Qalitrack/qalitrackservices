import { useState, useMemo } from "react";
import { FileDown, FileSpreadsheet, ArrowUpDown, ArrowUp, ArrowDown, Eye } from "lucide-react";
import dayjs from "dayjs";

export default function ReportsTable({
  transactions = [],
  loading = false,
  currentPage = 1,
  pageSize = 4, // default 4 rows per page
  totalRecords = 0,
  onPageChange = () => {},
  onPageSizeChange = () => {},
  onExportPDF,
  onExportExcel,
  showColumns = null,
  onRowClick = null,
  onPreview = null, // added preview callback
}) {
  const [sortField, setSortField] = useState(null);
  const [sortOrder, setSortOrder] = useState(null);

  const totalPages = Math.ceil(totalRecords / pageSize);

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
    if (sortField !== field) return <ArrowUpDown className="w-4 h-4 inline ml-1 opacity-30" />;

    return sortOrder === "asc" ? (
      <ArrowUp className="w-4 h-4 inline ml-1 text-amber-400" />
    ) : (
      <ArrowDown className="w-4 h-4 inline ml-1 text-amber-400" />
    );
  };

  // =========================
  // Process data with sorting & pagination
  // =========================
  const processedData = useMemo(() => {
    let data = [...transactions];

    if (sortField && sortOrder) {
      data.sort((a, b) => {
        const aVal = a?.[sortField] ?? "";
        const bVal = b?.[sortField] ?? "";

        if (typeof aVal === "number") return sortOrder === "asc" ? aVal - bVal : bVal - aVal;
        return sortOrder === "asc"
          ? String(aVal).localeCompare(String(bVal))
          : String(bVal).localeCompare(String(aVal));
      });
    }

    const start = (currentPage - 1) * pageSize;
    return data.slice(start, start + pageSize);
  }, [transactions, sortField, sortOrder, currentPage, pageSize]);

  const handleExportPDF = () => onExportPDF?.(processedData);
  const handleExportExcel = () => onExportExcel?.(processedData);

  // =========================
  // Badges
  // =========================
  const badge = (label, cls) => (
    <span className={`px-2 py-1 text-xs rounded border ${cls}`}>{label || "N/A"}</span>
  );

  const statusBadge = (status) => {
    const map = {
      Completed: "bg-green-100 text-green-800 border-green-200",
      Pending: "bg-yellow-100 text-yellow-800 border-yellow-200",
      "In Progress": "bg-blue-100 text-blue-800 border-blue-200",
    };
    return badge(status, map[status] || map.Pending);
  };

  const weighModeBadge = (mode) => {
    const map = {
      inbound: "bg-purple-100 text-purple-800 border-purple-200",
      outbound: "bg-orange-100 text-orange-800 border-orange-200",
      single: "bg-gray-100 text-gray-800 border-gray-200",
    };
    return badge(mode, map[mode] || map.single);
  };

  // =========================
  // Loading & Empty states
  // =========================
  const renderLoading = () =>
    Array.from({ length: pageSize }).map((_, i) => (
      <tr key={i} className="animate-pulse">
        {Array.from({ length: showColumns?.length || 13 }).map((_, j) => (
          <td key={j} className="p-2 sm:p-3">
            <div className="h-4 bg-gray-200 rounded" />
          </td>
        ))}
      </tr>
    ));

  const renderEmpty = () => (
    <tr>
      <td colSpan={showColumns?.length || 13} className="h-64 text-center text-gray-500">
        <FileDown className="w-12 h-12 mx-auto mb-4 opacity-30" />
        <p className="text-lg">No records found</p>
        <p className="text-sm mt-2">Adjust filters and try again</p>
      </td>
    </tr>
  );

  // =========================
  // Columns: dynamically map showColumns to labels
  // =========================
  const defaultColumns = [
    ["createdAt", "Date & Time"],
    ["receiptNo", "Receipt"],
    ["noPlate", "Vehicle"],
    ["driverName", "Driver"],
    ["transporterName", "Transporter"],
    ["originName", "Source"],
    ["destinationName", "Destination"],
    ["firstWeight", "First Wt (kg)"],
    ["secondWeight", "Second Wt (kg)"],
    ["netWeight", "Net Wt (kg)"],
    ["weighMode", "Mode"],
    ["status", "Status"],
    ["actions", "Actions"], // <- Preview button column
  ];

  const columnsToRender = showColumns
    ? showColumns.map((key) => {
        const found = defaultColumns.find(([k]) => k === key);
        return [key, found ? found[1] : key];
      })
    : defaultColumns;

  // =========================
  // Render
  // =========================
  return (
    <div className="bg-white rounded-lg border shadow-sm">
      {/* Header */}
      <div className="p-4 sm:p-6 border-b flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <div>
          <h2 className="text-xl font-semibold">Reports</h2>
          <p className="text-sm text-gray-600">
            Showing {processedData.length} of {totalRecords} records
          </p>
        </div>

        <div className="flex flex-col sm:flex-row gap-2 sm:gap-3 w-full sm:w-auto">
          <button
            onClick={handleExportPDF}
            disabled={!processedData.length}
            className="border px-4 py-2 rounded disabled:opacity-50 w-full sm:w-auto"
          >
            <FileDown className="inline w-4 h-4 mr-2" />
            PDF
          </button>

          <button
            onClick={handleExportExcel}
            disabled={!processedData.length}
            className="bg-amber-400 px-4 py-2 rounded disabled:opacity-50 w-full sm:w-auto"
          >
            <FileSpreadsheet className="inline w-4 h-4 mr-2" />
            Excel
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto -mx-4 sm:mx-0">
        <table className="w-full min-w-[600px] text-sm">
          <thead className="bg-gray-50">
            <tr>
              {columnsToRender.map(([key, label]) => (
                <th
                  key={key}
                  onClick={() => key !== "actions" && handleSort(key)}
                  className="p-2 sm:p-3 cursor-pointer text-left whitespace-nowrap"
                >
                  {label} {key !== "actions" && getSortIcon(key)}
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {loading
              ? renderLoading()
              : processedData.length === 0
              ? renderEmpty()
              : processedData.map((t) => (
                  <tr
                    key={t.id}
                    className="border-t hover:bg-gray-50"
                    onClick={() => onRowClick?.(t)}
                  >
                    {columnsToRender.map(([key]) => {
                      if (key === "createdAt")
                        return (
                          <td key={key} className="p-2 sm:p-3">
                            {t[key] ? dayjs(t[key]).format("DD MMM YYYY HH:mm") : "-"}
                          </td>
                        );
                      if (key === "netWeight" || key === "firstWeight" || key === "secondWeight")
                        return (
                          <td key={key} className="p-2 sm:p-3 text-right">
                            {t[key]?.toLocaleString() || "-"}
                          </td>
                        );
                      if (key === "status") return <td key={key} className="p-2 sm:p-3">{statusBadge(t[key])}</td>;
                      if (key === "weighMode") return <td key={key} className="p-2 sm:p-3">{weighModeBadge(t[key])}</td>;
                      if (key === "actions")
                        return (
                          <td key={key} className="p-2 sm:p-3">
                            <button
                              onClick={() => onPreview?.(t)}
                              className="px-2 py-1 bg-blue-100 text-blue-800 rounded hover:bg-blue-200 flex items-center gap-1"
                            >
                              <Eye className="w-4 h-4" /> Preview
                            </button>
                          </td>
                        );
                      return <td key={key} className="p-2 sm:p-3">{t[key] ?? "-"}</td>;
                    })}
                  </tr>
                ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="p-4 sm:p-6 border-t flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 sm:gap-0">
        <div className="flex items-center gap-2 w-full sm:w-auto">
          <span className="text-sm">Rows:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border rounded px-2 py-1"
          >
            {[4, 10, 25, 50].map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-2 w-full sm:w-auto justify-start sm:justify-end">
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="border px-3 py-1 rounded disabled:opacity-50"
          >
            Prev
          </button>
          <span className="text-sm">
            Page {currentPage} of {totalPages}
          </span>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="border px-3 py-1 rounded disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
