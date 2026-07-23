/**
 * SystemSettings.jsx — Centralized Configuration Management
 *
 * TABS:
 *   1. General          — Company info, timezone, currency, language
 *                         + LIVE sidebar logo & theme (via SidebarSettingsContext)
 *   2. Tickets          — PDF theme, colors, font size, logo/QR toggle
 *   3. Hardware         — RFID, NFC, ANPR, Scale, Printer
 *   4. Kiosk            — Self-service mode, auto-advance, timeouts
 *   5. Users & Auth     — Password policy, session timeout, 2FA
 *   6. API & Integration — Webhooks, external systems
 *   7. Backup & Logs    — Database backup, audit logs, retention
 *
 * Ticket theme changes are broadcast LIVE to Transactions.jsx via
 * window CustomEvent — no page refresh required.
 *
 * Sidebar logo + theme changes are broadcast LIVE via SidebarSettingsContext —
 * pure in-memory mutation, no save needed.
 *
 * Save  → writes to localStorage (+ tries API as bonus)
 * Reset → restores from localStorage, falls back to API defaults
 */

import React, { useState, useEffect, useCallback } from "react";
import {
  message,
  Tabs,
  Switch,
  Input,
  Select,
  Button,
  InputNumber,
  Upload,
} from "antd";
import {
  Settings,
  Building2,
  Wifi,
  CreditCard,
  Scale,
  Printer,
  Monitor,
  Users,
  Lock,
  KeyRound,
  ShieldCheck,
  Webhook,
  Database,
  FileText,
  Save,
  RotateCcw,
  CheckCircle2,
  Palette,
} from "lucide-react";
import dayjs from "dayjs";

// ── Shared ticket theme utility ──────────────────────────────────────────────
import {
  saveTicketSettings,
  TICKET_THEMES,
} from "../../utils/ticketThemeConfig";

// ── Sidebar live-mutation context ────────────────────────────────────────────
import {
  useSidebarSettings,
} from "../../components/Context/Sidebarsettingscontext";

// ── Global color scheme context ───────────────────────────────────────────────
import {
  useColorScheme,
  COLOR_SCHEMES,
} from "../../components/Context/ColorSchemeContext";

// ── License utilities ─────────────────────────────────────────────────────────
import FeatureLicenseGate from "../../components/FeatureLicenseGate";
import { LicenseFeatures } from "../../utils/LicenseFeatures";
import { getLicenseStatus, deactivateLicense } from "../../utils/licenseUtils";

// ── Weighbridges ──────────────────────────────────────────────────────────────
import WeighbridgesPortal from "./WeighingBridge";
import { getWeighbridges } from "../../api/MasterData/WeighingBridge";

const { Option } = Select;


// ── Keys that should be synced live to Transactions PDF ──────────────────────
const TICKET_KEYS = new Set([
  "ticketTheme",
  "ticketPrimaryColor",
  "ticketSecondaryColor",
  "ticketAccentColor",
  "ticketShowLogo",
  "ticketShowQRCode",
  "ticketFontSize",
  "companyName",
  "companyAddress",
  "companyPhone",
  "companyEmail",
]);

const DEFAULT_SETTINGS = {
  // General / Branding
  companyName: "QALIBRATED SYSTEMS LTD",
  companyLogo: null,
  companyAddress: "PO BOX 34463-00100, NAIROBI",
  companyPhone: "+254 714 999 996",
  companyEmail: "info@qalibrated.co.ke",
  timezone: "Africa/Nairobi",
  currency: "KES",
  language: "en",
  dateFormat: "DD/MM/YYYY",
  timeFormat: "24h",

  // Ticket / PDF theme
  ticketTheme: "amber",
  ticketPrimaryColor: "#f59e0b",
  ticketSecondaryColor: "#f97316",
  ticketAccentColor: "#d97706",
  ticketShowLogo: true,
  ticketShowQRCode: true,
  ticketFontSize: "normal",

  // Hardware — RFID
  rfidEnabled: true,
  rfidStreamUrl: import.meta.env.VITE_RFID_STREAM_URL,
  rfidReaderType: "UHF Reader",

  // Hardware — NFC
  nfcEnabled: true,
  nfcStreamUrl: import.meta.env.VITE_NFC_STREAM_URL,
  nfcReaderType: "MIFARE Classic",

  // Hardware — ANPR
  anprEnabled: false,
  anprStreamUrl: import.meta.env.VITE_ANPR_STREAM_URL,
  anprCameraUrl: import.meta.env.VITE_ANPR_STREAM_URL,
  anprApiUrl:    import.meta.env.VITE_ANPR_SNAPSHOT_URL,
  anprConfidenceThreshold: 85,
  anprCameraPosition: "entry",
  anprFallbackToManual: true,

  // Weighbridge
  weighbridgeName: "",
  selectedScaleName: "",
  manualWeighingEnabled: false,

  // Hardware — Scale
  scaleEnabled: true,
  scaleStreamUrl: import.meta.env.VITE_SCALE_STREAM_URL,
  scaleBrand: "Avery Weigh-Tronix",
  scaleCapacity: 60000,
  scaleStabilityThreshold: 5,

  // Hardware — Printer
  printerEnabled: true,
  printerModel: "Zebra ZD420",
  printerIp: "192.168.1.100",

  // Kiosk
  kioskMode: true,
  kioskAutoAdvance: true,
  kioskVehicleTimeout: 120,
  kioskDriverTimeout: 60,
  kioskWeighingTimeout: 300,
  kioskResetTimeout: 10,
  kioskDefaultWeighbridge: "Factory A",
  kioskWeighMode: "Gross/Tare",
  kioskShowDebug: false,

  // Users & Auth
  passwordMinLength: 8,
  passwordRequireUppercase: true,
  passwordRequireNumbers: true,
  passwordRequireSymbols: false,
  passwordExpiryDays: 90,
  sessionTimeout: 30,
  maxLoginAttempts: 5,
  lockoutDuration: 15,
  twoFactorEnabled: false,

  // API & Integration
  webhookEnabled: false,
  webhookUrl: "",
  webhookEvents: ["transaction.created", "vehicle.detected", "driver.authenticated"],
  apiRateLimit: 100,
  apiLogging: true,

  auditLogEnabled: true,
  auditLogRetentionDays: 365,
  errorLogEnabled: true,
  errorLogRetentionDays: 90,
};

