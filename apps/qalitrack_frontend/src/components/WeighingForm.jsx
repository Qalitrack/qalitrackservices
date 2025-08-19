import { useState } from "react";
import { useDispatch } from "react-redux";
import { addTransaction } from "../store/weighingSlice";
import { Truck, ClipboardList, User, Package } from "lucide-react";
import toast from "react-hot-toast";

export default function WeighingForm() {
  const dispatch = useDispatch();

  const [form, setForm] = useState({
    type: "inbound",
    plate: "",
    driver: "",
    orderId: "",
    batch: "",
    w1: "",
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.plate || !form.driver || !form.orderId || !form.w1) {
      toast.error("Please fill all required fields");
      return;
    }

    dispatch(
      addTransaction({
        ...form,
        id: Date.now().toString(),
        date: new Date().toISOString(),
        w1: parseFloat(form.w1),
        w2: null,
        ttat: null,
        deactivated: false,
      })
    );

    toast.success("Transaction saved");

    setForm({
      type: "inbound",
      plate: "",
      driver: "",
      orderId: "",
      batch: "",
      w1: "",
    });
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white shadow-md rounded-lg p-4 border space-y-4"
    >
      <h2 className="text-xl font-bold text-amber-600 flex items-center gap-2">
        <ClipboardList className="w-5 h-5" /> New Weighing Transaction
      </h2>

      {/* Transaction Type */}
      <div>
        <label className="block font-medium text-gray-700">Transaction Type</label>
        <select
          name="type"
          value={form.type}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full"
        >
          <option value="inbound">Inbound</option>
          <option value="outbound">Outbound</option>
        </select>
      </div>

      {/* Plate Number */}
      <div>
        <label className="block font-medium text-gray-700">Plate Number</label>
        <div className="flex items-center border rounded px-2">
          <Truck className="w-4 h-4 text-gray-500 mr-2" />
          <input
            type="text"
            name="plate"
            value={form.plate}
            onChange={handleChange}
            className="flex-1 py-2 outline-none"
            placeholder="e.g. KAA 123A"
            required
          />
        </div>
      </div>

      {/* Driver Name */}
      <div>
        <label className="block font-medium text-gray-700">Driver Name</label>
        <div className="flex items-center border rounded px-2">
          <User className="w-4 h-4 text-gray-500 mr-2" />
          <input
            type="text"
            name="driver"
            value={form.driver}
            onChange={handleChange}
            className="flex-1 py-2 outline-none"
            placeholder="Driver full name"
            required
          />
        </div>
      </div>

      {/* Order ID */}
      <div>
        <label className="block font-medium text-gray-700">Order ID</label>
        <div className="flex items-center border rounded px-2">
          <Package className="w-4 h-4 text-gray-500 mr-2" />
          <input
            type="text"
            name="orderId"
            value={form.orderId}
            onChange={handleChange}
            className="flex-1 py-2 outline-none"
            placeholder="Order number"
            required
          />
        </div>
      </div>

      {/* Batch */}
      <div>
        <label className="block font-medium text-gray-700">Batch</label>
        <input
          type="text"
          name="batch"
          value={form.batch}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full"
          placeholder="Batch number"
        />
      </div>

      {/* Weight 1 */}
      <div>
        <label className="block font-medium text-gray-700">Weight 1 (Tons)</label>
        <input
          type="number"
          name="w1"
          value={form.w1}
          onChange={handleChange}
          className="border rounded px-3 py-2 w-full"
          placeholder="First weight"
          required
        />
      </div>

      {/* Submit */}
      <button
        type="submit"
        className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded w-full font-semibold"
      >
        Save Transaction
      </button>

      <p className="text-xs text-gray-500">
        * ERP Integration for fetching driver/order/batch info will be added later.
      </p>
    </form>
  );
}
