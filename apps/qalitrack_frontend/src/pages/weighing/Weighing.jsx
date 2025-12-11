import { Suspense, lazy } from "react";
import { useSelector, useDispatch } from "react-redux";
import { Loader2 } from "lucide-react";
import { deactivateTransaction, completeWeighing } from "/src/store/weighingSlice";

// Lazy loaded components
const WeighbridgePanel = lazy(() => import("/src/components/WeighbridgePanel"));
const CameraGrid = lazy(() => import("/src/components/CameraGrid"));
const WeighingForm = lazy(() => import("/src/components/WeighingForm"));
const TransactionList = lazy(() => import("/src/components/TransactionList"));
const HardwareControls = lazy(() => import("/src/components/HardwareControls"));
const LiveWeighbridgeStatus = lazy(() => import("/src/components/LiveWeighbridgeStatus"));
const LiveWeighingPage = lazy(() => import("/src/components/LiveWeighingPage"));

export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);

  const handleDeactivate = (id) => {
    if (window.confirm("Deactivate this transaction?")) {
      dispatch(deactivateTransaction(id));
    }
  };

  const handleComplete = (id, w2) => {
    if (!w2) {
      alert("Second weight missing!");
      return;
    }
    dispatch(completeWeighing({ id, w2 }));
  };

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-6">
      <h1 className="mb-6 text-3xl font-bold text-amber-600">Factory Weighing System</h1>

      {/* TOP BAR: Hardware Controls - Always visible & accessible */}
      <div className="mb-8">
        <div className="rounded-xl border bg-white shadow-lg overflow-hidden">
          <div className="bg-gradient-to-r from-red-600 to-rose-600 px-6 py-4">
            <h2 className="text-xl font-bold text-white flex items-center gap-3">
              Hardware Controls
            </h2>
          </div>
          <div className="p-6 bg-gray-100">
            <Suspense fallback={<SectionLoader title="Loading hardware controls..." />}>
              <HardwareControls refreshMs={1500} />
            </Suspense>
          </div>
        </div>
      </div>

      {/* MAIN CONTENT GRID */}
      <div className="min-h-screen bg-gray-50 p-4 md:p-6">
        
        {/* LEFT: Weighbridge + Cameras */}
        <div className="lg:col-span-7 space-y-6">
          {/* Weighbridge Panel */}
          <div className="rounded-xl border bg-white shadow-sm overflow-hidden">
            {/* <div className="bg-gradient-to-r from-amber-500 to-amber-600 px-5 py-3">
              <h2 className="text-lg font-semibold text-white">Weighbridge Status</h2>
            </div> */}
            <div>
              {/* <Suspense fallback={<SectionLoader title="Loading live weight..." />}>
                <LiveWeighbridgeStatus />
              </Suspense> */}
            </div>
            {/* <div className="p-5">
              <Suspense fallback={<SectionLoader title="Loading weighbridge..." />}>
                <WeighbridgePanel />
              </Suspense>
            </div> */}
            
          </div>

          {/* Camera Grid */}
          {/* <div className="rounded-xl border bg-white shadow-sm overflow-hidden">
            <div className="bg-gradient-to-r from-blue-600 to-cyan-600 px-5 py-3">
              <h2 className="text-lg font-semibold text-white">Live Camera Feeds</h2>
            </div>
            <div className="p-4 bg-gray-900 aspect-video">
              <Suspense fallback={<SectionLoader title="Loading cameras..." />}>
                <CameraGrid refreshMs={1500} />
              </Suspense>
            </div>
          </div> */}
        </div>

        {/* RIGHT: Only Weighing Form now */}
        <div className="lg:col-span-5">
          <div className="rounded-xl border bg-white shadow-sm overflow-hidden h-full">
            <div className="bg-gradient-to-r from-green-600 to-emerald-600 px-5 py-3">
              <h2 className="text-lg font-semibold text-white">New Weighing Transaction</h2>
            </div>
            {/* <div className="p-5">
              <Suspense fallback={<SectionLoader title="Loading form..." />}>
                < LiveWeighbridgeStatus/>
              </Suspense>
            </div> */}
            <div className="p-5">
              <Suspense fallback={<SectionLoader title="Loading form..." />}>
                < WeighingForm/>
              </Suspense>
            </div>
          </div>
        </div>
      </div>

      {/* BOTTOM: Transaction List (Full Width) */}
      <div className="mt-8">
        <div className="rounded-xl border bg-white shadow-sm overflow-hidden">
          <div className="bg-gradient-to-r from-gray-700 to-gray-900 px-6 py-4">
            <h2 className="text-xl font-semibold text-white">Recent Transactions</h2>
          </div>
          <div className="p-6">
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

function SectionLoader({ title }) {
  return (
    <div className="flex items-center justify-center py-12 text-gray-500">
      <Loader2 className="w-6 h-6 animate-spin mr-3" />
      <span className="text-lg">{title}</span>
    </div>
  );
}