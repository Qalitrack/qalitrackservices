import { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import DriverReport from "./reportFiles/DriverReport";
import CustomerReport from "./reportFiles/CustomerReport";
import CommodityReport from "./reportFiles/CommodityReport";
import SupplierReport from "./reportFiles/SupplierReport";

import { fetchTransactions } from "../../store/weighingSlice";
import { RotateCcw } from "lucide-react";

// EXPORT LIBRARIES
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

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(4); // <- default 4 per page

  // Filters (Transactions)
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: "",
    search: "",
  });

  // Modal for preview
  const [previewRecord, setPreviewRecord] = useState(null);
  const [showPreview, setShowPreview] = useState(false);

  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // FILTERED TRANSACTIONS
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
          String(t.status).toLowerCase() ===
          filters.status.toLowerCase()
      );
    }

    if (filters.search) {
      const q = filters.search.toLowerCase();
      data = data.filter(
        (t) =>
          t.receiptNo?.toLowerCase().includes(q) ||
          t.noPlate?.toLowerCase().includes(q) ||
          t.driverName?.toLowerCase().includes(q)
      );
    }

    return data;
  }, [transactions, filters]);

  const totalRecords = filteredTransactions.length;

  // TOTALS (Preview)
  const totals = useMemo(() => {
    return filteredTransactions.reduce(
      (acc, t) => {
        acc.count += 1;
        acc.net += Number(t.netWeight || 0);
        return acc;
      },
      { count: 0, net: 0 }
    );
  }, [filteredTransactions]);

  // PDF Export
  const handleExportPDF = (rows) => {
    const doc = new jsPDF("landscape");
    const pageWidth = doc.internal.pageSize.getWidth();

    // Logo/Banner (orange)
    doc.setFillColor(251, 191, 36);
    doc.rect(0, 0, pageWidth, 15, "F");
    doc.setFontSize(14);
    doc.setTextColor(0, 0, 0);
    doc.text("TRANSACTIONS REPORT", pageWidth / 2, 10, { align: "center" });

    doc.setFontSize(10);
    doc.text(
      `Period: ${filters.startDate || "All"} ${filters.startTime || ""} → ${
        filters.endDate || "All"
      } ${filters.endTime || ""}`,
      14,
      20
    );

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
      styles: { fontSize: 9 },
      headStyles: { fillColor: [251, 191, 36], textColor: 0 },
    });

    doc.save("transaction-report.pdf");
  };

  // Excel Export
  const handleExportExcel = (rows) => {
    const data = rows.map((t) => ({
      DateTime: t.createdAt ? new Date(t.createdAt).toLocaleString() : "",
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

  const renderActiveReport = () => {
    switch (activeTab) {
      case "transactions":
        return (
          <>
            {/* PREVIEW SUMMARY */}
            <div className="mb-4 text-sm text-gray-700">
              <strong>{totals.count}</strong> transactions ·{" "}
              <strong>{totals.net.toLocaleString()}</strong> total net weight
            </div>

            <ReportsTable
              transactions={filteredTransactions}
              loading={loading}
              currentPage={currentPage}
              pageSize={pageSize}
              totalRecords={totalRecords}
              onPageChange={setCurrentPage}
              onPageSizeChange={setPageSize} // <- allow changing pageSize
              onExportPDF={handleExportPDF}
              onExportExcel={handleExportExcel}
              onPreview={(record) => { setPreviewRecord(record); setShowPreview(true); }}
              showColumns={null} // display all columns
              onRowClick={(record) => { setPreviewRecord(record); setShowPreview(true); }}
            />
          </>
        );
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
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-4xl mb-2">Reports</h1>
        <p className="text-gray-600">Operational and analytical system reports</p>
      </div>

      <div className="flex gap-3 mb-6 flex-wrap">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded border capitalize ${
              activeTab === tab ? "bg-yellow-400 border-yellow-400" : "bg-white"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {activeTab === "transactions" && (
        <div className="bg-white border rounded-lg p-4 mb-6">
          <div className="flex gap-4 flex-wrap items-end">
            <input
              type="date"
              value={filters.startDate}
              onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
              className="border px-3 py-2 rounded"
            />
            <input
              type="time"
              value={filters.startTime}
              onChange={(e) => setFilters({ ...filters, startTime: e.target.value })}
              className="border px-3 py-2 rounded"
            />
            <input
              type="date"
              value={filters.endDate}
              onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
              className="border px-3 py-2 rounded"
            />
            <input
              type="time"
              value={filters.endTime}
              onChange={(e) => setFilters({ ...filters, endTime: e.target.value })}
              className="border px-3 py-2 rounded"
            />
            <input
              type="text"
              placeholder="Search receipt, vehicle, driver"
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value })}
              className="border px-3 py-2 rounded"
            />
            <select
              value={filters.status}
              onChange={(e) => setFilters({ ...filters, status: e.target.value })}
              className="border px-3 py-2 rounded"
            >
              <option value="">All Status</option>
              <option value="Completed">Completed</option>
              <option value="In Progress">In Progress</option>
            </select>
            <button onClick={clearFilters} className="border px-4 py-2 rounded">
              <RotateCcw size={16} />
            </button>
          </div>
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
