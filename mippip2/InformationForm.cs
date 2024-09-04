using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;

namespace mippip2
{
    class InformationForm : Form
    {
        private PictureBox pictureBox;
        private Label location;
        private Button button;
        private string description;

        public InformationForm(MainForm.Person information)
        {
            this.Text = MainForm.AppName + " (" + information.name + ")";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(480, 280);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.Green;
            this.ForeColor = Color.White;

            pictureBox = new PictureBox();
            pictureBox.Size = information.image.Size;
            pictureBox.Location = new Point(8, 8);
            pictureBox.Image = information.image;
            this.Controls.Add(pictureBox);

            Label name = new Label();
            name.Text = information.name + " (" + information.screen_name + ")";
            name.AutoSize = true;
            name.Location = new Point(pictureBox.Width + 32, pictureBox.Bounds.Y + name.Height * 0);
            this.Controls.Add(name);

            LinkLabel url = new LinkLabel();
            url.Text = information.url;
            url.LinkVisited = false;
            url.LinkColor = Color.White;
            url.AutoSize = true;
            url.Location = new Point(pictureBox.Width + 32, pictureBox.Bounds.Y + url.Height * 1);
            url.LinkClicked += delegate
            {
                if (url != null)
                {
                    Process.Start(url.Text);
                }
            };
            this.Controls.Add(url);

            Label tweets = new Label();
            tweets.Text = information.statuses_count + " Tweets";
            tweets.AutoSize = true;
            tweets.Location = new Point(pictureBox.Width + 32, pictureBox.Bounds.Y + tweets.Height * 2);
            this.Controls.Add(tweets);

            Label count = new Label();
            count.Text = "Followers: " + information.followers_count + "  Friends: " + information.friends_count;
            count.AutoSize = true;
            count.Location = new Point(pictureBox.Width + 32, pictureBox.Bounds.Y + count.Height * 3);
            this.Controls.Add(count);

            location = new Label();
            location.Text = "Location: " + information.location;
            location.Size = new Size(this.ClientSize.Width - (pictureBox.Width + 32), location.Height);
            location.Location = new Point(pictureBox.Width + 32, pictureBox.Bounds.Y + location.Height * 4);
            this.Controls.Add(location);

            button = new Button();
            button.Text = "OK";
            button.Location = new Point(this.ClientSize.Width - button.Width - 4, this.ClientSize.Height - button.Height - 4);
            button.BackColor = Color.Green;
            button.ForeColor = Color.White;
            button.Click += delegate
            {
                this.Close();
            };
            this.Controls.Add(button);
            this.AcceptButton = button;
            this.CancelButton = button;

            this.description = information.description;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawString(this.description.Replace('\n', ' '), MainForm.font, Brushes.White,
                new RectangleF(pictureBox.Width + 32, pictureBox.Bounds.Y + location.Height * 5, this.ClientSize.Width - pictureBox.Width - 32, button.Bounds.Y - location.Bounds.Y - location.Height - 4));
        }
    }
}
