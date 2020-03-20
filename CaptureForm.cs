using Accord.Imaging.Filters;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace StudentPhotoCollection
{
    // 目前没有使用此窗体
    public partial class CaptureForm : Form
    {

        private VideoCaptureDevice videoDevice;

        private Bitmap currentPicture;

        private HaarObjectDetector detector;

        /*
        public CaptureForm()
        {
            InitializeComponent();
        }
        */
        public CaptureForm(VideoCaptureDevice videoDevice)
        {
            InitializeComponent();
            this.videoDevice = videoDevice;

        }

        public CaptureForm(VideoCaptureDevice videoDevice, int width, int height)
        {
            InitializeComponent();
            this.videoDevice = videoDevice;

            // 设置窗体大小
            this.Width = width;
            this.Height = height;
        }

        public CaptureForm(VideoCaptureDevice videoDevice, int width, int height, string formName)
        {
            InitializeComponent();
            this.videoDevice = videoDevice;

            // 设置窗体标题
            this.Text = formName;

            //这个区域不包括任务栏的
            Rectangle ScreenArea = System.Windows.Forms.Screen.GetWorkingArea(this);
            //这个区域包括任务栏，就是屏幕显示的物理范围
            //Rectangle ScreenArea = System.Windows.Forms.Screen.GetBounds(this);
            int screenWidth = ScreenArea.Width; //屏幕宽度 
            int screenHeight = ScreenArea.Height; //屏幕高度

            if (height > screenHeight)
            {
                height = screenHeight;
            }
            if(width > screenWidth / 2)
            {
                width = screenWidth / 2;
            }

            // 设置窗体大小
            this.Width = width;
            this.Height = height;

            // 指定窗体显示在左上角
            this.Location = new System.Drawing.Point(0, 0);

        }

        private void CaptureForm_Load(object sender, EventArgs e)
        {
            // FORM初始化

            // 初始化videoSourcePlayer长宽
            //this.videoSourcePlayer.SetBounds(0, 0, this.width, this.height);

            // 拍照图片圆角
            //setPictureBoxRegion(this.pictureBox_shoot);

            // 打开摄像头
            if (this.videoDevice != null)
            {
                this.videoSourcePlayer.VideoSource = this.videoDevice;
                this.videoSourcePlayer.Start();

                //AForge控件中的NewFrame事件获取要显示的每一帧的图像，做人脸标记处理
                this.videoSourcePlayer.NewFrame += VideoSourcePlayer_NewFrame;

            }

        }

        private void CaptureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // FORM关闭
            DisConnect();
        }

        private void CaptureForm_Resize(object sender, EventArgs e)
        {   
            // 设置拍照按钮图片始终左右居中
            int x = (int)(0.5 * (this.Width - this.pictureBox_shoot.Width));
            int y = (int)(0.9 * (this.Height - this.pictureBox_shoot.Height));

            //int y = this.pictureBox_shoot.Location.Y;
            this.pictureBox_shoot.Location = new System.Drawing.Point(x, y);
        }

        private void setPictureBoxRegion(PictureBox pictureBox)
        {
            // 拍照图片圆角，圆角有锯齿
            try
            {

                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(pictureBox.ClientRectangle);
                Region region = new Region(gp);
                pictureBox.Region = region;
                gp.Dispose();
                region.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void PictureBox_shoot_Click(object sender, EventArgs e)
        {
            //拍照
            Bitmap img = currentPicture ?? videoSourcePlayer.GetCurrentVideoFrame();

            //父窗体
            MainForm mainForm = (MainForm)this.Owner;
            //设置并保存照片
            mainForm.SaveShootPhoto(img);

        }

        private void VideoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            //AForge控件中的NewFrame回调事件获取要显示的每一帧的图像

            //人脸检测并标记
            image = FacePicDetect(image); //必须将返回值重新赋给image，否则摄像界面不会回显人脸标记框

        }

        private Bitmap FacePicDetect(Bitmap bitmap)
        {
            lock (this)
            {
                //留存原始的照片，如果正在拍照，则保存此未添加人脸识别框的照片
                currentPicture = (Bitmap)bitmap.Clone();

                if (detector == null)
                {
                    //先实例化用于检测人脸的对象detector
                    detector = new HaarObjectDetector(new FaceHaarCascade(), 100)
                    {
                        SearchMode = ObjectDetectorSearchMode.Single, //搜索模式
                        ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller, //缩放模式
                        ScalingFactor = 1.5f, //在搜索期间重新缩放搜索窗口时要使用的重新缩放因子
                        UseParallelProcessing = true
                    };//面部级联对象 + 搜索对象时使用的最小窗口大小
                }

                // 开始对检测区域进行检测并返回结果数组
                Rectangle[] regions = detector.ProcessFrame(bitmap);
                if (regions != null && regions.Length > 0)
                {
                    //人脸标记
                    RectanglesMarker marker = new RectanglesMarker(regions, Color.Orange);
                    regions = null;
                    return marker.Apply(bitmap);
                }
                regions = null;
                return bitmap;

            }
        }


        private void DisConnect()
        {
            //窗体关闭前设置
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                videoSourcePlayer.WaitForStop();
                videoSourcePlayer.VideoSource = null;
            }

            detector = null;
            currentPicture = null;
            videoDevice = null;


            // 更新组件状态
            MainForm mainForm = (MainForm)this.Owner;
            mainForm.EnableControlStatus(true);

        }


    }
}
