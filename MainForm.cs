using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;

namespace StudentPhotoCollection
{
    public partial class MainForm : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        private VideoCapabilities[] videoCapabilities;
        private HaarObjectDetector detector;

        private Bitmap currentPicture;

        private PreviewForm previewForm;

        private ImageCodecInfo myImageCodecInfo;
        private Encoder myEncoder;
        private EncoderParameter myEncoderParameter;
        private EncoderParameters myEncoderParameters;



        public MainForm()
        {
            InitializeComponent();


            //这个区域不包括任务栏的
            Rectangle ScreenArea = System.Windows.Forms.Screen.GetWorkingArea(this);
            //这个区域包括任务栏，就是屏幕显示的物理范围
            //Rectangle ScreenArea = System.Windows.Forms.Screen.GetBounds(this);
            int screenWidth = ScreenArea.Width; //屏幕宽度 
            int screenHeight = ScreenArea.Height; //屏幕高度

            //指定窗体显示在右上角
            this.Location = new System.Drawing.Point(screenWidth - this.Width, 0); 
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Form加载事件

            // 将PictureBox设为透明，在C#中，控件的透明指对父窗体透明。
            this.pictureBox_shootFocus.BackColor = Color.Transparent;
            this.pictureBox_shootFocus.Parent = this.videoSourcePlayer;

            // 初始化模板数据
            LoadResourceTemplate();

            // 初始化摄像设备
            LoadCamaraDevice();

        }

        // 初始化模板数据
        private void LoadResourceTemplate()
        {
            try
            {
                //显示等待框
                //LoadingHelper.ShowLoadingForm();
                //this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

                // 临时文件路径
                String filePath = Application.StartupPath + "\\data_template.xlsx";
                // 释放资源文件到临时文件
                byte[] data_template = global::StudentPhotoCollection.Properties.Resources.data_template;
                FileStream fs = new FileStream(filePath, FileMode.Create);
                fs.Write(data_template, 0, data_template.Length);
                fs.Close();

                // 加载默认表格数据
                LoadFileData(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("+++++++++初始化模板数据发生未知异常+++++++++");
                Console.WriteLine(e.Message);
                //弹框提示
                MessageBox.Show(e.Message, "初始化模板数据未知异常");

            }
            finally
            {
                //关闭等待框
                //LoadingHelper.CloseLoadingForm();
                //this.Cursor = System.Windows.Forms.Cursors.Default;

            }
        }

        // 初始化摄像设备
        private void LoadCamaraDevice()
        {
            // 先清空摄像设备下拉列表
            this.comboBox_camara.Items.Clear();

            // 初始化摄像设备
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices != null && videoDevices.Count != 0)
            {
                foreach (FilterInfo device in videoDevices)
                {
                    this.comboBox_camara.Items.Add(device.Name);
                }
                //默认选择一个摄像头
                this.comboBox_camara.SelectedIndex = 0;
                ComboBox_camara_SelectedIndexChanged(null, null);
            }
            else
            {
                this.comboBox_camara.Items.Add("重新加载摄像头");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            // 如果已经导入模板，退出时则询问是否需要先导出数据
            if (this.dataGridView.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show("是否已经导出新数据，确定要直接退出吗?", "操作提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    DisposeForm();
                    Dispose();
                    Close();
                    Application.Exit();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                DisposeForm();
            }

        }

        private void DisposeForm()
        {
            //窗体关闭前设置
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                videoSourcePlayer.WaitForStop();
                videoSourcePlayer.VideoSource = null;
            }
            videoDevices = null;
            videoDevice = null;
            videoCapabilities = null;
            currentPicture = null;
            detector = null;

            if(previewForm != null) previewForm.Dispose();
            previewForm = null;

            myImageCodecInfo = null;
            myEncoder = null;
            myEncoderParameter = null;
            myEncoderParameters = null;
    }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            
        }

        private void Button_loadCamara_Click(object sender, EventArgs e)
        {
            // 初始化摄像设备
            LoadCamaraDevice();

        }

