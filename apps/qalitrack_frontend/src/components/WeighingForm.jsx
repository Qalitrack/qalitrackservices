import React, { useEffect, useState } from "react";
import LiveWeighbridgeStatus from "./weighing/LiveWeighbridgeStatus";
import { useDispatch, useSelector } from "react-redux";
import { Form, Input, Select, Button, message, Card, Tabs, Table, Tag, Modal } from "antd";
import {
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
  fetchTransactions,
  addWeighing as addWeighingThunk,
  fetchSimulatedWeight,
} from "../store/weighingSlice";
import { debounce } from "lodash";

const { Option } = Select;
const { TabPane } = Tabs;

/* ---------------------------------------------------------------------------
   Incomplete Transactions Table
----------------------------------------------------------------------------*/
function IncompleteTransactions({ onAddWeighing, refreshKey }) {
  const dispatch = useDispatch();
  const { transactions, loading } = useSelector((state) => state.weighing);
  const [localTransactions, setLocalTransactions] = useState([]);

  useEffect(() => {
    dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
  }, [dispatch, refreshKey]);

  useEffect(() => {
    const incomplete = transactions.filter(t => !t.isCompleted);
    setLocalTransactions(incomplete);
  }, [transactions]);

  const columns = [
    {
      title: 'Receipt No',
      dataIndex: 'receiptNo',
      key: 'receiptNo',
    },
    {
      title: 'Vehicle',
      dataIndex: 'noPlate',
      key: 'noPlate',
    },
    {
      title: 'Driver',
      dataIndex: 'driverName',
      key: 'driverName',
    },
    {
      title: 'Commodity',
      dataIndex: 'commodityName',
      key: 'commodityName',
    },
    {
      title: 'Weighings',
      key: 'weighings',
      render: (_, record) => (
          <span>
          {record.completedWeighings || 0} / {record.expectedWeighings || 2}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => (
          <Tag color={status === 'Pending' ? 'orange' : 'blue'}>{status}</Tag>
      ),
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, record) => (
          <Button
              type="primary"
              size="small"
              onClick={() => onAddWeighing(record)}
              disabled={record.isCompleted}
          >
            Add Weighing
          </Button>
      ),
    },
  ];

  return (
      <div>
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-semibold">Incomplete Transactions</h3>
          <Button onClick={() => dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }))}>
            Refresh
          </Button>
        </div>
        <Table
            columns={columns}
            dataSource={localTransactions}
            rowKey="id"
            loading={loading}
            pagination={{ pageSize: 10 }}
        />
      </div>
  );
}

/* ---------------------------------------------------------------------------
   Add Weighing Modal
----------------------------------------------------------------------------*/
function AddWeighingModal({ visible, transaction, capturedWeight, onClose, onSuccess }) {
  const dispatch = useDispatch();
  const [formData, setFormData] = useState({
    weight: '',
    operatorName: '',
    scaleName: '',
    notes: ''
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (visible && capturedWeight) {
      setFormData(prev => ({ ...prev, weight: capturedWeight }));
    }
  }, [visible, capturedWeight]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.weight) {
      message.error('Please enter weight');
      return;
    }

    setLoading(true);
    try {
      const payload = {
        transactionId: transaction.id,
        weight: parseFloat(formData.weight),
        weighBridgeId: "00000000-0000-0000-0000-000000000001", // ← Hardcoded GUID for testing
        weighBridgeName: "Main Scale",
        scaleName: formData.scaleName || "Scale-01",
        operatorId: 1,
        operatorName: formData.operatorName || "Operator",
        notes: formData.notes || "",
      };

      const result = await dispatch(addWeighingThunk(payload)).unwrap();

      const completedWeighings = result.data?.completedWeighings || result.completedWeighings;
      const expectedWeighings = result.data?.expectedWeighings || result.expectedWeighings;

      message.success(`Weighing ${completedWeighings} of ${expectedWeighings} added successfully!`);
      setFormData({ weight: '', operatorName: '', scaleName: '', notes: '' });
      onSuccess();
      onClose();
    } catch (error) {
      message.error(error?.message || 'Failed to add weighing');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
      <Modal
          title={`Add Weighing - ${transaction?.receiptNo || ''}`}
          open={visible}
          onCancel={onClose}
          footer={null}
          width={600}
      >
        {transaction && (
            <div className="mb-4 p-4 bg-gray-50 rounded">
              <div className="grid grid-cols-2 gap-2 text-sm">
                <div><strong>Vehicle:</strong> {transaction.noPlate}</div>
                <div><strong>Driver:</strong> {transaction.driverName}</div>
                <div><strong>Commodity:</strong> {transaction.commodityName}</div>
                <div><strong>Progress:</strong> {transaction.completedWeighings || 0} / {transaction.expectedWeighings || 2}</div>
              </div>
            </div>
        )}

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Weight (kg) *</label>
            <Input
                type="number"
                value={formData.weight}
                onChange={(e) => handleChange('weight', e.target.value)}
                placeholder="Enter weight"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Operator Name</label>
            <Input
                value={formData.operatorName}
                onChange={(e) => handleChange('operatorName', e.target.value)}
                placeholder="Operator name"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Scale Name</label>
            <Input
                value={formData.scaleName}
                onChange={(e) => handleChange('scaleName', e.target.value)}
                placeholder="Scale name"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Notes</label>
            <Input.TextArea
                rows={3}
                value={formData.notes}
                onChange={(e) => handleChange('notes', e.target.value)}
                placeholder="Optional notes"
            />
          </div>

          <div className="flex justify-end gap-2 pt-4">
            <Button onClick={onClose}>Cancel</Button>
            <Button type="primary" onClick={handleSubmit} loading={loading}>
              Add Weighing
            </Button>
          </div>
        </div>
      </Modal>
  );
}

