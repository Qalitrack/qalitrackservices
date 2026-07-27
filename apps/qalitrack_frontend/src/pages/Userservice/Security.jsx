import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Shield, Users, Lock, ScrollText } from "lucide-react";
import PageHeader from "../../components/PageHeader.jsx";
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

  // useState(initialTab) only runs once on mount — sync on later :tab changes too
  // (browser back/forward, bookmarks, external links) since the component doesn't
  // remount just because the tab param changed.
  useEffect(() => {
    if (tab && SECURITY_TABS.some((t) => t.id === tab) && tab !== activeTab) {
      setActiveTab(tab);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab]);

  const selectTab = (id) => {
    setActiveTab(id);
    navigate(`/admin/security/${id}`, { replace: true });
  };

  return (
    <div className="h-full bg-gray-50 overflow-hidden flex flex-col">
      <PageHeader icon={Shield} title="SECURITY" subtitle="Permissions, roles, password policy, and audit logs" />

      {/* Tabs */}
      <div className="flex items-center gap-5 mb-3 flex-wrap px-4 sm:px-6 border-b border-gray-200 shrink-0">
        {SECURITY_TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => selectTab(tab.id)}
            className={`flex items-center gap-1.5 pb-2 -mb-px text-sm font-medium border-b-2 transition-colors ${
              activeTab === tab.id
                ? "border-amber-500 text-amber-600"
                : "border-transparent text-gray-500 hover:text-gray-700"
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
