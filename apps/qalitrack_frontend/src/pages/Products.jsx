import React, { useEffect, useState } from "react";
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
} from "../api/MasterData/Products";

const Products = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchAllProducts = async () => {
    try {
      setLoading(true);
      const data = await getProducts();
      setProducts(data);
    } catch (err) {
      console.error("Error fetching products:", err);
      setError("Failed to load products.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAllProducts();
  }, []);

  const handleAdd = async () => {
    const newProduct = {
      code: "P001",
      name: "New Product",
      description: "Description here",
      image: "",
    };
    try {
      const created = await createProduct(newProduct);
      setProducts((prev) => [...prev, created]);
    } catch (err) {
      console.error(err);
      setError("Failed to add product.");
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Delete this product?")) return;
    try {
      await deleteProduct(id);
      setProducts((prev) => prev.filter((p) => p.id !== id));
    } catch (err) {
      console.error(err);
      setError("Failed to delete product.");
    }
  };

  if (loading) return <p>Loading products...</p>;
  if (error) return <p className="text-red-600">{error}</p>;

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold mb-4">Products</h1>
      <button
        onClick={handleAdd}
        className="mb-4 px-4 py-2 bg-amber-600 text-white rounded hover:bg-amber-700"
      >
        + Add Product
      </button>

      {products.length === 0 ? (
        <p>No products found.</p>
      ) : (
        <table className="min-w-full border border-gray-300 rounded-md">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 border">Code</th>
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Description</th>
              <th className="p-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="p-2 border">{p.code}</td>
                <td className="p-2 border">{p.name}</td>
                <td className="p-2 border">{p.description || "—"}</td>
                <td className="p-2 border space-x-2">
                  <button
                    onClick={() => handleDelete(p.id)}
                    className="px-2 py-1 bg-red-600 text-white rounded hover:bg-red-700"
                  >
                    Delete
                  </button>
                  {/* Optional: Add Edit button here */}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Products;
