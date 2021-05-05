using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace DataWallServer
{
    class Client
    {
        public bool alive;

        public SslStream sslStream;
        private string id = "0";
        private Logger log;

        private Mutex mutex;
        private Thread clientThread;
        private DBActions db;
        private Random rnd = new Random();

        private bool authenticated = false;
        private Int64 device = -1;

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

                id = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + ":" +
                    ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();

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

        private void SetInactive()
        {
            if (authenticated)
            {
                db.SetUserActive(id, false);
            }
            if (device != -1)
            {
                db.SetDeviceActive(device, false);
            }
        }

        private void ClientHandler()
        {
            while (alive)
            {
                byte[] data = GetMessage();
                if (data == null)
                    return;

                if (data[0] == 230) //authication
                {
                    log.msg("User '" + id + "' start auth");

                    try
                    {
                        int nick_size = Array.IndexOf(data, (byte)0x00) - 1;
                        int pass_size = 64;

                        string nick = Encoding.UTF8.GetString(data, 1, nick_size);
                        string pass_hash = Encoding.UTF8.GetString(data, nick_size + 2, pass_size);

                        byte[] message;
                        authenticated = db.AuthentificateUser(nick, pass_hash);
                        if (authenticated)
                        {
                            message = GenerateMessage(200, "OK");
                            id = nick;
                            db.SetUserActive(id, true);
                            log.msg("User '" + id + "' auth success");
                        }
                        else
                        {
                            message = GenerateMessage(255, "Wrong login or password");
                            log.msg("User '" + id + "' auth failed");
                        }

                        if (!SendMessage(message))
                            throw new Exception("Error send message");

                        continue;
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        alive = false;
                        SetInactive();
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

                        continue;
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        alive = false;
                        return;
                    }
                }

                if (data[0] == 250) // recieved PC config
                {
                    if (!authenticated)
                        continue;

                    log.msg("User '" + id + "' sended PC config");

                    try
                    {
                        int mb_size = Array.IndexOf(data, (byte)0x00) - 1;
                        int cpu_size = Array.IndexOf(data, (byte)0x00, mb_size + 2) -
                            mb_size - 2;
                        int gpu_size = Array.IndexOf(data, (byte)0x00,
                            mb_size + cpu_size + 3) - mb_size - cpu_size - 3;

                        string mb = Encoding.UTF8.GetString(data, 1, mb_size);
                        string cpu = Encoding.UTF8.GetString(data, mb_size + 2, cpu_size);
                        string gpu = Encoding.UTF8.GetString(data, mb_size + cpu_size + 3, gpu_size);

                        log.msg("User '" + id + "' sended mb info: " + mb);
                        log.msg("User '" + id + "' sended cpu info: " + cpu);
                        log.msg("User '" + id + "' sended gpu info: " + gpu);

                        device = db.LoadDevice(mb, cpu, gpu);
                        if (device == -1)
                        {
                            DBDevice dev_config = new DBDevice();
                            dev_config.active = true;
                            dev_config.cpu = cpu;
                            dev_config.motherboard = mb;
                            dev_config.gpu = gpu;

                            if (!db.AddNewDevice(dev_config))
                                throw new Exception("Failed to add the device to the database");

                            device = db.LoadDevice(mb, cpu, gpu);
                            if (device == -1)
                                throw new Exception("Failed to load device from database");
                        }

                        if(!db.CheckDeviceUser(device, id))
                        {
                            if (!db.CorrelateUserDevice(device, id))
                                throw new Exception("Failed to correlate device and user");
                        }

                        if (!db.SetDeviceActive(device, true))
                            throw new Exception("Failed to activate device");

                        continue;
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        alive = false;
                        SetInactive();
                        return;
                    }
                }

                if (data[0] == 100)
                {
                    if (!authenticated)
                    {
                        try
                        {
                            byte[] err_mes = GenerateMessage(255,
                                "User don't authenticated");
                            if (!SendMessage(err_mes))
                                throw new Exception("Error send message");

                            continue;
                        }
                        catch (Exception exp)
                        {
                            log.msg("Error at user '" + id + "' " + exp.Message);
                            alive = false;
                            SetInactive();
                            return;
                        }
                    }

                    List<DBUnit> result = db.LoadUserLibrary(id);


                }
            }
        }

        private byte[] GetMessage()
        {
            int code;
            byte[] size = new byte[4];

            try
            {
                code = sslStream.Read(size, 0, 4);
                if (code == 0)
                    throw new Exception("User disconnected");
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
                log.msg("Error at user '" + id + "' " + exp.Message);
                alive = false;
                SetInactive();
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
                log.msg("Error at user '" + id + "' " + exp.Message);
                alive = false;
                SetInactive();
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
