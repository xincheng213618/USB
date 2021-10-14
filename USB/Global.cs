using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motor;

namespace USB
{
    public static class Global
    {
        public static MotorControl  MotorControl = new MotorControl();
        public static LenControl LenControl = new LenControl();

        public static async void Move(string control ,int num)
        {
            switch (control)
            {
                case "X+":
                    MotorControl.MoveX(num);
                    break;
                case "X-":
                    MotorControl.MoveX(-num);
                    break;
                case "Y+":
                    MotorControl.MoveY(num);
                    break;
                case "Y-":
                    MotorControl.MoveY(-num);
                    break;
                case "X++":
                    MotorControl.MoveX(1000);
                    break;
                case "X--":
                    MotorControl.MoveX(-1000);
                    break;
                case "Y++":
                    MotorControl.MoveY(1000);
                    break;
                case "Y--":
                    MotorControl.MoveY(-1000);
                    break;
                case "Set":
                    MotorControl.SetMove();
                    break;
                case "ReSet":
                    MotorControl.ReSetMove();
                    break;
                case "Z+":
                    MotorControl.MoveZ(num);
                    break;
                case "Z-":
                    MotorControl.MoveZ(-num);
                    break;
                case "Change":
                    MotorControl.MoveAll(h:num);
                    await Task.Delay(num / 15 + 700);

                    LenControl.Read();
                    break;
                case "Change-":
                    MotorControl.MoveAll(h: -num);
                    await Task.Delay(num / 15 + 700);

                    LenControl.Read();
                    break;
                case "Change1":
                    MotorControl.MoveAll(z: 10000);
                    await Task.Delay((int)(0.052 * 10000 + 250));
                    MotorControl.MoveAll(h: -num);

                    await Task.Delay(num / 15 + 700);

                    MotorControl.MoveAll(z: -10000);

                    await Task.Delay((int)(0.052 * 10000 + 250));

                    LenControl.Read();
                    break;
                default:
                    break;
            }
        }




    }
}
