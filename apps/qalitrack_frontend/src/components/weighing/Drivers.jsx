/**
 * DriverPortal.jsx — Driver Management (mirrors Vehicle RFID pattern)
 *
 * NEW FIELDS ADDED:
 *   ✅ nfCcode (NFC UID) — each driver gets one NFC card, stored as unique identifier
 *   ✅ transporterId, supplierId — company assignments (read from API response)
 *   ✅ assignedVehicleIds — list of vehicles this driver operates
 *   ✅ vehicleAssignments — detailed vehicle assignment history
 *
 * API ENDPOINTS (from Swagger):
 *   GET    /MasterData/Drivers?pageNumber=1&pageSize=10&searchTerm=
 *   POST   /MasterData/Drivers
 *   PUT    /MasterData/Drivers/{id}
 *   DELETE /MasterData/Drivers/{id}
 *   GET    /MasterData/Drivers/nfc/{nfcCode}  ← for kiosk NFC lookup
 */

import { useEffect, useState } from "react";
import { Pencil, Trash2, UserPlus, Search, X, CreditCard, Truck, Building2 } from "lucide-react";
import { message, Modal } from "antd";
import { useLicenseFeature } from "../../hooks/useLicenseFeature";
import { LicenseFeatures } from "../../utils/LicenseFeatures";
import {
  getDrivers,
  createDriver,
  updateDriver,
  deleteDriver,
} from "../../api/MasterData/Drivers";

