import { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import ReportsTable from "./ReportsTable";
import DriverReport from "./reportFiles/DriverReport";
import CustomerReport from "./reportFiles/CustomerReport";
import CommodityReport from "./reportFiles/CommodityReport";
import SupplierReport from "./reportFiles/SupplierReport";

import { fetchTransactions } from "../../store/weighingSlice";
import { RotateCcw, FileDown, FileSpreadsheet } from "lucide-react";
import dayjs from "dayjs";

import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";

export default function Reports() {
  const dispatch = useDispatch();
  const { transactions, loading } = useSelector((state) => state.weighing);

  const REPORT_TABS = ["transactions", "drivers", "customers", "commodities", "suppliers"];
  const [activeTab, setActiveTab] = useState("transactions");

  // Pagination (TABLE ONLY)
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(3);

  // Filters
  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    startTime: "",
    endTime: "",
    status: "",
    search: "",
  });

  // Export preview
  const [showExportPreview, setShowExportPreview] = useState(false);
  const [exportType, setExportType] = useState(null); // "pdf" | "excel"

  useEffect(() => {
    dispatch(fetchTransactions({ pageSize: 10000 }));
  }, [dispatch]);

  useEffect(() => {
    setCurrentPage(1);
  }, [filters, activeTab]);

  // =========================
  // Turnaround Calculation
  // =========================
  const calculateTurnaroundTime = (firstWeightTime, secondWeightTime) => {
    if (!firstWeightTime || !secondWeightTime) return "N/A";
    
    const first = dayjs(firstWeightTime);
    const second = dayjs(secondWeightTime);
    const diffMinutes = second.diff(first, 'minute');
    
    if (diffMinutes < 1) return '< 1m';
    if (diffMinutes < 60) return `${diffMinutes}m`;
    
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  };

  // =========================
  // FILTERED DATA (SINGLE SOURCE OF TRUTH)
  // =========================
  const filteredTransactions = useMemo(() => {
    let data = [...transactions];

    if (filters.startDate) {
      const start = new Date(`${filters.startDate}T${filters.startTime || "00:00"}`);
      data = data.filter((t) => new Date(t.createdAt) >= start);
    }

    if (filters.endDate) {
      const end = new Date(`${filters.endDate}T${filters.endTime || "23:59"}`);
      data = data.filter((t) => new Date(t.createdAt) <= end);
    }

    if (filters.status) {
      if (filters.status.toLowerCase() === 'completed') {
        data = data.filter((t) => 
          (t.secondWeight && parseFloat(t.secondWeight) > 0) || 
          t.status === 'Completed' || 
          t.status === 'completed'
        );
      } else if (filters.status.toLowerCase() === 'in progress') {
        data = data.filter((t) => 
          (!t.secondWeight || parseFloat(t.secondWeight) === 0) && 
          t.status !== 'Completed' && 
          t.status !== 'completed'
        );
      }
    }

    if (filters.search) {
      const q = filters.search.toLowerCase();
      data = data.filter(
        (t) =>
          t.receiptNo?.toLowerCase().includes(q) ||
          t.noPlate?.toLowerCase().includes(q) ||
          t.driverName?.toLowerCase().includes(q) ||
          t.commodityName?.toLowerCase().includes(q) ||
          t.supplierName?.toLowerCase().includes(q) ||
          t.transporterName?.toLowerCase().includes(q) ||
          t.customerName?.toLowerCase().includes(q)
      );
    }

    return data;
  }, [transactions, filters]);

  const totalRecords = filteredTransactions.length;

  const totals = useMemo(() => {
    return filteredTransactions.reduce(
      (acc, t) => {
        acc.count += 1;
        acc.net += Number(t.netWeight || 0);
        acc.first += Number(t.firstWeight || 0);
        acc.second += Number(t.secondWeight || 0);
        return acc;
      },
      { count: 0, net: 0, first: 0, second: 0 }
    );
  }, [filteredTransactions]);

  // =========================
  // EXPORTS (USE FULL FILTERED DATA)
  // =========================
  const exportPDF = () => {
    const doc = new jsPDF("landscape", "mm", "a4");
    const pageWidth = doc.internal.pageSize.getWidth();

    // Header
    doc.setFillColor(245, 158, 11); // Amber-500
    doc.rect(0, 0, pageWidth, 20, "F");
    
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(16);
    doc.setFont("helvetica", "bold");
    doc.text("TRANSACTIONS REPORT", pageWidth / 2, 10, { align: "center" });
    
    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, pageWidth / 2, 15, { align: "center" });

    // Summary
    doc.setTextColor(0, 0, 0);
    doc.setFontSize(9);
    doc.text(`Total Records: ${filteredTransactions.length}`, 14, 25);
    doc.text(`Total Net Weight: ${totals.net.toLocaleString()} kg`, 14, 30);

    // Table
    autoTable(doc, {
      startY: 35,
      head: [[
        "Date & Time",
        "Receipt",
        "Vehicle",
        "Driver",
        "Commodity",
        "Supplier",
        "Transporter",
        "Customer",
        "Origin",
        "Destination",
        "Weighbridge",
        "Scale",
        "Mode",
        "Operation",
        "Operator",
        "First Wt (kg)",
        "Second Wt (kg)",
        "Net Wt (kg)",
        "TAT",
        "Status",
      ]],
      body: filteredTransactions.map((t) => {
        const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
        const isCompleted = hasSecondWeight || t.status === 'Completed' || t.status === 'completed';
        
        return [
          dayjs(t.createdAt).format("DD MMM YY HH:mm"),
          t.receiptNo || '-',
          t.noPlate || '-',
          t.driverName || '-',
          t.commodityName || '-',
          t.supplierName || '-',
          t.transporterName || '-',
          t.customerName || '-',
          t.originName || '-',
          t.destinationName || '-',
          t.weighBridgeName || '-',
          t.scaleName || '-',
          t.weighMode || '-',
          t.operation || '-',
          t.operatorName || t.firstWeightOperator || '-',
          t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : '-',
          t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : '-',
          t.netWeight ? parseFloat(t.netWeight).toLocaleString() : '-',
          calculateTurnaroundTime(t.firstWeightTime, t.secondWeightTime),
          isCompleted ? 'COMPLETED' : 'IN PROGRESS',
        ];
      }),
      styles: { 
        fontSize: 7,
        cellPadding: 1.5,
      },
      headStyles: { 
        fillColor: [245, 158, 11],
        textColor: [0, 0, 0],
        fontStyle: 'bold',
        fontSize: 7,
      },
      alternateRowStyles: {
        fillColor: [250, 250, 250]
      },
      columnStyles: {
        15: { halign: 'right' },
        16: { halign: 'right' },
        17: { halign: 'right' },
      }
    });

    doc.save(`transaction-report-${dayjs().format('YYYY-MM-DD')}.pdf`);
  };

  const exportExcel = () => {
    const ws = XLSX.utils.json_to_sheet(
      filteredTransactions.map((t) => {
        const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
        const isCompleted = hasSecondWeight || t.status === 'Completed' || t.status === 'completed';
        
        return {
          'Date': dayjs(t.createdAt).format('DD MMM YYYY'),
          'Time': dayjs(t.createdAt).format('HH:mm:ss'),
          'Receipt No': t.receiptNo || '-',
          'Vehicle Registration': t.noPlate || '-',
          'Driver Name': t.driverName || '-',
          'Commodity': t.commodityName || '-',
          'Supplier': t.supplierName || '-',
          'Transporter': t.transporterName || '-',
          'Customer': t.customerName || '-',
          'Origin': t.originName || '-',
          'Destination': t.destinationName || '-',
          'Weighbridge': t.weighBridgeName || '-',
          'Scale': t.scaleName || '-',
          'Weigh Mode': t.weighMode || '-',
          'Operation': t.operation || '-',
          'Operator': t.operatorName || t.firstWeightOperator || '-',
          'Axle Type': t.axleType || '-',
          'Container No': t.containerNo || '-',
          'Seal No': t.sealNo || '-',
          'First Weight (kg)': t.firstWeight ? parseFloat(t.firstWeight) : 0,
          'Second Weight (kg)': t.secondWeight ? parseFloat(t.secondWeight) : 0,
          'Net Weight (kg)': t.netWeight ? parseFloat(t.netWeight) : 0,
          'First Weight Time': t.firstWeightTime ? dayjs(t.firstWeightTime).format('DD MMM YYYY HH:mm:ss') : '-',
          'Second Weight Time': t.secondWeightTime ? dayjs(t.secondWeightTime).format('DD MMM YYYY HH:mm:ss') : '-',
          'Turnaround Time': calculateTurnaroundTime(t.firstWeightTime, t.secondWeightTime),
          'Status': isCompleted ? 'COMPLETED' : 'IN PROGRESS',
          'Remarks': t.remarks || t.notes || '-',
        };
      })
    );

    // Auto-size columns
    const colWidths = [];
    const header = Object.keys(filteredTransactions[0] || {});
    header.forEach((_, i) => {
      colWidths.push({ wch: 15 });
    });
    ws['!cols'] = colWidths;

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Transactions");
    XLSX.writeFile(wb, `transaction-report-${dayjs().format('YYYY-MM-DD')}.xlsx`);
  };

  const clearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      startTime: "",
      endTime: "",
      status: "",
      search: "",
    });
    setCurrentPage(1);
  };

  const renderActiveReport = () => {
    if (activeTab !== "transactions") {
      const ComponentMap = {
        drivers: DriverReport,
        customers: CustomerReport,
        commodities: CommodityReport,
        suppliers: SupplierReport,
      };
      const Component = ComponentMap[activeTab];
      return <Component transactions={filteredTransactions} loading={loading} />;
    }

    return (
      <>
        <div className="mb-4 grid grid-cols-2 sm:grid-cols-4 gap-3">
          <div className="bg-amber-50 rounded-lg p-3 border border-amber-200 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-800">Total Transactions</div>
            <div className="text-xl font-bold mt-1 text-amber-900">{totals.count.toLocaleString()}</div>
          </div>
          <div className="bg-amber-100 rounded-lg p-3 border border-amber-300 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-900">Total Net Weight</div>
            <div className="text-xl font-bold mt-1 text-amber-950">{totals.net.toLocaleString()}</div>
            <div className="text-[10px] text-amber-800">kg</div>
          </div>
          <div className="bg-amber-50 rounded-lg p-3 border border-amber-200 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-800">First Weight Total</div>
            <div className="text-xl font-bold mt-1 text-amber-900">{totals.first.toLocaleString()}</div>
            <div className="text-[10px] text-amber-700">kg</div>
          </div>
          <div className="bg-amber-100 rounded-lg p-3 border border-amber-300 shadow-sm">
            <div className="text-[10px] font-semibold uppercase tracking-wide text-amber-900">Second Weight Total</div>
            <div className="text-xl font-bold mt-1 text-amber-950">{totals.second.toLocaleString()}</div>
            <div className="text-[10px] text-amber-800">kg</div>
          </div>
        </div>

        <ReportsTable
          transactions={filteredTransactions}
          loading={loading}
          currentPage={currentPage}
          pageSize={pageSize}
          totalRecords={totalRecords}
          onPageChange={setCurrentPage}
          onPageSizeChange={setPageSize}
        />
      </>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 p-4 sm:p-6">
      {/* Header */}
      <div className="mb-4">
        <div className="flex items-center gap-2 mb-2">
          <div className="w-10 h-10 rounded-lg bg-amber-100 flex items-center justify-center border border-amber-200">
            <svg className="w-6 h-6 text-amber-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">REPORTS</h1>
            <p className="text-xs text-gray-600">Operational and analytical system reports</p>
          </div>
        </div>
      </div>

      {/* TABS */}
      <div className="flex gap-2 mb-4 flex-wrap">
        {REPORT_TABS.map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded-lg text-sm font-medium capitalize transition-all ${
              activeTab === tab 
                ? "bg-amber-100 text-amber-900 border border-amber-300" 
                : "bg-white text-gray-700 hover:bg-amber-50 border border-gray-200"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* FILTERS */}
      {activeTab === "transactions" && (
        <div className="bg-white border border-gray-200 rounded-lg p-4 mb-4 shadow-sm">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-3 mb-3">
            {/* Start Date */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Start Date</label>
              <input
                type="date"
                value={filters.startDate}
                onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
              />
            </div>

            {/* Start Time */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Start Time</label>
              <input
                type="time"
                value={filters.startTime}
                onChange={(e) => setFilters({ ...filters, startTime: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
              />
            </div>

            {/* End Date */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">End Date</label>
              <input
                type="date"
                value={filters.endDate}
                onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
              />
            </div>

            {/* End Time */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">End Time</label>
              <input
                type="time"
                value={filters.endTime}
                onChange={(e) => setFilters({ ...filters, endTime: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
              />
            </div>

            {/* Status */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Status</label>
              <select
                value={filters.status}
                onChange={(e) => setFilters({ ...filters, status: e.target.value })}
                className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300 bg-white"
              >
                <option value="">All Status</option>
                <option value="Completed">Completed</option>
                <option value="In Progress">In Progress</option>
              </select>
            </div>
          </div>

          {/* Search */}
          <div className="space-y-1 mb-3">
            <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wider">Search</label>
            <input
              type="text"
              placeholder="Search by receipt, vehicle, driver, commodity, supplier..."
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value })}
              className="w-full border border-gray-300 px-2 py-1.5 rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-amber-300 focus:border-amber-300"
            />
          </div>

          {/* Action Buttons */}
          <div className="flex flex-wrap gap-2">
            <button 
              onClick={clearFilters} 
              className="flex items-center gap-1.5 px-3 py-1.5 border border-gray-300 rounded-lg text-xs font-medium hover:bg-gray-50 transition-colors"
            >
              <RotateCcw size={14} />
              Clear Filters
            </button>

            <div className="ml-auto flex gap-2">
              <button
                onClick={() => {
                  setExportType("pdf");
                  setShowExportPreview(true);
                }}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200 transition-all"
              >
                <FileDown size={14} />
                Preview PDF
              </button>

              <button
                onClick={() => {
                  setExportType("excel");
                  setShowExportPreview(true);
                }}
                className="flex items-center gap-1.5 px-3 py-1.5 border border-gray-300 bg-white rounded-lg text-xs font-medium hover:bg-gray-50 transition-colors"
              >
                <FileSpreadsheet size={14} />
                Preview Excel
              </button>
            </div>
          </div>
        </div>
      )}

      {renderActiveReport()}

      {/* =========================
          EXPORT PREVIEW MODAL
          ========================= */}
      {showExportPreview && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg w-full max-w-7xl max-h-[90vh] flex flex-col shadow-2xl">
            
            {/* Modal Header */}
            <div className="p-4 border-b bg-amber-50">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-900">
                    Export Preview
                  </h2>
                  <p className="text-xs text-gray-600 mt-1">
                    Showing <span className="font-semibold text-amber-700">{filteredTransactions.length}</span> filtered records
                  </p>
                </div>
                <button
                  onClick={() => setShowExportPreview(false)}
                  className="text-gray-400 hover:text-gray-600 transition-colors"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            {/* Modal Content - Scrollable Table */}
            <div className="flex-1 overflow-auto p-4">
              <div className="border rounded-lg overflow-auto">
                <table className="w-full text-xs min-w-[2000px]">
                  <thead className="bg-amber-50 sticky top-0">
                    <tr>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Date</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Time</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Receipt</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Vehicle</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Driver</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Commodity</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Supplier</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Transporter</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Customer</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Origin</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Destination</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Weighbridge</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Operator</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">First Wt</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">Second Wt</th>
                      <th className="p-2 text-right font-semibold border-b border-amber-200">Net Wt</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">TAT</th>
                      <th className="p-2 text-left font-semibold border-b border-amber-200">Status</th>
                    </tr>
                  </thead>

                  <tbody>
                    {filteredTransactions.map((t, idx) => {
                      const hasSecondWeight = t.secondWeight && parseFloat(t.secondWeight) > 0;
                      const isCompleted = hasSecondWeight || t.status === 'Completed' || t.status === 'completed';
                      
                      return (
                        <tr key={t.id} className={`border-t border-gray-100 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}>
                          <td className="p-2">{dayjs(t.createdAt).format('DD MMM YYYY')}</td>
                          <td className="p-2">{dayjs(t.createdAt).format('HH:mm:ss')}</td>
                          <td className="p-2 font-mono">{t.receiptNo || '-'}</td>
                          <td className="p-2 font-semibold">{t.noPlate || '-'}</td>
                          <td className="p-2">{t.driverName || '-'}</td>
                          <td className="p-2">{t.commodityName || '-'}</td>
                          <td className="p-2">{t.supplierName || '-'}</td>
                          <td className="p-2">{t.transporterName || '-'}</td>
                          <td className="p-2">{t.customerName || '-'}</td>
                          <td className="p-2">{t.originName || '-'}</td>
                          <td className="p-2">{t.destinationName || '-'}</td>
                          <td className="p-2">{t.weighBridgeName || '-'}</td>
                          <td className="p-2">{t.operatorName || t.firstWeightOperator || '-'}</td>
                          <td className="p-2 text-right font-semibold text-amber-700">
                            {t.firstWeight ? parseFloat(t.firstWeight).toLocaleString() : '-'}
                          </td>
                          <td className="p-2 text-right font-semibold text-amber-800">
                            {t.secondWeight ? parseFloat(t.secondWeight).toLocaleString() : '-'}
                          </td>
                          <td className="p-2 text-right font-bold text-amber-900">
                            {t.netWeight ? parseFloat(t.netWeight).toLocaleString() : '-'}
                          </td>
                          <td className="p-2 font-semibold">
                            {calculateTurnaroundTime(t.firstWeightTime, t.secondWeightTime)}
                          </td>
                          <td className="p-2">
                            <span className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                              isCompleted 
                                ? 'bg-green-100 text-green-800 border border-green-300' 
                                : 'bg-amber-50 text-amber-800 border border-amber-200'
                            }`}>
                              {isCompleted ? 'COMPLETED' : 'IN PROGRESS'}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="p-4 border-t bg-amber-50/50 flex justify-end gap-2">
              <button
                onClick={() => setShowExportPreview(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-xs font-medium hover:bg-white transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={() => {
                  exportType === "pdf" ? exportPDF() : exportExcel();
                  setShowExportPreview(false);
                }}
                className="px-4 py-2 bg-amber-100 text-amber-900 border border-amber-300 rounded-lg text-xs font-medium hover:bg-amber-200 transition-all"
              >
                Download {exportType?.toUpperCase()}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}