import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import { fetchTransactions } from "../../store/weighingSlice";
import {
  ChevronDown,
  ChevronUp,
  Filter,
  RotateCcw,
} from "lucide-react";

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
  const { transactions: allTransactions, loading } = useSelector(
    (state) => state.weighing
  );

  /* =========================
     Local State
     ========================= */
  const [filteredTransactions, setFilteredTransactions] = useState([]);
  const [showAdvanced, setShowAdvanced] = useState(false);

  /* =========================
     Pagination State
     ========================= */
  const [currentPage, setCurrentPage] = useState(1);
  const rowsPerPage = 10;

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

  /* Reset page when filters change */
  useEffect(() => {
    setCurrentPage(1);
  }, [filteredTransactions]);

  /* =========================
     Helpers
     ========================= */
  const updateFilter = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  /* =========================
     Apply Filters
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
     Pagination Logic
     ========================= */
  const totalPages = Math.ceil(
    filteredTransactions.length / rowsPerPage
  );

  const startIndex = (currentPage - 1) * rowsPerPage;
  const endIndex = startIndex + rowsPerPage;

  const paginatedTransactions = filteredTransactions.slice(
    startIndex,
    endIndex
  );

  const handlePageChange = (page) => {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  };

  /* =========================
     EXPORT: PDF
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
        fillColor: [251, 191, 36],
        textColor: 0,
      },
    });

    doc.save("transaction-report.pdf");
  };

  /* =========================
     EXPORT: EXCEL
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
      {/* HEADER */}
      <div className="mb-6">
        <h1 className="text-4xl mb-2">Reports</h1>
        <p className="text-gray-600">
          Transaction-based operational reports
        </p>
      </div>

      {/* FILTERS */}
      <div className="bg-white border rounded-lg p-4 mb-6">
        <div className="flex flex-wrap gap-4 items-end">
          <input
            type="date"
            value={filters.startDate}
            onChange={(e) =>
              updateFilter("startDate", e.target.value)
            }
            className="border rounded px-3 py-2"
          />

          <input
            type="date"
            value={filters.endDate}
            onChange={(e) =>
              updateFilter("endDate", e.target.value)
            }
            className="border rounded px-3 py-2"
          />

          <input
            type="text"
            placeholder="Receipt No"
            value={filters.receiptNumber}
            onChange={(e) =>
              updateFilter("receiptNumber", e.target.value)
            }
            className="border rounded px-3 py-2"
          />

          <input
            type="text"
            placeholder="Number Plate"
            value={filters.numberPlate}
            onChange={(e) =>
              updateFilter("numberPlate", e.target.value)
            }
            className="border rounded px-3 py-2"
          />

          <select
            value={filters.status}
            onChange={(e) =>
              updateFilter("status", e.target.value)
            }
            className="border rounded px-3 py-2"
          >
            <option value="">All</option>
            <option value="completed">Completed</option>
            <option value="InProgress">In Progress</option>
          </select>

          <button
            onClick={handleApplyFilters}
            className="bg-yellow-400 px-4 py-2 rounded"
          >
            <Filter size={16} />
          </button>

          <button
            onClick={handleClearFilters}
            className="border px-4 py-2 rounded"
          >
            <RotateCcw size={16} />
          </button>

          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="border px-3 py-2 rounded"
          >
            {showAdvanced ? <ChevronUp /> : <ChevronDown />}
          </button>
        </div>
      </div>

      {/* TABLE */}
      <ReportsTable
        transactions={paginatedTransactions}
        loading={loading}
        onExportPDF={handleExportPDF}
        onExportExcel={handleExportExcel}
      />

      {/* PAGINATION */}
      <div className="flex justify-between items-center mt-4">
        <p className="text-sm text-gray-600">
          Page {currentPage} of {totalPages}
        </p>

        <div className="flex gap-2">
          <button
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            Previous
          </button>

          {Array.from({ length: totalPages }, (_, i) => i + 1).map(
            (page) => (
              <button
                key={page}
                onClick={() => handlePageChange(page)}
                className={`px-3 py-1 border rounded ${
                  page === currentPage
                    ? "bg-yellow-400"
                    : ""
                }`}
              >
                {page}
              </button>
            )
          )}

          <button
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
