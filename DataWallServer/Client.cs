using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Text;

namespace DataWallServer
{
    class Client
    {
        public bool alive;

        public SslStream sslStream;
        private UInt64 id = 0;
        private Logger log;

        private Mutex mutex;
        private Thread clientThread;
        private DBActions db;
        private Random rnd = new Random();

        public Client(
            TcpClient client,
            ref Logger logger,
            X509Certificate serverCertificate,
            ref DBActions dbase)
        {
            mutex = new Mutex();
            sslStream = new SslStream(client.GetStream(), false);
            log = logger;
            db = dbase;

            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, 
                    clientCertificateRequired: false, 
                    checkCertificateRevocation: true);

                //sslStream.ReadTimeout = -1;
                //sslStream.WriteTimeout = 5000;

                log.msg("New client from " +
                    IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString())
                    + " connected at port " +
                    ((IPEndPoint)client.Client.RemoteEndPoint).Port);

                clientThread = new Thread(ClientHandler);
                clientThread.Start();
                alive = true;
            }
            catch (Exception exp)
            {
                log.msg("Failed to add new client with error: " + exp.Message);
                sslStream.Close();
                client.Close();
                return;
            }
        }

        private void ClientHandler()
        {
            byte[] data = GetMessage();
            if (data == null)
                return;

            if (data[0] == 230)
            {
                log.msg("User " + id.ToString() + "start auth");

                try
                {
                    int nick_size = Array.IndexOf(data, (byte)0x00) - 1;
                    int pass_size = 64;

                    string nick = Encoding.UTF8.GetString(data, 1, nick_size);
                    string pass_hash = Encoding.UTF8.GetString(data, nick_size + 2, pass_size);

                    byte[] message;
                    bool auth_success = db.AuthentificateUser(nick, pass_hash);
                    if (auth_success)
                    {
                        message = GenerateMessage(200, "OK");
                        log.msg("User " + id.ToString() + "auth success");
                    }
                    else
                    {
                        message = GenerateMessage(255, "Wrong login or password");
                        log.msg("User " + id.ToString() + "auth failed");
                    }

                    if(!SendMessage(message))
                        throw new Exception("Error send message");

                    if (!auth_success)
                        throw new Exception("User sended wrong authentification data");
                }
                catch (Exception exp)
                {
                    log.msg("Error at user " + id.ToString() + " " + exp.Message);
                    alive = false;
                    return;
                }
            }

            if (data[0] == 240) // registration
            {
                try
                {
                    int nick_size = Array.IndexOf(data, (byte)0x00) - 1;
                    int pass_size = Array.IndexOf(data, (byte)0x00, nick_size + 2) - nick_size - 2;

                    string nick = Encoding.UTF8.GetString(data, 1, nick_size);
                    string passwd = Encoding.UTF8.GetString(data, nick_size + 2, pass_size);

                    //string pas_hash = passwd.GetHashCode().ToString();

                    //if (!db.RegisterNewUser(user))
                    //    throw new Exception("DataBase Error");
                }
                catch(Exception exp)
                {
                    log.msg("Error at user " + id.ToString() + " " + exp.Message);
                    alive = false;
                    return;
                }


            }

            while (true)
            {
                data = GetMessage();
                if (data == null)
                    return;
            }
        }

        private byte[] GetMessage()
        {
            int code;
            byte[] size = new byte[4];

            try
            {
                code = sslStream.Read(size, 0, 4);
                if (code != 4)
                    throw new Exception("Wrong input message header");

                int buf_sz = BitConverter.ToInt32(size, 0);
                byte[] buffer = new byte[buf_sz];
                code = sslStream.Read(buffer, 0, buf_sz);
                if (code != buf_sz)
                    throw new Exception("Wrong input message");

                return buffer;
            }
            catch(Exception exp)
            {
                log.msg("Error at user " + id.ToString() + " " + exp.Message);
                alive = false;
                return null;
            }
        }

        private bool SendMessage(byte[] message)
        {
            try
            {
                int mes_size = message.Length;
                byte[] row_size = BitConverter.GetBytes(mes_size);

                sslStream.Write(row_size);
                sslStream.Write(message);
            }
            catch (Exception exp)
            {
                log.msg("Error at user " + id.ToString() + " " + exp.Message);
                alive = false;
                return false;
            }

            return true;
        }

        private byte[] GenerateMessage(byte code, string message)
        {
            byte[] row_message = Encoding.UTF8.GetBytes(message);
            byte[] full_message = new byte[3 + row_message.Length];
            full_message[0] = code;
            full_message[2 + row_message.Length] = 0;
            Array.Copy(row_message, 0, full_message, 2, row_message.Length);
            return full_message;
        }
    }
}
