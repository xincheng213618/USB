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
        Dictionary<int, int> SendKeyValuePairs;

        private Timer timer;
        private void Window_Initialized(object sender, EventArgs e)               
        {     
            timer = new Timer(_ => Dispatcher.BeginInvoke(new Action(() => TimeRun())), null, 0, 1000);//本来是60，不过没必要刷新这么快，就1s1次就好。

            ShowBorders = new List<Border> { Show1, Show2, Show3, Show4, Show5, Show6, Show7, Show8, Show9 };
            ClickButton = new List<Button> { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9 };

            string[] PortNames = SerialPort.GetPortNames();
            for (int i = 0; i < PortNames.Count(); i++)
                comboBox.Items.Add(PortNames[i]);   //将数组内容加载到comboBox控件中
            comboBox.SelectedIndex = 0;

            Open(comboBox.Text);
        }

        private void TimeRun()
        {
            if (ErrorGrid.Opacity == 0)
            {
                if (!Helper.serialPort.IsOpen)
                {
                    ErrorLabel.Content = "失去连接";
                    ErrorShow(0, 0.9, 2);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Rotate();
            comboBox.Items.Clear();
           
            Dispatcher.BeginInvoke(new Action(() => ErrorLabel.Content = "正在重试"));

            string[] PortNames = SerialPort.GetPortNames();
            for (int i = 0; i < PortNames.Count(); i++)
                comboBox.Items.Add(PortNames[i]);   //将数组内容加载到comboBox控件中
            comboBox.SelectedIndex = 0;

            Open(comboBox.Text);
        }

        public void Rotate()
        {
            Storyboard storyboard = new Storyboard();//创建故事板
            DoubleAnimation doubleAnimation = new DoubleAnimation();//实例化一个Double类型的动画
            RotateTransform rotate = new RotateTransform();//旋转转换实例
            this.RefreshImage.RenderTransform = rotate;//给图片空间一个转换的实例
            storyboard.SpeedRatio = 2;//播放的数度
            //设置从0 旋转360度
            doubleAnimation.From = 0;
            doubleAnimation.To = 360;
            doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 2));//播放时间长度为2秒
            Storyboard.SetTarget(doubleAnimation, this.RefreshImage);//给动画指定对象
            Storyboard.SetTargetProperty(doubleAnimation,
            new PropertyPath("RenderTransform.Angle"));//给动画指定依赖的属性
            storyboard.Children.Add(doubleAnimation);//将动画添加到动画板中
            storyboard.Begin(this.RefreshImage);//启动动画
        }

        public async void Open(string PortName)
        {
            await Task.Delay(1);
            if (PortName != null || PortName != "")
            {
                int Code = Helper.OpenPort(PortName);
                OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
                if (Code == 0)
                {
                    if (ErrorGrid.Opacity == 0.9)
                        ErrorShow(0.9, 0, 0.5);
                    Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    Helper.SendMsg(Util.NumsToDic(new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 }));
                }
                else
                {
                    if (ErrorGrid.Opacity == 0)
                    {
                        ErrorShow(0, 0.9, 1);
                    }
                    else
                    {
                        ErrorLabel.Content = "连接失败";
                    }

                }
            }
            else
            {
                if (ErrorGrid.Opacity == 0)
                {
                    ErrorShow(0, 0.9, 1);
                }
                else
                {
                    ErrorLabel.Content = "连接失败";
                }
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

            Dispatcher.BeginInvoke(new Action(() => Show(buff)));
        }



        private void Show(byte[] buff)
        {
            if (buff.Length == 5)
            {
                int num = Helper.CodeKeyValuePairs[buff[1]];
                ShowBorders[num].Background = buff[3] == 48 ? Brushes.Gray : Brushes.Green;
                ClickButton[num].Tag = buff[3] == 48 ? 1 : 0;
                Helper.CurrentKeyValuePairs[num] = buff[3] == 48 ? 0 : 1;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Helper.Close();
            Helper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            OpenShow.Background = Brushes.Red;
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Helper.SendMsg(int.Parse(button.Content.ToString()), int.Parse(button.Tag.ToString()));
        }




        private void Function_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;  
            switch (button.Tag)
            {
                case "AllOpen":
                    Helper.SendMsg(Util.NumsToDic(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 }));
                    break;
                case "AllClose":
                    Helper.SendMsg(Util.NumsToDic(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
                    break;
                case "Gongan":
                    Helper.SendMsg(Util.NumsToDic(new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1 }));
                    break;
                case "Huji":
                    Helper.SendMsg(Util.NumsToDic(new int[] { 1, 1, 0, 0, 0, 1, 1, 0, 0 }));
                    break;
                case "Waihui":
                    Helper.SendMsg(Util.NumsToDic(new int[] { 0, 1, 0, 1, 1, 0, 0, 1, 1 }));
                    break;
                default:
                    break;
            }
        }
    }
}
