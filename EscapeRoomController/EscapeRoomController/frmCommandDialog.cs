using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EscapeRoomController
{
    public partial class frmCommandDiaglog : Form
    {

        private EscapeRoomController Erc;

        public frmCommandDiaglog(EscapeRoomController erc)
        {
            Erc = erc;

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //var screen = Screen.FromPoint(this.Location);
            //this.Location = new Point(screen.WorkingArea.Right - this.Width, screen.WorkingArea.Bottom - this.Height);
            int x = Screen.PrimaryScreen.WorkingArea.Left + 25;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - (this.Height + 25);
            this.Location = new Point(x, y);

            base.OnLoad(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == (Keys.Return))
            {
                Return(txtCommand.Text);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void Return(string cmd)
        {

            string command = txtCommand.Text;

            string message;

            if (Erc.ProcessCommand(command, out message))
            {
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Message");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
