/**
 * ColorSchemeContext.jsx
 *
 * Manages the global primary color scheme: amber | green | blue.
 * On change, writes `data-color-scheme` to <html> and persists to localStorage.
 * CSS overrides in index.css then remap Tailwind amber-* classes to the active palette.
 */

import { createContext, useContext, useState, useEffect } from "react";

export const COLOR_SCHEMES = {
  amber: {
    name: "Amber",
    description: "Warm golden tones",
    primary:  "#f59e0b",
    secondary: "#d97706",
    preview:  ["#f59e0b", "#fbbf24", "#fef3c7"],
    label:    "text-amber-700",
  },
  green: {
    name: "Green",
    description: "Natural & fresh",
    primary:  "#22c55e",
    secondary: "#16a34a",
    preview:  ["#22c55e", "#4ade80", "#dcfce7"],
    label:    "text-green-700",
  },
  blue: {
    name: "Blue",
    description: "Professional & calm",
    primary:  "#3b82f6",
    secondary: "#2563eb",
    preview:  ["#3b82f6", "#60a5fa", "#dbeafe"],
    label:    "text-blue-700",
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
