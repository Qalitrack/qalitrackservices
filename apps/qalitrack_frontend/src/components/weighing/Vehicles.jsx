// src/pages/Vehicle.jsx
import { useState } from "react";
import { Plus, Trash2, Edit3, Truck } from "lucide-react";

export default function Vehicle() {
  const [vehicles, setVehicles] = useState([
    { id: 1, plate: "KAA 123A", type: "Truck", capacity: 15000 },
    { id: 2, plate: "KBX 456B", type: "Trailer", capacity: 30000 },
  ]);
  const [form, setForm] = useState({ plate: "", type: "", capacity: "" });
  const [editingId, setEditingId] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.plate || !form.type || !form.capacity) return;

    if (editingId) {
      setVehicles((prev) =>
        prev.map((v) =>
          v.id === editingId ? { ...v, ...form, capacity: parseInt(form.capacity) } : v
        )
      );
      setEditingId(null);
    } else {
      setVehicles((prev) => [
        ...prev,
        { id: Date.now(), ...form, capacity: parseInt(form.capacity) },
      ]);
    }

    setForm({ plate: "", type: "", capacity: "" });
  };

  const handleEdit = (vehicle) => {
    setForm({ plate: vehicle.plate, type: vehicle.type, capacity: vehicle.capacity });
    setEditingId(vehicle.id);
  };

  const handleDelete = (id) => {
    if (window.confirm("Are you sure you want to delete this vehicle?")) {
      setVehicles((prev) => prev.filter((v) => v.id !== id));
    }
  };

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-bold text-amber-600 flex items-center gap-2">
        <Truck className="w-6 h-6" /> Vehicle Management
      </h1>

      {/* Form */}
      <form
        onSubmit={handleSubmit}
        className="bg-white border rounded p-4 shadow space-y-4 max-w-lg"
      >
        <div>
          <label className="block text-sm font-medium text-gray-700">Plate</label>
          <input
            type="text"
            name="plate"
            value={form.plate}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="e.g. KAA 123A"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Type</label>
          <input
            type="text"
            name="type"
            value={form.type}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="e.g. Truck / Trailer"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Capacity (kg)</label>
          <input
            type="number"
            name="capacity"
            value={form.capacity}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="e.g. 15000"
          />
        </div>

        <button
          type="submit"
          className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          <Plus className="w-4 h-4" />
          {editingId ? "Update Vehicle" : "Add Vehicle"}
        </button>
      </form>

      {/* Table */}
      <div className="bg-white border rounded shadow overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-4 py-2 border">Plate</th>
              <th className="px-4 py-2 border">Type</th>
              <th className="px-4 py-2 border">Capacity</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v) => (
              <tr key={v.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 border">{v.plate}</td>
                <td className="px-4 py-2 border">{v.type}</td>
                <td className="px-4 py-2 border">{v.capacity} kg</td>
                <td className="px-4 py-2 border space-x-2">
                  <button
                    onClick={() => handleEdit(v)}
                    className="px-2 py-1 bg-blue-500 text-white rounded hover:bg-blue-600 inline-flex items-center gap-1"
                  >
                    <Edit3 size={14} /> Edit
                  </button>
                  <button
                    onClick={() => handleDelete(v.id)}
                    className="px-2 py-1 bg-red-500 text-white rounded hover:bg-red-600 inline-flex items-center gap-1"
                  >
                    <Trash2 size={14} /> Delete
                  </button>
                </td>
              </tr>
            ))}
            {vehicles.length === 0 && (
              <tr>
                <td colSpan="4" className="text-center py-4 text-gray-500">
                  No vehicles registered yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
