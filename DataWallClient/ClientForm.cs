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
    public partial class ClientForm : Form
    {
        private NamedPipeClientStream pipeClient;

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
            if (result == -1)
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
            SendCode(230);
            SendMessage(Login.Text);
            SendMessage(password.Text);

            byte code = RecvCode();
            if (code == 200)
            {
                MessageBox.Show("Auth successfull");
                AuthPanel.Visible = false;
                Workspace.Visible = true;
            }
            else
            {
                MessageBox.Show("Auth failed");
                this.Close();
            }
        }
    }
}
