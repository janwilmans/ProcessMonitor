using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace ProcessMonitor
{
    public partial class MainForm : Form
    {
        private List<Process> m_processes = new List<Process>();
        public MainForm()
        {
            InitializeComponent();
            //ProcessProperties props = new ProcessProperties();
            //props.Show();

            //this.m_processTree.CanExpandGetter = delegate(object x)
            //{
            //    return false;
            //};

            //this.m_processTree.ChildrenGetter = delegate(object x)
            //{
            //    return new ArrayList();
            //};

            RefreshProcessList();
        }

        public void RefreshProcessList()
        {
            m_processes.Clear();
            m_processes.AddRange(Process.GetProcesses());

            m_processes.Sort(x => x.ProcessName);
            this.m_processTree.SetObjects(m_processes);
        }

        //private static string GetCommandLine(this Process process)
        //{
        //    var commandLine = new StringBuilder(process.MainModule.FileName);

        //    commandLine.Append(" ");
        //    using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
        //    {
        //        foreach (var @object in searcher.Get())
        //        {
        //            commandLine.Append(@object["CommandLine"]);
        //            commandLine.Append(" ");
        //        }
        //    }

        //    return commandLine.ToString();
        //}



    }
}
