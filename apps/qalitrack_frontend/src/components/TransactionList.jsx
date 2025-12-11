import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from 'react-redux';
import { fetchTransactions } from '../store/weighingSlice'; // adjust path if needed
import { Table, Tag, Typography, Space, Button, Input } from 'antd';
import { ReloadOutlined, SearchOutlined } from '@ant-design/icons';

const { Text, Title } = Typography;
const { Search } = Input;

export default function TransactionList() {
  const dispatch = useDispatch();
  const { transactions = [], loading } = useSelector((state) => state.weighing);

  // Local state for search/filter
  const [searchPlate, setSearchPlate] = useState('');

  // Fetch all transactions on mount + when search changes
  useEffect(() => {
    dispatch(
        fetchTransactions({
          pageNumber: 1,
          pageSize: 100,
          noPlate: searchPlate || undefined, // only send if not empty
          sortBy: 'createdAt',
          sortDescending: true,
        })
    );
  }, [dispatch, searchPlate]);

  // Refresh handler
  const handleRefresh = () => {
    dispatch(
        fetchTransactions({
          pageNumber: 1,
          pageSize: 100,
          sortBy: 'createdAt',
          sortDescending: true,
        })
    );
  };

  // Search by plate number
  const handleSearch = (value) => {
    setSearchPlate(value.trim());
  };

  const columns = [
    {
      title: 'Receipt No',
      dataIndex: 'receiptNo',
      key: 'receiptNo',
      render: (text) => <Text strong copyable>{text}</Text>,
    },
    {
      title: 'Plate Number',
      dataIndex: 'noPlate',
      key: 'noPlate',
      render: (plate) => <Tag color="blue">{plate || '-'}</Tag>,
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
      render: (w) => (w ? <Text strong>{w.toLocaleString()} kg</Text> : '-'),
    },
    {
      title: 'Tare (W2)',
      dataIndex: 'secondWeight',
      key: 'secondWeight',
      align: 'right',
      render: (w) => (w > 0 ? `${w.toLocaleString()} kg` : '-'),
    },
    {
      title: 'Net Weight',
      key: 'netWeight',
      align: 'right',
      render: (_, record) => {
        const w1 = Number(record.firstWeight) || 0;
        const w2 = Number(record.secondWeight) || 0;
        const net = w1 > w2 ? w1 - w2 : w2 - w1;

        if (w1 > 0 && w2 > 0) {
          return <Tag color="green" className="font-semibold">{net.toLocaleString()} kg</Tag>;
        }
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
  ];

  return (
      <div className="p-6 max-w-7xl mx-auto bg-white rounded-lg shadow">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
          <Title level={3} className="text-amber-700 m-0">
            Recent Weighing Transactions
          </Title>

          <Space>
            <Search
                placeholder="Search by plate number"
                allowClear
                enterButton={<SearchOutlined />}
                size="large"
                onSearch={handleSearch}
                style={{ width: 300 }}
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
              pageSize: 15,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} transactions`,
            }}
            scroll={{ x: 1000 }}
            bordered
            size="middle"
        />
      </div>
  );
}