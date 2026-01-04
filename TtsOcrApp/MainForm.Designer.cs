namespace TtsOcrApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCapture = new Button();
            txtOcrResult = new TextBox();
            btnSelectRegion = new Button();
            SuspendLayout();
            // 
            // btnCapture
            // 
            btnCapture.Location = new Point(135, 12);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new Size(117, 23);
            btnCapture.TabIndex = 0;
            btnCapture.Text = "Capture + OCR";
            btnCapture.UseVisualStyleBackColor = true;
            btnCapture.Click += btnCapture_Click;
            // 
            // txtOcrResult
            // 
            txtOcrResult.Location = new Point(12, 41);
            txtOcrResult.Multiline = true;
            txtOcrResult.Name = "txtOcrResult";
            txtOcrResult.ScrollBars = ScrollBars.Vertical;
            txtOcrResult.Size = new Size(776, 397);
            txtOcrResult.TabIndex = 1;
            // 
            // btnSelectRegion
            // 
            btnSelectRegion.Location = new Point(12, 12);
            btnSelectRegion.Name = "btnSelectRegion";
            btnSelectRegion.Size = new Size(117, 23);
            btnSelectRegion.TabIndex = 2;
            btnSelectRegion.Text = "Select region";
            btnSelectRegion.UseVisualStyleBackColor = true;
            btnSelectRegion.Click += btnSelectRegion_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSelectRegion);
            Controls.Add(txtOcrResult);
            Controls.Add(btnCapture);
            Name = "MainForm";
            Text = "OCR Reader (v1)";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCapture;
        private TextBox txtOcrResult;
        private Button btnSelectRegion;
    }
}
