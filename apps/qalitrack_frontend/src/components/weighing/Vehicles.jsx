import { useEffect, useState } from "react";
import { Pencil, Trash2, Truck, Plus, Search, Filter, X } from "lucide-react";
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

  // ✅ Fetch vehicles
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

  // ✅ Input change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // ✅ Submit (Add/Edit)
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

  // ✅ Edit
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

  // ✅ Delete
  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this vehicle?")) return;
    try {
      await deleteVehicle(id);
      await fetchVehicles();
    } catch (error) {
      console.error("❌ Delete failed:", error.message);
    }
  };

  // ✅ Reset form
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
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <Truck className="w-4 h-4 text-black font-bold" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Vehicles</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{vehicles.length}</span> registered
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input 
              type="text"
              placeholder="Search vehicles..." 
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500" 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)} 
            />
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-emerald-50/20 border-b-2 border-amber-200 px-4 py-3 shrink-0 shadow-inner">
        <div className="bg-white rounded-lg p-4 border-2 border-amber-300 shadow-md">
          <h3 className="text-sm font-bold text-amber-900 mb-3 flex items-center gap-2">
            <Plus className="w-4 h-4" />
            {editingVehicle ? "Edit Vehicle" : "Add New Vehicle"}
          </h3>
          
          <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-3">
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>
                Registration Number *
              </label>
              <input
                name="registrationNumber"
                value={form.registrationNumber}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-blue-300 px-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-200"
                required
                placeholder="e.g., KXX 123Y"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full"></span>
                Type *
              </label>
              <input
                name="type"
                value={form.type}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-purple-300 px-2 focus:border-purple-500 focus:ring-1 focus:ring-purple-200"
                required
                placeholder="e.g., Truck, Van"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-pink-500 rounded-full"></span>
                Model
              </label>
              <input
                name="model"
                value={form.model}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-pink-300 px-2 focus:border-pink-500 focus:ring-1 focus:ring-pink-200"
                placeholder="e.g., Isuzu FRR"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-amber-500 rounded-full"></span>
                Color
              </label>
              <input
                name="color"
                value={form.color}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                placeholder="e.g., White, Blue"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-indigo-500 rounded-full"></span>
                Supplier 
              </label>
              <input
                name="supplierId"
                value={form.supplierId}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-indigo-300 px-2 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-200"
                placeholder="Supplier ID"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-cyan-500 rounded-full"></span>
                Transporter 
              </label>
              <input
                name="transporterId"
                value={form.transporterId}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-cyan-300 px-2 focus:border-cyan-500 focus:ring-1 focus:ring-cyan-200"
                placeholder="Transporter ID"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>
                Owner 
              </label>
              <input
                name="ownerId"
                value={form.ownerId}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-rose-300 px-2 focus:border-rose-500 focus:ring-1 focus:ring-rose-200"
                placeholder="Owner ID"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-teal-500 rounded-full"></span>
                Axle Config 
              </label>
              <input
                name="axleConfigurationId"
                value={form.axleConfigurationId}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-teal-300 px-2 focus:border-teal-500 focus:ring-1 focus:ring-teal-200"
                placeholder="Axle Config ID"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>
                Status
              </label>
              <select
                name="status"
                value={form.status}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-emerald-300 px-2 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-200"
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>

            <div className="col-span-4 flex gap-2 justify-end mt-2">
              {editingVehicle && (
                <button
                  type="button"
                  onClick={resetForm}
                  className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5"
                >
                  <X className="w-3.5 h-3.5" />
                  Cancel
                </button>
              )}
              <button
                type="submit"
                disabled={loading}
                className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5"
              >
                <Plus className="w-3.5 h-3.5" />
                {editingVehicle ? "Update Vehicle" : "Add Vehicle"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">Loading vehicles...</p>
            </div>
          ) : vehicles.length === 0 ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">No vehicles found.</p>
            </div>
          ) : (
            <div className="overflow-auto h-full">
              <table className="w-full text-sm">
                <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                  <tr>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Reg. Number</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Type</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Model</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Color</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Status</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-center uppercase tracking-wide">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {vehicles.map((v, index) => (
                    <tr 
                      key={v.id} 
                      className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                    >
                      <td className="px-4 py-2.5">
                        <div className="inline-block bg-gradient-to-r from-gray-800 to-black text-amber-400 px-2 py-0.5 rounded text-[10px] font-bold tracking-wider shadow-sm">
                          {v.registrationNumber}
                        </div>
                      </td>
                      <td className="px-4 py-2.5 text-[10px] text-purple-600 font-medium">{v.type}</td>
                      <td className="px-4 py-2.5 text-[10px] text-gray-700">{v.model || '-'}</td>
                      <td className="px-4 py-2.5 text-[10px] text-pink-600 font-medium">{v.color || '-'}</td>
                      <td className="px-4 py-2.5">
                        <span className={`px-2.5 py-1 rounded-full text-[9px] font-bold uppercase shadow-sm ${
                          v.status === "Active"
                            ? "bg-gradient-to-r from-emerald-100 to-green-200 text-emerald-700 border border-emerald-300"
                            : "bg-gradient-to-r from-red-100 to-rose-200 text-red-700 border border-red-300"
                        }`}>
                          {v.status === "Active" ? "✓ Active" : "✕ Inactive"}
                        </span>
                      </td>
                      <td className="px-4 py-2.5">
                        <div className="flex gap-2 justify-center">
                          <button
                            onClick={() => handleEdit(v)}
                            className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all"
                            title="Edit"
                          >
                            <Pencil className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => handleDelete(v.id)}
                            className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                            title="Delete"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}