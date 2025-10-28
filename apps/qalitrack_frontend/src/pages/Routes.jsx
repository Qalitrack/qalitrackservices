import React, { useEffect, useState } from "react";
import { getRoutes } from "../api/MasterData/Routes";

const Routes = () => {
  const [routes, setRoutes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchRoutes = async () => {
      try {
        const data = await getRoutes();
        setRoutes(data || []);
      } catch (err) {
        console.error("Error fetching routes:", err);
        setError("Failed to load routes.");
      } finally {
        setLoading(false);
      }
    };
    fetchRoutes();
  }, []);

  if (loading) return <p>Loading routes...</p>;
  if (error) return <p className="text-red-600">{error}</p>;

  return (
    <div className="p-6">
      <h1 className="text-xl font-semibold mb-4">Registered Routes</h1>

      {routes.length === 0 ? (
        <p>No routes found.</p>
      ) : (
        <table className="min-w-full border border-gray-300 rounded-md">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Start Point</th>
              <th className="p-2 border">End Point</th>
              <th className="p-2 border">Status</th>
              <th className="p-2 border">Created At</th>
            </tr>
          </thead>
          <tbody>
            {routes.map((r) => (
              <tr key={r.id} className="hover:bg-gray-50">
                <td className="p-2 border">{r.name}</td>
                <td className="p-2 border">{r.startPoint}</td>
                <td className="p-2 border">{r.endPoint}</td>
                <td className="p-2 border">{r.status || "—"}</td>
                <td className="p-2 border">
                  {r.createdAt ? new Date(r.createdAt).toLocaleDateString() : "—"}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Routes;
