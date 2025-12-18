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

// ✅ EXPORT LIBRARIES (ADDED)
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
  const { transactions: allTransactions, loading } = useSelector(
    (state) => state.weighing
  );

  /* =========================
     Local State
     ========================= */
  const [filteredTransactions, setFilteredTransactions] = useState([]);
  const [showAdvanced, setShowAdvanced] = useState(false);

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

  /* =========================
     Fetch on Load
     ========================= */
  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  /* =========================
     Sync Filtered Data
     ========================= */
  useEffect(() => {
    setFilteredTransactions(allTransactions);
  }, [allTransactions]);

  /* =========================
     Helpers
     ========================= */
  const updateFilter = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  /* =========================
     Apply Filters (LOCAL)
     ========================= */
  const handleApplyFilters = () => {
    let data = [...allTransactions];

    if (filters.startDate) {
      data = data.filter(
        (tx) => new Date(tx.createdAt) >= new Date(filters.startDate)
      );
    }

    if (filters.endDate) {
      data = data.filter(
        (tx) => new Date(tx.createdAt) <= new Date(filters.endDate)
      );
    }

    if (filters.receiptNumber) {
      data = data.filter((tx) =>
        tx.receiptNumber
          ?.toLowerCase()
          .includes(filters.receiptNumber.toLowerCase())
      );
    }

    if (filters.numberPlate) {
      data = data.filter((tx) =>
        tx.numberPlate
          ?.toLowerCase()
          .includes(filters.numberPlate.toLowerCase())
      );
    }

    if (filters.status === "completed") {
      data = data.filter((tx) => tx.isCompleted === true);
    }

    if (filters.status === "InProgress") {
      data = data.filter((tx) => tx.isCompleted === false);
    }

    [
      "driver",
      "supplier",
      "customer",
      "origin",
      "destination",
      "operator",
    ].forEach((key) => {
      if (filters[key]) {
        data = data.filter((tx) =>
          tx[key]?.toLowerCase().includes(filters[key].toLowerCase())
        );
      }
    });

    setFilteredTransactions(data);
  };

  /* =========================
     Clear Filters
     ========================= */
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

    setFilteredTransactions(allTransactions);
  };

  /* =========================
     Summary Calculations
     ========================= */
  const totalTransactions = filteredTransactions.length;

  const completedTransactions = filteredTransactions.filter(
    (tx) => tx.isCompleted === true
  ).length;

  const pendingTransactions = filteredTransactions.filter(
    (tx) => tx.isCompleted === false
  ).length;

  const totalNetWeight = filteredTransactions.reduce(
    (sum, tx) => sum + (tx?.netWeight || 0),
    0
  );

  /* =========================
     EXPORT: PDF (ADDED)
     ========================= */
  const handleExportPDF = () => {
    const doc = new jsPDF("landscape");

    doc.setFontSize(16);
    doc.text("Transaction Report", 14, 15);

    autoTable(doc, {
      startY: 25,
      head: [
        [
          "Date",
          "Receipt",
          "Vehicle",
          "Driver",
          "Commodity",
          "Supplier",
          "Customer",
          "Weight (kg)",
          "Mode",
          "Status",
        ],
      ],
      body: filteredTransactions.map((t) => [
        t.createdAt
          ? new Date(t.createdAt).toLocaleDateString()
          : "-",
        t.receiptNo || "-",
        t.noPlate || "-",
        t.driverName || "-",
        t.commodityName || "-",
        t.supplierName || "-",
        t.customerName || "-",
        t.firstWeight
          ? Number(t.firstWeight).toLocaleString()
          : "-",
        t.weighMode || "-",
        t.status || "-",
      ]),
      styles: { fontSize: 9 },
      headStyles: {
        fillColor: [251, 191, 36], // yellow / amber
        textColor: 0,
      },
    });

    doc.save("transaction-report.pdf");
  };

  /* =========================
     EXPORT: EXCEL (ADDED)
     ========================= */
  const handleExportExcel = () => {
    const data = filteredTransactions.map((t) => ({
      Date: t.createdAt
        ? new Date(t.createdAt).toLocaleDateString()
        : "",
      Receipt: t.receiptNo || "",
      Vehicle: t.noPlate || "",
      Driver: t.driverName || "",
      Commodity: t.commodityName || "",
      Supplier: t.supplierName || "",
      Customer: t.customerName || "",
      WeightKG: t.firstWeight || "",
      Mode: t.weighMode || "",
      Status: t.status || "",
    }));

    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(workbook, worksheet, "Transactions");
    XLSX.writeFile(workbook, "transaction-report.xlsx");
  };

  return (
  <div className="p-6">
    {/* ================= HEADER ================= */}
    <div className="mb-6">
      <h1 className="text-4xl mb-2">Reports</h1>
      <p className="text-gray-600">
        Transaction-based operational reports
      </p>
    </div>

    {/* ================= FILTERS ================= */}
    <div className="bg-white border rounded-lg p-4 mb-6">
      {/* Top Row */}
      <div className="flex flex-wrap gap-4 items-end">
        {/* Start Date */}
        <div className="flex flex-col">
          <label className="text-sm text-gray-600">Start Date</label>
          <input
            type="date"
            value={filters.startDate}
            onChange={(e) => updateFilter("startDate", e.target.value)}
            className="border rounded px-3 py-2"
          />
        </div>

        {/* End Date */}
        <div className="flex flex-col">
          <label className="text-sm text-gray-600">End Date</label>
          <input
            type="date"
            value={filters.endDate}
            onChange={(e) => updateFilter("endDate", e.target.value)}
            className="border rounded px-3 py-2"
          />
        </div>

        {/* Receipt */}
        <div className="flex flex-col">
          <label className="text-sm text-gray-600">Receipt No</label>
          <input
            type="text"
            placeholder="RCT123"
            value={filters.receiptNumber}
            onChange={(e) =>
              updateFilter("receiptNumber", e.target.value)
            }
            className="border rounded px-3 py-2"
          />
        </div>

        {/* Number Plate */}
        <div className="flex flex-col">
          <label className="text-sm text-gray-600">Number Plate</label>
          <input
            type="text"
            placeholder="KAA 123A"
            value={filters.numberPlate}
            onChange={(e) =>
              updateFilter("numberPlate", e.target.value)
            }
            className="border rounded px-3 py-2"
          />
        </div>

        {/* Status */}
        <div className="flex flex-col">
          <label className="text-sm text-gray-600">Status</label>
          <select
            value={filters.status}
            onChange={(e) => updateFilter("status", e.target.value)}
            className="border rounded px-3 py-2"
          >
            <option value="">All</option>
            <option value="completed">Completed</option>
            <option value="InProgress">In Progress</option>
          </select>
        </div>

        {/* Buttons */}
        <div className="flex gap-2">
          <button
            onClick={handleApplyFilters}
            className="bg-yellow-400 hover:bg-yellow-500 text-black px-4 py-2 rounded flex items-center gap-2"
          >
            <Filter size={16} />
            Apply
          </button>

          <button
            onClick={handleClearFilters}
            className="border px-4 py-2 rounded flex items-center gap-2"
          >
            <RotateCcw size={16} />
            Reset
          </button>

          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="border px-3 py-2 rounded"
          >
            {showAdvanced ? <ChevronUp /> : <ChevronDown />}
          </button>
        </div>
      </div>

      {/* ================= ADVANCED FILTERS ================= */}
      {showAdvanced && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
          {[
            ["driver", "Driver"],
            ["supplier", "Supplier"],
            ["customer", "Customer"],
            ["origin", "Origin"],
            ["destination", "Destination"],
            ["operator", "Operator"],
          ].map(([key, label]) => (
            <div key={key} className="flex flex-col">
              <label className="text-sm text-gray-600">{label}</label>
              <input
                type="text"
                value={filters[key]}
                onChange={(e) => updateFilter(key, e.target.value)}
                className="border rounded px-3 py-2"
              />
            </div>
          ))}
        </div>
      )}
    </div>

    {/* ================= SUMMARY ================= */}
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div className="bg-white border rounded-lg p-4">
        <p className="text-sm text-gray-500">Total Transactions</p>
        <p className="text-2xl font-semibold">{totalTransactions}</p>
      </div>

      <div className="bg-white border rounded-lg p-4">
        <p className="text-sm text-gray-500">Completed</p>
        <p className="text-2xl font-semibold text-green-600">
          {completedTransactions}
        </p>
      </div>

      <div className="bg-white border rounded-lg p-4">
        <p className="text-sm text-gray-500">In-Progress</p>
        <p className="text-2xl font-semibold text-yellow-600">
          {pendingTransactions}
        </p>
      </div>

      <div className="bg-white border rounded-lg p-4">
        <p className="text-sm text-gray-500">Total Net Weight (kg)</p>
        <p className="text-2xl font-semibold">
          {totalNetWeight.toLocaleString()}
        </p>
      </div>
    </div>

    {/* ================= TABLE ================= */}
    <ReportsTable
      transactions={filteredTransactions}
      loading={loading}
      onExportPDF={handleExportPDF}
      onExportExcel={handleExportExcel}
      onPageChange={}
    />
  </div>
);
}