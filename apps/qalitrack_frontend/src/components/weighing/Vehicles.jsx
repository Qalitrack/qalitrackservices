import { useEffect, useState } from "react";
import { Pencil, Trash2, Truck, Plus } from "lucide-react";
import {
  getVehicles,
  createVehicle,
  updateVehicle,
  deleteVehicle,
} from "../../api/MasterData/Vehicle";

export default function Vehicles() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({
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
  const [editingVehicle, setEditingVehicle] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

  // ✅ Fetch vehicles
  const fetchVehicles = async () => {
    try {
      setLoading(true);
      const data = await getVehicles(pageNumber, 10, searchTerm);
      setVehicles(data.items || []);
    } catch (error) {
      console.error("Failed to load vehicles:", error.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchVehicles();
  }, [pageNumber, searchTerm]);

  // ✅ Handle input change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // ✅ Create / Update vehicle
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      if (editingVehicle) {
        await updateVehicle(editingVehicle.id, form);
      } else {
        await createVehicle(form);
      }
      await fetchVehicles();
      resetForm();
    } catch (error) {
      alert("Error saving vehicle: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  // ✅ Edit existing vehicle
  const handleEdit = (vehicle) => {
    setEditingVehicle(vehicle);
    setForm({
      registrationNumber: vehicle.registrationNumber,
      type: vehicle.type,
      color: vehicle.color,
      model: vehicle.model,
      status: vehicle.status,
      supplierId: vehicle.supplierId,
      transporterId: vehicle.transporterId,
      ownerId: vehicle.ownerId,
      axleConfigurationId: vehicle.axleConfigurationId,
    });
  };

  // ✅ Delete vehicle
  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this vehicle?")) return;
    try {
      await deleteVehicle(id);
      await fetchVehicles();
    } catch (error) {
      console.error("Delete failed:", error.message);
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
    <div className="bg-white p-5 shadow rounded-lg border">
      <h2 className="text-xl font-bold text-amber-600 mb-4 flex items-center gap-2">
        <Truck className="w-5 h-5" /> Vehicle Management
      </h2>

      {/* Form */}
      <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-4 mb-6">
        {Object.keys(form).map((key) => (
          key !== "status" && (
            <div key={key}>
              <label className="block text-sm font-medium capitalize">{key}</label>
              <input
                name={key}
                value={form[key]}
                onChange={handleChange}
                className="w-full border rounded px-2 py-1"
                required={["registrationNumber", "type"].includes(key)}
              />
            </div>
          )
        ))}

        <div>
          <label className="block text-sm font-medium">Status</label>
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            className="w-full border rounded px-2 py-1"
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="col-span-2 bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded flex items-center justify-center gap-2 font-semibold"
        >
          <Plus className="w-4 h-4" />
          {editingVehicle ? "Update Vehicle" : "Add Vehicle"}
        </button>
      </form>

      {/* Search Bar */}
      <input
        type="text"
        placeholder="Search vehicles..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="border rounded px-3 py-1 mb-4 w-1/2"
      />

      {/* Vehicle Table */}
      {loading ? (
        <p>Loading vehicles...</p>
      ) : vehicles.length === 0 ? (
        <p className="text-gray-500 text-sm">No vehicles found.</p>
      ) : (
        <table className="w-full text-sm border">
          <thead className="bg-gray-100">
            <tr>
              <th className="border px-2 py-1">Reg. No</th>
              <th className="border px-2 py-1">Type</th>
              <th className="border px-2 py-1">Model</th>
              <th className="border px-2 py-1">Color</th>
              <th className="border px-2 py-1">Status</th>
              <th className="border px-2 py-1">Actions</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((vehicle) => (
              <tr key={vehicle.id}>
                <td className="border px-2 py-1">{vehicle.registrationNumber}</td>
                <td className="border px-2 py-1">{vehicle.type}</td>
                <td className="border px-2 py-1">{vehicle.model}</td>
                <td className="border px-2 py-1">{vehicle.color}</td>
                <td className="border px-2 py-1">{vehicle.status}</td>
                <td className="border px-2 py-1 flex gap-2">
                  <button onClick={() => handleEdit(vehicle)} className="text-blue-600 hover:text-blue-800">
                    <Pencil className="w-4 h-4" />
                  </button>
                  <button onClick={() => handleDelete(vehicle.id)} className="text-red-600 hover:text-red-800">
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
