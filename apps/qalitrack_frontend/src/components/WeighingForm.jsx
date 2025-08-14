// src/components/WeighingForm.jsx
import { useState } from "react";
import { useDispatch } from "react-redux";
import { startWeighing } from "../store/weighingSlice";
import { Truck, Package, User } from "lucide-react";

export default function WeighingForm() {
  const dispatch = useDispatch();

  const [transactionType, setTransactionType] = useState("inbound");
  const [plate, setPlate] = useState("");
  const [orderId, setOrderId] = useState("");
  const [driverName, setDriverName] = useState("");
  const [batchNo, setBatchNo] = useState("");
  const [w1, setW1] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!plate || !orderId || !w1) {
      alert("Plate, Order ID, and Weight 1 are required.");
      return;
    }

    dispatch(
      startWeighing({
        type: transactionType,
        plate,
        orderId,
        driverName,
        batchNo,
        w1: parseFloat(w1),
      })
    );

    // Reset
    setPlate("");
    setOrderId("");
    setDriverName("");
    setBatchNo("");
    setW1("");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white shadow rounded-lg p-4 space-y-4 border"
    >
      <h2 className="text-lg font-semibold text-gray-700">New Weighing Transaction</h2>

      {/* Transaction Type */}
      <div className="flex items-center gap-4">
        <label className="font-medium text-gray-600">Transaction Type:</label>
        <select
          value={transactionType}
          onChange={(e) => setTransactionType(e.target.value)}
          className="border rounded px-3 py-2"
        >
          <option value="inbound">Inbound</option>
          <option value="outbound">Outbound</option>
        </select>
      </div>

      {/* ERP Integration placeholder */}
      {/* 
        In future, fetch order, driver, and batch details from ERP based on orderId.
      */}

      {/* Plate & Order */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="flex items-center border rounded px-2">
          <Truck className="w-5 h-5 text-gray-500 mr-2" />
          <input
            type="text"
            placeholder="Plate Number"
            value={plate}
            onChange={(e) => setPlate(e.target.value)}
            className="w-full py-2 outline-none"
          />
        </div>
        <div className="flex items-center border rounded px-2">
          <Package className="w-5 h-5 text-gray-500 mr-2" />
          <input
            type="text"
            placeholder="Order ID"
            value={orderId}
            onChange={(e) => setOrderId(e.target.value)}
            className="w-full py-2 outline-none"
          />
        </div>
      </div>

      {/* Driver & Batch */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div className="flex items-center border rounded px-2">
          <User className="w-5 h-5 text-gray-500 mr-2" />
          <input
            type="text"
            placeholder="Driver Name"
            value={driverName}
            onChange={(e) => setDriverName(e.target.value)}
            className="w-full py-2 outline-none"
          />
        </div>
        <div className="flex items-center border rounded px-2">
          <Package className="w-5 h-5 text-gray-500 mr-2" />
          <input
            type="text"
            placeholder="Batch No."
            value={batchNo}
            onChange={(e) => setBatchNo(e.target.value)}
            className="w-full py-2 outline-none"
          />
        </div>
      </div>

      {/* Weight 1 */}
      <div className="flex items-center border rounded px-2">
        <input
          type="number"
          placeholder="Weight 1 (kg)"
          value={w1}
          onChange={(e) => setW1(e.target.value)}
          className="w-full py-2 outline-none"
        />
      </div>

      <button
        type="submit"
        className="bg-amber-500 text-white px-4 py-2 rounded hover:bg-amber-600"
      >
        Start Weighing
      </button>
    </form>
  );
}
