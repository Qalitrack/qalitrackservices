import { useEffect, useState } from "react";
import { X, Car } from "lucide-react";
import { message } from "antd";
import { getVehicles, patchVehicleRelation } from "../api/MasterData/Vehicles";

// Shared "which vehicles belong to this X" checklist, used by Owners,
// Transporters, and Suppliers. Checking a box assigns the vehicle to
// `entity`; unchecking clears it. A vehicle already linked to a different
// entity is shown (so you can see who has it) but disabled — one vehicle can
// only hold one value for a given relation field at a time.
export default function VehicleRelationModal({ entity, relationField, relationNameField, entityLabel, onClose }) {
  const [allVehicles, setAllVehicles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [busy, setBusy] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const data = await getVehicles(1, 500, "");
      const items = data?.items || data?.data?.items || (Array.isArray(data?.data) ? data.data : []) || (Array.isArray(data) ? data : []);
      setAllVehicles(items);
    } catch { setAllVehicles([]); }
    finally { setLoading(false); }
  };

  useEffect(() => { if (entity) load(); }, [entity?.id]);

  const handleToggle = async (vehicle, checked) => {
    setBusy(true);
    try {
      await patchVehicleRelation(vehicle.id, relationField, checked ? entity.id : null);
      message.success(checked ? "Vehicle assigned" : "Vehicle unassigned");
      await load();
    } catch (err) {
      message.error(err.response?.data?.message || err.response?.data || err.message || "Failed to update vehicle assignment");
    } finally {
      setBusy(false);
    }
  };

  if (!entity) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 backdrop-blur-sm">
      <div className="bg-white rounded-lg shadow-2xl w-full max-w-md border-2 border-amber-300">
        <div className="bg-gradient-to-r from-amber-500 to-amber-600 px-4 py-2.5 rounded-t-lg flex items-center justify-between">
          <h3 className="text-sm font-bold text-white flex items-center gap-2">
            <Car className="w-4 h-4" /> Vehicles – {entity.name}
          </h3>
          <button onClick={onClose} className="text-white hover:bg-white/20 rounded-lg p-1 transition-all">
            <X className="w-4 h-4" />
          </button>
        </div>
        <div className="p-4">
          {loading && allVehicles.length === 0 ? (
            <p className="text-gray-500 text-sm text-center py-4">Loading vehicles...</p>
          ) : allVehicles.length === 0 ? (
            <p className="text-gray-500 text-sm text-center py-4">No vehicles registered in the system.</p>
          ) : (
            <div className="overflow-auto max-h-80">
              <table className="w-full text-sm">
                <thead className="sticky top-0 bg-white">
                  <tr className="border-b-2 border-amber-200">
                    <th className="text-center py-2 w-8"></th>
                    <th className="text-left py-2 text-[10px] font-bold text-amber-700 uppercase">Registration</th>
                    <th className="text-left py-2 text-[10px] font-bold text-amber-700 uppercase">Make / Model</th>
                    <th className="text-left py-2 text-[10px] font-bold text-amber-700 uppercase">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {allVehicles.map((v) => {
                    const isThis = v[relationField] === entity.id;
                    const otherName = !isThis && v[relationField]
                      ? (v[relationNameField] || `another ${entityLabel.toLowerCase()}`)
                      : null;
                    return (
                      <tr key={v.id} className="border-b border-gray-100 hover:bg-amber-50 transition-all">
                        <td className="py-2 text-center">
                          <input
                            type="checkbox"
                            checked={isThis}
                            disabled={busy || !!otherName}
                            onChange={(e) => handleToggle(v, e.target.checked)}
                            className="w-3.5 h-3.5 accent-amber-500 disabled:opacity-40"
                          />
                        </td>
                        <td className="py-2 text-[10px] font-bold text-gray-800">{v.registrationNumber}</td>
                        <td className="py-2 text-[10px] text-gray-700">{v.make} {v.model}</td>
                        <td className="py-2 text-[10px]">
                          {isThis ? (
                            <span className="text-green-700 font-semibold">Assigned</span>
                          ) : otherName ? (
                            <span className="text-gray-400 italic">Assigned to {otherName}</span>
                          ) : (
                            <span className="text-gray-400">Unassigned</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
          <div className="mt-4 flex justify-end">
            <button
              onClick={onClose}
              className="h-7 px-4 text-[11px] font-semibold border border-gray-300 rounded hover:bg-gray-100 transition-all"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
