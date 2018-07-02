using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JJSCommon;

namespace EscapeRoomController
{
    public partial class frmSetup : Form
    {

        public string GetGameDirectory()
        {
            return txtGameDirectory.Text;
        }

        public string GetRemoteMachine()
        {
            return txtRemoteMachine.Text;
        }

        public frmSetup()
        {
            InitializeComponent();
        }

        private void frmSetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            RegistryHelper.SetRegistryFromTextBox(txtGameDirectory);
            RegistryHelper.SetRegistryFromTextBox(txtRemoteMachine);
        }

        private void frmSetup_Load(object sender, EventArgs e)
        {

            var gamesDirectory = ConfigurationManager.AppSettings["GamesDirectory"];
            var remoteMachine = ConfigurationManager.AppSettings["RoomMachineName"];

            RegistryHelper.SetTextBoxFromRegistry(txtGameDirectory, gamesDirectory);
            RegistryHelper.SetTextBoxFromRegistry(txtRemoteMachine, remoteMachine);

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserGameDirectory.RootFolder = Environment.SpecialFolder.Personal;

            DialogResult result = folderBrowserGameDirectory.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(folderBrowserGameDirectory.SelectedPath))
                {
                    txtGameDirectory.Text = folderBrowserGameDirectory.SelectedPath;
                    RegistryHelper.SetRegistryFromTextBox(txtGameDirectory);
                }
            }
        }
    }
}
