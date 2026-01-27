import React, { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, Typography, Space, Pagination, message, Spin } from "antd";
import { ReloadOutlined, SearchOutlined, CheckOutlined, CloseOutlined, EditOutlined, UserOutlined } from "@ant-design/icons";
import { fetchTransactions, updateTransactionApi } from "../../store/weighingSlice";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

dayjs.extend(relativeTime);

const { Text } = Typography;

export default function IncompleteTransactionsTable({ onAddWeighing }) {
  const dispatch = useDispatch();
  
  const { transactions = [], loading } = useSelector((state) => state.weighing);
  const currentUser = useSelector((state) => state.auth?.user);

  const [searchText, setSearchText] = useState("");
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [editingKey, setEditingKey] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [saving, setSaving] = useState(false);
  const [localLoading, setLocalLoading] = useState(false);

  const searchTimeoutRef = useRef(null);

  const loadTransactions = useCallback(() => {
    setLocalLoading(true);
    
    const params = {
      isCompleted: false,
      pageNumber: pagination.current,
      pageSize: pagination.pageSize,
    };
    
    console.log("📤 Fetching incomplete transactions with params:", params);
    
    dispatch(fetchTransactions(params))
      .unwrap()
      .then((data) => {
        console.log("✅ Incomplete transactions loaded:", data);
        setLocalLoading(false);
      })
      .catch((error) => {
        console.error("❌ Failed to load transactions:", error);
        message.error("Failed to load transactions: " + error);
        setLocalLoading(false);
      });
  }, [dispatch, pagination.current, pagination.pageSize]);

  useEffect(() => {
    loadTransactions();
  }, [loadTransactions]);

  useEffect(() => {
    console.log("📊 Transactions updated in Redux:", transactions.length);
  }, [transactions]);

  // Reset to page 1 when searching
  useEffect(() => {
    if (searchText && pagination.current !== 1) {
      setPagination(prev => ({ ...prev, current: 1 }));
    }
  }, [searchText, pagination.current]);

  const filteredData = useMemo(() => {
    let data = transactions.filter(tx => !tx.isCompleted && !tx.completed);
    
    // Client-side search filtering
    if (searchText?.trim()) {
      const searchLower = searchText.toLowerCase().trim();
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
    }
    
    return data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
  }, [transactions, searchText]);

  useEffect(() => {
    setPagination(prev => ({ ...prev, total: filteredData.length }));
  }, [filteredData]);

  const isEditing = (record) => record.ticketID === editingKey;

  const edit = (record) => {
    setEditingKey(record.ticketID);
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
        return editable ? (
          <Space size={4}>
            <Button
              type="text"
              size="small"
              icon={<CheckOutlined className="text-[10px]" />}
              onClick={() => save(record.ticketID)}
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
              onClick={() => onAddWeighing({
                ...record,
                id: record.ticketID,
                ticketID: record.ticketID,
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
              })}
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
                Awaiting second weighing
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
            rowKey="ticketID"
            columns={mergedColumns}
            dataSource={filteredData}
            loading={false}
            pagination={false}
            size="small"
            className="compact-table"
            rowClassName={(record) => 
              isEditing(record) 
                ? 'editing-row' 
                : 'regular-row'
            }
            scroll={{ x: 1400, y: 'calc(100vh - 250px)' }}
            locale={{
              emptyText: searchText 
                ? `No results for "${searchText}"`
                : "No incomplete transactions"
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
              <span className="text-gray-500">filtered from {transactions.filter(tx => !tx.isCompleted && !tx.completed).length}</span>
            </>
          ) : (
            <>
              <span className="font-semibold text-amber-600">{filteredData.length}</span> incomplete
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