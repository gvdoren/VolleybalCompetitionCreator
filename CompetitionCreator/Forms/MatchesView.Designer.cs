namespace CompetitionCreator
{
    partial class MatchesView
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.conflictLabel = new System.Windows.Forms.Label();
            this.checkBoxConflictsOnly = new System.Windows.Forms.CheckBox();
            this.labelGroup = new System.Windows.Forms.Label();
            this.labelSeriePoule = new System.Windows.Forms.Label();
            this.labelTeam = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn6 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn7 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn8 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn9 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.richTextConflict = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.conflictLabel);
            this.splitContainer1.Panel1.Controls.Add(this.checkBoxConflictsOnly);
            this.splitContainer1.Panel1.Controls.Add(this.labelGroup);
            this.splitContainer1.Panel1.Controls.Add(this.labelSeriePoule);
            this.splitContainer1.Panel1.Controls.Add(this.labelTeam);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.objectListView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richTextConflict);
            this.splitContainer1.Size = new System.Drawing.Size(486, 333);
            this.splitContainer1.SplitterDistance = 282;
            this.splitContainer1.TabIndex = 4;
            // 
            // conflictLabel
            // 
            this.conflictLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.conflictLabel.AutoSize = true;
            this.conflictLabel.Location = new System.Drawing.Point(24, 261);
            this.conflictLabel.Name = "conflictLabel";
            this.conflictLabel.Size = new System.Drawing.Size(45, 13);
            this.conflictLabel.TabIndex = 5;
            this.conflictLabel.Text = "Conflict:";
            // 
            // checkBoxConflictsOnly
            // 
            this.checkBoxConflictsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxConflictsOnly.AutoSize = true;
            this.checkBoxConflictsOnly.Checked = true;
            this.checkBoxConflictsOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxConflictsOnly.Location = new System.Drawing.Point(394, 2);
            this.checkBoxConflictsOnly.Name = "checkBoxConflictsOnly";
            this.checkBoxConflictsOnly.Size = new System.Drawing.Size(88, 17);
            this.checkBoxConflictsOnly.TabIndex = 7;
            this.checkBoxConflictsOnly.Text = "Conflicts only";
            this.checkBoxConflictsOnly.UseVisualStyleBackColor = true;
            this.checkBoxConflictsOnly.CheckedChanged += new System.EventHandler(this.checkBoxConflictsOnly_CheckedChanged);
            // 
            // labelGroup
            // 
            this.labelGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelGroup.AutoSize = true;
            this.labelGroup.Location = new System.Drawing.Point(30, 244);
            this.labelGroup.Name = "labelGroup";
            this.labelGroup.Size = new System.Drawing.Size(39, 13);
            this.labelGroup.TabIndex = 6;
            this.labelGroup.Text = "Group:";
            // 
            // labelSeriePoule
            // 
            this.labelSeriePoule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSeriePoule.AutoSize = true;
            this.labelSeriePoule.Location = new System.Drawing.Point(5, 228);
            this.labelSeriePoule.Name = "labelSeriePoule";
            this.labelSeriePoule.Size = new System.Drawing.Size(64, 13);
            this.labelSeriePoule.TabIndex = 5;
            this.labelSeriePoule.Text = "Serie-Poule:";
            // 
            // labelTeam
            // 
            this.labelTeam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTeam.AutoSize = true;
            this.labelTeam.Location = new System.Drawing.Point(32, 212);
            this.labelTeam.Name = "labelTeam";
            this.labelTeam.Size = new System.Drawing.Size(37, 13);
            this.labelTeam.TabIndex = 4;
            this.labelTeam.Text = "Team:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Matches:0  Conflicts:0  (0%)";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.olvColumn1);
            this.objectListView1.AllColumns.Add(this.olvColumn2);
            this.objectListView1.AllColumns.Add(this.olvColumn3);
            this.objectListView1.AllColumns.Add(this.olvColumn4);
            this.objectListView1.AllColumns.Add(this.olvColumn5);
            this.objectListView1.AllColumns.Add(this.olvColumn6);
            this.objectListView1.AllColumns.Add(this.olvColumn7);
            this.objectListView1.AllColumns.Add(this.olvColumn8);
            this.objectListView1.AllColumns.Add(this.olvColumn9);
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3,
            this.olvColumn4,
            this.olvColumn5,
            this.olvColumn6,
            this.olvColumn7,
            this.olvColumn8,
            this.olvColumn9});
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.HideSelection = false;
            this.objectListView1.Location = new System.Drawing.Point(0, 19);
            this.objectListView1.MultiSelect = false;
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.Size = new System.Drawing.Size(486, 190);
            this.objectListView1.SortGroupItemsByPrimaryColumn = false;
            this.objectListView1.TabIndex = 2;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.SelectionChanged += new System.EventHandler(this.objectListView1_SelectionChanged);
            this.objectListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.objectListView1_MouseDoubleClick);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "datetime";
            this.olvColumn1.AspectToStringFormat = "{0:dd-MM-yyyy}";
            this.olvColumn1.CellPadding = null;
            this.olvColumn1.MaximumWidth = 100;
            this.olvColumn1.Text = "Datum";
            this.olvColumn1.Width = 70;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "Time";
            this.olvColumn2.CellPadding = null;
            this.olvColumn2.DisplayIndex = 2;
            this.olvColumn2.Text = "Tijd";
            this.olvColumn2.Width = 40;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "DayString";
            this.olvColumn3.CellPadding = null;
            this.olvColumn3.DisplayIndex = 1;
            this.olvColumn3.Text = "Dag";
            this.olvColumn3.Width = 35;
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "homeTeam.name";
            this.olvColumn4.CellPadding = null;
            this.olvColumn4.FillsFreeSpace = true;
            this.olvColumn4.MaximumWidth = 200;
            this.olvColumn4.MinimumWidth = 60;
            this.olvColumn4.Text = "Team";
            // 
            // olvColumn5
            // 
            this.olvColumn5.AspectName = "visitorTeam.name";
            this.olvColumn5.CellPadding = null;
            this.olvColumn5.FillsFreeSpace = true;
            this.olvColumn5.MaximumWidth = 200;
            this.olvColumn5.MinimumWidth = 60;
            this.olvColumn5.Text = "Team";
            this.olvColumn5.Width = 100;
            // 
            // olvColumn6
            // 
            this.olvColumn6.AspectName = "poule.serie.name";
            this.olvColumn6.CellPadding = null;
            this.olvColumn6.Text = "Serie";
            this.olvColumn6.Width = 55;
            // 
            // olvColumn7
            // 
            this.olvColumn7.AspectName = "poule.name";
            this.olvColumn7.CellPadding = null;
            this.olvColumn7.Text = "Poule";
            this.olvColumn7.Width = 30;
            // 
            // olvColumn8
            // 
            this.olvColumn8.AspectName = "homeTeam.GroupName";
            this.olvColumn8.CellPadding = null;
            this.olvColumn8.Text = "Group";
            this.olvColumn8.Width = 45;
            // 
            // olvColumn9
            // 
            this.olvColumn9.AspectName = "ConflictString";
            this.olvColumn9.CellPadding = null;
            this.olvColumn9.FillsFreeSpace = true;
            this.olvColumn9.MaximumWidth = 300;
            this.olvColumn9.MinimumWidth = 70;
            this.olvColumn9.Text = "Conflict";
            this.olvColumn9.Width = 70;
            // 
            // richTextConflict
            // 
            this.richTextConflict.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextConflict.Location = new System.Drawing.Point(1, 3);
            this.richTextConflict.Name = "richTextConflict";
            this.richTextConflict.Size = new System.Drawing.Size(483, 41);
            this.richTextConflict.TabIndex = 4;
            this.richTextConflict.Text = "";
            // 
            // MatchesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 339);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MatchesView";
            this.Text = "Matches - conflicts";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConstraintView_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private BrightIdeasSoftware.ObjectListView objectListView1;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private BrightIdeasSoftware.OLVColumn olvColumn6;
        private BrightIdeasSoftware.OLVColumn olvColumn7;
        private BrightIdeasSoftware.OLVColumn olvColumn8;
        private System.Windows.Forms.Label conflictLabel;
        private System.Windows.Forms.RichTextBox richTextConflict;
        private BrightIdeasSoftware.OLVColumn olvColumn9;
        private System.Windows.Forms.Label labelGroup;
        private System.Windows.Forms.Label labelSeriePoule;
        private System.Windows.Forms.Label labelTeam;
        private System.Windows.Forms.CheckBox checkBoxConflictsOnly;


    }
}