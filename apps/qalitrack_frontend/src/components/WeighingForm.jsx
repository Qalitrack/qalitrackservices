import { useState, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { addTransaction, fetchVehicles, fetchDrivers, fetchProducts, fetchRoutes } from "../store/weighingSlice";
import { ClipboardList } from "lucide-react";
import toast from "react-hot-toast";

// Mock Weighing Panel
function WeighingPanel({ onCapture }) {
  const simulateCapture = () => {
    const weight = Math.floor(Math.random() * 30000) + 5000;
    toast.success("Weight Captured");
    onCapture(weight);
  };

  return (
    <div className="border rounded p-3 bg-gray-50">
      <h4 className="font-medium text-gray-700 mb-2">Weighing Panel</h4>
      <button type="button" onClick={simulateCapture} className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded">
        Capture Weight
      </button>
    </div>
  );
}

export default function WeighingForm() {
  const dispatch = useDispatch();
  const { vehicles, drivers, products, suppliers, saccos, routes, transporters, error } = useSelector((state) => state.weighing);
  const [form, setForm] = useState({
    vehicleId: "",
    driverId: "",
    productId: "",
    supplierId: "",
    saccoId: "",
    routeId: "",
    transporterId: "",
    operation: "Outbound Product Dispatch",
    w1: "",
    w2: "",
    netWeight: "",
    isManualVehicle: false,
    isManualDriver: false,
    isManualProduct: false,
    isManualSupplier: false,
    isManualSacco: false,
    isManualRoute: false,
    isManualTransporter: false,
    manualVehicle: { registrationNumber: "", type: "" },
    manualDriver: { name: "" },
    manualProduct: { name: "" },
    manualSupplier: { name: "" },
    manualSacco: { name: "" },
    manualRoute: { name: "" },
    manualTransporter: { name: "" },
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchData();
  }, [dispatch]);

  const fetchData = async () => {
    try {
      setLoading(true);
      await Promise.all([
        dispatch(fetchVehicles()),
        dispatch(fetchDrivers()),
        dispatch(fetchProducts()),
        dispatch(fetchRoutes()),
      ]);
    } catch (error) {
      console.error("❌ Failed to fetch data:", error.message);
      toast.error("Failed to load some data. Using manual entry as fallback.");
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    if (name.startsWith("isManual")) {
      setForm((prev) => ({
        ...prev,
        [name]: type === "checkbox" ? checked : value,
        ...(checked && {
          [`manual${name.slice(8)}`]: { ...(prev[`manual${name.slice(8)}`] || {}) },
        }),
      }));
    } else if (name.startsWith("manual")) {
      const field = name.split(".")[1] || name.replace("manual", "").toLowerCase();
      setForm((prev) => ({
        ...prev,
        [name]: { ...prev[name], [field]: value },
      }));
    } else {
      setForm((prev) => ({ ...prev, [name]: value }));
    }
  };

  const handleWeightCapture = (weight) => {
    setForm((prev) => {
      if (!prev.w1) {
        return { ...prev, w1: weight };
      } else if (!prev.w2) {
        const net = prev.w1 ? (prev.operation === "Inbound Product Receipt" ? prev.w1 - weight : weight - prev.w1) : null;
        return { ...prev, w2: weight, netWeight: net };
      } else {
        toast.error("Both weights already captured");
        return prev;
      }
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const finalForm = {
      vehicleId: form.isManualVehicle ? form.manualVehicle.registrationNumber : form.vehicleId,
      driverId: form.isManualDriver ? form.manualDriver.name : form.driverId,
      productId: form.isManualProduct ? form.manualProduct.name : form.productId,
      supplierId: form.isManualSupplier ? form.manualSupplier.name : form.supplierId,
      saccoId: form.isManualSacco ? form.manualSacco.name : form.saccoId,
      routeId: form.isManualRoute ? form.manualRoute.name : form.routeId,
      transporterId: form.isManualTransporter ? form.manualTransporter.name : form.transporterId,
      operation: form.operation,
      w1: form.w1,
      w2: form.w2,
      netWeight: form.netWeight,
    };

    if (!finalForm.vehicleId || !finalForm.driverId || !finalForm.productId || !finalForm.supplierId || !finalForm.transporterId || !finalForm.w1) {
      toast.error("Please fill required fields");
      return;
    }

    const net = finalForm.w1 && finalForm.w2 ? (finalForm.operation === "Inbound Product Receipt" ? parseFloat(finalForm.w1) - parseFloat(finalForm.w2) : parseFloat(finalForm.w2) - parseFloat(finalForm.w1)) : null;

    dispatch(
      addTransaction({
        ...finalForm,
        id: Date.now().toString(),
        date: new Date().toISOString(),
        w1: finalForm.w1 ? parseFloat(finalForm.w1) : null,
        w2: finalForm.w2 ? parseFloat(finalForm.w2) : null,
        netWeight: net,
        deactivated: false,
      })
    );

    toast.success("Transaction saved");
    setForm({
      vehicleId: "",
      driverId: "",
      productId: "",
      supplierId: "",
      saccoId: "",
      routeId: "",
      transporterId: "",
      operation: "Outbound Product Dispatch",
      w1: "",
      w2: "",
      netWeight: "",
      isManualVehicle: false,
      isManualDriver: false,
      isManualProduct: false,
      isManualSupplier: false,
      isManualSacco: false,
      isManualRoute: false,
      isManualTransporter: false,
      manualVehicle: { registrationNumber: "", type: "" },
      manualDriver: { name: "" },
      manualProduct: { name: "" },
      manualSupplier: { name: "" },
      manualSacco: { name: "" },
      manualRoute: { name: "" },
      manualTransporter: { name: "" },
    });
  };

  return (
    <form onSubmit={handleSubmit} className="bg-white shadow-md rounded-lg p-4 border space-y-6">
      <h2 className="text-xl font-bold text-amber-600 flex items-center gap-2">
        <ClipboardList className="w-5 h-5" /> Vehicle Weighing Transaction
      </h2>

      <section>
        <h3 className="font-semibold text-gray-700 mb-2">Transaction Details</h3>
        <div className="grid grid-cols-2 gap-4">
          {/* Vehicle */}
          <div className="col-span-2">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualVehicle"
                checked={form.isManualVehicle}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Vehicle Entry
            </label>
            {!form.isManualVehicle ? (
              <select
                name="vehicleId"
                value={form.vehicleId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Vehicle --</option>
                {vehicles && Array.isArray(vehicles) ? vehicles.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.registrationNumber || v.id} ({v.type})
                  </option>
                )) : <option disabled>No registered vehicles or loading...</option>}
              </select>
            ) : (
              <div className="grid grid-cols-2 gap-2">
                <input
                  name="manualVehicle.registrationNumber"
                  value={form.manualVehicle.registrationNumber}
                  onChange={handleChange}
                  placeholder="Registration Number"
                  className="w-full border rounded px-2 py-1"
                  required
                />
                <input
                  name="manualVehicle.type"
                  value={form.manualVehicle.type}
                  onChange={handleChange}
                  placeholder="Type"
                  className="w-full border rounded px-2 py-1"
                  required
                />
              </div>
            )}
            {error && <p className="text-red-500 text-sm mt-1">{error}</p>}
          </div>

          {/* Driver */}
          <div className="col-span-2">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualDriver"
                checked={form.isManualDriver}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Driver Entry
            </label>
            {!form.isManualDriver ? (
              <select
                name="driverId"
                value={form.driverId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Driver --</option>
                {drivers && Array.isArray(drivers) ? drivers.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name || d.id}
                  </option>
                )) : <option disabled>No registered drivers or loading...</option>}
              </select>
            ) : (
              <input
                name="manualDriver.name"
                value={form.manualDriver.name}
                onChange={handleChange}
                placeholder="Driver Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          {/* Product */}
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualProduct"
                checked={form.isManualProduct}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Product Entry
            </label>
            {!form.isManualProduct ? (
              <select
                name="productId"
                value={form.productId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Product --</option>
                {products && Array.isArray(products) ? products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name || p.id}
                  </option>
                )) : <option disabled>No registered products or loading...</option>}
              </select>
            ) : (
              <input
                name="manualProduct.name"
                value={form.manualProduct.name}
                onChange={handleChange}
                placeholder="Product Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          {/* Supplier */}
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualSupplier"
                checked={form.isManualSupplier}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Supplier Entry
            </label>
            {!form.isManualSupplier ? (
              <select
                name="supplierId"
                value={form.supplierId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Supplier --</option>
                <option disabled>No registered supplier available (use manual entry)</option>
              </select>
            ) : (
              <input
                name="manualSupplier.name"
                value={form.manualSupplier.name}
                onChange={handleChange}
                placeholder="Supplier Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          {/* Sacco */}
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualSacco"
                checked={form.isManualSacco}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Sacco Entry
            </label>
            {!form.isManualSacco ? (
              <select
                name="saccoId"
                value={form.saccoId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Sacco --</option>
                <option disabled>No registered sacco available (use manual entry)</option>
              </select>
            ) : (
              <input
                name="manualSacco.name"
                value={form.manualSacco.name}
                onChange={handleChange}
                placeholder="Sacco Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          {/* Route */}
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualRoute"
                checked={form.isManualRoute}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Route Entry
            </label>
            {!form.isManualRoute ? (
              <select
                name="routeId"
                value={form.routeId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Route --</option>
                {routes && Array.isArray(routes) ? routes.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.name || r.id}
                  </option>
                )) : <option disabled>No registered routes or loading...</option>}
              </select>
            ) : (
              <input
                name="manualRoute.name"
                value={form.manualRoute.name}
                onChange={handleChange}
                placeholder="Route Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          {/* Transporter */}
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isManualTransporter"
                checked={form.isManualTransporter}
                onChange={handleChange}
                className="mr-2"
              />
              Manual Transporter Entry
            </label>
            {!form.isManualTransporter ? (
              <select
                name="transporterId"
                value={form.transporterId}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                disabled={loading}
              >
                <option value="">-- Select Transporter --</option>
                <option disabled>No registered transporter available (use manual entry)</option>
              </select>
            ) : (
              <input
                name="manualTransporter.name"
                value={form.manualTransporter.name}
                onChange={handleChange}
                placeholder="Transporter Name"
                className="w-full border rounded px-2 py-1"
                required
              />
            )}
          </div>

          <div className="col-span-2">
            <label>Operation</label>
            <select
              name="operation"
              value={form.operation}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              disabled={loading}
            >
              <option value="Outbound Product Dispatch">Outbound Product Dispatch</option>
              <option value="Inbound Product Receipt">Inbound Product Receipt</option>
            </select>
          </div>
        </div>
      </section>

      <section>
        <h3 className="font-semibold text-gray-700 mb-2">Weight Management</h3>
        <div className="grid grid-cols-3 gap-3">
          <div>
            <label>First Weight (kg)</label>
            <input
              type="number"
              name="w1"
              value={form.w1}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              disabled={loading}
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
              disabled={loading}
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
        <WeighingPanel onCapture={handleWeightCapture} />
      </section>

      <button
        type="submit"
        className="w-full bg-amber-500 hover:bg-amber-600 text-white py-2 rounded font-semibold"
        disabled={loading}
      >
        Save Transaction
      </button>
    </form>
  );
}