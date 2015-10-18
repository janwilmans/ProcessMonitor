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
