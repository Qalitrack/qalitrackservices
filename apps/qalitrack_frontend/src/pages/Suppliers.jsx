import React, { useEffect, useState } from "react";
import { getSuppliers } from "../api/MasterData/Suppliers";

const Suppliers = () => {
  const [suppliers, setSuppliers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchSuppliers = async () => {
      try {
        const data = await getSuppliers();
        setSuppliers(data || []);
      } catch (err) {
        console.error("Error fetching suppliers:", err);
        setError("Failed to load suppliers.");
      } finally {
        setLoading(false);
      }
    };
    fetchSuppliers();
  }, []);

  if (loading) return <p>Loading suppliers...</p>;
  if (error) return <p className="text-red-600">{error}</p>;

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold text-amber-600 mb-4">Registered Suppliers</h1>

      {suppliers.length === 0 ? (
        <p>No suppliers found.</p>
      ) : (
        <table className="min-w-full border border-gray-300 rounded-md">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Contact Person</th>
              <th className="p-2 border">Email</th>
              <th className="p-2 border">Phone</th>
              <th className="p-2 border">City</th>
            </tr>
          </thead>
          <tbody>
            {suppliers.map((s) => (
              <tr key={s.id} className="hover:bg-gray-50">
                <td className="p-2 border">{s.name}</td>
                <td className="p-2 border">{s.contactPerson}</td>
                <td className="p-2 border">{s.email}</td>
                <td className="p-2 border">{s.phone}</td>
                <td className="p-2 border">{s.city}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Suppliers;
