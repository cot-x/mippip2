using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace mippip2
{
    class TweetDialog : Form
    {
        private TextBox textBox;
        private bool cursorFirst;
        private string title;

        public string Tweet
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
                if (cursorFirst)
                {
                    textBox.Select(0, 0);
                }
                else
                {
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        public TweetDialog(string title, bool cursorFirst = false)
        {
            this.Text = MainForm.AppName + " (" + title + ")";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(480, 120);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = MousePosition;
            this.BackColor = Color.Green;

            this.cursorFirst = cursorFirst;
            this.title = title;

            textBox = new TextBox();
            textBox.Location = new Point(4, 4);
            textBox.Multiline = true;
            textBox.Size = new Size(this.ClientSize.Width - 4, textBox.Height * 3);
            textBox.BackColor = Color.Green;
            textBox.ForeColor = Color.White;
            textBox.TextChanged += new EventHandler(textBox_TextChanged);
            this.Controls.Add(textBox);

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(this.Width - cancelButton.Width - 8, textBox.Bottom + 4);
            cancelButton.BackColor = Color.Green;
            cancelButton.ForeColor = Color.White;
            cancelButton.Click += delegate
            {
                this.Close();
            };
            this.Controls.Add(cancelButton);
            this.CancelButton = cancelButton;

            Button button = new Button();
            button.Text = "OK";
            button.Location = new Point(cancelButton.Bounds.X - button.Width - 4, cancelButton.Bounds.Y);
            button.BackColor = Color.Green;
            button.ForeColor = Color.White;
            button.Click += delegate
            {
                this.DialogResult = DialogResult.OK;
                Close();
            };
            this.Controls.Add(button);
            this.AcceptButton = button;
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            if (textBox.Text.Length == 0)
            {
                this.Text = MainForm.AppName + " (" + title + ")";
            }
            else
            {
                this.Text = MainForm.AppName + " (" + title + " : " + (140 - textBox.Text.Length) + ")";
            }
        }
    }
}
