using rk = RootKill.Component.Logitech.Lcd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RootKill.Demo.Component.Logitech.Lcd
{
    public partial class Form1 : Form
    {
        private rk.Controller LcdController = new rk.Controller();
        private PictureBox picToLoad;

        public Form1()
        {
            InitializeComponent();
            picToLoad = pic1;

            pic1.Image = Pics.Wave;
            pic2.Image = Pics.DanceBunny;
            pic3.Image = Pics.Surf;
            pic4.Image = Pics.Loading;

            LcdController.ButtonUp += LcdController_ButtonUp;
            LcdController.ButtonDown += LcdController_ButtonDown;
        }


        void LcdController_ButtonUp(object sender, EventArgs e)
        {
            var args = (rk.LogitechLcdEventArgs)e;
            if (args.ActiveButtons.Contains(rk.Button.G13_Button_1)) picToLoad = pic1;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_2)) picToLoad = pic2;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_3)) picToLoad = pic3;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_4)) picToLoad = pic4;
            else picToLoad = pic1;
        }
        void LcdController_ButtonDown(object sender, EventArgs e)
        {
            var args = (rk.LogitechLcdEventArgs)e;
            if (args.ActiveButtons.Contains(rk.Button.G13_Button_1)) picToLoad = pic1;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_2)) picToLoad = pic2;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_3)) picToLoad = pic3;
            else if (args.ActiveButtons.Contains(rk.Button.G13_Button_4)) picToLoad = pic4;
            else picToLoad = pic1;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (picToLoad != null)
                    if (picToLoad.Image != null)
                    {
                        Bitmap default_image = new Bitmap(picToLoad.Image);
                        LcdController.Render(default_image);
                    }
                    else
                    {
                        LcdController.Render(new Bitmap(1, 1));
                    }
            }
            catch
            {
                // Do Nothing
            }
        }

        private void btnLoadPic_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pic1.Load(openFileDialog1.FileName);
            }
        }

        private void chkStretch_CheckedChanged(object sender, EventArgs e)
        {
            LcdController.ResizeImage = chkStretch.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
