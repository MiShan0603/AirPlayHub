using AirHub.Common;
using AirHub.Control;
using AirHub.UI;
using AirHub.Wrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;

namespace AirHub
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object airSourcesLock = new object();
        private List<AirRender> airRenders = null;
        private System.Windows.Forms.NotifyIcon mNotifyIcon = null;

        public MainWindow()
        {
            InitializeComponent();

            if (!Config.Instance().StartWithWindow)
            {
                //this.Opacity = 0;
                //this.AllowsTransparency = true;
                //this.WindowStyle = WindowStyle.None;
                //this.ShowInTaskbar = false;

                _left = this.Left;
                _top = this.Top;
                _width = this.Width;
                _height = this.Height;

                // this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;

                this.Left = 0;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                this.Hide();
            }

            this.MaxWidth = SystemParameters.PrimaryScreenWidth;
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            airRenders = new List<AirRender>();

            m_connectCallback = OnAirplayConnect;
            m_disConnectCallback = OnAirplayDisConnect;

            connectAction += OnConnectAction;
            disConnectAction += OnDisConnectAction;

            bool ret = AirPlayWrapper.Init();
            if (!ret)
            {
                MessageBox.Show("AirHub start failed 1.");
                return;
            }

            ret = AirPlayWrapper.Start(Config.Instance().Name, Config.Instance().PinCode,
                m_connectCallback, m_disConnectCallback, IntPtr.Zero);
            if (!ret)
            {
                MessageBox.Show("AirHub start failed 2.");
                return;
            }

            InitialTray();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mNotifyIcon.Dispose();
            AirPlayWrapper.Stop();
        }

        private double _left = 0.0;
        private double _top = 0.0;
        private double _width = 0.0;
        private double _height = 0.0;

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (this.WindowStyle == WindowStyle.None)
                {
                    // this.WindowState = WindowState.Normal;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.ResizeMode = ResizeMode.CanResize;

                    this.Left = _left;
                    this.Top = _top; 
                    this.Width = _width;
                    this.Height = _height;
                }
                else
                {
                    _left = this.Left;
                    _top = this.Top;
                    _width = this.Width;
                    _height = this.Height;

                    // this.WindowState = WindowState.Maximized;
                    this.WindowStyle = WindowStyle.None;
                    this.ResizeMode = ResizeMode.NoResize;

                    this.Left = 0;
                    this.Top = 0;
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    this.Height = SystemParameters.PrimaryScreenHeight;
                }
                
            }
        }


        #region 回调
        private AirplayConnect m_connectCallback = null;
        private AirplayDisConnect m_disConnectCallback = null;
        void OnAirplayConnect(string session, string name, string address, int state, IntPtr customParam)
        {
            connectAction.Invoke(session, name, address, state);

            Console.WriteLine("OnAirplayConnect >>>> ");
        }

        void OnAirplayDisConnect(string session, IntPtr customParam)
        {
            Console.WriteLine("OnAirplayDisConnect >>>> 1");

            disConnectAction.Invoke(session);

            Console.WriteLine("OnAirplayDisConnect >>>> 2");
        }

        #endregion 回调

        #region Action
        private event Action<string /*session*/, string /*name*/, string /*address*/, int /*state*/> connectAction;
        private void OnConnectAction(string session, string name, string address, int state)
        {
            lock (airSourcesLock)
            {
                Debug.WriteLine($"OnConnect session:{session} name:{name} address:{address} state:{state}");

                this.Dispatcher.Invoke(new Action(() => {

                    try
                    {
                        AirSource airSource = new AirSource(session, name);
                        airSource.address = address;
                        airSource.state = state;

                        AirPlayWrapper.Show(session, true);

                        AirRender render = new AirRender();
                        render.airSource = airSource;
                        airRenders.Add(render);

                        ResetLayout();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    try
                    {
                        if (Visibility.Hidden == this.Visibility)
                        {
                            this.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }));


            }
        }

        private event Action<string /*session*/> disConnectAction;
        private void OnDisConnectAction(string session)
        {
            lock (airSourcesLock)
            {
                Debug.WriteLine($"OnDisConnect session:{session} ");


                /*App.Current*/
                this.Dispatcher.Invoke(new Action(() =>
                {
                    for (int itemIndex = 0; itemIndex < airRenders.Count; itemIndex++)
                    {
                        AirRender item = airRenders[itemIndex];
                        if (item.airSource.session == session)
                        {
                            airRenders.Remove(item);
                            break;
                        }
                    }

                    ResetLayout();

                    if (airRenders.Count == 0)
                    {
                        if (!Config.Instance().StartWithWindow)
                        {
                            if (Visibility.Visible == this.Visibility)
                            {
                                this.Visibility = Visibility.Hidden;
                            }
                            
                        }
                        
                    }
                }));
            }
        }
        #endregion Action

        /// <summary>
        /// 重置显示窗口的布局
        /// </summary>
        private void ResetLayout()
        {
            int itemCount = (int)(airRenders.Count);
            if (itemCount == 4)
            {
                AirPlayWrapper.SetLock(true);
            }
            else
            {
                AirPlayWrapper.SetLock(false);
            }

            rootGrid.Children.Clear();
            rootGrid.RowDefinitions.Clear();
            rootGrid.ColumnDefinitions.Clear();
            switch (itemCount)
            {
                case 1:
                    // 1 行 1 列
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    airRenders[0].SetValue(Grid.RowProperty, 0);
                    airRenders[0].SetValue(Grid.ColumnProperty, 0);
                    rootGrid.Children.Add(airRenders[0]);

                    break;
                case 2:
                    // 1 行 2 列
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
                    {
                        airRenders[itemIndex].SetValue(Grid.RowProperty, 0);
                        airRenders[itemIndex].SetValue(Grid.ColumnProperty, itemIndex);
                        rootGrid.Children.Add(airRenders[itemIndex]);
                    }
                    break;
                case 3:
                    // 2 行 2 列
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
                    {
                        if (itemIndex == 0)
                        {
                            airRenders[itemIndex].SetValue(Grid.RowProperty, 0);
                            airRenders[itemIndex].SetValue(Grid.ColumnProperty, 0);
                            airRenders[itemIndex].SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        else
                        {
                            airRenders[itemIndex].SetValue(Grid.RowProperty, (itemIndex + 1) / 2);
                            airRenders[itemIndex].SetValue(Grid.ColumnProperty, (itemIndex + 1) % 2);
                        }

                        rootGrid.Children.Add(airRenders[itemIndex]);

                    }
                    break;
                case 4:
                    // 2 行 2 列
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
                    {
                        airRenders[itemIndex].SetValue(Grid.RowProperty, itemIndex / 2);
                        airRenders[itemIndex].SetValue(Grid.ColumnProperty, itemIndex % 2);

                        rootGrid.Children.Add(airRenders[itemIndex]);
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 托盘初始化
        /// </summary>
        private void InitialTray()
        {
            //设置托盘的各个属性
            mNotifyIcon = new System.Windows.Forms.NotifyIcon();
            mNotifyIcon.BalloonTipText = "AirHub 已经启动。\r\n您可以在苹果设备中搜索 " + Config.Instance().Name + " 进行投屏。";
            if (Config.Instance().Name.Length > 0)
            {
                mNotifyIcon.Text = Config.Instance().Name;
            }
            else
            {
                mNotifyIcon.Text = "AirHub";
            }
            
 
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/AirHub;component/Assets/AirHub.ico")).Stream;
            mNotifyIcon.Icon = new System.Drawing.Icon(iconStream);


            mNotifyIcon.Visible = true;
            mNotifyIcon.ShowBalloonTip(2000);

            mNotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);

            System.Windows.Forms.MenuItem menu_set = new System.Windows.Forms.MenuItem("设置");
            System.Windows.Forms.MenuItem menu_about = new System.Windows.Forms.MenuItem("关于");
            System.Windows.Forms.MenuItem menu_separator = new System.Windows.Forms.MenuItem("-");
            System.Windows.Forms.MenuItem menu_exit = new System.Windows.Forms.MenuItem("退出");

            System.Windows.Forms.MenuItem[] menu_children = new System.Windows.Forms.MenuItem[] { menu_set, menu_about, menu_separator, menu_exit };
            mNotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menu_children);

            menu_about.Click += Menu_about_Click;
            menu_set.Click += Menu_set_Click;
            menu_exit.Click += Menu_exit_Click;

            //窗体状态改变时候触发
            this.StateChanged += new EventHandler(SysTray_StateChanged);
        }

        private void Menu_about_Click(object sender, EventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            if (this.IsLoaded)
            {
                helpWindow.Owner = this;
            }
            else
            {
                helpWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            helpWindow.ShowDialog();
        }

        private void Menu_set_Click(object sender, EventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            if (this.IsLoaded)
            {
                settingWindow.Owner = this;
            }
            else
            {
                settingWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            bool? dialogResult = settingWindow.ShowDialog();
            if (dialogResult == true)
            {
                Config.Instance().StartWithWindow = settingWindow.IsStartWithShowWindow();

                if (settingWindow.GetName().CompareTo(Config.Instance().Name) != 0 ||
                    settingWindow.GetPinCode().CompareTo(Config.Instance().PinCode) != 0)
                {
                    Config.Instance().Name = settingWindow.GetName();
                    Config.Instance().PinCode = settingWindow.GetPinCode();

                    mNotifyIcon.Text = Config.Instance().Name;

                    AirPlayWrapper.Stop();

                    bool ret = AirPlayWrapper.Start(Config.Instance().Name, Config.Instance().PinCode, m_connectCallback, m_disConnectCallback, IntPtr.Zero);
                    if (!ret)
                    {
                        MessageBox.Show("AirHub Start failed.");
                        return;
                    }
                }
            }
        }

        private void SysTray_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == WindowState.Minimized)
            //{
            //    this.Visibility = Visibility.Hidden;
            //}
        }

        private void Menu_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //if (this.Visibility == Visibility.Visible)
                //{
                //    this.Visibility = Visibility.Hidden;
                //}
                //else
                //{
                //    this.Visibility = Visibility.Visible;
                //    this.Activate();
                //}
            }
        }
    }
}
