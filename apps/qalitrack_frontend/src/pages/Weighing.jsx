import { Suspense, lazy } from "react";
import { useSelector, useDispatch } from "react-redux";
import { Loader2 } from "lucide-react";
import toast from "react-hot-toast";
import { deactivateTransaction, completeWeighing } from "../store/weighingSlice";

// Lazy load heavy components
const WeighbridgePanel = lazy(() => import("../components/WeighbridgePanel"));
const CameraGrid = lazy(() => import("../components/CameraGrid"));
const WeighingForm = lazy(() => import("../components/WeighingForm"));
const TransactionList = lazy(() => import("../components/TransactionList"));
const HardwareControls = lazy(() => import("../components/HardwareControls"));

export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);

  // Deactivate Handler
  const handleDeactivate = (id) => {
    if (window.confirm("Deactivate this transaction?")) {
      dispatch(deactivateTransaction(id));
      toast.success("Transaction deactivated");
    }
  };

  // Complete Handler with inbound/outbound rules
  const handleComplete = (id, w2) => {
    if (!w2) {
      toast.error("Second weight is required!");
      return;
    }

    const tx = transactions.find((t) => t.id === id);
    if (!tx) return;

    const w2Val = parseFloat(w2);

    if (tx.type === "inbound" && w2Val >= tx.w1) {
      toast.error("🚨 For inbound, W2 must be LESS than W1!");
      return;
    }
    if (tx.type === "outbound" && w2Val <= tx.w1) {
      toast.error("🚨 For outbound, W2 must be GREATER than W1!");
      return;
    }

    dispatch(completeWeighing({ id, w2: w2Val }));
    toast.success("Weighing completed successfully");
  };

  return (
    <div className="p-4 space-y-6">
      <h1 className="text-2xl font-bold text-amber-600">Factory Weighing</h1>

      {/* Top Section: Weighbridge + Cameras + Form */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Left: Weighbridge + Cameras */}
        <div className="lg:col-span-2 space-y-4">
          <Suspense fallback={<SectionLoader title="Loading weighbridge..." />}>
            <WeighbridgePanel />
          </Suspense>
          <Suspense fallback={<SectionLoader title="Loading cameras..." />}>
            <CameraGrid refreshMs={1500} />
          </Suspense>
          <Suspense fallback={<SectionLoader title="Loading controls..." />}>
            <HardwareControls refreshMs={1500} />
          </Suspense>
        </div>

        {/* Right: Transaction Form */}
        <div>
          <Suspense fallback={<SectionLoader title="Loading form..." />}>
            <WeighingForm />
          </Suspense>
        </div>
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
