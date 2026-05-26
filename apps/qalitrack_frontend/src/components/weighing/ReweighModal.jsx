import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Modal, Input, Button, message, Tag, Typography, Divider, Radio } from "antd";
import {
  RetweetOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UserOutlined,
  ExclamationCircleOutlined,
  NumberOutlined,
} from "@ant-design/icons";
import {
  requestReweighThunk,
  approveReweighThunk,
  rejectReweighThunk,
} from "../../store/weighingSlice";

const { Text } = Typography;

const FieldLabel = ({ children }) => (
  <label className="text-[10px] font-bold text-gray-400 uppercase tracking-widest block mb-1">
    {children}
  </label>
);

// Step dots shown at top of modal
function StepDots({ stepIndex }) {
  const labels = ["Request", "Decision", "Result"];
  return (
    <div className="flex items-center gap-2 mb-4">
      {labels.map((label, i) => (
        <React.Fragment key={label}>
          <div className="flex items-center gap-1.5">
            <div
              className={`w-5 h-5 rounded-full flex items-center justify-center text-[9px] font-bold border transition-all ${
                i < stepIndex
                  ? "bg-amber-500 border-amber-500 text-white"
                  : i === stepIndex
                  ? "bg-amber-500 border-amber-500 text-white shadow-md ring-2 ring-amber-200"
                  : "bg-white border-gray-200 text-gray-400"
              }`}
            >
              {i + 1}
            </div>
            <span
              className={`text-[9px] font-bold uppercase tracking-wide ${
                i === stepIndex ? "text-amber-700" : "text-gray-400"
              }`}
            >
              {label}
            </span>
          </div>
          {i < labels.length - 1 && (
            <div
              className={`flex-1 h-0.5 rounded ${i < stepIndex ? "bg-amber-400" : "bg-gray-200"}`}
            />
          )}
        </React.Fragment>
      ))}
    </div>
  );
}

