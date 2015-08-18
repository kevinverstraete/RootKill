using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootKill.Component.Logitech.Lcd
{
    public class LogitechLcdEventArgs : EventArgs
    {
        public Button Button { get; set; }
        public List<Button> ActiveButtons { get; set; }
    }
}
