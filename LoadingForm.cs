using System;
using System.Windows.Forms;

namespace StudentPhotoCollection
{
    public partial class LoadingForm : Form
    {

        public LoadingForm()
        {
            InitializeComponent();
        }

        public void CloseForm()
        {
            if (this.InvokeRequired)
            {
                //这里利用委托进行窗体的操作，避免跨线程调用时抛异常，后面给出具体定义
                CONSTANTDEFINE.SetUISomeInfo UIinfo = new CONSTANTDEFINE.SetUISomeInfo(new Action(() =>
                {
                    while (!this.IsHandleCreated)
                    {
                        ;
                    }
                    if (!this.IsDisposed)
                    {
                        this.Dispose();
                    }
                    this.Close();
                }));
                this.Invoke(UIinfo);
            }
            else
            {
                if (!this.IsDisposed)
                {
                    this.Dispose();
                }
                this.Close();
            }
        }

        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.IsDisposed)
            {
                this.Dispose(true);
            }
        }


    }

    //定义一个委托类
    class CONSTANTDEFINE
    {
        public delegate void SetUISomeInfo();
    }

}
