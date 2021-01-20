using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
            int Code = USBHelper.OpenPort(comboBox.Text);
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
            {
                USBHelper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                Dispatcher.BeginInvoke(new Action(() => GetCurrentStatus()));
            }
        }




        private void GetCurrentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(110);
                USBHelper.SendMsg(i, 2);
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
            USBHelper.Close();
            USBHelper.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            OpenShow.Background = Brushes.Red;
        }




        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            USBHelper.SendMsg(int.Parse(button.Content.ToString()), int.Parse(button.Tag.ToString()));
        }


        int nchange = 0;

        private unsafe void button2_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "Open":
                    int i = ID_FprCap.LIVESCAN_Init();
                    nchange = ID_FprCap.LIVESCAN_GetChannelCount();
                    MessageBox.Show(i==1?"采集器初始化成功": ID_FprCap.Code[i]);
                    break;
                case "Close":
                     i = ID_FprCap.LIVESCAN_Close();
                    MessageBox.Show(i == 1 ? "采集器关闭成功" : ID_FprCap.Code[i]);
                    break;
                case "BeginCapture":
                    i = ID_FprCap.LIVESCAN_BeginCapture(nchange);
                    MessageBox.Show(i == 1 ? "采集准备完成" : ID_FprCap.Code[i]);
                    break;
                case "GetFPRawData":
                    //string pRawData =null;
                    //i = ID_FprCap.LIVESCAN_GetFPRawData(nchange, ref pRawData);
                    //MessageBox.Show(i == 1 ? "获取成功是否保存" : ID_FprCap.Code[i]);
                    int j = 0;
                    i = ID_FprCap.LIVESCAN_GetBright(nchange,ref j);
                    MessageBox.Show(i.ToString());



                    break;


            }

        }
    }
}
