import { useEffect, useState } from "react";
import { Pencil, Trash2, UserPlus, Search } from "lucide-react";
import {
  getSuppliers,
  createSupplier,
  updateSupplier,
  deleteSupplier,
} from "../api/MasterData/Suppliers";

export default function SuppliersPortal() {
  const [suppliers, setSuppliers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingSupplier, setEditingSupplier] = useState(null);

  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    name: "",
    contactPerson: "",
    phone: "",
    email: "",
    city: "",
    address: "",
  });

  // ───────────────────────────────
  // Fetch suppliers (with search & pagination)
  // ───────────────────────────────
  const fetchSuppliersWithSearch = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getSuppliers(page, pageSize, search);
      const items = data?.items || data || [];
      const totalItems = data?.totalItems || items.length;

      setSuppliers(items);
      setTotalPages(Math.ceil(totalItems / pageSize));
    } catch (err) {
      setError(err.message || "Failed to load suppliers");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSuppliersWithSearch();
  }, [page]);

  // ───────────────────────────────
  // Handlers
  // ───────────────────────────────
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingSupplier) {
        await updateSupplier(editingSupplier.id, form);
      } else {
        await createSupplier(form);
      }
      resetForm();
      fetchSuppliersWithSearch();
    } catch (err) {
      alert(err.message || "Failed to save supplier");
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (supplier) => {
    setForm({
      name: supplier.name || "",
      contactPerson: supplier.contactPerson || "",
      phone: supplier.phone || "",
      email: supplier.email || "",
      city: supplier.city || "",
      address: supplier.address || "",
    });
    setEditingSupplier(supplier);
  };

  const handleDelete = async (id) => {
    if (!confirm("Delete this supplier?")) return;
    await deleteSupplier(id);
    fetchSuppliersWithSearch();
  };

  const resetForm = () => {
    setForm({
      name: "",
      contactPerson: "",
      phone: "",
      email: "",
      city: "",
      address: "",
    });
    setEditingSupplier(null);
  };

  // ───────────────────────────────
  // UI
  // ───────────────────────────────
  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <UserPlus className="w-6 h-6" /> Supplier Management
      </h2>

      {/* 🔍 Search */}
      <form
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
          fetchSuppliersWithSearch();
        }}
        className="flex items-center gap-3 mb-6 border rounded-lg px-3 py-2"
      >
        <Search className="w-5 h-5 text-gray-400" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search suppliers..."
          className="flex-1 bg-transparent outline-none"
        />
        <button className="bg-amber-500 text-white px-4 py-1 rounded-lg">
          Search
        </button>
      </form>

      {/* 📝 Supplier Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        <input
          name="name"
          value={form.name}
          onChange={handleChange}
          placeholder="Supplier Name"
          required
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="contactPerson"
          value={form.contactPerson}
          onChange={handleChange}
          placeholder="Contact Person"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="phone"
          value={form.phone}
          onChange={handleChange}
          placeholder="Phone"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="email"
          value={form.email}
          onChange={handleChange}
          placeholder="Email"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="city"
          value={form.city}
          onChange={handleChange}
          placeholder="City"
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
            {editingSupplier ? "Update Supplier" : "Add Supplier"}
          </button>
          {editingSupplier && (
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

      {/* 📋 Suppliers Table */}
      {loading ? (
        <p>Loading...</p>
      ) : suppliers.length === 0 ? (
        <p className="text-gray-500">No suppliers found</p>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm border rounded-lg">
              <thead className="bg-gray-50">
                <tr>
                  {["Name", "Contact", "Email", "Phone", "City", "Actions"].map(
                    (h) => (
                      <th key={h} className="border px-3 py-2 text-left">
                        {h}
                      </th>
                    )
                  )}
                </tr>
              </thead>
              <tbody>
                {suppliers.map((s) => (
                  <tr key={s.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2 font-medium">{s.name}</td>
                    <td className="px-3 py-2">{s.contactPerson}</td>
                    <td className="px-3 py-2">{s.email}</td>
                    <td className="px-3 py-2">{s.phone}</td>
                    <td className="px-3 py-2">{s.city}</td>
                    <td className="px-3 py-2 flex gap-2">
                      <button onClick={() => handleEdit(s)}>
                        <Pencil className="w-4 h-4 text-blue-600" />
                      </button>
                      <button onClick={() => handleDelete(s.id)}>
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

      {error && <p className="text-red-600 mt-3">{error}</p>}
    </div>
  );
}
