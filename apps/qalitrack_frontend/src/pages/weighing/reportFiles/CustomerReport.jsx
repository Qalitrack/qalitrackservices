import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft, RotateCcw, FileDown, FileSpreadsheet } from "lucide-react"
import dayjs from "dayjs"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"
import * as XLSX from "xlsx"

export default function CustomerReport({ transactions = [], loading }) {
  const [selectedCustomer, setSelectedCustomer] = useState(null)
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
        t.destinationName?.toLowerCase().includes(q) ||
        t.customerName?.toLowerCase().includes(q) ||
        t.commodityName?.toLowerCase().includes(q)
      )
    }

    return data
  }, [transactions, filters])

  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", search: "" })
    setCurrentPage(1)
  }

  /* =========================
     GROUP BY CUSTOMER
  ========================= */
  const customerSummary = useMemo(() => {
    const map = {}

    filteredTransactions.forEach((t) => {
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
  }, [filteredTransactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalCustomers = customerSummary.length
  const totalTrips = customerSummary.reduce((s, c) => s + c.trips, 0)
  const totalWeight = customerSummary.reduce((s, c) => s + c.totalNetWeight, 0)

  /* =========================
     EXPORT FUNCTIONS
  ========================= */
  const exportPDF = () => {
    const doc = new jsPDF()
    
    doc.setFillColor(245, 158, 11)
    doc.rect(0, 0, doc.internal.pageSize.getWidth(), 20, "F")
    doc.setTextColor(255, 255, 255)
    doc.setFontSize(16)
    doc.setFont("helvetica", "bold")
    doc.text("CUSTOMER REPORT", doc.internal.pageSize.getWidth() / 2, 10, { align: "center" })
    doc.setFontSize(10)
    doc.setFont("helvetica", "normal")
    doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, doc.internal.pageSize.getWidth() / 2, 15, { align: "center" })

    doc.setTextColor(0, 0, 0)
    doc.setFontSize(9)
    doc.text(`Total Customers: ${totalCustomers}`, 14, 25)
    doc.text(`Total Trips: ${totalTrips}`, 14, 30)
    doc.text(`Total Weight: ${totalWeight.toLocaleString()} kg`, 14, 35)

    autoTable(doc, {
      startY: 40,
      head: [["Customer Name", "Trips", "Commodities", "Net Weight (kg)"]],
      body: customerSummary.map((c) => [
        c.customerName,
        c.trips,
        c.commodities,
        c.totalNetWeight.toLocaleString(),
      ]),
      styles: { fontSize: 9, cellPadding: 2 },
      headStyles: { fillColor: [245, 158, 11], textColor: [0, 0, 0], fontStyle: 'bold' },
      alternateRowStyles: { fillColor: [250, 250, 250] },
    })

    doc.save(`customer-report-${dayjs().format('YYYY-MM-DD')}.pdf`)
  }

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      customerSummary.map((c) => ({
        'Customer Name': c.customerName,
        'Trips': c.trips,
        'Commodities': c.commodities,
        'Total Net Weight (kg)': c.totalNetWeight,
      }))
    )

    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, "Customers")
    XLSX.writeFile(wb, `customer-report-${dayjs().format('YYYY-MM-DD')}.xlsx`)
  }

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
          className="text-amber-700 hover:underline font-medium"
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

    return filteredTransactions
      .filter((t) => (t.destinationName || "Unknown Customer") === selectedCustomer)
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
  }, [filteredTransactions, selectedCustomer])

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
      avgNet: customerTrips.length > 0 ? Math.round(totalNet / customerTrips.length) : 0,
    }
  }, [customerTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* FILTERS */}
      {!selectedCustomer && (
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
                placeholder="Search customer or commodity..."
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
      {!selectedCustomer && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Customers" value={totalCustomers} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat label="Net Weight" value={totalWeight.toLocaleString()} />
        </div>
      )}

      {/* CUSTOMER SUMMARY TABLE */}
      {!selectedCustomer && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={["customerName", "trips", "commodities", "netWeight"]}
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
              className="flex items-center gap-1 text-xs border rounded px-2 py-1 hover:bg-gray-50"
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
            <CompactStat label="Total Net" value={customerTotals.totalNet.toLocaleString()} />
            <CompactStat label="Commodities" value={customerTotals.commodities} />
            <CompactStat label="Avg Net" value={customerTotals.avgNet.toLocaleString()} />
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
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Driver</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Destination</th>
                  <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {customerTrips.map((t, idx) => (
                  <tr key={t.id} className={`border-t border-gray-100 hover:bg-amber-50/30 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.commodity}</td>
                    <td className="p-2">{t.driver}</td>
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
                    <span className="font-semibold text-amber-700">{customerSummary.length}</span> customers
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
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Customer Name</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Trips</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Commodities</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                    </tr>
                  </thead>
                  <tbody>
                    {customerSummary.map((c, idx) => (
                      <tr key={c.id} className={`border-t border-gray-100 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                        <td className="p-2 font-medium">{c.customerName}</td>
                        <td className="p-2">{c.trips}</td>
                        <td className="p-2">{c.commodities}</td>
                        <td className="p-2 text-right font-bold text-amber-800">{c.totalNetWeight.toLocaleString()}</td>
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