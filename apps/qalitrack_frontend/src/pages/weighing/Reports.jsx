import { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import DriverReport from "./reportFiles/DriverReport";
import CustomerReport from "./reportFiles/CustomerReport";
import CommodityReport from "./reportFiles/CommodityReport";
import SupplierReport from "./reportFiles/SupplierReport";

import { fetchTransactions } from "../../store/weighingSlice";
import { RotateCcw } from "lucide-react";

import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";

export default function Reports() {
  const dispatch = useDispatch();

  const { transactions, loading } = useSelector((state) => state.weighing);

  const REPORT_TABS = [
    "transactions",
    "drivers",
    "customers",
    "commodities",
    "suppliers",
  ];

  const [activeTab, setActiveTab] = useState("transactions");

  // 🔒 Pagination ONLY for transactions
  const PAGE_SIZE = 5;
  const [currentPage, setCurrentPage] = useState(1);

  // Filters (transactions only)
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: "",
    search: "",
  });

  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  // Reset pagination when filters or tab change
  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // =========================
  // Filtered Transactions
  // =========================
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      const start = new Date(
        `${filters.startDate}T${filters.startTime || "00:00"}`
      );
      data = data.filter((t) => new Date(t.createdAt) >= start);
    }

    if (filters.endDate) {
      const end = new Date(
        `${filters.endDate}T${filters.endTime || "23:59"}`
      );
      data = data.filter((t) => new Date(t.createdAt) <= end);
    }

    if (filters.status) {
      data = data.filter(
        (t) =>
          String(t.status).toLowerCase() === filters.status.toLowerCase()
      );
    }

    return data;
  }, [transactions, filters]);

  const totalRecords = filteredTransactions.length;

  // =========================
  // Pagination (Transactions only)
  // =========================
  const paginatedTransactions = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    const end = start + PAGE_SIZE;
    return filteredTransactions.slice(start, end);
  }, [filteredTransactions, currentPage]);

  // =========================
  // EXPORTS
  // =========================
  const handleExportPDF = (rows) => {
    const doc = new jsPDF("landscape");
    doc.text("Transaction Report", 14, 15);

    autoTable(doc, {
      startY: 25,
      head: [[
        "Date & Time",
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
        t.createdAt ? new Date(t.createdAt).toLocaleString() : "-",
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
    });

    doc.save("transaction-report.pdf");
  };

  const handleExportExcel = (rows) => {
    const ws = XLSX.utils.json_to_sheet(
      rows.map((t) => ({
        Date: t.createdAt
          ? new Date(t.createdAt).toLocaleDateString()
          : "",
        Receipt: t.receiptNo || "",
        Vehicle: t.noPlate || "",
        Driver: t.driverName || "",
        Commodity: t.commodityName || "",
        Supplier: t.supplierName || "",
        Customer: t.customerName || "",
        NetWeight: t.netWeight || "",
        Status: t.status || "",
      }))
    );

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(wb, "transaction-report.xlsx");
  };

  const clearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      startTime: "",
      endTime: "",
      status: "",
      search: "",
    });
    setCurrentPage(1);
  };

  // =========================
  // Active Report Renderer
  // =========================
  const renderActiveReport = () => {
    if (activeTab === "transactions") {
      return (
        <ReportsTable
          transactions={paginatedTransactions}
          loading={loading}
          currentPage={currentPage}
          pageSize={PAGE_SIZE}
          totalRecords={totalRecords}
          onPageChange={setCurrentPage}
          onExportPDF={handleExportPDF}
          onExportExcel={handleExportExcel}
        />
      );
    }

    // 🔥 Summary reports get FULL filtered data (no slicing)
    switch (activeTab) {
      case "drivers":
        return <DriverReport transactions={filteredTransactions} loading={loading} />;
      case "customers":
        return <CustomerReport transactions={filteredTransactions} loading={loading} />;
      case "commodities":
        return <CommodityReport transactions={filteredTransactions} loading={loading} />;
      case "suppliers":
        return <SupplierReport transactions={filteredTransactions} loading={loading} />;
      default:
        return null;
    }
  };

  return (
    <div className="p-4 sm:p-6">
      <h1 className="text-3xl sm:text-4xl mb-2">Reports</h1>
      <p className="text-gray-600 mb-6">
        Operational and analytical system reports
      </p>

      {/* Tabs */}
      <div className="flex gap-3 mb-6 flex-wrap">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded border capitalize text-sm sm:text-base ${
              activeTab === tab
                ? "bg-yellow-400 border-yellow-400"
                : "bg-white"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Filters */}
      {activeTab === "transactions" && (
        <div className="bg-white border rounded-lg p-4 mb-6 flex gap-4 flex-wrap">
          <input
            type="date"
            value={filters.startDate}
            onChange={(e) =>
              setFilters({ ...filters, startDate: e.target.value })
            }
            className="border rounded px-3 py-2 w-full sm:w-auto"
          />
          <input
            type="date"
            value={filters.endDate}
            onChange={(e) =>
              setFilters({ ...filters, endDate: e.target.value })
            }
            className="border rounded px-3 py-2 w-full sm:w-auto"
          />
          <select
            value={filters.status}
            onChange={(e) =>
              setFilters({ ...filters, status: e.target.value })
            }
            className="border rounded px-3 py-2 w-full sm:w-auto"
          >
            <option value="">All Status</option>
            <option value="Completed">Completed</option>
            <option value="Pending">Pending</option>
            <option value="In Progress">In Progress</option>
          </select>

          <button
            onClick={clearFilters}
            className="border px-4 py-2 rounded"
          >
            <RotateCcw size={16} />
          </button>
        </div>
      )}

      {renderActiveReport()}

      {/* Preview Modal */}
      {showPreview && previewRecord && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg w-4/5 max-h-[90vh] overflow-auto">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold">Preview Transaction</h2>
              <button
                className="px-3 py-1 rounded border"
                onClick={() => setShowPreview(false)}
              >
                Close
              </button>
            </div>

            <div className="mb-4">
              <strong>Receipt No:</strong> {previewRecord.receiptNo}<br/>
              <strong>Vehicle:</strong> {previewRecord.noPlate}<br/>
              <strong>Driver:</strong> {previewRecord.driverName}<br/>
              <strong>Commodity:</strong> {previewRecord.commodityName}<br/>
              <strong>First Weight:</strong> {previewRecord.firstWeight} Kg<br/>
              <strong>Net Weight:</strong> {previewRecord.netWeight} Kg<br/>
              <strong>Status:</strong> {previewRecord.status}<br/>
              <strong>Date:</strong> {new Date(previewRecord.createdAt).toLocaleString()}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
