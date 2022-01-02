using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

namespace OMSI_Time_Sync
{
    public partial class frmMain : Form
    {
        public DateTime omsiTime = DateTime.MinValue;
        public DateTime systemTime;

        public bool omsiLoaded = false;
        public bool processAttached = false;

        public Mem m;

        globalKeyboardHook gkhManualSyncHotkey = new globalKeyboardHook();

        public frmMain()
        {
            InitializeComponent();
        }

        private bool getOmsiTime()
        {
            if (processAttached)
            {
                string dateStr = m.ReadInt(OmsiAddresses.day).ToString("D2") + "/" + m.ReadInt(OmsiAddresses.month).ToString("D2") + "/" + m.ReadInt(OmsiAddresses.year).ToString("D4") + " " + m.ReadByte(OmsiAddresses.hour).ToString("D2") + ":" + m.ReadByte(OmsiAddresses.minute).ToString("D2") + ":" + ((int)Math.Max(0, Math.Min(59, Math.Ceiling(m.ReadFloat(OmsiAddresses.second))))).ToString("D2");

                return DateTime.TryParse(dateStr, out omsiTime);
            }

            return false;
        }

        private bool syncOmsiTime()
        {
            try
            {
                if (processAttached && omsiLoaded)
                {
                    double timeDifference = (systemTime - omsiTime).TotalSeconds;

                    if (
                        (!AppConfig.onlyResyncOmsiTimeIfBehindActualTime) ||
                        (AppConfig.onlyResyncOmsiTimeIfBehindActualTime && timeDifference > 1.0)
                       )
                    {
                        // 0  - Always
                        // 1  - Only when bus is moving
                        // 2  - Only when bus is not moving
                        // 3  - Only when bus has a timetable
                        // 4  - Only when bus has no timetable
                        if (
                            AppConfig.autoSyncModeIndex == 0 ||
                            (
                             AppConfig.autoSyncModeIndex == 1 &&
                             OmsiTelemetry.pluginActive &&
                             OmsiTelemetry.busSpeedKph > 0.0
                            ) ||
                            (
                             AppConfig.autoSyncModeIndex == 2 &&
                             OmsiTelemetry.pluginActive &&
                             OmsiTelemetry.busSpeedKph == 0.0
                            ) ||
                            (
                             AppConfig.autoSyncModeIndex == 3 &&
                             OmsiTelemetry.pluginActive &&
                             OmsiTelemetry.scheduleActive == 1
                            ) ||
                            (
                             AppConfig.autoSyncModeIndex == 4 &&
                             OmsiTelemetry.pluginActive &&
                             OmsiTelemetry.scheduleActive == 0
                            )
                           )
                        {
                            DateTime newSystemTime = systemTime;

                            // This should prevent a rare scenario where BCS thinks the time has been set in the past
                            if (AppConfig.onlyResyncOmsiTimeIfBehindActualTime)
                            {
                                newSystemTime = newSystemTime.AddSeconds(2.0);
                            }

                            m.WriteMemory(OmsiAddresses.hour, "int", newSystemTime.Hour.ToString());
                            m.WriteMemory(OmsiAddresses.minute, "int", newSystemTime.Minute.ToString());
                            m.WriteMemory(OmsiAddresses.second, "float", newSystemTime.Second.ToString());

                            m.WriteMemory(OmsiAddresses.day, "int", newSystemTime.Day.ToString());
                            m.WriteMemory(OmsiAddresses.month, "int", newSystemTime.Month.ToString());
                            m.WriteMemory(OmsiAddresses.year, "int", newSystemTime.Year.ToString());
                        }
                    }

                    return getOmsiTime();
                }
            }
            catch { }

            return false;
        }

