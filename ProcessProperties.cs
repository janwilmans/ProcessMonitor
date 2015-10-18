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
using System.Collections.ObjectModel;

namespace ProcessMonitor
{
    public partial class ProcessProperties : Form
    {
        private Thread m_thread;
        private ProcessMonitor m_monitor;
        private bool m_end;
        private AutoResetEvent m_event = new AutoResetEvent(false);
        private static long viewwidth = 200;
        private double m_maximumY;
        private double m_minimumY;
        private double m_lastY;
        private long m_zoomLevel = 1;
        private bool m_userzoom = false;

        public ProcessProperties()
        {
            InitializeComponent();
            chartCPU.Series[0].Name = "CPU";
            chartPrivateBytes.Series[0].Name = "PrivateBytes";

            //chartCPU.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartPrivateBytes.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
            //chartPrivateBytes.ChartAreas[0].AxisX.CustomLabels.Add(10.0, DateTimeIntervalType.Seconds);
            chartPrivateBytes.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            chartPrivateBytes.ChartAreas[0].AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            chartPrivateBytes.ChartAreas[0].AxisX.MajorGrid.Interval = 10.0;
            //chartPrivateBytes.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            //chartPrivateBytes.ChartAreas[0].AxisX.MinorGrid.IntervalType = DateTimeIntervalType.Seconds;

            chartCPU.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0,}K";
            chartPrivateBytes.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0,}K";
            //chartPrivateBytes.ChartAreas[0].AxisX.CustomLabels.Add(1, 3, "Category 1", 1, LabelMarkStyle.LineSideMark);

            // events
            Closed += OnClosedEvent;
            //this.chartPrivateBytes.AxisViewChanged += chartPrivateBytes_AxisViewChanged;

            this.MouseWheel += new MouseEventHandler(OnMouseWheel);

            m_monitor = ProcessMonitor.CreateProcessMonitor("DebugView++");
            m_thread = new Thread(() => MonitoringLoop("foo"));
            m_thread.IsBackground = true;
            m_end = false;
            m_thread.Start();
        }

        private void OnClosedEvent(object sender, EventArgs e)
        {
            m_end = true;
            m_event.Set();
            m_thread.Join();
        }

        public static readonly ReadOnlyCollection<string> units = new ReadOnlyCollection<string>(
          new string[] { "bytes", "kB", "MB", "GB", "TB", "PB", "EB", null } );
    
        private string FormatBytes(long bytes)
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

        private string FormatBytes2(long bytes)
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

        private void testNumber(long number)
        {
            Log.WriteLine(string.Format("{0} = {1} or {2}", number, FormatBytes(number), FormatBytes2(number)));
        }

        private void LogMemoryDeltas()
        {
            var delta = m_monitor.GetPrivateBytesDelta();
            if (delta > 0)
            {
                Log.WriteLine("[" + m_monitor.GetName() + "] allocated " + FormatBytes2(delta));
            }
            else if (delta < 0)
            {
                Log.WriteLine("[" + m_monitor.GetName() + "] released " + FormatBytes2(delta));
            }
        }

        public void MonitoringLoop(string processname)
        {
            WaitHandle waitHandle = new AutoResetEvent(false);

            int seconds = 0;
            try
            {
                for (; !m_end; )
                {
                    m_event.WaitOne(1000);
                    m_monitor.Refresh();
                    var bytes = m_monitor.GetPrivateBytes();
                    LogMemoryDeltas();
                    this.UIThreadAsync(() => ScrollChart(seconds, bytes));
                    seconds = seconds + 1;
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                m_end = true;
            }
        }

        private void ScrollChart(double seconds, double bytes)
        {
            if (m_end) return;
            var serie = this.chartPrivateBytes.Series[0];
            Update(serie.Points, seconds, bytes);

            var area = this.chartPrivateBytes.ChartAreas[0];

            // 'scroll' the view 1 position to the left
            area.AxisX.Maximum = Math.Min(serie.Points.Count, viewwidth);
            area.AxisX.Minimum = area.AxisX.Maximum - viewwidth;
        }

        private bool UserZoom(double delta)
        {
            // e.Delta returns -120 (down) / + 120 (up)
            if (delta > 0.0)
            {
                m_zoomLevel = m_zoomLevel + 1;
                m_userzoom = true;
            }
            else
            {
                m_zoomLevel = m_zoomLevel - 1;
                m_userzoom = true;
            }
            if (m_zoomLevel <= 0)
            {
                m_zoomLevel = 1;
                m_userzoom = false;
            }

            if (!m_userzoom)
            {
                AutoScaleY(m_lastY);
            }
            return m_userzoom;
        }

        private double ConvertZoomLevelToPercentage(long zoomLevel)
        {
            // this starts the user zoom off at +90% / -90% 
            // moves in 10 steps to +10% / -10% 
            // continuing in finer steps of 1/(N-9)
            double perc = 1.0;
            if (zoomLevel < 10)
            {
                perc = (10 - (zoomLevel - 1)) / 10.0;
            }
            else
            {
                perc = 1.0 / ((zoomLevel - 9) * 10.0);
            }
            return perc;
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (UserZoom(e.Delta))
            {
                var area = this.chartPrivateBytes.ChartAreas[0];
                double perc = ConvertZoomLevelToPercentage(m_zoomLevel);
                area.AxisY.Minimum = Math.Max(0, m_lastY * (1.0 - perc));
                area.AxisY.Maximum = m_lastY * (1.0 + perc);
            }
        }

        private void Update(DataPointCollection points, double position, double value)
        {
            if (points.Count > viewwidth)
            {
                points.RemoveAt(0);
            }
            points.AddXY(position, value);
            m_lastY = value;
            AutoScaleY(value);
        }

        private void AutoScaleY(double value)   
        {
            if (m_userzoom) return;
            m_minimumY = Math.Min(m_minimumY, value);
            m_maximumY = Math.Max(m_maximumY, value);
            var area = this.chartPrivateBytes.ChartAreas[0];
            area.AxisY.Minimum = m_minimumY;
            area.AxisY.Maximum = m_maximumY * 1.1; // +10% at the top
        }

        //private void chartPrivateBytes_AxisViewChanged(object sender, ViewEventArgs e)
        //{
        //    Log.WriteLine(" chartPrivateBytes_AxisViewChanged...");

        //    if (e.Axis.AxisName == AxisName.X)
        //    {
        //        int start = (int)e.Axis.ScaleView.ViewMinimum;
        //        int end = (int)e.Axis.ScaleView.ViewMaximum;

        //        foreach (ChartArea area in this.chartPrivateBytes.ChartAreas)
        //        {
        //            List<double> allNumbers = new List<double>();

        //            foreach (Series item in this.chartPrivateBytes.Series)
        //            {
        //                if (item.ChartArea == area.Name)
        //                    allNumbers.AddRange(item.Points.Where((x, i) => i >= start && i <= end).Select(x => x.YValues[0]).ToList());
        //            }

        //            double ymin = allNumbers.Min();
        //            double ymax = allNumbers.Max();

        //            if (ymax > ymin)
        //            {
        //                double offset = 0.02 * (ymax - ymin);
        //                area.AxisY.Maximum = ymax + offset;
        //                area.AxisY.Minimum = ymin - offset;
        //            }
        //        }
        //    }
        //}


    }
}
