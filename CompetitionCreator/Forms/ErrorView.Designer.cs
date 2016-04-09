namespace CompetitionCreator
{
    partial class ErrorView
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
            this.buttonClearErrors = new System.Windows.Forms.Button();
            this.errorBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // buttonClearErrors
            // 
            this.buttonClearErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearErrors.Location = new System.Drawing.Point(169, 236);
            this.buttonClearErrors.Name = "buttonClearErrors";
            this.buttonClearErrors.Size = new System.Drawing.Size(106, 23);
            this.buttonClearErrors.TabIndex = 1;
            this.buttonClearErrors.Text = "Clear Timed Errors";
            this.buttonClearErrors.UseVisualStyleBackColor = true;
            this.buttonClearErrors.Click += new System.EventHandler(this.buttonClearErrors_Click);
            // 
            // errorBrowser
            // 
            this.errorBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.errorBrowser.Location = new System.Drawing.Point(1, 4);
            this.errorBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.errorBrowser.Name = "errorBrowser";
            this.errorBrowser.Size = new System.Drawing.Size(284, 226);
            this.errorBrowser.TabIndex = 2;
            // 
            // ErrorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.errorBrowser);
            this.Controls.Add(this.buttonClearErrors);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ErrorView";
            this.Text = "ErrorView";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ErrorView_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonClearErrors;
        private System.Windows.Forms.WebBrowser errorBrowser;
    }
}