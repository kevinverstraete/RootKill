using rka = Rootkill.App.SysTray.Apps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Rootkill.App.SysTray.Apps;

namespace Rootkill.App.SysTray
{
    public class SysTrayApp : Form
    {
        #region App: StartApp
        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }
        #endregion App: StartApp

        #region Tray: Private TrayItems Enum
        private enum TrayItems
        {
            LogitechAutoClicker
        }
        #endregion Tray: Private TrayItems Enum

        #region Tray: Private Members
        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private Dictionary<string, MenuItem> _menuItems = new Dictionary<string, MenuItem>();
        #endregion Tray: Private Members

        #region App: Ctor
        public SysTrayApp()
        {
            CreateSystemTray();
        }
        #endregion App: Ctor

        #region Tray: Create
        private void CreateSystemTray()
        {
            CreateSystemTrayMenu();
            CreateSystemTrayIcon();
        }
        private void CreateSystemTrayMenu()
        {
            // Tray: Clear Menu Items
            _menuItems = new Dictionary<string, MenuItem>();

            // Tray: Define Menu Items
            CreateMenuItem(TrayItems.LogitechAutoClicker.ToString(), "Logitech AutoClicker", new rka.LogitechAutoclicker.Clicker());

            // Tray: Build Menu
            _trayMenu = new ContextMenu();
            foreach (var item in _menuItems)
            {
                _trayMenu.MenuItems.Add(item.Value);
            }
            _trayMenu.MenuItems.Add("Exit", OnExit);
        }
        private void CreateMenuItem(string name, string text, IApp iApp)
        {
            var item = new MenuItem();
            item.Text = text;
            item.Name = name;
            item.Tag = iApp;
            item.Click += OnTrayItemClick;
            iApp.StopCalled += OnStopClicked;
            _menuItems.Add(name, item);
        }
        private void CreateSystemTrayIcon()
        {
            // Tray: Icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "RootKill Apps";
            _trayIcon.Icon = new Icon(RootKill.Icon.IconsLand.Shields.DataShield, 32, 32);
            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;
        }
        #endregion Tray: Create

        #region Tray: Item Click
        void OnTrayItemClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var app = (IApp)(menuItem.Tag);
            if (app.IsRunning())
            {
                menuItem.Checked = false;
                app.Stop();
            }
            else
            {
                menuItem.Checked = true;
                app.Start();
            }
        }
        void OnStopClicked(object sender, EventArgs e)
        {
            foreach (var item in _menuItems)
            {
                var menuItem = (MenuItem)item.Value;
                var app = (IApp)(menuItem.Tag);
                if (app == (IApp)sender)
                {
                    menuItem.Checked = false;
                    app.Stop();
                }
            }
        }
        #endregion Tray: Item Click

        #region BoilerPlate
        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }
        private void OnExit(object sender, EventArgs e)
        {
            foreach (var item in _menuItems)
            {
                var menuItem = (MenuItem)item.Value;
                var iApp = (IApp)menuItem.Tag;
                if (iApp.IsRunning()) iApp.Stop();
            }
            Application.Exit();
        }
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }
        #endregion BoilerPlate
    }
}
