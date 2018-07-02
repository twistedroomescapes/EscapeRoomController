using System;
using System.Windows.Forms;
using System.Drawing;

namespace EscapeRoomController
{
    public partial class fmrSetClueDialog : Form
    {

        private EscapeRoomController Erc;

        public fmrSetClueDialog(EscapeRoomController erc)
        {
            Erc = erc;

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            int x = Screen.PrimaryScreen.WorkingArea.Left + 25;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - (this.Height + 25);
            this.Location = new Point(x, y);

            base.OnLoad(e);
        }

        private void btnSetClueCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
        private void btnSetClueSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSetClue.Text))
            {
                Erc.WebBrowser.DocumentText = BuildHTMLMessage(txtSetClue.Text);
                // Play sound if message was sent
                Erc.clueMenuPlayItem_Click(sender, e, null, Erc.CurrentGameDirectory);
            }
            this.Close();
        }

        public string BuildHTMLMessage(string message)
        {
            string msg = @"<html><style type='text/css'>div {font-size:24pt;font-family:arial;color:#000000;} </style><body><div>#msg#</div></body></html>";
            msg = msg.Replace("#msg#", message);
            return msg;
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == (Keys.Return))
            {
                btnSetClueSend_Click(this, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