export default function ReweighModal({ visible, transaction, onClose, onSuccess, onApproved }) {
  const dispatch = useDispatch();
  const currentUser = useSelector((state) => state.auth?.user);

  // step: 'request' | 'decision' | 'rejected'
  const [step, setStep] = useState("request");
  const [loading, setLoading] = useState(false);

  const [reason, setReason] = useState("");
  const [approvedBy, setApprovedBy] = useState("");
  const [approveNotes, setApproveNotes] = useState("");
  const [reweighType, setReweighType] = useState("secondWeight");
  const [rejectionReason, setRejectionReason] = useState("");
  const [rejectNotes, setRejectNotes] = useState("");

  // Local copy — updated as the wizard progresses
  const [currentTx, setCurrentTx] = useState(null);

  const ticketId = currentTx?.ticketID || currentTx?.id;

  useEffect(() => {
    if (visible && transaction) {
      setCurrentTx(transaction);
      const s = transaction.status?.toLowerCase();
      setStep(s === "reweighrequested" ? "decision" : "request");
      setReason("");
      setApprovedBy(currentUser?.fullName || currentUser?.name || "");
      setApproveNotes("");
      setReweighType("secondWeight");
      setRejectionReason("");
      setRejectNotes("");
    }
  }, [visible, transaction, currentUser]);

  // ── Step 1: submit reweigh request ──────────────────────────────────────────
  const handleRequest = async () => {
    if (!reason.trim()) return message.error("A reason is required");
    setLoading(true);
    try {
      await dispatch(
        requestReweighThunk({ ticketID: ticketId, reason: reason.trim() })
      ).unwrap();
      setCurrentTx((prev) => ({ ...prev, status: "ReweighRequested" }));
      setStep("decision");
    } catch (err) {
      message.error(err?.message || "Failed to request reweigh");
    } finally {
      setLoading(false);
    }
  };

  // ── Step 2a: approve ─────────────────────────────────────────────────────────
  const handleApprove = async () => {
    setLoading(true);
    try {
      await dispatch(
        approveReweighThunk({ ticketID: ticketId, approvedBy, notes: approveNotes })
      ).unwrap();
      const approvedTx = {
        ...currentTx,
        status: "Active",
        isCompleted: false,
        secondWeight: null,
        netWeight: null,
        ...(reweighType !== "secondWeight" ? { firstWeight: null } : {}),
      };
      onSuccess?.();
      onApproved?.(approvedTx, reweighType); // parent opens weighing modal
      onClose();
    } catch (err) {
      message.error(err?.message || "Failed to approve reweigh");
    } finally {
      setLoading(false);
    }
  };

  // ── Step 2b: reject ──────────────────────────────────────────────────────────
  const handleReject = async () => {
    if (!rejectionReason.trim()) return message.error("A rejection reason is required");
    setLoading(true);
    try {
      await dispatch(
        rejectReweighThunk({
          ticketID: ticketId,
          rejectionReason: rejectionReason.trim(),
          rejectedBy: approvedBy,
          notes: rejectNotes,
        })
      ).unwrap();
      setStep("rejected");
    } catch (err) {
      message.error(err?.message || "Failed to reject reweigh");
    } finally {
      setLoading(false);
    }
  };

  const stepIndex = step === "request" ? 0 : step === "decision" ? 1 : 2;

  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <RetweetOutlined className="text-amber-600" />
          <span className="text-sm font-black uppercase tracking-tight">Reweigh Transaction</span>
        </div>
      }
      open={visible}
      onCancel={onClose}
      footer={null}
      width={520}
      centered
      destroyOnClose
    >
      {currentTx && (
        <div className="space-y-4">
          {/* ── Transaction Summary Card ─────────────────────────────────────── */}
          <div className="p-4 bg-slate-900 rounded-xl text-white shadow-inner">
            <div className="flex justify-between items-start mb-3">
              <div>
                <Tag color="gold" className="m-0 font-bold border-none uppercase text-[10px]">
                  {currentTx.receiptNo || "—"}
                </Tag>
                <div className="text-2xl font-black tracking-tighter mt-1">
                  {currentTx.noPlate || "—"}
                </div>
                <Text className="text-[10px] text-gray-400 block mt-0.5">
                  {currentTx.driverName || "—"} · {currentTx.commodityName || "—"}
                </Text>
              </div>
              <div className="text-right">
                <div className="text-[9px] text-gray-400 font-bold uppercase tracking-wide mb-1">
                  Status
                </div>
                <Tag
                  color={
                    currentTx.status === "ReweighRequested"
                      ? "blue"
                      : currentTx.status === "Completed" || currentTx.isCompleted
                      ? "green"
                      : "orange"
                  }
                  className="mr-0 border-none font-bold text-[10px] uppercase"
                >
                  {currentTx.status || "Active"}
                </Tag>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-2 bg-white/10 p-3 rounded-lg">
              <div className="text-center">
                <div className="text-[9px] text-gray-400 font-bold">W1 (KG)</div>
                <div className="font-mono text-sm font-bold">
                  {currentTx.firstWeight
                    ? parseFloat(currentTx.firstWeight).toLocaleString()
                    : "—"}
                </div>
              </div>
              <div className="text-center border-l border-r border-white/10">
                <div className="text-[9px] text-gray-400 font-bold">W2 (KG)</div>
                <div className="font-mono text-sm font-bold text-amber-400">
                  {currentTx.secondWeight
                    ? parseFloat(currentTx.secondWeight).toLocaleString()
                    : "—"}
                </div>
              </div>
              <div className="text-center">
                <div className="text-[9px] text-gray-400 font-bold">Net (KG)</div>
                <div className="font-mono text-sm font-bold text-green-400">
                  {currentTx.netWeight
                    ? parseFloat(currentTx.netWeight).toLocaleString()
                    : "—"}
                </div>
              </div>
            </div>
          </div>

          {/* ── Step Dots ────────────────────────────────────────────────────── */}
          <StepDots stepIndex={stepIndex} />

          {/* ══════════════════════════════════════════════════════════════════ */}
          {/* STEP 1 — Request                                                  */}
          {/* ══════════════════════════════════════════════════════════════════ */}
          {step === "request" && (
            <div className="space-y-4">
              <div className="flex items-start gap-2 p-3 bg-blue-50 rounded-lg border border-blue-100">
                <ExclamationCircleOutlined className="text-blue-500 mt-0.5 flex-shrink-0" />
                <Text className="text-[11px] text-blue-700 leading-relaxed">
                  Submitting this request will flag the transaction for manager review.
                  The second weight is <strong>only cleared after approval</strong>.
                </Text>
              </div>

              <div>
                <FieldLabel>Reason for Reweigh *</FieldLabel>
                <Input.TextArea
                  rows={3}
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder="Describe why a reweigh is needed…"
                  className="text-sm"
                  autoFocus
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
                  onClick={handleRequest}
                  icon={<RetweetOutlined />}
                  className="rounded-lg font-bold bg-blue-500 hover:bg-blue-600 border-none shadow-lg"
                >
                  Submit Request
                </Button>
              </div>
            </div>
          )}

          {/* ══════════════════════════════════════════════════════════════════ */}
          {/* STEP 2 — Decision (Approve / Reject)                             */}
          {/* ══════════════════════════════════════════════════════════════════ */}
          {step === "decision" && (
            <div className="space-y-3">
              <div>
                <FieldLabel>Actioned By</FieldLabel>
                <Input
                  value={approvedBy}
                  onChange={(e) => setApprovedBy(e.target.value)}
                  prefix={<UserOutlined className="text-gray-400" />}
                  placeholder="Manager / Supervisor name"
                />
              </div>

              {/* Approve panel */}
              <div className="p-3 bg-green-50 rounded-xl border border-green-100 space-y-3">
                <div className="flex items-center gap-1.5">
                  <CheckCircleOutlined className="text-green-600" />
                  <Text className="text-[11px] font-bold text-green-700 uppercase tracking-wide">
                    Approve
                  </Text>
                </div>

                {/* Reweigh scope selector */}
                <div>
                  <FieldLabel>What needs to be reweighed?</FieldLabel>
                  <Radio.Group
                    value={reweighType}
                    onChange={(e) => setReweighType(e.target.value)}
                    className="w-full"
                  >
                    <div className="grid grid-cols-3 gap-1.5">
                      {[
                        { value: "secondWeight", label: "2nd Weight", desc: "Redo W2 only", color: "text-blue-600" },
                        { value: "firstWeight",  label: "1st Weight", desc: "Redo full process", color: "text-orange-600" },
                        { value: "all",          label: "All Weights", desc: "Restart from W1", color: "text-red-600" },
                      ].map(({ value, label, desc, color }) => (
                        <label
                          key={value}
                          className={`flex flex-col items-center p-2 rounded-lg border cursor-pointer transition-all text-center ${
                            reweighType === value
                              ? "bg-white border-green-400 shadow-sm"
                              : "border-green-100 hover:border-green-300"
                          }`}
                        >
                          <Radio value={value} className="hidden" />
                          <NumberOutlined className={`text-base mb-0.5 ${color}`} />
                          <span className="text-[10px] font-bold text-gray-700 leading-tight">{label}</span>
                          <span className="text-[9px] text-gray-400 leading-tight">{desc}</span>
                        </label>
                      ))}
                    </div>
                  </Radio.Group>
                </div>

                <div>
                  <FieldLabel>Notes (optional)</FieldLabel>
                  <Input.TextArea
                    rows={2}
                    value={approveNotes}
                    onChange={(e) => setApproveNotes(e.target.value)}
                    placeholder="Optional approval notes…"
                  />
                </div>
                <Button
                  size="large"
                  block
                  loading={loading}
                  onClick={handleApprove}
                  icon={<CheckCircleOutlined />}
                  className="rounded-lg font-bold bg-green-500 hover:bg-green-600 border-none text-white shadow-md"
                >
                  {reweighType === "secondWeight"
                    ? "Approve — Enter 2nd Weight"
                    : "Approve — Redo Full Weighing"}
                </Button>
              </div>

              {/* Reject panel */}
              <div className="p-3 bg-red-50 rounded-xl border border-red-100 space-y-3">
                <div className="flex items-center gap-1.5">
                  <CloseCircleOutlined className="text-red-500" />
                  <Text className="text-[11px] font-bold text-red-600 uppercase tracking-wide">
                    Reject
                  </Text>
                  <Text className="text-[10px] text-red-400 ml-auto">
                    Keeps original weights intact
                  </Text>
                </div>
                <div>
                  <FieldLabel>Rejection Reason *</FieldLabel>
                  <Input.TextArea
                    rows={2}
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                    placeholder="State the reason for rejection…"
                  />
                </div>
                <div>
                  <FieldLabel>Notes (optional)</FieldLabel>
                  <Input
                    value={rejectNotes}
                    onChange={(e) => setRejectNotes(e.target.value)}
                    placeholder="Optional notes…"
                  />
                </div>
                <Button
                  danger
                  size="large"
                  block
                  loading={loading}
                  onClick={handleReject}
                  icon={<CloseCircleOutlined />}
                  className="rounded-lg font-bold shadow-md"
                >
                  Reject — Keep Completed
                </Button>
              </div>
            </div>
          )}

          {/* ══════════════════════════════════════════════════════════════════ */}
          {/* STEP 3 — Rejected                                                */}
          {/* ══════════════════════════════════════════════════════════════════ */}
          {step === "rejected" && (
            <div className="space-y-4 text-center">
              <div className="p-6 bg-red-50 rounded-xl border border-red-100">
                <CloseCircleOutlined className="text-red-500 text-4xl mb-3 block" />
                <Text className="text-base font-bold text-red-700 block mb-1">
                  Reweigh Rejected
                </Text>
                <Text className="text-[11px] text-red-600 block mb-2">
                  The request was rejected. The transaction has been restored to{" "}
                  <strong>Completed</strong> with the original weights intact.
                </Text>
                {rejectionReason && (
                  <div className="mt-3 p-2 bg-white rounded-lg border border-red-200 text-left">
                    <Text className="text-[9px] font-bold text-gray-400 uppercase tracking-wide block">
                      Reason
                    </Text>
                    <Text className="text-[11px] text-gray-700">{rejectionReason}</Text>
                  </div>
                )}
              </div>
              <Button
                size="large"
                block
                onClick={() => {
                  onSuccess?.();
                  onClose();
                }}
                className="rounded-lg font-bold"
              >
                Close — Back to Transactions
              </Button>
            </div>
          )}
        </div>
      )}
    </Modal>
  );
}
