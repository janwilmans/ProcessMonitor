namespace ProcessMonitor
{
    partial class ProcessTree
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
            this.components = new System.ComponentModel.Container();
            this.m_processTree = new BrightIdeasSoftware.TreeListView();
            this.olvProcess = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPrivateBytes = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvThreads = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvHandles = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPath = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.m_statusStrip = new System.Windows.Forms.StatusStrip();
            this.m_toolstrip = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.m_processTree)).BeginInit();
            this.m_statusStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_processTree
            // 
            this.m_processTree.AllColumns.Add(this.olvProcess);
            this.m_processTree.AllColumns.Add(this.olvPID);
            this.m_processTree.AllColumns.Add(this.olvPrivateBytes);
            this.m_processTree.AllColumns.Add(this.olvThreads);
            this.m_processTree.AllColumns.Add(this.olvHandles);
            this.m_processTree.AllColumns.Add(this.olvPath);
            this.m_processTree.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvProcess,
            this.olvPID,
            this.olvPrivateBytes,
            this.olvThreads,
            this.olvHandles,
            this.olvPath});
            this.m_processTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_processTree.Location = new System.Drawing.Point(0, 0);
            this.m_processTree.Name = "m_processTree";
            this.m_processTree.OwnerDraw = true;
            this.m_processTree.ShowGroups = false;
            this.m_processTree.Size = new System.Drawing.Size(756, 519);
            this.m_processTree.TabIndex = 0;
            this.m_processTree.UseCompatibleStateImageBehavior = false;
            this.m_processTree.View = System.Windows.Forms.View.Details;
            this.m_processTree.VirtualMode = true;
            // 
            // olvProcess
            // 
            this.olvProcess.AspectName = "Value.Name";
            this.olvProcess.Text = "Process";
            this.olvProcess.Width = 250;
            // 
            // olvPID
            // 
            this.olvPID.AspectName = "Value.PID";
            this.olvPID.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvPID.Text = "PID";
            this.olvPID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // olvPrivateBytes
            // 
            this.olvPrivateBytes.AspectName = "Value.Memory";
            this.olvPrivateBytes.AspectToStringFormat = "";
            this.olvPrivateBytes.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvPrivateBytes.Text = "Memory";
            this.olvPrivateBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvPrivateBytes.Width = 136;
            // 
            // olvThreads
            // 
            this.olvThreads.AspectName = "Value.Threads";
            this.olvThreads.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvThreads.Text = "Threads";
            this.olvThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // olvHandles
            // 
            this.olvHandles.AspectName = "Value.Handles";
            this.olvHandles.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvHandles.Text = "Handles";
            this.olvHandles.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // olvPath
            // 
            this.olvPath.AspectName = "Value.MainModuleFilename";
            this.olvPath.FillsFreeSpace = true;
            this.olvPath.Text = "Path";
            // 
            // m_statusStrip
            // 
            this.m_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_toolstrip});
            this.m_statusStrip.Location = new System.Drawing.Point(0, 519);
            this.m_statusStrip.Name = "m_statusStrip";
            this.m_statusStrip.Size = new System.Drawing.Size(756, 22);
            this.m_statusStrip.TabIndex = 1;
            // 
            // m_toolstrip
            // 
            this.m_toolstrip.Name = "m_toolstrip";
            this.m_toolstrip.Size = new System.Drawing.Size(118, 17);
            this.m_toolstrip.Text = "toolStripStatusLabel1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.m_processTree);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(756, 519);
            this.panel1.TabIndex = 2;
            // 
            // ProcessTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 541);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.m_statusStrip);
            this.Name = "ProcessTree";
            this.Text = "Process Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.m_processTree)).EndInit();
            this.m_statusStrip.ResumeLayout(false);
            this.m_statusStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.TreeListView m_processTree;
        private BrightIdeasSoftware.OLVColumn olvProcess;
        private BrightIdeasSoftware.OLVColumn olvPrivateBytes;
        private BrightIdeasSoftware.OLVColumn olvThreads;
        private BrightIdeasSoftware.OLVColumn olvHandles;
        private BrightIdeasSoftware.OLVColumn olvPath;
        private System.Windows.Forms.StatusStrip m_statusStrip;
        private BrightIdeasSoftware.OLVColumn olvPID;
        private System.Windows.Forms.ToolStripStatusLabel m_toolstrip;
        private System.Windows.Forms.Panel panel1;



    }
}

