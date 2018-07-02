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
    public partial class frmSetTimerDialog : Form
    {
        EscapeRoomController MainForm { get; set; }

        public frmSetTimerDialog()
        {
            InitializeComponent();
        }


        public frmSetTimerDialog(EscapeRoomController mainForm)
        {
            InitializeComponent();
            MainForm = mainForm;
        }

        protected override void OnLoad(EventArgs e)
        {
            int x = Screen.PrimaryScreen.WorkingArea.Left + 25;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - (this.Height + 25);
            this.Location = new Point(x, y);

            base.OnLoad(e);
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            TimeSpan time;
            if (!string.IsNullOrWhiteSpace(txtSetTimerTo.Text) && TimeSpan.TryParse("00:" + txtSetTimerTo.Text, out time))
            {
                MainForm.SetTimer(time);
 
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid time entered. Please enter a format of MM:SS", "Message");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == (Keys.Return))
            {
                btnApply_Click(this, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
