// src/pages/Drivers.jsx
import { useState } from "react";
import { User, PlusCircle, Trash2 } from "lucide-react";

export default function Drivers() {
  const [drivers, setDrivers] = useState([]);
  const [form, setForm] = useState({ name: "", idNumber: "", phone: "" });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.name || !form.idNumber) return;
    setDrivers([...drivers, { ...form, id: Date.now() }]);
    setForm({ name: "", idNumber: "", phone: "" });
  };

  const handleDelete = (id) => {
    setDrivers(drivers.filter((d) => d.id !== id));
  };

  return (
    <div className="p-4 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2 text-amber-600">
        <User className="w-7 h-7" />
        <h1 className="text-2xl font-bold">Drivers</h1>
      </div>

      {/* Form */}
      <form
        onSubmit={handleAdd}
        className="bg-white shadow rounded-lg p-4 border grid gap-3 md:grid-cols-4"
      >
        <input
          type="text"
          name="name"
          value={form.name}
          onChange={handleChange}
          placeholder="Driver Name"
          className="border rounded px-3 py-2"
        />
        <input
          type="text"
          name="idNumber"
          value={form.idNumber}
          onChange={handleChange}
          placeholder="ID Number"
          className="border rounded px-3 py-2"
        />
        <input
          type="text"
          name="phone"
          value={form.phone}
          onChange={handleChange}
          placeholder="Phone Number"
          className="border rounded px-3 py-2"
        />
        <button
          type="submit"
          className="flex items-center justify-center gap-2 bg-amber-500 text-white rounded px-4 py-2 hover:bg-amber-600"
        >
          <PlusCircle size={18} /> Add
        </button>
      </form>

      {/* Driver List */}
      <div className="bg-white shadow rounded-lg border overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-100 text-left">
            <tr>
              <th className="px-4 py-2 border">Name</th>
              <th className="px-4 py-2 border">ID</th>
              <th className="px-4 py-2 border">Phone</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {drivers.map((d) => (
              <tr key={d.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 border">{d.name}</td>
                <td className="px-4 py-2 border">{d.idNumber}</td>
                <td className="px-4 py-2 border">{d.phone}</td>
                <td className="px-4 py-2 border">
                  <button
                    onClick={() => handleDelete(d.id)}
                    className="text-red-600 hover:text-red-800 flex items-center gap-1"
                  >
                    <Trash2 size={16} /> Remove
                  </button>
                </td>
              </tr>
            ))}
            {drivers.length === 0 && (
              <tr>
                <td colSpan="4" className="text-center py-4 text-gray-500">
                  No drivers added yet 👷
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
