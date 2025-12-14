import React, { useEffect, useState } from "react";
import {
  Table, Button, Space, Modal, Form, Input, message, Popconfirm, Tag, Select, DatePicker
} from 'antd';
import {
  PlusOutlined, EditOutlined, DeleteOutlined, ReloadOutlined, UserOutlined, PhoneOutlined,
  MailOutlined, GlobalOutlined, CarOutlined
} from '@ant-design/icons';
import {
  getOwners,
  createOwner,
  updateOwner,
  deleteOwner,
  getOwnerVehicles
} from '../../api/MasterData/Owners';
import dayjs from 'dayjs';

const { Option } = Select;
const { TextArea } = Input;

// Owner Type Enum mapping
const OwnerTypeEnum = {
  INDIVIDUAL: 1,
  COMPANY: 2,
  GOVERNMENT: 3
};

const OwnerTypeLabels = {
  1: 'Individual',
  2: 'Company',
  3: 'Government'
};

const Owners = () => {
  const [owners, setOwners] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [isVehicleModalVisible, setIsVehicleModalVisible] = useState(false);
  const [editingOwner, setEditingOwner] = useState(null);
  const [ownerVehicles, setOwnerVehicles] = useState([]);
  const [loadingVehicles, setLoadingVehicles] = useState(false);
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 10,
    total: 0
  });
  const [form] = Form.useForm();

  const fetchOwners = async (page = pagination.current, pageSize = pagination.pageSize, search = '') => {
    try {
      console.log('Fetching owners with params:', { page, pageSize, search });
      setLoading(true);
      
      // Clear previous errors
      setError(null);
      
      // Show loading indicator
      const hideLoading = message.loading('Loading owners...', 0);
      
      try {
        const data = await getOwners(page, pageSize, search);
        
        // If we get here, the request was successful
        const items = Array.isArray(data) ? data : (data?.items || []);
        
        console.log('Successfully fetched owners:', { items, data });
        
        setOwners(items);
        setPagination({
          current: data?.pageNumber || page,
          pageSize: data?.pageSize || pageSize,
          total: data?.totalItems || items.length
        });
        
        // Show message for empty search results
        if (search && items.length === 0) {
          message.info('No owners found matching your search criteria');
        } else if (items.length === 0) {
          message.info('No owners found');
        }
        
        return items;
      } finally {
        // Hide loading indicator
        hideLoading();
      }
      
    } catch (error) {
      console.error('Error in fetchOwners:', {
        name: error.name,
        message: error.message,
        status: error.status,
        code: error.code,
        response: error.response,
        request: error.request,
        config: error.config,
        stack: error.stack
      });
      
      // Set error state for the UI
      const errorDetails = {
        message: error.message || 'Failed to load owners',
        status: error.status || error.response?.status,
        code: error.code,
        originalError: error.originalError || error
      };
      
      setError(errorDetails);
      
      // Show user-friendly error message based on error type
      let errorMessage = 'Failed to load owners. Please try again later.';
      
      if (error.response) {
        // The request was made and the server responded with a status code
        // that falls out of the range of 2xx
        const { status, data } = error.response;
        
        if (status === 401) {
          errorMessage = 'Session expired. Please log in again.';
        } else if (status === 403) {
          errorMessage = 'You do not have permission to view owners.';
        } else if (status === 404) {
          errorMessage = 'Owners endpoint not found. Please check the API URL.';
        } else if (status === 500) {
          errorMessage = 'Server error. Please try again later.';
        } else if (data?.message) {
          errorMessage = data.message;
        } else if (typeof data === 'string') {
          errorMessage = data;
        }
      } else if (error.request) {
        // The request was made but no response was received
        if (error.code === 'ECONNABORTED') {
          errorMessage = 'Request timeout: The server took too long to respond.';
        } else if (error.message === 'Network Error') {
          errorMessage = 'Network error: Unable to connect to the server. Please check your internet connection.';
        }
      }
      
      message.error(errorMessage);
      
      // Reset to empty state on error
      setOwners([]);
      setPagination(prev => ({
        ...prev,
        total: 0
      }));
      
      // Return empty array to allow destructuring in components
      return [];
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOwners();
  }, []);

  const handleAdd = () => {
    setEditingOwner(null);
    form.resetFields();
    form.setFieldsValue({ type: OwnerTypeEnum.INDIVIDUAL });
    setIsModalVisible(true);
  };

  const handleEdit = (record) => {
    setEditingOwner(record);
    form.setFieldsValue({
      name: record.name,
      contactPerson: record.contactPerson,
      email: record.email,
      phoneNumber: record.phoneNumber,
      address: record.address,
      type: record.type,
      businessRegistrationNumber: record.businessRegistrationNumber,
      taxIdentificationNumber: record.taxIdentificationNumber,
      registrationNumber: record.registrationNumber,
      registrationDate: record.registrationDate ? dayjs(record.registrationDate) : null,
      nationalId: record.nationalId,
      dateOfBirth: record.dateOfBirth ? dayjs(record.dateOfBirth) : null,
      gender: record.gender,
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (id) => {
    try {
      await deleteOwner(id);
      message.success('Owner deleted successfully');
      fetchOwners();
    } catch (error) {
      console.error('Error deleting owner:', error);
      message.error('Failed to delete owner');
    }
  };

  const handleViewVehicles = async (ownerId, ownerName) => {
    try {
      setLoadingVehicles(true);
      setIsVehicleModalVisible(true);
      const vehicles = await getOwnerVehicles(ownerId);
      setOwnerVehicles({
        ownerName,
        vehicles: vehicles?.data || vehicles || []
      });
    } catch (error) {
      console.error('Error fetching owner vehicles:', error);
      message.error('Failed to load vehicles');
    } finally {
      setLoadingVehicles(false);
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();

      // Format dates to ISO string
      const payload = {
        ...values,
        registrationDate: values.registrationDate ? values.registrationDate.toISOString() : null,
        dateOfBirth: values.dateOfBirth ? values.dateOfBirth.toISOString() : null,
      };

      if (editingOwner) {
        await updateOwner(editingOwner.id, payload);
        message.success('Owner updated successfully');
      } else {
        await createOwner(payload);
        message.success('Owner created successfully');
      }

      setIsModalVisible(false);
      form.resetFields();
      fetchOwners();
    } catch (error) {
      if (error.errorFields) {
        message.error('Please fill in all required fields');
      } else {
        console.error('Error saving owner:', error);
        message.error('Failed to save owner');
      }
    }
  };

  const handleTableChange = (newPagination) => {
    fetchOwners(newPagination.current, newPagination.pageSize);
  };

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a, b) => (a.name || '').localeCompare(b.name || ''),
      render: (text) => <span className="font-medium">{text || '-'}</span>
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      filters: [
        { text: 'Individual', value: OwnerTypeEnum.INDIVIDUAL },
        { text: 'Company', value: OwnerTypeEnum.COMPANY },
        { text: 'Government', value: OwnerTypeEnum.GOVERNMENT },
      ],
      onFilter: (value, record) => record.type === value,
      render: (type) => {
        const colorMap = {
          1: 'green',
          2: 'blue',
          3: 'orange'
        };
        return (
            <Tag color={colorMap[type]}>
              {OwnerTypeLabels[type] || 'N/A'}
            </Tag>
        );
      },
    },
    {
      title: 'Contact Person',
      dataIndex: 'contactPerson',
      key: 'contactPerson',
      render: (text) => text || '-',
    },
    {
      title: 'Contact',
      key: 'contact',
      render: (_, record) => (
          <div className="space-y-1 text-sm">
            {record.phoneNumber && (
                <div className="flex items-center gap-1">
                  <PhoneOutlined className="text-gray-400" /> {record.phoneNumber}
                </div>
            )}
            {record.email && (
                <div className="flex items-center gap-1">
                  <MailOutlined className="text-gray-400" /> {record.email}
                </div>
            )}
          </div>
      ),
    },
    {
      title: 'Registration',
      key: 'registration',
      render: (_, record) => (
          <div className="text-sm">
            {record.type === OwnerTypeEnum.COMPANY && record.businessRegistrationNumber && (
                <div>Reg: {record.businessRegistrationNumber}</div>
            )}
            {record.type === OwnerTypeEnum.INDIVIDUAL && record.nationalId && (
                <div>ID: {record.nationalId}</div>
            )}
          </div>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      align: 'center',
      fixed: 'right',
      width: 180,
      render: (_, record) => (
          <Space size="small">
            <Button
                type="default"
                size="small"
                icon={<CarOutlined />}
                onClick={() => handleViewVehicles(record.id, record.name)}
                title="View Vehicles"
            />
            <Button
                type="primary"
                size="small"
                icon={<EditOutlined />}
                onClick={() => handleEdit(record)}
            />
            <Popconfirm
                title="Delete Owner"
                description="Are you sure you want to delete this owner?"
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

  const vehicleColumns = [
    {
      title: 'Registration No.',
      dataIndex: 'registrationNumber',
      key: 'registrationNumber',
    },
    {
      title: 'Make/Model',
      key: 'makeModel',
      render: (_, record) => `${record.make || ''} ${record.model || ''}`.trim() || '-',
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => <Tag>{status || 'Unknown'}</Tag>,
    },
  ];

  return (
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-semibold text-amber-600">Owners Management</h1>
          <Space>
            <Button
                icon={<ReloadOutlined />}
                onClick={() => fetchOwners()}
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
              Add Owner
            </Button>
          </Space>
        </div>

        <Table
            dataSource={owners}
            columns={columns}
            rowKey="id"
            loading={loading}
            pagination={{
              ...pagination,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} owners`
            }}
            onChange={handleTableChange}
            bordered
            scroll={{ x: 1200 }}
        />

        {/* Add/Edit Owner Modal */}
        <Modal
            title={editingOwner ? 'Edit Owner' : 'Add New Owner'}
            open={isModalVisible}
            onOk={handleSubmit}
            onCancel={() => {
              setIsModalVisible(false);
              form.resetFields();
            }}
            okText={editingOwner ? 'Update' : 'Create'}
            cancelText="Cancel"
            width={800}
        >
          <Form
              form={form}
              layout="vertical"
              initialValues={{ type: OwnerTypeEnum.INDIVIDUAL }}
          >
            <div className="grid grid-cols-2 gap-4">
              <Form.Item
                  name="name"
                  label="Owner Name"
                  rules={[{ required: true, message: 'Please enter owner name' }]}
              >
                <Input placeholder="Full name or company name" prefix={<UserOutlined />} />
              </Form.Item>

              <Form.Item
                  name="type"
                  label="Owner Type"
                  rules={[{ required: true, message: 'Please select owner type' }]}
              >
                <Select placeholder="Select owner type">
                  <Option value={OwnerTypeEnum.INDIVIDUAL}>Individual</Option>
                  <Option value={OwnerTypeEnum.COMPANY}>Company</Option>
                  <Option value={OwnerTypeEnum.GOVERNMENT}>Government</Option>
                </Select>
              </Form.Item>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <Form.Item
                  name="contactPerson"
                  label="Contact Person"
                  rules={[{ required: true, message: 'Please enter contact person' }]}
              >
                <Input placeholder="Primary contact name" />
              </Form.Item>

              <Form.Item
                  name="phoneNumber"
                  label="Phone Number"
                  rules={[
                    { required: true, message: 'Please enter phone number' },
                    { pattern: /^[0-9+\-\s()]+$/, message: 'Invalid phone format' }
                  ]}
              >
                <Input placeholder="+254712345678" prefix={<PhoneOutlined />} />
              </Form.Item>
            </div>

            <Form.Item
                name="email"
                label="Email"
                rules={[{ type: 'email', message: 'Please enter a valid email' }]}
            >
              <Input placeholder="email@example.com" prefix={<MailOutlined />} />
            </Form.Item>

            <Form.Item name="address" label="Address">
              <TextArea rows={2} placeholder="Physical address" />
            </Form.Item>

            <Form.Item noStyle shouldUpdate={(prev, curr) => prev.type !== curr.type}>
              {({ getFieldValue }) => {
                const ownerType = getFieldValue('type');

                if (ownerType === OwnerTypeEnum.COMPANY) {
                  return (
                      <>
                        <div className="grid grid-cols-2 gap-4">
                          <Form.Item name="businessRegistrationNumber" label="Business Reg. Number">
                            <Input placeholder="Company registration number" />
                          </Form.Item>
                          <Form.Item name="taxIdentificationNumber" label="Tax ID (TIN/PIN)">
                            <Input placeholder="Tax identification number" />
                          </Form.Item>
                        </div>
                        <div className="grid grid-cols-2 gap-4">
                          <Form.Item name="registrationNumber" label="Registration Number">
                            <Input placeholder="General registration number" />
                          </Form.Item>
                          <Form.Item name="registrationDate" label="Registration Date">
                            <DatePicker className="w-full" format="YYYY-MM-DD" />
                          </Form.Item>
                        </div>
                      </>
                  );
                }

                if (ownerType === OwnerTypeEnum.INDIVIDUAL) {
                  return (
                      <div className="grid grid-cols-3 gap-4">
                        <Form.Item name="nationalId" label="National ID">
                          <Input placeholder="National ID number" />
                        </Form.Item>
                        <Form.Item name="dateOfBirth" label="Date of Birth">
                          <DatePicker className="w-full" format="YYYY-MM-DD" />
                        </Form.Item>
                        <Form.Item name="gender" label="Gender">
                          <Select placeholder="Select gender">
                            <Option value="Male">Male</Option>
                            <Option value="Female">Female</Option>
                            <Option value="Other">Other</Option>
                          </Select>
                        </Form.Item>
                      </div>
                  );
                }

                return null;
              }}
            </Form.Item>
          </Form>
        </Modal>

        {/* Owner Vehicles Modal */}
        <Modal
            title={`Vehicles - ${ownerVehicles?.ownerName || ''}`}
            open={isVehicleModalVisible}
            onCancel={() => setIsVehicleModalVisible(false)}
            footer={[
              <Button key="close" onClick={() => setIsVehicleModalVisible(false)}>
                Close
              </Button>
            ]}
            width={900}
        >
          <Table
              dataSource={ownerVehicles?.vehicles || []}
              columns={vehicleColumns}
              rowKey="id"
              loading={loadingVehicles}
              pagination={{ pageSize: 5 }}
              size="small"
          />
        </Modal>
      </div>
  );
};

export default Owners;