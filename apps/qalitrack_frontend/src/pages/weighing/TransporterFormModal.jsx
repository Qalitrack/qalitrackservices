import { useEffect, useState, useMemo } from "react";
import { Pencil, Trash2, Plus, Search, X, Truck, Car, UserCheck } from "lucide-react";
import { message, Modal, Switch } from "antd";
import TablePagination from "../../components/TablePagination";
import VehicleRelationModal from "../../components/VehicleRelationModal";
import DriverRelationModal from "../../components/DriverRelationModal";
import {
  getTransporters,
  createTransporter,
  updateTransporter,
  deleteTransporter,
} from "../../api/MasterData/Transporters";
import { assignDriverToTransporter, unassignDriverFromTransporter } from "../../api/MasterData/Drivers";

const PAGE_SIZE = 5;

export default function TransportersPortal() {
  const [transporters, setTransporters] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingTransporter, setEditingTransporter] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [vehiclesTransporter, setVehiclesTransporter] = useState(null);
  const [driversTransporter, setDriversTransporter] = useState(null);

  const [form, setForm] = useState({
    name: "",
    email: "",
    phone: "",
    address: "",
    licenseNumber: "",
    status: "Active",
  });

  useEffect(() => {
    fetchTransporters();
  }, []);

  const fetchTransporters = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getTransporters({ pageNumber: 1, pageSize: 500, search: "" });
      const items = data?.items || data || [];
      setTransporters([...items].reverse());
    } catch (err) {
      setError(err.message || "Failed to load transporters");
    } finally {
      setLoading(false);
    }
  };

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

  function parseContact(raw) {
    if (!raw) return {};
    if (typeof raw === "object") return raw;
    try { return JSON.parse(raw); }
    catch { return {}; }
  }

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
      setPage(1);
      await fetchTransporters();
    } catch (err) {
      message.error(err.message || "Failed to save transporter");
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
    });
    setEditingTransporter(t);
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Delete this transporter?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteTransporter(id);
          fetchTransporters();
        } catch (err) {
          message.error(err.message || "Failed to delete transporter");
        }
      },
    });
  };

  const resetForm = () => {
    setForm({ name: "", email: "", phone: "", address: "", licenseNumber: "", status: "Active" });
    setEditingTransporter(null);
  };

  const handleToggleStatus = async (transporter) => {
    const newStatus = transporter.status === "Active" ? "Inactive" : "Active";
    try {
      await updateTransporter(transporter.id, { name: transporter.name, status: newStatus, contactInfo: transporter.contactInfo });
      fetchTransporters();
    } catch (err) {
      message.error(err.message || "Failed to update status");
    }
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">

      {/* Header — navy app-bar (Navy-theme experiment, see Transaction.jsx) */}
      <div className="px-3 py-2 shrink-0" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md cs-icon-box flex items-center justify-center shadow-sm">
              <Truck className="w-4 h-4" style={{ color: "var(--cs-icon-accent)" }} />
            </div>
            <div>
              <div className="text-[11px] font-bold leading-tight" style={{ color: "var(--cs-appbar-text)" }}>Transporters</div>
              <div className="text-[9px] font-medium leading-tight" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
                <span className="font-semibold">{filtered.length}</span> registered
              </div>
            </div>
          </div>

          <div className="flex gap-2 items-center">
            <div className="relative">
              <Search size={10} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search transporters..."
                className="qt-filter-field w-52 h-7 pl-7 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Inline Form */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-amber-50/20 border-b border-amber-200 px-3 py-2.5 shrink-0">
        <div className="bg-white rounded-lg p-3 border border-amber-300 shadow-sm">
          <h3 className="text-xs font-bold text-amber-900 mb-2 flex items-center gap-1.5">
            <Truck className="w-3.5 h-3.5" />
            {editingTransporter ? "Edit Transporter" : "Add New Transporter"}
          </h3>

          <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-2">
            <div>
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">Name <span style={{ color: "var(--cs-required)" }}>*</span></label>
              <input name="name" value={form.name} onChange={handleChange} required placeholder="Transporter name"
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2" />
            </div>

            <div>
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">Status</label>
              <select name="status" value={form.status} onChange={handleChange}
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2">
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Suspended">Suspended</option>
              </select>
            </div>

            <div>
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">Email</label>
              <input name="email" value={form.email} onChange={handleChange} placeholder="email@example.com"
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2" />
            </div>

            <div>
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">Phone</label>
              <input name="phone" value={form.phone} onChange={handleChange} placeholder="+254 7XX XXX XXX"
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2" />
            </div>

            <div>
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">License Number</label>
              <input name="licenseNumber" value={form.licenseNumber} onChange={handleChange} placeholder="License no."
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2" />
            </div>

            <div className="col-span-2">
              <label className="text-[9px] font-semibold text-amber-700 uppercase mb-0.5 block">Address</label>
              <input name="address" value={form.address} onChange={handleChange} placeholder="Full address"
                className="qt-filter-field w-full h-6 text-[10px] rounded border border-gray-300 px-2" />
            </div>

            <div className="col-span-4 flex gap-2 justify-end mt-0.5">
              {editingTransporter && (
                <button type="button" onClick={resetForm}
                  className="h-6 px-3 text-[10px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1">
                  <X className="w-3 h-3" /> Cancel
                </button>
              )}
              <button type="submit" disabled={loading}
                className="h-6 px-3 text-[10px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow-sm transition-all flex items-center gap-1">
                <Plus className="w-3 h-3" />
                {editingTransporter ? "Update Transporter" : "Add Transporter"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden">
        <div className="h-full bg-white overflow-hidden flex flex-col">

          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading transporters…</p></div>
          ) : error ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-red-500 text-sm">Error: {error}</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center">
              <div className="text-center">
                <Truck className="w-12 h-12 text-gray-300 mx-auto mb-2" />
                <p className="text-gray-500 text-sm">No transporters found.</p>
                <p className="text-gray-400 text-xs mt-1">Add a transporter using the form above</p>
              </div>
            </div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full compact-table">
                  <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                    <tr>
                      {[
                        { label: "#", align: "text-left" },
                        { label: "Name", align: "text-left" },
                        { label: "Phone", align: "text-left" },
                        { label: "Email", align: "text-left" },
                        { label: "License", align: "text-left" },
                        { label: "Status", align: "text-center" },
                        { label: "Actions", align: "text-center" },
                      ].map((h) => (
                        <th key={h.label} className={`px-3 py-2 text-[9px] font-bold text-amber-900 uppercase tracking-wide ${h.align}`}>{h.label}</th>
                      ))}
                    </tr>
                  </thead>

                  <tbody>
                    {paginated.map((t, i) => {
                      const ci = parseContact(t.contactInfo);
                      return (
                        <tr
                          key={t.ticketID || t.id}
                          className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                            i % 2 === 0 ? 'bg-white' : 'bg-gray-50'
                          }`}
                        >
                          <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">
                            {(page - 1) * PAGE_SIZE + i + 1}
                          </td>
                          <td className="px-3 py-2 text-[10px] font-semibold text-gray-900">{t.name}</td>
                          <td className="px-3 py-2 text-[10px] text-gray-600">{ci.Phone || "—"}</td>
                          <td className="px-3 py-2 text-[10px] text-gray-600">{ci.Email || "—"}</td>
                          <td className="px-3 py-2">
                            {ci.LicenseNumber ? (
                              <span className="inline-block bg-purple-100 text-purple-800 px-2 py-0.5 rounded-full text-[9px] font-mono border border-purple-300">
                                {ci.LicenseNumber}
                              </span>
                            ) : <span className="text-[10px] text-gray-400">—</span>}
                          </td>
                          <td className="px-3 py-2 text-center">
                            <Switch checked={t.status === "Active"} onChange={() => handleToggleStatus(t)} size="small" />
                          </td>
                          <td className="px-3 py-2">
                            <div className="flex gap-1.5 justify-center">
                              <button onClick={() => setVehiclesTransporter(t)}
                                className="p-1 rounded text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all" title="View Vehicles">
                                <Car className="w-3 h-3" />
                              </button>
                              <button onClick={() => setDriversTransporter(t)}
                                className="p-1 rounded text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all" title="View Drivers">
                                <UserCheck className="w-3 h-3" />
                              </button>
                              <button onClick={() => handleEdit(t)}
                                className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all" title="Edit">
                                <Pencil className="w-3 h-3" />
                              </button>
                              <button onClick={() => handleDelete(t.id)}
                                className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete">
                                <Trash2 className="w-3 h-3" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>

                {/* Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                <TablePagination page={page} totalPages={totalPages} onPageChange={setPage} />
              </div>
            </>
          )}
        </div>
      </div>

      <VehicleRelationModal
        entity={vehiclesTransporter}
        relationField="transporterId"
        relationNameField="transporterName"
        entityLabel="Transporter"
        onClose={() => setVehiclesTransporter(null)}
      />

      <DriverRelationModal
        entity={driversTransporter}
        relationField="transporterId"
        assignFn={assignDriverToTransporter}
        removeFn={unassignDriverFromTransporter}
        entityLabel="Transporter"
        otherEntities={transporters}
        onClose={() => setDriversTransporter(null)}
      />

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