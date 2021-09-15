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

namespace USBDLL
{


    /// <summary>
    /// 电机控制封装部分
    /// </summary>
    public class MotorControl: INotifyPropertyChanged
    {

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


        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpen = false;


        //初始化
        public int Initialized()
        {
            string[] PortNames = SerialPort.GetPortNames();
            //这种写法不允许有多个串口；
            for (int i = 0; i < PortNames.Count(); i++)
            {
                int x = Helper.OpenPort(PortNames[i]);
                if (x == 0)
                {
                    Helper.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                    IsOpen = true;
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
            IsOpen = Helper.serialPort.IsOpen;

            if (IsOpen)
            {
                //发送0位移指令会一直运动，所以切换位1指令

                string x1 = x == 0 ? "" : x.ToString();//前为正
                string y1 = y == 0 ? "" : y.ToString();//左为正
                string z1 = z == 0 ? "" : (-z).ToString();//上为正


                string Msg = $"/1P{x1},{y1},{z1},R";
                //D指令
                Helper.SendMsg(Msg);

                X += x;
                Y += y;
                Z += z;
            }

        }




    }
}