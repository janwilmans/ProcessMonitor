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
using BrightIdeasSoftware;

namespace ProcessMonitor
{
    public partial class ProcessTree : Form
    {
        private List<ProcessInfo> m_processes = new List<ProcessInfo>();
        private TypedObjectListView<ProcessInfo> m_typedProcessTree;
        private AutoResetEvent m_event = new AutoResetEvent(false); 
        private Thread m_thread;
        private bool m_end;

        public ProcessTree()
        {
            InitializeComponent();
            m_typedProcessTree = new TypedObjectListView<ProcessInfo>(m_processTree);

            m_toolstrip.Text = "Running as administator in 64-bit mode";
            if (!Util.IsRunningAsAdministrator())
            {
                m_toolstrip.Text = "Notice: Not running as admin may result in lack of details";
            }

            m_processTree.FullRowSelect = true;
            m_processTree.RowFormatter = delegate(OLVListItem olvi)
            {
                ProcessInfo info = (ProcessInfo)olvi.RowObject;
                olvi.UseItemStyleForSubItems = false;
                if (info.PrivateBytes > 100*1024*1024)
                {
                    olvi.SubItems[2].ForeColor = Color.Red;
                }
            };

            KeyPreview = true;
            KeyDown += OnKeyDownEvent;

            m_processTree.DoubleClick += OnDoubleClickEvent;
            
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

            InitProcessList();
            m_thread = new Thread(() => MonitoringLoop());
            m_thread.IsBackground = true;
            m_end = false;
            m_thread.Start();
        }

        public void MonitoringLoop()
        {
            WaitHandle waitHandle = new AutoResetEvent(false);
            try
            {
                for (; !m_end; )
                {
                    RefreshProcessList();
                    m_event.WaitOne(1000);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                m_end = true;
            }
        }

        private void OnDoubleClickEvent(object sender, EventArgs e)
        {
            ProcessInfo info = m_processTree.GetItem(m_processTree.SelectedIndex).RowObject as ProcessInfo;
            ProcessProperties props = new ProcessProperties(info);
            props.Show(this);
        }

        private void OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                RefreshProcessList();
            }
        }

        public void InitProcessList()
        {
            m_processes.Clear();
            foreach (var process in Process.GetProcesses())
            {
                ProcessInfo info = new ProcessInfo(process);
                m_processes.Add(info);
            }
            m_processes = m_processes.OrderBy(x => x.Name).ToList();
            this.m_processTree.SetObjects(m_processes);
        }

        public void RefreshProcessList()
        {
            //Stopwatch st = Stopwatch.StartNew();

            List<ProcessInfo> processes = new List<ProcessInfo>();
            Dictionary<int, Process> processMap = new Dictionary<int,Process>();
            foreach (var process in Process.GetProcesses())
            {
                processMap.Add(process.Id, process);
            }

            foreach (var process in m_processes)
            {
                if (processMap.Remove(process.PID))
                {
                    processes.Add(process); // still alive 
                }
            }

            foreach (var process in processes)
            {
                process.Refresh();
            }

            foreach (var entry in processMap)
            {
                processes.Add(new ProcessInfo(entry.Value));
            }

            m_processes = processes;
            m_processes = m_processes.OrderBy(x => x.Name).ToList();    // stable sort
            this.m_processTree.SetObjects(m_processes);
            //st.Stop();
            //Log.WriteLine("RefreshProcessList took " + st.Elapsed.TotalMilliseconds + " ms ");
        }

    }

    public class ProcessInfo
    {
        private Process m_process;

        public ProcessInfo(Process process)
        {
            m_process = process;
            InitValues();
        }

        public void Refresh()
        {
            m_process.Refresh();
            UpdateValues();
        }

        private void InitValues()
        {
            Name = m_process.ProcessName;
            PID = m_process.Id;
            //Owner = m_process.GetOwner(); // takes ~5 seconds!?

            try
            {
                MainModuleFilename = m_process.MainModule.FileName;
            }
            catch (Exception)
            {
                //info.MainModuleFilename = "<" + e.Message + ">";
            }
            UpdateValues();
        }


        private void UpdateValues()
        {
            PrivateBytes = m_process.PrivateMemorySize64;
            Threads = m_process.Threads.Count;
            Handles = m_process.HandleCount;
        }

        public string Name { get; set; }
        public int PID { get; set; }
        public long PrivateBytes { get; set; }
        public string Memory { get { return Util.FormatBytes4(PrivateBytes); } }
        public string Owner { get; set; }
        public long Threads { get; set; }
        public long Handles { get; set; }
        public string MainModuleFilename { get; set; }
    }

}
