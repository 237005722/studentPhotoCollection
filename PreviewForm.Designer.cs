namespace StudentPhotoCollection
{
    partial class PreviewForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewForm));
            this.pictureBox_photoOne = new System.Windows.Forms.PictureBox();
            this.button_reShoot = new System.Windows.Forms.Button();
            this.button_okNext = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_photoOne)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox_photoOne
            // 
            this.pictureBox_photoOne.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox_photoOne.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_photoOne.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_photoOne.Name = "pictureBox_photoOne";
            this.pictureBox_photoOne.Size = new System.Drawing.Size(304, 261);
            this.pictureBox_photoOne.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_photoOne.TabIndex = 0;
            this.pictureBox_photoOne.TabStop = false;
            // 
            // button_reShoot
            // 
            this.button_reShoot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_reShoot.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button_reShoot.CausesValidation = false;
            this.button_reShoot.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_reShoot.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button_reShoot.Location = new System.Drawing.Point(0, 3);
            this.button_reShoot.Name = "button_reShoot";
            this.button_reShoot.Size = new System.Drawing.Size(115, 37);
            this.button_reShoot.TabIndex = 11;
            this.button_reShoot.Text = "NO，重新拍摄";
            this.button_reShoot.UseVisualStyleBackColor = true;
            this.button_reShoot.Click += new System.EventHandler(this.Button_reShoot_Click);
            // 
            // button_okNext
            // 
            this.button_okNext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_okNext.BackColor = System.Drawing.SystemColors.Control;
            this.button_okNext.CausesValidation = false;
            this.button_okNext.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_okNext.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button_okNext.Location = new System.Drawing.Point(144, 3);
            this.button_okNext.Name = "button_okNext";
            this.button_okNext.Size = new System.Drawing.Size(114, 37);
            this.button_okNext.TabIndex = 10;
            this.button_okNext.Text = "OK，继续拍摄";
            this.button_okNext.UseVisualStyleBackColor = true;
            this.button_okNext.Click += new System.EventHandler(this.Button_okNext_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.button_okNext);
            this.panel1.Controls.Add(this.button_reShoot);
            this.panel1.Location = new System.Drawing.Point(22, 176);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(258, 43);
            this.panel1.TabIndex = 12;
            this.panel1.Visible = false;
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 261);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureBox_photoOne);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(320, 300);
            this.Name = "PreviewForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "照片预览";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewForm_FormClosing);
            this.Load += new System.EventHandler(this.PreviewForm_Load);
            this.Resize += new System.EventHandler(this.PreviewForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_photoOne)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_photoOne;
        private System.Windows.Forms.Button button_reShoot;
        private System.Windows.Forms.Button button_okNext;
        private System.Windows.Forms.Panel panel1;
    }
}