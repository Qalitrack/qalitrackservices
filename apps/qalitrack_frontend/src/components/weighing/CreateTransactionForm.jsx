import React, { useEffect, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Input, Select, Button, message, Row, Col, Typography, Space } from "antd";
import { debounce } from "lodash";
import {
  fetchVehiclesByRegNumber, fetchDriversByName, fetchProductsByName,
  fetchSuppliersByName, fetchTransportersByName, fetchWeighbridgesByName,
  addTransaction, fetchCurrentUser,
} from "../../store/weighingSlice";

const { Option } = Select;
const { Text } = Typography;

export default function CreateTransactionForm({ formData, setFormData, capturedWeight, onTransactionCreated, weighbridges }) {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, suppliers, transporters, loading } = useSelector((state) => state.weighing);

  // Debounced search logic (Same as before)
  const debounced = useMemo(() => ({
    vehicles: debounce((q) => dispatch(fetchVehiclesByRegNumber(q)), 400),
    drivers: debounce((q) => dispatch(fetchDriversByName(q)), 400),
    products: debounce((q) => dispatch(fetchProductsByName(q)), 400),
    suppliers: debounce((q) => dispatch(fetchSuppliersByName(q)), 400),
    transporters: debounce((q) => dispatch(fetchTransportersByName(q)), 400),
    weighbridges: debounce((q) => dispatch(fetchWeighbridgesByName(q)), 400),
  }), [dispatch]);

  const handleChange = (field, value) => setFormData(prev => ({ ...prev, [field]: value }));
  
  const handleSelect = (list, id, idKey, nameKey) => {
    const item = list.find(i => i.id === id);
    setFormData(p => ({ ...p, [idKey]: id || null, [nameKey]: item?.name || item?.registrationNumber || item?.fullName || "" }));
  };

  // Modern Label Component
  const FieldLabel = ({ children, required }) => (
    <label className="text-[10px] font-bold text-gray-500 uppercase tracking-tight block mb-0.5">
      {children} {required && <span className="text-red-500">*</span>}
    </label>
  );

  return (
    <div className="flex flex-col h-full bg-white">
      {/* 1. INDICATOR BANNER - Modern Inset Style */}
      <div className={`mb-3 p-2 rounded-lg border flex justify-between items-center shrink-0 ${formData.id ? 'bg-blue-50 border-blue-200' : 'bg-amber-50 border-amber-200'}`}>
        <div>
          <Text className="text-[9px] uppercase font-bold text-gray-400 block leading-none">Live Weight</Text>
          <div className={`text-2xl font-black ${formData.id ? 'text-blue-700' : 'text-amber-700'}`}>
            {capturedWeight || 0} <small className="text-xs font-normal">KG</small>
          </div>
        </div>
        <Space size="small">
          <div className="text-right">
            <FieldLabel>Manual W1</FieldLabel>
            <Input size="small" className="w-20 font-bold" value={formData.firstWeight} onChange={e => handleChange("firstWeight", e.target.value)} />
          </div>
          {formData.id && (
            <div className="text-right">
              <FieldLabel>Manual W2</FieldLabel>
              <Input size="small" className="w-20 font-bold" value={formData.secondWeight} onChange={e => handleChange("secondWeight", e.target.value)} />
            </div>
          )}
        </Space>
      </div>

      {/* 2. SCROLLABLE FORM FIELDS - 3 Column Layout */}
      <div className="flex-1 overflow-y-auto pr-1">
        <Row gutter={[8, 10]}>
          <Col span={8}>
            <FieldLabel required>Receipt Number</FieldLabel>
            <Input placeholder="REQ-001" size="middle" value={formData.receiptNo} onChange={e => handleChange("receiptNo", e.target.value)} />
          </Col>
          <Col span={8}>
            <FieldLabel required>Weighbridge</FieldLabel>
            <Select size="middle" className="w-full" showSearch onSearch={debounced.weighbridges} onChange={id => handleSelect(weighbridges, id, "weighBridgeId", "weighBridgeName")} value={formData.weighBridgeId}>
              {weighbridges.map(it => <Option key={it.id} value={it.id}>{it.name}</Option>)}
            </Select>
          </Col>
          <Col span={8}>
            <FieldLabel>Scale Name</FieldLabel>
            <Input size="middle" value={formData.scaleName} onChange={e => handleChange("scaleName", e.target.value)} />
          </Col>

          <Col span={12}>
            <FieldLabel required>Vehicle Plate</FieldLabel>
            <Select size="middle" showSearch className="w-full font-mono" onSearch={debounced.vehicles} onChange={id => handleSelect(vehicles, id, "vehicleId", "noPlate")} value={formData.vehicleId || formData.noPlate}>
              {vehicles.map(it => <Option key={it.id} value={it.id}>{it.registrationNumber || it.plateNumber}</Option>)}
            </Select>
          </Col>
          <Col span={12}>
            <FieldLabel required>Driver Name</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.drivers} onChange={id => handleSelect(drivers, id, "driverId", "driverName")} value={formData.driverId || formData.driverName}>
              {drivers.map(it => <Option key={it.id} value={it.id}>{it.fullName || it.name}</Option>)}
            </Select>
          </Col>

          <Col span={8}>
            <FieldLabel required>Transporter</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.transporters} onChange={id => handleSelect(transporters, id, "transporterId", "transporterName")} value={formData.transporterId}>
              {transporters.map(it => <Option key={it.id} value={it.id}>{it.name}</Option>)}
            </Select>
          </Col>
          <Col span={8}>
            <FieldLabel>Commodity/Product</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.products} onChange={id => handleSelect(products, id, "commodityId", "commodityName")} value={formData.commodityId}>
              {products.map(it => <Option key={it.id} value={it.id}>{it.name}</Option>)}
            </Select>
          </Col>
          <Col span={8}>
            <FieldLabel>Supplier</FieldLabel>
            <Select size="middle" showSearch className="w-full" onSearch={debounced.suppliers} onChange={id => handleSelect(suppliers, id, "supplierId", "supplierName")} value={formData.supplierId}>
              {suppliers.map(it => <Option key={it.id} value={it.id}>{it.name}</Option>)}
            </Select>
          </Col>

          <Col span={12}>
            <FieldLabel>Customer Name</FieldLabel>
            <Input size="middle" value={formData.customerName} onChange={e => handleChange("customerName", e.target.value)} />
          </Col>
          <Col span={6}>
            <FieldLabel>Origin</FieldLabel>
            <Input size="middle" value={formData.originName} onChange={e => handleChange("originName", e.target.value)} />
          </Col>
          <Col span={6}>
            <FieldLabel>Destination</FieldLabel>
            <Input size="middle" value={formData.destinationName} onChange={e => handleChange("destinationName", e.target.value)} />
          </Col>

          <Col span={8}>
            <FieldLabel>Weigh Mode</FieldLabel>
            <Select size="middle" className="w-full" value={formData.weighMode} onChange={v => handleChange("weighMode", v)}>
              <Option value="Gross/Tare">Gross / Tare</Option>
            </Select>
          </Col>
          <Col span={16}>
            <FieldLabel>Operation Type</FieldLabel>
            <Select size="middle" className="w-full" value={formData.operation} onChange={v => handleChange("operation", v)}>
              <Option value="Inbound Product Receipt">Inbound Receipt</Option>
              <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
            </Select>
          </Col>
        </Row>
      </div>

      {/* 3. FIXED ACTION BUTTONS */}
      <div className="mt-3 pt-3 border-t flex gap-3 shrink-0 bg-white">
        <Button size="large" className="w-1/3 text-gray-400 font-bold border-gray-200">
          RESET
        </Button>
        <Button 
          type="primary" 
          size="large" 
          className={`flex-1 font-bold border-none shadow-md ${formData.id ? 'bg-blue-600' : 'bg-amber-500 hover:bg-amber-600'}`}
        >
          {formData.id ? "FINALIZE TRANSACTION" : "SAVE FIRST WEIGHT"}
        </Button>
      </div>
    </div>
  );
}