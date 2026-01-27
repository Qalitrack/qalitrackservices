import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, Space, Modal, message, Radio, Typography, Drawer } from "antd";
import { ReloadOutlined, SearchOutlined, EditOutlined, SaveOutlined, CloseOutlined, ClockCircleOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import duration from "dayjs/plugin/duration";
import { fetchTransactions, fetchUserById, updateTransactionApi } from "../store/weighingSlice";
import { Printer, Eye, Search, Filter } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";

dayjs.extend(relativeTime);
dayjs.extend(duration);

const { RangePicker } = DatePicker;
const { Text } = Typography;

export default function Transactions() {
  const dispatch = useDispatch();
  const { transactions, loading, total } = useSelector((state) => state.weighing);
  const [selectedRecord, setSelectedRecord] = useState(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [printMode, setPrintMode] = useState('color');
  const [isEditing, setIsEditing] = useState(false);
  const [editedRecord, setEditedRecord] = useState(null);
  const [saving, setSaving] = useState(false);
  const [filters, setFilters] = useState({ 
    search: "", 
    dateRange: null, 
    status: null, 
    vehicle: '',
    driver: '',
    commodity: '',
    supplier: '',
    transporter: '',
    customer: '',
    operator: '',
    origin: '',
    destination: '',
    weighbridge: '',
    weighMode: null,
    page: 1, 
    pageSize: 15 
  });
  const [showFilters, setShowFilters] = useState(false);

  const loadTransactions = useCallback(() => {
    const params = {
      pageNumber: filters.page,
      pageSize: filters.pageSize,
    };
    
    if (filters.search) {
      params.search = filters.search;
    }
    
    if (filters.dateRange && filters.dateRange[0] && filters.dateRange[1]) {
      params.startDate = filters.dateRange[0].format("YYYY-MM-DD");
      params.endDate = filters.dateRange[1].format("YYYY-MM-DD");
    }
    
    if (filters.status) {
      params.status = filters.status;
    }
    
    dispatch(fetchTransactions(params));
  }, [dispatch, filters.page, filters.pageSize, filters.search, filters.dateRange, filters.status]);

  // Client-side filtering for additional fields
  const filteredTransactions = React.useMemo(() => {
    let filtered = transactions || [];
    
    // Apply status filter based on second weight
    if (filters.status) {
      if (filters.status === 'completed') {
        filtered = filtered.filter(t => 
          (t.secondWeight && parseFloat(t.secondWeight) > 0) || 
          t.status === 'Completed' || 
          t.status === 'completed'
        );
      } else if (filters.status === 'inprogress') {
        filtered = filtered.filter(t => 
          (!t.secondWeight || parseFloat(t.secondWeight) === 0) && 
          t.status !== 'Completed' && 
          t.status !== 'completed'
        );
      }
    }
    
    // Apply client-side filters
    if (filters.vehicle) {
      const vehicleLower = filters.vehicle.toLowerCase();
      filtered = filtered.filter(t => t.noPlate?.toLowerCase().includes(vehicleLower));
    }
    
    if (filters.driver) {
      const driverLower = filters.driver.toLowerCase();
      filtered = filtered.filter(t => t.driverName?.toLowerCase().includes(driverLower));
    }
    
    if (filters.commodity) {
      const commodityLower = filters.commodity.toLowerCase();
      filtered = filtered.filter(t => t.commodityName?.toLowerCase().includes(commodityLower));
    }
    
    if (filters.supplier) {
      const supplierLower = filters.supplier.toLowerCase();
      filtered = filtered.filter(t => t.supplierName?.toLowerCase().includes(supplierLower));
    }
    
    if (filters.transporter) {
      const transporterLower = filters.transporter.toLowerCase();
      filtered = filtered.filter(t => t.transporterName?.toLowerCase().includes(transporterLower));
    }
    
    if (filters.customer) {
      const customerLower = filters.customer.toLowerCase();
      filtered = filtered.filter(t => t.customerName?.toLowerCase().includes(customerLower));
    }
    
    if (filters.operator) {
      const operatorLower = filters.operator.toLowerCase();
      filtered = filtered.filter(t => 
        t.operatorName?.toLowerCase().includes(operatorLower) ||
        t.firstWeightOperator?.toLowerCase().includes(operatorLower)
      );
    }
    
    if (filters.origin) {
      const originLower = filters.origin.toLowerCase();
      filtered = filtered.filter(t => t.originName?.toLowerCase().includes(originLower));
    }
    
    if (filters.destination) {
      const destinationLower = filters.destination.toLowerCase();
      filtered = filtered.filter(t => t.destinationName?.toLowerCase().includes(destinationLower));
    }
    
    if (filters.weighbridge) {
      const weighbridgeLower = filters.weighbridge.toLowerCase();
      filtered = filtered.filter(t => t.weighBridgeName?.toLowerCase().includes(weighbridgeLower));
    }
    
    if (filters.weighMode) {
      filtered = filtered.filter(t => t.weighMode?.toLowerCase() === filters.weighMode.toLowerCase());
    }
    
    return filtered;
  }, [transactions, filters]);

  useEffect(() => { 
    const timeoutId = setTimeout(() => {
      loadTransactions();
    }, filters.search ? 500 : 0); // Debounce search input
    
    return () => clearTimeout(timeoutId);
  }, [loadTransactions]);

  const openViewDrawer = async (record) => {
    let enriched = { ...record };
    if (record.operatorId) {
      try {
        const operator = await dispatch(fetchUserById(record.operatorId)).unwrap();
        enriched.operatorName = operator?.fullName || operator?.name || "N/A";
      } catch (err) {
        enriched.operatorName = "Unknown Operator";
      }
    }
    setSelectedRecord(enriched);
    setEditedRecord(enriched);
    setIsEditing(false);
    setIsDrawerOpen(true);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      await dispatch(updateTransactionApi({
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
          axleType: editedRecord.axleType,
          containerNo: editedRecord.containerNo,
          sealNo: editedRecord.sealNo,
          remarks: editedRecord.remarks,
          notes: editedRecord.notes,
        }
      })).unwrap();
      message.success('Updated successfully!');
      setSelectedRecord(editedRecord);
      setIsEditing(false);
      loadTransactions();
    } catch (error) {
      message.error('Failed to update: ' + (error.message || 'Unknown error'));
    } finally {
      setSaving(false);
    }
  };

  // ✅ NEW: Calculate turnaround time between first and second weight (for completed transactions)
  const calculateTurnaroundTime = (firstWeightTime, secondWeightTime) => {
    // Debug logging
    console.log("Calculating turnaround time:", { firstWeightTime, secondWeightTime });
    
    if (!firstWeightTime || !secondWeightTime) {
      console.log("Missing time data - returning N/A");
      return { display: "N/A", minutes: 0 };
    }
    
    const first = dayjs(firstWeightTime);
    const second = dayjs(secondWeightTime);
    const diffMinutes = second.diff(first, 'minute');
    
    console.log("Time difference in minutes:", diffMinutes);
    
    if (diffMinutes < 1) {
      return { display: '< 1m', minutes: 0 };
    } else if (diffMinutes < 60) {
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    } else {
      const hours = Math.floor(diffMinutes / 60);
      const mins = diffMinutes % 60;
      return { 
        display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, 
        minutes: diffMinutes 
      };
    }
  };

  // ✅ NEW: Calculate wait time from creation (for incomplete transactions)
  const calculateWaitTime = (createdAt) => {
    if (!createdAt) return { display: '-', minutes: 0 };
    
    const now = dayjs();
    const created = dayjs(createdAt);
    const diffMinutes = now.diff(created, 'minute');
    
    if (diffMinutes < 1) {
      return { display: '< 1m', minutes: 0 };
    } else if (diffMinutes < 60) {
      return { display: `${diffMinutes}m`, minutes: diffMinutes };
    } else {
      const hours = Math.floor(diffMinutes / 60);
      const mins = diffMinutes % 60;
      return { 
        display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, 
        minutes: diffMinutes 
      };
    }
  };

  // ✅ NEW: Get color based on turnaround/wait time
  const getTimeColor = (minutes, isCompleted) => {
    if (isCompleted) {
      // For completed transactions (turnaround time)
      if (minutes < 30) return 'green';      // < 30 min: excellent
      if (minutes < 60) return 'blue';       // 30-60 min: good
      if (minutes < 120) return 'orange';    // 60-120 min: acceptable
      return 'red';                          // > 120 min: slow
    } else {
      // For incomplete transactions (wait time)
      if (minutes < 30) return 'green';      // < 30 min: good
      if (minutes < 60) return 'orange';     // 30-60 min: warning
      return 'red';                          // > 60 min: critical
    }
  };

  // Original calculateTurnaroundTime for display (keeping for backwards compatibility)
  const formatTurnaroundTimeSimple = (first, second) => {
    if (!first || !second) return "N/A";
    const diffMin = dayjs(second).diff(dayjs(first), 'minute');
    return diffMin < 60 ? `${diffMin}m` : `${Math.floor(diffMin / 60)}h ${diffMin % 60}m`;
  };

  const generatePDF = (record, isColor) => {
    const doc = new jsPDF();
    const colors = isColor ? 
      { header: [255, 152, 0], text: [33, 33, 33], light: [245, 245, 245], accent: [254, 243, 199] } :
      { header: [240, 240, 240], text: [0, 0, 0], light: [240, 240, 240], accent: [220, 220, 220] };

    doc.setFontSize(14);
    doc.setFont("helvetica", "bold");
    doc.text("QALIBRATED SYSTEMS LTD", 105, 15, { align: "center" });
    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text("PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996", 105, 20, { align: "center" });
    doc.setFontSize(16);
    doc.text("WEIGHING TICKET", 105, 35, { align: "center" });

    let y = 42;
    doc.setFillColor(...colors.header);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
    doc.text("TICKET DETAILS", 105, y + 5, { align: "center" });

    y += 7;
    autoTable(doc, {
      startY: y,
      theme: "plain",
      styles: { fontSize: 8.5, cellPadding: 1.5 },
      columnStyles: { 0: { fontStyle: "bold", cellWidth: 32 }, 1: { cellWidth: 58 }, 2: { fontStyle: "bold", cellWidth: 32 }, 3: { cellWidth: 58 } },
      body: [
        ["TICKET NO", `: ${record.receiptNo || 'N/A'}`, "REGISTRATION", `: ${record.noPlate || 'N/A'}`],
        ["AXLE TYPE", `: ${record.axleType || "N/A"}`, "COMMODITY", `: ${record.commodityName || 'N/A'}`],
        ["TRANSPORTER", `: ${record.transporterName || 'N/A'}`, "DRIVER", `: ${record.driverName || "N/A"}`],
        ["SUPPLIER", `: ${record.supplierName || "N/A"}`, "CUSTOMER", `: ${record.customerName || "N/A"}`],
        ["SOURCE", `: ${record.originName || "N/A"}`, "DESTINATION", `: ${record.destinationName || "N/A"}`],
        ["CONTAINER", `: ${record.containerNo || "N/A"}`, "SEAL NO", `: ${record.sealNo || "N/A"}`],
        ["WEIGH MODE", `: ${record.weighMode || "N/A"}`, "OPERATION", `: ${record.operation || "N/A"}`],
      ],
    });

    y = doc.lastAutoTable.finalY + 5;
    doc.setFillColor(...colors.header);
    doc.rect(14, y, 182, 7, "F");
    doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
    doc.text("WEIGHT SUMMARY", 105, y + 5, { align: "center" });

    y += 7;
    autoTable(doc, {
      startY: y,
      theme: "grid",
      headStyles: { fillColor: colors.header, halign: "center" },
      styles: { halign: "center", fontSize: 9 },
      head: [["MEASUREMENT", "WEIGHT", "OPERATOR", "TIMESTAMP"]],
      body: [
        ["FIRST WEIGHT", `${record.firstWeight || 0} Kg`, record.firstWeightOperator || record.operatorName || "N/A", record.firstWeightTime ? dayjs(record.firstWeightTime).format("DD-MM-YY HH:mm") : "N/A"],
        ["SECOND WEIGHT", `${record.secondWeight || 0} Kg`, record.secondWeightOperator || record.operatorName || "N/A", record.secondWeightTime ? dayjs(record.secondWeightTime).format("DD-MM-YY HH:mm") : "N/A"],
        [{ content: "NET WEIGHT", styles: { fillColor: colors.light, fontStyle: "bold" } }, { content: `${record.netWeight || 0} Kg`, styles: { fillColor: colors.light, fontStyle: "bold" } }, "", ""],
        [{ content: "TURNAROUND", styles: { fillColor: colors.accent } }, { content: formatTurnaroundTimeSimple(record.firstWeightTime, record.secondWeightTime), colSpan: 3, styles: { fillColor: colors.accent } }],
      ],
    });

    // Add remarks/notes section
    if (record.remarks || record.notes) {
      y = doc.lastAutoTable.finalY + 5;
      doc.setFillColor(...colors.header);
      doc.rect(14, y, 182, 7, "F");
      doc.setTextColor(isColor ? 255 : 0, isColor ? 255 : 0, isColor ? 255 : 0);
      doc.text("REMARKS/NOTES", 105, y + 5, { align: "center" });
      
      y += 7;
      doc.setTextColor(0, 0, 0);
      doc.setFontSize(9);
      const remarkText = record.remarks || record.notes || 'N/A';
      const splitRemarks = doc.splitTextToSize(remarkText, 170);
      doc.text(splitRemarks, 14, y + 3);
    }

    doc.save(`Ticket_${record.receiptNo}_${isColor ? 'Color' : 'BW'}.pdf`);
    message.success("PDF generated!");
  };

  const columns = [
    { 
      title: 'Date & Time', 
      dataIndex: 'createdAt', 
      width: 105, 
      render: (d) => (
        <div className="leading-tight">
          <div className="text-[11px] font-semibold text-gray-800">{dayjs(d).format('DD MMM YYYY')}</div>
          <div className="text-[10px] text-gray-500">{dayjs(d).format('HH:mm:ss')}</div>
        </div>
      ) 
    },
    { 
      title: 'Receipt No.', 
      dataIndex: 'receiptNo', 
      width: 100, 
      render: (t) => <span className="text-[11px] font-mono font-semibold text-amber-700">{t || '-'}</span> 
    },
    { 
      title: 'Vehicle Reg.', 
      dataIndex: 'noPlate', 
      width: 95, 
      render: (t) => <div className="inline-block bg-gray-900 text-white px-2 py-1 rounded text-[11px] font-bold tracking-wide">{t || '-'}</div> 
    },
    { 
      title: 'Driver', 
      dataIndex: 'driverName', 
      width: 110, 
      render: (t) => <span className="text-[11px] font-medium text-gray-700">{t || '-'}</span> 
    },
    { 
      title: 'Commodity', 
      dataIndex: 'commodityName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Supplier', 
      dataIndex: 'supplierName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Transporter', 
      dataIndex: 'transporterName', 
      width: 115, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Customer', 
      dataIndex: 'customerName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Origin', 
      dataIndex: 'originName', 
      width: 100, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Destination', 
      dataIndex: 'destinationName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Weighbridge', 
      dataIndex: 'weighBridgeName', 
      width: 115, 
      render: (t) => <span className="text-[11px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Scale', 
      dataIndex: 'scaleName', 
      width: 95, 
      render: (t) => <span className="text-[11px] text-gray-700 font-medium">{t || '-'}</span> 
    },
    { 
      title: 'Mode', 
      dataIndex: 'weighMode', 
      width: 90, 
      render: (t) => (
        <Tag color="blue" className="text-[10px] font-semibold px-2.5 py-0.5 rounded-md border-0 m-0 uppercase">
          {t || 'N/A'}
        </Tag>
      )
    },
    { 
      title: 'Operation', 
      dataIndex: 'operation', 
      width: 95, 
      render: (t) => (
        <Tag color="purple" className="text-[10px] font-semibold px-2.5 py-0.5 rounded-md border-0 m-0 uppercase">
          {t || 'N/A'}
        </Tag>
      )
    },
    { 
      title: 'Operator', 
      dataIndex: 'operatorName', 
      width: 120, 
      render: (text, record) => {
        const operatorName = text || record.firstWeightOperator || 'N/A';
        return (
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white text-[10px] font-bold shadow-sm">
              {operatorName !== 'N/A' ? operatorName.charAt(0).toUpperCase() : '?'}
            </div>
            <span className="text-[11px] text-gray-700 font-medium">{operatorName}</span>
          </div>
        );
      }
    },
    { 
      title: 'First Weight', 
      dataIndex: 'firstWeight', 
      width: 100, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[11px] font-bold text-blue-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[9px] text-gray-500 uppercase tracking-wide">kg</span>
        </div>
      )
    },
    { 
      title: 'Second Weight', 
      dataIndex: 'secondWeight', 
      width: 100, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[11px] font-bold text-green-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[9px] text-gray-500 uppercase tracking-wide">kg</span>
        </div>
      )
    },
    { 
      title: 'Net Weight', 
      dataIndex: 'netWeight', 
      width: 105, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[12px] font-bold text-orange-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[9px] text-gray-500 uppercase tracking-wide">kg</span>
        </div>
      )
    },
    // ✅ UPDATED: Combined TAT/Wait column
    { 
      title: <span className="flex items-center gap-1"><ClockCircleOutlined />Turnaround</span>, 
      width: 100, 
      render: (_, record) => {
        const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted = hasSecondWeight || record.status === 'Completed' || record.status === 'completed';
        
        // Debug logging
        console.log("TAT Column Record:", {
          receiptNo: record.receiptNo,
          isCompleted,
          firstWeightTime: record.firstWeightTime,
          secondWeightTime: record.secondWeightTime,
          createdAt: record.createdAt,
          allFields: Object.keys(record)
        });
        
        let timeData;
        if (isCompleted) {
          // Show turnaround time for completed transactions
          timeData = calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime);
        } else {
          // Show wait time for incomplete transactions
          timeData = calculateWaitTime(record.createdAt);
        }
        
        const color = getTimeColor(timeData.minutes, isCompleted);
        const title = isCompleted 
          ? `Turnaround Time: ${timeData.display}` 
          : `Waiting Time: ${timeData.display}`;
        
        return (
          <Tag 
            color={color} 
            className="text-[10px] font-bold px-2.5 py-1 rounded-md border-0 m-0"
            title={title}
          >
            {timeData.display}
          </Tag>
        );
      }
    },
    { 
      title: 'Status', 
      dataIndex: 'status', 
      width: 110, 
      fixed: 'right',
      render: (s, record) => {
        // Determine actual status based on weights
        const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted = hasSecondWeight || s === 'Completed' || s === 'completed';
        
        return (
          <Tag 
            color={isCompleted ? '#10b981' : '#f59e0b'} 
            className="text-[10px] font-bold px-3 py-1 rounded-md border-0 m-0 uppercase tracking-wide"
          >
            {isCompleted ? 'COMPLETED' : 'IN PROGRESS'}
          </Tag>
        );
      } 
    },
    { 
      title: 'Actions', 
      width: 75, 
      fixed: 'right', 
      render: (_, r) => (
        <Button 
          size="small" 
          type="text"
          icon={<Eye size={14} />} 
          onClick={() => openViewDrawer(r)} 
          className="text-blue-600 hover:bg-blue-50 h-7 px-2.5 rounded-md font-medium"
        >
          View
        </Button>
      ) 
    },
  ];

  return (
    <div className="h-screen flex flex-col bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Modern Professional Header */}
      <div className="bg-white border-b border-gray-200 shadow-sm px-6 py-4 flex items-center gap-4 shrink-0">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-md">
            <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <div className="text-lg font-bold text-gray-900">Transactions Management</div>
            <div className="text-xs text-gray-500 font-medium">
              {Object.entries(filters).filter(([key, value]) => 
                value && !['page', 'pageSize'].includes(key)
              ).length > 0 ? (
                <>
                  <span className="font-semibold text-amber-600">{filteredTransactions.length}</span> filtered records from <span className="font-semibold">{total || 0}</span> total
                </>
              ) : (
                <>{total || 0} total records • TAT = Turnaround Time</>
              )}
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-3 justify-end items-center">
          <Input 
            allowClear 
            placeholder="Search by receipt, vehicle, driver..." 
            prefix={<Search size={16} className="text-gray-400" />} 
            className="w-80 h-9 text-sm rounded-lg shadow-sm border-gray-300" 
            value={filters.search}
            onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })} 
          />
          <Button 
            icon={<Filter size={16} />} 
            className={`h-9 text-sm rounded-lg font-medium ${showFilters ? 'bg-amber-50 text-amber-600 border-amber-300' : 'border-gray-300'}`}
            onClick={() => setShowFilters(!showFilters)}
          >
            {showFilters ? 'Hide Filters' : 'Show Filters'}
          </Button>
          <Button 
            type="primary" 
            icon={<ReloadOutlined />}
            className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0 h-9 text-sm rounded-lg shadow-md font-medium" 
            onClick={() => {
              loadTransactions();
            }}
            loading={loading}
          >
            Refresh Data
          </Button>
        </div>
      </div>

      {/* Modern Filters Panel */}
      {showFilters && (
        <div className="bg-gradient-to-br from-gray-50 via-slate-50 to-gray-50 border-b border-gray-200 px-6 py-4 shrink-0 shadow-sm">
          <div className="grid grid-cols-4 gap-4 mb-4">
            {/* Date Range Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Date Range</label>
              <RangePicker 
                className="w-full h-9 text-sm rounded-lg shadow-sm border-gray-300" 
                value={filters.dateRange}
                onChange={(d) => setFilters({ ...filters, dateRange: d, page: 1 })} 
                size="middle"
                format="DD-MM-YYYY"
                placeholder={['Start Date', 'End Date']}
              />
            </div>
            
            {/* Status Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Status</label>
              <select 
                className="w-full h-9 text-sm rounded-lg border border-gray-300 px-3 focus:border-amber-500 focus:outline-none focus:ring-2 focus:ring-amber-200 bg-white"
                value={filters.status || ''}
                onChange={(e) => setFilters({ ...filters, status: e.target.value || null, page: 1 })}
              >
                <option value="">All Status</option>
                <option value="completed">Completed</option>
                <option value="inprogress">In Progress</option>
              </select>
            </div>
            
            {/* Vehicle Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Vehicle Registration</label>
              <Input 
                placeholder="Enter vehicle plate..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.vehicle || ''}
                onChange={(e) => setFilters({ ...filters, vehicle: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Driver Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Driver Name</label>
              <Input 
                placeholder="Enter driver name..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.driver || ''}
                onChange={(e) => setFilters({ ...filters, driver: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>
          
          <div className="grid grid-cols-5 gap-4 mb-4">
            {/* Commodity Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Commodity</label>
              <Input 
                placeholder="Search commodity..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.commodity || ''}
                onChange={(e) => setFilters({ ...filters, commodity: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Supplier Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Supplier</label>
              <Input 
                placeholder="Search supplier..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.supplier || ''}
                onChange={(e) => setFilters({ ...filters, supplier: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Transporter Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Transporter</label>
              <Input 
                placeholder="Search transporter..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.transporter || ''}
                onChange={(e) => setFilters({ ...filters, transporter: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Customer Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Customer</label>
              <Input 
                placeholder="Search customer..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.customer || ''}
                onChange={(e) => setFilters({ ...filters, customer: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Operator Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Operator</label>
              <Input 
                placeholder="Search operator..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.operator || ''}
                onChange={(e) => setFilters({ ...filters, operator: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>
          
          <div className="grid grid-cols-4 gap-4">
            {/* Origin Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Origin</label>
              <Input 
                placeholder="Search origin..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.origin || ''}
                onChange={(e) => setFilters({ ...filters, origin: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Destination Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Destination</label>
              <Input 
                placeholder="Search destination..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.destination || ''}
                onChange={(e) => setFilters({ ...filters, destination: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Weighbridge Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Weighbridge</label>
              <Input 
                placeholder="Search weighbridge..."
                className="w-full h-9 text-sm rounded-lg border-gray-300"
                value={filters.weighbridge || ''}
                onChange={(e) => setFilters({ ...filters, weighbridge: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Weigh Mode Filter */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-semibold text-gray-700 uppercase tracking-wider">Weigh Mode</label>
              <select 
                className="w-full h-9 text-sm rounded-lg border border-gray-300 px-3 focus:border-amber-500 focus:outline-none focus:ring-2 focus:ring-amber-200 bg-white"
                value={filters.weighMode || ''}
                onChange={(e) => setFilters({ ...filters, weighMode: e.target.value || null, page: 1 })}
              >
                <option value="">All Modes</option>
                <option value="single">Single</option>
                <option value="double">Double</option>
                <option value="auto">Auto</option>
              </select>
            </div>
          </div>
          
          {/* Filter Actions Bar */}
          <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-300">
            <div className="flex items-center gap-3">
              {/* Active Filters Count */}
              {Object.entries(filters).filter(([key, value]) => 
                value && !['page', 'pageSize'].includes(key)
              ).length > 0 && (
                <div className="flex items-center gap-3">
                  <span className="text-xs text-amber-800 font-semibold bg-amber-100 px-3 py-1.5 rounded-lg shadow-sm border border-amber-200">
                    {Object.entries(filters).filter(([key, value]) => 
                      value && !['page', 'pageSize'].includes(key)
                    ).length} Active Filters
                  </span>
                  
                  {/* Show active filter tags */}
                  <div className="flex flex-wrap gap-2">
                    {filters.dateRange && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, dateRange: null, page: 1 })}
                        className="text-[11px] m-0 bg-blue-50 border-blue-300 px-2.5 py-1 rounded-md"
                      >
                        Date: {filters.dateRange[0].format('DD-MM')} - {filters.dateRange[1].format('DD-MM')}
                      </Tag>
                    )}
                    {filters.status && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, status: null, page: 1 })}
                        className="text-[11px] m-0 bg-green-50 border-green-300 px-2.5 py-1 rounded-md"
                      >
                        Status: {filters.status}
                      </Tag>
                    )}
                    {filters.vehicle && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, vehicle: '', page: 1 })}
                        className="text-[11px] m-0 bg-purple-50 border-purple-300 px-2.5 py-1 rounded-md"
                      >
                        Vehicle: {filters.vehicle}
                      </Tag>
                    )}
                    {filters.driver && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, driver: '', page: 1 })}
                        className="text-[11px] m-0 bg-pink-50 border-pink-300 px-2.5 py-1 rounded-md"
                      >
                        Driver: {filters.driver}
                      </Tag>
                    )}
                  </div>
                </div>
              )}
            </div>
            
            {/* Clear All Button */}
            {Object.entries(filters).filter(([key, value]) => 
              value && !['page', 'pageSize'].includes(key)
            ).length > 0 && (
              <Button 
                size="middle"
                danger
                icon={<CloseOutlined />}
                onClick={() => {
                  setFilters({ 
                    search: "", 
                    dateRange: null, 
                    status: null,
                    vehicle: '',
                    driver: '',
                    commodity: '',
                    supplier: '',
                    transporter: '',
                    customer: '',
                    operator: '',
                    origin: '',
                    destination: '',
                    weighbridge: '',
                    weighMode: null,
                    page: 1, 
                    pageSize: filters.pageSize 
                  });
                }}
                className="h-9 text-sm font-semibold shadow-sm rounded-lg"
              >
                Clear All Filters
              </Button>
            )}
          </div>
        </div>
      )}

      {/* Compact Table */}
      <div className="flex-1 overflow-hidden px-4 pb-4 pt-3">
        <div className="h-full bg-white rounded-lg border shadow-sm overflow-hidden">
          <Table 
            columns={columns} 
            dataSource={filteredTransactions} 
            rowKey="id" 
            loading={loading} 
            size="small" 
            className="compact-table" 
            scroll={{ y: "calc(100vh - 180px)", x: 1800 }}
            pagination={{ 
              current: filters.page, 
              pageSize: filters.pageSize, 
              total: filteredTransactions.length, 
              showSizeChanger: true,
              showTotal: (total) => `Total ${total}`,
              size: 'small',
              className: 'compact-pagination',
              onChange: (p, ps) => setFilters({ ...filters, page: p, pageSize: ps }) 
            }} 
          />
        </div>
      </div>

      {/* Side Drawer for Details */}
      <Drawer
        title={
          <div className="flex items-center justify-between">
            <span className="font-bold text-base">Ticket: {selectedRecord?.receiptNo}</span>
            <Radio.Group value={printMode} onChange={(e) => setPrintMode(e.target.value)} size="small">
              <Radio.Button value="color">Color</Radio.Button>
              <Radio.Button value="bw">B&W</Radio.Button>
            </Radio.Group>
          </div>
        }
        placement="right"
        onClose={() => setIsDrawerOpen(false)}
        open={isDrawerOpen}
        width={480}
        footer={
          <div className="flex gap-2 justify-end">
            {!isEditing ? (
              <>
                <Button icon={<EditOutlined />} onClick={() => setIsEditing(true)}>Edit</Button>
                <Button type="primary" icon={<Printer />} onClick={() => generatePDF(selectedRecord, printMode === 'color')}>
                  Export PDF
                </Button>
              </>
            ) : (
              <>
                <Button icon={<CloseOutlined />} onClick={() => { setEditedRecord(selectedRecord); setIsEditing(false); }}>
                  Cancel
                </Button>
                <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={handleSave} className="bg-green-600">
                  Save Changes
                </Button>
              </>
            )}
          </div>
        }
      >
        {selectedRecord && (
          <div className="space-y-4">
            {/* Basic Info Card */}
            <div className="bg-gradient-to-r from-amber-50 to-orange-50 rounded-lg p-4 border border-amber-200">
              <div className="grid grid-cols-2 gap-3 text-xs">
                {[
                  { label: 'Receipt', field: 'receiptNo', editable: true },
                  { label: 'Vehicle', field: 'noPlate', editable: true },
                  { label: 'Driver', field: 'driverName', editable: true },
                  { label: 'Commodity', field: 'commodityName', editable: true },
                  { label: 'Axle Type', field: 'axleType', editable: true },
                  { label: 'Weigh Mode', field: 'weighMode', editable: true },
                  { label: 'Operation', field: 'operation', editable: true },
                  { label: 'Scale', field: 'scaleName', editable: true },
                ].map(({ label, field, editable }) => (
                  <div key={field}>
                    <div className="text-gray-600 font-medium mb-1">{label}</div>
                    {isEditing && editable ? (
                      <Input 
                        value={editedRecord[field]} 
                        onChange={(e) => setEditedRecord({...editedRecord, [field]: e.target.value})} 
                        size="small"
                        className="text-xs"
                      />
                    ) : (
                      <div className="font-semibold text-gray-900">{selectedRecord[field] || 'N/A'}</div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Transport Details */}
            <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
              <div className="text-xs font-bold text-blue-900 mb-3">TRANSPORT DETAILS</div>
              <div className="space-y-2 text-xs">
                {[
                  { label: 'Transporter', field: 'transporterName' },
                  { label: 'Supplier', field: 'supplierName' },
                  { label: 'Customer', field: 'customerName' },
                  { label: 'Origin', field: 'originName' },
                  { label: 'Destination', field: 'destinationName' },
                  { label: 'Container No', field: 'containerNo' },
                  { label: 'Seal No', field: 'sealNo' },
                  { label: 'Weighbridge', field: 'weighBridgeName' },
                ].map(({ label, field }) => (
                  <div key={field} className="flex justify-between">
                    <span className="text-gray-600">{label}:</span>
                    {isEditing ? (
                      <Input 
                        value={editedRecord[field]} 
                        onChange={(e) => setEditedRecord({...editedRecord, [field]: e.target.value})} 
                        size="small"
                        className="text-xs w-64"
                      />
                    ) : (
                      <span className="font-semibold text-gray-900">{selectedRecord[field] || 'N/A'}</span>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Weight Summary */}
            <div className="bg-green-50 rounded-lg p-4 border border-green-200">
              <div className="text-xs font-bold text-green-900 mb-3">WEIGHT SUMMARY</div>
              
              {/* Weight and Time Grid */}
              <div className="grid grid-cols-3 gap-2 text-xs mb-3">
                {/* First Weight */}
                <div className="bg-white rounded p-2">
                  <div className="text-gray-600 mb-1 text-[10px]">First Weight</div>
                  <div className="text-base font-bold text-blue-600">{selectedRecord.firstWeight || 0} kg</div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.firstWeightOperator || selectedRecord.operatorName || 'N/A'}
                  </div>
                </div>
                
                {/* Second Weight */}
                <div className="bg-white rounded p-2">
                  <div className="text-gray-600 mb-1 text-[10px]">Second Weight</div>
                  <div className="text-base font-bold text-green-600">{selectedRecord.secondWeight || 0} kg</div>
                  <div className="text-[9px] text-gray-500 mt-1">
                    {selectedRecord.secondWeightOperator || selectedRecord.operatorName || 'N/A'}
                  </div>
                </div>
                
                {/* Net Weight */}
                <div className="bg-orange-50 rounded p-2 border-2 border-orange-300">
                  <div className="text-orange-700 mb-1 text-[10px] font-semibold">Net Weight</div>
                  <div className="text-base font-bold text-orange-600">{selectedRecord.netWeight || 0} kg</div>
                  <div className="text-[9px] text-orange-600 mt-1 font-medium">Difference</div>
                </div>
              </div>

              {/* Time Grid */}
              <div className="grid grid-cols-3 gap-2 text-xs">
                {/* First Weight Time */}
                <div className="bg-blue-50 rounded p-2 border border-blue-200">
                  <div className="text-blue-700 mb-1 text-[10px] font-semibold">1st Weight Time</div>
                  <div className="text-xs font-bold text-blue-900">
                    {selectedRecord.firstWeightTime ? dayjs(selectedRecord.firstWeightTime).format("DD-MM-YY") : 'N/A'}
                  </div>
                  <div className="text-sm font-bold text-blue-600">
                    {selectedRecord.firstWeightTime ? dayjs(selectedRecord.firstWeightTime).format("HH:mm:ss") : 'N/A'}
                  </div>
                </div>
                
                {/* Second Weight Time */}
                <div className="bg-green-50 rounded p-2 border border-green-200">
                  <div className="text-green-700 mb-1 text-[10px] font-semibold">2nd Weight Time</div>
                  <div className="text-xs font-bold text-green-900">
                    {selectedRecord.secondWeightTime ? dayjs(selectedRecord.secondWeightTime).format("DD-MM-YY") : 'N/A'}
                  </div>
                  <div className="text-sm font-bold text-green-600">
                    {selectedRecord.secondWeightTime ? dayjs(selectedRecord.secondWeightTime).format("HH:mm:ss") : 'N/A'}
                  </div>
                </div>
                
                {/* Turnaround Time (Difference) */}
                <div className="bg-orange-50 rounded p-2 border-2 border-orange-300">
                  <div className="text-orange-700 mb-1 text-[10px] font-semibold">Turnaround</div>
                  <div className="text-lg font-bold text-orange-600 mt-2">
                    {formatTurnaroundTimeSimple(selectedRecord.firstWeightTime, selectedRecord.secondWeightTime)}
                  </div>
                  <div className="text-[9px] text-orange-600 font-medium">Time Difference</div>
                </div>
              </div>
            </div>

            {/* Status & Remarks */}
            <div className="bg-gray-50 rounded-lg p-4 border border-gray-200">
              <div className="space-y-3 text-xs">
                <div>
                  <div className="text-gray-600 font-medium mb-1">Status</div>
                  {isEditing ? (
                    <Input 
                      value={editedRecord.status} 
                      onChange={(e) => setEditedRecord({...editedRecord, status: e.target.value})} 
                      size="small"
                    />
                  ) : (
                    <Tag 
                      color={
                        (selectedRecord.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) || 
                        selectedRecord.status === 'Completed' || 
                        selectedRecord.status === 'completed' 
                          ? '#10b981' 
                          : '#f59e0b'
                      } 
                      className="font-bold text-sm px-3 py-1"
                    >
                      {(selectedRecord.secondWeight && parseFloat(selectedRecord.secondWeight) > 0) || 
                       selectedRecord.status === 'Completed' || 
                       selectedRecord.status === 'completed'
                        ? 'COMPLETED' 
                        : 'IN PROGRESS'}
                    </Tag>
                  )}
                </div>
                <div>
                  <div className="text-gray-600 font-medium mb-1">Remarks / Notes</div>
                  {isEditing ? (
                    <Input.TextArea 
                      value={editedRecord.remarks || editedRecord.notes} 
                      onChange={(e) => setEditedRecord({...editedRecord, remarks: e.target.value, notes: e.target.value})} 
                      rows={3}
                      className="text-xs"
                    />
                  ) : (
                    <div className="text-gray-900 bg-white p-2 rounded border">{selectedRecord.remarks || selectedRecord.notes || 'No remarks'}</div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </Drawer>

      <style>{`
        .compact-table .ant-table {
          font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, #f8fafc, #f1f5f9) !important;
          border-bottom: 2px solid #cbd5e1 !important;
          padding: 10px 12px !important;
          font-weight: 600 !important;
          font-size: 11px !important;
          color: #475569 !important;
          text-transform: uppercase;
          letter-spacing: 0.5px;
          line-height: 1.4;
        }
        .compact-table .ant-table-tbody > tr > td {
          padding: 10px 12px !important;
          border-bottom: 1px solid #f1f5f9 !important;
          line-height: 1.5;
          background: white;
        }
        .compact-table .ant-table-tbody > tr:hover > td {
          background: #fafaf9 !important;
          transition: background-color 0.2s ease;
        }
        .compact-table .ant-table-tbody > tr:nth-child(even) > td {
          background: #fafafa;
        }
        .compact-table .ant-table-tbody > tr:nth-child(even):hover > td {
          background: #fafaf9 !important;
        }
        .compact-pagination {
          display: flex;
          justify-content: center;
          align-items: center;
          padding: 12px 0;
        }
        .compact-pagination .ant-pagination-item,
        .compact-pagination .ant-pagination-prev,
        .compact-pagination .ant-pagination-next {
          min-width: 28px !important;
          height: 28px !important;
          line-height: 26px !important;
          font-size: 12px !important;
          border-radius: 6px;
          border: 1px solid #e2e8f0;
          font-weight: 500;
        }
        .compact-pagination .ant-pagination-item-active {
          background: linear-gradient(135deg, #f59e0b, #f97316);
          border-color: #f59e0b;
          box-shadow: 0 2px 4px rgba(245, 158, 11, 0.2);
        }
        .compact-pagination .ant-pagination-item-active a {
          color: white !important;
          font-weight: 600;
        }
        .compact-pagination .ant-pagination-item:hover {
          border-color: #f59e0b;
          transform: translateY(-1px);
          transition: all 0.2s ease;
        }
        .compact-pagination .ant-select-selector {
          height: 28px !important;
          font-size: 12px !important;
          border-radius: 6px !important;
        }
        .compact-pagination .ant-pagination-options {
          margin-left: 16px;
        }
      `}</style>
    </div>
  );
}