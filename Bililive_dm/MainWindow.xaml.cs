using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Serialization;
using XiguaDanmakuHelper;

namespace Bililive_dm
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private uint[] abc = { 0, 0, 0 };

        private readonly Queue<MessageModel> _danmakuQueue = new Queue<MessageModel>();

        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();

        private readonly Thread ProcDanmakuThread;

        private readonly ObservableCollection<SessionItem> SessionItems = new ObservableCollection<SessionItem>();

        private readonly DispatcherTimer timer;
        private Api b;
        private IDanmakuWindow fulloverlay;
        private Thread getDanmakuThread;
        public MainOverlay overlay;
        private readonly Thread releaseThread;

        private StoreModel settings;

        private readonly string version = "2.2.0.9";

        public ConfigData ConfigData = new ConfigData();
        public Logger Logger = new Logger();

        public string[] BlackList;

        public MainWindow()
        {

            InitializeComponent();

            Info.Text += version;
            //初始化日志
            //LiverName.Text = "挂神";
            b = new Api();
            overlay_enabled = true;
            OpenOverlay();
            overlay.Show();

            Closed += MainWindow_Closed;

            Api.OnMessage += b_ReceivedDanmaku;
            Api.OnLeave += OnLiveStop;
            //            b.OnMessage += ProcDanmaku;
            Api.LogMessage += b_LogMessage;
            Api.OnRoomCounting += b_ReceivedRoomCount;

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                Dispatcher);
            timer.Start();

            Log.DataContext = _messageQueue;

            releaseThread = new Thread(() =>
            {
                while (true)
                {
                    Utils.ReleaseMemory(true);
                    Thread.Sleep(30 * 1000);
                }
            });
            releaseThread.IsBackground = true;
            getDanmakuThread = new Thread(() =>
            {
                while (true)
                    if (b.isLive)
                    {
                        b.GetDanmaku();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(100000);
                    }
            });
            getDanmakuThread.IsBackground = true;
            //            releaseThread.Start();
            ProcDanmakuThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_danmakuQueue)
                    {
                        var count = 0;
                        if (_danmakuQueue.Any()) count = (int)Math.Ceiling(_danmakuQueue.Count / 30.0);

                        for (var i = 0; i < count; i++)
                            if (_danmakuQueue.Any())
                            {
                                var danmaku = _danmakuQueue.Dequeue();
                                ProcDanmaku(danmaku);
                            }
                    }

                    Thread.Sleep(25);
                }
            })
            {
                IsBackground = true
            };
            ProcDanmakuThread.Start();

            for (var i = 0; i < 100; i++) _messageQueue.Add("");
            logging("可以点击日志复制到剪贴板");

            Loaded += MainWindow_Loaded;
            Setting.GetConfig += GetConfig;
            Setting.SetConfig += SetConfig;
            Landu();
            System.IO.Directory.CreateDirectory("log");//创造日志文件夹
            try
            {
                var j = JObject.Parse(Config.Read());
                LiverName.Text = ConfigData.Room = (string)j["Room"];
                showBrand.IsChecked = ConfigData.ShowBrand = (bool)j["ShowBrand"];
                showGrade.IsChecked = ConfigData.ShowGrade = (bool)j["ShowGrade"];
                showChat.IsChecked = ConfigData.ShowChat = (bool)j["ShowChat"];
                showPresent.IsChecked = ConfigData.ShowPresent = (bool)j["ShowPresent"];
                showLike.IsChecked = ConfigData.ShowLike = (bool)j["ShowLike"];
                danMu.IsChecked = ConfigData.DanMu = (bool)j["DanMu"];
                ConfigData.spd = (int)j["spd"];
                ConfigData.pit = (int)j["pit"];
                ConfigData.vol = (int)j["vol"];
                ConfigData.per = (int)j["per"];
                ConfigData.CanUpdate = (bool)j["CanUpdate"];
                ConfigData.DeBug = (bool)j["DeBug"];
                ConfigData.BlackList = (string)j["BlackList"];
                ConfigData.maxCapacity = (int)j["maxCapacity"];

            }
            catch
            {
                logging("配置文件损坏或不存在，已生成新的配置文件。");
                ConfigData = new ConfigData();
                Config.Write(ConfigData);
                LiverName.Text = ConfigData.Room;
                showBrand.IsChecked = ConfigData.ShowBrand;
                showGrade.IsChecked = ConfigData.ShowGrade;
                showChat.IsChecked = ConfigData.ShowChat;
                showPresent.IsChecked = ConfigData.ShowPresent;
                showLike.IsChecked = ConfigData.ShowLike;
                danMu.IsChecked = ConfigData.DanMu;
            }
            BlackList = ConfigData.BlackList.Split('|');
            new Thread(() =>
            {
                try
                {
                    var data = Common.HttpGet("http://vps.guation.cn:8080/Status");
                    var json = JObject.Parse(data);
                    if (ConfigData.CanUpdate)//是否检查更新
                    {
                        var version1 = version.Split('.');
                        var version2 = json["version"].ToString().Split('.');
                        for (var i = 0; i < 2; i++)
                        {
                            try
                            {
                                if (int.Parse(version1[i]) < int.Parse(version2[i]))
                                {
                                    logging($"检测到版本更新，当前版本{version}，最新版本{(string)json["version"]}，新版本简介：{(string)json["update"]}。");
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                logging("检查更新失败，不影响软件使用。");
                            }
                        }
                    }
                    if (json["msg"].ToString() != "") logging("公告：" + json["msg"].ToString());
                }
                catch (Exception)
                {
                    logging("获取服务器信息失败，弹幕朗读功能可能会受影响。");
                }
            }).Start();
        }

        private void b_LogMessage(string e)
        {
            logging(e);
        }

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var sc = Log.Template.FindName("LogScroll", Log) as ScrollViewer;
            sc?.ScrollToEnd();
            showChat.IsChecked = ConfigData.ShowChat;
            showPresent.IsChecked = ConfigData.ShowPresent;
            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                            IsolatedStorageScope.Domain |
                                                            IsolatedStorageScope.Assembly, null, null);
                var settingsreader =
                    new XmlSerializer(typeof(StoreModel));
                var reader = new StreamReader(new IsolatedStorageFileStream(
                    "settings.xml", FileMode.Open, isoStore));
                settings = (StoreModel)settingsreader.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                settings = new StoreModel();
            }

            settings.SaveConfig();
            settings.toStatic();
            OptionDialog.LayoutRoot.DataContext = settings;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ConfigData.Room = LiverName.Text.Trim();
            Config.Write(ConfigData);
        }

        ~MainWindow()
        {
            if (fulloverlay != null)
            {
                fulloverlay.Dispose();
                fulloverlay = null;
            }
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (fulloverlay != null) fulloverlay.ForceTopmost();
            if (overlay != null)
            {
                overlay.Topmost = false;
                overlay.Topmost = true;
            }
        }

        private void OpenOverlay()
        {
            overlay = new MainOverlay();
            overlay.Deactivated += overlay_Deactivated;
            overlay.SourceInitialized += delegate
            {
                var hwnd = new WindowInteropHelper(overlay).Handle;
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            overlay.Background = Brushes.Transparent;
            overlay.ShowInTaskbar = false;
            overlay.Topmost = true;
            overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            overlay.Height = SystemParameters.WorkArea.Height;
            overlay.Width = Store.MainOverlayWidth;
        }

        private void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay) (sender as MainOverlay).Topmost = true;
        }

        private async void connbtn_Click(object sender, RoutedEventArgs e)
        {
            ConfigData.Room = LiverName.Text.Trim();
            Config.Write(ConfigData);
            b = new Api(ConfigData.Room);

            logging("是否为房间号" + b.isRoomID.ToString(),"debug");

            ConnBtn.IsEnabled = false;
            DisconnBtn.IsEnabled = false;
            var connectresult = false;
            logging("正在连接");
            connectresult = await b.ConnectAsync();
            if (connectresult)
            {
                logging("連接成功");
                AddDMText("提示", "連接成功", true);
                getDanmakuThread.Start();
                logging(b.RoomID.ToString(),"debug");
            }
            else
            {
                logging("連接失敗");
                AddDMText("提示", "連接失敗", true);
                ConnBtn.IsEnabled = true;
            }
            if (b.isRoomID)
                LiverName.Text = b.liverName.ToString();
            else
                LiverName.Text = b.user.ToString();
            DisconnBtn.IsEnabled = true;
        }

        public void b_ReceivedRoomCount(long popularity)
        {
            //            logging("當前房間人數:" + e.UserCount);
            //            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
            if (CheckAccess())
            {
                OnlinePopularity.Text = popularity.ToString();
                //AddDMText("当前房间人气", popularity.ToString() + "", true);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { OnlinePopularity.Text = popularity.ToString(); }));
            }
        }

        public void b_ReceivedDanmaku(MessageModel e)
        {
            lock (_danmakuQueue)
            {
                _danmakuQueue.Enqueue(e);
            }
        }

        private void ProcDanmaku(MessageModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MessageEnum.Chat:
                    if (ConfigData.ShowChat)
                    {
                        logging(danmakuModel.ChatModel.ToString());
                        Hecheng(DelEmoji.filterEmoji(danmakuModel.ChatModel.content));
                        //经过多次测试发现弹幕中部分emoji不能被语音合成程序处理而导致主线程阻塞引发程序崩溃
                        //即使未引发崩溃也会导致程序不能继续正常工作出现假死状态
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText(danmakuModel.ChatModel.user, danmakuModel.ChatModel.content);
                        }));
                    }
                    break;
                case MessageEnum.Gifting:
                    break;
                case MessageEnum.Gift:
                    {
                        if (ConfigData.ShowPresent)
                        {
                            logging("收到礼物 : " + danmakuModel.GiftModel.user + " 赠送的 " + danmakuModel.GiftModel.count +
                                    " 个 " + danmakuModel.GiftModel.GetName());
                            Hecheng("感谢" + danmakuModel.GiftModel.user + " 赠送的 " + danmakuModel.GiftModel.count +
                                    " 个 " + danmakuModel.GiftModel.GetName());
                            Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText("收到礼物",
                                danmakuModel.GiftModel.ToString(), true);
                        }));
                        }
                        break;
                    }
                case MessageEnum.Join:
                    {
                        if (ConfigData.ShowPresent)
                        {
                            logging("粉丝团新成员 : 欢迎 " + danmakuModel.UserModel + " 加入了粉丝团");
                            Hecheng("欢迎 " + danmakuModel.UserModel + " 加入了粉丝团");
                            Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText("粉丝团新成员",
                                "欢迎" + danmakuModel.UserModel + "加入了粉丝团", true);
                        }));
                        }
                        break;
                    }
                case MessageEnum.Like:
                    {
                        if (ConfigData.ShowLike)
                        {
                            logging($"用户 {danmakuModel.UserModel} 点了喜欢");
                            AddDMText("点亮",
                                "用户" + danmakuModel.UserModel + "点了喜欢", true);
                        }
                        break;
                    }
            }
        }

        public void logging(string text, string level = "info")
        {
            if (Log.Dispatcher.CheckAccess())
                lock (_messageQueue)
                {
                    var time = DateTime.Now.ToString("T");
                    while (_messageQueue.Count >= 100) _messageQueue.RemoveAt(0);
                    if (level == "debug" && ConfigData.DeBug)
                    {
                        _messageQueue.Add($"[{time}][debug]{text}");
                    }
                    else if (level == "info")
                    {
                        _messageQueue.Add($"[{time}]{text}");
                    }
                    Logger.SaveLog($"[{time}]{text}", level);
                }
            else
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text, level)));
        }

        public void AddDMText(string notify, string text, bool warn = false)
        {
            if (!overlay_enabled) return;
            if (Dispatcher.CheckAccess())
            {
                var c = new DanmakuTextControl();

                c.UserName.Text = notify;
                if (warn) c.UserName.Foreground = Brushes.Red;
                c.Text.Text = text;
                c.ChangeHeight();
                var sb = (Storyboard)c.Resources["Storyboard1"];
                //Storyboard.SetTarget(sb,c);
                sb.Completed += sb_Completed;
                overlay.LayoutRoot.Children.Add(c);
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(notify, text, warn)));
            }
        }

        public void AddDMText(User user, string text)
        {
            if (!overlay_enabled) return;
            if (Dispatcher.CheckAccess())
            {
                var c = new DanmakuTextControl();

                c.UserName.Text = user.ToString();
                c.Text.Text = text;
                c.ChangeHeight();
                var sb = (Storyboard)c.Resources["Storyboard1"];
                //Storyboard.SetTarget(sb,c);
                sb.Completed += sb_Completed;
                overlay.LayoutRoot.Children.Add(c);
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user, text)));
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[2].Timeline) as DanmakuTextControl;
            if (c != null) overlay.LayoutRoot.Children.Remove(c);
        }

        public void Test_OnClick(object sender, RoutedEventArgs e)
        {
            AddDMText("提示", "這是一個測試😀😭", true);
        }

        private void Setting_OnClick(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            //Setting.GetConfig += GetConfig;
            //Setting.SetConfig += SetConfig;
            setting.ShowDialog();
            ConfigData.Room = LiverName.Text.Trim();
            if (ConfigData.BlackList != "") BlackList = ConfigData.BlackList.Split('|');
                
        }
        private void OnLiveStop()
        {
            logging("提示：主播已下播");
            Disconnbtn_OnClick(this, new RoutedEventArgs());
        }

        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            abc[0] = 0;//清除弹幕统计
            abc[1] = 0;//清除计划任务
            abc[2] = 0;//关闭弹幕朗读
            ConnBtn.IsEnabled = true;
            getDanmakuThread.Abort();
            getDanmakuThread = new Thread(() =>
            {
                while (true)
                    if (b.isLive)
                    {
                        b.GetDanmaku();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(100000);
                    }
            })
            { IsBackground = true };
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is TextBlock textBlock)
                {
                    Clipboard.SetText(textBlock.Text);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => { MessageBox.Show("本行记录已复制到剪贴板"); }));
                }
            }
            catch (Exception)
            {
            }
        }

        // 合成
        public void Hecheng(string wenzi)
        {
            if (ConfigData.DanMu)
            {
                if (ConfigData.BlackList != "")
                {
                    foreach (var Black in BlackList)
                    {
                        var black = Black.Trim();
                        try
                        {
                            if (Regex.IsMatch(wenzi, black)) return;
                        }
                        catch(Exception err)
                        {
                            logging(err.ToString(),"debug");
                        }
                        
                    }
                }
                var url = $"http://vps.guation.cn:8080/?msg={wenzi}&spd={ConfigData.spd}&pit={ConfigData.pit}&vol={ConfigData.vol}&per={ConfigData.per}";
                if (Common.HttpDownload(url, "tmp/" + abc[0] + ".mp3"))
                {
                    abc[0]++;
                    abc[2]++;
                }
            }
        }

        private void Landu()
        {
            Mp3Player mp3Player = new Mp3Player();
            Thread td = new Thread((ThreadStart)delegate //不在主线程运行时无法打开音频文件需要进行委托 https://zhidao.baidu.com/question/1988707588257169467.html
            {
                while (true)
                {
                    if (abc[2] > 0)
                    {
                        mp3Player.AutoPlay("tmp/" + abc[1] + ".mp3");
                        abc[1]++;
                        abc[2]--;
                    }
                    if (abc[2] > ConfigData.maxCapacity)
                    {
                        abc[1] = abc[0] - 1;
                        abc[2] = 1;//朗读最后一条弹幕
                        logging("弹幕缓存上限已跳过朗读部分弹幕。");
                    }
                    Thread.Sleep(200);
                }
            });
            td.SetApartmentState(ApartmentState.STA);
            td.IsBackground = true;
            td.Start();
        }

        private string Runcmd(string str)//自动升级暂留接口
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            string output = p.StandardOutput.ReadToEnd();//获取cmd窗口的输出信息
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            return output;
        }

        public void SetConfig(ConfigData configData)
        {
            ConfigData = configData;
            Config.Write(ConfigData);
        }
        public ConfigData GetConfig()
        {
            return ConfigData;
        }

        #region Runtime settings

        private readonly bool overlay_enabled = true;

        #endregion

        private void ShowChat_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowChat = false;
        }

        private void showPresent_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowPresent = false;
        }

        private void showPresent_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowPresent = true;
        }

        private void showChat_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowChat = true;
        }

        private void showBrand_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowBrand = User.showBrand = true;
        }

        private void showBrand_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowBrand = User.showBrand = false;
        }
        private void showGrade_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowGrade = true;
        }
        private void showGrade_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowGrade = false;
        }

        private void ShowLike_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowLike = true;
        }
        private void ShowLike_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowLike = false;
        }
        private void Danmu_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.DanMu = true;
        }
        private void Danmu_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.DanMu = false;
        }

    }
}
