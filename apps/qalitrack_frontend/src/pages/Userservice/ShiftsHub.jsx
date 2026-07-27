import { useState, useEffect } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import { CalendarClock, UserCog, ClipboardList } from "lucide-react";
import PageHeader from "../../components/PageHeader.jsx";
import Shifts from "./Shifts";
import ShiftAssignment from "./ShiftAssignment";
import Attendance from "./Attendance";

const SHIFTS_TABS = [
  { id: "shifts", label: "Shifts", icon: <CalendarClock size={14} /> },
  { id: "attendance", label: "Attendance", icon: <ClipboardList size={14} /> },
  { id: "shift-assignment", label: "Shift Assignment", icon: <UserCog size={14} /> },
];

export default function ShiftsHub() {
  const navigate = useNavigate();
  const location = useLocation();
  const { tab } = useParams();
  // Falls back to Shifts for both the bare /shifts route and any unrecognized
  // :tab (e.g. an old bookmark), instead of rendering nothing.
  const initialTab = SHIFTS_TABS.some((t) => t.id === tab) ? tab : "shifts";
  const [activeTab, setActiveTab] = useState(initialTab);
  // Populated when "Attendance" is clicked on a specific shift instance (see
  // Shifts -> ShiftInstances -> onViewAttendance) so the Attendance tab opens
  // scoped to that instance instead of the default "all attendance" view.
  const [attendanceContext, setAttendanceContext] = useState(null);
  // Populated by whichever tab is active (its own search/filter/refresh
  // controls), so they render inside this one shared header instead of each
  // tab drawing its own duplicate PageHeader underneath.
  const [tabActions, setTabActions] = useState(null);

  // useState(initialTab) only runs once on mount — without this, navigating
  // directly to a sibling tab's URL (browser back/forward, a bookmark, or an
  // external link) while this component stays mounted wouldn't switch tabs,
  // since :tab changing alone doesn't remount the component.
  useEffect(() => {
    if (tab && SHIFTS_TABS.some((t) => t.id === tab) && tab !== activeTab) {
      setActiveTab(tab);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab]);

  // This hub is mounted under both /admin/shifts and /operator/shifts.
  const basePath = location.pathname.startsWith("/admin") ? "/admin" : "/operator";

  const selectTab = (id) => {
    setActiveTab(id);
    setTabActions(null);
    navigate(`${basePath}/shifts/${id}`, { replace: true });
  };

  const handleViewAttendance = (context) => {
    setAttendanceContext(context);
    selectTab("attendance");
  };

  return (
    <div className="h-full bg-gray-50 overflow-hidden flex flex-col">
      <PageHeader icon={CalendarClock} title="SHIFTS" subtitle="Shifts, attendance, and shift assignment" actions={tabActions} />

      {/* Tabs */}
      <div className="flex items-center gap-5 mb-3 flex-wrap px-4 sm:px-6 border-b border-gray-200 shrink-0">
        {SHIFTS_TABS.map((t) => (
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
        {activeTab === "shifts" && <Shifts onViewAttendance={handleViewAttendance} onHeaderActionsChange={setTabActions} />}
        {activeTab === "attendance" && <Attendance instanceContext={attendanceContext} onHeaderActionsChange={setTabActions} />}
        {activeTab === "shift-assignment" && <ShiftAssignment onHeaderActionsChange={setTabActions} />}
      </div>
    </div>
  );
}
