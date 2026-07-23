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
 *                       Pinned to gold for every dark scheme (Navy, Indigo)
 *                       rather than reusing `primary` — an icon the same hue
 *                       as its own background (or a white accent on the white
 *                       input fields it's also used on) would be invisible.
 *                       Amber is the one scheme where `iconAccent` still
 *                       equals `primary` (gold-on-gold would be its own
 *                       separate collision — flagged as a follow-up, not yet
 *                       resolved).
 *   onAccent          — text/icon color for content sitting on a solid
 *                       primary-colored background (e.g. a filled button)
 *   sidebarBg/Text    — the sidebar's own background + text color
 *   appBarBg/Text     — page header banners / info bars (e.g. the dashboard
 *                       header, the weighbridge info bar) — for Navy and
 *                       Indigo this intentionally matches sidebarBg/Text so
 *                       the whole chrome reads as one consistent dark frame;
 *                       Amber keeps its distinct vivid-orange banner instead.
 *   highlightRgb      — the "r,g,b" base color used for the sidebar's
 *                       translucent hover/active nav-item background. Every
 *                       scheme's accent now shares a hue family with its own
 *                       surface color (navy accent on navy surface, indigo on
 *                       indigo, amber on near-black), so this can just match
 *                       `primary` everywhere — a same-hue tinted overlay reads
 *                       clean. (Historical note: when Navy still used the
 *                       amber accent on a navy surface, those were
 *                       near-complementary hues and a translucent overlay of
 *                       one over the other looked muddy/brown — this field
 *                       was added to let Navy use a neutral white overlay
 *                       instead. Kept as an explicit per-scheme field in case
 *                       a future scheme needs the same escape hatch.)
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
  amber: {
    name: "Amber",
    description: "Warm golden tones — the signature look",
    primary: "#f59e0b",
    secondary: "#d97706",
    iconAccent: "#f59e0b",
    onAccent: "#ffffff",
    sidebarBg: "#111111",
    sidebarText: "#ffffff",
    appBarBg: "#f59e0b",
    appBarText: "#1f2937",
    highlightRgb: "245,158,11",
    preview: ["#f59e0b", "#fbbf24", "#fef3c7"],
    label: "text-amber-700",
  },
  indigo: {
    // Like Navy: primary/buttons/sidebar/app-bar all stay one consistent
    // indigo, and iconAccent is pinned to gold (not `primary`) since an
    // indigo icon on an indigo background/white-input would have almost no
    // contrast — gold is the app's consistent "icons pop against a dark
    // scheme" color, matching Navy's sidebar identity.
    name: "Indigo",
    description: "Bold indigo accent — modern and professional",
    primary: "#6366f1",
    secondary: "#4f46e5",
    iconAccent: "#f59e0b",
    onAccent: "#ffffff",
    sidebarBg: "#1e1b4b",
    sidebarText: "#ffffff",
    appBarBg: "#1e1b4b",
    appBarText: "#ffffff",
    highlightRgb: "99,102,241",
    preview: ["#6366f1", "#818cf8", "#e0e7ff"],
    label: "text-indigo-700",
  },
};
