import dayjs from "dayjs";
import { message } from "antd";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import logoSrc from "../assets/logo.jpeg";
import {
  resolvePdfTheme,
  resolveFontSize,
  resolveReportColors,
  TICKET_THEMES,
} from "./ticketThemeConfig";

export const generateThemedPDF = async (record, ticketSettings, formatTurnaroundTimeSimple, previewOnly = false) => {
  const doc      = new jsPDF({ unit: "mm", format: "a4" });
  const W        = 210;
  const L        = 14;          // left margin
  const R        = 196;         // right edge  (W - 14)
  const TW       = R - L;       // table / section width = 182
  const palette  = resolvePdfTheme(ticketSettings);
  const fontSize = resolveFontSize(ticketSettings.ticketFontSize);

  const companyName    = String(ticketSettings.companyName    || "");
  const companyAddress = String(ticketSettings.companyAddress || "");
  const companyPhone   = String(ticketSettings.companyPhone   || "");
  const companyEmail   = String(ticketSettings.companyEmail   || "");

  const white     = [255, 255, 255];
  const black     = [0,   0,   0];
  const gray      = [107, 114, 128];
  const borderCol = [209, 213, 219];
  const { primary: amberFill, primaryDark: amberBdr } = resolveReportColors(ticketSettings);
  const ticketDate = dayjs(record.firstWeightDate || record.createdAt || new Date()).format("MMM D, YYYY HH:mm");

  // ── Pre-load logo ─────────────────────────────────────────────────────────
  let logoImg = null;
  try {
    logoImg = await new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = ticketSettings.companyLogo || logoSrc;
    });
  } catch (_) { /* logo unavailable – skip */ }

  // ── Logo for header — scaled to fit, never cropped ─────────────────────
  let circularLogo = null;
  if (logoImg) {
    try {
      const sz  = 200;
      const pad = sz * 0.08;
      const cCanvas = document.createElement("canvas");
      cCanvas.width = sz; cCanvas.height = sz;
      const cCtx = cCanvas.getContext("2d");


      // Scale whole logo to fit — no cropping
      const avail   = sz - pad * 2;
      const aspect  = logoImg.naturalWidth / logoImg.naturalHeight;
      const drawW   = aspect >= 1 ? avail : avail * aspect;
      const drawH   = aspect >= 1 ? avail / aspect : avail;
      cCtx.drawImage(logoImg, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH);

      const imgData = cCtx.getImageData(0, 0, sz, sz);
      const px = imgData.data;
      for (let p = 0; p < px.length; p += 4) {
        if (px[p] > 240 && px[p + 1] > 240 && px[p + 2] > 240) px[p + 3] = 0;
      }
      cCtx.putImageData(imgData, 0, 0);

      circularLogo = cCanvas.toDataURL("image/png");
    } catch (_) {}
  }

  // ── HEADER ───────────────────────────────────────────────────────────────
  if (circularLogo) {
    doc.addImage(circularLogo, "PNG", L, 7, 18, 18);
  }

  if (companyName) {
    doc.setFontSize(fontSize.title);
    doc.setFont("times", "bold");
    doc.setTextColor(...palette.bodyText);
    doc.text(companyName, W / 2, 13, { align: "center" });
  }

  if (companyAddress) {
    doc.setFontSize(fontSize.sub - 1);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(companyAddress, W / 2, 19, { align: "center" });
  }

  const contactParts = [companyPhone, companyEmail].filter(Boolean);
  if (contactParts.length > 0) {
    doc.setFontSize(fontSize.sub - 1);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(...gray);
    doc.text(contactParts.join("  |  "), W / 2, 24, { align: "center" });
  }

  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...palette.bodyText);
  doc.text(ticketDate, R, 10, { align: "right" });

  const showReweighed = record.isReweighed || (record.reweighCount > 0);
  if (showReweighed) {
    const rwBadgeW = 32;
    const violet = [124, 58, 237];
    doc.setFillColor(...violet);
    doc.roundedRect(R - rwBadgeW, 21, rwBadgeW, 6, 1.5, 1.5, "F");
    doc.setFontSize(fontSize.sub - 1);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...white);
    const rwLabel = record.reweighCount > 1 ? `REWEIGHED x${record.reweighCount}` : "REWEIGHED";
    doc.text(rwLabel, R - rwBadgeW / 2, 25, { align: "center" });
  }

  const dividerY = showReweighed ? 34 : 28;
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.5);
  doc.line(L, dividerY, R, dividerY);

  const drawTitle = (text, yPos) => {
    doc.setFontSize(fontSize.heading + 1);
    doc.setFont("times", "bold");
    doc.setTextColor(...amberBdr);
    doc.text(text, W / 2, yPos, { align: "center" });
    const tw = doc.getTextWidth(text);
    doc.setDrawColor(...amberBdr);
    doc.setLineWidth(0.5);
    doc.line(W / 2 - tw / 2, yPos + 1.2, W / 2 + tw / 2, yPos + 1.2);
    return yPos + 6;
  };

  let y = drawTitle("WEIGHING TICKET", 35);

  // ── TICKET DETAILS ────────────────────────────────────────────────────────
  const labelTint = [255, 249, 235];
  const detailsY  = y;
  autoTable(doc, {
    startY: detailsY,
    margin: { left: L, right: L },
    theme: "plain",
    styles: {
      fontSize: fontSize.body,
      font: "helvetica",
      cellPadding: { top: 1, right: 1.5, bottom: 1, left: 2 },
      textColor: black,
      lineColor: [225, 210, 180],
      lineWidth: 0.15,
      overflow: "linebreak",
    },
    columnStyles: {
      0: { fontStyle: "bold", cellWidth: 26, fillColor: labelTint },
      1: { cellWidth: 62 },
      2: { fontStyle: "bold", cellWidth: 28, fillColor: labelTint },
      3: { cellWidth: 66 },
    },
    body: [
      ["TICKET NO",   `${record.receiptNo        || "N/A"}`, "REGISTRATION", `${record.noPlate          || "N/A"}`],
      ["TRANSPORTER", `${record.transporterName   || "N/A"}`, "COMMODITY",    `${record.commodityName    || "N/A"}`],
      ["SOURCE",      `${record.originName        || "N/A"}`, "DESTINATION",  `${record.destinationName || "N/A"}`],
      ["DRIVER",      `${record.driverName        || "N/A"}`, "WEIGH MODE",   `${record.weighMode       || "N/A"}`],
      ["SUPPLIER",    `${record.supplierName      || "N/A"}`, "CUSTOMER",     `${record.customerName    || "N/A"}`],
    ],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.4);
  doc.rect(L, detailsY, TW, doc.lastAutoTable.finalY - detailsY, "S");

  // ── WEIGHT MEASUREMENTS ───────────────────────────────────────────────────
  y = drawTitle("WEIGHT MEASUREMENTS", doc.lastAutoTable.finalY + 6);

  const grossDate   = record.firstWeightDate  ? dayjs(record.firstWeightDate).format("DD-MM-YY hh:mm A")  : "—";
  const tareDate    = record.secondWeightDate ? dayjs(record.secondWeightDate).format("DD-MM-YY hh:mm A") : "—";
  const operator1st = record.operatorName    || "—";
  const operator2nd = record.operatorName2nd || "—";
  const scale       = record.scaleName       || "—";
  const bridge      = record.weighBridgeName || "—";
  const tatText     = formatTurnaroundTimeSimple(record.firstWeightDate, record.secondWeightDate, record.turnaroundTime);
  const netHl       = [255, 245, 200];

  const wmY = y;
  autoTable(doc, {
    startY: wmY,
    margin: { left: L, right: L },
    theme: "plain",
    headStyles: {
      fillColor: amberFill,
      textColor: black,
      fontStyle: "bold",
      font: "helvetica",
      fontSize: fontSize.body,
      halign: "center",
      lineColor: amberBdr,
      lineWidth: 0.2,
    },
    styles: {
      fontSize: fontSize.body,
      font: "helvetica",
      textColor: black,
      lineColor: [225, 210, 180],
      lineWidth: 0.15,
      cellPadding: { top: 1, right: 1.5, bottom: 1, left: 2 },
      overflow: "linebreak",
    },
    columnStyles: {
      0: { fontStyle: "bold", cellWidth: 32, fillColor: labelTint },
      1: { cellWidth: 28 },
      2: { cellWidth: 32 },
      3: { cellWidth: 28 },
      4: { cellWidth: 20 },
    },
    head: [["MEASUREMENT", "WEIGHT (kg)", "DATE", "OPERATOR", "SCALE", "WEIGHBRIDGE"]],
    body: [
      ["GROSS WEIGHT", record.firstWeight  ? `${Number(record.firstWeight).toLocaleString("en-US")} kg`  : "—", grossDate, operator1st, scale, bridge],
      ["TARE WEIGHT",  record.secondWeight ? `${Number(record.secondWeight).toLocaleString("en-US")} kg` : "—", tareDate,  operator2nd, scale, bridge],
      [
        { content: "NET WEIGHT", styles: { fontStyle: "bold", fillColor: netHl } },
        { content: record.netWeight ? `${Math.round(Number(record.netWeight)).toLocaleString("en-US")} kg` : "—", styles: { fontStyle: "bold", fillColor: netHl } },
        { content: "" },
        { content: "" },
        { content: "" },
        { content: "" },
      ],
      [
        { content: "TURNAROUND TIME", styles: { fontStyle: "bold", fillColor: labelTint } },
        { content: "", styles: { fillColor: labelTint } },
        { content: tatText, styles: { fontStyle: "bold", halign: "center" } },
        { content: "" },
        { content: "" },
        { content: "" },
      ],
    ],
  });
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.4);
  doc.rect(L, wmY, TW, doc.lastAutoTable.finalY - wmY, "S");

  // ── REMARKS (optional) ───────────────────────────────────────────────────
  y = doc.lastAutoTable.finalY + 6;
  if (record.remarks || record.notes) {
    y += 2;
    doc.setFillColor(...palette.headerBg);
    doc.roundedRect(L, y, TW, 7, 2, 2, "F");
    doc.setFontSize(fontSize.sub);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(...palette.headerText);
    doc.text("REMARKS / NOTES", W / 2, y + 5, { align: "center" });
    y += 10;
    doc.setTextColor(...palette.bodyText);
    doc.setFontSize(fontSize.body);
    doc.setFont("helvetica", "normal");
    const remarkLines = doc.splitTextToSize(record.remarks || record.notes, TW - 4);
    doc.text(remarkLines, L + 2, y);
    y += remarkLines.length * (fontSize.body * 0.35) + 4;
  }

  // ── FOOTER ───────────────────────────────────────────────────────────────
  y += 4;

  doc.setFillColor(248, 249, 250);
  doc.setDrawColor(...borderCol);
  doc.setLineWidth(0.3);
  doc.roundedRect(L, y, TW, 15, 3, 3, "FD");

  if (circularLogo) {
    doc.addImage(circularLogo, "PNG", L + 2, y + 2, 11, 11);
  }

  doc.setFontSize(fontSize.sub);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...palette.bodyText);
  doc.text("Powered by Qalibrated Systems", W / 2, y + 7, { align: "center" });
  doc.setFontSize(fontSize.body - 1);
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...gray);
  doc.text("www.qalibrated.co.ke", W / 2, y + 11, { align: "center" });

  const tagW = 46;
  const tagX = R - tagW - 2;
  doc.setFillColor(...amberFill);
  doc.setDrawColor(...amberBdr);
  doc.setLineWidth(0.3);
  doc.roundedRect(tagX, y + 4, tagW, 7, 2, 2, "FD");
  doc.setFontSize(6.5);
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...black);
  doc.text("Inventing and Making Happen", tagX + tagW / 2, y + 8.5, { align: "center" });

  // ── Watermark ─────────────────────────────────────────────────────────────
  if (logoImg) {
    try {
      const wmSize = 90;
      const PH     = doc.internal.pageSize.getHeight();
      const cSz    = 400;
      const pad    = cSz * 0.08;
      const wmCanvas = document.createElement("canvas");
      wmCanvas.width = cSz; wmCanvas.height = cSz;
      const wmCtx = wmCanvas.getContext("2d");

      // Scale whole logo to fit — no cropping
      wmCtx.globalAlpha = 0.07;
      const avail  = cSz - pad * 2;
      const aspect = logoImg.naturalWidth / logoImg.naturalHeight;
      const drawW  = aspect >= 1 ? avail : avail * aspect;
      const drawH  = aspect >= 1 ? avail / aspect : avail;
      wmCtx.drawImage(logoImg, (cSz - drawW) / 2, (cSz - drawH) / 2, drawW, drawH);

      const wmData = wmCanvas.toDataURL("image/png");
      const totalPages = doc.internal.getNumberOfPages();
      for (let p = 1; p <= totalPages; p++) {
        doc.setPage(p);
        doc.addImage(wmData, "PNG", W / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
      }
    } catch (_) {}
  }

  if (previewOnly) {
    return URL.createObjectURL(doc.output("blob"));
  }

  doc.save(`${record.receiptNo || "Ticket"}.pdf`);
  message.success(`Weighing ticket exported (${TICKET_THEMES[ticketSettings.ticketTheme]?.name || "Default"} theme)!`);
};
