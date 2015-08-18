using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rootkill.App.SysTray.Apps
{
    internal interface IApp
    {
        event EventHandler StopCalled;
        bool IsRunning();
        void Start();
        void Stop();
    }
}