        private bool loadConfig()
        {
            try
            {
                TextReader txtRdr = new StreamReader("config.txt");

                AppConfig.alwaysOnTop = Convert.ToBoolean(txtRdr.ReadLine());
                AppConfig.autoSyncOmsiTime = Convert.ToBoolean(txtRdr.ReadLine());
                AppConfig.onlyResyncOmsiTimeIfBehindActualTime = Convert.ToBoolean(txtRdr.ReadLine());
                AppConfig.offsetHour = Math.Max(-23, Math.Min(23, Convert.ToInt32(txtRdr.ReadLine())));
                AppConfig.offsetHourIndex = Convert.ToInt32(txtRdr.ReadLine());
                AppConfig.windowPositionLeft = Convert.ToInt32(txtRdr.ReadLine());
                AppConfig.windowPositionTop = Convert.ToInt32(txtRdr.ReadLine());
                AppConfig.manualSyncHotkeyIndex = Convert.ToInt32(txtRdr.ReadLine());
                AppConfig.autoSyncModeIndex = Convert.ToInt32(txtRdr.ReadLine());

                return true;
            }
            catch
            {
                AppConfig.alwaysOnTop = AppConfigDefaults.alwaysOnTop;
                AppConfig.autoSyncOmsiTime = AppConfigDefaults.autoSyncOmsiTime;
                AppConfig.onlyResyncOmsiTimeIfBehindActualTime = AppConfigDefaults.onlyResyncOmsiTimeIfBehindActualTime;
                AppConfig.offsetHour = AppConfigDefaults.offsetHour;
                AppConfig.offsetHourIndex = AppConfigDefaults.offsetHourIndex;
                AppConfig.windowPositionLeft = AppConfigDefaults.windowPositionLeft;
                AppConfig.windowPositionTop = AppConfigDefaults.windowPositionTop;
                AppConfig.manualSyncHotkeyIndex = AppConfigDefaults.manualSyncHotkeyIndex;
                AppConfig.autoSyncModeIndex = AppConfigDefaults.autoSyncModeIndex;

                return false; 
            }
        }

        private bool saveConfig()
        {
            try
            {
                TextWriter txtWtr = new StreamWriter("config.txt");

                txtWtr.WriteLine(AppConfig.alwaysOnTop.ToString());
                txtWtr.WriteLine(AppConfig.autoSyncOmsiTime.ToString());
                txtWtr.WriteLine(AppConfig.onlyResyncOmsiTimeIfBehindActualTime.ToString());
                txtWtr.WriteLine(AppConfig.offsetHour.ToString());
                txtWtr.WriteLine(AppConfig.offsetHourIndex.ToString());
                txtWtr.WriteLine(AppConfig.windowPositionLeft.ToString());
                txtWtr.WriteLine(AppConfig.windowPositionTop.ToString());
                txtWtr.WriteLine(AppConfig.manualSyncHotkeyIndex.ToString());
                txtWtr.WriteLine(AppConfig.autoSyncModeIndex.ToString());

                txtWtr.Close();

                return true;
            }
            catch { return false; }
        }

