import { useEffect, useState } from "react";
import { Plus, Pencil, Trash2 } from "lucide-react";
import { message, Modal } from "antd";
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
} from "../api/MasterData/Products";

export default function Products() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  const [form, setForm] = useState({
    code: "",
    name: "",
    description: "",
  });

  const [editingProduct, setEditingProduct] = useState(null);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await getProducts();
      let filtered = data || [];

      if (searchTerm) {
        filtered = filtered.filter(
          (p) =>
            p.code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
            p.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
            p.description?.toLowerCase().includes(searchTerm.toLowerCase())
        );
      }

      setProducts(filtered);
    } catch (error) {
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [searchTerm]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.code.trim() || !form.name.trim()) {
      message.warning("Code and Name are required!");
      return;
    }

    try {
      setLoading(true);
      if (editingProduct) {
        await updateProduct(editingProduct.id, form);
      } else {
        await createProduct(form);
      }
      resetForm();
      await fetchProducts();
    } catch (error) {
      message.error("Error saving product: " + (error.message || "Unknown error"));
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (product) => {
    setEditingProduct(product);
    setForm({
      code: product.code || "",
      name: product.name || "",
      description: product.description || "",
    });
    // Auto-scroll form into view if needed
    window.scrollTo(0, 0);
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Delete this product?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteProduct(id);
          await fetchProducts();
        } catch (err) {
          message.error(err.message || "Failed to delete product");
        }
      },
    });
  };

  const resetForm = () => {
    setForm({ code: "", name: "", description: "" });
    setEditingProduct(null);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow border overflow-hidden">
      {/* Header + Form */}
      <div className="p-3 border-b">
        <h2 className="text-lg font-bold text-amber-600 mb-3 flex items-center gap-2">
          <Plus className="w-5 h-5" />
          Products Management
        </h2>

        {/* Compact Form */}
        <form onSubmit={handleSubmit} className="grid grid-cols-1 sm:grid-cols-3 gap-2 text-xs">
          <div>
            <label className="block font-medium text-gray-700">Code *</label>
            <input
              name="code"
              value={form.code}
              onChange={handleChange}
              className="w-full mt-1 px-2 py-1 border rounded text-sm focus:outline-none focus:ring-1 focus:ring-amber-500"
              required
              autoComplete="off"
            />
          </div>

          <div>
            <label className="block font-medium text-gray-700">Name *</label>
            <input
              name="name"
              value={form.name}
              onChange={handleChange}
              className="w-full mt-1 px-2 py-1 border rounded text-sm focus:outline-none focus:ring-1 focus:ring-amber-500"
              required
              autoComplete="off"
            />
          </div>

          <div>
            <label className="block font-medium text-gray-700">Description</label>
            <input
              name="description"
              value={form.description}
              onChange={handleChange}
              className="w-full mt-1 px-2 py-1 border rounded text-sm focus:outline-none focus:ring-1 focus:ring-amber-500"
              autoComplete="off"
            />
          </div>

          <div className="sm:col-span-3 flex gap-2 mt-2">
            <button
              type="submit"
              disabled={loading}
              className="px-3 py-1.5 bg-amber-500 hover:bg-amber-600 disabled:opacity-70 text-white text-sm font-medium rounded flex items-center gap-1.5"
            >
              <Plus className="w-4 h-4" />
              {editingProduct ? "Update" : "Add"}
            </button>

            {editingProduct && (
              <button
                type="button"
                onClick={resetForm}
                className="px-3 py-1.5 border border-gray-300 rounded text-gray-700 text-sm hover:bg-gray-50"
              >
                Cancel
              </button>
            )}
          </div>
        </form>

        {/* Search */}
        <input
          type="text"
          placeholder="Search products..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full mt-3 px-3 py-1.5 border rounded text-sm focus:outline-none focus:ring-1 focus:ring-amber-500"
        />
      </div>

      {/* Scrollable Table */}
      <div className="flex-1 overflow-auto">
        {loading ? (
          <div className="flex items-center justify-center h-32 text-gray-500 text-sm">
            Loading...
          </div>
        ) : products.length === 0 ? (
          <div className="flex items-center justify-center h-32 text-gray-500 text-sm">
            {searchTerm ? "No matching products" : "No products yet"}
          </div>
        ) : (
          <table className="w-full text-xs">
            <thead className="bg-gray-100 sticky top-0">
              <tr>
                <th className="px-3 py-2 text-left font-medium text-gray-700">Code</th>
                <th className="px-3 py-2 text-left font-medium text-gray-700">Name</th>
                <th className="px-3 py-2 text-left font-medium text-gray-700">Description</th>
                <th className="px-3 py-2 text-center font-medium text-gray-700 w-20">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {products.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 font-medium">{p.code}</td>
                  <td className="px-3 py-2">{p.name}</td>
                  <td className="px-3 py-2 text-gray-600 truncate max-w-xs">
                    {p.description || "—"}
                  </td>
                  <td className="px-3 py-2 text-center">
                    <div className="flex justify-center gap-3">
                      <button
                        onClick={() => handleEdit(p)}
                        className="text-blue-600 hover:text-blue-800"
                        title="Edit"
                      >
                        <Pencil className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => handleDelete(p.id)}
                        className="text-red-600 hover:text-red-800"
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
        )}
      </div>
    </div>
  );
}