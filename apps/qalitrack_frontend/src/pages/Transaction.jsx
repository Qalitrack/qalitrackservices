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
  const [filters, setFilters] = useState({ search: "", dateRange: null, page: 1, pageSize: 15 });
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
    
    dispatch(fetchTransactions(params));
  }, [dispatch, filters]);

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
        id: editedRecord.id,
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
        ["TICKET NO", `: ${record.receiptNo}`, "REGISTRATION", `: ${record.noPlate}`],
        ["AXLE TYPE", `: ${record.axleType || "N/A"}`, "COMMODITY", `: ${record.commodityName}`],
        ["TRANSPORTER", `: ${record.transporterName}`, "DRIVER", `: ${record.driverName || "N/A"}`],
        ["SUPPLIER", `: ${record.supplierName || "N/A"}`, "CUSTOMER", `: ${record.customerName || "N/A"}`],
        ["SOURCE", `: ${record.originName || "N/A"}`, "DESTINATION", `: ${record.destinationName || "N/A"}`],
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
        ["FIRST WEIGHT", `${record.firstWeight || 0} Kg`, record.firstWeightOperator || "N/A", record.firstWeightTime ? dayjs(record.firstWeightTime).format("DD-MM-YY HH:mm") : "N/A"],
        ["SECOND WEIGHT", `${record.secondWeight || 0} Kg`, record.secondWeightOperator || "N/A", record.secondWeightTime ? dayjs(record.secondWeightTime).format("DD-MM-YY HH:mm") : "N/A"],
        [{ content: "NET WEIGHT", styles: { fillColor: colors.light, fontStyle: "bold" } }, { content: `${record.netWeight || 0} Kg`, styles: { fillColor: colors.light, fontStyle: "bold" } }, "", ""],
        [{ content: "TURNAROUND", styles: { fillColor: colors.accent } }, { content: calculateTurnaroundTime(record.firstWeightTime, record.secondWeightTime), colSpan: 3, styles: { fillColor: colors.accent } }],
      ],
    });

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
          <div className="text-xs font-bold text-gray-900">{dayjs(d).format('DD MMM')}</div>
          <div className="text-[10px] text-gray-400">{dayjs(d).format('YY')}</div>
        </div>
      ) 
    },
    { 
      title: 'Receipt', 
      dataIndex: 'receiptNo', 
      width: 110, 
      render: (t) => <span className="text-[11px] font-mono font-bold text-amber-600">{t}</span> 
    },
    { 
      title: 'Vehicle', 
      dataIndex: 'noPlate', 
      width: 85, 
      render: (t) => <div className="inline-block bg-gray-900 text-white px-1.5 py-0.5 rounded text-[10px] font-bold tracking-wide">{t}</div> 
    },
    { 
      title: 'Driver', 
      dataIndex: 'driverName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-700">{t}</span> 
    },
    { 
      title: 'Commodity', 
      dataIndex: 'commodityName', 
      width: 110, 
      render: (t) => <span className="text-[11px] text-gray-600">{t}</span> 
    },
    { 
      title: 'Transporter', 
      dataIndex: 'transporterName', 
      width: 120, 
      render: (t) => <span className="text-[11px] text-gray-600">{t}</span> 
    },
    { 
      title: 'Route', 
      dataIndex: 'originName', 
      width: 140, 
      render: (_, r) => (
        <div className="text-[10px] text-gray-500">
          <span className="font-medium">{r.originName || '-'}</span>
          <span className="mx-1">→</span>
          <span className="font-medium">{r.destinationName || '-'}</span>
        </div>
      )
    },
    { 
      title: '1st Wt', 
      dataIndex: 'firstWeight', 
      width: 75, 
      align: 'right', 
      render: (w) => <span className="text-[11px] font-semibold text-blue-600">{w ? `${(w/1000).toFixed(1)}t` : '-'}</span> 
    },
    { 
      title: '2nd Wt', 
      dataIndex: 'secondWeight', 
      width: 75, 
      align: 'right', 
      render: (w) => <span className="text-[11px] font-semibold text-green-600">{w ? `${(w/1000).toFixed(1)}t` : '-'}</span> 
    },
    { 
      title: 'Net Wt', 
      dataIndex: 'netWeight', 
      width: 80, 
      align: 'right', 
      render: (w) => <span className="text-[11px] font-bold text-orange-600">{w ? `${(w/1000).toFixed(1)}t` : '-'}</span> 
    },
    { 
      title: 'Status', 
      dataIndex: 'status', 
      width: 85, 
      render: (s) => (
        <Tag 
          color={s === 'Completed' ? '#10b981' : '#f59e0b'} 
          className="text-[10px] font-bold px-2 py-0.5 rounded-full border-0 m-0"
        >
          {s?.toUpperCase()}
        </Tag>
      ) 
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
            <div className="text-[10px] text-gray-500">{total || 0} records</div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end">
          <Input 
            allowClear 
            placeholder="Search..." 
            prefix={<Search size={14} className="text-gray-400" />} 
            className="w-56 h-8 text-xs rounded-lg" 
            value={filters.search}
            onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 1 })} 
          />
          <Button 
            icon={<Filter size={14} />} 
            className="h-8 text-xs rounded-lg"
            onClick={() => setShowFilters(!showFilters)}
          >
            {showFilters ? 'Hide' : 'Filters'}
          </Button>
          <Button 
            type="primary" 
            icon={<ReloadOutlined />}
            className="bg-gradient-to-r from-amber-500 to-amber-600 border-0 h-8 text-xs rounded-lg" 
            onClick={() => {
              setFilters({ ...filters, page: 1 });
              loadTransactions();
            }}
            loading={loading}
          >
            Refresh
          </Button>
        </div>
      </div>

      {/* Collapsible Filters */}
      {showFilters && (
        <div className="bg-white border-b px-4 py-2 flex items-center gap-3 shrink-0">
          <span className="text-xs text-gray-600 font-medium">Date Range:</span>
          <RangePicker 
            className="h-7 text-xs rounded-lg" 
            value={filters.dateRange}
            onChange={(d) => {
              setFilters({ ...filters, dateRange: d, page: 1 });
            }} 
            size="small"
            format="DD-MM-YYYY"
          />
          {(filters.search || filters.dateRange) && (
            <Button 
              size="small"
              onClick={() => {
                setFilters({ search: "", dateRange: null, page: 1, pageSize: filters.pageSize });
              }}
              className="h-7 text-xs"
            >
              Clear All
            </Button>
          )}
        </div>
      )}

      {/* Compact Table */}
      <div className="flex-1 overflow-hidden px-4 pb-4 pt-3">
        <div className="h-full bg-white rounded-lg border shadow-sm overflow-hidden">
          <Table 
            columns={columns} 
            dataSource={transactions} 
            rowKey="id" 
            loading={loading} 
            size="small" 
            className="compact-table" 
            scroll={{ y: "calc(100vh - 180px)", x: 1200 }}
            pagination={{ 
              current: filters.page, 
              pageSize: filters.pageSize, 
              total, 
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
                  { label: 'Axle Type', field: 'axleType' },
                  { label: 'Container', field: 'containerNo' },
                  { label: 'Seal', field: 'sealNo' },
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
                    {selectedRecord.firstWeightOperator || 'N/A'}<br/>
                    {selectedRecord.firstWeightTime ? dayjs(selectedRecord.firstWeightTime).format("DD-MM-YY HH:mm") : 'N/A'}
                  </div>
                </div>
                <div className="bg-white rounded p-2">
                  <div className="text-gray-600 mb-1">Second Weight</div>
                  <div className="text-lg font-bold text-green-600">{selectedRecord.secondWeight || 0} kg</div>
                  <div className="text-[10px] text-gray-500 mt-1">
                    {selectedRecord.secondWeightOperator || 'N/A'}<br/>
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
                    <Tag color={selectedRecord.status === 'Completed' ? '#10b981' : '#f59e0b'} className="font-bold">
                      {selectedRecord.status?.toUpperCase()}
                    </Tag>
                  )}
                </div>
                <div>
                  <div className="text-gray-600 font-medium mb-1">Remarks</div>
                  {isEditing ? (
                    <Input.TextArea 
                      value={editedRecord.remarks} 
                      onChange={(e) => setEditedRecord({...editedRecord, remarks: e.target.value})} 
                      rows={3}
                      className="text-xs"
                    />
                  ) : (
                    <div className="text-gray-900 bg-white p-2 rounded border">{selectedRecord.remarks || 'No remarks'}</div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </Drawer>

      <style>{`
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, #fafafa, #f5f5f5) !important;
          border-bottom: 2px solid #f59e0b !important;
          padding: 8px 12px !important;
          font-weight: 700 !important;
          font-size: 10px !important;
          text-transform: uppercase;
          letter-spacing: 0.3px;
        }
        .compact-table .ant-table-tbody > tr > td {
          padding: 6px 12px !important;
          border-bottom: 1px solid #f0f0f0 !important;
        }
        .compact-table .ant-table-tbody > tr:hover > td {
          background: #fffbeb !important;
        }
        .compact-pagination .ant-pagination-item,
        .compact-pagination .ant-pagination-prev,
        .compact-pagination .ant-pagination-next {
          min-width: 26px !important;
          height: 26px !important;
          line-height: 24px !important;
          font-size: 12px !important;
        }
        .compact-pagination .ant-select-selector {
          height: 26px !important;
          font-size: 12px !important;
        }
      `}</style>
    </div>
  );
}