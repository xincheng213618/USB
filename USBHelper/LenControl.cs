using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USBDLL
{
    public static class LenControl
    { 

        public static SerialPort serialPort = new SerialPort { };

        public static int OpenPort(string PortName)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort = new SerialPort { PortName = PortName, BaudRate = 115200 };
                    serialPort.Open();
                    byte[] buffer = new byte[2] { 0xFE ,0x01};
                    serialPort.Write(buffer, 0, buffer.Length);

                    for (int i = 0; i < 16; i++)
                    {
                        Thread.Sleep(16);
                        int bytesread = serialPort.BytesToRead;
                        if (bytesread > 0)
                        {
                            byte[] buff = new byte[bytesread];
                            serialPort.Read(buff, 0, bytesread);
                            if (buff[0] == 255)
                            {
                                //serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                                return 0;
                            }
  
                        }
                    }
                    serialPort.Close();
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

        public static Dictionary<int, string> ErrorCode = new Dictionary<int, string>()
        {
            { -1,"端口异常" },
            { -2,"端口占用/不存在端口" },
            { -9," 未定义事件" }
        };


        public static void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
        }


        public static void SendMsg(byte[] msg)
        {
            if (serialPort.IsOpen)
                serialPort.Write(msg, 0, msg.Length);
        }


        public static int Read()
        {
            if (serialPort.IsOpen)
            {
                byte[] buffer = new byte[2] { 0xFE, 0x02 };
                serialPort.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 16; i++)
                {
                    Thread.Sleep(16);
                    int bytesread = serialPort.BytesToRead;
                    if (bytesread > 0)
                    {
                        byte[] buff = new byte[bytesread];
                        serialPort.Read(buff, 0, bytesread);

                        if (buff.Length == 2)
                        {
                            if (buff[0] == 1)
                            {
                                return 1;
                            }
                            else if (buff[0] == 2)
                            {
                                return 2;
                            }
                            else if (buff[0] == 3)
                            {
                                return 3;
                            }
                            else if (buff[0] == 4)
                            {
                                return 4;
                            }
                            else
                            {
                                return -9;
                            }
                        }

                    }
                }
                return -2;
            }
            return -1;
        }


    }
}