export default function DriverPortal() {
  const nfcLicensed = useLicenseFeature(LicenseFeatures.NFC);
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
    idNumber: "",
    licenseNumber: "",
    licenseExpiryDate: "",
    nfCcode: "",  // ← NFC UID (API field name: nfCcode - note capital C)
    status: "active",
  });

  useEffect(() => {
    fetchDrivers();
  }, [page, search]);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getDrivers({ pageNumber: page, pageSize, searchTerm: search });

      const driverList = Array.isArray(data?.data?.items)
        ? data.data.items
        : Array.isArray(data?.items)
        ? data.items
        : [];
      const totalItems = data?.data?.totalItems || data?.totalItems || driverList.length;
      const pages = Math.ceil(totalItems / pageSize);

      setDrivers(driverList);
      setTotalPages(pages);
    } catch (error) {
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
      // Clean payload — only send fields that API expects
      const payload = {
        fullName: form.fullName.trim(),
        email: form.email.trim() || undefined,
        phone: form.phone.trim(),
        idNumber: form.idNumber.trim() || undefined,
        licenseNumber: form.licenseNumber.trim() || undefined,
        licenseExpiryDate: form.licenseExpiryDate || undefined,
        nfCcode: form.nfCcode.trim() || undefined,  // ← NFC UID
        status: form.status,
      };


      if (editingDriver) {
        await updateDriver(editingDriver.id, payload);
      } else {
        await createDriver(payload);
      }
      await fetchDrivers();
      resetForm();
    } catch (error) {
      message.error(`Error: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (driver) => {
    setForm({
      fullName: driver.fullName || "",
      email: driver.email || "",
      phone: driver.phone || "",
      idNumber: driver.idNumber || "",
      licenseNumber: driver.licenseNumber || "",
      licenseExpiryDate: driver.licenseExpiryDate?.split("T")[0] || "",
      nfCcode: driver.nfCcode || "",  // ← NFC UID
      status: (driver.status || "active").toLowerCase(),
    });
    setEditingDriver(driver);
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Are you sure you want to delete this driver?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        setLoading(true);
        try {
          await deleteDriver(id);
          await fetchDrivers();
        } catch (error) {
          message.error(`Delete failed: ${error.message}`);
        } finally {
          setLoading(false);
        }
      },
    });
  };

  const resetForm = () => {
    setForm({
      fullName: "",
      email: "",
      phone: "",
      idNumber: "",
      licenseNumber: "",
      licenseExpiryDate: "",
      nfCcode: "",
      status: "active",
    });
    setEditingDriver(null);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <UserPlus className="w-4 h-4 text-white" />
            </div>
            <div>
              <span className="text-[11px] font-bold text-gray-900 block leading-tight">
                Drivers
              </span>
              <span className="text-[9px] text-amber-700 font-medium">
                {drivers.length} registered · NFC-enabled
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search drivers..."
                className="w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 focus:border-amber-500 shadow-sm"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <button
              onClick={() => {
                setSearch("");
                setPage(1);
                fetchDrivers();
              }}
              className="h-7 px-3 text-[11px] rounded-md border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm font-medium bg-white"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
        <form onSubmit={handleSubmit} className="grid grid-cols-5 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Full Name *
            </label>
            <input
              type="text"
              name="fullName"
              value={form.fullName}
              onChange={handleChange}
              required
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="Driver name"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Phone *
            </label>
            <input
              type="text"
              name="phone"
              value={form.phone}
              onChange={handleChange}
              required
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="+254 7XX XXX XXX"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              ID Number
            </label>
            <input
              type="text"
              name="idNumber"
              value={form.idNumber}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 font-mono"
              placeholder="National ID number"
            />
          </div>

          {nfcLicensed && (
            <div>
              <label className="text-[10px] font-semibold text-gray-700 mb-1 block flex items-center gap-1">
                <CreditCard className="w-3 h-3 text-purple-600" />
                NFC UID
              </label>
              <input
                type="text"
                name="nfCcode"
                value={form.nfCcode}
                onChange={handleChange}
                className="w-full h-7 text-[11px] rounded border-purple-300 px-2 focus:border-purple-500 focus:ring-1 focus:ring-purple-200 font-mono"
                placeholder="e.g. 3CD2FF9D"
                title="NFC card unique identifier (8-16 hex chars)"
              />
            </div>
          )}

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              License Number
            </label>
            <input
              type="text"
              name="licenseNumber"
              value={form.licenseNumber}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 font-mono"
              placeholder="License number"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              License Expiry
            </label>
            <input
              type="date"
              name="licenseExpiryDate"
              value={form.licenseExpiryDate}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Email
            </label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
              placeholder="email@example.com"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Status
            </label>
            <select
              name="status"
              value={form.status}
              onChange={handleChange}
              className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
            >
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </div>

          <div className="col-span-5 flex gap-2 justify-end mt-1">
            {editingDriver && (
              <button
                type="button"
                onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1"
              >
                <X className="w-3 h-3" />
                Cancel
              </button>
            )}
            <button
              type="submit"
              disabled={loading}
              className="h-7 px-3 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all flex items-center gap-1 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <UserPlus className="w-3 h-3" />
              {editingDriver ? "Update" : "Add"} Driver
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <div className="w-8 h-8 border-2 border-amber-500 border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
              <p className="text-gray-500 text-sm">Loading drivers...</p>
            </div>
          </div>
        ) : error ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <p className="text-red-500 text-sm font-semibold mb-2">⚠️ Error</p>
              <p className="text-red-400 text-xs">{error}</p>
            </div>
          </div>
        ) : drivers.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <UserPlus className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500 text-sm">No drivers found.</p>
              <p className="text-gray-400 text-xs mt-1">Add a driver using the form above</p>
            </div>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Phone</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">ID Number</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">
                  <div className="flex items-center gap-1">
                    <CreditCard className="w-3 h-3" />
                    NFC UID
                  </div>
                </th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">License</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Expiry</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Email</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Assignments</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {drivers.map((driver, index) => {
                // Extract company assignments (API may populate these)
                const hasTransporter = Boolean(driver.transporterId);
                const hasSupplier = Boolean(driver.supplierId);
                const vehicleCount = driver.assignedVehicleIds?.length || 0;

                // Check license expiry
                const expiryDate = driver.licenseExpiryDate ? new Date(driver.licenseExpiryDate) : null;
                const isExpired = expiryDate && expiryDate < new Date();
                const isExpiringSoon = expiryDate && expiryDate < new Date(Date.now() + 30 * 24 * 60 * 60 * 1000); // 30 days

                return (
                  <tr
                    key={driver.id}
                    className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all ${
                      index % 2 === 0 ? "bg-white" : "bg-gray-50"
                    }`}
                  >
                    <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">
                      {(page - 1) * pageSize + index + 1}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">
                      {driver.fullName}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-600 font-medium">
                      {driver.phone || "-"}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-700 font-mono font-semibold">
                      {driver.idNumber || "-"}
                    </td>
                    <td className="px-3 py-2">
                      {driver.nfCcode ? (
                        <div className="flex items-center gap-1">
                          <span className="px-2 py-0.5 rounded-md bg-purple-50 border border-purple-200 text-purple-700 font-mono text-[10px] font-bold">
                            {driver.nfCcode}
                          </span>
                          <CreditCard className="w-3 h-3 text-purple-500" />
                        </div>
                      ) : (
                        <span className="text-[10px] text-gray-400 italic">No NFC</span>
                      )}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-700 font-mono font-semibold">
                      {driver.licenseNumber || "-"}
                    </td>
                    <td className="px-3 py-2 text-[10px]">
                      {expiryDate ? (
                        <span className={`px-2 py-0.5 rounded-full text-[9px] font-semibold ${
                          isExpired 
                            ? "bg-red-100 text-red-700 border border-red-300"
                            : isExpiringSoon
                            ? "bg-yellow-100 text-yellow-700 border border-yellow-300"
                            : "text-gray-600"
                        }`}>
                          {driver.licenseExpiryDate?.split("T")[0]}
                        </span>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </td>
                    <td className="px-3 py-2 text-[10px] text-gray-600">
                      {driver.email || <span className="text-gray-400 italic">-</span>}
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex items-center gap-1 flex-wrap">
                        {vehicleCount > 0 && (
                          <span className="px-1.5 py-0.5 rounded-full bg-blue-50 border border-blue-200 text-blue-700 text-[9px] font-semibold flex items-center gap-0.5" title={`${vehicleCount} vehicle(s) assigned`}>
                            🚗 {vehicleCount}
                          </span>
                        )}
                        {hasTransporter && (
                          <Truck className="w-3 h-3 text-green-600" title="Assigned to transporter" />
                        )}
                        {hasSupplier && (
                          <Building2 className="w-3 h-3 text-orange-600" title="Assigned to supplier" />
                        )}
                        {!vehicleCount && !hasTransporter && !hasSupplier && (
                          <span className="text-[9px] text-gray-400 italic">None</span>
                        )}
                      </div>
                    </td>
                    <td className="px-3 py-2">
                      <span
                        className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase ${
                          driver.status?.toLowerCase() === "active"
                            ? "bg-green-100 text-green-700 border border-green-300"
                            : "bg-red-100 text-red-700 border border-red-300"
                        }`}
                      >
                        {driver.status?.toLowerCase() === "active" ? "✓ Active" : "✕ Inactive"}
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex gap-1 justify-center">
                        <button
                          onClick={() => handleEdit(driver)}
                          className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                          title="Edit driver"
                        >
                          <Pencil className="w-3 h-3" />
                        </button>
                        <button
                          onClick={() => handleDelete(driver.id)}
                          className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                          title="Delete driver"
                        >
                          <Trash2 className="w-3 h-3" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Footer with Pagination */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <span className="text-[10px] text-gray-600 font-medium">
          Page <span className="font-semibold text-amber-600">{page}</span> of{" "}
          <span className="font-semibold text-amber-600">{totalPages}</span>
          {" · "}
          <span className="text-gray-500">{drivers.length} drivers shown</span>
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
}