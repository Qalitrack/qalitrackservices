// src/components/TransporterList.jsx
import React, { useEffect, useState } from "react";
import {
  Table,
  Tag,
  Typography,
  Space,
  Button,
  Input,
  message,
  Popconfirm,
  Avatar
} from 'antd';
import {
  ReloadOutlined,
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined
} from '@ant-design/icons';
import {
  getTransporters,
  deleteTransporter
} from '../api/MasterData/Transporters';
import TransporterFormModal from '../pages/weighing/TransporterFormModal';

const { Title } = Typography;
const { Search } = Input;

export default function TransporterList() {
  const [transporters, setTransporters] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalItems, setTotalItems] = useState(0);

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTransporter, setEditingTransporter] = useState(null);

  const fetchData = async (filters = {}) => {
    setLoading(true);
    try {
      const response = await getTransporters(
          filters.pageNumber || page,
          filters.pageSize || pageSize,
          filters.searchTerm || searchTerm
      );

      setTransporters(response.items || []);
      setTotalItems(response.totalItems || 0);
    } catch (error) {
      message.error("Failed to fetch transporters");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [page, pageSize, searchTerm]);

  const handleRefresh = () => {
    setSearchTerm('');
    setPage(1);
    fetchData({ pageNumber: 1, pageSize, searchTerm: '' });
  };

  const handleSearch = (value) => {
    setSearchTerm(value.trim());
    setPage(1);
  };

  const handleAdd = () => {
    setEditingTransporter(null);
    setIsModalOpen(true);
  };

  const handleEdit = (transporter) => {
    setEditingTransporter(transporter);
    setIsModalOpen(true);
  };

  const handleDelete = async (id) => {
    try {
      await deleteTransporter(id);
      message.success("Transporter deleted successfully");
      fetchData();
    } catch (error) {
      message.error("Failed to delete transporter");
    }
  };

  const handleModalClose = (shouldRefresh) => {
    setIsModalOpen(false);
    setEditingTransporter(null);
    if (shouldRefresh) {
      fetchData();
    }
  };

  const columns = [
    {
      title: 'Logo',
      dataIndex: 'logo',
      key: 'logo',
      width: 80,
      render: (logo, record) => (
          <Avatar
              src={logo}
              size={40}
              style={{ backgroundColor: '#1890ff' }}
          >
            {record.name?.charAt(0).toUpperCase()}
          </Avatar>
      ),
    },
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (text) => <strong>{text || '-'}</strong>,
    },
    {
      title: 'Contact Info',
      dataIndex: 'contactInfo',
      key: 'contactInfo',
      render: (text) => text || '-',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        const color = status?.toLowerCase() === 'active' ? 'success' : 'default';
        return <Tag color={color}>{status || 'Unknown'}</Tag>;
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      align: 'center',
      width: 180,
      render: (_, record) => (
          <Space size="small">
            <Button
                type="primary"
                icon={<EditOutlined />}
                size="small"
                onClick={() => handleEdit(record)}
            >
              Edit
            </Button>
            <Popconfirm
                title="Delete this transporter?"
                description="This action cannot be undone."
                onConfirm={() => handleDelete(record.id)}
                okText="Yes"
                cancelText="No"
            >
              <Button
                  danger
                  icon={<DeleteOutlined />}
                  size="small"
              >
                Delete
              </Button>
            </Popconfirm>
          </Space>
      ),
    },
  ];

  return (
      <div className="p-6 max-w-7xl mx-auto bg-white rounded-lg shadow">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
          <Title level={3} className="text-amber-700 m-0">
            Transporters Management
          </Title>
          <Space>
            <Search
                placeholder="Search by name..."
                allowClear
                enterButton={<SearchOutlined />}
                size="large"
                onSearch={handleSearch}
                style={{ width: 250 }}
            />
            <Button
                type="default"
                icon={<ReloadOutlined />}
                onClick={handleRefresh}
                loading={loading}
            >
              Refresh
            </Button>
            <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleAdd}
                size="large"
            >
              Add Transporter
            </Button>
          </Space>
        </div>

        <Table
            dataSource={transporters}
            columns={columns}
            rowKey="id"
            loading={loading}
            pagination={{
              current: page,
              pageSize,
              total: totalItems,
              showSizeChanger: true,
              showQuickJumper: true,
              onChange: (p, size) => {
                setPage(p);
                setPageSize(size);
              },
              showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} transporters`,
            }}
            bordered
            size="middle"
        />

        <TransporterFormModal
            open={isModalOpen}
            transporter={editingTransporter}
            onClose={handleModalClose}
        />
      </div>
  );
}