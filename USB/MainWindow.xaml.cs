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
        private SerialPort serialPort =null;

        List<Border> ShowBorders;
        List<Button> ClickButton;
        Dictionary<int, int> CurrentKeyValuePairs = new Dictionary<int, int>
        {
            {0,0},
            {1,0},
            {2,0},
            {3,0},
            {4,0},
            {5,0},
            {6,0},
            {7,0},
            {8,0},
        };
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
                if (!USBHelper.USBHelper.serialPort.IsOpen)
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
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Open(comboBox.Text);
        }
      

        public async void Open(string PortName)
        {
            await Task.Delay(1);
            if (PortName != null || PortName != "")
            {
                int Code = USBHelper.USBHelper.OpenPort(PortName);
                OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
                if (Code == 0)
                {
                    if (ErrorGrid.Opacity == 0.9)
                        ErrorShow(0.9, 0, 0.5);
                    USBHelper.USBHelper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    GetCurrentStatus();
                }
                else
                {
                    if (ErrorGrid.Opacity == 0)
                    {
                        ErrorShow(0, 0.9, 2);
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
                    ErrorShow(0, 0.9, 2);
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




        private async void GetCurrentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                await Task.Delay(180);
                USBHelper.USBHelper.SendMsg(i, 2);
            }
        }

        private async void SendMsg(int[] nums ,int Func)
        {
            foreach (var num in nums)
            {
                await Task.Delay(180);
                USBHelper.USBHelper.SendMsg(num, Func);
            }
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

        Dictionary<byte, int> keyValuePairs = new Dictionary<byte, int>
        {
            { 0x31,0},
            { 0x32,1},
            { 0x33,2},
            { 0x34,3},
            { 0x35,4},
            { 0x36,5},
            { 0x37,6},
            { 0x38,7},
            { 0x39,8},
        };

        private void Show(byte[] buff)
        {
            if (buff.Length == 5)
            {
                int num = keyValuePairs[buff[1]];
                ShowBorders[num].Background = buff[3] == 48 ? Brushes.Gray : Brushes.Green;
                ClickButton[num].Tag = buff[3] == 48 ? 1 : 0;
                CurrentKeyValuePairs[num] = buff[3] == 48 ? 0 : 1;
            }
        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            USBHelper.USBHelper.Close();
            USBHelper.USBHelper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            OpenShow.Background = Brushes.Red;
        }




        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            USBHelper.USBHelper.SendMsg(int.Parse(button.Content.ToString()), int.Parse(button.Tag.ToString()));
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int Function = int.Parse(button.Tag.ToString());
            SendMsg(Function);
        }


        Dictionary<int, int> SendKeyValuePairs;
        Dictionary<int, int> TempKeyValuePairs;

        private async void SendMsg(int Function)
        {
            SendKeyValuePairs = new Dictionary<int, int> { } ;

            foreach (var item in CurrentKeyValuePairs)
                SendKeyValuePairs.Add(item.Key, item.Value);



            foreach (var item in SendKeyValuePairs)
            {
                if (item.Value != Function)
                {
                    await Task.Delay(180);
                    USBHelper.USBHelper.SendMsg(item.Key+1, Function);
                }
            }
        }

        private async void SendMsg(Dictionary<int, int> keyValuePairs)
        {
            TempKeyValuePairs = new Dictionary<int, int> { };

            foreach (var item in CurrentKeyValuePairs)
                TempKeyValuePairs.Add(item.Key, item.Value);

            foreach (var item in keyValuePairs)
            {
                if (TempKeyValuePairs[item.Key] != item.Value)
                {
                    await Task.Delay(180);
                    USBHelper.USBHelper.SendMsg(item.Key + 1, item.Value);
                }
            }
        }

        private void Function_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;  
            switch (button.Tag)
            {
                case "Gongan":
                    SendKeyValuePairs = NumsToDic(new int[] { 1, 0, 1, 0, 1, 0, 1, 0,1});
                    SendMsg(SendKeyValuePairs);
                    break;
                case "Huji":
                    SendKeyValuePairs = NumsToDic(new int[] { 1, 1, 0, 0, 0, 1, 1, 0,0 });
                    SendMsg(SendKeyValuePairs);
                    break;
                case "Waihui":
                    SendKeyValuePairs = NumsToDic(new int[] { 0, 1, 0, 1, 1, 0, 0, 1,1 });
                    SendMsg(SendKeyValuePairs);
                    break;
                default:
                    break;
            }
        }

        private Dictionary<int, int> NumsToDic(int[] Nums)
        {
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int> { };
            for (int i = 0; i < Nums.Length; i++)
            {
                keyValuePairs.Add(i, Nums[i]);
            }

            return keyValuePairs;
        }
    }
}
