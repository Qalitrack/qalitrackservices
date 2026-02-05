import { useEffect, useState } from "react";
import { Pencil, Trash2, Truck, Plus, Search, X } from "lucide-react";
import {
  getVehicles,
  createVehicle,
  updateVehicle,
  deleteVehicle,
} from "../../api/MasterData/Vehicles";

export default function Vehicles() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({
    registrationNumber: "",
    type: "",
    color: "",
    model: "",
    status: "Active",
    supplierName: "",
    transporterName: "",
    ownerName: "",
    axleConfigurationId: "",
  });
  const [editingVehicle, setEditingVehicle] = useState(null);
  const [pageNumber] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

  const fetchVehicles = async () => {
    try {
      setLoading(true);
      const data = await getVehicles(pageNumber, 50, searchTerm);
      console.log("🚗 Vehicles fetched:", data);
      setVehicles(data.data?.items || data.items || data || []);
    } catch (error) {
      console.error("❌ Failed to fetch vehicles:", error.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchVehicles();
  }, [searchTerm]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      if (editingVehicle) {
        await updateVehicle(editingVehicle.id, form);
      } else {
        await createVehicle(form);
      }
      resetForm();
      await fetchVehicles();
    } catch (error) {
      alert("Error saving vehicle: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (vehicle) => {
    setEditingVehicle(vehicle);
    setForm({
      registrationNumber: vehicle.registrationNumber || "",
      type: vehicle.type || "",
      color: vehicle.color || "",
      model: vehicle.model || "",
      status: vehicle.status || "Active",
      supplierId: vehicle.supplierId || "",
      transporterId: vehicle.transporterId || "",
      ownerId: vehicle.ownerId || "",
      axleConfigurationId: vehicle.axleConfigurationId || "",
    });
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this vehicle?")) return;
    try {
      await deleteVehicle(id);
      await fetchVehicles();
    } catch (error) {
      console.error("❌ Delete failed:", error.message);
    }
  };

  const resetForm = () => {
    setForm({
      registrationNumber: "",
      type: "",
      color: "",
      model: "",
      status: "Active",
      supplierId: "",
      transporterId: "",
      ownerId: "",
      axleConfigurationId: "",
    });
    setEditingVehicle(null);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <Truck className="w-4 h-4 text-white" />
            </div>
            <div>
              <span className="text-[11px] font-bold text-gray-900 block leading-tight">
                Vehicles
              </span>
              <span className="text-[9px] text-amber-700 font-medium">
                {vehicles.length} registered vehicles
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search vehicles..."
                className="w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <button
              onClick={() => {
                setSearchTerm("");
                fetchVehicles();
              }}
              className="h-7 px-3 text-[11px] rounded-md border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm font-medium bg-white"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
        <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Registration Number *
            </label>
            <input
              name="registrationNumber"
              value={form.registrationNumber}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              required
              placeholder="e.g., KXX 123Y"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Type *
            </label>
            <input
              name="type"
              value={form.type}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              required
              placeholder="e.g., Truck, Van"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Model
            </label>
            <input
              name="model"
              value={form.model}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="e.g., Isuzu FRR"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Color
            </label>
            <input
              name="color"
              value={form.color}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="e.g., White, Blue"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Supplier
            </label>
            <input
              name="supplierId"
              value={form.supplierId}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="Supplier ID"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Transporter
            </label>
            <input
              name="transporterId"
              value={form.transporterId}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="Transporter ID"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Owner
            </label>
            <input
              name="ownerId"
              value={form.ownerId}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="Owner ID"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Status
            </label>
            <select
              name="status"
              value={form.status}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            >
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </div>

          <div className="col-span-4 flex gap-2 justify-end mt-1">
            {editingVehicle && (
              <button
                type="button"
                onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1"
              >
                <X className="w-3 h-3" />
                Cancel
              </button>
            )}
            <button
              type="submit"
              disabled={loading}
              className="h-7 px-3 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all flex items-center gap-1"
            >
              <Plus className="w-3 h-3" />
              {editingVehicle ? "Update" : "Add"} Vehicle
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading vehicles...</p>
          </div>
        ) : vehicles.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">No vehicles found.</p>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Reg. Number</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Type</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Model</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Color</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {vehicles.map((v, index) => (
                <tr
                  key={v.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2">
                    <div className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[10px] font-bold tracking-wider">
                      {v.registrationNumber}
                    </div>
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-700 font-medium">{v.type}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{v.model || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{v.color || "-"}</td>
                  <td className="px-3 py-2">
                    <span
                      className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase ${
                        v.status === "Active"
                          ? "bg-green-100 text-green-700 border border-green-300"
                          : "bg-red-100 text-red-700 border border-red-300"
                      }`}
                    >
                      {v.status === "Active" ? "✓ Active" : "✕ Inactive"}
                    </span>
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        onClick={() => handleEdit(v)}
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(v.id)}
                        className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                        title="Delete"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Footer */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <span className="text-[10px] text-gray-600 font-medium">
          <span className="font-semibold text-amber-600">{vehicles.length}</span> total vehicles
        </span>
      </div>

      <style>{`
        .compact-table {
          font-size: 10px;
        }
        .compact-table thead tr th {
          padding: 6px 12px;
          font-weight: 700;
          font-size: 9px;
          line-height: 1.2;
        }
        .compact-table tbody tr td {
          padding: 6px 12px;
          line-height: 1.3;
        }
      `}</style>
    </div>
  );
}