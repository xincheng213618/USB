using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using USBDLL;

namespace USB
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        List<Border> ShowBorders;
        List<Button> ClickButton;

        List<Border> FunctionBorders;
        //倒计时检测
        private DispatcherTimer pageTimer = null;
        private int TempIndex = 0;

        private Timer timer;
        private void Window_Initialized(object sender, EventArgs e)               
        {     
            timer = new Timer(_ => Dispatcher.BeginInvoke(new Action(() => TimeRun())), null, 0, 1000);//本来是60，不过没必要刷新这么快，就1s1次就好。
            ShowBorders = new List<Border> { Show1, Show2, Show3, Show4, Show5, Show6, Show7, Show8, Show9 };
            ClickButton = new List<Button> { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9 };
            FunctionBorders = new List<Border> { AllOpenBorder, AllCloseBorder,GonganBorder, HujiBorder, WaihuiBorder };
            GetComBox();
            OpenFast();


            pageTimer = new DispatcherTimer() { IsEnabled = false, Interval = TimeSpan.FromSeconds(1)};
            pageTimer.Tick += new EventHandler((sender1, e1) =>
            {

                if (--Countdown > 0)
                {
                    ErrorCountLabel.Content = Countdown + "秒后自动重连";
                }
                else
                {
                    ErrorLabel.Content = "正在重新连接";
                    pageTimer.IsEnabled = false;
                    Countdown = 10;
                    GetComBox();
                    Open(comboBox.Text);
                }
            });
        }

        int Countdown = 10;

        private void TimeRun()
        {
            if (ErrorGrid.Opacity == 0)
                if (!Helper.serialPort.IsOpen)
                {
                    ErrorLabel.Content = "失去连接";
                    ErrorShow(0, 0.9, 2);
                    //做一个取不等判定
                    if (!pageTimer.IsEnabled)
                    {
                        Countdown = 10;
                        pageTimer.IsEnabled = true;
                    }

                }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (pageTimer.IsEnabled)
                pageTimer.IsEnabled = false;

            Rotate();
            ErrorLabel.Content = "正在重新连接";
            GetComBox();
            Open(comboBox.Text);
        }

        private void GetComBox()
        {
            comboBox.Items.Clear();
            string[] PortNames = SerialPort.GetPortNames();
            //这种写法不允许有多个串口；
            for (int i = 0; i < PortNames.Count(); i++)
                comboBox.Items.Add(PortNames[i]);   //将数组内容加载到comboBox控件中
            comboBox.SelectedIndex = TempIndex;
        }

        public void Rotate()
        {
            Storyboard storyboard = new Storyboard
            {
                SpeedRatio = 2//播放的数度
            };//创建故事板
            RotateTransform rotate = new RotateTransform();//旋转转换实例
            RefreshImage.RenderTransform = rotate;//给图片空间一个转换的实例
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 2))//播放时间长度为2秒
            };
            Storyboard.SetTarget(doubleAnimation, RefreshImage);//给动画指定对象
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.Angle"));//给动画指定依赖的属性
            storyboard.Children.Add(doubleAnimation);//将动画添加到动画板中
            storyboard.Begin(RefreshImage);//启动动画
        }

        public async void OpenFast()
        {
            await Task.Delay(1);
            int Code = -1 ;

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                comboBox.SelectedIndex = i;
                Code = Helper.OpenPort(comboBox.Text);
                if (Code == 0)
                {
                    TempIndex = i;
                    break;
                }

            }

            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
            {
                pageTimer.IsEnabled = false;
                if (ErrorGrid.Opacity == 0.9)
                    ErrorShow(0.9, 0, 0.5);
                Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                Helper.SendMsg(Util.NumsToDic(new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 }));
            }
            else
            {
                ErrorLabel.Content = "连接失败";
                Countdown = 10;
                pageTimer.IsEnabled = true;
            }
        }



        public async void Open(string PortName)
        {
            await Task.Delay(1);
            int Code = Helper.OpenPort(PortName);
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
            {
                pageTimer.IsEnabled = false;
                if (ErrorGrid.Opacity == 0.9)
                    ErrorShow(0.9, 0, 0.5);
                Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                Helper.SendMsg(Util.NumsToDic(new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 }));
            }
            else
            {
                ErrorLabel.Content = "连接失败";
                Countdown = 10;
                pageTimer.IsEnabled = true;
            }
        }

       
        public void ErrorShow(double a,double b,double time)
        {
            DoubleAnimation daV = new DoubleAnimation(a, b, new Duration(TimeSpan.FromSeconds(time)));
            ErrorGrid.BeginAnimation(OpacityProperty, daV);
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            Thread.Sleep(50);
            int bytesread = serialPort.BytesToRead;
            byte[] buff = new byte[bytesread];
            serialPort.Read(buff, 0, bytesread);
            //这里必须要用异步,返回原本线程
            Dispatcher.BeginInvoke(new Action(() => Show(buff)));
        }
        private readonly BrushConverter Use1 = new BrushConverter();

        private void Show(byte[] buff)
        {
            if (buff.Length == 5)
            {
                int num = Helper.CodeKeyValuePairs[buff[1]];
                ShowBorders[num].Background = buff[3] == 48 ? Brushes.Gray : Brushes.Green;
                ClickButton[num].Tag = buff[3] == 48 ? 1 : 0;
                Helper.CurrentKeyValuePairs[num] = buff[3] == 48 ? 0 : 1;

                foreach (Border border in FunctionBorders)
                {
                    border.Background = Brushes.AliceBlue;
                }


                if (Equals(Helper.CurrentKeyValuePairs,AllOpenkeys))
                {
                    AllOpenBorder.Background = (Brush)Use1.ConvertFromInvariantString("#ff8160"); 
                }
                else if(Equals(Helper.CurrentKeyValuePairs, AllClosekeys))
                {
                    AllCloseBorder.Background = (Brush)Use1.ConvertFromInvariantString("#ff8160");
                }
                else if (Equals(Helper.CurrentKeyValuePairs, Gongankeys))
                {
                    GonganBorder.Background = (Brush)Use1.ConvertFromInvariantString("#ff8160");
                }
                else if (Equals(Helper.CurrentKeyValuePairs, Hujikeys))
                {
                    HujiBorder.Background = (Brush)Use1.ConvertFromInvariantString("#ff8160");
                }
                else if (Equals(Helper.CurrentKeyValuePairs, Waihuikeys))
                {
                    WaihuiBorder.Background = (Brush)Use1.ConvertFromInvariantString("#ff8160");

                }

            }
        }
        public bool Equals(Dictionary<int,int> dict1, Dictionary<int, int> dict2)
        {
            return dict1.Keys.Count == dict2.Keys.Count &&dict1.Keys.All(k => dict2.ContainsKey(k) && object.Equals(dict2[k], dict1[k]));
        }


        private void ServiceClose()
        {
            Helper.Close();
            Helper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            OpenShow.Background = Brushes.Red;
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Helper.SendMsg(int.Parse(button.Content.ToString())-1, int.Parse(button.Tag.ToString()));
        }

        private readonly Dictionary<int, int> AllOpenkeys = Util.NumsToDic(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        private readonly Dictionary<int, int> AllClosekeys = Util.NumsToDic(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        private readonly Dictionary<int, int> Gongankeys = Util.NumsToDic(new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1 });
        private readonly Dictionary<int, int> Hujikeys = Util.NumsToDic(new int[] { 1, 1, 0, 0, 0, 1, 1, 0, 0 });
        private readonly Dictionary<int, int> Waihuikeys = Util.NumsToDic(new int[] { 0, 1, 0, 1, 1, 0, 0, 1, 1 });

        private void Function_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;  
            switch (button.Tag)
            {
                case "AllOpen":
                    Helper.SendMsg(AllOpenkeys);
                    break;
                case "AllClose":
                    Helper.SendMsg(AllClosekeys);
                    break;
                case "Gongan":
                    Helper.SendMsg(Gongankeys);
                    break;
                case "Huji":
                    Helper.SendMsg(Hujikeys);
                    break;
                case "Waihui":
                    Helper.SendMsg(Waihuikeys);
                    break;
                default:
                    break;
            }
        }
    }
}
