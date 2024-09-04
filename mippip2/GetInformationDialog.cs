using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace mippip2
{
    class GetInformationDialog : Form
    {
        private TextBox textBox;

        public string Information
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }

        public GetInformationDialog(string title)
        {
            this.Text = MainForm.AppName + " (" + title + ")";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(360, 80);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = MousePosition;
            this.Deactivate += delegate
            {
                this.Close();
            };
            this.BackColor = Color.Green;

            textBox = new TextBox();
            textBox.Location = new Point(4, 4);
            textBox.Size = new Size(this.ClientSize.Width - 4, textBox.Height);
            textBox.BackColor = Color.Green;
            textBox.ForeColor = Color.White;
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
    }
}