// ═════════════════════════════════════════════════════════════════════════════
// MAIN COMPONENT
// ═════════════════════════════════════════════════════════════════════════════
export default function SystemSettings() {
  let isDark = false;
  try {
    const { useTheme } = require("../../components/Context/ThemeContext");
    const theme = useTheme();
    isDark = theme?.isDark ?? false;
  } catch (_) {}

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [activeTab, setActiveTab] = useState("general");
  const [lastSaved, setLastSaved] = useState(null);

  const [settings, setSettings] = useState(DEFAULT_SETTINGS);

  // ── Pull updateSidebarSettings here so Reset can restore sidebar state ─────
  const { updateSidebarSettings } = useSidebarSettings();

  // ── On mount: prefer API, fall back to localStorage ───────────────────────
  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = () => {
    setLoading(true);
    const saved = localStorage.getItem("systemSettings");
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        // Wipe stale hardware URLs from the old wrong IP
        if (parsed.rfidStreamUrl?.includes("172.16.0.134")) delete parsed.rfidStreamUrl;
        if (parsed.nfcStreamUrl?.includes("172.16.0.134"))  delete parsed.nfcStreamUrl;
        if (parsed.scaleStreamUrl?.includes("172.16.0.134")) delete parsed.scaleStreamUrl;
        const merged = { ...DEFAULT_SETTINGS, ...parsed };
        setSettings(merged);
        saveTicketSettings(merged);
      } catch (_) {
        // corrupt — fall through to defaults
      }
    }
    setLoading(false);
  };

  // ── Save: always writes to localStorage, tries API as a bonus ────────────
  const handleSave = () => {
    setSaving(true);
    try {
      saveTicketSettings(settings);
      localStorage.setItem("systemSettings", JSON.stringify(settings));
      window.dispatchEvent(new CustomEvent("systemSettingsChanged", { detail: settings }));

      setLastSaved(new Date());
      message.success("Settings saved successfully!");
    } catch (error) {
      message.error("Failed to save: " + error.message);
    } finally {
      setSaving(false);
    }
  };

  // ── Reset: restore from localStorage, fall back to API ───────────────────
  const handleReset = () => {
    const saved = localStorage.getItem("systemSettings");
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        const merged = { ...DEFAULT_SETTINGS, ...parsed };
        setSettings(merged);
        saveTicketSettings(merged);

        // Re-sync sidebar context to match saved state
        updateSidebarSettings({
          companyLogo: merged.companyLogo ?? null,
          companyName: merged.companyName,
        });

        message.info("Settings reset to last saved values");
        return;
      } catch (_) {
        // Corrupt data — fall through to API reload
      }
    }
    loadSettings();
    message.info("Settings reset to last saved values");
  };

  /**
   * setField — updates local state AND immediately broadcasts ticket-related
   * changes to Transactions.jsx via saveTicketSettings.
   */
  const setField = (key, value) => {
    setSettings((prev) => {
      const next = { ...prev, [key]: value };
      if (TICKET_KEYS.has(key)) {
        saveTicketSettings(next);
      }
      return next;
    });
  };

  const currentThemeMeta = TICKET_THEMES[settings.ticketTheme] || TICKET_THEMES.modern;

  // ── Tab items ─────────────────────────────────────────────────────────────
  const tabs = [
    {
      key: "general",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Building2 className="w-3.5 h-3.5" /> General
        </span>
      ),
      children: <GeneralTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "tickets",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Palette className="w-3.5 h-3.5" /> Tickets &amp; Printing
          <span
            className="w-2 h-2 rounded-full ml-0.5"
            style={{ backgroundColor: currentThemeMeta.preview.header }}
            title={`Active theme: ${currentThemeMeta.name}`}
          />
        </span>
      ),
      children: <TicketsTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "kiosk",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Monitor className="w-3.5 h-3.5" /> Unmanned
          <Lock className="w-3 h-3 text-amber-500" />
        </span>
      ),
      children: (
        <FeatureLicenseGate feature={LicenseFeatures.KIOSK}>
          <KioskTab settings={settings} setField={setField} isDark={isDark} />
        </FeatureLicenseGate>
      ),
    },
    {
      key: "license",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <KeyRound className="w-3.5 h-3.5" /> License
        </span>
      ),
      children: <LicenseTab />,
    },
    {
      key: "weighbridges",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Scale className="w-3.5 h-3.5" /> Weighbridges
        </span>
      ),
      children: <WeighbridgesPortal />,
    },
  ];

  return (
    <div
      className={`h-full flex flex-col rounded-lg shadow-md border overflow-hidden ${
        isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"
      }`}
    >
      {/* Header */}
      <div
        className={`px-6 py-4 border-b shrink-0 ${
          isDark
            ? "bg-gray-800 border-gray-700"
            : "bg-gradient-to-r from-amber-50 to-amber-50 border-amber-200"
        }`}
      >
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-lg">
              <Settings className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className={`text-xl font-black ${isDark ? "text-white" : "text-gray-900"}`}>
                System Settings
              </h1>
              <p className={`text-xs ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                Configure hardware, tickets, kiosk, security, and integrations
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <Button
              onClick={handleReset}
              icon={<RotateCcw className="w-4 h-4" />}
              disabled={loading || saving}
              className={`${isDark ? "border-gray-700 text-gray-300" : ""}`}
            >
              Reset
            </Button>
            <Button
              type="primary"
              onClick={handleSave}
              loading={saving}
              disabled={loading}
              icon={<Save className="w-4 h-4" />}
              className="bg-gradient-to-r from-amber-500 to-amber-600 border-0 shadow-md"
            >
              Save Settings
            </Button>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex-1 overflow-auto">
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabs}
          className={`px-4 ${isDark ? "dark-tabs" : ""}`}
          size="small"
        />
      </div>

      {/* Footer */}
      <div
        className={`px-6 py-2.5 border-t shrink-0 ${
          isDark ? "bg-gray-800 border-gray-700" : "bg-gray-50 border-gray-200"
        }`}
      >
        <p className={`text-xs ${isDark ? "text-gray-400" : "text-gray-500"}`}>
          <CheckCircle2 className="w-3 h-3 inline mr-1 text-green-500" />
          {lastSaved
            ? `Last saved: ${dayjs(lastSaved).format("DD MMM YYYY HH:mm")}`
            : "Not yet saved this session"}{" "}
          · System version: 2.1.0
        </p>
      </div>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: GENERAL
// ═════════════════════════════════════════════════════════════════════════════
function GeneralTab({ settings, setField, isDark }) {
  const { updateSidebarSettings } = useSidebarSettings();
  const { colorScheme, setColorScheme } = useColorScheme();

  // ── Logo: update SystemSettings state + sidebar context instantly ──────────
  const handleLogoUpload = (file) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target.result;
      setField("companyLogo", base64);
      updateSidebarSettings({ companyLogo: base64 });
    };
    reader.readAsDataURL(file);
    return false;
  };

  const handleLogoRemove = () => {
    setField("companyLogo", null);
    updateSidebarSettings({ companyLogo: null });
  };

  // ── Company name — also pushes to sidebar ────────────────────────────────
  const handleCompanyNameChange = (value) => {
    setField("companyName", value);
    updateSidebarSettings({ companyName: value });
  };

  return (
    <div className="space-y-5 pb-6 pt-2">

      {/* ── Branding ──────────────────────────────────────────────────────── */}
      <Section title="Branding" icon={<Building2 className="w-4 h-4 text-amber-600" />}>
        <div className="space-y-4">

          {/* Logo upload */}
          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-2">
              Company Logo
            </label>
            <div className="flex items-center gap-4">
              {settings.companyLogo ? (
                <img
                  src={settings.companyLogo}
                  alt="Logo"
                  className="w-16 h-16 object-contain border-2 border-amber-200 rounded-lg p-1 bg-white"
                />
              ) : (
                <div className="w-16 h-16 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center bg-gray-50">
                  <Building2 className="w-6 h-6 text-gray-300" />
                </div>
              )}
              <div className="flex gap-2">
                <Upload beforeUpload={handleLogoUpload} showUploadList={false} accept="image/*">
                  <Button size="small" icon={<Building2 className="w-3.5 h-3.5" />}>
                    {settings.companyLogo ? "Change Logo" : "Upload Logo"}
                  </Button>
                </Upload>
                {settings.companyLogo && (
                  <Button size="small" danger onClick={handleLogoRemove}>
                    Remove
                  </Button>
                )}
              </div>
            </div>
            <p className="text-[10px] text-gray-400 mt-1.5">
              ✨ Logo updates in the sidebar instantly — no save needed.
            </p>
          </div>

          {/* Company fields */}
          <div className="grid grid-cols-2 gap-4">
            <Field label="Company Name">
              <Input
                value={settings.companyName}
                onChange={(e) => handleCompanyNameChange(e.target.value)}
                placeholder="Company name"
              />
            </Field>
            <Field label="Email">
              <Input
                value={settings.companyEmail}
                onChange={(e) => setField("companyEmail", e.target.value)}
                placeholder="info@company.co.ke"
              />
            </Field>
            <Field label="Phone">
              <Input
                value={settings.companyPhone}
                onChange={(e) => setField("companyPhone", e.target.value)}
                placeholder="+254 7XX XXX XXX"
              />
            </Field>
            <Field label="Address">
              <Input
                value={settings.companyAddress}
                onChange={(e) => setField("companyAddress", e.target.value)}
                placeholder="PO BOX ..."
              />
            </Field>
          </div>
        </div>
      </Section>

      {/* ── App Color Scheme ──────────────────────────────────────────────── */}
      <Section
        title="App Color Scheme"
        icon={<Palette className="w-4 h-4 text-amber-500" />}
      >
        <p className="text-xs text-gray-400 mb-4">
          Changes the primary accent color across the entire application and sidebar — instant, no save needed.
        </p>
        <div className="grid grid-cols-3 gap-3">
          {Object.entries(COLOR_SCHEMES).map(([key, scheme]) => {
            const isActive = colorScheme === key;
            return (
              <button
                key={key}
                onClick={() => setColorScheme(key)}
                className={`
                  relative p-4 rounded-xl border-2 text-left transition-all duration-200 cursor-pointer
                  ${isActive
                    ? "shadow-lg scale-[1.02]"
                    : "border-gray-200 hover:shadow-md hover:scale-[1.01]"
                  }
                `}
                style={{
                  borderColor: isActive ? scheme.primary : undefined,
                  background: isActive ? scheme.preview[2] : "#f9fafb",
                }}
              >
                {isActive && (
                  <div
                    className="absolute top-2 right-2 w-5 h-5 rounded-full flex items-center justify-center shadow"
                    style={{ backgroundColor: scheme.primary }}
                  >
                    <CheckCircle2 className="w-3.5 h-3.5 text-white" />
                  </div>
                )}

                {/* Color swatches preview */}
                <div className="flex gap-1.5 mb-3">
                  {scheme.preview.map((color, i) => (
                    <div
                      key={i}
                      className="rounded-full border border-white/30"
                      style={{
                        width: i === 0 ? 20 : i === 1 ? 16 : 12,
                        height: i === 0 ? 20 : i === 1 ? 16 : 12,
                        backgroundColor: color,
                      }}
                    />
                  ))}
                </div>

                <p className="text-sm font-bold text-gray-800">{scheme.name}</p>
                <p className="text-[10px] text-gray-500 mt-0.5">{scheme.description}</p>
              </button>
            );
          })}
        </div>
      </Section>

      {/* ── Localization ──────────────────────────────────────────────────── */}
      <Section title="Localization" icon={<Settings className="w-4 h-4 text-blue-500" />}>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Timezone">
            <Select value={settings.timezone} onChange={(v) => setField("timezone", v)} style={{ width: "100%" }}>
              <Option value="Africa/Nairobi">Africa/Nairobi (EAT +3)</Option>
              <Option value="UTC">UTC</Option>
              <Option value="Africa/Johannesburg">Africa/Johannesburg (SAST)</Option>
              <Option value="Africa/Lagos">Africa/Lagos (WAT)</Option>
              <Option value="Africa/Cairo">Africa/Cairo (EET)</Option>
            </Select>
          </Field>
          <Field label="Currency">
            <Select value={settings.currency} onChange={(v) => setField("currency", v)} style={{ width: "100%" }}>
              <Option value="KES">KES — Kenyan Shilling</Option>
              <Option value="USD">USD — US Dollar</Option>
              <Option value="EUR">EUR — Euro</Option>
              <Option value="GBP">GBP — British Pound</Option>
              <Option value="UGX">UGX — Ugandan Shilling</Option>
              <Option value="TZS">TZS — Tanzanian Shilling</Option>
            </Select>
          </Field>
          <Field label="Date Format">
            <Select value={settings.dateFormat} onChange={(v) => setField("dateFormat", v)} style={{ width: "100%" }}>
              <Option value="DD/MM/YYYY">DD/MM/YYYY — 26/02/2026</Option>
              <Option value="MM/DD/YYYY">MM/DD/YYYY — 02/26/2026</Option>
              <Option value="YYYY-MM-DD">YYYY-MM-DD — 2026-02-26</Option>
              <Option value="DD-MMM-YYYY">DD-MMM-YYYY — 26-Feb-2026</Option>
            </Select>
          </Field>
          <Field label="Time Format">
            <Select value={settings.timeFormat} onChange={(v) => setField("timeFormat", v)} style={{ width: "100%" }}>
              <Option value="24h">24-hour — 14:30</Option>
              <Option value="12h">12-hour — 2:30 PM</Option>
            </Select>
          </Field>
        </div>
      </Section>

      {/* ── Weighbridge ───────────────────────────────────────────────────── */}
      <WeighbridgeSection settings={settings} setField={setField} />
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// WEIGHBRIDGE SECTION (used inside GeneralTab)
// ═════════════════════════════════════════════════════════════════════════════
function WeighbridgeSection({ settings, setField }) {
  const [weighbridges, setWeighbridges] = useState([]);
  const [wbLoading, setWbLoading] = useState(true);

  useEffect(() => {
    getWeighbridges(1, 100)
      .then((items) => {
        setWeighbridges(items);
        if (items.length === 0) return;

        const savedExists = items.some((wb) => wb.location === settings.weighbridgeName);

        if (!settings.weighbridgeName || !savedExists) {
          // Saved value is empty or stale — reset to first from backend
          setField("weighbridgeName", items[0].location);
          setField("selectedScaleName", items[0].scales?.[0] || "");
        } else {
          // Saved weighbridge still exists — sync the scale if it's stale/empty
          const savedWb = items.find((wb) => wb.location === settings.weighbridgeName);
          const scaleExists = savedWb?.scales?.includes(settings.selectedScaleName);
          if (!settings.selectedScaleName || !scaleExists) {
            setField("selectedScaleName", savedWb?.scales?.[0] || "");
          }
        }
      })
      .catch(() => setWeighbridges([]))
      .finally(() => setWbLoading(false));
  }, []);

  const selectedWb = weighbridges.find((wb) => wb.location === settings.weighbridgeName);
  const scaleOptions = selectedWb?.scales || [];

  const handleWeighbridgeChange = (location) => {
    setField("weighbridgeName", location);
    const wb = weighbridges.find((w) => w.location === location);
    setField("selectedScaleName", wb?.scales[0] || "");
  };

  return (
    <Section title="Weighbridge" icon={<Scale className="w-4 h-4 text-amber-600" />}>
      <p className="text-xs text-gray-400 mb-4">
        Select the active weighbridge and scale. The selection is shown on the New Transaction form.
      </p>
      <div className="space-y-4">
        <Field label="Weighbridge Name">
          <Select
            value={settings.weighbridgeName}
            onChange={handleWeighbridgeChange}
            style={{ width: "100%" }}
            placeholder={wbLoading ? "Loading…" : "Select weighbridge"}
            loading={wbLoading}
            disabled={wbLoading}
          >
            {weighbridges.map((wb) => (
              <Option key={wb.id} value={wb.location}>
                {wb.location}
              </Option>
            ))}
          </Select>
        </Field>

        <Field label="Scale Name">
          <Select
            value={settings.selectedScaleName}
            onChange={(v) => setField("selectedScaleName", v)}
            style={{ width: "100%" }}
            placeholder="Select scale"
            disabled={wbLoading || scaleOptions.length === 0}
          >
            {scaleOptions.map((scale) => (
              <Option key={scale} value={scale}>
                {scale}
              </Option>
            ))}
          </Select>
        </Field>

        {/* Active selection summary */}
        <div className="p-3 bg-amber-50 rounded-lg border border-amber-200">
          <p className="text-[10px] font-bold text-amber-700 uppercase tracking-wide mb-1">
            Currently Active on New Transaction Form
          </p>
          <p className="text-sm font-semibold text-amber-900">
            {settings.weighbridgeName || "—"} &nbsp;·&nbsp;{" "}
            <span className="text-amber-600">{settings.selectedScaleName || "—"}</span>
          </p>
        </div>

        <ToggleRow
          label="Manual Weight Entry"
          description="Allow operators to type weights directly instead of reading from the scale. Disable to enforce scale-only capture."
          checked={!!settings.manualWeighingEnabled}
          onChange={(v) => setField("manualWeighingEnabled", v)}
        />
      </div>
    </Section>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: TICKETS & PRINTING
// ═════════════════════════════════════════════════════════════════════════════
function TicketsTab({ settings, setField, isDark }) {
  const themesArray = Object.entries(TICKET_THEMES).map(([key, val]) => ({
    key,
    ...val,
  }));

  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Export / PDF Theme" icon={<Palette className="w-4 h-4 text-amber-600" />}>
        <p className="text-xs text-gray-400 mb-4">
          Applied to all PDF exports — transaction tickets, reports, and data tables.
        </p>
        <div className="grid grid-cols-2 gap-4 mb-5">
          {themesArray.map((theme) => {
            const isActive = settings.ticketTheme === theme.key;
            return (
              <div
                key={theme.key}
                onClick={() => setField("ticketTheme", theme.key)}
                className={`relative p-4 rounded-xl border-2 cursor-pointer transition-all duration-150 ${
                  isActive ? "shadow-lg" : "border-gray-200 hover:shadow-md bg-white"
                }`}
                style={isActive ? { borderColor: theme.preview.header, backgroundColor: theme.preview.bg } : {}}
              >
                {isActive && (
                  <div
                    className="absolute top-2 right-2 w-6 h-6 rounded-full flex items-center justify-center shadow"
                    style={{ backgroundColor: theme.preview.header }}
                  >
                    <CheckCircle2 className="w-4 h-4 text-white" />
                  </div>
                )}
                {isActive && (
                  <div className="absolute top-2 left-2 flex items-center gap-1 bg-green-100 border border-green-300 text-green-700 text-[9px] font-bold px-1.5 py-0.5 rounded-full">
                    <span className="w-1.5 h-1.5 rounded-full bg-green-500" />
                    ACTIVE
                  </div>
                )}

                <div className="mb-3 mt-1">
                  <h4 className="font-bold text-sm text-gray-900">{theme.name}</h4>
                  <p className="text-xs text-gray-400">{theme.description}</p>
                </div>

                {/* Mini PDF document preview */}
                <div
                  className="rounded-lg p-3 border"
                  style={{ backgroundColor: theme.preview.bg, borderColor: theme.preview.accent }}
                >
                  {/* Header bar */}
                  <div
                    className="h-5 rounded mb-2 flex items-center px-2 gap-1.5"
                    style={{ backgroundColor: theme.preview.header }}
                  >
                    <div className="h-1.5 w-10 rounded opacity-70" style={{ backgroundColor: theme.preview.bg }} />
                    <div className="h-1.5 w-6 rounded opacity-40" style={{ backgroundColor: theme.preview.bg }} />
                  </div>
                  {/* Body rows */}
                  <div className="space-y-1.5">
                    {[["40%","55%"],["30%","65%"],["50%","40%"]].map(([w1, w2], i) => (
                      <div key={i} className="flex gap-2">
                        <div className="h-1 rounded opacity-30" style={{ width: w1, backgroundColor: theme.preview.text }} />
                        <div className="h-1 rounded opacity-20" style={{ width: w2, backgroundColor: theme.preview.text }} />
                      </div>
                    ))}
                  </div>
                  {/* Footer accent bar */}
                  <div className="mt-2 h-1.5 rounded" style={{ backgroundColor: theme.preview.accent }} />
                </div>

                {/* Color swatches */}
                <div className="flex gap-1.5 mt-2.5">
                  <div className="w-5 h-2.5 rounded-sm" style={{ backgroundColor: theme.preview.header }} title="Header" />
                  <div className="w-5 h-2.5 rounded-sm" style={{ backgroundColor: theme.preview.accent }} title="Accent" />
                  <div className="w-5 h-2.5 rounded-sm border border-gray-200" style={{ backgroundColor: theme.preview.bg }} title="Background" />
                  <div className="w-5 h-2.5 rounded-sm opacity-50" style={{ backgroundColor: theme.preview.text }} title="Text" />
                </div>
              </div>
            );
          })}
        </div>

        <div className="grid grid-cols-2 gap-4 border-t border-gray-100 pt-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-semibold text-gray-800">Show Company Logo</p>
              <p className="text-xs text-gray-400">Display logo at ticket top</p>
            </div>
            <Switch
              checked={settings.ticketShowLogo}
              onChange={(v) => setField("ticketShowLogo", v)}
            />
          </div>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-semibold text-gray-800">Show QR Code</p>
              <p className="text-xs text-gray-400">Verification QR on ticket</p>
            </div>
            <Switch
              checked={settings.ticketShowQRCode}
              onChange={(v) => setField("ticketShowQRCode", v)}
            />
          </div>
        </div>

        <div className="mt-4">
          <Field label="Font Size">
            <Select
              value={settings.ticketFontSize}
              onChange={(v) => setField("ticketFontSize", v)}
              style={{ width: "100%" }}
            >
              <Option value="small">Small (7.5pt — compact receipts)</Option>
              <Option value="normal">Normal (8.5pt — standard)</Option>
              <Option value="large">Large (10pt — easy to read)</Option>
            </Select>
          </Field>
        </div>
      </Section>

      <Section title="Custom Colors (Advanced)" icon={<Palette className="w-4 h-4 text-purple-500" />}>
        <p className="text-xs text-gray-400 mb-4">
          Override the selected theme's header and accent colors. Applied live.
        </p>
        <div className="grid grid-cols-3 gap-4">
          {[
            ["ticketPrimaryColor", "Primary (Header)", "#f59e0b"],
            ["ticketSecondaryColor", "Secondary", "#f97316"],
            ["ticketAccentColor", "Accent (Bands)", "#d97706"],
          ].map(([field, label, placeholder]) => (
            <Field key={field} label={label}>
              <div className="flex items-center gap-2">
                <input
                  type="color"
                  value={settings[field]}
                  onChange={(e) => setField(field, e.target.value)}
                  className="w-10 h-9 rounded border border-gray-300 cursor-pointer p-0.5"
                />
                <Input
                  value={settings[field]}
                  onChange={(e) => setField(field, e.target.value)}
                  placeholder={placeholder}
                  className="font-mono text-xs"
                />
              </div>
            </Field>
          ))}
        </div>
      </Section>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: KIOSK
// ═════════════════════════════════════════════════════════════════════════════
function KioskTab({ settings, setField, isDark }) {
  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Unmanned Mode" icon={<Monitor className="w-4 h-4 text-amber-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Unmanned Mode"
            description="Unmanned weighing without operator login"
            checked={settings.kioskMode}
            onChange={(v) => setField("kioskMode", v)}
          />
          <ToggleRow
            label="Auto-Advance"
            description="Automatically proceed to next stage after detection"
            checked={settings.kioskAutoAdvance}
            onChange={(v) => setField("kioskAutoAdvance", v)}
          />
          <ToggleRow
            label="Show Debug Panel"
            description="Display SSE stream logs (for troubleshooting)"
            checked={settings.kioskShowDebug}
            onChange={(v) => setField("kioskShowDebug", v)}
          />
        </div>
      </Section>

      <Section title="Timeouts" icon={<Settings className="w-4 h-4 text-gray-500" />}>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Vehicle Detection Timeout">
            <InputNumber value={settings.kioskVehicleTimeout} onChange={(v) => setField("kioskVehicleTimeout", v)} min={30} max={300} addonAfter="sec" style={{ width: "100%" }} />
          </Field>
          <Field label="Driver NFC Timeout">
            <InputNumber value={settings.kioskDriverTimeout} onChange={(v) => setField("kioskDriverTimeout", v)} min={15} max={120} addonAfter="sec" style={{ width: "100%" }} />
          </Field>
          <Field label="Weighing Timeout">
            <InputNumber value={settings.kioskWeighingTimeout} onChange={(v) => setField("kioskWeighingTimeout", v)} min={60} max={600} addonAfter="sec" style={{ width: "100%" }} />
          </Field>
          <Field label="Auto-Reset After Complete">
            <InputNumber value={settings.kioskResetTimeout} onChange={(v) => setField("kioskResetTimeout", v)} min={5} max={60} addonAfter="sec" style={{ width: "100%" }} />
          </Field>
        </div>
      </Section>

      <Section title="Defaults">
        <div className="grid grid-cols-2 gap-4">
          <Field label="Default Weighbridge">
            <Input value={settings.kioskDefaultWeighbridge} onChange={(e) => setField("kioskDefaultWeighbridge", e.target.value)} placeholder="Factory A" />
          </Field>
          <Field label="Weigh Mode">
            <Select value={settings.kioskWeighMode} onChange={(v) => setField("kioskWeighMode", v)} style={{ width: "100%" }}>
              <Option value="Gross/Tare">Gross/Tare</Option>
              <Option value="Entry/Exit">Entry/Exit</Option>
            </Select>
          </Field>
        </div>
      </Section>
    </div>
  );
}


// ═════════════════════════════════════════════════════════════════════════════
// TAB: LICENSE
// ═════════════════════════════════════════════════════════════════════════════
function LicenseTab() {
  const [license,    setLicense]   = useState(null);
  const [confirming, setConfirm]   = useState(false);

  useEffect(() => {
    getLicenseStatus("").then(setLicense);
  }, []);

  const handleDeactivate = () => {
    deactivateLicense();
    window.location.reload();
  };

  const features = license?.features ?? [];
  const expiry   = license?.expiresAt ? new Date(license.expiresAt) : null;
  const daysLeft = expiry ? Math.ceil((expiry - Date.now()) / 86400000) : null;

  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Active License" icon={<KeyRound className="w-4 h-4 text-amber-600" />}>
        {!license ? (
          <div className="flex justify-center py-6">
            <div className="w-6 h-6 border-2 border-amber-500 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <InfoRow label="Customer ID"  value={license.customerId  ?? "—"} />
              <InfoRow label="App"          value={license.appId       ?? "—"} />
              <InfoRow label="Issued"       value={license.issuedAt  ? new Date(license.issuedAt).toLocaleDateString()  : "—"} />
              <InfoRow
                label="Expires"
                value={expiry ? expiry.toLocaleDateString() : "—"}
                badge={
                  daysLeft !== null
                    ? daysLeft > 30
                      ? { text: `${daysLeft}d left`, color: "green" }
                      : daysLeft > 7
                      ? { text: `${daysLeft}d left`, color: "orange" }
                      : { text: `${daysLeft}d left`, color: "red" }
                    : null
                }
              />
            </div>

            <div>
              <p className="text-xs font-semibold text-gray-600 mb-2">Licensed Features</p>
              <div className="flex flex-wrap gap-1.5">
                {features.length === 0
                  ? <span className="text-xs text-gray-400">None</span>
                  : features.map(f => (
                      <span key={f} className="px-2 py-0.5 rounded-full bg-amber-100 text-amber-800 text-xs font-medium border border-amber-200">
                        {f}
                      </span>
                    ))
                }
              </div>
            </div>
          </div>
        )}
      </Section>

      <Section title="Update License" icon={<ShieldCheck className="w-4 h-4 text-gray-500" />}>
        <p className="text-sm text-gray-500 mb-4">
          Deactivating clears the stored token. The app will lock and prompt for a new license key on next load.
          Use this when renewing, upgrading, or transferring the license.
        </p>
        {!confirming ? (
          <button
            onClick={() => setConfirm(true)}
            className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm font-semibold hover:bg-red-100 transition"
          >
            <Lock className="w-4 h-4" /> Deactivate &amp; Enter New License
          </button>
        ) : (
          <div className="rounded-lg border border-red-200 bg-red-50 p-4 space-y-3">
            <p className="text-sm font-semibold text-red-800">
              This will lock the app immediately. You will need a valid license token to continue.
            </p>
            <div className="flex gap-2">
              <button
                onClick={handleDeactivate}
                className="px-4 py-2 rounded-lg bg-red-600 text-white text-sm font-bold hover:bg-red-700 transition"
              >
                Yes, deactivate
              </button>
              <button
                onClick={() => setConfirm(false)}
                className="px-4 py-2 rounded-lg bg-white border border-gray-300 text-gray-700 text-sm font-semibold hover:bg-gray-50 transition"
              >
                Cancel
              </button>
            </div>
          </div>
        )}
      </Section>
    </div>
  );
}

function InfoRow({ label, value, badge }) {
  return (
    <div className="rounded-lg bg-gray-50 border border-gray-200 px-3 py-2.5">
      <p className="text-[10px] font-semibold text-gray-400 uppercase tracking-wide mb-0.5">{label}</p>
      <div className="flex items-center gap-2">
        <p className="text-sm font-mono text-gray-800">{value}</p>
        {badge && (
          <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded-full
            ${badge.color === "green"  ? "bg-green-100 text-green-700"  : ""}
            ${badge.color === "orange" ? "bg-amber-100 text-amber-700" : ""}
            ${badge.color === "red"    ? "bg-red-100 text-red-700"      : ""}
          `}>
            {badge.text}
          </span>
        )}
      </div>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// HELPER COMPONENTS
// ═════════════════════════════════════════════════════════════════════════════

function Section({ title, icon, children }) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <div className="flex items-center gap-2 mb-4 pb-3 border-b border-gray-100">
        {icon}
        <h3 className="text-sm font-bold text-gray-900">{title}</h3>
      </div>
      {children}
    </div>
  );
}

function Field({ label, children }) {
  return (
    <div>
      <label className="block text-xs font-semibold text-gray-600 mb-1.5">{label}</label>
      {children}
    </div>
  );
}

function ToggleRow({ label, description, checked, onChange }) {
  return (
    <div className="flex items-center justify-between">
      <div>
        <p className="text-sm font-semibold text-gray-800">{label}</p>
        {description && <p className="text-xs text-gray-400">{description}</p>}
      </div>
      <Switch checked={checked} onChange={onChange} />
    </div>
  );
}

const borderColorMap = {
  green: "border-green-200",
  purple: "border-purple-200",
  blue: "border-blue-200",
  orange: "border-amber-200",
  indigo: "border-indigo-200",
};

function IndentGroup({ color = "gray", children }) {
  return (
    <div className={`ml-4 pl-4 border-l-2 space-y-3 ${borderColorMap[color] || "border-gray-200"}`}>
      {children}
    </div>
  );
}