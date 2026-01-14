import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import { ChevronLeft } from "lucide-react";

export default function CommodityReport({ transactions = [], loading }) {
  const [selectedCommodity, setSelectedCommodity] = useState(null);

  // =========================
  // Group by Commodity
  // =========================
  const commoditySummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const commodity = tx.commodityName || "Unknown Commodity";

      if (!map[commodity]) {
        map[commodity] = {
          id: commodity,
          destinationName: commodity, // 👈 mapped for table
          count: 0,
          netWeight: 0,
        };
      }

      map[commodity].count += 1;
      map[commodity].netWeight += Number(tx.netWeight) || 0;
    });

    return Object.values(map);
  }, [transactions]);

  // =========================
  // KPIs
  // =========================
  const totalCommodities = commoditySummary.length;
  const totalTransactions = commoditySummary.reduce(
    (s, c) => s + c.count,
    0
  );
  const totalWeight = commoditySummary.reduce(
    (s, c) => s + c.netWeight,
    0
  );

  // =========================
  // Drill-down rows
  // =========================
  const commodityRows = useMemo(() => {
    if (!selectedCommodity) return [];

    return transactions
      .filter(
        (t) => (t.commodityName || "Unknown Commodity") === selectedCommodity
      )
      .map((t) => ({
        id: t.id,
        destinationName: selectedCommodity, // Commodity
        originName: t.originName || "-",     // Supplier
        netWeight: t.netWeight || 0,
      }));
  }, [selectedCommodity, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Commodity Report</h2>
        <p className="text-sm text-gray-600">
          Weight breakdown per commodity
        </p>
      </div>

      {/* KPIs */}
      {!selectedCommodity && (
        <div className="flex gap-6 mb-6 flex-wrap">
          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Commodities</p>
            <p className="text-xl font-semibold">{totalCommodities}</p>
          </div>

          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Transactions</p>
            <p className="text-xl font-semibold">{totalTransactions}</p>
          </div>

          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Net Weight (kg)</p>
            <p className="text-xl font-semibold">
              {totalWeight.toFixed(2)}
            </p>
          </div>
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedCommodity && (
        <ReportsTable
          transactions={commoditySummary}
          loading={loading}
          showColumns={["destinationName", "netWeight"]}
          onRowClick={(row) => setSelectedCommodity(row.destinationName)}
          totalRecords={commoditySummary.length}
          pageSize={commoditySummary.length}
        />
      )}

      {/* DRILL-DOWN */}
      {selectedCommodity && (
        <div>
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setSelectedCommodity(null)}
              className="flex items-center gap-1 text-sm border rounded px-3 py-1"
            >
              <ChevronLeft size={16} /> Back
            </button>

            <h3 className="text-xl font-semibold">
              {selectedCommodity} — Suppliers
            </h3>
          </div>

          <ReportsTable
            transactions={commodityRows}
            loading={loading}
            showColumns={["originName", "netWeight"]}
            totalRecords={commodityRows.length}
            pageSize={commodityRows.length}
          />
        </div>
      )}
    </div>
  );
}
