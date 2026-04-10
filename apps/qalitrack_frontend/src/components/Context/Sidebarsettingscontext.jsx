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

// ── Sidebar themes ────────────────────────────────────────────────────────────
export const SIDEBAR_THEMES = {
  obsidian: {
    name: "Obsidian",
    description: "Classic dark black with amber accents",
    bg: "#000000",
    text: "#ffffff",
    accent: "#f59e0b",       // amber-400
    hoverBg: "rgba(255,255,255,0.05)",
    activeBg: "rgba(255,255,255,0.08)",
    border: "rgba(255,255,255,0.06)",
    preview: "#f59e0b",
  },
  midnight: {
    name: "Midnight Blue",
    description: "Deep navy with cyan highlights",
    bg: "#0f172a",
    text: "#e2e8f0",
    accent: "#38bdf8",       // sky-400
    hoverBg: "rgba(56,189,248,0.08)",
    activeBg: "rgba(56,189,248,0.12)",
    border: "rgba(255,255,255,0.07)",
    preview: "#38bdf8",
  },
  forest: {
    name: "Forest",
    description: "Dark green with emerald accents",
    bg: "#0d1f17",
    text: "#d1fae5",
    accent: "#34d399",       // emerald-400
    hoverBg: "rgba(52,211,153,0.08)",
    activeBg: "rgba(52,211,153,0.13)",
    border: "rgba(52,211,153,0.10)",
    preview: "#34d399",
  },
  charcoal: {
    name: "Charcoal",
    description: "Warm grey with rose gold accents",
    bg: "#1c1917",
    text: "#f5f5f4",
    accent: "#fb923c",       // orange-400
    hoverBg: "rgba(251,146,60,0.08)",
    activeBg: "rgba(251,146,60,0.12)",
    border: "rgba(255,255,255,0.06)",
    preview: "#fb923c",
  },
  slate: {
    name: "Slate",
    description: "Cool grey with violet highlights",
    bg: "#1e1b4b",
    text: "#e0e7ff",
    accent: "#a78bfa",       // violet-400
    hoverBg: "rgba(167,139,250,0.08)",
    activeBg: "rgba(167,139,250,0.13)",
    border: "rgba(167,139,250,0.10)",
    preview: "#a78bfa",
  },
  custom: {
    name: "Custom",
    description: "Your own colors",
    bg: "#111111",
    text: "#ffffff",
    accent: "#f59e0b",
    hoverBg: "rgba(255,255,255,0.05)",
    activeBg: "rgba(255,255,255,0.08)",
    border: "rgba(255,255,255,0.06)",
    preview: "#f59e0b",
  },
};

// ── Default state ─────────────────────────────────────────────────────────────
const DEFAULT_SIDEBAR_SETTINGS = {
  companyName: "QALIBRATED SYSTEMS LTD",
  companyLogo: null,           // base64 string or null
  sidebarTheme: "obsidian",    // key from SIDEBAR_THEMES
  customBg: "#111111",
  customAccent: "#f59e0b",
  customText: "#ffffff",
};

// ── Context ───────────────────────────────────────────────────────────────────
const SidebarSettingsContext = createContext(null);

export function SidebarSettingsProvider({ children }) {
  const [sidebarSettings, setSidebarSettings] = useState(() => {
    // Hydrate companyName and companyLogo from localStorage on first mount
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
   * e.g. updateSidebarSettings({ companyLogo: base64, sidebarTheme: "midnight" })
   * companyLogo is persisted to its own localStorage key so PDF generators can read it.
   */
  const updateSidebarSettings = useCallback((patch) => {
    setSidebarSettings((prev) => {
      const next = { ...prev, ...patch };
      // Persist logo to its own key whenever it changes
      if ("companyLogo" in patch) {
        try {
          saveCompanyLogo(next.companyLogo);
        } catch (_) {}
      }
      return next;
    });
  }, []);

  /**
   * Derive the resolved theme object (merging custom colors if theme === "custom")
   */
  const resolvedTheme = (() => {
    const base = SIDEBAR_THEMES[sidebarSettings.sidebarTheme] || SIDEBAR_THEMES.obsidian;
    if (sidebarSettings.sidebarTheme === "custom") {
      return {
        ...base,
        bg: sidebarSettings.customBg,
        accent: sidebarSettings.customAccent,
        text: sidebarSettings.customText,
      };
    }
    return base;
  })();

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