// src/components/TransporterFormModal.jsx
import React, { useEffect } from "react";
import { Modal, Form, Input, Select, message } from 'antd';
import { createTransporter, updateTransporter } from '../../api/MasterData/Transporters';

const { Option } = Select;

export default function TransporterFormModal({ open, transporter, onClose }) {
    const [form] = Form.useForm();
    const [loading, setLoading] = React.useState(false);

    const isEditing = !!transporter;

    useEffect(() => {
        if (open && transporter) {
            // Parse contactInfo if it's a JSON string or object
            let contactInfo = transporter.contactInfo;
            if (typeof contactInfo === 'string') {
                try {
                    contactInfo = JSON.parse(contactInfo);
                } catch (e) {
                    contactInfo = {};
                }
            }

            form.setFieldsValue({
                name: transporter.name,
                email: contactInfo?.Email || '',
                phone: contactInfo?.Phone || '',
                address: contactInfo?.Address || '',
                licenseNumber: contactInfo?.LicenseNumber || '',
                status: transporter.status,
                logo: transporter.logo,
            });
        } else if (open) {
            form.resetFields();
        }
    }, [open, transporter, form]);

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            setLoading(true);

            // Convert form values to the correct API format
            const payload = {
                name: values.name,
                // ContactInfo must be a JSON object (as a string or object depending on API)
                contactInfo: JSON.stringify({
                    Email: values.email || "",
                    Phone: values.phone || "",
                    Address: values.address || "",
                    LicenseNumber: values.licenseNumber || ""
                }),
                status: values.status,
                logo: values.logo || ""
            };

            if (isEditing) {
                await updateTransporter(transporter.id, payload);
                message.success("Transporter updated successfully");
            } else {
                await createTransporter(payload);
                message.success("Transporter created successfully");
            }

            form.resetFields();
            onClose(true);
        } catch (error) {
            if (error.errorFields) {
                message.error("Please fill in all required fields");
            } else {
                message.error(`Failed to ${isEditing ? 'update' : 'create'} transporter`);
                console.error(error);
            }
        } finally {
            setLoading(false);
        }
    };

    const handleCancel = () => {
        form.resetFields();
        onClose(false);
    };

    return (
        <Modal
            title={isEditing ? "Edit Transporter" : "Add New Transporter"}
            open={open}
            onOk={handleSubmit}
            onCancel={handleCancel}
            confirmLoading={loading}
            width={600}
            okText={isEditing ? "Update" : "Create"}
            cancelText="Cancel"
        >
            <Form
                form={form}
                layout="vertical"
                name="transporterForm"
                initialValues={{
                    status: 'Active',
                }}
            >
                <Form.Item
                    label="Transporter Name"
                    name="name"
                    rules={[
                        { required: true, message: 'Please enter transporter name' },
                        { min: 2, message: 'Name must be at least 2 characters' }
                    ]}
                >
                    <Input
                        placeholder="Enter transporter name"
                        size="large"
                    />
                </Form.Item>

                <div className="grid grid-cols-2 gap-4">
                    <Form.Item
                        label="Email"
                        name="email"
                        rules={[
                            { type: 'email', message: 'Please enter a valid email' }
                        ]}
                    >
                        <Input
                            placeholder="email@example.com"
                            size="large"
                        />
                    </Form.Item>

                    <Form.Item
                        label="Phone"
                        name="phone"
                        rules={[
                            { required: true, message: 'Please enter phone number' }
                        ]}
                    >
                        <Input
                            placeholder="+254712345678"
                            size="large"
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Address"
                    name="address"
                >
                    <Input
                        placeholder="Physical address"
                        size="large"
                    />
                </Form.Item>

                <Form.Item
                    label="License Number"
                    name="licenseNumber"
                >
                    <Input
                        placeholder="TRN-XXX"
                        size="large"
                    />
                </Form.Item>

                <Form.Item
                    label="Status"
                    name="status"
                    rules={[
                        { required: true, message: 'Please select status' }
                    ]}
                >
                    <Select size="large" placeholder="Select status">
                        <Option value="Active">Active</Option>
                        <Option value="Inactive">Inactive</Option>
                        <Option value="Suspended">Suspended</Option>
                    </Select>
                </Form.Item>

                <Form.Item
                    label="Logo URL"
                    name="logo"
                    help="Enter a URL for the transporter logo (optional)"
                >
                    <Input
                        placeholder="https://example.com/logo.png"
                        size="large"
                    />
                </Form.Item>
            </Form>
        </Modal>
    );
}