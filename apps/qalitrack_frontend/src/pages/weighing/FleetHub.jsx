import { useState, useEffect } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import { Tabs } from "antd";
import { Truck, User, Tractor, Users, List } from "lucide-react";
import PageHeader from "../../components/PageHeader.jsx";
import Vehicles from "../../components/weighing/Vehicles.jsx";
import Drivers from "../../components/weighing/Drivers.jsx";
import Transporters from "./TransporterFormModal.jsx";
import Owners from "./Owners.jsx";
import AxleConfigs from "./AxleConfigs.jsx";

const FLEET_TABS = [
  { id: "vehicles", label: "Vehicles", icon: <Truck size={14} />, Component: Vehicles },
  { id: "drivers", label: "Drivers", icon: <User size={14} />, Component: Drivers },
  { id: "transporters", label: "Transporters", icon: <Tractor size={14} />, Component: Transporters },
  { id: "owners", label: "Owners", icon: <Users size={14} />, Component: Owners },
  { id: "axle-config", label: "Axle Configuration", icon: <List size={14} />, Component: AxleConfigs },
];

export default function FleetHub() {
  const navigate = useNavigate();
  const location = useLocation();
  const { tab } = useParams();
  const initialTab = FLEET_TABS.some((t) => t.id === tab) ? tab : "vehicles";
  const [activeTab, setActiveTab] = useState(initialTab);
  // Populated by whichever tab is active (its own search/filter/refresh
  // controls), so they render inside this one shared header instead of each
  // tab drawing its own duplicate PageHeader underneath.
  const [tabActions, setTabActions] = useState(null);

  // useState(initialTab) only runs once on mount — sync on later :tab changes too
  // (browser back/forward, bookmarks, external links) since the component doesn't
  // remount just because the tab param changed.
  useEffect(() => {
    if (tab && FLEET_TABS.some((t) => t.id === tab) && tab !== activeTab) {
      setActiveTab(tab);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab]);

  const basePath = location.pathname.startsWith("/admin") ? "/admin" : "/operator";

  const selectTab = (id) => {
    setActiveTab(id);
    setTabActions(null);
    navigate(`${basePath}/fleet/${id}`, { replace: true });
  };

  const ActiveComponent = FLEET_TABS.find((t) => t.id === activeTab)?.Component;

  return (
    <div className="h-full flex flex-col rounded-lg shadow-md border border-gray-200 bg-white overflow-hidden">
      <PageHeader icon={Truck} title="FLEET & TRANSPORT" subtitle="Vehicles, drivers, transporters, owners, and axle configuration" actions={tabActions} flush className="border-b border-white/10" />

      {/* Tabs */}
      <Tabs
        activeKey={activeTab}
        onChange={selectTab}
        size="small"
        className="px-4 sm:px-6 shrink-0"
        items={FLEET_TABS.map((t) => ({
          key: t.id,
          label: (
            <span className="flex items-center gap-1.5">
              {t.icon}
              {t.label}
            </span>
          ),
        }))}
      />

      {/* Active tab content */}
      <div className="flex-1 overflow-hidden px-4 sm:px-6 pb-4 sm:pb-6">
        {ActiveComponent && <ActiveComponent onHeaderActionsChange={setTabActions} />}
      </div>
    </div>
  );
}
