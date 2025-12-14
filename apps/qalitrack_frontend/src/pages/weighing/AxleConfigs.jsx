import React, { useEffect, useState } from "react";
import {
  Table, Button, Space, Modal, Form, Input, InputNumber, message, Popconfirm, Tag, Switch
} from 'antd';
import {
  PlusOutlined, EditOutlined, DeleteOutlined, ReloadOutlined
} from '@ant-design/icons';
import {
  getAxleConfigs,
  createAxleConfig,
  updateAxleConfig,
  deleteAxleConfig,
  toggleAxleConfigStatus
} from '../../api/MasterData/AxleConfigs';

const { Column } = Table;
const { TextArea } = Input;

const AxleConfigs = () => {
  const [configs, setConfigs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingConfig, setEditingConfig] = useState(null);
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 10,
    total: 0
  });
  const [form] = Form.useForm();

  const fetchConfigs = async (page = pagination.current, pageSize = pagination.pageSize) => {
    try {
      setLoading(true);
      const response = await getAxleConfigs(page, pageSize);

      // API returns { success, data: { items, pageNumber, ... } }
      const data = response?.data || response;

      setConfigs(data?.items || []);
      setPagination({
        current: data?.pageNumber || page,
        pageSize: data?.pageSize || pageSize,
        total: data?.totalItems || 0
      });
    } catch (error) {
      console.error('Error fetching axle configurations:', error);
      message.error('Failed to load axle configurations');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchConfigs();
  }, []);

  const handleAdd = () => {
    setEditingConfig(null);
    form.resetFields();
    form.setFieldsValue({ isActive: true });
    setIsModalVisible(true);
  };

  const handleEdit = (record) => {
    setEditingConfig(record);
    form.setFieldsValue({
      code: record.code,
      description: record.description,
      axleCount: record.axleCount,
      maxLoadCapacity: record.maxLoadCapacity,
      isActive: record.isActive,
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (id) => {
    try {
      await deleteAxleConfig(id);
      message.success('Axle configuration deleted successfully');
      fetchConfigs();
    } catch (error) {
      console.error('Error deleting axle configuration:', error);
      message.error('Failed to delete axle configuration');
    }
  };

  const handleStatusToggle = async (id, currentStatus) => {
    try {
      await toggleAxleConfigStatus(id, !currentStatus);
      message.success('Status updated successfully');
      fetchConfigs();
    } catch (error) {
      console.error('Error toggling status:', error);
      message.error('Failed to update status');
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();

      if (editingConfig) {
        // For PUT, include the ID in the payload
        const payload = { id: editingConfig.id, ...values };
        await updateAxleConfig(editingConfig.id, payload);
        message.success('Axle configuration updated successfully');
      } else {
        await createAxleConfig(values);
        message.success('Axle configuration created successfully');
      }

      setIsModalVisible(false);
      form.resetFields();
      fetchConfigs();
    } catch (error) {
      if (error.errorFields) {
        message.error('Please fill in all required fields');
      } else {
        console.error('Error saving axle configuration:', error);
        message.error('Failed to save axle configuration');
      }
    }
  };

  const handleTableChange = (newPagination) => {
    fetchConfigs(newPagination.current, newPagination.pageSize);
  };

  const columns = [
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
      sorter: (a, b) => (a.code || '').localeCompare(b.code || ''),
      render: (text) => <Tag color="blue">{text || '-'}</Tag>
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Axle Count',
      dataIndex: 'axleCount',
      key: 'axleCount',
      sorter: (a, b) => a.axleCount - b.axleCount,
      align: 'center',
      render: (count) => <Tag color="cyan">{count}</Tag>
    },
    {
      title: 'Max Load Capacity (kg)',
      dataIndex: 'maxLoadCapacity',
      key: 'maxLoadCapacity',
      sorter: (a, b) => a.maxLoadCapacity - b.maxLoadCapacity,
      align: 'right',
      render: (weight) => weight?.toLocaleString() || '-'
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      align: 'center',
      filters: [
        { text: 'Active', value: true },
        { text: 'Inactive', value: false }
      ],
      onFilter: (value, record) => record.isActive === value,
      render: (isActive, record) => (
          <Switch
              checked={isActive}
              onChange={() => handleStatusToggle(record.id, isActive)}
              checkedChildren="Active"
              unCheckedChildren="Inactive"
          />
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      align: 'center',
      fixed: 'right',
      width: 120,
      render: (_, record) => (
          <Space size="small">
            <Button
                type="primary"
                size="small"
                icon={<EditOutlined />}
                onClick={() => handleEdit(record)}
            />
            <Popconfirm
                title="Delete Configuration"
                description="Are you sure you want to delete this configuration?"
                onConfirm={() => handleDelete(record.id)}
                okText="Yes"
                cancelText="No"
                okButtonProps={{ danger: true }}
            >
              <Button
                  danger
                  size="small"
                  icon={<DeleteOutlined />}
              />
            </Popconfirm>
          </Space>
      )
    }
  ];

  return (
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-semibold text-amber-600">Axle Configurations</h1>
          <Space>
            <Button
                icon={<ReloadOutlined />}
                onClick={() => fetchConfigs()}
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
              Add Configuration
            </Button>
          </Space>
        </div>

        <Table
            dataSource={configs}
            columns={columns}
            rowKey="id"
            loading={loading}
            pagination={{
              ...pagination,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} configurations`
            }}
            onChange={handleTableChange}
            bordered
            scroll={{ x: 1000 }}
        />

        <Modal
            title={editingConfig ? 'Edit Axle Configuration' : 'Add New Axle Configuration'}
            open={isModalVisible}
            onOk={handleSubmit}
            onCancel={() => {
              setIsModalVisible(false);
              form.resetFields();
            }}
            okText={editingConfig ? 'Update' : 'Create'}
            cancelText="Cancel"
            width={600}
        >
          <Form
              form={form}
              layout="vertical"
              initialValues={{ isActive: true }}
          >
            <Form.Item
                name="code"
                label="Configuration Code"
                rules={[
                  { required: true, message: 'Please enter a code' },
                  { pattern: /^[A-Z0-9-]+$/, message: 'Code must be uppercase letters, numbers, or hyphens' }
                ]}
            >
              <Input
                  placeholder="e.g., AXL-3"
                  style={{ textTransform: 'uppercase' }}
              />
            </Form.Item>

            <Form.Item
                name="description"
                label="Description"
                rules={[{ required: true, message: 'Please enter a description' }]}
            >
              <TextArea
                  rows={3}
                  placeholder="Enter a brief description of this configuration"
              />
            </Form.Item>

            <div className="grid grid-cols-2 gap-4">
              <Form.Item
                  name="axleCount"
                  label="Number of Axles"
                  rules={[
                    { required: true, message: 'Please enter number of axles' },
                    { type: 'number', min: 1, max: 20, message: 'Must be between 1 and 20' }
                  ]}
              >
                <InputNumber
                    min={1}
                    max={20}
                    className="w-full"
                    placeholder="e.g., 3"
                />
              </Form.Item>

              <Form.Item
                  name="maxLoadCapacity"
                  label="Max Load Capacity (kg)"
                  rules={[
                    { required: true, message: 'Please enter maximum load capacity' },
                    { type: 'number', min: 100, message: 'Must be at least 100 kg' }
                  ]}
              >
                <InputNumber
                    min={100}
                    step={100}
                    className="w-full"
                    placeholder="e.g., 30000"
                    formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                    parser={value => value.replace(/\$\s?|(,*)/g, '')}
                />
              </Form.Item>
            </div>

            <Form.Item
                name="isActive"
                label="Status"
                valuePropName="checked"
            >
              <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
            </Form.Item>
          </Form>
        </Modal>
      </div>
  );
};

export default AxleConfigs;