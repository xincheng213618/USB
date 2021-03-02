using System.IO.Ports;
using System.Threading;

namespace USBHelper
{
    public static class USBHelper
    {
        public static SerialPort serialPort = new SerialPort { };
        public static int OpenPort(string PortName)
        {
            try
            {
                if (!serialPort.IsOpen)
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
                                    //serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                                    return 0;
                                }
                            }
                            break;
                        }
                    }
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

        public static int Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
                return 0;
            }
            else
            {
                return -1;
            }
        }

        private static int[] Coo = new int[9] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };
        private static int[] Function = new int[3] { 0x30, 0x31, 0x3F };

        public static void SendMsg(int Num, int Func)
        {
            if (serialPort.IsOpen)
            {
                Num -= 1;
                serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, (byte)Coo[Num], 0x3D, (byte)Function[Func], 0x0D }, 0, 7);
            }

        }

        


    }
}
