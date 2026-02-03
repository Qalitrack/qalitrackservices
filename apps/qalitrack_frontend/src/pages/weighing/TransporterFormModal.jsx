import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, Plus, Search, X, Truck } from "lucide-react";
import {
  getTransporters,
  createTransporter,
  updateTransporter,
  deleteTransporter,
} from "../../api/MasterData/Transporters";

const PAGE_SIZE = 5;

export default function TransportersPortal() {
  const [transporters, setTransporters] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingTransporter, setEditingTransporter] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);

  const [form, setForm] = useState({
    name: "",
    email: "",
    phone: "",
    address: "",
    licenseNumber: "",
    status: "Active",
    logo: "",
  });

  // ── fetch (pull all, paginate client-side) ──
  useEffect(() => {
    fetchTransporters();
  }, []);

  const fetchTransporters = async () => {
    try {
      setLoading(true);
      setError(null);
      // fetch a large page so all records land in memory
      const data = await getTransporters(1, 200, "");
      const items = data?.items || data || [];
      setTransporters(items);
    } catch (err) {
      setError(err.message || "Failed to load transporters");
    } finally {
      setLoading(false);
    }
  };

  // ── derived / pagination ──
  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return transporters;
    return transporters.filter((tr) => {
      const ci = parseContact(tr.contactInfo);
      return (
        tr.name?.toLowerCase().includes(t) ||
        ci.Phone?.toLowerCase().includes(t) ||
        ci.Email?.toLowerCase().includes(t) ||
        tr.status?.toLowerCase().includes(t)
      );
    });
  }, [transporters, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  // ── helpers ──
  function parseContact(raw) {
    if (!raw) return {};
    if (typeof raw === "object") return raw;
    try { return JSON.parse(raw); }
    catch { return {}; }
  }

  // ── handlers (original logic preserved exactly) ──
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const payload = {
        name: form.name,
        status: form.status,
        logo: form.logo || "",
        contactInfo: JSON.stringify({
          Email: form.email || "",
          Phone: form.phone || "",
          Address: form.address || "",
          LicenseNumber: form.licenseNumber || "",
        }),
      };
      if (editingTransporter) await updateTransporter(editingTransporter.id, payload);
      else await createTransporter(payload);
      resetForm();
      fetchTransporters();
    } catch (err) {
      alert(err.message || "Failed to save transporter");
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (t) => {
    const ci = parseContact(t.contactInfo);
    setForm({
      name: t.name || "",
      email: ci.Email || "",
      phone: ci.Phone || "",
      address: ci.Address || "",
      licenseNumber: ci.LicenseNumber || "",
      status: t.status || "Active",
      logo: t.logo || "",
    });
    setEditingTransporter(t);
  };

  const handleDelete = async (id) => {
    if (!confirm("Delete this transporter?")) return;
    await deleteTransporter(id);
    fetchTransporters();
  };

  const resetForm = () => {
    setForm({ name: "", email: "", phone: "", address: "", licenseNumber: "", status: "Active", logo: "" });
    setEditingTransporter(null);
  };

  // ── status badge colour map ──
  const statusStyle = (s) => {
    if (s === "Active")    return "bg-gradient-to-r from-emerald-100 to-green-200   text-emerald-700  border-emerald-300";
    if (s === "Suspended") return "bg-gradient-to-r from-amber-100   to-orange-200  text-amber-700    border-amber-300";
    /* Inactive */          return "bg-gradient-to-r from-red-100     to-rose-200    text-red-700      border-red-300";
  };
  const statusIcon = (s) => (s === "Active" ? "✓" : s === "Suspended" ? "⚠" : "✕");

  // ── render ──
  return (
    <div className="h-screen flex flex-col bg-gray-50">

      {/* ─── Header ─── */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <Truck className="w-4 h-4 text-black" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Transporters</div>
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
              placeholder="Search transporters..."
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
            <Truck className="w-4 h-4" />
            {editingTransporter ? "Edit Transporter" : "Add New Transporter"}
          </h3>

          <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-3">
            {/* Name */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>Name *
              </label>
              <input name="name" value={form.name} onChange={handleChange} required placeholder="Transporter name"
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
                <option value="Suspended">Suspended</option>
              </select>
            </div>

            {/* Email */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-cyan-500 rounded-full"></span>Email
              </label>
              <input name="email" value={form.email} onChange={handleChange} placeholder="email@example.com"
                className="w-full h-7 text-[11px] rounded border border-cyan-300 px-2 focus:border-cyan-500 focus:ring-1 focus:ring-cyan-200 focus:outline-none" />
            </div>

            {/* Phone */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-pink-500 rounded-full"></span>Phone
              </label>
              <input name="phone" value={form.phone} onChange={handleChange} placeholder="+254 7XX XXX XXX"
                className="w-full h-7 text-[11px] rounded border border-pink-300 px-2 focus:border-pink-500 focus:ring-1 focus:ring-pink-200 focus:outline-none" />
            </div>

            {/* License Number */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full"></span>License Number
              </label>
              <input name="licenseNumber" value={form.licenseNumber} onChange={handleChange} placeholder="License no."
                className="w-full h-7 text-[11px] rounded border border-purple-300 px-2 focus:border-purple-500 focus:ring-1 focus:ring-purple-200 focus:outline-none" />
            </div>

            {/* Logo URL */}
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-indigo-500 rounded-full"></span>Logo URL
              </label>
              <input name="logo" value={form.logo} onChange={handleChange} placeholder="https://…/logo.png"
                className="w-full h-7 text-[11px] rounded border border-indigo-300 px-2 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-200 focus:outline-none" />
            </div>

            {/* Address (spans 2 cols) */}
            <div className="col-span-2">
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>Address
              </label>
              <input name="address" value={form.address} onChange={handleChange} placeholder="Full address"
                className="w-full h-7 text-[11px] rounded border border-rose-300 px-2 focus:border-rose-500 focus:ring-1 focus:ring-rose-200 focus:outline-none" />
            </div>

            {/* Buttons */}
            <div className="col-span-4 flex gap-2 justify-end mt-1">
              {editingTransporter && (
                <button type="button" onClick={resetForm}
                  className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5">
                  <X className="w-3.5 h-3.5" /> Cancel
                </button>
              )}
              <button type="submit" disabled={loading}
                className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5">
                <Plus className="w-3.5 h-3.5" />
                {editingTransporter ? "Update Transporter" : "Add Transporter"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* ─── Table ─── */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden flex flex-col">

          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading transporters…</p></div>
          ) : error ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-red-500 text-sm">Error: {error}</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">No transporters found.</p></div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      {[
                        { label: "Name",        align: "text-left" },
                        { label: "Phone",       align: "text-left" },
                        { label: "Email",       align: "text-left" },
                        { label: "License",     align: "text-left" },
                        { label: "Status",      align: "text-left" },
                        { label: "Actions",    align: "text-center" },
                      ].map((h) => (
                        <th key={h.label} className={`px-4 py-2.5 text-[10px] font-bold text-amber-400 uppercase tracking-wide ${h.align}`}>{h.label}</th>
                      ))}
                    </tr>
                  </thead>

                  <tbody>
                    {paginated.map((t, i) => {
                      const ci = parseContact(t.contactInfo);
                      return (
                        <tr key={t.id} className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${i % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                          {/* Name */}
                          <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{t.name}</td>

                          {/* Phone */}
                          <td className="px-4 py-2.5 text-[10px] text-pink-600 font-medium">{ci.Phone || "—"}</td>

                          {/* Email */}
                          <td className="px-4 py-2.5 text-[10px] text-cyan-600">{ci.Email || "—"}</td>

                          {/* License */}
                          <td className="px-4 py-2.5">
                            {ci.LicenseNumber ? (
                              <span className="inline-block bg-purple-100 text-purple-700 border border-purple-300 px-2.5 py-0.5 rounded-full text-[9px] font-bold font-mono tracking-wide">
                                {ci.LicenseNumber}
                              </span>
                            ) : <span className="text-[10px] text-gray-400">—</span>}
                          </td>

                          {/* Status */}
                          <td className="px-4 py-2.5">
                            <span className={`px-2.5 py-0.5 rounded-full text-[9px] font-bold uppercase shadow-sm border ${statusStyle(t.status)}`}>
                              {statusIcon(t.status)} {t.status}
                            </span>
                          </td>

                          {/* Actions */}
                          <td className="px-4 py-2.5">
                            <div className="flex gap-2 justify-center">
                              <button onClick={() => handleEdit(t)}
                                className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all" title="Edit">
                                <Pencil className="w-3.5 h-3.5" />
                              </button>
                              <button onClick={() => handleDelete(t.id)}
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