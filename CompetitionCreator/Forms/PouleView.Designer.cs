namespace CompetitionCreator
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
            this.olvColumn7 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.objectListView2 = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn6 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn9 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn8 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.objectListWeeks = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn12 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn13 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn14 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn10 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn11 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label2 = new System.Windows.Forms.Label();
            this.weekSwitchButton = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.buttonOptimizeMatch = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListWeeks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.invisible);
            this.objectListView1.AllColumns.Add(this.olvColumn3);
            this.objectListView1.AllColumns.Add(this.olvColumn2);
            this.objectListView1.AllColumns.Add(this.olvColumn7);
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.invisible,
            this.olvColumn3,
            this.olvColumn2,
            this.olvColumn7});
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.IsSimpleDragSource = true;
            this.objectListView1.IsSimpleDropSink = true;
            this.objectListView1.Location = new System.Drawing.Point(3, 22);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.Size = new System.Drawing.Size(404, 274);
            this.objectListView1.TabIndex = 0;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.objectListView1_CellClick);
            this.objectListView1.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.objectListView1_ModelCanDrop);
            this.objectListView1.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.objectListView1_ModelDropped);
            this.objectListView1.SelectionChanged += new System.EventHandler(this.objectListView1_SelectionChanged);
            this.objectListView1.SelectedIndexChanged += new System.EventHandler(this.objectListView1_SelectedIndexChanged);
            // 
            // invisible
            // 
            this.invisible.AspectName = "Index";
            this.invisible.CellPadding = null;
            this.invisible.IsVisible = false;
            this.invisible.MaximumWidth = 0;
            this.invisible.Width = 0;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "Index";
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
            this.olvColumn2.MaximumWidth = 300;
            this.olvColumn2.MinimumWidth = 100;
            this.olvColumn2.Sortable = false;
            this.olvColumn2.Text = "Team";
            this.olvColumn2.Width = 100;
            // 
            // olvColumn7
            // 
            this.olvColumn7.AspectName = "conflict";
            this.olvColumn7.CellPadding = null;
            this.olvColumn7.Sortable = false;
            this.olvColumn7.Text = "Conflicts";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Teams";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(413, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(57, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "/\\";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(413, 52);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(57, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "\\/";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(413, 82);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(57, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Switch";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Switch_Click);
            // 
            // objectListView2
            // 
            this.objectListView2.AllColumns.Add(this.olvColumn6);
            this.objectListView2.AllColumns.Add(this.olvColumn1);
            this.objectListView2.AllColumns.Add(this.olvColumn9);
            this.objectListView2.AllColumns.Add(this.olvColumn4);
            this.objectListView2.AllColumns.Add(this.olvColumn5);
            this.objectListView2.AllColumns.Add(this.olvColumn8);
            this.objectListView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn6,
            this.olvColumn1,
            this.olvColumn9,
            this.olvColumn4,
            this.olvColumn5,
            this.olvColumn8});
            this.objectListView2.FullRowSelect = true;
            this.objectListView2.Location = new System.Drawing.Point(1, 343);
            this.objectListView2.Name = "objectListView2";
            this.objectListView2.Size = new System.Drawing.Size(806, 306);
            this.objectListView2.TabIndex = 5;
            this.objectListView2.UseCompatibleStateImageBehavior = false;
            this.objectListView2.View = System.Windows.Forms.View.Details;
            this.objectListView2.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.objectListView2_CellClick);
            this.objectListView2.SelectionChanged += new System.EventHandler(this.objectListView2_SelectionChanged);
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
            // olvColumn9
            // 
            this.olvColumn9.AspectName = "DayString";
            this.olvColumn9.CellPadding = null;
            this.olvColumn9.Text = "Dag";
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
            // olvColumn8
            // 
            this.olvColumn8.AspectName = "conflict";
            this.olvColumn8.CellPadding = null;
            this.olvColumn8.Text = "Conflict";
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(414, 112);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(56, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Optimize";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Enabled = false;
            this.button5.Location = new System.Drawing.Point(813, 373);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Uit / Thuis";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.Location = new System.Drawing.Point(812, 402);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 8;
            this.button6.Text = "Optimize";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // objectListWeeks
            // 
            this.objectListWeeks.AllColumns.Add(this.olvColumn12);
            this.objectListWeeks.AllColumns.Add(this.olvColumn13);
            this.objectListWeeks.AllColumns.Add(this.olvColumn14);
            this.objectListWeeks.AllColumns.Add(this.olvColumn10);
            this.objectListWeeks.AllColumns.Add(this.olvColumn11);
            this.objectListWeeks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListWeeks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn12,
            this.olvColumn13,
            this.olvColumn14,
            this.olvColumn10});
            this.objectListWeeks.FullRowSelect = true;
            this.objectListWeeks.Location = new System.Drawing.Point(3, 22);
            this.objectListWeeks.Name = "objectListWeeks";
            this.objectListWeeks.Size = new System.Drawing.Size(345, 274);
            this.objectListWeeks.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.objectListWeeks.TabIndex = 9;
            this.objectListWeeks.UseCompatibleStateImageBehavior = false;
            this.objectListWeeks.View = System.Windows.Forms.View.Details;
            this.objectListWeeks.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.objectListView3_CellClick);
            this.objectListWeeks.SelectionChanged += new System.EventHandler(this.objectListView3_SelectionChanged);
            // 
            // olvColumn12
            // 
            this.olvColumn12.AspectName = "Key.Start";
            this.olvColumn12.AspectToStringFormat = "";
            this.olvColumn12.CellPadding = null;
            this.olvColumn12.MaximumWidth = 40;
            this.olvColumn12.MinimumWidth = 40;
            this.olvColumn12.Sortable = false;
            this.olvColumn12.Text = "From";
            this.olvColumn12.Width = 40;
            // 
            // olvColumn13
            // 
            this.olvColumn13.AspectName = "Key.End";
            this.olvColumn13.AspectToStringFormat = "";
            this.olvColumn13.CellPadding = null;
            this.olvColumn13.Sortable = false;
            this.olvColumn13.Text = "Until";
            this.olvColumn13.Width = 40;
            // 
            // olvColumn14
            // 
            this.olvColumn14.AspectName = "Value";
            this.olvColumn14.CellPadding = null;
            this.olvColumn14.Sortable = false;
            this.olvColumn14.Text = "Schema";
            // 
            // olvColumn10
            // 
            this.olvColumn10.AspectName = "Key.conflict";
            this.olvColumn10.CellPadding = null;
            this.olvColumn10.Sortable = false;
            this.olvColumn10.Text = "Conflicts";
            // 
            // olvColumn11
            // 
            this.olvColumn11.AspectName = "Key.FirstDayInWeek";
            this.olvColumn11.CellPadding = null;
            this.olvColumn11.DisplayIndex = 4;
            this.olvColumn11.IsVisible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Weeks";
            // 
            // weekSwitchButton
            // 
            this.weekSwitchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.weekSwitchButton.Location = new System.Drawing.Point(353, 22);
            this.weekSwitchButton.Name = "weekSwitchButton";
            this.weekSwitchButton.Size = new System.Drawing.Size(57, 23);
            this.weekSwitchButton.TabIndex = 4;
            this.weekSwitchButton.Text = "Switch";
            this.weekSwitchButton.UseVisualStyleBackColor = true;
            this.weekSwitchButton.Click += new System.EventHandler(this.Switch1_Click);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(354, 52);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(56, 23);
            this.button8.TabIndex = 6;
            this.button8.Text = "Optimize";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button9.Location = new System.Drawing.Point(6, 302);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(99, 23);
            this.button9.TabIndex = 11;
            this.button9.Text = "Optimize team";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Visible = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button10.Location = new System.Drawing.Point(115, 302);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(101, 23);
            this.button10.TabIndex = 12;
            this.button10.Text = "Optimize poule";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Visible = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button11.Location = new System.Drawing.Point(414, 141);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(56, 23);
            this.button11.TabIndex = 13;
            this.button11.Text = "Analyse";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Visible = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button12.Location = new System.Drawing.Point(414, 170);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(56, 23);
            this.button12.TabIndex = 14;
            this.button12.Text = "A + O";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Visible = false;
            // 
            // button13
            // 
            this.button13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button13.Location = new System.Drawing.Point(813, 431);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 23);
            this.button13.TabIndex = 15;
            this.button13.Text = "Optimize";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Visible = false;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button14.Location = new System.Drawing.Point(813, 344);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 23);
            this.button14.TabIndex = 7;
            this.button14.Text = "A + O";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Visible = false;
            // 
            // buttonOptimizeMatch
            // 
            this.buttonOptimizeMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOptimizeMatch.Location = new System.Drawing.Point(6, 302);
            this.buttonOptimizeMatch.Name = "buttonOptimizeMatch";
            this.buttonOptimizeMatch.Size = new System.Drawing.Size(107, 23);
            this.buttonOptimizeMatch.TabIndex = 6;
            this.buttonOptimizeMatch.Text = "Optimize Match";
            this.buttonOptimizeMatch.UseVisualStyleBackColor = true;
            this.buttonOptimizeMatch.Click += new System.EventHandler(this.optimizeMatch_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.objectListView1);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.button12);
            this.splitContainer1.Panel1.Controls.Add(this.button2);
            this.splitContainer1.Panel1.Controls.Add(this.button11);
            this.splitContainer1.Panel1.Controls.Add(this.button3);
            this.splitContainer1.Panel1.Controls.Add(this.button10);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.button4);
            this.splitContainer1.Panel1.Controls.Add(this.button9);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.objectListWeeks);
            this.splitContainer1.Panel2.Controls.Add(this.weekSwitchButton);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.button8);
            this.splitContainer1.Panel2.Controls.Add(this.buttonOptimizeMatch);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(896, 333);
            this.splitContainer1.SplitterDistance = 475;
            this.splitContainer1.TabIndex = 16;
            // 
            // PouleView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(897, 648);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.objectListView2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PouleView";
            this.Text = "PouleView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PouleView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.objectListWeeks)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private BrightIdeasSoftware.OLVColumn olvColumn7;
        private BrightIdeasSoftware.OLVColumn olvColumn8;
        private BrightIdeasSoftware.OLVColumn olvColumn9;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private BrightIdeasSoftware.ObjectListView objectListWeeks;
        private System.Windows.Forms.Label label2;
        private BrightIdeasSoftware.OLVColumn olvColumn12;
        private BrightIdeasSoftware.OLVColumn olvColumn14;
        private System.Windows.Forms.Button weekSwitchButton;
        private BrightIdeasSoftware.OLVColumn olvColumn10;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button buttonOptimizeMatch;
        private BrightIdeasSoftware.OLVColumn olvColumn13;
        private BrightIdeasSoftware.OLVColumn olvColumn11;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}