/* ---------------------------------------------------------------------------
   Main Component
----------------------------------------------------------------------------*/
export default function WeighingDashboard() {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, routes, suppliers, saccos, transporters, loading, error } =
      useSelector((state) => state.weighing || {});

  const [capturedWeight, setCapturedWeight] = useState(null);
  const [formData, setFormData] = useState({
    receiptNo: '',
    expectedWeighings: 2,
    noPlate: '',
    driverName: '',
    vehicleId: null,
    driverId: null,
    commodityId: null,
    commodityName: '',
    transporterId: null,
    transporterName: '',
    supplierId: null,
    supplierName: '',
    operation: '',
    firstWeight: ''
  });
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [showWeighingModal, setShowWeighingModal] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

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

  useEffect(() => {
    if (capturedWeight !== null) {
      setFormData(prev => ({ ...prev, firstWeight: capturedWeight }));
    }
  }, [capturedWeight]);

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

  const handleChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleVehicleSelect = (vehicleId) => {
    const vehicle = vehicles.find(v => v.id === vehicleId);
    setFormData(prev => ({
      ...prev,
      vehicleId,
      noPlate: vehicle?.registrationNumber || ''
    }));
  };

  const handleDriverSelect = (driverId) => {
    const driver = drivers.find(d => d.id === driverId);
    setFormData(prev => ({
      ...prev,
      driverId,
      driverName: driver?.fullName || ''
    }));
  };

  const handleProductSelect = (productId) => {
    const product = products.find(p => p.id === productId);
    setFormData(prev => ({
      ...prev,
      commodityId: productId,
      commodityName: product?.name || ''
    }));
  };

  const handleTransporterSelect = (transporterId) => {
    const transporter = transporters.find(t => t.id === transporterId);
    setFormData(prev => ({
      ...prev,
      transporterId,
      transporterName: transporter?.name || ''
    }));
  };

  const handleSupplierSelect = (supplierId) => {
    const supplier = suppliers.find(s => s.id === supplierId);
    setFormData(prev => ({
      ...prev,
      supplierId,
      supplierName: supplier?.name || ''
    }));
  };

  const handleCreateTransaction = async (e) => {
    e.preventDefault();

    if (!formData.receiptNo || !formData.noPlate || !formData.driverName || !formData.transporterId) {
      message.error('Please fill in all required fields');
      return;
    }

    try {
      const payload = {
        receiptNo: formData.receiptNo,
        expectedWeighings: parseInt(formData.expectedWeighings) || 2,
        noPlate: formData.noPlate,
        driverName: formData.driverName,
        transporterId: parseInt(formData.transporterId),
        transporterName: formData.transporterName || "",

        ...(formData.vehicleId && { vehicleId: parseInt(formData.vehicleId) }),

        ...(formData.commodityId && { commodityId: parseInt(formData.commodityId) }),
        ...(formData.supplierId && { supplierId: parseInt(formData.supplierId) }),

        commodityName: formData.commodityName || "",
        supplierName: formData.supplierName || "",
        weighMode: "Gross/Tare",
        operation: formData.operation || "",

        // First weight with hardcoded GUID
        ...(formData.firstWeight && {
          firstWeight: parseFloat(formData.firstWeight),
          weighBridgeId: "00000000-0000-0000-0000-000000000001", // ← Hardcoded GUID for testing
          weighBridgeName: "Main Scale",
          scaleName: "Scale-01",
          operatorId: 1,
          operatorName: "Operator"
        }),
      };

      await dispatch(addTransaction(payload)).unwrap();
      message.success('Transaction created successfully!');

      setFormData({
        receiptNo: '',
        expectedWeighings: 2,
        noPlate: '',
        driverName: '',
        vehicleId: null,
        driverId: null,
        commodityId: null,
        commodityName: '',
        transporterId: null,
        transporterName: '',
        supplierId: null,
        supplierName: '',
        operation: '',
        firstWeight: ''
      });
      setCapturedWeight(null);
      setRefreshKey(prev => prev + 1);

      dispatch(fetchTransactions({ pageNumber: 1, pageSize: 50 }));
    } catch (err) {
      message.error(err?.message || 'Error creating transaction');
    }
  };

  const handleAddWeighing = (transaction) => {
    setSelectedTransaction(transaction);
    setShowWeighingModal(true);
  };

  const handleWeighingSuccess = () => {
    setRefreshKey(prev => prev + 1);
    dispatch(fetchTransactions({ pageNumber: 1, pageSize: 50 }));
  };

  return (
      <div className="p-6 max-w-7xl mx-auto">
        <Tabs defaultActiveKey="1">
          <TabPane tab="Create Transaction" key="1">
            <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
              <div className="lg:col-span-2">
                <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
              </div>

              <div className="lg:col-span-3">
                <Card title="New Transaction">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">Receipt Number *</label>
                      <Input
                          value={formData.receiptNo}
                          onChange={(e) => handleChange('receiptNo', e.target.value)}
                          placeholder="WB-2025-001"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Expected Weighings *</label>
                      <Select
                          value={formData.expectedWeighings}
                          onChange={(value) => handleChange('expectedWeighings', value)}
                          style={{ width: '100%' }}
                      >
                        <Option value={2}>2 Weighings (Standard)</Option>
                        <Option value={3}>3 Weighings</Option>
                        <Option value={4}>4 Weighings</Option>
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Vehicle *</label>
                      <Select
                          showSearch
                          placeholder="Search vehicle"
                          onSearch={debounced.vehicles}
                          onChange={handleVehicleSelect}
                          filterOption={false}
                          value={formData.vehicleId}
                          style={{ width: '100%' }}
                      >
                        {makeOptions(vehicles, "id", "registrationNumber")}
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Driver *</label>
                      <Select
                          showSearch
                          placeholder="Search driver"
                          onSearch={debounced.drivers}
                          onChange={handleDriverSelect}
                          filterOption={false}
                          value={formData.driverId}
                          style={{ width: '100%' }}
                      >
                        {makeOptions(drivers, "id", "fullName")}
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Product</label>
                      <Select
                          showSearch
                          placeholder="Search product"
                          onSearch={debounced.products}
                          onChange={handleProductSelect}
                          filterOption={false}
                          value={formData.commodityId}
                          style={{ width: '100%' }}
                      >
                        {makeOptions(products)}
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Transporter *</label>
                      <Select
                          showSearch
                          placeholder="Search transporter"
                          onSearch={debounced.transporters}
                          onChange={handleTransporterSelect}
                          filterOption={false}
                          value={formData.transporterId}
                          style={{ width: '100%' }}
                      >
                        {makeOptions(transporters)}
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Supplier</label>
                      <Select
                          showSearch
                          placeholder="Search supplier"
                          onSearch={debounced.suppliers}
                          onChange={handleSupplierSelect}
                          filterOption={false}
                          value={formData.supplierId}
                          style={{ width: '100%' }}
                      >
                        {makeOptions(suppliers)}
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Operation</label>
                      <Select
                          value={formData.operation}
                          onChange={(value) => handleChange('operation', value)}
                          style={{ width: '100%' }}
                          placeholder="Select operation"
                      >
                        <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                        <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">First Weight (Optional)</label>
                      <Input
                          type="number"
                          value={formData.firstWeight}
                          onChange={(e) => handleChange('firstWeight', e.target.value)}
                          placeholder="Auto-filled from capture"
                          className="bg-gray-50"
                      />
                    </div>

                    <div className="col-span-2 flex justify-end gap-2 pt-4">
                      <Button onClick={() => {
                        setFormData({
                          receiptNo: '',
                          expectedWeighings: 2,
                          noPlate: '',
                          driverName: '',
                          vehicleId: null,
                          driverId: null,
                          commodityId: null,
                          commodityName: '',
                          transporterId: null,
                          transporterName: '',
                          supplierId: null,
                          supplierName: '',
                          operation: '',
                          firstWeight: ''
                        });
                      }}>
                        Reset
                      </Button>
                      <Button
                          type="primary"
                          onClick={handleCreateTransaction}
                          loading={loading}
                          className="bg-blue-500"
                      >
                        Create Transaction
                      </Button>
                    </div>
                  </div>
                </Card>
              </div>
            </div>
          </TabPane>

          <TabPane tab="Add Weighings" key="2">
            <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
              <div className="lg:col-span-2">
                <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
              </div>

              <div className="lg:col-span-3">
                <Card>
                  <IncompleteTransactions
                      onAddWeighing={handleAddWeighing}
                      refreshKey={refreshKey}
                  />
                </Card>
              </div>
            </div>
          </TabPane>
        </Tabs>

        <AddWeighingModal
            visible={showWeighingModal}
            transaction={selectedTransaction}
            capturedWeight={capturedWeight}
            onClose={() => setShowWeighingModal(false)}
            onSuccess={handleWeighingSuccess}
        />
      </div>
  );
}