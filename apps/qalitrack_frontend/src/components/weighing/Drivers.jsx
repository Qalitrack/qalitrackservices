import { useEffect, useState } from "react";
import { Pencil, Trash2, UserPlus, Search, X } from "lucide-react";
import {
  getDrivers,
  createDriver,
  updateDriver,
  deleteDriver,
} from "../../api/MasterData/Drivers";

export default function DriverPortal() {
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editingDriver, setEditingDriver] = useState(null);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    phone: "",
    licenseNumber: "",
    licenseExpiryDate: "",
    status: "Active",
  });

  useEffect(() => {
    fetchDrivers();
  }, [page, search]);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      setError(null);
      console.log("📡 Fetching drivers... page:", page, "search:", search);

      const data = await getDrivers({ pageNumber: page, pageSize, search });

      console.log("🚀 Drivers API Response:", data);

      const driverList = Array.isArray(data?.data?.items)
        ? data.data.items
        : [];
      const totalItems = data?.data?.totalItems || driverList.length;
      const pages = Math.ceil(totalItems / pageSize);

      setDrivers(driverList);
      setTotalPages(pages);
    } catch (error) {
      console.error("❌ Failed to load drivers:", error.message);
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (editingDriver) {
        await updateDriver(editingDriver.id, form);
      } else {
        await createDriver(form);
      }
      await fetchDrivers();
      resetForm();
    } catch (error) {
      console.error("Error saving driver:", error.message);
      alert(`Error: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (driver) => {
    setForm({
      fullName: driver.fullName || "",
      email: driver.email || "",
      phone: driver.phone || "",
      licenseNumber: driver.licenseNumber || "",
      licenseExpiryDate: driver.licenseExpiryDate?.split("T")[0] || "",
      status: driver.status || "Active",
    });
    setEditingDriver(driver);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this driver?")) return;
    setLoading(true);
    try {
      await deleteDriver(id);
      await fetchDrivers();
    } catch (error) {
      console.error("Delete failed:", error.message);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({
      fullName: "",
      email: "",
      phone: "",
      licenseNumber: "",
      licenseExpiryDate: "",
      status: "Active",
    });
    setEditingDriver(null);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchDrivers();
  };

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500 px-4 py-2 flex items-center gap-2.5 shrink-0 shadow-lg">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg ring-2 ring-amber-400/50">
            <UserPlus className="w-4 h-4 text-black font-bold" />
          </div>
          <div>
            <div className="text-sm font-bold text-white leading-none">Drivers</div>
            <div className="text-[10px] text-amber-400 leading-none mt-1">
              <span className="font-semibold">{drivers.length}</span> registered
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex gap-2 justify-end items-center">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input 
              type="text"
              placeholder="Search drivers..." 
              className="w-64 h-8 pl-9 pr-3 text-[11px] bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 rounded focus:outline-none focus:ring-2 focus:ring-amber-500" 
              value={search}
              onChange={(e) => setSearch(e.target.value)} 
            />
          </div>
          <button
            onClick={handleSearch}
            className="h-8 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded shadow-lg transition-all"
          >
            Search
          </button>
        </div>
      </div>

      {/* Form Section */}
      <div className="bg-gradient-to-br from-gray-50 via-amber-50/30 to-emerald-50/20 border-b-2 border-amber-200 px-4 py-3 shrink-0 shadow-inner">
        <div className="bg-white rounded-lg p-4 border-2 border-amber-300 shadow-md">
          <h3 className="text-sm font-bold text-amber-900 mb-3 flex items-center gap-2">
            <UserPlus className="w-4 h-4" />
            {editingDriver ? "Edit Driver" : "Add New Driver"}
          </h3>
          
          <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-3">
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full"></span>
                Full Name *
              </label>
              <input
                type="text"
                name="fullName"
                value={form.fullName}
                onChange={handleChange}
                required
                className="w-full h-7 text-[11px] rounded border border-blue-300 px-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-200"
                placeholder="Driver name"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full"></span>
                Email
              </label>
              <input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-purple-300 px-2 focus:border-purple-500 focus:ring-1 focus:ring-purple-200"
                placeholder="email@example.com"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-pink-500 rounded-full"></span>
                Phone
              </label>
              <input
                type="text"
                name="phone"
                value={form.phone}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-pink-300 px-2 focus:border-pink-500 focus:ring-1 focus:ring-pink-200"
                placeholder="+254 7XX XXX XXX"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-cyan-500 rounded-full"></span>
                License Number
              </label>
              <input
                type="text"
                name="licenseNumber"
                value={form.licenseNumber}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-cyan-300 px-2 focus:border-cyan-500 focus:ring-1 focus:ring-cyan-200"
                placeholder="License number"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>
                License Expiry Date
              </label>
              <input
                type="date"
                name="licenseExpiryDate"
                value={form.licenseExpiryDate}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-rose-300 px-2 focus:border-rose-500 focus:ring-1 focus:ring-rose-200"
              />
            </div>

            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></span>
                Status
              </label>
              <select
                name="status"
                value={form.status}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border border-emerald-300 px-2 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-200"
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>

            <div className="col-span-4 flex gap-2 justify-end mt-2">
              {editingDriver && (
                <button
                  type="button"
                  onClick={resetForm}
                  className="h-7 px-4 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg transition-all flex items-center gap-1.5"
                >
                  <X className="w-3.5 h-3.5" />
                  Cancel
                </button>
              )}
              <button
                type="submit"
                disabled={loading}
                className="h-7 px-4 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-400 hover:to-amber-500 text-black rounded-lg shadow-lg transition-all flex items-center gap-1.5"
              >
                <UserPlus className="w-3.5 h-3.5" />
                {editingDriver ? "Update Driver" : "Add Driver"}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-hidden px-2.5 pb-2.5 pt-1.5">
        <div className="h-full bg-white rounded border border-gray-200 overflow-hidden">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">Loading drivers...</p>
            </div>
          ) : error ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-red-500 text-sm">Error: {error}</p>
            </div>
          ) : drivers.length === 0 ? (
            <div className="flex items-center justify-center h-full">
              <p className="text-gray-500 text-sm">No drivers found.</p>
            </div>
          ) : (
            <div className="flex flex-col h-full">
              <div className="flex-1 overflow-auto">
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gradient-to-r from-gray-900 via-black to-gray-900 border-b-2 border-amber-500">
                    <tr>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Name</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Email</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Phone</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">License</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Expiry</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-left uppercase tracking-wide">Status</th>
                      <th className="px-4 py-2.5 text-[10px] font-bold text-amber-400 text-center uppercase tracking-wide">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {drivers.map((driver, index) => (
                      <tr 
                        key={driver.id} 
                        className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-yellow-50 transition-all ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                      >
                        <td className="px-4 py-2.5 text-[10px] text-blue-700 font-bold">{driver.fullName}</td>
                        <td className="px-4 py-2.5 text-[10px] text-purple-600">{driver.email || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-pink-600 font-medium">{driver.phone || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-cyan-700 font-mono font-semibold">{driver.licenseNumber || '-'}</td>
                        <td className="px-4 py-2.5 text-[10px] text-rose-600">
                          {driver.licenseExpiryDate?.split("T")[0] || "-"}
                        </td>
                        <td className="px-4 py-2.5">
                          <span className={`px-2.5 py-1 rounded-full text-[9px] font-bold uppercase shadow-sm ${
                            driver.status === "Active"
                              ? "bg-gradient-to-r from-emerald-100 to-green-200 text-emerald-700 border border-emerald-300"
                              : "bg-gradient-to-r from-red-100 to-rose-200 text-red-700 border border-red-300"
                          }`}>
                            {driver.status === "Active" ? "✓ Active" : "✕ Inactive"}
                          </span>
                        </td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-2 justify-center">
                            <button
                              onClick={() => handleEdit(driver)}
                              className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all"
                              title="Edit"
                            >
                              <Pencil className="w-3.5 h-3.5" />
                            </button>
                            <button
                              onClick={() => handleDelete(driver.id)}
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

              {/* Pagination */}
              <div className="border-t-2 border-amber-200 bg-gradient-to-r from-gray-50 to-amber-50/30 px-4 py-2.5 flex justify-between items-center">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all"
                >
                  Previous
                </button>
                <span className="text-[11px] font-bold text-gray-700">
                  Page <span className="text-amber-600">{page}</span> of <span className="text-amber-600">{totalPages}</span>
                </span>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="h-7 px-3 text-[11px] font-semibold border-2 border-gray-300 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed hover:bg-gray-100 transition-all"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}