using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;

namespace RootKill.Component.Logitech.Lcd
{
    public class Controller: IDisposable
    {
        #region Properties: Image
        public bool ResizeImage { get; set; }
        public Device BlackAndWhiteImage
        {
            get { return _deviceType; }
        }
        public Size ImageSize { get; private set; }
        #endregion Properties: Image

        #region Private Members: Device Connection
        private int _connection = Api.DMcLgLCD.LGLCD_INVALID_CONNECTION;
        private int _device = Api.DMcLgLCD.LGLCD_INVALID_DEVICE;
        private Device _deviceType = Device.None;
        #endregion Private Members: Device Connection

        #region Private Members: Buttons
        private Timer _timer;
        private Dictionary<Button, bool> _buttonStatus;
        private Dictionary<Button, uint> _buttonLink;
        #endregion Private Members: Buttons

        #region Constructor
        public Controller()
        {
            ResizeImage = false;
            CtorDefineButtonMembers();
            CtorConnectDevice();
        }
        private void CtorConnectDevice()
        {
            _connection = Api.DMcLgLCD.LGLCD_INVALID_CONNECTION;
            _device = Api.DMcLgLCD.LGLCD_INVALID_DEVICE;
            _deviceType = Device.None;
            ImageSize = new Size(160, 43);

            //Init Lcd Api
            if (Api.DMcLgLCD.LcdInit() != Api.DMcLgLCD.ERROR_SUCCESS) return;

            // Make Connection
            _connection = Api.DMcLgLCD.LcdConnectEx("Rootkill.Lcd.Controller" + DateTime.Now.ToString(), 0, 0);
            if (Api.DMcLgLCD.LGLCD_INVALID_CONNECTION == _connection) return;

            //Determine Device
            _device = Api.DMcLgLCD.LcdOpenByType(_connection, Api.DMcLgLCD.LGLCD_DEVICE_QVGA);
            if (_device != Api.DMcLgLCD.LGLCD_INVALID_DEVICE)
            {
                _deviceType = Device.Qvga;
                ImageSize = new Size(320, 240);
                _timer.Enabled = true;
                return;
            }
            _device = Api.DMcLgLCD.LcdOpenByType(_connection, Api.DMcLgLCD.LGLCD_DEVICE_BW);
            if (_device != Api.DMcLgLCD.LGLCD_INVALID_DEVICE)
            {
                _deviceType = Device.BlackAndWhite;
                _timer.Enabled = true;
                return;
            }
        }
        private void CtorDefineButtonMembers()
        {
            _timer = new Timer(25);
            _timer.Elapsed += _timer_Elapsed;

            _buttonStatus = new Dictionary<Button, bool>();
            _buttonLink = new Dictionary<Button, uint>();
            CtorAddButtonLink(Button.G13_Button_1, Api.DMcLgLCD.LGLCD_BUTTON_1);
            CtorAddButtonLink(Button.G13_Button_2, Api.DMcLgLCD.LGLCD_BUTTON_2);
            CtorAddButtonLink(Button.G13_Button_3, Api.DMcLgLCD.LGLCD_BUTTON_3);
            CtorAddButtonLink(Button.G13_Button_4, Api.DMcLgLCD.LGLCD_BUTTON_4);
            CtorAddButtonLink(Button.G19_Button_Left, Api.DMcLgLCD.LGLCD_BUTTON_LEFT);
            CtorAddButtonLink(Button.G19_Button_Right, Api.DMcLgLCD.LGLCD_BUTTON_RIGHT);
            CtorAddButtonLink(Button.G19_Button_Ok, Api.DMcLgLCD.LGLCD_BUTTON_OK);
            CtorAddButtonLink(Button.G19_Button_Cancel, Api.DMcLgLCD.LGLCD_BUTTON_CANCEL);
            CtorAddButtonLink(Button.G19_Button_Up, Api.DMcLgLCD.LGLCD_BUTTON_UP);
            CtorAddButtonLink(Button.G19_Button_Down, Api.DMcLgLCD.LGLCD_BUTTON_DOWN);
            CtorAddButtonLink(Button.G19_Button_Menu, Api.DMcLgLCD.LGLCD_BUTTON_MENU);
        }
        private void CtorAddButtonLink(Button button, uint lcdButton)
        {
            _buttonLink.Add(button, lcdButton);
            _buttonStatus.Add(button, false);
        }
        #endregion Constructor

        #region Destructor
        ~Controller()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_timer != null)
                    {
                        _timer.Close();
                        _timer = null;
                    }
                }
                if (_device != Api.DMcLgLCD.LGLCD_INVALID_DEVICE)
                {
                    Api.DMcLgLCD.LcdClose(_device);
                    _device = Api.DMcLgLCD.LGLCD_INVALID_DEVICE;
                }
                if (_connection != Api.DMcLgLCD.LGLCD_INVALID_CONNECTION)
                {
                    Api.DMcLgLCD.LcdDisconnect(_connection);
                    _connection = Api.DMcLgLCD.LGLCD_INVALID_CONNECTION;
                }
                Api.DMcLgLCD.LcdDeInit();
            }
            catch
            {
                // Do Nothing
            }
        }
        #endregion Destructor

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
                if (_deviceType == Device.BlackAndWhite) Api.DMcLgLCD.LcdUpdateBitmap(_device, resized.GetHbitmap(), Api.DMcLgLCD.LGLCD_DEVICE_BW);
                else Api.DMcLgLCD.LcdUpdateBitmap(_device, resized.GetHbitmap(), Api.DMcLgLCD.LGLCD_DEVICE_QVGA);
                Api.DMcLgLCD.LcdSetAsLCDForegroundApp(_device, Api.DMcLgLCD.LGLCD_FORE_YES);
            }
            catch
            {
                // Do nothing
            }
        }
        #endregion DotheRendering

        #region Events: Button
        public event EventHandler ButtonDown;
        private void OnButtonDown(Button button)
        {
            if (ButtonDown == null) return;
            ButtonDown(this, new LogitechLcdEventArgs()
            {
                Button = button,
                ActiveButtons = GetActiveButtons()
            });
        }

        public event EventHandler ButtonUp;
        private void OnButtonUp(Button button)
        {
            if (ButtonUp == null) return;
            ButtonUp(this, new LogitechLcdEventArgs()
            {
                Button = button,
                ActiveButtons = GetActiveButtons()
            });
        }

        private List<Button> GetActiveButtons()
        {
            var list = new List<Button>();
            foreach (var item in _buttonStatus) 
                if (item.Value) 
                    list.Add(item.Key);
            return list;
        }
        #endregion Events: Button

        #region Timer: Buttons
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            uint buttons = Api.DMcLgLCD.LcdReadSoftButtons(_device);
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
        #endregion Timer: Buttons
    }
}
