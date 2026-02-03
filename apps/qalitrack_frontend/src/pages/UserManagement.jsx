import React, { useState, useEffect } from "react";
import { Users, Search, Filter, X } from "lucide-react";
import { fetchRoles } from "../api/helpers/UserService/Roles/Roles.js";
import UsersComponent from "../pages/Userservice/Users.jsx";

export default function UserManagement() {
  const [searchTerm, setSearchTerm]         = useState("");
  const [selectedRole, setSelectedRole]     = useState("");
  const [selectedStatus, setSelectedStatus] = useState("");
  const [roles, setRoles]                   = useState([]);

  useEffect(() => {
    const load = async () => {
      try {
        const rolesData = await fetchRoles();
        setRoles(rolesData);
      } catch (err) {
        console.error("Failed to fetch roles:", err);
      }
    };
    load();
  }, []);

  const handleClearFilters = () => {
    setSearchTerm("");
    setSelectedRole("");
    setSelectedStatus("");
  };

  const hasActiveFilters = searchTerm || selectedRole || selectedStatus;

  return (
    <div className="h-full flex flex-col px-6 py-5 gap-4" style={{ background: "#fafafa" }}>

      {/* ── Top row: title left, search + filters right ── */}
      <div className="shrink-0 flex items-center justify-between gap-6">
        {/* Title block */}
        <div className="flex items-center gap-3 shrink-0">
          <div
            className="w-10 h-10 rounded-xl flex items-center justify-center shadow-md"
            style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)" }}
          >
            <Users size={20} color="#fff" />
          </div>
          <div>
            <h2 className="text-xl font-bold" style={{ color: "#111827" }}>User Management</h2>
            <p className="text-xs" style={{ color: "#6b7280" }}>Manage users, roles, and permissions</p>
          </div>
        </div>

        {/* Search + filters — fills remaining width */}
        <div className="flex items-center gap-2 flex-1">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2" style={{ color: "#9ca3af" }} />
            <input
              type="text"
              placeholder="Search by name, email or username…"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-9 pr-3 py-2 rounded-lg text-sm outline-none transition-all"
              style={{ background: "#fff", border: "1.5px solid #e5e7eb", color: "#111827" }}
              onFocus={(e) => { e.target.style.borderColor="#d97706"; e.target.style.boxShadow="0 0 0 3px rgba(217,119,6,0.15)"; }}
              onBlur={(e)  => { e.target.style.borderColor="#e5e7eb"; e.target.style.boxShadow="none"; }}
            />
          </div>

          <div className="relative">
            <Filter size={14} className="absolute left-2.5 top-1/2 -translate-y-1/2 pointer-events-none" style={{ color:"#9ca3af" }} />
            <select
              value={selectedRole}
              onChange={(e) => setSelectedRole(e.target.value)}
              className="pl-8 pr-7 py-2 rounded-lg text-sm appearance-none cursor-pointer outline-none transition-all"
              style={{
                background:"#fff", border:"1.5px solid #e5e7eb", color:"#374151", minWidth:"140px",
                backgroundImage:`url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23d97706' d='M6 9L1 4h10z'/%3E%3C/svg%3E")`,
                backgroundRepeat:"no-repeat", backgroundPosition:"right 0.65rem center",
              }}
              onFocus={(e) => { e.target.style.borderColor="#d97706"; e.target.style.boxShadow="0 0 0 3px rgba(217,119,6,0.15)"; }}
              onBlur={(e)  => { e.target.style.borderColor="#e5e7eb"; e.target.style.boxShadow="none"; }}
            >
              <option value="">All Roles</option>
              {roles.map((role) => (
                <option key={role.id} value={role.name}>{role.name}</option>
              ))}
            </select>
          </div>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="pl-3 pr-7 py-2 rounded-lg text-sm appearance-none cursor-pointer outline-none transition-all"
            style={{
              background:"#fff", border:"1.5px solid #e5e7eb", color:"#374151", minWidth:"120px",
              backgroundImage:`url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23d97706' d='M6 9L1 4h10z'/%3E%3C/svg%3E")`,
              backgroundRepeat:"no-repeat", backgroundPosition:"right 0.65rem center",
            }}
            onFocus={(e) => { e.target.style.borderColor="#d97706"; e.target.style.boxShadow="0 0 0 3px rgba(217,119,6,0.15)"; }}
            onBlur={(e)  => { e.target.style.borderColor="#e5e7eb"; e.target.style.boxShadow="none"; }}
          >
            <option value="">All Status</option>
            <option value="active">Active</option>
            <option value="offline">Offline</option>
          </select>

          {hasActiveFilters && (
            <button
              onClick={handleClearFilters}
              className="flex items-center gap-1 px-3 py-2 rounded-lg text-sm font-semibold transition-all hover:opacity-75"
              style={{ background:"#fef3c7", color:"#d97706", border:"1.5px solid #fcd34d" }}
            >
              <X size={14} /> Clear
            </button>
          )}
        </div>
      </div>

      {/* Active-filter pills */}
      {hasActiveFilters && (
        <div className="shrink-0 flex items-center gap-2">
          <span className="text-xs font-medium" style={{ color:"#9ca3af" }}>Active:</span>
          {searchTerm   && <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold" style={{ background:"#fef3c7", color:"#d97706", border:"1px solid #fcd34d" }}>Search: "{searchTerm}"</span>}
          {selectedRole && <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold" style={{ background:"#fef3c7", color:"#d97706", border:"1px solid #fcd34d" }}>Role: {selectedRole}</span>}
          {selectedStatus && <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold" style={{ background:"#fef3c7", color:"#d97706", border:"1px solid #fcd34d" }}>Status: {selectedStatus}</span>}
        </div>
      )}

      {/* ── Directory card — fills remaining height, internal scroll only ── */}
      <div
        className="flex-1 flex flex-col rounded-2xl overflow-hidden"
        style={{ background:"#ffffff", border:"1px solid #e5e7eb", boxShadow:"0 2px 8px rgba(0,0,0,0.07)", minHeight:0 }}
      >
        {/* Sticky card header */}
        <div
          className="shrink-0 px-5 py-3 flex items-center justify-between"
          style={{ background:"linear-gradient(135deg, #fffbeb, #fff7ed)", borderBottom:"1px solid #f0ecdf" }}
        >
          <div className="flex items-center gap-2.5">
            <div className="w-1 h-6 rounded-full" style={{ background:"linear-gradient(180deg, #d97706, #f59e0b)" }} />
            <div>
              <h3 className="text-sm font-bold" style={{ color:"#111827" }}>User Directory</h3>
              <p className="text-xs" style={{ color:"#9ca3af" }}>Browse and manage all users</p>
            </div>
          </div>
          <div className="px-2.5 py-0.5 rounded-full text-xs font-bold" style={{ background:"#fef3c7", color:"#d97706", border:"1px solid #fcd34d" }}>
            5 per page
          </div>
        </div>

        {/* Scrollable body — wraps UsersComponent full-width */}
        <div className="flex-1 overflow-auto" style={{ minHeight:0 }}>
          {/*
            The inner wrapper forces the component to stretch to card width.
            text-align:left undoes any centering the table applies internally.
          */}
          <div style={{ width:"100%", textAlign:"left" }}>
            <UsersComponent
              compact={false}
              pageSize={5}
              searchTerm={searchTerm}
              selectedRole={selectedRole}
              selectedStatus={selectedStatus}
            />
          </div>
        </div>
      </div>
    </div>
  );
}