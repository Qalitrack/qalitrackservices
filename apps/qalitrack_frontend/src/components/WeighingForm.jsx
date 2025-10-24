import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  fetchVehicles,
  fetchDrivers,
  fetchProducts,
  fetchRoutes,
  fetchSaccoById,
  fetchSupplierById,
  fetchTransporterById,
  addTransaction,
} from "../store/weighingSlice";
import { Input, Select, Button, Form, Spin, message } from "antd";

const WeighingForm = () => {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, routes, suppliers, saccos, transporters, loading } =
    useSelector((state) => state.weighing);

  const [form] = Form.useForm();

  // Fetch master data
  useEffect(() => {
    dispatch(fetchVehicles());
    dispatch(fetchDrivers());
    dispatch(fetchProducts());
    dispatch(fetchRoutes());
  }, [dispatch]);

  // Fetch linked entities by ID if selected
  useEffect(() => {
    const supplierId = form.getFieldValue("supplierId");
    const saccoId = form.getFieldValue("saccoId");
    const transporterId = form.getFieldValue("transporterId");

    if (supplierId) dispatch(fetchSupplierById(supplierId));
    if (saccoId) dispatch(fetchSaccoById(saccoId));
    if (transporterId) dispatch(fetchTransporterById(transporterId));
  }, [dispatch, form]);

  const handleSubmit = (values) => {
    try {
      dispatch(addTransaction(values));
      message.success("Weighing transaction added successfully!");
      form.resetFields();
    } catch (err) {
      message.error("Error adding transaction");
    }
  };

  const handleManualInput = (value) => value;

  return (
    <div className="p-6 bg-white rounded-2xl shadow-md max-w-4xl mx-auto">
      <h2 className="text-xl font-semibold mb-4">Weighing Transaction Form</h2>

      {loading ? (
        <div className="flex justify-center items-center p-6">
          <Spin size="large" />
        </div>
      ) : (
        <Form
          layout="vertical"
          form={form}
          onFinish={handleSubmit}
          className="grid grid-cols-2 gap-4"
        >
          {/* VEHICLE */}
          <Form.Item
            label="Vehicle"
            name="vehicle"
            rules={[{ required: true, message: "Please select or type vehicle" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type vehicle"
              onChange={handleManualInput}
              filterOption={(input, option) =>
                option?.children?.toLowerCase().includes(input.toLowerCase())
              }
            >
              {Array.isArray(vehicles) &&
                vehicles.map((v) => (
                  <Select.Option key={v.id} value={v.registrationNumber}>
                    {v.registrationNumber}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* DRIVER */}
          <Form.Item
            label="Driver"
            name="driver"
            rules={[{ required: true, message: "Please select or type driver" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type driver"
              onChange={handleManualInput}
            >
              {Array.isArray(drivers) &&
                drivers.map((d) => (
                  <Select.Option key={d.id} value={d.name}>
                    {d.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* PRODUCT */}
          <Form.Item
            label="Product"
            name="product"
            rules={[{ required: true, message: "Please select or type product" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type product"
              onChange={handleManualInput}
            >
              {Array.isArray(products) &&
                products.map((p) => (
                  <Select.Option key={p.id} value={p.name}>
                    {p.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* SUPPLIER */}
          <Form.Item label="Supplier" name="supplier">
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type supplier"
              onChange={handleManualInput}
            >
              {Array.isArray(suppliers) &&
                suppliers.map((s) => (
                  <Select.Option key={s.id} value={s.name}>
                    {s.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* TRANSPORTER */}
          <Form.Item label="Transporter" name="transporter">
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type transporter"
              onChange={handleManualInput}
            >
              {Array.isArray(transporters) &&
                transporters.map((t) => (
                  <Select.Option key={t.id} value={t.name}>
                    {t.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* SACCO */}
          <Form.Item label="Sacco" name="sacco">
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type sacco"
              onChange={handleManualInput}
            >
              {Array.isArray(saccos) &&
                saccos.map((s) => (
                  <Select.Option key={s.id} value={s.name}>
                    {s.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* ROUTE */}
          <Form.Item label="Route" name="route">
            <Select
              showSearch
              mode="combobox"
              placeholder="Select or type route"
              onChange={handleManualInput}
            >
              {Array.isArray(routes) &&
                routes.map((r) => (
                  <Select.Option key={r.id} value={r.name}>
                    {r.name}
                  </Select.Option>
                ))}
            </Select>
          </Form.Item>

          {/* WEIGHT 1 */}
          <Form.Item
            label="Weight 1 (W1)"
            name="w1"
            rules={[{ required: true, message: "Enter weight 1" }]}
          >
            <Input type="number" placeholder="Enter W1" />
          </Form.Item>

          {/* OPERATION */}
          <Form.Item
            label="Operation"
            name="operation"
            rules={[{ required: true, message: "Select operation type" }]}
          >
            <Select placeholder="Select operation">
              <Select.Option value="Inbound Product Receipt">
                Inbound Product Receipt
              </Select.Option>
              <Select.Option value="Outbound Product Dispatch">
                Outbound Product Dispatch
              </Select.Option>
            </Select>
          </Form.Item>

          <div className="col-span-2 flex justify-end">
            <Button
              type="primary"
              htmlType="submit"
              className="bg-amber-500 hover:bg-amber-600"
            >
              Save Transaction
            </Button>
          </div>
        </Form>
      )}
    </div>
  );
};

export default WeighingForm;
