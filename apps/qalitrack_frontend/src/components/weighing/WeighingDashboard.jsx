// src/components/weighing/WeighingDashboard.jsx

import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Tabs, message, Card } from "antd";
import LiveWeighbridgeStatus from "./LiveWeighbridgeStatus";
import CreateTransactionForm from "./CreateTransactionForm";
import IncompleteTransactionsTable from "./IncompleteTransactionsTable";
import AddWeighingModal from "./AddWeighingModal";
import {
    fetchVehicles,
    fetchDrivers,
    fetchProducts,
    fetchRoutes,
    fetchSaccosByName,
    fetchSuppliersByName,
    fetchTransportersByName,
    fetchWeighbridges,        // ← NEW: Load all weighbridges
    fetchTransactions,
    fetchSimulatedWeight,
} from "../../store/weighingSlice";

const { TabPane } = Tabs;

const initialFormData = {
    receiptNo: "",
    expectedWeighings: 2,
    noPlate: "",
    driverName: "",
    vehicleId: null,
    driverId: null,
    commodityId: null,
    commodityName: "",
    transporterId: null,
    transporterName: "",
    supplierId: null,
    supplierName: "",
    customerId: null,
    customerName: "",
    originId: null,
    originName: "",
    destinationId: null,
    destinationName: "",
    operation: "",
    weighMode: "Gross/Tare",
    firstWeight: "",
    scaleName: "Scale-01",
    operatorName: "Operator",
    weighBridgeId: null,      // ← NEW
    weighBridgeName: "",      // ← NEW
};

export default function WeighingDashboard() {
    const dispatch = useDispatch();
    const { error } = useSelector((state) => state.weighing);

    const [capturedWeight, setCapturedWeight] = useState(null);
    const [formData, setFormData] = useState(initialFormData);
    const [selectedTransaction, setSelectedTransaction] = useState(null);
    const [showWeighingModal, setShowWeighingModal] = useState(false);
    const [refreshKey, setRefreshKey] = useState(0);

    // Load all master data + weighbridges on mount
    useEffect(() => {
        dispatch(fetchVehicles());
        dispatch(fetchDrivers());
        dispatch(fetchProducts());
        dispatch(fetchRoutes());
        dispatch(fetchSaccosByName(""));
        dispatch(fetchSuppliersByName(""));
        dispatch(fetchTransportersByName(""));
        dispatch(fetchWeighbridges({ pageSize: 100 })); // ← Load all weighbridges
        dispatch(fetchTransactions({ pageNumber: 1, pageSize: 50 }));
        dispatch(fetchSimulatedWeight());
    }, [dispatch]);

    // Global error handling
    useEffect(() => {
        if (error) {
            message.error(error);
        }
    }, [error]);

    // Auto-fill captured weight into form
    useEffect(() => {
        if (capturedWeight !== null) {
            setFormData((prev) => ({
                ...prev,
                firstWeight: capturedWeight,
            }));
        }
    }, [capturedWeight]);

    const handleManualCapture = (weight) => {
        if (isNaN(weight) || weight === null || weight < 0) {
            message.error("Captured weight is invalid.");
            return;
        }
        setCapturedWeight(weight);
        message.success(`Captured weight: ${weight} kg`);
    };

    const handleTransactionCreated = () => {
        setRefreshKey((k) => k + 1);
        setCapturedWeight(null);
        // Reset form to initial state after successful creation
        setFormData(initialFormData);
    };

    const handleAddWeighing = (transaction) => {
        setSelectedTransaction(transaction);
        setShowWeighingModal(true);
    };

    const handleWeighingSuccess = () => {
        setRefreshKey((k) => k + 1);
        setCapturedWeight(null);
    };

    const handleModalClose = () => {
        setShowWeighingModal(false);
        setSelectedTransaction(null);
        setCapturedWeight(null);
    };

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <Tabs defaultActiveKey="1" tabBarGutter={30} className="weighing-tabs">
                <TabPane tab="Create New Transaction" key="1">
                    <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
                        <div className="lg:col-span-2">
                            <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
                        </div>
                        <div className="lg:col-span-3">
                            <CreateTransactionForm
                                formData={formData}
                                setFormData={setFormData}
                                capturedWeight={capturedWeight}
                                onTransactionCreated={handleTransactionCreated}
                            />
                        </div>
                    </div>
                </TabPane>

                <TabPane tab="Continue Incomplete Weighings" key="2">
                    <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
                        <div className="lg:col-span-2">
                            <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
                        </div>
                        <div className="lg:col-span-3">
                            <Card title="Incomplete Transactions" className="h-full">
                                <IncompleteTransactionsTable
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
                onClose={handleModalClose}
                onSuccess={handleWeighingSuccess}
            />
        </div>
    );
}