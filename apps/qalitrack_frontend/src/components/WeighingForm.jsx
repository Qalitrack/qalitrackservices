// src/components/WeighingForm.jsx
import React, { useEffect, useRef, useState } from "react";
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
} from "../store/weighingSlice";

import { Input, Select, Button, Form, Spin, message } from "antd";
import { debounce } from "lodash";

/* ---------------------------------------------------------------------------
   LiveWeighbridgeStatus (No changes here, preserving your SSE logic)
----------------------------------------------------------------------------*/
function LiveWeighbridgeStatus({ onManualCapture, onWeightStable }) {
  const [totalWeight, setTotalWeight] = useState("--- kg");
  const [isStable, setIsStable] = useState(false);

  const bufferRef = useRef(null); // latest raw weight value from SSE
  const lastReportedStableWeightRef = useRef(null);
  const stabilizationCounterRef = useRef(0);

  // config
  const STABILITY_CYCLES = 3;
  const UPDATE_INTERVAL_MS = 2000;

  // SSE: high-frequency feed (store raw value in buffer only)
  useEffect(() => {
    // NOTE: Replace with your actual SSE endpoint
    const source = new EventSource("http://172.16.0.215:5000/api/PlatformData/stream");

    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data?.type === "total" && data?.weight !== undefined) {
          bufferRef.current = data.weight;
        }
      } catch (err) {
        console.error("SSE parse error:", err);
      }
    };

    source.onerror = (err) => {
      console.error("SSE error:", err);
    };

    return () => {
      try {
        source.close();
      } catch (e) {
        /* ignore close errors */
      }
    };
  }, []);

  // UI update + stabilization check (lower frequency)
  useEffect(() => {
    const interval = setInterval(() => {
      const currentBufferWeight = bufferRef.current;

      if (currentBufferWeight !== null && currentBufferWeight !== undefined) {
        // IMPORTANT: Ensure this part (display) uses the raw buffered weight, 
        // which includes the 'kg' unit if the SSE stream provides it.
        const formatted =
          typeof currentBufferWeight === "number"
            ? `${currentBufferWeight} kg`
            : String(currentBufferWeight);

        setTotalWeight(formatted);

        const currentWeightString = String(currentBufferWeight);
        const lastReportedString = String(lastReportedStableWeightRef.current);

        if (currentWeightString === lastReportedString) {
          stabilizationCounterRef.current += 1;
        } else {
          stabilizationCounterRef.current = 1;
          lastReportedStableWeightRef.current = currentBufferWeight;
          setIsStable(false);
        }

        if (stabilizationCounterRef.current >= STABILITY_CYCLES && !isStable) {
          if (typeof onWeightStable === "function") {
            try {
              onWeightStable(currentBufferWeight);
            } catch (e) {
              console.error("onWeightStable error:", e);
            }
          }
          setIsStable(true);
          console.log("Weight stabilized:", currentBufferWeight);
        }
      }
    }, UPDATE_INTERVAL_MS);

    return () => clearInterval(interval);
  }, [onWeightStable, isStable]);

  // Manual capture button reads buffered latest value and calls parent callback
  const handleCapture = () => {
    const current = bufferRef.current;
    if (current === null || current === undefined) {
      message.warning("No live weight available to capture yet.");
      return;
    }
    if (typeof onManualCapture === "function") {
      onManualCapture(current);
    }
  };

  return (
    <div className="bg-black text-amber-600 rounded-2xl shadow-xl p-6 mb-6 w-full max-w-sm mx-auto">
      <h3 className="text-lg font-semibold tracking-widest text-center">LIVE WEIGHT</h3>

      <div className="text-5xl md:text-6xl font-mono font-bold mt-4 text-center">
        {totalWeight}
      </div>

      <div className="mt-3 text-sm text-amber-400 flex items-center justify-center">
        <span
          className={`w-3 h-3 rounded-full mr-2 ${isStable ? "bg-green-500 animate-pulse" : "bg-yellow-500"}`}
        />
        <span>{isStable ? "STABLE - Auto-capture available" : "Live reading"}</span>
      </div>

      <div className="flex justify-center mt-5">
        <button
          onClick={handleCapture}
          className="bg-amber-600 hover:bg-amber-700 text-black font-semibold px-4 py-2 rounded-lg shadow"
          type="button"
        >
          CAPTURE WEIGHT
        </button>
      </div>
    </div>
  );
}

