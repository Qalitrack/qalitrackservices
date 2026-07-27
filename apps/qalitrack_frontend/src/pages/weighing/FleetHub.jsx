import { useState } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
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

  const basePath = location.pathname.startsWith("/admin") ? "/admin" : "/operator";

  const selectTab = (id) => {
    setActiveTab(id);
    navigate(`${basePath}/fleet/${id}`, { replace: true });
  };

  const ActiveComponent = FLEET_TABS.find((t) => t.id === activeTab)?.Component;

  return (
    <div className="h-full bg-gray-50 overflow-hidden flex flex-col">
      <PageHeader icon={Truck} title="FLEET & TRANSPORT" subtitle="Vehicles, drivers, transporters, owners, and axle configuration" />

      {/* Tabs */}
      <div className="flex items-center gap-5 mb-3 flex-wrap px-4 sm:px-6 border-b border-gray-200 shrink-0">
        {FLEET_TABS.map((t) => (
          <button
            key={t.id}
            onClick={() => selectTab(t.id)}
            className={`flex items-center gap-1.5 pb-2 -mb-px text-sm font-medium border-b-2 transition-colors ${
              activeTab === t.id
                ? "border-amber-500 text-amber-600"
                : "border-transparent text-gray-500 hover:text-gray-700"
            }`}
          >
            {t.icon}
            {t.label}
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
