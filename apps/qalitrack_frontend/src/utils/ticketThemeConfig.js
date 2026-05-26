/**
 * ticketThemeConfig.js
 * Shared utility for export/PDF theming across the entire app.
 * Settings are persisted in localStorage under "ticketSettings".
 *
 * Themes: monochrome · amber · blue · green
 */

// ── Transaction-ticket PDF themes (used by Transaction.jsx) ──────────────────
export const TICKET_THEMES = {
  monochrome: {
    name: "Black & White",
    description: "Classic monochrome — clean and printer-friendly",
    preview: { bg: "#ffffff", header: "#111111", text: "#333333", accent: "#888888" },
    pdf: {
      headerBg:      [17, 17, 17],
      headerText:    [255, 255, 255],
      bodyText:      [33, 33, 33],
      lightBg:       [245, 245, 245],
      accentBg:      [210, 210, 210],
      border:        [120, 120, 120],
      netWeightBg:   [235, 235, 235],
      tableGridColor:[150, 150, 150],
    },
  },
  amber: {
    name: "Amber",
    description: "Warm golden — the classic QaliTrack look",
    preview: { bg: "#fffbeb", header: "#f59e0b", text: "#78350f", accent: "#fcd34d" },
    pdf: {
      headerBg:      [245, 158, 11],
      headerText:    [0, 0, 0],
      bodyText:      [33, 33, 33],
      lightBg:       [255, 251, 235],
      accentBg:      [252, 211, 77],
      border:        [245, 158, 11],
      netWeightBg:   [254, 243, 199],
      tableGridColor:[245, 158, 11],
    },
  },
  blue: {
    name: "Blue",
    description: "Professional cobalt — clear and corporate",
    preview: { bg: "#eff6ff", header: "#3b82f6", text: "#1e3a8a", accent: "#93c5fd" },
    pdf: {
      headerBg:      [59, 130, 246],
      headerText:    [255, 255, 255],
      bodyText:      [30, 58, 138],
      lightBg:       [239, 246, 255],
      accentBg:      [147, 197, 253],
      border:        [59, 130, 246],
      netWeightBg:   [219, 234, 254],
      tableGridColor:[59, 130, 246],
    },
  },
  green: {
    name: "Green",
    description: "Natural emerald — fresh and vibrant",
    preview: { bg: "#f0fdf4", header: "#22c55e", text: "#14532d", accent: "#86efac" },
    pdf: {
      headerBg:      [34, 197, 94],
      headerText:    [255, 255, 255],
      bodyText:      [20, 83, 45],
      lightBg:       [240, 253, 244],
      accentBg:      [134, 239, 172],
      border:        [34, 197, 94],
      netWeightBg:   [220, 252, 231],
      tableGridColor:[34, 197, 94],
    },
  },
};

// Backward-compat aliases for stored settings written before the rename
const _KEY_ALIASES = { modern: "amber", classic: "monochrome", minimal: "monochrome", colorful: "green" };

const STORAGE_KEY = "ticketSettings";
// companyLogo is stored under its own key to prevent the general settings
// save/load cycle (which doesn't include the logo) from wiping it.
const LOGO_STORAGE_KEY = "companyLogo";

/** Read the stored company logo (base64 data URL) or null */
export function getCompanyLogo() {
  return localStorage.getItem(LOGO_STORAGE_KEY) || null;
}

/** Persist the company logo. Pass null to remove it. */
export function saveCompanyLogo(logo) {
  if (logo) {
    localStorage.setItem(LOGO_STORAGE_KEY, logo);
  } else {
    localStorage.removeItem(LOGO_STORAGE_KEY);
  }
}

/** Read current ticket settings from localStorage */
export function getTicketSettings() {
  let result = {
    ticketTheme: "amber",
    ticketPrimaryColor: "#f59e0b",
    ticketSecondaryColor: "#f97316",
    ticketAccentColor: "#d97706",
    ticketShowLogo: true,
    ticketShowQRCode: true,
    ticketFontSize: "normal",
    companyName: "QALIBRATED SYSTEMS LTD",
    companyAddress: "PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996",
    companyLogo: null,
  };
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) result = { ...result, ...JSON.parse(raw) };
  } catch (_) {}
  // Always read logo from its own key so it is never wiped by a settings save
  result.companyLogo = getCompanyLogo();
  return result;
}

/** Save ticket settings to localStorage (called from SystemSettings) */
export function saveTicketSettings(settings) {
  const toSave = {
    ticketTheme:         settings.ticketTheme,
    ticketPrimaryColor:  settings.ticketPrimaryColor,
    ticketSecondaryColor:settings.ticketSecondaryColor,
    ticketAccentColor:   settings.ticketAccentColor,
    ticketShowLogo:      settings.ticketShowLogo,
    ticketShowQRCode:    settings.ticketShowQRCode,
    ticketFontSize:      settings.ticketFontSize,
    companyName:         settings.companyName,
    companyAddress:      settings.companyAddress,
    companyPhone:        settings.companyPhone,
    companyEmail:        settings.companyEmail,
  };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(toSave));
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
 * Resolve the full PDF color palette for Transaction ticket PDFs.
 * Custom primary/accent colors override the theme defaults when present.
 */
export function resolvePdfTheme(settings) {
  const rawKey  = settings.ticketTheme || "amber";
  const themeKey= _KEY_ALIASES[rawKey] || rawKey;
  return { ...(TICKET_THEMES[themeKey]?.pdf || TICKET_THEMES.amber.pdf) };
}

/**
 * resolveReportColors — returns the accent palette for report/table PDFs.
 * Used by DriverReport, SupplierReport, CommodityReport, CustomerReport,
 * Roles, Permissions, Shifts, Users, Reports, CustomReportBuilder, etc.
 *
 * Returns: { primary, primaryDark, primaryLight, headerText }
 */
export function resolveReportColors(settings) {
  const rawKey  = settings?.ticketTheme || "amber";
  const themeKey= _KEY_ALIASES[rawKey] || rawKey;

  const palettes = {
    monochrome: {
      primary:      [33, 33, 33],
      primaryDark:  [0, 0, 0],
      primaryLight: [245, 245, 245],
      headerText:   [255, 255, 255],
    },
    amber: {
      primary:      [245, 158, 11],
      primaryDark:  [217, 119, 6],
      primaryLight: [254, 243, 199],
      headerText:   [0, 0, 0],
    },
    blue: {
      primary:      [59, 130, 246],
      primaryDark:  [37, 99, 235],
      primaryLight: [219, 234, 254],
      headerText:   [255, 255, 255],
    },
    green: {
      primary:      [34, 197, 94],
      primaryDark:  [22, 163, 74],
      primaryLight: [220, 252, 231],
      headerText:   [255, 255, 255],
    },
  };

  return palettes[themeKey] || palettes.amber;
}

/** Font size px values for PDF */
export function resolveFontSize(ticketFontSize) {
  switch (ticketFontSize) {
    case "small": return { title: 12, heading: 8,  body: 7.5, sub: 7   };
    case "large": return { title: 16, heading: 10, body: 10,  sub: 8.5 };
    default:      return { title: 14, heading: 9,  body: 8.5, sub: 8   };
  }
}
