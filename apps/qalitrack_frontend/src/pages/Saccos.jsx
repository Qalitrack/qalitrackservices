import React, { useEffect, useState } from "react";
import { Plus, Trash2, Pencil, Search, X, Building2 } from "lucide-react";
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

  if (loading) return <div className="flex items-center justify-center h-screen"><p className="text-gray-500">Loading saccos...</p></div>;
  if (error) return <div className="flex items-center justify-center h-screen"><p className="text-red-600">{error}</p></div>;

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <Building2 className="w-4 h-4 text-black font-bold" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Saccos</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{filteredSaccos.length}</span> organizations
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input 
              type="text"
              placeholder="Search saccos..." 
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500" 
              value={search}
              onChange={handleSearch} 
            />
          </div>
          <button
            onClick={() => setShowAddModal(true)}
            className="h-8 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded shadow-lg transition-all flex items-center gap-1.5"
          >
            <Plus className="w-3.5 h-3.5" />
            Add Sacco
          </button>
        </div>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden">
          {filteredSaccos.length === 0 ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">No saccos found.</p>
            </div>
          ) : (
            <div className="overflow-auto h-full">
              <table className="w-full text-sm">
                <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                  <tr>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Name</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Registration Number</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Members</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Other Details</th>
                    <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-center uppercase tracking-wide">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredSaccos.map((sacco, index) => (
                    <tr 
                      key={sacco.id} 
                      className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                    >
                      <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{sacco.name || "N/A"}</td>
                      <td className="px-4 py-2.5 text-[10px] text-purple-600 font-mono font-semibold">
                        {sacco.contactInfo?.RegistrationNumber || "N/A"}
                      </td>
                      <td className="px-4 py-2.5 text-[10px] text-emerald-700">
                        {sacco.contactInfo?.Members?.length
                          ? sacco.contactInfo.Members.join(", ")
                          : "N/A"}
                      </td>
                      <td className="px-4 py-2.5 text-[10px] text-gray-600 max-w-xs truncate">
                        {sacco.otherDetails
                          ? JSON.stringify(sacco.otherDetails)
                          : "N/A"}
                      </td>
                      <td className="px-4 py-2.5">
                        <div className="flex gap-2 justify-center">
                          <button 
                            className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all"
                            title="Edit"
                          >
                            <Pencil className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => handleDelete(sacco.id)}
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
          )}
        </div>
      </div>

      {/* Add Sacco Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md border-2 border-amber-300">
            {/* Modal Header */}
            <div className="bg-gradient-to-r from-amber-500 to-amber-600 px-5 py-3 rounded-t-xl flex items-center justify-between">
              <h3 className="text-sm font-bold text-black flex items-center gap-2">
                <Building2 className="w-4 h-4" />
                Add New Sacco
              </h3>
              <button
                onClick={() => setShowAddModal(false)}
                className="text-black hover:bg-black/10 rounded-lg p-1 transition-all"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-5">
              <div className="flex flex-col gap-3">
                <div>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>
                    Sacco Name *
                  </label>
                  <input
                    type="text"
                    placeholder="Enter sacco name"
                    value={newSacco.name}
                    onChange={(e) => setNewSacco({ ...newSacco, name: e.target.value })}
                    className="w-full h-8 text-[11px] border-2 border-blue-300 rounded-lg px-3 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                  />
                </div>

                <div>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-purple-500 rounded-full"></span>
                    Registration Number *
                  </label>
                  <input
                    type="text"
                    placeholder="Enter registration number"
                    value={newSacco.registrationNumber}
                    onChange={(e) =>
                      setNewSacco({ ...newSacco, registrationNumber: e.target.value })
                    }
                    className="w-full h-8 text-[11px] border-2 border-purple-300 rounded-lg px-3 focus:border-purple-500 focus:ring-2 focus:ring-purple-200 transition-all"
                  />
                </div>

                <div>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>
                    Members (comma separated)
                  </label>
                  <input
                    type="text"
                    placeholder="John Doe, Jane Smith"
                    value={newSacco.members}
                    onChange={(e) => setNewSacco({ ...newSacco, members: e.target.value })}
                    className="w-full h-8 text-[11px] border-2 border-emerald-300 rounded-lg px-3 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-200 transition-all"
                  />
                </div>

                <div>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-amber-500 rounded-full"></span>
                    Other Details (JSON format, optional)
                  </label>
                  <textarea
                    placeholder='{"key": "value"}'
                    value={newSacco.otherDetails}
                    onChange={(e) => setNewSacco({ ...newSacco, otherDetails: e.target.value })}
                    className="w-full text-[11px] border-2 border-amber-300 rounded-lg px-3 py-2 focus:border-amber-500 focus:ring-2 focus:ring-amber-200 resize-none transition-all"
                    rows={3}
                  />
                </div>
              </div>

              {/* Modal Footer */}
              <div className="flex gap-2 mt-5">
                <button
                  onClick={() => setShowAddModal(false)}
                  className="flex-1 h-8 text-[11px] font-semibold border-2 border-gray-300 rounded-lg hover:bg-gray-100 transition-all"
                >
                  Cancel
                </button>
                <button
                  onClick={handleAddSacco}
                  className="flex-1 h-8 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all"
                >
                  Add Sacco
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SaccosPortal;