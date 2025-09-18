'use client';

import { useState, useEffect } from "react";

const UserFormModal = ({ open, onClose, onSubmit, initialData }) => {
  const isEdit = !!initialData;

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
    role: "Client",
    status: "Active",
  });

  useEffect(() => {
    if (initialData) {
      setForm({ ...initialData, password: "" }); // Never prefill password
    } else {
      setForm({
        name: "",
        email: "",
        password: "",
        role: "Client",
        status: "Active",
      });
    }
  }, [initialData]);

  if (!open) return null;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(form, isEdit);
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-lg max-w-md w-full p-6">
        <h2 className="text-xl font-bold mb-4">
          {isEdit ? "Edit User" : "Add New User"}
        </h2>

        <form onSubmit={handleSubmit} className="space-y-3">
          <input
            name="name"
            placeholder="Name"
            value={form.name}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />
          <input
            type="email"
            name="email"
            placeholder="Email"
            value={form.email}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required
          />
          <input
            type="password"
            name="password"
            placeholder={isEdit ? "Leave blank to keep current password" : "Password"}
            value={form.password}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
            required={!isEdit}
          />

          {/* Role */}
          <select
            name="role"
            value={form.role}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
          >
            <option>Admin</option>
            <option>Client</option>
            <option>Guest</option>
          </select>

          {/* Status */}
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            className="w-full border rounded-md p-2"
          >
            <option>Active</option>
            <option>Pending</option>
            <option>Inactive</option>
          </select>

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
              className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md"
            >
              {isEdit ? "Update" : "Create"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default UserFormModal;
