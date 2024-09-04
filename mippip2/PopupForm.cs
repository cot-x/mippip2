using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace mippip2
{
    class PopupForm : Form
    {
        private string information;

        public PopupForm(string information)
        {
            this.information = information;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Opacity = 0.5;
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Text = MainForm.AppName + " Information";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.ClientSize = new Size(160, MainForm.font.Height);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(SystemInformation.WorkingArea.Width - this.Width, SystemInformation.WorkingArea.Height - this.Height * 2);
            this.Paint += delegate (object sender, PaintEventArgs e)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(this.information, MainForm.font, Brushes.Black, this.ClientRectangle, format);
            };

            Timer timer = new Timer();
            timer.Interval = 10000;
            timer.Tick += delegate
            {
                this.Close();
            };
            timer.Start();
        }
    }
}
