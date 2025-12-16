// src/components/weighing/CreateTransactionForm.jsx
import React, { useEffect, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Card, Input, Select, Button, message } from "antd";
import { debounce } from "lodash";
import {
  fetchVehiclesByRegNumber,
  fetchDriversByName,
  fetchProductsByName,
  fetchSuppliersByName,
  fetchTransportersByName,
  fetchWeighbridgesByName,
  addTransaction,
} from "../../store/weighingSlice";

const { Option } = Select;

export default function CreateTransactionForm({
  formData,
  setFormData,
  capturedWeight,
  onTransactionCreated,
}) {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, suppliers, transporters, weighbridges, loading } =
    useSelector((state) => state.weighing);

  const debounced = useMemo(
    () => ({
      vehicles: debounce((q) => dispatch(fetchVehiclesByRegNumber(q)), 400),
      drivers: debounce((q) => dispatch(fetchDriversByName(q)), 400),
      products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
      suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
      transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
      weighbridges: debounce((q) => dispatch(fetchWeighbridgesByName(q)), 400),
    }),
    [dispatch]
  );

  useEffect(() => {
    return () => Object.values(debounced).forEach((fn) => fn.cancel());
  }, [debounced]);

  useEffect(() => {
    if (capturedWeight && !formData.firstWeight) {
      setFormData((prev) => ({ ...prev, firstWeight: capturedWeight.toString() }));
    }
  }, [capturedWeight, formData.firstWeight, setFormData]);

  const makeOptions = (items = [], idField = "id", labelField = "name") =>
    items.map((it) => (
      <Option key={it[idField]} value={it[idField]}>
        {it[labelField] ?? it[idField]}
      </Option>
    ));

  const handleChange = (field, value) => setFormData((prev) => ({ ...prev, [field]: value }));

  const handleVehicleSelect = (vehicleId) => {
    const vehicle = vehicles.find((v) => v.id === vehicleId);
    setFormData((prev) => ({
      ...prev,
      vehicleId: vehicleId || null,
      noPlate: vehicle?.registrationNumber || "",
    }));
  };

  const handleDriverSelect = (driverId) => {
    const driver = drivers.find((d) => d.id === driverId);
    setFormData((prev) => ({ ...prev, driverName: driver?.fullName || "" }));
  };

  const handleProductSelect = (commodityId) => {
    const product = products.find((p) => p.id === commodityId);
    setFormData((prev) => ({
      ...prev,
      commodityId: commodityId || null,
      commodityName: product?.name || "",
    }));
  };

  const handleTransporterSelect = (transporterId) => {
    const transporter = transporters.find((t) => t.id === transporterId);
    setFormData((prev) => ({
      ...prev,
      transporterId,
      transporterName: transporter?.name || "",
    }));
  };

  const handleSupplierSelect = (supplierId) => {
    const supplier = suppliers.find((s) => s.id === supplierId);
    setFormData((prev) => ({
      ...prev,
      supplierId: supplierId || null,
      supplierName: supplier?.name || "",
    }));
  };

  const handleWeighbridgeSelect = (weighbridgeId) => {
    const wb = weighbridges.find((w) => w.id === weighbridgeId);
    setFormData((prev) => ({
      ...prev,
      weighBridgeId: weighbridgeId || null,
      weighBridgeName: wb?.name || "",
      scaleName: wb?.scaleName || "Scale-01",
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.receiptNo?.trim()) return message.error("Receipt Number is required");
    if (!formData.noPlate?.trim()) return message.error("Vehicle Plate Number is required");
    if (!formData.driverName?.trim()) return message.error("Driver Name is required");
    if (!formData.transporterId) return message.error("Transporter is required");
    if (!formData.weighBridgeId) return message.error("Weighbridge is required");

    try {
      const payload = {
        receiptNo: formData.receiptNo.trim(),
        expectedWeighings: formData.expectedWeighings || 2,
        noPlate: formData.noPlate.trim().toUpperCase(),
        driverName: formData.driverName.trim(),
        transporterId: formData.transporterId,
        weighBridgeId: formData.weighBridgeId,
        ...(formData.vehicleId && { vehicleId: formData.vehicleId }),
        ...(formData.commodityId && { commodityId: formData.commodityId }),
        ...(formData.supplierId && { supplierId: formData.supplierId }),
        ...(formData.customerId && { customerId: formData.customerId }),
        ...(formData.originId && { originId: formData.originId }),
        ...(formData.destinationId && { destinationId: formData.destinationId }),
        ...(formData.transporterName?.trim() && { transporterName: formData.transporterName.trim() }),
        ...(formData.commodityName?.trim() && { commodityName: formData.commodityName.trim() }),
        ...(formData.supplierName?.trim() && { supplierName: formData.supplierName.trim() }),
        ...(formData.customerName?.trim() && { customerName: formData.customerName.trim() }),
        ...(formData.originName?.trim() && { originName: formData.originName.trim() }),
        ...(formData.destinationName?.trim() && { destinationName: formData.destinationName.trim() }),
        ...(formData.weighMode?.trim() && { weighMode: formData.weighMode.trim() }),
        ...(formData.operation?.trim() && { operation: formData.operation.trim() }),
        ...(formData.weighBridgeName?.trim() && { weighBridgeName: formData.weighBridgeName.trim() }),
        ...(formData.firstWeight && !isNaN(formData.firstWeight) && {
          firstWeight: parseFloat(formData.firstWeight),
          ...(formData.scaleName?.trim() && { scaleName: formData.scaleName.trim() }),
          ...(formData.operatorName?.trim() && { operatorName: formData.operatorName.trim() }),
        }),
      };

      await dispatch(addTransaction(payload)).unwrap();
      message.success("Transaction created successfully!");
      handleReset();
      if (typeof onTransactionCreated === "function") onTransactionCreated();
    } catch (err) {
      message.error(err?.message || "Failed to create transaction");
      console.error(err);
    }
  };

  const handleReset = () => {
    setFormData({
      receiptNo: "",
      expectedWeighings: 2,
      noPlate: "",
      driverName: "",
      vehicleId: null,
      commodityId: null,
      transporterId: null,
      transporterName: "",
      supplierId: null,
      supplierName: "",
      customerId: null,
      customerName: "",
      originName: "",
      destinationName: "",
      operation: "",
      weighMode: "",
      firstWeight: "",
      weighBridgeId: null,
      weighBridgeName: "",
      scaleName: "",
      operatorName: "",
    });
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-start justify-center py-10 px-4">
      <Card
        title="New Weighing Transaction"
        className="w-full max-w-4xl shadow-xl"
        bodyStyle={{ padding: "2rem" }}
      >
        <form className="space-y-6" onSubmit={handleSubmit}>
          {/* Receipt & Expected Weighings */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Receipt Number <span className="text-red-500">*</span>
              </label>
              <Input
                value={formData.receiptNo}
                onChange={(e) => handleChange("receiptNo", e.target.value)}
                placeholder="e.g. WB-2025-001"
              />
            </div>
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Expected Weighings <span className="text-red-500">*</span>
              </label>
              <Select
                value={formData.expectedWeighings}
                onChange={(v) => handleChange("expectedWeighings", v)}
                placeholder="Select number of weighings"
                className="w-full"
              >
                <Option value={2}>2 Weighings (Gross/Tare)</Option>
                <Option value={3}>3 Weighings</Option>
                <Option value={4}>4 Weighings</Option>
              </Select>
            </div>
          </div>

          {/* Vehicle & Driver */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Vehicle Plate <span className="text-red-500">*</span>
              </label>
              <Select
                showSearch
                placeholder="Search by registration number"
                onSearch={debounced.vehicles}
                onChange={handleVehicleSelect}
                filterOption={false}
                value={formData.vehicleId}
                allowClear
                loading={loading}
                className="w-full"
              >
                {makeOptions(vehicles, "id", "registrationNumber")}
              </Select>
              <Input
                className="mt-2"
                value={formData.noPlate}
                onChange={(e) => handleChange("noPlate", e.target.value.toUpperCase())}
                placeholder="Or type manually"
              />
            </div>
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Driver Name <span className="text-red-500">*</span>
              </label>
              <Select
                showSearch
                placeholder="Search driver"
                onSearch={debounced.drivers}
                onChange={handleDriverSelect}
                filterOption={false}
                allowClear
                loading={loading}
                className="w-full"
              >
                {makeOptions(drivers, "id", "fullName")}
              </Select>
              <Input
                className="mt-2"
                value={formData.driverName}
                onChange={(e) => handleChange("driverName", e.target.value)}
                placeholder="Or type manually"
              />
            </div>
          </div>

          {/* Transporter & Weighbridge */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Transporter <span className="text-red-500">*</span>
              </label>
              <Select
                showSearch
                placeholder="Search transporter"
                onSearch={debounced.transporters}
                onChange={handleTransporterSelect}
                filterOption={false}
                value={formData.transporterId}
                loading={loading}
                className="w-full"
              >
                {makeOptions(transporters)}
              </Select>
            </div>
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">
                Weighbridge <span className="text-red-500">*</span>
              </label>
              <Select
                showSearch
                placeholder="Search weighbridge"
                onSearch={debounced.weighbridges}
                onChange={handleWeighbridgeSelect}
                filterOption={false}
                value={formData.weighBridgeId}
                loading={loading}
                className="w-full"
              >
                {makeOptions(weighbridges, "id", "name")}
              </Select>
            </div>
          </div>

          {/* Optional fields */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block font-medium text-gray-700 mb-1">Commodity</label>
              <Select
                showSearch
                placeholder="Search commodity"
                onSearch={debounced.products}
                onChange={handleProductSelect}
                filterOption={false}
                value={formData.commodityId}
                allowClear
                className="w-full"
              >
                {makeOptions(products)}
              </Select>
            </div>
            <div>
              <label className="block font-medium text-gray-700 mb-1">Supplier</label>
              <Select
                showSearch
                placeholder="Search supplier"
                onSearch={debounced.suppliers}
                onChange={handleSupplierSelect}
                filterOption={false}
                value={formData.supplierId}
                allowClear
                className="w-full"
              >
                {makeOptions(suppliers)}
              </Select>
            </div>
            <div>
              <label className="block font-medium text-gray-700 mb-1">Customer</label>
              <Input
                value={formData.customerName}
                onChange={(e) => handleChange("customerName", e.target.value)}
                placeholder="Customer name"
              />
            </div>
          </div>

          {/* Origin & Destination */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">Origin</label>
              <Input
                value={formData.originName}
                onChange={(e) => handleChange("originName", e.target.value)}
                placeholder="e.g. Farm, Warehouse"
              />
            </div>
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">Destination</label>
              <Input
                value={formData.destinationName}
                onChange={(e) => handleChange("destinationName", e.target.value)}
                placeholder="e.g. Factory, Market"
              />
            </div>
          </div>

          {/* Operation & Weigh Mode */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">Operation</label>
              <Select
                value={formData.operation}
                onChange={(v) => handleChange("operation", v)}
                placeholder="Select operation"
                allowClear
                className="w-full"
              >
                <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                <Option value="Internal Transfer">Internal Transfer</Option>
              </Select>
            </div>
            <div className="flex-1">
              <label className="block font-medium text-gray-700 mb-1">Weigh Mode</label>
              <Select
                value={formData.weighMode}
                onChange={(v) => handleChange("weighMode", v)}
                placeholder="Select weigh mode"
                allowClear
                className="w-full"
              >
                <Option value="Gross/Tare">Gross/Tare</Option>
                <Option value="Single">Single Weighing</Option>
              </Select>
            </div>
          </div>

          {/* First Weight, Operator, Scale */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block font-medium text-gray-700 mb-1">
                First Weight (kg)
                {formData.firstWeight && <span className="text-green-600 ml-2">(Captured)</span>}
              </label>
              <Input
                type="number"
                value={formData.firstWeight}
                onChange={(e) => handleChange("firstWeight", e.target.value)}
                placeholder="Auto-filled on capture"
                className="bg-gray-50"
              />
            </div>
            <div>
              <label className="block font-medium text-gray-700 mb-1">Operator Name</label>
              <Input
                value={formData.operatorName}
                onChange={(e) => handleChange("operatorName", e.target.value)}
                placeholder="Operator"
              />
            </div>
            <div>
              <label className="block font-medium text-gray-700 mb-1">Scale Name</label>
              <Input
                value={formData.scaleName}
                onChange={(e) => handleChange("scaleName", e.target.value)}
                placeholder="e.g. Main Platform, Scale-01"
              />
            </div>
          </div>

          {/* Buttons */}
          <div className="flex flex-col sm:flex-row justify-end gap-4 mt-6">
            <Button size="large" onClick={handleReset}>
              Reset Form
            </Button>
            <Button type="primary" size="large" htmlType="submit" loading={loading}>
              Create Transaction
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
