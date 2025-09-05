import { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { addTransaction } from "../store/weighingSlice";
import { ClipboardList } from "lucide-react";
import toast from "react-hot-toast";

// Mock Weighing Panel (replace with real hardware integration later)
function WeighingPanel({ onCapture }) {
  const simulateCapture = () => {
    const weight = Math.floor(Math.random() * 30000) + 5000; // mock random weight
    toast.success("Weight Captured");
    onCapture(weight);
  };

  return (
    <div className="border rounded p-3 bg-gray-50">
      <h4 className="font-medium text-gray-700 mb-2">Weighing Panel</h4>
      <button
        type="button"
        onClick={simulateCapture}
        className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded"
      >
        Capture Weight
      </button>
      {/* 
        Integration point: Replace simulateCapture() with 
        actual hardware API (e.g., from Masterdom/ERP).
      */}
    </div>
  );
}

export default function WeighingForm() {
  const dispatch = useDispatch();
  const vehicles = useSelector((state) => state.vehicles);

  const defaultForm = {
    documentType: "Delivery Note",
    deliveryNoteNo: "",
    referenceNo: "",
    batch: "",
    sourcePlant: "",
    destination: "",
    operation: "Outbound Product Dispatch",
    plate: "",
    vehicleType: "",
    model: "",
    fromAnotherPlant: false,
    otherPlantName: "",
    material: "",
    supplier: "",
    cementType: "",
    bagSize: "",
    w1: "",
    w2: "",
    netWeight: "",
  };

  const [form, setForm] = useState(defaultForm);
  const [manualEntry, setManualEntry] = useState(false);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleWeightCapture = (weight) => {
    setForm((prev) => {
      if (!prev.w1) {
        return { ...prev, w1: weight };
      } else if (!prev.w2) {
        const net = prev.w1 ? prev.w1 - weight : null;
        return { ...prev, w2: weight, netWeight: net };
      } else {
        toast.error("Both weights already captured");
        return prev;
      }
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.plate || !form.deliveryNoteNo || !form.w1) {
      toast.error("Please fill required fields");
      return;
    }

    const net =
      form.w1 && form.w2 ? parseFloat(form.w1) - parseFloat(form.w2) : null;

    dispatch(
      addTransaction({
        ...form,
        id: Date.now().toString(),
        date: new Date().toISOString(),
        w1: form.w1 ? parseFloat(form.w1) : null,
        w2: form.w2 ? parseFloat(form.w2) : null,
        netWeight: net,
        deactivated: false,
      })
    );

    toast.success("Transaction saved");
    setForm(defaultForm);
    setManualEntry(false);
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white shadow-md rounded-lg p-4 border space-y-6"
    >
      <h2 className="text-xl font-bold text-amber-600 flex items-center gap-2">
        <ClipboardList className="w-5 h-5" /> Vehicle Weighing Transaction
      </h2>

      {/* Document Information */}
      <section>
        <h3 className="font-semibold text-gray-700 mb-2">
          Document Information
        </h3>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label>Document Type</label>
            <select
              name="documentType"
              value={form.documentType}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            >
              <option value="Delivery Note">Delivery Note</option>
              <option value="Invoice">Invoice</option>
            </select>
          </div>
          <div>
            <label>Delivery Note No</label>
            <input
              name="deliveryNoteNo"
              value={form.deliveryNoteNo}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              required
            />
          </div>
          <div>
            <label>Reference No</label>
            <input
              name="referenceNo"
              value={form.referenceNo}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Batch</label>
            <input
              name="batch"
              value={form.batch}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Source Plant</label>
            <input
              name="sourcePlant"
              value={form.sourcePlant}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Destination</label>
            <input
              name="destination"
              value={form.destination}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div className="col-span-2">
            <label>Operation</label>
            <select
              name="operation"
              value={form.operation}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            >
              <option>Outbound Product Dispatch</option>
              <option>Inbound Material Receipt</option>
            </select>
          </div>
        </div>
      </section>

      {/* Vehicle Information */}
      <section>
        <h3 className="font-semibold text-gray-700 mb-2">
          Vehicle Information
        </h3>

        {!manualEntry ? (
          <div className="grid grid-cols-2 gap-3">
            <div className="col-span-2">
              <label>Select Vehicle</label>
              <select
                value={form.plate}
                onChange={(e) => {
                  const plate = e.target.value;
                  if (plate === "manual") {
                    setManualEntry(true);
                    setForm((prev) => ({
                      ...prev,
                      plate: "",
                      vehicleType: "",
                      model: "",
                      fromAnotherPlant: false,
                      otherPlantName: "",
                    }));
                    return;
                  }
                  const selectedVehicle = vehicles.find(
                    (v) => v.plate === plate
                  );
                  if (selectedVehicle) {
                    setForm((prev) => ({
                      ...prev,
                      plate: selectedVehicle.plate,
                      vehicleType: selectedVehicle.vehicleType,
                      model: selectedVehicle.model,
                      fromAnotherPlant: selectedVehicle.fromAnotherPlant,
                      otherPlantName: selectedVehicle.otherPlantName,
                    }));
                  }
                }}
                className="w-full border rounded px-2 py-1"
              >
                <option value="">-- Select Vehicle --</option>
                {vehicles.map((v, i) => (
                  <option key={i} value={v.plate}>
                    {v.plate} ({v.vehicleType})
                  </option>
                ))}
                <option value="manual">+ Manual Entry</option>
              </select>
            </div>
            {form.plate && (
              <>
                <div>
                  <label>Vehicle Type</label>
                  <input
                    value={form.vehicleType}
                    readOnly
                    className="w-full border rounded px-2 py-1 bg-gray-100"
                  />
                </div>
                <div>
                  <label>Model</label>
                  <input
                    value={form.model}
                    readOnly
                    className="w-full border rounded px-2 py-1 bg-gray-100"
                  />
                </div>
                {form.fromAnotherPlant && (
                  <div className="col-span-2">
                    <label>Other Plant Name</label>
                    <input
                      value={form.otherPlantName}
                      readOnly
                      className="w-full border rounded px-2 py-1 bg-gray-100"
                    />
                  </div>
                )}
              </>
            )}
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label>Registration Number</label>
              <input
                name="plate"
                value={form.plate}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                required
              />
            </div>
            <div>
              <label>Vehicle Type</label>
              <select
                name="vehicleType"
                value={form.vehicleType}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
              >
                <option>Truck</option>
                <option>Trailer</option>
              </select>
            </div>
            <div>
              <label>Model</label>
              <input
                name="model"
                value={form.model}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                name="fromAnotherPlant"
                checked={form.fromAnotherPlant}
                onChange={handleChange}
              />
              <label>Vehicle from another plant</label>
            </div>
            {form.fromAnotherPlant && (
              <div className="col-span-2">
                <label>Other Plant Name</label>
                <input
                  name="otherPlantName"
                  value={form.otherPlantName}
                  onChange={handleChange}
                  className="w-full border rounded px-2 py-1"
                />
              </div>
            )}
          </div>
        )}
      </section>

      {/* Material Information */}
      <section>
        <h3 className="font-semibold text-gray-700 mb-2">
          Material Information
        </h3>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label>Material</label>
            <input
              name="material"
              value={form.material}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Supplier</label>
            <input
              name="supplier"
              value={form.supplier}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Type</label>
            <input
              name="cementType"
              value={form.cementType}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Bag Size</label>
            <input
              name="bagSize"
              value={form.bagSize}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
        </div>
      </section>

      {/* Weight Management */}
      <section>
        <h3 className="font-semibold text-gray-700 mb-2">
          Weight Management
        </h3>
        <div className="grid grid-cols-3 gap-3">
          <div>
            <label>First Weight (kg)</label>
            <input
              type="number"
              name="w1"
              value={form.w1}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Second Weight (kg)</label>
            <input
              type="number"
              name="w2"
              value={form.w2}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div>
            <label>Net Weight (kg)</label>
            <input
              type="number"
              name="netWeight"
              value={form.netWeight || ""}
              readOnly
              className="w-full border rounded px-2 py-1 bg-gray-100"
            />
          </div>
        </div>

        {/* Weighing Panel */}
        <WeighingPanel onCapture={handleWeightCapture} />
      </section>

      {/* Submit */}
      <button
        type="submit"
        className="w-full bg-amber-500 hover:bg-amber-600 text-white py-2 rounded font-semibold"
      >
        Save Transaction
      </button>
    </form>
  );
}
