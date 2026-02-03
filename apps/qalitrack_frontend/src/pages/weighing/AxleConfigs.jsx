import React, { useEffect, useState, useMemo } from "react";
import { Modal, Form, Input, InputNumber, message, Switch } from "antd";
import { Plus, Pencil, Trash2, Search, RotateCcw } from "lucide-react";
import {
  getAxleConfigs,
  createAxleConfig,
  updateAxleConfig,
  deleteAxleConfig,
  toggleAxleConfigStatus,
} from "../../api/MasterData/AxleConfigs";

const { TextArea } = Input;
const PAGE_SIZE = 5;

const AxleConfigs = () => {
  const [configs, setConfigs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingConfig, setEditingConfig] = useState(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [form] = Form.useForm();

  // ── fetch (pull all, paginate client-side) ──
  const fetchConfigs = async () => {
    try {
      setLoading(true);
      const response = await getAxleConfigs(1, 200);
      const data = response?.data || response;
      setConfigs(data?.items || []);
    } catch (error) {
      console.error("Error fetching axle configurations:", error);
      message.error("Failed to load axle configurations");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchConfigs(); }, []);

  // ── derived ──
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

  // ── handlers ──
  const handleAdd = () => {
    setEditingConfig(null);
    form.resetFields();
    form.setFieldsValue({ isActive: true });
    setIsModalVisible(true);
  };

  const handleEdit = (record) => {
    setEditingConfig(record);
    form.setFieldsValue({
      code: record.code,
      description: record.description,
      axleCount: record.axleCount,
      maxLoadCapacity: record.maxLoadCapacity,
      isActive: record.isActive,
    });
    setIsModalVisible(true);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this configuration?")) return;
    try {
      await deleteAxleConfig(id);
      message.success("Axle configuration deleted successfully");
      fetchConfigs();
    } catch (error) {
      console.error("Error deleting axle configuration:", error);
      message.error("Failed to delete axle configuration");
    }
  };

  const handleStatusToggle = async (id, currentStatus) => {
    try {
      await toggleAxleConfigStatus(id, !currentStatus);
      message.success("Status updated successfully");
      fetchConfigs();
    } catch (error) {
      console.error("Error toggling status:", error);
      message.error("Failed to update status");
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (editingConfig) {
        await updateAxleConfig(editingConfig.id, { id: editingConfig.id, ...values });
        message.success("Axle configuration updated successfully");
      } else {
        await createAxleConfig(values);
        message.success("Axle configuration created successfully");
      }
      setIsModalVisible(false);
      form.resetFields();
      fetchConfigs();
    } catch (error) {
      if (error.errorFields) message.error("Please fill in all required fields");
      else {
        console.error("Error saving axle configuration:", error);
        message.error("Failed to save axle configuration");
      }
    }
  };

  // ── UI ──
  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <svg className="w-4 h-4 text-black" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="3" strokeWidth={2.5} />
              <path d="M12 2v3M12 19v3M4.93 4.93l2.12 2.12M16.95 16.95l2.12 2.12M2 12h3M19 12h3M4.93 19.07l2.12-2.12M16.95 7.05l2.12-2.12" strokeWidth={2} strokeLinecap="round" />
            </svg>
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Axle Configurations</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1"><span className="font-semibold">{filtered.length}</span> configurations</div>
          </div>
        </div>
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search configs..."
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500"
              value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          <button onClick={fetchConfigs} className="h-8 px-3 text-[11px] font-semibold bg-gray-800 border border-gray-700 text-white rounded hover:bg-gray-700 transition-all flex items-center gap-1.5">
            <RotateCcw className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`} /> Refresh
          </button>
          <button onClick={handleAdd} className="h-8 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded shadow-lg transition-all flex items-center gap-1.5">
            <Plus className="w-3.5 h-3.5" /> Add Config
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden flex flex-col">
          {loading ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">Loading configurations...</p></div>
          ) : paginated.length === 0 ? (
            <div className="flex-1 flex items-center justify-center"><p className="text-gray-500 text-sm">No configurations found.</p></div>
          ) : (
            <>
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      {["Code","Description","Axles","Max Load (kg)","Status","Actions"].map(h => (
                        <th key={h} className={`px-4 py-2.5 text-[10px] font-bold text-amber-400 uppercase tracking-wide ${["Actions","Axles","Status"].includes(h) ? "text-center" : h === "Max Load (kg)" ? "text-right" : "text-left"}`}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {paginated.map((c, i) => (
                      <tr key={c.id} className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${i % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                        <td className="px-4 py-2.5">
                          <span className="inline-block bg-blue-100 text-blue-700 border border-blue-300 px-2.5 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wide">{c.code || "—"}</span>
                        </td>
                        <td className="px-4 py-2.5 text-[10px] text-gray-700 max-w-xs truncate">{c.description || "—"}</td>
                        <td className="px-4 py-2.5 text-center">
                          <span className="inline-block bg-cyan-100 text-cyan-700 border border-cyan-300 px-2.5 py-0.5 rounded-full text-[9px] font-bold">{c.axleCount}</span>
                        </td>
                        <td className="px-4 py-2.5 text-[10px] text-right text-amber-700 font-bold">{c.maxLoadCapacity?.toLocaleString() || "—"}</td>
                        <td className="px-4 py-2.5 text-center">
                          <Switch checked={c.isActive} onChange={() => handleStatusToggle(c.id, c.isActive)} size="small" />
                        </td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-2 justify-center">
                            <button onClick={() => handleEdit(c)} className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all" title="Edit"><Pencil className="w-3.5 h-3.5" /></button>
                            <button onClick={() => handleDelete(c.id)} className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all" title="Delete"><Trash2 className="w-3.5 h-3.5" /></button>
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

      {/* Ant Modal — form logic kept 100% intact */}
      <Modal
        title={<span className="text-sm font-bold text-amber-900">{editingConfig ? "Edit Axle Configuration" : "Add New Axle Configuration"}</span>}
        open={isModalVisible}
        onOk={handleSubmit}
        onCancel={() => { setIsModalVisible(false); form.resetFields(); }}
        okText={editingConfig ? "Update" : "Create"}
        cancelText="Cancel"
        width={580}
        okButtonProps={{ className: "bg-gradient-to-r from-amber-500 to-amber-600 text-black border-0 font-semibold hover:from-amber-400 hover:to-amber-500" }}
      >
        <Form form={form} layout="vertical" initialValues={{ isActive: true }}>
          <Form.Item name="code" label="Configuration Code"
            rules={[
              { required: true, message: "Please enter a code" },
              { pattern: /^[A-Z0-9-]+$/, message: "Uppercase letters, numbers, or hyphens only" },
            ]}>
            <Input placeholder="e.g., AXL-3" style={{ textTransform: "uppercase" }} />
          </Form.Item>

          <Form.Item name="description" label="Description" rules={[{ required: true, message: "Please enter a description" }]}>
            <TextArea rows={3} placeholder="Brief description of this configuration" />
          </Form.Item>

          <div className="grid grid-cols-2 gap-4">
            <Form.Item name="axleCount" label="Number of Axles"
              rules={[
                { required: true, message: "Required" },
                { type: "number", min: 1, max: 20, message: "Must be 1–20" },
              ]}>
              <InputNumber min={1} max={20} className="w-full" placeholder="e.g., 3" />
            </Form.Item>

            <Form.Item name="maxLoadCapacity" label="Max Load Capacity (kg)"
              rules={[
                { required: true, message: "Required" },
                { type: "number", min: 100, message: "At least 100 kg" },
              ]}>
              <InputNumber min={100} step={100} className="w-full" placeholder="e.g., 30000"
                formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")}
                parser={(v) => v.replace(/\$\s?|(,*)/g, "")} />
            </Form.Item>
          </div>

          <Form.Item name="isActive" label="Status" valuePropName="checked">
            <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default AxleConfigs;