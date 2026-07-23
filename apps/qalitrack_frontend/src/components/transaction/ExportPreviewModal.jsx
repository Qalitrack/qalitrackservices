import React, { useState, useEffect } from "react";
import { Modal, Button, message } from "antd";
import { Printer } from "lucide-react";
import { generateThemedPDF } from "../../utils/generateTransactionPDF";
import { TICKET_THEMES } from "../../utils/ticketThemeConfig";

export default function ExportPreviewModal({
  open,
  onClose,
  record,
  pdfTheme,
  onPdfThemeChange,
  ticketSettings,
  reweighRecords,
  formatTurnaroundTime,
}) {
  const [previewBlobUrl, setPreviewBlobUrl] = useState(null);
  const [previewLoading, setPreviewLoading] = useState(false);

  const selectedThemeMeta = TICKET_THEMES[pdfTheme] || TICKET_THEMES.modern;
  const selectedThemeColor = selectedThemeMeta.preview.header;

  const generatePreview = async (theme) => {
    setPreviewLoading(true);
    try {
      const url = await generateThemedPDF(
        { ...record, isReweighed: reweighRecords.length > 0, reweighCount: reweighRecords.length },
        { ...ticketSettings, ticketTheme: theme },
        formatTurnaroundTime,
        true
      );
      setPreviewBlobUrl(url);
    } catch (err) {
      message.error("Preview failed: " + err.message);
    } finally {
      setPreviewLoading(false);
    }
  };

  useEffect(() => {
    if (open && record) generatePreview(pdfTheme);
  }, [open]);

  const handleClose = () => {
    setPreviewBlobUrl(null);
    onClose();
  };

  const handleThemeChange = async (theme) => {
    onPdfThemeChange(theme);
    await generatePreview(theme);
  };

  return (
    <Modal
      open={open}
      onCancel={handleClose}
      width={980}
      title={
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center">
            <Printer size={13} className="text-white" />
          </div>
          <span className="text-sm font-bold text-gray-900">
            Ticket Preview — {record?.receiptNo}
          </span>
        </div>
      }
      footer={null}
      destroyOnClose
    >
      <div className="flex gap-4" style={{ height: 680 }}>
        {/* Left: PDF preview */}
        <div className="flex-1 bg-gray-100 rounded-lg overflow-hidden border border-gray-200 flex items-center justify-center">
          {previewLoading || !previewBlobUrl ? (
            <div className="text-gray-400 text-sm font-medium">Generating preview…</div>
          ) : (
            <iframe
              src={previewBlobUrl}
              title="Ticket Preview"
              className="w-full h-full rounded-lg"
              style={{ border: "none" }}
            />
          )}
        </div>

        {/* Right: Export options */}
        <div className="w-52 shrink-0 flex flex-col gap-3">
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
            <div className="text-[10px] font-bold text-gray-600 uppercase mb-2">Print Style</div>
            <div className="flex gap-1">
              <button
                onClick={() => handleThemeChange("modern")}
                className={`flex-1 py-2 text-[10px] font-bold rounded-l border transition-all ${
                  pdfTheme === "modern"
                    ? "bg-amber-500 text-white border-amber-500 shadow-sm"
                    : "bg-white text-gray-500 border-gray-300 hover:border-amber-400 hover:text-amber-600"
                }`}
              >
                Modern
              </button>
              <button
                onClick={() => handleThemeChange("classic")}
                className={`flex-1 py-2 text-[10px] font-bold rounded-r border-t border-b border-r transition-all ${
                  pdfTheme === "classic"
                    ? "bg-gray-800 text-white border-gray-800 shadow-sm"
                    : "bg-white text-gray-500 border-gray-300 hover:border-gray-500 hover:text-gray-700"
                }`}
              >
                B&amp;W
              </button>
            </div>
          </div>

          <Button
            type="primary"
            block
            icon={<Printer size={14} />}
            onClick={() => {
              generateThemedPDF(
                { ...record, isReweighed: reweighRecords.length > 0, reweighCount: reweighRecords.length },
                { ...ticketSettings, ticketTheme: pdfTheme },
                formatTurnaroundTime
              );
              handleClose();
            }}
            style={{
              background:
                pdfTheme === "modern"
                  ? "linear-gradient(135deg, var(--cs-500), var(--cs-600))"
                  : "linear-gradient(135deg, #374151, #111827)",
              border: "none",
              fontWeight: 700,
            }}
          >
            Download PDF
          </Button>

          <Button block onClick={handleClose}>
            Cancel
          </Button>

          <div className="text-[10px] text-gray-400 text-center mt-auto pt-2 border-t border-gray-100">
            <div className="font-medium">{record?.receiptNo}</div>
            <div>{record?.noPlate}</div>
            <div className="mt-1" style={{ color: selectedThemeColor }}>
              {selectedThemeMeta.name} theme
            </div>
          </div>
        </div>
      </div>
    </Modal>
  );
}
