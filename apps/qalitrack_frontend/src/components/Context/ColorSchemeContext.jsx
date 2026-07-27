/**
 * ColorSchemeContext.jsx
 *
 * Manages the global primary color scheme: midnight (Navy, default) | amber | indigo.
 * On change, writes `data-color-scheme` to <html> and persists to localStorage.
 * amber-* Tailwind classes resolve through CSS variables (tailwind.config.cjs +
 * index.css), so every amber-* utility follows the active scheme automatically.
 *
 * The actual per-scheme colors (primary/secondary/sidebar/app-bar/text) live
 * in one place, src/theme/palettes.js — re-exported here as COLOR_SCHEMES for
 * existing consumers.
 */

import { createContext, useContext, useState, useEffect } from "react";
import { PALETTES } from "../../theme/palettes";

export const COLOR_SCHEMES = PALETTES;

const ColorSchemeContext = createContext(null);

function applyScheme(scheme) {
  document.documentElement.dataset.colorScheme = scheme;
}

export function ColorSchemeProvider({ children }) {
  const [colorScheme, setColorScheme] = useState(() => {
    return localStorage.getItem("color-scheme") || "midnight";
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
