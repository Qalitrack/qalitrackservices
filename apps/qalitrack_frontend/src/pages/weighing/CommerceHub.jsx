import { useState } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
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

  const basePath = location.pathname.startsWith("/admin") ? "/admin" : "/operator";

  const selectTab = (id) => {
    setActiveTab(id);
    navigate(`${basePath}/commerce/${id}`, { replace: true });
  };

  const ActiveComponent = COMMERCE_TABS.find((t) => t.id === activeTab)?.Component;

  return (
    <div className="h-full bg-gray-50 overflow-hidden flex flex-col">
      <PageHeader icon={ShoppingBag} title="COMMERCE" subtitle="Suppliers, products, and saccos" />

      {/* Tabs */}
      <div className="flex items-center gap-5 mb-3 flex-wrap px-4 sm:px-6 border-b border-gray-200 shrink-0">
        {COMMERCE_TABS.map((t) => (
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
