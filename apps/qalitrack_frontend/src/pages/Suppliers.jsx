import { useEffect, useState } from "react";
import { Pencil, Trash2, UserPlus, Search, X, Building } from "lucide-react";
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

  // Fetch suppliers
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

  // Handlers
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

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <Building className="w-4 h-4 text-black font-bold" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Suppliers</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{suppliers.length}</span> registered
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input 
              type="text"
              placeholder="Search suppliers..." 
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500" 
              value={search}
              onChange={(e) => setSearch(e.target.value)} 
            />
          </div>
          <button
            onClick={(e) => {
              e.preventDefault();
              setPage(1);
              fetchSuppliersWithSearch();
            }}
            className="h-8 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded shadow-lg transition-all"
          >
            Search
          </button>
        </div>
      </div>

      {/* Form Section */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-emerald-50/20 border-b-2 border-amber-200 px-4 py-3 shrink-0 shadow-inner">
        <div className="bg-white rounded-lg p-4 border-2 border-amber-300 shadow-md">
          <h3 className="text-sm font-bold text-amber-900 mb-3 flex items-center gap-2">
            <Building className="w-4 h-4" />
            {editingSupplier ? "Edit Supplier" : "Add New Supplier"}
          </h3>
          
          <form onSubmit={handleSubmit} className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>
                Supplier Name *
              </label>
              <input
                name="name"
                value={form.name}
                onChange={handleChange}
                required
                className="w-full h-7 text-[11px] rounded border border-blue-300 px-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-200"
                placeholder="Company name"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full"></span>
                Contact Person
              </label>
              <input
                name="contactPerson"
                value={form.contactPerson}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-purple-300 px-2 focus:border-purple-500 focus:ring-1 focus:ring-purple-200"
                placeholder="Contact name"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-pink-500 rounded-full"></span>
                Phone
              </label>
              <input
                name="phone"
                value={form.phone}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-pink-300 px-2 focus:border-pink-500 focus:ring-1 focus:ring-pink-200"
                placeholder="+254 7XX XXX XXX"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-cyan-500 rounded-full"></span>
                Email
              </label>
              <input
                name="email"
                value={form.email}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-cyan-300 px-2 focus:border-cyan-500 focus:ring-1 focus:ring-cyan-200"
                placeholder="email@example.com"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>
                City
              </label>
              <input
                name="city"
                value={form.city}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-rose-300 px-2 focus:border-rose-500 focus:ring-1 focus:ring-rose-200"
                placeholder="City"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>
                Address
              </label>
              <input
                name="address"
                value={form.address}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-emerald-300 px-2 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-200"
                placeholder="Full address"
              />
            </div>

            <div className="col-span-3 flex gap-2 justify-end mt-2">
              {editingSupplier && (
                <button
                  type="button"
                  onClick={resetForm}
                  className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5"
                >
                  <X className="w-3.5 h-3.5" />
                  Cancel
                </button>
              )}
              <button
                type="submit"
                className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5"
              >
                <Building className="w-3.5 h-3.5" />
                {editingSupplier ? "Update Supplier" : "Add Supplier"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">Loading suppliers...</p>
            </div>
          ) : suppliers.length === 0 ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">No suppliers found.</p>
            </div>
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Name</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Contact Person</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Email</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Phone</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">City</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Address</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-center uppercase tracking-wide">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {suppliers.map((s, index) => (
                      <tr 
                        key={s.id} 
                        className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                      >
                        <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{s.name}</td>
                        <td className="px-4 py-2.5 text-[10px] text-purple-600 font-medium">{s.contactPerson || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-cyan-600">{s.email || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-pink-600 font-medium">{s.phone || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-rose-600">{s.city || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-gray-600 max-w-xs truncate">{s.address || '-'}</td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-2 justify-center">
                            <button
                              onClick={() => handleEdit(s)}
                              className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all"
                              title="Edit"
                            >
                              <Pencil className="w-3.5 h-3.5" />
                            </button>
                            <button
                              onClick={() => handleDelete(s.id)}
                              className="p-1.5 rounded-lg text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                              title="Delete"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              <div className="border-t-2 border-amber-200 bg-gradient-to-r from-gray-50 to-amber-50/30 px-4 py-2.5 flex justify-between items-center">
                <button
                  disabled={page === 1}
                  onClick={() => setPage((p) => p - 1)}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all"
                >
                  Previous
                </button>
                <span className="text-[11px] font-bold text-gray-700">
                  Page <span className="text-amber-600">{page}</span> of <span className="text-amber-600">{totalPages}</span>
                </span>
                <button
                  disabled={page === totalPages}
                  onClick={() => setPage((p) => p + 1)}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {error && (
        <div className="fixed bottom-4 right-4 bg-red-100 border-2 border-red-400 text-red-700 px-4 py-2 rounded-lg shadow-lg">
          <p className="text-xs font-semibold">{error}</p>
        </div>
      )}
    </div>
  );
}