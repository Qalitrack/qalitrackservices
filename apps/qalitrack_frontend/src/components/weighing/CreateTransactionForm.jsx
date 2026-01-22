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
} from "../../store/weighingSlice";

const { Option } = Select;
const { Text } = Typography;

// Persistent daily counter for receipt number
const getDailyCounter = () => {
  const today = new Date().toISOString().slice(0, 10).replace(/-/g, "");
  const stored = localStorage.getItem("receiptCounter");
  if (!stored) return { date: today, count: 0 };

  const parsed = JSON.parse(stored);
  if (parsed.date === today) return { date: today, count: parsed.count };
  return { date: today, count: 0 };
};

const incrementDailyCounter = () => {
  const counter = getDailyCounter();
  counter.count += 1;
  localStorage.setItem("receiptCounter", JSON.stringify(counter));
  return counter.count;
};

const formatSequentialNumber = (num) => String(num).padStart(2, "0");

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

  const currentUser = useSelector((state) => state.auth?.user);
  const [submitError, setSubmitError] = useState(null);

  const isSecondWeighing = !!formData.id;

  // Load weighbridges on mount
  useEffect(() => {
    if (weighbridges.length === 0) {
      console.log("🔄 Loading weighbridges...");
      dispatch(fetchWeighbridges({ pageNumber: 1, pageSize: 100 }));
    } else {
      console.log("✅ Weighbridges already loaded:", weighbridges);
    }
  }, [dispatch, weighbridges.length]);

  // Auto-populate operator information
  useEffect(() => {
    if (currentUser && !isSecondWeighing) {
      setFormData((prev) => ({
        ...prev,
        operatorId: currentUser.id,
        operatorName: currentUser.fullName || currentUser.name || currentUser.username || "Operator",
      }));
    }
  }, [currentUser, isSecondWeighing, setFormData]);

  // Generate receipt number
  const generateReceiptNumber = useCallback(() => {
    const date = new Date();
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    const datePart = `${year}${month}${day}`;
    const seqNumber = incrementDailyCounter();
    const seqPart = formatSequentialNumber(seqNumber);

    return `RCT-${datePart}-${seqPart}`;
  }, []);

  useEffect(() => {
    if (!formData.id && !formData.receiptNo) {
      setFormData((prev) => ({ ...prev, receiptNo: generateReceiptNumber() }));
    }
  }, [formData.id, formData.receiptNo, setFormData, generateReceiptNumber]);

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
    console.log("🔍 Selected weighbridge:", selected);
    setFormData((prev) => ({
      ...prev,
      weighBridgeId: selected?.id || null,
      weighBridgeName: scaleName || "",
      scaleName: scaleName || "",
    }));
  };

  const { netWeight, isValid, errorMsg } = useMemo(() => {
    if (!isSecondWeighing) {
      return { netWeight: 0, isValid: true, errorMsg: "" };
    }

    const w1 = parseFloat(formData.firstWeight || 0);
    const w2 = parseFloat(formData.secondWeight || capturedWeight || 0);
    const operation = formData.operation || "Inbound Product Receipt";

    if (!w2 || w2 <= 0) {
      return {
        netWeight: 0,
        isValid: false,
        errorMsg: "Please enter or capture a valid second weight",
      };
    }

    let net = 0;
    let valid = true;
    let msg = "";

    if (operation.includes("Inbound")) {
      net = w1 - w2;
      if (w2 >= w1) {
        valid = false;
        msg = "Inbound Error: Truck must leave lighter (W2 < W1)";
      }
    } else {
      net = w2 - w1;
      if (w2 <= w1) {
        valid = false;
        msg = "Outbound Error: Truck must leave heavier (W2 > W1)";
      }
    }

    return { netWeight: net > 0 ? net : 0, isValid: valid, errorMsg: msg };
  }, [formData, capturedWeight, isSecondWeighing]);

  const handleSubmit = async () => {
    setSubmitError(null);

    console.log("┌───────────────────────────────────────┐");
    console.log("│         SUBMIT HANDLER CALLED         │");
    console.log("└───────────────────────────────────────┘");
    console.log("isSecondWeighing:", isSecondWeighing);
    console.log("formData:", formData);
    console.log("capturedWeight:", capturedWeight);

    // Validation
    if (!formData.receiptNo) return message.error("Receipt number is required");
    if (!formData.noPlate) return message.error("Vehicle plate is required");
    if (!formData.scaleName || !formData.weighBridgeId) return message.error("Please select a weighbridge scale");
    if (!formData.transporterId) return message.error("Transporter is required (backend needs transporterID)");

    try {
      if (!isSecondWeighing) {
        // FIRST WEIGHING - POST /Transaction
        const payload = {
          noPlate: formData.noPlate.toUpperCase(),
          driverName: formData.driverName || "",
          vehicleID: formData.vehicleId ? parseInt(formData.vehicleId) : 0,
          firstWeight: String(parseFloat(formData.firstWeight || capturedWeight || 0)),
          transporterID: formData.transporterId ? parseInt(formData.transporterId) : 0,
          transporterName: formData.transporterName || "",
          weighBridgeID: formData.weighBridgeId ? parseInt(formData.weighBridgeId) : 0,
          weighBridgeName: formData.weighBridgeName || formData.scaleName || "",
          scaleName: formData.scaleName || "",
          operatorID: formData.operatorId ? parseInt(formData.operatorId) : (currentUser?.id || 0),
          operatorName: formData.operatorName || currentUser?.fullName || currentUser?.name || "Operator",
          commodityID: formData.commodityId ? parseInt(formData.commodityId) : 0,
          commodityName: formData.commodityName || "",
          supplierID: formData.supplierId ? parseInt(formData.supplierId) : 0,
          supplierName: formData.supplierName || "",
          customerID: formData.customerId ? parseInt(formData.customerId) : 0,
          customerName: formData.customerName || "",
          originID: formData.originId ? parseInt(formData.originId) : 0,
          originName: formData.originName || "",
          destinationID: formData.destinationId ? parseInt(formData.destinationId) : 0,
          destinationName: formData.destinationName || "",
          weighMode: formData.weighMode || "Gross/Tare",
          operation: formData.operation || "Inbound Product Receipt",
          notes: formData.notes || "",
        };

        console.log("┌───────────────────────────────────────┐");
        console.log("│   FINAL FIRST WEIGHT PAYLOAD (SENDING)  │");
        console.log("└───────────────────────────────────────┘");
        console.log(JSON.stringify(payload, null, 2));

        const result = await dispatch(addTransaction(payload)).unwrap();

        console.log("┌───────────────────────────────────────┐");
        console.log("│       TRANSACTION SAVED SUCCESS       │");
        console.log("└───────────────────────────────────────┘");
        console.log("Backend response:", JSON.stringify(result, null, 2));

        message.success("First Weight Saved! Vehicle added to queue.");
      } else {
        // SECOND WEIGHING - POST /Transaction/add-second-weight
        if (!isValid) {
          message.error(errorMsg);
          return;
        }

        const payload = {
          ticketID: parseInt(formData.id),
          secondWeight: String(parseFloat(formData.secondWeight || capturedWeight)),
          weighBridgeName2nd: formData.weighBridgeName || formData.scaleName || "",
          scaleName2nd: formData.scaleName || "",
          operatorID2nd: String(formData.operatorId || currentUser?.id || ""),
          operatorName2nd: formData.operatorName || currentUser?.fullName || currentUser?.name || "Operator",
          notes: formData.notes || "",
        };

        console.log("┌───────────────────────────────────────┐");
        console.log("│   FINAL SECOND WEIGHT PAYLOAD         │");
        console.log("└───────────────────────────────────────┘");
        console.log(JSON.stringify(payload, null, 2));

        const result = await dispatch(addSecondWeight(payload)).unwrap();

        console.log("┌───────────────────────────────────────┐");
        console.log("│       SECOND WEIGHT SAVED SUCCESS     │");
        console.log("└───────────────────────────────────────┘");
        console.log("Backend response:", JSON.stringify(result, null, 2));

        message.success(`Transaction Finalized! Net Weight: ${netWeight.toLocaleString()} KG`);
      }

      if (onTransactionCreated) onTransactionCreated();
    } catch (err) {
      console.error("┌───────────────────────────────────────┐");
      console.error("│         TRANSACTION SAVE FAILED       │");
      console.error("└───────────────────────────────────────┘");
      console.error("Error object:", err);
      console.error("Message:", err?.message);
      console.error("Response:", err?.response?.data || err?.response);
      console.error("Stack:", err?.stack);

      const errorMsg = err?.message || "Failed to save transaction. Check console for details.";
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
      {/* Live Weight Banner with Manual Inputs */}
      <div
        className={`mb-3 p-2 rounded-lg border flex justify-between items-center shrink-0 ${
          isSecondWeighing
            ? "bg-blue-50 border-blue-200"
            : "bg-amber-50 border-amber-200"
        }`}
      >
        <div>
          <Text className="text-[9px] uppercase font-bold text-gray-400 block leading-none">
            Live Weight
          </Text>
          <div
            className={`text-2xl font-black ${
              isSecondWeighing ? "text-blue-700" : "text-amber-700"
            }`}
          >
            {capturedWeight || 0}{" "}
            <small className="text-xs font-normal">KG</small>
          </div>
        </div>
        <Space size="small">
          {!isSecondWeighing && (
            <div className="text-right">
              <FieldLabel>Manual W1</FieldLabel>
              <Input
                size="small"
                className="w-20 font-bold"
                value={formData.firstWeight}
                onChange={(e) => handleChange("firstWeight", e.target.value)}
                placeholder="0"
                type="number"
              />
            </div>
          )}

          {isSecondWeighing && (
            <>
              <div className="text-right">
                <FieldLabel>W1 (First)</FieldLabel>
                <Input
                  size="small"
                  className="w-20 font-bold"
                  value={formData.firstWeight}
                  readOnly
                />
              </div>
              <div className="text-right">
                <FieldLabel>Manual W2</FieldLabel>
                <Input
                  size="small"
                  className="w-20 font-bold"
                  value={formData.secondWeight}
                  onChange={(e) => handleChange("secondWeight", e.target.value)}
                  placeholder="0"
                  type="number"
                />
              </div>
            </>
          )}
        </Space>
      </div>

      {/* Net Weight Preview */}
      {isSecondWeighing && (
        <div
          className={`mb-4 p-3 rounded-lg border text-center ${
            isValid
              ? "bg-green-50 border-green-300"
              : "bg-red-50 border-red-300"
          }`}
        >
          <Text className="text-[11px] uppercase font-bold text-gray-600">
            Net Payload
          </Text>
          <div
            className={`text-3xl font-black font-mono mt-1 ${
              isValid ? "text-green-700" : "text-red-700"
            }`}
          >
            {netWeight.toLocaleString()}{" "}
            <small className="text-sm font-normal">KG</small>
          </div>
          {!isValid && formData.secondWeight && (
            <Text type="danger" className="text-[10px] block mt-2">
              {errorMsg}
            </Text>
          )}
        </div>
      )}

      {/* Error Display */}
      {submitError && (
        <Alert
          message="Save Failed"
          description={submitError}
          type="error"
          showIcon
          closable
          onClose={() => setSubmitError(null)}
          className="mb-4"
        />
      )}

      {/* Form Fields */}
      <div className="flex-1 overflow-y-auto pr-1">
        <Row gutter={[8, 10]}>
          <Col span={8}>
            <FieldLabel required>Receipt Number</FieldLabel>
            <Input
              size="middle"
              value={formData.receiptNo}
              readOnly
              className="bg-gray-50 font-mono font-semibold"
            />
          </Col>

          <Col span={8}>
            <FieldLabel required>Scale Name</FieldLabel>
            <Select
              size="middle"
              className="w-full"
              showSearch
              onChange={handleScaleSelect}
              value={formData.scaleName}
              disabled={isSecondWeighing}
              placeholder="Select weighbridge scale"
              filterOption={(input, option) =>
                option.children.toLowerCase().includes(input.toLowerCase())
              }
              notFoundContent={
                weighbridges.length === 0 ? "Loading weighbridges..." : "No weighbridges found"
              }
            >
              {weighbridges.map((wb) => {
                const displayName = wb.location || wb.name || `Weighbridge ${wb.id}`;
                return (
                  <Option key={wb.id} value={displayName}>
                    {displayName}
                  </Option>
                );
              })}
            </Select>
          </Col>

          <Col span={8}>
            <FieldLabel required>Operator</FieldLabel>
            <Input
              size="middle"
              value={formData.operatorName}
              readOnly
              className="bg-gray-50 font-semibold text-gray-700"
              placeholder="Auto-filled from login"
            />
          </Col>

          <Col span={12}>
            <FieldLabel required>Vehicle Plate</FieldLabel>
            <Select
              size="middle"
              showSearch
              className="w-full"
              onSearch={debounced.vehicles}
              onChange={(id) =>
                handleSelect(vehicles, id, "vehicleId", "noPlate")
              }
              value={formData.vehicleId || formData.noPlate}
              disabled={isSecondWeighing}
              placeholder="Search vehicle..."
            >
              {vehicles.map((it) => (
                <Option key={it.id} value={it.id}>
                  {it.registrationNumber || it.plateNumber}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel required>Driver Name</FieldLabel>
            <Select
              size="middle"
              showSearch
              className="w-full"
              onSearch={debounced.drivers}
              onChange={(id) =>
                handleSelect(drivers, id, "driverId", "driverName")
              }
              value={formData.driverId || formData.driverName}
              disabled={isSecondWeighing}
              placeholder="Search driver..."
            >
              {drivers.map((it) => (
                <Option key={it.id} value={it.id}>
                  {it.fullName || it.name}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={8}>
            <FieldLabel required>Transporter</FieldLabel>
            <Select
              size="middle"
              showSearch
              className="w-full"
              onSearch={debounced.transporters}
              onChange={(id) =>
                handleSelect(transporters, id, "transporterId", "transporterName")
              }
              value={formData.transporterId}
              disabled={isSecondWeighing}
              placeholder="Select transporter..."
            >
              {transporters.map((it) => (
                <Option key={it.id} value={it.id}>
                  {it.name}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={8}>
            <FieldLabel>Commodity</FieldLabel>
            <Select
              size="middle"
              showSearch
              className="w-full"
              onSearch={debounced.products}
              onChange={(id) =>
                handleSelect(products, id, "commodityId", "commodityName")
              }
              value={formData.commodityId}
              disabled={isSecondWeighing}
              placeholder="Select commodity..."
            >
              {products.map((it) => (
                <Option key={it.id} value={it.id}>
                  {it.name}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={8}>
            <FieldLabel>Supplier</FieldLabel>
            <Select
              size="middle"
              showSearch
              className="w-full"
              onSearch={debounced.suppliers}
              onChange={(id) =>
                handleSelect(suppliers, id, "supplierId", "supplierName")
              }
              value={formData.supplierId}
              disabled={isSecondWeighing}
              placeholder="Select supplier..."
            >
              {suppliers.map((it) => (
                <Option key={it.id} value={it.id}>
                  {it.name}
                </Option>
              ))}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Customer Name</FieldLabel>
            <Input
              size="middle"
              value={formData.customerName}
              onChange={(e) => handleChange("customerName", e.target.value)}
              disabled={isSecondWeighing}
              placeholder="Enter customer name"
            />
          </Col>

          <Col span={6}>
            <FieldLabel>Origin</FieldLabel>
            <Input
              size="middle"
              value={formData.originName}
              onChange={(e) => handleChange("originName", e.target.value)}
              disabled={isSecondWeighing}
              placeholder="Origin"
            />
          </Col>

          <Col span={6}>
            <FieldLabel>Destination</FieldLabel>
            <Input
              size="middle"
              value={formData.destinationName}
              onChange={(e) => handleChange("destinationName", e.target.value)}
              disabled={isSecondWeighing}
              placeholder="Destination"
            />
          </Col>

          <Col span={8}>
            <FieldLabel>Weigh Mode</FieldLabel>
            <Select
              size="middle"
              className="w-full"
              value={formData.weighMode}
              onChange={(v) => handleChange("weighMode", v)}
              disabled={isSecondWeighing}
            >
              <Option value="Gross/Tare">Gross / Tare</Option>
            </Select>
          </Col>

          <Col span={16}>
            <FieldLabel>Operation Type</FieldLabel>
            <Select
              size="middle"
              className="w-full"
              value={formData.operation}
              onChange={(v) => handleChange("operation", v)}
              disabled={isSecondWeighing}
            >
              <Option value="Inbound Product Receipt">Inbound Receipt</Option>
              <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
            </Select>
          </Col>
        </Row>
      </div>

      {/* Footer Buttons */}
      <div className="mt-3 pt-3 border-t flex gap-3 shrink-0 bg-white">
        <Button
          size="large"
          className="w-1/3 text-gray-400 font-bold"
          onClick={onTransactionCreated}
        >
          RESET
        </Button>
        <Button
          type="primary"
          size="large"
          loading={loading}
          onClick={handleSubmit}
          disabled={isSecondWeighing && !isValid}
          className={`flex-1 font-bold border-none shadow-md ${
            isSecondWeighing
              ? "bg-blue-600 hover:bg-blue-700"
              : "bg-amber-500 hover:bg-amber-600"
          }`}
        >
          {isSecondWeighing ? "FINALIZE TRANSACTION" : "SAVE FIRST WEIGHT"}
        </Button>
      </div>
    </div>
  );
}