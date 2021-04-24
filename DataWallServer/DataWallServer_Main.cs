using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace DataWallServer
{
    public partial class DataWallServer_Main : Form
    {
        private Logger log;
        private Server server;
        private DBActions db;

        public DataWallServer_Main()
        {
            InitializeComponent();

            log = new Logger();
            db = new DBActions(ref log);
            db.InitConnection("127.0.0.1",
                "datawallinfo", "3306", "root",
                "192837465564738291yashka");
            server = new Server(50001, ref log, "D:\\DataWall\\DataWall.cer", ref db);

            //servThrd = new Thread(server.ClientsHandler);
            //servThrd.Start();
        }

        private void Redriver_Tick(object sender, EventArgs e)
        {
            ConsoleText.Text += log.print();
        }

        private void DataWallServer_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("When the window is closed, the server will be stopped!" +
                " Proceed?", "Attention!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void DataWallServer_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.StopServer();
        }

        private void ActivitiesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
