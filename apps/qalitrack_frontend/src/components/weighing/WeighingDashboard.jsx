import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Card, message } from "antd";
import CreateTransactionForm from "./CreateTransactionForm";
import IncompleteTransactionsTable from "./IncompleteTransactionsTable";
import LiveWeighbridgeStatus from "./LiveWeighbridgeStatus";
import CameraGrid from "../CameraGrid";

import {
  fetchVehicles, fetchDrivers, fetchProducts, fetchRoutes,
  fetchSaccosByName, fetchSuppliersByName, fetchTransportersByName,
  fetchWeighbridges, fetchTransactions, fetchSimulatedWeight,
} from "../../store/weighingSlice";

const INITIAL_FORM_DATA = {
  receiptNo: "", expectedWeighings: 2, noPlate: "", driverName: "",
  vehicleId: null, driverId: null, commodityId: null, commodityName: "",
  transporterId: null, transporterName: "", supplierId: null, supplierName: "",
  customerId: null, customerName: "", originId: null, originName: "",
  destinationId: null, destinationName: "", operation: "Inbound Product Receipt", weighMode: "Gross/Tare",
  firstWeight: "", secondWeight: "", scaleName: "", operatorName: "", weighBridgeId: null, weighBridgeName: "",
};

export default function WeighingDashboard() {
  const dispatch = useDispatch();
  const { error, weighbridges = [] } = useSelector((state) => state.weighing);
  const [capturedWeight, setCapturedWeight] = useState(null);
  const [formData, setFormData] = useState(INITIAL_FORM_DATA);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    dispatch(fetchVehicles());
    dispatch(fetchDrivers());
    dispatch(fetchProducts());
    dispatch(fetchRoutes());
    dispatch(fetchSaccosByName(""));
    dispatch(fetchSuppliersByName(""));
    dispatch(fetchTransportersByName(""));
    dispatch(fetchWeighbridges({ pageSize: 100 }));
    dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
    dispatch(fetchSimulatedWeight());
  }, [dispatch]);

  useEffect(() => { if (error) message.error(error); }, [error]);

  const handleManualCapture = (weight) => {
    if (!weight || weight <= 0) return message.error("Invalid weight");
    setCapturedWeight(weight);
    message.success(`Captured: ${weight} kg`);
  };

  const handleTransactionCreated = () => {
    // 1. Reset Dashboard Local State
    setCapturedWeight(null);
    setFormData(INITIAL_FORM_DATA);
    
    // 2. Trigger Queue Table Refresh via refreshKey
    setRefreshKey((k) => k + 1);
    
    // 3. Force re-fetch incomplete transactions to ensure queue is updated
    dispatch(fetchTransactions({ isCompleted: false, pageSize: 50 }));
  };

  return (
    <div className="flex flex-col h-full gap-2 overflow-hidden">
      <div className="flex gap-2 h-[62%] shrink-0">
        <div className="w-[38%] h-full">
          <Card
            title={<span className="text-[10px] font-bold uppercase">New Transaction</span>}
            size="small"
            className="h-full shadow-sm flex flex-col overflow-hidden"
            bodyStyle={{ flex: 1, padding: "4px 8px", display: "flex", flexDirection: "column", overflow: "hidden" }}
          >
            <CreateTransactionForm
              formData={formData}
              setFormData={setFormData}
              capturedWeight={capturedWeight}
              weighbridges={weighbridges}
              onTransactionCreated={handleTransactionCreated}
            />
          </Card>
        </div>

        <div className="w-[62%] flex flex-col gap-2 h-full">
          <div className="grid grid-cols-2 gap-2 h-1/2">
            <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
            <div className="bg-black rounded overflow-hidden"><CameraGrid type="live" /></div>
          </div>
          <div className="grid grid-cols-2 gap-2 h-1/2">
            <div className="bg-black rounded overflow-hidden"><CameraGrid type="snapshot" /></div>
            <div className="bg-black rounded overflow-hidden"><CameraGrid type="plate" /></div>
          </div>
        </div>
      </div>

      <div className="flex-1 min-h-0">
        <Card
          title={<span className="text-[10px] font-bold uppercase">Active Yard Queue</span>}
          size="small"
          className="h-full shadow-sm flex flex-col overflow-hidden"
          bodyStyle={{ flex: 1, padding: 0, overflow: "hidden" }}
        >
          <IncompleteTransactionsTable
            refreshKey={refreshKey}
            onAddWeighing={(transaction) => {
              setFormData({
                ...transaction,
                firstWeight: transaction.firstWeight?.toString() || "",
                secondWeight: "",
              });
              setCapturedWeight(transaction.currentWeight || null);
            }}
          />
        </Card>
      </div>
    </div>
  );
}