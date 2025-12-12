// src/pages/Weighing.jsx - FINAL, FULLY STYLED, NO-SCROLL LAYOUT

import { Suspense, lazy } from "react";
import { useSelector, useDispatch } from "react-redux";
import { Loader2 } from "lucide-react";

// Assume these components now exist and are styled correctly (Light/Green Theme)
const WeighingForm = lazy(() => import("/src/components/WeighingForm"));
const HardwareControls = lazy(() => import("/src/components/HardwareControls"));
const TransactionList = lazy(() => import("/src/components/TransactionList"));

// Standard Section Loader (Formal Theme)
function SectionLoader({ title }) {
  return (
    <div className="flex items-center justify-center h-full text-gray-500">
      <Loader2 className="w-6 h-6 animate-spin mr-2" />
      <span className="text-lg">{title}</span>
    </div>
  );
}

export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);
  
  // Handlers (kept minimal for layout focus)
  const handleDeactivate = (id) => { /* ... */ };
  const handleComplete = (id, w2) => { /* ... */ };

  return (
    // Full Viewport Height, No external scrolling, Light theme background
    // We use dynamic height classes based on the screen to prevent scrollbar
    <div className="h-screen overflow-hidden bg-gray-100 text-gray-900 p-4 lg:p-6 flex flex-col gap-4 lg:gap-6">
      
      {/* 1. MAIN TRANSACTION FORM AREA (Approx 65% height) */}
      <div className="flex-grow min-h-0 basis-[65%]">
          <div className="h-full bg-white rounded-2xl shadow-xl p-6 overflow-y-auto">
            <h2 className="text-2xl font-bold text-gray-700 mb-4">New Weighing Transaction</h2>
            <Suspense fallback={<SectionLoader title="Loading weighing module..." />}>
              <WeighingForm />
            </Suspense>
          </div>
      </div>

      {/* 2. LOWER ROW: Hardware + Transactions (Approx 35% height) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 lg:gap-6 flex-grow min-h-0 basis-[35%]">
        
        {/* Hardware Controls (lg:col-span-4) */}
        <div className="lg:col-span-4 bg-white rounded-2xl shadow-xl p-4 flex flex-col min-h-0">
          <h2 className="text-xl font-semibold text-gray-700 mb-3">
            <i className="fas fa-microchip mr-2 opacity-60"></i> Hardware Controls
          </h2>
          <div className="flex-1 overflow-y-auto">
            <Suspense fallback={<SectionLoader title="Loading hardware controls..." />}>
              <HardwareControls refreshMs={1500} />
            </Suspense>
          </div>
        </div>

        {/* Transactions List (lg:col-span-8) */}
        <div className="lg:col-span-8 bg-white rounded-2xl shadow-xl p-4 flex flex-col min-h-0">
          <h2 className="text-xl font-semibold text-gray-700 mb-3">
            <i className="fas fa-list-alt mr-2 opacity-60"></i> Recent Transactions
          </h2>
          <div className="flex-1 overflow-y-auto">
            <Suspense fallback={<SectionLoader title="Loading transactions..." />}>
              <TransactionList
                transactions={transactions}
                onDeactivate={handleDeactivate}
                onComplete={handleComplete}
              />
            </Suspense>
          </div>
        </div>
      </div>
    </div>
  );
}