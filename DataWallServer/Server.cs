using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Collections.Generic;

namespace DataWallServer
{
    class Server
    {
        private TcpListener Listener;
        private X509Certificate serverCertificate = null;
        private ushort port;
        private Logger log;

        private Mutex mutex = new Mutex();
        private Thread handle;
        private DBActions db;

        //private MessageParser parser = new MessageParser();

        //public List<Client> unconfirmed;
        public List<Client> clients;

        public Server(ushort Port, 
            ref Logger logger, 
            string certFile,
            ref DBActions dbase)
        {
            port = Port;
            log = logger;
            db = dbase;
            clients = new List<Client>();
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();
            log.msg("Server started at port " + port.ToString());
            serverCertificate = X509Certificate.CreateFromCertFile(certFile);

            handle = new Thread(ClientsHandler);
            handle.Start();
        }

        private void ClientsHandler()
        {
            try
            {
                while (true)
                {
                    TcpClient newClient = Listener.AcceptTcpClient();
                    clients.Add(new Client(newClient,
                        ref log,
                        serverCertificate,
                        ref db));
                }
            }
            catch (Exception)
            {
            }
        }

        public void StopServer()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
            foreach(Client client in clients)
            {
                client.sslStream.Close();
            }
            log.msg("Server stopped");
        }

        ~Server()
        {
            //clients.Clear();
        }
    }
}
