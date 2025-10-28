import React, { useEffect, useState } from "react";
import { getTransporters } from "../api/MasterData/Transporters";

const Transporters = () => {
  const [transporters, setTransporters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTransporters = async () => {
      try {
        const data = await getTransporters();
        setTransporters(data || []);
      } catch (err) {
        console.error("Error fetching transporters:", err);
        setError("Failed to load transporters.");
      } finally {
        setLoading(false);
      }
    };
    fetchTransporters();
  }, []);

  if (loading) return <p>Loading transporters...</p>;
  if (error) return <p className="text-red-600">{error}</p>;

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold text-amber-600 mb-4">Registered Transporters</h1>

      {transporters.length === 0 ? (
        <p>No transporters found.</p>
      ) : (
        <table className="min-w-full border border-gray-300 rounded-md">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Contact Info</th>
              <th className="p-2 border">Status</th>
            </tr>
          </thead>
          <tbody>
            {transporters.map((t) => (
              <tr key={t.id} className="hover:bg-gray-50">
                <td className="p-2 border">{t.name}</td>
                <td className="p-2 border">{t.contactInfo}</td>
                <td className="p-2 border">{t.status || "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Transporters;
