import React, { useEffect, useState, useMemo } from "react";
import { Plus, Trash2, Pencil, Search, X, Building2 } from "lucide-react";
import { getSaccos, createSacco, deleteSacco } from "../api/MasterData/Saccos";

const PAGE_SIZE = 5;

const SaccosPortal = () => {
  const [saccos, setSaccos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
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
    } catch (err) {
      console.error("Error fetching saccos:", err);
      setError("Failed to load saccos. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const t = search.toLowerCase();
    if (!t) return saccos;
    return saccos.filter(
      (s) =>
        s.name?.toLowerCase().includes(t) ||
        s.contactInfo?.RegistrationNumber?.toLowerCase().includes(t)
    );
  }, [saccos, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => {
    setPage(1);
  }, [search]);

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
      setSaccos((prev) => [...prev, data]);
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
      setSaccos((prev) => prev.filter((s) => s.id !== id));
    } catch (err) {
      console.error("Error deleting sacco:", err);
      alert("Failed to delete sacco. Check console for details.");
    }
  };

  if (loading)
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-gray-500">Loading saccos...</p>
      </div>
    );
  if (error)
    return (
      <div className="flex items-center justify-center h-screen">
        <p className="text-red-600">{error}</p>
      </div>
    );

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <Building2 className="w-4 h-4 text-white" />
            </div>
            <div>
              <span className="text-[11px] font-bold text-gray-900 block leading-tight">
                Saccos
              </span>
              <span className="text-[9px] text-amber-700 font-medium">
                {filtered.length} organizations
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search saccos..."
                className="w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border-gray-300 focus:border-amber-500 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <button
              onClick={() => setShowAddModal(true)}
              className="h-7 px-3 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all flex items-center gap-1"
            >
              <Plus className="w-3 h-3" />
              Add Sacco
            </button>
          </div>
        </div>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {paginated.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">No saccos found.</p>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Registration Number</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Members</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Other Details</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginated.map((sacco, index) => (
                <tr
                  key={sacco.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">{sacco.name || "N/A"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 font-mono font-medium">
                    {sacco.contactInfo?.RegistrationNumber || "N/A"}
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">
                    {sacco.contactInfo?.Members?.length ? sacco.contactInfo.Members.join(", ") : "N/A"}
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 max-w-xs truncate">
                    {sacco.otherDetails ? JSON.stringify(sacco.otherDetails) : "N/A"}
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(sacco.id)}
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
      </div>

      {/* Footer with Pagination */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <span className="text-[10px] text-gray-600 font-medium">
          Page <span className="font-semibold text-amber-600">{page}</span> of{" "}
          <span className="font-semibold text-amber-600">{totalPages}</span>
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Previous
          </button>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Next
          </button>
        </div>
      </div>

      {/* Add Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
          <div className="bg-white rounded-lg shadow-2xl w-full max-w-md border-2 border-amber-300">
            <div className="bg-gradient-to-r from-amber-500 to-orange-600 px-4 py-2.5 rounded-t-lg flex items-center justify-between">
              <h3 className="text-sm font-bold text-white flex items-center gap-2">
                <Building2 className="w-4 h-4" /> Add New Sacco
              </h3>
              <button
                onClick={() => setShowAddModal(false)}
                className="text-white hover:bg-white/20 rounded-lg p-1 transition-all"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
            <div className="p-4 flex flex-col gap-3">
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Sacco Name *
                </label>
                <input
                  type="text"
                  placeholder="Enter sacco name"
                  value={newSacco.name}
                  onChange={(e) => setNewSacco({ ...newSacco, name: e.target.value })}
                  className="w-full h-7 text-[11px] border border-amber-300 rounded px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 transition-all"
                />
              </div>
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Registration Number *
                </label>
                <input
                  type="text"
                  placeholder="Enter reg. number"
                  value={newSacco.registrationNumber}
                  onChange={(e) => setNewSacco({ ...newSacco, registrationNumber: e.target.value })}
                  className="w-full h-7 text-[11px] border border-amber-300 rounded px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 transition-all"
                />
              </div>
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Members (comma separated)
                </label>
                <input
                  type="text"
                  placeholder="John Doe, Jane Smith"
                  value={newSacco.members}
                  onChange={(e) => setNewSacco({ ...newSacco, members: e.target.value })}
                  className="w-full h-7 text-[11px] border border-amber-300 rounded px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 transition-all"
                />
              </div>
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Other Details (optional)
                </label>
                <textarea
                  placeholder='{"key": "value"}'
                  value={newSacco.otherDetails}
                  onChange={(e) => setNewSacco({ ...newSacco, otherDetails: e.target.value })}
                  className="w-full text-[11px] border border-amber-300 rounded px-2 py-1.5 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 resize-none transition-all"
                  rows={3}
                />
              </div>
              <div className="flex gap-2 mt-2">
                <button
                  onClick={() => setShowAddModal(false)}
                  className="flex-1 h-7 text-[11px] font-semibold border border-gray-300 rounded hover:bg-gray-100 transition-all"
                >
                  Cancel
                </button>
                <button
                  onClick={handleAddSacco}
                  className="flex-1 h-7 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all"
                >
                  Add Sacco
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

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
};

export default SaccosPortal;