        private void TextBox_sjmb_Click(object sender, EventArgs e)
        {
            //导入数据模板
            //选择数据模板
            this.openFileDialog_chooseFile = new OpenFileDialog();
            // 设定打开的文件类型
            openFileDialog_chooseFile.Filter = "Files|*.xls;*.xlsx";
            // 设置打开的路径
            if(this.textBox_sjmb.Text.Length > 0)
            {
                // 打开记忆
                openFileDialog_chooseFile.InitialDirectory = this.textBox_sjmb.Text.Substring(0, this.textBox_sjmb.Text.LastIndexOf("\\") + 1); //获取路径，不带文件名
            }
            else
            {
                // 打开桌面
                openFileDialog_chooseFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            // 如果选定了文件
            if (openFileDialog_chooseFile.ShowDialog() == DialogResult.OK)
            {

                // 取得文件路径及文件名
                string filePath = openFileDialog_chooseFile.FileName;
                // 根据文件路径加载表格数据
                LoadFileData(filePath);

            }
        }

        #region private 加载表格数据
        /// <summary>
        /// 根据文件路径加载表格数据
        /// </summary>
        /// <param name="filePath">excel文件存放的路径</param>
        private void LoadFileData(string filePath)
        {
            try
            {
                //显示等待框
                LoadingHelper.ShowLoadingForm();
                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

                // 显示文件路径及文件名
                this.textBox_sjmb.Text = filePath;
                // 每次打开清空内容
                this.dataGridView.DataSource = null;
                // 读出excel并放入datatable
                string sheetName = this.textBox_excelSheetName.Text;
                // 输出到dataGridView
                this.dataGridView.DataSource = ReadExcelToTable(filePath, sheetName);

                // 初始化拍摄照片自动保存照片名的列、保存文件夹的列
                comboBox_photoName.Items.Clear();
                comboBox_saveFile01.Items.Clear();
                comboBox_saveFile02.Items.Clear();
                for (int i = 0; i < this.dataGridView.Columns.Count; i++)
                {
                    //禁止排序
                    this.dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                    comboBox_photoName.Items.Add(this.dataGridView.Columns[i].HeaderText);
                    //如果有姓名这一列，则直接使用它作为默认的照片名，否则需要人工选择一列名作为图片保存名
                    if (this.dataGridView.Columns[i].HeaderText.Contains("姓名") || this.dataGridView.Columns[i].HeaderText.Contains("名字"))
                    {
                        comboBox_photoName.SelectedIndex = i;
                        ComboBox_photoName_SelectedIndexChanged(null, null);

                    }

                    comboBox_saveFile01.Items.Add(this.dataGridView.Columns[i].HeaderText);
                    //如果有年级这一列，则直接使用它作为默认的文件夹1，否则可以人工选择一列名作为文件夹1（可不选）
                    if (this.dataGridView.Columns[i].HeaderText.Contains("年级"))
                    {
                        comboBox_saveFile01.SelectedIndex = i;
                        ComboBox_saveFile01_SelectedIndexChanged(null, null);
                    }

                    comboBox_saveFile02.Items.Add(this.dataGridView.Columns[i].HeaderText);
                    //如果有班级这一列，则直接使用它作为默认的文件夹2，否则可以人工选择一列名作为文件夹2（可不选）
                    if (this.dataGridView.Columns[i].HeaderText.Contains("班级"))
                    {
                        comboBox_saveFile02.SelectedIndex = i;
                        ComboBox_saveFile02_SelectedIndexChanged(null, null);
                    }

                }

                // 启用导出数据按钮
                this.button_ExportData.Enabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("+++++++++加载表格数据发生未知异常+++++++++");
                Console.WriteLine(e.Message);
                //弹框提示
                MessageBox.Show(e.Message, "加载表格数据未知异常");
            }
            finally
            {
                //关闭等待框
                LoadingHelper.CloseLoadingForm();
                this.Cursor = System.Windows.Forms.Cursors.Default;

            }
        }
        #endregion

        #region private 根据excle的路径把第一个sheet中的内容放入datatable
        /// <summary>
        /// 根据excle的路径把第一个sheet中的内容放入datatable
        /// </summary>
        /// <param name="filePath">excel文件存放的路径</param>
        /// <param name="sheetName">excel工作表名</param>
        /// <returns>DataTable</returns>
        private static DataTable ReadExcelToTable(string filePath, string sheetName)
        {
            try
            {
                if (sheetName == null || sheetName.Length == 0)
                {
                    sheetName = "Sheet1";
                }
                DataTable dataTable = ExcelHelper.ExcelToDataTable(filePath, sheetName, true);

                // 测试打印
                //PrintDataTable(dataTable);

                return dataTable;

            }
            catch (Exception e)
            {
                Console.WriteLine("+++++++++读取Excel发生未知异常+++++++++");
                Console.WriteLine(e.Message);
                //弹框提示
                MessageBox.Show(e.Message, "读取Excel未知异常");

                return null;
            }
        }
        #endregion

        /// <summary>
        /// 打印输出DataTable的值
        /// </summary>
        /// <param name="dataTable"></param>
        static void PrintDataTable(DataTable dataTable)
        {
            if (dataTable == null) return;

            // 打印信息到输出窗口，但是只能在Debug版本运行，到了release版本中，Debug类的函数都会被忽略
            System.Diagnostics.Debug.WriteLine("_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_");
            for (int row = 0; row < dataTable.Rows.Count; row++) //行长度
            {
                for (int column = 0; column < dataTable.Columns.Count; column++) //列长度
                {
                    System.Diagnostics.Debug.Write(dataTable.Rows[row][column] + "  ");
                }
                System.Diagnostics.Debug.WriteLine("\t");
            }
            System.Diagnostics.Debug.WriteLine("-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-");
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //当双击列为最后一列（照片路径）时，打开图片文件
            //MessageBox.Show("RowIndex=" + e.RowIndex + ",ColumnIndex=" + e.ColumnIndex, "CellDoubleClick");
            //非标题栏的行点击
            if (e.RowIndex > -1)
            {
                //双击最后一列（照片路径）时
                if (this.dataGridView.Columns[e.ColumnIndex].HeaderText.Contains("照片路径"))
                {
                    string pathName = "" + this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    //直接打开某路径下的文件或者文件夹
                    if (pathName.Length > 0)
                    {
                        System.Diagnostics.Process.Start(pathName);
                    }
                }
            }
        }

        private void ComboBox_camara_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 选择摄像头
            if (videoDevices != null && videoDevices.Count != 0)
            { 
                videoDevice = null;
                videoDevice = new VideoCaptureDevice(videoDevices[this.comboBox_camara.SelectedIndex].MonikerString);
                // 初始化分辨率
                GetDeviceResolution(videoDevice);
            }
            else
            {
                // 重新加载摄像头
                LoadCamaraDevice();

            }
        }

