import React, { useEffect, useState, useMemo } from "react";
import { Plus, Trash2, Pencil, Search, X, Building2 } from "lucide-react";
import { getSaccos, createSacco, deleteSacco } from "../api/MasterData/Saccos";

const PAGE_SIZE = 5;

const SaccosPortal = () => {
  const [saccos, setSaccos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [showAddModal, setShowAddModal] = useState(false);
  const [newSacco, setNewSacco] = useState({
    name: "",
    registrationNumber: "",
    members: "",
    otherDetails: "",
  });

  useEffect(() => { fetchSaccos(); }, []);

  const fetchSaccos = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await getSaccos();
      setSaccos(data);
    } catch (err) {
      console.error("Error fetching saccos:", err);
      setError("Failed to load saccos. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  // ── filtered + paginated ──
  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return saccos;
    return saccos.filter(
      (s) =>
        s.name?.toLowerCase().includes(t) ||
        s.contactInfo?.RegistrationNumber?.toLowerCase().includes(t)
    );
  }, [saccos, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  // ── handlers ──
  const handleAddSacco = async () => {
    if (!newSacco.name || !newSacco.registrationNumber) return;
    try {
      const data = await createSacco({
        name: newSacco.name,
        contactInfo: {
          RegistrationNumber: newSacco.registrationNumber,
          Members: newSacco.members.split(",").map((m) => m.trim()),
        },
        otherDetails: newSacco.otherDetails ? JSON.parse(newSacco.otherDetails) : {},
      });
      setSaccos((prev) => [...prev, data]);
      setShowAddModal(false);
      setNewSacco({ name: "", registrationNumber: "", members: "", otherDetails: "" });
    } catch (err) {
      console.error("Error adding sacco:", err);
      alert("Failed to add sacco. Check console for details.");
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this sacco?")) return;
    try {
      await deleteSacco(id);
      setSaccos((prev) => prev.filter((s) => s.id !== id));
    } catch (err) {
      console.error("Error deleting sacco:", err);
      alert("Failed to delete sacco. Check console for details.");
    }
  };

  if (loading) return <div className="flex items-center justify-center h-screen"><p className="text-gray-500">Loading saccos...</p></div>;
  if (error)   return <div className="flex items-center justify-center h-screen"><p className="text-red-600">{error}</p></div>;

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <Building2 className="w-4 h-4 text-black font-bold" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Saccos</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{filtered.length}</span> organizations
            </div>
          </div>
        </div>
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search saccos..."
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500"
              value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          <button onClick={() => setShowAddModal(true)}
            className="h-8 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded shadow-lg transition-all flex items-center gap-1.5">
            <Plus className="w-3.5 h-3.5" /> Add Sacco
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden flex flex-col">
          {paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">No saccos found.</p></div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      {["Name","Registration Number","Members","Other Details","Actions"].map(h => (
                        <th key={h} className={`px-4 py-2.5 text-[10px] font-bold text-amber-400 uppercase tracking-wide ${h === "Actions" ? "text-center" : "text-left"}`}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {paginated.map((sacco, index) => (
                      <tr key={sacco.id} className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${index % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                        <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{sacco.name || "N/A"}</td>
                        <td className="px-4 py-2.5 text-[10px] text-purple-600 font-mono font-semibold">{sacco.contactInfo?.RegistrationNumber || "N/A"}</td>
                        <td className="px-4 py-2.5 text-[10px] text-emerald-700">{sacco.contactInfo?.Members?.length ? sacco.contactInfo.Members.join(", ") : "N/A"}</td>
                        <td className="px-4 py-2.5 text-[10px] text-gray-600 max-w-xs truncate">{sacco.otherDetails ? JSON.stringify(sacco.otherDetails) : "N/A"}</td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-2 justify-center">
                            <button className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all" title="Edit"><Pencil className="w-3.5 h-3.5" /></button>
                            <button onClick={() => handleDelete(sacco.id)} className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete"><Trash2 className="w-3.5 h-3.5" /></button>
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

      {/* Add Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md border-2 border-amber-300">
            <div className="bg-gradient-to-r from-amber-500 to-amber-600 px-5 py-3 rounded-t-xl flex items-center justify-between">
              <h3 className="text-sm font-bold text-black flex items-center gap-2"><Building2 className="w-4 h-4" /> Add New Sacco</h3>
              <button onClick={() => setShowAddModal(false)} className="text-black hover:bg-black/10 rounded-lg p-1 transition-all"><X className="w-4 h-4" /></button>
            </div>
            <div className="p-5 flex flex-col gap-3">
              {[
                { label: "Sacco Name *", key: "name", color: "blue", ph: "Enter sacco name" },
                { label: "Registration Number *", key: "registrationNumber", color: "purple", ph: "Enter reg. number" },
                { label: "Members (comma separated)", key: "members", color: "emerald", ph: "John Doe, Jane Smith" },
              ].map(f => (
                <div key={f.key}>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                    <span className={`w-1.5 h-1.5 rounded-full`} style={{ background: f.color === "blue" ? "#3b82f6" : f.color === "purple" ? "#a855f7" : "#10b981" }}></span>{f.label}
                  </label>
                  <input type="text" placeholder={f.ph} value={newSacco[f.key]} onChange={(e) => setNewSacco({ ...newSacco, [f.key]: e.target.value })}
                    className="w-full h-8 text-[11px] border-2 rounded-lg px-3 transition-all focus:outline-none"
                    style={{ borderColor: f.color === "blue" ? "#93c5fd" : f.color === "purple" ? "#c4b5fd" : "#6ee7b7" }} />
                </div>
              ))}
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                  <span className="w-1.5 h-1.5 bg-amber-500 rounded-full"></span>Other Details (optional)
                </label>
                <textarea placeholder='{"key": "value"}' value={newSacco.otherDetails} onChange={(e) => setNewSacco({ ...newSacco, otherDetails: e.target.value })}
                  className="w-full text-[11px] border-2 border-amber-300 rounded-lg px-3 py-2 focus:border-amber-500 focus:ring-2 focus:ring-amber-200 resize-none transition-all focus:outline-none" rows={3} />
              </div>
              <div className="flex gap-2 mt-2">
                <button onClick={() => setShowAddModal(false)} className="flex-1 h-8 text-[11px] font-semibold border-2 border-gray-300 rounded-lg hover:bg-gray-100 transition-all">Cancel</button>
                <button onClick={handleAddSacco} className="flex-1 h-8 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all">Add Sacco</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SaccosPortal;