import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function SupplierReport({ transactions = [], loading }) {
  const [selectedSupplier, setSelectedSupplier] = useState(null);

  const [currentPage, setCurrentPage] = useState(1);
  const PAGE_SIZE = 5;

  // =========================
  // Summary
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
  // KPIs
  // =========================
  const totalSuppliers = supplierSummary.length;
  const totalTrips = supplierSummary.reduce((s, d) => s + d.trips, 0);
  const totalWeight = supplierSummary.reduce(
    (s, d) => s + d.totalNetWeight,
    0
  );

  // =========================
  // Paginated Summary
  // =========================
  const paginatedSummaryRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return supplierSummary.slice(start, start + PAGE_SIZE).map((s) => ({
      id: s.id,
      supplierName: s.supplierName,
      trips: s.trips,
      netWeight: s.totalNetWeight.toLocaleString(),
    }));
  }, [supplierSummary, currentPage]);

  // =========================
  // Drill-down
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
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* KPIs */}
      {!selectedSupplier && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Suppliers" value={totalSuppliers} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat
            label="Net Weight"
            value={totalWeight.toLocaleString()}
          />
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedSupplier && (
        <>
          <ReportsTable
            transactions={paginatedSummaryRows}
            loading={loading}
            onRowClick={(row) => setSelectedSupplier(row.supplierName)}
            showColumns={["supplierName", "trips", "netWeight"]}
            compact
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={supplierSummary.length}
            pageSize={PAGE_SIZE}
            onPageChange={setCurrentPage}
          />
        </>
      )}

      {/* DRILL-DOWN */}
      {selectedSupplier && (
        <>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setSelectedSupplier(null)}
              className="flex items-center gap-1 text-xs border rounded px-2 py-1"
            >
              <ChevronLeft size={14} /> Back
            </button>

            <h3 className="text-sm font-semibold">
              {selectedSupplier} — Commodities
            </h3>
          </div>

          <ReportsTable
            transactions={supplierTransactions}
            loading={loading}
            showColumns={["commodityName", "netWeight"]}
            compact
          />
        </>
      )}
    </div>
  );
}

function CompactStat({ label, value }) {
  return (
    <div className="bg-yellow-50 border rounded px-3 py-2">
      <p className="text-[11px] text-gray-500 uppercase">{label}</p>
      <p className="text-lg font-semibold leading-tight">{value}</p>
    </div>
  );
}