        #region 根据选择的摄像头初始化所有分辨率
        /// <summary>
        /// 根据选择的摄像头初始化所有分辨率
        /// </summary>
        private void GetDeviceResolution(VideoCaptureDevice videoCaptureDevice)
        {
            //先清空所有分辨率
            this.comboBox_Resolution.Items.Clear();
            videoCapabilities = videoCaptureDevice.VideoCapabilities;

            //将所有分辨率的长和高存入内存
            int index = 0;
            int defaultIndex = 0; //分辨率最高的那个的index
            int maxHeight = 0; //分辨率最高的那个的高度
            foreach (VideoCapabilities capabilty in videoCapabilities)
            {
                if(capabilty.FrameSize.Height > maxHeight)
                {
                    maxHeight = capabilty.FrameSize.Height;
                    defaultIndex = index;
                }
                this.comboBox_Resolution.Items.Add($"{capabilty.FrameSize.Width} x {capabilty.FrameSize.Height}");

                index += 1;
            }
            if(this.comboBox_Resolution.Items.Count > 0)
            {
                //  默认选择分辨率最高的那个
                this.comboBox_Resolution.SelectedIndex = defaultIndex;
                ComboBox_Resolution_SelectedIndexChanged(null, null);
            }
        }
        #endregion

        private void ComboBox_Resolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 选择分辨率

