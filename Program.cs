﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Security.Principal;
using System.Text;
using System.Diagnostics;
using System.Management;

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

    public static class ProcessExtensions
    {
        public static string GetOwner(this Process process)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + process.Id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }
            return "<not found>";
        }

        public static string GetCommandLine(this Process process)
        {
            var commandLine = new StringBuilder(process.MainModule.FileName);

            commandLine.Append(" ");
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }
    }

    public static class Util
    {

        public static List<WMIProcessInfo> GetWMIProcesesInfo()
        {
            List<WMIProcessInfo> list = new List<WMIProcessInfo>();
            string query = "Select * From Win32_Process";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {

                    WMIProcessInfo info = new WMIProcessInfo();
                    int id = 0;
                    if (Int32.TryParse(obj["ProcessId"].ToString(), out id))
                    {
                        info.Id = id;
                    }

                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        info.Owner = argList[1] + "\\" + argList[0];
                    }

                    var cmdline = obj["CommandLine"];
                    if (cmdline != null)
                    {
                        info.Commandline = cmdline.ToString();
                    }
                
                    list.Add(info);
                    Log.WriteLine(string.Format("id: {0}, Owner: {1}, Cmd: {2} ", info.Id, info.Owner, info.Commandline));
                }
                catch (Exception e)
                {
                    Log.WriteLine(e.ToString());
                }
            }
            return list;
        }

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

        public static string FormatBytes(long bytes)        // shortest notation, like '12 MB'
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

        public static string FormatBytes2(long bytes)       // more accuracy for numbers up to 16 like '12005 kB'
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

        public static string FormatBytes3(long bytes)       // more accuracy for numbers up to 64 like '64005 kB'
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

        public static string FormatBytes4(long bytes)
        {
            long absValue = Math.Abs(bytes);
            double accValue = absValue;
            const int kb = 1024;
            int index = 0;

            while ((absValue / kb) > 0 && units[index] != null)
            {
                absValue = absValue / kb;
                accValue = accValue / kb;
                ++index;
            }
            return string.Format("{0:f2} {1}", accValue, units[index]);
        }

        public static void Sort<T, U>(this List<T> list, Func<T, U> expression)
        where U : IComparable<U>
        {
            list.Sort((x, y) => expression.Invoke(x).CompareTo(expression.Invoke(y)));
        }

        public static bool IsRunningAsAdministrator()
        {
            bool isAdmin = false;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private static string GetPerformanceCounterProcessName(int pid)
        {
            var category = new PerformanceCounterCategory("Process");
            var instances = category.GetInstanceNames();
            foreach (var instance in instances)
            {
                var perf = new PerformanceCounter("Process", "ID Process", instance, true);
                if (perf.RawValue == pid)
                {
                    return instance;
                }
            }
            throw new Exception("No instance for pid " + pid + " found.");
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
            Application.Run(new ProcessTree());
        }
    }
}
