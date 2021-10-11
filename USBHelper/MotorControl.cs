using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace USBDLL
{
    /// <summary>
    /// 电机控制封装部分
    /// </summary>
    public class MotorControl: INotifyPropertyChanged
    {
        public static class Port
        {
            public static SerialPort serialPort = new SerialPort { };
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

                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(16);
                            int bytesread = serialPort.BytesToRead;
                            if (bytesread > 0)
                            {
                                byte[] buff = new byte[bytesread];
                                serialPort.Read(buff, 0, bytesread);
                                if (buff.Length == 8 && buff[3] == 64)
                                {
                                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);

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
            public static Dictionary<int, string> OpenCode = new Dictionary<int, string>()
        {
            { 0,"正常" },
            { -1,"端口异常" },
            { -2,"端口占用/不存在端口" },
        };

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


            public static void SendMsg(byte[] msg)
            {
                if (serialPort.IsOpen)
                    serialPort.Write(msg, 0, msg.Length);
            }

            public static  void SendMsg(string Msg)
            {
                Msg.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                Msg += "\r";
                SendMsg(System.Text.Encoding.UTF8.GetBytes(Msg));
            }

        }

        public int runWait =0;

        public int RunWait
        {
            get => runWait; set
            {
                runWait = value;
                NotifyPropertyChanged();
            }
        }


        public DispatcherTimer IsRun = null;

        public MotorControl()
        {
            IsRun = new DispatcherTimer() { IsEnabled = false, Interval = TimeSpan.FromMilliseconds(100) };
            IsRun.Tick += new EventHandler((sender, e) =>
            {
                IsRun.IsEnabled = RunWait-- > 1;

            });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(propertyName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int x;
        /// <summary>
        /// X轴方向
        /// </summary>
        public int X
        {
            get => x; set
            {
                x = value;
                NotifyPropertyChanged();
            }
        }
        public int y;
        /// <summary>
        /// Y轴方向
        /// </summary>
        public int Y
        {
            get => y; set
            {
                y = value;
                NotifyPropertyChanged();
            }
        }


        public int z;
        /// <summary>
        /// Z轴方向
        /// </summary>
        public int Z
        {
            get => z; set
            {
                z = value;
                NotifyPropertyChanged();
            }
        }


        //重连优化代码(保留上次的串口)
        string RePortName = null;

        //初始化
        public int Initialized()
        {

            string[] PortNames;
            string[] TempPortNames = SerialPort.GetPortNames();
            if (RePortName!=null && TempPortNames.Contains(RePortName))
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
                if (Port.OpenPort(PortNames[i]) == 0)
                {
                    RePortName = PortNames[i];
                    Port.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    return 0;
                }
            }
            return -1;


        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            Thread.Sleep(50);
            int bytesread = serialPort.BytesToRead;
            byte[] buff = new byte[bytesread];
            serialPort.Read(buff, 0, bytesread);
            //MessageBox.Show(System.Text.Encoding.UTF8.GetString(buff));
            //这里必须要用异步,返回原本线程
        }
        public void AutoSetMove()
        {
            X = Y = Z = 0;

        }
        public void SetMove()
        {
            X = Y = Z = 0;
        }

        public void ReSetMove()
        {
            Move(-X,-Y,-Z);
        }







        public  void MoveX(int len = 0)
        {
            Move(len, 0, 0);
        }
        public void MoveY(int len = 0)
        {
            Move(0, len, 0);
        }
        public void MoveZ(int len = 0)
        {
            Move(0, 0, len);
        }



        /// <summary>
        /// 位移台的复合调用
        /// </summary>
        /// <param name="x">X距离</param>
        /// <param name="y">Y距离</param>
        /// <param name="z">Z距离</param>
        public void Move(int x = 0,int y = 0,int z = 0)
        {
            if (Port.serialPort.IsOpen)
            {
                //发送0位移指令会一直运动，所以切换位1指令

                string x1 = x == 0 ? "" : x.ToString();//前为正
                string y1 = y == 0 ? "" : y.ToString();//左为正
                string z1 = z == 0 ? "" : (-z).ToString();//上为正

                X += x;
                Y += y;
                Z += z;

                x = Math.Abs(x);
                y = Math.Abs(y);
                z = Math.Abs(z);

                int max = x > y ? (x > z ? x : z) : (y > z ? y : z);

                RunWait = (int)((0.052 * max + 250) / 100);
                IsRun.IsEnabled = true;

                string Msg = $"/1P{x1},{y1},{z1},R";
                //D指令
                Port.SendMsg(Msg);
                
                
                



            }

        }




    }
}