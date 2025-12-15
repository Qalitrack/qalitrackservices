import React, { useEffect, useState } from "react";
import { Form, Input, Select, Button, message, Card, Tabs, Table, Tag, Modal } from "antd";
import { useDispatch, useSelector } from "react-redux";
import { createTransaction } from "../store/weighingSlice";

const { Option } = Select;
const { TabPane } = Tabs;

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
      <div className="bg-black text-amber-600 rounded-2xl shadow-xl p-6">
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
      </div>
  );
}

/* ---------------------------------------------------------------------------
   Incomplete Transactions Table
----------------------------------------------------------------------------*/
function IncompleteTransactions({ onAddWeighing, onRefresh }) {
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);

  const fetchIncomplete = async () => {
    setLoading(true);
    try {
      const response = await fetch('/api/Transaction?IsCompleted=false&PageSize=50');
      const result = await response.json();

      if (result.success && result.data) {
        setTransactions(result.data.items || []);
      }
    } catch (error) {
      message.error('Failed to fetch incomplete transactions');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchIncomplete();
  }, [onRefresh]);

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
          {record.completedWeighings} / {record.expectedWeighings}
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
          <Button onClick={fetchIncomplete}>Refresh</Button>
        </div>
        <Table
            columns={columns}
            dataSource={transactions}
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
        weighBridgeId: 1,
        weighBridgeName: "Main Scale",
        scaleName: formData.scaleName || "Scale-01",
        operatorId: 1,
        operatorName: formData.operatorName || "Operator",
        notes: formData.notes || "",
      };

      const response = await fetch('/api/Transaction/add-weighing', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      const result = await response.json();

      if (result.success) {
        message.success(`Weighing ${result.data.completedWeighings} of ${result.data.expectedWeighings} added!`);
        setFormData({ weight: '', operatorName: '', scaleName: '', notes: '' });
        onSuccess();
        onClose();
      } else {
        message.error(result.message || 'Failed to add weighing');
      }
    } catch (error) {
      message.error('Error adding weighing');
      console.error(error);
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
                <div><strong>Progress:</strong> {transaction.completedWeighings} / {transaction.expectedWeighings}</div>
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
  const { loading, error } = useSelector((state) => state.weighing);
  
  const [capturedWeight, setCapturedWeight] = useState(null);
  const [formData, setFormData] = useState({
    receiptNo: '',
    expectedWeighings: '2',
    noPlate: '',
    driverName: '',
    commodityName: '',
    transporterId: '',
    transporterName: '',
    operation: '',
    firstWeight: ''
  });
  
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [showWeighingModal, setShowWeighingModal] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  
  // Show error message if there's an error
  useEffect(() => {
    if (error) {
      message.error(error);
    }
  }, [error]);

  useEffect(() => {
    if (capturedWeight !== null) {
      setFormData(prev => ({ ...prev, firstWeight: capturedWeight }));
    }
  }, [capturedWeight]);

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

  const handleCreateTransaction = async (e) => {
    e.preventDefault();

    // Validation
    if (!formData.receiptNo || !formData.noPlate || !formData.driverName || !formData.transporterId) {
      message.error('Please fill in all required fields');
      return;
    }

    const payload = {
      receiptNo: formData.receiptNo,
      expectedWeighings: parseInt(formData.expectedWeighings),
      noPlate: formData.noPlate,
      driverName: formData.driverName,
      transporterId: parseInt(formData.transporterId),
      transporterName: formData.transporterName,

      firstWeight: formData.firstWeight ? parseFloat(formData.firstWeight) : null,
      weighBridgeId: formData.firstWeight ? 1 : null,
      weighBridgeName: formData.firstWeight ? "Main Scale" : "",
      scaleName: formData.firstWeight ? "Scale-01" : "",
      operatorId: formData.firstWeight ? 1 : null,
      operatorName: formData.firstWeight ? "Operator" : "",

      commodityName: formData.commodityName,
      weighMode: "Gross/Tare",
      operation: formData.operation,
    };

    try {
      const resultAction = await dispatch(createTransaction(payload));
      
      if (createTransaction.fulfilled.match(resultAction)) {
        message.success('Transaction created successfully!');
        setFormData({
          receiptNo: '',
          expectedWeighings: '2',
          noPlate: '',
          driverName: '',
          commodityName: '',
          transporterId: '',
          transporterName: '',
          operation: '',
          firstWeight: ''
        });
        setCapturedWeight(null);
        setRefreshKey(prev => prev + 1);
      } else if (resultAction.error) {
        throw new Error(resultAction.error.message || 'Failed to create transaction');
      }
    } catch (error) {
      message.error(error.message || 'Error creating transaction');
    }
  };

  const handleAddWeighing = (transaction) => {
    setSelectedTransaction(transaction);
    setShowWeighingModal(true);
  };

  const handleWeighingSuccess = () => {
    setRefreshKey(prev => prev + 1);
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
                        <Option value="2">2 Weighings (Standard)</Option>
                        <Option value="3">3 Weighings</Option>
                        <Option value="4">4 Weighings</Option>
                      </Select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Vehicle Plate *</label>
                      <Input
                          value={formData.noPlate}
                          onChange={(e) => handleChange('noPlate', e.target.value)}
                          placeholder="KAA 123A"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Driver Name *</label>
                      <Input
                          value={formData.driverName}
                          onChange={(e) => handleChange('driverName', e.target.value)}
                          placeholder="John Doe"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Commodity Name</label>
                      <Input
                          value={formData.commodityName}
                          onChange={(e) => handleChange('commodityName', e.target.value)}
                          placeholder="Sugar"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Transporter ID *</label>
                      <Input
                          type="number"
                          value={formData.transporterId}
                          onChange={(e) => handleChange('transporterId', e.target.value)}
                          placeholder="10"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium mb-1">Transporter Name</label>
                      <Input
                          value={formData.transporterName}
                          onChange={(e) => handleChange('transporterName', e.target.value)}
                          placeholder="ABC Transport Ltd"
                      />
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
                      <Button onClick={() => setFormData({
                        receiptNo: '',
                        expectedWeighings: '2',
                        noPlate: '',
                        driverName: '',
                        commodityName: '',
                        transporterId: '',
                        transporterName: '',
                        operation: '',
                        firstWeight: ''
                      })}>
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
                      onRefresh={refreshKey}
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