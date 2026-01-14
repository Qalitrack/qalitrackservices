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
  const [pageSize] = useState(7);

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    status: "",
  });

  // Fetch transactions on mount
  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  // Reset page when filters or tab change
  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // Filtered transactions
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      const start = new Date(filters.startDate);
      start.setHours(0, 0, 0, 0);
      data = data.filter((t) => new Date(t.createdAt) >= start);
    }

    if (filters.endDate) {
      const end = new Date(filters.endDate);
      end.setHours(23, 59, 59, 999);
      data = data.filter((t) => new Date(t.createdAt) <= end);
    }

    if (filters.status) {
      data = data.filter(
        (t) => String(t.status).toLowerCase() === filters.status.toLowerCase()
      );
    }

    return data;
  }, [transactions, filters]);

  const totalRecords = filteredTransactions.length;

  // EXPORT: PDF
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

  // EXPORT: EXCEL
  const handleExportExcel = (rows) => {
    const data = rows.map((t) => ({
      Date: t.createdAt ? new Date(t.createdAt).toLocaleDateString() : "",
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

  // Reset filters
  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", status: "" });
    setCurrentPage(1);
  };

  // Render active report
  const renderActiveReport = () => {
    switch (activeTab) {
      case "transactions":
        return (
          <ReportsTable
            transactions={filteredTransactions} // PASS FULL DATA
            loading={loading}
            currentPage={currentPage}
            pageSize={pageSize}
            totalRecords={totalRecords}
            onPageChange={setCurrentPage}
            onExportPDF={handleExportPDF}
            onExportExcel={handleExportExcel}
          />
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

      {/* FILTERS (Transactions only) */}
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
        </div>
      )}

      {/* ACTIVE REPORT */}
      {renderActiveReport()}
    </div>
  );
}
