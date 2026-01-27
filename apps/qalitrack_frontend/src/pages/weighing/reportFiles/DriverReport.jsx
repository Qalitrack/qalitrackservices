import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft } from "lucide-react"
import dayjs from "dayjs"

export default function DriverReport({ transactions = [], loading }) {
  const [selectedDriver, setSelectedDriver] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 5

  /* =========================
     GROUP BY DRIVER
  ========================= */
  const driverSummary = useMemo(() => {
    const map = {}

    transactions.forEach((t) => {
      const name = t.driverName || "Unknown Driver"

      if (!map[name]) {
        map[name] = {
          id: name,
          driverName: name,
          trips: 0,
          totalNetWeight: 0,
          vehicles: new Set(),
        }
      }

      map[name].trips += 1
      map[name].totalNetWeight += Number(t.netWeight || 0)
      if (t.noPlate) map[name].vehicles.add(t.noPlate)
    })

    return Object.values(map).map((d) => ({
      ...d,
      vehicles: Array.from(d.vehicles).join(", "),
    }))
  }, [transactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalDrivers = driverSummary.length
  const totalTrips = driverSummary.reduce((s, d) => s + d.trips, 0)
  const totalWeight = driverSummary.reduce(
    (s, d) => s + d.totalNetWeight,
    0
  )

  /* =========================
     PAGINATED SUMMARY
  ========================= */
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE

    return driverSummary.slice(start, start + PAGE_SIZE).map((d) => ({
      id: d.id,
      driverName: (
        <button
          onClick={() => setSelectedDriver(d.driverName)}
          className="text-yellow-600 hover:underline font-medium"
        >
          {d.driverName}
        </button>
      ),
      trips: d.trips,
      vehicles: d.vehicles,
      netWeight: d.totalNetWeight.toLocaleString(),
    }))
  }, [driverSummary, currentPage])

  /* =========================
     DRIVER TRIPS (DETAIL PAGE)
  ========================= */
  const driverTrips = useMemo(() => {
    if (!selectedDriver) return []

    return transactions
      .filter(
        (t) => (t.driverName || "Unknown Driver") === selectedDriver
      )
      .map((t) => ({
        id: t.id,
        date: dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"),
        vehicle: t.noPlate || "-",
        supplier: t.originName || "-",
        commodity: t.commodityName || "-",
        customer: t.customerName || "-",
        destination: t.destinationName || "-",
        netWeight: Number(t.netWeight || 0),
      }))
  }, [transactions, selectedDriver])

  /* =========================
     DRIVER KPIs
  ========================= */
  const driverTotals = useMemo(() => {
    const totalNet = driverTrips.reduce((s, t) => s + t.netWeight, 0)
    const vehicles = new Set(driverTrips.map((t) => t.vehicle))

    return {
      trips: driverTrips.length,
      totalNet,
      vehicles: vehicles.size,
      avgNet:
        driverTrips.length > 0
          ? Math.round(totalNet / driverTrips.length)
          : 0,
    }
  }, [driverTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* SUMMARY KPIs */}
      {!selectedDriver && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Drivers" value={totalDrivers} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat
            label="Net Weight"
            value={totalWeight.toLocaleString()}
          />
        </div>
      )}

      {/* DRIVER SUMMARY TABLE */}
      {!selectedDriver && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
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

      {/* DRIVER DETAIL PAGE */}
      {selectedDriver && (
        <>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setSelectedDriver(null)}
              className="flex items-center gap-1 text-xs border rounded px-2 py-1"
            >
              <ChevronLeft size={14} /> Back
            </button>

            <h3 className="text-sm font-semibold">
              {selectedDriver} — Driver Report
            </h3>
          </div>

          {/* DRIVER KPIs */}
          <div className="grid grid-cols-4 gap-3">
            <CompactStat label="Trips" value={driverTotals.trips} />
            <CompactStat
              label="Total Net"
              value={driverTotals.totalNet.toLocaleString()}
            />
            <CompactStat label="Vehicles" value={driverTotals.vehicles} />
            <CompactStat
              label="Avg Net"
              value={driverTotals.avgNet.toLocaleString()}
            />
          </div>

          {/* TRIPS TABLE */}
          <div className="border rounded-lg overflow-auto">
            <table className="w-full text-sm min-w-[1200px]">
              <thead className="bg-yellow-50">
                <tr>
                  <th className="p-2 text-left">Date</th>
                  <th className="p-2 text-left">Vehicle</th>
                  <th className="p-2 text-left">Supplier</th>
                  <th className="p-2 text-left">Commodity</th>
                  <th className="p-2 text-left">Customer</th>
                  <th className="p-2 text-left">Destination</th>
                  <th className="p-2 text-right">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {driverTrips.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.commodity}</td>
                    <td className="p-2">{t.customer}</td>
                    <td className="p-2">{t.destination}</td>
                    <td className="p-2 text-right font-medium">
                      {t.netWeight.toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}

function CompactStat({ label, value }) {
  return (
    <div className="bg-yellow-50 border rounded px-3 py-2">
      <p className="text-[11px] text-gray-500 uppercase">{label}</p>
      <p className="text-lg font-semibold leading-tight">{value}</p>
    </div>
  )
}
