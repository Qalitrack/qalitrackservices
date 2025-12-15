import { useEffect, useState } from "react";
import {
  Pencil,
  Trash2,
  UserPlus,
  Search,
  Car,
} from "lucide-react";
import dayjs from "dayjs";
import {
  getOwners,
  createOwner,
  updateOwner,
  deleteOwner,
  getOwnerVehicles,
} from "../../api/MasterData/Owners";

// Owner Type Enum
const OWNER_TYPES = {
  1: "Individual",
  2: "Company",
  3: "Government",
};

export default function OwnersPortal() {
  const [owners, setOwners] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [viewingVehiclesFor, setViewingVehiclesFor] = useState(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingOwner, setEditingOwner] = useState(null);

  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    name: "",
    type: 1,
    contactPerson: "",
    phoneNumber: "",
    email: "",
    address: "",
  });

  // ─────────────────────────────────────────────
  // Fetch Owners
  // ─────────────────────────────────────────────
  useEffect(() => {
    fetchOwners();
  }, [page, search]);

  const fetchOwners = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getOwners(page, pageSize, search);
      const items = data?.items || data || [];
      const totalItems = data?.totalItems || items.length;

      setOwners(items);
      setTotalPages(Math.ceil(totalItems / pageSize));
    } catch (err) {
      setError(err.message || "Failed to load owners");
    } finally {
      setLoading(false);
    }
  };

  // ─────────────────────────────────────────────
  // Handlers
  // ─────────────────────────────────────────────
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingOwner) {
        await updateOwner(editingOwner.id, form);
      } else {
        await createOwner(form);
      }
      resetForm();
      fetchOwners();
    } catch (err) {
      alert(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (owner) => {
    setForm({
      name: owner.name || "",
      type: owner.type || 1,
      contactPerson: owner.contactPerson || "",
      phoneNumber: owner.phoneNumber || "",
      email: owner.email || "",
      address: owner.address || "",
    });
    setEditingOwner(owner);
  };

  const handleDelete = async (id) => {
    if (!confirm("Delete this owner?")) return;
    await deleteOwner(id);
    fetchOwners();
  };

  const handleViewVehicles = async (owner) => {
    setViewingVehiclesFor(owner);
    const data = await getOwnerVehicles(owner.id);
    setVehicles(data?.data || data || []);
  };

  const resetForm = () => {
    setForm({
      name: "",
      type: 1,
      contactPerson: "",
      phoneNumber: "",
      email: "",
      address: "",
    });
    setEditingOwner(null);
  };

  // ─────────────────────────────────────────────
  // UI
  // ─────────────────────────────────────────────
  return (
    <div className="bg-white shadow-sm rounded-xl p-6 border border-gray-100">
      <h2 className="text-2xl font-semibold text-amber-600 mb-6 flex items-center gap-2">
        <UserPlus className="w-6 h-6" /> Owner Management
      </h2>

      {/* 🔍 Search */}
      <form
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
          fetchOwners();
        }}
        className="flex items-center gap-3 mb-6 border rounded-lg px-3 py-2"
      >
        <Search className="w-5 h-5 text-gray-400" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search owners..."
          className="flex-1 bg-transparent outline-none"
        />
        <button className="bg-amber-500 text-white px-4 py-1 rounded-lg">
          Search
        </button>
      </form>

      {/* 📝 Owner Form */}
      <form
        onSubmit={handleSubmit}
        className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
      >
        <input
          name="name"
          value={form.name}
          onChange={handleChange}
          placeholder="Owner Name"
          required
          className="border rounded-lg px-3 py-2"
        />

        <select
          name="type"
          value={form.type}
          onChange={handleChange}
          className="border rounded-lg px-3 py-2"
        >
          {Object.entries(OWNER_TYPES).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>

        <input
          name="contactPerson"
          value={form.contactPerson}
          onChange={handleChange}
          placeholder="Contact Person"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="phoneNumber"
          value={form.phoneNumber}
          onChange={handleChange}
          placeholder="Phone Number"
          className="border rounded-lg px-3 py-2"
        />

        <input
          name="email"
          value={form.email}
          onChange={handleChange}
          placeholder="Email"
          className="border rounded-lg px-3 py-2"
        />

        <textarea
          name="address"
          value={form.address}
          onChange={handleChange}
          placeholder="Address"
          className="border rounded-lg px-3 py-2 md:col-span-2"
        />

        <div className="md:col-span-2 flex gap-3">
          <button className="bg-amber-500 text-white px-4 py-2 rounded-lg">
            {editingOwner ? "Update Owner" : "Add Owner"}
          </button>
          {editingOwner && (
            <button
              type="button"
              onClick={resetForm}
              className="bg-gray-200 px-4 py-2 rounded-lg"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {/* 📋 Owners Table */}
      {loading ? (
        <p>Loading...</p>
      ) : owners.length === 0 ? (
        <p className="text-gray-500">No owners found</p>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full text-sm border rounded-lg">
              <thead className="bg-gray-50">
                <tr>
                  {["Name", "Type", "Contact", "Actions"].map((h) => (
                    <th key={h} className="border px-3 py-2 text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {owners.map((o) => (
                  <tr key={o.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2 font-medium">{o.name}</td>
                    <td className="px-3 py-2">{OWNER_TYPES[o.type]}</td>
                    <td className="px-3 py-2">{o.phoneNumber}</td>
                    <td className="px-3 py-2 flex gap-2">
                      <button onClick={() => handleViewVehicles(o)}>
                        <Car className="w-4 h-4 text-gray-600" />
                      </button>
                      <button onClick={() => handleEdit(o)}>
                        <Pencil className="w-4 h-4 text-blue-600" />
                      </button>
                      <button onClick={() => handleDelete(o.id)}>
                        <Trash2 className="w-4 h-4 text-red-600" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="flex justify-between mt-4">
            <button
              disabled={page === 1}
              onClick={() => setPage(p => p - 1)}
              className="border px-3 py-1 rounded-lg"
            >
              Previous
            </button>
            <span>Page {page} of {totalPages}</span>
            <button
              disabled={page === totalPages}
              onClick={() => setPage(p => p + 1)}
              className="border px-3 py-1 rounded-lg"
            >
              Next
            </button>
          </div>
        </>
      )}

      {/* 🚗 Vehicles Drawer */}
      {viewingVehiclesFor && (
        <div className="mt-8 border-t pt-4">
          <h3 className="font-semibold mb-2">
            Vehicles – {viewingVehiclesFor.name}
          </h3>
          {vehicles.length === 0 ? (
            <p className="text-sm text-gray-500">No vehicles</p>
          ) : (
            <ul className="text-sm list-disc ml-5">
              {vehicles.map((v) => (
                <li key={v.id}>
                  {v.registrationNumber} – {v.make} {v.model}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
