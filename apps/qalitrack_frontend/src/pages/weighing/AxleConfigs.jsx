import React, { useEffect, useState, useMemo } from "react";
import { message, Modal, Switch } from "antd";
import { Plus, Pencil, Trash2, Search, RotateCcw, X, Settings2 } from "lucide-react";
import TablePagination from "../../components/TablePagination";
import PageHeader from "../../components/PageHeader.jsx";
import {
  getAxleConfigs,
  createAxleConfig,
  updateAxleConfig,
  deleteAxleConfig,
  toggleAxleConfigStatus,
} from "../../api/MasterData/AxleConfigs";

const PAGE_SIZE = 5;

// Axle config codes follow the truck-industry "NxM" convention (e.g. "8x4" =
// 8 wheels total, 4 driven) — the leading number is the wheel count. Spelling
// that out lets non-technical staff tell configs apart without already
// knowing the convention.
const getWheelCount = (code) => {
  const match = code?.match(/^(\d+)/);
  return match ? parseInt(match[1], 10) : null;
};

const EMPTY_FORM = { code: "", description: "", axleCount: "", maxLoadCapacity: "", isActive: true };

const AxleConfigs = () => {
  const [configs, setConfigs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [editingConfig, setEditingConfig] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [form, setForm] = useState(EMPTY_FORM);

  const fetchConfigs = async () => {
    try {
      setLoading(true);
      const response = await getAxleConfigs(1, 200);
      const data = response?.data || response;
      setConfigs([...(data?.items || [])].reverse());
    } catch (error) {
      message.error("Failed to load axle configurations");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchConfigs(); }, []);

  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return configs;
    return configs.filter(
      (c) =>
        c.code?.toLowerCase().includes(t) ||
        c.description?.toLowerCase().includes(t)
    );
  }, [configs, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { setPage(1); }, [search]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: name === "code" ? value.toUpperCase() : value }));
  };

  const resetForm = () => {
    setForm(EMPTY_FORM);
    setEditingConfig(null);
  };

  const handleEdit = (record) => {
    setForm({
      code: record.code || "",
      description: record.description || "",
      axleCount: record.axleCount ?? "",
      maxLoadCapacity: record.maxLoadCapacity ?? "",
      isActive: record.isActive,
    });
    setEditingConfig(record);
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Are you sure you want to delete this configuration?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteAxleConfig(id);
          message.success("Axle configuration deleted successfully");
          fetchConfigs();
        } catch (error) {
          message.error("Failed to delete axle configuration");
        }
      },
    });
  };

  const handleStatusToggle = async (id, currentStatus) => {
    try {
      await toggleAxleConfigStatus(id, !currentStatus);
      message.success("Status updated successfully");
      fetchConfigs();
    } catch (error) {
      message.error("Failed to update status");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        code: form.code,
        description: form.description,
        axleCount: parseInt(form.axleCount, 10),
        maxLoadCapacity: parseFloat(form.maxLoadCapacity),
        isActive: !!form.isActive,
      };
      if (editingConfig) {
        await updateAxleConfig(editingConfig.id, { id: editingConfig.id, ...payload });
        message.success("Axle configuration updated successfully");
      } else {
        await createAxleConfig(payload);
        message.success("Axle configuration created successfully");
      }
      resetForm();
      setPage(1);
      fetchConfigs();
    } catch (error) {
      message.error("Failed to save axle configuration");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      <PageHeader
        flush
        icon={Settings2}
        title="Axle Configurations"
        subtitle={<><span className="font-semibold">{filtered.length}</span> configurations</>}
        actions={
          <>
            <div className="relative">
              <Search size={10} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input type="text" placeholder="Search configs..."
                className="qt-filter-field w-52 h-7 pl-7 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
                value={search} onChange={(e) => setSearch(e.target.value)} />
            </div>
            <button onClick={fetchConfigs} className="h-7 px-3 text-[11px] font-medium rounded-md cs-solid-chip-btn shadow-sm transition-all flex items-center gap-1.5">
              <RotateCcw className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`} /> Refresh
            </button>
          </>
        }
      />

      {/* Inline Form */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm shrink-0">
        <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Configuration Code <span style={{ color: "var(--cs-required)" }}>*</span></label>
            <input
              name="code"
              value={form.code}
              onChange={handleChange}
              required
              placeholder="e.g., AXL-3"
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2 uppercase"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Number of Axles <span style={{ color: "var(--cs-required)" }}>*</span></label>
            <input
              type="number"
              name="axleCount"
              min={1}
              max={20}
              value={form.axleCount}
              onChange={handleChange}
              required
              placeholder="e.g., 3"
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Max Load Capacity (kg) <span style={{ color: "var(--cs-required)" }}>*</span></label>
            <input
              type="number"
              name="maxLoadCapacity"
              min={100}
              step={100}
              value={form.maxLoadCapacity}
              onChange={handleChange}
              required
              placeholder="e.g., 30000"
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Status</label>
            <select
              name="isActive"
              value={form.isActive ? "true" : "false"}
              onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.value === "true" }))}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
            >
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>

          <div className="col-span-4">
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Description <span style={{ color: "var(--cs-required)" }}>*</span></label>
            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
              required
              rows={2}
              placeholder="Brief description of this configuration"
              className="qt-filter-field w-full text-[11px] rounded border border-gray-300 px-2 py-1 resize-none"
            />
          </div>

          <div className="col-span-4 flex gap-2 justify-end mt-1">
            {editingConfig && (
              <button type="button" onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1">
                <X className="w-3 h-3" /> Cancel
              </button>
            )}
            <button type="submit" disabled={saving}
              className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1">
              <Plus className="w-3 h-3" />
              {editingConfig ? "Update Configuration" : "Add Configuration"}
            </button>
          </div>
        </form>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden">
        <div className="h-full bg-white overflow-hidden flex flex-col">
          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading configurations...</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center">
              <div className="text-center">
                <Settings2 className="w-12 h-12 text-gray-300 mx-auto mb-2" />
                <p className="text-gray-500 text-sm">No configurations found.</p>
                <p className="text-gray-400 text-xs mt-1">Add a configuration using the form above</p>
              </div>
            </div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full compact-table">
                  <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                    <tr>
                      {["Code","Wheels","Description","Axles","Max Load (kg)","Status","Actions"].map(h => (
                        <th key={h} className={`px-3 py-2 text-[9px] font-bold text-amber-900 uppercase tracking-wide ${["Actions","Wheels","Axles","Status"].includes(h) ? "text-center" : h === "Max Load (kg)" ? "text-right" : "text-left"}`}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {paginated.map((c, i) => (
                      <tr
                        key={c.id}
                        className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                          i % 2 === 0 ? 'bg-white' : 'bg-gray-50'
                        }`}
                      >
                        <td className="px-3 py-2">
                          <span className="inline-block bg-blue-100 text-blue-800 px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase border border-blue-300">{c.code || "—"}</span>
                        </td>
                        <td className="px-3 py-2 text-center">
                          {getWheelCount(c.code) ? (
                            <span className="inline-block bg-gray-100 text-gray-700 px-2 py-0.5 rounded-full text-[9px] font-semibold border border-gray-300">{getWheelCount(c.code)} wheels</span>
                          ) : (
                            <span className="text-gray-400">—</span>
                          )}
                        </td>
                        <td className="px-3 py-2 text-[10px] text-gray-600 max-w-xs truncate">{c.description || "—"}</td>
                        <td className="px-3 py-2 text-center">
                          <span className="inline-block bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full text-[9px] font-semibold border border-amber-300">{c.axleCount}</span>
                        </td>
                        <td className="px-3 py-2 text-[10px] text-right text-amber-700 font-semibold">{c.maxLoadCapacity?.toLocaleString() || "—"}</td>
                        <td className="px-3 py-2 text-center">
                          <Switch checked={c.isActive} onChange={() => handleStatusToggle(c.id, c.isActive)} size="small" />
                        </td>
                        <td className="px-3 py-2">
                          <div className="flex gap-1.5 justify-center">
                            <button onClick={() => handleEdit(c)} className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all" title="Edit"><Pencil className="w-3 h-3" /></button>
                            <button onClick={() => handleDelete(c.id)} className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete"><Trash2 className="w-3 h-3" /></button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {/* Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                <TablePagination page={page} totalPages={totalPages} onPageChange={setPage} />
              </div>
            </>
          )}
        </div>
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
};

export default AxleConfigs;
