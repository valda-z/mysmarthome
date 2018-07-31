using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeJablotron
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
                if(!(lastMove.AddMinutes(delayToOff) < DateTime.Now
                    && lastMove.AddMinutes(delayToOff+delayToOn) > DateTime.Now))
                {
                    lastMove = DateTime.Now;
                }
            }
        }
    }
}
