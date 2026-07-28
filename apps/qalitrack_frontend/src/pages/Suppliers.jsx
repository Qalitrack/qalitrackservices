import { useEffect, useState } from "react";
import { Pencil, Trash2, Building, Search, X, Car, UserCheck } from "lucide-react";
import { message, Modal } from "antd";
import TablePagination from "../components/TablePagination";
import VehicleRelationModal from "../components/VehicleRelationModal";
import DriverRelationModal from "../components/DriverRelationModal";
import {
  getSuppliers,
  createSupplier,
  updateSupplier,
  deleteSupplier,
} from "../api/MasterData/Suppliers";
import { assignDriverToSupplier, unassignDriverFromSupplier } from "../api/MasterData/Drivers";

export default function SuppliersPortal({ onHeaderActionsChange }) {
  const [suppliers, setSuppliers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingSupplier, setEditingSupplier] = useState(null);
  const [vehiclesSupplier, setVehiclesSupplier] = useState(null);
  const [driversSupplier, setDriversSupplier] = useState(null);

  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    name: "",
    contactPerson: "",
    phone: "",
    email: "",
    city: "",
    address: "",
  });

  const fetchSuppliersWithSearch = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getSuppliers(page, pageSize, search);
      const items = data?.items || data || [];
      const totalItems = data?.totalItems || items.length;

      setSuppliers(items);
      setTotalPages(Math.ceil(totalItems / pageSize));
    } catch (err) {
      setError(err.message || "Failed to load suppliers");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSuppliersWithSearch();
  }, [page, search]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((p) => ({ ...p, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const payload = { ...form, email: form.email || undefined, phone: form.phone || undefined };
      if (editingSupplier) {
        await updateSupplier(editingSupplier.id, payload);
      } else {
        await createSupplier(payload);
      }
      resetForm();
      setPage(1);
      fetchSuppliersWithSearch();
    } catch (err) {
      message.error(err.message || "Failed to save supplier");
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (supplier) => {
    setForm({
      name: supplier.name || "",
      contactPerson: supplier.contactPerson || "",
      phone: supplier.phone || "",
      email: supplier.email || "",
      city: supplier.city || "",
      address: supplier.address || "",
    });
    setEditingSupplier(supplier);
  };

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Delete this supplier?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteSupplier(id);
          fetchSuppliersWithSearch();
        } catch (err) {
          message.error(err.message || "Failed to delete supplier");
        }
      },
    });
  };

  const resetForm = () => {
    setForm({
      name: "",
      contactPerson: "",
      phone: "",
      email: "",
      city: "",
      address: "",
    });
    setEditingSupplier(null);
  };

  useEffect(() => {
    onHeaderActionsChange?.(
      <>
        <div className="relative">
          <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search suppliers..."
            className="qt-filter-field w-52 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <button
          onClick={(e) => {
            e.preventDefault();
            setPage(1);
            fetchSuppliersWithSearch();
          }}
          className="h-7 px-3 text-[11px] rounded-md cs-solid-chip-btn shadow-sm font-medium"
        >
          Search
        </button>
      </>
    );
    return () => onHeaderActionsChange?.(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
        <form onSubmit={handleSubmit} className="grid grid-cols-3 gap-2">
          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Supplier Name <span style={{ color: "var(--cs-required)" }}>*</span>
            </label>
            <input
              name="name"
              value={form.name}
              onChange={handleChange}
              required
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="Company name"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Contact Person
            </label>
            <input
              name="contactPerson"
              value={form.contactPerson}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="Contact name"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Phone
            </label>
            <input
              name="phone"
              value={form.phone}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="+254 7XX XXX XXX"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Email
            </label>
            <input
              name="email"
              value={form.email}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="email@example.com"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              City
            </label>
            <input
              name="city"
              value={form.city}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="City"
            />
          </div>

          <div>
            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
              Address
            </label>
            <input
              name="address"
              value={form.address}
              onChange={handleChange}
              className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
              placeholder="Full address"
            />
          </div>

          <div className="col-span-3 flex gap-2 justify-end mt-1">
            {editingSupplier && (
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
              className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1"
            >
              <Building className="w-3 h-3" />
              {editingSupplier ? "Update" : "Add"} Supplier
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading && suppliers.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading suppliers...</p>
          </div>
        ) : suppliers.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <Building className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500 text-sm">No suppliers found.</p>
              <p className="text-gray-400 text-xs mt-1">Add a supplier using the form above</p>
            </div>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Contact Person</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Email</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Phone</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">City</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Address</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {suppliers.map((s, index) => (
                <tr
                  key={s.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">{(page - 1) * pageSize + index + 1}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-900 font-bold">{s.name}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 font-medium">{s.contactPerson || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{s.email || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 font-medium">{s.phone || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">{s.city || "-"}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600 max-w-xs truncate">{s.address || "-"}</td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        onClick={() => setVehiclesSupplier(s)}
                        className="p-1 rounded text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all"
                        title="View Vehicles"
                      >
                        <Car className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => setDriversSupplier(s)}
                        className="p-1 rounded text-gray-600 hover:bg-gray-50 border border-gray-300 hover:border-gray-500 transition-all"
                        title="View Drivers"
                      >
                        <UserCheck className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleEdit(s)}
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(s.id)}
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

        {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
        <TablePagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      {error && (
        <div className="fixed bottom-4 right-4 bg-red-100 border-2 border-red-400 text-red-700 px-4 py-2 rounded-lg shadow-lg">
          <p className="text-xs font-semibold">{error}</p>
        </div>
      )}

      <VehicleRelationModal
        entity={vehiclesSupplier}
        relationField="supplierId"
        relationNameField="supplierName"
        entityLabel="Supplier"
        onClose={() => setVehiclesSupplier(null)}
      />

      <DriverRelationModal
        entity={driversSupplier}
        relationField="supplierId"
        assignFn={assignDriverToSupplier}
        removeFn={unassignDriverFromSupplier}
        entityLabel="Supplier"
        otherEntities={suppliers}
        onClose={() => setDriversSupplier(null)}
      />

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