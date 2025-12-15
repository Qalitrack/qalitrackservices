import { useEffect, useState } from "react";
import { Pencil, Trash2, PackagePlus, Search } from "lucide-react";
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
} from "../../api/MasterData/Products";

export default function ProductsPortal() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    name: "",
    code: "",
    description: "",
    unit: "",
    status: "Active",
  });

  // ─────────────────────────────────────────────
  // Fetch products
  // ─────────────────────────────────────────────
  useEffect(() => {
    fetchProducts();
  }, [page, search]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getProducts(page, pageSize, search);

      const productList = Array.isArray(data?.items) ? data.items : [];
      const totalItems = data?.totalCount || productList.length;
      const pages = Math.ceil(totalItems / pageSize);

      setProducts(productList);
      setTotalPages(pages);
    } catch (err) {
      console.error("❌ Failed to load products:", err.message);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // ─────────────────────────────────────────────
  // Form handlers
  // ─────────────────────────────────────────────
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingProduct) {
        await updateProduct(editingProduct.id, form);
      } else {
        await createProduct(form);
      }
      await fetchProducts();
      resetForm();
    } catch (err) {
      console.error("❌ Save failed:", err.message);
      alert(`Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (product) => {
    setForm({
      name: product.name || "",
      code: product.code || "",
      description: product.description || "",
      unit: product.unit || "",
      status: product.status || "Active",
    });
    setEditingProduct(product);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this product?")) return;

    setLoading(true);
    try {
      await deleteProduct(id);
      await fetchProducts();
    } catch (err) {
      console.error("❌ Delete failed:", err.message);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({
      name: "",
      code: "",
      description: "",
      unit: "",
      status: "Active",
    });
    setEditingProduct(null);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchProducts();
  };

  // ─────────────────────────────────────────────
  // UI
  // ─────────────────────────────────────────────
  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <PackagePlus className="w-6 h-6" /> Product Management
      </h2>

      {/* 🔍 Search */}
      <form
        onSubmit={handleSearch}
        className="flex items-center gap-3 mb-6 border border-gray-200 rounded-lg px-3 py-2"
      >
        <Search className="w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search products by name or code..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="flex-1 focus:outline-none bg-transparent text-gray-700"
        />
        <button className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-1 rounded-lg font-medium">
          Search
        </button>
      </form>

      {/* 📝 Product Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        {[
          { label: "Product Name", name: "name" },
          { label: "Product Code", name: "code" },
          { label: "Unit", name: "unit" },
        ].map((field) => (
          <div key={field.name}>
            <label className="block text-sm font-medium mb-1">
              {field.label}
            </label>
            <input
              name={field.name}
              value={form[field.name]}
              onChange={handleChange}
              required={field.name === "name"}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
            />
          </div>
        ))}

        <div className="md:col-span-2">
          <label className="block text-sm font-medium mb-1">
            Description
          </label>
          <textarea
            name="description"
            value={form.description}
            onChange={handleChange}
            rows={3}
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Status</label>
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-amber-400"
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
        </div>

        <div className="md:col-span-2 flex gap-3 mt-2">
          <button
            type="submit"
            disabled={loading}
            className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-lg font-semibold"
          >
            {editingProduct ? "Update Product" : "Add Product"}
          </button>

          {editingProduct && (
            <button
              type="button"
              onClick={resetForm}
              className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-semibold"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {/* 📦 Product Table */}
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        Registered Products
      </h3>

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : error ? (
        <p className="text-red-500">Error: {error}</p>
      ) : products.length === 0 ? (
        <p className="text-gray-500 text-sm">No products found.</p>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm border border-gray-200 rounded-lg">
              <thead className="bg-gray-50">
                <tr>
                  {["Name", "Code", "Unit", "Status", "Actions"].map((h) => (
                    <th key={h} className="border px-3 py-2 text-left">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr key={p.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2">{p.name}</td>
                    <td className="px-3 py-2">{p.code}</td>
                    <td className="px-3 py-2">{p.unit}</td>
                    <td className="px-3 py-2">{p.status}</td>
                    <td className="px-3 py-2 flex gap-2">
                      <button
                        onClick={() => handleEdit(p)}
                        className="text-blue-600 hover:text-blue-800"
                      >
                        <Pencil className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(p.id)}
                        className="text-red-600 hover:text-red-800"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="flex justify-between items-center mt-4">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-3 py-1 border rounded-lg disabled:opacity-40"
            >
              Previous
            </button>
            <span className="text-sm text-gray-600">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-3 py-1 border rounded-lg disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
