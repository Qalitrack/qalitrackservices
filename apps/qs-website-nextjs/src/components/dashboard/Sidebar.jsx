'use client';

import { useRouter } from "next/navigation";
import logo from "@/assets/logorange.svg";
const Sidebar = ({ selectedView, setSelectedView }) => {
  const menuItems = [
    { key: "analytics", label: "Analytics" },
    { key: "products", label: "Products" },
    { key: "users", label: "Users" },
    { key: "profile", label: "My Profile" },
  ];
const router = useRouter(); 
  return (
    <aside className="w-full md:w-64 bg-gray-800 text-white p-6 space-y-4 shadow-lg">
        <div
        className="p-6 flex justify-center cursor-pointer hover:bg-gray-700 transition"
        onClick={() => router.push("/")} // ✅ redirect to homepage
      >
        <img
          src={logo.src}
          alt="Organization Logo"
          className="h-12 object-contain"
        />
      </div>
      <h2 className="text-2xl font-bold mb-6 text-amber-400">Admin Panel</h2>

      {menuItems.map((item) => (
        <button
          key={item.key}
          onClick={() => setSelectedView(item.key)}
          className={`w-full py-3 px-4 text-left rounded-md transition ${
            selectedView === item.key
              ? "bg-amber-500 text-white shadow-md"
              : "hover:bg-gray-700 text-gray-300"
          }`}
        >
          {item.label}
        </button>
      ))}
    </aside>
  );
};

export default Sidebar;
