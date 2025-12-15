// src/components/weighing/AddWeighingModal.jsx

import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Modal, Input, Button, message } from "antd";
import { addWeighing as addWeighingThunk } from "../../store/weighingSlice";

export default function AddWeighingModal({
                                             visible,
                                             transaction,
                                             capturedWeight,
                                             onClose,
                                             onSuccess,
                                         }) {
    const dispatch = useDispatch();
    const currentUser = useSelector(state => state.user);  // Get current user from user slice

    const [formData, setFormData] = useState({
        weight: "",
        operatorName: currentUser?.name || "",  // Initialize with current user's name if available
        scaleName: "",
        notes: "",
    });

    const [loading, setLoading] = useState(false);

    // Auto-fill form when modal opens
    useEffect(() => {
        if (visible && transaction) {
            setFormData({
                weight: capturedWeight || "",
                operatorName: transaction.operatorName || "Operator",
                scaleName: transaction.scaleName || "Scale-01",
                notes: "",
            });
        }
    }, [visible, transaction, capturedWeight]);

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = async (e) => {
        e?.preventDefault();

        if (!formData.weight || isNaN(formData.weight) || parseFloat(formData.weight) <= 0) {
            message.error("Please enter a valid positive weight");
            return;
        }

        setLoading(true);

        try {
            const payload = {
                transactionId: transaction.id,
                weight: parseFloat(formData.weight),
                weighBridgeId: transaction.weighBridgeId || null,
                weighBridgeName: transaction.weighBridgeName || "Main Scale",
                scaleName: formData.scaleName?.trim() || transaction.scaleName || "Scale-01",
                operatorId: currentUser?.id,  // Just pass the current user's ID
                operatorName: formData.operatorName?.trim() || transaction.operatorName || "Operator",
                notes: formData.notes?.trim() || "",
            };

            const result = await dispatch(addWeighingThunk(payload)).unwrap();

            // Extract progress info from response
            const completedWeighings =
                result?.data?.completedWeighings ||
                result?.completedWeighings ||
                transaction.completedWeighings + 1;

            const expectedWeighings =
                result?.data?.expectedWeighings ||
                result?.expectedWeighings ||
                transaction.expectedWeighings;

            message.success(
                `Weighing ${completedWeighings} of ${expectedWeighings} added successfully!`
            );

            // Reset only user-editable fields
            setFormData((prev) => ({
                ...prev,
                weight: "",
                notes: "",
            }));

            onSuccess();
            onClose();
        } catch (error) {
            message.error(error?.message || "Failed to add weighing");
            console.error("Add weighing error:", error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            title={`Add Weighing - ${transaction?.receiptNo || "Unknown"}`}
            open={visible}
            onCancel={onClose}
            footer={null}
            width={600}
            destroyOnClose // Clears state on close
        >
            {transaction && (
                <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                    <div className="grid grid-cols-2 gap-3 text-sm">
                        <div><strong>Receipt:</strong> {transaction.receiptNo}</div>
                        <div><strong>Vehicle:</strong> {transaction.noPlate}</div>
                        <div><strong>Driver:</strong> {transaction.driverName}</div>
                        <div><strong>Commodity:</strong> {transaction.commodityName || "—"}</div>
                        <div><strong>Weighbridge:</strong> {transaction.weighBridgeName || "Main Scale"}</div>
                        <div>
                            <strong>Progress:</strong>{" "}
                            <span className="font-semibold text-blue-600">
                {transaction.completedWeighings || 0} → {transaction.completedWeighings + 1 || 1}
              </span>{" "}
                            / {transaction.expectedWeighings || 2}
                        </div>
                    </div>
                </div>
            )}

            <div className="space-y-5">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Weight (kg) <span className="text-red-500">*</span>
                        {capturedWeight && <span className="text-green-600 ml-2">(Auto-captured)</span>}
                    </label>
                    <Input
                        type="number"
                        value={formData.weight}
                        onChange={(e) => handleChange("weight", e.target.value)}
                        placeholder="Enter weight or use captured value"
                        size="large"
                        className="font-mono text-lg"
                        addonAfter="kg"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Operator Name
                    </label>
                    <Input
                        value={formData.operatorName}
                        onChange={(e) => handleChange("operatorName", e.target.value)}
                        placeholder="e.g. John Doe"
                    />
                    <p className="text-xs text-gray-500 mt-1">
                        Default: {transaction?.operatorName || "Operator"}
                    </p>
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Scale Name
                    </label>
                    <Input
                        value={formData.scaleName}
                        onChange={(e) => handleChange("scaleName", e.target.value)}
                        placeholder="e.g. Platform A"
                    />
                    <p className="text-xs text-gray-500 mt-1">
                        Using: {transaction?.scaleName || "Scale-01"}
                    </p>
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Notes (Optional)
                    </label>
                    <Input.TextArea
                        rows={3}
                        value={formData.notes}
                        onChange={(e) => handleChange("notes", e.target.value)}
                        placeholder="Any remarks about this weighing..."
                    />
                </div>

                <div className="flex justify-end gap-3 pt-4">
                    <Button onClick={onClose} size="large">
                        Cancel
                    </Button>
                    <Button
                        type="primary"
                        size="large"
                        onClick={handleSubmit}
                        loading={loading}
                        disabled={!formData.weight || isNaN(formData.weight)}
                        className="bg-blue-600"
                    >
                        Add Weighing
                    </Button>
                </div>
            </div>
        </Modal>
    );
}