import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Card, message } from "antd";
import CreateTransactionForm from "./CreateTransactionForm";
import IncompleteTransactionsTable from "./IncompleteTransactionsTable";
import LiveWeighbridgeStatus from "./LiveWeighbridgeStatus";
import CameraGrid from "../CameraGrid";
import { useLicenseFeature } from "../../hooks/useLicenseFeature";
import { LicenseFeatures } from "../../utils/LicenseFeatures";

import {
  fetchVehicles, fetchDrivers, fetchProducts, fetchRoutes,
  fetchSaccosByName, fetchSuppliersByName, fetchTransportersByName,
  fetchWeighbridges, fetchTransactions, fetchSimulatedWeight,
} from "../../store/weighingSlice";

const INITIAL_FORM_DATA = {
  // Transaction identity — must be explicitly null so isSecondWeighing = false after reset
  id: null,
  ticketID: null,
  receiptNo: "",
  expectedWeighings: 2,
  noPlate: "",
  driverName: "",
  vehicleId: null,
  vehicleID: null,
  driverId: null,
  driverID: null,
  commodityId: null,
  commodityID: null,
  commodityName: "",
  transporterId: null,
  transporterID: null,
  transporterName: "",
  supplierId: null,
  supplierID: null,
  supplierName: "",
  customerId: null,
  customerID: null,
  customerName: "",
  originId: null,
  originID: null,
  originName: "",
  destinationId: null,
  destinationID: null,
  destinationName: "",
  operation: "Inbound Product Receipt",
  weighMode: "Gross/Tare",
  firstWeight: "",
  secondWeight: "",
  scaleName: "Katani Simple",
  operatorName: "",
  operatorId: null,
  operatorID: null,
  weighBridgeId: null,
  weighBridgeID: null,
  weighBridgeName: "Katani Simple",
};

export default function WeighingDashboard() {
  const dispatch = useDispatch();
  const { error, weighbridges = [] } = useSelector((state) => state.weighing);
  const anprLicensed = useLicenseFeature(LicenseFeatures.ANPR);
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

  useEffect(() => { 
    if (error) message.error(error); 
  }, [error]);

  // Debug: Log weighbridges when they change
  useEffect(() => {
    console.log("🔍 Dashboard - weighbridges state:", weighbridges);
  }, [weighbridges]);

  const handleManualCapture = (weight) => {
    if (!weight || weight <= 0) return message.error("Invalid weight");
    setCapturedWeight(weight);
    message.success(`Captured: ${weight} kg`);
  };

  const handlePlateConfirmed = (plate, source) => {
    setFormData((prev) => ({ ...prev, noPlate: plate, nprSource: source }));
    message.success(`Plate set: ${plate} (${source === "auto" ? "Auto NPR" : "Manual"})`);
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
            styles={{ 
              body: { 
                flex: 1, 
                padding: "4px 8px", 
                display: "flex", 
                flexDirection: "column", 
                overflow: "hidden" 
              } 
            }}
          >
            <CreateTransactionForm
              formData={formData}
              setFormData={setFormData}
              capturedWeight={capturedWeight}
              onTransactionCreated={handleTransactionCreated}
            />
          </Card>
        </div>

        <div className="w-[62%] flex flex-col gap-2 h-full">
          <div className="grid grid-cols-2 gap-2 h-1/2">
            <LiveWeighbridgeStatus onManualCapture={handleManualCapture} />
            <div className="bg-black rounded overflow-hidden">
              {anprLicensed ? <CameraGrid type="live" /> : <AnprLockedTile label="Live Feed" />}
            </div>
          </div>
          <div className="grid grid-cols-2 gap-2 h-1/2">
            <div className="bg-black rounded overflow-hidden">
              {anprLicensed ? <CameraGrid type="snapshot" /> : <AnprLockedTile label="Snapshot" />}
            </div>
            <div className="bg-black rounded overflow-hidden">
              {anprLicensed
                ? <CameraGrid type="plate" onPlateConfirmed={handlePlateConfirmed} />
                : <AnprLockedTile label="Plate Recognition" />}
            </div>
          </div>
        </div>
      </div>

      <div className="flex-1 min-h-0">
        <Card
          title={<span className="text-[10px] font-bold uppercase">Active Yard Queue</span>}
          size="small"
          className="h-full shadow-sm flex flex-col overflow-hidden"
          styles={{ 
            body: { 
              flex: 1, 
              padding: 0, 
              overflow: "hidden" 
            } 
          }}
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

function AnprLockedTile({ label }) {
  return (
    <div className="w-full h-full flex flex-col items-center justify-center gap-2 bg-gray-950 text-gray-600">
      <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
      </svg>
      <p className="text-[10px] font-medium">{label}</p>
      <p className="text-[9px] text-gray-700">ANPR not licensed</p>
    </div>
  );
}