            //是否允许剪裁图片
            //默认检测图片长宽：720x 960（我们需要手机样式的高清图）
            int nowWidth = 720, nowHeight = 960;
            if (videoCapabilities[this.comboBox_Resolution.SelectedIndex].FrameSize.Width >= nowWidth 
                && videoCapabilities[this.comboBox_Resolution.SelectedIndex].FrameSize.Height >= nowHeight)
            {
                this.checkBox_everCutPhoto.Enabled = true;
                this.checkBox_everCutPhoto.Checked = true;
            }
            else
            {
                this.checkBox_everCutPhoto.Enabled = false;
                this.checkBox_everCutPhoto.Checked = false;
            }

        }


        private void ComboBox_saveFile01_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_saveFile01.Items.Count > 0)
            {
                // 选择作为照片名的列
                initLastFilePathName();
            }
        }

        private void ComboBox_saveFile02_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_saveFile02.Items.Count > 0)
            {
                // 选择作为照片名的列
                initLastFilePathName();
            }
        }

        private void ComboBox_photoName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_photoName.Items.Count > 0)
            {
                // 选择作为照片名的列
                initLastFilePathName();
            }
        }

        //点击设置图片保存路径
        private void TextBox_photoFileSavePath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog
            {
                Description = "请选择文件保存路径"
            };
            if (this.textBox_photoFileSavePath.Text.Length > 0)
            {
                //记忆路径
                folderBrowserDialog.SelectedPath = this.textBox_photoFileSavePath.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_photoFileSavePath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void TextBox_photoFileSavePath_TextChanged(object sender, EventArgs e)
        {
            //请选择文件保存路径变化事件
            initLastFilePathName();
        }

        private void initLastFilePathName()
        {
            // 先判断是否已经导入模板数据了
            if(this.dataGridView.Rows.Count == 0)
            {
                return;
            }
            // 显示照片存储路径样式
            string filePath = this.textBox_photoFileSavePath.Text;
            // 照片命名
            string photoName = "";
            if (this.comboBox_photoName.SelectedIndex >= 0) photoName = this.dataGridView.Columns[this.comboBox_photoName.SelectedIndex].HeaderText;

            if (filePath.Length > 0 && photoName.Length > 0)
            {
                this.label_lastFilePathName.Visible = true;
                
                // 照片存储文件夹1命名
                string file1Name = "";
                if(this.comboBox_saveFile01.SelectedIndex >= 0) file1Name = this.dataGridView.Columns[this.comboBox_saveFile01.SelectedIndex].HeaderText;
                // 照片存储文件夹2命名
                string file2Name = "";
                if (this.comboBox_saveFile02.SelectedIndex >= 0) file2Name = this.dataGridView.Columns[this.comboBox_saveFile02.SelectedIndex].HeaderText;

                if (file1Name.Length > 0)
                {
                    filePath += @"\" + file1Name;

                }
                if (file2Name.Length > 0)
                {
                    filePath += @"\" + file2Name;
                }

                this.label_lastFilePathName.Text = "照片路径样式：" + filePath + @"\" + photoName + ".png";
            }
            else
            {
                this.label_lastFilePathName.Visible = false;
            }

        }

        //是否需要自动剪裁图片720x960
        private void CheckBox_everCutPhoto_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Button_Connect_Click(object sender, EventArgs e)
        {
            if (videoDevices == null || videoDevices.Count == 0)
            {
                // 初始化摄像设备
                LoadCamaraDevice();
                //不阻断
            }

            // 是否选择导入模板
            if (this.dataGridView.Rows.Count == 0)
            {

                MessageBox.Show("请先选择数据模板", "温馨提示");
                return;
            }

            // 是否选择文件保存路径
            if (this.textBox_photoFileSavePath.Text.Length == 0)
            {
                MessageBox.Show("请先选择文件保存路径", "温馨提示");
                return;
            }

            // 是否选择照片名称列
            if (this.comboBox_photoName.SelectedItem == null)
            {

                MessageBox.Show("请先选择照片名称列", "温馨提示");
                return;
            }

            // 连接摄像头（打开拍照FORM）
            if (this.videoDevice != null)
            {
                if ((videoCapabilities != null) && (videoCapabilities.Length != 0))
                {
                    this.videoDevice.VideoResolution = videoCapabilities[this.comboBox_Resolution.SelectedIndex];

                    // 更新组件状态
                    EnableControlStatus(false);

                    // 打开摄像头
                    this.videoSourcePlayer.VideoSource = this.videoDevice;
                    this.videoSourcePlayer.Start();

                    //AForge控件中的NewFrame事件获取要显示的每一帧的图像，做人脸标记处理
                    this.videoSourcePlayer.NewFrame += VideoSourcePlayer_NewFrame;
                }
            }
            else
            {
                MessageBox.Show("请先选择摄像头", "温馨提示");
            }
        }

        private void VideoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            //AForge控件中的NewFrame回调事件获取要显示的每一帧的图像


            //留存原始的照片，如果正在拍照，则保存此未添加人脸识别框的照片
            currentPicture = (Bitmap)image.Clone();

            //人脸检测并标记
            image = FacePicDetect(image); //必须将返回值重新赋给image，否则摄像界面不会回显人脸标记框

        }

        private Bitmap FacePicDetect(Bitmap bitmap)
        {
            lock (this)
            {

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

            // 更新组件状态
            EnableControlStatus(true);

        }

        private void Button_Disconnect_Click(object sender, EventArgs e)
        {
            // 断开摄像头
            DisConnect();

        }

        #region 连接或断开连接时更新各组件状态
        /// <summary>
        /// 连接或断开连接时更新各组件状态
        /// </summary>
        /// <param name="status">状态</param>
        public void EnableControlStatus(bool status)
        {
            this.comboBox_camara.Enabled = status;
            this.comboBox_Resolution.Enabled = status;
            this.button_Connect.Enabled = status;
            this.button_Disconnect.Enabled = !status;
            this.button_Shoot.Enabled = !status;
        }
        #endregion

        //是否需要立即检查拍摄照片
        private void CheckBox_everCheckPhoto_CheckedChanged(object sender, EventArgs e)
        {

        }

        //拍照
        private void Button_shoot_Click(object sender, EventArgs e)
        {
            // 文件保存路径
            string filePath = this.textBox_photoFileSavePath.Text;
            if (filePath.Length == 0)
            {
                MessageBox.Show("请先选择文件保存路径", "温馨提示");
                return;
            }

            //当前的照片帧
            Bitmap bitmap = currentPicture ?? videoSourcePlayer.GetCurrentVideoFrame();

            //允许并满足剪裁成合适大小的图片
            if (this.checkBox_everCutPhoto.Enabled && this.checkBox_everCutPhoto.Checked)
            {
                bitmap = CutCurrentPicture(bitmap);
            }

            if(this.checkBox_everCheckPhoto.Enabled && this.checkBox_everCheckPhoto.Checked)
            {
                //检测图片并且指定是否需要重拍
                CheckShootPhoto(bitmap);
            }
            else
            {
                //设置并保存照片
                SaveShootPhoto(bitmap);
            }

        }

        //先剪裁成合适大小的图片
        private Bitmap CutCurrentPicture(Bitmap bitmap)
        {
            //默认检测图片长宽：720x 960（我们需要手机样式的高清图）
            int nowWidth = 720, nowHeight = 960;
            
            //已焦点位置为初始剪裁坐标，并且像素本身就比较高的
            if (bitmap.Width >= nowWidth 
                && bitmap.Height >= nowHeight)
            {
                //从人脸位置坐标为中心向外辐射，辐射后的图片长宽是否在原图内，否则不予裁剪
                try
                {
                    Bitmap newbm = new Bitmap(nowWidth, nowHeight);
                    Graphics g = Graphics.FromImage(newbm);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;


                    int x = bitmap.Width/2 - newbm.Width/2;
                    int y = bitmap.Height / 2 - newbm.Height / 2;

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;

                    g.DrawImage(bitmap, new Rectangle(0, 0, nowWidth, nowHeight), new Rectangle(x, y, nowWidth, nowHeight), GraphicsUnit.Pixel);
                    g.Dispose();

                    return newbm;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("+++++++++剪裁图片发生未知异常+++++++++");
                    Console.WriteLine(ex.Message);
                    //弹框提示
                    MessageBox.Show(ex.Message, "剪裁图片异常");
                }
            }
            return bitmap;
        }

        //检测图片并且指定是否需要重拍
        private void CheckShootPhoto(Bitmap bitmap)
        {
            try
            {
                // 当前选中行Index态
                int rowIndex = this.dataGridView.SelectedRows[0].Index;
                // 照片命名
                string photoName = "" + this.dataGridView.Rows[rowIndex].Cells[this.comboBox_photoName.SelectedIndex].Value;
                if (photoName.Length == 0)
                {
                    photoName = "无名氏" + GetTimeStamp();
                }

                //直接检测图片并且指定是否需要重拍
                if (previewForm != null)
                {
                    previewForm.Dispose();
                    previewForm = null;
                }

                previewForm = new PreviewForm(bitmap, "当前照片检测 -- " + photoName, true);

                previewForm.Show();
                previewForm.Owner = this;

            }
            catch(Exception ex)
            {
                Console.WriteLine("+++++++++当前照片检测准备发生未知异常+++++++++");
                Console.WriteLine(ex.Message);
                //弹框提示
                MessageBox.Show(ex.Message, "当前照片检测准备异常");
            }
        }

        //保存拍摄的照片，并位移1
        public void SaveShootPhoto(Bitmap bitmap)
        {
            try
            {
                //设置上张照片和当前照片
                if (this.pictureBox_photo.Image != null)
                {
                    this.pictureBox_photoLast.Image = this.pictureBox_photo.Image;
                    this.groupBox_photoPreviewLast.Text = this.groupBox_photoPreview.Text.Replace("当前", "上张");
                }
                //当前照片
                this.pictureBox_photo.Image = bitmap;

                // 当前选中行Index态
                int rowIndex = this.dataGridView.SelectedRows[0].Index;
                // 照片命名
                string photoName = "" + this.dataGridView.Rows[rowIndex].Cells[this.comboBox_photoName.SelectedIndex].Value;
                if(photoName.Length == 0)
                {
                    photoName = "无名氏" + GetTimeStamp();
                }

                //照片预览标题
                this.groupBox_photoPreview.Text = "当前照片预览 -- " + photoName;


                // 文件保存路径
                string filePath = this.textBox_photoFileSavePath.Text;

                // 照片存储文件夹1命名
                string file1Name = "";
                if(this.comboBox_saveFile01.SelectedIndex >= 0) file1Name = "" + this.dataGridView.Rows[rowIndex].Cells[this.comboBox_saveFile01.SelectedIndex].Value;
                // 照片存储文件夹2命名
                string file2Name = "";
                if(this.comboBox_saveFile02.SelectedIndex >= 0) file2Name = "" + this.dataGridView.Rows[rowIndex].Cells[this.comboBox_saveFile02.SelectedIndex].Value;

                if (file1Name.Length > 0)
                {
                    filePath += @"\" + file1Name;
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                        Console.WriteLine(filePath + "创建文件夹完毕");
                    }
                }
                if (file2Name.Length > 0)
                {
                    filePath += @"\" + file2Name;
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                        Console.WriteLine(filePath + "创建文件夹完毕");
                    }
                }

                // 存储照片路径
                string pathName = filePath + @"\" + photoName + ".png";


                // 存储高质量照片
                if (SaveHighlyQualityPicture(bitmap, pathName))
                {
                    //更新最后两列的状态为已拍照
                    this.dataGridView.Rows[rowIndex].Cells[this.dataGridView.Rows[rowIndex].Cells.Count - 2].Value = "已拍照";
                    //更新最后一列的路径为照片路径
                    this.dataGridView.Rows[rowIndex].Cells[this.dataGridView.Rows[rowIndex].Cells.Count - 1].Value = pathName;

                    // 自动位移，下一个待拍照的照片名
                    // 设定下一行为选中状态
                    rowIndex += 1;
                    if (rowIndex >= this.dataGridView.Rows.Count)
                    {
                        //到底回到第一条
                        rowIndex = 0;
                    }
                    this.dataGridView.Rows[rowIndex].Selected = true;
                    //下面这一行是最关键的，不然永远会回到第一行
                    this.dataGridView.CurrentCell = this.dataGridView.Rows[rowIndex].Cells[0];
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("+++++++++设置拍摄的照片发生未知异常+++++++++");
                Console.WriteLine(ex.Message);
                //弹框提示
                MessageBox.Show(ex.Message, "设置拍摄的照片异常");
            }
        }

        //保存高质量的图片
        public bool SaveHighlyQualityPicture(Bitmap bitmap, string pathName)
        {

            // Get an ImageCodecInfo object that represents the PNG codec.
            myImageCodecInfo = GetEncoderInfo("image/png");

            // for the Quality parameter category.
            myEncoder = Encoder.Quality;

            // EncoderParameter object in the array.
            myEncoderParameters = new EncoderParameters(1);

            //设置质量 数字越大质量越好，但是到了一定程度质量就不会增加了，MSDN上没有给范围，只说是32为非负整数
            myEncoderParameter = new EncoderParameter(myEncoder, 1000000L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            //保存图片
            try
            {
                bitmap.Save(pathName, myImageCodecInfo, myEncoderParameters);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("+++++++++保存拍摄的照片发生未知异常+++++++++");
                Console.WriteLine(ex.Message);
                //弹框提示
                MessageBox.Show(ex.Message, "保存拍摄的照片异常");
            }
            finally
            {
                //释放
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
                myEncoderParameter = null;
                myEncoderParameters = null;
            }
            return false;

        }

        //获得编码器的函数
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }


        //点击预览原图-当前
        private void PictureBox_photo_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_photo.Image == null)
            {

                MessageBox.Show("无当前照片可预览", "温馨提示");
                return;
            }

            if (previewForm != null)
            {
                previewForm.Dispose();
                previewForm = null;
            }

            previewForm = new PreviewForm(this.pictureBox_photo.Image, this.groupBox_photoPreview.Text);

            previewForm.ShowDialog();

        }

        //点击预览原图-上张
        private void PictureBoxLast_photo_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_photoLast.Image == null)
            {

                MessageBox.Show("无上张照片可预览", "温馨提示");
                return;
            }

            if (previewForm != null)
            {
                previewForm.Dispose();
                previewForm = null;
            }

            previewForm = new PreviewForm(this.pictureBox_photoLast.Image, this.groupBox_photoPreviewLast.Text);

            previewForm.ShowDialog();
        }

        //点击导出DataGridView数据到Excel
        private void Button_ExportData_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.Rows.Count == 0)
            {

                MessageBox.Show("无数据可以导出", "温馨提示");
                return;
            }

            saveFileDialog = new SaveFileDialog
            {
                //设置文件类型
                Filter = "Files|*.xls;*.xlsx",
                //设置默认文件类型显示顺序
                FilterIndex = 1,
                //保存对话框是否记忆上次打开的目录
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string localFilePath = saveFileDialog.FileName.ToString(); //获得文件路径
                    string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径

                    DataTable dataTable = GetDgvToTable(this.dataGridView);

                    //保存路径，使用原数据模板文件
                    string filePath = localFilePath;
                    if (!File.Exists(filePath))
                    {
                        FileStream fs = File.Create(filePath);
                        fs.Close();
                        Console.WriteLine(filePath + "创建文件完毕");
                    }
                    //把DataTable写入到excel文件中
                    int writeCount = ExcelHelper.DataTableToExcel(filePath, dataTable, this.textBox_excelSheetName.Text, true);
                    Console.WriteLine("成功写入" + writeCount + "行");

                    //释放对象
                    dataTable.Dispose();

                    MessageBox.Show(fileNameExt + "文件保存成功", "温馨提示");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("+++++++++文件保存发生未知异常+++++++++");
                    Console.WriteLine(ex.Message);
                    //弹框提示
                    MessageBox.Show(ex.Message, "文件保存异常");
                }
                
            }

            

        }

        //从DataGridView控件数据，转成获取DataTable
        private DataTable GetDgvToTable(DataGridView dgv)
        {
            DataTable dt = new DataTable();
            for (int count = 0; count < dgv.Columns.Count; count++)
            {
                DataColumn dc = new DataColumn(dgv.Columns[count].Name.ToString());
                dt.Columns.Add(dc);
            }
            for (int count = 0; count < dgv.Rows.Count; count++)
            {
                DataRow dr = dt.NewRow();
                for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                {
                    dr[countsub] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }


        private void VideoSourcePlayer_DoubleClick(object sender, EventArgs e)
        {

        }

        private void PictureBox_showMsg1_Click(object sender, EventArgs e)
        {

            MessageBox.Show("必须选择一个路径，拍照后自动生成的文件夹或照片则保存在此路径，如桌面。", "选择文件保存路径？");
        }

        private void PictureBox_showMsg2_Click(object sender, EventArgs e)
        {

            MessageBox.Show("可以从下拉列表选择1到2个列名作为图片保存的文件夹，如\\年级\\班级\\。这些列来自于左侧表格标题栏。", "选择照片文件夹列？");
        }

        private void PictureBox_showMsg3_Click(object sender, EventArgs e)
        {

            MessageBox.Show("必须从下拉列表选择1个列名作为图片名称，如姓名.png。这些列来自于左侧表格标题栏。", "选择照片名称列？");
        }

        private void PictureBox_showMsg4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("选择的分辨率大于等于720x960时，可以开启自动裁剪照片为720x960大小的功能。", "自动裁剪功能？");
        }

        private void PictureBox_showMsg5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("excel文件里一般有一个工作表Sheet1，所以默认填写Sheet1即可，程序就会导入此工作表的内容。如果需要导入其他工作表，则设置工作表为对应的名字即可。", "设置工作表？");
        }

        private void PictureBox_showMsg6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("分辨率列表来自于所选摄像头所支持的分辨率，如果列表为空，请去电脑-管理-设备管理器-摄像头-更新驱动，再重启电脑。", "选择分辨率？");
        }

        private void PictureBox_showMsg7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("选中，则每次点击拍照按钮后，会立即预览拍摄的照片，检查拍照结果。", "检查拍摄照片？");
        }
    }
}
