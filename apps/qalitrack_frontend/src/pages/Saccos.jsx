import React, { useEffect, useState } from "react";
import { getSaccos } from "../api/MasterData/Saccos";

const Saccos = () => {
  const [saccos, setSaccos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchSaccos = async () => {
      try {
        const data = await getSaccos();
        setSaccos(data || []);
      } catch (err) {
        console.error("Error fetching saccos:", err);
        setError("Failed to load saccos.");
      } finally {
        setLoading(false);
      }
    };
    fetchSaccos();
  }, []);

  if (loading) return <p>Loading saccos...</p>;
  if (error) return <p className="text-red-600">{error}</p>;

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold mb-4">Registered Saccos</h1>

      {saccos.length === 0 ? (
        <p>No saccos found.</p>
      ) : (
        <table className="min-w-full border border-gray-300 rounded-md">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Contact Info</th>
              <th className="p-2 border">Other Details</th>
            </tr>
          </thead>
          <tbody>
            {saccos.map((s) => (
              <tr key={s.id} className="hover:bg-gray-50">
                <td className="p-2 border">{s.name}</td>
                <td className="p-2 border">{s.contactInfo}</td>
                <td className="p-2 border">{s.otherDetails || "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Saccos;
