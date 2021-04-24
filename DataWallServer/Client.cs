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

            if (data[0] == 240) // registration
            {
                try
                {
                    int nick_size = Array.IndexOf(data, (byte)0x00) - 1;
                    int pass_size = Array.IndexOf(data, (byte)0x00, nick_size + 2) - nick_size - 2;

                    string nick = Encoding.UTF8.GetString(data, 1, nick_size);
                    string passwd = Encoding.UTF8.GetString(data, nick_size + 2, pass_size);


                    string pas_hash = passwd.GetHashCode().ToString();

                    id = db.GetMaxUserID() + 1;
                    byte[] rnd_bytes = new byte[8];
                    rnd.NextBytes(rnd_bytes);
                    UInt64 rnd_code = BitConverter.ToUInt64(rnd_bytes, 0);

                    DBUser user = new DBUser {
                        id_user = id,
                        nickname = nick,
                        passwd_hash = pas_hash,
                        user_code = rnd_code,
                        active = true
                    };

                    if (!db.RegisterNewUser(user))
                        throw new Exception("DataBase Error");
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
    }
}
