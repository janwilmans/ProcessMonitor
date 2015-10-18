using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessMonitor
{
    static class Log
    {
        public static void WriteLine(string s)
        {
            System.Diagnostics.Trace.WriteLine(s);
        }
    }

    public static class ControlExtensions
    {
        public static void UIThread(this Control @this, Action code)
        {
            @this.Invoke(code);
        }
        public static void UIThreadAsync(this Control @this, Action code)
        {
            @this.BeginInvoke(code);
        }
    }

    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
