using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeCore
{
    public static class StaticJablotronState
    {
        private static DateTime lastMove = DateTime.Now.AddHours(-1);

        const int delayToOff = 10;
        const int delayToOn = 5;

        public static bool IsMovementThere
        {
            get { return (lastMove.AddMinutes(delayToOff)>DateTime.Now); }
            set
            {
                Console.WriteLine(" >>last-move: try to set - now:{0}  => lastMove:{1}", 
                    DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo),
                    lastMove.ToString("u", DateTimeFormatInfo.InvariantInfo));
                if (!(lastMove.AddMinutes(delayToOff) < DateTime.Now
                    && lastMove.AddMinutes(delayToOff+delayToOn) > DateTime.Now))
                {
                    Console.WriteLine(" >>last-move: SET");
                    lastMove = DateTime.Now;
                }
            }
        }
    }
}
