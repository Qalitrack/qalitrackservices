'use client';

import { useState, useEffect } from "react";

import { usersAPI } from '../../utils/apiFactory';

const MyProfile = ({ userId }) => {
  const [user, setUser] = useState(null);
  const [form, setForm] = useState({ name: "", email: "", password: "", profileImage: "" });
  const [preview, setPreview] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  // ✅ Fetch user profile
  const fetchProfile = async () => {
    setLoading(true);
    try {
      const res = await usersAPI.getById(userId);
      setUser(res.data);
      setForm({
        name: res.data.name,
        email: res.data.email,
        password: "",
        profileImage: res.data.profileImage || "",
      });
      setPreview(res.data.profileImage || "");
    } catch (err) {
      setMessage("Failed to load profile.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (userId) fetchProfile();
  }, [userId]);

  // ✅ Handle form change
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // ✅ Handle image upload
  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      setPreview(reader.result);
      setForm((prev) => ({ ...prev, profileImage: reader.result }));
    };
    reader.readAsDataURL(file);
  };

  // ✅ Save profile
  const handleSave = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");
    try {
      await usersAPI.update(userId, form);
      setMessage("Profile updated successfully!");
      fetchProfile(); // Refresh profile
    } catch (err) {
      setMessage("Failed to update profile.");
    } finally {
      setLoading(false);
    }
  };

  if (loading && !user) return <p className="text-center mt-6">Loading profile...</p>;

  return (
    <div className="max-w-2xl mx-auto bg-white rounded-lg shadow p-6">
      <h2 className="text-2xl font-bold mb-4 text-center">👤 My Profile</h2>

      {/* Message */}
      {message && (
        <p className={`text-center mb-4 ${message.includes("success") ? "text-green-600" : "text-red-500"}`}>
          {message}
        </p>
      )}

      <form onSubmit={handleSave} className="space-y-4">
        {/* Profile Image */}
        <div className="flex flex-col items-center gap-3">
          <img
            src={preview || "https://via.placeholder.com/100"}
            alt="Profile"
            className="w-24 h-24 rounded-full object-cover border"
          />
          <input type="file" accept="image/*" onChange={handleImageUpload} />
        </div>

        {/* Name */}
        <div>
          <label className="block text-gray-700 text-sm mb-1">Name</label>
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            className="w-full border rounded-md px-3 py-2"
            required
          />
        </div>

        {/* Email (read-only) */}
        <div>
          <label className="block text-gray-700 text-sm mb-1">Email</label>
          <input
            name="email"
            value={form.email}
            disabled
            className="w-full border rounded-md px-3 py-2 bg-gray-100 cursor-not-allowed"
          />
        </div>

        {/* Password (optional) */}
        <div>
          <label className="block text-gray-700 text-sm mb-1">New Password</label>
          <input
            type="password"
            name="password"
            placeholder="Leave blank to keep current password"
            value={form.password}
            onChange={handleChange}
            className="w-full border rounded-md px-3 py-2"
          />
        </div>

        {/* Role & Status (read-only for normal users) */}
        {user && (
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-gray-700 text-sm mb-1">Role</label>
              <input
                value={user.role}
                disabled
                className="w-full border rounded-md px-3 py-2 bg-gray-100 cursor-not-allowed"
              />
            </div>
            <div>
              <label className="block text-gray-700 text-sm mb-1">Status</label>
              <input
                value={user.status}
                disabled
                className="w-full border rounded-md px-3 py-2 bg-gray-100 cursor-not-allowed"
              />
            </div>
          </div>
        )}

        {/* Save Button */}
        <div className="flex justify-center">
          <button
            type="submit"
            disabled={loading}
            className="bg-blue-500 hover:bg-blue-600 text-white px-6 py-2 rounded-md disabled:opacity-50"
          >
            {loading ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </form>
    </div>
  );
};

export default MyProfile;
