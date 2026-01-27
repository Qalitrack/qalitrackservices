import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft } from "lucide-react"
import dayjs from "dayjs"

export default function CommodityReport({ transactions = [], loading }) {
  const [selectedCommodity, setSelectedCommodity] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 5

  /* =========================
     GROUP BY COMMODITY
  ========================= */
  const commoditySummary = useMemo(() => {
    const map = {}

    transactions.forEach((t) => {
      const name = t.commodityName || "Unknown Commodity"

      if (!map[name]) {
        map[name] = {
          id: name,
          commodityName: name,
          trips: 0,
          totalNetWeight: 0,
          customers: new Set(),
        }
      }

      map[name].trips += 1
      map[name].totalNetWeight += Number(t.netWeight || 0)
      if (t.destinationName) map[name].customers.add(t.destinationName)
    })

    return Object.values(map).map((c) => ({
      ...c,
      customers: Array.from(c.customers).join(", "),
    }))
  }, [transactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalCommodities = commoditySummary.length
  const totalTrips = commoditySummary.reduce((s, c) => s + c.trips, 0)
  const totalWeight = commoditySummary.reduce(
    (s, c) => s + c.totalNetWeight,
    0
  )

  /* =========================
     PAGINATED SUMMARY
  ========================= */
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE

    return commoditySummary.slice(start, start + PAGE_SIZE).map((c) => ({
      id: c.id,
      commodityName: (
        <button
          onClick={() => setSelectedCommodity(c.commodityName)}
          className="text-yellow-600 hover:underline font-medium"
        >
          {c.commodityName}
        </button>
      ),
      trips: c.trips,
      customers: c.customers,
      netWeight: c.totalNetWeight.toLocaleString(),
    }))
  }, [commoditySummary, currentPage])

  /* =========================
     COMMODITY TRIPS (DETAIL PAGE)
  ========================= */
  const commodityTrips = useMemo(() => {
    if (!selectedCommodity) return []

    return transactions
      .filter(
        (t) =>
          (t.commodityName || "Unknown Commodity") === selectedCommodity
      )
      .map((t) => ({
        id: t.id,
        date: dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"),
        vehicle: t.noPlate || "-",
        supplier: t.originName || "-",
        customer: t.destinationName || "-",
        driver: t.driverName || "-",
        netWeight: Number(t.netWeight || 0),
      }))
  }, [transactions, selectedCommodity])

  /* =========================
     COMMODITY KPIs
  ========================= */
  const commodityTotals = useMemo(() => {
    const totalNet = commodityTrips.reduce((s, t) => s + t.netWeight, 0)
    const customers = new Set(commodityTrips.map((t) => t.customer))

    return {
      trips: commodityTrips.length,
      totalNet,
      customers: customers.size,
      avgNet:
        commodityTrips.length > 0
          ? Math.round(totalNet / commodityTrips.length)
          : 0,
    }
  }, [commodityTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* SUMMARY KPIs */}
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

      {/* COMMODITY SUMMARY TABLE */}
      {!selectedCommodity && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={[
              "commodityName",
              "trips",
              "customers",
              "netWeight",
            ]}
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

      {/* COMMODITY DETAIL PAGE */}
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
              {selectedCommodity} — Commodity Report
            </h3>
          </div>

          {/* COMMODITY KPIs */}
          <div className="grid grid-cols-4 gap-3">
            <CompactStat label="Trips" value={commodityTotals.trips} />
            <CompactStat
              label="Total Net"
              value={commodityTotals.totalNet.toLocaleString()}
            />
            <CompactStat
              label="Customers"
              value={commodityTotals.customers}
            />
            <CompactStat
              label="Avg Net"
              value={commodityTotals.avgNet.toLocaleString()}
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
                  <th className="p-2 text-left">Customer</th>
                  <th className="p-2 text-left">Driver</th>
                  <th className="p-2 text-right">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {commodityTrips.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.customer}</td>
                    <td className="p-2">{t.driver}</td>
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
