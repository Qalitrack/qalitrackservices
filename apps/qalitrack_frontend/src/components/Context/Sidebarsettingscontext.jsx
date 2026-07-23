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

// ── Sidebar themes — one per system color scheme ──────────────────────────────
export const SIDEBAR_THEMES = {
  amber: {
    name: "Amber",
    description: "Dark with warm amber accents",
    bg: "#111111",
    text: "#ffffff",
    accent: "#f59e0b",
    hoverBg: "rgba(245,158,11,0.08)",
    activeBg: "rgba(245,158,11,0.13)",
    border: "rgba(245,158,11,0.10)",
  },
  midnight: {
    // Same amber/gold accent as the default theme — only the sidebar's dark
    // base shifts from near-black to navy. A mood variant, not a hue change.
    name: "Midnight",
    description: "Deep navy with warm gold accents",
    bg: "#0f172a",
    text: "#e2e8f0",
    accent: "#f59e0b",
    hoverBg: "rgba(245,158,11,0.08)",
    activeBg: "rgba(245,158,11,0.13)",
    border: "rgba(245,158,11,0.10)",
  },
  emerald: {
    name: "Emerald",
    description: "Dark with refined emerald accents",
    bg: "#0d1f1a",
    text: "#ffffff",
    accent: "#10b981",
    hoverBg: "rgba(16,185,129,0.08)",
    activeBg: "rgba(16,185,129,0.13)",
    border: "rgba(16,185,129,0.10)",
  },
};

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
  const resolvedTheme = SIDEBAR_THEMES[colorScheme] ?? SIDEBAR_THEMES.amber;

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