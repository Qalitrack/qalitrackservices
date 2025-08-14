// src/pages/Weighing.jsx
import { Suspense, lazy } from "react";
import { useSelector, useDispatch } from "react-redux";
import { deactivateTransaction, completeWeighing } from "../store/weighingSlice";
import { Loader2 } from "lucide-react";

// Lazy load heavy sections
const WeighingForm = lazy(() => import("../components/WeighingForm"));
const WeighingTable = lazy(() => import("../components/WeighingTable"));
const CameraGrid = lazy(() => import("../components/CameraGrid"));

export default function Weighing() {
  const dispatch = useDispatch();
  const transactions = useSelector((state) => state.weighing.transactions);

  const handleDeactivate = (id) => {
    if (window.confirm("Deactivate this transaction?")) {
      dispatch(deactivateTransaction(id));
    }
  };

  const handleComplete = (id, w2) => {
    dispatch(completeWeighing({ id, w2 }));
  };

  return (
    <div className="p-4 space-y-4">
      <h1 className="text-2xl font-bold text-amber-600">Weighing Module</h1>

      {/* Form Section */}
      <Suspense fallback={<SectionLoader title="Loading form..." />}>
        <WeighingForm />
      </Suspense>

      {/* Cameras */}
      <Suspense fallback={<SectionLoader title="Loading cameras..." />}>
        <CameraGrid refreshMs={1500} />
      </Suspense>

      {/* Transactions Table */}
      <Suspense fallback={<SectionLoader title="Loading transactions..." />}>
        <WeighingTable
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
