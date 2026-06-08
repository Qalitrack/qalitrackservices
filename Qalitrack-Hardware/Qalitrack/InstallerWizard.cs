// InstallerWizard.cs
// WinForms setup wizard — only compiled and used on Windows.
// Shown when the exe is double-clicked or run with --install.

#if WINDOWS

using System.Diagnostics;
using System.ServiceProcess;
using System.Text.Json;
using Microsoft.Win32;

namespace Qalitrack.Installer;

public class InstallerWizard : Form
{
    // ── Layout constants ──────────────────────────────────────────────────────
    private const int LabelX = 20;
    private const int FieldX = 220;
    private const int FieldW = 220;
    private const int RowH   = 34;
    private const int StartY = 20;
    private const int FormW  = 520;

    // ── Service constants ─────────────────────────────────────────────────────
    private const string ServiceName = "QalitrackPlatformService";
    private const string DisplayName = "Qalitrack Platform Data Service";
    private const string Description = "Qalitrack weighbridge platform data collection and camera service with NPR";
    private const string InstallPath = @"C:\Program Files\Qalitrack";
    private const string ConfigPath  = @"C:\ProgramData\Qalitrack\qalitrack.json";

    // ── Controls ──────────────────────────────────────────────────────────────
    private RadioButton _wbModeTcp    = null!;
    private RadioButton _wbModeSerial = null!;

    // TCP rows
    private Label   _lblWbIp   = null!;
    private TextBox _wbIp      = null!;
    private Label   _lblWbPort = null!;
    private TextBox _wbPort    = null!;

    // Scale type
    private Label    _lblScaleType = null!;
    private ComboBox _wbScaleType  = null!;

    // Serial rows
    private Label    _lblSerialPort = null!;
    private TextBox  _wbSerialPort  = null!;
    private Label    _lblBaudRate   = null!;
    private TextBox  _wbBaudRate    = null!;
    private Label    _lblDataBits   = null!;
    private ComboBox _wbDataBits    = null!;
    private Label    _lblParity     = null!;
    private ComboBox _wbParity      = null!;
    private Label    _lblStopBits   = null!;
    private ComboBox _wbStopBits    = null!;

    // Camera
    private TextBox  _camIp   = null!;
    private TextBox  _camPort = null!;
    private TextBox  _camUser = null!;
    private TextBox  _camPass = null!;
    private TextBox  _camRtsp = null!;
    private CheckBox _camSnap = null!;
    private CheckBox _useNpr  = null!;
    private TextBox  _nprHttp = null!;
    private TextBox  _nprWs   = null!;

    // RFID
    private CheckBox _useRfid  = null!;
    private TextBox  _rfidIp   = null!;
    private TextBox  _rfidPort = null!;

    // NFC
    private CheckBox _useNfc  = null!;
    private TextBox  _nfcPort = null!;

    private Button _installBtn  = null!;
    private Label  _statusLabel = null!;

    public InstallerWizard()
    {
        BuildForm();
        LoadExistingConfig();
    }

    // ── Form builder ──────────────────────────────────────────────────────────
    private void BuildForm()
    {
        Text            = "Qalitrack — Service Installer";
        Width           = FormW;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9f);
        BackColor       = Color.White;

        var panel = new Panel
        {
            AutoScroll = true,
            Dock       = DockStyle.Fill,
            Padding    = new Padding(0, 0, 0, 60)
        };
        Controls.Add(panel);

        int y = StartY;

