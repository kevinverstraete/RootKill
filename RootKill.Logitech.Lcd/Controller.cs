using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;

namespace RootKill.Logitech.Lcd
{
    public enum Button
    {
        G13_Button_1,
        G13_Button_2,
        G13_Button_3,
        G13_Button_4,
        G19_Button_Left,
        G19_Button_Right,
        G19_Button_Ok,
        G19_Button_Cancel,
        G19_Button_Up,
        G19_Button_Down,
        G19_Button_Menu
    }

    public class LogitechLcdEventArgs : EventArgs
    {
        public Button Button { get; set; }
        public List<Button> ActiveButtons { get; set; }
    }

    public class Controller
    {
        #region Enums
        private enum Device
        {
            None = 0,
            BlackAndWhite = 1,
            Qvga = 2
        }
        #endregion Enums

        #region Properties
        public bool ResizeImage { get; set; }
        public Size ImageSize { get; private set; }
        #endregion Properties

        #region Device Connection
        private int _connection = Api.LGLCD_INVALID_CONNECTION;
        private int _device = Api.LGLCD_INVALID_DEVICE;
        private Device _deviceType = Device.None;
        #endregion Device Connection

        #region Constructor
        public Controller()
        {
            ResizeImage = false;

            _timer = new Timer(25);
            _timer.Elapsed += _timer_Elapsed;

            _buttonStatus = new Dictionary<Button, bool>();
            _buttonLink = new Dictionary<Button, uint>();
            AddButtonLink(Button.G13_Button_1, Api.LGLCD_BUTTON_1);
            AddButtonLink(Button.G13_Button_2, Api.LGLCD_BUTTON_2);
            AddButtonLink(Button.G13_Button_3, Api.LGLCD_BUTTON_3);
            AddButtonLink(Button.G13_Button_4, Api.LGLCD_BUTTON_4);
            AddButtonLink(Button.G19_Button_Left, Api.LGLCD_BUTTON_LEFT);
            AddButtonLink(Button.G19_Button_Right, Api.LGLCD_BUTTON_RIGHT);
            AddButtonLink(Button.G19_Button_Ok, Api.LGLCD_BUTTON_OK);
            AddButtonLink(Button.G19_Button_Cancel, Api.LGLCD_BUTTON_CANCEL);
            AddButtonLink(Button.G19_Button_Up, Api.LGLCD_BUTTON_UP);
            AddButtonLink(Button.G19_Button_Down, Api.LGLCD_BUTTON_DOWN);
            AddButtonLink(Button.G19_Button_Menu, Api.LGLCD_BUTTON_MENU);

            _connection = Api.LGLCD_INVALID_CONNECTION;
            _device = Api.LGLCD_INVALID_DEVICE;
            _deviceType = Device.None;
            ImageSize = new Size(160, 43);

            //Init Lcd Api
            if (Api.LcdInit() != Api.ERROR_SUCCESS) return;

            // Make Connection
            _connection = Api.LcdConnectEx("Rootkill.Lcd.Controller", 0, 0);
            if (Api.LGLCD_INVALID_CONNECTION == _connection) return;

            //Determine Device
            _device = Api.LcdOpenByType(_connection, Api.LGLCD_DEVICE_QVGA);
            if (_device != Api.LGLCD_INVALID_DEVICE)
            {
                _deviceType = Device.Qvga;
                ImageSize = new Size(320, 240);
                _timer.Enabled = true;
                return;
            }
            _device = Api.LcdOpenByType(_connection, Api.LGLCD_DEVICE_BW);
            if (_device != Api.LGLCD_INVALID_DEVICE)
            {
                _deviceType = Device.BlackAndWhite;
                _timer.Enabled = true;
                return;
            }
        }
        private void AddButtonLink(Button button, uint lcdButton)
        {
            _buttonLink.Add(button, lcdButton);
            _buttonStatus.Add(button, false);
        }
        #endregion Constructor

        #region DotheRendering
        public void Render(Bitmap bitmap)
        {
            try
            {
                if (_deviceType == Device.None) return;
                if (bitmap == null) return;

                var newSize = new Size(bitmap.Size.Width, bitmap.Size.Height);
                if (ResizeImage)
                {
                    var widthScale = (bitmap.Size.Width > ImageSize.Width) ? (double)bitmap.Size.Width / (double)ImageSize.Width : 1.0;
                    var heightScale = (bitmap.Size.Height > ImageSize.Height) ? (double)bitmap.Size.Height / (double)ImageSize.Height : 1.0;
                    if (heightScale > widthScale) widthScale = heightScale;
                    else heightScale = widthScale;
                    newSize = new Size(Convert.ToInt32(bitmap.Size.Width / widthScale), Convert.ToInt32(bitmap.Size.Height / heightScale));
                }

                Bitmap resized = new Bitmap(bitmap, newSize);
                if (_deviceType == Device.BlackAndWhite) Api.LcdUpdateBitmap(_device, resized.GetHbitmap(), Api.LGLCD_DEVICE_BW);
                else Api.LcdUpdateBitmap(_device, resized.GetHbitmap(), Api.LGLCD_DEVICE_QVGA);
                Api.LcdSetAsLCDForegroundApp(_device, Api.LGLCD_FORE_YES);
            }
            catch
            {
                // Do nothing
            }
        }
        #endregion DotheRendering

        #region Buttons

        #region Buttons private members
        private Timer _timer;
        private Dictionary<Button, bool> _buttonStatus;
        private Dictionary<Button, uint> _buttonLink;
        #endregion Buttons private members

        #region Buttons Events
        public event EventHandler ButtonDown;
        private void OnButtonDown(Button button)
        {

            OnButtonDown(new LogitechLcdEventArgs()
            {
                Button = button,
                ActiveButtons = GetActiveButtons()
            });
        }
        private void OnButtonDown(LogitechLcdEventArgs e)
        {
            if (ButtonDown == null) return;
            ButtonDown(this, e);
        }

        public event EventHandler ButtonUp;
        private void OnButtonUp(Button button)
        {
            OnButtonUp(new LogitechLcdEventArgs()
            {
                Button = button,
                ActiveButtons = GetActiveButtons()
            });
        }
        private void OnButtonUp(LogitechLcdEventArgs e)
        {
            if (ButtonUp == null) return;
            ButtonUp(this, e);
        }

        private List<Button> GetActiveButtons()
        {
            var list = new List<Button>();
            foreach (var item in _buttonStatus) if (item.Value) list.Add(item.Key);
            return list;
        }
        #endregion Buttons Events

        #region Buttons Timer
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            uint buttons = Api.LcdReadSoftButtons(_device);
            foreach (var item in _buttonLink)
            {
                CheckButton(buttons, item.Key, item.Value);
            }
        }
        private void CheckButton(uint allButtons, Button button, uint lglcd_button)
        {
            // get previous status
            var previousStatus = false;
            _buttonStatus.TryGetValue(button, out previousStatus);
            // get current Status
            var currentStatus = ((allButtons & lglcd_button) == lglcd_button);
            // Check
            if (previousStatus && !currentStatus)
            {
                _buttonStatus[button] = currentStatus;
                OnButtonUp(button);
            }
            else if (!previousStatus && currentStatus)
            {
                _buttonStatus[button] = currentStatus;
                OnButtonDown(button);
            }
        }
        #endregion Buttons Timer

        #endregion Buttons

        #region Destructor
        ~Controller()
        {
            Api.LcdClose(_device);
            Api.LcdDisconnect(_connection);
            Api.LcdDeInit();
        }
        #endregion Destructor
    }
}
