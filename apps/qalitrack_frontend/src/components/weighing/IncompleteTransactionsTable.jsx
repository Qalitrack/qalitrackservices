import React, { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, Typography, Space, Pagination, message, Spin, Tooltip, Popover, Badge } from "antd";
import { ReloadOutlined, SearchOutlined, CheckOutlined, CloseOutlined, EditOutlined, UserOutlined, ClockCircleOutlined, EyeOutlined, RetweetOutlined } from "@ant-design/icons";
import { fetchTransactions, updateTransactionApi } from "../../store/weighingSlice";
import ReweighModal from "./ReweighModal";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import duration from "dayjs/plugin/duration";

dayjs.extend(relativeTime);
dayjs.extend(duration);

const { Text } = Typography;

// ✨ Column definitions with visibility config
const COLUMN_DEFINITIONS = [
  { key: "createdAt",       label: "Date",        defaultVisible: true,  alwaysVisible: false },
  { key: "receiptNo",       label: "Receipt",     defaultVisible: true,  alwaysVisible: false },
  { key: "noPlate",         label: "Vehicle",     defaultVisible: true,  alwaysVisible: false },
  { key: "driverName",      label: "Driver",      defaultVisible: true,  alwaysVisible: false },
  { key: "commodityName",   label: "Commodity",   defaultVisible: true,  alwaysVisible: false },
  { key: "supplierName",    label: "Supplier",    defaultVisible: true,  alwaysVisible: false },
  { key: "transporterName", label: "Transporter", defaultVisible: true,  alwaysVisible: false },
  { key: "customerName",    label: "Customer",    defaultVisible: false, alwaysVisible: false },
  { key: "originName",      label: "Origin",      defaultVisible: false, alwaysVisible: false },
  { key: "destinationName", label: "Destination", defaultVisible: false, alwaysVisible: false },
  { key: "operatorName",    label: "Operator",    defaultVisible: true,  alwaysVisible: false },
  { key: "weighBridgeName", label: "Weighbridge", defaultVisible: true,  alwaysVisible: false },
  { key: "scaleName",       label: "Scale",       defaultVisible: true,  alwaysVisible: false },
  { key: "firstWeight",     label: "1st Weight",  defaultVisible: true,  alwaysVisible: false },
  { key: "wait",            label: "Wait",        defaultVisible: true,  alwaysVisible: false },
  { key: "tat",             label: "TAT",         defaultVisible: true,  alwaysVisible: false },
  { key: "nprSource",        label: "NPR",         defaultVisible: true,  alwaysVisible: false },
  { key: "status",          label: "Status",      defaultVisible: true,  alwaysVisible: false },
  { key: "action",          label: "Actions",     defaultVisible: true,  alwaysVisible: true  },
];

const STORAGE_KEY = "incompleteTransactions_columnVisibility";

function loadColumnVisibility() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) return JSON.parse(saved);
  } catch (_) {}
  return COLUMN_DEFINITIONS.reduce((acc, col) => {
    acc[col.key] = col.defaultVisible;
    return acc;
  }, {});
}

function saveColumnVisibility(visibility) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(visibility));
  } catch (_) {}
}

