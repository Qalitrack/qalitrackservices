import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Modal, Input, Button, message, Tag, Typography, Divider, Tooltip } from "antd";
import { SyncOutlined, MonitorOutlined, InfoCircleOutlined } from "@ant-design/icons";
import { updateTransactionApi } from "../../store/weighingSlice";

const { Text } = Typography;

const FieldLabel = ({ children }) => (
  <label className="text-[10px] font-bold text-gray-400 uppercase tracking-widest block mb-1">
    {children}
  </label>
);

export default function ReweighFirstWeightModal({
  visible,
  transaction,
  capturedWeight,
  onClose,
  onSuccess, // called with updated transaction after first weight saved
}) {
  const dispatch = useDispatch();
  const currentUser = useSelector((state) => state.auth?.user);

  const [weight, setWeight] = useState("");
  const [operatorName, setOperatorName] = useState("");
  const [scaleName, setScaleName] = useState("");
  const [notes, setNotes] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (visible && transaction) {
      setWeight(capturedWeight || "");
      setOperatorName(
        currentUser?.fullName || currentUser?.name || transaction.operatorName || ""
      );
      setScaleName(transaction.scaleName || "Scale-01");
      setNotes("");
    }
  }, [visible, transaction, capturedWeight, currentUser]);

  const isValid = weight && parseFloat(weight) > 0;

  const handleSubmit = async () => {
    if (!isValid) return message.error("Enter a valid first weight");
    const ticketId = transaction.ticketID || transaction.id;
    if (!ticketId) return message.error("Transaction ID missing");

    setLoading(true);
    try {
      await dispatch(
        updateTransactionApi({
          ticketId,
          data: {
            firstWeight: parseFloat(weight),
            secondWeight: null,
            netWeight: null,
            scaleName: scaleName || transaction.scaleName,
            operatorName: operatorName || transaction.operatorName,
            notes: notes || transaction.notes,
          },
        })
      ).unwrap();

      message.success("First weight saved — proceeding to second weight");
      onSuccess({
        ...transaction,
        firstWeight: String(parseFloat(weight)),
        secondWeight: null,
        netWeight: null,
        status: "Active",
        isCompleted: false,
      });
    } catch (err) {
      message.error(err?.message || "Failed to save first weight");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <MonitorOutlined className="text-orange-500" />
          <span className="text-sm font-black uppercase tracking-tight">
            Re-enter First Weight
          </span>
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
          {/* Transaction Summary */}
          <div className="p-4 bg-slate-900 rounded-xl text-white shadow-inner">
            <div className="flex justify-between items-start mb-3">
              <div>
                <Tag color="gold" className="m-0 font-bold border-none uppercase text-[10px]">
                  {transaction.receiptNo || "—"}
                </Tag>
                <div className="text-2xl font-black tracking-tighter mt-1">
                  {transaction.noPlate || "—"}
                </div>
                <Text className="text-[10px] text-gray-400 block mt-0.5">
                  {transaction.driverName || "—"} · {transaction.commodityName || "—"}
                </Text>
              </div>
              <Tag
                color={transaction.operation?.includes("Inbound") ? "green" : "orange"}
                className="mr-0 border-none font-bold text-[10px]"
              >
                {transaction.operation?.includes("Inbound") ? "INBOUND" : "OUTBOUND"}
              </Tag>
            </div>

            <div className="grid grid-cols-2 gap-2 bg-white/10 p-3 rounded-lg text-center">
              <div>
                <div className="text-[9px] text-gray-400 font-bold">Supplier</div>
                <div className="text-[11px] font-semibold truncate">
                  {transaction.supplierName || "—"}
                </div>
              </div>
              <div className="border-l border-white/10">
                <div className="text-[9px] text-gray-400 font-bold">Transporter</div>
                <div className="text-[11px] font-semibold truncate">
                  {transaction.transporterName || "—"}
                </div>
              </div>
            </div>
          </div>

          {/* Info banner */}
          <div className="flex items-start gap-2 p-3 bg-orange-50 rounded-lg border border-orange-100">
            <InfoCircleOutlined className="text-orange-500 mt-0.5 flex-shrink-0" />
            <Text className="text-[11px] text-orange-700 leading-relaxed">
              All transaction details are pre-filled. Enter the new first weight reading from
              the scale, then proceed to capture the second weight.
            </Text>
          </div>

          {/* Weight input */}
          <div>
            <FieldLabel>First Weight Reading *</FieldLabel>
            <div className="flex gap-2">
              <Input
                type="number"
                value={weight}
                onChange={(e) => setWeight(e.target.value)}
                placeholder="0.00"
                size="large"
                autoFocus
                className="font-mono text-2xl font-black flex-1 border-orange-200 focus:border-orange-400"
                suffix={<span className="text-gray-400 text-sm">KG</span>}
              />
              {capturedWeight && (
                <Tooltip title="Pull from Scale">
                  <Button
                    icon={<SyncOutlined />}
                    onClick={() => setWeight(capturedWeight)}
                    size="large"
                    className="h-auto px-4 border-orange-300 text-orange-600"
                  />
                </Tooltip>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <FieldLabel>Operator</FieldLabel>
              <Input
                size="middle"
                value={operatorName}
                onChange={(e) => setOperatorName(e.target.value)}
              />
            </div>
            <div>
              <FieldLabel>Scale ID</FieldLabel>
              <Input
                size="middle"
                value={scaleName}
                onChange={(e) => setScaleName(e.target.value)}
              />
            </div>
          </div>

          <div>
            <FieldLabel>Remarks</FieldLabel>
            <Input.TextArea
              rows={2}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Optional notes..."
            />
          </div>

          <Divider className="my-2" />

          <div className="flex gap-3">
            <Button
              onClick={onClose}
              block
              size="large"
              className="rounded-lg font-bold text-gray-400"
            >
              Cancel
            </Button>
            <Button
              type="primary"
              size="large"
              block
              loading={loading}
              disabled={!isValid}
              onClick={handleSubmit}
              className="rounded-lg font-bold shadow-lg border-none bg-orange-500 hover:bg-orange-600"
            >
              Save W1 → Enter W2
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}
