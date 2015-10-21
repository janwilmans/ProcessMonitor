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

            RefreshProcessList();
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

        public void RefreshProcessList()
        {
            m_processes.Clear();
            foreach (var process in Process.GetProcesses())
            {
                ProcessInfo info = new ProcessInfo(process);
                try
                {
                    info.MainModuleFilename = process.MainModule.FileName;
                }
                catch (Exception)
                {
                    //info.MainModuleFilename = "<" + e.Message + ">";
                }
                m_processes.Add(info);
            }
            m_processes.Sort(x => x.Name);
            this.m_processTree.SetObjects(m_processes);
        }
    }

    public class ProcessInfo
    {
        private Process m_process;

        public ProcessInfo(Process process)
        {
            m_process = process;
            UpdateValues();
        }

        public void Refresh()
        {
            m_process.Refresh();
            UpdateValues();
        }

        private void UpdateValues()
        {
            Name = m_process.ProcessName;
            PID = m_process.Id;
            //Owner = process.GetOwner();  // quite slow
            PrivateBytes = m_process.PrivateMemorySize64;
            Threads = m_process.Threads.Count;
            Handles = m_process.HandleCount;

            try
            {
                MainModuleFilename = m_process.MainModule.FileName;
            }
            catch (Exception)
            {
                //info.MainModuleFilename = "<" + e.Message + ">";
            }
        }

        public string Name { get; set; }
        public int PID { get; set; }
        public long PrivateBytes { get; set; }
        public string Memory { get { return Util.FormatBytes4(PrivateBytes); } }
        //public string Owner { get; set; }
        public long Threads { get; set; }
        public long Handles { get; set; }
        public string MainModuleFilename { get; set; }
    }

}
