using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        private SerialPort serialPort;

        List<Border> ShowBorders;
        List<Button> ClickButton;

        private void Window_Initialized(object sender, EventArgs e)
        {
            ShowBorders = new List<Border> { Show1, Show2, Show3, Show4, Show5, Show6, Show7, Show8, Show9 };
            ClickButton = new List<Button> { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9 };

            string[] PortNames = SerialPort.GetPortNames();
            for (int i = 0; i < PortNames.Count(); i++)
                comboBox.Items.Add(PortNames[i]);   //将数组内容加载到comboBox控件中
            comboBox.SelectedIndex = 0;
            Open();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }


        


        public void Open()
        {
            int Code = USBHelper.USBHelper.OpenPort(comboBox.Text);
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
            {
                USBHelper.USBHelper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                Dispatcher.BeginInvoke(new Action(() => GetCurrentStatus()));
            }
            else
            {
                MessageBox.Show("连不上服务器");
            }
        }




        private async void GetCurrentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                await Task.Delay(170);
                USBHelper.USBHelper.SendMsg(i, 2);
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
                try
                {
                    int num = keyValuePairs[buff[1]];
                    ShowBorders[num].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                    ClickButton[num].Tag = buff[3] == 48 ? 1 : 0;
                }
                catch
                {

                }
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


        int nchange = 0;

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
