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
            Process processById = Process.GetProcessById(pid);
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
        private ProcessInfo m_info;
        private long m_lastPrivateBytes;
        private long m_lastPrivateBytesDelta;

        public ProcessInfo Info { get { return m_info; } }
        public long PrivateBytesDelta { get { return m_lastPrivateBytesDelta; } }

        private PerformanceCounter m_cpucounter;

        public ProcessMonitor(Process p)
        {
            m_process = p;
            m_info = new ProcessInfo(p);
            UpdateDeltas();
            //m_cpucounter = new PerformanceCounter("Processor", "% Processor Time", GetPerformanceCounterProcessName(m_process.Id), true);
        }

        public void Refresh()
        {
            m_info.Refresh();
            UpdateDeltas();
        }

        private void UpdateDeltas()
        {
            m_lastPrivateBytesDelta = m_info.PrivateBytes - m_lastPrivateBytes;
            m_lastPrivateBytes = m_info.PrivateBytes;
        }

        public float CPU()
        {
            //return m_cpucounter.NextValue() / Environment.ProcessorCount;
            return 1.0f;
        }
    }
}
