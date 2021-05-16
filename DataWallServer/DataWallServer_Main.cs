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
            server = new Server(22876, ref log, "D:\\DataWall\\DataWall.cer", ref db);

            DrawActivitiesTable(ActivityType.ALL_USERS);
            DrawUsersList();
        }

        private void DrawActivitiesTable(ActivityType type)
        {
            List<DBUser> users = db.LoadAllUsersData(type);
            ActivitiesGrid.Rows.Clear();
            foreach (DBUser user in users)
            {
                ActivitiesGrid.Rows.Add(user.login, user.passwd_hash, user.active, user.user_code);
            }
        }

        private void DrawUsersList()
        {
            List<DBUser> users = db.LoadAllUsersData(ActivityType.ALL_USERS);
            UsersList.Items.Clear();
            foreach (DBUser user in users)
            {
                UsersList.Items.Add(user.login);
            }
        }

        private void Redriver_Tick(object sender, EventArgs e)
        {
            ConsoleText.Text += log.print();

            if (ShowAll.Checked)
                DrawActivitiesTable(ActivityType.ALL_USERS);
            else if (ShowActive.Checked)
                DrawActivitiesTable(ActivityType.ACTIVE_ONLY);
            else if (ShowInactive.Checked)
                DrawActivitiesTable(ActivityType.INACTIVE_ONLY);

            if (UsersList.SelectedItem != null)
            {
                string login = UsersList.SelectedItem.ToString();
                List<DBDevice> comps = db.LoadUserDevices(login);
                List<DBUnit> soft = db.LoadUserLibrary(login);

                SoftDataGrid.Rows.Clear();
                DeviceDataGrid.Rows.Clear();

                foreach (DBDevice computer in comps)
                {
                    DeviceDataGrid.Rows.Add(
                        computer.id_computer,
                        computer.active,
                        computer.cpu,
                        computer.motherboard,
                        computer.gpu);
                }

                foreach (DBUnit software in soft)
                {
                    SoftDataGrid.Rows.Add(
                        software.id_software,
                        software.name,
                        software.product_code);
                }
            }
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

        private void ShowInactive_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowInactive.Checked)
                DrawActivitiesTable(ActivityType.INACTIVE_ONLY);
        }

        private void ShowAll_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowAll.Checked)
                DrawActivitiesTable(ActivityType.ALL_USERS);
        }

        private void ShowActive_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowActive.Checked)
                DrawActivitiesTable(ActivityType.ACTIVE_ONLY);
        }

        private void UsersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string login = UsersList.SelectedItem.ToString();
            List<DBDevice> comps = db.LoadUserDevices(login);
            List<DBUnit> soft = db.LoadUserLibrary(login);

            SoftDataGrid.Rows.Clear();
            DeviceDataGrid.Rows.Clear();

            foreach (DBDevice computer in comps)
            {
                DeviceDataGrid.Rows.Add(
                    computer.id_computer,
                    computer.active,
                    computer.cpu,
                    computer.motherboard,
                    computer.gpu);
            }

            foreach (DBUnit software in soft)
            {
                SoftDataGrid.Rows.Add(
                    software.id_software,
                    software.name,
                    software.product_code);
            }
        }
    }
}
