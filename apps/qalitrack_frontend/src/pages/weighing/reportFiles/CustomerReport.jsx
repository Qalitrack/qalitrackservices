import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft } from "lucide-react"
import dayjs from "dayjs"

export default function CustomerReport({ transactions = [], loading }) {
  const [selectedCustomer, setSelectedCustomer] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 5

  /* =========================
     GROUP BY CUSTOMER
  ========================= */
  const customerSummary = useMemo(() => {
    const map = {}

    transactions.forEach((t) => {
      const name = t.destinationName || "Unknown Customer"

      if (!map[name]) {
        map[name] = {
          id: name,
          customerName: name,
          trips: 0,
          totalNetWeight: 0,
          commodities: new Set(),
        }
      }

      map[name].trips += 1
      map[name].totalNetWeight += Number(t.netWeight || 0)
      if (t.commodityName) map[name].commodities.add(t.commodityName)
    })

    return Object.values(map).map((c) => ({
      ...c,
      commodities: Array.from(c.commodities).join(", "),
    }))
  }, [transactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalCustomers = customerSummary.length
  const totalTrips = customerSummary.reduce((s, c) => s + c.trips, 0)
  const totalWeight = customerSummary.reduce(
    (s, c) => s + c.totalNetWeight,
    0
  )

  /* =========================
     PAGINATED SUMMARY
  ========================= */
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE

    return customerSummary.slice(start, start + PAGE_SIZE).map((c) => ({
      id: c.id,
      customerName: (
        <button
          onClick={() => setSelectedCustomer(c.customerName)}
          className="text-yellow-600 hover:underline font-medium"
        >
          {c.customerName}
        </button>
      ),
      trips: c.trips,
      commodities: c.commodities,
      netWeight: c.totalNetWeight.toLocaleString(),
    }))
  }, [customerSummary, currentPage])

  /* =========================
     CUSTOMER TRIPS (DETAIL PAGE)
  ========================= */
  const customerTrips = useMemo(() => {
    if (!selectedCustomer) return []

    return transactions
      .filter(
        (t) =>
          (t.destinationName || "Unknown Customer") === selectedCustomer
      )
      .map((t) => ({
        id: t.id,
        date: dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"),
        vehicle: t.noPlate || "-",
        supplier: t.originName || "-",
        commodity: t.commodityName || "-",
        driver: t.driverName || "-",
        destination: t.destinationName || "-",
        netWeight: Number(t.netWeight || 0),
      }))
  }, [transactions, selectedCustomer])

  /* =========================
     CUSTOMER KPIs
  ========================= */
  const customerTotals = useMemo(() => {
    const totalNet = customerTrips.reduce((s, t) => s + t.netWeight, 0)
    const commodities = new Set(customerTrips.map((t) => t.commodity))

    return {
      trips: customerTrips.length,
      totalNet,
      commodities: commodities.size,
      avgNet:
        customerTrips.length > 0
          ? Math.round(totalNet / customerTrips.length)
          : 0,
    }
  }, [customerTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* SUMMARY KPIs */}
      {!selectedCustomer && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Customers" value={totalCustomers} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat
            label="Net Weight"
            value={totalWeight.toLocaleString()}
          />
        </div>
      )}

      {/* CUSTOMER SUMMARY TABLE */}
      {!selectedCustomer && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={[
              "customerName",
              "trips",
              "commodities",
              "netWeight",
            ]}
            compact
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={customerSummary.length}
            pageSize={PAGE_SIZE}
            onPageChange={setCurrentPage}
          />
        </>
      )}

      {/* CUSTOMER DETAIL PAGE */}
      {selectedCustomer && (
        <>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setSelectedCustomer(null)}
              className="flex items-center gap-1 text-xs border rounded px-2 py-1"
            >
              <ChevronLeft size={14} /> Back
            </button>

            <h3 className="text-sm font-semibold">
              {selectedCustomer} — Customer Report
            </h3>
          </div>

          {/* CUSTOMER KPIs */}
          <div className="grid grid-cols-4 gap-3">
            <CompactStat label="Trips" value={customerTotals.trips} />
            <CompactStat
              label="Total Net"
              value={customerTotals.totalNet.toLocaleString()}
            />
            <CompactStat
              label="Commodities"
              value={customerTotals.commodities}
            />
            <CompactStat
              label="Avg Net"
              value={customerTotals.avgNet.toLocaleString()}
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
                  <th className="p-2 text-left">Driver</th>
                  <th className="p-2 text-left">Destination</th>
                  <th className="p-2 text-right">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {customerTrips.map((t) => (
                  <tr key={t.id} className="border-t hover:bg-gray-50">
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.commodity}</td>
                    <td className="p-2">{t.driver}</td>
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
