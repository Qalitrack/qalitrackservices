// src/pages/WeighingDashboard.jsx
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import {
  fetchTransactions,
  fetchVehicles,
  fetchVehiclesByRegNumber,
  fetchDrivers,
  fetchDriversByName,
  fetchProducts,
  fetchProductsByName,
  fetchRoutes,
  fetchRoutesByName,
  fetchSaccosByName,
  fetchSuppliersByName,
  fetchTransportersByName,
  addTransaction,
  fetchSimulatedWeight,
} from "../store/weighingSlice";
import { Form, Input, Select, Button, Spin, message } from "antd";
import { debounce } from "lodash";
import CameraGrid from "../components/CameraGrid";

const { Option } = Select;

/* ---------------------------------------------------------------------------
   Live Weighbridge Status
----------------------------------------------------------------------------*/
function LiveWeighbridgeStatus({ onManualCapture }) {
  const [totalWeight, setTotalWeight] = useState("--- kg");
  const [isStable, setIsStable] = useState(false);
  const bufferRef = React.useRef(null);
  const lastStableRef = React.useRef(null);
  const stabilityCounterRef = React.useRef(0);

  const STABILITY_CYCLES = 3;
  const UPDATE_INTERVAL_MS = 2000;

  // SSE connection
  useEffect(() => {
    const source = new EventSource("http://172.16.0.215:5000/api/PlatformData/stream");
    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data?.type === "total" && data?.weight !== undefined) {
          bufferRef.current = data.weight;
        }
      } catch (err) {
        console.error(err);
      }
    };
    return () => source.close();
  }, []);

  // Stabilization & UI update
  useEffect(() => {
    const interval = setInterval(() => {
      const current = bufferRef.current;
      if (current !== undefined && current !== null) {
        const formatted = typeof current === "number" ? `${current} kg` : String(current);
        setTotalWeight(formatted);

        if (current === lastStableRef.current) {
          stabilityCounterRef.current += 1;
        } else {
          lastStableRef.current = current;
          stabilityCounterRef.current = 1;
          setIsStable(false);
        }

        if (stabilityCounterRef.current >= STABILITY_CYCLES && !isStable) {
          setIsStable(true);
        }
      }
    }, UPDATE_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [isStable]);

  const handleCapture = () => {
    if (typeof onManualCapture === "function" && bufferRef.current !== null) {
      let val = bufferRef.current;
      if (typeof val === "string") val = Number(val.replace(/[^0-9.-]/g, ""));
      onManualCapture(val);
    }
  };

  return (
    <div className="bg-black text-amber-600 rounded-2xl shadow-xl p-6 w-full max-w-sm mx-auto">
      <h3 className="text-lg font-semibold tracking-widest text-center">LIVE WEIGHT</h3>
      <div className="text-5xl md:text-6xl font-mono font-bold mt-4 text-center">{totalWeight}</div>
      <div className="mt-3 text-sm text-amber-400 flex items-center justify-center">
        <span className={`w-3 h-3 rounded-full mr-2 ${isStable ? "bg-green-500 animate-pulse" : "bg-yellow-500"}`} />
        <span>{isStable ? "STABLE - Auto-capture available" : "Live reading"}</span>
      </div>
      <div className="flex justify-center mt-5">
        <button
          onClick={handleCapture}
          className="bg-amber-600 hover:bg-amber-700 text-black font-semibold px-4 py-2 rounded-lg shadow"
        >
          CAPTURE WEIGHT
        </button>
      </div>
      <CameraGrid />
    </div>
  );
}

/* ---------------------------------------------------------------------------
   Main Weighing Form integrated with LiveWeighbridgeStatus
----------------------------------------------------------------------------*/
export default function WeighingDashboard() {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, routes, suppliers, saccos, transporters, loading, error } =
    useSelector((state) => state.weighing || {});

  const [capturedWeight, setCapturedWeight] = useState(null);
  const [form] = Form.useForm();

  // Apply captured weight to W1 field immediately
  useEffect(() => {
    if (capturedWeight !== null && capturedWeight !== undefined) {
      form.setFieldsValue({ w1: capturedWeight });
    }
  }, [capturedWeight, form]);

  // Fetch master data
  useEffect(() => {
    dispatch(fetchVehicles());
    dispatch(fetchDrivers());
    dispatch(fetchProducts());
    dispatch(fetchRoutes());
    dispatch(fetchSaccosByName(""));
    dispatch(fetchSuppliersByName(""));
    dispatch(fetchTransportersByName(""));
    dispatch(fetchTransactions({ pageNumber: 1, pageSize: 50 }));
    dispatch(fetchSimulatedWeight());
  }, [dispatch]);

  useEffect(() => {
    if (error) message.error(error);
  }, [error]);

  const debounced = {
    vehicles: debounce((q) => dispatch(fetchVehiclesByRegNumber(q)), 400),
    drivers: debounce((q) => dispatch(fetchDriversByName(q)), 400),
    products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
    routes: debounce((q) => dispatch(fetchRoutesByName(q)), 400),
    suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
    saccos: debounce((q) => dispatch(fetchSaccosByName(q)), 400),
    transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
  };

  const makeOptions = (items = [], idField = "id", labelField = "name") =>
    items.map((it) => (
      <Option key={it[idField]} value={it[idField]}>
        {it[labelField] ?? it[idField]}
      </Option>
    ));

  const handleManualCapture = (weight) => {
    if (isNaN(weight)) {
      message.error("Captured weight is invalid.");
      return;
    }
    setCapturedWeight(weight);
    message.success(`Captured weight: ${weight} kg`);
  };

  const handleSubmit = async (values) => {
    try {
      const payload = {
        receiptNo: values.referenceNumber,
        expectedWeighings: values.w2 ? 2 : 1,
        vehicleId: values.vehicleId,
        noPlate: vehicles.find((v) => v.id === values.vehicleId)?.registrationNumber,
        driverName: drivers.find((d) => d.id === values.driverId)?.fullName,
        firstWeight: values.w1,
        secondWeight: values.w2,
        commodityId: values.productId,
        commodityName: products.find((p) => p.id === values.productId)?.name,
        supplierId: values.supplierId,
        supplierName: suppliers.find((s) => s.id === values.supplierId)?.name,
        transporterId: values.transporterId,
        transporterName: transporters.find((t) => t.id === values.transporterId)?.name,
        routeId: values.routeId,
        operation: values.operation,
        weighBridgeId: 1,
        weighBridgeName: "Weighbridge 1",
        operatorId: 1,
        operatorName: "Operator",
        weighMode: "Gross/Tare",
      };
      await dispatch(addTransaction(payload)).unwrap();
      message.success("Transaction saved!");
      form.resetFields();
      if (capturedWeight) form.setFieldsValue({ w1: capturedWeight });
      dispatch(fetchTransactions());
    } catch (err) {
      message.error(err?.message || "Error saving transaction.");
      console.error(err);
    }
  };

  return (
    <div className="p-6 max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-5 gap-6">
      <div className="lg:col-span-2">
        <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
      </div>

      <div className="lg:col-span-3 bg-white rounded-2xl p-6 shadow">
        <h2 className="text-xl font-semibold mb-4">Weighing Transaction Form</h2>
        {loading ? (
          <div className="flex items-center justify-center py-10">
            <Spin size="large" />
          </div>
        ) : (
          <Form form={form} layout="vertical" onFinish={handleSubmit} initialValues={{ w1: capturedWeight }}>
            <div className="grid grid-cols-2 gap-4">
              <Form.Item label="Delivery Note" name="deliveryNote" rules={[{ required: true }]}>
                <Input placeholder="Enter delivery note" />
              </Form.Item>

              <Form.Item label="Reference Number" name="referenceNumber" rules={[{ required: true }]}>
                <Input placeholder="Enter reference number" />
              </Form.Item>

              <Form.Item label="Batch Number" name="batchNumber" rules={[{ required: true }]}>
                <Input placeholder="Enter batch number" />
              </Form.Item>

              <Form.Item label="Vehicle" name="vehicleId" rules={[{ required: true }]}>
                <Select showSearch placeholder="Search vehicle" onSearch={debounced.vehicles} filterOption={false}>
                  {makeOptions(vehicles, "id", "registrationNumber")}
                </Select>
              </Form.Item>

              <Form.Item label="Driver" name="driverId" rules={[{ required: true }]}>
                <Select showSearch placeholder="Search driver" onSearch={debounced.drivers} filterOption={false}>
                  {makeOptions(drivers, "id", "fullName")}
                </Select>
              </Form.Item>

              <Form.Item label="Sacco" name="saccoId">
                <Select showSearch placeholder="Search sacco" onSearch={debounced.saccos} filterOption={false}>
                  {makeOptions(saccos)}
                </Select>
              </Form.Item>

              <Form.Item label="Route" name="routeId">
                <Select showSearch placeholder="Search route" onSearch={debounced.routes} filterOption={false}>
                  {makeOptions(routes)}
                </Select>
              </Form.Item>

              <Form.Item label="Product" name="productId" rules={[{ required: true }]}>
                <Select showSearch placeholder="Search product" onSearch={debounced.products} filterOption={false}>
                  {makeOptions(products)}
                </Select>
              </Form.Item>

              <Form.Item label="Supplier" name="supplierId">
                <Select showSearch placeholder="Search supplier" onSearch={debounced.suppliers} filterOption={false}>
                  {makeOptions(suppliers)}
                </Select>
              </Form.Item>

              <Form.Item label="Transporter" name="transporterId">
                <Select
                  showSearch
                  placeholder="Search transporter"
                  onSearch={debounced.transporters}
                  filterOption={false}
                >
                  {makeOptions(transporters)}
                </Select>
              </Form.Item>

              <Form.Item label="Weight 1 (W1)" name="w1" rules={[{ required: true }]}>
                <Input type="number" readOnly className="bg-gray-100 cursor-not-allowed" />
              </Form.Item>

              <Form.Item label="Weight 2 (W2)" name="w2">
                <Input type="number" placeholder="Optional second weighing" />
              </Form.Item>

              <Form.Item label="Operation" name="operation" rules={[{ required: true }]}>
                <Select placeholder="Select operation">
                  <Option value="Inbound Product Receipt">Inbound Product Receipt</Option>
                  <Option value="Outbound Product Dispatch">Outbound Product Dispatch</Option>
                </Select>
              </Form.Item>

              <div className="col-span-2 flex justify-end">
                <Button type="primary" htmlType="submit" className="bg-amber-500 hover:bg-amber-600" loading={loading}>
                  Save Transaction
                </Button>
              </div>
            </div>
          </Form>
        )}
      </div>
    </div>
  );
}
