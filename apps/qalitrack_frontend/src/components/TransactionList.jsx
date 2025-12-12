// src/components/TransactionList.jsx
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from 'react-redux';
import { 
  fetchTransactions, 
  completeTransaction, 
  deactivateTransactionApi 
} from '../store/weighingSlice';
import { Table, Tag, Typography, Space, Button, Input, message, Popconfirm } from 'antd';
import { ReloadOutlined, SearchOutlined, CheckOutlined, StopOutlined } from '@ant-design/icons';

const { Title } = Typography;
const { Search } = Input;

export default function TransactionList() {
  const dispatch = useDispatch();
  const { transactions = [], loading, error } = useSelector((state) => state.weighing);

  // Local state for search and pagination
  const [searchPlate, setSearchPlate] = useState('');
  const [searchReceipt, setSearchReceipt] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(15);

  // Fetch transactions
  const fetchData = (filters = {}) => {
    dispatch(fetchTransactions({ 
      pageNumber: filters.pageNumber || page,
      pageSize: filters.pageSize || pageSize,
      noPlate: filters.noPlate,
      receiptNo: filters.receiptNo,
      sortBy: 'createdAt',
      sortDescending: true
    }));
  };

  useEffect(() => {
    fetchData({ noPlate: searchPlate, receiptNo: searchReceipt });
  }, [dispatch, searchPlate, searchReceipt, page, pageSize]);

  const handleRefresh = () => {
    setSearchPlate('');
    setSearchReceipt('');
    fetchData({ pageNumber: 1, pageSize });
  };

  const handleSearchPlate = (value) => setSearchPlate(value.trim());
  const handleSearchReceipt = (value) => setSearchReceipt(value.trim());

  const handleComplete = async (txId) => {
    try {
      await dispatch(completeTransaction({ transactionId: txId })).unwrap();
      message.success("Transaction completed");
    } catch (err) {
      message.error("Failed to complete transaction");
    }
  };

  const handleDeactivate = async (txId) => {
    try {
      await dispatch(deactivateTransactionApi(txId)).unwrap();
      message.success("Transaction deactivated");
    } catch (err) {
      message.error("Failed to deactivate transaction");
    }
  };

  const columns = [
    {
      title: 'Receipt No',
      dataIndex: 'receiptNo',
      key: 'receiptNo',
      render: (text) => <Tag color="geekblue">{text || '-'}</Tag>,
    },
    {
      title: 'Plate Number',
      dataIndex: 'noPlate',
      key: 'noPlate',
      render: (plate) => plate ? <Tag color="blue">{plate}</Tag> : '-',
    },
    {
      title: 'Driver',
      dataIndex: 'driverName',
      key: 'driverName',
      render: (name) => name || '-',
    },
    {
      title: 'Commodity',
      dataIndex: 'commodityName',
      key: 'commodityName',
      render: (name) => name || '-',
    },
    {
      title: 'Gross (W1)',
      dataIndex: 'firstWeight',
      key: 'firstWeight',
      align: 'right',
      render: (w) => w ? `${w.toLocaleString()} kg` : '-',
    },
    {
      title: 'Tare (W2)',
      dataIndex: 'secondWeight',
      key: 'secondWeight',
      align: 'right',
      render: (w) => w > 0 ? `${w.toLocaleString()} kg` : '-',
    },
    {
      title: 'Net Weight',
      key: 'netWeight',
      align: 'right',
      render: (_, record) => {
        const w1 = Number(record.firstWeight) || 0;
        const w2 = Number(record.secondWeight) || 0;
        if (w1 && w2) return <Tag color="green">{(w1 - w2).toLocaleString()} kg</Tag>;
        return <Tag color="orange">Pending Tare</Tag>;
      },
    },
    {
      title: 'Status',
      dataIndex: 'isCompleted',
      key: 'status',
      render: (completed, record) => (
        <Tag color={completed ? 'success' : record.secondWeight > 0 ? 'processing' : 'warning'}>
          {completed ? 'Completed' : record.secondWeight > 0 ? 'Tare Done' : 'Pending Tare'}
        </Tag>
      ),
    },
    {
      title: 'Date & Time',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => date ? new Date(date).toLocaleString() : '-',
    },
    {
      title: 'Actions',
      key: 'actions',
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          {!record.isCompleted && (
            <Popconfirm title="Complete this transaction?" onConfirm={() => handleComplete(record.id)}>
              <Button type="primary" icon={<CheckOutlined />} size="small">
                Complete
              </Button>
            </Popconfirm>
          )}
          <Popconfirm title="Deactivate this transaction?" onConfirm={() => handleDeactivate(record.id)}>
            <Button danger icon={<StopOutlined />} size="small">
              Deactivate
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className="p-6 max-w-7xl mx-auto bg-white rounded-lg shadow">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <Title level={3} className="text-amber-700 m-0">Recent Weighing Transactions</Title>
        <Space>
          <Search
            placeholder="Search Plate Number"
            allowClear
            enterButton={<SearchOutlined />}
            size="large"
            onSearch={handleSearchPlate}
            style={{ width: 200 }}
          />
          <Search
            placeholder="Search Receipt No"
            allowClear
            enterButton={<SearchOutlined />}
            size="large"
            onSearch={handleSearchReceipt}
            style={{ width: 200 }}
          />
          <Button
            type="primary"
            icon={<ReloadOutlined />}
            onClick={handleRefresh}
            loading={loading}
          >
            Refresh
          </Button>
        </Space>
      </div>

      <Table
        dataSource={transactions}
        columns={columns}
        rowKey="id"
        loading={loading}
        pagination={{
          current: page,
          pageSize,
          total: transactions.length,
          showSizeChanger: true,
          showQuickJumper: true,
          onChange: (p, size) => {
            setPage(p);
            setPageSize(size);
          },
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} transactions`,
        }}
        scroll={{ x: 1200 }}
        bordered
        size="middle"
      />
      {error && <div className="text-red-600 mt-2">{error}</div>}
    </div>
  );
}
