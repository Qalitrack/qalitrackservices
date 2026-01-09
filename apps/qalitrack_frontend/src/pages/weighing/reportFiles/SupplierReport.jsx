import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function SupplierReport({
  transactions = [],
  loading,
  currentPage,
  pageSize,
  onPageChange,
}) {
  const [selectedSupplier, setSelectedSupplier] = useState(null);

  // =========================
  // Group Transactions by Supplier
  // =========================
  const supplierSummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const supplier = tx.originName || "Unknown Supplier";

      if (!map[supplier]) {
        map[supplier] = {
          id: supplier,
          supplierName: supplier,
          trips: 0,
          totalNetWeight: 0,
        };
      }

      map[supplier].trips += 1;
      map[supplier].totalNetWeight += Number(tx.netWeight) || 0;
    });

    return Object.values(map);
  }, [transactions]);

  // =========================
  // KPIs (NOT PAGINATED)
  // =========================
  const totalSuppliers = supplierSummary.length;
  const totalTrips = supplierSummary.reduce(
    (sum, s) => sum + s.trips,
    0
  );
  const totalWeight = supplierSummary.reduce(
    (sum, s) => sum + s.totalNetWeight,
    0
  );

  // =========================
  // Paginated Summary Rows
  // =========================
  const paginatedSummaryRows = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;

    return supplierSummary.slice(start, end).map((s) => ({
      id: s.id,
      supplierName: s.supplierName,
      trips: s.trips,
      netWeight: s.totalNetWeight.toFixed(2),
    }));
  }, [supplierSummary, currentPage, pageSize]);

  // =========================
  // Drill-down: Commodities per Supplier
  // =========================
  const supplierTransactions = useMemo(() => {
    if (!selectedSupplier) return [];

    return transactions
      .filter(
        (t) => (t.originName || "Unknown Supplier") === selectedSupplier
      )
      .map((t) => ({
        id: t.id,
        commodityName: t.commodityName || "-",
        netWeight: t.netWeight || 0,
      }));
  }, [selectedSupplier, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Supplier Report</h2>
        <p className="text-sm text-gray-600">
          Summary and detailed breakdown per supplier
        </p>
      </div>

      {/* KPIs */}
      {!selectedSupplier && (
        <div className="flex gap-6 mb-6 flex-wrap">
          <div className="bg-amber-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Suppliers</p>
            <p className="text-xl font-semibold">{totalSuppliers}</p>
          </div>

          <div className="bg-amber-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Transactions</p>
            <p className="text-xl font-semibold">{totalTrips}</p>
          </div>

          <div className="bg-amber-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Net Weight (kg)</p>
            <p className="text-xl font-semibold">
              {totalWeight.toFixed(2)}
            </p>
          </div>
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedSupplier && (
        <>
          <ReportsTable
            transactions={paginatedSummaryRows}
            loading={loading}
            onRowClick={(row) =>
              setSelectedSupplier(row.supplierName)
            }
            showColumns={["supplierName", "trips", "netWeight"]}
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={supplierSummary.length}
            pageSize={pageSize}
            onPageChange={onPageChange}
          />
        </>
      )}

      {/* DRILL-DOWN TABLE */}
      {selectedSupplier && (
        <div>
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setSelectedSupplier(null)}
              className="flex items-center gap-1 text-sm border rounded px-3 py-1"
            >
              <ChevronLeft size={16} /> Back
            </button>

            <h3 className="text-xl font-semibold">
              {selectedSupplier} — Commodities
            </h3>
          </div>

          <ReportsTable
            transactions={supplierTransactions}
            loading={loading}
            showColumns={["commodityName", "netWeight"]}
          />
        </div>
      )}
    </div>
  );
}
