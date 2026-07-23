/**
 * ColorSchemeContext.jsx
 *
 * Manages the global primary color scheme: amber | midnight | emerald.
 * On change, writes `data-color-scheme` to <html> and persists to localStorage.
 * amber-* Tailwind classes resolve through CSS variables (tailwind.config.cjs +
 * index.css), so every amber-* utility follows the active scheme automatically.
 */

import { createContext, useContext, useState, useEffect } from "react";

export const COLOR_SCHEMES = {
  amber: {
    name: "Amber",
    description: "Warm golden tones — the signature look",
    primary:  "#f59e0b",
    secondary: "#d97706",
    preview:  ["#f59e0b", "#fbbf24", "#fef3c7"],
    label:    "text-amber-700",
  },
  midnight: {
    name: "Midnight",
    description: "Deep navy sidebar, same warm gold accents",
    primary:  "#f59e0b",
    secondary: "#d97706",
    preview:  ["#0f172a", "#f59e0b", "#fef3c7"],
    label:    "text-amber-700",
  },
  emerald: {
    name: "Emerald",
    description: "Refined, professional green accent",
    primary:  "#10b981",
    secondary: "#059669",
    preview:  ["#10b981", "#34d399", "#d1fae5"],
    label:    "text-emerald-700",
  },
};

const ColorSchemeContext = createContext(null);

function applyScheme(scheme) {
  document.documentElement.dataset.colorScheme = scheme;
}

export function ColorSchemeProvider({ children }) {
  const [colorScheme, setColorScheme] = useState(() => {
    return localStorage.getItem("color-scheme") || "amber";
  });

  // Apply immediately on mount and whenever it changes
  useEffect(() => {
    applyScheme(colorScheme);
    localStorage.setItem("color-scheme", colorScheme);
  }, [colorScheme]);

  return (
    <ColorSchemeContext.Provider value={{ colorScheme, setColorScheme, COLOR_SCHEMES }}>
      {children}
    </ColorSchemeContext.Provider>
  );
}

export function useColorScheme() {
  const ctx = useContext(ColorSchemeContext);
  if (!ctx) throw new Error("useColorScheme must be inside ColorSchemeProvider");
  return ctx;
}
