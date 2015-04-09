namespace IronManConsole
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.hand = new System.Windows.Forms.PictureBox();
            this.pinch = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pinch)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(222, 177);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 51);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // hand
            // 
            this.hand.Image = ((System.Drawing.Image)(resources.GetObject("hand.Image")));
            this.hand.Location = new System.Drawing.Point(191, 121);
            this.hand.Name = "hand";
            this.hand.Size = new System.Drawing.Size(100, 50);
            this.hand.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.hand.TabIndex = 1;
            this.hand.TabStop = false;
            this.hand.Visible = false;
            // 
            // pinch
            // 
            this.pinch.Image = ((System.Drawing.Image)(resources.GetObject("pinch.Image")));
            this.pinch.Location = new System.Drawing.Point(184, 121);
            this.pinch.Name = "pinch";
            this.pinch.Size = new System.Drawing.Size(100, 50);
            this.pinch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pinch.TabIndex = 2;
            this.pinch.TabStop = false;
            this.pinch.Visible = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 240);
            this.Controls.Add(this.pinch);
            this.Controls.Add(this.hand);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pinch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox hand;
        private System.Windows.Forms.PictureBox pinch;
    }
}