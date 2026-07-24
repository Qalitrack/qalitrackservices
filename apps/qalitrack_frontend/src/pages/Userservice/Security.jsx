import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Shield, Users, Lock, ScrollText } from "lucide-react";
import Permissions from "./Permissions";
import Roles from "./Roles";
import PasswordPolicy from "./PasswordPolicy";
import AuditLogs from "./AuditLogs";

const SECURITY_TABS = [
  { id: "permissions", label: "Permissions", icon: <Shield size={14} />, Component: Permissions },
  { id: "roles", label: "Roles", icon: <Users size={14} />, Component: Roles },
  { id: "password-policy", label: "Password Policy", icon: <Lock size={14} />, Component: PasswordPolicy },
  { id: "audit-logs", label: "Audit Logs", icon: <ScrollText size={14} />, Component: AuditLogs },
];

export default function Security() {
  const navigate = useNavigate();
  const { tab } = useParams();
  // Falls back to Permissions for both the bare /security route and any
  // unrecognized :tab (e.g. an old bookmark), instead of rendering nothing.
  const initialTab = SECURITY_TABS.some((t) => t.id === tab) ? tab : "permissions";
  const [activeTab, setActiveTab] = useState(initialTab);
  const ActiveComponent = SECURITY_TABS.find((t) => t.id === activeTab)?.Component;

  const selectTab = (id) => {
    setActiveTab(id);
    navigate(`/admin/security/${id}`, { replace: true });
  };

  return (
    <div className="h-full bg-gray-50 overflow-hidden flex flex-col">
      {/* Header */}
      <div className="mb-3 mx-4 sm:mx-6 mt-4 sm:mt-6 rounded-lg px-4 py-2.5 shrink-0" style={{ backgroundColor: "var(--cs-appbar-bg)" }}>
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-lg cs-icon-box flex items-center justify-center shadow-sm shrink-0">
            <Shield className="w-5 h-5" style={{ color: "var(--cs-icon-accent)" }} />
          </div>
          <div>
            <div className="text-sm font-bold leading-tight" style={{ color: "var(--cs-appbar-text)" }}>SECURITY</div>
            <div className="text-[11px] font-medium leading-tight" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
              Permissions, roles, password policy, and audit logs
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-3 flex-wrap px-4 sm:px-6 shrink-0">
        {SECURITY_TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => selectTab(tab.id)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-all flex items-center gap-1.5 ${
              activeTab === tab.id
                ? "bg-gradient-to-r from-amber-500 to-amber-600 text-white border border-amber-500 shadow-sm"
                : "bg-white text-gray-700 hover:bg-amber-50 border border-gray-200"
            }`}
          >
            {tab.icon}
            {tab.label}
          </button>
        ))}
      </div>

      {/* Active tab content */}
      <div className="flex-1 overflow-hidden px-4 sm:px-6 pb-4 sm:pb-6">
        {ActiveComponent && <ActiveComponent />}
      </div>
    </div>
  );
}
