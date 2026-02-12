import React, { useState, useEffect, useMemo } from "react";
import { Users, Search, Filter, X, ChevronDown, ChevronLeft, ChevronRight } from "lucide-react";
import { fetchRoles } from "../api/helpers/UserService/Roles/Roles.js";
import UsersComponent from "../pages/Userservice/Users.jsx";

const FilterSelect = ({ value, onChange, options, placeholder, icon: Icon, className = "" }) => (
  <div className={`relative ${className}`}>
    {Icon && (
      <Icon
        size={12}
        className="absolute left-2 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
      />
    )}
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className={`
        w-full pl-${Icon ? "7" : "2.5"} pr-7 py-1 text-[10px]
        bg-white border border-amber-200 rounded-md
        appearance-none cursor-pointer outline-none
        focus:border-amber-500 focus:ring-1 focus:ring-amber-300/40
        transition-all
      `}
    >
      <option value="">{placeholder}</option>
      {options.map((opt) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
    </select>
    <ChevronDown
      size={10}
      className="absolute right-1.5 top-1/2 -translate-y-1/2 text-amber-500 pointer-events-none"
    />
  </div>
);

export default function UserManagement() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRole, setSelectedRole] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("");
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);

  // ── Pagination ──
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 5; // ← changed to 5 users per page
  const [totalUsers, setTotalUsers] = useState(0);

  useEffect(() => {
    const loadRoles = async () => {
      try {
        setLoading(true);
        const data = await fetchRoles();
        setRoles(data || []);
      } catch (err) {
        console.error("Roles fetch failed:", err);
      } finally {
        setLoading(false);
      }
    };
    loadRoles();
  }, []);

  const hasFilters = !!(searchTerm || selectedRole || selectedStatus);

  const clearFilters = () => {
    setSearchTerm("");
    setSelectedRole("");
    setSelectedStatus("");
    setCurrentPage(1);
  };

  const statusOptions = useMemo(
    () => [
      { value: "active", label: "Active" },
      { value: "offline", label: "Offline" },
      { value: "suspended", label: "Suspended" },
    ],
    []
  );

  const roleOptions = useMemo(
    () => roles.map((r) => ({ value: r.name, label: r.name })),
    [roles]
  );

  // Calculate pagination numbers to show
  const totalPages = Math.ceil(totalUsers / pageSize);
  const maxVisiblePages = 7;
  
  let pages = [];
  if (totalPages <= maxVisiblePages) {
    pages = Array.from({ length: totalPages }, (_, i) => i + 1);
  } else {
    if (currentPage <= 4) {
      pages = [1, 2, 3, 4, 5, "...", totalPages];
    } else if (currentPage >= totalPages - 3) {
      pages = [1, "...", totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    } else {
      pages = [1, "...", currentPage - 1, currentPage, currentPage + 1, "...", totalPages];
    }
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  return (
    <div className="h-full flex flex-col px-2 py-1.5 gap-1.5 bg-gray-50">

      {/* Header */}
      <div className="shrink-0 flex items-center justify-between gap-2">
        <div className="flex items-center gap-1.5">
          <div className="w-6 h-6 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
            <Users size={14} className="text-white" />
          </div>
          <div>
            <h2 className="text-xs font-bold text-gray-900 leading-tight">User Management</h2>
            <p className="text-[9px] text-amber-700 font-medium">Users • Roles • Permissions</p>
          </div>
        </div>

        <div className="flex-1 flex items-center gap-1.5 max-w-3xl">
          <div className="relative flex-1">
            <Search size={12} className="absolute left-2 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Name, email, username..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setCurrentPage(1);
              }}
              className="
                w-full pl-7 pr-2.5 py-1 text-[10px] rounded-md
                border border-amber-200 focus:border-amber-500 focus:ring-1 focus:ring-amber-300/40
                bg-white placeholder:text-gray-400 transition-all
              "
            />
          </div>

          <FilterSelect
            value={selectedRole}
            onChange={(v) => {
              setSelectedRole(v);
              setCurrentPage(1);
            }}
            options={roleOptions}
            placeholder={loading ? "Loading..." : "All Roles"}
            icon={Filter}
            className="min-w-[110px]"
            disabled={loading}
          />

          <FilterSelect
            value={selectedStatus}
            onChange={(v) => {
              setSelectedStatus(v);
              setCurrentPage(1);
            }}
            options={statusOptions}
            placeholder="All Status"
            className="min-w-[95px]"
          />

          {hasFilters && (
            <button
              onClick={clearFilters}
              className="
                flex items-center gap-1 px-2 py-1 text-[10px] font-semibold
                bg-amber-100 text-amber-800 border border-amber-300 rounded-md
                hover:bg-amber-200 active:opacity-90 transition-colors
              "
            >
              <X size={10} /> Clear
            </button>
          )}
        </div>
      </div>

      {/* Active filters */}
      {hasFilters && (
        <div className="flex items-center gap-1.5 text-[9px] flex-wrap">
          <span className="text-gray-500 font-medium">Filters:</span>
          {searchTerm && (
            <span className="px-2 py-0.5 bg-amber-100 text-amber-800 rounded-full border border-amber-200">
              "{searchTerm.slice(0, 15)}{searchTerm.length > 15 ? "..." : ""}
            </span>
          )}
          {selectedRole && (
            <span className="px-2 py-0.5 bg-amber-100 text-amber-800 rounded-full border border-amber-200">
              {selectedRole}
            </span>
          )}
          {selectedStatus && (
            <span className="px-2 py-0.5 bg-amber-100 text-amber-800 rounded-full border border-amber-200">
              {selectedStatus}
            </span>
          )}
        </div>
      )}

      {/* Main content card */}
      <div className="flex-1 flex flex-col rounded-lg overflow-hidden bg-white border border-amber-200 shadow-sm">
        <div className="shrink-0 px-2.5 py-1.5 bg-gradient-to-r from-amber-50 to-amber-100 border-b border-amber-200 flex items-center justify-between">
          <div className="flex items-center gap-1.5">
            <div className="w-0.5 h-4 bg-gradient-to-b from-amber-500 to-orange-500 rounded-full" />
            <h3 className="text-[11px] font-semibold text-gray-900">User Directory</h3>
          </div>
          <span className="text-[9px] font-medium text-amber-700 bg-amber-50 px-1.5 py-0.5 rounded border border-amber-200">
            5 per page
          </span>
        </div>

        <div className="flex-1 overflow-auto">
          <UsersComponent
            compact={true}
            pageSize={pageSize}
            currentPage={currentPage}
            searchTerm={searchTerm.trim()}
            selectedRole={selectedRole}
            selectedStatus={selectedStatus}
            showRowNumbers={true}           // ← new prop suggestion
            onPageChange={setCurrentPage}
            onTotalChange={setTotalUsers}
          />
        </div>

        {/* Pagination with page numbers */}
        {totalUsers > 0 && totalPages > 1 && (
          <div className="shrink-0 px-2 py-1.5 border-t border-amber-200 bg-amber-50/60 flex items-center justify-between text-[10px]">
            <div className="text-gray-600 font-medium">
              {totalUsers} users • page {currentPage} of {totalPages}
            </div>

            <div className="flex items-center gap-1">
              <button
                disabled={currentPage === 1}
                onClick={() => goToPage(currentPage - 1)}
                className="p-1 rounded hover:bg-amber-100 disabled:opacity-40 transition-colors"
              >
                <ChevronLeft size={14} className="text-amber-700" />
              </button>

              {pages.map((page, idx) => (
                <React.Fragment key={idx}>
                  {page === "..." ? (
                    <span className="px-2 py-1 text-gray-500">...</span>
                  ) : (
                    <button
                      onClick={() => goToPage(page)}
                      className={`
                        min-w-[24px] h-6 flex items-center justify-center rounded text-[10px] font-medium
                        ${currentPage === page 
                          ? "bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-sm" 
                          : "hover:bg-amber-100 text-amber-800"}
                      `}
                    >
                      {page}
                    </button>
                  )}
                </React.Fragment>
              ))}

              <button
                disabled={currentPage === totalPages}
                onClick={() => goToPage(currentPage + 1)}
                className="p-1 rounded hover:bg-amber-100 disabled:opacity-40 transition-colors"
              >
                <ChevronRight size={14} className="text-amber-700" />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}