using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using rklcd = RootKill.Component.Logitech.Lcd;

namespace Rootkill.App.SysTray.Apps
{
    internal class LogitechAutoclicker: IApp
    {
        #region IApp
        public void Start()
        {
            if (IsRunning()) return;

            Controller = new rklcd.Controller();
            Controller.ButtonUp += Controller_ButtonUp;
            PaintImage();
            _timer = new System.Timers.Timer(40);
            _timer.Elapsed += Timer_Tick;
            _timer.Enabled = true;
        }
        public void Stop()
        {
            if (!IsRunning()) return;
            _timer.Enabled = false;
            Controller.Dispose();
            Controller = null;
        }
        public bool IsRunning()
        {
            return (Controller != null);
        }
        private void OnStopCalled()
        {
            if (StopCalled != null)
            {
                StopCalled(this, new EventArgs());
            }
        }
        public event EventHandler StopCalled;
        #endregion IApp

        #region Private Members
        private rklcd.Controller Controller;
        private System.Timers.Timer _timer;
        private int _count = 0;
        #endregion Private Members

        #region Controller Events
        void Controller_ButtonUp(object sender, EventArgs e)
        {
            var args = (RootKill.Component.Logitech.Lcd.LogitechLcdEventArgs)e;
            if (args.Button == rklcd.Button.G13_Button_1) _count = _count + 1000;
            if (args.Button == rklcd.Button.G13_Button_2) _count = _count + 10000;
            if (args.Button == rklcd.Button.G13_Button_3) _count = 0;
            if (args.Button == rklcd.Button.G13_Button_4) OnStopCalled();
            PaintImage();
        }
        #endregion Controller Events

        #region Timer Events
        private void Timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_count <= 0) return;
                DoMouseClick();
                _count = _count - 1;
                PaintImage();
            }
            catch
            {
                // Do Nothing
            }
        }
        #endregion Timer Events

        #region Paint
        private void PaintImage()
        {
            try
            {
                Bitmap b = new Bitmap(Controller.ImageSize.Width, Controller.ImageSize.Height);
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                Font sFont = new Font("Arial", 7, FontStyle.Regular);
                g.DrawString("Clicks To Go: " + (_count <= 0 ? "No Clicks" : _count.ToString()), sFont, SystemBrushes.WindowText, 0, 0);
                g.DrawString("+1000", sFont, Brushes.Black, 2, 30);
                g.DrawString("+10000", sFont, Brushes.Black, 40, 30);
                g.DrawString("Stop", sFont, Brushes.Black, 87, 30);
                g.DrawString("Quit", sFont, Brushes.Black, 128, 30);
                g.Dispose();
                Controller.Render(b);
            }
            catch
            {
                // Do Nothing
            }
        }
        #endregion Paint

        #region MouseClick
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        #endregion MouseClick

    }
}
