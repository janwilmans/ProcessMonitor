namespace ProcessMonitor
{
    partial class ProcessProperties
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartCPU = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPrivateBytes = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chartCPU)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPrivateBytes)).BeginInit();
            this.SuspendLayout();
            // 
            // chartCPU
            // 
            chartArea3.Name = "ChartArea1";
            this.chartCPU.ChartAreas.Add(chartArea3);
            this.chartCPU.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartCPU.Location = new System.Drawing.Point(0, 0);
            this.chartCPU.Name = "chartCPU";
            this.chartCPU.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series3.Name = "CPU";
            this.chartCPU.Series.Add(series3);
            this.chartCPU.Size = new System.Drawing.Size(784, 129);
            this.chartCPU.TabIndex = 0;
            this.chartCPU.Text = "chart1";
            // 
            // chartPrivateBytes
            // 
            chartArea4.Name = "ChartPrivateBytes";
            this.chartPrivateBytes.ChartAreas.Add(chartArea4);
            this.chartPrivateBytes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPrivateBytes.Location = new System.Drawing.Point(0, 129);
            this.chartPrivateBytes.Name = "chartPrivateBytes";
            this.chartPrivateBytes.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
            series4.ChartArea = "ChartPrivateBytes";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series4.Name = "PrivateBytes";
            this.chartPrivateBytes.Series.Add(series4);
            this.chartPrivateBytes.Size = new System.Drawing.Size(784, 304);
            this.chartPrivateBytes.TabIndex = 1;
            this.chartPrivateBytes.Text = "chart2";
            // 
            // ProcessProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 433);
            this.Controls.Add(this.chartPrivateBytes);
            this.Controls.Add(this.chartCPU);
            this.Name = "ProcessProperties";
            this.Text = "ProcessProperties";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.chartCPU)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPrivateBytes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartCPU;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPrivateBytes;
    }
}