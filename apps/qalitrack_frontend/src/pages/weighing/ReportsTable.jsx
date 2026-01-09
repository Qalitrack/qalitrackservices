import { useState, useMemo } from "react";
import {
  FileDown,
  FileSpreadsheet,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from "lucide-react";
import dayjs from "dayjs";

export default function ReportsTable({
  transactions = [],
  loading = false,
  currentPage = 1,
  pageSize = 5,
  totalRecords = 0,
  onPageChange = () => {},
  onExportPDF,
  onExportExcel,
  showColumns = null,
  onRowClick = null,
}) {
  const [sortField, setSortField] = useState(null);
  const [sortOrder, setSortOrder] = useState(null);

  const totalPages = Math.ceil(totalRecords / pageSize);

  // =========================
  // Sorting ONLY (no pagination here)
  // =========================
  const processedData = useMemo(() => {
    let data = [...transactions];

    if (sortField && sortOrder) {
      data.sort((a, b) => {
        const aVal = a?.[sortField] ?? "";
        const bVal = b?.[sortField] ?? "";

        if (typeof aVal === "number") {
          return sortOrder === "asc" ? aVal - bVal : bVal - aVal;
        }

        return sortOrder === "asc"
          ? String(aVal).localeCompare(String(bVal))
          : String(bVal).localeCompare(String(aVal));
      });
    }

    return data;
  }, [transactions, sortField, sortOrder]);

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
      <ArrowUp className="w-4 h-4 inline ml-1 text-amber-400" />
    ) : (
      <ArrowDown className="w-4 h-4 inline ml-1 text-amber-400" />
    );
  };

  // =========================
  // Badges
  // =========================
  const badge = (label, cls) => (
    <span className={`px-2 py-1 text-xs rounded border ${cls}`}>
      {label || "N/A"}
    </span>
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
  // Columns
  // =========================
  const defaultColumns = [
    ["createdAt", "Date"],
    ["receiptNo", "Receipt"],
    ["noPlate", "Vehicle"],
    ["driverName", "Driver"],
    ["originName", "Source"],
    ["destinationName", "Destination"],
    ["netWeight", "Net Wt (kg)"],
    ["weighMode", "Mode"],
    ["status", "Status"],
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
      <div className="p-4 sm:p-6 border-b flex flex-col sm:flex-row justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold">Reports</h2>
          <p className="text-sm text-gray-600">
            Showing {processedData.length} of {totalRecords} records
          </p>
        </div>

        <div className="flex gap-2">
          <button
            onClick={() => onExportPDF?.(processedData)}
            disabled={!processedData.length}
            className="border px-4 py-2 rounded disabled:opacity-50"
          >
            <FileDown className="inline w-4 h-4 mr-2" />
            PDF
          </button>

          <button
            onClick={() => onExportExcel?.(processedData)}
            disabled={!processedData.length}
            className="bg-amber-400 px-4 py-2 rounded disabled:opacity-50"
          >
            <FileSpreadsheet className="inline w-4 h-4 mr-2" />
            Excel
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full min-w-[600px] text-sm">
          <thead className="bg-gray-50">
            <tr>
              {columnsToRender.map(([key, label]) => (
                <th
                  key={key}
                  onClick={() => handleSort(key)}
                  className="p-3 cursor-pointer text-left whitespace-nowrap"
                >
                  {label} {getSortIcon(key)}
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {loading ? (
              Array.from({ length: pageSize }).map((_, i) => (
                <tr key={i} className="animate-pulse">
                  {columnsToRender.map((_, j) => (
                    <td key={j} className="p-3">
                      <div className="h-4 bg-gray-200 rounded" />
                    </td>
                  ))}
                </tr>
              ))
            ) : processedData.length === 0 ? (
              <tr>
                <td
                  colSpan={columnsToRender.length}
                  className="h-40 text-center text-gray-500"
                >
                  No records found
                </td>
              </tr>
            ) : (
              processedData.map((t) => (
                <tr
                  key={t.id}
                  onClick={() => onRowClick?.(t)}
                  className="border-t hover:bg-gray-50 cursor-pointer"
                >
                  {columnsToRender.map(([key]) => {
                    if (key === "createdAt")
                      return (
                        <td key={key} className="p-3">
                          {t[key]
                            ? dayjs(t[key]).format("DD MMM YYYY")
                            : "-"}
                        </td>
                      );
                    if (key === "netWeight")
                      return (
                        <td key={key} className="p-3 text-right">
                          {t[key]?.toLocaleString() || "-"}
                        </td>
                      );
                    if (key === "status")
                      return (
                        <td key={key} className="p-3">
                          {statusBadge(t[key])}
                        </td>
                      );
                    if (key === "weighMode")
                      return (
                        <td key={key} className="p-3">
                          {weighModeBadge(t[key])}
                        </td>
                      );
                    return (
                      <td key={key} className="p-3">
                        {t[key] ?? "-"}
                      </td>
                    );
                  })}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="p-4 sm:p-6 border-t flex justify-between items-center">
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
  );
}
