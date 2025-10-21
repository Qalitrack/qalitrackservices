import { useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { addVehicle, updateVehicle, deleteVehicle } from "../../store/Vehicleslice";
import { Pencil, Trash2, PlusCircle } from "lucide-react";

export default function VehiclePortal() {
  const vehicles = useSelector((state) => state.vehicles);
  const dispatch = useDispatch();

  const [editingIndex, setEditingIndex] = useState(null);
  const [form, setForm] = useState({
    plate: "",
    vehicleType: "Truck",
    model: "",
    fromAnotherPlant: false,
    otherPlantName: "",
  });

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.plate) return alert("Registration number is required");

    if (editingIndex !== null) {
      dispatch(updateVehicle({ index: editingIndex, vehicle: form }));
      setEditingIndex(null);
    } else {
      dispatch(addVehicle(form));
    }

    setForm({
      plate: "",
      vehicleType: "Truck",
      model: "",
      fromAnotherPlant: false,
      otherPlantName: "",
    });
  };

  const handleEdit = (index) => {
    setForm(vehicles[index]);
    setEditingIndex(index);
  };

  const handleDelete = (index) => {
    if (confirm("Are you sure you want to delete this vehicle?")) {
      dispatch(deleteVehicle(index));
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-xl font-bold text-amber-600 mb-4 flex items-center gap-2">
        <PlusCircle className="w-5 h-5" /> Vehicle Management
      </h2>

      {/* Vehicle Form */}
      <form onSubmit={handleSubmit} className="space-y-4 mb-6">
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
          <div className="flex items-center space-x-2">
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
        <button
          type="submit"
          className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded font-semibold"
        >
          {editingIndex !== null ? "Update Vehicle" : "Add Vehicle"}
        </button>
      </form>

      {/* Vehicle List */}
      <h3 className="text-lg font-semibold text-gray-700 mb-2">Registered Vehicles</h3>
      {vehicles.length === 0 ? (
        <p className="text-gray-500 text-sm">No vehicles added yet.</p>
      ) : (
        <table className="w-full text-sm border">
          <thead className="bg-gray-100">
            <tr>
              <th className="border px-2 py-1">Plate</th>
              <th className="border px-2 py-1">Type</th>
              <th className="border px-2 py-1">Model</th>
              <th className="border px-2 py-1">From Plant?</th>
              <th className="border px-2 py-1">Other Plant</th>
              <th className="border px-2 py-1">Actions</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v, i) => (
              <tr key={i}>
                <td className="border px-2 py-1">{v.plate}</td>
                <td className="border px-2 py-1">{v.vehicleType}</td>
                <td className="border px-2 py-1">{v.model}</td>
                <td className="border px-2 py-1">{v.fromAnotherPlant ? "Yes" : "No"}</td>
                <td className="border px-2 py-1">
                  {v.fromAnotherPlant ? v.otherPlantName : "-"}
                </td>
                <td className="border px-2 py-1 flex gap-2">
                  <button
                    type="button"
                    onClick={() => handleEdit(i)}
                    className="text-blue-600 hover:text-blue-800"
                  >
                    <Pencil className="w-4 h-4" />
                  </button>
                  <button
                    type="button"
                    onClick={() => handleDelete(i)}
                    className="text-red-600 hover:text-red-800"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
