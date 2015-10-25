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
using System.Windows.Forms.DataVisualization.Charting;

namespace ProcessMonitor
{
    public partial class ProcessProperties : Form
    {
        private Thread m_thread;
        private ProcessMonitor m_process;
        private bool m_end;
        private AutoResetEvent m_event = new AutoResetEvent(false);

        private MonitorChart m_privateBytesChart;
        private MonitorChart m_handleChart;

        public ProcessProperties(ProcessInfo info)
        {
            InitializeComponent();

            m_privateBytesChart = new MonitorChart(chartPrivateBytes, "PrivateBytes");
            m_privateBytesChart.FormatValueEvent = ((value) => { return Util.FormatBytes2((long)value); });
            m_privateBytesChart.LabelFormatAuto = "FormatBytes";
            m_privateBytesChart.LabelFormatZoomed = "RelativeBytes";

            m_handleChart = new MonitorChart(chartHandles, "Handle count");
            m_handleChart.FormatValueEvent = ((value) => { return string.Format("{0}", (long)value); });

            this.Text = "Monitor [ " + info.Name + " ]";
            m_status.Text = "Process: " + info.Name + ", PID: " + info.PID;

            // events
            Closed += OnClosedEvent;
            
            m_process = ProcessMonitor.CreateProcessMonitor(info.PID);
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
                    m_process.Refresh();
                    LogMemoryDeltas();
                    this.UIThreadAsync(() => UpdateValues());
                    // todo: we should actually wait for UpdateValues() to return, but then a OnClose waiting for m_thread.Join() deadlocks, find a better solution
                    m_event.WaitOne(1000);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                m_end = true;
            }
        }

        private void UpdateValues()
        {
            m_privateBytesChart.AddValue(m_process.Info.PrivateBytes);
            m_handleChart.AddValue(m_process.Info.Handles);
        }

        private void OnClosedEvent(object sender, EventArgs e)
        {
            m_end = true;
            m_event.Set();
            m_thread.Join();
        }

        private void LogMemoryDeltas()
        {
            var delta = m_process.PrivateBytesDelta;
            if (delta > 0)
            {
                Log.WriteLine("[" + m_process.Info.Name + "] allocated " + Util.FormatBytes3(delta));
            }
            else if (delta < 0)
            {
                Log.WriteLine("[" + m_process.Info.Name + "] released " + Util.FormatBytes3(delta));
            }
        }

    }
}
