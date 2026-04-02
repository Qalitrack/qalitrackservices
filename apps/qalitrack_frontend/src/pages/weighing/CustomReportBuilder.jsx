import { useState, useMemo } from "react";
import {
  Plus, X, Save, Eye, Download, Settings,
  GripVertical, ChevronDown, ChevronUp
} from "lucide-react";
import { message } from "antd";
import dayjs from "dayjs";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import * as XLSX from "xlsx";
import logoSrc from "../../assets/logo.jpeg";
import { getTicketSettings } from "../../utils/ticketThemeConfig";

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
    message.success("Report saved successfully!");
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
  const exportPDF = async () => {
    const settings    = getTicketSettings();
    const companyName = settings.companyName    || "QALIBRATED SYSTEMS LTD";
    const companyAddr = settings.companyAddress || "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996";

    const dataArray = Array.isArray(reportData) ? reportData : Object.values(reportData).flat();

    const doc = new jsPDF("landscape", "mm", "a4");
    const PW  = doc.internal.pageSize.getWidth();
    const L   = 14;
    const R   = PW - 14;
    const TW  = R - L;

    const black      = [0,   0,   0];
    const amber      = [245, 158, 11];
    const amberDark  = [217, 119,  6];
    const amberLight = [254, 243, 199];
    const gray       = [107, 114, 128];
    const borderCol  = [229, 231, 235];
    const green      = [21,  128, 61];

    // Circular logo
    let circularLogo = null;
    try {
      const img = await new Promise((resolve, reject) => {
        const i = new Image();
        i.onload = () => resolve(i);
        i.onerror = reject;
        i.src = logoSrc;
      });
      const sz = Math.min(img.naturalWidth, img.naturalHeight);
      const cv = document.createElement("canvas");
      cv.width = sz; cv.height = sz;
      const ctx = cv.getContext("2d");
      ctx.beginPath();
      ctx.arc(sz / 2, sz / 2, sz / 2, 0, Math.PI * 2);
      ctx.clip();
      ctx.drawImage(img, 0, 0, sz, sz);
      circularLogo = cv.toDataURL("image/png");
    } catch (_) { /* logo unavailable */ }

    // Header
    if (circularLogo) doc.addImage(circularLogo, "PNG", L, 5, 17, 17);

    doc.setFontSize(14);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...black);
    doc.text(companyName, PW / 2, 11, { align: "center" });

    doc.setFontSize(7.5);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(companyAddr, PW / 2, 16, { align: "center" });

    // Report badge
    const badgeW = 52;
    doc.setFillColor(...amber);
    doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, "F");
    doc.setFontSize(8);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...black);
    doc.text(reportName.toUpperCase(), R - badgeW / 2, 9.5, { align: "center" });

    doc.setFontSize(7);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(`Generated: ${dayjs().format("DD MMM YYYY HH:mm")}`, R, 16, { align: "right" });

    // Amber divider
    doc.setDrawColor(...amber);
    doc.setLineWidth(0.8);
    doc.line(L, 23, R, 23);

    // Summary stats
    let y = 27;
    const statW = (TW - 8) / 3;
    const netTotal = dataArray.reduce((s, r) => s + (parseFloat(r.netWeight) || 0), 0);
    const stats = [
      { label: "TOTAL RECORDS",    value: `${dataArray.length}` },
      { label: "TOTAL NET WEIGHT", value: `${netTotal.toLocaleString()} kg` },
      { label: "REPORT DATE",      value: dayjs().format("DD MMM YYYY") },
    ];

    stats.forEach((s, i) => {
      const bx = L + i * (statW + 4);
      doc.setFillColor(...amberLight);
      doc.setDrawColor(...amberDark);
      doc.setLineWidth(0.3);
      doc.roundedRect(bx, y, statW, 10, 2, 2, "FD");
      doc.setFontSize(6.5);
      doc.setFont("helvetica", "normal");
      doc.setTextColor(...gray);
      doc.text(s.label, bx + statW / 2, y + 3.8, { align: "center" });
      doc.setFontSize(9);
      doc.setFont("helvetica", "bold");
      doc.setTextColor(...black);
      doc.text(s.value, bx + statW / 2, y + 8.2, { align: "center" });
    });

    y += 14;

    // Data table
    const headers = selectedFields.map(fieldKey => {
      const field = availableFields.find(f => f.key === fieldKey);
      return field ? field.label : fieldKey;
    });

    const body = dataArray.map((row, idx) =>
      selectedFields.map(fieldKey => {
        const value = row[fieldKey];
        if (fieldKey === "createdAt") return dayjs(value).format("DD MMM YY HH:mm");
        if (fieldKey.includes("Weight")) return value ? parseFloat(value).toLocaleString() : "-";
        return value || "-";
      })
    );

    const statusColIdx = selectedFields.indexOf("status");

    autoTable(doc, {
      startY: y,
      margin: { left: L, right: L },
      head: [headers],
      body,
      styles: {
        fontSize: 6.5,
        cellPadding: 1.5,
        textColor: black,
        lineColor: borderCol,
      },
      headStyles: {
        fillColor: amber,
        textColor: black,
        fontStyle: "bold",
        fontSize: 7,
        halign: "center",
        lineColor: amberDark,
      },
      alternateRowStyles: { fillColor: [252, 252, 252] },
      didParseCell: (data) => {
        if (statusColIdx >= 0 && data.column.index === statusColIdx && data.section === "body") {
          const raw = String(data.cell.raw || "").toLowerCase();
          if (raw === "completed") {
            data.cell.styles.textColor = green;
            data.cell.styles.fontStyle = "bold";
          } else if (raw === "in progress") {
            data.cell.styles.textColor = amberDark;
            data.cell.styles.fontStyle = "bold";
          }
        }
      },
    });

    // Footer
    const footerY = doc.lastAutoTable.finalY + 4;
    doc.setFillColor(...amberLight);
    doc.setDrawColor(...amberDark);
    doc.setLineWidth(0.3);
    doc.roundedRect(L, footerY, TW, 10, 2, 2, "FD");

    if (circularLogo) doc.addImage(circularLogo, "PNG", L + 2, footerY + 1, 8, 8);

    doc.setFontSize(7.5);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...black);
    doc.text("Powered by Qalibrated Systems  |  www.qalibrated.co.ke", PW / 2, footerY + 5, { align: "center" });
    doc.setFontSize(6.5);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text("Inventing and Making Happen", PW / 2, footerY + 8.5, { align: "center" });

    doc.save(`${reportName.replace(/\s+/g, "-")}-${dayjs().format("YYYY-MM-DD")}.pdf`);
  };

  const exportExcel = () => {
    const dataArray = Array.isArray(reportData) ? reportData : Object.values(reportData).flat();

    const ws = XLSX.utils.json_to_sheet(
      dataArray.map((row, idx) => {
        const obj = { "#": idx + 1 };
        selectedFields.forEach(fieldKey => {
          const field = availableFields.find(f => f.key === fieldKey);
          const label = field ? field.label : fieldKey;
          if (fieldKey === "createdAt") {
            obj[label] = row[fieldKey] ? dayjs(row[fieldKey]).format("DD MMM YYYY HH:mm:ss") : "-";
          } else if (fieldKey.includes("Weight")) {
            obj[label] = row[fieldKey] ? parseFloat(row[fieldKey]) : 0;
          } else {
            obj[label] = row[fieldKey] || "-";
          }
        });
        return obj;
      })
    );

    // Column widths
    const colCount = selectedFields.length + 1;
    ws["!cols"] = Array.from({ length: colCount }, (_, i) => ({ wch: i === 0 ? 5 : 18 }));

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, reportName.substring(0, 31));
    XLSX.writeFile(wb, `${reportName.replace(/\s+/g, "-")}-${dayjs().format("YYYY-MM-DD")}.xlsx`);
  };

  return (
    <div className="h-full overflow-y-auto px-1">
    <div className="space-y-4 pb-8">
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
    </div>
  );
}