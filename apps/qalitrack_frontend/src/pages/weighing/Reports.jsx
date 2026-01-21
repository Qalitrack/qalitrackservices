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

  const REPORT_TABS = ["transactions", "drivers", "customers", "commodities", "suppliers"];
  const [activeTab, setActiveTab] = useState("transactions");

  // Pagination (TABLE ONLY)
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(4);

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: "",
    search: "",
  });

  // Export preview
  const [showExportPreview, setShowExportPreview] = useState(false);
  const [exportType, setExportType] = useState(null); // "pdf" | "excel"

  useEffect(() => {
    dispatch(fetchTransactions());
  }, [dispatch]);

  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // =========================
  // FILTERED DATA (SINGLE SOURCE OF TRUTH)
  // =========================
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      const start = new Date(`${filters.startDate}T${filters.startTime || "00:00"}`);
      data = data.filter((t) => new Date(t.createdAt) >= start);
    }

    if (filters.endDate) {
      const end = new Date(`${filters.endDate}T${filters.endTime || "23:59"}`);
      data = data.filter((t) => new Date(t.createdAt) <= end);
    }

    if (filters.status) {
      data = data.filter(
        (t) => String(t.status).toLowerCase() === filters.status.toLowerCase()
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

  // =========================
  // EXPORTS (USE FULL FILTERED DATA)
  // =========================
  const exportPDF = () => {
    const doc = new jsPDF("landscape");
    const pageWidth = doc.internal.pageSize.getWidth();

    doc.setFillColor(251, 191, 36);
    doc.rect(0, 0, pageWidth, 15, "F");
    doc.setFontSize(14);
    doc.text("TRANSACTIONS REPORT", pageWidth / 2, 10, { align: "center" });

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
      body: filteredTransactions.map((t) => [
        new Date(t.createdAt).toLocaleString(),
        t.receiptNo,
        t.noPlate,
        t.driverName,
        t.commodityName,
        t.supplierName,
        t.customerName,
        t.firstWeight,
        t.netWeight,
        t.status,
      ]),
      styles: { fontSize: 9 },
      headStyles: { fillColor: [251, 191, 36], textColor: 0 },
    });

    doc.save("transaction-report.pdf");
  };

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map((t) => ({
        DateTime: new Date(t.createdAt).toLocaleString(),
        Receipt: t.receiptNo,
        Vehicle: t.noPlate,
        Driver: t.driverName,
        Commodity: t.commodityName,
        Supplier: t.supplierName,
        Customer: t.customerName,
        FirstWeight: t.firstWeight,
        NetWeight: t.netWeight,
        Status: t.status,
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

  const renderActiveReport = () => {
    if (activeTab !== "transactions") {
      const ComponentMap = {
        drivers: DriverReport,
        customers: CustomerReport,
        commodities: CommodityReport,
        suppliers: SupplierReport,
      };
      const Component = ComponentMap[activeTab];
      return <Component transactions={filteredTransactions} loading={loading} />;
    }

    return (
      <>
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
          onPageSizeChange={setPageSize}
        />
      </>
    );
  };

  return (
    <div className="p-6">
      <h1 className="text-4xl mb-2">Reports</h1>
      <p className="text-gray-600 mb-6">
        Operational and analytical system reports
      </p>

      {/* TABS */}
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

      {/* FILTERS */}
      {activeTab === "transactions" && (
        <div className="bg-white border rounded-lg p-4 mb-6 flex gap-4 flex-wrap items-end">
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

          <div className="ml-auto flex gap-2">
            <button
              onClick={() => {
                setExportType("pdf");
                setShowExportPreview(true);
              }}
              className="px-4 py-2 bg-yellow-400 rounded"
            >
              Preview PDF
            </button>

            <button
              onClick={() => {
                setExportType("excel");
                setShowExportPreview(true);
              }}
              className="px-4 py-2 border rounded"
            >
              Preview Excel
            </button>
          </div>
        </div>
      )}

      {renderActiveReport()}

      {/* =========================
          EXPORT PREVIEW MODAL
          ========================= */}
      {showExportPreview && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg w-5/6 max-h-[90vh] flex flex-col">

            <h2 className="text-xl font-bold mb-2">
              Preview ({filteredTransactions.length} records)
            </h2>

            <p className="text-sm text-gray-600 mb-4">
              Showing all filtered transactions
            </p>

            <div className="border rounded-lg overflow-auto flex-1 mb-4">
              <table className="w-full text-sm min-w-[1000px]">
                <thead className="bg-gray-50 sticky top-0">
                  <tr>
                    <th className="p-2 text-left">Date</th>
                    <th className="p-2 text-left">Receipt</th>
                    <th className="p-2 text-left">Vehicle</th>
                    <th className="p-2 text-left">Driver</th>
                    <th className="p-2 text-left">Commodity</th>
                    <th className="p-2 text-right">Net Weight</th>
                    <th className="p-2 text-left">Status</th>
                  </tr>
                </thead>

                <tbody>
                  {filteredTransactions.map((t) => (
                    <tr key={t.id} className="border-t">
                      <td className="p-2">
                        {new Date(t.createdAt).toLocaleString()}
                      </td>
                      <td className="p-2">{t.receiptNo}</td>
                      <td className="p-2">{t.noPlate}</td>
                      <td className="p-2">{t.driverName}</td>
                      <td className="p-2">{t.commodityName}</td>
                      <td className="p-2 text-right">
                        {t.netWeight?.toLocaleString()}
                      </td>
                      <td className="p-2">{t.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex justify-end gap-2">
              <button
                onClick={() => {
                  exportType === "pdf" ? exportPDF() : exportExcel();
                  setShowExportPreview(false);
                }}
                className="px-4 py-2 bg-yellow-400 rounded"
              >
                Download {exportType?.toUpperCase()}
              </button>

              <button
                onClick={() => setShowExportPreview(false)}
                className="px-4 py-2 border rounded"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
