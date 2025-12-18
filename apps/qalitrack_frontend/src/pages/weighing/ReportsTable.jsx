import { useState, useMemo } from "react";
import {
  FileDown,
  FileSpreadsheet,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from "lucide-react";

/**
 * ReportsTable
 * - Displays paginated & sortable transaction records
 */
export default function ReportsTable({
  transactions = [],
  loading = false,
  currentPage = 1,
  pageSize = 10,
  totalRecords = 0,
  onPageChange,
  onPageSizeChange,
  onExportPDF,
  onExportExcel,
}) {
  /* =========================
     Local State
     ========================= */
  const [sortField, setSortField] = useState(null);
  const [sortOrder, setSortOrder] = useState(null);

  const totalPages = Math.ceil(totalRecords / pageSize);

  /* =========================
     Sorting Logic
     ========================= */
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

  /* =========================
     Sorted + Paginated Data
     ========================= */
  const processedData = useMemo(() => {
    let data = [...transactions];

    if (sortField && sortOrder) {
      data.sort((a, b) => {
        const aVal = a[sortField] ?? "";
        const bVal = b[sortField] ?? "";

        if (typeof aVal === "number") {
          return sortOrder === "asc" ? aVal - bVal : bVal - aVal;
        }

        return sortOrder === "asc"
          ? String(aVal).localeCompare(String(bVal))
          : String(bVal).localeCompare(String(aVal));
      });
    }

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;

    return data.slice(start, end);
  }, [transactions, sortField, sortOrder, currentPage, pageSize]);

  /* =========================
     Badge Helpers
     ========================= */
  const renderBadge = (label, className) => (
    <span className={`px-2 py-1 text-xs rounded border ${className}`}>
      {label || "-"}
    </span>
  );

  const getStatusBadge = (status) => {
    const map = {
      completed: "bg-green-100 text-green-800 border-green-200",
      pending: "bg-yellow-100 text-yellow-800 border-yellow-200",
      "in-progress": "bg-blue-100 text-blue-800 border-blue-200",
    };
    return renderBadge(status, map[status] || map.pending);
  };

  const getWeighModeBadge = (mode) => {
    const map = {
      inbound: "bg-purple-100 text-purple-800 border-purple-200",
      outbound: "bg-orange-100 text-orange-800 border-orange-200",
      single: "bg-gray-100 text-gray-800 border-gray-200",
    };
    return renderBadge(mode, map[mode] || map.single);
  };

  /* =========================
     Render States
     ========================= */
  const renderLoading = () =>
    Array.from({ length: pageSize }).map((_, i) => (
      <tr key={i} className="animate-pulse">
        {Array.from({ length: 15 }).map((_, j) => (
          <td key={j} className="p-3">
            <div className="h-4 bg-gray-200 rounded" />
          </td>
        ))}
      </tr>
    ));

  const renderEmpty = () => (
    <tr>
      <td colSpan={15} className="h-64 text-center text-gray-500">
        <FileDown className="w-12 h-12 mx-auto mb-4 opacity-30" />
        <p className="text-lg">No transactions found</p>
        <p className="text-sm mt-2">Adjust filters and try again</p>
      </td>
    </tr>
  );

  return (
    <div className="bg-white rounded-lg border shadow-sm">
      {/* Header */}
      <div className="p-6 border-b flex justify-between items-center">
        <div>
          <h2 className="text-xl">Transaction Records</h2>
          <p className="text-sm text-gray-600">
            Showing {processedData.length} of {totalRecords}
          </p>
        </div>

        <div className="flex gap-3">
          <button onClick={onExportPDF} className="border px-4 py-2 rounded">
            <FileDown className="inline w-4 h-4 mr-2" />
            PDF
          </button>
          <button
            onClick={onExportExcel}
            className="bg-amber-400 px-4 py-2 rounded"
          >
            <FileSpreadsheet className="inline w-4 h-4 mr-2" />
            Excel
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              {[
                ["createdAt", "Date"],
                ["receiptNo", "Receipt"],
                ["noPlate", "Vehicle"],
                ["driverName", "Driver"],
                ["transporterName", "Transporter"],
                ["source", "Source"],
                ["destination", "Destination"],
                ["firstWeight", "First Weight (kg)"],
                ["secondWeight", "Second Weight (kg)"],
                ["netWeight", "Net Weight (kg)"],
              ].map(([key, label]) => (
                <th
                  key={key}
                  onClick={() => handleSort(key)}
                  className="p-3 cursor-pointer text-left"
                >
                  {label} {getSortIcon(key)}
                </th>
              ))}
              <th className="p-3">Mode</th>
              <th className="p-3">Status</th>
            </tr>
          </thead>

          <tbody>
            {loading
              ? renderLoading()
              : processedData.length === 0
              ? renderEmpty()
              : processedData.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-3">
                      {t.createdAt
                        ? new Date(t.createdAt).toLocaleDateString()
                        : "-"}
                    </td>
                    <td className="p-3 font-mono">{t.receiptNo || "-"}</td>
                    <td className="p-3">{t.noPlate || "-"}</td>
                    <td className="p-3">{t.driverName || "-"}</td>
                    <td className="p-3">{t.transporterName || "-"}</td>
                    <td className="p-3">{t.source || "-"}</td>
                    <td className="p-3">{t.destination || "-"}</td>
                    <td className="p-3 text-right">
                      {t.firstWeight?.toLocaleString() || "-"}
                    </td>
                    <td className="p-3 text-right">
                      {t.secondWeight?.toLocaleString() || "-"}
                    </td>
                    <td className="p-3 text-right">
                      {t.netWeight?.toLocaleString() || "-"}
                    </td>
                    <td className="p-3">
                      {getWeighModeBadge(t.weighMode)}
                    </td>
                    <td className="p-3">
                      {getStatusBadge(t.status)}
                    </td>
                  </tr>
                ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="p-6 border-t flex justify-between items-center">
        <div className="flex items-center gap-2">
          <span className="text-sm">Rows:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border rounded px-2 py-1"
          >
            {[10, 25, 50].map((n) => (
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
            className="border px-3 py-1 rounded"
          >
            Prev
          </button>
          <span className="text-sm">
            Page {currentPage} of {totalPages}
          </span>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="border px-3 py-1 rounded"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
