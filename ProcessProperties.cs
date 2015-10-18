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
        private ProcessMonitor m_monitor;
        private bool m_end;
        private AutoResetEvent m_event = new AutoResetEvent(false);
        private static long viewwidth = 180; // show three minutes of data
        private double m_maximumY;
        private double m_minimumY;
        private double m_lastY;
        private long m_zoomLevel = 1;
        private bool m_userzoom = false;
        private TextAnnotation m_annotation = new TextAnnotation();

        public ProcessProperties()
        {
            InitializeComponent();
            chartCPU.Series[0].Name = "CPU";
            chartPrivateBytes.Series[0].Name = "PrivateBytes";
            chartPrivateBytes.Annotations.Add(m_annotation);

            // horizontal 
            var axisX = chartPrivateBytes.ChartAreas[0].AxisX;
            axisX.MajorGrid.Enabled = true;
            axisX.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            axisX.Interval = 10.0;
            axisX.MajorTickMark.Enabled = true;
            axisX.MajorTickMark.Interval = 60;
            axisX.MajorTickMark.Size = 3;
            axisX.LabelStyle.Enabled = true;
            axisX.LabelStyle.Format = "RelativeTime";
            axisX.LabelStyle.Interval = 60;

            // verical 
            var axisY = chartPrivateBytes.ChartAreas[0].AxisY;
            axisY.MajorGrid.Enabled = true;
            axisY.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            axisY.LabelStyle.Enabled = true;
            axisY.MajorTickMark.Enabled = false;

            axisY.LabelStyle.Format = "FormatBytes";
            chartPrivateBytes.FormatNumber += OnFormatNumberEvent;

            // events
            Closed += OnClosedEvent;

            chartPrivateBytes.MouseClick += OnMouseClickEvent;
            chartPrivateBytes.MouseDoubleClick += OnMouseDoubleClickEvent;
            this.MouseWheel += new MouseEventHandler(OnMouseWheel);

            m_monitor = ProcessMonitor.CreateProcessMonitor("DebugView++");
            m_thread = new Thread(() => MonitoringLoop("foo"));
            m_thread.IsBackground = true;
            m_end = false;
            m_thread.Start();
        }

        private void OnClosedEvent(object sender, EventArgs e)
        {
            ClearTooltips();
            m_end = true;
            m_event.Set();
            m_thread.Join();
        }

        private void LogMemoryDeltas()
        {
            var delta = m_monitor.GetPrivateBytesDelta();
            if (delta > 0)
            {
                Log.WriteLine("[" + m_monitor.GetName() + "] allocated " + Util.FormatBytes3(delta));
            }
            else if (delta < 0)
            {
                Log.WriteLine("[" + m_monitor.GetName() + "] released " + Util.FormatBytes3(delta));
            }
        }

        public void MonitoringLoop(string processname)
        {
            WaitHandle waitHandle = new AutoResetEvent(false);

            m_minimumY = m_monitor.GetPrivateBytes();
            m_maximumY = m_minimumY;

            int seconds = 0;
            try
            {
                for (; !m_end; )
                {
                    m_event.WaitOne(1000);
                    m_monitor.Refresh();
                    var bytes = m_monitor.GetPrivateBytes();
                    LogMemoryDeltas();
                    var s = seconds;    // keep s local to the lambda
                    this.UIThreadAsync(() => ScrollChart(s, bytes));
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
            m_annotation.Text = "Private Bytes: " + Util.FormatBytes3((long)bytes);
            m_annotation.X = 10;
            m_annotation.Y = 5;

            var serie = chartPrivateBytes.Series[0];
            Update(serie.Points, seconds, bytes);

            var area = chartPrivateBytes.ChartAreas[0];
            // 'scroll' the view 1 position to the left
            area.AxisX.Maximum = seconds;
            area.AxisX.Minimum = area.AxisX.Maximum - viewwidth;
        }

        private bool UserZoom(double delta)
        {
            // e.Delta returns -120 (down) / + 120 (up)
            if (delta > 0.0)
            {
                m_zoomLevel = m_zoomLevel + 1;
            }
            else
            {
                m_zoomLevel = Math.Max(1, m_zoomLevel - 1);
            }
            m_userzoom = (m_zoomLevel > 1);
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
            Log.WriteLine("m_zoomLevel: " + m_zoomLevel);
            var axisY = chartPrivateBytes.ChartAreas[0].AxisY;
            if (UserZoom(e.Delta))
            {
                double perc = ConvertZoomLevelToPercentage(m_zoomLevel);
                var last = m_lastY; //.RoundUp((long)m_lastY, 8 * 1024);
                var min = last * (1.0 - perc);
                var max = last * (1.0 + perc);
                SetYRange(min, max);
                //axisY.LabelStyle.Format = "RelativeBytes";
            }
            else
            {
                AutoScaleY(m_lastY);
                //axisY.LabelStyle.Format = "FormatBytes";
            }
        }

        private void SetYRange(double minValue, double maxValue)
        {
            Log.WriteLine("SetYRange: " + minValue + ", " + maxValue);

            var area = chartPrivateBytes.ChartAreas[0];
            area.AxisY.Minimum = (long)minValue;
            area.AxisY.Maximum = (long)maxValue;
            var interval = (area.AxisY.Maximum - area.AxisY.Minimum) / 4;
            chartPrivateBytes.ChartAreas[0].AxisY.Interval = interval;
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
            SetYRange(0, Util.NextPow2( (long) (m_maximumY * 1.25))); // at least +25% at the top
        }

        private List<ToolTip> tooltips = new List<ToolTip>();
        private void ClearTooltips()
        {
            foreach (var tooltip in tooltips)
                tooltip.RemoveAll();
            tooltips.Clear();
        }

        private void OnMouseClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ClearTooltips();
            }
        }

        private void OnMouseDoubleClickEvent(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            var clickPosition = pos;
            var area = chartPrivateBytes.ChartAreas[0];
            var xVal = area.AxisX.PixelPositionToValue(pos.X);
            var yVal = area.AxisY.PixelPositionToValue(pos.Y);

            // todo: lookup nearest real point on the graph and put a marker on it!

            ToolTip newtooltip = new ToolTip();
            newtooltip.Show(Util.FormatBytes3((long)yVal), chartPrivateBytes, e.Location.X, e.Location.Y - 15);
            tooltips.Add(newtooltip);
        }

        private void OnFormatNumberEvent(object sender, FormatNumberEventArgs e)
        {
            if (e.ElementType == ChartElementType.AxisLabels)
            {
                if (e.Format == "FormatBytes")
                {
                    e.LocalizedValue = Util.FormatBytes2((long)e.Value);
                }
                else if (e.Format == "RelativeTime")
                {
                    var position = (long)e.Value;
                    var relativePos = position - chartPrivateBytes.ChartAreas[0].AxisX.Minimum - viewwidth;
                    e.LocalizedValue = string.Format("{0}", relativePos);
                }
                else if (e.Format == "RelativeBytes")
                {
                    var height = chartPrivateBytes.ChartAreas[0].AxisY.Maximum - chartPrivateBytes.ChartAreas[0].AxisY.Minimum;
                    long position = (long) Math.Floor(e.Value);
                    long relativePos = (long) Math.Floor(position - m_lastY);
                    var bytes = Util.FormatBytes2(relativePos);
                    if (relativePos == 0)
                    {
                        e.LocalizedValue = Util.FormatBytes2(position);
                    }
                    else if (relativePos > 0)
                    {
                        e.LocalizedValue = string.Format("+{0}", bytes);
                    }
                    else
                    {
                        e.LocalizedValue = string.Format("-{0}", bytes);
                    }
                }
            }
        }

    }
}
