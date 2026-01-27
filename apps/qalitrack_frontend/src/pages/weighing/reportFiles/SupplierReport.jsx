import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft } from "lucide-react"
import dayjs from "dayjs"

export default function SupplierReport({ transactions = [], loading }) {
  const [selectedSupplier, setSelectedSupplier] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 5

  /* =========================
     GROUP BY SUPPLIER
  ========================= */
  const supplierSummary = useMemo(() => {
    const map = {}

    transactions.forEach((t) => {
      const name = t.originName || "Unknown Supplier"

      if (!map[name]) {
        map[name] = {
          id: name,
          supplierName: name,
          trips: 0,
          totalNetWeight: 0,
          commodities: new Set(),
        }
      }

      map[name].trips += 1
      map[name].totalNetWeight += Number(t.netWeight || 0)
      if (t.commodityName) map[name].commodities.add(t.commodityName)
    })

    return Object.values(map).map((s) => ({
      ...s,
      commodities: Array.from(s.commodities).join(", "),
    }))
  }, [transactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalSuppliers = supplierSummary.length
  const totalTrips = supplierSummary.reduce((s, d) => s + d.trips, 0)
  const totalWeight = supplierSummary.reduce(
    (s, d) => s + d.totalNetWeight,
    0
  )

  /* =========================
     PAGINATED SUMMARY
  ========================= */
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE

    return supplierSummary.slice(start, start + PAGE_SIZE).map((s) => ({
      id: s.id,
      supplierName: (
        <button
          onClick={() => setSelectedSupplier(s.supplierName)}
          className="text-yellow-600 hover:underline font-medium"
        >
          {s.supplierName}
        </button>
      ),
      trips: s.trips,
      commodities: s.commodities,
      netWeight: s.totalNetWeight.toLocaleString(),
    }))
  }, [supplierSummary, currentPage])

  /* =========================
     SUPPLIER TRANSACTIONS
  ========================= */
  const supplierTrips = useMemo(() => {
    if (!selectedSupplier) return []

    return transactions
      .filter(
        (t) => (t.originName || "Unknown Supplier") === selectedSupplier
      )
      .map((t) => ({
        id: t.id,
        date: dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"),
        vehicle: t.noPlate || "-",
        driver: t.driverName || "-",
        commodity: t.commodityName || "-",
        customer: t.customerName || "-",
        destination: t.destinationName || "-",
        netWeight: Number(t.netWeight || 0),
      }))
  }, [transactions, selectedSupplier])

  /* =========================
     SUPPLIER KPIs
  ========================= */
  const supplierTotals = useMemo(() => {
    const totalNet = supplierTrips.reduce((s, t) => s + t.netWeight, 0)
    const commodities = new Set(supplierTrips.map((t) => t.commodity))

    return {
      trips: supplierTrips.length,
      totalNet,
      commodities: commodities.size,
      avgNet:
        supplierTrips.length > 0
          ? Math.round(totalNet / supplierTrips.length)
          : 0,
    }
  }, [supplierTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* SUMMARY KPIs */}
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

      {/* SUPPLIER SUMMARY TABLE */}
      {!selectedSupplier && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={["supplierName", "trips", "commodities", "netWeight"]}
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

      {/* SUPPLIER DETAIL PAGE */}
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
              {selectedSupplier} — Supplier Report
            </h3>
          </div>

          {/* SUPPLIER KPIs */}
          <div className="grid grid-cols-4 gap-3">
            <CompactStat label="Trips" value={supplierTotals.trips} />
            <CompactStat
              label="Total Net"
              value={supplierTotals.totalNet.toLocaleString()}
            />
            <CompactStat
              label="Commodities"
              value={supplierTotals.commodities}
            />
            <CompactStat
              label="Avg Net"
              value={supplierTotals.avgNet.toLocaleString()}
            />
          </div>

          {/* TRANSACTIONS TABLE */}
          <div className="border rounded-lg overflow-auto">
            <table className="w-full text-sm min-w-[1200px]">
              <thead className="bg-yellow-50">
                <tr>
                  <th className="p-2 text-left">Date</th>
                  <th className="p-2 text-left">Vehicle</th>
                  <th className="p-2 text-left">Driver</th>
                  <th className="p-2 text-left">Commodity</th>
                  <th className="p-2 text-left">Customer</th>
                  <th className="p-2 text-left">Destination</th>
                  <th className="p-2 text-right">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {supplierTrips.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.driver}</td>
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
