import { useEffect, useState } from "react";
import { Pencil, Trash2, PackagePlus, Search } from "lucide-react";
import {
  getWeighbridges,
  createWeighbridge,
  updateWeighbridge,
  deleteWeighbridge,
  checkWeighbridgeLocation,
} from "../../api/MasterData/WeighingBridge";

export default function WeighbridgesPortal() {
  const [weighbridges, setWeighbridges] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    location: "",
    description: "",
    status: "Active",
  });

  // ──────────────── FETCH WEIGHBRIDGES ────────────────
  const fetchWeighbridges = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getWeighbridges(page, pageSize, search);
      console.log("Fetched weighbridges:", data);

      setWeighbridges(data.items || []);
      setTotalPages(data.totalPages || 1);
    } catch (err) {
      console.error("❌ Failed to load weighbridges:", err);
      setError(err.message || "Failed to fetch weighbridges");
      setWeighbridges([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWeighbridges();
  }, [page, search]);

  // ──────────────── FORM HANDLERS ────────────────
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      // Check location availability
      const available = await checkWeighbridgeLocation(form.location, editing?.id);
      if (!available) {
        alert("This location is already taken!");
        setLoading(false);
        return;
      }

      if (editing) {
        await updateWeighbridge(editing.id, form);
      } else {
        await createWeighbridge(form);
      }

      resetForm();
      fetchWeighbridges();
    } catch (err) {
      console.error("❌ Save failed:", err.message);
      alert(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (wb) => {
    setForm({
      location: wb.location || "",
      description: wb.description || "",
      status: wb.status || "Active",
    });
    setEditing(wb);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this weighbridge?")) return;

    setLoading(true);
    try {
      await deleteWeighbridge(id);
      fetchWeighbridges();
    } catch (err) {
      console.error("❌ Delete failed:", err.message);
      alert(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({ location: "", description: "", status: "Active" });
    setEditing(null);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchWeighbridges();
  };

  // ──────────────── RENDER ────────────────
  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <PackagePlus className="w-6 h-6" /> Weighbridge Management
      </h2>

      {/* Search */}
      <form
        onSubmit={handleSearch}
        className="flex items-center gap-3 mb-6 border border-gray-200 rounded-lg px-3 py-2"
      >
        <Search className="w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search by location..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="flex-1 focus:outline-none bg-transparent text-gray-700"
        />
        <button className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-1 rounded-lg font-medium">
          Search
        </button>
      </form>

      {/* Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        <div>
          <label className="block text-sm font-medium mb-1">Location</label>
          <input
            name="location"
            value={form.location}
            onChange={handleChange}
            required
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
          />
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm font-medium mb-1">Description</label>
          <textarea
            name="description"
            value={form.description}
            onChange={handleChange}
            rows={3}
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
            className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-lg font-semibold"
          >
            {editing ? "Update Weighbridge" : "Add Weighbridge"}
          </button>
          {editing && (
            <button
              type="button"
              onClick={resetForm}
              className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-semibold"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {/* Table */}
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        Registered Weighbridges
      </h3>

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : error ? (
        <p className="text-red-500">Error: {error}</p>
      ) : weighbridges.length === 0 ? (
        <p className="text-gray-500 text-sm">No weighbridges found.</p>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm border border-gray-200 rounded-lg">
              <thead className="bg-gray-50">
                <tr>
                  {["Location", "Description", "Status", "Actions"].map((h) => (
                    <th key={h} className="border px-3 py-2 text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {weighbridges.map((wb) => (
                  <tr key={wb.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2">{wb.location}</td>
                    <td className="px-3 py-2">{wb.description}</td>
                    <td className="px-3 py-2">{wb.status}</td>
                    <td className="px-3 py-2 flex gap-2">
                      <button
                        onClick={() => handleEdit(wb)}
                        className="text-blue-600 hover:text-blue-800"
                      >
                        <Pencil className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(wb.id)}
                        className="text-red-600 hover:text-red-800"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="flex justify-between items-center mt-4">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-3 py-1 border rounded-lg disabled:opacity-40"
            >
              Previous
            </button>
            <span className="text-sm text-gray-600">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-3 py-1 border rounded-lg disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
