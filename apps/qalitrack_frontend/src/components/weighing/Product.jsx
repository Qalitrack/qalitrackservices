import { useEffect, useState } from "react";
import { Pencil, Trash2, PackagePlus, Search, X, Check } from "lucide-react";
import { message, Modal } from "antd";
import TablePagination from "../TablePagination";
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
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

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
      message.error(`Error: ${err.message}`);
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

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Are you sure you want to delete this product?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        setLoading(true);
        try {
          await deleteProduct(id);
          await fetchProducts();
        } catch (err) {
          message.error(err.message || "Failed to delete product");
        } finally {
          setLoading(false);
        }
      },
    });
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

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header — navy app-bar (Navy-theme experiment, see Transaction.jsx) */}
      <div className="px-3 py-2" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md cs-icon-box flex items-center justify-center shadow-sm">
              <PackagePlus className="w-4 h-4" style={{ color: "var(--cs-icon-accent)" }} />
            </div>
            <div>
              <span className="text-[11px] font-bold block leading-tight" style={{ color: "var(--cs-appbar-text)" }}>
                Products
              </span>
              <span className="text-[9px] font-medium" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
                {products.length} products available
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search products..."
                className="qt-filter-field w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <button
              onClick={() => {
                setSearch("");
                setPage(1);
                fetchProducts();
              }}
              className="h-7 px-3 text-[11px] rounded-md cs-ghost-btn shadow-sm font-medium"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
        <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Product Name <span style={{ color: "var(--cs-required)" }}>*</span>
            </label>
            <input
              name="name"
              value={form.name}
              onChange={handleChange}
              required
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="Product name"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Product Code
            </label>
            <input
              name="code"
              value={form.code}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="Product code"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Unit
            </label>
            <input
              name="unit"
              value={form.unit}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="e.g., kg, pcs"
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
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
            >
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </div>

          <div className="col-span-4">
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Description
            </label>
            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
              rows={2}
              className="qt-filter-field w-full text-[11px] rounded border border-gray-300 px-2 py-1 resize-none"
              placeholder="Product description"
            />
          </div>

          <div className="col-span-4 flex gap-2 justify-end mt-1">
            {editingProduct && (
              <button
                type="button"
                onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1"
              >
                <X className="w-3 h-3" />
                Cancel
              </button>
            )}
            <button
              type="submit"
              disabled={loading}
              className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1"
            >
              <PackagePlus className="w-3 h-3" />
              {editingProduct ? "Update" : "Add"} Product
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading products...</p>
          </div>
        ) : error ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-red-500 text-sm">Error: {error}</p>
          </div>
        ) : products.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <PackagePlus className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500 text-sm">No products found.</p>
              <p className="text-gray-400 text-xs mt-1">Add a product using the form above</p>
            </div>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Code</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Unit</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Description</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((p, index) => (
                <tr
                  key={p.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">{p.name}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 font-mono font-medium">{p.code || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{p.unit || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 max-w-xs truncate">{p.description || "-"}</td>
                  <td className="px-3 py-2">
                    <span
                      className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase inline-flex items-center gap-0.5 ${
                        p.status === "Active"
                          ? "bg-green-100 text-green-700 border border-green-300"
                          : "bg-red-100 text-red-700 border border-red-300"
                      }`}
                    >
                      {p.status === "Active"
                        ? <><Check className="w-2.5 h-2.5" /> Active</>
                        : <><X className="w-2.5 h-2.5" /> Inactive</>}
                    </span>
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        onClick={() => handleEdit(p)}
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(p.id)}
                        className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                        title="Delete"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
        <TablePagination page={page} totalPages={totalPages} onPageChange={setPage} />
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