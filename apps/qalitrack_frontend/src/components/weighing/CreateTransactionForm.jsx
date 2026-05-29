// ✅ FULLY FIXED - Empty Transaction Problem Resolved
// KEY FIX: Only send valid GUIDs to backend, omit empty/null GUIDs entirely
// Operator field is now HIDDEN from UI but still recorded in transaction

import React, { useEffect, useMemo, useCallback, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Input, Select, AutoComplete, Button, message, Row, Col, Typography, Space, Alert, Modal } from "antd";
import { Scale } from "lucide-react";
import {
  fetchWeighbridges,
  addTransaction,
  addSecondWeight,
  fetchTransactions,
  fetchCurrentUser,
} from "../../store/weighingSlice";

const { Option } = Select;
const { Text } = Typography;

export default function CreateTransactionForm({
  formData,
  setFormData,
  capturedWeight,
  onTransactionCreated,
  manualWeighingEnabled = false,
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
    currentUser: reduxCurrentUser,
  } = useSelector((state) => state.weighing);

  const authUser = useSelector((state) => state.auth?.user);
  const [sessionUser, setSessionUser] = useState(null);

  useEffect(() => {
    if (!reduxCurrentUser && !authUser) {
      try {
        const authSession = localStorage.getItem("authSession");
        if (authSession) {
          const parsed = JSON.parse(authSession);
          const user = parsed.userData || parsed.user || parsed;
          setSessionUser(user);
        } else {
          dispatch(fetchCurrentUser());
        }
      } catch (err) {
        dispatch(fetchCurrentUser());
      }
    }
  }, [reduxCurrentUser, authUser, dispatch]);

  const currentUser = reduxCurrentUser || authUser || sessionUser;
  const [submitError, setSubmitError] = useState(null);
  const [showFinalizePreview, setShowFinalizePreview] = useState(false);
  const [previewEditData, setPreviewEditData] = useState({});

  const isSecondWeighing = !!(formData.id || formData.ticketID);
  const [manualPlate, setManualPlate] = useState(false);

  useEffect(() => {
    if (weighbridges.length === 0) {
      dispatch(fetchWeighbridges({ pageNumber: 1, pageSize: 100 }));
    }
  }, [dispatch, weighbridges.length]);


  // ✅ Automatically set operator information (still sent to backend)
  useEffect(() => {
    if (currentUser) {
      let operatorName = "Unknown Operator";

      if (currentUser.fullName?.trim()) {
        operatorName = currentUser.fullName.trim();
      } else if (currentUser.firstName || currentUser.lastName) {
        const firstName = currentUser.firstName?.trim() || '';
        const lastName = currentUser.lastName?.trim() || '';
        operatorName = `${firstName} ${lastName}`.trim();
      } else if (currentUser.name?.trim()) {
        operatorName = currentUser.name.trim();
      } else if (currentUser.username?.trim()) {
        operatorName = currentUser.username.trim();
      } else if (currentUser.email) {
        operatorName = currentUser.email.split('@')[0];
      }

      const operatorId =
        currentUser.id ||
        currentUser.userId ||
        currentUser.uid ||
        "00000000-0000-0000-0000-000000000000";

      setFormData((prev) => ({
        ...prev,
        operatorID: operatorId,
        operatorName: operatorName,
      }));
    }
  }, [currentUser, setFormData]);

  const filterOptions = (list, input, labelFn) => {
    const q = (input || "").toLowerCase();
    return list
      .filter((it) => labelFn(it).toLowerCase().includes(q))
      .map((it) => ({ value: labelFn(it), label: labelFn(it), id: it.id }));
  };

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

  const validateForm = useCallback(() => {
    const errors = [];

    if (!formData.noPlate?.trim()) errors.push("Vehicle plate is required");
    if (!formData.transporterID && !formData.transporterName?.trim()) errors.push("Transporter is required");

    const weight = parseFloat(formData.firstWeight || capturedWeight || 0);
    if (!isSecondWeighing) {
      if (weight <= 0) errors.push("Valid first weight is required");
      else if (weight < 500) errors.push("First weight must be at least 500 kg");
    }

    if (isSecondWeighing) {
      const w2 = parseFloat(formData.secondWeight || capturedWeight || 0);
      if (w2 <= 0) errors.push("Valid second weight is required");
      else if (w2 < 500) errors.push("Second weight must be at least 500 kg");
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

  const isValidGuid = (guid) => {
    return (
      guid &&
      guid !== "00000000-0000-0000-0000-000000000000" &&
      guid.length > 0
    );
  };

  const openFinalizePreview = () => {
    const validationErrors = validateForm();
    if (validationErrors.length > 0) {
      message.error(validationErrors.join("; "));
      return;
    }
    if (!isValid) {
      message.error(errorMsg);
      return;
    }
    setPreviewEditData({ ...formData, secondWeight: formData.secondWeight || capturedWeight || "" });
    setShowFinalizePreview(true);
  };

  const previewNetWeight = (() => {
    const w1 = parseFloat(previewEditData.firstWeight || 0);
    const w2 = parseFloat(previewEditData.secondWeight || 0);
    const op = previewEditData.operation || "Inbound Product Receipt";
    if (!w1 || !w2) return 0;
    return op.includes("Inbound") ? Math.max(0, w1 - w2) : Math.max(0, w2 - w1);
  })();

  const doFinalizeSubmit = async (data) => {
    setSubmitError(null);
    const transactionId = data.ticketID || data.id;
    if (!transactionId) {
      message.error("Transaction ID missing - cannot complete transaction");
      return;
    }

    let resolvedOperatorName = data.operatorName;
    let resolvedOperatorID = data.operatorID;
    if (!resolvedOperatorName || resolvedOperatorName === "Unknown Operator") {
      if (currentUser) {
        resolvedOperatorID = currentUser.id || currentUser.userId || "00000000-0000-0000-0000-000000000000";
        if (currentUser.firstName || currentUser.lastName) {
          resolvedOperatorName = `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim();
        } else if (currentUser.email) {
          resolvedOperatorName = currentUser.email.split('@')[0];
        }
      }
    }

    const payload = {
      ticketID: String(transactionId),
      secondWeight: String(parseFloat(data.secondWeight || capturedWeight)),
      weighBridgeName2nd: data.weighBridgeName || data.scaleName || "",
      scaleName2nd: data.scaleName || "",
      operatorID2nd: String(resolvedOperatorID || currentUser?.id || ""),
      operatorName2nd: resolvedOperatorName || data.operatorName,
      notes: data.notes || "",
    };

    try {
      await dispatch(addSecondWeight(payload)).unwrap();
      const finalNet = (() => {
        const w1 = parseFloat(data.firstWeight || 0);
        const w2 = parseFloat(data.secondWeight || capturedWeight || 0);
        const op = data.operation || "Inbound Product Receipt";
        return op.includes("Inbound") ? Math.max(0, w1 - w2) : Math.max(0, w2 - w1);
      })();
      message.success({ content: `✓ Transaction Completed! Net: ${finalNet.toLocaleString()} KG`, duration: 5 });
      await dispatch(fetchTransactions({ isCompleted: false, pageNumber: 1, pageSize: 100 }));
      if (onTransactionCreated) onTransactionCreated();
    } catch (err) {
      let errMsg = "Failed to save transaction";
      if (err.message) errMsg = err.message;
      if (err.response?.status === 404) errMsg = "API endpoint not found (404)";
      else if (err.response?.status === 400) errMsg = "Invalid data sent to server";
      else if (err.response?.status === 500) errMsg = "Server error";
      setSubmitError(errMsg);
      message.error(errMsg);
    }
  };

  const handleSubmit = async () => {
    setSubmitError(null);

    const validationErrors = validateForm();
    if (validationErrors.length > 0) {
      message.error(validationErrors.join("; "));
      return;
    }

    // Resolve operator name at submit time (handles form reset losing operatorName)
    let resolvedOperatorName = formData.operatorName;
    let resolvedOperatorID = formData.operatorID;
    if (!resolvedOperatorName || resolvedOperatorName === "Unknown Operator") {
      if (currentUser) {
        resolvedOperatorID = currentUser.id || currentUser.userId || "00000000-0000-0000-0000-000000000000";
        if (currentUser.firstName || currentUser.lastName) {
          resolvedOperatorName = `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim();
        } else if (currentUser.email) {
          resolvedOperatorName = currentUser.email.split('@')[0];
        }
      }
    }

    try {
      if (!isSecondWeighing) {
        // FIRST WEIGHING
        const payload = {
          noPlate: formData.noPlate.toUpperCase(),
          firstWeight: String(parseFloat(formData.firstWeight || capturedWeight || 0)),
          weighMode: formData.weighMode || "entry",
          operation: formData.operation || "weighing",
        };

        if (formData.driverName?.trim()) payload.driverName = formData.driverName.trim();
        if (isValidGuid(formData.vehicleID)) payload.vehicleID = formData.vehicleID;

        if (isValidGuid(formData.transporterID)) payload.transporterID = formData.transporterID;
        if (formData.transporterName) payload.transporterName = formData.transporterName;

        if (isValidGuid(formData.weighBridgeID)) payload.weighBridgeID = formData.weighBridgeID;
        if (formData.weighBridgeName) payload.weighBridgeName = formData.weighBridgeName;
        if (formData.scaleName) payload.scaleName = formData.scaleName;

        // Operator info - ALWAYS included
        payload.operatorID = resolvedOperatorID || formData.operatorID || currentUser?.id;
        payload.operatorName = resolvedOperatorName || formData.operatorName;

        if (isValidGuid(formData.commodityID)) payload.commodityID = formData.commodityID;
        if (formData.commodityName) payload.commodityName = formData.commodityName;

        if (isValidGuid(formData.supplierID)) payload.supplierID = formData.supplierID;
        if (formData.supplierName) payload.supplierName = formData.supplierName;

        if (isValidGuid(formData.customerID)) payload.customerID = formData.customerID;
        if (formData.customerName?.trim()) payload.customerName = formData.customerName.trim();

        if (isValidGuid(formData.originID)) payload.originID = formData.originID;
        if (formData.originName?.trim()) payload.originName = formData.originName.trim();

        if (isValidGuid(formData.destinationID)) payload.destinationID = formData.destinationID;
        if (formData.destinationName?.trim()) payload.destinationName = formData.destinationName.trim();

        if (formData.notes?.trim()) payload.notes = formData.notes.trim();
        if (formData.nprSource) payload.nprSource = formData.nprSource;

        const result = await dispatch(addTransaction(payload)).unwrap();
        message.success("✓ First Weight Saved!");
        await dispatch(fetchTransactions({ isCompleted: false, pageNumber: 1, pageSize: 100 }));
        if (onTransactionCreated) onTransactionCreated();
      } else {
        // SECOND WEIGHING
        if (!isValid) {
          message.error(errorMsg);
          return;
        }

        const transactionId = formData.ticketID || formData.id;

        if (!transactionId) {
          message.error("Transaction ID missing - cannot complete transaction");
          return;
        }

        const payload = {
          ticketID: String(transactionId),
          secondWeight: String(parseFloat(formData.secondWeight || capturedWeight)),
          weighBridgeName2nd: formData.weighBridgeName || formData.scaleName || "",
          scaleName2nd: formData.scaleName || "",
          operatorID2nd: String(formData.operatorID || currentUser?.id || ""),
          operatorName2nd: formData.operatorName,
          notes: formData.notes || "",
        };

        const result = await dispatch(addSecondWeight(payload)).unwrap();
        message.success({
          content: `✓ Transaction Completed! Net: ${netWeight.toLocaleString()} KG`,
          duration: 5,
        });

        await dispatch(fetchTransactions({ isCompleted: false, pageNumber: 1, pageSize: 100 }));

        if (onTransactionCreated) onTransactionCreated();
      }
    } catch (err) {
      let errorMsg = "Failed to save transaction";
      if (err.message) errorMsg = err.message;
      if (err.response?.status === 404) errorMsg = "API endpoint not found (404)";
      else if (err.response?.status === 400) errorMsg = "Invalid data sent to server";
      else if (err.response?.status === 500) errorMsg = "Server error";

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
    <>
    <div className="flex flex-col h-full bg-white">
      {/* WEIGHBRIDGE INFO BAR */}
      <div className="mb-2 px-3 py-2 bg-slate-800 rounded-lg flex items-center gap-3 shrink-0">
        <Scale className="w-3.5 h-3.5 text-amber-400 flex-shrink-0" />
        <div className="flex items-center gap-1">
          <span className="text-[9px] font-bold text-gray-400 uppercase tracking-wide">Weighbridge:</span>
          <span className="text-[10px] font-bold text-white uppercase tracking-wide">
            {formData.weighBridgeName || "—"}
          </span>
        </div>
        <span className="text-gray-600 text-[10px]">|</span>
        <div className="flex items-center gap-1">
          <span className="text-[9px] font-bold text-gray-400 uppercase tracking-wide">Scale:</span>
          <span className="text-[10px] font-semibold text-amber-400">
            {formData.scaleName || "—"}
          </span>
        </div>
      </div>

      {/* LIVE WEIGHT HEADER */}
      <div
        className={`mb-3 p-2 rounded-lg border flex justify-between items-center shrink-0 ${
          isSecondWeighing ? "bg-amber-50 border-amber-200" : "bg-amber-50 border-amber-200"
        }`}
      >
        <div>
          <Text className="text-[9px] uppercase font-bold text-gray-400 block leading-none">
            Live Weight
          </Text>
          <div
            className={`text-2xl font-black ${
              isSecondWeighing ? "text-amber-700" : "text-amber-700"
            }`}
          >
            {capturedWeight || 0} <small className="text-xs font-normal">KG</small>
          </div>
        </div>
        <Space size="small">
          {!isSecondWeighing && manualWeighingEnabled && (
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
                  className="w-20 font-bold bg-gray-50"
                  value={formData.firstWeight}
                  readOnly
                />
              </div>
              {manualWeighingEnabled && (
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
              )}
            </>
          )}
        </Space>
      </div>

      {/* NET WEIGHT DISPLAY (Second Weighing Only) */}
      {isSecondWeighing && (
        <div
          className={`mb-4 p-3 rounded-lg border text-center ${
            isValid ? "bg-green-50 border-green-300" : "bg-red-50 border-red-300"
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
            {netWeight.toLocaleString()} <small className="text-sm font-normal">KG</small>
          </div>
          {!isValid && formData.secondWeight && (
            <Text type="danger" className="text-[10px] block mt-2">
              {errorMsg}
            </Text>
          )}
        </div>
      )}

      {/* SECOND WEIGHING INFO ALERT */}
      {isSecondWeighing && (
        <Alert
          message="Second Weighing Mode"
          description={
            <div className="text-[11px]">
              <div><strong>Vehicle:</strong> {formData.noPlate}</div>
              <div><strong>First Weight:</strong> {formData.firstWeight} kg</div>
              <div><strong>Ticket ID:</strong> {formData.ticketID || formData.id}</div>
            </div>
          }
          type="warning"
          showIcon
          className="mb-3"
        />
      )}

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

      {!currentUser && (
        <Alert
          message="Warning"
          description="Operator information not loaded. Please refresh if this persists."
          type="warning"
          showIcon
          className="mb-3"
        />
      )}

      {/* FORM FIELDS - Operator column removed */}
      <div className="flex-1 overflow-y-auto pr-1">
        <Row gutter={[8, 10]}>
          {/* Vehicle Plate */}
          <Col span={12}>
            <div className="flex items-center justify-between mb-0.5">
              <FieldLabel required>Vehicle Plate</FieldLabel>
              <label className="flex items-center gap-1 text-[10px] text-gray-500 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={manualPlate}
                  onChange={(e) => setManualPlate(e.target.checked)}
                  disabled={isSecondWeighing}
                  className="accent-amber-500 cursor-pointer"
                />
                Enter manually
              </label>
            </div>
            {manualPlate ? (
              <Input
                size="middle"
                className="w-full"
                value={formData.noPlate}
                onChange={(e) => setFormData((prev) => ({ ...prev, noPlate: e.target.value.toUpperCase(), vehicleID: null }))}
                disabled={isSecondWeighing}
                placeholder="Type plate number"
              />
            ) : (
              <AutoComplete
                size="middle"
                className="w-full"
                value={formData.noPlate}
                options={filterOptions(vehicles, formData.noPlate, (it) => it.registrationNumber || it.plateNumber || "")}
                onChange={(val) => setFormData((prev) => ({ ...prev, noPlate: val.toUpperCase(), vehicleID: null }))}
                onSelect={(val, opt) => setFormData((prev) => ({ ...prev, noPlate: val.toUpperCase(), vehicleID: opt.id }))}
                disabled={isSecondWeighing}
                placeholder="Type or search plate"
              />
            )}
          </Col>

          {/* Driver */}
          <Col span={12}>
            <FieldLabel>Driver Name</FieldLabel>
            <AutoComplete
              size="middle"
              className="w-full"
              value={formData.driverName}
              options={filterOptions(drivers, formData.driverName, (it) => it.fullName || it.name || "")}
              onChange={(val) => setFormData((prev) => ({ ...prev, driverName: val, driverID: null }))}
              onSelect={(val, opt) => setFormData((prev) => ({ ...prev, driverName: val, driverID: opt.id }))}
              disabled={isSecondWeighing}
              placeholder="Type or search driver"
            />
          </Col>

          {/* Transporter */}
          <Col span={12}>
            <FieldLabel required>Transporter</FieldLabel>
            <AutoComplete
              size="middle"
              className="w-full"
              value={formData.transporterName}
              options={filterOptions(transporters, formData.transporterName, (it) => it.name || "")}
              onChange={(val) => setFormData((prev) => ({ ...prev, transporterName: val, transporterID: null }))}
              onSelect={(val, opt) => setFormData((prev) => ({ ...prev, transporterName: val, transporterID: opt.id }))}
              disabled={isSecondWeighing}
              placeholder="Type or search transporter"
            />
          </Col>

          {/* Commodity */}
          <Col span={12}>
            <FieldLabel>Commodity</FieldLabel>
            <AutoComplete
              size="middle"
              className="w-full"
              value={formData.commodityName}
              options={filterOptions(products, formData.commodityName, (it) => it.name || "")}
              onChange={(val) => setFormData((prev) => ({ ...prev, commodityName: val, commodityID: null }))}
              onSelect={(val, opt) => setFormData((prev) => ({ ...prev, commodityName: val, commodityID: opt.id }))}
              disabled={isSecondWeighing}
              placeholder="Type or search commodity"
            />
          </Col>

          {/* Supplier */}
          <Col span={12}>
            <FieldLabel>Supplier</FieldLabel>
            <AutoComplete
              size="middle"
              className="w-full"
              value={formData.supplierName}
              options={filterOptions(suppliers, formData.supplierName, (it) => it.name || "")}
              onChange={(val) => setFormData((prev) => ({ ...prev, supplierName: val, supplierID: null }))}
              onSelect={(val, opt) => setFormData((prev) => ({ ...prev, supplierName: val, supplierID: opt.id }))}
              disabled={isSecondWeighing}
              placeholder="Type or search supplier"
            />
          </Col>

          {/* Customer */}
          <Col span={12}>
            <FieldLabel>Customer Name</FieldLabel>
            <Input
              size="middle"
              value={formData.customerName}
              onChange={(e) => handleChange("customerName", e.target.value)}
              disabled={isSecondWeighing}
              placeholder="Enter customer"
            />
          </Col>

          {/* Origin + Destination - smaller */}
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

          {/* Weigh Mode */}
          <Col span={8}>
            <FieldLabel>Weigh Mode</FieldLabel>
            <Select
              size="middle"
              className="w-full"
              value={formData.weighMode}
              onChange={(v) => handleChange("weighMode", v)}
              disabled={isSecondWeighing}
            >
              <Option value="entry">Entry</Option>
              <Option value="Gross/Tare">Gross / Tare</Option>
            </Select>
          </Col>

          {/* Operation Type */}
          <Col span={16}>
            <FieldLabel>Operation Type</FieldLabel>
            <Select
              size="middle"
              className="w-full"
              value={formData.operation}
              onChange={(v) => handleChange("operation", v)}
              disabled={isSecondWeighing}
            >
              <Option value="weighing">Weighing</Option>
              <Option value="Inbound Product Receipt">Inbound Receipt</Option>
              <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
            </Select>
          </Col>
        </Row>
      </div>

      {/* FOOTER BUTTONS */}
      <div className="mt-3 pt-3 border-t flex gap-3 shrink-0 bg-white">
        <Button
          size="large"
          className="w-1/3 font-bold"
          style={isSecondWeighing ? { color: "#ef4444", borderColor: "#fca5a5" } : { color: "#9ca3af" }}
          onClick={onTransactionCreated}
        >
          {isSecondWeighing ? "CANCEL" : "RESET"}
        </Button>
        <Button
          size="large"
          loading={loading}
          onClick={isSecondWeighing ? openFinalizePreview : handleSubmit}
          disabled={isSecondWeighing && !isValid}
          style={{ backgroundColor: 'var(--cs-500)', borderColor: 'var(--cs-500)', color: 'white' }}
          className="flex-1 font-bold shadow-md"
        >
          {isSecondWeighing ? "FINALIZE TRANSACTION" : "SAVE FIRST WEIGHT"}
        </Button>
      </div>
    </div>

    {/* ── Finalize Preview Modal ─────────────────────────────────────── */}
    <Modal
      open={showFinalizePreview}
      onCancel={() => setShowFinalizePreview(false)}
      width={720}
      title={
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
            <span className="text-white text-xs font-bold">📋</span>
          </div>
          <span className="text-sm font-bold text-gray-900">
            Transaction Preview — Confirm Before Finalizing
          </span>
          <span className="ml-auto text-[10px] font-bold px-2 py-0.5 rounded-full border border-amber-500 text-amber-600 bg-amber-50">
            {previewEditData.noPlate || "—"}
          </span>
        </div>
      }
      footer={
        <div className="flex gap-2 justify-end">
          <Button onClick={() => setShowFinalizePreview(false)}>Back to Form</Button>
          <Button
            type="primary"
            loading={loading}
            onClick={() => { setShowFinalizePreview(false); doFinalizeSubmit(previewEditData); }}
            style={{ background: "linear-gradient(135deg, var(--cs-500), var(--cs-600))", border: "none" }}
            className="font-bold"
          >
            CONFIRM &amp; FINALIZE
          </Button>
        </div>
      }
    >
      {showFinalizePreview && (
        <div className="space-y-3 text-xs max-h-[70vh] overflow-y-auto pr-1">
          {/* Net Weight Banner */}
          <div className="bg-gradient-to-r from-amber-400 to-orange-500 rounded-lg p-3 text-center shadow-md">
            <div className="text-white text-[10px] font-bold uppercase tracking-wider mb-1">Net Payload</div>
            <div className="text-white text-3xl font-black font-mono">
              {previewNetWeight.toLocaleString()} <small className="text-base font-normal">KG</small>
            </div>
            <div className="text-white/80 text-[10px] mt-1">
              W1: {previewEditData.firstWeight || 0} kg → W2: {previewEditData.secondWeight || 0} kg
            </div>
          </div>

          {/* Basic Info */}
          <div className="bg-gradient-to-br from-amber-50 to-orange-50 rounded-lg p-3 border-2 border-amber-200">
            <div className="text-[10px] font-extrabold text-amber-800 uppercase tracking-wide mb-2">📋 Basic Information</div>
            <div className="grid grid-cols-2 gap-2">
              {[
                { label: "Ticket ID", field: "ticketID", readOnly: true },
                { label: "Vehicle Plate", field: "noPlate", readOnly: true },
                { label: "Driver", field: "driverName", readOnly: false },
                { label: "Commodity", field: "commodityName", readOnly: false },
                { label: "Weigh Mode", field: "weighMode", readOnly: false },
                { label: "Operation", field: "operation", readOnly: false },
              ].map(({ label, field, readOnly }) => (
                <div key={field} className="bg-white/80 rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[9px] font-bold uppercase mb-1">{label}</div>
                  {readOnly ? (
                    <div className="font-bold text-gray-900 text-xs">{previewEditData[field] || "—"}</div>
                  ) : (
                    <Input
                      size="small"
                      value={previewEditData[field] || ""}
                      onChange={(e) => setPreviewEditData(prev => ({ ...prev, [field]: e.target.value }))}
                      className="text-xs border-amber-300 focus:border-amber-500"
                    />
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Parties */}
          <div className="bg-gradient-to-br from-amber-50 to-orange-50 rounded-lg p-3 border-2 border-amber-200">
            <div className="text-[10px] font-extrabold text-amber-800 uppercase tracking-wide mb-2">🏢 Parties</div>
            <div className="grid grid-cols-2 gap-2">
              {[
                { label: "Transporter", field: "transporterName" },
                { label: "Supplier", field: "supplierName" },
                { label: "Customer", field: "customerName" },
                { label: "Operator", field: "operatorName" },
              ].map(({ label, field }) => (
                <div key={field} className="bg-white/80 rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[9px] font-bold uppercase mb-1">{label}</div>
                  <Input
                    size="small"
                    value={previewEditData[field] || ""}
                    onChange={(e) => setPreviewEditData(prev => ({ ...prev, [field]: e.target.value }))}
                    className="text-xs border-amber-300"
                  />
                </div>
              ))}
            </div>
          </div>

          {/* Locations */}
          <div className="bg-gradient-to-br from-amber-50 to-orange-50 rounded-lg p-3 border-2 border-amber-200">
            <div className="text-[10px] font-extrabold text-amber-800 uppercase tracking-wide mb-2">📍 Locations</div>
            <div className="grid grid-cols-2 gap-2">
              {[
                { label: "Origin", field: "originName" },
                { label: "Destination", field: "destinationName" },
                { label: "Scale / Weighbridge", field: "scaleName" },
              ].map(({ label, field }) => (
                <div key={field} className="bg-white/80 rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[9px] font-bold uppercase mb-1">{label}</div>
                  <Input
                    size="small"
                    value={previewEditData[field] || ""}
                    onChange={(e) => setPreviewEditData(prev => ({ ...prev, [field]: e.target.value }))}
                    className="text-xs border-amber-300"
                  />
                </div>
              ))}
            </div>
          </div>

          {/* Weight Summary */}
          <div className="bg-gradient-to-br from-amber-50 to-orange-50 rounded-lg p-3 border-2 border-amber-200">
            <div className="text-[10px] font-extrabold text-amber-800 uppercase tracking-wide mb-2">⚖️ Weight Summary</div>
            <div className="grid grid-cols-3 gap-2">
              <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 text-center">
                <div className="text-[9px] text-amber-700 font-bold uppercase mb-1">1st Weight</div>
                <div className="text-lg font-extrabold text-orange-600">{previewEditData.firstWeight || 0}</div>
                <div className="text-[9px] text-gray-500">KG (locked)</div>
              </div>
              <div className="bg-white rounded-lg p-2.5 border-2 border-amber-400 text-center">
                <div className="text-[9px] text-amber-700 font-bold uppercase mb-1">2nd Weight</div>
                <Input
                  size="small"
                  type="number"
                  value={previewEditData.secondWeight || ""}
                  onChange={(e) => setPreviewEditData(prev => ({ ...prev, secondWeight: e.target.value }))}
                  className="text-center font-bold text-lg border-amber-400"
                />
                <div className="text-[9px] text-gray-500 mt-1">KG</div>
              </div>
              <div className="bg-gradient-to-br from-amber-200 to-orange-300 rounded-lg p-2.5 border-2 border-amber-500 text-center">
                <div className="text-[9px] text-amber-900 font-extrabold uppercase mb-1">Net Weight</div>
                <div className="text-lg font-black text-amber-950">{previewNetWeight.toLocaleString()}</div>
                <div className="text-[9px] text-amber-800 font-bold">KG</div>
              </div>
            </div>
          </div>

          {/* Notes */}
          <div className="bg-gradient-to-br from-amber-50 to-orange-50 rounded-lg p-3 border-2 border-amber-200">
            <div className="text-[10px] font-extrabold text-amber-800 uppercase tracking-wide mb-2">📝 Notes / Remarks</div>
            <Input.TextArea
              rows={2}
              value={previewEditData.notes || ""}
              onChange={(e) => setPreviewEditData(prev => ({ ...prev, notes: e.target.value }))}
              placeholder="Add any remarks..."
              className="text-xs border-amber-300 focus:border-amber-500"
            />
          </div>
        </div>
      )}
    </Modal>
    </>
  );
}