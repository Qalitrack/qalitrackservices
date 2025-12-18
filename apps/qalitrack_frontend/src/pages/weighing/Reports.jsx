import { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import { fetchTransactions } from "../../store/weighingSlice";
import { Filter, RotateCcw } from "lucide-react";

// EXPORT LIBRARIES
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";

/**
 * Reports Page
 */
export default function Reports() {
  const dispatch = useDispatch();

  /* =========================
     Redux State
     ========================= */
  const { transactions, loading } = useSelector(
    (state) => state.weighing
  );

  /* =========================
     Report Type Tabs
     ========================= */
  const REPORT_TABS = [
    "transactions",
    "drivers",
    "customers",
    "commodities",
    "suppliers",
    // "status",
  ];

  const [activeTab, setActiveTab] = useState("transactions");

  /* =========================
     Pagination
     ========================= */
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  /* =========================
     Filters
     ========================= */
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    status: "",
  });

  /* =========================
     Fetch on Load
     ========================= */
  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  /* =========================
     Filtered Transactions
     ========================= */
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      data = data.filter(
        (t) => new Date(t.createdAt) >= new Date(filters.startDate)
      );
    }

    if (filters.endDate) {
      data = data.filter(
        (t) => new Date(t.createdAt) <= new Date(filters.endDate)
      );
    }

    if (filters.status) {
      data = data.filter((t) => t.status === filters.status);
    }

    return data;
  }, [transactions, filters]);

  /* =========================
     Pagination Slice
     ========================= */
  const totalRecords = filteredTransactions.length;

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return filteredTransactions.slice(start, start + pageSize);
  }, [filteredTransactions, currentPage, pageSize]);

  /* =========================
     Export PDF
     ========================= */
  const handleExportPDF = (rows) => {
    const doc = new jsPDF("landscape");
    doc.setFontSize(16);
    doc.text("Transaction Report", 14, 15);

    autoTable(doc, {
      startY: 25,
      head: [[
        "Date",
        "Receipt",
        "Vehicle",
        "Driver",
        "Commodity",
        "Supplier",
        "Customer",
        "First Weight",
        "Net Weight",
        "Status",
      ]],
      body: rows.map((t) => [
        t.createdAt ? new Date(t.createdAt).toLocaleDateString() : "-",
        t.receiptNo || "-",
        t.noPlate || "-",
        t.driverName || "-",
        t.commodityName || "-",
        t.supplierName || "-",
        t.customerName || "-",
        t.firstWeight || "-",
        t.netWeight || "-",
        t.status || "-",
      ]),
      styles: { fontSize: 9 },
      headStyles: { fillColor: [251, 191, 36], textColor: 0 },
    });

    doc.save("transaction-report.pdf");
  };

  /* =========================
     Export Excel
     ========================= */
  const handleExportExcel = (rows) => {
    const data = rows.map((t) => ({
      Date: t.createdAt
        ? new Date(t.createdAt).toLocaleDateString()
        : "",
      Receipt: t.receiptNo || "",
      Vehicle: t.noPlate || "",
      Driver: t.driverName || "",
      Commodity: t.commodityName || "",
      Supplier: t.supplierName || "",
      Customer: t.customerName || "",
      FirstWeight: t.firstWeight || "",
      NetWeight: t.netWeight || "",
      Status: t.status || "",
    }));

    const ws = XLSX.utils.json_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(wb, "transaction-report.xlsx");
  };

  /* =========================
     Reset Filters
     ========================= */
  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", status: "" });
    setCurrentPage(1);
  };

  return (
    <div className="p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h1 className="text-4xl mb-2">Reports</h1>
        <p className="text-gray-600">
          Operational and analytical system reports
        </p>
      </div>

      {/* REPORT TYPE BUTTONS */}
      <div className="flex gap-3 mb-6 flex-wrap">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded border capitalize ${
              activeTab === tab
                ? "bg-yellow-400 border-yellow-400"
                : "bg-white"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* FILTERS (Transactions Only for now) */}
      {activeTab === "transactions" && (
        <div className="bg-white border rounded-lg p-4 mb-6">
          <div className="flex gap-4 flex-wrap items-end">
            <input
              type="date"
              value={filters.startDate}
              onChange={(e) =>
                setFilters({ ...filters, startDate: e.target.value })
              }
              className="border rounded px-3 py-2"
            />

            <input
              type="date"
              value={filters.endDate}
              onChange={(e) =>
                setFilters({ ...filters, endDate: e.target.value })
              }
              className="border rounded px-3 py-2"
            />

            <select
              value={filters.status}
              onChange={(e) =>
                setFilters({ ...filters, status: e.target.value })
              }
              className="border rounded px-3 py-2"
            >
              <option value="">All Status</option>
              <option value="completed">Completed</option>
              <option value="pending">InProgress</option>
            </select>

            <button
              onClick={clearFilters}
              className="border px-4 py-2 rounded"
            >
              <RotateCcw size={16} />
            </button>
          </div>
        </div>
      )}

      {/* CONTENT */}
      {activeTab === "transactions" ? (
        <ReportsTable
          transactions={paginatedData}
          loading={loading}
          currentPage={currentPage}
          pageSize={pageSize}
          totalRecords={totalRecords}
          onPageChange={setCurrentPage}
          onPageSizeChange={setPageSize}
          onExportPDF={handleExportPDF}
          onExportExcel={handleExportExcel}
        />
      ) : (
        <div className="bg-white border rounded-lg p-12 text-center text-gray-500">
          <h2 className="text-xl mb-2 capitalize">
            {activeTab} report
          </h2>
          <p>This report view will be implemented next.</p>
        </div>
      )}
    </div>
  );
}
