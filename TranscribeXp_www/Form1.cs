using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TranscribeXp_www
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string guid;
        string path;
        private void buttonNewScreen_Click(object sender, EventArgs e)
        {
            guid = Guid.NewGuid().ToString();
            var p = System.AppDomain.CurrentDomain.BaseDirectory;

            if (!Directory.Exists(path + "\\Image"))
            {
                path = p + "\\Image";
                Directory.CreateDirectory(path);
            }
            if (!Directory.Exists(path + "\\Image\\" + guid))
            {
                path = p + "\\Image\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + guid;
                Directory.CreateDirectory(path);
            }

        }

        private void buttonImgName_Click(object sender, EventArgs e)
        {
            string imgName = (sender as Button).Text;
            var lTop = this.Top;
            var lLeft = this.Left;

            this.Top = -this.Height;
            this.Left = -this.Width;

            int iWidth = Screen.PrimaryScreen.Bounds.Width;
            int iHeight = Screen.PrimaryScreen.Bounds.Height;

            using (Image img = new Bitmap(iWidth, iHeight))
            {
                using (Graphics gc = Graphics.FromImage(img))
                {
                    gc.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
                    img.Save(path + "\\" + imgName + ".png");
                }
            }

            this.Top = lTop;
            this.Left = lLeft;
        }

        public void myStaticThreadMethod()
        {
            this.Hide();
        }
    }
}
