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

  // ── fetch (pull all, paginate client-side) ──
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

  // ── derived ──
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

  // ── handlers ──
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

  // ── UI ──
  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <UserPlus className="w-4 h-4 text-black" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Owners</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1"><span className="font-semibold">{filtered.length}</span> registered</div>
          </div>
        </div>
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search owners..."
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500"
              value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
        </div>
      </div>

      {/* Form */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-emerald-50/20 border-b-2 border-amber-200 px-4 py-3 shrink-0 shadow-inner">
        <div className="bg-white rounded-lg p-4 border-2 border-amber-300 shadow-md">
          <h3 className="text-sm font-bold text-amber-900 mb-3 flex items-center gap-2">
            <UserPlus className="w-4 h-4" />{editingOwner ? "Edit Owner" : "Add New Owner"}
          </h3>
          <form onSubmit={handleSubmit} className="grid grid-cols-3 gap-3">
            {[
              { label: "Owner Name *", name: "name", dot: "#3b82f6", border: "#93c5fd", ph: "Owner name", req: true },
              { label: "Contact Person", name: "contactPerson", dot: "#a855f7", border: "#c4b5fd", ph: "Contact person" },
              { label: "Phone Number", name: "phoneNumber", dot: "#ec4899", border: "#f9a8d4", ph: "+254 7XX XXX XXX" },
              { label: "Email", name: "email", dot: "#06b6d4", border: "#67e8f9", ph: "email@example.com" },
              { label: "Address", name: "address", dot: "#f43f5e", border: "#fda4af", ph: "Full address" },
            ].map(f => (
              <div key={f.name}>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                  <span className="w-1.5 h-1.5 rounded-full" style={{ background: f.dot }}></span>{f.label}
                </label>
                <input name={f.name} value={form[f.name]} onChange={handleChange} required={f.req} placeholder={f.ph}
                  className="w-full h-7 text-[11px] rounded border-2 px-2 focus:outline-none transition-all"
                  style={{ borderColor: f.border }} />
              </div>
            ))}

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>Owner Type
              </label>
              <select name="type" value={form.type} onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border-2 border-emerald-300 px-2 focus:border-emerald-500 focus:outline-none">
                {Object.entries(OWNER_TYPES).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>

            <div className="col-span-3 flex gap-2 justify-end mt-1">
              {editingOwner && (
                <button type="button" onClick={resetForm} className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5">
                  <X className="w-3.5 h-3.5" /> Cancel
                </button>
              )}
              <button type="submit" disabled={loading} className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5">
                <UserPlus className="w-3.5 h-3.5" />{editingOwner ? "Update Owner" : "Add Owner"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden flex flex-col">
          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading...</p></div>
          ) : error ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-red-500 text-sm">Error: {error}</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">No owners found.</p></div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      {["Name","Type","Contact Person","Phone","Email","Actions"].map(h => (
                        <th key={h} className={`px-4 py-2.5 text-[10px] font-bold text-amber-400 uppercase tracking-wide ${h === "Actions" ? "text-center" : "text-left"}`}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {paginated.map((o, i) => (
                      <tr key={o.id} className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${i % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                        <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{o.name}</td>
                        <td className="px-4 py-2.5">
                          <span className={`px-2.5 py-0.5 rounded-full text-[9px] font-bold uppercase shadow-sm border ${
                            o.type === 1 ? "bg-blue-100 text-blue-700 border-blue-300" :
                            o.type === 2 ? "bg-purple-100 text-purple-700 border-purple-300" :
                            "bg-emerald-100 text-emerald-700 border-emerald-300"
                          }`}>{OWNER_TYPES[o.type] || "—"}</span>
                        </td>
                        <td className="px-4 py-2.5 text-[10px] text-purple-600">{o.contactPerson || "—"}</td>
                        <td className="px-4 py-2.5 text-[10px] text-pink-600 font-medium">{o.phoneNumber || "—"}</td>
                        <td className="px-4 py-2.5 text-[10px] text-cyan-600">{o.email || "—"}</td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-2 justify-center">
                            <button onClick={() => handleViewVehicles(o)} className="p-1.5 rounded-lg text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all" title="View Vehicles"><Car className="w-3.5 h-3.5" /></button>
                            <button onClick={() => handleEdit(o)} className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all" title="Edit"><Pencil className="w-3.5 h-3.5" /></button>
                            <button onClick={() => handleDelete(o.id)} className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete"><Trash2 className="w-3.5 h-3.5" /></button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              <div className="border-t-2 border-amber-200 bg-gradient-to-r from-gray-50 to-amber-50/30 px-4 py-2 flex justify-between items-center shrink-0">
                <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all">Previous</button>
                <div className="flex items-center gap-1.5">
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map(n => (
                    <button key={n} onClick={() => setPage(n)} className={`w-7 h-7 text-[11px] font-semibold rounded-lg transition-all ${n === page ? "bg-gradient-to-br from-amber-500 to-amber-600 text-black shadow-md" : "border border-gray-300 hover:bg-gray-100"}`}>{n}</button>
                  ))}
                </div>
                <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all">Next</button>
              </div>
            </>
          )}
        </div>
      </div>

      {/* Vehicles Modal */}
      {vehiclesOwner && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md border-2 border-amber-300">
            <div className="bg-gradient-to-r from-amber-500 to-amber-600 px-5 py-3 rounded-t-xl flex items-center justify-between">
              <h3 className="text-sm font-bold text-black flex items-center gap-2"><Car className="w-4 h-4" /> Vehicles – {vehiclesOwner.name}</h3>
              <button onClick={() => setVehiclesOwner(null)} className="text-black hover:bg-black/10 rounded-lg p-1 transition-all"><X className="w-4 h-4" /></button>
            </div>
            <div className="p-5">
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
                            <div className="inline-block bg-gradient-to-r from-gray-800 to-black text-amber-400 px-2 py-0.5 rounded text-[10px] font-bold tracking-wider shadow-sm">{v.registrationNumber}</div>
                          </td>
                          <td className="py-2 text-[10px] text-gray-700">{v.make} {v.model}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
              <div className="mt-4 flex justify-end">
                <button onClick={() => setVehiclesOwner(null)} className="h-7 px-4 text-[11px] font-semibold border-2 border-gray-300 rounded-lg hover:bg-gray-100 transition-all">Close</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}