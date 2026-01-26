import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function CommodityReport({ transactions = [], loading }) {
  const [selectedCommodity, setSelectedCommodity] = useState(null);

  const [currentPage, setCurrentPage] = useState(1);
  const PAGE_SIZE = 5;

  // =========================
  // Summary
  // =========================
  const commoditySummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const commodity = tx.commodityName || "Unknown Commodity";

      if (!map[commodity]) {
        map[commodity] = {
          id: commodity,
          commodityName: commodity,
          trips: 0,
          totalNetWeight: 0,
        };
      }

      map[commodity].trips += 1;
      map[commodity].totalNetWeight += Number(tx.netWeight) || 0;
    });

    return Object.values(map);
  }, [transactions]);

  // =========================
  // KPIs
  // =========================
  const totalCommodities = commoditySummary.length;
  const totalTrips = commoditySummary.reduce((s, c) => s + c.trips, 0);
  const totalWeight = commoditySummary.reduce(
    (s, c) => s + c.totalNetWeight,
    0
  );

  // =========================
  // Paginated Summary
  // =========================
  const paginatedSummaryRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return commoditySummary.slice(start, start + PAGE_SIZE).map((c) => ({
      id: c.id,
      commodityName: c.commodityName,
      trips: c.trips,
      netWeight: c.totalNetWeight.toLocaleString(),
    }));
  }, [commoditySummary, currentPage]);

  // =========================
  // Drill-down
  // =========================
  const commodityTransactions = useMemo(() => {
    if (!selectedCommodity) return [];

    return transactions
      .filter(
        (t) => (t.commodityName || "Unknown Commodity") === selectedCommodity
      )
      .map((t) => ({
        id: t.id,
        supplier: t.originName || "-",
        netWeight: t.netWeight || 0,
      }));
  }, [selectedCommodity, transactions]);

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* KPIs */}
      {!selectedCommodity && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Commodities" value={totalCommodities} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat
            label="Net Weight"
            value={totalWeight.toLocaleString()}
          />
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedCommodity && (
        <>
          <ReportsTable
            transactions={paginatedSummaryRows}
            loading={loading}
            onRowClick={(row) => setSelectedCommodity(row.commodityName)}
            showColumns={["commodityName", "trips", "netWeight"]}
            compact
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={commoditySummary.length}
            pageSize={PAGE_SIZE}
            onPageChange={setCurrentPage}
          />
        </>
      )}

      {/* DRILL-DOWN */}
      {selectedCommodity && (
        <>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setSelectedCommodity(null)}
              className="flex items-center gap-1 text-xs border rounded px-2 py-1"
            >
              <ChevronLeft size={14} /> Back
            </button>

            <h3 className="text-sm font-semibold">
              {selectedCommodity} — Suppliers
            </h3>
          </div>

          <ReportsTable
            transactions={commodityTransactions}
            loading={loading}
            showColumns={["supplier", "netWeight"]}
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