        // ── Banner ────────────────────────────────────────────────────────────
        var banner = new Label
        {
            Text      = "Qalitrack Platform Service — Setup",
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 80, 180),
            Location  = new Point(LabelX, y),
            AutoSize  = true
        };
        panel.Controls.Add(banner);
        y += 36;

        var sub = new Label
        {
            Text      = "Fill in the details for this site and click Install.",
            ForeColor = Color.Gray,
            Location  = new Point(LabelX, y),
            AutoSize  = true
        };
        panel.Controls.Add(sub);
        y += 30;

        // ── Weighbridge ───────────────────────────────────────────────────────
        y = Section(panel, y, "Weighbridge");

        // Connection type label + radio buttons
        var lblMode = new Label
        {
            Text     = "Connection Type",
            Location = new Point(LabelX, y + 3),
            Size     = new Size(FieldX - LabelX - 10, 22),
            AutoSize = false
        };
        panel.Controls.Add(lblMode);

        _wbModeTcp = new RadioButton
        {
            Text     = "TCP / Network",
            Checked  = true,
            Location = new Point(FieldX, y),
            Size     = new Size(120, 22)
        };
        _wbModeSerial = new RadioButton
        {
            Text     = "Serial / RS-232",
            Checked  = false,
            Location = new Point(FieldX + 125, y),
            Size     = new Size(130, 22)
        };
        panel.Controls.Add(_wbModeTcp);
        panel.Controls.Add(_wbModeSerial);
        y += RowH;

        // ── TCP rows ──────────────────────────────────────────────────────────
        _lblWbIp = new Label { Text = "IP Address", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbIp    = new TextBox { Text = "172.16.1.243", Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        panel.Controls.Add(_lblWbIp);
        panel.Controls.Add(_wbIp);
        y += RowH;

        _lblWbPort = new Label { Text = "Port", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbPort    = new TextBox { Text = "3002", Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        panel.Controls.Add(_lblWbPort);
        panel.Controls.Add(_wbPort);
        y += RowH;

        // ── Scale type row (always visible) ───────────────────────────────
        _lblScaleType = new Label { Text = "Scale Type", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbScaleType  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        _wbScaleType.Items.AddRange(new object[] { "Generic", "YaghuaXK3190DS8" });
        _wbScaleType.SelectedIndex = 0;
        panel.Controls.Add(_lblScaleType);
        panel.Controls.Add(_wbScaleType);
        y += RowH;

        // ── Serial rows ───────────────────────────────────────────────────────
        _lblSerialPort = new Label { Text = "Serial Port", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbSerialPort  = new TextBox { Text = "COM1", Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        panel.Controls.Add(_lblSerialPort);
        panel.Controls.Add(_wbSerialPort);
        y += RowH;

        _lblBaudRate = new Label { Text = "Baud Rate", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbBaudRate  = new TextBox { Text = "9600", Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        panel.Controls.Add(_lblBaudRate);
        panel.Controls.Add(_wbBaudRate);
        y += RowH;

        _lblDataBits = new Label { Text = "Data Bits", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbDataBits  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        _wbDataBits.Items.AddRange(new object[] { "8", "7", "6", "5" });
        _wbDataBits.SelectedIndex = 0;
        panel.Controls.Add(_lblDataBits);
        panel.Controls.Add(_wbDataBits);
        y += RowH;

        // Parity values match System.IO.Ports.Parity enum names
        _lblParity = new Label { Text = "Parity", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbParity  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        _wbParity.Items.AddRange(new object[] { "None", "Odd", "Even", "Mark", "Space" });
        _wbParity.SelectedIndex = 0;
        panel.Controls.Add(_lblParity);
        panel.Controls.Add(_wbParity);
        y += RowH;

        // StopBits values match System.IO.Ports.StopBits enum names
        _lblStopBits = new Label { Text = "Stop Bits", Location = new Point(LabelX, y + 3), Size = new Size(FieldX - LabelX - 10, 22), AutoSize = false };
        _wbStopBits  = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(FieldX, y), Size = new Size(FieldW, 24) };
        _wbStopBits.Items.AddRange(new object[] { "One", "Two", "OnePointFive" });
        _wbStopBits.SelectedIndex = 0;
        panel.Controls.Add(_lblStopBits);
        panel.Controls.Add(_wbStopBits);
        y += RowH;

        // Hide serial rows initially (TCP is default)
        SetSerialVisible(false);

        // Toggle when radio changes
        _wbModeTcp.CheckedChanged += (_, _) =>
        {
            bool tcp = _wbModeTcp.Checked;
            SetTcpVisible(tcp);
            SetSerialVisible(!tcp);
        };

        // ── Camera ────────────────────────────────────────────────────────────
        y = Section(panel, y, "Camera (RTSP)");
        _camIp   = Field(panel, ref y, "IP Address",  "172.16.0.22");
        _camPort = Field(panel, ref y, "RTSP Port",   "554");
        _camRtsp = Field(panel, ref y, "RTSP Path",   "");
        _camUser = Field(panel, ref y, "Username",    "admin");
        _camPass = Field(panel, ref y, "Password",    "admin", password: true);
        _camSnap = CheckField(panel, ref y, "Supports Snapshots", defaultChecked: true);

        _useNpr  = CheckField(panel, ref y, "Enable Plate Recognition (NPR)", defaultChecked: false);
        _nprHttp = Field(panel, ref y, "NPR HTTP URL",      "http://172.16.0.22:80");
        _nprWs   = Field(panel, ref y, "NPR WebSocket URL", "ws://172.16.0.22:9080");

        _camIp.TextChanged += (_, _) =>
        {
            var ip = _camIp.Text.Trim();
            if (!string.IsNullOrWhiteSpace(ip))
            {
                _nprHttp.Text = $"http://{ip}:80";
                _nprWs.Text   = $"ws://{ip}:9080";
            }
        };

        void ToggleNpr() { _nprHttp.Enabled = _nprWs.Enabled = _useNpr.Checked; }
        _useNpr.CheckedChanged += (_, _) => ToggleNpr();
        ToggleNpr();

        // ── RFID ──────────────────────────────────────────────────────────────
        y = Section(panel, y, "RFID Reader (optional)");
        _useRfid  = CheckField(panel, ref y, "RFID Reader Installed", defaultChecked: false);
        _rfidIp   = Field(panel, ref y, "IP Address", "192.168.1.100");
        _rfidPort = Field(panel, ref y, "Port",       "2022");

        void ToggleRfid() { _rfidIp.Enabled = _rfidPort.Enabled = _useRfid.Checked; }
        _useRfid.CheckedChanged += (_, _) => ToggleRfid();
        ToggleRfid();

        // ── NFC ───────────────────────────────────────────────────────────────
        y = Section(panel, y, "NFC Reader (optional)");
        _useNfc  = CheckField(panel, ref y, "NFC Reader Installed", defaultChecked: false);
        _nfcPort = Field(panel, ref y, "Serial Port", "COM6");

        void ToggleNfc() { _nfcPort.Enabled = _useNfc.Checked; }
        _useNfc.CheckedChanged += (_, _) => ToggleNfc();
        ToggleNfc();

        y += 10;

        // ── Status label ──────────────────────────────────────────────────────
        _statusLabel = new Label
        {
            Text      = "",
            Location  = new Point(LabelX, y),
            Size      = new Size(FormW - 50, 24),
            ForeColor = Color.Gray
        };
        panel.Controls.Add(_statusLabel);
        y += 28;

        // ── Install button ────────────────────────────────────────────────────
        _installBtn = new Button
        {
            Text      = "Install Service",
            Location  = new Point(LabelX, y),
            Size      = new Size(160, 36),
            BackColor = Color.FromArgb(60, 80, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _installBtn.FlatAppearance.BorderSize = 0;
        _installBtn.Click += OnInstallClick;
        panel.Controls.Add(_installBtn);

        Height = Math.Min(y + 120, Screen.PrimaryScreen!.WorkingArea.Height - 40);
    }

    // ── Show/hide TCP vs Serial rows ──────────────────────────────────────────
    private void SetTcpVisible(bool visible)
    {
        _lblWbIp.Visible   = visible;
        _wbIp.Visible      = visible;
        _lblWbPort.Visible = visible;
        _wbPort.Visible    = visible;
    }

    private void SetSerialVisible(bool visible)
    {
        _lblSerialPort.Visible = visible;
        _wbSerialPort.Visible  = visible;
        _lblBaudRate.Visible   = visible;
        _wbBaudRate.Visible    = visible;
        _lblDataBits.Visible   = visible;
        _wbDataBits.Visible    = visible;
        _lblParity.Visible     = visible;
        _wbParity.Visible      = visible;
        _lblStopBits.Visible   = visible;
        _wbStopBits.Visible    = visible;
    }

    // ── Field helpers ─────────────────────────────────────────────────────────
    private static int Section(Panel panel, int y, string title)
    {
        y += 8;
        var sep = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 80, 180),
            Location  = new Point(LabelX, y),
            AutoSize  = true
        };
        panel.Controls.Add(sep);

        var line = new Panel
        {
            Location  = new Point(LabelX, y + 20),
            Size      = new Size(FormW - 55, 1),
            BackColor = Color.FromArgb(200, 200, 220)
        };
        panel.Controls.Add(line);
        return y + 28;
    }

    private static TextBox Field(Panel panel, ref int y, string label, string defaultValue,
        bool password = false)
    {
        var lbl = new Label
        {
            Text     = label,
            Location = new Point(LabelX, y + 3),
            Size     = new Size(FieldX - LabelX - 10, 22),
            AutoSize = false
        };
        panel.Controls.Add(lbl);

        var txt = new TextBox
        {
            Text                  = defaultValue,
            Location              = new Point(FieldX, y),
            Size                  = new Size(FieldW, 24),
            UseSystemPasswordChar = password
        };
        panel.Controls.Add(txt);
        y += RowH;
        return txt;
    }

    private static CheckBox CheckField(Panel panel, ref int y, string label, bool defaultChecked)
    {
        var chk = new CheckBox
        {
            Text     = label,
            Checked  = defaultChecked,
            Location = new Point(FieldX, y),
            AutoSize = true
        };
        panel.Controls.Add(chk);
        y += RowH;
        return chk;
    }

    // ── Pre-fill from existing config ─────────────────────────────────────────
    private void LoadExistingConfig()
    {
        if (!File.Exists(ConfigPath)) return;
        try
        {
            var json = File.ReadAllText(ConfigPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("TcpListener", out var tcp))
            {
                // ConnectionType = "TCP" or "Serial" — matches ConnectionSettings model
                var connType = tcp.GetStringOrDefault("ConnectionType", "TCP").ToUpperInvariant();
                if (connType == "SERIAL")
                {
                    _wbModeSerial.Checked = true;
                    _wbSerialPort.Text    = tcp.GetStringOrDefault("SerialPort", "COM1");
                    _wbBaudRate.Text      = tcp.GetIntOrDefault("BaudRate", 9600).ToString();

                    var dataBits = tcp.GetIntOrDefault("DataBits", 8).ToString();
                    var parity   = tcp.GetStringOrDefault("Parity",   "None");
                    var stopBits = tcp.GetStringOrDefault("StopBits", "One");

                    if (_wbDataBits.Items.Contains(dataBits)) _wbDataBits.SelectedItem = dataBits;
                    if (_wbParity.Items.Contains(parity))     _wbParity.SelectedItem   = parity;
                    if (_wbStopBits.Items.Contains(stopBits)) _wbStopBits.SelectedItem = stopBits;
                }
                else
                {
                    _wbModeTcp.Checked = true;
                    _wbIp.Text   = tcp.GetStringOrDefault("IpAddress", _wbIp.Text);
                    _wbPort.Text = tcp.GetIntOrDefault("Port", 3002).ToString();
                }

                var scaleType = tcp.GetStringOrDefault("ScaleType", "Generic");
                if (_wbScaleType.Items.Contains(scaleType)) _wbScaleType.SelectedItem = scaleType;
            }

            if (root.TryGetProperty("CameraSettings", out var cam) &&
                cam.TryGetProperty("Cameras", out var cameras) &&
                cameras.GetArrayLength() > 0)
            {
                var c = cameras[0];
                _camIp.Text      = c.GetStringOrDefault("IpAddress", _camIp.Text);
                _camPort.Text    = c.GetIntOrDefault("Port", 554).ToString();
                _camRtsp.Text    = c.GetStringOrDefault("RtspPath", "");
                _camUser.Text    = c.GetStringOrDefault("Username", "admin");
                _camSnap.Checked = c.GetBoolOrDefault("SupportsSnapshot", true);

                if (c.TryGetProperty("NprSettings", out var npr))
                {
                    _useNpr.Checked = npr.GetBoolOrDefault("Enabled", false);
                    _nprHttp.Text   = npr.GetStringOrDefault("CameraQueryUrl", _nprHttp.Text);
                    _nprWs.Text     = npr.GetStringOrDefault("WebSocketUrl",   _nprWs.Text);
                }
            }

            if (root.TryGetProperty("RfidSettings", out var rfid))
            {
                _useRfid.Checked = rfid.GetBoolOrDefault("Enabled",  false);
                _rfidIp.Text     = rfid.GetStringOrDefault("Host",   _rfidIp.Text);
                _rfidPort.Text   = rfid.GetIntOrDefault("Port",      2022).ToString();
            }

            if (root.TryGetProperty("NfcSettings", out var nfc))
            {
                _useNfc.Checked = nfc.GetBoolOrDefault("Enabled", false);
                _nfcPort.Text   = nfc.GetStringOrDefault("Port",   "COM6");
            }

            _statusLabel.Text      = "Existing config loaded — review and click Install to update.";
            _statusLabel.ForeColor = Color.DarkGoldenrod;
            _installBtn.Text       = "Update & Reinstall";
        }
        catch
        {
            // If config is unreadable just leave the defaults
        }
    }

    // ── Install button handler ────────────────────────────────────────────────
    private void OnInstallClick(object? sender, EventArgs e)
    {
        bool useTcp = _wbModeTcp.Checked;

        if (useTcp)
        {
            if (string.IsNullOrWhiteSpace(_wbIp.Text))
            {
                ShowError("Weighbridge IP address is required.");
                return;
            }
            if (!int.TryParse(_wbPort.Text, out _))
            {
                ShowError("Weighbridge port must be a number.");
                return;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_wbSerialPort.Text))
            {
                ShowError("Serial port name is required (e.g. COM1 or /dev/ttyUSB0).");
                return;
            }
            if (!int.TryParse(_wbBaudRate.Text, out _))
            {
                ShowError("Baud rate must be a number.");
                return;
            }
        }

        if (!int.TryParse(_camPort.Text, out _))
        {
            ShowError("Camera port must be a number.");
            return;
        }

        _installBtn.Enabled    = false;
        _statusLabel.Text      = "Installing...";
        _statusLabel.ForeColor = Color.DarkBlue;
        Application.DoEvents();

        try
        {
            WriteConfig();
            InstallService();

            _statusLabel.Text      = "✓ Installation complete! Service is running.";
            _statusLabel.ForeColor = Color.DarkGreen;
            _installBtn.Text       = "Installed ✓";

            MessageBox.Show(
                $"Qalitrack service installed and started successfully.\n\n" +
                $"API:       http://localhost:5000\n" +
                $"Config UI: http://localhost:5000/config.html\n\n" +
                $"Config file:\n{ConfigPath}",
                "Installation Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _statusLabel.Text      = $"✗ Error: {ex.Message}";
            _statusLabel.ForeColor = Color.Red;
            _installBtn.Enabled    = true;
            _installBtn.Text       = "Retry Install";

            MessageBox.Show(
                $"Installation failed:\n\n{ex.Message}\n\n" +
                "Make sure you are running as Administrator.",
                "Installation Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ── Write config file ─────────────────────────────────────────────────────
    private void WriteConfig()
    {
        var dir = Path.GetDirectoryName(ConfigPath)!;
        Directory.CreateDirectory(dir);

        if (File.Exists(ConfigPath))
            File.Copy(ConfigPath, ConfigPath + ".bak", overwrite: true);

        bool useTcp = _wbModeTcp.Checked;
        var  camIp  = _camIp.Text.Trim();

        // ConnectionType matches the ConnectionSettings.ConnectionType property ("TCP" or "Serial")
        // Parity/StopBits are written as enum name strings — JsonSerializer deserialises them
        // back into System.IO.Ports.Parity and System.IO.Ports.StopBits automatically.
        var config = new
        {
            TcpListener = new
            {
                ConnectionType   = useTcp ? "TCP" : "Serial",
                ScaleType        = _wbScaleType.SelectedItem?.ToString() ?? "Generic",
                IpAddress        = useTcp ? _wbIp.Text.Trim() : "172.16.1.243",
                Port             = useTcp && int.TryParse(_wbPort.Text, out int tp) ? tp : 3002,
                ReadTimeoutMs    = 1000,
                ReconnectDelayMs = 5000,
                SerialPort       = useTcp ? "AUTO" : _wbSerialPort.Text.Trim(),
                BaudRate         = !useTcp && int.TryParse(_wbBaudRate.Text, out int br) ? br : 9600,
                DataBits         = !useTcp && int.TryParse(_wbDataBits.SelectedItem?.ToString() ?? "8", out int db) ? db : 8,
                Parity           = useTcp ? "None" : (_wbParity.SelectedItem?.ToString()   ?? "None"),
                StopBits         = useTcp ? "One"  : (_wbStopBits.SelectedItem?.ToString() ?? "One")
            },
            NfcSettings = new
            {
                Enabled  = _useNfc.Checked,
                Port     = _nfcPort.Text.Trim(),
                BaudRate = 9600
            },
            CameraSettings = new
            {
                Cameras = new[]
                {
                    new
                    {
                        Id               = "npr1",
                        Name             = "NPR Camera 1",
                        IpAddress        = camIp,
                        Port             = int.Parse(_camPort.Text.Trim()),
                        Username         = _camUser.Text.Trim(),
                        Password         = _camPass.Text,
                        RtspPath         = _camRtsp.Text.Trim(),
                        Enabled          = true,
                        SupportsSnapshot = _camSnap.Checked,
                        NprSettings      = new
                        {
                            Enabled           = _useNpr.Checked,
                            CameraQueryUrl    = _nprHttp.Text.Trim(),
                            WebSocketUrl      = _nprWs.Text.Trim(),
                            Username          = _camUser.Text.Trim(),
                            Password          = _camPass.Text,
                            PollingIntervalMs = 1000,
                            UseWebSocket      = true
                        }
                    }
                },
                ReconnectDelayMs = 5000,
                FrameBufferSize  = 100
            },
            RfidSettings = new
            {
                Enabled          = _useRfid.Checked,
                Host             = _rfidIp.Text.Trim(),
                Port             = int.TryParse(_rfidPort.Text, out int rp) ? rp : 2022,
                ScanIntervalMs   = 1500,
                ReconnectDelayMs = 5000
            },
            Logging = new
            {
                LogLevel = new Dictionary<string, string>
                {
                    ["Default"]                      = "Information",
                    ["Microsoft"]                    = "Warning",
                    ["Microsoft.Hosting.Lifetime"]   = "Information",
                    ["Microsoft.AspNetCore"]         = "Warning",
                    ["Microsoft.AspNetCore.Routing"] = "Warning",
                    ["Microsoft.AspNetCore.Cors"]    = "Warning",
                    ["System"]                       = "Warning",
                    ["Qalitrack"]                    = "Information"
                },
                Console = new
                {
                    IncludeScopes   = true,
                    TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ",
                    LogLevel        = new Dictionary<string, string>
                    {
                        ["Default"]                    = "Information",
                        ["Microsoft"]                  = "Warning",
                        ["Microsoft.Hosting.Lifetime"] = "Information",
                        ["Qalitrack"]                  = "Information"
                    }
                },
                EventLog = new
                {
                    SourceName = "QalitrackPlatformService",
                    LogName    = "Application",
                    LogLevel   = new Dictionary<string, string>
                    {
                        ["Default"]   = "Information",
                        ["Microsoft"] = "Warning",
                        ["Qalitrack"] = "Information"
                    }
                },
                Systemd = new
                {
                    IncludeScopes   = true,
                    TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ",
                    LogLevel        = new Dictionary<string, string>
                    {
                        ["Default"]   = "Information",
                        ["Microsoft"] = "Warning",
                        ["Qalitrack"] = "Information"
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    // ── Install / reinstall service ───────────────────────────────────────────
    private static void InstallService()
    {
        var exePath = Process.GetCurrentProcess().MainModule!.FileName!;

        try
        {
            using var sc = new ServiceController(ServiceName);
            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
        }
        catch { /* service doesn't exist yet — fine */ }

        Run("sc.exe", $"delete \"{ServiceName}\"", throwOnError: false);
        Thread.Sleep(1500);

        Directory.CreateDirectory(InstallPath);
        CopyDirectory(Path.GetDirectoryName(exePath)!, InstallPath);
        var dest = Path.Combine(InstallPath, "Qalitrack.exe");

        Run("sc.exe", $"create \"{ServiceName}\" binPath= \"\\\"{dest}\\\"\" DisplayName= \"{DisplayName}\" start= auto");
        Run("sc.exe", $"description \"{ServiceName}\" \"{Description}\"");
        Run("sc.exe", $"failure \"{ServiceName}\" reset= 86400 actions= restart/5000/restart/10000/restart/60000");
        Run("sc.exe", $"failureflag \"{ServiceName}\" 1");

        try
        {
            Run("netsh", "advfirewall firewall delete rule name=\"Qalitrack Platform Service (TCP 5000)\"",
                throwOnError: false);
            Run("netsh",
                "advfirewall firewall add rule name=\"Qalitrack Platform Service (TCP 5000)\" " +
                "dir=in action=allow protocol=TCP localport=5000");
        }
        catch { /* non-fatal */ }

        Run("sc.exe", $"start \"{ServiceName}\"");
    }

    private static void Run(string exe, string args, bool throwOnError = true)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute        = false,
            CreateNoWindow         = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true
        };
        using var p = Process.Start(psi)!;
        p.WaitForExit(10000);
        if (throwOnError && p.ExitCode != 0)
            throw new Exception($"{exe} {args}\nExit code: {p.ExitCode}\n{p.StandardError.ReadToEnd()}");
    }

    private static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.GetFiles(src))
            File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(src))
            CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
    }

    private void ShowError(string msg)
    {
        _statusLabel.Text      = $"✗ {msg}";
        _statusLabel.ForeColor = Color.Red;
        MessageBox.Show(msg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}

// ── JsonElement extension helpers ─────────────────────────────────────────────
internal static class JsonElementExtensions
{
    public static string GetStringOrDefault(this JsonElement el, string prop, string def)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? def : def;

    public static int GetIntOrDefault(this JsonElement el, string prop, int def)
        => el.TryGetProperty(prop, out var v) && v.TryGetInt32(out int i) ? i : def;

    public static bool GetBoolOrDefault(this JsonElement el, string prop, bool def)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True ||
           (v.ValueKind != JsonValueKind.False && def);
}

#endif