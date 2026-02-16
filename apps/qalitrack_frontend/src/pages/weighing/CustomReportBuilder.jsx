import { useState, useMemo } from "react";
import {
  Plus, X, Save, Eye, Download, Settings,
  GripVertical, ChevronDown, ChevronUp
} from "lucide-react";
import dayjs from "dayjs";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";

export default function CustomReportBuilder({ transactions = [] }) {
  const [reportName, setReportName] = useState("Custom Report");
  const [selectedFields, setSelectedFields] = useState([
    "receiptNo", "createdAt", "noPlate", "driverName",
    "commodityName", "netWeight"
  ]);
  const [filters, setFilters] = useState([]);
  const [groupBy, setGroupBy] = useState(null);
  const [sortBy, setSortBy] = useState({ field: "createdAt", order: "desc" });
  const [aggregations, setAggregations] = useState([]);
  const [showPreview, setShowPreview] = useState(false);
  const [savedReports, setSavedReports] = useState([]);

  // Available fields
  const availableFields = [
    { key: "receiptNo", label: "Receipt No", type: "text" },
    { key: "createdAt", label: "Date/Time", type: "datetime" },
    { key: "noPlate", label: "Vehicle", type: "text" },
    { key: "driverName", label: "Driver", type: "text" },
    { key: "commodityName", label: "Commodity", type: "text" },
    { key: "supplierName", label: "Supplier", type: "text" },
    { key: "transporterName", label: "Transporter", type: "text" },
    { key: "customerName", label: "Customer", type: "text" },
    { key: "originName", label: "Origin", type: "text" },
    { key: "destinationName", label: "Destination", type: "text" },
    { key: "weighBridgeName", label: "Weighbridge", type: "text" },
    { key: "weighMode", label: "Weigh Mode", type: "text" },
    { key: "operation", label: "Operation", type: "text" },
    { key: "operatorName", label: "Operator", type: "text" },
    { key: "firstWeight", label: "First Weight", type: "number" },
    { key: "secondWeight", label: "Second Weight", type: "number" },
    { key: "netWeight", label: "Net Weight", type: "number" },
    { key: "status", label: "Status", type: "text" },
  ];

  // Filter operators by field type
  const getOperators = (type) => {
    if (type === "number") {
      return ["equals", "not equals", "greater than", "less than", "between"];
    }
    if (type === "datetime") {
      return ["equals", "before", "after", "between"];
    }
    return ["contains", "equals", "not equals", "starts with", "ends with"];
  };

  // Add field to report
  const addField = (fieldKey) => {
    if (!selectedFields.includes(fieldKey)) {
      setSelectedFields([...selectedFields, fieldKey]);
    }
  };

  // Remove field
  const removeField = (fieldKey) => {
    setSelectedFields(selectedFields.filter(f => f !== fieldKey));
  };

  // Move field up/down
  const moveField = (fieldKey, direction) => {
    const index = selectedFields.indexOf(fieldKey);
    if (direction === "up" && index > 0) {
      const newFields = [...selectedFields];
      [newFields[index], newFields[index - 1]] = [newFields[index - 1], newFields[index]];
      setSelectedFields(newFields);
    } else if (direction === "down" && index < selectedFields.length - 1) {
      const newFields = [...selectedFields];
      [newFields[index], newFields[index + 1]] = [newFields[index + 1], newFields[index]];
      setSelectedFields(newFields);
    }
  };

  // Add filter
  const addFilter = () => {
    setFilters([
      ...filters,
      {
        id: Date.now(),
        field: "receiptNo",
        operator: "contains",
        value: "",
      },
    ]);
  };

  // Update filter
  const updateFilter = (id, updates) => {
    setFilters(filters.map(f => f.id === id ? { ...f, ...updates } : f));
  };

  // Remove filter
  const removeFilter = (id) => {
    setFilters(filters.filter(f => f.id !== id));
  };

  // Add aggregation
  const addAggregation = () => {
    setAggregations([
      ...aggregations,
      {
        id: Date.now(),
        field: "netWeight",
        function: "sum",
        label: "Total Weight",
      },
    ]);
  };

  // Generate report data
  const reportData = useMemo(() => {
    let data = [...transactions];

    // Apply filters
    filters.forEach(filter => {
      const field = availableFields.find(f => f.key === filter.field);
      if (!field || !filter.value) return;

      data = data.filter(row => {
        const value = row[filter.field];
        
        if (field.type === "number") {
          const numValue = parseFloat(value);
          const filterValue = parseFloat(filter.value);
          
          switch (filter.operator) {
            case "equals": return numValue === filterValue;
            case "not equals": return numValue !== filterValue;
            case "greater than": return numValue > filterValue;
            case "less than": return numValue < filterValue;
            default: return true;
          }
        } else if (field.type === "datetime") {
          const dateValue = dayjs(value);
          const filterDate = dayjs(filter.value);
          
          switch (filter.operator) {
            case "equals": return dateValue.isSame(filterDate, "day");
            case "before": return dateValue.isBefore(filterDate);
            case "after": return dateValue.isAfter(filterDate);
            default: return true;
          }
        } else {
          const strValue = String(value || "").toLowerCase();
          const filterValue = String(filter.value).toLowerCase();
          
          switch (filter.operator) {
            case "contains": return strValue.includes(filterValue);
            case "equals": return strValue === filterValue;
            case "not equals": return strValue !== filterValue;
            case "starts with": return strValue.startsWith(filterValue);
            case "ends with": return strValue.endsWith(filterValue);
            default: return true;
          }
        }
      });
    });

    // Apply sorting
    if (sortBy.field) {
      data.sort((a, b) => {
        const aVal = a[sortBy.field];
        const bVal = b[sortBy.field];
        
        if (sortBy.order === "asc") {
          return aVal > bVal ? 1 : -1;
        } else {
          return aVal < bVal ? 1 : -1;
        }
      });
    }

    // Group by (if specified)
    if (groupBy) {
      const grouped = {};
      data.forEach(row => {
        const key = row[groupBy] || "Unknown";
        if (!grouped[key]) grouped[key] = [];
        grouped[key].push(row);
      });
      return grouped;
    }

    return data;
  }, [transactions, filters, sortBy, groupBy, selectedFields, availableFields]);

  // Calculate aggregations
  const aggregatedValues = useMemo(() => {
    const results = {};
    const dataArray = Array.isArray(reportData) ? reportData : Object.values(reportData).flat();
    
    aggregations.forEach(agg => {
      const values = dataArray.map(row => parseFloat(row[agg.field]) || 0);
      
      switch (agg.function) {
        case "sum":
          results[agg.id] = values.reduce((a, b) => a + b, 0);
          break;
        case "avg":
          results[agg.id] = values.reduce((a, b) => a + b, 0) / values.length;
          break;
        case "min":
          results[agg.id] = Math.min(...values);
          break;
        case "max":
          results[agg.id] = Math.max(...values);
          break;
        case "count":
          results[agg.id] = dataArray.length;
          break;
        default:
          results[agg.id] = 0;
      }
    });
    
    return results;
  }, [reportData, aggregations]);

  // Save report configuration
  const saveReport = () => {
    const config = {
      id: Date.now(),
      name: reportName,
      fields: selectedFields,
      filters,
      groupBy,
      sortBy,
      aggregations,
      createdAt: new Date().toISOString(),
    };
    setSavedReports([...savedReports, config]);
    alert("Report saved successfully!");
  };

  // Load report configuration
  const loadReport = (config) => {
    setReportName(config.name);
    setSelectedFields(config.fields);
    setFilters(config.filters);
    setGroupBy(config.groupBy);
    setSortBy(config.sortBy);
    setAggregations(config.aggregations);
  };

  // Export functions
  const exportPDF = () => {
    const doc = new jsPDF("landscape");
    const dataArray = Array.isArray(reportData) ? reportData : Object.values(reportData).flat();
    
    doc.setFillColor(245, 158, 11);
    doc.rect(0, 0, doc.internal.pageSize.getWidth(), 20, "F");
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(16);
    doc.text(reportName.toUpperCase(), doc.internal.pageSize.getWidth() / 2, 12, { align: "center" });

    const headers = selectedFields.map(fieldKey => {
      const field = availableFields.find(f => f.key === fieldKey);
      return field ? field.label : fieldKey;
    });

    const body = dataArray.map(row => 
      selectedFields.map(fieldKey => {
        const value = row[fieldKey];
        if (fieldKey === "createdAt") {
          return dayjs(value).format("DD MMM YY HH:mm");
        }
        if (fieldKey.includes("Weight")) {
          return value ? parseFloat(value).toLocaleString() : "-";
        }
        return value || "-";
      })
    );

    autoTable(doc, {
      startY: 25,
      head: [headers],
      body,
      styles: { fontSize: 8 },
      headStyles: { fillColor: [245, 158, 11] },
    });

    doc.save(`${reportName.replace(/\s+/g, "-")}.pdf`);
  };

  const exportExcel = () => {
    const dataArray = Array.isArray(reportData) ? reportData : Object.values(reportData).flat();
    
    const ws = XLSX.utils.json_to_sheet(
      dataArray.map(row => {
        const obj = {};
        selectedFields.forEach(fieldKey => {
          const field = availableFields.find(f => f.key === fieldKey);
          const label = field ? field.label : fieldKey;
          obj[label] = row[fieldKey] || "-";
        });
        return obj;
      })
    );

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, reportName);
    XLSX.writeFile(wb, `${reportName.replace(/\s+/g, "-")}.xlsx`);
  };

  return (
    <div className="space-y-4">
      {/* HEADER - STICKY */}
      <div className="bg-white border border-amber-200 rounded-lg p-4 sticky top-0 z-10 shadow-sm backdrop-blur-sm bg-white/95">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            <Settings className="w-5 h-5 text-amber-600" />
            <h2 className="text-lg font-bold text-gray-900">Custom Report Builder</h2>
          </div>
          <input
            type="text"
            value={reportName}
            onChange={(e) => setReportName(e.target.value)}
            className="border border-amber-300 rounded px-3 py-1 text-sm font-semibold focus:ring-2 focus:ring-amber-200"
            placeholder="Report Name"
          />
        </div>

        <div className="flex gap-2">
          <button
            onClick={saveReport}
            className="flex items-center gap-1 px-3 py-1.5 bg-amber-500 text-white rounded-lg text-xs font-semibold hover:bg-amber-600"
          >
            <Save size={14} />
            Save
          </button>
          <button
            onClick={() => setShowPreview(!showPreview)}
            className="flex items-center gap-1 px-3 py-1.5 border border-amber-300 rounded-lg text-xs font-semibold hover:bg-amber-50"
          >
            <Eye size={14} />
            Preview
          </button>
          <button
            onClick={exportPDF}
            className="flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-xs font-semibold hover:bg-gray-50"
          >
            <Download size={14} />
            PDF
          </button>
          <button
            onClick={exportExcel}
            className="flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-xs font-semibold hover:bg-gray-50"
          >
            <Download size={14} />
            Excel
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* FIELD SELECTOR */}
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">Available Fields</h3>
          <div className="space-y-1 max-h-96 overflow-y-auto">
            {availableFields.map(field => (
              <button
                key={field.key}
                onClick={() => addField(field.key)}
                disabled={selectedFields.includes(field.key)}
                className={`w-full text-left px-2 py-1.5 rounded text-xs font-medium transition-colors ${
                  selectedFields.includes(field.key)
                    ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                    : "hover:bg-amber-50 border border-gray-200"
                }`}
              >
                {field.label}
              </button>
            ))}
          </div>
        </div>

        {/* SELECTED FIELDS */}
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">Selected Fields</h3>
          <div className="space-y-1 max-h-96 overflow-y-auto">
            {selectedFields.map(fieldKey => {
              const field = availableFields.find(f => f.key === fieldKey);
              return (
                <div
                  key={fieldKey}
                  className="flex items-center gap-2 px-2 py-1.5 bg-amber-50 border border-amber-200 rounded"
                >
                  <GripVertical className="w-4 h-4 text-gray-400 cursor-move" />
                  <span className="flex-1 text-xs font-medium">{field?.label || fieldKey}</span>
                  <button
                    onClick={() => moveField(fieldKey, "up")}
                    className="p-0.5 hover:bg-amber-100 rounded"
                  >
                    <ChevronUp size={14} />
                  </button>
                  <button
                    onClick={() => moveField(fieldKey, "down")}
                    className="p-0.5 hover:bg-amber-100 rounded"
                  >
                    <ChevronDown size={14} />
                  </button>
                  <button
                    onClick={() => removeField(fieldKey)}
                    className="p-0.5 hover:bg-red-100 rounded text-red-600"
                  >
                    <X size={14} />
                  </button>
                </div>
              );
            })}
          </div>
        </div>

        {/* CONFIGURATION */}
        <div className="space-y-4">
          {/* FILTERS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-bold text-gray-900">Filters</h3>
              <button
                onClick={addFilter}
                className="flex items-center gap-1 px-2 py-1 bg-amber-100 text-amber-900 rounded text-xs font-semibold hover:bg-amber-200"
              >
                <Plus size={12} />
                Add
              </button>
            </div>
            <div className="space-y-2">
              {filters.map(filter => {
                const field = availableFields.find(f => f.key === filter.field);
                return (
                  <div key={filter.id} className="border border-gray-200 rounded p-2 space-y-1">
                    <select
                      value={filter.field}
                      onChange={(e) => updateFilter(filter.id, { field: e.target.value })}
                      className="w-full border border-gray-300 rounded px-2 py-1 text-xs"
                    >
                      {availableFields.map(f => (
                        <option key={f.key} value={f.key}>{f.label}</option>
                      ))}
                    </select>
                    <select
                      value={filter.operator}
                      onChange={(e) => updateFilter(filter.id, { operator: e.target.value })}
                      className="w-full border border-gray-300 rounded px-2 py-1 text-xs"
                    >
                      {getOperators(field?.type || "text").map(op => (
                        <option key={op} value={op}>{op}</option>
                      ))}
                    </select>
                    <div className="flex gap-1">
                      <input
                        type={field?.type === "number" ? "number" : field?.type === "datetime" ? "date" : "text"}
                        value={filter.value}
                        onChange={(e) => updateFilter(filter.id, { value: e.target.value })}
                        className="flex-1 border border-gray-300 rounded px-2 py-1 text-xs"
                        placeholder="Value"
                      />
                      <button
                        onClick={() => removeFilter(filter.id)}
                        className="px-2 bg-red-100 text-red-600 rounded hover:bg-red-200"
                      >
                        <X size={12} />
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* SORT & GROUP */}
          <div className="bg-white border border-amber-200 rounded-lg p-4 space-y-3">
            <div>
              <label className="text-xs font-semibold text-gray-700 block mb-1">Sort By</label>
              <div className="flex gap-2">
                <select
                  value={sortBy.field}
                  onChange={(e) => setSortBy({ ...sortBy, field: e.target.value })}
                  className="flex-1 border border-gray-300 rounded px-2 py-1 text-xs"
                >
                  {availableFields.map(f => (
                    <option key={f.key} value={f.key}>{f.label}</option>
                  ))}
                </select>
                <select
                  value={sortBy.order}
                  onChange={(e) => setSortBy({ ...sortBy, order: e.target.value })}
                  className="border border-gray-300 rounded px-2 py-1 text-xs"
                >
                  <option value="asc">Asc</option>
                  <option value="desc">Desc</option>
                </select>
              </div>
            </div>

            <div>
              <label className="text-xs font-semibold text-gray-700 block mb-1">Group By</label>
              <select
                value={groupBy || ""}
                onChange={(e) => setGroupBy(e.target.value || null)}
                className="w-full border border-gray-300 rounded px-2 py-1 text-xs"
              >
                <option value="">None</option>
                {availableFields.map(f => (
                  <option key={f.key} value={f.key}>{f.label}</option>
                ))}
              </select>
            </div>
          </div>

          {/* AGGREGATIONS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-bold text-gray-900">Aggregations</h3>
              <button
                onClick={addAggregation}
                className="flex items-center gap-1 px-2 py-1 bg-amber-100 text-amber-900 rounded text-xs font-semibold hover:bg-amber-200"
              >
                <Plus size={12} />
                Add
              </button>
            </div>
            <div className="space-y-2">
              {aggregations.map(agg => (
                <div key={agg.id} className="bg-blue-50 border border-blue-200 rounded p-2">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-semibold">{agg.label}</span>
                    <span className="text-sm font-bold text-blue-600">
                      {aggregatedValues[agg.id]?.toLocaleString() || 0}
                    </span>
                  </div>
                  <select
                    value={agg.function}
                    onChange={(e) => {
                      const newAggs = aggregations.map(a =>
                        a.id === agg.id ? { ...a, function: e.target.value } : a
                      );
                      setAggregations(newAggs);
                    }}
                    className="w-full border border-gray-300 rounded px-2 py-1 text-xs mb-1"
                  >
                    <option value="sum">Sum</option>
                    <option value="avg">Average</option>
                    <option value="min">Minimum</option>
                    <option value="max">Maximum</option>
                    <option value="count">Count</option>
                  </select>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* PREVIEW */}
      {showPreview && (
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">
            Preview ({Array.isArray(reportData) ? reportData.length : Object.keys(reportData).length} records)
          </h3>
          <div className="overflow-auto max-h-96 border rounded">
            <table className="w-full text-xs">
              <thead className="bg-amber-50 sticky top-0">
                <tr>
                  {selectedFields.map(fieldKey => {
                    const field = availableFields.find(f => f.key === fieldKey);
                    return (
                      <th key={fieldKey} className="p-2 text-left font-semibold border-b">
                        {field?.label || fieldKey}
                      </th>
                    );
                  })}
                </tr>
              </thead>
              <tbody>
                {(Array.isArray(reportData) ? reportData : Object.values(reportData).flat())
                  .slice(0, 50)
                  .map((row, idx) => (
                    <tr key={idx} className="border-b hover:bg-amber-50/30">
                      {selectedFields.map(fieldKey => (
                        <td key={fieldKey} className="p-2">
                          {fieldKey === "createdAt"
                            ? dayjs(row[fieldKey]).format("DD MMM YY HH:mm")
                            : fieldKey.includes("Weight")
                            ? (row[fieldKey] ? parseFloat(row[fieldKey]).toLocaleString() : "-")
                            : row[fieldKey] || "-"}
                        </td>
                      ))}
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* SAVED REPORTS */}
      {savedReports.length > 0 && (
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">Saved Reports</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
            {savedReports.map(report => (
              <button
                key={report.id}
                onClick={() => loadReport(report)}
                className="text-left p-3 border border-gray-200 rounded hover:bg-amber-50 transition-colors"
              >
                <div className="font-semibold text-sm text-gray-900">{report.name}</div>
                <div className="text-xs text-gray-600 mt-1">
                  {dayjs(report.createdAt).format("DD MMM YYYY HH:mm")}
                </div>
                <div className="text-xs text-amber-600 mt-1">
                  {report.fields.length} fields • {report.filters.length} filters
                </div>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}