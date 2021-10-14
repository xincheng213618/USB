using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Motor
{
    public class LenControl : INotifyPropertyChanged
    {

        public SerialPort serialPort;

        public LenControl()
        {
            serialPort = new SerialPort();
        }
        public int OpenPort(string PortName)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort = new SerialPort { PortName = PortName, BaudRate = 115200 };
                    serialPort.Open();
                    byte[] buffer = new byte[4] { 0xFE, 0x01, 0x0D, 0x0A };
                    serialPort.Write(buffer, 0, buffer.Length);

                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(16);
                        int bytesread = serialPort.BytesToRead;
                        if (bytesread > 0)
                        {
                            byte[] buff = new byte[bytesread];
                            serialPort.Read(buff, 0, bytesread);
                            if (buff[0] == 255)
                            {
                                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                                return 0;
                            }

                        }
                    }
                    serialPort.Close();
                    serialPort.Dispose();
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return -2;
            }
        }


        public Dictionary<int, string> ErrorCode = new Dictionary<int, string>()
            {
                { -1,"端口异常" },
                { -2,"端口占用/不存在端口" },
                { -9," 未定义事件" }
            };

        public void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
        }
        public void SendMsg(byte[] msg)
        {
            if (serialPort.IsOpen)
                serialPort.Write(msg, 0, msg.Length);
        }

        public void Read()
        {
            if (serialPort.IsOpen)
            {
                byte[] buffer = new byte[4] { 0xFE, 0x02, 0x0D, 0x0A };
                serialPort.Write(buffer, 0, buffer.Length);
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

            if (buff.Length == 2)
            {
                if (buff[1] == 0)
                {
                    Len = 0;
                }
                else if (buff[1] == 1)
                {
                    Len = 1;
                }
                else if (buff[1] == 2)
                {
                    Len = 2;
                }
                else if (buff[1] == 3)
                {
                    Len = 3;
                }
                else if (buff[1] == 4)
                {
                    Len = 4;
                }

            }

        }

        //重连优化代码(保留上次的串口)
        string RePortName =null;
        public int Initialized()
        {
            string[] PortNames;
            string[] TempPortNames = SerialPort.GetPortNames();
            if (RePortName != null && TempPortNames.Contains(RePortName))
            {
                PortNames = new string[TempPortNames.Length + 1];
                TempPortNames.CopyTo(PortNames, 1);
                PortNames[0] = RePortName;
                PortNames = PortNames.Distinct().ToArray();
            }
            else
            {
                PortNames = TempPortNames;
            }

            //这种写法不允许有多个串口；
            for (int i = 0; i < PortNames.Count(); i++)
            {
                if (OpenPort(PortNames[i]) == 0)
                {
                    RePortName = PortNames[i];
                    Read();
                    return 0;
                }
            }
            return -1;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public int len=0;
        /// <summary>
        /// X轴方向
        /// </summary>
        public int Len
        {
            get => len; set
            {
                len = value;
                NotifyPropertyChanged();
            }
        }












    }
}
