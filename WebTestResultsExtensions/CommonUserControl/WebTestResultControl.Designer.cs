namespace WebTestResultsExtensions
{
    partial class WebTestResultControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.resultControlDataGridView = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // resultControlDataGridView
            // 
            this.resultControlDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultControlDataGridView.Location = new System.Drawing.Point(0, 0);
            this.resultControlDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.resultControlDataGridView.Name = "resultControlDataGridView";
            this.resultControlDataGridView.Size = new System.Drawing.Size(200, 185);
            this.resultControlDataGridView.TabIndex = 0;
            this.resultControlDataGridView.Text = "";
            this.resultControlDataGridView.TextChanged += new System.EventHandler(this.resultControlDataGridView_TextChanged);
            // 
            // WebTestResultControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.resultControlDataGridView);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "WebTestResultControl";
            this.Size = new System.Drawing.Size(200, 185);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox resultControlDataGridView;
    }
}
