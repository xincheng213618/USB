using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
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
        }

        private void TimeRun()
        {
            if (!MotorControl.Port.serialPort.IsOpen)
            {
                int Code = MotorControl.Initialized();
                OpenShow.Background = Code == 0 ? Brushes.Red : Brushes.Green;
            }
            else
            {
                OpenShow.Background = MotorControl.Port.serialPort.IsOpen ? Brushes.Red : Brushes.Green;
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




        private void Move_Click(object sender, RoutedEventArgs e)
        {
            if (!MotorControl.Port.serialPort.IsOpen)
            {
                MessageBox.Show("请检查位移台的是否正确连接");
                return;
            }
            if (MotorControl.IsRun.IsEnabled)
            {
                //MessageBox.Show("正在运动中");
                return;
            }
            Move(((Button)sender).Tag.ToString());


        }
        private  async void Move( string control)
        {
            switch (control)
            {
                case "X+":
                    MotorControl.MoveX(int.Parse(Num.Text));

                    break;
                case "X-":
                    MotorControl.MoveX(-int.Parse(Num.Text));
                    break;
                case "Y+":
                    MotorControl.MoveY(int.Parse(Num.Text));
                    break;
                case "Y-":
                    MotorControl.MoveY(-int.Parse(Num.Text));
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
                    MotorControl.MoveZ(int.Parse(Num.Text));
                    break;
                case "Z-":
                    MotorControl.MoveZ(-int.Parse(Num.Text));
                    break;
                case "Change":
                    int Z = MotorControl.Z;
                    int ZHlight = int.Parse(Num.Text);


                    ZHlight = int.Parse(Num.Text);
                    //MotorControl.MoveZ(ZHlight);

                    //await Task.Delay(ZHlight / 15 + 1000);
                    //MotorControl.MoveZ(-ZHlight);

                    MotorControl.MoveY(ZHlight);
                    await Task.Delay(ZHlight / 15 + 700);
                    MotorControl.MoveY(ZHlight);

                    break;
                case "ChangeX":
                    ZHlight = int.Parse(Num.Text);
                    //MotorControl.MoveZ(ZHlight);
                    MotorControl.MoveY(ZHlight);
                    await Task.Delay(ZHlight / 15 + 700);
                    MotorControl.MoveY(-ZHlight);

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


        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

}
