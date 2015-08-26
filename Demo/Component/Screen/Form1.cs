using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using rcs = RootKill.Component.Screen;

namespace RootKill.Demo.Component.Screen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            timerDesktop.Enabled = true ;
        }

        private void timerDesktop_Tick(object sender, EventArgs e)
        {
            ShowDesktop();
        }

        private void ShowDesktop()
        {
            picDesktop.Image = rcs.Desktop.Capture(true);
        }
    }
}
