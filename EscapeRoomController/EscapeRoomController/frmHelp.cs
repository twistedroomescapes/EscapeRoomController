using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace EscapeRoomController
{
    public partial class frmHelp : Form
    {
        public frmHelp(EscapeRoomController esc)
        {
            InitializeComponent();

            string file = Path.Combine(esc.GamesDirectory, "help.html");
            if (File.Exists(file))
            {

                string readText = File.ReadAllText(file);

                webBrowserHelp.DocumentText = file + "<br>" + readText;
            }
        }
    }
}
