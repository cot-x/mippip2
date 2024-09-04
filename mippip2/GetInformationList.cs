using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace mippip2
{
    class GetInformationList : Form
    {
        public string SelectedItem
        {
            private set;
            get;
        }

        public GetInformationList(string title, string[] items)
        {
            this.Text = MainForm.AppName + " (" + title + ")";
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(360, 155);
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

            ListBox listBox = new ListBox();
            listBox.Location = new Point(4, 4);
            listBox.Size = new Size(this.ClientSize.Width - 8, listBox.Height);
            listBox.BackColor = Color.Green;
            listBox.ForeColor = Color.White;
            listBox.Items.AddRange(items);
            this.Controls.Add(listBox);

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(this.Width - cancelButton.Width - 8, listBox.Bottom + 4);
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
                this.SelectedItem = (string)listBox.SelectedItem;
                Close();
            };
            this.Controls.Add(button);
            this.AcceptButton = button;
        }
    }
}