/* ---------------------------------------------------------------------------
   Main: WeighingForm 
----------------------------------------------------------------------------*/
export default function WeighingForm() {
  const dispatch = useDispatch();
  const {
    vehicles,
    drivers,
    products,
    routes,
    suppliers,
    saccos,
    transporters,
    loading,
    error,
  } = useSelector((state) => state.weighing || {});

  const [capturedWeight, setCapturedWeight] = useState(null);
  const [form] = Form.useForm();

  // apply captured weight into W1 immediately
  useEffect(() => {
    // Only sets the field if capturedWeight is a valid number
    if (capturedWeight !== null && capturedWeight !== undefined) {
      form.setFieldsValue({ w1: capturedWeight });
    }
  }, [capturedWeight, form]);

  // fetch master data on mount
  useEffect(() => {
    dispatch(fetchVehicles());
    dispatch(fetchDrivers());
    dispatch(fetchProducts());
    dispatch(fetchRoutes());
    dispatch(fetchSaccosByName(""));
    dispatch(fetchSuppliersByName(""));
    dispatch(fetchTransportersByName(""));
  }, [dispatch]);

  // show errors from redux slice
  useEffect(() => {
    if (error) message.error(error);
  }, [error]);

  // debounced searches (implementation omitted for brevity, but calls the search thunks)
  const debounced = {
    vehicles: debounce((q) => dispatch(fetchVehiclesByRegNumber(q)), 400),
    drivers: debounce((q) => dispatch(fetchDriversByName(q)), 400),
    products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
    routes: debounce((q) => dispatch(fetchRoutesByName(q)), 400),
    suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
    saccos: debounce((q) => dispatch(fetchSaccosByName(q)), 400),
    transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
  };

  // Convert lists to Select.Option elements safely
  const makeOptions = (items = [], idField = "id", labelField = "name") =>
    items.map((it) => (
      <Select.Option key={it[idField]} value={it[idField]}>
        {it[labelField] ?? it[idField]}
      </Select.Option>
    ));

  const handleManualCapture = (weight) => {
    let numeric = weight;
    
    // --------------------------------------------------------
    // *** FIX APPLIED HERE ***
    // 1. Clean the string to remove units (e.g., "50 kg" -> 50)
    if (typeof weight === "string") {
        // Regex to keep only digits, dots, and negative signs
        const cleanedString = weight.replace(/[^0-9.-]/g, ''); 
        numeric = Number(cleanedString);
    }
    // --------------------------------------------------------
    
    // 2. Validation
    if (isNaN(numeric) || numeric === null) {
        message.error("Failed to parse weight value. Ensure the stream sends a number.");
        return;
    }

    setCapturedWeight(numeric);
    message.success(`Captured weight: ${numeric} kg`);
  };

  const handleSubmit = async (values) => {
    const getName = (list, id, field) => {
        const item = list.find(item => item.id === id);
        return item ? item[field] : null;
    };
    
    // 1. Build the API Payload (Mapping form fields to API fields)
    const payload = {
      // Required Identifiers
      receiptNo: values.referenceNumber, 
      expectedWeighings: values.w2 ? 2 : 1, 
      
      // Vehicle and Driver Details
      vehicleId: values.vehicleId,
      noPlate: getName(vehicles, values.vehicleId, "registrationNumber"),
      driverName: getName(drivers, values.driverId, "fullName"),
      
      // Weights
      firstWeight: values.w1 !== undefined && values.w1 !== null ? Number(values.w1) : 0,
      secondWeight: values.w2 !== undefined && values.w2 !== null ? Number(values.w2) : 0,
      
      // Commodity/Product
      commodityId: values.productId,
      commodityName: getName(products, values.productId, "name"),
      
      // Transporter/Supplier
      transporterId: values.transporterId,
      transporterName: getName(transporters, values.transporterId, "name"),
      supplierId: values.supplierId,
      supplierName: getName(suppliers, values.supplierId, "name"),

      // Operation
      operation: values.operation,
      
      // Default/Placeholder values (assuming fixed values for simplicity)
      weighBridgeId: 1, 
      weighBridgeName: "Weighbridge 1",
      operatorId: 1, 
      operatorName: "Operator",
      weighMode: "Gross/Tare", 
      // ... other required fields ...
    };
    
    try {
      // 2. Save transaction to the database
      await dispatch(addTransaction(payload)).unwrap();
      
      message.success("Weighing transaction saved successfully!");
      form.resetFields();
      
      // 3. Reapply captured weight (if W1 needs to persist)
      if (capturedWeight !== null && capturedWeight !== undefined) {
        form.setFieldsValue({ w1: capturedWeight });
      }
      
      // 4. *** INTEGRATION STEP: Refresh the transactions list ***
      dispatch(fetchTransactions()); 
      // --------------------------------------------------------

    } catch (err) {
      const errMsg = err?.response?.data?.message || err?.message || "Error saving transaction. Check console for details.";
      message.error(errMsg);
      console.error("Save transaction error:", err);
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-1">
          <LiveWeighbridgeStatus onManualCapture={handleManualCapture} onWeightStable={(w) => {}} />
        </div>

        <div className="md:col-span-2 bg-white rounded-2xl p-6 shadow">
          <h2 className="text-xl font-semibold mb-4">Weighing Transaction Form</h2>

          {loading ? (
            <div className="flex items-center justify-center py-10"><Spin size="large" /></div>
          ) : (
            <Form
              layout="vertical"
              form={form}
              onFinish={handleSubmit}
              initialValues={{ w1: capturedWeight ?? undefined }}
              className="grid grid-cols-2 gap-4"
            >
              {/* DELIVERY NOTE */}
              <Form.Item
                label="Delivery Note"
                name="deliveryNote"
                rules={[{ required: true, message: "Enter delivery note" }]}
              >
                <Input placeholder="Enter delivery note" />
              </Form.Item>

              {/* REFERENCE NUMBER */}
              <Form.Item
                label="Reference Number"
                name="referenceNumber"
                rules={[{ required: true, message: "Enter reference number" }]}
              >
                <Input placeholder="Enter reference number" />
              </Form.Item>

              {/* BATCH NUMBER */}
              <Form.Item
                label="Batch Number"
                name="batchNumber"
                rules={[{ required: true, message: "Enter batch number" }]}
              >
                <Input placeholder="Enter batch number" />
              </Form.Item>

              {/* VEHICLE */}
              <Form.Item
                label="Vehicle"
                name="vehicleId"
                rules={[{ required: true, message: "Select vehicle" }]}
              >
                <Select
                  showSearch
                  placeholder="Search vehicle by reg"
                  onSearch={debounced.vehicles}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(vehicles, "id", "registrationNumber")}
                </Select>
              </Form.Item>

              {/* DRIVER */}
              <Form.Item
                label="Driver"
                name="driverId"
                rules={[{ required: true, message: "Select driver" }]}
              >
                <Select
                  showSearch
                  placeholder="Search driver"
                  onSearch={debounced.drivers}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(drivers, "id", "fullName")}
                </Select>
              </Form.Item>

              {/* SACCO */}
              <Form.Item label="Sacco" name="saccoId">
                <Select
                  showSearch
                  placeholder="Search sacco"
                  onSearch={debounced.saccos}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(saccos, "id", "name")}
                </Select>
              </Form.Item>

              {/* ROUTE */}
              <Form.Item label="Route" name="routeId">
                <Select
                  showSearch
                  placeholder="Search route"
                  onSearch={debounced.routes}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(routes, "id", "name")}
                </Select>
              </Form.Item>

              {/* PRODUCT */}
              <Form.Item label="Product" name="productId" rules={[{ required: true, message: "Select product" }]}>
                <Select
                  showSearch
                  placeholder="Search product"
                  onSearch={debounced.products}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(products, "id", "name")}
                </Select>
              </Form.Item>

              {/* SUPPLIER */}
              <Form.Item label="Supplier" name="supplierId">
                <Select
                  showSearch
                  placeholder="Search supplier"
                  onSearch={debounced.suppliers}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(suppliers, "id", "name")}
                </Select>
              </Form.Item>

              {/* TRANSPORTER */}
              <Form.Item label="Transporter" name="transporterId">
                <Select
                  showSearch
                  placeholder="Search transporter"
                  onSearch={debounced.transporters}
                  filterOption={false}
                  allowClear
                >
                  {makeOptions(transporters, "id", "name")}
                </Select>
              </Form.Item>

              {/* W1 (captured) */}
              <Form.Item
                label="Weight 1 (W1)"
                name="w1"
                rules={[{ required: true, message: "Capture or enter W1" }]}
              >
                <Input
                  type="number"
                  readOnly
                  className="bg-gray-100 cursor-not-allowed"
                  placeholder="Click CAPTURE on the live card"
                />
              </Form.Item>

              {/* W2 */}
              <Form.Item label="Weight 2 (W2)" name="w2">
                <Input type="number" placeholder="Optional: second pass (W2)" />
              </Form.Item>

              {/* OPERATION */}
              <Form.Item
                label="Operation"
                name="operation"
                rules={[{ required: true, message: "Select operation type" }]}
              >
                <Select placeholder="Select operation">
                  <Select.Option value="Inbound Product Receipt">Inbound Product Receipt</Select.Option>
                  <Select.Option value="Outbound Product Dispatch">Outbound Product Dispatch</Select.Option>
                </Select>
              </Form.Item>

              {/* Submit button */}
              <div className="col-span-2 flex justify-end">
                <Button
                  type="primary"
                  htmlType="submit"
                  className="bg-amber-500 hover:bg-amber-600"
                  loading={loading}
                >
                  Save Transaction
                </Button>
              </div>
            </Form>
          )}
        </div>
      </div>
    </div>
  );
}