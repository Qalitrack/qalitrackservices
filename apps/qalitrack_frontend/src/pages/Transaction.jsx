import React, { useEffect, useState, useCallback } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, DatePicker, Space, Modal, message, Radio, Typography, Drawer } from "antd";
import { ReloadOutlined, SearchOutlined, EditOutlined, SaveOutlined, CloseOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { fetchTransactions, fetchUserById, updateTransactionApi } from "../store/weighingSlice";
import { Printer, Eye, Search, Filter } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";

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

  const calculateTurnaroundTime = (first, second) => {
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
        [{ content: "TURNAROUND", styles: { fillColor: colors.accent } }, { content: calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime), colSpan: 3, styles: { fillColor: colors.accent } }],
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
      title: 'Date', 
      dataIndex: 'createdAt', 
      width: 75, 
      render: (d) => (
        <div className="leading-tight">
          <div className="text-[10px] font-semibold text-gray-800">{dayjs(d).format('DD MMM')}</div>
          <div className="text-[9px] text-gray-500">{dayjs(d).format('HH:mm')}</div>
        </div>
      ) 
    },
    { 
      title: 'Receipt', 
      dataIndex: 'receiptNo', 
      width: 85, 
      render: (t) => <span className="text-[10px] font-mono font-semibold text-amber-700">{t || '-'}</span> 
    },
    { 
      title: 'Vehicle', 
      dataIndex: 'noPlate', 
      width: 75, 
      render: (t) => <div className="inline-block bg-gray-900 text-white px-1.5 py-0.5 rounded text-[10px] font-bold">{t || '-'}</div> 
    },
    { 
      title: 'Driver', 
      dataIndex: 'driverName', 
      width: 90, 
      render: (t) => <span className="text-[10px] font-medium text-gray-700">{t || '-'}</span> 
    },
    { 
      title: 'Axle Type', 
      dataIndex: 'axleType', 
      width: 75, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Commodity', 
      dataIndex: 'commodityName', 
      width: 90, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Supplier', 
      dataIndex: 'supplierName', 
      width: 90, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Transporter', 
      dataIndex: 'transporterName', 
      width: 95, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Customer', 
      dataIndex: 'customerName', 
      width: 90, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Origin', 
      dataIndex: 'originName', 
      width: 80, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Destination', 
      dataIndex: 'destinationName', 
      width: 90, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Container', 
      dataIndex: 'containerNo', 
      width: 85, 
      render: (t) => <span className="text-[10px] font-mono text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Seal No', 
      dataIndex: 'sealNo', 
      width: 80, 
      render: (t) => <span className="text-[10px] font-mono text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Weighbridge', 
      dataIndex: 'weighBridgeName', 
      width: 100, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Scale', 
      dataIndex: 'scaleName', 
      width: 80, 
      render: (t) => <span className="text-[10px] text-gray-600">{t || '-'}</span> 
    },
    { 
      title: 'Weigh Mode', 
      dataIndex: 'weighMode', 
      width: 85, 
      render: (t) => (
        <Tag color="blue" className="text-[9px] font-medium px-1.5 py-0 rounded-full border-0 m-0">
          {t || 'N/A'}
        </Tag>
      )
    },
    { 
      title: 'Operation', 
      dataIndex: 'operation', 
      width: 85, 
      render: (t) => (
        <Tag color="purple" className="text-[9px] font-medium px-1.5 py-0 rounded-full border-0 m-0">
          {t || 'N/A'}
        </Tag>
      )
    },
    { 
      title: 'Operator', 
      dataIndex: 'operatorName', 
      width: 95, 
      render: (text, record) => {
        const operatorName = text || record.firstWeightOperator || 'N/A';
        return (
          <div className="flex items-center gap-1.5">
            <div className="w-5 h-5 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white text-[9px] font-bold shadow-sm">
              {operatorName !== 'N/A' ? operatorName.charAt(0).toUpperCase() : '?'}
            </div>
            <span className="text-[10px] text-gray-700 font-medium truncate max-w-[55px]">{operatorName}</span>
          </div>
        );
      }
    },
    { 
      title: '1st Wt', 
      dataIndex: 'firstWeight', 
      width: 75, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[10px] font-semibold text-blue-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[8px] text-gray-500">kg</span>
        </div>
      )
    },
    { 
      title: '2nd Wt', 
      dataIndex: 'secondWeight', 
      width: 75, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[10px] font-semibold text-green-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[8px] text-gray-500">kg</span>
        </div>
      )
    },
    { 
      title: 'Net Wt', 
      dataIndex: 'netWeight', 
      width: 80, 
      align: 'right', 
      render: (w) => (
        <div className="flex flex-col items-end leading-tight">
          <span className="text-[10px] font-bold text-orange-600">{w ? `${parseFloat(w).toLocaleString()}` : '-'}</span>
          <span className="text-[8px] text-gray-500">kg</span>
        </div>
      )
    },
    { 
      title: 'Turnaround', 
      width: 80, 
      render: (_, record) => {
        const time = calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime);
        return (
          <Tag color="cyan" className="text-[9px] font-medium px-1.5 py-0 rounded-full border-0 m-0">
            {time}
          </Tag>
        );
      }
    },
    { 
      title: 'Status', 
      dataIndex: 'status', 
      width: 90, 
      fixed: 'right',
      render: (s, record) => {
        // Determine actual status based on weights
        const hasSecondWeight = record.secondWeight && parseFloat(record.secondWeight) > 0;
        const isCompleted = hasSecondWeight || s === 'Completed' || s === 'completed';
        
        return (
          <Tag 
            color={isCompleted ? '#10b981' : '#f59e0b'} 
            className="text-[9px] font-semibold px-2 py-0.5 rounded-full border-0 m-0"
          >
            {isCompleted ? 'COMPLETED' : 'IN PROGRESS'}
          </Tag>
        );
      } 
    },
    { 
      title: '', 
      width: 60, 
      fixed: 'right', 
      render: (_, r) => (
        <Button 
          size="small" 
          type="text"
          icon={<Eye size={13} />} 
          onClick={() => openViewDrawer(r)} 
          className="text-blue-600 hover:bg-blue-50 h-6 px-2"
        />
      ) 
    },
  ];

  return (
    <div className="h-screen flex flex-col bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Compact Header */}
      <div className="bg-white border-b shadow-sm px-4 py-2.5 flex items-center gap-3 shrink-0">
        <div className="flex items-center gap-2">
          <div className="w-1 h-7 bg-gradient-to-b from-amber-500 to-orange-600 rounded-full" />
          <div>
            <div className="text-sm font-bold text-gray-900">Transactions</div>
            <div className="text-[10px] text-gray-500">
              {Object.entries(filters).filter(([key, value]) => 
                value && !['page', 'pageSize'].includes(key)
              ).length > 0 ? (
                <>
                  <span className="font-semibold text-amber-600">{filteredTransactions.length}</span> filtered from <span className="font-semibold">{total || 0}</span> total
                </>
              ) : (
                <>{total || 0} records</>
              )}
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end">
          <Input 
            allowClear 
            placeholder="Search receipt, vehicle, driver..." 
            prefix={<Search size={14} className="text-gray-400" />} 
            className="w-64 h-8 text-xs rounded-lg shadow-sm" 
            value={filters.search}
            onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })} 
          />
          <Button 
            icon={<Filter size={14} />} 
            className={`h-8 text-xs rounded-lg ${showFilters ? 'bg-amber-50 text-amber-600 border-amber-300' : ''}`}
            onClick={() => setShowFilters(!showFilters)}
          >
            {showFilters ? 'Hide Filters' : 'Show Filters'}
          </Button>
          <Button 
            type="primary" 
            icon={<ReloadOutlined />}
            className="bg-gradient-to-r from-amber-500 to-amber-600 border-0 h-8 text-xs rounded-lg shadow-sm" 
            onClick={() => {
              loadTransactions();
            }}
            loading={loading}
          >
            Refresh
          </Button>
        </div>
      </div>

      {/* Enhanced Multi-Column Filters */}
      {showFilters && (
        <div className="bg-gradient-to-br from-amber-50 via-orange-50 to-yellow-50 border-b border-amber-200 px-4 py-3 shrink-0 shadow-sm">
          <div className="grid grid-cols-4 gap-3 mb-3">
            {/* Date Range Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Date Range</label>
              <RangePicker 
                className="w-full h-7 text-xs rounded-md shadow-sm" 
                value={filters.dateRange}
                onChange={(d) => setFilters({ ...filters, dateRange: d, page: 1 })} 
                size="small"
                format="DD-MM-YYYY"
                placeholder={['From', 'To']}
              />
            </div>
            
            {/* Status Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Status</label>
              <select 
                className="w-full h-7 text-xs rounded-md border border-gray-300 px-2 focus:border-amber-500 focus:outline-none focus:ring-1 focus:ring-amber-500 bg-white"
                value={filters.status || ''}
                onChange={(e) => setFilters({ ...filters, status: e.target.value || null, page: 1 })}
              >
                <option value="">All Status</option>
                <option value="completed">Completed</option>
                <option value="inprogress">In Progress</option>
              </select>
            </div>
            
            {/* Vehicle Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Vehicle</label>
              <Input 
                placeholder="Vehicle plate..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.vehicle || ''}
                onChange={(e) => setFilters({ ...filters, vehicle: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Driver Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Driver</label>
              <Input 
                placeholder="Driver name..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.driver || ''}
                onChange={(e) => setFilters({ ...filters, driver: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>
          
          <div className="grid grid-cols-5 gap-3 mb-3">
            {/* Commodity Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Commodity</label>
              <Input 
                placeholder="Commodity..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.commodity || ''}
                onChange={(e) => setFilters({ ...filters, commodity: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Supplier Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Supplier</label>
              <Input 
                placeholder="Supplier..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.supplier || ''}
                onChange={(e) => setFilters({ ...filters, supplier: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Transporter Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Transporter</label>
              <Input 
                placeholder="Transporter..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.transporter || ''}
                onChange={(e) => setFilters({ ...filters, transporter: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Customer Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Customer</label>
              <Input 
                placeholder="Customer..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.customer || ''}
                onChange={(e) => setFilters({ ...filters, customer: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Operator Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Operator</label>
              <Input 
                placeholder="Operator..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.operator || ''}
                onChange={(e) => setFilters({ ...filters, operator: e.target.value, page: 1 })}
                allowClear
              />
            </div>
          </div>
          
          <div className="grid grid-cols-4 gap-3">
            {/* Origin Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Origin</label>
              <Input 
                placeholder="Origin..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.origin || ''}
                onChange={(e) => setFilters({ ...filters, origin: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Destination Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Destination</label>
              <Input 
                placeholder="Destination..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.destination || ''}
                onChange={(e) => setFilters({ ...filters, destination: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Weighbridge Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Weighbridge</label>
              <Input 
                placeholder="Weighbridge..."
                className="w-full h-7 text-xs rounded-md"
                value={filters.weighbridge || ''}
                onChange={(e) => setFilters({ ...filters, weighbridge: e.target.value, page: 1 })}
                allowClear
              />
            </div>
            
            {/* Weigh Mode Filter */}
            <div className="space-y-1">
              <label className="text-[10px] font-semibold text-gray-700 uppercase tracking-wide">Weigh Mode</label>
              <select 
                className="w-full h-7 text-xs rounded-md border border-gray-300 px-2 focus:border-amber-500 focus:outline-none focus:ring-1 focus:ring-amber-500 bg-white"
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
          <div className="flex items-center justify-between mt-3 pt-3 border-t border-amber-300">
            <div className="flex items-center gap-2">
              {/* Active Filters Count */}
              {Object.entries(filters).filter(([key, value]) => 
                value && !['page', 'pageSize'].includes(key)
              ).length > 0 && (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-amber-800 font-semibold bg-amber-200 px-2.5 py-1 rounded-full shadow-sm">
                    {Object.entries(filters).filter(([key, value]) => 
                      value && !['page', 'pageSize'].includes(key)
                    ).length} active filters
                  </span>
                  
                  {/* Show active filter tags */}
                  <div className="flex flex-wrap gap-1.5">
                    {filters.dateRange && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, dateRange: null, page: 1 })}
                        className="text-[10px] m-0 bg-blue-50 border-blue-200"
                      >
                        Date: {filters.dateRange[0].format('DD-MM')} - {filters.dateRange[1].format('DD-MM')}
                      </Tag>
                    )}
                    {filters.status && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, status: null, page: 1 })}
                        className="text-[10px] m-0 bg-green-50 border-green-200"
                      >
                        Status: {filters.status}
                      </Tag>
                    )}
                    {filters.vehicle && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, vehicle: '', page: 1 })}
                        className="text-[10px] m-0 bg-purple-50 border-purple-200"
                      >
                        Vehicle: {filters.vehicle}
                      </Tag>
                    )}
                    {filters.driver && (
                      <Tag 
                        closable 
                        onClose={() => setFilters({ ...filters, driver: '', page: 1 })}
                        className="text-[10px] m-0 bg-pink-50 border-pink-200"
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
                size="small"
                danger
                icon={<CloseOutlined className="text-[10px]" />}
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
                className="h-7 text-xs font-semibold shadow-sm"
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
            scroll={{ y: "calc(100vh - 180px)", x: 2100 }}
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
              <div className="grid grid-cols-2 gap-3 text-xs">
                <div className="bg-white rounded p-2">
                  <div className="text-gray-600 mb-1">First Weight</div>
                  <div className="text-lg font-bold text-blue-600">{selectedRecord.firstWeight || 0} kg</div>
                  <div className="text-[10px] text-gray-500 mt-1">
                    {selectedRecord.firstWeightOperator || selectedRecord.operatorName || 'N/A'}<br/>
                    {selectedRecord.firstWeightTime ? dayjs(selectedRecord.firstWeightTime).format("DD-MM-YY HH:mm") : 'N/A'}
                  </div>
                </div>
                <div className="bg-white rounded p-2">
                  <div className="text-gray-600 mb-1">Second Weight</div>
                  <div className="text-lg font-bold text-green-600">{selectedRecord.secondWeight || 0} kg</div>
                  <div className="text-[10px] text-gray-500 mt-1">
                    {selectedRecord.secondWeightOperator || selectedRecord.operatorName || 'N/A'}<br/>
                    {selectedRecord.secondWeightTime ? dayjs(selectedRecord.secondWeightTime).format("DD-MM-YY HH:mm") : 'N/A'}
                  </div>
                </div>
              </div>
              <div className="bg-orange-100 rounded p-3 mt-3 border-2 border-orange-300">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-bold text-orange-900">NET WEIGHT</span>
                  <span className="text-2xl font-bold text-orange-600">{selectedRecord.netWeight || 0} kg</span>
                </div>
                <div className="flex justify-between items-center mt-2 pt-2 border-t border-orange-200">
                  <span className="text-xs text-orange-800">Turnaround Time</span>
                  <span className="text-sm font-bold text-orange-700">
                    {calculateTurnaroundTime(selectedRecord.firstWeightTime, selectedRecord.secondWeightTime)}
                  </span>
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
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, #fffbeb, #fef3c7) !important;
          border-bottom: 1.5px solid #f59e0b !important;
          padding: 6px 8px !important;
          font-weight: 700 !important;
          font-size: 9px !important;
          text-transform: uppercase;
          letter-spacing: 0.3px;
          line-height: 1.2;
        }
        .compact-table .ant-table-tbody > tr > td {
          padding: 6px 8px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          line-height: 1.3;
        }
        .compact-table .ant-table-tbody > tr:hover > td {
          background: #fffbeb !important;
        }
        .compact-pagination .ant-pagination-item,
        .compact-pagination .ant-pagination-prev,
        .compact-pagination .ant-pagination-next {
          min-width: 24px !important;
          height: 24px !important;
          line-height: 22px !important;
          font-size: 11px !important;
          border-radius: 4px;
        }
        .compact-pagination .ant-pagination-item-active {
          background: linear-gradient(135deg, #f59e0b, #f97316);
          border-color: #f59e0b;
        }
        .compact-pagination .ant-pagination-item-active a {
          color: white !important;
        }
        .compact-pagination .ant-select-selector {
          height: 24px !important;
          font-size: 11px !important;
        }
      `}</style>
    </div>
  );
}