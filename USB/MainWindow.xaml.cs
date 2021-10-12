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

        private Timer timer;

        MotorControl MotorControl = new MotorControl();
        LenControl LenControl = new LenControl();

        private void Window_Initialized(object sender, EventArgs e)               
        {
            timer = new Timer(_ => Dispatcher.BeginInvoke(new Action(() => TimeRun())), null, 0, 500);//本来是60，不过没必要刷新这么快，就1s1次就好。

            DataContext = MotorControl;
            LenLabel.DataContext = LenControl;
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
            if (!LenControl.serialPort.IsOpen)
            {
                int Code = LenControl.Initialized();
                OpenShow1.Background = Code == 0 ? Brushes.Red : Brushes.Green;
            }
            else
            {
                OpenShow1.Background = LenControl.serialPort.IsOpen ? Brushes.Red : Brushes.Green;
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
                    int ZHlight = int.Parse(Num.Text);
                    MotorControl.MoveY(ZHlight);
                    await Task.Delay(ZHlight / 15 + 700);

                    LenControl.Read();



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
                if (x == 0)
                {
                    MessageBox.Show("连接成功");
                    return;
                }
            }

        }

        private void Len_Read(object sender, RoutedEventArgs e)
        {
            LenControl.Read();
        }


        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

}
