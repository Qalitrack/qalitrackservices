import { useEffect, useState } from "react";
import { Pencil, Trash2, UserPlus } from "lucide-react";
import {
  getDrivers,
  createDriver,
  updateDriver,
  deleteDriver,
} from "../../api/MasterData/Drivers";

export default function DriverPortal() {
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editingDriver, setEditingDriver] = useState(null);
  const [error, setError] = useState(null);

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    phone: "",
    licenseNumber: "",
    licenseExpiryDate: "",
    status: "Active",
  });

  // ✅ Fetch all drivers
  useEffect(() => {
    fetchDrivers();
  }, []);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getDrivers();
      setDrivers(data || []);
    } catch (error) {
      console.error("Failed to load drivers:", error.message);
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  // ✅ Handle input change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // ✅ Submit (Create or Update)
  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingDriver) {
        await updateDriver(editingDriver.id, form);
      } else {
        await createDriver(form);
      }

      await fetchDrivers();
      resetForm();
    } catch (error) {
      console.error("Error saving driver:", error.message);
      alert(`Error: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (driver) => {
    setForm({
      fullName: driver.fullName || "",
      email: driver.email || "",
      phone: driver.phone || "",
      licenseNumber: driver.licenseNumber || "",
      licenseExpiryDate: driver.licenseExpiryDate?.split("T")[0] || "",
      status: driver.status || "Active",
    });
    setEditingDriver(driver);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this driver?")) return;
    setLoading(true);

    try {
      await deleteDriver(id);
      await fetchDrivers();
    } catch (error) {
      console.error("Delete failed:", error.message);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({
      fullName: "",
      email: "",
      phone: "",
      licenseNumber: "",
      licenseExpiryDate: "",
      status: "Active",
    });
    setEditingDriver(null);
  };

  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <UserPlus className="w-6 h-6" /> Driver Management
      </h2>

      {/* ✅ Driver Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        {[
          { label: "Full Name", name: "fullName" },
          { label: "Email", name: "email", type: "email" },
          { label: "Phone", name: "phone" },
          { label: "License Number", name: "licenseNumber" },
        ].map((field) => (
          <div key={field.name}>
            <label className="block text-sm font-medium mb-1">
              {field.label}
            </label>
            <input
              type={field.type || "text"}
              name={field.name}
              value={form[field.name]}
              onChange={handleChange}
              required={field.name === "fullName"}
              className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-amber-400"
            />
          </div>
        ))}

        <div>
          <label className="block text-sm font-medium mb-1">
            License Expiry Date
          </label>
          <input
            type="date"
            name="licenseExpiryDate"
            value={form.licenseExpiryDate}
            onChange={handleChange}
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Status</label>
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
        </div>

        <div className="md:col-span-2 flex gap-3 mt-2">
          <button
            type="submit"
            disabled={loading}
            className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-lg font-semibold transition"
          >
            {editingDriver ? "Update Driver" : "Add Driver"}
          </button>

          {editingDriver && (
            <button
              type="button"
              onClick={resetForm}
              className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-semibold transition"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {/* ✅ Driver List */}
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        Registered Drivers
      </h3>

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : error ? (
        <p className="text-red-500">Error: {error}</p>
      ) : drivers.length === 0 ? (
        <p className="text-gray-500 text-sm">No drivers found.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm border border-gray-200 rounded-lg">
            <thead className="bg-gray-50 text-left">
              <tr>
                {[
                  "Name",
                  "Phone",
                  "License",
                  "Expiry",
                  "Status",
                  "Actions",
                ].map((col) => (
                  <th key={col} className="border px-3 py-2 font-medium">
                    {col}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {drivers.map((driver) => (
                <tr
                  key={driver.id}
                  className="border-t hover:bg-gray-50 transition"
                >
                  <td className="px-3 py-2">{driver.fullName}</td>
                  <td className="px-3 py-2">{driver.phone}</td>
                  <td className="px-3 py-2">{driver.licenseNumber}</td>
                  <td className="px-3 py-2">
                    {driver.licenseExpiryDate?.split("T")[0] || "-"}
                  </td>
                  <td className="px-3 py-2">{driver.status}</td>
                  <td className="px-3 py-2 flex gap-2">
                    <button
                      onClick={() => handleEdit(driver)}
                      className="text-blue-600 hover:text-blue-800"
                      title="Edit Driver"
                    >
                      <Pencil className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(driver.id)}
                      className="text-red-600 hover:text-red-800"
                      title="Delete Driver"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
