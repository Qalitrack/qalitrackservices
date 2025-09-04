// src/components/KPIWidget.jsx
export default function KPIWidget({ title, value, icon, bg = "bg-white" }) {
  return (
    <div
      className={`${bg} rounded-lg p-5 flex items-center space-x-4 
                  shadow-md hover:shadow-lg transition-all duration-300 
                  hover:-translate-y-1`}
    >
      {/* Icon Container */}
      <div className="flex-shrink-0 w-12 h-12 flex items-center justify-center rounded-full bg-amber-100">
        {icon}
      </div>

      {/* Text Content */}
      <div>
        <h4 className="text-sm font-medium text-gray-500">{title}</h4>
        <p className="text-2xl font-bold text-amber-500 animate-pulse">
          {value}
        </p>
      </div>
    </div>
  );
}
/Users/zawadi/qalitrackservices/apps/qalitrack_frontend/src/components/KPIWidget.jsx