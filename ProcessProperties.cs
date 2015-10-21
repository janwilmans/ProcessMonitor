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
        private static long viewwidth = 180; // show three minutes of data
        private double m_maximumY;
        private double m_minimumY;
        private double m_lastY;
        private long m_zoomLevel = 1;
        private bool m_userzoom = false;
        private TextAnnotation m_annotation = new TextAnnotation();
        private bool m_cursorEnabled = false;

        private void InitChart(Chart c)
        {
            // horizontal 
            var axisX = c.ChartAreas[0].AxisX;
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
            var axisY = c.ChartAreas[0].AxisY;
            axisY.MajorGrid.Enabled = true;
            axisY.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            axisY.LabelStyle.Enabled = true;
            axisY.MajorTickMark.Enabled = false;

            c.FormatNumber += OnFormatNumberEvent;

            //c.Series[0].ToolTip = "#VALX, #VALY";
            // https://bitbucket.org/grumly57/mschartfriend
        }

        public ProcessProperties(ProcessInfo info)
        {
            InitializeComponent();
            chartPrivateBytes.Series[0].Name = "PrivateBytes";
            //chartPrivateBytes.Annotations.Add(m_annotation);
            this.Text = "Monitor [ " + info.Name + " ]";
            m_status.Text = "Process: " + info.Name + ", PID: " + info.PID;

            InitChart(chartPrivateBytes);
            InitChart(chartHandles);

            chartPrivateBytes.ChartAreas[0].AxisY.LabelStyle.Format = "FormatBytes";

            // events
            Closed += OnClosedEvent;
            MouseWheel += new MouseEventHandler(OnMouseWheel);
            
            chartPrivateBytes.MouseClick += OnMouseClickEvent;
            chartPrivateBytes.MouseDoubleClick += OnMouseDoubleClickEvent;
            chartPrivateBytes.MouseMove += OnMouseMoveEvent;
            
            m_process = ProcessMonitor.CreateProcessMonitor(info.PID);
            m_thread = new Thread(() => MonitoringLoop());
            m_thread.IsBackground = true;
            m_end = false;
            m_thread.Start();
        }

        private void OnMouseMoveEvent(object sender, MouseEventArgs e)
        {
            var area = chartPrivateBytes.ChartAreas[0];
            if (m_cursorEnabled)
            {
                try
                {
                    //HitTestResult result = chartPrivateBytes.HitTest(e.X, e.Y);
                    var pos = (int)area.AxisX.PixelPositionToValue(e.X);
                    var bytes = (long)chartPrivateBytes.Series[0].Points[pos].YValues[0];
                    //chartPrivateBytes.ChartAreas[0].CursorY.SetCursorPixelPosition(p, true);
                    chartPrivateBytes.Series[0].ToolTip = Util.FormatBytes4(bytes);
                }
                catch (Exception)
                {
                    m_cursorEnabled = false;            
                }
            }

            if (m_cursorEnabled)
            {
                System.Drawing.Point p = new System.Drawing.Point(e.X, e.Y);
                area.CursorX.SetCursorPixelPosition(p, true);
            }
            else
            {
                System.Drawing.Point p = new System.Drawing.Point(0, 0);
                area.CursorX.SetCursorPixelPosition(p, true);
            }
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

        public void MonitoringLoop()
        {
            WaitHandle waitHandle = new AutoResetEvent(false);
            m_minimumY = m_process.Info.PrivateBytes;
            m_maximumY = m_minimumY;

            int seconds = 0;
            try
            {
                for (; !m_end; )
                {
                    m_process.Refresh();
                    LogMemoryDeltas();
                    var s = seconds;    // keep s local to the lambda
                    this.UIThreadAsync(() => ScrollChart(s));
                    seconds = seconds + 1;
                    m_event.WaitOne(1000);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                m_end = true;
            }
        }

        private void ScrollChart(double seconds)
        {
            if (m_end) return;

            {
                var bytes = m_process.Info.PrivateBytes;
                var serie = chartPrivateBytes.Series[0];
                Update(serie.Points, seconds, bytes);
                var area = chartPrivateBytes.ChartAreas[0];
                // 'scroll' the view 1 position to the left
                area.AxisX.Maximum = seconds;
                area.AxisX.Minimum = area.AxisX.Maximum - viewwidth;
                serie.Name = "Private Bytes: " + Util.FormatBytes4((long)bytes);
            }

            { 
                var handles = m_process.Info.Handles;
                var serie2 = chartHandles.Series[0];
                Update(serie2.Points, seconds, handles);
                var area2 = chartHandles.ChartAreas[0];
                // 'scroll' the view 1 position to the left
                area2.AxisX.Maximum = seconds;
                area2.AxisX.Minimum = area2.AxisX.Maximum - viewwidth;
                serie2.Name = "Handles: " + handles;
            }
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
                axisY.LabelStyle.Format = "FormatBytes";
            }
        }

        private void SetYRange(double minValue, double maxValue)
        {
            var area = chartPrivateBytes.ChartAreas[0];
            area.AxisY.Minimum = (long)minValue;
            area.AxisY.Maximum = (long)maxValue;
            var interval = (area.AxisY.Maximum - area.AxisY.Minimum) / 4;
            var powInterval = Util.NextPow2((long) interval);
            chartPrivateBytes.ChartAreas[0].AxisY.Interval = powInterval;
            //chartPrivateBytes.ChartAreas[0].AxisY.IntervalOffset = 2 * powInterval;
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                m_cursorEnabled = !m_cursorEnabled;
            }
        }

        private void OnMouseDoubleClickEvent(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            var clickPosition = pos;
            var area = chartPrivateBytes.ChartAreas[0];
            var xVal = area.AxisX.PixelPositionToValue(pos.X);
            var yVal = area.AxisY.PixelPositionToValue(pos.Y);

            var datapoint = chartPrivateBytes.Series[0].Points[(int)xVal]; // nearest point on the graph
            //datapoint.MarkerStyle = MarkerStyle.Circle;
            //datapoint.MarkerSize = 5;
            //datapoint.MarkerColor = System.Drawing.Color.Red;
            var bytes = (long)datapoint.YValues[0];

            //TextAnnotation a = new TextAnnotation();
            //a.BackColor = 
            //a.Text = Util.FormatBytes3(bytes);
            //a.AnchorDataPoint = datapoint;
            //a.AnchorAlignment = ContentAlignment.BottomRight;
            //a.SmartLabelStyle.Enabled = false;
            //chartPrivateBytes.Annotations.Add(a);

            ToolTip newtooltip = new ToolTip();
            newtooltip.Show(Util.FormatBytes3(bytes), chartPrivateBytes, e.Location.X, e.Location.Y - 15);
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
                    long position = (long) (e.Value);
                    long relativePos = (long) (position - m_lastY);
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
