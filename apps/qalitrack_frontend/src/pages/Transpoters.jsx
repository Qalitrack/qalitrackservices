import React, { useEffect, useState } from "react";
import {
  getTransporters,
  deleteTransporter
} from "../api/MasterData/Transporters";
import TransporterFormModal from "./weighing/TransporterFormModal";

const Transporters = () => {
  const [transporters, setTransporters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTransporter, setEditingTransporter] = useState(null);

  const fetchTransporters = async () => {
    try {
      setLoading(true);
      const response = await getTransporters();
      setTransporters(response?.items || []);
      setError(null);
    } catch (err) {
      console.error("Error fetching transporters:", err);
      setError("Failed to load transporters.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransporters();
  }, []);

  // Handle Add
  const handleAdd = () => {
    setEditingTransporter(null);
    setIsModalOpen(true);
  };

  // Handle Edit
  const handleEdit = (transporter) => {
    setEditingTransporter(transporter);
    setIsModalOpen(true);
  };

  // Handle Delete
  const handleDelete = async (id, name) => {
    if (window.confirm(`Are you sure you want to delete "${name}"?`)) {
      try {
        await deleteTransporter(id);
        alert("Transporter deleted successfully!");
        fetchTransporters();
      } catch (err) {
        console.error("Error deleting transporter:", err);
        alert("Failed to delete transporter.");
      }
    }
  };

  // Handle Modal Close
  const handleModalClose = (shouldRefresh) => {
    setIsModalOpen(false);
    setEditingTransporter(null);
    if (shouldRefresh) {
      fetchTransporters();
    }
  };

  if (loading) return <p className="p-6">Loading transporters...</p>;
  if (error) return <p className="p-6 text-red-600">{error}</p>;

  return (
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-semibold text-amber-600">Registered Transporters</h1>
          <button
              onClick={handleAdd}
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2"
          >
            <span>➕</span> Add Transporter
          </button>
        </div>

        {transporters.length === 0 ? (
            <p>No transporters found.</p>
        ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full border border-gray-300 rounded-md bg-white shadow">
                <thead className="bg-gray-100">
                <tr>
                  <th className="p-3 border text-left">Logo</th>
                  <th className="p-3 border text-left">Name</th>
                  <th className="p-3 border text-left">Contact Info</th>
                  <th className="p-3 border text-left">Status</th>
                  <th className="p-3 border text-center">Actions</th>
                </tr>
                </thead>
                <tbody>
                {transporters.map((t) => (
                    <tr key={t.id} className="hover:bg-gray-50">
                      <td className="p-3 border">
                        {t.logo ? (
                            <img src={t.logo} alt={t.name} className="w-10 h-10 rounded-full object-cover" />
                        ) : (
                            <div className="w-10 h-10 rounded-full bg-blue-500 text-white flex items-center justify-center font-bold">
                              {t.name?.charAt(0).toUpperCase()}
                            </div>
                        )}
                      </td>
                      <td className="p-3 border font-semibold">{t.name}</td>
                      <td className="p-3 border">
                        {(() => {
                          try {
                            const contact = typeof t.contactInfo === 'string'
                                ? JSON.parse(t.contactInfo)
                                : t.contactInfo;
                            return (
                                <div className="text-sm">
                                  {contact.Email && <div>📧 {contact.Email}</div>}
                                  {contact.Phone && <div>📱 {contact.Phone}</div>}
                                  {contact.Address && <div>📍 {contact.Address}</div>}
                                  {contact.LicenseNumber && <div>🆔 {contact.LicenseNumber}</div>}
                                </div>
                            );
                          } catch (e) {
                            return t.contactInfo || '-';
                          }
                        })()}
                      </td>
                      <td className="p-3 border">
                    <span className={`px-2 py-1 rounded text-sm ${
                        t.status?.toLowerCase() === 'active'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                    }`}>
                      {t.status || "—"}
                    </span>
                      </td>
                      <td className="p-3 border text-center">
                        <div className="flex gap-2 justify-center">
                          <button
                              onClick={() => handleEdit(t)}
                              className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded text-sm"
                          >
                            ✏️ Edit
                          </button>
                          <button
                              onClick={() => handleDelete(t.id, t.name)}
                              className="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded text-sm"
                          >
                            🗑️ Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                ))}
                </tbody>
              </table>
            </div>
        )}

        {/* Use the TransporterFormModal component we created */}
        <TransporterFormModal
            open={isModalOpen}
            transporter={editingTransporter}
            onClose={handleModalClose}
        />
      </div>
  );
};

export default Transporters;