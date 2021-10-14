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
using Motor;

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


        private void Window_Initialized(object sender, EventArgs e)               
        {
            timer = new Timer(_ => Dispatcher.BeginInvoke(new Action(() => TimeRun())), null, 0, 2000);//本来是60，不过没必要刷新这么快，就1s1次就好。

            DataContext = Global.MotorControl;
            LenLabel.DataContext = Global.LenControl;
        }

        private void TimeRun()
        {
            if (!Global.MotorControl.serialPort.IsOpen)
            {
                int Code = Global.MotorControl.Initialized();
                OpenShow.Background = Code == 0 ? Brushes.Red : Brushes.Green;
            }
            else
            {
                OpenShow.Background = Global.MotorControl.serialPort.IsOpen ? Brushes.Red : Brushes.Green;
            }
            if (!Global.LenControl.serialPort.IsOpen)
            {
                int Code = Global.LenControl.Initialized();
                OpenShow1.Background = Code == 0 ? Brushes.Red : Brushes.Green;
            }
            else
            {
                OpenShow1.Background = Global.LenControl.serialPort.IsOpen ? Brushes.Red : Brushes.Green;
            }
        }


        private void Move_Click(object sender, RoutedEventArgs e)
        {
            if (!Global.MotorControl.serialPort.IsOpen)
            {
                MessageBox.Show("请检查位移台的是否正确连接");
                return;
            }
            if (Global.MotorControl.IsRun.IsEnabled)
            {
                //MessageBox.Show("正在运动中");
                return;
            }
            Global.Move(((Button)sender).Tag.ToString(), int.Parse(Num.Text));


        }



        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

}
