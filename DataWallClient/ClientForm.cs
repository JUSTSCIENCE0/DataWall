using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace DataWallClient
{

    struct Unit
    {
        public UInt64 code;
        public string name;

        public Unit(UInt64 _code, string _name)
        {
            code = _code;
            name = _name;
        }

        public override string ToString()
        {
            return code.ToString() + " " + name;
        }
    }

    public partial class ClientForm : Form
    {
        private NamedPipeClientStream pipeClient;
        private List<Unit> lib_units = new List<Unit>();
        private string currentPath = "";
        private Unit currentUnit;
        private string login = "";

        private void SendMessage(string message)
        {
            if (pipeClient == null)
                return;

            byte[] row_message = Encoding.UTF8.GetBytes(message);
            pipeClient.Write(row_message);
            byte[] end = { 0 };
            pipeClient.Write(end);
            pipeClient.Flush();
            pipeClient.WaitForPipeDrain();
        }

        private void SendCode(byte code)
        {
            if (pipeClient == null)
                return;

            byte[] row_message = { 0, 0 };
            row_message[0] = code;
            pipeClient.Write(row_message);
            pipeClient.Flush();
            pipeClient.WaitForPipeDrain();
        }

        private byte RecvCode()
        {
            if (pipeClient == null)
                return 255;

            int result = pipeClient.ReadByte();
            int EoS = pipeClient.ReadByte();
            if (result == -1 || EoS != 0)
                return 255;

            return (byte)result;
        }

        private string RecvMessage()
        {
            if (pipeClient == null)
                return "";

            List<byte> row_message = new List<byte>();
            int result = -1;
            do
            {
                result = pipeClient.ReadByte();
                if (result == -1)
                    break;
                row_message.Add((byte)result);
            } while ((byte)result != 0);

            string message = Encoding.UTF8.GetString(row_message.ToArray());
            return message;
        }

        public ClientForm()
        {
            pipeClient = new NamedPipeClientStream("datawallpipe");

            try
            {
                pipeClient.Connect(5000);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Data Wall Service not found on this computer: " + exp.Message, "Error!");
                Environment.Exit(-1);
            }

            InitializeComponent();
        }

        private void Enter_Click(object sender, EventArgs e)
        {
            try
            {
                SendCode(230);
                SendMessage(Login.Text);
                SendMessage(password.Text);

                byte code = RecvCode();
                if (code == 200)
                {
                    MessageBox.Show("Auth successfull");
                    AuthPanel.Visible = false;
                    Workspace.Visible = true;

                    login = Login.Text;
                    RegistryKey dwKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DataWall\\" + login, true);
                    if (dwKey == null)
                    {
                        dwKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\DataWall\\" + login, true);
                    }
                    dwKey.Close();

                    SendCode(100);

                    byte answ_code = RecvCode();
                    if (answ_code == 200)
                    {
                        string str_lib_size = RecvMessage();
                        int lib_size = Convert.ToInt32(str_lib_size);
                        for (int i=0; i<lib_size; i++)
                        {
                            string unit_code = RecvMessage();
                            string unit_name = RecvMessage();
                            Unit current_unit = new Unit(
                                Convert.ToUInt64(unit_code),
                                unit_name);
                            lib_units.Add(current_unit);
                            LibraryList.Items.Add(current_unit);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Auth failed");
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error");
            }
        }

        private void LibraryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LibraryList.SelectedItem == null)
                return;

            Unit selectedUnit = (Unit)LibraryList.SelectedItem;
            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + selectedUnit.ToString(), true);
            if (soft_key == null)
            {
                soft_key = Registry.LocalMachine.CreateSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + selectedUnit.ToString(), true);
                soft_key.SetValue("Installed", "false");
                soft_key.SetValue("InstallPath", "");
            }

            bool Installed = Convert.ToBoolean(soft_key.GetValue("Installed"));
            string InstallPath = soft_key.GetValue("InstallPath").ToString();
            currentPath = InstallPath;

            if (!Installed)
            {
                Download.Enabled = true;
                Delete.Enabled = false;
                Repack.Enabled = false;
                Check.Enabled = false;
                Run.Enabled = false;
            }
            else
            {
                Download.Enabled = false;
                Delete.Enabled = true;
                Repack.Enabled = true;
                Check.Enabled = true;
                Run.Enabled = true;
            }

            currentUnit = selectedUnit;
        }

        private void Download_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (DialogResult.OK != folderBrowser.ShowDialog())
                return;

            SendCode(110);
            SendMessage(currentUnit.code.ToString());
            SendMessage(folderBrowser.SelectedPath + "\\" + currentUnit.name);

            Workspace.Visible = false;
            WaitPanel.Visible = true;

            byte result = RecvCode();

            Workspace.Visible = true;
            WaitPanel.Visible = false;

            if (result != 200)
            {
                MessageBox.Show("Error when load soft", "Error!");
                return;
            }

            MessageBox.Show("Downloaded!", "Success!");

            string installPath = (folderBrowser.SelectedPath + "\\" + currentUnit.name);

            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + currentUnit.ToString(), true);
            soft_key.SetValue("Installed", "true");
            soft_key.SetValue("InstallPath", installPath);

            Download.Enabled = false;
            Delete.Enabled = true;
            Repack.Enabled = true;
            Check.Enabled = true;
            Run.Enabled = true;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + currentUnit.ToString(), true);

            string InstallPath = soft_key.GetValue("InstallPath").ToString();
            byte[] row_path = Encoding.Unicode.GetBytes(InstallPath);
            InstallPath = Encoding.Unicode.GetString(row_path, 0, row_path.Length - 2);

            Directory.Delete(InstallPath, true);
            
            soft_key.SetValue("Installed", "false");
            soft_key.SetValue("InstallPath", "");

            Download.Enabled = true;
            Delete.Enabled = false;
            Repack.Enabled = false;
            Check.Enabled = false;
            Run.Enabled = false;
        }

        private void Run_Click(object sender, EventArgs e)
        {
            Unit selectedUnit = (Unit)LibraryList.SelectedItem;
            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + selectedUnit.ToString(), true);
            string InstallPath = soft_key.GetValue("InstallPath").ToString();
            byte[] row_path = Encoding.Unicode.GetBytes(InstallPath);
            InstallPath = Encoding.Unicode.GetString(row_path, 0, row_path.Length - 2);

            List<string> files = new List<string>(Directory.GetFiles(InstallPath));
            string binPath = files.Find(file => file.Contains(".exe"));

            //Hash check before launch

            SendCode(130);
            SendMessage(selectedUnit.code.ToString());
            SendMessage(InstallPath);

            byte result = RecvCode();

            if (result != 200)
                MessageBox.Show("Сheck failed! Start cancelled!", "Warning!");

            //Start
            SendCode(150);
            SendMessage(currentUnit.code.ToString());

            Process soft = new Process();
            ProcessStartInfo info = new ProcessStartInfo(binPath, InstallPath);
            soft.StartInfo = info;
            soft.Start();

            Workspace.Visible = false;
            WaitPanel.Visible = true;

            result = RecvCode();

            Workspace.Visible = true;
            WaitPanel.Visible = false;
        }

        private void Check_Click(object sender, EventArgs e)
        {
            Unit selectedUnit = (Unit)LibraryList.SelectedItem;
            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + selectedUnit.ToString(), true);
            string InstallPath = soft_key.GetValue("InstallPath").ToString();
            byte[] row_path = Encoding.Unicode.GetBytes(InstallPath);
            InstallPath = Encoding.Unicode.GetString(row_path, 0, row_path.Length - 2);

            SendCode(130);
            SendMessage(selectedUnit.code.ToString());
            SendMessage(InstallPath);

            byte result = RecvCode();

            if (result == 200)
                MessageBox.Show("Сheck passed!", "Success!");
            else
                MessageBox.Show("Сheck failed!", "Warning!");
        }

        private void Repack_Click(object sender, EventArgs e)
        {
            RegistryKey soft_key = Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\DataWall\\" + login + "\\" + currentUnit.ToString(), true);

            string InstallPath = soft_key.GetValue("InstallPath").ToString();
            byte[] row_path = Encoding.Unicode.GetBytes(InstallPath);
            InstallPath = Encoding.Unicode.GetString(row_path, 0, row_path.Length - 2);

            Directory.Delete(InstallPath, true);

            SendCode(110);
            SendMessage(currentUnit.code.ToString());
            SendMessage(InstallPath);

            Workspace.Visible = false;
            WaitPanel.Visible = true;

            byte result = RecvCode();

            Workspace.Visible = true;
            WaitPanel.Visible = false;

            if (result != 200)
            {
                MessageBox.Show("Error when load soft", "Error!");
                return;
            }

            MessageBox.Show("Repacked!", "Success!");
        }
    }
}
