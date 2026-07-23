import { useMemo, useState, useEffect, useRef } from "react"
import { FileDown, FileSpreadsheet, Filter, X, RefreshCw } from "lucide-react"
import { Button, Input, Spin } from "antd"
import { CloseOutlined } from "@ant-design/icons"
import dayjs from "dayjs"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"
import * as XLSX from "xlsx"
import logoSrc from "../../../assets/logo.jpeg"
import { getTicketSettings, resolveReportColors } from "../../../utils/ticketThemeConfig"
import { getReweighRecords } from "../../../api/Transaction/Transaction"

// ─── helpers ──────────────────────────────────────────────────────────────────
const STATUS_STYLES = {
  Approved: {
    bg: "bg-green-100",
    text: "text-green-800",
    border: "border-green-300",
    dot: "bg-green-500",
    label: "Approved",
  },
  Rejected: {
    bg: "bg-red-100",
    text: "text-red-800",
    border: "border-red-300",
    dot: "bg-red-500",
    label: "Rejected",
  },
  Pending: {
    bg: "bg-amber-50",
    text: "text-amber-800",
    border: "border-amber-300",
    dot: "bg-amber-500",
    label: "Pending",
  },
}

function StatusBadge({ status }) {
  const s = STATUS_STYLES[status] || STATUS_STYLES.Pending
  return (
    <span
      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold border ${s.bg} ${s.text} ${s.border}`}
    >
      <span className={`w-1.5 h-1.5 rounded-full ${s.dot}`} />
      {s.label}
    </span>
  )
}

// Derive reweigh decision status from a reweigh record
// Handles both camelCase (status) and PascalCase (Status) from the API
function resolveRecordStatus(record) {
  if (!record) return "Pending"
  const s = (record.status || record.Status || "").toLowerCase()
  if (s === "approved" || s === "approve") return "Approved"
  if (s === "rejected" || s === "reject") return "Rejected"
  // Extra fallback: a decision record always has a PerformedBy value
  const performer = record.performedBy || record.PerformedBy
  if (performer) {
    // Can't determine from performer alone; check notes/reason for rejection hint
    const notes = (record.notes || record.Notes || "").toLowerCase()
    if (notes.includes("rejection") || notes.includes("rejected")) return "Rejected"
    return "Approved"  // performer set but no rejection hint → approved
  }
  return "Pending"
}

// ─── GROUP REWEIGH RECORDS INTO REQUEST/DECISION PAIRS ───────────────────────
// Records arrive ordered ASC by attemptNumber.
// Each Pending record starts a new cycle; the next non-Pending record closes it.
function groupIntoCycles(records) {
  const cycles = []
  let pending = null
  for (const rec of records) {
    const s = (rec.status || "").toLowerCase()
    if (s === "pending") {
      pending = rec
    } else if (pending) {
      cycles.push({ request: pending, decision: rec, n: cycles.length + 1 })
      pending = null
    }
  }
  if (pending) cycles.push({ request: pending, decision: null, n: cycles.length + 1 })
  return cycles
}

// ─── LOGO HELPER — scaled to fit, never cropped ──────────────────────────────
async function buildCircularLogo(src) {
  try {
    const img = await new Promise((res, rej) => {
      const i = new Image(); i.onload = () => res(i); i.onerror = rej; i.src = src
    })
    const sz  = 200
    const pad = sz * 0.06
    const cv = document.createElement("canvas"); cv.width = sz; cv.height = sz
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
    return cv.toDataURL("image/png")
  } catch (_) { return null }
}

// ─── WATERMARK HELPER ─────────────────────────────────────────────────────────
async function addWatermark(doc, circularLogo) {
  if (!circularLogo) return
  try {
    const wmSize = 90
    const PW = doc.internal.pageSize.getWidth()
    const PH = doc.internal.pageSize.getHeight()
    const wmCanvas = document.createElement("canvas"); wmCanvas.width = 200; wmCanvas.height = 200
    const wmCtx = wmCanvas.getContext("2d")
    const wmImg = await new Promise((res, rej) => {
      const i = new Image(); i.onload = () => res(i); i.onerror = rej; i.src = circularLogo
    })
    wmCtx.globalAlpha = 0.07; wmCtx.drawImage(wmImg, 0, 0, 200, 200)
    const wmData = wmCanvas.toDataURL("image/png")
    const total = doc.internal.getNumberOfPages()
    for (let p = 1; p <= total; p++) {
      doc.setPage(p)
      doc.addImage(wmData, "PNG", PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize)
    }
  } catch (_) {}
}

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function ReweighedTransactionsReport({ transactions: propTransactions = [], loading: parentLoading }) {
  // Map: ticketId → reweigh records array (undefined = not fetched, [] = fetched/empty)
  const [reweighMap, setReweighMap]     = useState({})
  const [fetchingIds, setFetchingIds]   = useState(new Set())
  const [isScanning, setIsScanning]     = useState(false)
  const [scanDone, setScanDone]         = useState(false)
  const loadedRef = useRef(new Set())       // IDs already fetched — survive re-renders
  const loadedStatusRef = useRef({})        // ticketId → transaction.status at fetch time

  // Detail popup
  const [selectedEntry, setSelectedEntry] = useState(null)
  const [showDetailModal, setShowDetailModal] = useState(false)

  // Filters
  const [filters, setFilters] = useState({ startDate: "", endDate: "", search: "", status: "" })
  const [showFilters, setShowFilters] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 10

  // Export
  const [showExportPreview, setShowExportPreview] = useState(false)
  const [exportType, setExportType]               = useState(null)
  const [showPopupExport, setShowPopupExport]     = useState(false)
  const [popupExportType, setPopupExportType]     = useState(null)

  // ── Fetch records for one ticket ID — returns the fetched array ─────────────
  const fetchOne = async (ticketId, currentStatus = "") => {
    if (!ticketId || loadedRef.current.has(ticketId)) return []
    loadedRef.current.add(ticketId)
    loadedStatusRef.current[ticketId] = currentStatus
    setFetchingIds((prev) => new Set([...prev, ticketId]))
    try {
      const data    = await getReweighRecords(ticketId)
      // Handle plain array, { $values: [...] } (.NET ref-cycle), { data: [...] }, or { items: [...] }
      const records = Array.isArray(data) ? data : (data?.["$values"] ?? data?.data ?? data?.items ?? [])
      setReweighMap((prev) => ({ ...prev, [ticketId]: records }))
      return records
    } catch (_) {
      setReweighMap((prev) => ({ ...prev, [ticketId]: [] }))
      return []
    } finally {
      setFetchingIds((prev) => { const s = new Set(prev); s.delete(ticketId); return s })
    }
  }

  // ── On mount / when propTransactions arrive: fetch for all reweigh candidates ─
  useEffect(() => {
    if (propTransactions.length === 0) return
    const candidates = propTransactions.filter((t) => {
      const s = (t.status || "").toLowerCase()
      return (
        t.isReweighed === true ||
        Number(t.reweighCount) > 0 ||
        s.includes("reweigh")
      )
    })
    candidates.forEach((t) => {
      const id      = t.ticketID || t.id
      const status  = t.status || ""
      // If the transaction's status changed since last fetch (e.g. ReweighRequested → Completed
      // after a rejection/approval), invalidate the cache so we re-fetch with the new record
      if (
        loadedStatusRef.current[id] !== undefined &&
        loadedStatusRef.current[id] !== status
      ) {
        loadedRef.current.delete(id)
        delete loadedStatusRef.current[id]
        // Also clear stale map entry so the row shows a loading spinner
        setReweighMap((prev) => { const n = { ...prev }; delete n[id]; return n })
      }
      fetchOne(id, status)
    })
  }, [propTransactions]) // eslint-disable-line react-hooks/exhaustive-deps

  // ── "Scan All" — try every transaction (catches cases where flags aren't set) ─
  const scanAll = async () => {
    if (isScanning) return
    setIsScanning(true)
    const ids = propTransactions
      .map((t) => t.ticketID || t.id)
      .filter((id) => id && !loadedRef.current.has(id))
    // Batch in groups of 10 to avoid overwhelming the API
    const BATCH = 10
    for (let i = 0; i < ids.length; i += BATCH) {
      await Promise.allSettled(ids.slice(i, i + BATCH).map(fetchOne))
    }
    setScanDone(true)
    setIsScanning(false)
  }

  // ── Build flat table rows — 1 row per transaction ────────────────────────────
  const tableRows = useMemo(() => {
    const rows = []
    propTransactions.forEach((t) => {
      const id      = t.ticketID || t.id
      const records = reweighMap[id]  // undefined = not fetched, [] = fetched empty, [...] = has data
      const s       = (t.status || "").toLowerCase()
      const isReweighCandidate =
        t.isReweighed === true ||
        Number(t.reweighCount) > 0 ||
        s.includes("reweigh")

      if (!isReweighCandidate) return  // not a reweigh transaction — skip entirely

      const isFetching = fetchingIds.has(id)

      if (records && records.length > 0) {
        // Find the latest decision record (Approved/Rejected) searching from end
        const decisionRec = [...records].reverse().find(r => {
          const rs = (r.status || r.Status || "").toLowerCase()
          return rs === "approved" || rs === "rejected"
        })
        const latestRec  = decisionRec || records[records.length - 1]
        const requestRec = records.find(r => (r.status || r.Status || "").toLowerCase() === "pending") || records[0]
        let reweighStatus = resolveRecordStatus(latestRec)
        // Final fallback: if still "Pending" but transaction is Completed/Active → must be Approved
        if (reweighStatus === "Pending") {
          if (s === "completed" || s === "active") reweighStatus = "Approved"
        }
        rows.push({
          transaction:   t,
          record:        latestRec,
          requestRecord: requestRec,
          allRecords:    records,
          reweighStatus,
        })
      } else if (isFetching || records === undefined) {
        // Still loading — placeholder row
        rows.push({ transaction: t, record: null, requestRecord: null, allRecords: [], reweighStatus: "Pending" })
      } else {
        // Fetch done but no records in DB — derive status from the transaction itself
        let txReweighStatus = "Pending"
        if (s === "completed") txReweighStatus = "Approved"
        rows.push({ transaction: t, record: null, requestRecord: null, allRecords: [], reweighStatus: txReweighStatus })
      }
    })
    return rows
  }, [propTransactions, reweighMap, fetchingIds])

  // ── Apply filters ────────────────────────────────────────────────────────────
  const filteredRows = useMemo(() => {
    let data = [...tableRows]

    if (filters.startDate) {
      const start = new Date(`${filters.startDate}T00:00`)
      data = data.filter((r) => new Date(r.transaction.createdAt) >= start)
    }
    if (filters.endDate) {
      const end = new Date(`${filters.endDate}T23:59`)
      data = data.filter((r) => new Date(r.transaction.createdAt) <= end)
    }
    if (filters.status) {
      data = data.filter((r) => r.reweighStatus === filters.status)
    }
    if (filters.search) {
      const q = filters.search.toLowerCase()
      data = data.filter((r) =>
        r.transaction.receiptNo?.toLowerCase().includes(q) ||
        r.transaction.noPlate?.toLowerCase().includes(q) ||
        r.transaction.driverName?.toLowerCase().includes(q) ||
        r.transaction.commodityName?.toLowerCase().includes(q)
      )
    }

    return data
  }, [tableRows, filters])

  const paginatedRows = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return filteredRows.slice(start, start + PAGE_SIZE)
  }, [filteredRows, currentPage])

  const totalPages = Math.ceil(filteredRows.length / PAGE_SIZE)

  // KPIs
  const kpis = useMemo(() => ({
    total:    tableRows.length,
    pending:  tableRows.filter((r) => r.reweighStatus === "Pending").length,
    approved: tableRows.filter((r) => r.reweighStatus === "Approved").length,
    rejected: tableRows.filter((r) => r.reweighStatus === "Rejected").length,
  }), [tableRows])

  const clearFilters = () => {
    setFilters({ startDate: "", endDate: "", search: "", status: "" })
    setCurrentPage(1)
  }
  const activeFilterCount = Object.values(filters).filter(Boolean).length

  // ── Row click → open detail popup ───────────────────────────────────────────
  const handleRowClick = async (row) => {
    const id     = row.transaction.ticketID || row.transaction.id
    const status = row.transaction.status || ""
    // Always re-fetch so popup reflects the latest decision (clear cache first)
    loadedRef.current.delete(id)
    delete loadedStatusRef.current[id]
    setReweighMap((prev) => { const n = { ...prev }; delete n[id]; return n })
    // fetchOne returns the records directly — avoids stale closure on reweighMap state
    const freshRecords = await fetchOne(id, status)
    setSelectedEntry({
      transaction: row.transaction,
      records: freshRecords ?? row.allRecords ?? [],
    })
    setShowDetailModal(true)
  }

  // ── PDF export: TABLE ────────────────────────────────────────────────────────
  const exportTablePDF = async () => {
    const settings    = getTicketSettings()
    const companyName = settings.companyName    || "QALIBRATED SYSTEMS LTD"
    const companyAddr = settings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996"
    const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings)

    const doc = new jsPDF("landscape", "mm", "a4")
    const PW = doc.internal.pageSize.getWidth()
    const L = 14, R = PW - 14, TW = R - L
    const black = [0, 0, 0], gray = [107, 114, 128], borderCol = [229, 231, 235]
    const green = [21, 128, 61], red = [185, 28, 28], amber = [180, 83, 9]

    const circularLogo = await buildCircularLogo(settings.companyLogo || logoSrc)

    // Header
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17)
    doc.setFontSize(14); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text(companyName, PW / 2, 11, { align: "center" })
    doc.setFontSize(7.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(companyAddr, PW / 2, 16, { align: "center" })

    const badgeW = 60
    doc.setFillColor(...accent); doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, "F")
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...accentHeaderText)
    doc.text("REWEIGH TRANSACTIONS REPORT", R - badgeW / 2, 9.5, { align: "center" })
    doc.setFontSize(7); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(`Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`, R, 16, { align: "right" })

    doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23)

    // Stats
    let y = 27
    const statW = (TW - 12) / 4
    const stats = [
      { label: "TOTAL REWEIGHS",  value: `${filteredRows.length}` },
      { label: "PENDING",         value: `${filteredRows.filter(r => r.reweighStatus === "Pending").length}` },
      { label: "APPROVED",        value: `${filteredRows.filter(r => r.reweighStatus === "Approved").length}` },
      { label: "REJECTED",        value: `${filteredRows.filter(r => r.reweighStatus === "Rejected").length}` },
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

    // Table
    autoTable(doc, {
      startY: y,
      margin: { left: L, right: L },
      head: [["#", "Date", "Receipt No", "Vehicle", "Driver", "Commodity", "First Wt (kg)", "Second Wt (kg)", "Net Wt (kg)", "Request Reason", "Decision By", "Decision Notes", "Reweigh Status"]],
      body: filteredRows.map((row, idx) => {
        const t    = row.transaction
        const req  = row.requestRecord || {}
        const dec  = row.record || {}
        return [
          idx + 1,
          dayjs(t.createdAt).format("DD MMM YY HH:mm"),
          t.receiptNo || "-",
          t.noPlate || "-",
          t.driverName || "-",
          t.commodityName || "-",
          t.firstWeight  ? parseFloat(t.firstWeight).toLocaleString()  : "-",
          t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : "-",
          t.netWeight    ? parseFloat(t.netWeight).toLocaleString()    : "-",
          req.reason || t.reweighPermission || "-",
          dec.performedBy || "-",
          dec.notes || "-",
          row.reweighStatus,
        ]
      }),
      styles: { fontSize: 6.5, cellPadding: 1.5, textColor: black, lineColor: borderCol },
      headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: "bold", fontSize: 7, halign: "center", lineColor: accentDark },
      columnStyles: {
        0:  { halign: "center", cellWidth: 6 },
        6:  { halign: "right" },
        7:  { halign: "right" },
        8:  { halign: "right", fontStyle: "bold" },
        12: { halign: "center", cellWidth: 18 },
      },
      didParseCell: (data) => {
        if (data.column.index === 12 && data.section === "body") {
          if (data.cell.raw === "Approved")  { data.cell.styles.textColor = green; data.cell.styles.fontStyle = "bold" }
          if (data.cell.raw === "Rejected")  { data.cell.styles.textColor = red;   data.cell.styles.fontStyle = "bold" }
          if (data.cell.raw === "Pending")   { data.cell.styles.textColor = amber; data.cell.styles.fontStyle = "bold" }
        }
      },
    })

    // Footer
    const footerY = doc.lastAutoTable.finalY + 4
    doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3)
    doc.roundedRect(L, footerY, TW, 10, 2, 2, "FD")
    if (circularLogo) doc.addImage(circularLogo, "PNG", L + 2, footerY + 1, 8, 8)
    doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("Powered by Qalibrated Systems  |  www.qalibrated.co.ke", PW / 2, footerY + 5, { align: "center" })
    doc.setFontSize(6.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text("Inventing and Making Happen", PW / 2, footerY + 8.5, { align: "center" })

    await addWatermark(doc, circularLogo)
    doc.save(`reweigh-report-${dayjs().format("YYYY-MM-DD")}.pdf`)
  }

  // ── Excel export: TABLE ──────────────────────────────────────────────────────
  const exportTableExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredRows.map((row, idx) => {
        const t   = row.transaction
        const req = row.requestRecord || {}
        const dec = row.record || {}
        return {
          "#": idx + 1,
          Date: dayjs(t.createdAt).format("DD MMM YYYY HH:mm"),
          "Receipt No": t.receiptNo || "-",
          Vehicle: t.noPlate || "-",
          Driver: t.driverName || "-",
          Commodity: t.commodityName || "-",
          "First Weight (kg)": t.firstWeight ? parseFloat(t.firstWeight) : 0,
          "Second Weight (kg)": t.secondWeight ? parseFloat(t.secondWeight) : 0,
          "Net Weight (kg)": t.netWeight ? parseFloat(t.netWeight) : 0,
          "Request Reason": req.reason || t.reweighPermission || "-",
          "Decision By": dec.performedBy || "-",
          "Decision Notes": dec.notes || "-",
          "Reweigh Status": row.reweighStatus,
        }
      })
    )
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, "Reweigh Transactions")
    XLSX.writeFile(wb, `reweigh-report-${dayjs().format("YYYY-MM-DD")}.xlsx`)
  }

  // ── PDF export: POPUP ────────────────────────────────────────────────────────
  const exportPopupPDF = async () => {
    if (!selectedEntry) return
    const settings    = getTicketSettings()
    const companyName = settings.companyName    || "QALIBRATED SYSTEMS LTD"
    const companyAddr = settings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996"
    const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings)

    const doc = new jsPDF("portrait", "mm", "a4")
    const PW  = doc.internal.pageSize.getWidth()
    const L = 14, R = PW - 14, TW = R - L
    const black = [0, 0, 0], gray = [107, 114, 128], lgray = [229, 231, 235]
    const green = [21, 128, 61], red = [185, 28, 28], amberCol = [180, 83, 9]

    const circularLogo = await buildCircularLogo(settings.companyLogo || logoSrc)
    const t       = selectedEntry.transaction
    const records = [...(selectedEntry.records || [])].sort((a, b) => (a.attemptNumber || 0) - (b.attemptNumber || 0))

    // ── Derived data ─────────────────────────────────────────────────────────
    const approvedRecs   = records.filter(r => resolveRecordStatus(r) === "Approved")
    const pendingRecs    = records.filter(r => resolveRecordStatus(r) === "Pending")
    const latestApproved = approvedRecs[approvedRecs.length - 1]
    const latestStatus   = records.length > 0 ? resolveRecordStatus(records[records.length - 1]) : "Pending"
    const finalNet       = latestApproved?.netWeight ?? t.netWeight

    // ── Header ───────────────────────────────────────────────────────────────
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17)
    doc.setFontSize(13); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text(companyName, PW / 2, 11, { align: "center" })
    doc.setFontSize(7.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text(companyAddr, PW / 2, 16, { align: "center" })
    const bW = 58
    doc.setFillColor(...accent); doc.roundedRect(R - bW, 4, bW, 9, 2, 2, "F")
    doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...accentHeaderText)
    doc.text("REWEIGH ATTEMPTS REPORT", R - bW / 2, 9.5, { align: "center" })
    doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23)

    let y = 27

    // ── Ticket info box ───────────────────────────────────────────────────────
    doc.setFillColor(248, 248, 248); doc.setDrawColor(...lgray); doc.setLineWidth(0.3)
    doc.roundedRect(L, y, TW, 20, 2, 2, "FD")
    const iCol = TW / 3
    ;[
      ["TICKET NO",      t.receiptNo   || "-"],
      ["TRANSACTION ID", t.ticketID    || "-"],
      ["GENERATED AT",   dayjs().format("DD-MM-YYYY hh:mm A")],
    ].forEach(([lbl, val], i) => {
      const fx = L + 4 + i * iCol
      doc.setFontSize(5.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text(lbl, fx, y + 5)
      doc.setFontSize(7); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
      doc.text(String(val), fx, y + 10, { maxWidth: iCol - 4 })
    })
    ;[
      ["VEHICLE",   t.noPlate       || "-"],
      ["DRIVER",    t.driverName    || "-"],
      ["COMMODITY", t.commodityName || "-"],
    ].forEach(([lbl, val], i) => {
      const fx = L + 4 + i * iCol
      doc.setFontSize(5.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text(lbl, fx, y + 14.5)
      doc.setFontSize(6.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
      doc.text(String(val), fx + (lbl.length * 1.6), y + 14.5)
    })
    y += 24

    // ── Summary section ───────────────────────────────────────────────────────
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("REWEIGH SUMMARY", L, y)
    doc.setDrawColor(...lgray); doc.line(L, y + 2, R, y + 2)
    y += 5

    const summaryItems = [
      { lbl: "TOTAL ATTEMPTS",      val: String(records.length),       col: black },
      { lbl: "APPROVED",            val: String(approvedRecs.length),  col: green },
      { lbl: "PENDING",             val: String(pendingRecs.length),   col: amberCol },
      { lbl: "FINAL NET WEIGHT",    val: finalNet != null ? `${parseFloat(finalNet).toLocaleString()} KG` : "—", col: green },
      { lbl: "LATEST STATUS",       val: latestStatus.toUpperCase(),   col: latestStatus === "Approved" ? green : latestStatus === "Rejected" ? red : amberCol },
    ]
    const sW = TW / summaryItems.length
    summaryItems.forEach(({ lbl, val, col }, i) => {
      const bx = L + i * sW
      doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3)
      doc.roundedRect(bx + 0.5, y, sW - 1, 13, 1.5, 1.5, "FD")
      doc.setFontSize(5.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text(lbl, bx + sW / 2, y + 4, { align: "center" })
      doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...col)
      doc.text(val, bx + sW / 2, y + 10, { align: "center", maxWidth: sW - 3 })
    })
    y += 17

    // ── Attempts table ────────────────────────────────────────────────────────
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("REWEIGH ATTEMPTS", L, y)
    doc.setDrawColor(...lgray); doc.line(L, y + 2, R, y + 2)
    y += 4

    if (records.length === 0) {
      doc.setFontSize(7.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text("No reweigh records found for this transaction.", L, y + 6)
      y += 12
    } else {
      autoTable(doc, {
        startY: y,
        margin: { left: L, right: L },
        head: [["#", "Status", "Started At", "Reason", "W1 (kg)", "W2 (kg)", "Net (kg)", "Operator", "Notes"]],
        body: records.map((rec) => {
          const rs = resolveRecordStatus(rec)
          return [
            rec.attemptNumber || "-",
            rs,
            rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YY hh:mm A") : "-",
            rec.reason || "-",
            rec.weight1   != null ? parseFloat(rec.weight1).toLocaleString()   : "-",
            rec.weight2   != null ? parseFloat(rec.weight2).toLocaleString()   : "-",
            rec.netWeight != null ? parseFloat(rec.netWeight).toLocaleString() : "-",
            rec.performedBy || rec.operator1 || "-",
            rec.notes || "-",
          ]
        }),
        styles:           { fontSize: 6.5, cellPadding: 1.8, textColor: black, lineColor: lgray },
        headStyles:       { fillColor: accent, textColor: accentHeaderText, fontStyle: "bold", fontSize: 7, halign: "center", lineColor: accentDark },
        columnStyles: {
          0: { halign: "center", cellWidth: 7 },
          1: { halign: "center", cellWidth: 17 },
          2: { cellWidth: 27 },
          4: { halign: "right", cellWidth: 14 },
          5: { halign: "right", cellWidth: 14 },
          6: { halign: "right", cellWidth: 14, fontStyle: "bold" },
        },
        didParseCell: (data) => {
          if (data.column.index === 1 && data.section === "body") {
            if (data.cell.raw === "Approved") { data.cell.styles.textColor = green;    data.cell.styles.fontStyle = "bold" }
            if (data.cell.raw === "Rejected") { data.cell.styles.textColor = red;      data.cell.styles.fontStyle = "bold" }
            if (data.cell.raw === "Pending")  { data.cell.styles.textColor = amberCol; data.cell.styles.fontStyle = "bold" }
          }
        },
      })
      y = doc.lastAutoTable.finalY + 6
    }

    // ── Approval history ──────────────────────────────────────────────────────
    if (approvedRecs.length > 0) {
      doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
      doc.text("APPROVAL HISTORY", L, y)
      doc.setDrawColor(...lgray); doc.line(L, y + 2, R, y + 2)
      y += 5

      approvedRecs.forEach((rec, idx) => {
        const isFinal = idx === approvedRecs.length - 1
        const label   = `Attempt ${rec.attemptNumber} approved${isFinal ? " (FINAL DECISION)" : ""}`
        doc.setFontSize(isFinal ? 7.5 : 7); doc.setFont("helvetica", isFinal ? "bold" : "normal")
        doc.setTextColor(...(isFinal ? green : gray))
        doc.text(`•  ${label}`, L + 2, y)
        y += 4
        if (rec.performedBy) {
          doc.setFontSize(6); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
          doc.text(`   by ${rec.performedBy}`, L + 6, y)
          y += 4
        }
      })
      if (finalNet != null) {
        doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...green)
        doc.text(`Final Net Weight Used: ${parseFloat(finalNet).toLocaleString()} KG`, L + 2, y + 1)
        y += 7
      }
    }

    // ── Timeline ──────────────────────────────────────────────────────────────
    if (records.length > 0) {
      doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
      doc.text("TIMELINE", L, y)
      doc.setDrawColor(...lgray); doc.line(L, y + 2, R, y + 2)
      y += 5

      records.forEach((rec) => {
        const rs      = resolveRecordStatus(rec)
        const isFinal = rec === latestApproved
        const dt      = rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YY HH:mm") : "—"
        const label   = rs === "Pending"
          ? `Attempt ${rec.attemptNumber} created (Pending)`
          : rs === "Approved"
          ? `Attempt ${rec.attemptNumber} approved${isFinal ? " (Final)" : ""}`
          : `Attempt ${rec.attemptNumber} rejected`
        const col = rs === "Approved" ? green : rs === "Rejected" ? red : amberCol
        doc.setFontSize(7); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
        doc.text(dt, L + 2, y)
        doc.setFont("helvetica", isFinal ? "bold" : "normal"); doc.setTextColor(...col)
        doc.text(`→  ${label}`, L + 32, y)
        y += 4.5
      })
      y += 3
    }

    // ── Final weight used ─────────────────────────────────────────────────────
    doc.setFontSize(8); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("FINAL WEIGHT USED IN SYSTEM", L, y)
    doc.setDrawColor(...lgray); doc.line(L, y + 2, R, y + 2)
    y += 5

    doc.setFillColor(240, 253, 244); doc.setDrawColor(134, 239, 172); doc.setLineWidth(0.3)
    doc.roundedRect(L, y, TW, 18, 2, 2, "FD")
    const fCol = TW / 3
    ;[
      ["Gross Weight",  t.firstWeight],
      ["Tare Weight",   t.secondWeight],
      ["Final Net Wt",  finalNet],
    ].forEach(([lbl, val], i) => {
      const fx = L + 4 + i * fCol
      doc.setFontSize(6); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
      doc.text(lbl.toUpperCase(), fx, y + 5)
      doc.setFontSize(9); doc.setFont("helvetica", "bold"); doc.setTextColor(...green)
      doc.text(val != null ? `${parseFloat(val).toLocaleString()} KG` : "—", fx, y + 11)
    })
    if (latestApproved) {
      doc.setFontSize(6); doc.setFont("helvetica", "italic"); doc.setTextColor(...gray)
      doc.text(`Source: Approved Reweigh Attempt #${latestApproved.attemptNumber}`, L + 4, y + 16.5)
    }
    y += 22

    // ── Footer ───────────────────────────────────────────────────────────────
    const footerY = y + 2
    doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3)
    doc.roundedRect(L, footerY, TW, 10, 2, 2, "FD")
    if (circularLogo) doc.addImage(circularLogo, "PNG", L + 2, footerY + 1, 8, 8)
    doc.setFontSize(7.5); doc.setFont("helvetica", "bold"); doc.setTextColor(...black)
    doc.text("Powered by Qalibrated Systems  |  www.qalibrated.co.ke", PW / 2, footerY + 5, { align: "center" })
    doc.setFontSize(6.5); doc.setFont("helvetica", "normal"); doc.setTextColor(...gray)
    doc.text("Inventing and Making Happen", PW / 2, footerY + 8.5, { align: "center" })

    await addWatermark(doc, circularLogo)
    doc.save(`reweigh-attempts-${t.receiptNo || "N-A"}-${dayjs().format("YYYY-MM-DD")}.pdf`)
  }

  // ── Excel export: POPUP ───────────────────────────────────────────────────────
  const exportPopupExcel = () => {
    if (!selectedEntry) return
    const t       = selectedEntry.transaction
    const records = [...(selectedEntry.records || [])].sort((a, b) => (a.attemptNumber || 0) - (b.attemptNumber || 0))

    const approvedRecs   = records.filter(r => resolveRecordStatus(r) === "Approved")
    const pendingRecs    = records.filter(r => resolveRecordStatus(r) === "Pending")
    const latestApproved = approvedRecs[approvedRecs.length - 1]
    const latestStatus   = records.length > 0 ? resolveRecordStatus(records[records.length - 1]) : "Pending"
    const finalNet       = latestApproved?.netWeight ?? t.netWeight

    const wb = XLSX.utils.book_new()

    // Sheet 1: Summary
    const summarySheet = XLSX.utils.json_to_sheet([{
      "Ticket No":              t.receiptNo        || "-",
      "Transaction ID":         t.ticketID         || "-",
      "Vehicle":                t.noPlate          || "-",
      "Driver":                 t.driverName       || "-",
      "Commodity":              t.commodityName    || "-",
      "Supplier":               t.supplierName     || "-",
      "Customer":               t.customerName     || "-",
      "Transporter":            t.transporterName  || "-",
      "Date":                   dayjs(t.firstWeightDate || t.createdAt).format("DD-MM-YYYY HH:mm"),
      "Generated At":           dayjs().format("DD-MM-YYYY HH:mm"),
      "Total Attempts":         records.length,
      "Approved Attempts":      approvedRecs.length,
      "Pending Attempts":       pendingRecs.length,
      "Final Approved Net (kg)": finalNet != null ? parseFloat(finalNet) : "-",
      "Latest Status":          latestStatus,
      "Source Attempt":         latestApproved ? `Attempt #${latestApproved.attemptNumber}` : "-",
    }])
    XLSX.utils.book_append_sheet(wb, summarySheet, "Summary")

    // Sheet 2: All attempts
    const attemptsSheet = XLSX.utils.json_to_sheet(
      records.length > 0
        ? records.map((rec) => ({
            "Attempt #":    rec.attemptNumber || "-",
            "Status":       resolveRecordStatus(rec),
            "Started At":   rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YYYY HH:mm") : "-",
            "Reason":       rec.reason || "-",
            "W1 (kg)":      rec.weight1   != null ? parseFloat(rec.weight1)   : "-",
            "W2 (kg)":      rec.weight2   != null ? parseFloat(rec.weight2)   : "-",
            "Net (kg)":     rec.netWeight != null ? parseFloat(rec.netWeight) : "-",
            "Operator":     rec.performedBy || rec.operator1 || "-",
            "Notes":        rec.notes || "-",
          }))
        : [{ Note: "No reweigh records found for this transaction" }]
    )
    XLSX.utils.book_append_sheet(wb, attemptsSheet, "Attempts")

    // Sheet 3: Timeline
    const timelineSheet = XLSX.utils.json_to_sheet(
      records.map((rec, idx) => {
        const rs      = resolveRecordStatus(rec)
        const isFinal = rec === latestApproved
        return {
          "Order":       idx + 1,
          "Timestamp":   rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YYYY HH:mm") : "-",
          "Event":       rs === "Pending"
            ? `Attempt ${rec.attemptNumber} created (Pending)`
            : rs === "Approved"
            ? `Attempt ${rec.attemptNumber} approved${isFinal ? " — FINAL" : ""}`
            : `Attempt ${rec.attemptNumber} rejected`,
          "Status":      rs,
          "By":          rec.performedBy || "-",
        }
      })
    )
    XLSX.utils.book_append_sheet(wb, timelineSheet, "Timeline")

    // Sheet 4: Final weights
    const finalSheet = XLSX.utils.json_to_sheet([{
      "Gross Weight (kg)":     t.firstWeight  ? parseFloat(t.firstWeight)  : "-",
      "Tare Weight (kg)":      t.secondWeight ? parseFloat(t.secondWeight) : "-",
      "Final Net Weight (kg)": finalNet != null ? parseFloat(finalNet) : "-",
      "Source":                latestApproved ? `Approved Reweigh Attempt #${latestApproved.attemptNumber}` : "-",
    }])
    XLSX.utils.book_append_sheet(wb, finalSheet, "Final Weights")

    XLSX.writeFile(wb, `reweigh-attempts-${t.receiptNo || "N-A"}-${dayjs().format("YYYY-MM-DD")}.xlsx`)
  }


  // ── RENDER ──────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-3">

      {/* ── FILTER BAR ──────────────────────────────────────────────────────── */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-amber-50/20 border border-amber-200 rounded-lg p-3">
        <div className="flex justify-between items-center mb-2">
          <span className="text-[10px] font-bold text-gray-900 uppercase tracking-wide">
            Reweigh Transactions
          </span>
          <div className="flex items-center gap-2">
            {!scanDone ? (
              <button
                onClick={scanAll}
                disabled={isScanning || parentLoading}
                className="flex items-center gap-1 px-2 py-1 text-[9px] font-bold bg-blue-50 border border-blue-200 text-blue-700 rounded hover:bg-blue-100 disabled:opacity-50"
              >
                {isScanning ? <Spin size="small" /> : <RefreshCw size={10} />}
                {isScanning ? `Scanning…` : "Scan All Transactions"}
              </button>
            ) : (
              <span className="text-[9px] font-bold text-green-700 bg-green-50 border border-green-200 px-2 py-0.5 rounded">
                ✓ Full scan done
              </span>
            )}
            <Button
              icon={<Filter size={12} />}
              size="small"
              className={`h-6 text-[10px] font-medium ${showFilters ? "bg-amber-500 text-white border-amber-500" : "border-gray-300"}`}
              onClick={() => setShowFilters(!showFilters)}
            >
              {showFilters ? "Hide" : "Filters"}
            </Button>
          </div>
        </div>

        {showFilters && (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-4 gap-2 mb-2">
              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block">Start Date</label>
                <input type="date" value={filters.startDate}
                  onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                  className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                />
              </div>
              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block">End Date</label>
                <input type="date" value={filters.endDate}
                  onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                  className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                />
              </div>
              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block">Status</label>
                <select value={filters.status}
                  onChange={(e) => setFilters({ ...filters, status: e.target.value })}
                  className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 bg-white focus:border-amber-500"
                >
                  <option value="">All</option>
                  <option value="Pending">Pending</option>
                  <option value="Approved">Approved</option>
                  <option value="Rejected">Rejected</option>
                </select>
              </div>
              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block">Search</label>
                <Input
                  placeholder="Receipt, vehicle, driver..."
                  value={filters.search}
                  onChange={(e) => setFilters({ ...filters, search: e.target.value })}
                  className="h-6 text-[10px] border-amber-300 focus:border-amber-500"
                  allowClear
                />
              </div>
            </div>

            <div className="flex justify-between items-center">
              <div className="flex gap-2 items-center">
                {activeFilterCount > 0 && (
                  <span className="text-[9px] text-amber-900 font-bold bg-gradient-to-r from-amber-100 to-amber-200 px-2 py-0.5 rounded-full border border-amber-300 shadow-sm">
                    🎯 {activeFilterCount} active
                  </span>
                )}
              </div>
              <div className="flex gap-2">
                {activeFilterCount > 0 && (
                  <Button size="small" danger icon={<CloseOutlined />} onClick={clearFilters}
                    className="h-6 text-[10px] font-semibold shadow-sm rounded bg-red-50 border-red-300 text-red-700 hover:bg-red-100">
                    Clear
                  </Button>
                )}
                <button onClick={() => { setExportType("pdf"); setShowExportPreview(true) }}
                  className="flex items-center gap-1.5 px-3 py-1 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-[10px] font-medium hover:bg-amber-200">
                  <FileDown size={12} /> PDF
                </button>
                <button onClick={() => { setExportType("excel"); setShowExportPreview(true) }}
                  className="flex items-center gap-1.5 px-3 py-1 border border-gray-300 bg-white rounded-lg text-[10px] font-medium hover:bg-gray-50">
                  <FileSpreadsheet size={12} /> Excel
                </button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* ── KPIs ────────────────────────────────────────────────────────────── */}
      <div className="grid grid-cols-4 gap-3">
        <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 shadow-sm">
          <p className="text-[10px] text-gray-700 uppercase font-semibold tracking-wide">Total</p>
          <p className="text-xl font-bold leading-tight text-amber-900">{kpis.total}</p>
        </div>
        <div className="bg-amber-100 border border-amber-300 rounded-lg px-3 py-2 shadow-sm">
          <p className="text-[10px] text-amber-900 uppercase font-semibold tracking-wide">Pending</p>
          <p className="text-xl font-bold leading-tight text-amber-950">{kpis.pending}</p>
        </div>
        <div className="bg-green-50 border border-green-200 rounded-lg px-3 py-2 shadow-sm">
          <p className="text-[10px] text-green-700 uppercase font-semibold tracking-wide">Approved</p>
          <p className="text-xl font-bold leading-tight text-green-900">{kpis.approved}</p>
        </div>
        <div className="bg-red-50 border border-red-200 rounded-lg px-3 py-2 shadow-sm">
          <p className="text-[10px] text-red-700 uppercase font-semibold tracking-wide">Rejected</p>
          <p className="text-xl font-bold leading-tight text-red-900">{kpis.rejected}</p>
        </div>
      </div>

      {/* ── TABLE ───────────────────────────────────────────────────────────── */}
      <div className="border border-amber-200 rounded-lg overflow-auto bg-white">
        {parentLoading && propTransactions.length === 0 ? (
          <div className="flex items-center justify-center py-16">
            <Spin size="default" />
            <span className="ml-3 text-[11px] text-gray-500">Loading transactions…</span>
          </div>
        ) : filteredRows.length === 0 ? (
          <div className="py-16 text-center">
            <p className="text-[11px] text-gray-400 font-medium">No reweigh transactions found.</p>
            <p className="text-[10px] text-gray-300 mt-1">
              {Object.values(filters).some(Boolean)
                ? "No records match the current filters."
                : scanDone
                ? "All transactions scanned — none have reweigh records."
                : 'If you expected results, click "Scan All Transactions" to search every record.'}
            </p>
          </div>
        ) : (
          <table className="w-full text-xs min-w-[1100px]">
            <thead className="bg-gradient-to-b from-amber-50 to-amber-100/50 sticky top-0">
              <tr>
                {["#","Date","Receipt No","Vehicle","Driver","Commodity","First Wt (kg)","Second Wt (kg)","Net Wt (kg)","Reweigh Status",""].map((h, i) => (
                  <th key={i} className={`p-2 font-bold text-[9px] text-amber-900 uppercase border-b-2 border-amber-200 ${i >= 6 && i <= 8 ? "text-right" : "text-left"}`}>
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {paginatedRows.map((row, idx) => {
                const t = row.transaction
                const id = t.ticketID || t.id
                const isFetching = fetchingIds.has(id)
                return (
                  <tr
                    key={`${id}-${idx}`}
                    className={`border-t border-gray-100 hover:bg-amber-50/30 transition-colors cursor-pointer ${
                      idx % 2 === 0 ? "bg-white" : "bg-gray-50/50"
                    }`}
                    onClick={() => handleRowClick(row)}
                  >
                    <td className="p-2 text-[10px] text-gray-400 font-mono">{(currentPage - 1) * PAGE_SIZE + idx + 1}</td>
                    <td className="p-2 text-[10px]">{dayjs(t.createdAt).format("DD MMM YYYY, HH:mm")}</td>
                    <td className="p-2 text-[10px] font-mono font-bold text-amber-600">{t.receiptNo || "-"}</td>
                    <td className="p-2">
                      <span className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[9px] font-bold">{t.noPlate || "-"}</span>
                    </td>
                    <td className="p-2 text-[10px] font-medium">{t.driverName || "-"}</td>
                    <td className="p-2 text-[10px] text-gray-600">{t.commodityName || "-"}</td>
                    <td className="p-2 text-right text-[10px] font-bold text-amber-600">
                      {t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : "-"}
                    </td>
                    <td className="p-2 text-right text-[10px] font-bold text-blue-600">
                      {t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : "-"}
                    </td>
                    <td className="p-2 text-right text-[10px] font-bold text-amber-700">
                      {t.netWeight ? parseFloat(t.netWeight).toLocaleString() : "-"}
                    </td>
                    <td className="p-2">
                      {isFetching
                        ? <Spin size="small" />
                        : <StatusBadge status={row.reweighStatus} />}
                    </td>
                    <td className="p-2">
                      <button className="text-[9px] text-amber-700 hover:underline font-semibold">
                        View Details
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* ── PAGINATION ──────────────────────────────────────────────────────── */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between px-1">
          <span className="text-[10px] text-gray-500">
            {filteredRows.length} records · Page {currentPage} of {totalPages}
          </span>
          <div className="flex gap-1">
            <button
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              disabled={currentPage === 1}
              className="px-2 py-1 text-[10px] border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
            >
              ← Prev
            </button>
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              const page = Math.max(1, Math.min(currentPage - 2, totalPages - 4)) + i
              return page <= totalPages ? (
                <button
                  key={page}
                  onClick={() => setCurrentPage(page)}
                  className={`px-2 py-1 text-[10px] border rounded ${
                    page === currentPage ? "bg-amber-500 text-white border-amber-500" : "border-gray-300 hover:bg-gray-50"
                  }`}
                >
                  {page}
                </button>
              ) : null
            })}
            <button
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
              className="px-2 py-1 text-[10px] border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
            >
              Next →
            </button>
          </div>
        </div>
      )}

      {/* ── TABLE EXPORT PREVIEW MODAL ──────────────────────────────────────── */}
      {showExportPreview && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg w-full max-w-6xl max-h-[90vh] flex flex-col shadow-2xl">
            <div className="p-4 border-b bg-amber-50">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-900">Export Preview — Reweigh Transactions</h2>
                  <p className="text-xs text-gray-600 mt-1">
                    <span className="font-semibold text-amber-700">{filteredRows.length}</span> records
                  </p>
                </div>
                <button onClick={() => setShowExportPreview(false)} className="text-gray-400 hover:text-gray-600">
                  <X size={20} />
                </button>
              </div>
            </div>

            <div className="flex-1 overflow-auto p-4">
              <div className="border rounded-lg overflow-auto">
                <table className="w-full text-xs min-w-[1400px]">
                  <thead className="bg-amber-50 sticky top-0">
                    <tr>
                      {["#","Date","Receipt No","Vehicle","Driver","Commodity","Request Reason","First Wt","Second Wt","Net Wt","Decision By","Decision Notes","Status"].map((h) => (
                        <th key={h} className="p-2 text-left font-semibold text-[9px] text-amber-900 uppercase border-b border-amber-200">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {filteredRows.map((row, idx) => {
                      const t = row.transaction
                      const rec = row.record || {}
                      return (
                        <tr key={idx} className={`border-t border-gray-100 ${idx % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                          <td className="p-2 text-gray-400 font-mono">{idx + 1}</td>
                          <td className="p-2">{dayjs(t.createdAt).format("DD MMM YYYY HH:mm")}</td>
                          <td className="p-2 font-mono font-bold text-amber-600">{t.receiptNo || "-"}</td>
                          <td className="p-2">
                            <span className="bg-gray-900 text-white px-1.5 py-0.5 rounded text-[9px] font-bold">{t.noPlate || "-"}</span>
                          </td>
                          <td className="p-2 font-medium">{t.driverName || "-"}</td>
                          <td className="p-2 text-gray-600">{t.commodityName || "-"}</td>
                          <td className="p-2 text-gray-600 max-w-[140px] truncate">{row.requestRecord?.reason || t.reweighPermission || "-"}</td>
                          <td className="p-2 text-right font-bold text-amber-600">{t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : "-"}</td>
                          <td className="p-2 text-right font-bold text-blue-600">{t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : "-"}</td>
                          <td className="p-2 text-right font-bold text-amber-700">{t.netWeight ? parseFloat(t.netWeight).toLocaleString() : "-"}</td>
                          <td className="p-2 font-medium">{row.record?.performedBy || "-"}</td>
                          <td className="p-2 text-gray-600 max-w-[120px] truncate">{row.record?.notes || "-"}</td>
                          <td className="p-2"><StatusBadge status={row.reweighStatus} /></td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="p-4 border-t bg-amber-50/50 flex justify-end gap-2">
              <button onClick={() => setShowExportPreview(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white">
                Cancel
              </button>
              <button
                onClick={() => { exportType === "pdf" ? exportTablePDF() : exportTableExcel(); setShowExportPreview(false) }}
                className="px-4 py-2 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200">
                Download {exportType?.toUpperCase()}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── DETAIL POPUP MODAL ──────────────────────────────────────────────── */}
      {showDetailModal && selectedEntry && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl w-full max-w-3xl max-h-[90vh] flex flex-col shadow-2xl">

            {/* Modal header */}
            <div className="p-4 border-b bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50 rounded-t-xl">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-base font-black text-gray-900">Reweigh Details</h2>
                  <p className="text-[10px] text-gray-500 mt-0.5">
                    Transaction{" "}
                    <span className="font-bold text-amber-700 font-mono">
                      {selectedEntry.transaction.receiptNo || "-"}
                    </span>
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => { setPopupExportType("pdf"); setShowPopupExport(true) }}
                    className="flex items-center gap-1 px-2.5 py-1 bg-amber-100 text-amber-900 border border-amber-300 rounded text-[10px] font-medium hover:bg-amber-200"
                  >
                    <FileDown size={11} /> PDF
                  </button>
                  <button
                    onClick={() => { setPopupExportType("excel"); setShowPopupExport(true) }}
                    className="flex items-center gap-1 px-2.5 py-1 border border-gray-300 bg-white rounded text-[10px] font-medium hover:bg-gray-50"
                  >
                    <FileSpreadsheet size={11} /> Excel
                  </button>
                  <button onClick={() => setShowDetailModal(false)} className="text-gray-400 hover:text-gray-600">
                    <X size={18} />
                  </button>
                </div>
              </div>
            </div>

            {/* ── Scrollable content ──────────────────────────────────────── */}
            <div className="flex-1 overflow-auto p-4 space-y-4">
              {(() => {
                const tx           = selectedEntry.transaction
                const id           = tx.ticketID || tx.id
                const loading      = fetchingIds.has(id)
                const records      = [...(selectedEntry.records || [])].sort((a, b) => (a.attemptNumber || 0) - (b.attemptNumber || 0))
                const approvedRecs = records.filter(r => resolveRecordStatus(r) === "Approved")
                const pendingRecs  = records.filter(r => resolveRecordStatus(r) === "Pending")
                const latestApproved = approvedRecs[approvedRecs.length - 1]
                const latestStatus = records.length > 0 ? resolveRecordStatus(records[records.length - 1]) : "Pending"
                const finalNet     = latestApproved?.netWeight ?? tx.netWeight
                return (
                  <>
                    {/* Transaction info */}
                    <div className="rounded-xl bg-gray-900 text-white p-3">
                      <div className="flex items-start justify-between mb-2">
                        <div>
                          <div className="text-[7px] text-amber-400 font-bold uppercase">Ticket No</div>
                          <div className="text-base font-black font-mono text-amber-400 leading-tight">{tx.receiptNo || "—"}</div>
                          <div className="text-[8px] text-gray-400 mt-0.5 font-mono truncate max-w-[180px]">{tx.ticketID || "—"}</div>
                        </div>
                        <div className="text-right">
                          <div className="text-[7px] text-gray-400 font-bold uppercase">Vehicle</div>
                          <div className="text-base font-black leading-tight">{tx.noPlate || "—"}</div>
                          <div className="text-[8px] text-gray-400 mt-0.5">{tx.driverName || "—"} · {tx.commodityName || "—"}</div>
                        </div>
                      </div>
                      <div className="grid grid-cols-3 gap-1.5 bg-white/10 rounded-lg p-2 mt-1">
                        {[["Gross", tx.firstWeight, "text-white"], ["Tare", tx.secondWeight, "text-amber-400"], ["Net", finalNet, "text-green-400"]].map(([lbl, val, col]) => (
                          <div key={lbl} className="text-center">
                            <div className="text-[7px] text-gray-400 font-bold uppercase">{lbl}</div>
                            <div className={`text-sm font-black font-mono ${col}`}>
                              {val != null && val !== "" ? parseFloat(val).toLocaleString() : "—"}
                              <span className="text-[8px] font-normal text-gray-500 ml-0.5">kg</span>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Summary KPIs */}
                    <div className="grid grid-cols-4 gap-2">
                      {[
                        { lbl: "Total Attempts", val: records.length,       bg: "bg-gray-50",  text: "text-gray-800",  border: "border-gray-200" },
                        { lbl: "Approved",        val: approvedRecs.length,  bg: "bg-green-50", text: "text-green-800", border: "border-green-200" },
                        { lbl: "Pending",         val: pendingRecs.length,   bg: "bg-amber-50", text: "text-amber-800", border: "border-amber-200" },
                        { lbl: "Latest Status",   val: latestStatus,
                          bg:     latestStatus === "Approved" ? "bg-green-50"  : latestStatus === "Rejected" ? "bg-red-50"  : "bg-amber-50",
                          text:   latestStatus === "Approved" ? "text-green-800" : latestStatus === "Rejected" ? "text-red-800" : "text-amber-800",
                          border: latestStatus === "Approved" ? "border-green-200" : latestStatus === "Rejected" ? "border-red-200" : "border-amber-200",
                        },
                      ].map(({ lbl, val, bg, text, border }) => (
                        <div key={lbl} className={`${bg} border ${border} rounded-lg p-2 text-center`}>
                          <div className="text-[7px] text-gray-500 uppercase font-bold">{lbl}</div>
                          <div className={`text-base font-black ${text}`}>{val}</div>
                        </div>
                      ))}
                    </div>

                    {/* Attempts table */}
                    {loading ? (
                      <div className="flex items-center gap-2 py-8 justify-center">
                        <Spin /> <span className="text-[11px] text-gray-500">Loading attempts…</span>
                      </div>
                    ) : records.length === 0 ? (
                      <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-center">
                        <p className="text-[11px] text-gray-400">No reweigh records found.</p>
                      </div>
                    ) : (
                      <>
                        <div>
                          <div className="text-[9px] font-black text-gray-800 uppercase tracking-wide mb-1.5">
                            Reweigh Attempts
                            <span className="ml-2 text-[8px] font-normal text-gray-400 normal-case">{records.length} record{records.length !== 1 ? "s" : ""}</span>
                          </div>
                          <div className="border border-gray-200 rounded-lg overflow-hidden">
                            <table className="w-full text-xs">
                              <thead className="bg-gray-800 text-white">
                                <tr>
                                  {["#","Status","Started At","Reason","W1","W2","Net","Operator","Notes"].map(h => (
                                    <th key={h} className="p-1.5 text-left text-[8px] font-bold uppercase whitespace-nowrap">{h}</th>
                                  ))}
                                </tr>
                              </thead>
                              <tbody>
                                {records.map((rec, idx) => {
                                  const rs = resolveRecordStatus(rec)
                                  return (
                                    <tr key={idx} className={`border-t border-gray-100 ${idx % 2 === 0 ? "bg-white" : "bg-gray-50/50"}`}>
                                      <td className="p-1.5 text-[9px] font-bold text-gray-400 text-center">{rec.attemptNumber || idx + 1}</td>
                                      <td className="p-1.5"><StatusBadge status={rs} /></td>
                                      <td className="p-1.5 text-[9px] whitespace-nowrap">{rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YY HH:mm") : "—"}</td>
                                      <td className="p-1.5 text-[9px] max-w-[90px] truncate">{rec.reason || "—"}</td>
                                      <td className="p-1.5 text-[9px] text-right font-mono">{rec.weight1   != null ? parseFloat(rec.weight1).toLocaleString()   : "—"}</td>
                                      <td className="p-1.5 text-[9px] text-right font-mono">{rec.weight2   != null ? parseFloat(rec.weight2).toLocaleString()   : "—"}</td>
                                      <td className="p-1.5 text-[9px] text-right font-mono font-bold">{rec.netWeight != null ? parseFloat(rec.netWeight).toLocaleString() : "—"}</td>
                                      <td className="p-1.5 text-[9px]">{rec.performedBy || rec.operator1 || "—"}</td>
                                      <td className="p-1.5 text-[9px] text-gray-500 max-w-[80px] truncate">{rec.notes || "—"}</td>
                                    </tr>
                                  )
                                })}
                              </tbody>
                            </table>
                          </div>
                        </div>

                        {/* Approval history */}
                        {approvedRecs.length > 0 && (
                          <div>
                            <div className="text-[9px] font-black text-gray-800 uppercase tracking-wide mb-1.5">Approval History</div>
                            <div className="border border-green-200 bg-green-50 rounded-lg p-2.5 space-y-1.5">
                              {approvedRecs.map((rec, idx) => {
                                const isFinal = idx === approvedRecs.length - 1
                                return (
                                  <div key={idx} className="flex items-center gap-2">
                                    <span className={`text-[9px] ${isFinal ? "font-black text-green-800" : "text-green-700"}`}>
                                      • Attempt {rec.attemptNumber} approved{isFinal ? " (FINAL DECISION)" : ""}
                                    </span>
                                    {rec.performedBy && <span className="text-[8px] text-gray-500">by {rec.performedBy}</span>}
                                  </div>
                                )
                              })}
                              {finalNet != null && (
                                <div className="mt-1 pt-1.5 border-t border-green-200 text-[9px] font-black text-green-800">
                                  Final Net Weight: {parseFloat(finalNet).toLocaleString()} KG
                                </div>
                              )}
                            </div>
                          </div>
                        )}

                        {/* Timeline */}
                        <div>
                          <div className="text-[9px] font-black text-gray-800 uppercase tracking-wide mb-1.5">Timeline</div>
                          <div className="border border-gray-200 rounded-lg overflow-hidden divide-y divide-gray-100">
                            {records.map((rec, idx) => {
                              const rs      = resolveRecordStatus(rec)
                              const isFinal = rec === latestApproved
                              const col     = rs === "Approved" ? "text-green-700" : rs === "Rejected" ? "text-red-700" : "text-amber-700"
                              const bg      = rs === "Approved" ? "bg-green-50/60"  : rs === "Rejected" ? "bg-red-50/60"  : "bg-amber-50/60"
                              return (
                                <div key={idx} className={`flex items-center gap-3 px-3 py-1.5 ${bg}`}>
                                  <span className="text-[8px] text-gray-400 font-mono whitespace-nowrap">
                                    {rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YY HH:mm") : "—"}
                                  </span>
                                  <span className="text-gray-300">→</span>
                                  <span className={`text-[9px] font-semibold ${col}`}>
                                    Attempt {rec.attemptNumber} {rs === "Pending" ? "created (Pending)" : rs === "Approved" ? `approved${isFinal ? " — Final" : ""}` : "rejected"}
                                  </span>
                                </div>
                              )
                            })}
                          </div>
                        </div>

                        {/* Final weight */}
                        <div>
                          <div className="text-[9px] font-black text-gray-800 uppercase tracking-wide mb-1.5">Final Weight Used in System</div>
                          <div className="rounded-xl border border-green-200 bg-green-50 p-3">
                            <div className="grid grid-cols-3 gap-3 mb-2">
                              {[["Gross Weight", tx.firstWeight], ["Tare Weight", tx.secondWeight], ["Final Net Wt", finalNet]].map(([lbl, val]) => (
                                <div key={lbl} className="text-center">
                                  <div className="text-[7px] text-gray-500 uppercase font-bold">{lbl}</div>
                                  <div className="text-base font-black font-mono text-green-800">
                                    {val != null ? parseFloat(val).toLocaleString() : "—"}
                                    <span className="text-[8px] font-normal text-gray-500 ml-0.5">kg</span>
                                  </div>
                                </div>
                              ))}
                            </div>
                            {latestApproved && (
                              <div className="text-center text-[8px] text-gray-500 italic">
                                Source: Approved Reweigh Attempt #{latestApproved.attemptNumber}
                              </div>
                            )}
                          </div>
                        </div>
                      </>
                    )}
                  </>
                )
              })()}
            </div>{/* /scrollable content */}

            <div className="p-4 border-t bg-amber-50/50 flex justify-end gap-2 flex-shrink-0">
              <button onClick={() => setShowDetailModal(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white">
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── POPUP EXPORT PREVIEW MODAL ──────────────────────────────────────── */}
      {showPopupExport && selectedEntry && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-xl w-full max-w-2xl max-h-[80vh] flex flex-col shadow-2xl">

            {/* Header */}
            <div className="p-4 border-b bg-amber-50 flex-shrink-0">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-sm font-bold text-gray-900">Export Preview — Reweigh Attempts</h2>
                  <p className="text-[10px] text-gray-500 mt-0.5">
                    Ticket: <span className="font-bold text-amber-700">{selectedEntry.transaction.receiptNo || "—"}</span>
                    {" · "}
                    {(selectedEntry.records || []).length} attempt{(selectedEntry.records || []).length !== 1 ? "s" : ""}
                  </p>
                </div>
                <button onClick={() => setShowPopupExport(false)} className="text-gray-400 hover:text-gray-600">
                  <X size={18} />
                </button>
              </div>
            </div>

            {/* Body — mirrors PDF/Excel structure */}
            {(() => {
              const tx           = selectedEntry.transaction
              const records      = [...(selectedEntry.records || [])].sort((a, b) => (a.attemptNumber || 0) - (b.attemptNumber || 0))
              const approvedRecs = records.filter(r => resolveRecordStatus(r) === "Approved")
              const pendingRecs  = records.filter(r => resolveRecordStatus(r) === "Pending")
              const latestApproved = approvedRecs[approvedRecs.length - 1]
              const latestStatus = records.length > 0 ? resolveRecordStatus(records[records.length - 1]) : "Pending"
              const finalNet     = latestApproved?.netWeight ?? tx.netWeight
              return (
                <div className="flex-1 overflow-auto p-4 space-y-4">

                  {/* Ticket info */}
                  <div className="rounded-xl bg-gray-900 text-white p-3">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <div className="text-[7px] text-amber-400 font-bold uppercase">Ticket No</div>
                        <div className="text-base font-black font-mono text-amber-400">{tx.receiptNo || "—"}</div>
                        <div className="text-[8px] text-gray-400 mt-0.5">{tx.driverName || "—"} · {tx.commodityName || "—"}</div>
                      </div>
                      <div className="text-right">
                        <div className="text-[7px] text-gray-400 font-bold uppercase">Vehicle</div>
                        <div className="text-base font-black">{tx.noPlate || "—"}</div>
                        <div className="text-[8px] text-gray-400 mt-0.5">{dayjs(tx.firstWeightDate || tx.createdAt).format("DD MMM YYYY HH:mm")}</div>
                      </div>
                    </div>
                    <div className="grid grid-cols-3 gap-1.5 bg-white/10 rounded-lg p-2">
                      {[["Gross", tx.firstWeight, "text-white"], ["Tare", tx.secondWeight, "text-amber-400"], ["Net", finalNet, "text-green-400"]].map(([lbl, val, col]) => (
                        <div key={lbl} className="text-center">
                          <div className="text-[7px] text-gray-400 font-bold uppercase">{lbl}</div>
                          <div className={`text-sm font-black font-mono ${col}`}>
                            {val != null && val !== "" ? parseFloat(val).toLocaleString() : "—"}
                            <span className="text-[8px] font-normal text-gray-500 ml-0.5">kg</span>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Summary */}
                  <div className="grid grid-cols-4 gap-2">
                    {[
                      { lbl: "Total", val: records.length,       bg: "bg-gray-50",  text: "text-gray-800",  bd: "border-gray-200" },
                      { lbl: "Approved", val: approvedRecs.length, bg: "bg-green-50", text: "text-green-800", bd: "border-green-200" },
                      { lbl: "Pending",  val: pendingRecs.length,  bg: "bg-amber-50", text: "text-amber-800", bd: "border-amber-200" },
                      { lbl: "Status",   val: latestStatus,
                        bg: latestStatus === "Approved" ? "bg-green-50" : latestStatus === "Rejected" ? "bg-red-50" : "bg-amber-50",
                        text: latestStatus === "Approved" ? "text-green-800" : latestStatus === "Rejected" ? "text-red-800" : "text-amber-800",
                        bd: latestStatus === "Approved" ? "border-green-200" : latestStatus === "Rejected" ? "border-red-200" : "border-amber-200",
                      },
                    ].map(({ lbl, val, bg, text, bd }) => (
                      <div key={lbl} className={`${bg} border ${bd} rounded-lg p-2 text-center`}>
                        <div className="text-[7px] text-gray-500 uppercase font-bold">{lbl}</div>
                        <div className={`text-base font-black ${text}`}>{val}</div>
                      </div>
                    ))}
                  </div>

                  {/* Attempts table */}
                  <div>
                    <div className="text-[9px] font-black text-gray-700 uppercase tracking-wide mb-1.5">Reweigh Attempts</div>
                    {records.length === 0 ? (
                      <div className="text-[10px] text-gray-400 text-center py-4 border border-gray-200 rounded-lg">No records found.</div>
                    ) : (
                      <div className="border border-gray-200 rounded-lg overflow-hidden">
                        <table className="w-full text-xs">
                          <thead className="bg-gray-800 text-white">
                            <tr>
                              {["#","Status","Started At","Reason","W1","W2","Net","Operator","Notes"].map(h => (
                                <th key={h} className="p-1.5 text-left text-[8px] font-bold uppercase whitespace-nowrap">{h}</th>
                              ))}
                            </tr>
                          </thead>
                          <tbody>
                            {records.map((rec, idx) => {
                              const rs = resolveRecordStatus(rec)
                              return (
                                <tr key={idx} className={`border-t border-gray-100 ${idx % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                                  <td className="p-1.5 text-[9px] font-bold text-gray-400 text-center">{rec.attemptNumber || idx + 1}</td>
                                  <td className="p-1.5"><StatusBadge status={rs} /></td>
                                  <td className="p-1.5 text-[9px] whitespace-nowrap">{rec.startedAt ? dayjs(rec.startedAt).format("DD-MM-YY HH:mm") : "—"}</td>
                                  <td className="p-1.5 text-[9px] max-w-[90px] truncate">{rec.reason || "—"}</td>
                                  <td className="p-1.5 text-[9px] text-right font-mono">{rec.weight1   != null ? parseFloat(rec.weight1).toLocaleString()   : "—"}</td>
                                  <td className="p-1.5 text-[9px] text-right font-mono">{rec.weight2   != null ? parseFloat(rec.weight2).toLocaleString()   : "—"}</td>
                                  <td className="p-1.5 text-[9px] text-right font-mono font-bold">{rec.netWeight != null ? parseFloat(rec.netWeight).toLocaleString() : "—"}</td>
                                  <td className="p-1.5 text-[9px]">{rec.performedBy || rec.operator1 || "—"}</td>
                                  <td className="p-1.5 text-[9px] text-gray-500 max-w-[70px] truncate">{rec.notes || "—"}</td>
                                </tr>
                              )
                            })}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>

                  {/* Approval history */}
                  {approvedRecs.length > 0 && (
                    <div>
                      <div className="text-[9px] font-black text-gray-700 uppercase tracking-wide mb-1.5">Approval History</div>
                      <div className="border border-green-200 bg-green-50 rounded-lg p-2.5 space-y-1">
                        {approvedRecs.map((rec, idx) => {
                          const isFinal = idx === approvedRecs.length - 1
                          return (
                            <div key={idx} className="flex items-center gap-2">
                              <span className={`text-[9px] ${isFinal ? "font-black text-green-800" : "text-green-700"}`}>
                                • Attempt {rec.attemptNumber} approved{isFinal ? " (FINAL DECISION)" : ""}
                              </span>
                              {rec.performedBy && <span className="text-[8px] text-gray-500">by {rec.performedBy}</span>}
                            </div>
                          )
                        })}
                        {finalNet != null && (
                          <div className="pt-1.5 border-t border-green-200 text-[9px] font-black text-green-800">
                            Final Net Weight: {parseFloat(finalNet).toLocaleString()} KG
                          </div>
                        )}
                      </div>
                    </div>
                  )}

                  {/* Final weights */}
                  <div>
                    <div className="text-[9px] font-black text-gray-700 uppercase tracking-wide mb-1.5">Final Weight Used in System</div>
                    <div className="border border-green-200 bg-green-50 rounded-lg p-3">
                      <div className="grid grid-cols-3 gap-2 mb-2">
                        {[["Gross Weight", tx.firstWeight], ["Tare Weight", tx.secondWeight], ["Final Net Wt", finalNet]].map(([lbl, val]) => (
                          <div key={lbl} className="text-center">
                            <div className="text-[7px] text-gray-500 uppercase font-bold">{lbl}</div>
                            <div className="text-base font-black font-mono text-green-800">
                              {val != null ? parseFloat(val).toLocaleString() : "—"}
                              <span className="text-[8px] font-normal text-gray-500 ml-0.5">kg</span>
                            </div>
                          </div>
                        ))}
                      </div>
                      {latestApproved && (
                        <div className="text-center text-[8px] text-gray-500 italic">
                          Source: Approved Reweigh Attempt #{latestApproved.attemptNumber}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )
            })()}

            <div className="p-4 border-t bg-amber-50/50 flex justify-end gap-2 flex-shrink-0">
              <button onClick={() => setShowPopupExport(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white">
                Cancel
              </button>
              <button
                onClick={() => {
                  popupExportType === "pdf" ? exportPopupPDF() : exportPopupExcel()
                  setShowPopupExport(false)
                }}
                className="px-4 py-2 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200">
                Download {popupExportType?.toUpperCase()}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
