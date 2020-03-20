using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentPhotoCollection
{
    public partial class PreviewForm : Form
    {
        private Image img;
        private Bitmap bitmap;
        private bool confirm;
        private bool saveFlag;

        public PreviewForm(Image img, string formName)
        {
            InitializeComponent();
            this.img = img;
            this.Text = formName;

            // 设置窗体大小
            if (this.img.Width > 10)
            {
                this.Width = this.img.Width;
            }
            if(this.img.Height > 10)
            {
                this.Height = this.img.Height;
            }

            this.pictureBox_photoOne.Image = this.img;
        }

        public PreviewForm(Bitmap img, string formName, bool confirm)
        {
            InitializeComponent();
            this.bitmap = img;
            this.Text = formName;
            this.confirm = confirm;
            this.saveFlag = false;

            // 设置窗体大小
            if (this.bitmap.Width > 10)
            {
                this.Width = this.bitmap.Width;
            }
            if (this.bitmap.Height > 10)
            {
                this.Height = this.bitmap.Height;
            }

            this.pictureBox_photoOne.Image = this.bitmap;

            // 是否显示确认按钮
            if (this.confirm)
            {
                // 显示重拍和继续按钮
                this.panel1.Visible = true;


                // 将PictureBox设为透明，在C#中，控件的透明指对父窗体透明。
                this.panel1.BackColor = Color.Transparent;
                this.panel1.Parent = this.pictureBox_photoOne;

                // 设置拍照按钮图片始终左右居中
                int x = (int)(0.5 * (this.Width - this.panel1.Width));
                int y = (int)(0.9 * (this.Height - this.panel1.Height));
                //int y = this.panel1.Location.Y;

                this.panel1.Location = new System.Drawing.Point(x, y);

            }
        }

        private void PreviewForm_Load(object sender, EventArgs e)
        {

        }

        private void PreviewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.confirm && !this.saveFlag)
            {
                //父窗体
                MainForm mainForm = (MainForm)this.Owner;
                //设置并保存照片
                mainForm.SaveShootPhoto(this.bitmap);
            }
        }

        private void PreviewForm_Resize(object sender, EventArgs e)
        {
            // 设置图片始终跟随，注意pictureBox的SizeMode 属性值需设置为 StretchImage
            this.pictureBox_photoOne.Width = this.Width;
            this.pictureBox_photoOne.Height = this.Height;

            if (this.confirm)
            {
                // 设置拍照按钮图片始终左右居中
                int x = (int)(0.5 * (this.Width - this.panel1.Width));
                int y = (int)(0.9 * (this.Height - this.panel1.Height));
                //int y = this.panel1.Location.Y;

                this.panel1.Location = new System.Drawing.Point(x, y);
            }
                
        }

        private void Button_reShoot_Click(object sender, EventArgs e)
        {
            //重拍照片
            Dispose();
            Close();
        }

        private void Button_okNext_Click(object sender, EventArgs e)
        {
            //继续拍照
            //父窗体
            MainForm mainForm = (MainForm)this.Owner;
            //设置并保存照片
            mainForm.SaveShootPhoto(this.bitmap);

            this.saveFlag = true;

            Dispose();
            Close();

        }
    }
}
