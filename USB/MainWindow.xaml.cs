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

        MotorControl MotorControl = new MotorControl();
        private void Window_Initialized(object sender, EventArgs e)               
        {
            MotorControl.Initialized();
            this.DataContext = MotorControl;

            GetComBox();
            //OpenFast();

            pageTimer = new DispatcherTimer() { IsEnabled = false, Interval = TimeSpan.FromSeconds(1)};
            pageTimer.Tick += new EventHandler((sender1, e1) =>
            {

                if (--Countdown > 0)
                {
                }
                else
                {
                    pageTimer.IsEnabled = false;
                    Countdown = 10;
                    GetComBox();
                    Open(comboBox.Text);
                }
            });
        }

        int Countdown = 10;
        public int TempIndex = 0;


        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (pageTimer.IsEnabled)
                pageTimer.IsEnabled = false;

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
                    break;
                }

            }

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

       
        public void ErrorShow(double a,double b,double time)
        {
            DoubleAnimation daV = new DoubleAnimation(a, b, new Duration(TimeSpan.FromSeconds(time)));
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
        public bool Equals(Dictionary<int,int> dict1, Dictionary<int, int> dict2)
        {
            return dict1.Keys.Count == dict2.Keys.Count &&dict1.Keys.All(k => dict2.ContainsKey(k) && object.Equals(dict2[k], dict1[k]));
        }


        private void ServiceClose()
        {
            Helper.Close();
            Helper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Byte[] vs = new byte[] { 0x2F, 0x31, 0x44, 0x2C, 0x31, 0x30, 0x30, 0x30, 0x30, 0x52, 0x0D, 0x0A };
            Helper.SendMsg(vs);
        }


        private void Function_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;  
            switch (button.Tag)
            {
                case "Open":
                    Open(comboBox.Text);
                    break;
                case "Close":
                    ServiceClose();
                    break;
                case "Run":
                    Helper.SendMsg(SetMsg.Text);
                    break;
                default:
                    break;
            }
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

                default:
                    break;
            }
        }
    }
}
