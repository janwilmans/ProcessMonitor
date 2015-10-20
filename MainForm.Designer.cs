namespace ProcessMonitor
{
    partial class MainForm
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
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvRam = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.m_processTree = new BrightIdeasSoftware.TreeListView();
            this.olvThreads = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvHandles = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPath = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.m_processTree)).BeginInit();
            this.SuspendLayout();
            // 
            // olvName
            // 
            this.olvName.AspectName = "ProcessName";
            this.olvName.Text = "Process";
            this.olvName.Width = 200;
            // 
            // olvRam
            // 
            this.olvRam.AspectName = "PrivateMemorySize64";
            this.olvRam.Text = "Private bytes";
            this.olvRam.Width = 140;
            // 
            // m_processTree
            // 
            this.m_processTree.AllColumns.Add(this.olvName);
            this.m_processTree.AllColumns.Add(this.olvRam);
            this.m_processTree.AllColumns.Add(this.olvThreads);
            this.m_processTree.AllColumns.Add(this.olvHandles);
            this.m_processTree.AllColumns.Add(this.olvPath);
            this.m_processTree.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvRam,
            this.olvThreads,
            this.olvHandles,
            this.olvPath});
            this.m_processTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_processTree.Location = new System.Drawing.Point(0, 0);
            this.m_processTree.Name = "m_processTree";
            this.m_processTree.OwnerDraw = true;
            this.m_processTree.ShowGroups = false;
            this.m_processTree.Size = new System.Drawing.Size(752, 423);
            this.m_processTree.TabIndex = 0;
            this.m_processTree.UseCompatibleStateImageBehavior = false;
            this.m_processTree.View = System.Windows.Forms.View.Details;
            this.m_processTree.VirtualMode = true;
            // 
            // olvThreads
            // 
            this.olvThreads.AspectName = "Threads";
            this.olvThreads.Text = "Threads";
            // 
            // olvHandles
            // 
            this.olvHandles.AspectName = "HandleCount";
            this.olvHandles.Text = "Handles";
            // 
            // olvPath
            // 
            this.olvPath.AspectName = "MainModule.FileName";
            this.olvPath.Text = "Path";
            this.olvPath.Width = 266;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 423);
            this.Controls.Add(this.m_processTree);
            this.Name = "MainForm";
            this.Text = "Process Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.m_processTree)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvRam;
        private BrightIdeasSoftware.TreeListView m_processTree;
        private BrightIdeasSoftware.OLVColumn olvThreads;
        private BrightIdeasSoftware.OLVColumn olvHandles;
        private BrightIdeasSoftware.OLVColumn olvPath;



    }
}

