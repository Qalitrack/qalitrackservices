import { Suspense, lazy } from "react";
import { useSelector, useDispatch } from "react-redux";
import { Loader2 } from "lucide-react";
import { deactivateTransaction, completeWeighing } from "/src/store/weighingSlice";

// Lazy load heavy components
const WeighbridgePanel = lazy(() => import("/src/components/WeighbridgePanel"));
const CameraGrid = lazy(() => import("/src/components/CameraGrid"));
const WeighingForm = lazy(() => import("/src/components/WeighingForm"));
const TransactionList = lazy(() => import("/src/components/TransactionList"));
const HardwareControls = lazy(() => import("/src/components/HardwareControls"));
export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);

  // Handlers
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
    <div className="p-4 space-y-6">
      <h1 className="text-2xl font-bold text-amber-600">Factory Weighing</h1>

      {/* Top Section: Weighbridge + Cameras + Form */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Left: Weighbridge + Cameras */}
        <div className="lg:col-span-2 space-y-4">
          <Suspense fallback={<SectionLoader title="Loading weighbridge..." />}>
            <form/>
          </Suspense>
          <div>
          <Suspense fallback={<SectionLoader title="Loading form..." />}>
            <WeighingForm />
          </Suspense>
        </div>
          {/* <Suspense fallback={<SectionLoader title="Loading cameras..." />}>
            <CameraGrid refreshMs={1500} />
          </Suspense> */}
          <Suspense fallback={<SectionLoader title="Loading controls..." />}>
            <HardwareControls refreshMs={1500} />
          </Suspense>
          
        
        </div>

        {/* Right: Transaction Form */}
        {/* <div>
          <Suspense fallback={<SectionLoader title="Loading form..." />}>
            <WeighingForm />
          </Suspense>
        </div> */}
      </div>

      {/* Bottom: Transaction List */}
      <Suspense fallback={<SectionLoader title="Loading transactions..." />}>
        <TransactionList
          transactions={transactions}
          onDeactivate={handleDeactivate}
          onComplete={handleComplete}
        />
      </Suspense>
    </div>
  );
}

function SectionLoader({ title }) {
  return (
    <div className="flex items-center gap-2 text-gray-500">
      <Loader2 className="w-5 h-5 animate-spin" /> {title}
    </div>
  );
}
