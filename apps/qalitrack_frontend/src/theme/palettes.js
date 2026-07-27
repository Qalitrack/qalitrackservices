/**
 * palettes.js — single source of truth for every color scheme's full swatch.
 *
 * Each scheme defines named roles (primary/secondary/accent-on-background,
 * sidebar surface, app-bar/banner surface, and their paired text colors) so
 * no component has to hardcode a hex value or guess what text color reads
 * well on it. ColorSchemeContext and Sidebarsettingscontext both read from
 * here instead of keeping their own separate copies.
 *
 * Roles:
 *   primary/secondary — the brand accent (drives --cs-* / Tailwind amber-*),
 *                       used for buttons, badges, highlighted bars, etc.
 *   iconAccent        — sidebar nav icon + active-item border/chevron color,
 *                       AND page-header icon glyphs, AND the focus ring on
 *                       search/filter inputs (all via shared --cs-icon-accent).
 *                       Pinned to gold for every scheme rather than reusing
 *                       `primary` — an icon the same hue as its own
 *                       background (or a white accent on the white input
 *                       fields it's also used on) would be invisible.
 *   onAccent          — text/icon color for content sitting on a solid
 *                       primary-colored background (e.g. a filled button)
 *   sidebarBg/Text    — the sidebar's own background + text color
 *   appBarBg/Text     — page header banners / info bars (e.g. the dashboard
 *                       header, the weighbridge info bar) — matches
 *                       sidebarBg/Text for all three schemes now, so the
 *                       whole chrome reads as one consistent dark frame.
 *   highlightRgb      — the "r,g,b" base color used for the sidebar's
 *                       translucent hover/active nav-item background. Every
 *                       scheme uses white here rather than its own `primary`
 *                       — since `primary` now equals each scheme's own
 *                       sidebar surface color exactly (one canonical color,
 *                       see appBarBg/Text above), a same-color-on-itself
 *                       overlay would be invisible. (Historical note: this
 *                       field exists because an EARLIER Navy attempt used
 *                       its old gold accent as the overlay on a navy surface
 *                       — a near-complementary hue pairing that read as
 *                       muddy/brown — so a neutral white overlay was used
 *                       instead; that fix generalizes to any scheme whose
 *                       primary equals its own surface color.)
 */
export const PALETTES = {
  // Navy is the app's primary/default theme — listed first.
  midnight: {
    // Every navy-family surface (sidebar, app-bar, buttons) uses the exact
    // same #20293a — one canonical navy, not a separate lighter blue for
    // "accent" purposes. highlightRgb stays white (not navy-on-navy) purely
    // because a same-color translucent overlay on top of an identical
    // background would be invisible — that's a functional hover/active
    // affordance, not a second brand color.
    name: "Navy",
    description: "One deep navy, used consistently everywhere",
    primary: "#20293a",
    secondary: "#1b2331",
    iconAccent: "#f59e0b",
    onAccent: "#ffffff",
    sidebarBg: "#20293a",
    sidebarText: "#ffffff",
    appBarBg: "#20293a",
    appBarText: "#ffffff",
    highlightRgb: "255,255,255",
    preview: ["#20293a", "#1b2331", "#636975"],
    label: "text-slate-700",
  },
  // Replaces the old "Amber" gold theme — same unified-surface pattern as
  // Navy/Indigo (one canonical color for primary/sidebar/app-bar) instead of
  // Amber's old split bright-gold-bar-on-near-black-sidebar look.
  amber: {
    name: "Emerald",
    description: "One rich emerald, used consistently everywhere",
    primary: "#065f46",
    secondary: "#05503b",
    iconAccent: "#f59e0b",
    onAccent: "#ffffff",
    sidebarBg: "#065f46",
    sidebarText: "#ffffff",
    appBarBg: "#065f46",
    appBarText: "#ffffff",
    highlightRgb: "255,255,255",
    preview: ["#065f46", "#05503b", "#518f7e"],
    label: "text-emerald-700",
  },
  indigo: {
    // Like Navy: primary/buttons/sidebar/app-bar all use the exact same
    // #1e1b4b — one canonical dark indigo, not a separate brighter indigo
    // for "accent"/button purposes (that was the bug: buttons used to be
    // #6366f1, a noticeably lighter/brighter purple than the header/sidebar,
    // so a "Save" button looked like a different color from the chrome
    // around it). iconAccent is pinned to gold (not `primary`) since an
    // indigo icon on an indigo background/white-input would have almost no
    // contrast — gold is the app's consistent "icons pop against a dark
    // scheme" color, matching Navy's sidebar identity. highlightRgb is white
    // for the same reason as Navy: primary now equals the sidebar's own
    // surface color, so a same-color overlay would be invisible.
    name: "Indigo",
    description: "Bold indigo accent — modern and professional",
    primary: "#1e1b4b",
    secondary: "#1a1740",
    iconAccent: "#f59e0b",
    onAccent: "#ffffff",
    sidebarBg: "#1e1b4b",
    sidebarText: "#ffffff",
    appBarBg: "#1e1b4b",
    appBarText: "#ffffff",
    highlightRgb: "255,255,255",
    preview: ["#1e1b4b", "#1a1740", "#625f81"],
    label: "text-indigo-700",
  },
};
