using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProcessMonitor
{
    class MonitorChart
    {
        private static long viewwidth = 180; // show three minutes of data

        private Chart m_chart;
        private double m_newestValue;
        private long m_seconds = 0;
        private double m_maximumY;
        private double m_minimumY;
        private string m_name;
        private bool m_userzoom = false;
        private long m_zoomLevel = 1;

        private ChartArea m_area;
        private Series m_series;
        private DataPointCollection m_points;
        private Axis m_axisX;
        private Axis m_axisY;
        private Dictionary<int, List<Annotation>> m_annotations;
        private LegendItem m_legendItem;

        public delegate string FormatValueEventDelegate(double value);
        public FormatValueEventDelegate FormatValueEvent;
        public string LabelFormatAuto;
        public string LabelFormatZoomed;
        

        public MonitorChart(Chart c, string name)
        {
            m_chart = c;

            // some shortcuts
            m_area = m_chart.ChartAreas[0];
            m_series = m_chart.Series[0];
            m_points = m_series.Points;
            m_axisX = m_area.AxisX;
            m_axisY = m_area.AxisY;

            m_annotations = new Dictionary<int, List<Annotation>>();
            m_name = name;
            Init();
        }

        ~MonitorChart()
        {
            ClearTooltips();
        }

        private void Init()
        {
            // horizontal 
            m_axisX.MajorGrid.Enabled = true;
            m_axisX.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            m_axisX.Interval = 10.0;
            m_axisX.MajorTickMark.Enabled = true;
            m_axisX.MajorTickMark.Interval = 60;
            m_axisX.MajorTickMark.Size = 3;
            m_axisX.LabelStyle.Enabled = true;
            m_axisX.LabelStyle.Format = "RelativeTime";
            m_axisX.LabelStyle.Interval = 60;

            // verical 
            m_axisY.MajorGrid.Enabled = true;
            m_axisY.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            m_axisY.LabelStyle.Enabled = true;
            m_axisY.MajorTickMark.Enabled = false;
            m_axisY.LabelStyle.Format = LabelFormatAuto;

            m_chart.FormatNumber += OnFormatNumberEvent;
            m_chart.MouseWheel += OnMouseWheelEvent;
            m_chart.MouseEnter += OnMouseEnterEvent;
            m_chart.MouseLeave += OnMouseLeaveEvent;

            m_chart.MouseClick += OnMouseClickEvent;
            m_chart.MouseDoubleClick += OnMouseDoubleClickEvent;
            m_chart.MouseMove += OnMouseMoveEvent;

            m_chart.ApplyPaletteColors();

            m_chart.Legends.Clear();
            Legend legend = new Legend();
            m_chart.Legends.Add(legend);
            //legend.BackGradientStyle = GradientStyle.TopBottom;
            //legend.BackColor = Color.Red;
            //legend.BorderColor = Color.Black;
            legend.Docking = Docking.Top;
            
            m_legendItem = new LegendItem();
            m_legendItem.ImageStyle = LegendImageStyle.Line;
            m_legendItem.BorderWidth = 2;
            m_legendItem.Color = m_chart.Series[0].Color;
            m_legendItem.Name = m_name;
            legend.CustomItems.Add(m_legendItem);

            //c.Series[0].ToolTip = "#VALX, #VALY";
            // https://bitbucket.org/grumly57/mschartfriend
        }

        public void AddValue(double value)
        {
            var serie = m_chart.Series[0];

            if (m_points.Count > viewwidth)
            {
                if (m_points.Count > 0)
                {
                    RemoveMarkers((int)m_points[0].XValue);
                    m_points.RemoveAt(0);
                }
            }
            m_points.AddXY(m_seconds, value);
            m_newestValue = value;

            // 'scroll' the view 1 position to the left
            m_axisX.Maximum = m_seconds;
            m_axisX.Minimum = m_axisX.Maximum - viewwidth;

            // changing the name of m_chart.Series[0].Name very quickly causes exceptions, appearently the name is internally used as a key.
            // so instead, we are using a custom legend.
            m_legendItem.Name = m_name + ": " + FormatValueEvent(value);
            m_minimumY = Math.Min(m_minimumY, value);
            m_maximumY = Math.Max(m_maximumY, value);
            m_seconds = m_seconds + 1;
            RecalculateYScale();
        }

        private void RecalculateYScale()
        {
            if (m_userzoom)
            {
                DoUserZoomStrategy2();
            }
            else
            {
                SetAutoRange(0, Util.NextPow2((long)(m_maximumY * 1.25))); // at least +25% at the top
                m_axisY.LabelStyle.Format = LabelFormatAuto;
            }
        }

        private void SetAutoRange(double minValue, double maxValue)
        {
            m_axisY.Minimum = (long)minValue;
            m_axisY.Maximum = (long)maxValue;
            var interval = (m_axisY.Maximum - m_axisY.Minimum) / 4;
            var powInterval = Util.NextPow2((long)interval);
            m_axisY.Interval = powInterval;
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

        private void DoUserZoomStrategy1()
        {
            double perc = ConvertZoomLevelToPercentage(m_zoomLevel);
            var last = m_newestValue;
            var min = last * (1.0 - perc);
            var max = last * (1.0 + perc);
            SetAutoRange(min, max);
        }

        private void DoUserZoomStrategy2()
        {
            var last = (long)m_newestValue;
            var zoombase = Util.NextPow2(last) / 2;
            var spacing = zoombase / Math.Pow(2, m_zoomLevel);
            spacing = Math.Max(spacing, 2);

            m_axisY.Minimum = last - (2 * spacing);
            m_axisY.Maximum = last + (2 * spacing);
            m_axisY.Interval = spacing;
            m_axisY.IntervalOffset = -1 * (spacing / 2);
            m_axisY.LabelStyle.Format = LabelFormatZoomed;
        }

        private List<ToolTip> tooltips = new List<ToolTip>();
        private void ClearTooltips()
        {
            foreach (var tooltip in tooltips)
                tooltip.RemoveAll();
            tooltips.Clear();
        }

        private void EnableCursor(int X, int Y)
        {
            var p = new Point(X, Y);
            m_area.CursorX.SetCursorPixelPosition(p, true);
            m_area.CursorX.LineColor = Color.Red;
        }

        private void DisableCursor()
        {
            m_area.CursorX.IsUserEnabled = false;
            m_area.CursorX.IsUserSelectionEnabled = false;
            m_area.CursorX.LineColor = Color.Transparent;
        }

        void OnMouseLeaveEvent(object sender, EventArgs e)
        {
            if (m_chart.Focused)
                m_chart.Parent.Focus();
            DisableCursor();
        }

        void OnMouseEnterEvent(object sender, EventArgs e)
        {
            if (!m_chart.Focused)
                m_chart.Focus();
        }

        private void OnMouseMoveEvent(object sender, MouseEventArgs e)
        {
            EnableCursor(e.X, e.Y);
        }

        private void OnMouseWheelEvent(object sender, MouseEventArgs e)
        {
            // e.Delta returns -120 (scroll down) or + 120 (scroll up)
            if (e.Delta > 0.0)
            {
                m_zoomLevel = m_zoomLevel + 1;
            }
            else
            {
                m_zoomLevel = Math.Max(1, m_zoomLevel - 1);
            }
            m_userzoom = (m_zoomLevel > 1);
            RecalculateYScale();
        }

        private DataPoint GetNearestPointByX(int x)
        {
            var offset = m_points[0].XValue;
            var valueX = (int) (m_axisX.PixelPositionToValue(x) - offset);
            return m_points[valueX]; // nearest point on the graph
        }

        private void AddMarker(DataPoint dp)
        {
            dp.MarkerStyle = MarkerStyle.Circle;
            dp.MarkerSize = 5;
            dp.MarkerColor = Color.Red;
            var value = (long)dp.YValues[0];

            CalloutAnnotation a = new CalloutAnnotation();
            a.AllowMoving = true;
            a.AllowSelecting = true;
            a.BackColor = Color.BlanchedAlmond;
            a.Text = FormatValueEvent(value);
            a.AnchorDataPoint = dp;
            a.AllowTextEditing = true;
            m_chart.Annotations.Add(a);

            int key = (int)dp.XValue;
            if (!m_annotations.ContainsKey(key))
            {
                m_annotations.Add(key, new List<Annotation>());
            }
            m_annotations[(int)dp.XValue].Add(a);
        }

        private void RemoveMarkers(int x)
        {
            if (m_annotations.ContainsKey(x))
            {
                RemoveMarkers(m_annotations[x]);
                m_annotations.Remove(x);
            }
        }

        private void RemoveMarkers(List<Annotation> list)
        {
            foreach (var a in list)
            {
                m_chart.Annotations.Remove(a);
            }
        }

        private void RemoveMarkers()
        {
            foreach (var p in m_series.Points)
            {
                RemoveMarkers((int)p.XValue);
                p.MarkerStyle = MarkerStyle.None;
            }
            m_annotations.Clear();
        }

        private void OnMouseClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RemoveMarkers();
            }
        }

        private void OnMouseDoubleClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    var datapoint = GetNearestPointByX(e.X);
                    AddMarker(datapoint);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    // ignore negative indices
                }
            }
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
                    var relativePos = position - m_axisX.Minimum - viewwidth;
                    e.LocalizedValue = string.Format("{0}", relativePos);
                }
                else if (e.Format == "RelativeBytes")
                {
                    var height = m_axisY.Maximum - m_axisY.Minimum;
                    long position = (long)(e.Value);
                    long relativePos = (long)(position - m_newestValue);
                    var bytes = Util.FormatBytes(relativePos);
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
