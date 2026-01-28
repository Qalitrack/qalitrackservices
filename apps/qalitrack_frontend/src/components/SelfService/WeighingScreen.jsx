import React, { useEffect, useState, useRef, useMemo } from "react";
import { Input, Select, Button, Typography, message, Alert, Tag, Spin } from "antd";
import { debounce } from "lodash";
import { useDispatch, useSelector } from "react-redux";

import {
  fetchVehiclesByRegNumber,
  fetchDriversByName,
  fetchProductsByName,
  fetchSuppliersByName,
  fetchTransportersByName,
  fetchWeighbridges,
  addTransaction,
  fetchCurrentUser,
} from "../../store/weighingSlice";

const { Option } = Select;

export default function WeighingScreen({
  vehicleData = {},
  driverData = {},
  onWeighingComplete,
  onTransactionCreated,
}) {
  const dispatch = useDispatch();
  const {
    vehicles = [],
    drivers = [],
    transporters = [],
    products = [],
    suppliers = [],
    weighbridges = [],
    currentUser,
    loading: globalLoading,
  } = useSelector((state) => state.weighing);

  const [formData, setFormData] = useState({
    noPlate: vehicleData?.plateNumber || "KBU 510 G",
    driverName: driverData?.fullName || "John Kamau",
    transporterID: null,
    transporterName: "",
    commodityID: null,
    commodityName: "",
    supplierID: null,
    supplierName: "",
    customerName: "",
    originName: "",
    destinationName: "",
    weighBridgeID: null,
    weighBridgeName: "",
    scaleName: "",
    operatorID: null,
    operatorName: "",
    weighMode: "entry",
    operation: "weighing",
    notes: "",
  });

  const [totalWeight, setTotalWeight] = useState(0);
  const [isStable, setIsStable] = useState(false);
  const [capturing, setCapturing] = useState(false);
  const [apiError, setApiError] = useState(null);

  const bufferRef = useRef(null);
  const lastStableRef = useRef(null);
  const stabilityCounterRef = useRef(0);
  const iterationCountRef = useRef(0);

  const STABILITY_CYCLES = 5;

  /* LOAD DATA */
  useEffect(() => {
    dispatch(fetchWeighbridges({ pageNumber: 1, pageSize: 50 }));
    dispatch(fetchCurrentUser());
  }, [dispatch]);

  useEffect(() => {
    if (currentUser) {
      setFormData((p) => ({
        ...p,
        operatorName:
          currentUser.fullName ||
          `${currentUser.firstName || ""} ${currentUser.lastName || ""}`.trim() ||
          currentUser.username ||
          "Operator",
        operatorID: currentUser.id || currentUser.userId || "system-operator",
      }));
    }
  }, [currentUser]);

  /* SIMULATE WEIGHT */
  useEffect(() => {
    const base = 19011;
    let iter = 0;
    const interval = setInterval(() => {
      iter++;
      iterationCountRef.current = iter;
      const w =
        iter < 6 ? base + Math.floor(Math.random() * 40 - 20) : base;
      bufferRef.current = w;
      setTotalWeight(w);
    }, 1200);
    return () => clearInterval(interval);
  }, []);

  /* STABILITY CHECK */
  useEffect(() => {
    const interval = setInterval(() => {
      const curr = bufferRef.current;
      if (curr == null) return;

      if (curr === lastStableRef.current) {
        stabilityCounterRef.current++;
        if (stabilityCounterRef.current >= STABILITY_CYCLES) {
          setIsStable(true);
        }
      } else {
        lastStableRef.current = curr;
        stabilityCounterRef.current = 1;
        setIsStable(false);
      }
    }, 800);
    return () => clearInterval(interval);
  }, []);

  /* SEARCH */
  const debounced = useMemo(
    () => ({
      transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
      products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
      suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
    }),
    [dispatch]
  );

  const handleChange = (field, value) =>
    setFormData((p) => ({ ...p, [field]: value }));

  const handleSelect = (list, id, idKey, nameKey) => {
    const item = list.find((i) => i.id === id);
    setFormData((p) => ({
      ...p,
      [idKey]: id,
      [nameKey]: item?.name || item?.fullName || item?.registrationNumber || item?.plateNumber || "",
    }));
  };

  const handleScaleSelect = (value) => {
    const selected = weighbridges.find((wb) => wb.location === value || wb.name === value);
    setFormData((prev) => ({
      ...prev,
      weighBridgeID: selected?.id || null,
      weighBridgeName: value || "",
      scaleName: value || "",
    }));
  };

  const handleCapture = async () => {
    if (!isStable || capturing) return;
    setCapturing(true);
    setApiError(null);

    try {
      const payload = {
        noPlate: formData.noPlate.toUpperCase().trim(),
        driverName: formData.driverName || "",
        firstWeight: String(totalWeight),
        transporterID: formData.transporterID,
        transporterName: formData.transporterName || "",
        weighBridgeID: formData.weighBridgeID,
        weighBridgeName: formData.weighBridgeName || formData.scaleName || "",
        scaleName: formData.scaleName || "",
        operatorID: formData.operatorID,
        operatorName: formData.operatorName || "Operator",
        commodityID: formData.commodityID,
        commodityName: formData.commodityName || "",
        supplierID: formData.supplierID,
        supplierName: formData.supplierName || "",
        customerName: formData.customerName || "",
        originName: formData.originName || "",
        destinationName: formData.destinationName || "",
        weighMode: formData.weighMode,
        operation: formData.operation,
        notes: formData.notes || "",
      };

      await dispatch(addTransaction(payload)).unwrap();
      message.success("Transaction saved");
      onWeighingComplete?.();
      onTransactionCreated?.();
    } catch (e) {
      setApiError("Failed to save transaction");
      message.error("Failed to save");
    } finally {
      setCapturing(false);
    }
  };

  return (
    <div className="h-screen w-screen bg-gray-100 flex flex-col">
      {/* HEADER */}
      <header className="h-16 bg-slate-800 text-white flex justify-between items-center px-6">
        <div>
          <h1 className="font-semibold">Self-Service Weighing</h1>
          <p className="text-xs text-gray-300">
            Automated Tea Collection System
          </p>
        </div>
        <div className="flex items-center gap-4 text-sm">
          <Tag color="green">LIVE</Tag>
          <span>{formData.operatorName || "Operator"}</span>
        </div>
      </header>

      {/* MAIN GRID */}
      <div className="flex-1 grid grid-cols-12 gap-4 p-4">
        {/* VEHICLE */}
        <div className="col-span-3 bg-white rounded shadow p-4">
          <h3 className="text-sm font-semibold text-gray-600 mb-2">
            ANPR NUMBER PLATE
          </h3>
          <div className="text-3xl font-bold text-center border py-4">
            {formData.noPlate}
          </div>
          <div className="mt-4 h-32 bg-gray-200 flex items-center justify-center text-gray-500">
            Vehicle Image
          </div>
        </div>

        {/* DRIVER */}
        <div className="col-span-4 bg-white rounded shadow p-4">
          <h3 className="text-sm font-semibold text-gray-600 mb-3">
            DRIVER DETAILS
          </h3>
          <div className="text-sm space-y-1">
            <p><b>Name:</b> {formData.driverName}</p>
            <p><b>License:</b> DL-94502</p>
            <p><b>ID:</b> 28000000</p>
            <p><b>Company:</b> Transport Ltd</p>
          </div>
        </div>

        {/* WEIGHT */}
        <div className="col-span-5 bg-black rounded shadow flex flex-col items-center justify-center">
          <div className="text-green-500 text-6xl lg:text-8xl font-mono font-bold">
            {totalWeight.toLocaleString()}
          </div>
          <div className="text-green-400 text-lg mt-2">KG</div>
          <div className="mt-4 text-base">
            {isStable ? (
              <span className="text-green-500">● Stable</span>
            ) : (
              <span className="text-yellow-400">● Stabilizing</span>
            )}
          </div>
        </div>

        {/* ORDER DETAILS – All fields from CreateTransactionForm */}
        <div className="col-span-12 bg-white rounded shadow p-4 grid grid-cols-6 gap-4">
          {/* Transporter */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Transporter *</label>
            <Select 
              size="middle" 
              className="w-full" 
              showSearch 
              onSearch={debounced.transporters}
              onChange={(id) => handleSelect(transporters, id, "transporterID", "transporterName")}
              value={formData.transporterID}
              placeholder="Select..."
            >
              {transporters.map(t => (
                <Option key={t.id} value={t.id}>{t.name}</Option>
              ))}
            </Select>
          </div>

          {/* Commodity */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Commodity</label>
            <Select 
              size="middle" 
              className="w-full" 
              showSearch 
              onSearch={debounced.products}
              onChange={(id) => handleSelect(products, id, "commodityID", "commodityName")}
              value={formData.commodityID}
              placeholder="Select..."
            >
              {products.map(p => (
                <Option key={p.id} value={p.id}>{p.name}</Option>
              ))}
            </Select>
          </div>

          {/* Supplier */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Supplier</label>
            <Select 
              size="middle" 
              className="w-full" 
              showSearch 
              onSearch={debounced.suppliers}
              onChange={(id) => handleSelect(suppliers, id, "supplierID", "supplierName")}
              value={formData.supplierID}
              placeholder="Select..."
            >
              {suppliers.map(s => (
                <Option key={s.id} value={s.id}>{s.name}</Option>
              ))}
            </Select>
          </div>

          {/* Customer */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Customer</label>
            <Input 
              size="middle" 
              placeholder="Enter customer" 
              value={formData.customerName}
              onChange={e => handleChange("customerName", e.target.value)}
            />
          </div>

          {/* Origin */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Origin</label>
            <Input 
              size="middle" 
              placeholder="Origin" 
              value={formData.originName}
              onChange={e => handleChange("originName", e.target.value)}
            />
          </div>

          {/* Destination */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Destination</label>
            <Input 
              size="middle" 
              placeholder="Destination" 
              value={formData.destinationName}
              onChange={e => handleChange("destinationName", e.target.value)}
            />
          </div>

          {/* Scale Name */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Scale Name *</label>
            <Select 
              size="middle" 
              className="w-full" 
              showSearch 
              onChange={handleScaleSelect}
              value={formData.scaleName}
              placeholder="Select scale"
            >
              {weighbridges.map((wb) => (
                <Option key={wb.id} value={wb.location || wb.name}>
                  {wb.location || wb.name}
                </Option>
              ))}
            </Select>
          </div>

          {/* Weigh Mode */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Weigh Mode</label>
            <Select 
              size="middle" 
              className="w-full" 
              value={formData.weighMode}
              onChange={(v) => handleChange("weighMode", v)}
            >
              <Option value="entry">Entry</Option>
              <Option value="Gross/Tare">Kiosk</Option>
            </Select>
          </div>

          {/* Operation Type */}
          <div>
            <label className="block text-xs text-gray-600 mb-1">Operation Type</label>
            <Select 
              size="middle" 
              className="w-full" 
              value={formData.operation}
              onChange={(v) => handleChange("operation", v)}
            >
              <Option value="weighing">Weighing</Option>
              <Option value="Inbound Product Receipt">Inbound Receipt</Option>
              <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
            </Select>
          </div>

          {/* Notes */}
          {/* <div className="col-span-6">
            <label className="block text-xs text-gray-600 mb-1">Notes</label>
            <Input.TextArea 
              size="middle" 
              rows={2} 
              placeholder="Any remarks..." 
              value={formData.notes}
              onChange={(e) => handleChange("notes", e.target.value)}
            />
          </div> */}
        </div>
      </div>

      {/* FOOTER */}
      <div className="h-20 bg-white border-t flex justify-between items-center px-6">
        <span className="text-sm">
          Status:{" "}
          <b className={isStable ? "text-green-600" : "text-amber-600"}>
            {isStable ? "Ready" : "Waiting"}
          </b>
        </span>
        <Button
          type="primary"
          size="large"
          disabled={!isStable || capturing}
          loading={capturing || globalLoading}
          onClick={handleCapture}
          className="bg-green-600 px-10"
        >
          CAPTURE
        </Button>
      </div>

      {apiError && (
        <Alert
          message="Error"
          description={apiError}
          type="error"
          showIcon
          className="fixed bottom-4 left-1/2 transform -translate-x-1/2 max-w-lg"
        />
      )}
    </div>
  );
}