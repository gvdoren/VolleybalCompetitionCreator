﻿namespace CompetitionCreator
{
    partial class ClubListView
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
            this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
            this.FirstColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.FirstColumn);
            this.objectListView1.AllColumns.Add(this.olvColumn1);
            this.objectListView1.AllColumns.Add(this.olvColumn2);
            this.objectListView1.AllColumns.Add(this.olvColumn3);
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FirstColumn,
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3});
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.HasCollapsibleGroups = false;
            this.objectListView1.HideSelection = false;
            this.objectListView1.Location = new System.Drawing.Point(1, 1);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.Size = new System.Drawing.Size(282, 260);
            this.objectListView1.TabIndex = 0;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.UseFiltering = true;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.objectListView1_CellClick);
            this.objectListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.objectListView1_MouseDoubleClick);
            // 
            // FirstColumn
            // 
            this.FirstColumn.AspectName = "name";
            this.FirstColumn.CellPadding = null;
            this.FirstColumn.FillsFreeSpace = true;
            this.FirstColumn.MaximumWidth = 500;
            this.FirstColumn.MinimumWidth = 60;
            this.FirstColumn.Text = "Club";
            this.FirstColumn.UseInitialLetterForGroup = true;
            this.FirstColumn.Width = 104;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "conflict";
            this.olvColumn1.CellPadding = null;
            this.olvColumn1.MaximumWidth = 55;
            this.olvColumn1.Text = "Conflicts";
            this.olvColumn1.Width = 50;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "teams.Count";
            this.olvColumn2.CellPadding = null;
            this.olvColumn2.MaximumWidth = 45;
            this.olvColumn2.Text = "Teams";
            this.olvColumn2.Width = 45;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "percentage";
            this.olvColumn3.CellPadding = null;
            this.olvColumn3.MaximumWidth = 30;
            this.olvColumn3.MinimumWidth = 25;
            this.olvColumn3.Text = "%";
            this.olvColumn3.Width = 54;
            // 
            // ClubListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.objectListView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ClubListView";
            this.Text = "Club-conflicts";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClubListView_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView objectListView1;
        private BrightIdeasSoftware.OLVColumn FirstColumn;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
    }
}