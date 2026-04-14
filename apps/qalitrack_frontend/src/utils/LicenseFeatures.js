/**
 * LicenseFeatures.js — canonical feature flag constants for QaliTrack
 *
 * These values MUST match exactly what the Lante ERP embeds in the JWT
 * features[] claim and what LicenseFeatures.cs defines on the backend.
 *
 * Usage in code — always use the constant, never a bare string:
 *
 *   import { LicenseFeatures } from "./LicenseFeatures";
 *
 *   <FeatureLicenseGate feature={LicenseFeatures.ANPR}>
 *     <AnprSettings />
 *   </FeatureLicenseGate>
 *
 * Adding a new feature:
 *   1. Add the constant here.
 *   2. Add the matching entry in LicenseFeatures.cs (backend).
 *   3. Wrap your module with <FeatureLicenseGate feature={LicenseFeatures.YOUR_FEATURE}>.
 *   4. Issue a license that includes the new feature value.
 */

// ── Hardware peripherals ──────────────────────────────────────────────────────

/** ANPR / NPR camera — automatic number plate recognition on entry/exit. */
export const ANPR           = "anpr";

/** Thermal ticket printer — auto-print weigh tickets at booth or kiosk. */
export const TICKET_PRINTER = "ticket_printer";

/** RFID reader — vehicle identification by UHF tag. */
export const RFID           = "rfid";

/** NFC reader — driver identification by NFC card tap. */
export const NFC            = "nfc";

// ── Software modules ──────────────────────────────────────────────────────────

/** Self-service weighbridge kiosk terminal. */
export const KIOSK          = "kiosk";

/** Dual lane / second independent weighbridge. */
export const DUAL_LANE      = "dual_lane";

/** Advanced report builder beyond standard weigh reports. */
export const REPORTS        = "reports";

/** Analytics dashboard — charts, trends, KPIs. */
export const ANALYTICS      = "analytics";

/** Boom barrier / gate controller — auto-open on weigh completion (future). */
export const BOOM_BARRIER   = "boom_barrier";

/** SMS gateway — automated alerts to drivers and managers (future). */
export const SMS_ALERTS     = "sms_alerts";

/** User management — create, edit, deactivate operator and admin accounts. */
export const USER_MANAGEMENT = "user_management";

/** Shifts — define, assign, and track operator work shifts. */
export const SHIFTS          = "shifts";

/** Backup & microservice management — scheduled and manual database backups. */
export const BACKUP         = "backup";

// ── Catalogue — used for validation ──────────────────────────────────────────

export const ALL = [
  // Hardware
  { value: ANPR,           label: "ANPR / NPR Camera",               group: "Hardware" },
  { value: TICKET_PRINTER, label: "Ticket Printer",                   group: "Hardware" },
  { value: RFID,           label: "RFID Reader",                      group: "Hardware" },
  { value: NFC,            label: "NFC Reader",                       group: "Hardware" },
  // Modules
  { value: KIOSK,          label: "Unmanned Kiosk",                   group: "Modules"  },
  { value: DUAL_LANE,      label: "Dual Lane / Second Scale",         group: "Modules"  },
  { value: REPORTS,        label: "Advanced Reports",                 group: "Modules"  },
  { value: ANALYTICS,      label: "Analytics Dashboard",              group: "Modules"  },
  { value: USER_MANAGEMENT, label: "User Management",                 group: "Modules"  },
  { value: SHIFTS,          label: "Shifts",                          group: "Modules"  },
  { value: BOOM_BARRIER,   label: "Boom Barrier Controller",          group: "Modules"  },
  { value: SMS_ALERTS,     label: "SMS Alerts",                       group: "Modules"  },
  { value: BACKUP,         label: "Backup & Microservice Management", group: "Modules"  },
];

const VALID = new Set(ALL.map(f => f.value));

/** Returns true if the string is a recognised feature value. */
export function isValid(value) {
  return VALID.has(value);
}

// Named export object for import { LicenseFeatures } style
export const LicenseFeatures = {
  ANPR, TICKET_PRINTER, RFID, NFC,
  KIOSK, DUAL_LANE, REPORTS, ANALYTICS,
  USER_MANAGEMENT, SHIFTS, BOOM_BARRIER, SMS_ALERTS, BACKUP,
  ALL, isValid,
};

export default LicenseFeatures;
