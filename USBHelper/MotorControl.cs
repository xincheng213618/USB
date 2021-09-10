using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        public void Initialized()
        {
            string[] PortNames = SerialPort.GetPortNames();
            //这种写法不允许有多个串口；
            for (int i = 0; i < PortNames.Count(); i++)
            {
                int x = Helper.OpenPort(PortNames[i]);
                if (x == 0)
                    IsOpen = true;
            }
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
        public void Move(int x = 1,int y = 1,int z = 1)
        {

            //发送0位移指令会一直运动，所以切换位1指令
            x = x == 0 ? 1 : x;
            y = y == 0 ? 1 : y;
            z = z == 0 ? 1 : z;


            string Msg = $"/1P{x},{y},{z},1R";
            //D指令
            Helper.SendMsg(Msg);

            x = x == 1 ? 0 : x;
            y = y == 1 ? 0 : y;
            z = z == 1 ? 0 : z;

            X += x;
            Y += y;
            Z += z;
        }




    }
}