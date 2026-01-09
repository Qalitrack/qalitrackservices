import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function CommodityReport({
  transactions = [],
  loading,
  currentPage,
  pageSize,
  onPageChange,
}) {
  const [selectedCommodity, setSelectedCommodity] = useState(null);

  // =========================
  // Group Transactions by Commodity
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
  // KPIs (NOT PAGINATED)
  // =========================
  const totalCommodities = commoditySummary.length;
  const totalTrips = commoditySummary.reduce(
    (sum, c) => sum + c.trips,
    0
  );
  const totalWeight = commoditySummary.reduce(
    (sum, c) => sum + c.totalNetWeight,
    0
  );

  // =========================
  // Paginated Summary Rows
  // =========================
  const paginatedSummaryRows = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;

    return commoditySummary.slice(start, end).map((c) => ({
      id: c.id,
      commodityName: c.commodityName,
      trips: c.trips,
      netWeight: c.totalNetWeight.toFixed(2),
    }));
  }, [commoditySummary, currentPage, pageSize]);

  // =========================
  // Drill-down: Suppliers per Commodity
  // =========================
  const commodityTransactions = useMemo(() => {
    if (!selectedCommodity) return [];

    return transactions
      .filter(
        (t) =>
          (t.commodityName || "Unknown Commodity") === selectedCommodity
      )
      .map((t) => ({
        id: t.id,
        supplier: t.originName || "-",
        netWeight: t.netWeight || 0,
      }));
  }, [selectedCommodity, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Commodity Report</h2>
        <p className="text-sm text-gray-600">
          Summary and detailed breakdown per commodity
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
            <p className="text-xl font-semibold">{totalTrips}</p>
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
        <>
          <ReportsTable
            transactions={paginatedSummaryRows}
            loading={loading}
            onRowClick={(row) =>
              setSelectedCommodity(row.commodityName)
            }
            showColumns={["commodityName", "trips", "netWeight"]}
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={commoditySummary.length}
            pageSize={pageSize}
            onPageChange={onPageChange}
          />
        </>
      )}

      {/* DRILL-DOWN TABLE */}
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
            transactions={commodityTransactions}
            loading={loading}
            showColumns={["supplier", "netWeight"]}
          />
        </div>
      )}
    </div>
  );
}
