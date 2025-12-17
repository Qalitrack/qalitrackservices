// src/pages/weighing/Reports.jsx
import { useState, useEffect } from "react";
import ReportsTable from "./ReportsTable";
import {
  Calendar,
  ChevronDown,
  ChevronUp,
  Filter,
  RotateCcw,
} from "lucide-react";
import { getTransactions } from "../../api/Weighing/Transactions";
import jsPDF from "jspdf";
import "jspdf-autotable";
import * as XLSX from "xlsx";

/**
 * Reports Page
 */
export default function Reports() {
  /* =========================
     Filter State
     ========================= */
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    receiptNumber: "",
    numberPlate: "",
    weighbridge: "",
    status: "",
    driver: "",
    commodity: "",
    supplier: "",
    customer: "",
    transporter: "",
    origin: "",
    destination: "",
    operator: "",
    weighMode: "",
    completed: "",
  });

  const [showAdvanced, setShowAdvanced] = useState(false);

  /* =========================
     Table State
     ========================= */
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalRecords, setTotalRecords] = useState(0);

  /* =========================
     Summary Analytics State
     ========================= */
  const [summaryData, setSummaryData] = useState({
    drivers: 0,
    vehicles: 0,
    commodities: [],
    statuses: {},
  });

  /* =========================
     Helper Functions
     ========================= */
  const updateFilter = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  // Clear all filters
  const handleClearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      receiptNumber: "",
      numberPlate: "",
      weighbridge: "",
      status: "",
      driver: "",
      commodity: "",
      supplier: "",
      customer: "",
      transporter: "",
      origin: "",
      destination: "",
      operator: "",
      weighMode: "",
      completed: "",
    });
  };

  /* =========================
     Fetch Transactions
     ========================= */
  const fetchTransactions = async () => {
    setLoading(true);
    try {
      // Send filters + pagination
      const params = {
        ...filters,
        page: currentPage,
        pageSize,
      };
      const data = await getTransactions(params);

      setTransactions(data.transactions || []); // adapt based on API response
      setTotalRecords(data.total || data.transactions?.length || 0);
    } catch (error) {
      console.error("Error fetching transactions:", error);
    } finally {
      setLoading(false);
    }
  };

  // Fetch transactions on mount and whenever filters or pagination change
  useEffect(() => {
    fetchTransactions();
  }, [filters, currentPage, pageSize]);

  /* =========================
     Compute Summary Analytics
     ========================= */
  useEffect(() => {
    if (!transactions || transactions.length === 0) return;

    const driversSet = new Set();
    const vehiclesSet = new Set();
    const commoditiesMap = {};
    const statusesMap = {};

    transactions.forEach((t) => {
      if (t.driver) driversSet.add(t.driver);
      if (t.numberPlate) vehiclesSet.add(t.numberPlate);

      if (t.commodity) {
        commoditiesMap[t.commodity] = (commoditiesMap[t.commodity] || 0) + 1;
      }

      if (t.status) {
        statusesMap[t.status] = (statusesMap[t.status] || 0) + 1;
      }
    });

    setSummaryData({
      drivers: driversSet.size,
      vehicles: vehiclesSet.size,
      commodities: Object.entries(commoditiesMap),
      statuses: statusesMap,
    });
  }, [transactions]);

  /* =========================
     Export Functions
     ========================= */
  const exportPDF = () => {
    const doc = new jsPDF();
    doc.text("Transactions Report", 14, 16);
    const tableColumn = Object.keys(transactions[0] || {});
    const tableRows = transactions.map((t) => Object.values(t));
    doc.autoTable({
      head: [tableColumn],
      body: tableRows,
      startY: 20,
    });
    doc.save("transactions-report.pdf");
  };

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(transactions);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(wb, "transactions-report.xlsx");
  };

  return (
    <div className="p-6">
      {/* =========================
          Page Header
         ========================= */}
      <div className="mb-6">
        <h1 className="text-black text-4xl mb-2">Reports</h1>
        <p className="text-gray-600">
          Generate transaction-based operational reports
        </p>
      </div>

      {/* =========================
          Filters
         ========================= */}
      <div className="bg-white rounded-lg border border-gray-200 p-6 mb-8">
        <div className="flex items-center gap-2 mb-4">
          <Filter className="w-5 h-5 text-gray-600" />
          <h2 className="text-lg text-black">Filters</h2>
        </div>

        {/* Date Range */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block text-sm text-gray-700 mb-2">
              Start Date
            </label>
            <div className="relative">
              <input
                type="date"
                value={filters.startDate}
                onChange={(e) => updateFilter("startDate", e.target.value)}
                className="w-full border border-gray-300 rounded-md px-3 py-2 pl-10"
              />
              <Calendar className="w-4 h-4 text-gray-500 absolute left-3 top-1/2 -translate-y-1/2" />
            </div>
          </div>

          <div>
            <label className="block text-sm text-gray-700 mb-2">
              End Date
            </label>
            <div className="relative">
              <input
                type="date"
                value={filters.endDate}
                onChange={(e) => updateFilter("endDate", e.target.value)}
                className="w-full border border-gray-300 rounded-md px-3 py-2 pl-10"
              />
              <Calendar className="w-4 h-4 text-gray-500 absolute left-3 top-1/2 -translate-y-1/2" />
            </div>
          </div>
        </div>

        {/* Basic Filters */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
          <input
            type="text"
            placeholder="Receipt Number"
            value={filters.receiptNumber}
            onChange={(e) => updateFilter("receiptNumber", e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2"
          />

          <input
            type="text"
            placeholder="Number Plate"
            value={filters.numberPlate}
            onChange={(e) => updateFilter("numberPlate", e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2"
          />

          <select
            value={filters.status}
            onChange={(e) => updateFilter("status", e.target.value)}
            className="border border-gray-300 rounded-md px-3 py-2"
          >
            <option value="">All Statuses</option>
            <option value="completed">Completed</option>
            <option value="pending">Pending</option>
            <option value="in-progress">In Progress</option>
          </select>
        </div>

        {/* Advanced Toggle */}
        <div className="mt-4 pt-4 border-t border-gray-200">
          <button
            type="button"
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="flex items-center text-gray-700 hover:text-black"
          >
            {showAdvanced ? (
              <>
                <ChevronUp className="w-4 h-4 mr-2" /> Hide Advanced Filters
              </>
            ) : (
              <>
                <ChevronDown className="w-4 h-4 mr-2" /> Show Advanced Filters
              </>
            )}
          </button>
        </div>

        {/* Advanced Filters */}
        {showAdvanced && (
          <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4">
            {[
              ["driver", "Driver"],
              ["supplier", "Supplier"],
              ["customer", "Customer"],
              ["origin", "Origin"],
              ["destination", "Destination"],
              ["operator", "Operator"],
            ].map(([key, label]) => (
              <input
                key={key}
                type="text"
                placeholder={label}
                value={filters[key]}
                onChange={(e) => updateFilter(key, e.target.value)}
                className="border border-gray-300 rounded-md px-3 py-2"
              />
            ))}
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 mt-6">
          <button
            type="button"
            onClick={fetchTransactions}
            className="bg-[#FBBF24] hover:bg-[#F59E0B] text-black px-4 py-2 rounded-md"
          >
            Apply Filters
          </button>

          <button
            type="button"
            onClick={handleClearFilters}
            className="flex items-center border border-gray-300 px-4 py-2 rounded-md"
          >
            <RotateCcw className="w-4 h-4 mr-2" />
            Clear Filters
          </button>

          <button
            type="button"
            onClick={exportPDF}
            className="bg-blue-500 text-white px-4 py-2 rounded-md"
          >
            Export PDF
          </button>

          <button
            type="button"
            onClick={exportExcel}
            className="bg-green-500 text-white px-4 py-2 rounded-md"
          >
            Export Excel
          </button>
        </div>
      </div>

      {/* =========================
          Transactions Table
         ========================= */}
      <ReportsTable
        transactions={transactions}
        loading={loading}
        currentPage={currentPage}
        pageSize={pageSize}
        totalRecords={totalRecords}
        onPageChange={setCurrentPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setCurrentPage(1);
        }}
      />

      {/* =========================
          Summary Analytics
         ========================= */}
      <div className="mt-10">
        <h2 className="text-xl font-bold mb-4">Summary</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Drivers */}
          <div className="bg-white border p-4 rounded">
            <h3 className="font-semibold mb-2">Number of Drivers</h3>
            <p>{summaryData.drivers}</p>
          </div>

          {/* Vehicles */}
          <div className="bg-white border p-4 rounded">
            <h3 className="font-semibold mb-2">Number of Vehicles</h3>
            <p>{summaryData.vehicles}</p>
          </div>

          {/* Commodities */}
          <div className="bg-white border p-4 rounded">
            <h3 className="font-semibold mb-2">Commodities</h3>
            <ul>
              {summaryData.commodities.map(([commodity, count]) => (
                <li key={commodity}>
                  {commodity}: {count}
                </li>
              ))}
            </ul>
          </div>

          {/* Status */}
          <div className="bg-white border p-4 rounded">
            <h3 className="font-semibold mb-2">Status</h3>
            <ul>
              {Object.entries(summaryData.statuses).map(([status, count]) => (
                <li key={status}>
                  {status}: {count}
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
