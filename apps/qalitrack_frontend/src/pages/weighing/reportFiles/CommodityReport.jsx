import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft, RotateCcw, FileDown, FileSpreadsheet } from "lucide-react"
import dayjs from "dayjs"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"
import * as XLSX from "xlsx"
import logoSrc from "../../../assets/logo.jpeg"
import { getTicketSettings, resolveReportColors } from "../../../utils/ticketThemeConfig"

export default function CommodityReport({ transactions = [], loading }) {
  const [selectedCommodity, setSelectedCommodity] = useState(null)
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
        t.commodityName?.toLowerCase().includes(q) ||
        t.destinationName?.toLowerCase().includes(q)
      )
    }

    return data
  }, [transactions, filters])

  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", search: "" })
    setCurrentPage(1)
  }

  /* =========================
     GROUP BY COMMODITY
  ========================= */
  const commoditySummary = useMemo(() => {
    const map = {}

    filteredTransactions.forEach((t) => {
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
  }, [filteredTransactions])

  /* =========================
     KPIs (SUMMARY)
  ========================= */
  const totalCommodities = commoditySummary.length
  const totalTrips = commoditySummary.reduce((s, c) => s + c.trips, 0)
  const totalWeight = commoditySummary.reduce((s, c) => s + c.totalNetWeight, 0)

  /* =========================
     EXPORT FUNCTIONS
  ========================= */
  const exportPDF = async () => {
    const settings    = getTicketSettings()
    const companyName = settings.companyName    || "QALIBRATED SYSTEMS LTD"
    const companyAddr = settings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996"

    const doc = new jsPDF("landscape", "mm", "a4")
    const PW = doc.internal.pageSize.getWidth()
    const L = 14, R = PW - 14, TW = R - L

    const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings)
    const black      = [0, 0, 0]
    const gray       = [107, 114, 128]
    const borderCol  = [229, 231, 235]

    let circularLogo = null
    try {
      const img = await new Promise((resolve, reject) => {
        const i = new Image(); i.onload = () => resolve(i); i.onerror = reject; i.src = settings.companyLogo || logoSrc
      })
      const sz = Math.min(img.naturalWidth, img.naturalHeight)
      const cv = document.createElement("canvas")
      cv.width = sz; cv.height = sz
      const ctx = cv.getContext("2d")
      ctx.beginPath(); ctx.arc(sz / 2, sz / 2, sz / 2, 0, Math.PI * 2); ctx.clip()
      ctx.drawImage(img, 0, 0, sz, sz)
      circularLogo = cv.toDataURL("image/png")
    } catch (_) {}

    // ── HEADER ──────────────────────────────────────────────────────────
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17)
    doc.setFontSize(14); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text(companyName, PW / 2, 11, { align: "center" })
    doc.setFontSize(7.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(companyAddr, PW / 2, 16, { align: "center" })

    const badgeW = 46
    doc.setFillColor(...accent); doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, "F")
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...accentHeaderText)
    doc.text("COMMODITY REPORT", R - badgeW / 2, 9.5, { align: "center" })
    doc.setFontSize(7); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(`Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`, R, 16, { align: "right" })

    doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23)

    // ── STATS ───────────────────────────────────────────────────────────
    let y = 27
    const statW = (TW - 8) / 3
    const stats = [
      { label: "TOTAL COMMODITIES", value: `${totalCommodities}` },
      { label: "TOTAL TRIPS",       value: `${totalTrips}` },
      { label: "TOTAL NET WEIGHT",  value: `${totalWeight.toLocaleString()} kg` },
    ]
    stats.forEach((s, i) => {
      const bx = L + i * (statW + 4)
      doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3)
      doc.roundedRect(bx, y, statW, 10, 2, 2, "FD")
      doc.setFontSize(6.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text(s.label, bx + statW / 2, y + 3.8, { align: "center" })
      doc.setFontSize(9); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
      doc.text(s.value, bx + statW / 2, y + 8.2, { align: "center" })
    })
    y += 14

    // ── TABLE ───────────────────────────────────────────────────────────
    autoTable(doc, {
      startY: y,
      margin: { left: L, right: L },
      head: [["Commodity", "Trips", "Customers", "Net Weight (kg)"]],
      body: commoditySummary.map((c) => [c.commodityName, c.trips, c.customers, c.totalNetWeight.toLocaleString()]),
      styles: { fontSize: 8, cellPadding: 2, textColor: black, lineColor: borderCol },
      headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: "bold", fontSize: 8.5, halign: "center", lineColor: accentDark },
      alternateRowStyles: { fillColor: [252, 252, 252] },
      columnStyles: { 1: { halign: "center" }, 2: { halign: "center" }, 3: { halign: "right", fontStyle: "bold" } },
    })

    // ── FOOTER ──────────────────────────────────────────────────────────
    const footerY = doc.lastAutoTable.finalY + 4
    doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3)
    doc.roundedRect(L, footerY, TW, 10, 2, 2, "FD")
    if (circularLogo) doc.addImage(circularLogo, "PNG", L + 2, footerY + 1, 8, 8)
    doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("Powered by Qalibrated Systems  |  www.qalibrated.co.ke", PW / 2, footerY + 5, { align: "center" })
    doc.setFontSize(6.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text("Inventing and Making Happen", PW / 2, footerY + 8.5, { align: "center" })

    doc.save(`commodity-report-${dayjs().format("YYYY-MM-DD")}.pdf`)
  }

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      commoditySummary.map((c) => ({
        'Commodity': c.commodityName,
        'Trips': c.trips,
        'Customers': c.customers,
        'Total Net Weight (kg)': c.totalNetWeight,
      }))
    )

    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, "Commodities")
    XLSX.writeFile(wb, `commodity-report-${dayjs().format('YYYY-MM-DD')}.xlsx`)
  }

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
          className="text-amber-700 hover:underline font-medium"
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

    return filteredTransactions
      .filter((t) => (t.commodityName || "Unknown Commodity") === selectedCommodity)
      .map((t) => ({
        id: t.id,
        date: dayjs(t.createdAt).format("DD MMM YYYY, HH:mm"),
        vehicle: t.noPlate || "-",
        supplier: t.originName || "-",
        customer: t.destinationName || "-",
        driver: t.driverName || "-",
        netWeight: Number(t.netWeight || 0),
      }))
  }, [filteredTransactions, selectedCommodity])

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
      avgNet: commodityTrips.length > 0 ? Math.round(totalNet / commodityTrips.length) : 0,
    }
  }, [commodityTrips])

  return (
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* FILTERS */}
      {!selectedCommodity && (
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
                placeholder="Search commodity or customer..."
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
      {!selectedCommodity && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Commodities" value={totalCommodities} />
          <CompactStat label="Trips" value={totalTrips} />
          <CompactStat label="Net Weight" value={totalWeight.toLocaleString()} />
        </div>
      )}

      {/* COMMODITY SUMMARY TABLE */}
      {!selectedCommodity && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={["commodityName", "trips", "customers", "netWeight"]}
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
              className="flex items-center gap-1 text-xs border rounded px-2 py-1 hover:bg-gray-50"
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
            <CompactStat label="Total Net" value={commodityTotals.totalNet.toLocaleString()} />
            <CompactStat label="Customers" value={commodityTotals.customers} />
            <CompactStat label="Avg Net" value={commodityTotals.avgNet.toLocaleString()} />
          </div>

          {/* TRIPS TABLE */}
          <div className="border rounded-lg overflow-auto">
            <table className="w-full text-xs min-w-[1200px]">
              <thead className="bg-amber-50">
                <tr>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Date</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Vehicle</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Supplier</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Customer</th>
                  <th className="p-2 text-left font-semibold border-b border-amber-200">Driver</th>
                  <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {commodityTrips.map((t, idx) => (
                  <tr key={t.id} className={`border-t border-gray-100 hover:bg-amber-50/30 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                    <td className="p-2">{t.date}</td>
                    <td className="p-2">{t.vehicle}</td>
                    <td className="p-2">{t.supplier}</td>
                    <td className="p-2">{t.customer}</td>
                    <td className="p-2">{t.driver}</td>
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
                    <span className="font-semibold text-amber-700">{commoditySummary.length}</span> commodities
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
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Commodity</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Trips</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Customers</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">Net Weight</th>
                    </tr>
                  </thead>
                  <tbody>
                    {commoditySummary.map((c, idx) => (
                      <tr key={c.id} className={`border-t border-gray-100 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                        <td className="p-2 font-medium">{c.commodityName}</td>
                        <td className="p-2">{c.trips}</td>
                        <td className="p-2">{c.customers}</td>
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