import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Button, Input, DatePicker, message } from "antd";
import { ReloadOutlined } from "@ant-design/icons";
import ReweighModal from "../components/weighing/ReweighModal";
import AddWeighingModal from "../components/weighing/AddWeighingModal";
import ReweighFirstWeightModal from "../components/weighing/ReweighFirstWeightModal";
import {
  fetchTransactions,
  fetchUserById,
  updateTransactionApi,
  fetchReweighRecords,
} from "../store/weighingSlice";
import { Eye, Search, Filter, X, CheckCircle2, Receipt } from "lucide-react";
import PageHeader from "../components/PageHeader.jsx";
import TransactionDrawer from "../components/transaction/TransactionDrawer";
import ExportPreviewModal from "../components/transaction/ExportPreviewModal";
import { getTransactionColumns, formatTurnaroundTimeSimple } from "../components/transaction/transactionColumns";

// ─── Theme utilities ──────────────────────────────────────────────────────────
import {
  getTicketSettings,
} from "../utils/ticketThemeConfig";

const { RangePicker } = DatePicker;

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function Transactions() {
  const dispatch = useDispatch();
  const { transactions, loading, total } = useSelector(
    (state) => state.weighing
  );
  const [selectedRecord, setSelectedRecord] = useState(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [reweighRecordsForDrawer, setReweighRecordsForDrawer] = useState([]);
  const [reweighModal, setReweighModal] = useState({ visible: false, transaction: null });
  const [weighingModal, setWeighingModal] = useState({ visible: false, transaction: null });
  const [firstWeightModal, setFirstWeightModal] = useState({ visible: false, transaction: null });
  const [pdfTheme, setPdfTheme] = useState(() => getTicketSettings().ticketTheme || "modern");
  const [isExportPreviewOpen, setIsExportPreviewOpen] = useState(false);


  // ── Live ticket settings (synced from SystemSettings) ─────────────────────
  const [ticketSettings, setTicketSettings] = useState(getTicketSettings);

  // Listen for changes broadcast by SystemSettings
  useEffect(() => {
    const handler = (e) => setTicketSettings((prev) => ({ ...prev, ...e.detail }));
    window.addEventListener("ticketSettingsChanged", handler);
    return () => window.removeEventListener("ticketSettingsChanged", handler);
  }, []);

  const [filters, setFilters] = useState({
    search: "",
    dateRange: null,
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: null,
    vehicle: "",
    driver: "",
    commodity: "",
    supplier: "",
    transporter: "",
    customer: "",
    operator: "",
    origin: "",
    destination: "",
    weighbridge: "",
    weighMode: null,
    page: 1,
    pageSize: 10,
  });

  const loadTransactions = useCallback(() => {
    const params = {
      pageNumber: filters.page,
      pageSize: filters.pageSize,
    };

    // Search is applied client-side below (see filteredTransactions) instead
    // of sent to the backend — the Transaction endpoint's `search` param only
    // matches TransactionNumber, so a driver/vehicle/commodity search sent to
    // the server would come back empty even though matches exist on this page.
    if (filters.dateRange && filters.dateRange[0] && filters.dateRange[1]) {
      params.startDate = filters.dateRange[0].format("YYYY-MM-DD");
      params.endDate = filters.dateRange[1].format("YYYY-MM-DD");
    } else {
      if (filters.startDate) params.startDate = filters.startDate;
      if (filters.endDate) params.endDate = filters.endDate;
    }

    if (filters.startTime) params.startTime = filters.startTime;
    if (filters.endTime) params.endTime = filters.endTime;
    if (filters.status) params.status = filters.status;

    dispatch(fetchTransactions(params));
  }, [
    dispatch,
    filters.page,
    filters.pageSize,
    filters.dateRange,
    filters.startDate,
    filters.endDate,
    filters.startTime,
    filters.endTime,
    filters.status,
  ]);

  const filteredTransactions = React.useMemo(() => {
    let filtered = transactions || [];

    if (filters.search) {
      const q = filters.search.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.receiptNo?.toLowerCase().includes(q) ||
          t.noPlate?.toLowerCase().includes(q) ||
          t.driverName?.toLowerCase().includes(q) ||
          t.commodityName?.toLowerCase().includes(q) ||
          t.supplierName?.toLowerCase().includes(q) ||
          t.transporterName?.toLowerCase().includes(q) ||
          t.customerName?.toLowerCase().includes(q) ||
          t.originName?.toLowerCase().includes(q) ||
          t.destinationName?.toLowerCase().includes(q) ||
          t.weighBridgeName?.toLowerCase().includes(q)
      );
    }

    if (filters.status) {
      if (filters.status === "completed") {
        filtered = filtered.filter(
          (t) =>
            (t.secondWeight && parseFloat(t.secondWeight) > 0) ||
            t.status === "Completed" ||
            t.status === "completed"
        );
      } else if (filters.status === "inprogress") {
        filtered = filtered.filter(
          (t) =>
            (!t.secondWeight || parseFloat(t.secondWeight) === 0) &&
            t.status !== "Completed" &&
            t.status !== "completed"
        );
      }
    }

    if (filters.startDate && !filters.dateRange) {
      const start = new Date(`${filters.startDate}T${filters.startTime || "00:00"}`);
      filtered = filtered.filter((t) => new Date(t.createdAt) >= start);
    }
    if (filters.endDate && !filters.dateRange) {
      const end = new Date(`${filters.endDate}T${filters.endTime || "23:59"}`);
      filtered = filtered.filter((t) => new Date(t.createdAt) <= end);
    }

    const textFilters = [
      ["vehicle",     "noPlate"],
      ["driver",      "driverName"],
      ["commodity",   "commodityName"],
      ["supplier",    "supplierName"],
      ["transporter", "transporterName"],
      ["customer",    "customerName"],
      ["origin",      "originName"],
      ["destination", "destinationName"],
      ["weighbridge", "weighBridgeName"],
    ];

    textFilters.forEach(([filterKey, dataKey]) => {
      if (filters[filterKey]) {
        const val = filters[filterKey].toLowerCase();
        filtered = filtered.filter((t) => t[dataKey]?.toLowerCase().includes(val));
      }
    });

    if (filters.operator) {
      const op = filters.operator.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.operatorName?.toLowerCase().includes(op) ||
          t.firstWeightOperator?.toLowerCase().includes(op)
      );
    }
    if (filters.weighMode) {
      filtered = filtered.filter(
        (t) => t.weighMode?.toLowerCase() === filters.weighMode.toLowerCase()
      );
    }

    return filtered;
  }, [transactions, filters]);

  useEffect(() => {
    loadTransactions();
  }, [loadTransactions]);

  const activeFilterCount = Object.entries(filters).filter(([key, value]) => {
    if (["page", "pageSize"].includes(key)) return false;
    if (key === "dateRange") return value !== null;
    return value !== "" && value !== null;
  }).length;

  const clearFilters = () => {
    setFilters({
      search: "",
      dateRange: null,
      startDate: "",
      endDate: "",
      startTime: "",
      endTime: "",
      status: null,
      vehicle: "",
      driver: "",
      commodity: "",
      supplier: "",
      transporter: "",
      customer: "",
      operator: "",
      origin: "",
      destination: "",
      weighbridge: "",
      weighMode: null,
      page: 1,
      pageSize: filters.pageSize,
    });
  };

  const openViewDrawer = async (record) => {
    let enriched = { ...record };
    if (record.operatorId) {
      try {
        const operator = await dispatch(fetchUserById(record.operatorId)).unwrap();
        enriched.operatorName = operator?.fullName || operator?.name || "N/A";
      } catch {
        enriched.operatorName = "Unknown Operator";
      }
    }
    setSelectedRecord(enriched);
    setReweighRecordsForDrawer([]);
    setIsDrawerOpen(true);
    // Fetch reweigh history (non-blocking)
    const ticketId = record.ticketID || record.id;
    if (ticketId) {
      dispatch(fetchReweighRecords(ticketId))
        .unwrap()
        .then((records) => setReweighRecordsForDrawer(Array.isArray(records) ? records : []))
        .catch(() => {});
    }
  };

  const handleSave = async (editedRecord) => {
    await dispatch(
      updateTransactionApi({
        ticketId: editedRecord.ticketID || editedRecord.id,
        data: {
          receiptNo: editedRecord.receiptNo,
          noPlate: editedRecord.noPlate,
          driverName: editedRecord.driverName,
          commodityName: editedRecord.commodityName,
          transporterName: editedRecord.transporterName,
          supplierName: editedRecord.supplierName,
          customerName: editedRecord.customerName,
          originName: editedRecord.originName,
          destinationName: editedRecord.destinationName,
          weighMode: editedRecord.weighMode,
          status: editedRecord.status,
          containerNo: editedRecord.containerNo,
          sealNo: editedRecord.sealNo,
          remarks: editedRecord.remarks,
          notes: editedRecord.notes,
        },
      })
    ).unwrap();
    message.success("Updated successfully!");
    setSelectedRecord(editedRecord);
    loadTransactions();
  };

  const columns = React.useMemo(
    () => getTransactionColumns({ filters, setReweighModal }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filters.page, filters.pageSize]
  );

  return (
    <div className="h-full flex flex-col bg-white">
      <PageHeader
        icon={Receipt}
        title="Transactions"
        subtitle={<><span className="font-semibold">{filteredTransactions.length}</span> of <span className="font-semibold">{total || 0}</span></>}
        actions={
          <>
            <Input
              allowClear
              placeholder="Search..."
              prefix={<Search size={10} className="text-gray-400" />}
              className="qt-filter-field w-52 h-7 text-[11px] rounded-md border-gray-300 shadow-sm"
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })}
            />

            <Button
              icon={<Filter size={14} />}
              className={`h-7 text-[11px] font-medium border shadow-none ${showFilters ? "" : "cs-ghost-btn"}`}
              style={
                showFilters
                  // Inverted chip (white fill + the bar's own dark color as text) instead of
                  // filling with --cs-icon-accent: for Navy, iconAccent (gold) happens to
                  // differ from appBarBg (navy) so a gold fill would've popped, but Indigo's
                  // iconAccent === its own primary === its own appBarBg, so that fill would
                  // vanish into the bar. White-on-dark-bar-color works for any dark app-bar,
                  // and (since text follows appBarBg) amber-on-white for Amber's bright bar too.
                  ? { backgroundColor: "#ffffff", borderColor: "#ffffff", color: "var(--cs-appbar-bg)" }
                  : undefined
              }
              onClick={() => setShowFilters(!showFilters)}
            >
              Filters
            </Button>

            {activeFilterCount > 0 && (
              <span className="h-7 text-[10px] font-bold px-3 rounded border shadow-sm flex items-center gap-1 cs-appbar-badge">
                <CheckCircle2 size={10} style={{ color: "var(--cs-icon-accent)" }} />
                {activeFilterCount} active
              </span>
            )}

            {activeFilterCount > 0 && (
              <Button
                size="small"
                icon={<X size={12} />}
                onClick={clearFilters}
                className="h-7 text-[10px] font-semibold shadow-sm rounded cs-ghost-btn"
              >
                Clear
              </Button>
            )}

            <div className="w-px h-5 mx-1 cs-appbar-divider" />

            <Button
              icon={<ReloadOutlined />}
              className="h-7 text-[10px] font-semibold cs-solid-chip-btn shadow-none"
              onClick={loadTransactions}
              loading={loading}
            >
              Refresh
            </Button>
          </>
        }
      />

      {/* Filters Panel — one consistent 5-column grid throughout so every
          field's edges line up across rows, and 15 fields fill exactly
          3 rows of 5 with no leftover space. */}
      {showFilters && (
        <div className="bg-gray-50 border-b border-gray-200 px-4 py-4 shrink-0">
          <div className="grid grid-cols-5 gap-3 mb-3">
            <div>
              <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Date Range
              </label>
              <RangePicker
                className="qt-filter-field w-full h-8 text-[11px] rounded-lg border-gray-300"
                value={filters.dateRange}
                onChange={(d) =>
                  setFilters({ ...filters, dateRange: d, startDate: "", endDate: "", page: 1 })
                }
                format="DD-MM-YY"
                placeholder={["Start", "End"]}
              />
            </div>
            <div>
              <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Start Time
              </label>
              <input
                type="time"
                value={filters.startTime}
                onChange={(e) => setFilters({ ...filters, startTime: e.target.value, page: 1 })}
                className="qt-filter-field w-full h-8 text-[11px] rounded-lg border border-gray-300 px-2"
              />
            </div>
            <div>
              <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> End Time
              </label>
              <input
                type="time"
                value={filters.endTime}
                onChange={(e) => setFilters({ ...filters, endTime: e.target.value, page: 1 })}
                className="qt-filter-field w-full h-8 text-[11px] rounded-lg border border-gray-300 px-2"
              />
            </div>
            <div>
              <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Status
              </label>
              <select
                className="qt-filter-field w-full h-8 text-[11px] rounded-lg border border-gray-300 px-2"
                value={filters.status || ""}
                onChange={(e) => setFilters({ ...filters, status: e.target.value || null, page: 1 })}
              >
                <option value="">All</option>
                <option value="completed">Completed</option>
                <option value="inprogress">In Progress</option>
              </select>
            </div>
            <div>
              <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                <span className="w-1 h-1 bg-amber-500 rounded-full" /> Vehicle
              </label>
              <Input
                placeholder="Vehicle..."
                className="qt-filter-field h-8 text-[11px] rounded-lg border-gray-300"
                value={filters.vehicle || ""}
                onChange={(e) => setFilters({ ...filters, vehicle: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>

          <div className="grid grid-cols-5 gap-3">
            {[
              ["driver", "Driver", "text"],
              ["commodity", "Commodity", "text"],
              ["supplier", "Supplier", "text"],
              ["transporter", "Transporter", "text"],
              ["customer", "Customer", "text"],
              ["operator", "Operator", "text"],
              ["origin", "Origin", "text"],
              ["destination", "Destination", "text"],
              ["weighbridge", "Weighbridge", "text"],
              ["weighMode", "Mode", "select"],
            ].map(([key, label, type]) => (
              <div key={key}>
                <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wide mb-1 flex items-center gap-1">
                  <span className="w-1 h-1 bg-amber-500 rounded-full" /> {label}
                </label>
                {type === "select" ? (
                  <select
                    className="qt-filter-field w-full h-8 text-[11px] rounded-lg border border-gray-300 px-2"
                    value={filters.weighMode || ""}
                    onChange={(e) =>
                      setFilters({ ...filters, weighMode: e.target.value || null, page: 1 })
                    }
                  >
                    <option value="">All</option>
                    <option value="single">Single</option>
                    <option value="double">Double</option>
                    <option value="auto">Auto</option>
                  </select>
                ) : (
                  <Input
                    placeholder={`${label}...`}
                    className="qt-filter-field h-8 text-[11px] rounded-lg border-gray-300"
                    value={filters[key] || ""}
                    onChange={(e) => setFilters({ ...filters, [key]: e.target.value, page: 1 })}
                    allowClear
                  />
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Table */}
      <div className="flex-1 overflow-hidden">
        <div className="h-full bg-white overflow-hidden">
          <Table
            columns={columns}
            dataSource={filteredTransactions}
            rowKey="id"
            loading={loading}
            size="small"
            className="compact-table"
            scroll={{ y: "calc(100vh - 120px)", x: "max-content" }}
            pagination={{
              current: filters.page,
              pageSize: filters.pageSize,
              total: total || 0,
              showSizeChanger: true,
              showTotal: (total) => `${total} records`,
              size: "small",
              pageSizeOptions: ["10", "20", "50", "100"],
              onChange: (p, ps) => setFilters({ ...filters, page: p, pageSize: ps }),
            }}
            rowClassName={(record) => {
              const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
              const isCompleted =
                hasSecondWeight || record.status === "Completed" || record.status === "completed";
              return `${isCompleted ? "completed-row" : "incomplete-row"} cursor-pointer`;
            }}
            onRow={(record) => ({
              onClick: () => openViewDrawer(record),
            })}
          />
        </div>
      </div>

      {/* Drawer */}
      <TransactionDrawer
        open={isDrawerOpen}
        onClose={() => setIsDrawerOpen(false)}
        record={selectedRecord}
        isCompleted={
          (selectedRecord?.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) ||
          selectedRecord?.status === "Completed" ||
          selectedRecord?.status === "completed"
        }
        pdfTheme={pdfTheme}
        onPdfThemeChange={setPdfTheme}
        ticketSettings={ticketSettings}
        reweighRecords={reweighRecordsForDrawer}
        formatTurnaroundTime={formatTurnaroundTimeSimple}
        onSave={handleSave}
        onExportPDF={() => setIsExportPreviewOpen(true)}
      />

      {/* Export Preview Modal */}
      <ExportPreviewModal
        open={isExportPreviewOpen}
        onClose={() => setIsExportPreviewOpen(false)}
        record={selectedRecord}
        pdfTheme={pdfTheme}
        onPdfThemeChange={setPdfTheme}
        ticketSettings={ticketSettings}
        reweighRecords={reweighRecordsForDrawer}
        formatTurnaroundTime={formatTurnaroundTimeSimple}
      />

      {/* ── Reweigh wizard modal ──────────────────────────────────────────── */}
      <ReweighModal
        visible={reweighModal.visible}
        transaction={reweighModal.transaction}
        onClose={() => setReweighModal({ visible: false, transaction: null })}
        onSuccess={() => dispatch(fetchTransactions({}))}
        onApproved={(approvedTx, reweighType) => {
          setReweighModal({ visible: false, transaction: null });
          if (reweighType === "secondWeight") {
            setWeighingModal({ visible: true, transaction: approvedTx });
          } else {
            // First weight or all — capture new first weight, then second weight
            setFirstWeightModal({ visible: true, transaction: approvedTx });
          }
        }}
      />

      {/* ── Re-enter first weight (first-weight reweigh approval) ──────── */}
      <ReweighFirstWeightModal
        visible={firstWeightModal.visible}
        transaction={firstWeightModal.transaction}
        onClose={() => setFirstWeightModal({ visible: false, transaction: null })}
        onSuccess={(updatedTx) => {
          setFirstWeightModal({ visible: false, transaction: null });
          setWeighingModal({ visible: true, transaction: updatedTx });
        }}
      />

      {/* ── Add second weight modal (opened after reweigh approval) ─────── */}
      <AddWeighingModal
        visible={weighingModal.visible}
        transaction={weighingModal.transaction}
        onClose={() => setWeighingModal({ visible: false, transaction: null })}
        onSuccess={() => {
          setWeighingModal({ visible: false, transaction: null });
          dispatch(fetchTransactions({}));
        }}
      />

      <style>{`
        .compact-table .ant-table { font-size: 10px; }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, var(--cs-50), var(--cs-100)) !important;
          border-bottom: 1.5px solid var(--cs-500) !important;
          padding: 5px 8px !important;
          font-weight: 700 !important;
          font-size: 9px !important;
          color: var(--cs-900) !important;
          text-transform: uppercase;
          letter-spacing: 0.3px;
          line-height: 1.2;
        }
        .compact-table .ant-table-tbody > tr > td {
          padding: 2px 6px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          transition: all 0.12s ease;
          line-height: 1.15;
        }
        .compact-table .ant-table-tbody > tr.completed-row > td {
          background: rgba(236, 253, 245, 0.4) !important;
        }
        .compact-table .ant-table-tbody > tr.completed-row:hover > td {
          background: rgba(236, 253, 245, 0.8) !important;
        }
        .compact-table .ant-table-tbody > tr.incomplete-row > td {
          background: white !important;
        }
        .compact-table .ant-table-tbody > tr.incomplete-row:hover > td {
          background: var(--cs-50) !important;
        }
        .compact-table .ant-pagination {
          margin: 6px 0 !important;
          padding: 0 8px !important;
        }
        .compact-table .ant-pagination-item,
        .compact-table .ant-pagination-prev,
        .compact-table .ant-pagination-next {
          min-width: 24px !important;
          height: 24px !important;
          line-height: 22px !important;
          font-size: 11px !important;
          border-radius: 4px !important;
          margin: 0 2px !important;
        }
        .compact-table .ant-pagination-item-active {
          background: linear-gradient(135deg, var(--cs-500), var(--cs-600)) !important;
          border-color: var(--cs-500) !important;
        }
        .compact-table .ant-pagination-item-active a {
          color: white !important;
          font-weight: 700 !important;
        }
        .compact-table .ant-select-selector {
          height: 24px !important;
          padding: 0 8px !important;
        }
        .compact-table .ant-select-selection-item {
          line-height: 22px !important;
          font-size: 11px !important;
        }
        .compact-table .ant-pagination-options { margin-left: 8px !important; }
        .compact-table .ant-pagination-total-text {
          font-size: 11px !important;
          line-height: 24px !important;
        }
      `}</style>
    </div>
  );
}