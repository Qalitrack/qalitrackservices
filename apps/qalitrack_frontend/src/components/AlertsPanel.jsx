import { AlertTriangle, Bell, Info } from "lucide-react";

export default function AlertsPanel({ alerts }) {
  return (
    <div className="bg-white rounded-lg shadow-md p-5">
      <h3 className="text-lg font-semibold text-gray-800 mb-4">
        Alerts & Errors
      </h3>

      {alerts.length === 0 ? (
        <p className="text-gray-500 text-sm">No alerts at the moment 🎉</p>
      ) : (
        <ul className="space-y-3">
          {alerts.map((alert, index) => (
            <li
              key={index}
              className={`flex items-center p-3 rounded-lg shadow-sm border-l-4 ${
                alert.type === "error"
                  ? "border-red-500 bg-red-50"
                  : alert.type === "warning"
                  ? "border-amber-500 bg-amber-50"
                  : "border-blue-500 bg-blue-50"
              }`}
            >
              {/* Icon */}
              <span className="mr-3">
                {alert.type === "error" && (
                  <AlertTriangle className="w-5 h-5 text-red-500" />
                )}
                {alert.type === "warning" && (
                  <Bell className="w-5 h-5 text-amber-500" />
                )}
                {alert.type === "info" && (
                  <Info className="w-5 h-5 text-blue-500" />
                )}
              </span>

              {/* Content */}
              <div>
                <p className="text-sm font-medium text-gray-800">
                  {alert.message}
                </p>
                <p className="text-xs text-gray-500">{alert.timestamp}</p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
