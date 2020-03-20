using System;
using System.Threading;

namespace StudentPhotoCollection
{
    class LoadingHelper
    {
        
        #region 相关变量定义
        /// <summary>
        /// 定义委托进行窗口关闭
        /// </summary>
        private delegate void CloseDelegate();
        private static LoadingForm loadingForm;
        private static readonly Object syncLock = new Object();  //加锁使用

        #endregion

        /// <summary>
        /// 显示loading框，使用者调用
        /// </summary>
        public static void ShowLoadingForm()
        {
            // Make sure it is only launched once.
            if (loadingForm != null)
                return;

            Thread thread;
            try
            {
                thread = new Thread(new ThreadStart(LoadingHelper.ShowForm))
                {
                    IsBackground = true
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            catch (Exception)
            {
                thread = null;
            }

        }


        /// <summary>
        /// 关闭loading框，使用者调用
        /// </summary>
        public static void CloseLoadingForm()
        {
            Thread.Sleep(50); //可能到这里线程还未起来，所以进行延时，可以确保线程起来，彻底关闭窗口
            if (loadingForm != null)
            {
                lock (syncLock)
                {
                    Thread.Sleep(50);
                    if (loadingForm != null)
                    {
                        Thread.Sleep(50);  //通过三次延时，确保可以彻底关闭窗口
                        loadingForm.Invoke(new CloseDelegate(LoadingHelper.CloseForm));
                    }
                }
            }
        }


        /// <summary>
        /// 显示窗口
        /// </summary>
        private static void ShowForm()
        {
            LoadingHelper.CloseForm();

            loadingForm = new LoadingForm();
            loadingForm.ShowDialog();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private static void CloseForm()
        {

            if (loadingForm != null)
            {
                loadingForm.CloseForm();
                loadingForm = null;
            }

        }


    }
}
