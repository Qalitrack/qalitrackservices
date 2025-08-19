// src/pages/Vehicles.jsx
import { useState } from "react";
import { Car, PlusCircle, Trash2 } from "lucide-react";

export default function Vehicles() {
  const [vehicles, setVehicles] = useState([]);
  const [form, setForm] = useState({
    plate: "",
    brand: "",
    model: "",
    capacity: "",
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.plate || !form.brand || !form.model) return;
    setVehicles([...vehicles, { ...form, id: Date.now() }]);
    setForm({ plate: "", brand: "", model: "", capacity: "" });
  };

  const handleDelete = (id) => {
    setVehicles(vehicles.filter((v) => v.id !== id));
  };

  return (
    <div className="p-4 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2 text-amber-600">
        <Car className="w-8 h-8" />
        <h1 className="text-2xl font-bold">Vehicles Management</h1>
      </div>

      {/* Form Section */}
      <div className="bg-white shadow-lg rounded-lg border p-6">
        <h2 className="text-lg font-semibold text-gray-700 mb-4">
          Add New Vehicle
        </h2>
        <form
          onSubmit={handleAdd}
          className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5"
        >
          <input
            type="text"
            name="plate"
            value={form.plate}
            onChange={handleChange}
            placeholder="Plate Number"
            className="border rounded px-3 py-2 w-full focus:ring focus:ring-amber-200"
          />
          <input
            type="text"
            name="brand"
            value={form.brand}
            onChange={handleChange}
            placeholder="Brand"
            className="border rounded px-3 py-2 w-full focus:ring focus:ring-amber-200"
          />
          <input
            type="text"
            name="model"
            value={form.model}
            onChange={handleChange}
            placeholder="Model"
            className="border rounded px-3 py-2 w-full focus:ring focus:ring-amber-200"
          />
          <input
            type="number"
            name="capacity"
            value={form.capacity}
            onChange={handleChange}
            placeholder="Capacity (tons)"
            className="border rounded px-3 py-2 w-full focus:ring focus:ring-amber-200"
          />
          <button
            type="submit"
            className="flex items-center justify-center gap-2 bg-amber-500 text-white rounded px-4 py-2 hover:bg-amber-600 transition"
          >
            <PlusCircle size={18} /> Add Vehicle
          </button>
        </form>
      </div>

      {/* Vehicle List */}
      <div className="bg-white shadow-lg rounded-lg border overflow-x-auto">
        <h2 className="text-lg font-semibold text-gray-700 p-4 border-b">
          Vehicle List
        </h2>
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50 text-gray-600">
            <tr>
              <th className="px-4 py-2 border">Plate</th>
              <th className="px-4 py-2 border">Brand</th>
              <th className="px-4 py-2 border">Model</th>
              <th className="px-4 py-2 border">Capacity (tons)</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v, i) => (
              <tr
                key={v.id}
                className={i % 2 === 0 ? "bg-white" : "bg-gray-50"}
              >
                <td className="px-4 py-2 border font-medium">{v.plate}</td>
                <td className="px-4 py-2 border">{v.brand}</td>
                <td className="px-4 py-2 border">{v.model}</td>
                <td className="px-4 py-2 border text-center">{v.capacity}</td>
                <td className="px-4 py-2 border text-center">
                  <button
                    onClick={() => handleDelete(v.id)}
                    className="flex items-center gap-1 px-3 py-1 text-red-600 bg-red-50 rounded hover:bg-red-100 transition"
                  >
                    <Trash2 size={16} /> Remove
                  </button>
                </td>
              </tr>
            ))}
            {vehicles.length === 0 && (
              <tr>
                <td
                  colSpan="5"
                  className="text-center py-6 text-gray-500 italic"
                >
                  No vehicles added yet 🚚
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
