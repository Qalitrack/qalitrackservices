// src/pages/Saccos.jsx
import React, { useEffect, useState } from "react";
import { Plus, Trash2, Pencil, Search } from "lucide-react";
import { getSaccos, createSacco, deleteSacco } from "../api/MasterData/Saccos";

const SaccosPortal = () => {
  const [saccos, setSaccos] = useState([]);
  const [filteredSaccos, setFilteredSaccos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [showAddModal, setShowAddModal] = useState(false);
  const [newSacco, setNewSacco] = useState({
    name: "",
    registrationNumber: "",
    members: "",
    otherDetails: "",
  });

  useEffect(() => {
    fetchSaccos();
  }, []);

  const fetchSaccos = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await getSaccos();
      setSaccos(data);
      setFilteredSaccos(data);
    } catch (err) {
      console.error("Error fetching saccos:", err);
      setError("Failed to load saccos. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e) => {
    const term = e.target.value.toLowerCase();
    setSearch(term);
    const filtered = saccos.filter(
      (sacco) =>
        sacco.name?.toLowerCase().includes(term) ||
        sacco.contactInfo?.RegistrationNumber?.toLowerCase().includes(term)
    );
    setFilteredSaccos(filtered);
  };

  const handleAddSacco = async () => {
    if (!newSacco.name || !newSacco.registrationNumber) return;

    try {
      const data = await createSacco({
        name: newSacco.name,
        contactInfo: {
          RegistrationNumber: newSacco.registrationNumber,
          Members: newSacco.members.split(",").map((m) => m.trim()),
        },
        otherDetails: newSacco.otherDetails ? JSON.parse(newSacco.otherDetails) : {},
      });
      setSaccos([...saccos, data]);
      setFilteredSaccos([...filteredSaccos, data]);
      setShowAddModal(false);
      setNewSacco({ name: "", registrationNumber: "", members: "", otherDetails: "" });
    } catch (err) {
      console.error("Error adding sacco:", err);
      alert("Failed to add sacco. Check console for details.");
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this sacco?")) return;

    try {
      await deleteSacco(id);
      const updated = saccos.filter((s) => s.id !== id);
      setSaccos(updated);
      setFilteredSaccos(updated);
    } catch (err) {
      console.error("Error deleting sacco:", err);
      alert("Failed to delete sacco. Check console for details.");
    }
  };

  if (loading) return <p className="p-4">Loading saccos...</p>;
  if (error) return <p className="p-4 text-red-600">{error}</p>;

  return (
    <div className="p-4">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-bold">Saccos Portal</h2>
        <button
          className="flex items-center gap-2 bg-amber-500 text-white px-3 py-1 rounded hover:bg-amber-600"
          onClick={() => setShowAddModal(true)}
        >
          <Plus size={16} /> Add Sacco
        </button>
      </div>

      {/* Search Input */}
      <div className="mb-4">
        <div className="flex items-center border border-gray-300 rounded">
          <Search className="ml-2 text-gray-400" size={16} />
          <input
            type="text"
            value={search}
            onChange={handleSearch}
            placeholder="Search by name or registration number..."
            className="w-full p-2 outline-none"
          />
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full border border-gray-200">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-3 py-2 text-left">Name</th>
              <th className="px-3 py-2 text-left">Registration Number</th>
              <th className="px-3 py-2 text-left">Members</th>
              <th className="px-3 py-2 text-left">Other Details</th>
              <th className="px-3 py-2 text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredSaccos.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-3 py-2 text-center">
                  No saccos found.
                </td>
              </tr>
            ) : (
              filteredSaccos.map((sacco) => (
                <tr key={sacco.id} className="border-t border-gray-200">
                  <td className="px-3 py-2">{sacco.name || "N/A"}</td>
                  <td className="px-3 py-2">
                    {sacco.contactInfo?.RegistrationNumber || "N/A"}
                  </td>
                  <td className="px-3 py-2">
                    {sacco.contactInfo?.Members?.length
                      ? sacco.contactInfo.Members.join(", ")
                      : "N/A"}
                  </td>
                  <td className="px-3 py-2">
                    {sacco.otherDetails
                      ? JSON.stringify(sacco.otherDetails)
                      : "N/A"}
                  </td>
                  <td className="px-3 py-2 flex gap-2">
                    <button className="p-1 text-amber-500 border border-amber-500 rounded hover:bg-amber-50">
                      <Pencil size={16} />
                    </button>
                    <button
                      className="p-1 text-red-500 border border-red-500 rounded hover:bg-red-50"
                      onClick={() => handleDelete(sacco.id)}
                    >
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Add Sacco Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-30 flex justify-center items-center z-50">
          <div className="bg-white rounded shadow-lg p-6 w-full max-w-md">
            <h3 className="text-lg font-bold mb-4">Add New Sacco</h3>
            <div className="flex flex-col gap-3">
              <input
                type="text"
                placeholder="Sacco Name"
                value={newSacco.name}
                onChange={(e) => setNewSacco({ ...newSacco, name: e.target.value })}
                className="p-2 border border-gray-300 rounded"
              />
              <input
                type="text"
                placeholder="Registration Number"
                value={newSacco.registrationNumber}
                onChange={(e) =>
                  setNewSacco({ ...newSacco, registrationNumber: e.target.value })
                }
                className="p-2 border border-gray-300 rounded"
              />
              <input
                type="text"
                placeholder="Members (comma separated)"
                value={newSacco.members}
                onChange={(e) => setNewSacco({ ...newSacco, members: e.target.value })}
                className="p-2 border border-gray-300 rounded"
              />
              <textarea
                placeholder='Other Details (JSON format, optional)'
                value={newSacco.otherDetails}
                onChange={(e) => setNewSacco({ ...newSacco, otherDetails: e.target.value })}
                className="p-2 border border-gray-300 rounded"
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button
                className="px-4 py-2 border rounded hover:bg-gray-100"
                onClick={() => setShowAddModal(false)}
              >
                Cancel
              </button>
              <button
                className="px-4 py-2 bg-amber-500 text-white rounded hover:bg-amber-600"
                onClick={handleAddSacco}
              >
                Add Sacco
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SaccosPortal;
