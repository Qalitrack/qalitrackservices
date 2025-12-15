// src/components/weighing/AddWeighingModal.jsx

import React, { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
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
    const [formData, setFormData] = useState({
        weight: "",
        operatorName: "",
        scaleName: "",
        notes: "",
    });
    const [loading, setLoading] = useState(false);

    // Auto-fill weight when modal opens and a weight was just captured
    useEffect(() => {
        if (visible && capturedWeight !== null && capturedWeight !== undefined) {
            setFormData((prev) => ({ ...prev, weight: capturedWeight }));
        }
    }, [visible, capturedWeight]);

    const handleChange = (field, value) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = async (e) => {
        e?.preventDefault();

        if (!formData.weight || isNaN(formData.weight)) {
            message.error("Please enter a valid weight");
            return;
        }

        setLoading(true);
        try {
            const payload = {
                transactionId: transaction.id,
                weight: parseFloat(formData.weight),
                weighBridgeId: "00000000-0000-0000-0000-000000000001", // Hardcoded for testing
                weighBridgeName: "Main Scale",
                scaleName: formData.scaleName || "Scale-01",
                operatorId: 1,
                operatorName: formData.operatorName || "Operator",
                notes: formData.notes || "",
            };

            const result = await dispatch(addWeighingThunk(payload)).unwrap();

            const completedWeighings =
                result.data?.completedWeighings || result.completedWeighings;
            const expectedWeighings =
                result.data?.expectedWeighings || result.expectedWeighings;

            message.success(
                `Weighing ${completedWeighings} of ${expectedWeighings} added successfully!`
            );

            // Reset form
            setFormData({ weight: "", operatorName: "", scaleName: "", notes: "" });
            onSuccess();
            onClose();
        } catch (error) {
            message.error(error?.message || "Failed to add weighing");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            title={`Add Weighing - ${transaction?.receiptNo || ""}`}
            open={visible}
            onCancel={onClose}
            footer={null}
            width={600}
            destroyOnClose
        >
            {transaction && (
                <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                    <div className="grid grid-cols-2 gap-3 text-sm">
                        <div><strong>Vehicle:</strong> {transaction.noPlate}</div>
                        <div><strong>Driver:</strong> {transaction.driverName}</div>
                        <div><strong>Commodity:</strong> {transaction.commodityName}</div>
                        <div>
                            <strong>Progress:</strong>{" "}
                            {transaction.completedWeighings || 0} /{" "}
                            {transaction.expectedWeighings || 2}
                        </div>
                    </div>
                </div>
            )}

            <div className="space-y-5">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Weight (kg) *
                    </label>
                    <Input
                        type="number"
                        value={formData.weight}
                        onChange={(e) => handleChange("weight", e.target.value)}
                        placeholder="Enter or capture weight"
                        size="large"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Operator Name
                    </label>
                    <Input
                        value={formData.operatorName}
                        onChange={(e) => handleChange("operatorName", e.target.value)}
                        placeholder="Operator name"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Scale Name
                    </label>
                    <Input
                        value={formData.scaleName}
                        onChange={(e) => handleChange("scaleName", e.target.value)}
                        placeholder="e.g. Scale-01"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Notes (Optional)
                    </label>
                    <Input.TextArea
                        rows={3}
                        value={formData.notes}
                        onChange={(e) => handleChange("notes", e.target.value)}
                        placeholder="Any additional notes..."
                    />
                </div>

                <div className="flex justify-end gap-3 pt-4">
                    <Button onClick={onClose}>Cancel</Button>
                    <Button
                        type="primary"
                        onClick={handleSubmit}
                        loading={loading}
                        disabled={!formData.weight}
                    >
                        Add Weighing
                    </Button>
                </div>
            </div>
        </Modal>
    );
}