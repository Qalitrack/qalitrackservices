import React, { useState, useEffect } from "react";
import { Drawer, Button, Input, Tag } from "antd";
import { EditOutlined, SaveOutlined, CloseOutlined } from "@ant-design/icons";
import {
  Printer, ClipboardList, Building2, MapPin, Scale, FileText,
  RotateCcw, Clock, CheckCircle2, Hourglass, MessageSquare,
} from "lucide-react";
import dayjs from "dayjs";
import { TICKET_THEMES } from "../../utils/ticketThemeConfig";

export default function TransactionDrawer({
  open,
  onClose,
  record,
  isCompleted,
  pdfTheme,
  onPdfThemeChange,
  reweighRecords,
  formatTurnaroundTime,
  onSave,
  onExportPDF,
}) {
  const [isEditing, setIsEditing] = useState(false);
  const [editedRecord, setEditedRecord] = useState(record);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setEditedRecord(record);
    setIsEditing(false);
  }, [record]);

  const selectedThemeMeta = TICKET_THEMES[pdfTheme] || TICKET_THEMES.modern;
  const selectedThemeColor = selectedThemeMeta.preview.header;

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSave(editedRecord);
      setIsEditing(false);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setEditedRecord(record);
    setIsEditing(false);
  };

  return (
    <Drawer
      title={
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
            <ClipboardList className="w-3.5 h-3.5 text-white" strokeWidth={2.5} />
          </div>
          <span className="text-sm font-bold text-gray-900">
            Ticket: {record?.receiptNo}
          </span>
          <span
            className="ml-auto text-[9px] font-bold px-2 py-0.5 rounded-full border"
            style={{
              color: selectedThemeColor,
              borderColor: selectedThemeColor,
              backgroundColor: `${selectedThemeColor}18`,
            }}
          >
            {selectedThemeMeta.name}
          </span>
        </div>
      }
      placement="right"
      onClose={onClose}
      open={open}
      width={450}
      footer={
        <div className="flex gap-2 justify-between items-center">
          <div className="flex items-center gap-1">
            <span className="text-[9px] font-semibold text-gray-500 mr-1">PDF:</span>
            <button
              onClick={() => onPdfThemeChange("modern")}
              className={`h-6 px-2 text-[9px] font-bold rounded-l border transition-all ${
                pdfTheme === "modern"
                  ? "bg-amber-500 text-white border-amber-500"
                  : "bg-white text-gray-500 border-gray-300 hover:border-amber-400 hover:text-amber-600"
              }`}
            >
              Modern
            </button>
            <button
              onClick={() => onPdfThemeChange("classic")}
              className={`h-6 px-2 text-[9px] font-bold rounded-r border-t border-b border-r transition-all ${
                pdfTheme === "classic"
                  ? "bg-gray-800 text-white border-gray-800"
                  : "bg-white text-gray-500 border-gray-300 hover:border-gray-500 hover:text-gray-700"
              }`}
            >
              B&W
            </button>
          </div>
          <div className="flex gap-2">
            {!isEditing ? (
              <>
                {!isCompleted && (
                  <Button
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => setIsEditing(true)}
                    className="text-xs border-amber-300 text-amber-600 hover:border-amber-500"
                  >
                    Edit
                  </Button>
                )}
                <Button
                  size="small"
                  type="primary"
                  icon={<Printer size={14} />}
                  onClick={onExportPDF}
                  className="text-xs border-0"
                  style={{
                    background:
                      pdfTheme === "modern"
                        ? "linear-gradient(135deg, var(--cs-500), var(--cs-600))"
                        : "linear-gradient(135deg, #374151, #111827)",
                  }}
                >
                  Export PDF
                </Button>
              </>
            ) : (
              <>
                <Button
                  size="small"
                  icon={<CloseOutlined />}
                  onClick={handleCancel}
                  className="text-xs"
                >
                  Cancel
                </Button>
                <Button
                  size="small"
                  type="primary"
                  icon={<SaveOutlined />}
                  loading={saving}
                  onClick={handleSave}
                  className="text-xs bg-gradient-to-r from-amber-500 to-amber-600 border-0"
                >
                  Save
                </Button>
              </>
            )}
          </div>
        </div>
      }
    >
      {record && (
        <div className="space-y-3 text-xs">
          {/* Basic Info */}
          <div className="bg-gradient-to-br from-amber-50 via-amber-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
                <ClipboardList className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-amber-900">BASIC INFORMATION</span>
            </div>
            <div className="grid grid-cols-2 gap-2.5">
              {[
                { label: "Receipt",   field: "receiptNo",    editable: true },
                { label: "Vehicle",   field: "noPlate",      editable: true },
                { label: "Driver",    field: "driverName",   editable: true },
                { label: "Commodity", field: "commodityName",editable: true },
                { label: "Weigh Mode",field: "weighMode",    editable: true },
                { label: "Container", field: "containerNo",  editable: true },
                { label: "Seal No",   field: "sealNo",       editable: true },
              ].map(({ label, field, editable }) => (
                <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                  {isEditing && editable ? (
                    <Input
                      value={editedRecord[field]}
                      onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                      size="small"
                      className="text-xs border-amber-300 focus:border-amber-500"
                    />
                  ) : (
                    <div className="font-bold text-gray-900 text-xs">{record[field] || "N/A"}</div>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Parties */}
          <div className="bg-gradient-to-br from-amber-50 via-amber-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
                <Building2 className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-amber-900">PARTIES</span>
            </div>
            <div className="grid grid-cols-2 gap-2.5">
              {[
                { label: "Supplier",    field: "supplierName",   editable: true },
                { label: "Customer",    field: "customerName",   editable: true },
                { label: "Transporter", field: "transporterName",editable: true },
                { label: "Operator",    field: "operatorName",   editable: false },
              ].map(({ label, field, editable }) => (
                <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                  {isEditing && editable ? (
                    <Input
                      value={editedRecord[field]}
                      onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                      size="small"
                      className="text-xs border-amber-300"
                    />
                  ) : (
                    <div className="font-bold text-gray-900 text-xs">{record[field] || "N/A"}</div>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Locations */}
          <div className="bg-gradient-to-br from-amber-50 via-amber-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
                <MapPin className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-amber-900">LOCATIONS</span>
            </div>
            <div className="grid grid-cols-2 gap-2.5">
              {[
                { label: "Origin",      field: "originName",     editable: true },
                { label: "Destination", field: "destinationName",editable: true },
                { label: "Weighbridge", field: "weighBridgeName",editable: false },
                { label: "Operation",   field: "operation",      editable: false },
              ].map(({ label, field, editable }) => (
                <div key={field} className="bg-white/70 backdrop-blur rounded px-2.5 py-2 border border-amber-200">
                  <div className="text-amber-700 text-[10px] mb-1 font-bold uppercase tracking-wide">{label}</div>
                  {isEditing && editable ? (
                    <Input
                      value={editedRecord[field]}
                      onChange={(e) => setEditedRecord({ ...editedRecord, [field]: e.target.value })}
                      size="small"
                      className="text-xs border-amber-300"
                    />
                  ) : (
                    <div className="font-bold text-gray-900 text-xs">{record[field] || "N/A"}</div>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Weight Summary */}
          <div className="bg-gradient-to-br from-amber-50 via-amber-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
                <Scale className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-amber-900">WEIGHT SUMMARY</span>
            </div>
            <div className="grid grid-cols-3 gap-2.5 mb-3">
              <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">1st Weight</div>
                <div className="text-base font-extrabold text-amber-600">{record.firstWeight || 0}</div>
                <div className="text-[9px] text-amber-600 font-semibold">KILOGRAMS</div>
                <div className="text-[9px] text-gray-500 mt-1">
                  {record.firstWeightDate ? dayjs(record.firstWeightDate).format("DD-MM-YY HH:mm") : "N/A"}
                </div>
              </div>
              <div className="bg-white rounded-lg p-2.5 border-2 border-amber-200 shadow-sm">
                <div className="text-[10px] text-amber-700 font-bold uppercase mb-1">2nd Weight</div>
                <div className="text-base font-extrabold text-green-600">{record.secondWeight || 0}</div>
                <div className="text-[9px] text-amber-600 font-semibold">KILOGRAMS</div>
                <div className="text-[9px] text-gray-500 mt-1">
                  {record.secondWeightDate ? dayjs(record.secondWeightDate).format("DD-MM-YY HH:mm") : "N/A"}
                </div>
              </div>
              <div className="bg-gradient-to-br from-amber-200 via-amber-300 to-amber-300 rounded-lg p-2.5 border-2 border-amber-500 shadow-lg">
                <div className="text-[10px] text-amber-900 font-extrabold uppercase mb-1">Net Weight</div>
                <div className="text-lg font-black text-amber-950">{record.netWeight || 0}</div>
                <div className="text-[9px] text-amber-800 font-bold">KILOGRAMS</div>
              </div>
            </div>
            {reweighRecords.length > 0 && (
              <div className="flex items-center gap-2 mb-2.5 px-3 py-2 bg-violet-50 border-2 border-violet-300 rounded-lg shadow-sm">
                <RotateCcw className="w-4 h-4 text-violet-600" strokeWidth={2.5} />
                <span className="text-[11px] font-black text-violet-800 uppercase tracking-wide">REWEIGHED</span>
                {reweighRecords.length > 1 && (
                  <span className="text-[9px] font-bold text-violet-600 bg-violet-100 border border-violet-300 px-1.5 py-0.5 rounded-full">
                    ×{reweighRecords.length}
                  </span>
                )}
                <span className="ml-auto text-[9px] text-violet-500 font-semibold">
                  {reweighRecords.length} reweigh record{reweighRecords.length !== 1 ? "s" : ""}
                </span>
              </div>
            )}
            <div className="bg-white rounded-lg px-3 py-2.5 border-2 border-amber-300 flex items-center justify-between">
              <span className="text-[11px] text-amber-800 font-bold flex items-center gap-1.5">
                <Clock className="w-3.5 h-3.5" strokeWidth={2.5} /> TURNAROUND TIME:
              </span>
              <span className="text-sm font-black text-amber-900 bg-amber-100 px-3 py-1 rounded-full">
                {formatTurnaroundTime(record.firstWeightDate, record.secondWeightDate, record.turnaroundTime)}
              </span>
            </div>
          </div>

          {/* Status & Remarks */}
          <div className="bg-gradient-to-br from-amber-50 via-amber-50 to-amber-100 rounded-lg p-3 border-2 border-amber-300 shadow-md">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-full bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
                <FileText className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-sm font-bold text-amber-900">STATUS & REMARKS</span>
            </div>
            <div className="mb-3">
              <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">Status</div>
              <div className="flex items-center gap-2 flex-wrap">
                <Tag
                  color={
                    (record.secondWeight && parseFloat(record.secondWeight) > 0) ||
                    record.status === "Completed"
                      ? "success"
                      : "warning"
                  }
                  className="text-xs font-bold px-3 py-1 shadow-sm m-0 inline-flex items-center gap-1"
                >
                  {(record.secondWeight && parseFloat(record.secondWeight) > 0) ||
                  record.status === "Completed" ? (
                    <><CheckCircle2 className="w-3.5 h-3.5" strokeWidth={2.5} /> COMPLETED</>
                  ) : (
                    <><Hourglass className="w-3.5 h-3.5" strokeWidth={2.5} /> IN PROGRESS</>
                  )}
                </Tag>
                {reweighRecords.length > 0 && (
                  <Tag
                    color="purple"
                    className="text-xs font-bold px-3 py-1 shadow-sm m-0 uppercase tracking-wide inline-flex items-center gap-1"
                  >
                    <RotateCcw className="w-3.5 h-3.5" strokeWidth={2.5} /> REWEIGHED ×{reweighRecords.length}
                  </Tag>
                )}
              </div>
            </div>
            <div>
              <div className="text-amber-700 text-[10px] mb-1.5 font-bold uppercase">Remarks / Notes</div>
              {isEditing ? (
                <Input.TextArea
                  value={editedRecord.remarks || editedRecord.notes}
                  onChange={(e) => setEditedRecord({ ...editedRecord, remarks: e.target.value })}
                  rows={3}
                  className="text-xs border-amber-300 focus:border-amber-500"
                />
              ) : (
                <div className="text-gray-900 bg-white p-2.5 rounded-lg border-2 border-amber-200 text-xs font-medium">
                  {record.remarks || record.notes || (
                    <span className="inline-flex items-center gap-1.5 text-gray-400">
                      <MessageSquare className="w-3.5 h-3.5" strokeWidth={2.5} /> No remarks available
                    </span>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </Drawer>
  );
}
