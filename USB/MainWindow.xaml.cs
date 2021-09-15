using System;
using System.Collections.Generic;
using System.IO;
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
        //倒计时检测
        private DispatcherTimer pageTimer = null;
        private Timer timer;


        MotorControl MotorControl = new MotorControl();

        private void Window_Initialized(object sender, EventArgs e)               
        {

            int Code = MotorControl.Initialized();
            MotorControl.AutoSetMove();
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            timer = new Timer(_ => Dispatcher.BeginInvoke(new Action(() => TimeRun())), null, 0, 1000);//本来是60，不过没必要刷新这么快，就1s1次就好。

            this.DataContext = MotorControl;

            //pageTimer = new DispatcherTimer() { IsEnabled = false, Interval = TimeSpan.FromSeconds(1)};
            //pageTimer.Tick += new EventHandler((sender1, e1) =>
            //{

            //    if (--Countdown > 0)
            //    {
            //    }
            //    else
            //    {
            //        pageTimer.IsEnabled = false;
            //        Countdown = 10;
            //        GetComBox();
            //    }
            //});
        }

        private void TimeRun()
        {
            OpenShow.Background = MotorControl.IsOpen ? Brushes.Red : Brushes.Green;
            if (!MotorControl.IsOpen)
            {
                int Code = MotorControl.Initialized();
                OpenShow.Background = Code == 0 ? Brushes.Red : Brushes.Green;
            }
        }


        int Countdown = 10;
        public int TempIndex = 0;


        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (pageTimer.IsEnabled)
                pageTimer.IsEnabled = false;
        }



        public async void OpenFast()
        {
            await Task.Delay(1);
            int Code = -1 ;

            if (Code == 0)
            {
                pageTimer.IsEnabled = false;

                Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            }
            else
            {
                Countdown = 10;
                pageTimer.IsEnabled = true;
            }
        }



        public async void Open(string PortName)
        {
            await Task.Delay(1);
            int Code = Helper.OpenPort(PortName);
            MessageBox.Show(Code.ToString());

            if (Code == 0)
            {
                pageTimer.IsEnabled = false;
                Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            }
            else
            {
                Countdown = 10;
                pageTimer.IsEnabled = true;
            }
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

            }
        }


        private void ServiceClose()
        {
            Helper.Close();
            Helper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
        }



        private void Move_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "X+":
                    MotorControl.MoveX(100);
                    break;
                case "X-":
                    MotorControl.MoveX(-100);
                    break;
                case "Y+":
                    MotorControl.MoveY(100);
                    break;
                case "Y-":
                    MotorControl.MoveY(-100);
                    break;
                case "X++":
                    MotorControl.MoveX(1000);
                    break;
                case "X--":
                    MotorControl.MoveX(-1000);
                    break;
                case "Y++":
                    MotorControl.MoveY(1000);
                    break;
                case "Y--":
                    MotorControl.MoveY(-1000);
                    break;
                case "Set":
                    MotorControl.SetMove();
                    break;
                case "ReSet":
                    MotorControl.ReSetMove();
                    break;
                case "Z+":
                    MotorControl.MoveZ(-100);
                    break;
                case "Z-":
                    MotorControl.MoveZ(100);
                    break;
                default:
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Len_Ini(object sender, RoutedEventArgs e)
        {
            string[] PortNames = SerialPort.GetPortNames();
            //这种写法不允许有多个串口；
            for (int i = 0; i < PortNames.Count(); i++)
            {
                int x = LenControl.OpenPort(PortNames[i]);
                MessageBox.Show(x.ToString());
                if (x == 0) return;
            }

        }

        private void Len_Read(object sender, RoutedEventArgs e)
        {
            int x = LenControl.Read();
            MessageBox.Show(x.ToString());
        }
    }
}
