import { useState, useMemo } from "react";
import {
  Calendar, Clock, Mail, User, Send, Repeat,
  Save, Trash2, Eye, Play, Pause, Check, X
} from "lucide-react";
import dayjs from "dayjs";

export default function ReportScheduler({ transactions = [] }) {
  const [schedules, setSchedules] = useState([]);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState(null);

  const [formData, setFormData] = useState({
    name: "",
    reportType: "transactions",
    frequency: "daily",
    dayOfWeek: "monday",
    dayOfMonth: 1,
    time: "09:00",
    recipients: "",
    format: "pdf",
    filters: {
      startDate: "",
      endDate: "",
      status: "",
    },
    enabled: true,
  });

  // Schedule frequency options
  const frequencies = [
    { value: "daily", label: "Daily" },
    { value: "weekly", label: "Weekly" },
    { value: "biweekly", label: "Bi-weekly" },
    { value: "monthly", label: "Monthly" },
    { value: "quarterly", label: "Quarterly" },
  ];

  const daysOfWeek = [
    "monday", "tuesday", "wednesday", "thursday",
    "friday", "saturday", "sunday"
  ];

  const reportTypes = [
    { value: "transactions", label: "Transaction Report" },
    { value: "drivers", label: "Driver Performance" },
    { value: "customers", label: "Customer Activity" },
    { value: "commodities", label: "Commodity Analysis" },
    { value: "suppliers", label: "Supplier Report" },
    { value: "summary", label: "Executive Summary" },
  ];

  // Create new schedule
  const createSchedule = () => {
    const newSchedule = {
      id: Date.now(),
      ...formData,
      createdAt: new Date().toISOString(),
      lastRun: null,
      nextRun: calculateNextRun(formData),
      runCount: 0,
      status: "active",
    };

    setSchedules([...schedules, newSchedule]);
    setShowCreateModal(false);
    resetForm();
  };

  // Calculate next run time
  const calculateNextRun = (schedule) => {
    const now = dayjs();
    const [hours, minutes] = schedule.time.split(":").map(Number);

    let nextRun = now.hour(hours).minute(minutes).second(0);

    if (nextRun.isBefore(now)) {
      nextRun = nextRun.add(1, "day");
    }

    switch (schedule.frequency) {
      case "weekly":
        while (nextRun.format("dddd").toLowerCase() !== schedule.dayOfWeek) {
          nextRun = nextRun.add(1, "day");
        }
        break;
      case "monthly":
        nextRun = nextRun.date(schedule.dayOfMonth);
        if (nextRun.isBefore(now)) {
          nextRun = nextRun.add(1, "month");
        }
        break;
      case "quarterly":
        // Next quarter start
        const currentQuarter = Math.floor(now.month() / 3);
        nextRun = now.month(currentQuarter * 3 + 3).date(1).hour(hours).minute(minutes);
        break;
      default:
        break;
    }

    return nextRun.toISOString();
  };

  // Toggle schedule enabled/disabled
  const toggleSchedule = (id) => {
    setSchedules(schedules.map(s =>
      s.id === id ? { ...s, enabled: !s.enabled } : s
    ));
  };

  // Delete schedule
  const deleteSchedule = (id) => {
    if (confirm("Are you sure you want to delete this schedule?")) {
      setSchedules(schedules.filter(s => s.id !== id));
    }
  };

  // Run schedule manually
  const runScheduleNow = (schedule) => {
    // Simulate sending report
    const updatedSchedule = {
      ...schedule,
      lastRun: new Date().toISOString(),
      runCount: schedule.runCount + 1,
      nextRun: calculateNextRun(schedule),
    };

    setSchedules(schedules.map(s => s.id === schedule.id ? updatedSchedule : s));
    alert(`Report "${schedule.name}" sent successfully to ${schedule.recipients}`);
  };

  // Reset form
  const resetForm = () => {
    setFormData({
      name: "",
      reportType: "transactions",
      frequency: "daily",
      dayOfWeek: "monday",
      dayOfMonth: 1,
      time: "09:00",
      recipients: "",
      format: "pdf",
      filters: {
        startDate: "",
        endDate: "",
        status: "",
      },
      enabled: true,
    });
    setEditingSchedule(null);
  };

  // Schedule statistics
  const stats = useMemo(() => {
    const active = schedules.filter(s => s.enabled).length;
    const totalRuns = schedules.reduce((sum, s) => sum + s.runCount, 0);
    const avgRuns = schedules.length > 0 ? totalRuns / schedules.length : 0;

    return {
      total: schedules.length,
      active,
      inactive: schedules.length - active,
      totalRuns,
      avgRuns: Math.round(avgRuns),
    };
  }, [schedules]);

  return (
    <div className="space-y-4">
      {/* HEADER */}
      <div className="bg-white border border-amber-200 rounded-lg p-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-lg font-bold text-gray-900">Report Scheduler</h2>
            <p className="text-xs text-gray-600 mt-1">
              Automate report generation and email delivery
            </p>
          </div>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg text-sm font-semibold hover:bg-amber-600"
          >
            <Calendar size={16} />
            New Schedule
          </button>
        </div>
      </div>

      {/* STATISTICS */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
        <StatCard label="Total Schedules" value={stats.total} color="amber" />
        <StatCard label="Active" value={stats.active} color="green" />
        <StatCard label="Inactive" value={stats.inactive} color="gray" />
        <StatCard label="Total Runs" value={stats.totalRuns} color="blue" />
        <StatCard label="Avg Runs" value={stats.avgRuns} color="purple" />
      </div>

      {/* SCHEDULES LIST */}
      <div className="bg-white border border-amber-200 rounded-lg overflow-hidden">
        <div className="p-3 bg-gradient-to-r from-amber-50 to-orange-50 border-b border-amber-200">
          <h3 className="text-sm font-bold text-gray-900">Scheduled Reports</h3>
        </div>

        {schedules.length === 0 ? (
          <div className="p-8 text-center text-gray-500">
            <Calendar className="w-12 h-12 mx-auto mb-3 opacity-30" />
            <p className="text-sm font-semibold">No schedules configured</p>
            <p className="text-xs mt-1">Create your first schedule to automate reports</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {schedules.map(schedule => (
              <div
                key={schedule.id}
                className={`p-4 transition-colors ${
                  schedule.enabled ? "hover:bg-amber-50/30" : "bg-gray-50"
                }`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-2">
                      <h4 className="text-sm font-bold text-gray-900">{schedule.name}</h4>
                      <span
                        className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                          schedule.enabled
                            ? "bg-green-100 text-green-800 border border-green-300"
                            : "bg-gray-200 text-gray-600"
                        }`}
                      >
                        {schedule.enabled ? "ACTIVE" : "PAUSED"}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-xs">
                      <div>
                        <span className="text-gray-600">Type:</span>
                        <div className="font-semibold text-gray-900 capitalize">
                          {reportTypes.find(t => t.value === schedule.reportType)?.label}
                        </div>
                      </div>
                      <div>
                        <span className="text-gray-600">Frequency:</span>
                        <div className="font-semibold text-gray-900 capitalize">
                          {schedule.frequency}
                        </div>
                      </div>
                      <div>
                        <span className="text-gray-600">Time:</span>
                        <div className="font-semibold text-gray-900">{schedule.time}</div>
                      </div>
                      <div>
                        <span className="text-gray-600">Format:</span>
                        <div className="font-semibold text-gray-900 uppercase">
                          {schedule.format}
                        </div>
                      </div>
                    </div>

                    <div className="mt-2 flex flex-wrap items-center gap-3 text-xs">
                      <div className="flex items-center gap-1 text-gray-600">
                        <Mail size={12} />
                        <span>{schedule.recipients.split(",").length} recipient(s)</span>
                      </div>
                      <div className="flex items-center gap-1 text-gray-600">
                        <Repeat size={12} />
                        <span>{schedule.runCount} runs</span>
                      </div>
                      {schedule.lastRun && (
                        <div className="flex items-center gap-1 text-gray-600">
                          <Clock size={12} />
                          <span>Last: {dayjs(schedule.lastRun).format("DD MMM, HH:mm")}</span>
                        </div>
                      )}
                      {schedule.nextRun && schedule.enabled && (
                        <div className="flex items-center gap-1 text-green-600 font-semibold">
                          <Clock size={12} />
                          <span>Next: {dayjs(schedule.nextRun).format("DD MMM, HH:mm")}</span>
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="flex gap-1 ml-4">
                    <button
                      onClick={() => runScheduleNow(schedule)}
                      className="p-1.5 hover:bg-blue-100 rounded text-blue-600"
                      title="Run now"
                    >
                      <Play size={14} />
                    </button>
                    <button
                      onClick={() => toggleSchedule(schedule.id)}
                      className="p-1.5 hover:bg-amber-100 rounded text-amber-600"
                      title={schedule.enabled ? "Pause" : "Resume"}
                    >
                      {schedule.enabled ? <Pause size={14} /> : <Play size={14} />}
                    </button>
                    <button
                      onClick={() => deleteSchedule(schedule.id)}
                      className="p-1.5 hover:bg-red-100 rounded text-red-600"
                      title="Delete"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* CREATE MODAL */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="p-4 border-b bg-amber-50 sticky top-0">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-bold text-gray-900">Create Schedule</h3>
                <button
                  onClick={() => {
                    setShowCreateModal(false);
                    resetForm();
                  }}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <X size={20} />
                </button>
              </div>
            </div>

            <div className="p-4 space-y-4">
              {/* Schedule Name */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  Schedule Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                  placeholder="e.g., Weekly Driver Performance Report"
                />
              </div>

              {/* Report Type */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  Report Type *
                </label>
                <select
                  value={formData.reportType}
                  onChange={(e) => setFormData({ ...formData, reportType: e.target.value })}
                  className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                >
                  {reportTypes.map(type => (
                    <option key={type.value} value={type.value}>{type.label}</option>
                  ))}
                </select>
              </div>

              {/* Frequency */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1">
                    Frequency *
                  </label>
                  <select
                    value={formData.frequency}
                    onChange={(e) => setFormData({ ...formData, frequency: e.target.value })}
                    className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                  >
                    {frequencies.map(freq => (
                      <option key={freq.value} value={freq.value}>{freq.label}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1">
                    Time *
                  </label>
                  <input
                    type="time"
                    value={formData.time}
                    onChange={(e) => setFormData({ ...formData, time: e.target.value })}
                    className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                  />
                </div>
              </div>

              {/* Day of Week (for weekly) */}
              {formData.frequency === "weekly" && (
                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1">
                    Day of Week
                  </label>
                  <select
                    value={formData.dayOfWeek}
                    onChange={(e) => setFormData({ ...formData, dayOfWeek: e.target.value })}
                    className="w-full border border-gray-300 rounded px-3 py-2 text-sm capitalize"
                  >
                    {daysOfWeek.map(day => (
                      <option key={day} value={day} className="capitalize">{day}</option>
                    ))}
                  </select>
                </div>
              )}

              {/* Day of Month (for monthly) */}
              {formData.frequency === "monthly" && (
                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1">
                    Day of Month
                  </label>
                  <input
                    type="number"
                    min="1"
                    max="31"
                    value={formData.dayOfMonth}
                    onChange={(e) => setFormData({ ...formData, dayOfMonth: parseInt(e.target.value) })}
                    className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                  />
                </div>
              )}

              {/* Recipients */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  Email Recipients * (comma-separated)
                </label>
                <textarea
                  value={formData.recipients}
                  onChange={(e) => setFormData({ ...formData, recipients: e.target.value })}
                  className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
                  rows="2"
                  placeholder="email1@example.com, email2@example.com"
                />
              </div>

              {/* Format */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  Export Format
                </label>
                <div className="flex gap-3">
                  {["pdf", "excel", "both"].map(format => (
                    <label key={format} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="radio"
                        name="format"
                        value={format}
                        checked={formData.format === format}
                        onChange={(e) => setFormData({ ...formData, format: e.target.value })}
                        className="text-amber-500"
                      />
                      <span className="text-sm capitalize">{format}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>

            <div className="p-4 border-t bg-gray-50 flex justify-end gap-2 sticky bottom-0">
              <button
                onClick={() => {
                  setShowCreateModal(false);
                  resetForm();
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium hover:bg-white"
              >
                Cancel
              </button>
              <button
                onClick={createSchedule}
                disabled={!formData.name || !formData.recipients}
                className="px-4 py-2 bg-amber-500 text-white rounded-lg text-sm font-semibold hover:bg-amber-600 disabled:opacity-50"
              >
                Create Schedule
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function StatCard({ label, value, color = "amber" }) {
  const colors = {
    amber: "bg-amber-50 border-amber-200 text-amber-900",
    green: "bg-green-50 border-green-200 text-green-900",
    blue: "bg-blue-50 border-blue-200 text-blue-900",
    gray: "bg-gray-50 border-gray-200 text-gray-900",
    purple: "bg-purple-50 border-purple-200 text-purple-900",
  };

  return (
    <div className={`border rounded-lg p-3 ${colors[color]}`}>
      <div className="text-[10px] font-semibold uppercase tracking-wide opacity-75">
        {label}
      </div>
      <div className="text-2xl font-bold mt-1">{value}</div>
    </div>
  );
}