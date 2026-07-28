import { useState, useEffect } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import { Tabs } from "antd";
import { Satellite, ShoppingBag, User2 } from "lucide-react";
import PageHeader from "../../components/PageHeader.jsx";
import Suppliers from "../Suppliers.jsx";
import Products from "../../components/weighing/Product.jsx";
import Saccos from "../Saccos.jsx";

const COMMERCE_TABS = [
  { id: "suppliers", label: "Suppliers", icon: <Satellite size={14} />, Component: Suppliers },
  { id: "products", label: "Products", icon: <ShoppingBag size={14} />, Component: Products },
  { id: "saccos", label: "Saccos", icon: <User2 size={14} />, Component: Saccos },
];

export default function CommerceHub() {
  const navigate = useNavigate();
  const location = useLocation();
  const { tab } = useParams();
  const initialTab = COMMERCE_TABS.some((t) => t.id === tab) ? tab : "suppliers";
  const [activeTab, setActiveTab] = useState(initialTab);
  // Populated by whichever tab is active (its own search/filter/refresh
  // controls), so they render inside this one shared header instead of each
  // tab drawing its own duplicate PageHeader underneath.
  const [tabActions, setTabActions] = useState(null);

  // useState(initialTab) only runs once on mount — sync on later :tab changes too
  // (browser back/forward, bookmarks, external links) since the component doesn't
  // remount just because the tab param changed.
  useEffect(() => {
    if (tab && COMMERCE_TABS.some((t) => t.id === tab) && tab !== activeTab) {
      setActiveTab(tab);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab]);

  const basePath = location.pathname.startsWith("/admin") ? "/admin" : "/operator";

  const selectTab = (id) => {
    setActiveTab(id);
    setTabActions(null);
    navigate(`${basePath}/commerce/${id}`, { replace: true });
  };

  const ActiveComponent = COMMERCE_TABS.find((t) => t.id === activeTab)?.Component;

  return (
    <div className="h-full flex flex-col rounded-lg shadow-md border border-gray-200 bg-white overflow-hidden">
      <PageHeader icon={ShoppingBag} title="COMMERCE" subtitle="Suppliers, products, and saccos" actions={tabActions} flush className="border-b border-white/10" />

      {/* Tabs */}
      <Tabs
        activeKey={activeTab}
        onChange={selectTab}
        size="small"
        className="px-4 sm:px-6 shrink-0"
        items={COMMERCE_TABS.map((t) => ({
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
