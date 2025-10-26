import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  fetchVehicles,
  fetchVehiclesByName,
  fetchDrivers,
  fetchDriversByName,
  fetchProducts,
  fetchProductsByName,
  fetchRoutes,
  fetchRoutesByName,
  fetchSaccosByName,
  fetchSaccoById,
  fetchSupplierById,
  fetchSuppliersByName,
  fetchTransporterById,
  fetchTransportersByName,
  addTransaction,
} from "../store/weighingSlice";
import { Input, Select, Button, Form, Spin, message } from "antd";
import { debounce } from "lodash"; // Ensure lodash is installed: npm install lodash

const WeighingForm = () => {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, routes, suppliers, saccos, transporters, loading, error } =
    useSelector((state) => state.weighing);

  console.log("State:", { vehicles, drivers, products, routes, suppliers, saccos, transporters }); // Debug log

  const [form] = Form.useForm();

  // Fetch master data on mount
  useEffect(() => {
    dispatch(fetchVehicles());
    dispatch(fetchDrivers());
    dispatch(fetchProducts());
    dispatch(fetchRoutes());
    dispatch(fetchSaccosByName("")); // Initial fetch for saccos
  }, [dispatch]);

  // Fetch linked entities by ID when selected
  useEffect(() => {
    const supplierId = form.getFieldValue("supplierId");
    const saccoId = form.getFieldValue("saccoId");
    const transporterId = form.getFieldValue("transporterId");
    const vehicleId = form.getFieldValue("vehicleId");
    const driverId = form.getFieldValue("driverId");

    if (supplierId) dispatch(fetchSupplierById(supplierId));
    if (saccoId) dispatch(fetchSaccoById(saccoId));
    if (transporterId) dispatch(fetchTransporterById(transporterId));
    if (vehicleId) dispatch(fetchVehicleById(vehicleId));
    if (driverId) dispatch(fetchDriverById(driverId));
  }, [dispatch, form]);

  // Display error messages
  useEffect(() => {
    if (error) {
      message.error(`Error: ${error}`);
    }
  }, [error]);

  // Debounced search handlers
  const debouncedSearch = {
    vehicles: debounce((name) => dispatch(fetchVehiclesByName(name)), 500),
    drivers: debounce((name) => dispatch(fetchDriversByName(name)), 500),
    products: debounce((name) => dispatch(fetchProductsByName(name)), 500),
    routes: debounce((name) => dispatch(fetchRoutesByName(name)), 500),
    suppliers: debounce((name) => dispatch(fetchSuppliersByName(name)), 500),
    saccos: debounce((name) => dispatch(fetchSaccosByName(name)), 500),
    transporters: debounce((name) => dispatch(fetchTransportersByName(name)), 500),
  };

  const handleSubmit = (values) => {
    try {
      dispatch(addTransaction({ id: Date.now().toString(), ...values }));
      message.success("Weighing transaction added successfully!");
      form.resetFields();
    } catch (err) {
      message.error("Error adding transaction");
    }
  };

  const handleManualInput = (value) => value;

  // Consistent filter for Select components
  const filterOption = (input, option) =>
    option?.children?.toLowerCase().includes(input.toLowerCase());

  // Filter out items with null/undefined values and customize display/value
  const getValidOptions = (items, valueField, displayField) =>
    Array.isArray(items)
      ? items
          .filter((item) => item?.[valueField] && item[valueField] !== null && item[valueField] !== undefined)
          .map((item) => (
            <Select.Option key={item[valueField]} value={item[valueField]}>
              {item[displayField] || item[valueField]}
            </Select.Option>
          ))
      : [];

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
            name="vehicleId"
            rules={[{ required: true, message: "Please select or type vehicle ID" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type vehicle ID"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.vehicles(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading vehicles</span> : null}
            >
              {getValidOptions(vehicles, "id", "id")}
              {!vehicles?.length && !error && <Select.Option disabled>No vehicles available</Select.Option>}
            </Select>
          </Form.Item>

          {/* DRIVER */}
          <Form.Item
            label="Driver"
            name="driverId"
            rules={[{ required: true, message: "Please select or type driver" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type driver name"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.drivers(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading drivers</span> : null}
            >
              {getValidOptions(drivers, "fullName", "fullName")}
              {!drivers?.length && !error && <Select.Option disabled>No drivers available</Select.Option>}
            </Select>
          </Form.Item>

          {/* SACCO */}
          <Form.Item
            label="Sacco"
            name="saccoId"
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type sacco"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.saccos(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading saccos</span> : null}
            >
              {getValidOptions(saccos, "name", "name")}
              {!saccos?.length && !error && <Select.Option disabled>No saccos available</Select.Option>}
            </Select>
          </Form.Item>

          {/* ROUTE */}
          <Form.Item
            label="Route"
            name="routeId"
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type route"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.routes(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading routes</span> : null}
            >
              {getValidOptions(routes, "name", "name")}
              {!routes?.length && !error && <Select.Option disabled>No routes available</Select.Option>}
            </Select>
          </Form.Item>

          {/* PRODUCT - Assumed pattern */}
          <Form.Item
            label="Product"
            name="productId"
            rules={[{ required: true, message: "Please select or type product" }]}
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type product name"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.products(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading products</span> : null}
            >
              {getValidOptions(products, "name", "name")}
              {!products?.length && !error && <Select.Option disabled>No products available</Select.Option>}
            </Select>
          </Form.Item>

          {/* SUPPLIER - Assumed pattern */}
          <Form.Item
            label="Supplier"
            name="supplierId"
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type supplier"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.suppliers(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading suppliers</span> : null}
            >
              {getValidOptions(suppliers, "name", "name")}
              {!suppliers?.length && !error && <Select.Option disabled>No suppliers available</Select.Option>}
            </Select>
          </Form.Item>

          {/* TRANSPORTER - Assumed pattern */}
          <Form.Item
            label="Transporter"
            name="transporterId"
          >
            <Select
              showSearch
              mode="combobox"
              placeholder="Search or type transporter"
              onChange={handleManualInput}
              onSearch={(value) => debouncedSearch.transporters(value)}
              filterOption={filterOption}
              notFoundContent={error ? <span>Error loading transporters</span> : null}
            >
              {getValidOptions(transporters, "name", "name")}
              {!transporters?.length && !error && <Select.Option disabled>No transporters available</Select.Option>}
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