import React, { useEffect, useState, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Modal, Input, Button, message, Tag, Typography, Tooltip, Divider } from "antd";
import { SyncOutlined, MonitorOutlined, ArrowRightOutlined, InfoCircleOutlined } from "@ant-design/icons";
import { addWeighing as addWeighingThunk } from "../../store/weighingSlice";

const { Text } = Typography;

export default function AddWeighingModal({
  visible,
  transaction,
  capturedWeight,
  onClose,
  onSuccess,
}) {
  const dispatch = useDispatch();
  const currentUser = useSelector((state) => state.auth?.user);

  const [formData, setFormData] = useState({
    weight: "",
    operatorName: "",
    scaleName: "",
    notes: "",
  });

  const [loading, setLoading] = useState(false);

  // Initialize form when modal opens
  useEffect(() => {
    if (visible && transaction) {
      setFormData({
        weight: capturedWeight || "",
        operatorName: currentUser?.fullName || currentUser?.name || transaction.operatorName || "Operator",
        scaleName: transaction.scaleName || "Scale-01",
        notes: "",
      });
    }
  }, [visible, transaction, capturedWeight, currentUser]);

  // Logic: Calculate Net Weight and identify if weights are valid for the operation
  const { netWeight, isValid, errorMsg } = useMemo(() => {
    const w1 = parseFloat(transaction?.firstWeight || 0);
    const w2 = parseFloat(formData.weight || 0);
    const operation = transaction?.operation || "";
    
    let net = 0;
    let valid = true;
    let error = "";

    if (isNaN(w2) || w2 <= 0) {
      return { netWeight: 0, isValid: false, errorMsg: "Enter a valid weight" };
    }

    if (operation.includes("Inbound")) {
      // Inbound: Must leave lighter than you arrived (W1 > W2)
      net = w1 - w2;
      if (w2 >= w1) {
        valid = false;
        error = "Inbound Error: W2 must be LESS than W1 (Truck must be empty/lighter)";
      }
    } else {
      // Outbound: Must leave heavier than you arrived (W2 > W1)
      net = w2 - w1;
      if (w2 <= w1) {
        valid = false;
        error = "Outbound Error: W2 must be GREATER than W1 (Truck must be loaded)";
      }
    }

    return { netWeight: net, isValid: valid, errorMsg: error };
  }, [transaction, formData.weight]);

  const handleChange = (field, value) => setFormData((prev) => ({ ...prev, [field]: value }));

  const handleSyncWeight = () => {
    if (capturedWeight) {
      handleChange("weight", capturedWeight);
      message.info("Weight synchronized");
    }
  };

  const handleSubmit = async () => {
    if (!isValid) return message.error(errorMsg);

    setLoading(true);
    try {
      const payload = {
        transactionId: transaction.id,
        weight: parseFloat(formData.weight),
        weighBridgeId: transaction.weighBridgeId,
        weighBridgeName: transaction.weighBridgeName,
        scaleName: formData.scaleName || "Scale-01",
        operatorId: currentUser?.id,
        operatorName: formData.operatorName,
        notes: formData.notes,
        netWeight: netWeight,
      };

      await dispatch(addWeighingThunk(payload)).unwrap();
      message.success(`Transaction Finalized! Net: ${netWeight} KG`);
      onSuccess();
      onClose();
    } catch (error) {
      message.error(error?.message || "Failed to save weight");
    } finally {
      setLoading(false);
    }
  };

  const FieldLabel = ({ children }) => (
    <label className="text-[10px] font-bold text-gray-400 uppercase tracking-widest block mb-1">
      {children}
    </label>
  );

  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <MonitorOutlined className="text-blue-600" />
          <span className="text-sm font-black uppercase tracking-tight">Finalize Transaction</span>
        </div>
      }
      open={visible}
      onCancel={onClose}
      footer={null}
      width={480}
      centered
      destroyOnClose
    >
      {transaction && (
        <div className="space-y-4">
          {/* Transaction Summary Header */}
          <div className="p-4 bg-slate-900 rounded-xl text-white shadow-inner">
            <div className="flex justify-between items-start mb-4">
              <div>
                <Tag color="gold" className="m-0 font-bold border-none uppercase text-[10px]">
                  {transaction.receiptNo}
                </Tag>
                <div className="text-2xl font-black tracking-tighter mt-1">{transaction.noPlate}</div>
              </div>
              <div className="text-right">
                <FieldLabel>Operation</FieldLabel>
                <Tag color={transaction.operation.includes('Inbound') ? 'green' : 'blue'} className="mr-0 border-none font-bold">
                  {transaction.operation.includes('Inbound') ? 'INBOUND' : 'OUTBOUND'}
                </Tag>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-2 bg-white/10 p-3 rounded-lg backdrop-blur-sm">
              <div className="text-center">
                <div className="text-[9px] text-gray-400 font-bold">W1 (KG)</div>
                <div className="font-mono text-sm font-bold">{transaction.firstWeight?.toLocaleString()}</div>
              </div>
              <div className="flex items-center justify-center">
                <ArrowRightOutlined className="text-gray-500" />
              </div>
              <div className="text-center border-l border-white/10">
                <div className="text-[9px] text-gray-400 font-bold">W2 (KG)</div>
                <div className="font-mono text-sm font-bold text-blue-400">
                   {formData.weight ? parseFloat(formData.weight).toLocaleString() : '---'}
                </div>
              </div>
            </div>

            {/* Net Weight Preview - Only visible when weight is entered */}
            {formData.weight && (
                <div className={`mt-3 p-2 rounded flex justify-between items-center ${isValid ? 'bg-green-500/20' : 'bg-red-500/20'}`}>
                    <Text className="text-[10px] text-white font-bold uppercase">Net Payload</Text>
                    <Text className={`text-xl font-black font-mono ${isValid ? 'text-green-400' : 'text-red-400'}`}>
                        {netWeight.toLocaleString()} <small className="text-[10px] font-normal">KG</small>
                    </Text>
                </div>
            )}
          </div>

          {/* Input Section */}
          <div className="space-y-4 px-1">
            <div>
              <FieldLabel>Current Reading (Second Weight)</FieldLabel>
              <div className="flex gap-2">
                <Input
                  type="number"
                  value={formData.weight}
                  onChange={(e) => handleChange("weight", e.target.value)}
                  placeholder="0.00"
                  size="large"
                  autoFocus
                  className={`font-mono text-2xl font-black flex-1 ${!isValid && formData.weight ? 'border-red-500 bg-red-50' : 'border-blue-200'}`}
                  suffix={<span className="text-gray-400 text-sm">KG</span>}
                />
                <Tooltip title="Pull from Scale">
                  <Button 
                    icon={<SyncOutlined />} 
                    onClick={handleSyncWeight}
                    size="large"
                    className="h-auto px-4 border-blue-200 text-blue-600"
                  />
                </Tooltip>
              </div>
              {!isValid && formData.weight && (
                <div className="mt-1 flex items-center gap-1 text-red-500">
                    <InfoCircleOutlined className="text-[10px]" />
                    <span className="text-[10px] font-bold">{errorMsg}</span>
                </div>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <FieldLabel>Operator</FieldLabel>
                <Input size="middle" value={formData.operatorName} onChange={e => handleChange("operatorName", e.target.value)} />
              </div>
              <div>
                <FieldLabel>Scale ID</FieldLabel>
                <Input size="middle" value={formData.scaleName} onChange={e => handleChange("scaleName", e.target.value)} />
              </div>
            </div>

            <div>
              <FieldLabel>Remarks</FieldLabel>
              <Input.TextArea rows={2} value={formData.notes} onChange={e => handleChange("notes", e.target.value)} placeholder="Optional notes..." />
            </div>
          </div>

          <Divider className="my-2" />

          <div className="flex gap-3">
            <Button onClick={onClose} block size="large" className="rounded-lg font-bold text-gray-400">
              CANCEL
            </Button>
            <Button
              type="primary"
              size="large"
              block
              onClick={handleSubmit}
              loading={loading}
              disabled={!isValid}
              className={`rounded-lg font-bold shadow-lg border-none ${isValid ? 'bg-blue-600' : 'bg-gray-300'}`}
            >
              FINALIZE & PRINT
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}