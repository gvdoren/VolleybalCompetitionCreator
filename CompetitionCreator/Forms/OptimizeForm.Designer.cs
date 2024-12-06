namespace CompetitionCreator
{
    partial class OptimizeForm
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
            this.nameColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.evaluatedColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.importedColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.NumberOptimization = new System.Windows.Forms.CheckBox();
            this.HomeVisitOptimization = new System.Windows.Forms.CheckBox();
            this.SchemaOptimization = new System.Windows.Forms.CheckBox();
            this.OptimizingThreshold = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.nameColumn);
            this.objectListView1.AllColumns.Add(this.evaluatedColumn);
            this.objectListView1.AllColumns.Add(this.importedColumn);
            this.objectListView1.AllColumns.Add(this.olvColumn1);
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.objectListView1.CheckedAspectName = "";
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.evaluatedColumn,
            this.importedColumn});
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.GridLines = true;
            this.objectListView1.HideSelection = false;
            this.objectListView1.Location = new System.Drawing.Point(2, 8);
            this.objectListView1.Margin = new System.Windows.Forms.Padding(2);
            this.objectListView1.MaximumSize = new System.Drawing.Size(602, 6501);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.OwnerDraw = true;
            this.objectListView1.ShowGroups = false;
            this.objectListView1.ShowImagesOnSubItems = true;
            this.objectListView1.Size = new System.Drawing.Size(172, 328);
            this.objectListView1.TabIndex = 0;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.UseSubItemCheckBoxes = true;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.objectListView1_CellEditFinishing);
            this.objectListView1.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(this.objectListView1_CellEditStarting);
            this.objectListView1.SubItemChecking += new System.EventHandler<BrightIdeasSoftware.SubItemCheckingEventArgs>(this.objectListView1_SubItemChecking);
            // 
            // nameColumn
            // 
            this.nameColumn.AspectName = "name";
            this.nameColumn.CellPadding = null;
            this.nameColumn.FillsFreeSpace = true;
            this.nameColumn.Hyperlink = true;
            this.nameColumn.MinimumWidth = 60;
            this.nameColumn.Text = "Serie";
            this.nameColumn.Width = 80;
            // 
            // evaluatedColumn
            // 
            this.evaluatedColumn.AspectName = "evaluated";
            this.evaluatedColumn.CellPadding = null;
            this.evaluatedColumn.CheckBoxes = true;
            this.evaluatedColumn.MaximumWidth = 60;
            this.evaluatedColumn.MinimumWidth = 60;
            this.evaluatedColumn.Text = "Evaluate";
            // 
            // importedColumn
            // 
            this.importedColumn.AspectName = "imported";
            this.importedColumn.CellPadding = null;
            this.importedColumn.CheckBoxes = true;
            this.importedColumn.Text = "Immutable";
            this.importedColumn.Width = 100;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "optimize";
            this.olvColumn1.CellPadding = null;
            this.olvColumn1.CheckBoxes = true;
            this.olvColumn1.IsVisible = false;
            this.olvColumn1.Text = "Optimize";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(4, 16);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Optimize";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(3, 16);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(114, 21);
            this.button3.TabIndex = 3;
            this.button3.Text = "Optimize";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(344, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(118, 54);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "All";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Location = new System.Drawing.Point(344, 65);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(121, 51);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Selected Clubs";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.OptimizingThreshold);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Location = new System.Drawing.Point(335, 203);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(124, 110);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Expert";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Optimization Type:";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(7, 34);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(115, 21);
            this.comboBox1.TabIndex = 11;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.comboBox1.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
            // 
            // NumberOptimization
            // 
            this.NumberOptimization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NumberOptimization.AutoSize = true;
            this.NumberOptimization.Location = new System.Drawing.Point(191, 27);
            this.NumberOptimization.Margin = new System.Windows.Forms.Padding(2);
            this.NumberOptimization.Name = "NumberOptimization";
            this.NumberOptimization.Size = new System.Drawing.Size(121, 17);
            this.NumberOptimization.TabIndex = 11;
            this.NumberOptimization.Text = "Number optimization";
            this.NumberOptimization.UseVisualStyleBackColor = true;
            this.NumberOptimization.CheckedChanged += new System.EventHandler(this.NumberOptimization_CheckedChanged);
            // 
            // HomeVisitOptimization
            // 
            this.HomeVisitOptimization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HomeVisitOptimization.AutoSize = true;
            this.HomeVisitOptimization.Location = new System.Drawing.Point(191, 58);
            this.HomeVisitOptimization.Margin = new System.Windows.Forms.Padding(2);
            this.HomeVisitOptimization.Name = "HomeVisitOptimization";
            this.HomeVisitOptimization.Size = new System.Drawing.Size(136, 17);
            this.HomeVisitOptimization.TabIndex = 12;
            this.HomeVisitOptimization.Text = "Home/Visit optimization";
            this.HomeVisitOptimization.UseVisualStyleBackColor = true;
            this.HomeVisitOptimization.CheckedChanged += new System.EventHandler(this.HomeVisitOptimization_CheckedChanged);
            // 
            // SchemaOptimization
            // 
            this.SchemaOptimization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SchemaOptimization.AutoSize = true;
            this.SchemaOptimization.Location = new System.Drawing.Point(191, 87);
            this.SchemaOptimization.Margin = new System.Windows.Forms.Padding(2);
            this.SchemaOptimization.Name = "SchemaOptimization";
            this.SchemaOptimization.Size = new System.Drawing.Size(123, 17);
            this.SchemaOptimization.TabIndex = 13;
            this.SchemaOptimization.Text = "Schema optimization";
            this.SchemaOptimization.UseVisualStyleBackColor = true;
            this.SchemaOptimization.CheckedChanged += new System.EventHandler(this.SchemaOptimization_CheckedChanged);
            // 
            // OptimizingThreshold
            // 
            this.OptimizingThreshold.Location = new System.Drawing.Point(9, 78);
            this.OptimizingThreshold.Name = "OptimizingThreshold";
            this.OptimizingThreshold.Size = new System.Drawing.Size(100, 20);
            this.OptimizingThreshold.TabIndex = 13;
            this.OptimizingThreshold.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Threshold";
            // 
            // OptimizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 343);
            this.Controls.Add(this.SchemaOptimization);
            this.Controls.Add(this.HomeVisitOptimization);
            this.Controls.Add(this.NumberOptimization);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.objectListView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "OptimizeForm";
            this.Text = "OptimizeForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptimizeForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView objectListView1;
        private BrightIdeasSoftware.OLVColumn nameColumn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private BrightIdeasSoftware.OLVColumn importedColumn;
        private BrightIdeasSoftware.OLVColumn evaluatedColumn;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox NumberOptimization;
        private System.Windows.Forms.CheckBox HomeVisitOptimization;
        private System.Windows.Forms.CheckBox SchemaOptimization;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OptimizingThreshold;
    }
}