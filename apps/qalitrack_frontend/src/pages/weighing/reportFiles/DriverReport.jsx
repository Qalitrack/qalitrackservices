import { useMemo, useState } from "react"
import ReportsTable from "../ReportsTable"
import ReportsPagination from "../ReportsPagination"
import { ChevronLeft, RotateCcw, FileDown, FileSpreadsheet, Filter } from "lucide-react"
import { Button, Input } from "antd"
import { CloseOutlined } from "@ant-design/icons"
import dayjs from "dayjs"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"
import * as XLSX from "xlsx"
import logoSrc from "../../../assets/logo.jpeg"
import { getTicketSettings, resolveReportColors } from "../../../utils/ticketThemeConfig"

export default function DriverReport({ transactions = [], loading }) {
  const [selectedDriver, setSelectedDriver] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 10

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    search: "",
  })

  const [showFilters, setShowFilters] = useState(false)

  // Export preview
  const [showExportPreview, setShowExportPreview] = useState(false)
  const [exportType, setExportType] = useState(null)

  /* FILTERED TRANSACTIONS */
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

  /* GROUP BY DRIVER */
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

  /* KPIs (SUMMARY) */
  const totalDrivers = driverSummary.length
  const totalTrips = driverSummary.reduce((s, d) => s + d.trips, 0)
  const totalWeight = driverSummary.reduce((s, d) => s + d.totalNetWeight, 0)

  /* EXPORT FUNCTIONS */
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
      const sz  = 200
      const pad = sz * 0.06
      const cv = document.createElement("canvas")
      cv.width = sz; cv.height = sz
      const ctx = cv.getContext("2d")
      const avail  = sz - pad * 2
      const aspect = img.naturalWidth / img.naturalHeight
      const drawW  = aspect >= 1 ? avail : avail * aspect
      const drawH  = aspect >= 1 ? avail / aspect : avail
      ctx.drawImage(img, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH)
      const imgData = ctx.getImageData(0, 0, sz, sz)
      const px = imgData.data
      for (let p = 0; p < px.length; p += 4) {
        if (px[p] > 240 && px[p + 1] > 240 && px[p + 2] > 240) px[p + 3] = 0
      }
      ctx.putImageData(imgData, 0, 0)
      circularLogo = cv.toDataURL("image/png")
    } catch (_) {}

    // ── HEADER ──────────────────────────────────────────────────────────
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17)
    doc.setFontSize(14); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text(companyName, PW / 2, 11, { align: "center" })
    doc.setFontSize(7.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(companyAddr, PW / 2, 16, { align: "center" })

    const badgeW = 40
    doc.setFillColor(...accent); doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, "F")
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...accentHeaderText)
    doc.text("DRIVER REPORT", R - badgeW / 2, 9.5, { align: "center" })
    doc.setFontSize(7); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(`Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`, R, 16, { align: "right" })

    doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23)

    // ── STATS ───────────────────────────────────────────────────────────
    let y = 27
    const statW = (TW - 8) / 3
    const stats = [
      { label: "TOTAL DRIVERS",    value: `${totalDrivers}` },
      { label: "TOTAL TRIPS",      value: `${totalTrips}` },
      { label: "TOTAL NET WEIGHT", value: `${totalWeight.toLocaleString()} kg` },
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
      head: [["Driver Name", "Trips", "Vehicles", "Net Weight (kg)"]],
      body: driverSummary.map((d) => [d.driverName, d.trips, d.vehicles, d.totalNetWeight.toLocaleString()]),
      styles: { fontSize: 8, cellPadding: 2, textColor: black, lineColor: borderCol },
      headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: "bold", fontSize: 8.5, halign: "center", lineColor: accentDark },
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

    // Watermark on all pages
    if (circularLogo) {
      try {
        const wmSize = 90
        const PH = doc.internal.pageSize.getHeight()
        const wmCanvas = document.createElement("canvas")
        wmCanvas.width = 200; wmCanvas.height = 200
        const wmCtx = wmCanvas.getContext("2d")
        const wmImg = await new Promise((resolve, reject) => {
          const i = new Image(); i.onload = () => resolve(i); i.onerror = reject
          i.src = circularLogo
        })
        wmCtx.globalAlpha = 0.07
        wmCtx.drawImage(wmImg, 0, 0, 200, 200)
        const wmData = wmCanvas.toDataURL("image/png")
        const totalPages = doc.internal.getNumberOfPages()
        for (let p = 1; p <= totalPages; p++) {
          doc.setPage(p)
          doc.addImage(wmData, "PNG", PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize)
        }
      } catch (_) {}
    }

    doc.save(`driver-report-${dayjs().format("YYYY-MM-DD")}.pdf`)
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

  /* PAGINATED SUMMARY */
  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE

    return driverSummary.slice(start, start + PAGE_SIZE).map((d) => ({
      id: d.id,
      driverName: (
        <button
          onClick={() => setSelectedDriver(d.driverName)}
          className="text-amber-700 hover:underline font-medium text-[10px]"
        >
          {d.driverName}
        </button>
      ),
      trips: d.trips,
      vehicles: d.vehicles,
      netWeight: d.totalNetWeight.toLocaleString(),
    }))
  }, [driverSummary, currentPage])

  /* DRIVER TRIPS (DETAIL PAGE) */
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

  /* DRIVER KPIs */
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
    <div className="space-y-3">
      {/* FILTERS */}
      {!selectedDriver && (
        <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-amber-50/20 border border-amber-200 rounded-lg p-3">
          <div className="flex justify-between items-center mb-2">
            <span className="text-[10px] font-bold text-gray-900 uppercase tracking-wide">Filter Options</span>
            <Button 
              icon={<Filter size={12} />}
              size="small"
              className={`h-6 text-[10px] font-medium ${showFilters ? 'bg-amber-500 text-white border-amber-500' : 'border-gray-300'}`}
              onClick={() => setShowFilters(!showFilters)}
            >
              {showFilters ? 'Hide' : 'Show'}
            </Button>
          </div>

          {showFilters && (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-2 mb-2">
                <div>
                  <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                    <span className="w-1 h-1 bg-amber-500 rounded-full"></span>
                    Start Date
                  </label>
                  <input
                    type="date"
                    value={filters.startDate}
                    onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                    className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  />
                </div>

                <div>
                  <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                    <span className="w-1 h-1 bg-amber-500 rounded-full"></span>
                    End Date
                  </label>
                  <input
                    type="date"
                    value={filters.endDate}
                    onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                    className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  />
                </div>

                <div>
                  <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                    <span className="w-1 h-1 bg-amber-500 rounded-full"></span>
                    Search
                  </label>
                  <Input
                    placeholder="Driver or vehicle..."
                    value={filters.search}
                    onChange={(e) => setFilters({ ...filters, search: e.target.value })}
                    className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                    allowClear
                  />
                </div>
              </div>

              <div className="flex justify-between items-center">
                <div className="flex gap-2 items-center">
                  {Object.entries(filters).filter(([key, value]) => value && value !== '').length > 0 && (
                    <span className="text-[9px] text-amber-900 font-bold bg-gradient-to-r from-amber-100 to-amber-200 px-2 py-0.5 rounded-full border border-amber-300 shadow-sm">
                      🎯 {Object.entries(filters).filter(([key, value]) => value && value !== '').length} active
                    </span>
                  )}
                </div>

                <div className="flex gap-2">
                  {Object.entries(filters).filter(([key, value]) => value && value !== '').length > 0 && (
                    <Button 
                      size="small"
                      danger
                      icon={<CloseOutlined />}
                      onClick={clearFilters}
                      className="h-6 text-[10px] font-semibold shadow-sm rounded bg-red-50 border-red-300 text-red-700 hover:bg-red-100"
                    >
                      Clear
                    </Button>
                  )}

                  <button
                    onClick={() => { setExportType("pdf"); setShowExportPreview(true); }}
                    className="flex items-center gap-1.5 px-3 py-1 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-[10px] font-medium hover:bg-amber-200"
                  >
                    <FileDown size={12} />
                    PDF
                  </button>

                  <button
                    onClick={() => { setExportType("excel"); setShowExportPreview(true); }}
                    className="flex items-center gap-1.5 px-3 py-1 border border-gray-300 bg-white rounded-lg text-[10px] font-medium hover:bg-gray-50"
                  >
                    <FileSpreadsheet size={12} />
                    Excel
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      )}

      {/* SUMMARY KPIs */}
      {!selectedDriver && (
        <div className="grid grid-cols-3 gap-3">
          <div className="bg-white border border-amber-200 rounded-lg px-3 py-2 shadow-sm">
            <p className="text-[10px] text-gray-600 uppercase font-semibold tracking-wide">Drivers</p>
            <p className="text-xl font-bold leading-tight text-gray-900">{totalDrivers}</p>
          </div>
          <div className="bg-white border border-amber-200 rounded-lg px-3 py-2 shadow-sm">
            <p className="text-[10px] text-gray-600 uppercase font-semibold tracking-wide">Trips</p>
            <p className="text-xl font-bold leading-tight text-gray-900">{totalTrips}</p>
          </div>
          <div className="bg-white border border-amber-200 rounded-lg px-3 py-2 shadow-sm">
            <p className="text-[10px] text-gray-600 uppercase font-semibold tracking-wide">Net Weight</p>
            <p className="text-xl font-bold leading-tight text-gray-900">{totalWeight.toLocaleString()}</p>
            <p className="text-[9px] text-gray-500">kg</p>
          </div>
        </div>
      )}

      {/* DRIVER SUMMARY TABLE */}
      {!selectedDriver && (
        <>
          <ReportsTable
            transactions={paginatedRows}
            loading={loading}
            showColumns={["driverName", "trips", "vehicles", "netWeight"]}
            currentPage={currentPage}
            pageSize={PAGE_SIZE}
            totalRecords={driverSummary.length}
            onPageChange={setCurrentPage}
            onPageSizeChange={() => {}}
          />
        </>
      )}

      {/* DRIVER DETAIL PAGE */}
      {selectedDriver && (
        <>
          <div className="flex items-center gap-2 bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50 border border-amber-200 rounded-lg p-2">
            <button
              onClick={() => setSelectedDriver(null)}
              className="flex items-center gap-1 text-[10px] border border-amber-300 rounded px-2 py-1 hover:bg-amber-50 font-medium"
            >
              <ChevronLeft size={12} /> Back
            </button>

            <h3 className="text-[11px] font-bold text-gray-900">
              {selectedDriver} — Driver Report
            </h3>
          </div>

          {/* DRIVER KPIs */}
          <div className="grid grid-cols-4 gap-3">
            <div className="bg-white border border-amber-200 rounded px-3 py-2 shadow-sm">
              <p className="text-[10px] text-gray-600 uppercase font-semibold">Trips</p>
              <p className="text-lg font-bold leading-tight text-gray-900">{driverTotals.trips}</p>
            </div>
            <div className="bg-white border border-amber-200 rounded px-3 py-2 shadow-sm">
              <p className="text-[10px] text-gray-600 uppercase font-semibold">Total Net</p>
              <p className="text-lg font-bold leading-tight text-gray-900">{driverTotals.totalNet.toLocaleString()}</p>
              <p className="text-[9px] text-gray-500">kg</p>
            </div>
            <div className="bg-white border border-amber-200 rounded px-3 py-2 shadow-sm">
              <p className="text-[10px] text-gray-600 uppercase font-semibold">Vehicles</p>
              <p className="text-lg font-bold leading-tight text-gray-900">{driverTotals.vehicles}</p>
            </div>
            <div className="bg-white border border-amber-200 rounded px-3 py-2 shadow-sm">
              <p className="text-[10px] text-gray-600 uppercase font-semibold">Avg Net</p>
              <p className="text-lg font-bold leading-tight text-gray-900">{driverTotals.avgNet.toLocaleString()}</p>
              <p className="text-[9px] text-gray-500">kg</p>
            </div>
          </div>

          {/* TRIPS TABLE */}
          <div className="border border-amber-200 rounded-lg overflow-auto bg-white">
            <table className="w-full text-xs min-w-[1200px]">
              <thead className="bg-gradient-to-b from-amber-50 to-amber-100/50 sticky top-0">
                <tr>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Date</th>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Vehicle</th>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Supplier</th>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Commodity</th>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Customer</th>
                  <th className="p-2 text-left font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Destination</th>
                  <th className="p-2 text-right font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200">Net Weight</th>
                </tr>
              </thead>

              <tbody>
                {driverTrips.map((t, idx) => (
                  <tr key={t.ticketID || t.id} className={`border-t border-gray-100 hover:bg-amber-50/30 transition-colors ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                    <td className="p-2 text-[10px]">{t.date}</td>
                    <td className="p-2 text-[10px] font-semibold">{t.vehicle}</td>
                    <td className="p-2 text-[10px]">{t.supplier}</td>
                    <td className="p-2 text-[10px]">{t.commodity}</td>
                    <td className="p-2 text-[10px]">{t.customer}</td>
                    <td className="p-2 text-[10px]">{t.destination}</td>
                    <td className="p-2 text-right text-[10px] font-bold text-amber-800">
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