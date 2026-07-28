// Chart bars need a vivid, readable-on-white color — a scheme's own preview
// swatches are tuned for subtle UI accents (dots, borders), not solid fills,
// so bump saturation/lightness from the scheme's hue instead of using them
// directly. Saturation is clamped on both ends — floored so muted hues (Navy,
// Indigo) still pop, capped so already-vivid hues (Emerald's green reads
// neon at high saturation + high lightness) don't blow out; lightness is
// kept a bit lower than a "medium" 50% for the same reason.
export function vibrantAccent(hex) {
  const [h, s] = hexToHsl(hex);
  const sat = Math.min(Math.max(s, 60), 75);
  return hslToHex(h, sat, 42);
}

function hexToHsl(hex) {
  const c = hex.replace("#", "");
  const r = parseInt(c.substring(0, 2), 16) / 255;
  const g = parseInt(c.substring(2, 4), 16) / 255;
  const b = parseInt(c.substring(4, 6), 16) / 255;
  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  const l = (max + min) / 2;
  let h = 0, s = 0;
  if (max !== min) {
    const d = max - min;
    s = d / (1 - Math.abs(2 * l - 1));
    if (max === r) h = ((g - b) / d) % 6;
    else if (max === g) h = (b - r) / d + 2;
    else h = (r - g) / d + 4;
    h *= 60;
    if (h < 0) h += 360;
  }
  return [h, s * 100, l * 100];
}

function hslToHex(h, s, l) {
  s /= 100; l /= 100;
  const k = (n) => (n + h / 30) % 12;
  const a = s * Math.min(l, 1 - l);
  const f = (n) => l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
  const toHex = (x) => Math.round(x * 255).toString(16).padStart(2, "0");
  return `#${toHex(f(0))}${toHex(f(8))}${toHex(f(4))}`;
}
