import { useState } from "react";
import {
  FileDown,
  FileSpreadsheet,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from "lucide-react";

/**
 * ReportsTable
 */
export default function ReportsTable({
  transactions,
  loading = false,
  currentPage,
  pageSize,
  totalRecords,
  onPageChange,
  onPageSizeChange,
  onExportPDF,
  onExportExcel,
}) {
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
     Badge Helpers
     ========================= */
  const renderBadge = (label, className) => (
    <span
      className={`px-2 py-1 text-xs rounded border ${className}`}
    >
      {label}
    </span>
  );

  const getStatusBadge = (status) => {
    const map = {
      completed: "bg-green-100 text-green-800 border-green-200",
      pending: "bg-yellow-100 text-yellow-800 border-yellow-200",
      "in-progress": "bg-blue-100 text-blue-800 border-blue-200",
    };
    return renderBadge(status, map[status] || map.completed);
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
        {Array.from({ length: 11 }).map((_, j) => (
          <td key={j} className="p-3">
            <div className="h-4 bg-gray-200 rounded" />
          </td>
        ))}
      </tr>
    ));

  const renderEmpty = () => (
    <tr>
      <td colSpan={11} className="h-64 text-center text-gray-500">
        <FileDown className="w-12 h-12 mx-auto mb-4 opacity-30" />
        <p className="text-lg">No transactions found</p>
        <p className="text-sm mt-2">Adjust filters and try again</p>
      </td>
    </tr>
  );

  return (
    <div className="bg-white rounded-lg border border-gray-200 shadow-sm">
      {/* Header */}
      <div className="p-6 border-b flex justify-between items-center">
        <div>
          <h2 className="text-xl text-black">Transaction Records</h2>
          <p className="text-sm text-gray-600">
            Showing {transactions.length} of {totalRecords}
          </p>
        </div>

        <div className="flex gap-3">
          <button
            onClick={onExportPDF}
            disabled={loading || transactions.length === 0}
            className="border border-amber-400 text-amber-400 px-4 py-2 rounded hover:bg-amber-50 disabled:opacity-50"
          >
            <FileDown className="w-4 h-4 inline mr-2" />
            PDF
          </button>

          <button
            onClick={onExportExcel}
            disabled={loading || transactions.length === 0}
            className="bg-amber-400 hover:bg-amber-500 text-black px-4 py-2 rounded disabled:opacity-50"
          >
            <FileSpreadsheet className="w-4 h-4 inline mr-2" />
            Excel
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 sticky top-0">
            <tr>
              {[
                ["date", "Date"],
                ["receiptNo", "Receipt"],
                ["numberPlate", "Vehicle"],
                ["driver", "Driver"],
                ["commodity", "Commodity"],
                ["supplier", "Supplier"],
                ["customer", "Customer"],
                ["weighbridge", "Weighbridge"],
                ["firstWeight", "Weight (kg)"],
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
              : transactions.length === 0
              ? renderEmpty()
              : transactions.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-3">{t.date}</td>
                    <td className="p-3 font-mono">{t.receiptNo}</td>
                    <td className="p-3">{t.numberPlate}</td>
                    <td className="p-3">{t.driver}</td>
                    <td className="p-3">{t.commodity}</td>
                    <td className="p-3">{t.supplier}</td>
                    <td className="p-3">{t.customer}</td>
                    <td className="p-3">{t.weighbridge}</td>
                    <td className="p-3 text-right">
                      {t.firstWeight.toLocaleString()}
                    </td>
                    <td className="p-3">{getWeighModeBadge(t.weighMode)}</td>
                    <td className="p-3">{getStatusBadge(t.status)}</td>
                  </tr>
                ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="p-6 border-t flex justify-between items-center">
        <div className="flex items-center gap-3">
          <span className="text-sm text-gray-600">Rows:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="border rounded px-2 py-1"
          >
            <option value={10}>10</option>
            <option value={25}>25</option>
            <option value={50}>50</option>
          </select>
        </div>

        <div className="flex items-center gap-2">
          <span className="text-sm text-gray-600">
            Page {currentPage} of {totalPages}
          </span>
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="border px-3 py-1 rounded disabled:opacity-50"
          >
            Prev
          </button>
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
