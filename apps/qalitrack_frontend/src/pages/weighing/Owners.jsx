import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, UserPlus, Search, X, Car } from "lucide-react";
import {
  getOwners,
  createOwner,
  updateOwner,
  deleteOwner,
  getOwnerVehicles,
} from "../../api/MasterData/Owners";

const PAGE_SIZE = 5;
const OWNER_TYPES = { 1: "Individual", 2: "Company", 3: "Government" };

export default function OwnersPortal() {
  const [owners, setOwners] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingOwner, setEditingOwner] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);

  // vehicles modal
  const [vehiclesOwner, setVehiclesOwner] = useState(null);
  const [vehicles, setVehicles] = useState([]);
  const [vehiclesLoading, setVehiclesLoading] = useState(false);

  const [form, setForm] = useState({
    name: "", type: 1, contactPerson: "", phoneNumber: "", email: "", address: "",
  });

  useEffect(() => { fetchOwners(); }, []);

  const fetchOwners = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getOwners(1, 200, "");
      setOwners(data?.items || data || []);
    } catch (err) {
      setError(err.message || "Failed to load owners");
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return owners;
    return owners.filter(
      (o) =>
        o.name?.toLowerCase().includes(t) ||
        o.phoneNumber?.toLowerCase().includes(t) ||
        o.email?.toLowerCase().includes(t)
    );
  }, [owners, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (editingOwner) await updateOwner(editingOwner.id, form);
      else await createOwner(form);
      resetForm();
      await fetchOwners();
    } catch (err) { alert(err.message); }
    finally { setLoading(false); }
  };

  const handleEdit = (owner) => {
    setForm({
      name: owner.name || "", type: owner.type || 1, contactPerson: owner.contactPerson || "",
      phoneNumber: owner.phoneNumber || "", email: owner.email || "", address: owner.address || "",
    });
    setEditingOwner(owner);
  };

  const handleDelete = async (id) => {
    if (!confirm("Delete this owner?")) return;
    await deleteOwner(id);
    await fetchOwners();
  };

  const handleViewVehicles = async (owner) => {
    setVehiclesOwner(owner);
    setVehiclesLoading(true);
    try {
      const data = await getOwnerVehicles(owner.id);
      setVehicles(data?.data || data || []);
    } catch { setVehicles([]); }
    finally { setVehiclesLoading(false); }
  };

  const resetForm = () => {
    setForm({ name: "", type: 1, contactPerson: "", phoneNumber: "", email: "", address: "" });
    setEditingOwner(null);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <UserPlus className="w-4 h-4 text-white" />
            </div>
            <div>
              <span className="text-[11px] font-bold text-gray-900 block leading-tight">
                Owners
              </span>
              <span className="text-[9px] text-amber-700 font-medium">
                {filtered.length} registered owners
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search owners..."
                className="w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <button
              onClick={() => {
                setSearch("");
                fetchOwners();
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
        <form onSubmit={handleSubmit} className="grid grid-cols-3 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Owner Name *
            </label>
            <input
              name="name"
              value={form.name}
              onChange={handleChange}
              required
              placeholder="Owner name"
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Contact Person
            </label>
            <input
              name="contactPerson"
              value={form.contactPerson}
              onChange={handleChange}
              placeholder="Contact person"
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Phone Number
            </label>
            <input
              name="phoneNumber"
              value={form.phoneNumber}
              onChange={handleChange}
              placeholder="+254 7XX XXX XXX"
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Email
            </label>
            <input
              name="email"
              value={form.email}
              onChange={handleChange}
              placeholder="email@example.com"
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Address
            </label>
            <input
              name="address"
              value={form.address}
              onChange={handleChange}
              placeholder="Full address"
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Owner Type
            </label>
            <select
              name="type"
              value={form.type}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            >
              {Object.entries(OWNER_TYPES).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
          </div>

          <div className="col-span-3 flex gap-2 justify-end mt-1">
            {editingOwner && (
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
              <UserPlus className="w-3 h-3" />
              {editingOwner ? "Update" : "Add"} Owner
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading...</p>
          </div>
        ) : error ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-red-500 text-sm">Error: {error}</p>
          </div>
        ) : paginated.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">No owners found.</p>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Type</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Contact Person</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Phone</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Email</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginated.map((o, i) => (
                <tr
                  key={o.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all ${
                    i % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">
                    {(page - 1) * PAGE_SIZE + i + 1}
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">{o.name}</td>
                  <td className="px-3 py-2">
                    <span
                      className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase shadow-sm border ${
                        o.type === 1 ? "bg-blue-100 text-blue-700 border-blue-300" :
                        o.type === 2 ? "bg-purple-100 text-purple-700 border-purple-300" :
                        "bg-green-100 text-green-700 border-green-300"
                      }`}
                    >
                      {OWNER_TYPES[o.type] || "—"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{o.contactPerson || "—"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 font-medium">{o.phoneNumber || "—"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{o.email || "—"}</td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        onClick={() => handleViewVehicles(o)}
                        className="p-1 rounded text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all"
                        title="View Vehicles"
                      >
                        <Car className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleEdit(o)}
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(o.id)}
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

      {/* Footer with Pagination */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <span className="text-[10px] text-gray-600 font-medium">
          Page <span className="font-semibold text-amber-600">{page}</span> of{" "}
          <span className="font-semibold text-amber-600">{totalPages}</span>
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Previous
          </button>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Next
          </button>
        </div>
      </div>

      {/* Vehicles Modal */}
      {vehiclesOwner && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
          <div className="bg-white rounded-lg shadow-2xl w-full max-w-md border-2 border-amber-300">
            <div className="bg-gradient-to-r from-amber-500 to-orange-600 px-4 py-2.5 rounded-t-lg flex items-center justify-between">
              <h3 className="text-sm font-bold text-white flex items-center gap-2">
                <Car className="w-4 h-4" /> Vehicles – {vehiclesOwner.name}
              </h3>
              <button
                onClick={() => setVehiclesOwner(null)}
                className="text-white hover:bg-white/20 rounded-lg p-1 transition-all"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
            <div className="p-4">
              {vehiclesLoading ? (
                <p className="text-gray-500 text-sm text-center py-4">Loading vehicles...</p>
              ) : vehicles.length === 0 ? (
                <p className="text-gray-500 text-sm text-center py-4">No vehicles registered for this owner.</p>
              ) : (
                <div className="overflow-auto max-h-64">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b-2 border-amber-200">
                        <th className="text-left py-2 text-[10px] font-bold text-amber-700 uppercase">Registration</th>
                        <th className="text-left py-2 text-[10px] font-bold text-amber-700 uppercase">Make / Model</th>
                      </tr>
                    </thead>
                    <tbody>
                      {vehicles.map((v) => (
                        <tr key={v.id} className="border-b border-gray-100 hover:bg-amber-50 transition-all">
                          <td className="py-2">
                            <div className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[10px] font-bold tracking-wider">
                              {v.registrationNumber}
                            </div>
                          </td>
                          <td className="py-2 text-[10px] text-gray-700">{v.make} {v.model}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
              <div className="mt-4 flex justify-end">
                <button
                  onClick={() => setVehiclesOwner(null)}
                  className="h-7 px-4 text-[11px] font-semibold border border-gray-300 rounded hover:bg-gray-100 transition-all"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

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