        void RunClient()
        {
            while (true)
            {
                try
                {
                    using (var pipeClient = new NamedPipeClientStream(".", "OmsiTimeSyncTelemetryPlugin", PipeDirection.InOut))
                    {
                        OmsiTelemetry.pluginActive = pipeClient.IsConnected;

                        if (!pipeClient.IsConnected)
                        {
                            pipeClient.Connect();
                        }

                        OmsiTelemetry.pluginActive = pipeClient.IsConnected;

                        using (var reader = new StreamReader(pipeClient))
                        {
                            using (var writer = new StreamWriter(pipeClient))
                            {
                                while (true)
                                {
                                    writer.WriteLine("telemetry");
                                    writer.Flush();

                                    pipeClient.WaitForPipeDrain();

                                    var message = reader.ReadLine();

                                    if (message != null)
                                    {
                                        try
                                        {
                                            string[] telemetryData = message.Split(new char[] { '*' });

                                            float.TryParse(telemetryData[0], out OmsiTelemetry.busSpeedKph);
                                            byte.TryParse(telemetryData[1], out OmsiTelemetry.scheduleActive);
                                        }
                                        catch { }
                                    };

                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void tmrOMSI_Tick(object sender, EventArgs e)
        {
            if (OmsiTelemetry.pluginActive)
            {
                this.lblOmsiTelemetryPluginStatus.Text = "Active";
            }
            else
            {
                this.lblOmsiTelemetryPluginStatus.Text = "Not Detected";
            }

            systemTime = DateTime.Now.AddHours(AppConfig.offsetHour);
            lblSystemTime.Text = systemTime.ToString();

            int processID = m.GetProcIdFromName("omsi");

            if (!processAttached)
            {
                if (processID > 0)
                {
                    processAttached = m.OpenProcess(processID);
                }
            }

            if (processID <= 0 && processAttached)
            {
                processAttached = false;

                m.CloseProcess();
            }
            
            if (processAttached)
            {
                omsiLoaded = getOmsiTime();

                if (!omsiLoaded)
                {
                    lblOmsiTime.Text = "OMSI is running, waiting for a map to load!";

                    return;
                }

                if (AppConfig.autoSyncOmsiTime)
                {
                    syncOmsiTime();
                }

                lblOmsiTime.Text = omsiTime.ToString();
            }
            else
            {
                lblOmsiTime.Text = "OMSI is not running!";
            }
        }

        private void chkAutoSyncOmsiTime_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.autoSyncOmsiTime = chkAutoSyncOmsiTime.Checked;

            btnManualSyncOmsiTime.Enabled = !chkAutoSyncOmsiTime.Checked;
        }

        private void chkOnlyResyncOmsiTimeIfBehindActualTime_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.onlyResyncOmsiTimeIfBehindActualTime = chkOnlyResyncOmsiTimeIfBehindActualTime.Checked;
        }

        private void cmbOffsetHours_SelectedIndexChanged(object sender, EventArgs e)
        {
            AppConfig.offsetHour = Convert.ToInt32(cmbOffsetHours.SelectedItem);
            AppConfig.offsetHourIndex = cmbOffsetHours.SelectedIndex;
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            AppConfig.alwaysOnTop = chkAlwaysOnTop.Checked;
            TopMost = chkAlwaysOnTop.Checked;
        }

        private void btnManualSyncOmsiTime_Click(object sender, EventArgs e)
        {
            if (!syncOmsiTime())
            {
                MessageBox.Show("ERROR: Unable to sync OMSI time. Please check that OMSI is running and a map has been loaded.", "OMSI Time Sync - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbManualSyncHotkey_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gkhManualSyncHotkey.HookedKeys.Count > 0)
            {
                gkhManualSyncHotkey.HookedKeys.Clear();
            }

            if ((Keys)cmbManualSyncHotkey.SelectedItem != Keys.None)
            {
                gkhManualSyncHotkey.HookedKeys.Add((Keys)cmbManualSyncHotkey.SelectedItem);
            }

            if (cmbManualSyncHotkey.Visible) AppConfig.manualSyncHotkeyIndex = cmbManualSyncHotkey.SelectedIndex;
        }

        private void manualSyncHotkey_KeyUp(object sender, KeyEventArgs e)
        {
            syncOmsiTime();
        }

        private void cmbAutoSyncMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            AppConfig.autoSyncModeIndex = cmbAutoSyncMode.SelectedIndex;
        }

        private void lnkGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://github.com/Ixe1/OMSI-Time-Sync");
        }

        private void lnkDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://paypal.me/ixe1");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            loadConfig();

            if (AppConfig.windowPositionTop != -1 && AppConfig.windowPositionLeft != -1)
            {
                StartPosition = FormStartPosition.Manual;

                Top = AppConfig.windowPositionTop;
                Left = AppConfig.windowPositionLeft;
            }

            cmbManualSyncHotkey.DataSource = Enum.GetValues(typeof(Keys));

            cmbOffsetHours.SelectedIndex = AppConfig.offsetHourIndex;
            cmbManualSyncHotkey.SelectedIndex = AppConfig.manualSyncHotkeyIndex;
            cmbAutoSyncMode.SelectedIndex = AppConfig.autoSyncModeIndex;

            chkAlwaysOnTop.Checked = AppConfig.alwaysOnTop;
            chkAutoSyncOmsiTime.Checked = AppConfig.autoSyncOmsiTime;
            chkOnlyResyncOmsiTimeIfBehindActualTime.Checked = AppConfig.onlyResyncOmsiTimeIfBehindActualTime;

            gkhManualSyncHotkey.KeyUp += new KeyEventHandler(manualSyncHotkey_KeyUp);

            cmbManualSyncHotkey.Visible = true;

            if (!File.Exists("config.txt"))
            {
                if (MessageBox.Show(
                    "Thanks for downloading and running OMSI Time Sync.\n" +
                    "\n" +
                    "It's important that you close any games that have anti-cheat protection before pressing 'Yes'! This program performs memory editing which might be falsely flagged as a hack.\n" +
                    "\n" +
                    "This notice will not be shown again unless the 'config.txt' file is deleted. The author of this program will not be liable.\n" +
                    "\n" +
                    "While this is a free program, a donation is highly appreciated if you like this program.\n" +
                    "\n" +
                    "Do you acknowledge the above notice and agree?",
                    "OMSI Time Sync", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    this.Close();
                    Application.Exit();

                    return;
                }
            }

            m = new Mem();

            tmrOMSI.Enabled = true;

            var client = Task.Factory.StartNew(() => RunClient());
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppConfig.windowPositionTop = Top;
            AppConfig.windowPositionLeft = Left;

            if (tmrOMSI.Enabled)
            {
                saveConfig();
            }
        }
    }

    static class OmsiAddresses
    {
        public const string hour = "base+0x0046176C";     // int (h)
        public const string minute = "base+0x0046176D";   // int (m)
        public const string second = "base+0x00461770";   // float (second.millisecond)
        public const string year = "base+0x00461790";     // int (yyyy)
        public const string month = "base+0x0046178C";    // int (m)
        public const string day = "base+0x00461778";      // int (d)
    }

    static class OmsiTelemetry
    {
        public static bool pluginActive = false;
        public static float busSpeedKph = 0.0f;
        public static byte scheduleActive = 0;
    }

    static class AppConfig
    {
        public static bool alwaysOnTop = AppConfigDefaults.alwaysOnTop;
        public static bool autoSyncOmsiTime = AppConfigDefaults.autoSyncOmsiTime;
        public static bool onlyResyncOmsiTimeIfBehindActualTime = AppConfigDefaults.onlyResyncOmsiTimeIfBehindActualTime;
        public static int offsetHour = AppConfigDefaults.offsetHour;
        public static int offsetHourIndex = AppConfigDefaults.offsetHourIndex;
        public static int windowPositionLeft = AppConfigDefaults.windowPositionLeft;
        public static int windowPositionTop = AppConfigDefaults.windowPositionTop;
        public static int manualSyncHotkeyIndex = AppConfigDefaults.manualSyncHotkeyIndex;
        public static int autoSyncModeIndex = AppConfigDefaults.autoSyncModeIndex;
    }

    static class AppConfigDefaults
    {
        public static bool alwaysOnTop = false;
        public static bool autoSyncOmsiTime = true;
        public static bool onlyResyncOmsiTimeIfBehindActualTime = true;
        public static int offsetHour = 0;
        public static int offsetHourIndex = 23;
        public static int windowPositionLeft = -1;
        public static int windowPositionTop = -1;
        public static int manualSyncHotkeyIndex = 0;
        public static int autoSyncModeIndex = 0;
    }
}
