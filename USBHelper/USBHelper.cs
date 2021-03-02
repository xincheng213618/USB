using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace USBDLL
{
    public static class Helper
    {
        public static SerialPort serialPort = new SerialPort { };
        public static Dictionary<int, int> CurrentKeyValuePairs = Util.NumsToDic(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        public static Dictionary<byte, int> CodeKeyValuePairs = new Dictionary<byte, int>
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
                                if (buff[0] == 115)
                                    return 0;
                            return -1;
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

        public static void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
           }
        }

        private static int[] CooCode = new int[9] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };
        private static int[] FunctionCode = new int[3] { 0x30, 0x31, 0x3F };

        public static void SendMsg(int Num, int Function)
        {
            if (serialPort.IsOpen)
            {
                Num -= 1;
                serialPort.Write(new byte[7] { 0x61, 0x74, 0x73, (byte)CooCode[Num], 0x3D, (byte)FunctionCode[Function], 0x0D }, 0, 7);
            }
        }

        public static async void SendMsg(Dictionary<int, int> keyValuePairs)
        {
            Dictionary<int, int> TempKeyValuePairs = new Dictionary<int, int> { };

            foreach (var item in CurrentKeyValuePairs)
                TempKeyValuePairs.Add(item.Key, item.Value);

            foreach (var item in keyValuePairs)
            {
                if (TempKeyValuePairs[item.Key] != item.Value|| item.Value==2)
                {
                    await Task.Delay(180);
                    SendMsg(item.Key + 1, item.Value);
                }
            }
        }

    }

    public static class Util
    {
        public static Dictionary<int, int> NumsToDic(int[] Nums)
        {
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int> { };
            for (int i = 0; i < Nums.Length; i++)
            {
                keyValuePairs.Add(i, Nums[i]);
            }
            return keyValuePairs;
        }
    }
}
