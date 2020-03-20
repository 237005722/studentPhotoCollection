namespace StudentPhotoCollection
{
    partial class CaptureForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaptureForm));
            this.videoSourcePlayer = new AForge.Controls.VideoSourcePlayer();
            this.pictureBox_shoot = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_shoot)).BeginInit();
            this.SuspendLayout();
            // 
            // videoSourcePlayer
            // 
            this.videoSourcePlayer.BackColor = System.Drawing.SystemColors.Window;
            this.videoSourcePlayer.BorderColor = System.Drawing.SystemColors.Window;
            this.videoSourcePlayer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoSourcePlayer.Location = new System.Drawing.Point(0, 0);
            this.videoSourcePlayer.Name = "videoSourcePlayer";
            this.videoSourcePlayer.Size = new System.Drawing.Size(583, 676);
            this.videoSourcePlayer.TabIndex = 0;
            this.videoSourcePlayer.Text = "videoSourcePlayer";
            this.videoSourcePlayer.VideoSource = null;
            // 
            // pictureBox_shoot
            // 
            this.pictureBox_shoot.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_shoot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox_shoot.Image = global::StudentPhotoCollection.Properties.Resources.拍照;
            this.pictureBox_shoot.Location = new System.Drawing.Point(245, 553);
            this.pictureBox_shoot.Name = "pictureBox_shoot";
            this.pictureBox_shoot.Size = new System.Drawing.Size(50, 50);
            this.pictureBox_shoot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_shoot.TabIndex = 1;
            this.pictureBox_shoot.TabStop = false;
            this.pictureBox_shoot.Click += new System.EventHandler(this.PictureBox_shoot_Click);
            // 
            // CaptureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 676);
            this.Controls.Add(this.pictureBox_shoot);
            this.Controls.Add(this.videoSourcePlayer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CaptureForm";
            this.Text = "摄像设备";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CaptureForm_FormClosing);
            this.Load += new System.EventHandler(this.CaptureForm_Load);
            this.Resize += new System.EventHandler(this.CaptureForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_shoot)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private System.Windows.Forms.PictureBox pictureBox_shoot;
    }
}