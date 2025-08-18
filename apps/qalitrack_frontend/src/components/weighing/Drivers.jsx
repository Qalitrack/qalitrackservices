// src/pages/Drivers.jsx
import { useState } from "react";
import { Plus, Trash2, Edit3, User } from "lucide-react";

export default function Drivers() {
  const [drivers, setDrivers] = useState([
    { id: 1, name: "John Doe", driverId: "D001", license: "LIC12345", phone: "0712345678" },
    { id: 2, name: "Jane Smith", driverId: "D002", license: "LIC67890", phone: "0798765432" },
  ]);

  const [form, setForm] = useState({ name: "", driverId: "", license: "", phone: "" });
  const [editingId, setEditingId] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.name || !form.driverId || !form.license || !form.phone) return;

    if (editingId) {
      setDrivers((prev) =>
        prev.map((d) =>
          d.id === editingId ? { ...d, ...form } : d
        )
      );
      setEditingId(null);
    } else {
      setDrivers((prev) => [
        ...prev,
        { id: Date.now(), ...form },
      ]);
    }

    setForm({ name: "", driverId: "", license: "", phone: "" });
  };

  const handleEdit = (driver) => {
    setForm({
      name: driver.name,
      driverId: driver.driverId,
      license: driver.license,
      phone: driver.phone,
    });
    setEditingId(driver.id);
  };

  const handleDelete = (id) => {
    if (window.confirm("Are you sure you want to delete this driver?")) {
      setDrivers((prev) => prev.filter((d) => d.id !== id));
    }
  };

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-bold text-amber-600 flex items-center gap-2">
        <User className="w-6 h-6" /> Driver Management
      </h1>

      {/* Form */}
      <form
        onSubmit={handleSubmit}
        className="bg-white border rounded p-4 shadow space-y-4 max-w-lg"
      >
        <div>
          <label className="block text-sm font-medium text-gray-700">Name</label>
          <input
            type="text"
            name="name"
            value={form.name}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="Driver Name"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Driver ID</label>
          <input
            type="text"
            name="driverId"
            value={form.driverId}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="Unique ID"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">License No.</label>
          <input
            type="text"
            name="license"
            value={form.license}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="License Number"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Phone</label>
          <input
            type="text"
            name="phone"
            value={form.phone}
            onChange={handleChange}
            className="w-full border rounded px-3 py-2"
            placeholder="e.g. 0712345678"
          />
        </div>

        <button
          type="submit"
          className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          <Plus className="w-4 h-4" />
          {editingId ? "Update Driver" : "Add Driver"}
        </button>
      </form>

      {/* Table */}
      <div className="bg-white border rounded shadow overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-4 py-2 border">Name</th>
              <th className="px-4 py-2 border">Driver ID</th>
              <th className="px-4 py-2 border">License</th>
              <th className="px-4 py-2 border">Phone</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {drivers.map((d) => (
              <tr key={d.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 border">{d.name}</td>
                <td className="px-4 py-2 border">{d.driverId}</td>
                <td className="px-4 py-2 border">{d.license}</td>
                <td className="px-4 py-2 border">{d.phone}</td>
                <td className="px-4 py-2 border space-x-2">
                  <button
                    onClick={() => handleEdit(d)}
                    className="px-2 py-1 bg-blue-500 text-white rounded hover:bg-blue-600 inline-flex items-center gap-1"
                  >
                    <Edit3 size={14} /> Edit
                  </button>
                  <button
                    onClick={() => handleDelete(d.id)}
                    className="px-2 py-1 bg-red-500 text-white rounded hover:bg-red-600 inline-flex items-center gap-1"
                  >
                    <Trash2 size={14} /> Delete
                  </button>
                </td>
              </tr>
            ))}
            {drivers.length === 0 && (
              <tr>
                <td colSpan="5" className="text-center py-4 text-gray-500">
                  No drivers registered yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
