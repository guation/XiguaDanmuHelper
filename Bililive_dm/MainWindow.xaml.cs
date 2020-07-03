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
//using XiguaDanmakuHelper;
using GT_XiguaAPI;
using System.Text;

namespace Bililive_dm
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        //private uint[] abc = { 0, 0, 0 };

        private readonly Queue<MessageModel> _danmakuQueue = new Queue<MessageModel>();

        private readonly Queue<string> DanmuHecheng = new Queue<string>();

        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();

        private readonly Thread ProcDanmakuThread;

        private readonly ObservableCollection<SessionItem> SessionItems = new ObservableCollection<SessionItem>();

        private readonly DispatcherTimer timer;
        //private Api b;
        private XiguaAPI b;
        private IDanmakuWindow fulloverlay;
        private Thread getDanmakuThread;
        public MainOverlay overlay;
        private readonly Thread releaseThread;

        private StoreModel settings;

        public const string version = "3.0.4.19";

        public ConfigData ConfigData = new ConfigData();
        public Logger Logger = new Logger();

        public string[] BlackList;

        public bool isSaveToggle = false;

        private YuYin YuYin = new YuYin();

        public MainWindow()
        {

            InitializeComponent();

            Info.Text += version;
            //初始化日志
            //b = new Api();
            b = new XiguaAPI();
            overlay_enabled = true;
            OpenOverlay();
            overlay.Show();

            Closed += MainWindow_Closed;

            //Api.OnMessage += b_ReceivedDanmaku;
            //Api.OnLeave += OnLiveStop;
            //            b.OnMessage += ProcDanmaku;
            //Api.LogMessage += b_LogMessage;
            //Api.OnRoomCounting += b_ReceivedRoomCount;

            XiguaAPI.OnMessage += b_ReceivedDanmaku;
            XiguaAPI.OnLeave += OnLiveStop;
            XiguaAPI.LogMessage += b_LogMessage;
            XiguaAPI.OnRoomCounting += b_ReceivedRoomCount;

            YuYin.LogMessage += b_LogMessage;
            //YuYin.Landu += Landu;

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
            Setting.GetYuyin += GetYuyin;
            YuYin.GetConfig += GetConfig;
            XiguaAPI.GetYuyin += GetYuyin;
            //Landu();
            YuYin.HeCheng();

            System.IO.Directory.CreateDirectory("log");//创造日志文件夹
            System.IO.Directory.CreateDirectory("toggle");//创造记录文件夹
            try
            {
                ConfigData = (ConfigData)Config.Read();
                UpdateUI();
            }
            catch
            {
                ConfigData = new ConfigData();
                Config.Write(ConfigData);
                UpdateUI();
                MessageBox.Show("    欢迎使用西瓜直非官方助手。\n    本软件原作者 q792602257 ，当前分支由 挂神 维护。\n    本软件完全开源，挂神不会以任何形式销售本软件，严禁倒卖。\n    更多说明请前往 更多设置-关于程序 查看，或翻阅软件附带的“说明”文件。\n    您可以在关闭软件后将原安装目录下的Config.json文件覆盖本目录下的Config.json进行配置迁移。", "关于本软件");

            }
#if DEBUG
            ConfigData.DeBug = true;
#endif

            BlackList = ConfigData.BlackList.Split('|');
            User.targetBrand = ConfigData.Brand;
            YuYin.version = version;
            YuYin.Connect();
        }

        private void UpdateUI()
        {
            LiverName.Text = ConfigData.Room;
            showBrand.IsChecked = ConfigData.ShowBrand;
            showGrade.IsChecked = ConfigData.ShowGrade;
            showChat.IsChecked = ConfigData.ShowChat;
            showPresent.IsChecked = ConfigData.ShowPresent;
            showLike.IsChecked = ConfigData.ShowLike;
            //showLike.IsChecked = ConfigData.JoinRoom;
            danMu.IsChecked = ConfigData.DanMu;
        }
        private void b_LogMessage(string e, string level = "info")
        {
            logging(e, level);
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
            ConfigData.Brand = User.targetBrand;
            Config.Write(ConfigData);
            if (isSaveToggle == false) 
                Logger.SaveToggle();
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
            User.UserList.Clear();
            Logger.DisplayText("", true);
            Logger.DisplayText("", true);
            Logger.DisplayText("", true);
            Logger.DisplayText("", true);
            Logger.DisplayText("", true);
            ConfigData.Room = LiverName.Text.Trim();
            Config.Write(ConfigData);
            //b = new Api(LiverName.Text.Trim());
            b = new XiguaAPI(LiverName.Text.Trim());
            logging("是否为房间号" + b.isRoomID.ToString(), "debug");

            ConnBtn.IsEnabled = false;
            DisconnBtn.IsEnabled = false;
            var connectresult = false;
            logging("正在连接");
            connectresult = await b.ConnectAsync();
            if (connectresult)
            {
                logging("连接成功");
                AddDMText("提示", "连接成功", true);
                getDanmakuThread.Start();
                logging(b.RoomID.ToString(), "debug");
                isSaveToggle = false;
                if (!b.isRoomID)
                    LiverName.Text = b.user.ToString();
                b.UpdateAdminList();
                b.UpdateBrand();
                logging($"主播 {b.user.Name} ， 正在直播 {b.Title} 。");
                logging(XiguaAPI.UserID.ToString(),"debug");
                logging(User.AdminList.Count.ToString(), "debug");
                logging(User.targetBrand, "debug");
            }
            else
            {
                logging("连接失败");
                AddDMText("提示", "连接失败", true);
                ConnBtn.IsEnabled = true;
            }
            DisconnBtn.IsEnabled = true;
        }

        public void b_ReceivedRoomCount(string popularity)
        {
            //            logging("當前房間人數:" + e.UserCount);
            //            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
            if (CheckAccess())
            {
                OnlinePopularity.Text = popularity;
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
                        Logger.DisplayText(danmakuModel.ChatModel.ToString());
                        Hecheng(danmakuModel.ChatModel);
                        
                        new Thread(()=> {//创建一个线程来发送歌曲信息 防止因为点歌姬未开启导致的线程阻塞引发崩溃
                            var a = danmakuModel.ChatModel.content;
                            var b = a.Split(' ');
                            if (a != "点歌" && b[0] == "点歌")
                            {
                                try
                                {
                                    logging(Common.HttpGet("http://localhost:23333/" + a.Replace("点歌 ", "")));
                                }
                                catch(Exception err)
                                {
                                    logging(err.ToString(), "debug");
                                }
                            }
                        }).Start();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText(danmakuModel.ChatModel.user, danmakuModel.ChatModel.content);
                        }));
                    }
                    break;
                case MessageEnum.Gift:
                    {
                        if (ConfigData.ShowPresent && danmakuModel.GiftModel.isEnd)
                        {
                            logging($"收到礼物 : {danmakuModel.GiftModel.user} 赠送的 {danmakuModel.GiftModel.count} 个 {danmakuModel.GiftModel.GetName()}");
                            Logger.DisplayText($"收到礼物 : {danmakuModel.GiftModel.user} 赠送的 {danmakuModel.GiftModel.count} 个 {danmakuModel.GiftModel.GetName()}", true);
                            Hecheng($"感谢{danmakuModel.GiftModel.user.Name}赠送的{danmakuModel.GiftModel.count}个{danmakuModel.GiftModel.GetName()}");
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddDMText("收到礼物", danmakuModel.GiftModel.ToString(), true);
                            }));
                        }
                        break;
                    }
                case MessageEnum.LuckyBox:
                    {
                        if (ConfigData.ShowPresent)
                        {
                            logging($"收到礼物 : {danmakuModel.LuckyBox}");
                            Logger.DisplayText($"收到礼物 : {danmakuModel.LuckyBox}", true);
                            Hecheng($"感谢{danmakuModel.LuckyBox}");
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddDMText("红包礼物", danmakuModel.LuckyBox.ToString(), true);
                            }));
                        }
                        break;
                    }
                case MessageEnum.JoinFansclub:
                    {
                        if (ConfigData.ShowPresent)
                        {
                            logging($"粉丝团新成员 : {danmakuModel.UserModel} 加入了粉丝团");
                            Hecheng($"欢迎{danmakuModel.UserModel.Name}加入了粉丝团");
                            Logger.DisplayText($"粉丝团 : {danmakuModel.UserModel} 加入了粉丝团");
                           Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddDMText("粉丝团新成员", $"{danmakuModel.UserModel} 加入了粉丝团", true);
                            }));
                        }
                        break;
                    }
                case MessageEnum.JoinRoom:
                    {
                        if (ConfigData.JoinRoom)
                        {
                            if (danmakuModel.UserModel.isImportant)
                            {
                                Hecheng($"欢迎{danmakuModel.UserModel.Name}进入直播间");
                                logging($"进入房间 : {danmakuModel.UserModel} 来了");
                            }
                            if (ConfigData.ImportantJoinRoom)
                            {
                                if (danmakuModel.UserModel.isImportant)
                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        AddDMText("进入房间", $"{danmakuModel.UserModel} 来了");
                                    }));
                            }
                            else
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    AddDMText("进入房间", $"{danmakuModel.UserModel} 来了");
                                }));
                        }
                        break;
                    }
                case MessageEnum.Subscribe:
                    {
                        if (ConfigData.ShowFollow && danmakuModel.UserModel.Name != "") 
                        {
                            logging($"新的粉丝 {danmakuModel.UserModel} 的关注");
                            Hecheng($"感谢{danmakuModel.UserModel.Name}的关注");
                            Logger.DisplayText($"新的粉丝：{danmakuModel.UserModel} 的关注");
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddDMText("新的粉丝", $"{danmakuModel.UserModel} 的关注", true);
                            }));
                        }
                        break;
                    }
                case MessageEnum.Like:
                    {
                        if (!ConfigData.ShowLike) 
                        {
                            logging($"{danmakuModel.Like}");
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddDMText("点亮消息", $"{danmakuModel.Like}", true);
                            }));
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
            AddDMText("提示", "欢迎使用挂神西瓜直播非官方弹幕助手。");
            Hecheng("欢迎使用挂神西瓜直播非官方弹幕助手");
            Logger.DisplayText("礼物测试1", true);
            Logger.DisplayText("礼物测试2", true);
            Logger.DisplayText("礼物测试3", true);
            Logger.DisplayText("礼物测试4", true);
            Logger.DisplayText("礼物测试5", true);
            Logger.DisplayText("弹幕测试1");
            Logger.DisplayText("弹幕测试2");
            Logger.DisplayText("弹幕测试3");
            Logger.DisplayText("弹幕测试4");
            Logger.DisplayText("弹幕测试5");
        }

        private void Setting_OnClick(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
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
            lock (DanmuHecheng)
            {
                while (true)
                {
                    if (DanmuHecheng.Any())
                    {
                        _ = DanmuHecheng.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
            }
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
            logging(Logger.SaveToggle());
            isSaveToggle = true;
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
        public void Hecheng(Chat chat)
        {
            if (ConfigData.BlackList != "")
                foreach (var Black in BlackList)
                {
                    if (Black.Contains("Name:"))
                    {
                        var black = Black.Replace("Name:", "");
                        if (chat.user.Name == black) return;
                    }else if (Black.Contains("Name："))
                    {
                        var black = Black.Replace("Name：", "");
                        if (chat.user.Name == black.Trim()) return;
                    }
                }
            //经过多次测试发现弹幕中部分emoji不能被语音合成程序处理而导致主线程阻塞引发程序崩溃
            //即使未引发崩溃也会导致程序不能继续正常工作出现假死状态
            Hecheng(DelEmoji.delEmoji(chat.content), true);
        }
        public void Hecheng(string wenzi, bool isChat = false)
        {
            if (wenzi == "") 
                return;
            if (ConfigData.DanMu)
            {
                if (isChat) {
                    if (wenzi.Length > ConfigData.maxSize)
                        return;
                    if (ConfigData.BlackList != "")
                        foreach (var Black in BlackList)
                        {
                            if (Black.Contains("Name:") || Black.Contains("Name：")) continue;
                            var black = Black.Trim();
                            try
                            {
                                if (Regex.IsMatch(wenzi, black)) return;
                            }
                            catch (Exception err)
                            {
                                logging(err.ToString(), "debug");
                            }
                        }

                }
                lock (YuYin.DanmuHecheng)
                    YuYin.DanmuHecheng.Enqueue(wenzi);
            }
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

        public ConfigData GetConfig()
        {
            return ConfigData;
        }

        public YuYin GetYuyin()
        {
            return YuYin;
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
            ConfigData.ShowBrand = true;
            User.showBrand = true;
        }

        private void showBrand_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowBrand = false;
            User.showBrand = false;
        }
        private void showGrade_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowGrade = true;
            User.showPay = true;
        }
        private void showGrade_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowGrade = false;
            User.showPay = false;
        }

        private void ShowLike_OnChecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowLike = true;
            ConfigData.ImportantJoinRoom = true;
        }
        private void ShowLike_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ConfigData.ShowLike = false;
            ConfigData.ImportantJoinRoom = false;
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
