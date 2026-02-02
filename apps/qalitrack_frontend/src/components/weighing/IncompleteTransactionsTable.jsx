// ✅ FIXED: Enhanced transaction fetching with better logging
// Changes:
// 1. Added detailed console logging for debugging empty data
// 2. Improved Redux state reading
// 3. Better error handling for empty responses
// 4. Enhanced filtering with completion status checks

import React, { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, Typography, Space, Pagination, message, Spin } from "antd";
import { ReloadOutlined, SearchOutlined, CheckOutlined, CloseOutlined, EditOutlined, UserOutlined, ClockCircleOutlined } from "@ant-design/icons";
import { fetchTransactions, updateTransactionApi } from "../../store/weighingSlice";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import duration from "dayjs/plugin/duration";

dayjs.extend(relativeTime);
dayjs.extend(duration);

const { Text } = Typography;

export default function IncompleteTransactionsTable({ onAddWeighing }) {
  const dispatch = useDispatch();
  
  // ✅ ENHANCED: Better Redux state reading with fallbacks
  const { transactions = [], loading } = useSelector((state) => {
    console.log("📊 Redux State - Full weighing state:", state.weighing);
    return {
      transactions: state.weighing?.transactions || [],
      loading: state.weighing?.loading || false
    };
  });
  
  const currentUser = useSelector((state) => state.auth?.user);

  const [searchText, setSearchText] = useState("");
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [editingKey, setEditingKey] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [saving, setSaving] = useState(false);
  const [localLoading, setLocalLoading] = useState(false);

  const searchTimeoutRef = useRef(null);

  const loadTransactions = useCallback(() => {
    console.log("");
    console.log("🔄 ========== LOADING INCOMPLETE TRANSACTIONS ==========");
    setLocalLoading(true);
    
    const params = {
      isCompleted: false,
      pageNumber: pagination.current,
      pageSize: pagination.pageSize,
    };
    
    console.log("📤 Fetching with params:", params);
    
    dispatch(fetchTransactions(params))
      .unwrap()
      .then((data) => {
        console.log("✅ ========== FETCH SUCCESS ==========");
        console.log("📊 Data type:", typeof data);
        console.log("📊 Is Array:", Array.isArray(data));
        console.log("📊 Length:", data?.length);
        console.log("📊 First 3 items:", data?.slice(0, 3));
        console.log("");
        
        if (!data || data.length === 0) {
          console.warn("⚠️ No incomplete transactions returned from API");
          console.warn("⚠️ This might mean:");
          console.warn("   - All transactions are completed");
          console.warn("   - API is returning data in unexpected format");
          console.warn("   - Filtering is too strict");
        }
        
        message.success(`Loaded ${data?.length || 0} incomplete transactions`);
        setLocalLoading(false);
      })
      .catch((error) => {
        console.error("❌ ========== FETCH FAILED ==========");
        console.error("❌ Error:", error);
        console.error("❌ Error message:", error.message);
        console.error("");
        
        message.error("Failed to load transactions: " + error);
        setLocalLoading(false);
      });
  }, [dispatch, pagination.current, pagination.pageSize]);

  useEffect(() => {
    loadTransactions();
  }, [loadTransactions]);

  // ✅ ENHANCED: Watch Redux transactions with detailed logging
  useEffect(() => {
    console.log("");
    console.log("📊 ========== REDUX TRANSACTIONS CHANGED ==========");
    console.log("📊 Total in Redux:", transactions.length);
    console.log("📊 Sample (first 2):", transactions.slice(0, 2));
    
    if (transactions.length === 0) {
      console.warn("⚠️ Redux transactions array is EMPTY");
      console.warn("⚠️ Check:");
      console.warn("   1. Is fetchTransactions dispatching correctly?");
      console.warn("   2. Is weighingSlice reducer updating state?");
      console.warn("   3. Is response data being extracted correctly?");
    }
    
    const incomplete = transactions.filter(tx => {
      const isCompleted = tx.isCompleted === true || 
                         tx.completed === true || 
                         tx.status === 'Completed' || 
                         tx.status === 'completed';
      return !isCompleted;
    });
    
    console.log("📊 Incomplete count:", incomplete.length);
    console.log("📊 Completed count:", transactions.length - incomplete.length);
    console.log("");
  }, [transactions]);

  // Reset to page 1 when searching
  useEffect(() => {
    if (searchText && pagination.current !== 1) {
      setPagination(prev => ({ ...prev, current: 1 }));
    }
  }, [searchText, pagination.current]);

  // ✅ Calculate turnaround time in minutes
  const calculateTurnaroundTime = (createdAt) => {
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

  // ✅ Get color based on turnaround time
  const getTurnaroundColor = (minutes) => {
    if (minutes < 30) return 'green';
    if (minutes < 60) return 'orange';
    return 'red';
  };

  // ✅ ENHANCED FILTERING with better logging
  const filteredData = useMemo(() => {
    console.log("🔍 ========== FILTERING TRANSACTIONS ==========");
    console.log("🔍 Input transactions:", transactions.length);
    
    // Filter out completed transactions
    let data = transactions.filter(tx => {
      const isCompleted = tx.isCompleted === true || 
                         tx.completed === true || 
                         tx.status === 'Completed' || 
                         tx.status === 'completed';
      
      if (isCompleted) {
        console.log("⏭️ Excluding completed:", tx.receiptNo || tx.ticketID);
      }
      
      return !isCompleted;
    });
    
    console.log("✅ After completion filter:", data.length);
    
    // Client-side search filtering
    if (searchText?.trim()) {
      const searchLower = searchText.toLowerCase().trim();
      const beforeSearch = data.length;
      
      data = data.filter(tx => 
        tx.receiptNo?.toLowerCase().includes(searchLower) ||
        tx.noPlate?.toLowerCase().includes(searchLower) ||
        tx.driverName?.toLowerCase().includes(searchLower) ||
        tx.commodityName?.toLowerCase().includes(searchLower) ||
        tx.supplierName?.toLowerCase().includes(searchLower) ||
        tx.transporterName?.toLowerCase().includes(searchLower) ||
        tx.customerName?.toLowerCase().includes(searchLower) ||
        tx.originName?.toLowerCase().includes(searchLower) ||
        tx.destinationName?.toLowerCase().includes(searchLower) ||
        tx.operatorName?.toLowerCase().includes(searchLower)
      );
      
      console.log(`✅ After search filter: ${data.length} (filtered out ${beforeSearch - data.length})`);
    }
    
    // Sort by creation date (newest first)
    data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    
    console.log("✅ Final filtered count:", data.length);
    console.log("");
    
    return data;
  }, [transactions, searchText]);

  useEffect(() => {
    setPagination(prev => ({ ...prev, total: filteredData.length }));
  }, [filteredData]);

  const isEditing = (record) => record.ticketID === editingKey || record.id === editingKey;

  const edit = (record) => {
    const recordId = record.ticketID || record.id;
    setEditingKey(recordId);
    setEditedData({ ...record });
  };

  const cancel = () => {
    setEditingKey(null);
    setEditedData({});
  };

  const save = async (ticketId) => {
    try {
      setSaving(true);
      
      const dataToUpdate = {
        receiptNo: editedData.receiptNo,
        noPlate: editedData.noPlate?.toUpperCase(),
        driverName: editedData.driverName,
        vehicleID: editedData.vehicleID,
        driverID: editedData.driverID,
        transporterID: editedData.transporterID,
        commodityID: editedData.commodityID,
        supplierID: editedData.supplierID,
        customerID: editedData.customerID,
        weighBridgeID: editedData.weighBridgeID,
        operatorID: editedData.operatorID,
        originID: editedData.originID,
        destinationID: editedData.destinationID,
        transporterName: editedData.transporterName,
        commodityName: editedData.commodityName,
        supplierName: editedData.supplierName,
        customerName: editedData.customerName,
        originName: editedData.originName,
        destinationName: editedData.destinationName,
        weighBridgeName: editedData.weighBridgeName,
        operatorName: editedData.operatorName,
        firstWeight: editedData.firstWeight,
        status: editedData.status,
        weighMode: editedData.weighMode,
        operation: editedData.operation,
        scaleName: editedData.scaleName,
        notes: editedData.notes,
      };

      console.log("📝 Updating transaction:", ticketId, dataToUpdate);

      await dispatch(updateTransactionApi({ ticketId, data: dataToUpdate })).unwrap();
      message.success('✓ Updated successfully');
      setEditingKey(null);
      setEditedData({});
      loadTransactions();
    } catch (error) {
      console.error("❌ Update failed:", error);
      message.error('Failed to update: ' + (error.message || 'Unknown error'));
    } finally {
      setSaving(false);
    }
  };

  const handleFieldChange = (field, value) => {
    setEditedData(prev => ({ ...prev, [field]: value }));
  };

  const EditableCell = ({ editing, dataIndex, record, children, ...restProps }) => {
    const inputNode = (
      <Input
        value={editedData[dataIndex]}
        onChange={(e) => handleFieldChange(dataIndex, e.target.value)}
        size="small"
        className="h-6 text-[11px] rounded border-amber-300 focus:border-amber-500"
      />
    );

    return (
      <td {...restProps}>
        {editing ? inputNode : children}
      </td>
    );
  };

  const calculateWaitTime = (createdAt) => {
    if (!createdAt) return '-';
    return dayjs(createdAt).fromNow();
  };

  const handleSecondWeighing = (record) => {
    console.log("🎯 Starting second weighing for:", record.receiptNo || record.ticketID);
    
    const transactionId = record.ticketID || record.id;
    
    if (!transactionId) {
      message.error("Cannot start second weighing - transaction ID missing");
      console.error("❌ Missing transaction ID in record:", record);
      return;
    }
    
    console.log("✅ Transaction ID for second weighing:", transactionId);
    
    onAddWeighing({
      ...record,
      id: transactionId,
      ticketID: transactionId,
      firstWeight: record.firstWeight?.toString() || "",
      secondWeight: "",
      vehicleID: record.vehicleID,
      driverID: record.driverID,
      transporterID: record.transporterID,
      commodityID: record.commodityID,
      supplierID: record.supplierID,
      customerID: record.customerID,
      weighBridgeID: record.weighBridgeID,
      operatorID: record.operatorID,
      originID: record.originID,
      destinationID: record.destinationID,
    });
  };

  const columns = [
    {
      title: 'Date',
      dataIndex: 'createdAt',
      width: 75,
      render: (date) => (
        <div className="flex flex-col leading-tight">
          <Text className="text-[10px] font-semibold text-gray-800">
            {date ? dayjs(date).format('DD MMM') : '-'}
          </Text>
          <Text className="text-[9px] text-gray-500">
            {date ? dayjs(date).format('HH:mm') : ''}
          </Text>
        </div>
      ),
    },
    {
      title: 'Receipt',
      dataIndex: 'receiptNo',
      width: 85,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] font-mono font-semibold text-amber-700">
          {text || '-'}
        </Text>
      ),
    },
    {
      title: 'Vehicle',
      dataIndex: 'noPlate',
      width: 75,
      editable: true,
      render: (text) => (
        <div className="inline-block bg-gray-900 text-white px-1.5 py-0.5 rounded text-[10px] font-bold">
          {text || '-'}
        </div>
      ),
    },
    {
      title: 'Driver',
      dataIndex: 'driverName',
      width: 90,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] font-medium text-gray-700">{text || '-'}</Text>
      ),
    },
    {
      title: 'Commodity',
      dataIndex: 'commodityName',
      width: 90,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Supplier',
      dataIndex: 'supplierName',
      width: 90,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Transporter',
      dataIndex: 'transporterName',
      width: 95,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Customer',
      dataIndex: 'customerName',
      width: 90,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Origin',
      dataIndex: 'originName',
      width: 80,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Destination',
      dataIndex: 'destinationName',
      width: 90,
      editable: true,
      render: (text) => (
        <Text className="text-[10px] text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Operator',
      dataIndex: 'operatorName',
      width: 95,
      render: (text) => {
        const operatorName = text || currentUser?.fullName || currentUser?.name || currentUser?.username || 'N/A';
        return (
          <div className="flex items-center gap-1.5">
            <div className="w-5 h-5 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white text-[9px] font-bold shadow-sm">
              {operatorName !== 'N/A' ? operatorName.charAt(0).toUpperCase() : <UserOutlined className="text-[8px]" />}
            </div>
            <Text className="text-[10px] text-gray-700 font-medium truncate max-w-[55px]">{operatorName}</Text>
          </div>
        );
      },
    },
    {
      title: '1st Weight',
      dataIndex: 'firstWeight',
      width: 80,
      align: 'right',
      render: (weight) => (
        <div className="flex flex-col items-end leading-tight">
          <Text className="text-[10px] font-semibold text-orange-600">
            {weight ? `${parseFloat(weight).toLocaleString()}` : '-'}
          </Text>
          <Text className="text-[8px] text-gray-500">kg</Text>
        </div>
      ),
    },
    {
      title: 'Wait',
      dataIndex: 'createdAt',
      width: 70,
      render: (date) => (
        <Tag color="orange" className="text-[9px] font-medium px-1.5 py-0 rounded-full border-0 m-0">
          {calculateWaitTime(date)}
        </Tag>
      ),
    },
    {
      title: <span><ClockCircleOutlined className="mr-1" />TAT</span>,
      dataIndex: 'createdAt',
      width: 65,
      render: (date) => {
        const turnaround = calculateTurnaroundTime(date);
        const color = getTurnaroundColor(turnaround.minutes);
        return (
          <Tag 
            color={color} 
            className="text-[9px] font-bold px-1.5 py-0 rounded-full border-0 m-0"
            title={`Turnaround Time: ${turnaround.display}`}
          >
            {turnaround.display}
          </Tag>
        );
      },
    },
    {
      title: 'Status',
      dataIndex: 'status',
      width: 75,
      render: (status, record) => {
        const editable = isEditing(record);
        if (editable) {
          return (
            <Input
              value={editedData.status}
              onChange={(e) => handleFieldChange('status', e.target.value)}
              size="small"
              className="h-6 text-[10px] rounded border-amber-300"
            />
          );
        }
        return (
          <Tag 
            color="#f59e0b" 
            className="text-[9px] font-semibold px-2 py-0 rounded-full border-0 m-0"
          >
            {status || 'PENDING'}
          </Tag>
        );
      },
    },
    {
      title: 'Actions',
      key: 'action',
      width: 140,
      fixed: 'right',
      render: (_, record) => {
        const editable = isEditing(record);
        const recordId = record.ticketID || record.id;
        
        return editable ? (
          <Space size={4}>
            <Button
              type="text"
              size="small"
              icon={<CheckOutlined className="text-[10px]" />}
              onClick={() => save(recordId)}
              loading={saving}
              className="text-green-600 hover:text-green-700 hover:bg-green-50 h-6 px-2 text-[10px] font-medium"
            >
              Save
            </Button>
            <Button
              type="text"
              size="small"
              icon={<CloseOutlined className="text-[10px]" />}
              onClick={cancel}
              className="text-gray-600 hover:text-gray-700 hover:bg-gray-100 h-6 px-2 text-[10px]"
            >
              Cancel
            </Button>
          </Space>
        ) : (
          <Space size={4}>
            <Button
              type="text"
              size="small"
              icon={<EditOutlined className="text-[9px]" />}
              onClick={() => edit(record)}
              className="text-amber-600 hover:text-amber-700 hover:bg-amber-50 h-6 px-1.5 text-[10px] font-medium"
            >
              Edit
            </Button>
            <Button
              type="primary"
              size="small"
              onClick={() => handleSecondWeighing(record)}
              className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0 text-[10px] font-semibold h-6 px-2 shadow-sm"
            >
              2nd Weight
            </Button>
          </Space>
        );
      },
    },
  ];

  const mergedColumns = columns.map((col) => {
    if (!col.editable) {
      return col;
    }
    return {
      ...col,
      onCell: (record) => ({
        record,
        dataIndex: col.dataIndex,
        editing: isEditing(record),
      }),
    };
  });

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <span className="text-white text-sm font-bold">{filteredData.length}</span>
            </div>
            <div>
              <Text className="text-[11px] font-bold text-gray-900 block leading-tight">
                Incomplete Transactions
              </Text>
              <Text className="text-[9px] text-amber-700 font-medium">
                Awaiting second weighing • TAT = Turnaround Time
              </Text>
            </div>
          </div>
          <Space size="small">
            <Input
              placeholder="Search..."
              prefix={<SearchOutlined className="text-gray-400 text-[10px]" />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              className="w-52 h-7 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
              allowClear
            />
            <Button
              icon={<ReloadOutlined className="text-[11px]" />}
              onClick={() => {
                setSearchText("");
                setPagination(prev => ({ ...prev, current: 1 }));
                loadTransactions();
              }}
              loading={loading || localLoading}
              className="h-7 px-3 text-[11px] rounded-md border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm font-medium"
            >
              Refresh
            </Button>
          </Space>
        </div>
      </div>

      {/* Compact Table */}
      <div className="flex-1 overflow-auto bg-white">
        <Spin spinning={loading || localLoading} tip="Loading..." size="small">
          <Table
            components={{
              body: {
                cell: EditableCell,
              },
            }}
            rowKey={(record) => record.ticketID || record.id}
            columns={mergedColumns}
            dataSource={filteredData}
            loading={false}
            pagination={false}
            size="small"
            className="compact-table"
            rowClassName={(record) => {
              const turnaround = calculateTurnaroundTime(record.createdAt);
              const isEditing = editingKey === (record.ticketID || record.id);
              
              if (isEditing) return 'editing-row';
              if (turnaround.minutes > 60) return 'urgent-row';
              return 'regular-row';
            }}
            scroll={{ x: 1500, y: 'calc(100vh - 250px)' }}
            locale={{
              emptyText: (
                <div className="py-8">
                  {searchText ? (
                    <div className="text-gray-500">
                      <SearchOutlined className="text-2xl mb-2" />
                      <p>No results for "{searchText}"</p>
                    </div>
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

      {/* Compact Footer */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <Text className="text-[10px] text-gray-600 font-medium">
          {searchText ? (
            <>
              <span className="font-semibold text-amber-600">{filteredData.length}</span> found
              <span className="text-gray-400 mx-1">·</span>
              <span className="text-gray-500">filtered from {transactions.filter(tx => 
                !tx.isCompleted && 
                !tx.completed && 
                tx.status !== 'Completed'
              ).length}</span>
            </>
          ) : (
            <>
              <span className="font-semibold text-amber-600">{filteredData.length}</span> incomplete
              <span className="text-gray-400 mx-2">•</span>
              <span className="text-green-600">🟢 &lt;30m</span>
              <span className="text-gray-400 mx-1">•</span>
              <span className="text-orange-600">🟠 30-60m</span>
              <span className="text-gray-400 mx-1">•</span>
              <span className="text-red-600">🔴 &gt;60m</span>
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

      {/* Compact Styles */}
      <style>{`
        .compact-table .ant-table {
          background: white;
          font-size: 10px;
        }
        .compact-table .ant-table-thead > tr > th {
          background: linear-gradient(to bottom, #fffbeb, #fef3c7) !important;
          border-bottom: 1.5px solid #f59e0b !important;
          padding: 6px 8px !important;
          font-weight: 700 !important;
          font-size: 9px !important;
          color: #78350f !important;
          text-transform: uppercase;
          letter-spacing: 0.3px;
          line-height: 1.2;
        }
        .compact-table .ant-table-tbody > tr.regular-row > td {
          padding: 6px 8px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          background: white !important;
          transition: all 0.12s ease;
          line-height: 1.3;
        }
        .compact-table .ant-table-tbody > tr.regular-row:hover > td {
          background: #fffbeb !important;
          box-shadow: inset 0 0 0 1px #fef3c7;
        }
        .compact-table .ant-table-tbody > tr.urgent-row > td {
          padding: 6px 8px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          background: #fef2f2 !important;
          transition: all 0.12s ease;
          line-height: 1.3;
        }
        .compact-table .ant-table-tbody > tr.urgent-row:hover > td {
          background: #fee2e2 !important;
          box-shadow: inset 0 0 0 1px #fecaca;
        }
        .compact-table .ant-table-tbody > tr.editing-row > td {
          padding: 6px 8px !important;
          background: #fef3c7 !important;
          border-bottom: 1px solid #fbbf24 !important;
          box-shadow: inset 0 1px 2px rgba(251, 191, 36, 0.12);
        }
        .compact-table .ant-spin-container {
          min-height: 200px;
        }
        .compact-pagination .ant-pagination-item {
          border-radius: 4px;
          border-color: #e5e7eb;
          font-weight: 500;
          font-size: 11px;
          min-width: 24px;
          height: 24px;
          line-height: 22px;
          margin: 0 2px;
        }
        .compact-pagination .ant-pagination-item-active {
          background: linear-gradient(135deg, #f59e0b, #f97316);
          border-color: #f59e0b;
          box-shadow: 0 1px 3px rgba(245, 158, 11, 0.25);
        }
        .compact-pagination .ant-pagination-item-active a {
          color: white !important;
          font-weight: 700;
        }
        .compact-pagination .ant-pagination-item:hover {
          border-color: #f59e0b;
        }
        .compact-pagination .ant-pagination-item:hover a {
          color: #f59e0b;
        }
        .compact-pagination .ant-pagination-options {
          font-size: 11px;
        }
        .compact-pagination .ant-select-selector {
          height: 24px !important;
          font-size: 11px !important;
        }
        .compact-pagination .ant-pagination-total-text {
          font-size: 10px;
        }
        .compact-pagination .ant-pagination-prev,
        .compact-pagination .ant-pagination-next {
          min-width: 24px;
          height: 24px;
          line-height: 22px;
          font-size: 11px;
        }
      `}</style>
    </div>
  );
}