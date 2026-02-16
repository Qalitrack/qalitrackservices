import { useMemo, useState } from "react";
import {
  BarChart, Bar, LineChart, Line, RadarChart, Radar,
  PolarGrid, PolarAngleAxis, PolarRadiusAxis,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend,
  ResponsiveContainer, Cell
} from "recharts";
import { ArrowRight, TrendingUp, TrendingDown, Minus } from "lucide-react";

export default function CrossEntityComparison({ transactions = [] }) {
  const [entityType, setEntityType] = useState("drivers");
  const [metric, setMetric] = useState("weight");
  const [selectedEntities, setSelectedEntities] = useState([]);
  const [comparisonView, setComparisonView] = useState("bar");
  const [timeframe, setTimeframe] = useState("all");

  // Entity types
  const entityTypes = [
    { value: "drivers", label: "Drivers", key: "driverName" },
    { value: "customers", label: "Customers", key: "destinationName" },
    { value: "commodities", label: "Commodities", key: "commodityName" },
    { value: "suppliers", label: "Suppliers", key: "originName" },
    { value: "vehicles", label: "Vehicles", key: "noPlate" },
  ];

  // Metrics
  const metrics = [
    { value: "weight", label: "Total Weight", unit: "kg" },
    { value: "trips", label: "Trip Count", unit: "trips" },
    { value: "avgWeight", label: "Average Weight", unit: "kg/trip" },
    { value: "efficiency", label: "Efficiency Score", unit: "%" },
  ];

  // Get entity data
  const entityData = useMemo(() => {
    const currentType = entityTypes.find(t => t.value === entityType);
    if (!currentType) return [];

    const entityMap = {};

    transactions.forEach(t => {
      const entityName = t[currentType.key] || "Unknown";
      
      if (!entityMap[entityName]) {
        entityMap[entityName] = {
          name: entityName,
          trips: 0,
          totalWeight: 0,
          completedTrips: 0,
          avgTurnaround: [],
        };
      }

      entityMap[entityName].trips += 1;
      entityMap[entityName].totalWeight += parseFloat(t.netWeight) || 0;
      
      if (t.secondWeight && parseFloat(t.secondWeight) > 0) {
        entityMap[entityName].completedTrips += 1;
      }

      if (t.firstWeightTime && t.secondWeightTime) {
        const tat = new Date(t.secondWeightTime) - new Date(t.firstWeightTime);
        entityMap[entityName].avgTurnaround.push(tat / (1000 * 60)); // minutes
      }
    });

    // Calculate derived metrics
    return Object.values(entityMap)
      .map(entity => ({
        ...entity,
        avgWeight: entity.trips > 0 ? entity.totalWeight / entity.trips : 0,
        completionRate: entity.trips > 0 ? (entity.completedTrips / entity.trips) * 100 : 0,
        avgTAT: entity.avgTurnaround.length > 0
          ? entity.avgTurnaround.reduce((a, b) => a + b, 0) / entity.avgTurnaround.length
          : 0,
        efficiency: entity.trips > 0
          ? Math.min(100, (entity.completedTrips / entity.trips) * 100 * 
              (entity.totalWeight / (entity.trips * 5000))) // Assuming 5000kg target per trip
          : 0,
      }))
      .sort((a, b) => b.totalWeight - a.totalWeight);
  }, [transactions, entityType]);

  // Get comparison data for selected entities
  const comparisonData = useMemo(() => {
    if (selectedEntities.length === 0) return [];

    const selected = entityData.filter(e => selectedEntities.includes(e.name));
    
    // Format data based on metric
    return selected.map(entity => {
      let value;
      switch (metric) {
        case "weight":
          value = Math.round(entity.totalWeight);
          break;
        case "trips":
          value = entity.trips;
          break;
        case "avgWeight":
          value = Math.round(entity.avgWeight);
          break;
        case "efficiency":
          value = Math.round(entity.efficiency);
          break;
        default:
          value = 0;
      }

      return {
        name: entity.name,
        value,
        trips: entity.trips,
        weight: Math.round(entity.totalWeight),
        avgWeight: Math.round(entity.avgWeight),
        completionRate: Math.round(entity.completionRate),
        avgTAT: Math.round(entity.avgTAT),
        efficiency: Math.round(entity.efficiency),
      };
    });
  }, [entityData, selectedEntities, metric]);

  // Radar chart data (multi-metric comparison)
  const radarData = useMemo(() => {
    if (selectedEntities.length === 0) return [];

    const selected = entityData.filter(e => selectedEntities.includes(e.name));
    const maxWeight = Math.max(...selected.map(e => e.totalWeight));
    const maxTrips = Math.max(...selected.map(e => e.trips));
    const maxAvgWeight = Math.max(...selected.map(e => e.avgWeight));

    return [
      {
        metric: "Trips",
        ...selected.reduce((acc, e) => ({
          ...acc,
          [e.name]: Math.round((e.trips / maxTrips) * 100),
        }), {}),
        fullMark: 100,
      },
      {
        metric: "Total Weight",
        ...selected.reduce((acc, e) => ({
          ...acc,
          [e.name]: Math.round((e.totalWeight / maxWeight) * 100),
        }), {}),
        fullMark: 100,
      },
      {
        metric: "Avg Weight",
        ...selected.reduce((acc, e) => ({
          ...acc,
          [e.name]: Math.round((e.avgWeight / maxAvgWeight) * 100),
        }), {}),
        fullMark: 100,
      },
      {
        metric: "Completion Rate",
        ...selected.reduce((acc, e) => ({
          ...acc,
          [e.name]: Math.round(e.completionRate),
        }), {}),
        fullMark: 100,
      },
      {
        metric: "Efficiency",
        ...selected.reduce((acc, e) => ({
          ...acc,
          [e.name]: Math.round(e.efficiency),
        }), {}),
        fullMark: 100,
      },
    ];
  }, [entityData, selectedEntities]);

  // Toggle entity selection
  const toggleEntity = (entityName) => {
    if (selectedEntities.includes(entityName)) {
      setSelectedEntities(selectedEntities.filter(e => e !== entityName));
    } else if (selectedEntities.length < 5) {
      setSelectedEntities([...selectedEntities, entityName]);
    }
  };

  // Calculate growth/change indicators
  const getChangeIndicator = (value, index) => {
    if (index === 0) return null;
    const prevValue = comparisonData[index - 1]?.value || 0;
    if (prevValue === 0) return null;
    
    const change = ((value - prevValue) / prevValue) * 100;
    if (Math.abs(change) < 1) return <Minus className="w-3 h-3 text-gray-400" />;
    if (change > 0) return <TrendingUp className="w-3 h-3 text-green-600" />;
    return <TrendingDown className="w-3 h-3 text-red-600" />;
  };

  const COLORS = ["#f59e0b", "#10b981", "#3b82f6", "#ef4444", "#8b5cf6"];

  return (
    <div className="space-y-4">
      {/* CONTROLS - STICKY */}
      <div className="bg-white border border-amber-200 rounded-lg p-4 sticky top-0 z-10 shadow-sm backdrop-blur-sm bg-white/95">
        <h2 className="text-lg font-bold text-gray-900 mb-3">Cross-Entity Comparison</h2>
        
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
              Compare
            </label>
            <select
              value={entityType}
              onChange={(e) => {
                setEntityType(e.target.value);
                setSelectedEntities([]);
              }}
              className="w-full border border-amber-300 rounded px-3 py-1.5 text-sm"
            >
              {entityTypes.map(type => (
                <option key={type.value} value={type.value}>{type.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
              Metric
            </label>
            <select
              value={metric}
              onChange={(e) => setMetric(e.target.value)}
              className="w-full border border-amber-300 rounded px-3 py-1.5 text-sm"
            >
              {metrics.map(m => (
                <option key={m.value} value={m.value}>{m.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
              View
            </label>
            <select
              value={comparisonView}
              onChange={(e) => setComparisonView(e.target.value)}
              className="w-full border border-amber-300 rounded px-3 py-1.5 text-sm"
            >
              <option value="bar">Bar Chart</option>
              <option value="line">Line Chart</option>
              <option value="radar">Radar Chart</option>
              <option value="table">Table</option>
            </select>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* ENTITY SELECTION */}
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-bold text-gray-900">
              Select {entityTypes.find(t => t.value === entityType)?.label}
            </h3>
            <span className="text-xs text-gray-600">
              {selectedEntities.length}/5 selected
            </span>
          </div>

          <div className="space-y-1 max-h-96 overflow-y-auto">
            {entityData.slice(0, 20).map(entity => (
              <button
                key={entity.name}
                onClick={() => toggleEntity(entity.name)}
                disabled={!selectedEntities.includes(entity.name) && selectedEntities.length >= 5}
                className={`w-full text-left px-3 py-2 rounded text-xs transition-colors ${
                  selectedEntities.includes(entity.name)
                    ? "bg-amber-100 border-2 border-amber-500 font-semibold"
                    : "border border-gray-200 hover:bg-gray-50 disabled:opacity-50"
                }`}
              >
                <div className="flex items-center justify-between">
                  <span className="truncate">{entity.name}</span>
                  <span className="text-amber-600 font-bold ml-2">
                    {Math.round(entity.totalWeight).toLocaleString()}
                  </span>
                </div>
                <div className="text-[10px] text-gray-600 mt-0.5">
                  {entity.trips} trips • {Math.round(entity.avgWeight)} kg avg
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* VISUALIZATION */}
        <div className="lg:col-span-2 bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">
            Comparison: {metrics.find(m => m.value === metric)?.label}
          </h3>

          {selectedEntities.length === 0 ? (
            <div className="h-96 flex items-center justify-center text-gray-500">
              <div className="text-center">
                <ArrowRight className="w-12 h-12 mx-auto mb-3 opacity-30 rotate-180" />
                <p className="text-sm font-semibold">Select entities to compare</p>
                <p className="text-xs mt-1">Choose up to 5 {entityTypes.find(t => t.value === entityType)?.label.toLowerCase()}</p>
              </div>
            </div>
          ) : (
            <>
              {comparisonView === "bar" && (
                <ResponsiveContainer width="100%" height={350}>
                  <BarChart data={comparisonData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                    <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} />
                    <Tooltip
                      contentStyle={{ fontSize: 11, borderRadius: 8 }}
                      formatter={(value) => [
                        value.toLocaleString() + " " + (metrics.find(m => m.value === metric)?.unit || ""),
                        metrics.find(m => m.value === metric)?.label
                      ]}
                    />
                    <Bar dataKey="value" fill="#f59e0b">
                      {comparisonData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              )}

              {comparisonView === "line" && (
                <ResponsiveContainer width="100%" height={350}>
                  <LineChart data={comparisonData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                    <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} />
                    <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8 }} />
                    <Line
                      type="monotone"
                      dataKey="value"
                      stroke="#f59e0b"
                      strokeWidth={3}
                      dot={{ r: 6, fill: "#f59e0b" }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              )}

              {comparisonView === "radar" && (
                <ResponsiveContainer width="100%" height={350}>
                  <RadarChart data={radarData}>
                    <PolarGrid stroke="#e5e7eb" />
                    <PolarAngleAxis dataKey="metric" tick={{ fontSize: 11 }} />
                    <PolarRadiusAxis angle={90} domain={[0, 100]} tick={{ fontSize: 10 }} />
                    <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8 }} />
                    <Legend wrapperStyle={{ fontSize: 11 }} />
                    {selectedEntities.map((entity, index) => (
                      <Radar
                        key={entity}
                        name={entity}
                        dataKey={entity}
                        stroke={COLORS[index % COLORS.length]}
                        fill={COLORS[index % COLORS.length]}
                        fillOpacity={0.3}
                      />
                    ))}
                  </RadarChart>
                </ResponsiveContainer>
              )}

              {comparisonView === "table" && (
                <div className="overflow-auto">
                  <table className="w-full text-xs">
                    <thead className="bg-amber-50 sticky top-0">
                      <tr>
                        <th className="p-2 text-left font-semibold border-b">Entity</th>
                        <th className="p-2 text-right font-semibold border-b">Trips</th>
                        <th className="p-2 text-right font-semibold border-b">Total Weight</th>
                        <th className="p-2 text-right font-semibold border-b">Avg Weight</th>
                        <th className="p-2 text-right font-semibold border-b">Completion</th>
                        <th className="p-2 text-right font-semibold border-b">Efficiency</th>
                      </tr>
                    </thead>
                    <tbody>
                      {comparisonData.map((entity, idx) => (
                        <tr key={entity.name} className="border-b hover:bg-amber-50/30">
                          <td className="p-2 font-semibold">{entity.name}</td>
                          <td className="p-2 text-right">{entity.trips}</td>
                          <td className="p-2 text-right font-bold text-amber-600">
                            {entity.weight.toLocaleString()}
                          </td>
                          <td className="p-2 text-right">{entity.avgWeight.toLocaleString()}</td>
                          <td className="p-2 text-right">{entity.completionRate}%</td>
                          <td className="p-2 text-right">
                            <div className="flex items-center justify-end gap-1">
                              {entity.efficiency}%
                              {getChangeIndicator(entity.value, idx)}
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      {/* SUMMARY STATS */}
      {selectedEntities.length > 0 && (
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">Summary Statistics</h3>
          <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
            <StatCard
              label="Total Selected"
              value={selectedEntities.length}
              color="amber"
            />
            <StatCard
              label="Combined Trips"
              value={comparisonData.reduce((sum, e) => sum + e.trips, 0)}
              color="blue"
            />
            <StatCard
              label="Combined Weight"
              value={`${comparisonData.reduce((sum, e) => sum + e.weight, 0).toLocaleString()} kg`}
              color="green"
            />
            <StatCard
              label="Avg Efficiency"
              value={`${Math.round(comparisonData.reduce((sum, e) => sum + e.efficiency, 0) / comparisonData.length)}%`}
              color="purple"
            />
            <StatCard
              label="Best Performer"
              value={comparisonData[0]?.name || "N/A"}
              color="orange"
            />
          </div>
        </div>
      )}
    </div>
  );
}

function StatCard({ label, value, color = "amber" }) {
  const colors = {
    amber: "bg-amber-50 border-amber-200",
    blue: "bg-blue-50 border-blue-200",
    green: "bg-green-50 border-green-200",
    purple: "bg-purple-50 border-purple-200",
    orange: "bg-orange-50 border-orange-200",
  };

  return (
    <div className={`border rounded-lg p-3 ${colors[color]}`}>
      <div className="text-[10px] font-semibold text-gray-600 uppercase tracking-wide mb-1">
        {label}
      </div>
      <div className="text-sm font-bold text-gray-900 truncate">{value}</div>
    </div>
  );
}