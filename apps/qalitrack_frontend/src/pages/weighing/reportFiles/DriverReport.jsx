import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function DriverReport({ transactions = [], loading }) {
  const [selectedDriver, setSelectedDriver] = useState(null);

  // Pagination (summary only)
  const [currentPage, setCurrentPage] = useState(1);
  const PAGE_SIZE = 5;

  // =========================
  // GROUP BY DRIVER
  // =========================
  const driverSummary = useMemo(() => {
    const map = {};

    transactions.forEach((t) => {
      const name = t.driverName || "Unknown Driver";

      if (!map[name]) {
        map[name] = {
          id: name,
          driverName: name,
          trips: 0,
          totalNetWeight: 0,
          vehicles: new Set(),
        };
      }

      map[name].trips += 1;
      map[name].totalNetWeight += Number(t.netWeight || 0);
      if (t.noPlate) map[name].vehicles.add(t.noPlate);
    });

    return Object.values(map).map((d) => ({
      ...d,
      vehicles: Array.from(d.vehicles).join(", "),
    }));
  }, [transactions]);

  // =========================
  // KPIs
  // =========================
  const totalDrivers = driverSummary.length;
  const totalTrips = driverSummary.reduce((s, d) => s + d.trips, 0);
  const totalWeight = driverSummary.reduce(
    (s, d) => s + d.totalNetWeight,
    0
  );

  // =========================
  // PAGINATED SUMMARY
  // =========================
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return driverSummary.slice(start, start + PAGE_SIZE).map((d) => ({
      id: d.id,
      driverName: d.driverName,
      trips: d.trips,
      vehicles: d.vehicles,
      netWeight: d.totalNetWeight.toLocaleString(),
    }));
  }, [driverSummary, currentPage]);

  // =========================
  // DRIVER DETAIL (TRIPS)
  // =========================
  const driverTrips = useMemo(() => {
    if (!selectedDriver) return [];

    return transactions
      .filter((t) => (t.driverName || "Unknown Driver") === selectedDriver)
      .map((t) => ({
        id: t.id,
        date: new Date(t.createdAt).toLocaleString(),
        vehicle: t.noPlate || "-",
        firstWeight: t.firstWeight || 0,
        secondWeight: t.secondWeight || 0,
        netWeight: t.netWeight || 0,
        source: t.source || "-",
        destination: t.destination || "-",
      }));
  }, [transactions, selectedDriver]);

  return (
    <div className="bg-white border rounded-lg p-5">
      {/* HEADER */}
      <div className="mb-5">
        <h2 className="text-2xl font-semibold">Driver Report</h2>
        <p className="text-sm text-gray-600">
          Summary and detailed trip breakdown per driver
        </p>
      </div>

      {/* KPIs */}
      {!selectedDriver && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-5">
          <div className="bg-yellow-100 p-3 rounded">
            <p className="text-xs text-gray-500">Drivers</p>
            <p className="text-lg font-semibold">{totalDrivers}</p>
          </div>
          <div className="bg-yellow-100 p-3 rounded">
            <p className="text-xs text-gray-500">Trips</p>
            <p className="text-lg font-semibold">{totalTrips}</p>
          </div>
          <div className="bg-yellow-100 p-3 rounded">
            <p className="text-xs text-gray-500">Total Net Weight</p>
            <p className="text-lg font-semibold">
              {totalWeight.toLocaleString()}
            </p>
          </div>
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedDriver && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            onRowClick={(row) => setSelectedDriver(row.driverName)}
            showColumns={["driverName", "trips", "vehicles", "netWeight"]}
            compact
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={driverSummary.length}
            pageSize={PAGE_SIZE}
            onPageChange={setCurrentPage}
          />
        </>
      )}

      {/* DRIVER DETAILS */}
      {selectedDriver && (
        <>
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setSelectedDriver(null)}
              className="flex items-center gap-1 text-sm border rounded px-3 py-1"
            >
              <ChevronLeft size={16} /> Back
            </button>

            <h3 className="text-xl font-semibold">
              {selectedDriver} — Trip Details
            </h3>
          </div>

          <div className="border rounded-lg overflow-auto">
            <table className="w-full text-sm min-w-[1000px]">
              <thead className="bg-gray-50">
                <tr>
                  <th className="p-2 text-left">Date & Time</th>
                  <th className="p-2 text-left">Vehicle</th>
                  <th className="p-2 text-right">1st Weight</th>
                  <th className="p-2 text-right">2nd Weight</th>
                  <th className="p-2 text-right">Net Weight</th>
                  <th className="p-2 text-left">Source</th>
                  <th className="p-2 text-left">Destination</th>
                </tr>
              </thead>

              <tbody>
                {driverTrips.map((t) => (
                  <tr key={t.id} className="border-t">
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2 text-right">
                      {Number(t.firstWeight).toLocaleString()}
                    </td>
                    <td className="p-2 text-right">
                      {Number(t.secondWeight).toLocaleString()}
                    </td>
                    <td className="p-2 text-right font-medium">
                      {Number(t.netWeight).toLocaleString()}
                    </td>
                    <td className="p-2">{t.source}</td>
                    <td className="p-2">{t.destination}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
