import { useEffect, useState } from "react";
import {
  Pencil,
  Trash2,
  UserPlus,
  Search,
  Truck,
} from "lucide-react";
import {
  getTransporters,
  createTransporter,
  updateTransporter,
  deleteTransporter,
} from "../../api/MasterData/Transporters";

export default function TransportersPortal() {
  const [transporters, setTransporters] = useState([]);
  const [editingTransporter, setEditingTransporter] = useState(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    name: "",
    email: "",
    phone: "",
    address: "",
    licenseNumber: "",
    status: "Active",
    logo: "",
  });

  // ─────────────────────────────────────────────
  // Fetch Transporters
  // ─────────────────────────────────────────────
  useEffect(() => {
    fetchTransporters();
  }, [page, search]);

  const fetchTransporters = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getTransporters(page, pageSize, search);
      const items = data?.items || data || [];
      const totalItems = data?.totalItems || items.length;

      setTransporters(items);
      setTotalPages(Math.ceil(totalItems / pageSize));
    } catch (err) {
      setError(err.message || "Failed to load transporters");
    } finally {
      setLoading(false);
    }
  };

  // ─────────────────────────────────────────────
  // Handlers
  // ─────────────────────────────────────────────
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

      if (editingTransporter) {
        await updateTransporter(editingTransporter.id, payload);
      } else {
        await createTransporter(payload);
      }

      resetForm();
      fetchTransporters();
    } catch (err) {
      alert(err.message || "Failed to save transporter");
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (t) => {
    let contactInfo = t.contactInfo || {};
    if (typeof contactInfo === "string") {
      try {
        contactInfo = JSON.parse(contactInfo);
      } catch {
        contactInfo = {};
      }
    }

    setForm({
      name: t.name || "",
      email: contactInfo.Email || "",
      phone: contactInfo.Phone || "",
      address: contactInfo.Address || "",
      licenseNumber: contactInfo.LicenseNumber || "",
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
    setForm({
      name: "",
      email: "",
      phone: "",
      address: "",
      licenseNumber: "",
      status: "Active",
      logo: "",
    });
    setEditingTransporter(null);
  };

  // ─────────────────────────────────────────────
  // UI (MATCH OWNERS / DRIVERS)
  // ─────────────────────────────────────────────
  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <Truck className="w-6 h-6" /> Transporter Management
      </h2>

      {/* 🔍 Search */}
      <form
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
          fetchTransporters();
        }}
        className="flex items-center gap-3 mb-6 border rounded-lg px-3 py-2"
      >
        <Search className="w-5 h-5 text-gray-400" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search transporters..."
          className="flex-1 bg-transparent outline-none"
        />
        <button className="bg-amber-500 text-white px-4 py-1 rounded-lg">
          Search
        </button>
      </form>

      {/* 📝 Transporter Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        <input
          name="name"
          value={form.name}
          onChange={handleChange}
          placeholder="Transporter Name"
          required
          className="border rounded-lg px-3 py-2"
        />

        <select
          name="status"
          value={form.status}
          onChange={handleChange}
          className="border rounded-lg px-3 py-2"
        >
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
          <option value="Suspended">Suspended</option>
        </select>

        <input
          name="email"
          value={form.email}
          onChange={handleChange}
          placeholder="Email"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="phone"
          value={form.phone}
          onChange={handleChange}
          placeholder="Phone Number"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="licenseNumber"
          value={form.licenseNumber}
          onChange={handleChange}
          placeholder="License Number"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="logo"
          value={form.logo}
          onChange={handleChange}
          placeholder="Logo URL"
          className="border rounded-lg px-3 py-2"
        />

        <textarea
          name="address"
          value={form.address}
          onChange={handleChange}
          placeholder="Address"
          className="border rounded-lg px-3 py-2 md:col-span-2"
        />

        <div className="md:col-span-2 flex gap-3">
          <button className="bg-amber-500 text-white px-4 py-2 rounded-lg">
            {editingTransporter ? "Update Transporter" : "Add Transporter"}
          </button>

          {editingTransporter && (
            <button
              type="button"
              onClick={resetForm}
              className="bg-gray-200 px-4 py-2 rounded-lg"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {/* 📋 Transporters Table */}
      {loading ? (
        <p>Loading...</p>
      ) : transporters.length === 0 ? (
        <p className="text-gray-500">No transporters found</p>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm border rounded-lg">
              <thead className="bg-gray-50">
                <tr>
                  {["Name", "Phone", "Status", "Actions"].map((h) => (
                    <th key={h} className="border px-3 py-2 text-left">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {transporters.map((t) => (
                  <tr key={t.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2 font-medium">{t.name}</td>
                    <td className="px-3 py-2">
                      {JSON.parse(t.contactInfo || "{}")?.Phone || "-"}
                    </td>
                    <td className="px-3 py-2">{t.status}</td>
                    <td className="px-3 py-2 flex gap-2">
                      <button onClick={() => handleEdit(t)}>
                        <Pencil className="w-4 h-4 text-blue-600" />
                      </button>
                      <button onClick={() => handleDelete(t.id)}>
                        <Trash2 className="w-4 h-4 text-red-600" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="flex justify-between mt-4">
            <button
              disabled={page === 1}
              onClick={() => setPage((p) => p - 1)}
              className="border px-3 py-1 rounded-lg"
            >
              Previous
            </button>
            <span>
              Page {page} of {totalPages}
            </span>
            <button
              disabled={page === totalPages}
              onClick={() => setPage((p) => p + 1)}
              className="border px-3 py-1 rounded-lg"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
