namespace VolleybalCompetition_creator
{
    partial class PouleView
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
            this.invisible = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.objectListView2 = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn6 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView2)).BeginInit();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.invisible);
            this.objectListView1.AllColumns.Add(this.olvColumn3);
            this.objectListView1.AllColumns.Add(this.olvColumn2);
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.invisible,
            this.olvColumn3,
            this.olvColumn2});
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.IsSimpleDragSource = true;
            this.objectListView1.IsSimpleDropSink = true;
            this.objectListView1.Location = new System.Drawing.Point(-2, 25);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.Size = new System.Drawing.Size(173, 275);
            this.objectListView1.TabIndex = 0;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.ItemsChanged += new System.EventHandler<BrightIdeasSoftware.ItemsChangedEventArgs>(this.objectListView1_ItemsChanged);
            this.objectListView1.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.objectListView1_ModelCanDrop);
            this.objectListView1.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.objectListView1_ModelDropped);
            this.objectListView1.SelectionChanged += new System.EventHandler(this.objectListView1_SelectionChanged);
            // 
            // invisible
            // 
            this.invisible.CellPadding = null;
            this.invisible.IsVisible = false;
            this.invisible.MaximumWidth = 0;
            this.invisible.Width = 0;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "";
            this.olvColumn3.AspectToStringFormat = "";
            this.olvColumn3.CellPadding = null;
            this.olvColumn3.Sortable = false;
            this.olvColumn3.Text = "";
            this.olvColumn3.Width = 30;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "name";
            this.olvColumn2.CellPadding = null;
            this.olvColumn2.FillsFreeSpace = true;
            this.olvColumn2.MaximumWidth = 200;
            this.olvColumn2.MinimumWidth = 100;
            this.olvColumn2.Sortable = false;
            this.olvColumn2.Text = "Team";
            this.olvColumn2.Width = 100;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(-2, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Team mapping";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(177, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(49, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "/\\";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(177, 55);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(49, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "\\/";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(177, 85);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(49, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Switch";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Switch_Click);
            // 
            // objectListView2
            // 
            this.objectListView2.AllColumns.Add(this.olvColumn6);
            this.objectListView2.AllColumns.Add(this.olvColumn1);
            this.objectListView2.AllColumns.Add(this.olvColumn4);
            this.objectListView2.AllColumns.Add(this.olvColumn5);
            this.objectListView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.objectListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn6,
            this.olvColumn1,
            this.olvColumn4,
            this.olvColumn5});
            this.objectListView2.Location = new System.Drawing.Point(1, 325);
            this.objectListView2.Name = "objectListView2";
            this.objectListView2.Size = new System.Drawing.Size(352, 117);
            this.objectListView2.TabIndex = 5;
            this.objectListView2.UseCompatibleStateImageBehavior = false;
            this.objectListView2.View = System.Windows.Forms.View.Details;
            // 
            // olvColumn6
            // 
            this.olvColumn6.AspectName = "datetime";
            this.olvColumn6.AspectToStringFormat = "{0:dd-MM-yyyy}";
            this.olvColumn6.CellPadding = null;
            this.olvColumn6.Text = "Datum";
            this.olvColumn6.Width = 70;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "homeTeam.defaultTime";
            this.olvColumn1.AspectToStringFormat = "";
            this.olvColumn1.CellPadding = null;
            this.olvColumn1.MaximumWidth = 40;
            this.olvColumn1.MinimumWidth = 40;
            this.olvColumn1.Text = "Tijd";
            this.olvColumn1.Width = 40;
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "homeTeam.name";
            this.olvColumn4.CellPadding = null;
            this.olvColumn4.FillsFreeSpace = true;
            this.olvColumn4.MaximumWidth = 200;
            this.olvColumn4.MinimumWidth = 100;
            this.olvColumn4.Text = "Thuis Team";
            this.olvColumn4.Width = 100;
            // 
            // olvColumn5
            // 
            this.olvColumn5.AspectName = "visitorTeam.name";
            this.olvColumn5.CellPadding = null;
            this.olvColumn5.FillsFreeSpace = true;
            this.olvColumn5.MaximumWidth = 200;
            this.olvColumn5.MinimumWidth = 100;
            this.olvColumn5.Text = "bezoekers Team";
            this.olvColumn5.Width = 100;
            // 
            // PouleView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 441);
            this.Controls.Add(this.objectListView2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.objectListView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PouleView";
            this.Text = "PouleView";
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView objectListView1;
        private System.Windows.Forms.Label label1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private BrightIdeasSoftware.OLVColumn invisible;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private BrightIdeasSoftware.ObjectListView objectListView2;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private BrightIdeasSoftware.OLVColumn olvColumn6;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
    }
}