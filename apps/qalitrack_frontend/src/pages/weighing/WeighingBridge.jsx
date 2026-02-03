import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, Plus, Search, X } from "lucide-react";
import {
  getWeighbridges,
  createWeighbridge,
  updateWeighbridge,
  deleteWeighbridge,
  checkWeighbridgeLocation,
} from "../../api/MasterData/WeighingBridge";

const PAGE_SIZE = 5;

export default function WeighbridgesPortal() {
  const [weighbridges, setWeighbridges] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);

  const [form, setForm] = useState({
    location: "",
    description: "",
    status: "Active",
  });

  // ── fetch (pull all, paginate client-side) ──
  const fetchWeighbridges = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getWeighbridges(1, 200, "");
      setWeighbridges(data?.items || []);
    } catch (err) {
      console.error("❌ Failed to load weighbridges:", err);
      setError(err.message || "Failed to fetch weighbridges");
      setWeighbridges([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchWeighbridges(); }, []);

  // ── derived / pagination ──
  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return weighbridges;
    return weighbridges.filter(
      (wb) =>
        wb.location?.toLowerCase().includes(t) ||
        wb.description?.toLowerCase().includes(t) ||
        wb.status?.toLowerCase().includes(t)
    );
  }, [weighbridges, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  // ── handlers (original logic preserved exactly) ──
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      // location-uniqueness guard — kept exactly as original
      const available = await checkWeighbridgeLocation(form.location, editing?.id);
      if (!available) {
        alert("This location is already taken!");
        setLoading(false);
        return;
      }

      if (editing) await updateWeighbridge(editing.id, form);
      else        await createWeighbridge(form);

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

  // ── render ──
  return (
    <div className="h-screen flex flex-col bg-gray-50">

      {/* ─── Header ─── */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            {/* scale / weighbridge icon */}
            <svg className="w-4 h-4 text-black" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path d="M12 2v20M2 22h20M6 22V12l6-4 6 4v10" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Weighbridges</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{filtered.length}</span> registered
            </div>
          </div>
        </div>

        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search weighbridges..."
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
      </div>

      {/* ─── Inline Form ─── */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-emerald-50/20 border-b-2 border-amber-200 px-4 py-3 shrink-0 shadow-inner">
        <div className="bg-white rounded-lg p-4 border-2 border-amber-300 shadow-md">
          <h3 className="text-sm font-bold text-amber-900 mb-3 flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path d="M12 2v20M2 22h20M6 22V12l6-4 6 4v10" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
            </svg>
            {editing ? "Edit Weighbridge" : "Add New Weighbridge"}
          </h3>

          <form onSubmit={handleSubmit} className="grid grid-cols-3 gap-3">
            {/* Location */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>Location *
              </label>
              <input name="location" value={form.location} onChange={handleChange} required placeholder="e.g., Nairobi Main Gate"
                className="w-full h-7 text-[11px] rounded border border-blue-300 px-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-200 focus:outline-none" />
            </div>

            {/* Status */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>Status
              </label>
              <select name="status" value={form.status} onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-emerald-300 px-2 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-200 focus:outline-none">
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>

            {/* Description — spans remaining col */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-amber-500 rounded-full"></span>Description
              </label>
              <input name="description" value={form.description} onChange={handleChange} placeholder="Brief description"
                className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 focus:outline-none" />
            </div>

            {/* Buttons */}
            <div className="col-span-3 flex gap-2 justify-end mt-1">
              {editing && (
                <button type="button" onClick={resetForm}
                  className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5">
                  <X className="w-3.5 h-3.5" /> Cancel
                </button>
              )}
              <button type="submit" disabled={loading}
                className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5">
                <Plus className="w-3.5 h-3.5" />
                {editing ? "Update Weighbridge" : "Add Weighbridge"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* ─── Table ─── */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden flex flex-col">

          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading weighbridges…</p></div>
          ) : error ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-red-500 text-sm">Error: {error}</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">No weighbridges found.</p></div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      {[
                        { label: "Location",    align: "text-left" },
                        { label: "Description", align: "text-left" },
                        { label: "Status",      align: "text-left" },
                        { label: "Actions",    align: "text-center" },
                      ].map((h) => (
                        <th key={h.label} className={`px-4 py-2.5 text-[10px] font-bold text-amber-400 uppercase tracking-wide ${h.align}`}>{h.label}</th>
                      ))}
                    </tr>
                  </thead>

                  <tbody>
                    {paginated.map((wb, i) => {
                      const isActive = wb.status === "Active";
                      return (
                        <tr key={wb.id} className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${i % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                          {/* Location — styled like a registration plate but in teal */}
                          <td className="px-4 py-2.5">
                            <span className="inline-block bg-gradient-to-r from-teal-800 to-teal-900 text-teal-200 px-2.5 py-0.5 rounded text-[10px] font-bold tracking-wider shadow-sm">
                              {wb.location}
                            </span>
                          </td>

                          {/* Description */}
                          <td className="px-4 py-2.5 text-[10px] text-gray-600 max-w-xs truncate">{wb.description || "—"}</td>

                          {/* Status */}
                          <td className="px-4 py-2.5">
                            <span className={`px-2.5 py-0.5 rounded-full text-[9px] font-bold uppercase shadow-sm border ${
                              isActive
                                ? "bg-gradient-to-r from-emerald-100 to-green-200 text-emerald-700 border-emerald-300"
                                : "bg-gradient-to-r from-red-100 to-rose-200 text-red-700 border-red-300"
                            }`}>
                              {isActive ? "✓ Active" : "✕ Inactive"}
                            </span>
                          </td>

                          {/* Actions */}
                          <td className="px-4 py-2.5">
                            <div className="flex gap-2 justify-center">
                              <button onClick={() => handleEdit(wb)}
                                className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all" title="Edit">
                                <Pencil className="w-3.5 h-3.5" />
                              </button>
                              <button onClick={() => handleDelete(wb.id)}
                                className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete">
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              {/* ─── Pagination ─── */}
              <div className="border-t-2 border-amber-200 bg-gradient-to-r from-gray-50 to-amber-50/30 px-4 py-2 flex justify-between items-center shrink-0">
                <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all">
                  Previous
                </button>
                <div className="flex items-center gap-1.5">
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
                    <button key={n} onClick={() => setPage(n)}
                      className={`w-7 h-7 text-[11px] font-semibold rounded-lg transition-all ${
                        n === page
                          ? "bg-gradient-to-br from-amber-500 to-amber-600 text-black shadow-md"
                          : "border border-gray-300 hover:bg-gray-100"
                      }`}>
                      {n}
                    </button>
                  ))}
                </div>
                <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all">
                  Next
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}