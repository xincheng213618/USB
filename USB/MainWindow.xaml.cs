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

        private void Window_Initialized(object sender, EventArgs e)
        {
            ShowBorders = new List<Border> { Show1, Show2, Show3, Show4, Show5, Show6, Show7, Show8, Show9 };
            string[] PortNames = SerialPort.GetPortNames();
            for (int i = 0; i < PortNames.Count(); i++)

            {
                comboBox.Items.Add(PortNames[i]);   //将数组内容加载到comboBox控件中
            }
            comboBox.SelectedIndex = 0;

            int Code = OpenPort(comboBox.Text);
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
                GetCurrentStatus();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int Code = OpenPort(comboBox.Text);
            OpenShow.Background = Code != 0 ? Brushes.Red : Brushes.Green;
            if (Code == 0)
                GetCurrentStatus();
        }

        private int OpenPort(string PortName)
        {
            try
            {
                serialPort = new SerialPort { PortName = PortName, BaudRate = 9600 };
                serialPort.Open();
                serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x31, 0x3D, 0x3F, 0x0D }, 0, 7);

                for (int i = 0; i < 16; i++)
                {
                    Thread.Sleep(16);
                    int bytesread = serialPort.BytesToRead;
                    if (bytesread > 0)
                    {
                        byte[] buff = new byte[bytesread];
                        serialPort.Read(buff, 0, bytesread);

                        if (buff.Length == 5)
                        {
                            if (buff[0] == 115)
                            {
                                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                                return 0;
                            }
                        }
                        break;
                    }
                }
                return -1;
            }
            catch
            {
                return -2;
            }

        }


        private void GetCurrentStatus()
        {
            for (int i = 1; i < 10; i++)
            {
                Thread.Sleep(200);
                Send(i, 2);
            }
        }


        private  void DataReceived(object sender, SerialDataReceivedEventArgs e)
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
                switch (buff[1])
                {
                    case 0x31:
                        ShowBorders[0].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x32:
                        ShowBorders[1].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x33:
                        ShowBorders[2].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x34:
                        ShowBorders[3].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x35:
                        ShowBorders[4].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x36:
                        ShowBorders[5].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x37:
                        ShowBorders[6].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x38:
                        ShowBorders[7].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                    case 0x39:
                        ShowBorders[8].Background = buff[3] == 48 ? Brushes.Red : Brushes.Green;
                        break;
                }
            }
        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            serialPort.Close();
            serialPort.Dispose();
            OpenShow.Background = Brushes.Red;

        }

        int[] Coo = new int[9] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };
        int[] Function = new int[3] { 0x30,0x31, 0x3F };

        private void Send(int n ,int code)
        {
            n -= 1;
            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, (byte)Coo[n], 0x3D, (byte)Function[code], 0x0D }, 0, 7);
            //ShowBorders[n].Background = code == 0 ? Brushes.Red : Brushes.Green;
        }





        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "0x31":
                    Send(1,1);      
                    break;
                case "0x32":
                    Send(2, 1);
                    break;
                case "0x33":
                    Send(3, 1);
                    break;
                case "0x34":
                    Send(4, 1);
                    break;
                case "0x35":
                    Send(5, 1);
                    break;
                case "0x36":
                    Send(6, 1);
                    break;
                case "0x37":
                    Send(7, 1);
                    break;
                case "0x38":
                    Send(8, 1);
                    break;
                case "0x39":
                    Send(9, 1);
                    break;
                case "0x61":
                    Send(1, 0);
                    break;
                case "0x62":
                    Send(2, 0);
                    break;
                case "0x63":
                    Send(3, 0);
                    break;
                case "0x64":
                    Send(4, 0);
                    break;
                case "0x65":
                    Send(5, 0);
                    break;
                case "0x66":
                    Send(6, 0);
                    break;
                case "0x67":
                    Send(7, 0);
                    break;
                case "0x68":
                    Send(8, 0);
                    break;
                case "0x69":
                    Send(9, 0);
                    break;
            }
        }

        //private void Send_Click(object sender, RoutedEventArgs e)
        //{
        //    Button button = sender as Button;
        //    //serialPort.Write(new byte[1] { (byte)button.Tag }, 0, 1);
        //    switch (button.Tag)
        //    {
        //        case "0x31":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x31, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x32":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x32, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x33":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x33, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x34":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x34, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x35":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x35, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x36":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x36, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x37":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x37, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x38":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x38, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x39":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x39, 0x3D, 0x31, 0x0D }, 0, 7);
        //            break;
        //        case "0x61":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x31, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x62":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x32, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x63":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x33, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x64":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x34, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x65":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x35, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x66":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x36, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x67":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x37, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x68":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x38, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //        case "0x69":
        //            serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, 0x39, 0x3D, 0x30, 0x0D }, 0, 7);
        //            break;
        //    }
        //}

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
