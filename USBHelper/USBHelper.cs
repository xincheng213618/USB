using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace USBDLL
{
    public static class Helper
    {
        public static SerialPort serialPort = new SerialPort { };


        public static byte[] name  = new byte[] { 0x2F, 0x31, 0x61, 0x4D, 0x31, 0x6A, 0x31, 0x36, 0x6D, 0x33, 0x30, 0x68, 0x31, 0x35, 0x4C, 0x31, 0x30, 0x30, 0x56, 0x31, 0x36, 0x30, 0x30, 0x30, 0x61, 0x4D, 0x32, 0x6A, 0x31, 0x36, 0x6D, 0x33, 0x30, 0x68, 0x31, 0x35, 0x4C, 0x31, 0x30, 0x30, 0x56, 0x31, 0x36, 0x30, 0x30, 0x30, 0x61, 0x4D, 0x33, 0x6A, 0x31, 0x36, 0x6D, 0x33, 0x30, 0x68, 0x31, 0x35, 0x4C, 0x31, 0x30, 0x30, 0x56, 0x31, 0x36, 0x30, 0x30, 0x30, 0x61, 0x4D, 0x34, 0x6A, 0x31, 0x36, 0x6D, 0x33, 0x30, 0x68, 0x31, 0x35, 0x4C, 0x31, 0x30, 0x30, 0x56, 0x31, 0x36, 0x30, 0x30, 0x30, 0x52, 0x0D, 0x0A };

        public static string SetMsg = "/1aM1j16m30h15L100V16000aM2j16m30h15L100V16000aM3j16m30h15L100V16000aM4j16m30h15L100V16000R\r";


        public static int OpenPort(string PortName)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort = new SerialPort { PortName = PortName, BaudRate = 9600 };
                    serialPort.Open();
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(SetMsg);
                    serialPort.Write(buffer, 0, buffer.Length);

                    for (int i = 0; i < 16; i++)
                    {
                        Thread.Sleep(16);
                        int bytesread = serialPort.BytesToRead;
                        if (bytesread > 0)
                        {
                            byte[] buff = new byte[bytesread];
                            serialPort.Read(buff, 0, bytesread);
                            if (buff.Length == 8)
                                if (buff[3] == 64)
                                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                                    return 0;
                            serialPort.Close();
                            return -1;
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

        private static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            Thread.Sleep(50);
            int bytesread = serialPort.BytesToRead;
            byte[] buff = new byte[bytesread];
            serialPort.Read(buff, 0, bytesread);
            //这里必须要用异步,返回原本线程
        }

        public static void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
           }
        }


        public static void SendMsg(byte []  msg)
        {
            if (serialPort.IsOpen)
                serialPort.Write(msg, 0, msg.Length);
        }
        public static void SendMsg(string Msg)
        {
            Msg.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
            Msg += "\r";
            SendMsg(System.Text.Encoding.UTF8.GetBytes(Msg));
        }

    }
}
