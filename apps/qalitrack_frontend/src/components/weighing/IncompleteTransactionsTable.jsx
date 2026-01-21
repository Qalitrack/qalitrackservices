import React, { useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Table, Tag, Button, Input, Typography, Space, Pagination, message } from "antd";
import { ReloadOutlined, SearchOutlined, CheckOutlined, CloseOutlined, EditOutlined } from "@ant-design/icons";
import { fetchTransactions, updateTransactionApi } from "../../store/weighingSlice";
import dayjs from "dayjs";

const { Text } = Typography;

const PAGE_SIZE = 2;

export default function IncompleteTransactionsTable({ onAddWeighing, refreshKey }) {
  const dispatch = useDispatch();
  const { transactions = [], loading } = useSelector((state) => state.weighing);

  const [searchText, setSearchText] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [editingKey, setEditingKey] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    dispatch(fetchTransactions({ isCompleted: false, pageSize: 100 }));
  }, [dispatch, refreshKey]);

  const filteredData = useMemo(() => {
    const lowerSearch = searchText.toLowerCase();
    return transactions
      .filter((t) => {
        if (!searchText) return true;
        return (
          t.receiptNo?.toLowerCase().includes(lowerSearch) ||
          t.noPlate?.toLowerCase().includes(lowerSearch) ||
          t.driverName?.toLowerCase().includes(lowerSearch) ||
          t.commodityName?.toLowerCase().includes(lowerSearch) ||
          t.transporterName?.toLowerCase().includes(lowerSearch)
        );
      })
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
  }, [transactions, searchText]);

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return filteredData.slice(start, start + PAGE_SIZE);
  }, [filteredData, currentPage]);

  const isEditing = (record) => record.id === editingKey;

  const edit = (record) => {
    setEditingKey(record.id);
    setEditedData({ ...record });
  };

  const cancel = () => {
    setEditingKey(null);
    setEditedData({});
  };

  const save = async (id) => {
    try {
      setSaving(true);
      const dataToUpdate = {
        receiptNo: editedData.receiptNo,
        noPlate: editedData.noPlate,
        driverName: editedData.driverName,
        commodityName: editedData.commodityName,
        supplierName: editedData.supplierName,
        customerName: editedData.customerName,
        transporterName: editedData.transporterName,
        originName: editedData.originName,
        destinationName: editedData.destinationName,
        status: editedData.status,
        weighMode: editedData.weighMode,
      };

      await dispatch(updateTransactionApi({ id, data: dataToUpdate })).unwrap();
      message.success('✓ Updated successfully');
      setEditingKey(null);
      setEditedData({});
      dispatch(fetchTransactions({ isCompleted: false, pageSize: 100 }));
    } catch (error) {
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
        className="rounded border-amber-300 focus:border-amber-500 focus:ring-1 focus:ring-amber-500"
      />
    );

    return (
      <td {...restProps}>
        {editing ? inputNode : children}
      </td>
    );
  };

  const columns = [
    {
      title: 'Date',
      dataIndex: 'createdAt',
      width: 100,
      render: (date) => (
        <div className="flex flex-col">
          <Text className="text-xs font-semibold text-gray-800">
            {date ? dayjs(date).format('DD MMM') : '-'}
          </Text>
          <Text className="text-[10px] text-gray-500">
            {date ? dayjs(date).format('YYYY') : ''}
          </Text>
        </div>
      ),
    },
    {
      title: 'Receipt',
      dataIndex: 'receiptNo',
      width: 130,
      editable: true,
      render: (text) => (
        <Text className="text-xs font-mono font-bold text-amber-700">
          {text || '-'}
        </Text>
      ),
    },
    {
      title: 'Vehicle',
      dataIndex: 'noPlate',
      width: 100,
      editable: true,
      render: (text) => (
        <div className="inline-block bg-gray-900 text-white px-2 py-1 rounded text-xs font-bold">
          {text || '-'}
        </div>
      ),
    },
    {
      title: 'Driver',
      dataIndex: 'driverName',
      width: 130,
      editable: true,
      render: (text) => (
        <Text className="text-xs font-medium text-gray-700">{text || '-'}</Text>
      ),
    },
    {
      title: 'Commodity',
      dataIndex: 'commodityName',
      width: 120,
      editable: true,
      render: (text) => (
        <Text className="text-xs text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Transporter',
      dataIndex: 'transporterName',
      width: 130,
      editable: true,
      render: (text) => (
        <Text className="text-xs text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Source',
      dataIndex: 'originName',
      width: 100,
      editable: true,
      render: (text) => (
        <Text className="text-xs text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'Destination',
      dataIndex: 'destinationName',
      width: 110,
      editable: true,
      render: (text) => (
        <Text className="text-xs text-gray-600">{text || '-'}</Text>
      ),
    },
    {
      title: 'First Wt',
      dataIndex: 'firstWeight',
      width: 90,
      align: 'right',
      render: (weight) => (
        <Text className="text-xs font-semibold text-blue-600">
          {weight ? `${weight.toLocaleString()} kg` : '-'}
        </Text>
      ),
    },
    {
      title: 'Second Wt',
      dataIndex: 'secondWeight',
      width: 95,
      align: 'right',
      render: (weight) => (
        <Text className="text-xs font-semibold text-green-600">
          {weight ? `${weight.toLocaleString()} kg` : '-'}
        </Text>
      ),
    },
    {
      title: 'Net Wt',
      dataIndex: 'netWeight',
      width: 90,
      align: 'right',
      render: (weight) => (
        <Text className="text-xs font-bold text-orange-600">
          {weight ? `${weight.toLocaleString()} kg` : '-'}
        </Text>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      width: 100,
      render: (status, record) => {
        const editable = isEditing(record);
        if (editable) {
          return (
            <Input
              value={editedData.status}
              onChange={(e) => handleFieldChange('status', e.target.value)}
              size="small"
              className="rounded border-amber-300"
            />
          );
        }
        return (
          <Tag 
            color={status === 'Completed' ? '#10b981' : '#f59e0b'} 
            className="text-xs font-bold px-2.5 py-0.5 rounded-full border-0"
          >
            {status || 'InProgress'}
          </Tag>
        );
      },
    },
    {
      title: 'Actions',
      key: 'action',
      width: 160,
      fixed: 'right',
      render: (_, record) => {
        const editable = isEditing(record);
        return editable ? (
          <Space size={8}>
            <Button
              type="text"
              size="small"
              icon={<CheckOutlined />}
              onClick={() => save(record.id)}
              loading={saving}
              className="text-green-600 hover:text-green-700 hover:bg-green-50 h-7 px-2 font-medium"
            >
              Save
            </Button>
            <Button
              type="text"
              size="small"
              icon={<CloseOutlined />}
              onClick={cancel}
              className="text-gray-600 hover:text-gray-700 hover:bg-gray-100 h-7 px-2"
            >
              Cancel
            </Button>
          </Space>
        ) : (
          <Space size={6}>
            <Button
              type="text"
              size="small"
              icon={<EditOutlined className="text-xs" />}
              onClick={() => edit(record)}
              className="text-blue-600 hover:text-blue-700 hover:bg-blue-50 h-7 px-2 text-xs font-medium"
            >
              Edit
            </Button>
            <Button
              type="primary"
              size="small"
              onClick={() => onAddWeighing({
                ...record,
                firstWeight: record.firstWeight?.toString() || "",
                secondWeight: "",
                expectedWeighings: record.expectedWeighings || 2,
              })}
              className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 border-0 text-xs font-bold h-7 px-3 shadow-sm"
            >
              Finalize
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
    <div className="h-full flex flex-col bg-white rounded-xl shadow-lg border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="px-5 py-4 bg-gradient-to-r from-amber-50 to-orange-50 border-b border-amber-100">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-1 h-8 bg-gradient-to-b from-amber-500 to-orange-500 rounded-full"></div>
            <div>
              <Text className="text-sm font-bold text-gray-800 block">
                Incomplete Transactions
              </Text>
              <Text className="text-xs text-gray-500">
                {filteredData.length} pending weighings
              </Text>
            </div>
          </div>
          <Space size="middle">
            <Input
              placeholder="Search by receipt, vehicle, driver..."
              prefix={<SearchOutlined className="text-gray-400" />}
              value={searchText}
              onChange={(e) => {
                setSearchText(e.target.value);
                setCurrentPage(1);
              }}
              className="w-72 h-9 text-sm rounded-lg border-gray-300 focus:border-amber-500 shadow-sm"
              allowClear
            />
            <Button
              icon={<ReloadOutlined />}
              onClick={() => dispatch(fetchTransactions({ isCompleted: false, pageSize: 100 }))}
              className="h-9 px-4 rounded-lg border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm"
            >
              Refresh
            </Button>
          </Space>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-auto bg-gray-50">
        <Table
          components={{
            body: {
              cell: EditableCell,
            },
          }}
          rowKey="id"
          columns={mergedColumns}
          dataSource={paginatedData}
          loading={loading}
          pagination={false}
          size="small"
          className="modern-operator-table"
          rowClassName={(record) => 
            isEditing(record) 
              ? 'editing-row' 
              : 'regular-row'
          }
          scroll={{ x: 1400 }}
        />
      </div>

      {/* Footer with Pagination */}
      {filteredData.length > PAGE_SIZE && (
        <div className="px-5 py-3 border-t border-gray-200 bg-white flex justify-between items-center">
          <Text className="text-xs text-gray-600 font-medium">
            Showing <span className="font-bold text-gray-800">{((currentPage - 1) * PAGE_SIZE) + 1}</span> to <span className="font-bold text-gray-800">{Math.min(currentPage * PAGE_SIZE, filteredData.length)}</span> of <span className="font-bold text-gray-800">{filteredData.length}</span>
          </Text>
          <Pagination
            current={currentPage}
            pageSize={PAGE_SIZE}
            total={filteredData.length}
            onChange={setCurrentPage}
            size="small"
            showSizeChanger={false}
            className="custom-pagination"
          />
        </div>
      )}

      {/* Creative but Simple Styling */}
      <style jsx>{`
        .modern-operator-table :global(.ant-table) {
          background: white;
        }
        .modern-operator-table :global(.ant-table-thead > tr > th) {
          background: linear-gradient(to bottom, #ffffff, #f9fafb) !important;
          border-bottom: 2px solid #f59e0b !important;
          padding: 14px 16px !important;
          font-weight: 700 !important;
          font-size: 11px !important;
          color: #1f2937 !important;
          text-transform: uppercase;
          letter-spacing: 0.5px;
        }
        .modern-operator-table :global(.ant-table-tbody > tr.regular-row > td) {
          padding: 14px 16px !important;
          border-bottom: 1px solid #f3f4f6 !important;
          background: white !important;
          transition: all 0.2s ease;
        }
        .modern-operator-table :global(.ant-table-tbody > tr.regular-row:hover > td) {
          background: #fffbeb !important;
          box-shadow: inset 0 0 0 1px #fef3c7;
        }
        .modern-operator-table :global(.ant-table-tbody > tr.editing-row > td) {
          padding: 14px 16px !important;
          background: #fef3c7 !important;
          border-bottom: 1px solid #fbbf24 !important;
          box-shadow: inset 0 2px 4px rgba(251, 191, 36, 0.1);
        }
        .custom-pagination :global(.ant-pagination-item) {
          border-radius: 6px;
          border-color: #e5e7eb;
        }
        .custom-pagination :global(.ant-pagination-item-active) {
          background: linear-gradient(135deg, #f59e0b, #f97316);
          border-color: #f59e0b;
        }
        .custom-pagination :global(.ant-pagination-item-active a) {
          color: white;
          font-weight: 600;
        }
      `}</style>
    </div>
  );
}
