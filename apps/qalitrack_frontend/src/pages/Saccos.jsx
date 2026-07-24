import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, Plus, Search, X, Building2 } from "lucide-react";
import { message, Modal, Switch } from "antd";
import TablePagination from "../components/TablePagination";
import { getSaccos, createSacco, updateSacco, deleteSacco } from "../api/MasterData/Saccos";

const PAGE_SIZE = 10;

export default function SaccosPortal() {
  const [saccos, setSaccos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState("All");

  const [form, setForm] = useState({
    name: "",
    registrationNumber: "",
    otherDetails: "",
    status: "Active",
  });

  const fetchSaccos = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getSaccos(1, 200, "");
      setSaccos(Array.isArray(data) ? data : (data?.items || []));
    } catch (err) {
      setError(err.message || "Failed to fetch saccos");
      setSaccos([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchSaccos(); }, []);

  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    return saccos.filter((s) => {
      const matchesSearch = !t ||
        s.name?.toLowerCase().includes(t) ||
        s.registrationNumber?.toLowerCase().includes(t) ||
        s.otherDetails?.toLowerCase().includes(t) ||
        s.status?.toLowerCase().includes(t);
      const matchesStatus = statusFilter === "All" || s.status?.toLowerCase() === statusFilter.toLowerCase();
      return matchesSearch && matchesStatus;
    });
  }, [saccos, search, statusFilter]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search, statusFilter]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const payload = {
        name: form.name,
        registrationNumber: form.registrationNumber || undefined,
        otherDetails: form.otherDetails || undefined,
        status: form.status,
      };
      if (editing) {
        await updateSacco(editing.id, payload);
      } else {
        await createSacco(payload);
      }
      resetForm();
      fetchSaccos();
    } catch (err) {
      message.error(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (sacco) => {
    setForm({
      name: sacco.name || "",
      registrationNumber: sacco.registrationNumber || "",
      otherDetails: sacco.otherDetails || "",
      status: sacco.status ? (sacco.status.charAt(0).toUpperCase() + sacco.status.slice(1).toLowerCase()) : "Active",
    });
    setEditing(sacco);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Are you sure you want to delete this sacco?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        setLoading(true);
        try {
          await deleteSacco(id);
          fetchSaccos();
        } catch (err) {
          message.error(`Error: ${err.message}`);
        } finally {
          setLoading(false);
        }
      },
    });
  };

  const handleToggleStatus = async (sacco) => {
    const newStatus = sacco.status?.toLowerCase() === "active" ? "Inactive" : "Active";
    try {
      await updateSacco(sacco.id, { ...sacco, status: newStatus });
      fetchSaccos();
    } catch (err) {
      message.error(`Failed to update status: ${err.message}`);
    }
  };

  const resetForm = () => {
    setForm({ name: "", registrationNumber: "", otherDetails: "", status: "Active" });
    setEditing(null);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">

      {/* Header — navy app-bar (Navy-theme experiment, see Transaction.jsx) */}
      <div className="px-3 py-2" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md cs-icon-box flex items-center justify-center shadow-sm">
              <Building2 className="w-4 h-4" style={{ color: "var(--cs-icon-accent)" }} />
            </div>
            <div>
              <span className="text-[11px] font-bold block leading-tight" style={{ color: "var(--cs-appbar-text)" }}>Saccos</span>
              <span className="text-[9px] font-medium" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
                {filtered.length} registered saccos
              </span>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search saccos..."
                className="qt-filter-field w-44 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="qt-filter-field h-7 px-2 text-[11px] rounded-md cs-ghost-btn shadow-sm"
            >
              <option value="All" className="text-black">All Status</option>
              <option value="Active" className="text-black">Active</option>
              <option value="Inactive" className="text-black">Inactive</option>
            </select>
            <button
              onClick={() => { setSearch(""); setStatusFilter("All"); setPage(1); fetchSaccos(); }}
              className="h-7 px-3 text-[11px] rounded-md cs-solid-chip-btn shadow-sm font-medium"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
        <form onSubmit={handleSubmit} className="space-y-2">
          <div>
            <h3 className="text-[10px] font-bold text-amber-900 uppercase mb-2 flex items-center gap-1.5">
              <Building2 className="w-3 h-3" />
              {editing ? "Edit Sacco" : "Add New Sacco"}
            </h3>
            <div className="grid grid-cols-4 gap-2">
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Sacco Name <span style={{ color: "var(--cs-required)" }}>*</span>
                </label>
                <input
                  name="name"
                  value={form.name}
                  onChange={handleChange}
                  required
                  placeholder="e.g., Unity Sacco"
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Registration Number
                </label>
                <input
                  name="registrationNumber"
                  value={form.registrationNumber}
                  onChange={handleChange}
                  placeholder="e.g., SAC/2024/001"
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Other Details
                </label>
                <input
                  name="otherDetails"
                  value={form.otherDetails}
                  onChange={handleChange}
                  placeholder="e.g., Members: 500"
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2 bg-white"
                >
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>
            </div>
          </div>

          <div className="flex gap-2 justify-end pt-1 border-t border-amber-200">
            {editing && (
              <button
                type="button"
                onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1"
              >
                <X className="w-3 h-3" /> Cancel
              </button>
            )}
            <button
              type="submit"
              disabled={loading}
              className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1 disabled:opacity-50"
            >
              <Plus className="w-3 h-3" />
              {editing ? "Update Sacco" : "Add Sacco"}
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading saccos…</p>
          </div>
        ) : error ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-red-500 text-sm">Error: {error}</p>
          </div>
        ) : paginated.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <Building2 className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500 text-sm">No saccos found.</p>
              <p className="text-gray-400 text-xs mt-1">Add a sacco using the form above</p>
            </div>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Reg Number</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Other Details</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginated.map((sacco, index) => {
                const isActive = sacco.status?.toLowerCase() === "active";
                return (
                  <tr
                    key={sacco.id}
                    className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                      index % 2 === 0 ? "bg-white" : "bg-gray-50"
                    }`}
                  >
                    <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">
                      {(page - 1) * PAGE_SIZE + index + 1}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">
                      {sacco.name}
                    </td>
                    <td className="px-3 py-2 text-[10px] font-mono text-gray-700 font-medium">
                      {sacco.registrationNumber || "—"}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-600 max-w-xs truncate">
                      {sacco.otherDetails || "—"}
                    </td>
                    <td className="px-3 py-2 text-center">
                      <Switch checked={isActive} onChange={() => handleToggleStatus(sacco)} size="small" />
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex gap-1 justify-center">
                        <button
                          onClick={() => handleEdit(sacco)}
                          className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                          title="Edit"
                        >
                          <Pencil className="w-3 h-3" />
                        </button>
                        <button
                          onClick={() => handleDelete(sacco.id)}
                          className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                          title="Delete"
                        >
                          <Trash2 className="w-3 h-3" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}

        {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
        <TablePagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          itemCount={filtered.length}
          itemLabel="saccos total"
        />
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
