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
        private Thread handle, clients_cleaner;
        private DBActions db;

        private bool server_alive = true;
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
            clients_cleaner = new Thread(CleanDead);
            handle.Start();
            clients_cleaner.Start();
        }

        private void ClientsHandler()
        {
            bool mtx = false;
            try
            {
                while (server_alive)
                {
                    TcpClient newClient = Listener.AcceptTcpClient();
                    mutex.WaitOne();
                    mtx = true;
                    clients.Add(new Client(newClient,
                        ref log,
                        serverCertificate,
                        ref db));
                    mutex.ReleaseMutex();
                    mtx = false;
                }
            }
            catch (Exception)
            {
                log.msg("Error, when accept new client");
                if (mtx) mutex.ReleaseMutex();
            }
        }

        private void CleanDead()
        {
            while(server_alive)
            {
                mutex.WaitOne();
                clients.RemoveAll(client => !client.alive);
                mutex.ReleaseMutex();

                Thread.Sleep(1000);
            }
        }

        public void StopServer()
        {
            server_alive = false;
            if (Listener != null)
            {
                Listener.Stop();
            }

            handle.Join();
            clients_cleaner.Join();

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
