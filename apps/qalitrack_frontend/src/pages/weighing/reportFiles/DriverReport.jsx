import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft, RotateCcw, FileDown, FileSpreadsheet } from "lucide-react"
import dayjs from "dayjs"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"
import * as XLSX from "xlsx"

export default function DriverReport({ transactions = [], loading }) {
  const [selectedDriver, setSelectedDriver] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 5

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    search: "",
  })

  // Export preview
  const [showExportPreview, setShowExportPreview] = useState(false)
  const [exportType, setExportType] = useState(null)

  /* =========================
     FILTERED TRANSACTIONS
  ========================= */
  const filteredTransactions = useMemo(() => {
    let data = [...transactions]

    if (filters.startDate) {
      const start = new Date(`${filters.startDate}T00:00`)
      data = data.filter((t) => new Date(t.createdAt) >= start)
    }

    if (filters.endDate) {
      const end = new Date(`${filters.endDate}T23:59`)
      data = data.filter((t) => new Date(t.createdAt) <= end)
    }

    if (filters.search) {
      const q = filters.search.toLowerCase()
      data = data.filter((t) =>
        t.driverName?.toLowerCase().includes(q) ||
        t.noPlate?.toLowerCase().includes(q)
      )
    }

    return data
  }, [transactions, filters])

  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", search: "" })
    setCurrentPage(1)
  }

  /* =========================
     GROUP BY DRIVER
  ========================= */
  const driverSummary = useMemo(() => {
    const map = {}

    filteredTransactions.forEach((t) => {
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
  }, [filteredTransactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalDrivers = driverSummary.length
  const totalTrips = driverSummary.reduce((s, d) => s + d.trips, 0)
  const totalWeight = driverSummary.reduce((s, d) => s + d.totalNetWeight, 0)

  /* =========================
     EXPORT FUNCTIONS
  ========================= */
  const exportPDF = () => {
    const doc = new jsPDF()
    
    // Header
    doc.setFillColor(245, 158, 11)
    doc.rect(0, 0, doc.internal.pageSize.getWidth(), 20, "F")
    doc.setTextColor(255, 255, 255)
    doc.setFontSize(16)
    doc.setFont("helvetica", "bold")
    doc.text("DRIVER REPORT", doc.internal.pageSize.getWidth() / 2, 10, { align: "center" })
    doc.setFontSize(10)
    doc.setFont("helvetica", "normal")
    doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, doc.internal.pageSize.getWidth() / 2, 15, { align: "center" })

    // Summary
    doc.setTextColor(0, 0, 0)
    doc.setFontSize(9)
    doc.text(`Total Drivers: ${totalDrivers}`, 14, 25)
    doc.text(`Total Trips: ${totalTrips}`, 14, 30)
    doc.text(`Total Weight: ${totalWeight.toLocaleString()} kg`, 14, 35)

    // Table
    autoTable(doc, {
      startY: 40,
      head: [["Driver Name", "Trips", "Vehicles", "Net Weight (kg)"]],
      body: driverSummary.map((d) => [
        d.driverName,
        d.trips,
        d.vehicles,
        d.totalNetWeight.toLocaleString(),
      ]),
      styles: { fontSize: 9, cellPadding: 2 },
      headStyles: { fillColor: [245, 158, 11], textColor: [0, 0, 0], fontStyle: 'bold' },
      alternateRowStyles: { fillColor: [250, 250, 250] },
    })

    doc.save(`driver-report-${dayjs().format('YYYY-MM-DD')}.pdf`)
  }

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      driverSummary.map((d) => ({
        'Driver Name': d.driverName,
        'Trips': d.trips,
        'Vehicles': d.vehicles,
        'Total Net Weight (kg)': d.totalNetWeight,
      }))
    )

    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, "Drivers")
    XLSX.writeFile(wb, `driver-report-${dayjs().format('YYYY-MM-DD')}.xlsx`)
  }

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
          className="text-amber-700 hover:underline font-medium"
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

    return filteredTransactions
      .filter((t) => (t.driverName || "Unknown Driver") === selectedDriver)
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
  }, [filteredTransactions, selectedDriver])

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
      avgNet: driverTrips.length > 0 ? Math.round(totalNet / driverTrips.length) : 0,
    }
  }, [driverTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* FILTERS */}
      {!selectedDriver && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-3">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-3">
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Start Date</label>
              <input
                type="date"
                value={filters.startDate}
                onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300"
              />
            </div>

            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">End Date</label>
              <input
                type="date"
                value={filters.endDate}
                onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300"
              />
            </div>

            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Search</label>
              <input
                type="text"
                placeholder="Search driver or vehicle..."
                value={filters.search}
                onChange={(e) => setFilters({ ...filters, search: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300"
              />
            </div>
          </div>

          <div className="flex flex-wrap gap-2">
            <button 
              onClick={clearFilters} 
              className="flex items-center gap-1.5 px-3 py-1.5 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white transition-colors"
            >
              <RotateCcw size={14} />
              Clear
            </button>

            <div className="ml-auto flex gap-2">
              <button
                onClick={() => { setExportType("pdf"); setShowExportPreview(true); }}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200"
              >
                <FileDown size={14} />
                PDF
              </button>

              <button
                onClick={() => { setExportType("excel"); setShowExportPreview(true); }}
                className="flex items-center gap-1.5 px-3 py-1.5 border border-gray-300 bg-white rounded-lg text-xs font-medium hover:bg-gray-50"
              >
                <FileSpreadsheet size={14} />
                Excel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* SUMMARY KPIs */}
      {!selectedDriver && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Drivers" value={totalDrivers} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat label="Net Weight" value={totalWeight.toLocaleString()} />
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
              className="flex items-center gap-1 text-xs border rounded px-2 py-1 hover:bg-gray-50"
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
            <CompactStat label="Total Net" value={driverTotals.totalNet.toLocaleString()} />
            <CompactStat label="Vehicles" value={driverTotals.vehicles} />
            <CompactStat label="Avg Net" value={driverTotals.avgNet.toLocaleString()} />
          </div>

          {/* TRIPS TABLE */}
          <div className="border rounded-lg overflow-auto">
            <table className="w-full text-xs min-w-[1200px]">
              <thead className="bg-amber-50">
                <tr>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Date</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Vehicle</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Supplier</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Commodity</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Customer</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Destination</th>
                  <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {driverTrips.map((t, idx) => (
                  <tr key={t.id} className={`border-t border-gray-100 hover:bg-amber-50/30 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.commodity}</td>
                    <td className="p-2">{t.customer}</td>
                    <td className="p-2">{t.destination}</td>
                    <td className="p-2 text-right font-medium text-amber-800">
                      {t.netWeight.toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* EXPORT PREVIEW MODAL */}
      {showExportPreview && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg w-full max-w-4xl max-h-[90vh] flex flex-col shadow-2xl">
            <div className="p-4 border-b bg-amber-50">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-900">Export Preview</h2>
                  <p className="text-xs text-gray-600 mt-1">
                    <span className="font-semibold text-amber-700">{driverSummary.length}</span> drivers
                  </p>
                </div>
                <button onClick={() => setShowExportPreview(false)} className="text-gray-400 hover:text-gray-600">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <div className="flex-1 overflow-auto p-4">
              <div className="border rounded-lg overflow-auto">
                <table className="w-full text-xs">
                  <thead className="bg-amber-50 sticky top-0">
                    <tr>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Driver Name</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Trips</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Vehicles</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                    </tr>
                  </thead>
                  <tbody>
                    {driverSummary.map((d, idx) => (
                      <tr key={d.id} className={`border-t border-gray-100 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                        <td className="p-2 font-medium">{d.driverName}</td>
                        <td className="p-2">{d.trips}</td>
                        <td className="p-2">{d.vehicles}</td>
                        <td className="p-2 text-right font-bold text-amber-800">{d.totalNetWeight.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="p-4 border-t bg-amber-50/50 flex justify-end gap-2">
              <button onClick={() => setShowExportPreview(false)} className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white">
                Cancel
              </button>
              <button
                onClick={() => { exportType === "pdf" ? exportPDF() : exportExcel(); setShowExportPreview(false); }}
                className="px-4 py-2 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200"
              >
                Download {exportType?.toUpperCase()}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

function CompactStat({ label, value }) {
  return (
    <div className="bg-amber-50 border border-amber-200 rounded px-3 py-2">
      <p className="text-[10px] text-gray-700 uppercase font-semibold">{label}</p>
      <p className="text-lg font-bold leading-tight text-amber-900">{value}</p>
    </div>
  )
}