import React, { useState, useEffect, useMemo } from "react";
import { Users, Search, Filter, X, ChevronDown } from "lucide-react";
import { fetchRoles } from "../api/helpers/UserService/Roles/Roles.js";
import PageHeader from "../components/PageHeader.jsx";
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
        w-full pl-${Icon ? "7" : "2.5"} pr-7 py-1 text-xs
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

  useEffect(() => {
    const loadRoles = async () => {
      try {
        setLoading(true);
        const data = await fetchRoles();
        setRoles(data || []);
      } catch (err) {
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

  return (
    <div className="h-full flex flex-col bg-gray-50 overflow-hidden">
      <PageHeader
        icon={Users}
        title="User Management"
        subtitle="Users • Roles • Permissions"
        actions={
          <div className="flex items-center gap-1.5">
            <div className="relative">
              <Search size={12} className="absolute left-2 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Name, email, username..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="
                  w-56 pl-7 pr-2.5 py-1 text-xs rounded-md
                  border border-amber-200 focus:border-amber-500 focus:ring-1 focus:ring-amber-300/40
                  bg-white placeholder:text-gray-400 transition-all
                "
              />
            </div>

            <FilterSelect
              value={selectedRole}
              onChange={setSelectedRole}
              options={roleOptions}
              placeholder={loading ? "Loading..." : "All Roles"}
              icon={Filter}
              className="min-w-[110px]"
              disabled={loading}
            />

            <FilterSelect
              value={selectedStatus}
              onChange={setSelectedStatus}
              options={statusOptions}
              placeholder="All Status"
              className="min-w-[95px]"
            />

            {hasFilters && (
              <button
                onClick={clearFilters}
                className="
                  flex items-center gap-1 px-2 py-1 text-xs font-semibold
                  bg-amber-100 text-amber-800 border border-amber-300 rounded-md
                  hover:bg-amber-200 active:opacity-90 transition-colors
                "
              >
                <X size={10} /> Clear
              </button>
            )}
          </div>
        }
      />

      {/* Active filters */}
      {hasFilters && (
        <div className="flex items-center gap-1.5 text-xs flex-wrap mx-4 sm:mx-6 mb-2">
          <span className="text-gray-700 font-medium">Filters:</span>
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
      <div className="flex-1 overflow-hidden px-4 sm:px-6 pb-4 sm:pb-6">
        <div className="h-full flex flex-col rounded-lg overflow-hidden bg-white border border-amber-200 shadow-sm">
          <div className="shrink-0 px-2.5 py-1.5 bg-gradient-to-r from-amber-50 to-amber-100 border-b border-amber-200 flex items-center justify-between">
            <div className="flex items-center gap-1.5">
              <div className="w-0.5 h-4 bg-gradient-to-b from-amber-500 to-amber-500 rounded-full" />
              <h3 className="text-xs font-semibold text-gray-900">User Directory</h3>
            </div>
          </div>

          <div className="flex-1 overflow-hidden">
            <UsersComponent
              compact={true}
              searchTerm={searchTerm.trim()}
              selectedRole={selectedRole}
              selectedStatus={selectedStatus}
            />
          </div>
        </div>
      </div>
    </div>
  );
}