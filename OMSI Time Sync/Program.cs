using System;
using System.Threading;
using System.Windows.Forms;

namespace OMSI_Time_Sync
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "{8E846A9C-8972-4349-B67E-F5333CFE4030}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMain());
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                MessageBox.Show("ERROR: OMSI Time Sync is already running!", "OMSI Time Sync", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
