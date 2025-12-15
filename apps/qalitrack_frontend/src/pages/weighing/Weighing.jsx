// src/pages/Weighing.jsx

import { Suspense, lazy } from "react";
import { Loader2 } from "lucide-react";

// Lazy load the main dashboard — this is now our full-featured weighing interface
const WeighingDashboard = lazy(() =>
    import("../../components/weighing/WeighingDashboard")
);

// Reusable centered loader
function SectionLoader({ title }) {
  return (
      <div className="flex items-center justify-center h-full text-gray-500">
        <Loader2 className="w-6 h-6 animate-spin mr-2" />
        <span className="text-lg">{title}</span>
      </div>
  );
}

export default function Weighing() {
  return (
      // Full-screen, no outer scroll, clean light theme
      <div className="h-screen overflow-hidden bg-gray-100 text-gray-900 flex flex-col">
        {/* Optional: Thin top padding or header space */}
        <div className="flex-1 p-4 lg:p-6">
          <div className="h-full bg-white rounded-2xl shadow-2xl overflow-hidden flex flex-col">
            {/* Optional title bar */}
            <div className="px-6 py-4 border-b border-gray-200">
              <h1 className="text-2xl lg:text-3xl font-bold text-gray-800">
                Weighbridge Operations Dashboard
              </h1>
              <p className="text-sm text-gray-600 mt-1">
                Create transactions • Capture live weights • Manage incomplete weighings
              </p>
            </div>

            {/* Main Content Area */}
            <div className="flex-1 overflow-y-auto bg-gray-50">
              <Suspense fallback={<SectionLoader title="Loading Weighing Dashboard..." />}>
                <WeighingDashboard />
              </Suspense>
            </div>
          </div>
        </div>
      </div>
  );
}