import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import { ChevronLeft } from "lucide-react";

export default function SupplierReport({ transactions = [], loading }) {
  const [selectedSupplier, setSelectedSupplier] = useState(null);

  // =========================
  // Group by Supplier
  // =========================
  const supplierSummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const supplier = tx.originName || "Unknown Supplier";

      if (!map[supplier]) {
        map[supplier] = {
          id: supplier,
          originName: supplier,
          count: 0,
          netWeight: 0,
        };
      }

      map[supplier].count += 1;
      map[supplier].netWeight += Number(tx.netWeight) || 0;
    });

    return Object.values(map);
  }, [transactions]);

  // =========================
  // KPIs
  // =========================
  const totalSuppliers = supplierSummary.length;
  const totalTransactions = supplierSummary.reduce(
    (s, sup) => s + sup.count,
    0
  );
  const totalWeight = supplierSummary.reduce(
    (s, sup) => s + sup.netWeight,
    0
  );

  // =========================
  // Drill-down rows
  // =========================
  const supplierRows = useMemo(() => {
    if (!selectedSupplier) return [];

    return transactions
      .filter(
        (t) => (t.originName || "Unknown Supplier") === selectedSupplier
      )
      .map((t) => ({
        id: t.id,
        originName: selectedSupplier,          // Supplier
        destinationName: t.commodityName || "-", // Commodity (mapped)
        netWeight: t.netWeight || 0,
      }));
  }, [selectedSupplier, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Supplier Report</h2>
        <p className="text-sm text-gray-600">
          Supply weight analysis per supplier
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
            <p className="text-xl font-semibold">{totalTransactions}</p>
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
        <ReportsTable
          transactions={supplierSummary}
          loading={loading}
          showColumns={["originName", "netWeight"]}
          onRowClick={(row) => setSelectedSupplier(row.originName)}
          totalRecords={supplierSummary.length}
          pageSize={supplierSummary.length}
        />
      )}

      {/* DRILL-DOWN */}
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
            transactions={supplierRows}
            loading={loading}
            showColumns={["destinationName", "netWeight"]}
            totalRecords={supplierRows.length}
            pageSize={supplierRows.length}
          />
        </div>
      )}
    </div>
  );
}
