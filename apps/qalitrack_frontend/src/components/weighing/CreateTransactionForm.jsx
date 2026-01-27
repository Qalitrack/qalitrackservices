// ✅ CRITICAL FIXES:
// 1. Keep ticketID as GUID string (don't convert to parseInt)
// 2. Get user from session storage as fallback
// 3. Better logging for debugging 404 errors

import React, { useEffect, useMemo, useCallback, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Input, Select, Button, message, Row, Col, Typography, Space, Alert } from "antd";
import { debounce } from "lodash";
import {
  fetchVehiclesByRegNumber,
  fetchDriversByName,
  fetchProductsByName,
  fetchSuppliersByName,
  fetchTransportersByName,
  fetchWeighbridges,
  addTransaction,
  addSecondWeight,
  fetchTransactions,
} from "../../store/weighingSlice";

const { Option } = Select;
const { Text } = Typography;

export default function CreateTransactionForm({
  formData,
  setFormData,
  capturedWeight,
  onTransactionCreated,
}) {
  const dispatch = useDispatch();
  const {
    vehicles,
    drivers,
    products,
    suppliers,
    transporters,
    weighbridges = [],
    loading,
  } = useSelector((state) => state.weighing);

  // ✅ Get user from Redux OR sessionStorage
  const reduxUser = useSelector((state) => state.auth?.user || state.weighing?.currentUser);
  const [sessionUser, setSessionUser] = useState(null);
  
  useEffect(() => {
    if (!reduxUser) {
      try {
        const session = sessionStorage.getItem("authSession") || sessionStorage.getItem("user");
        if (session) {
          const parsed = JSON.parse(session);
          setSessionUser(parsed.user || parsed);
        }
      } catch (err) {
        console.warn("Could not read session storage:", err);
      }
    }
  }, [reduxUser]);
  
  const currentUser = reduxUser || sessionUser;
  const [submitError, setSubmitError] = useState(null);

  const isSecondWeighing = !!(formData.id || formData.ticketID);

  console.log("🔍 CreateTransactionForm State:");
  console.log("  - Mode:", isSecondWeighing ? "SECOND WEIGHING" : "FIRST WEIGHING");
  console.log("  - formData.ticketID:", formData.ticketID);
  console.log("  - formData.id:", formData.id);
  console.log("  - currentUser:", currentUser);

  useEffect(() => {
    if (weighbridges.length === 0) {
      dispatch(fetchWeighbridges({ pageNumber: 1, pageSize: 100 }));
    }
  }, [dispatch, weighbridges.length]);

  useEffect(() => {
    if (currentUser) {
      const operatorName = 
        currentUser.fullName || 
        currentUser.name || 
        currentUser.username || 
        currentUser.email ||
        "Operator";
      
      const operatorId = 
        currentUser.id || 
        currentUser.userId || 
        "00000000-0000-0000-0000-000000000000";
      
      setFormData((prev) => ({
        ...prev,
        operatorID: operatorId,
        operatorName: operatorName,
      }));
    }
  }, [currentUser, setFormData]);

  const debounced = useMemo(
    () => ({
      vehicles: debounce((q) => dispatch(fetchVehiclesByRegNumber(q)), 400),
      drivers: debounce((q) => dispatch(fetchDriversByName(q)), 400),
      products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
      suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
      transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
    }),
    [dispatch]
  );

  const handleChange = (field, value) =>
    setFormData((prev) => ({ ...prev, [field]: value }));

  const handleSelect = (list, id, idKey, nameKey) => {
    const item = list.find((i) => i.id === id);
    setFormData((p) => ({
      ...p,
      [idKey]: id || null,
      [nameKey]:
        item?.name ||
        item?.registrationNumber ||
        item?.plateNumber ||
        item?.fullName ||
        "",
    }));
  };

  const handleScaleSelect = (scaleName) => {
    const selected = weighbridges.find((wb) => wb.location === scaleName || wb.name === scaleName);
    setFormData((prev) => ({
      ...prev,
      weighBridgeID: selected?.id || null,
      weighBridgeName: scaleName || "",
      scaleName: scaleName || "",
    }));
  };

  const validateForm = useCallback(() => {
    const errors = [];
    
    if (!formData.noPlate?.trim()) errors.push("Vehicle plate is required");
    if (!formData.weighBridgeID) errors.push("Weighbridge is required");
    if (!formData.transporterID) errors.push("Transporter is required");
    
    const weight = parseFloat(formData.firstWeight || capturedWeight || 0);
    if (!isSecondWeighing) {
      if (weight <= 0) errors.push("Valid first weight is required");
    }
    
    if (isSecondWeighing) {
      const w2 = parseFloat(formData.secondWeight || capturedWeight || 0);
      if (w2 <= 0) errors.push("Valid second weight is required");
    }
    
    return errors;
  }, [formData, capturedWeight, isSecondWeighing]);

  const { netWeight, isValid, errorMsg } = useMemo(() => {
    if (!isSecondWeighing) {
      return { netWeight: 0, isValid: true, errorMsg: "" };
    }

    const w1 = parseFloat(formData.firstWeight || 0);
    const w2 = parseFloat(formData.secondWeight || capturedWeight || 0);
    const operation = formData.operation || "Inbound Product Receipt";

    if (!w2 || w2 <= 0) {
      return { netWeight: 0, isValid: false, errorMsg: "Enter valid second weight" };
    }

    let net = 0;
    let valid = true;
    let msg = "";

    if (operation.includes("Inbound")) {
      net = w1 - w2;
      if (w2 >= w1) {
        valid = false;
        msg = "Inbound: Truck must leave lighter (W2 < W1)";
      }
    } else {
      net = w2 - w1;
      if (w2 <= w1) {
        valid = false;
        msg = "Outbound: Truck must leave heavier (W2 > W1)";
      }
    }

    return { netWeight: net > 0 ? net : 0, isValid: valid, errorMsg: msg };
  }, [formData, capturedWeight, isSecondWeighing]);

  const handleSubmit = async () => {
    setSubmitError(null);

    const validationErrors = validateForm();
    if (validationErrors.length > 0) {
      message.error(validationErrors.join("; "));
      return;
    }

    try {
      if (!isSecondWeighing) {
        const payload = {
          noPlate: formData.noPlate.toUpperCase(),
          driverName: formData.driverName || "",
          vehicleID: formData.vehicleID || "00000000-0000-0000-0000-000000000000",
          firstWeight: String(parseFloat(formData.firstWeight || capturedWeight || 0)),
          transporterID: formData.transporterID || "00000000-0000-0000-0000-000000000000",
          transporterName: formData.transporterName || "",
          weighBridgeID: formData.weighBridgeID || "00000000-0000-0000-0000-000000000000",
          weighBridgeName: formData.weighBridgeName || formData.scaleName || "",
          scaleName: formData.scaleName || "",
          operatorID: formData.operatorID || currentUser?.id || "00000000-0000-0000-0000-000000000000",
          operatorName: formData.operatorName || currentUser?.fullName || currentUser?.name || "Operator",
          commodityID: formData.commodityID || "00000000-0000-0000-0000-000000000000",
          commodityName: formData.commodityName || "",
          supplierID: formData.supplierID || "00000000-0000-0000-0000-000000000000",
          supplierName: formData.supplierName || "",
          customerID: formData.customerID || "00000000-0000-0000-0000-000000000000",
          customerName: formData.customerName || "",
          originID: formData.originID || "00000000-0000-0000-0000-000000000000",
          originName: formData.originName || "",
          destinationID: formData.destinationID || "00000000-0000-0000-0000-000000000000",
          destinationName: formData.destinationName || "",
          weighMode: formData.weighMode || "entry",
          operation: formData.operation || "weighing",
          notes: formData.notes || "",
        };

        await dispatch(addTransaction(payload)).unwrap();
        message.success("✓ First Weight Saved!");
        await dispatch(fetchTransactions({ isCompleted: false, pageNumber: 1, pageSize: 100 }));
        if (onTransactionCreated) onTransactionCreated();
      } else {
        if (!isValid) {
          message.error(errorMsg);
          return;
        }

        // ✅ CRITICAL: Get GUID and keep as string
        const transactionId = formData.ticketID || formData.id;
        
        if (!transactionId) {
          message.error("Transaction ID missing");
          return;
        }

        console.log("✅ Submitting second weight for ticket:", transactionId);

        const payload = {
          ticketID: String(transactionId), // ✅ Keep as GUID string!
          secondWeight: String(parseFloat(formData.secondWeight || capturedWeight)),
          weighBridgeName2nd: formData.weighBridgeName || formData.scaleName || "",
          scaleName2nd: formData.scaleName || "",
          operatorID2nd: String(formData.operatorID || currentUser?.id || ""),
          operatorName2nd: formData.operatorName || currentUser?.fullName || currentUser?.name || "Operator",
          notes: formData.notes || "",
        };

        console.log("📤 Second weight payload:", payload);

        await dispatch(addSecondWeight(payload)).unwrap();
        message.success(`✓ Finalized! Net: ${netWeight.toLocaleString()} KG`);
        await dispatch(fetchTransactions({ isCompleted: false, pageNumber: 1, pageSize: 100 }));
        if (onTransactionCreated) onTransactionCreated();
      }
    } catch (err) {
      console.error("❌ Save failed:", err);
      let errorMsg = err.message || "Failed to save";
      
      if (err.response?.status === 404) {
        errorMsg = "API endpoint not found (404). Check backend configuration.";
      }
      
      setSubmitError(errorMsg);
      message.error(errorMsg);
    }
  };

  const FieldLabel = ({ children, required }) => (
    <label className="text-[10px] font-bold text-gray-500 uppercase tracking-tight block mb-0.5">
      {children} {required && <span className="text-red-500">*</span>}
    </label>
  );

  return (
    <div className="flex flex-col h-full bg-white">
      <div className={`mb-3 p-2 rounded-lg border flex justify-between items-center shrink-0 ${
          isSecondWeighing ? "bg-blue-50 border-blue-200" : "bg-amber-50 border-amber-200"
        }`}>
        <div>
          <Text className="text-[9px] uppercase font-bold text-gray-400 block leading-none">
            Live Weight
          </Text>
          <div className={`text-2xl font-black ${isSecondWeighing ? "text-blue-700" : "text-amber-700"}`}>
            {capturedWeight || 0} <small className="text-xs font-normal">KG</small>
          </div>
        </div>
        <Space size="small">
          {!isSecondWeighing && (
            <div className="text-right">
              <FieldLabel>Manual W1</FieldLabel>
              <Input size="small" className="w-20 font-bold" value={formData.firstWeight}
                onChange={(e) => handleChange("firstWeight", e.target.value)} placeholder="0" type="number" />
            </div>
          )}
          {isSecondWeighing && (
            <>
              <div className="text-right">
                <FieldLabel>W1 (First)</FieldLabel>
                <Input size="small" className="w-20 font-bold bg-gray-50" value={formData.firstWeight} readOnly />
              </div>
              <div className="text-right">
                <FieldLabel>Manual W2</FieldLabel>
                <Input size="small" className="w-20 font-bold" value={formData.secondWeight}
                  onChange={(e) => handleChange("secondWeight", e.target.value)} placeholder="0" type="number" />
              </div>
            </>
          )}
        </Space>
      </div>

      {isSecondWeighing && (
        <div className={`mb-4 p-3 rounded-lg border text-center ${
            isValid ? "bg-green-50 border-green-300" : "bg-red-50 border-red-300"
          }`}>
          <Text className="text-[11px] uppercase font-bold text-gray-600">Net Payload</Text>
          <div className={`text-3xl font-black font-mono mt-1 ${isValid ? "text-green-700" : "text-red-700"}`}>
            {netWeight.toLocaleString()} <small className="text-sm font-normal">KG</small>
          </div>
          {!isValid && formData.secondWeight && (
            <Text type="danger" className="text-[10px] block mt-2">{errorMsg}</Text>
          )}
        </div>
      )}

      {isSecondWeighing && (
        <Alert message={`Second Weighing Mode`}
          description={`Vehicle: ${formData.noPlate} | First Weight: ${formData.firstWeight} kg`}
          type="info" showIcon className="mb-3" />
      )}

      {submitError && (
        <Alert message="Save Failed" description={submitError} type="error" showIcon closable
          onClose={() => setSubmitError(null)} className="mb-4" />
      )}

      <div className="flex-1 overflow-y-auto pr-1">
        <Row gutter={[8, 10]}>
          <Col span={12}>
            <FieldLabel required>Scale Name</FieldLabel>
            <Select size="middle" className="w-full" showSearch onChange={handleScaleSelect}
              value={formData.scaleName} placeholder="Select weighbridge scale"
              filterOption={(input, option) => option.children.toLowerCase().includes(input.toLowerCase())}
              notFoundContent={weighbridges.length === 0 ? "Loading..." : "No weighbridges"}>
              {weighbridges.map((wb) => (
                <Option key={wb.id} value={wb.location || wb.name}>
                  {wb.location || wb.name}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel required>Operator</FieldLabel>
            <Input size="middle" value={formData.operatorName} readOnly
              className="bg-amber-50 font-semibold text-gray-800 border-amber-200"
              placeholder="Auto-filled" prefix={
                <div className="w-6 h-6 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white text-[10px] font-bold mr-1">
                  {formData.operatorName ? formData.operatorName.charAt(0).toUpperCase() : '?'}
                </div>
              } />
            <div className="text-[9px] text-amber-600 mt-1 font-medium">
              ✓ {currentUser ? `${currentUser.username || currentUser.email}` : 'Loading...'}
            </div>
          </Col>

          <Col span={12}>
            <FieldLabel required>Vehicle Plate</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.vehicles}
              onChange={(id) => handleSelect(vehicles, id, "vehicleID", "noPlate")}
              value={formData.vehicleID || formData.noPlate} disabled={isSecondWeighing} placeholder="Search...">
              {vehicles.map((it) => (
                <Option key={it.id} value={it.id}>{it.registrationNumber || it.plateNumber}</Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Driver Name</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.drivers}
              onChange={(id) => handleSelect(drivers, id, "driverID", "driverName")}
              value={formData.driverID || formData.driverName} disabled={isSecondWeighing} placeholder="Search...">
              {drivers.map((it) => (
                <Option key={it.id} value={it.id}>{it.fullName || it.name}</Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel required>Transporter</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.transporters}
              onChange={(id) => handleSelect(transporters, id, "transporterID", "transporterName")}
              value={formData.transporterID} disabled={isSecondWeighing} placeholder="Select...">
              {transporters.map((it) => (
                <Option key={it.id} value={it.id}>{it.name}</Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Commodity</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.products}
              onChange={(id) => handleSelect(products, id, "commodityID", "commodityName")}
              value={formData.commodityID} disabled={isSecondWeighing} placeholder="Select...">
              {products.map((it) => (
                <Option key={it.id} value={it.id}>{it.name}</Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Supplier</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.suppliers}
              onChange={(id) => handleSelect(suppliers, id, "supplierID", "supplierName")}
              value={formData.supplierID} disabled={isSecondWeighing} placeholder="Select...">
              {suppliers.map((it) => (
                <Option key={it.id} value={it.id}>{it.name}</Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Customer Name</FieldLabel>
            <Input size="middle" value={formData.customerName}
              onChange={(e) => handleChange("customerName", e.target.value)}
              disabled={isSecondWeighing} placeholder="Enter customer" />
          </Col>

          <Col span={6}>
            <FieldLabel>Origin</FieldLabel>
            <Input size="middle" value={formData.originName}
              onChange={(e) => handleChange("originName", e.target.value)}
              disabled={isSecondWeighing} placeholder="Origin" />
          </Col>

          <Col span={6}>
            <FieldLabel>Destination</FieldLabel>
            <Input size="middle" value={formData.destinationName}
              onChange={(e) => handleChange("destinationName", e.target.value)}
              disabled={isSecondWeighing} placeholder="Destination" />
          </Col>

          <Col span={8}>
            <FieldLabel>Weigh Mode</FieldLabel>
            <Select size="middle" className="w-full" value={formData.weighMode}
              onChange={(v) => handleChange("weighMode", v)} disabled={isSecondWeighing}>
              <Option value="entry">Entry</Option>
              <Option value="Gross/Tare">Gross / Tare</Option>
            </Select>
          </Col>

          <Col span={16}>
            <FieldLabel>Operation Type</FieldLabel>
            <Select size="middle" className="w-full" value={formData.operation}
              onChange={(v) => handleChange("operation", v)} disabled={isSecondWeighing}>
              <Option value="weighing">Weighing</Option>
              <Option value="Inbound Product Receipt">Inbound Receipt</Option>
              <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
            </Select>
          </Col>
        </Row>
      </div>

      <div className="mt-3 pt-3 border-t flex gap-3 shrink-0 bg-white">
        <Button size="large" className="w-1/3 text-gray-400 font-bold" onClick={onTransactionCreated}>
          RESET
        </Button>
        <Button type="primary" size="large" loading={loading} onClick={handleSubmit}
          disabled={isSecondWeighing && !isValid}
          className={`flex-1 font-bold border-none shadow-md ${
            isSecondWeighing ? "bg-blue-600 hover:bg-blue-700" : "bg-amber-500 hover:bg-amber-600"
          }`}>
          {isSecondWeighing ? "FINALIZE TRANSACTION" : "SAVE FIRST WEIGHT"}
        </Button>
      </div>
    </div>
  );
}