export default function IncompleteTransactionsTable({ onAddWeighing }) {
  const dispatch = useDispatch();

  const transactions = useSelector((state) => state.weighing?.transactions) || [];
  const loading = useSelector((state) => state.weighing?.loading) || false;
  const serverTotal = useSelector((state) => state.weighing?.total) || 0;

  const currentUser = useSelector((state) => state.auth?.user);

  const [searchText, setSearchText] = useState("");
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [editingKey, setEditingKey] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [saving, setSaving] = useState(false);
  const [localLoading, setLocalLoading] = useState(false);

  // Reweigh modal state
  const [reweighModal, setReweighModal] = useState({ visible: false, transaction: null });

  // ✨ Column visibility state — persisted to localStorage
  const [columnVisibility, setColumnVisibility] = useState(loadColumnVisibility);
  const [columnPanelOpen, setColumnPanelOpen] = useState(false);

  const hiddenCount = COLUMN_DEFINITIONS.filter(
    (col) => !col.alwaysVisible && !columnVisibility[col.key]
  ).length;

  const visibleCount = COLUMN_DEFINITIONS.filter(
    (col) => col.alwaysVisible || columnVisibility[col.key]
  ).length;

  const toggleColumn = (key) => {
    const col = COLUMN_DEFINITIONS.find((c) => c.key === key);
    if (col?.alwaysVisible) return;
    setColumnVisibility((prev) => {
      const next = { ...prev, [key]: !prev[key] };
      saveColumnVisibility(next);
      return next;
    });
  };

  const showAll = () => {
    const next = COLUMN_DEFINITIONS.reduce((acc, col) => { acc[col.key] = true; return acc; }, {});
    setColumnVisibility(next);
    saveColumnVisibility(next);
  };

  const resetDefaults = () => {
    const next = COLUMN_DEFINITIONS.reduce((acc, col) => { acc[col.key] = col.defaultVisible; return acc; }, {});
    setColumnVisibility(next);
    saveColumnVisibility(next);
  };

  const loadTransactions = useCallback(() => {
    setLocalLoading(true);
    const params = { pageNumber: pagination.current, pageSize: pagination.pageSize, isCompleted: false };
    dispatch(fetchTransactions(params))
      .unwrap()
      .then((data) => {
        setLocalLoading(false);
      })
      .catch((error) => {
        message.error("Failed to load transactions: " + error);
        setLocalLoading(false);
      });
  }, [dispatch, pagination.current, pagination.pageSize]);

  useEffect(() => { loadTransactions(); }, [loadTransactions]);

  useEffect(() => {
  }, [transactions]);

  useEffect(() => {
    if (searchText && pagination.current !== 1) setPagination(prev => ({ ...prev, current: 1 }));
  }, [searchText, pagination.current]);

  const calculateTurnaroundTime = (createdAt) => {
    if (!createdAt) return { display: '-', minutes: 0 };
    const diffMinutes = dayjs().diff(dayjs(createdAt), 'minute');
    if (diffMinutes < 1) return { display: '< 1m', minutes: 0 };
    if (diffMinutes < 60) return { display: `${diffMinutes}m`, minutes: diffMinutes };
    const hours = Math.floor(diffMinutes / 60);
    const mins = diffMinutes % 60;
    return { display: mins > 0 ? `${hours}h ${mins}m` : `${hours}h`, minutes: diffMinutes };
  };

  const getTurnaroundColor = (minutes) => {
    if (minutes < 30) return 'green';
    if (minutes < 60) return 'orange';
    return 'red';
  };

  const filteredData = useMemo(() => {
    // Always exclude completed transactions from the active queue
    let data = transactions.filter(tx => {
      const s = tx.status?.toLowerCase();
      return !tx.isCompleted && !tx.completed && s !== 'completed';
    });

    if (searchText?.trim()) {
      const q = searchText.toLowerCase().trim();
      data = data.filter(tx =>
        tx.receiptNo?.toLowerCase().includes(q) ||
        tx.noPlate?.toLowerCase().includes(q) ||
        tx.driverName?.toLowerCase().includes(q) ||
        tx.commodityName?.toLowerCase().includes(q) ||
        tx.supplierName?.toLowerCase().includes(q) ||
        tx.transporterName?.toLowerCase().includes(q) ||
        tx.customerName?.toLowerCase().includes(q) ||
        tx.originName?.toLowerCase().includes(q) ||
        tx.destinationName?.toLowerCase().includes(q) ||
        tx.operatorName?.toLowerCase().includes(q)
      );
    }
    data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    return data;
  }, [transactions, searchText]);

  useEffect(() => { setPagination(prev => ({ ...prev, total: serverTotal })); }, [serverTotal]);

  const isEditing = (record) => record.ticketID === editingKey || record.id === editingKey;
  const edit = (record) => { setEditingKey(record.ticketID || record.id); setEditedData({ ...record }); };
  const cancel = () => { setEditingKey(null); setEditedData({}); };

  const save = async (ticketId) => {
    try {
      setSaving(true);
      const dataToUpdate = {
        receiptNo: editedData.receiptNo, noPlate: editedData.noPlate?.toUpperCase(),
        driverName: editedData.driverName, vehicleID: editedData.vehicleID,
        driverID: editedData.driverID, transporterID: editedData.transporterID,
        commodityID: editedData.commodityID, supplierID: editedData.supplierID,
        customerID: editedData.customerID, weighBridgeID: editedData.weighBridgeID,
        operatorID: editedData.operatorID, originID: editedData.originID,
        destinationID: editedData.destinationID, transporterName: editedData.transporterName,
        commodityName: editedData.commodityName, supplierName: editedData.supplierName,
        customerName: editedData.customerName, originName: editedData.originName,
        destinationName: editedData.destinationName, weighBridgeName: editedData.weighBridgeName,
        operatorName: editedData.operatorName, firstWeight: editedData.firstWeight,
        status: editedData.status, weighMode: editedData.weighMode,
        operation: editedData.operation, scaleName: editedData.scaleName, notes: editedData.notes,
      };
      await dispatch(updateTransactionApi({ ticketId, data: dataToUpdate })).unwrap();
      message.success('✓ Updated successfully');
      setEditingKey(null); setEditedData({});
      loadTransactions();
    } catch (error) {
      message.error('Failed to update: ' + (error.message || 'Unknown error'));
    } finally {
      setSaving(false);
    }
  };

  const handleFieldChange = (field, value) => setEditedData(prev => ({ ...prev, [field]: value }));

  const EditableCell = ({ editing, dataIndex, record, children, ...restProps }) => (
    <td {...restProps}>
      {editing ? (
        <Input value={editedData[dataIndex]} onChange={(e) => handleFieldChange(dataIndex, e.target.value)}
          size="small" className="h-6 text-[11px] rounded border-amber-300 focus:border-amber-500" />
      ) : children}
    </td>
  );

  const calculateWaitTime = (createdAt) => createdAt ? dayjs(createdAt).fromNow() : '-';

  const handleSecondWeighing = (record) => {
    const transactionId = record.ticketID || record.id;
    if (!transactionId) { message.error("Cannot start second weighing - transaction ID missing"); return; }
    onAddWeighing({ ...record, id: transactionId, ticketID: transactionId, firstWeight: record.firstWeight?.toString() || "", secondWeight: "" });
  };

  // ─── All column definitions ───────────────────────────────────────────────
  const allColumns = [
    {
      key: "createdAt", title: 'Date', dataIndex: 'createdAt', width: 75,
      render: (date) => (
        <div className="flex flex-col leading-tight">
          <Text className="text-[10px] font-semibold text-gray-800">{date ? dayjs(date).format('DD MMM') : '-'}</Text>
          <Text className="text-[9px] text-gray-500">{date ? dayjs(date).format('HH:mm') : ''}</Text>
        </div>
      ),
    },
    {
      key: "receiptNo", title: 'Receipt', dataIndex: 'receiptNo', width: 85, editable: true,
      render: (text) => <Text className="text-[10px] font-mono font-semibold text-amber-700">{text || '-'}</Text>,
    },
    {
      key: "noPlate", title: 'Vehicle', dataIndex: 'noPlate', width: 75, editable: true,
      render: (text) => <span className="text-[10px] font-bold text-gray-800">{text || '-'}</span>,
    },
    {
      key: "driverName", title: 'Driver', dataIndex: 'driverName', width: 90, editable: true,
      render: (text) => <Text className="text-[10px] font-medium text-gray-700">{text || '-'}</Text>,
    },
    {
      key: "commodityName", title: 'Commodity', dataIndex: 'commodityName', width: 90, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "supplierName", title: 'Supplier', dataIndex: 'supplierName', width: 90, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "transporterName", title: 'Transporter', dataIndex: 'transporterName', width: 95, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "customerName", title: 'Customer', dataIndex: 'customerName', width: 90, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "originName", title: 'Origin', dataIndex: 'originName', width: 80, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "destinationName", title: 'Destination', dataIndex: 'destinationName', width: 90, editable: true,
      render: (text) => <Text className="text-[10px] text-gray-600">{text || '-'}</Text>,
    },
    {
      key: "operatorName", title: 'Operator', dataIndex: 'operatorName', width: 95,
      render: (text) => {
        const name = text || currentUser?.fullName || currentUser?.name || currentUser?.username || 'N/A';
        return (
          <div className="flex items-center gap-1.5">
            <div className="w-5 h-5 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center text-white text-[9px] font-bold shadow-sm">
              {name !== 'N/A' ? name.charAt(0).toUpperCase() : <UserOutlined className="text-[8px]" />}
            </div>
            <Text className="text-[10px] text-gray-700 font-medium truncate max-w-[55px]">{name}</Text>
          </div>
        );
      },
    },
    {
      key: "weighBridgeName", title: 'Weighbridge', dataIndex: 'weighBridgeName', width: 95, editable: true,
      render: (text) => (
        <Text className="text-[10px] font-medium text-gray-700">{text || '-'}</Text>
      ),
    },
    {
      key: "scaleName", title: 'Scale', dataIndex: 'scaleName', width: 80, editable: true,
      render: (text) => (
        <span className="inline-flex items-center gap-1">
          <span className="w-1.5 h-1.5 rounded-full bg-amber-400 flex-shrink-0" />
          <Text className="text-[10px] font-medium text-amber-700">{text || '-'}</Text>
        </span>
      ),
    },
    {
      key: "firstWeight", title: '1st Weight', dataIndex: 'firstWeight', width: 80, align: 'right',
      render: (weight) => (
        <div className="flex flex-col items-end leading-tight">
          <Text className="text-[10px] font-semibold text-amber-600">{weight ? `${parseFloat(weight).toLocaleString()}` : '-'}</Text>
          <Text className="text-[8px] text-gray-500">kg</Text>
        </div>
      ),
    },
    {
      key: "wait", title: 'Wait', dataIndex: 'createdAt', width: 70,
      render: (date) => (
        <Tag color="orange" className="text-[9px] font-medium px-1.5 py-0 rounded-full border-0 m-0">{calculateWaitTime(date)}</Tag>
      ),
    },
    {
      key: "tat", title: <span><ClockCircleOutlined className="mr-1" />TAT</span>, dataIndex: 'createdAt', width: 65,
      render: (date) => {
        const tat = calculateTurnaroundTime(date);
        return <Tag color={getTurnaroundColor(tat.minutes)} className="text-[9px] font-bold px-1.5 py-0 rounded-full border-0 m-0">{tat.display}</Tag>;
      },
    },
    {
      key: "nprSource", title: 'NPR', dataIndex: 'nprSource', width: 65,
      render: (val) => {
        const isAuto = val === "auto";
        return (
          <Tag
            color={isAuto ? "green" : "blue"}
            className="text-[9px] font-semibold px-1.5 py-0 rounded-full border-0 m-0"
          >
            {isAuto ? "Auto" : "Manual"}
          </Tag>
        );
      },
    },
    {
      key: "status", title: 'Status', dataIndex: 'status', width: 95,
      render: (status, record) => {
        if (isEditing(record)) {
          return <Input value={editedData.status} onChange={(e) => handleFieldChange('status', e.target.value)}
            size="small" className="h-6 text-[10px] rounded border-amber-300" />;
        }
        const s = status?.toLowerCase();
        if (s === 'reweighrequested') {
          return <Tag color="blue" className="text-[9px] font-semibold px-2 py-0 rounded-full border-0 m-0">Reweigh Req.</Tag>;
        }
        if (s === 'completed') {
          return <Tag color="green" className="text-[9px] font-semibold px-2 py-0 rounded-full border-0 m-0">Completed</Tag>;
        }
        return (
          <Tag
            style={{ backgroundColor: 'var(--cs-500)', borderColor: 'var(--cs-500)', color: 'white' }}
            className="text-[9px] font-semibold px-2 py-0 rounded-full border-0 m-0"
          >
            {status || 'Active'}
          </Tag>
        );
      },
    },
    {
      key: "action", title: 'Actions', dataIndex: 'action', width: 165, fixed: 'right',
      render: (_, record) => {
        const editable = isEditing(record);
        const recordId = record.ticketID || record.id;
        const txStatus = record.status?.toLowerCase();
        const isReweighRequested = txStatus === 'reweighrequested';
        const isCompleted =
          record.isCompleted === true || record.completed === true ||
          txStatus === 'completed';

        if (editable) {
          return (
            <Space size={4}>
              <Button type="text" size="small" icon={<CheckOutlined className="text-[10px]" />} onClick={() => save(recordId)} loading={saving}
                className="text-green-600 hover:text-green-700 hover:bg-green-50 h-6 px-2 text-[10px] font-medium">Save</Button>
              <Button type="text" size="small" icon={<CloseOutlined className="text-[10px]" />} onClick={cancel}
                className="text-gray-600 hover:text-gray-700 hover:bg-gray-100 h-6 px-2 text-[10px]">Cancel</Button>
            </Space>
          );
        }

        // ReweighRequested — Approve / Reject
        if (isReweighRequested) {
          return (
            <Space size={4}>
              <Button
                size="small"
                icon={<RetweetOutlined className="text-[9px]" />}
                onClick={() => setReweighModal({ visible: true, transaction: record })}
                className="bg-blue-500 hover:bg-blue-600 border-0 text-white text-[10px] font-semibold h-6 px-2 shadow-sm"
              >
                Review
              </Button>
            </Space>
          );
        }

        // Completed — Request Reweigh
        if (isCompleted) {
          return (
            <Space size={4}>
              <Button
                size="small"
                icon={<RetweetOutlined className="text-[9px]" />}
                onClick={() => setReweighModal({ visible: true, transaction: record })}
                className="text-blue-600 hover:text-blue-700 hover:bg-blue-50 border-blue-200 h-6 px-2 text-[10px] font-medium"
              >
                Reweigh
              </Button>
            </Space>
          );
        }

        // Active / Pending — normal weighing flow
        return (
          <Space size={4}>
            <Button type="text" size="small" icon={<EditOutlined className="text-[9px]" />} onClick={() => edit(record)}
              className="text-amber-600 hover:text-amber-700 hover:bg-amber-50 h-6 px-1.5 text-[10px] font-medium">Edit</Button>
            <Button type="primary" size="small" onClick={() => handleSecondWeighing(record)}
              className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0 text-[10px] font-semibold h-6 px-2 shadow-sm">
              2nd Weight
            </Button>
          </Space>
        );
      },
    },
  ];

  // ✨ Filter to only visible columns
  const columns = allColumns.filter((col) => {
    const def = COLUMN_DEFINITIONS.find((d) => d.key === col.key);
    return def?.alwaysVisible || columnVisibility[col.key];
  });

  const mergedColumns = columns.map((col) => {
    if (!col.editable) return col;
    return { ...col, onCell: (record) => ({ record, dataIndex: col.dataIndex, editing: isEditing(record) }) };
  });

  // ✨ Column visibility popover panel
  const columnPanelContent = (
    <div className="w-52">
      {/* Header row */}
      <div className="flex items-center justify-between mb-2 pb-2 border-b border-gray-100">
        <Text className="text-[10px] font-bold text-gray-600 uppercase tracking-wide">Columns</Text>
        <Space size={4}>
          <button onClick={showAll} className="text-[10px] text-amber-600 hover:text-amber-700 font-medium bg-none border-none cursor-pointer p-0">All</button>
          <span className="text-gray-300 text-[10px]">|</span>
          <button onClick={resetDefaults} className="text-[10px] text-gray-400 hover:text-gray-600 font-medium bg-none border-none cursor-pointer p-0">Default</button>
        </Space>
      </div>

      {/* Column list */}
      <div className="flex flex-col gap-0.5 max-h-72 overflow-y-auto pr-1">
        {COLUMN_DEFINITIONS.map((col) => {
          const visible = col.alwaysVisible || columnVisibility[col.key];
          const locked = col.alwaysVisible;
          return (
            <div
              key={col.key}
              onClick={() => !locked && toggleColumn(col.key)}
              className={`flex items-center gap-2 px-2 py-1.5 rounded-md transition-colors select-none ${
                locked ? 'opacity-40 cursor-not-allowed' : 'cursor-pointer hover:bg-amber-50'
              }`}
            >
              {/* Custom checkbox */}
              <div className={`w-3.5 h-3.5 rounded border-[1.5px] flex-shrink-0 flex items-center justify-center transition-all ${
                visible ? 'bg-amber-500 border-amber-500' : 'bg-white border-gray-300'
              }`}>
                {visible && (
                  <svg viewBox="0 0 8 8" className="w-2 h-2" fill="none">
                    <path d="M1 4l2 2 4-4" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                )}
              </div>

              {/* Label */}
              <Text className={`text-[11px] flex-1 ${visible ? 'text-gray-800 font-medium' : 'text-gray-400'}`}>
                {col.label}
              </Text>

              {locked && (
                <span className="text-[8px] text-gray-300 bg-gray-100 px-1 py-0.5 rounded font-medium">
                  locked
                </span>
              )}
            </div>
          );
        })}
      </div>

      {/* Footer */}
      <div className="mt-2 pt-2 border-t border-gray-100 flex items-center justify-between">
        <Text className="text-[10px] text-gray-400">
          {visibleCount}/{COLUMN_DEFINITIONS.length} shown
        </Text>
        {hiddenCount > 0 && (
          <span className="text-[10px] text-amber-600 font-semibold">{hiddenCount} hidden</span>
        )}
      </div>
    </div>
  );

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-amber-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-sm">
              <span className="text-white text-sm font-bold">{filteredData.length}</span>
            </div>
            <div>
              <Text className="text-[11px] font-bold text-gray-900 block leading-tight">Transaction Queue</Text>
              <Text className="text-[9px] text-amber-700 font-medium">TAT = Turnaround Time</Text>
            </div>

          </div>

          <Space size="small">
            <Input
              placeholder="Search..."
              prefix={<SearchOutlined className="text-gray-400 text-[10px]" />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              className="w-44 h-7 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
              allowClear
            />

            {/* ✨ Columns toggle button */}
            <Popover
              content={columnPanelContent}
              trigger="click"
              placement="bottomRight"
              open={columnPanelOpen}
              onOpenChange={setColumnPanelOpen}
              overlayInnerStyle={{ padding: '10px 10px 8px', borderRadius: '10px', boxShadow: '0 6px 20px rgba(0,0,0,0.12)', minWidth: 0 }}
            >
              <Tooltip title="Manage columns">
                <Badge count={hiddenCount} size="small" color="var(--cs-500)" offset={[-2, 2]}>
                  <Button
                    icon={<EyeOutlined className="text-[11px]" />}
                    className={`h-7 px-2.5 text-[11px] rounded-md shadow-sm font-medium transition-colors ${
                      columnPanelOpen
                        ? 'border-amber-500 text-amber-600 bg-amber-50'
                        : 'border-gray-300 hover:border-amber-500 hover:text-amber-600'
                    }`}
                  >
                    Columns
                  </Button>
                </Badge>
              </Tooltip>
            </Popover>

            <Button
              icon={<ReloadOutlined className="text-[11px]" />}
              onClick={() => { setSearchText(""); setPagination(prev => ({ ...prev, current: 1 })); loadTransactions(); }}
              loading={loading || localLoading}
              className="h-7 px-3 text-[11px] rounded-md border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm font-medium"
            >
              Refresh
            </Button>
          </Space>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-auto bg-white">
        <Spin spinning={loading || localLoading} tip="Loading..." size="small">
          <Table
            components={{ body: { cell: EditableCell } }}
            rowKey={(record) => record.ticketID || record.id}
            columns={mergedColumns}
            dataSource={filteredData}
            loading={false}
            pagination={false}
            size="small"
            className="compact-table"
            rowClassName={(record) => {
              const tat = calculateTurnaroundTime(record.createdAt);
              const editing = editingKey === (record.ticketID || record.id);
              if (editing) return 'editing-row';
              if (tat.minutes > 60) return 'urgent-row';
              return 'regular-row';
            }}
            scroll={{ x: 1500, y: 'calc(100vh - 250px)' }}
            locale={{
              emptyText: (
                <div className="py-8">
                  {searchText ? (
                    <div className="text-gray-500"><SearchOutlined className="text-2xl mb-2" /><p>No results for "{searchText}"</p></div>
                  ) : transactions.length === 0 ? (
                    <div className="text-gray-500">
                      <p className="font-semibold mb-2">No transactions found</p>
                      <p className="text-xs">Click "Refresh" or create a new transaction</p>
                    </div>
                  ) : (
                    <div className="text-green-600">
                      <CheckOutlined className="text-2xl mb-2" />
                      <p className="font-semibold">All transactions completed!</p>
                      <p className="text-xs text-gray-500">No pending transactions</p>
                    </div>
                  )}
                </div>
              )
            }}
          />
        </Spin>
      </div>

      {/* Footer */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <Text className="text-[10px] text-gray-600 font-medium">
          {searchText ? (
            <>
              <span className="font-semibold text-amber-600">{filteredData.length}</span> found
              <span className="text-gray-400 mx-1">·</span>
              <span className="text-gray-500">filtered</span>
            </>
          ) : (
            <>
              <span className="font-semibold text-amber-600">{filteredData.length}</span>
              {' active'}
              <span className="text-gray-400 mx-2">•</span>
              <span className="text-green-600">🟢 &lt;30m</span>
              <span className="text-gray-400 mx-1">•</span>
              <span className="text-amber-600">🟠 30-60m</span>
              <span className="text-gray-400 mx-1">•</span>
              <span className="text-red-600">🔴 &gt;60m</span>
              {hiddenCount > 0 && (
                <>
                  <span className="text-gray-400 mx-2">•</span>
                  <span className="text-amber-600 cursor-pointer hover:underline" onClick={() => setColumnPanelOpen(true)}>
                    {hiddenCount} column{hiddenCount > 1 ? 's' : ''} hidden
                  </span>
                </>
              )}
            </>
          )}
        </Text>
        <Pagination
          current={pagination.current}
          pageSize={pagination.pageSize}
          total={pagination.total}
          onChange={(page, pageSize) => setPagination({ ...pagination, current: page, pageSize })}
          showSizeChanger
          showTotal={(total) => `${total} total`}
          size="small"
          className="compact-pagination"
        />
      </div>

      {/* Reweigh Modal */}
      <ReweighModal
        visible={reweighModal.visible}
        transaction={reweighModal.transaction}
        onClose={() => setReweighModal({ visible: false, transaction: null })}
        onSuccess={() => {
          setReweighModal({ visible: false, transaction: null });
          loadTransactions();
        }}
      />

      <style>{`
        .compact-table .ant-table { background: white; font-size: 10px; }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, var(--cs-50), var(--cs-100)) !important;
          border-bottom: 1.5px solid var(--cs-500) !important;
          padding: 6px 8px !important; font-weight: 700 !important; font-size: 9px !important;
          color: var(--cs-900) !important; text-transform: uppercase; letter-spacing: 0.3px; line-height: 1.2;
        }
        .compact-table .ant-table-tbody > tr.regular-row > td {
          padding: 6px 8px !important; border-bottom: 1px solid #f3f4f6 !important;
          background: white !important; transition: all 0.12s ease; line-height: 1.3;
        }
        .compact-table .ant-table-tbody > tr.regular-row:hover > td {
          background: var(--cs-50) !important; box-shadow: inset 0 0 0 1px var(--cs-100);
        }
        .compact-table .ant-table-tbody > tr.urgent-row > td {
          padding: 6px 8px !important; border-bottom: 1px solid #f3f4f6 !important;
          background: #fef2f2 !important; transition: all 0.12s ease; line-height: 1.3;
        }
        .compact-table .ant-table-tbody > tr.urgent-row:hover > td {
          background: #fee2e2 !important; box-shadow: inset 0 0 0 1px #fecaca;
        }
        .compact-table .ant-table-tbody > tr.editing-row > td {
          padding: 6px 8px !important; background: var(--cs-100) !important;
          border-bottom: 1px solid var(--cs-300) !important; box-shadow: inset 0 1px 2px rgba(0,0,0,0.06);
        }
        .compact-table .ant-spin-container { min-height: 200px; }
        .compact-pagination .ant-pagination-item {
          border-radius: 4px; border-color: #e5e7eb; font-weight: 500; font-size: 11px;
          min-width: 24px; height: 24px; line-height: 22px; margin: 0 2px;
        }
        .compact-pagination .ant-pagination-item-active {
          background: linear-gradient(135deg, var(--cs-500), var(--cs-600)); border-color: var(--cs-500);
          box-shadow: 0 1px 3px rgba(0,0,0,0.15);
        }
        .compact-pagination .ant-pagination-item-active a { color: white !important; font-weight: 700; }
        .compact-pagination .ant-pagination-item:hover { border-color: var(--cs-500); }
        .compact-pagination .ant-pagination-item:hover a { color: var(--cs-500); }
        .compact-pagination .ant-pagination-options { font-size: 11px; }
        .compact-pagination .ant-select-selector { height: 24px !important; font-size: 11px !important; }
        .compact-pagination .ant-pagination-total-text { font-size: 10px; }
        .compact-pagination .ant-pagination-prev, .compact-pagination .ant-pagination-next {
          min-width: 24px; height: 24px; line-height: 22px; font-size: 11px;
        }
      `}</style>
    </div>
  );
}