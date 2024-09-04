using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace mippip2
{
    class InputSecretDialog : Form
    {
        private TextBox textBox;

        public string AuthToken
        {
            get
            {
                return textBox.Text;
            }
        }

        public InputSecretDialog()
        {
            this.Text = MainForm.AppName + " (暗証番号)";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(192, 112);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.Green;
            this.TopMost = true;

            textBox = new TextBox();
            textBox.Location = new Point(8, 16);
            textBox.BackColor = Color.Green;
            textBox.ForeColor = Color.White;
            this.Controls.Add(textBox);

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(this.ClientSize.Width - cancelButton.Width - 2, this.ClientSize.Height - cancelButton.Height - 2);
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
                this.Close();
            };
            this.Controls.Add(button);
            this.AcceptButton = button;
        }
    }
}
