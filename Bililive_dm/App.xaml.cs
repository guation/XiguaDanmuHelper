using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Bililive_dm
{
    /// <summary>
    ///     App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private System.Threading.Mutex mutex;
        public App()
        {
            AddArchSpecificDirectory();
            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Startup += new StartupEventHandler(App_Startup);
        }

        [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        private void AddArchSpecificDirectory()
        {
            var archPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                IntPtr.Size == 8 ? "x64" : "Win32");
            SetDllDirectory(archPath);
        }

        private void App_DispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給我 ");
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                using (var outfile = new StreamWriter(path + @"\彈幕姬錯誤報告.txt"))
                {
                    outfile.WriteLine("請有空發給我，謝謝");
                    outfile.WriteLine(DateTime.Now + "");
                    outfile.Write(e.Exception.ToString());
                }
            }
            catch (Exception)
            {
            }

            Environment.Exit(1);
        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            string path = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/");
            mutex = new System.Threading.Mutex(true, path, out bool ret);

            if (!ret)
            {
                MessageBox.Show("弹幕姬无法在同一目录下多次运行。" + path);
                Environment.Exit(0);
            }

        }
    }
}