using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ProcessMonitor
{
    class ProcessMonitor
    {
        static public ProcessMonitor CreateProcessMonitor(int pid)
        {
            Process processById = Process.GetProcessById(1234);
            if (processById == null)
                throw new Exception("No process with pid " + pid);
            return new ProcessMonitor(processById);
        }

        static public ProcessMonitor CreateProcessMonitor(string s)
        {
            Process[] processByName = Process.GetProcessesByName(s);
            if (processByName.Length == 0)
                throw new Exception("No process with name '" + s + "'");
            return new ProcessMonitor(processByName[0]);
        }

        private Process m_process;
        private long m_lastPrivateBytes;
        private long m_lastPrivateBytesDelta;
        private PerformanceCounter m_cpucounter;

        public ProcessMonitor(Process p)
        {
            m_process = p;
            GetPrivateBytes();
            //m_cpucounter = new PerformanceCounter("Processor", "% Processor Time", GetPerformanceCounterProcessName(m_process.Id), true);
        }

        public void Refresh()
        {
            m_process.Refresh();
        }

        public long GetPrivateBytes()
        {
            //Log.WriteLine("PrivateMemorySize64: " + m_process.PrivateMemorySize64);

            var bytes = m_process.PrivateMemorySize64;
            m_lastPrivateBytesDelta = bytes - m_lastPrivateBytes;
            m_lastPrivateBytes = bytes;
            return bytes;
        }

        public string GetName()
        {
            return m_process.ProcessName;
        }

        public long GetPrivateBytesDelta()
        {
            return m_lastPrivateBytesDelta;
        }

        public long GetWorkingSet()
        {
            return m_process.WorkingSet64;
        }

        public float CPU()
        {
            //return m_cpucounter.NextValue() / Environment.ProcessorCount;
            return 1.0f;
        }

        private string GetPerformanceCounterProcessName(int pid)
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
}
