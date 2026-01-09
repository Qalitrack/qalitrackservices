import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function DriverReport({
  transactions = [],
  loading,
  currentPage,
  pageSize,
  onPageChange,
}) {
  const [selectedDriver, setSelectedDriver] = useState(null);

  // =========================
  // Group Transactions by Driver
  // =========================
  const driverSummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const driver = tx.driverName || "Unknown Driver";

      if (!map[driver]) {
        map[driver] = {
          id: driver,
          driverName: driver,
          trips: 0,
          totalNetWeight: 0,
          vehicles: new Set(),
        };
      }

      map[driver].trips += 1;
      map[driver].totalNetWeight += tx.netWeight || 0;

      if (tx.noPlate) {
        map[driver].vehicles.add(tx.noPlate);
      }
    });

    return Object.values(map).map((d) => ({
      ...d,
      vehicles: Array.from(d.vehicles).join(", "),
    }));
  }, [transactions]);

  // =========================
  // KPIs (NOT PAGINATED)
  // =========================
  const totalDrivers = driverSummary.length;
  const totalTrips = driverSummary.reduce((sum, d) => sum + d.trips, 0);
  const totalWeight = driverSummary.reduce(
    (sum, d) => sum + d.totalNetWeight,
    0
  );

  // =========================
  // Paginated Summary Rows
  // =========================
  const paginatedSummaryRows = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;

    return driverSummary.slice(start, end).map((d) => ({
      id: d.id,
      driverName: d.driverName,
      trips: d.trips,
      vehicles: d.vehicles,
      netWeight: d.totalNetWeight.toFixed(2),
    }));
  }, [driverSummary, currentPage, pageSize]);

  // =========================
  // Drill-down: Vehicles & Weights per Driver
  // =========================
  const driverTransactions = useMemo(() => {
    if (!selectedDriver) return [];

    return transactions
      .filter(
        (t) => (t.driverName || "Unknown Driver") === selectedDriver
      )
      .map((t) => ({
        id: t.id,
        vehicle: t.noPlate || "-",
        netWeight: t.netWeight || 0,
      }));
  }, [selectedDriver, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Driver Report</h2>
        <p className="text-sm text-gray-600">
          Summary and detailed breakdown per driver
        </p>
      </div>

      {/* KPIs */}
      {!selectedDriver && (
        <div className="flex gap-6 mb-6 flex-wrap">
          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Drivers</p>
            <p className="text-xl font-semibold">{totalDrivers}</p>
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
      {!selectedDriver && (
        <>
          <ReportsTable
            transactions={paginatedSummaryRows}
            loading={loading}
            onRowClick={(row) => setSelectedDriver(row.driverName)}
            showColumns={["driverName", "trips", "vehicles", "netWeight"]}
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={driverSummary.length}
            pageSize={pageSize}
            onPageChange={onPageChange}
          />
        </>
      )}

      {/* DRILL-DOWN TABLE */}
      {selectedDriver && (
        <div>
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setSelectedDriver(null)}
              className="flex items-center gap-1 text-sm border rounded px-3 py-1"
            >
              <ChevronLeft size={16} /> Back
            </button>

            <h3 className="text-xl font-semibold">
              {selectedDriver} — Vehicle Loads
            </h3>
          </div>

          <ReportsTable
            transactions={driverTransactions}
            loading={loading}
            showColumns={["vehicle", "netWeight"]}
          />
        </div>
      )}
    </div>
  );
}
