import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, Plus, Search, X } from "lucide-react";
import { message } from "antd";
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
    scales: [],
  });
  const [scaleInput, setScaleInput] = useState("");

  const fetchWeighbridges = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getWeighbridges(1, 200, "");
      setWeighbridges(Array.isArray(data) ? data : (data?.items || []));
    } catch (err) {
      setError(err.message || "Failed to fetch weighbridges");
      setWeighbridges([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchWeighbridges(); }, []);

  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return weighbridges;
    return weighbridges.filter(
      (wb) =>
        wb.location?.toLowerCase().includes(t) ||
        wb.description?.toLowerCase().includes(t) ||
        wb.status?.toLowerCase().includes(t) ||
        (Array.isArray(wb.scales) && wb.scales.some((s) => s.toLowerCase().includes(t)))
    );
  }, [weighbridges, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const available = await checkWeighbridgeLocation(form.location, editing?.id);
      if (!available) {
        message.warning("This location is already taken!");
        setLoading(false);
        return;
      }

      if (editing) await updateWeighbridge(editing.id, form);
      else        await createWeighbridge(form);

      resetForm();
      fetchWeighbridges();
    } catch (err) {
      message.error(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (wb) => {
    setForm({
      location: wb.location || "",
      description: wb.description || "",
      status: wb.status ? (wb.status.charAt(0).toUpperCase() + wb.status.slice(1).toLowerCase()) : "Active",
      scales: Array.isArray(wb.scales) ? wb.scales : [],
    });
    setScaleInput("");
    setEditing(wb);
  };

  const addScale = () => {
    const name = scaleInput.trim();
    if (!name) return;
    if (form.scales.includes(name)) { message.warning("Scale already added"); return; }
    setForm((prev) => ({ ...prev, scales: [...prev.scales, name] }));
    setScaleInput("");
  };

  const removeScale = (name) => {
    setForm((prev) => ({ ...prev, scales: prev.scales.filter((s) => s !== name) }));
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this weighbridge?")) return;
    setLoading(true);
    try {
      await deleteWeighbridge(id);
      fetchWeighbridges();
    } catch (err) {
      message.error(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async (wb) => {
    const newStatus = wb.status?.toLowerCase() === "active" ? "Inactive" : "Active";
    try {
      await updateWeighbridge(wb.id, { ...wb, status: newStatus });
      fetchWeighbridges();
    } catch (err) {
      message.error(`Failed to update status: ${err.message}`);
    }
  };

  const resetForm = () => {
    setForm({ location: "", description: "", status: "Active", scales: [] });
    setScaleInput("");
    setEditing(null);
  };

  return (
    <div className="h-screen flex flex-col bg-white">

      {/* Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 shrink-0">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path d="M12 2v20M2 22h20M6 22V12l6-4 6 4v10" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
              </svg>
            </div>
            <div>
              <div className="text-[11px] font-bold text-gray-900 leading-tight">Weighbridges</div>
              <div className="text-[9px] text-amber-700 font-medium leading-tight">
                <span className="font-semibold">{filtered.length}</span> registered
              </div>
            </div>
          </div>

          <div className="flex gap-2 items-center">
            <div className="relative">
              <Search size={10} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search weighbridges..."
                className="w-52 h-7 pl-7 pr-3 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm focus:outline-none focus:ring-1 focus:ring-amber-500"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Inline Form */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-orange-50/20 border-b border-amber-200 px-3 py-2.5 shrink-0">
        <div className="bg-white rounded-lg p-3 border border-amber-300 shadow-sm">
          <h3 className="text-xs font-bold text-amber-900 mb-2 flex items-center gap-1.5">
            <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path d="M12 2v20M2 22h20M6 22V12l6-4 6 4v10" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
            </svg>
            {editing ? "Edit Weighbridge" : "Add New Weighbridge"}
          </h3>

          <form onSubmit={handleSubmit} className="space-y-2">
            {/* Row 1: Location, Status, Description */}
            <div className="grid grid-cols-3 gap-2">
              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                  <span className="w-1 h-1 bg-blue-500 rounded-full"></span>Location *
                </label>
                <input name="location" value={form.location} onChange={handleChange} required placeholder="e.g., Nairobi Main Gate"
                  className="w-full h-6 text-[10px] rounded border border-blue-300 px-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-200 focus:outline-none" />
              </div>

              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                  <span className="w-1 h-1 bg-emerald-500 rounded-full"></span>Status
                </label>
                <select name="status" value={form.status} onChange={handleChange}
                  className="w-full h-6 text-[10px] rounded border border-emerald-300 px-2 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-200 focus:outline-none">
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>

              <div>
                <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                  <span className="w-1 h-1 bg-amber-500 rounded-full"></span>Description
                </label>
                <input name="description" value={form.description} onChange={handleChange} placeholder="Brief description"
                  className="w-full h-6 text-[10px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 focus:outline-none" />
              </div>
            </div>

            {/* Row 2: Scales */}
            <div>
              <label className="text-[9px] font-semibold text-gray-700 mb-0.5 block flex items-center gap-1">
                <span className="w-1 h-1 bg-violet-500 rounded-full"></span>Scales
                <span className="text-[8px] font-normal text-gray-400 ml-1">(one weighbridge can have multiple scales)</span>
              </label>
              <div className="flex gap-1.5 items-center">
                <input
                  value={scaleInput}
                  onChange={(e) => setScaleInput(e.target.value)}
                  onKeyDown={(e) => { if (e.key === "Enter") { e.preventDefault(); addScale(); } }}
                  placeholder="e.g., Scale A, Platform 1..."
                  className="flex-1 h-6 text-[10px] rounded border border-violet-300 px-2 focus:border-violet-500 focus:ring-1 focus:ring-violet-200 focus:outline-none"
                />
                <button type="button" onClick={addScale}
                  className="h-6 px-2 text-[10px] font-semibold bg-violet-100 hover:bg-violet-200 text-violet-700 border border-violet-300 rounded transition-all flex items-center gap-1">
                  <Plus className="w-3 h-3" /> Add
                </button>
              </div>
              {form.scales.length > 0 && (
                <div className="flex flex-wrap gap-1 mt-1">
                  {form.scales.map((s) => (
                    <span key={s} className="inline-flex items-center gap-1 bg-violet-100 text-violet-800 text-[9px] font-semibold px-2 py-0.5 rounded-full border border-violet-200">
                      {s}
                      <button type="button" onClick={() => removeScale(s)} className="text-violet-500 hover:text-red-500 transition-colors">
                        <X className="w-2.5 h-2.5" />
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            {/* Actions */}
            <div className="flex gap-2 justify-end pt-0.5">
              {editing && (
                <button type="button" onClick={resetForm}
                  className="h-6 px-3 text-[10px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1">
                  <X className="w-3 h-3" /> Cancel
                </button>
              )}
              <button type="submit" disabled={loading}
                className="h-6 px-3 text-[10px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 text-white rounded shadow-sm transition-all flex items-center gap-1">
                <Plus className="w-3 h-3" />
                {editing ? "Update Weighbridge" : "Add Weighbridge"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden">
        <div className="h-full bg-white overflow-hidden flex flex-col">

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
                  <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50/80 border-b-[1.5px] border-amber-500">
                    <tr>
                      {[
                        { label: "Location", align: "text-left" },
                        { label: "Description", align: "text-left" },
                        { label: "Scales", align: "text-left" },
                        { label: "Status", align: "text-left" },
                        { label: "Actions", align: "text-center" },
                      ].map((h) => (
                        <th key={h.label} className={`px-4 py-2 text-[9px] font-bold text-amber-900 uppercase tracking-wide ${h.align}`}>{h.label}</th>
                      ))}
                    </tr>
                  </thead>

                  <tbody>
                    {paginated.map((wb, i) => {
                      const isActive = wb.status?.toLowerCase() === "active";
                      return (
                        <tr key={wb.id} className={`border-b border-gray-100 transition-all ${
                          isActive 
                            ? 'hover:bg-emerald-50/50 bg-emerald-50/20' 
                            : 'hover:bg-amber-50'
                        }`}>
                          <td className="px-4 py-2.5">
                            <span className="inline-block bg-gradient-to-r from-teal-800 to-teal-900 text-teal-200 px-2.5 py-0.5 rounded text-[10px] font-bold tracking-wider shadow-sm">
                              {wb.location}
                            </span>
                          </td>
                          <td className="px-4 py-2.5 text-[10px] text-gray-600 max-w-xs truncate">{wb.description || "—"}</td>
                          <td className="px-4 py-2.5">
                            {Array.isArray(wb.scales) && wb.scales.length > 0 ? (
                              <div className="flex flex-wrap gap-1">
                                {wb.scales.map((s) => (
                                  <span key={s} className="bg-violet-100 text-violet-800 text-[9px] font-semibold px-1.5 py-0.5 rounded-full border border-violet-200">{s}</span>
                                ))}
                              </div>
                            ) : (
                              <span className="text-gray-400 text-[10px]">—</span>
                            )}
                          </td>
                          <td className="px-4 py-2.5">
                            <span className={`px-2.5 py-0.5 rounded-full text-[9px] font-bold uppercase shadow-sm border ${
                              isActive
                                ? "bg-gradient-to-r from-emerald-100 to-green-200 text-emerald-700 border-emerald-300"
                                : "bg-gradient-to-r from-red-100 to-rose-200 text-red-700 border-red-300"
                            }`}>
                              {isActive ? "✓ Active" : "✕ Inactive"}
                            </span>
                          </td>
                          <td className="px-4 py-2.5">
                            <div className="flex gap-2 justify-center">
                              <button
                                onClick={() => handleToggleStatus(wb)}
                                className={`p-1.5 rounded-lg border text-[9px] font-bold transition-all ${
                                  isActive
                                    ? "text-green-700 border-green-300 hover:bg-green-50"
                                    : "text-red-700 border-red-300 hover:bg-red-50"
                                }`}
                                title={isActive ? "Set Inactive" : "Set Active"}
                              >
                                {isActive ? "✓" : "✕"}
                              </button>
                              <button onClick={() => handleEdit(wb)}
                                className="p-1.5 rounded-lg text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all" title="Edit">
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

              {/* Pagination */}
              <div className="border-t border-gray-200 bg-gray-50 px-3 py-2 flex justify-between items-center shrink-0">
                <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
                  className="h-6 px-3 text-[11px] font-medium border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:border-amber-500 hover:text-amber-600 transition-all">
                  Previous
                </button>
                <div className="flex items-center gap-1">
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
                    <button key={n} onClick={() => setPage(n)}
                      className={`w-6 h-6 text-[11px] font-semibold rounded transition-all ${
                        n === page
                          ? "bg-gradient-to-br from-amber-500 to-orange-600 text-white shadow-sm"
                          : "border border-gray-300 hover:border-amber-500 hover:text-amber-600"
                      }`}>
                      {n}
                    </button>
                  ))}
                </div>
                <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}
                  className="h-6 px-3 text-[11px] font-medium border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:border-amber-500 hover:text-amber-600 transition-all">
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