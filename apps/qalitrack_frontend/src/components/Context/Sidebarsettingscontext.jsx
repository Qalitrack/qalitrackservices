/**
 * SidebarSettingsContext.jsx
 *
 * Provides live sidebar settings (logo, theme) across the app.
 * NO API calls — pure in-memory mutation via React Context.
 *
 * Usage:
 *   1. Wrap your app root with <SidebarSettingsProvider>
 *   2. Consume with useSidebarSettings() in any component
 */

import { createContext, useContext, useState, useCallback } from "react";
import { getTicketSettings, getCompanyLogo, saveCompanyLogo } from "../../utils/ticketThemeConfig";
import { useColorScheme } from "./ColorSchemeContext";
import { PALETTES } from "../../theme/palettes";

// ── Sidebar themes — derived from the shared PALETTES (src/theme/palettes.js)
// so bg/text/accent never drift out of sync with the color-scheme swatches.
// `accent` (icon + active-item border/chevron color) uses each scheme's own
// iconAccent, not necessarily `primary` — see the iconAccent doc comment in
// palettes.js (Navy and Indigo both keep gold here even though their primary
// is navy-blue/indigo, since an icon the same hue as its own background
// would be invisible).
// hoverBg/activeBg/border use each scheme's own highlightRgb similarly.
export const SIDEBAR_THEMES = Object.fromEntries(
  Object.entries(PALETTES).map(([key, p]) => {
    const rgb = p.highlightRgb;
    return [key, {
      name: p.name,
      description: p.description,
      bg: p.sidebarBg,
      text: p.sidebarText,
      accent: p.iconAccent,
      hoverBg: `rgba(${rgb},0.08)`,
      activeBg: `rgba(${rgb},0.13)`,
      border: `rgba(${rgb},0.10)`,
    }];
  })
);

// ── Default state ─────────────────────────────────────────────────────────────
const DEFAULT_SIDEBAR_SETTINGS = {
  companyName: "QALIBRATED SYSTEMS LTD",
  companyLogo: null,           // base64 string or null
};

// ── Context ───────────────────────────────────────────────────────────────────
const SidebarSettingsContext = createContext(null);

export function SidebarSettingsProvider({ children }) {
  const { colorScheme } = useColorScheme();

  const [sidebarSettings, setSidebarSettings] = useState(() => {
    try {
      const saved = getTicketSettings();
      return {
        ...DEFAULT_SIDEBAR_SETTINGS,
        ...(saved.companyName && { companyName: saved.companyName }),
        companyLogo: getCompanyLogo(),
      };
    } catch (_) {
      return DEFAULT_SIDEBAR_SETTINGS;
    }
  });

  /**
   * Merge partial updates — call this from SystemSettings GeneralTab
   * e.g. updateSidebarSettings({ companyLogo: base64, companyName: "Acme" })
   * companyLogo is persisted to its own localStorage key so PDF generators can read it.
   */
  const updateSidebarSettings = useCallback((patch) => {
    setSidebarSettings((prev) => {
      const next = { ...prev, ...patch };
      if ("companyLogo" in patch) {
        try {
          saveCompanyLogo(next.companyLogo);
        } catch (_) {}
      }
      return next;
    });
  }, []);

  // Sidebar theme follows the system color scheme automatically
  const resolvedTheme = SIDEBAR_THEMES[colorScheme] ?? SIDEBAR_THEMES.midnight;

  return (
    <SidebarSettingsContext.Provider
      value={{ sidebarSettings, updateSidebarSettings, resolvedTheme, SIDEBAR_THEMES }}
    >
      {children}
    </SidebarSettingsContext.Provider>
  );
}

export function useSidebarSettings() {
  const ctx = useContext(SidebarSettingsContext);
  if (!ctx) throw new Error("useSidebarSettings must be used inside SidebarSettingsProvider");
  return ctx;
}