/**
 * SystemSettings.jsx — Centralized Configuration Management
 *
 * TABS:
 *   1. General          — Company info, timezone, currency, language
 *   2. Tickets          — PDF theme, colors, font size, logo/QR toggle
 *   3. Hardware         — RFID, NFC, ANPR, Scale, Printer
 *   4. Kiosk            — Self-service mode, auto-advance, timeouts
 *   5. Users & Auth     — Password policy, session timeout, 2FA
 *   6. API & Integration — Webhooks, external systems
 *   7. Backup & Logs    — Database backup, audit logs, retention
 *
 * Ticket theme changes are broadcast LIVE to Transactions.jsx via
 * window CustomEvent — no page refresh required.
 */

import React, { useState, useEffect } from "react";
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
// Adjust path to match your project structure
import {
  saveTicketSettings,
  TICKET_THEMES,
} from "../../utils/ticketThemeConfig";

const { Option } = Select;

// ── API Helpers ───────────────────────────────────────────────────────────────
import { apiClient } from "../../api/helpers/apiClients";

const getSettings = async () => {
  const response = await apiClient.get("/Settings");
  return response.data;
};

const postSettings = async (settings) => {
  const response = await apiClient.post("/Settings", settings);
  return response.data;
};

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

  const [settings, setSettings] = useState({
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
    ticketTheme: "modern",
    ticketPrimaryColor: "#f59e0b",
    ticketSecondaryColor: "#f97316",
    ticketAccentColor: "#d97706",
    ticketShowLogo: true,
    ticketShowQRCode: true,
    ticketFontSize: "normal",

    // Hardware — RFID
    rfidEnabled: true,
    rfidStreamUrl: "http://172.16.0.134:5000/api/rfid/stream",
    rfidReaderType: "UHF Reader",

    // Hardware — NFC
    nfcEnabled: true,
    nfcStreamUrl: "http://172.16.0.134:5000/api/nfc/stream",
    nfcReaderType: "MIFARE Classic",

    // Hardware — ANPR
    anprEnabled: false,
    anprCameraUrl: "http://192.168.1.50/stream",
    anprApiUrl: "http://192.168.1.50/api/detect",
    anprConfidenceThreshold: 85,
    anprCameraPosition: "entry",
    anprFallbackToManual: true,

    // Hardware — Scale
    scaleEnabled: true,
    scaleStreamUrl: "http://172.16.0.134:5000/api/scale/stream",
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

    // Backup & Logs
    autoBackupEnabled: true,
    backupFrequency: "daily",
    backupTime: "02:00",
    backupRetentionDays: 30,
    auditLogEnabled: true,
    auditLogRetentionDays: 365,
    errorLogEnabled: true,
    errorLogRetentionDays: 90,
  });

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    setLoading(true);
    try {
      const data = await getSettings();
      if (data?.settings) {
        const merged = { ...settings, ...data.settings };
        setSettings(merged);
        // Sync stored ticket settings on load
        saveTicketSettings(merged);
      }
    } catch (error) {
      console.warn("Using default settings:", error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await postSettings(settings);
      // Persist ticket settings to localStorage so Transactions picks them up
      saveTicketSettings(settings);
      setLastSaved(new Date());
      message.success("Settings saved successfully!");
    } catch (error) {
      message.error("Failed to save: " + error.message);
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    loadSettings();
    message.info("Settings reset to last saved values");
  };

  /**
   * setField — updates local state AND immediately broadcasts ticket-related
   * changes to Transactions.jsx via a CustomEvent on window.
   */
  const setField = (key, value) => {
    setSettings((prev) => {
      const next = { ...prev, [key]: value };
      if (TICKET_KEYS.has(key)) {
        // Live sync — no need to hit Save first
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
          {/* Live indicator dot */}
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
      key: "hardware",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Wifi className="w-3.5 h-3.5" /> Hardware
        </span>
      ),
      children: <HardwareTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "kiosk",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Monitor className="w-3.5 h-3.5" /> Kiosk
        </span>
      ),
      children: <KioskTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "users",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Users className="w-3.5 h-3.5" /> Users &amp; Auth
        </span>
      ),
      children: <UsersTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "api",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Webhook className="w-3.5 h-3.5" /> API &amp; Integration
        </span>
      ),
      children: <ApiTab settings={settings} setField={setField} isDark={isDark} />,
    },
    {
      key: "backup",
      label: (
        <span className="flex items-center gap-1.5 text-xs">
          <Database className="w-3.5 h-3.5" /> Backup &amp; Logs
        </span>
      ),
      children: <BackupTab settings={settings} setField={setField} isDark={isDark} />,
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
            : "bg-gradient-to-r from-amber-50 to-orange-50 border-amber-200"
        }`}
      >
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-lg">
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
              disabled={loading}
              className={`${isDark ? "border-gray-700 text-gray-300" : ""}`}
            >
              Reset
            </Button>
            <Button
              type="primary"
              onClick={handleSave}
              loading={saving}
              icon={<Save className="w-4 h-4" />}
              className="bg-gradient-to-r from-amber-500 to-orange-600 border-0 shadow-md"
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
  const handleLogoUpload = (file) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      setField("companyLogo", e.target.result);
      localStorage.setItem("tempCompanyLogo", e.target.result);
    };
    reader.readAsDataURL(file);
    return false;
  };

  return (
    <div className="space-y-5 pb-6 pt-2">
      {/* Branding */}
      <Section title="Branding" icon={<Building2 className="w-4 h-4 text-amber-600" />}>
        <div className="space-y-4">
          {/* Logo */}
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
                    {settings.companyLogo ? "Change" : "Upload Logo"}
                  </Button>
                </Upload>
                {settings.companyLogo && (
                  <Button size="small" danger onClick={() => setField("companyLogo", null)}>
                    Remove
                  </Button>
                )}
              </div>
            </div>
            <p className="text-[10px] text-gray-400 mt-1.5">
              💡 Updates sidebar immediately. Save Settings to persist.
            </p>
          </div>

          {/* Company Name */}
          <div className="grid grid-cols-2 gap-4">
            <Field label="Company Name">
              <Input
                value={settings.companyName}
                onChange={(e) => setField("companyName", e.target.value)}
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

      {/* Localization */}
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
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: TICKETS & PRINTING  (with live sync)
// ═════════════════════════════════════════════════════════════════════════════
function TicketsTab({ settings, setField, isDark }) {
  const themesArray = Object.entries(TICKET_THEMES).map(([key, val]) => ({
    key,
    ...val,
  }));

  return (
    <div className="space-y-5 pb-6 pt-2">
      {/* Theme selector */}
      <Section title="PDF Theme" icon={<Palette className="w-4 h-4 text-amber-600" />}>
        <div className="grid grid-cols-2 gap-4 mb-5">
          {themesArray.map((theme) => {
            const isActive = settings.ticketTheme === theme.key;
            return (
              <div
                key={theme.key}
                onClick={() => setField("ticketTheme", theme.key)}
                className={`
                  relative p-4 rounded-xl border-2 cursor-pointer transition-all duration-150
                  ${isActive
                    ? "border-amber-500 bg-amber-50 shadow-lg"
                    : "border-gray-200 hover:border-amber-300 hover:shadow-md bg-white"
                  }
                `}
              >
                {/* Active checkmark */}
                {isActive && (
                  <div className="absolute top-2 right-2 w-6 h-6 rounded-full bg-amber-500 flex items-center justify-center shadow">
                    <CheckCircle2 className="w-4 h-4 text-white" />
                  </div>
                )}
                {/* LIVE badge */}
                {isActive && (
                  <div className="absolute top-2 left-2 flex items-center gap-1 bg-green-100 border border-green-300 text-green-700 text-[9px] font-bold px-1.5 py-0.5 rounded-full">
                    <span className="w-1.5 h-1.5 rounded-full bg-green-500" />
                    LIVE
                  </div>
                )}

                <div className="mb-3 mt-1">
                  <h4 className="font-bold text-sm text-gray-900">{theme.name}</h4>
                  <p className="text-xs text-gray-400">{theme.description}</p>
                </div>

                {/* Preview card */}
                <div
                  className="rounded-lg p-3 border-2"
                  style={{ backgroundColor: theme.preview.bg, borderColor: theme.preview.accent }}
                >
                  <div
                    className="h-4 rounded mb-2 flex items-center justify-center gap-1"
                    style={{ backgroundColor: theme.preview.header }}
                  >
                    <div className="h-1 w-8 rounded opacity-60" style={{ backgroundColor: theme.preview.bg }} />
                    <div className="h-1 w-4 rounded opacity-40" style={{ backgroundColor: theme.preview.bg }} />
                  </div>
                  <div className="space-y-1">
                    <div className="flex gap-2">
                      <div className="h-1 w-1/3 rounded opacity-30" style={{ backgroundColor: theme.preview.text }} />
                      <div className="h-1 w-1/2 rounded opacity-20" style={{ backgroundColor: theme.preview.text }} />
                    </div>
                    <div className="flex gap-2">
                      <div className="h-1 w-1/4 rounded opacity-30" style={{ backgroundColor: theme.preview.text }} />
                      <div className="h-1 w-2/3 rounded opacity-20" style={{ backgroundColor: theme.preview.text }} />
                    </div>
                    <div className="flex gap-2">
                      <div className="h-1 w-1/3 rounded opacity-30" style={{ backgroundColor: theme.preview.text }} />
                      <div className="h-1 w-1/3 rounded opacity-20" style={{ backgroundColor: theme.preview.text }} />
                    </div>
                  </div>
                  <div className="mt-2 h-2 rounded" style={{ backgroundColor: theme.preview.accent }} />
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

        {/* Options */}
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

      {/* Custom Colors */}
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
// TAB: HARDWARE
// ═════════════════════════════════════════════════════════════════════════════
function HardwareTab({ settings, setField, isDark }) {
  return (
    <div className="space-y-5 pb-6 pt-2">
      {/* RFID */}
      <Section title="RFID Reader" icon={<Wifi className="w-4 h-4 text-green-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable RFID Reader"
            description="Detect vehicles via RFID tags"
            checked={settings.rfidEnabled}
            onChange={(v) => setField("rfidEnabled", v)}
          />
          {settings.rfidEnabled && (
            <IndentGroup color="green">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Stream URL">
                  <Input
                    value={settings.rfidStreamUrl}
                    onChange={(e) => setField("rfidStreamUrl", e.target.value)}
                    placeholder="http://172.16.0.134:5000/api/rfid/stream"
                  />
                </Field>
                <Field label="Reader Type">
                  <Select value={settings.rfidReaderType} onChange={(v) => setField("rfidReaderType", v)} style={{ width: "100%" }}>
                    <Option value="UHF Reader">UHF Reader</Option>
                    <Option value="HF Reader">HF Reader</Option>
                    <Option value="LF Reader">LF Reader</Option>
                  </Select>
                </Field>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      {/* NFC */}
      <Section title="NFC Reader" icon={<CreditCard className="w-4 h-4 text-purple-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable NFC Reader"
            description="Authenticate drivers via NFC cards"
            checked={settings.nfcEnabled}
            onChange={(v) => setField("nfcEnabled", v)}
          />
          {settings.nfcEnabled && (
            <IndentGroup color="purple">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Stream URL">
                  <Input
                    value={settings.nfcStreamUrl}
                    onChange={(e) => setField("nfcStreamUrl", e.target.value)}
                    placeholder="http://172.16.0.134:5000/api/nfc/stream"
                  />
                </Field>
                <Field label="Reader Type">
                  <Select value={settings.nfcReaderType} onChange={(v) => setField("nfcReaderType", v)} style={{ width: "100%" }}>
                    <Option value="MIFARE Classic">MIFARE Classic</Option>
                    <Option value="MIFARE DESFire">MIFARE DESFire</Option>
                    <Option value="NTAG">NTAG</Option>
                  </Select>
                </Field>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      {/* ANPR */}
      <Section title="ANPR Camera" icon={<Monitor className="w-4 h-4 text-blue-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable ANPR Camera"
            description="Automatic number plate recognition via camera"
            checked={settings.anprEnabled}
            onChange={(v) => setField("anprEnabled", v)}
          />
          {settings.anprEnabled && (
            <IndentGroup color="blue">
              <div className="grid grid-cols-2 gap-4 mb-3">
                <Field label="Camera Stream URL">
                  <Input
                    value={settings.anprCameraUrl}
                    onChange={(e) => setField("anprCameraUrl", e.target.value)}
                    placeholder="http://192.168.1.50/stream"
                  />
                </Field>
                <Field label="API Endpoint">
                  <Input
                    value={settings.anprApiUrl}
                    onChange={(e) => setField("anprApiUrl", e.target.value)}
                    placeholder="http://192.168.1.50/api/detect"
                  />
                </Field>
                <Field label="Camera Position">
                  <Select value={settings.anprCameraPosition} onChange={(v) => setField("anprCameraPosition", v)} style={{ width: "100%" }}>
                    <Option value="entry">Entry Gate Only</Option>
                    <Option value="exit">Exit Gate Only</Option>
                    <Option value="both">Both Entry &amp; Exit</Option>
                  </Select>
                </Field>
                <Field label="Confidence Threshold">
                  <InputNumber
                    value={settings.anprConfidenceThreshold}
                    onChange={(v) => setField("anprConfidenceThreshold", v)}
                    min={50} max={100}
                    addonAfter="% accuracy"
                    style={{ width: "100%" }}
                  />
                </Field>
              </div>
              <ToggleRow
                label="Fallback to Manual Entry"
                description="Allow operator to type plate if ANPR fails"
                checked={settings.anprFallbackToManual}
                onChange={(v) => setField("anprFallbackToManual", v)}
              />
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mt-3">
                <p className="text-xs text-blue-800 font-semibold mb-1">💡 Integration Notes:</p>
                <ul className="text-xs text-blue-700 space-y-0.5 ml-3 list-disc">
                  <li>Camera must support HTTP/RTSP streaming</li>
                  <li>API should return JSON: <code className="bg-blue-100 px-1 rounded">{'{"plate":"KCB 123A","confidence":95}'}</code></li>
                  <li>Low confidence readings trigger manual verification</li>
                </ul>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      {/* Scale */}
      <Section title="Weighing Scale" icon={<Scale className="w-4 h-4 text-blue-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Scale Integration"
            description="Live weight readings from scale hardware"
            checked={settings.scaleEnabled}
            onChange={(v) => setField("scaleEnabled", v)}
          />
          {settings.scaleEnabled && (
            <IndentGroup color="blue">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Stream URL">
                  <Input
                    value={settings.scaleStreamUrl}
                    onChange={(e) => setField("scaleStreamUrl", e.target.value)}
                    placeholder="http://172.16.0.134:5000/api/scale/stream"
                  />
                </Field>
                <Field label="Scale Brand">
                  <Select value={settings.scaleBrand} onChange={(v) => setField("scaleBrand", v)} style={{ width: "100%" }}>
                    <Option value="Avery Weigh-Tronix">Avery Weigh-Tronix</Option>
                    <Option value="Mettler Toledo">Mettler Toledo</Option>
                    <Option value="Rice Lake">Rice Lake</Option>
                    <Option value="Cardinal">Cardinal Scale</Option>
                  </Select>
                </Field>
                <Field label="Capacity (kg)">
                  <InputNumber value={settings.scaleCapacity} onChange={(v) => setField("scaleCapacity", v)} min={1000} max={200000} style={{ width: "100%" }} />
                </Field>
                <Field label="Stability Threshold">
                  <InputNumber value={settings.scaleStabilityThreshold} onChange={(v) => setField("scaleStabilityThreshold", v)} min={3} max={10} addonAfter="readings" style={{ width: "100%" }} />
                </Field>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      {/* Printer */}
      <Section title="Ticket Printer" icon={<Printer className="w-4 h-4 text-orange-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Printer"
            description="Print weighbridge tickets"
            checked={settings.printerEnabled}
            onChange={(v) => setField("printerEnabled", v)}
          />
          {settings.printerEnabled && (
            <IndentGroup color="orange">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Printer Model">
                  <Select value={settings.printerModel} onChange={(v) => setField("printerModel", v)} style={{ width: "100%" }}>
                    <Option value="Zebra ZD420">Zebra ZD420</Option>
                    <Option value="Zebra ZD620">Zebra ZD620</Option>
                    <Option value="TSC TTP-244 Pro">TSC TTP-244 Pro</Option>
                    <Option value="Brother QL-820NWB">Brother QL-820NWB</Option>
                  </Select>
                </Field>
                <Field label="Printer IP Address">
                  <Input
                    value={settings.printerIp}
                    onChange={(e) => setField("printerIp", e.target.value)}
                    placeholder="192.168.1.100"
                  />
                </Field>
              </div>
            </IndentGroup>
          )}
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
      <Section title="Kiosk Mode" icon={<Monitor className="w-4 h-4 text-amber-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Kiosk Mode"
            description="Self-service weighing without operator login"
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
// TAB: USERS & AUTH
// ═════════════════════════════════════════════════════════════════════════════
function UsersTab({ settings, setField, isDark }) {
  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Password Policy" icon={<Lock className="w-4 h-4 text-red-500" />}>
        <div className="space-y-4">
          <Field label="Minimum Length">
            <InputNumber value={settings.passwordMinLength} onChange={(v) => setField("passwordMinLength", v)} min={6} max={32} addonAfter="characters" style={{ width: "240px" }} />
          </Field>
          <ToggleRow label="Require Uppercase Letters" checked={settings.passwordRequireUppercase} onChange={(v) => setField("passwordRequireUppercase", v)} />
          <ToggleRow label="Require Numbers" checked={settings.passwordRequireNumbers} onChange={(v) => setField("passwordRequireNumbers", v)} />
          <ToggleRow label="Require Symbols (!@#$%)" checked={settings.passwordRequireSymbols} onChange={(v) => setField("passwordRequireSymbols", v)} />
          <Field label="Password Expiry">
            <InputNumber value={settings.passwordExpiryDays} onChange={(v) => setField("passwordExpiryDays", v)} min={0} max={365} addonAfter="days (0 = never)" style={{ width: "240px" }} />
          </Field>
        </div>
      </Section>

      <Section title="Session &amp; Login" icon={<Users className="w-4 h-4 text-blue-500" />}>
        <div className="grid grid-cols-2 gap-4 mb-4">
          <Field label="Session Timeout">
            <InputNumber value={settings.sessionTimeout} onChange={(v) => setField("sessionTimeout", v)} min={5} max={480} addonAfter="minutes" style={{ width: "100%" }} />
          </Field>
          <Field label="Max Login Attempts">
            <InputNumber value={settings.maxLoginAttempts} onChange={(v) => setField("maxLoginAttempts", v)} min={3} max={10} style={{ width: "100%" }} />
          </Field>
          <Field label="Lockout Duration">
            <InputNumber value={settings.lockoutDuration} onChange={(v) => setField("lockoutDuration", v)} min={5} max={120} addonAfter="minutes" style={{ width: "100%" }} />
          </Field>
        </div>
        <ToggleRow
          label="Two-Factor Authentication (2FA)"
          description="Require OTP for all user logins"
          checked={settings.twoFactorEnabled}
          onChange={(v) => setField("twoFactorEnabled", v)}
        />
      </Section>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: API & INTEGRATION
// ═════════════════════════════════════════════════════════════════════════════
function ApiTab({ settings, setField, isDark }) {
  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Webhooks" icon={<Webhook className="w-4 h-4 text-indigo-500" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Webhooks"
            description="Send events to external systems"
            checked={settings.webhookEnabled}
            onChange={(v) => setField("webhookEnabled", v)}
          />
          {settings.webhookEnabled && (
            <IndentGroup color="indigo">
              <Field label="Webhook URL">
                <Input
                  value={settings.webhookUrl}
                  onChange={(e) => setField("webhookUrl", e.target.value)}
                  placeholder="https://your-app.com/api/webhooks"
                />
              </Field>
              <div className="mt-3">
                <Field label="Events to Send">
                  <Select
                    mode="multiple"
                    value={settings.webhookEvents}
                    onChange={(v) => setField("webhookEvents", v)}
                    style={{ width: "100%" }}
                    placeholder="Select events..."
                  >
                    <Option value="transaction.created">Transaction Created</Option>
                    <Option value="transaction.completed">Transaction Completed</Option>
                    <Option value="vehicle.detected">Vehicle Detected (RFID)</Option>
                    <Option value="driver.authenticated">Driver Authenticated (NFC)</Option>
                    <Option value="weight.captured">Weight Captured</Option>
                    <Option value="ticket.printed">Ticket Printed</Option>
                  </Select>
                </Field>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      <Section title="API Configuration" icon={<Settings className="w-4 h-4 text-gray-500" />}>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Rate Limit">
            <InputNumber value={settings.apiRateLimit} onChange={(v) => setField("apiRateLimit", v)} min={10} max={1000} addonAfter="req/min" style={{ width: "100%" }} />
          </Field>
          <div className="flex items-center justify-between mt-5">
            <span className="text-sm text-gray-700">Enable API Request Logging</span>
            <Switch checked={settings.apiLogging} onChange={(v) => setField("apiLogging", v)} />
          </div>
        </div>
      </Section>
    </div>
  );
}

// ═════════════════════════════════════════════════════════════════════════════
// TAB: BACKUP & LOGS
// ═════════════════════════════════════════════════════════════════════════════
function BackupTab({ settings, setField, isDark }) {
  return (
    <div className="space-y-5 pb-6 pt-2">
      <Section title="Database Backup" icon={<Database className="w-4 h-4 text-green-600" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Enable Auto Backup"
            description="Automatic scheduled database backups"
            checked={settings.autoBackupEnabled}
            onChange={(v) => setField("autoBackupEnabled", v)}
          />
          {settings.autoBackupEnabled && (
            <IndentGroup color="green">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Frequency">
                  <Select value={settings.backupFrequency} onChange={(v) => setField("backupFrequency", v)} style={{ width: "100%" }}>
                    <Option value="daily">Daily</Option>
                    <Option value="weekly">Weekly</Option>
                    <Option value="monthly">Monthly</Option>
                  </Select>
                </Field>
                <Field label="Backup Time">
                  <input
                    type="time"
                    value={settings.backupTime}
                    onChange={(e) => setField("backupTime", e.target.value)}
                    className="w-full h-8 text-sm rounded border border-gray-300 px-3 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  />
                </Field>
                <Field label="Retention Period">
                  <InputNumber value={settings.backupRetentionDays} onChange={(v) => setField("backupRetentionDays", v)} min={7} max={365} addonAfter="days" style={{ width: "100%" }} />
                </Field>
              </div>
            </IndentGroup>
          )}
        </div>
      </Section>

      <Section title="Logging &amp; Audit" icon={<FileText className="w-4 h-4 text-gray-500" />}>
        <div className="space-y-4">
          <ToggleRow
            label="Audit Logs"
            description="Track all user actions — login, CRUD operations"
            checked={settings.auditLogEnabled}
            onChange={(v) => setField("auditLogEnabled", v)}
          />
          {settings.auditLogEnabled && (
            <div className="ml-4 pl-4 border-l-2 border-gray-200">
              <Field label="Audit Log Retention">
                <InputNumber value={settings.auditLogRetentionDays} onChange={(v) => setField("auditLogRetentionDays", v)} min={30} max={3650} addonAfter="days" style={{ width: "240px" }} />
              </Field>
            </div>
          )}

          <ToggleRow
            label="Error Logs"
            description="Capture system errors and exceptions"
            checked={settings.errorLogEnabled}
            onChange={(v) => setField("errorLogEnabled", v)}
          />
          {settings.errorLogEnabled && (
            <div className="ml-4 pl-4 border-l-2 border-gray-200">
              <Field label="Error Log Retention">
                <InputNumber value={settings.errorLogRetentionDays} onChange={(v) => setField("errorLogRetentionDays", v)} min={7} max={365} addonAfter="days" style={{ width: "240px" }} />
              </Field>
            </div>
          )}
        </div>
      </Section>
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
  orange: "border-orange-200",
  indigo: "border-indigo-200",
};

function IndentGroup({ color = "gray", children }) {
  return (
    <div className={`ml-4 pl-4 border-l-2 space-y-3 ${borderColorMap[color] || "border-gray-200"}`}>
      {children}
    </div>
  );
}