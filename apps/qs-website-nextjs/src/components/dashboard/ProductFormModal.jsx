'use client';

import { useState, useEffect } from "react";

const ProductFormModal = ({ open, onClose, onSubmit, initialData }) => {
  const isEdit = !!initialData;

  const [form, setForm] = useState({
    name: "",
    category: "",
    stock: 0,
    price: 0,
    status: "Active",
    description: "",
    image: "",
  });
  const [filePreview, setFilePreview] = useState("");

  // Populate fields when editing
  useEffect(() => {
    if (initialData) {
      setForm(initialData);
      setFilePreview(initialData.image || "");
    } else {
      setForm({
        name: "",
        category: "",
        stock: 0,
        price: 0,
        status: "Active",
        description: "",
        image: "",
      });
      setFilePreview("");
    }
  }, [initialData]);

  if (!open) return null;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      setFilePreview(reader.result);
      setForm((prev) => ({ ...prev, image: reader.result }));
    };
    reader.readAsDataURL(file);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(form, isEdit);
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-lg max-w-md w-full p-6 overflow-y-auto">
        <h2 className="text-xl font-bold mb-4">
          {isEdit ? "Edit Product" : "Add New Product"}
        </h2>

        <form onSubmit={handleSubmit} className="space-y-3">
          <input
            name="name"
            placeholder="Product Name"
            value={form.name}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />
          <input
            name="category"
            placeholder="Category"
            value={form.category}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />
          <input
            type="number"
            name="stock"
            placeholder="Stock"
            value={form.stock}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />
          <input
            type="number"
            name="price"
            placeholder="Price (Ksh)"
            value={form.price}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />

          {/* Status */}
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
          >
            <option>Active</option>
            <option>Low Stock</option>
            <option>Inactive</option>
          </select>

          {/* Description */}
          <textarea
            name="description"
            placeholder="Description"
            value={form.description}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            rows="3"
          />

          {/* Image Upload */}
          <input
            type="file"
            accept="image/*"
            onChange={handleImageUpload}
            className="w-full"
          />
          {filePreview && (
            <img
              src={filePreview}
              alt="Preview"
              className="w-24 h-24 object-cover mt-2 rounded-md border"
            />
          )}

          {/* Buttons */}
          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="bg-gray-300 hover:bg-gray-400 text-gray-800 px-4 py-2 rounded-md"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-md"
            >
              {isEdit ? "Update" : "Create"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ProductFormModal;
