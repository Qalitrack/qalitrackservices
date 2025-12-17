import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import { fetchTransactions } from "../../store/weighingSlice";
import {
  Calendar,
  ChevronDown,
  ChevronUp,
  Filter,
  RotateCcw,
} from "lucide-react";

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
     Local UI State
     ========================= */
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    receiptNumber: "",
    numberPlate: "",
    status: "",
    driver: "",
    supplier: "",
    customer: "",
    origin: "",
    destination: "",
    operator: "",
  });

  const [showAdvanced, setShowAdvanced] = useState(false);

  /* =========================
     Fetch on Page Load
     ========================= */
  useEffect(() => {
    // Fetch ALL transactions initially (no filters)
    dispatch(fetchTransactions());
  }, [dispatch]);

  /* =========================
     Helpers
     ========================= */
  const updateFilter = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleApplyFilters = () => {
    // Remove empty filters before sending
    const cleanedFilters = Object.fromEntries(
      Object.entries(filters).filter(([_, v]) => v !== "")
    );

    dispatch(fetchTransactions(cleanedFilters));
  };

  const handleClearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      receiptNumber: "",
      numberPlate: "",
      status: "",
      driver: "",
      supplier: "",
      customer: "",
      origin: "",
      destination: "",
      operator: "",
    });

    // Reload all data
    dispatch(fetchTransactions());
  };

  /* =========================
     Export Placeholders
     ========================= */
  const handleExportPDF = () => {
    // TODO: Use jsPDF / backend PDF endpoint
    console.log("Export PDF", transactions);
  };

  const handleExportExcel = () => {
    // TODO: Use SheetJS (xlsx)
    console.log("Export Excel", transactions);
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-4xl text-black mb-2">Reports</h1>
        <p className="text-gray-600">
          Transaction-based operational reports
        </p>
      </div>

      {/* Filters */}
      <div className="bg-white border rounded-lg p-6">
        <div className="flex items-center gap-2 mb-4">
          <Filter className="w-5 h-5" />
          <h2 className="text-lg">Filters</h2>
        </div>

        {/* Date Range */}
        <div className="grid md:grid-cols-2 gap-4 mb-4">
          {["startDate", "endDate"].map((key) => (
            <div key={key}>
              <label className="block text-sm mb-1">
                {key === "startDate" ? "Start Date" : "End Date"}
              </label>
              <div className="relative">
                <input
                  type="date"
                  value={filters[key]}
                  onChange={(e) => updateFilter(key, e.target.value)}
                  className="w-full border px-3 py-2 pl-10 rounded"
                />
                <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4" />
              </div>
            </div>
          ))}
        </div>

        {/* Basic Filters */}
        <div className="grid md:grid-cols-4 gap-4">
          <input
            placeholder="Receipt Number"
            value={filters.receiptNumber}
            onChange={(e) => updateFilter("receiptNumber", e.target.value)}
            className="border px-3 py-2 rounded"
          />
          <input
            placeholder="Number Plate"
            value={filters.numberPlate}
            onChange={(e) => updateFilter("numberPlate", e.target.value)}
            className="border px-3 py-2 rounded"
          />
          <select
            value={filters.status}
            onChange={(e) => updateFilter("status", e.target.value)}
            className="border px-3 py-2 rounded"
          >
            <option value="">All Status</option>
            <option value="1">Completed</option>
            <option value="0">Pending</option>
          </select>
        </div>

        {/* Advanced Toggle */}
        <div className="mt-4 border-t pt-4">
          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="flex items-center gap-2"
          >
            {showAdvanced ? <ChevronUp /> : <ChevronDown />}
            {showAdvanced ? "Hide Advanced" : "Show Advanced"}
          </button>
        </div>

        {/* Advanced Filters */}
        {showAdvanced && (
          <div className="grid md:grid-cols-3 gap-4 mt-4">
            {["driver", "supplier", "customer", "origin", "destination", "operator"].map(
              (key) => (
                <input
                  key={key}
                  placeholder={key}
                  value={filters[key]}
                  onChange={(e) => updateFilter(key, e.target.value)}
                  className="border px-3 py-2 rounded"
                />
              )
            )}
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 mt-6">
          <button
            onClick={handleApplyFilters}
            className="bg-yellow-400 px-4 py-2 rounded"
          >
            Apply Filters
          </button>
          <button
            onClick={handleClearFilters}
            className="border px-4 py-2 rounded flex items-center gap-2"
          >
            <RotateCcw size={16} />
            Clear
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="mt-8">
        <ReportsTable
          transactions={transactions}
          loading={loading}
          onExportPDF={handleExportPDF}
          onExportExcel={handleExportExcel}
        />
      </div>
    </div>
  );
}
