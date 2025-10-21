import { useState } from "react";
import { Pencil, Trash2, UserPlus } from "lucide-react";

export default function DriverPortal() {
  const [drivers, setDrivers] = useState([]);
  const [editingIndex, setEditingIndex] = useState(null);
  const [form, setForm] = useState({
    name: "",
    licenseNo: "",
    phone: "",
    employer: "",
    assignedVehicle: "",
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.name || !form.licenseNo) {
      alert("Driver name and license number are required");
      return;
    }

    if (editingIndex !== null) {
      const updated = [...drivers];
      updated[editingIndex] = form;
      setDrivers(updated);
      setEditingIndex(null);
    } else {
      setDrivers([...drivers, form]);
    }

    setForm({
      name: "",
      licenseNo: "",
      phone: "",
      employer: "",
      assignedVehicle: "",
    });
  };

  const handleEdit = (index) => {
    setForm(drivers[index]);
    setEditingIndex(index);
  };

  const handleDelete = (index) => {
    if (confirm("Are you sure you want to delete this driver?")) {
      setDrivers(drivers.filter((_, i) => i !== index));
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-xl font-bold text-amber-600 mb-4 flex items-center gap-2">
        <UserPlus className="w-5 h-5" /> Driver Management
      </h2>

      {/* Driver Form */}
      <form onSubmit={handleSubmit} className="space-y-4 mb-6">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label>Driver Name</label>
            <input
              name="name"
              value={form.name}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              required
            />
          </div>
          <div>
            <label>License No.</label>
            <input
              name="licenseNo"
              value={form.licenseNo}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              required
            />
          </div>
          <div>
            <label>Phone</label>
            <input
              name="phone"
              value={form.phone}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
              type="tel"
            />
          </div>
          <div>
            <label>Employer / Transporter</label>
            <input
              name="employer"
              value={form.employer}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
          <div className="col-span-2">
            <label>Assigned Vehicle (Plate)</label>
            <input
              name="assignedVehicle"
              value={form.assignedVehicle}
              onChange={handleChange}
              className="w-full border rounded px-2 py-1"
            />
          </div>
        </div>
        <button
          type="submit"
          className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded font-semibold"
        >
          {editingIndex !== null ? "Update Driver" : "Add Driver"}
        </button>
      </form>

      {/* Driver List */}
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        Registered Drivers
      </h3>
      {drivers.length === 0 ? (
        <p className="text-gray-500 text-sm">No drivers added yet.</p>
      ) : (
        <table className="w-full text-sm border">
          <thead className="bg-gray-100">
            <tr>
              <th className="border px-2 py-1">Name</th>
              <th className="border px-2 py-1">License No.</th>
              <th className="border px-2 py-1">Phone</th>
              <th className="border px-2 py-1">Employer</th>
              <th className="border px-2 py-1">Assigned Vehicle</th>
              <th className="border px-2 py-1">Actions</th>
            </tr>
          </thead>
          <tbody>
            {drivers.map((d, i) => (
              <tr key={i}>
                <td className="border px-2 py-1">{d.name}</td>
                <td className="border px-2 py-1">{d.licenseNo}</td>
                <td className="border px-2 py-1">{d.phone}</td>
                <td className="border px-2 py-1">{d.employer}</td>
                <td className="border px-2 py-1">
                  {d.assignedVehicle || "-"}
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
