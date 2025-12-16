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

export default function Reports() {
  // =========================
  // Filters state
  // =========================
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

  // =========================
  // Table state
  // =========================
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalRecords, setTotalRecords] = useState(0);

  // =========================
  // Helpers
  // =========================
  const updateFilter = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  // Fetch transactions from API
  const fetchTransactions = async (params = {}) => {
    setLoading(true);
    try {
      // Include pagination params
      const data = await getTransactions({
        page: currentPage,
        pageSize,
        ...params, // spread filters here
      });

      // Assuming API returns: { data: [], totalRecords: number }
      setTransactions(data.data || []);
      setTotalRecords(data.totalRecords || 0);
    } catch (error) {
      console.error("Error fetching transactions:", error.message);
    } finally {
      setLoading(false);
    }
  };

  // On initial load, fetch all transactions without filters
  useEffect(() => {
    fetchTransactions();
  }, [currentPage, pageSize]);

  // Apply filters
  const handleApplyFilters = () => {
    // Build params object with only filled filters
    const filterParams = Object.fromEntries(
      Object.entries(filters).filter(([_, value]) => value)
    );

    setCurrentPage(1); // reset to first page
    fetchTransactions(filterParams);
  };

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
    setCurrentPage(1);
    fetchTransactions(); // fetch all again
  };

  // =========================
  // Export handlers
  // =========================
  const handleExportPDF = () => {
    // Placeholder: Implement backend PDF generation or client-side library
    console.log("Export PDF clicked", transactions);
  };

  const handleExportExcel = () => {
    // Placeholder: Implement backend Excel generation or client-side library
    console.log("Export Excel clicked", transactions);
  };

  return (
    <div className="p-6">
      {/* Page Header */}
      <div className="mb-6">
        <h1 className="text-black text-4xl mb-2">Reports</h1>
        <p className="text-gray-600">
          Generate transaction-based operational reports
        </p>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
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
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
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
                <ChevronUp className="w-4 h-4 mr-2" />
                Hide Advanced Filters
              </>
            ) : (
              <>
                <ChevronDown className="w-4 h-4 mr-2" />
                Show Advanced Filters
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
            onClick={handleApplyFilters}
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
        </div>
      </div>

      {/* Reports Table */}
      <div className="mt-8">
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
          onExportPDF={handleExportPDF}
          onExportExcel={handleExportExcel}
        />
      </div>
    </div>
  );
}
