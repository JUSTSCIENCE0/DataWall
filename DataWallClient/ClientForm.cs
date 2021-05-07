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
    }
}
