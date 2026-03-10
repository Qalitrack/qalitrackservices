/**
 * ticketThemeConfig.js
 * Shared utility for ticket PDF theming.
 * Settings are persisted in localStorage under "ticketSettings"
 * so that SystemSettings ↔ Transactions stay in sync.
 */

export const TICKET_THEMES = {
  classic: {
    name: "Classic",
    description: "Traditional black & white receipt style",
    preview: { bg: "#ffffff", header: "#000000", text: "#333333", accent: "#666666" },
    pdf: {
      headerBg: [0, 0, 0],
      headerText: [255, 255, 255],
      bodyText: [33, 33, 33],
      lightBg: [245, 245, 245],
      accentBg: [220, 220, 220],
      border: [180, 180, 180],
      netWeightBg: [240, 240, 240],
      tableGridColor: [180, 180, 180],
    },
  },
  modern: {
    name: "Modern",
    description: "Clean gradient design with amber accents",
    preview: { bg: "#fffbeb", header: "#f59e0b", text: "#78350f", accent: "#fcd34d" },
    pdf: {
      headerBg: [245, 158, 11],      // amber-500
      headerText: [255, 255, 255],
      bodyText: [33, 33, 33],
      lightBg: [255, 251, 235],      // amber-50
      accentBg: [252, 211, 77],      // amber-300
      border: [245, 158, 11],
      netWeightBg: [254, 243, 199],  // amber-100
      tableGridColor: [245, 158, 11],
    },
  },
  minimal: {
    name: "Minimal",
    description: "Minimalist monochrome with subtle borders",
    preview: { bg: "#fafafa", header: "#404040", text: "#262626", accent: "#d4d4d4" },
    pdf: {
      headerBg: [64, 64, 64],
      headerText: [255, 255, 255],
      bodyText: [38, 38, 38],
      lightBg: [250, 250, 250],
      accentBg: [212, 212, 212],
      border: [212, 212, 212],
      netWeightBg: [245, 245, 245],
      tableGridColor: [200, 200, 200],
    },
  },
  colorful: {
    name: "Colorful",
    description: "Vibrant emerald & amber gradient theme",
    preview: { bg: "#fef3c7", header: "#10b981", text: "#065f46", accent: "#34d399" },
    pdf: {
      headerBg: [16, 185, 129],      // emerald-500
      headerText: [255, 255, 255],
      bodyText: [6, 95, 70],         // emerald-900
      lightBg: [254, 243, 199],      // amber-100
      accentBg: [52, 211, 153],      // emerald-400
      border: [16, 185, 129],
      netWeightBg: [209, 250, 229],  // emerald-100
      tableGridColor: [16, 185, 129],
    },
  },
};

const STORAGE_KEY = "ticketSettings";

/** Read current ticket settings from localStorage */
export function getTicketSettings() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch (_) {}
  return {
    ticketTheme: "modern",
    ticketPrimaryColor: "#f59e0b",
    ticketSecondaryColor: "#f97316",
    ticketAccentColor: "#d97706",
    ticketShowLogo: true,
    ticketShowQRCode: true,
    ticketFontSize: "normal",
    companyName: "QALIBRATED SYSTEMS LTD",
    companyAddress: "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996",
  };
}

/** Save ticket settings to localStorage (called from SystemSettings) */
export function saveTicketSettings(settings) {
  const toSave = {
    ticketTheme: settings.ticketTheme,
    ticketPrimaryColor: settings.ticketPrimaryColor,
    ticketSecondaryColor: settings.ticketSecondaryColor,
    ticketAccentColor: settings.ticketAccentColor,
    ticketShowLogo: settings.ticketShowLogo,
    ticketShowQRCode: settings.ticketShowQRCode,
    ticketFontSize: settings.ticketFontSize,
    companyName: settings.companyName,
    companyAddress: settings.companyAddress,
    companyPhone: settings.companyPhone,
    companyEmail: settings.companyEmail,
  };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(toSave));
  // Dispatch a custom event so Transactions.jsx can react in real-time
  window.dispatchEvent(new CustomEvent("ticketSettingsChanged", { detail: toSave }));
}

/** Convert hex color string to [r, g, b] array */
export function hexToRgb(hex) {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result
    ? [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)]
    : [245, 158, 11];
}

/**
 * Resolve the full PDF color palette for a given settings object.
 * Custom colors (if set) override the theme defaults.
 */
export function resolvePdfTheme(settings) {
  const themeKey = settings.ticketTheme || "modern";
  const base = TICKET_THEMES[themeKey]?.pdf || TICKET_THEMES.modern.pdf;

  // If user has set custom primary color that differs from theme default,
  // overlay it on the header
  const customPrimary = settings.ticketPrimaryColor
    ? hexToRgb(settings.ticketPrimaryColor)
    : null;
  const customAccent = settings.ticketAccentColor
    ? hexToRgb(settings.ticketAccentColor)
    : null;

  return {
    ...base,
    headerBg: customPrimary || base.headerBg,
    accentBg: customAccent || base.accentBg,
    border: customPrimary || base.border,
    tableGridColor: customPrimary || base.tableGridColor,
  };
}

/** Font size px values for PDF */
export function resolveFontSize(ticketFontSize) {
  switch (ticketFontSize) {
    case "small": return { title: 12, heading: 8, body: 7.5, sub: 7 };
    case "large": return { title: 16, heading: 10, body: 10, sub: 8.5 };
    default:      return { title: 14, heading: 9, body: 8.5, sub: 8 };
  }
}