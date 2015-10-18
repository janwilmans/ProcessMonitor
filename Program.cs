using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;

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

    public static class Util
    {
        public static long RoundUp(long numToRound, long multiple)
        {
            if (multiple == 0)
            {
                return numToRound;
            }

            long remainder = numToRound % multiple;
            if (remainder == 0)
                return numToRound;
            return numToRound + multiple - remainder;
        }

        public static long NextPow2(long value)
        {
            return (long)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));

        }

        public static readonly ReadOnlyCollection<string> units = new ReadOnlyCollection<string>(
          new string[] { "bytes", "kB", "MB", "GB", "TB", "PB", "EB", null });

        public static string FormatBytes(long bytes)
        {
            long absValue = Math.Abs(bytes);
            const int kb = 1024;
            int index = 0;

            while ((absValue / kb) > 0 && units[index] != null)
            {
                absValue = absValue / kb;
                ++index;
            }
            return string.Format("{0} {1}", absValue, units[index]);
        }

        public static string FormatBytes2(long bytes)
        {
            long absValue = Math.Abs(bytes);
            const int kb = 1024;
            const int treshold = 16;
            int index = 0;

            while ((absValue / kb) >= treshold && units[index] != null)
            {
                absValue = absValue / kb;
                ++index;
            }
            return string.Format("{0} {1}", absValue, units[index]);
        }

        public static string FormatBytes3(long bytes)
        {
            long absValue = Math.Abs(bytes);
            const int kb = 1024;
            const int treshold = 64;
            int index = 0;

            while ((absValue / kb) >= treshold && units[index] != null)
            {
                absValue = absValue / kb;
                ++index;
            }
            return string.Format("{0} {1}", absValue, units[index]);
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
