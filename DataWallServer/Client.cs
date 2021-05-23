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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DataWallServer
{
    enum ContentTypes
    {
        DLL = 0x4C4C44,
        IMG = 0x474D49,
        TXT = 0x545854,
        SND = 0x444E53,
        BIN = 0x4E4942,
        NON = 0x0
    }

    struct DWFileInfo
    {
        public string name;
        public ContentTypes type;

        public DWFileInfo(string _name, ContentTypes _type)
        {
            name = _name;
            type = _type;
        }
    }

    class DataWallEngine
    {
        [DllImport("DataWallEngine.dll", EntryPoint = "PackInContainer")]
        public static extern int PackInContainer(
            byte[] data, 
            int size, 
            int type, 
            byte[] key, 
            byte[] container_name);
    }

    class Client
    {
        public bool alive;

        public SslStream sslStream;
        private string id = "0";
        private string user_code = "0";
        private string user_pc = "";
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

                alive = true;

                clientThread = new Thread(ClientHandler);
                clientThread.Start();
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
                            id = nick;
                            DBUser userInfo = db.LoadUserData(id);
                            message = GenerateMessage(200, userInfo.user_code.ToString());
                            user_code = userInfo.user_code.ToString();
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

                        user_pc = cpu + " " + mb + " " + gpu;

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

                if (data[0] == 100) //Get library
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

                    try
                    {
                        List<DBUnit> result = db.LoadUserLibrary(id);

                        int answ_size = 5;
                        int offset = 5;
                        foreach (DBUnit unit in result)
                        {
                            answ_size += (17 + Encoding.UTF8.GetBytes(unit.name).Length);
                        }

                        byte[] answer = new byte[answ_size];

                        answer[0] = 200;
                        byte[] lib_size = BitConverter.GetBytes(result.Count);
                        Array.Copy(lib_size, 0, answer, 1, 4);

                        foreach (DBUnit unit in result)
                        {
                            byte[] unit_id = BitConverter.GetBytes(unit.id_software);
                            Array.Copy(unit_id, 0, answer, offset, 8);
                            offset += 8;
                            byte[] unit_code = BitConverter.GetBytes(unit.product_code);
                            Array.Copy(unit_code, 0, answer, offset, 8);
                            offset += 8;
                            byte[] unit_name = Encoding.UTF8.GetBytes(unit.name);
                            Array.Copy(unit_name, 0, answer, offset, unit_name.Length);
                            offset += unit_name.Length;
                            answer[offset] = 0;
                            offset += 1;
                        }

                        if (!SendMessage(answer))
                            throw new Exception("Error when send library");
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        alive = false;
                        SetInactive();
                        return;
                    }
                }

                if (data[0] == 110) //Install soft
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

                    try
                    {
                        byte[] id_soft = GetMessage();
                        string id_software = Encoding.UTF8.GetString(id_soft);

                        DBUnit soft = db.LoadUnitInfo(Convert.ToUInt64(id_software));
                        log.msg("User '" + id + "' want software " + soft.name);

                        string key_info = user_pc + " " + 
                            user_code + " " + soft.product_code.ToString();
                        byte[] row_key_info = Encoding.UTF8.GetBytes(key_info);
                        SHA256 sha = SHA256.Create();
                        byte[] key = sha.ComputeHash(row_key_info);

                        string config = "D:\\DataWall\\" +
                            id_software + " " + soft.name + "\\files.info";
                        StreamReader conf_reader = new StreamReader(config);
                        List<byte[]> config_list = new List<byte[]>();
                        byte[] b200 = { 200 };
                        config_list.Add(b200);
                        List<DWFileInfo> files = new List<DWFileInfo>();
                        while (!conf_reader.EndOfStream)
                        {
                            byte[] btype = { 0 };
                            byte[] bnull = { 0 };
                            byte[] bcontent = { 0, 0, 0, 0 };

                            string fname = conf_reader.ReadLine();
                            string type = conf_reader.ReadLine();
                            string content_code = "";
                            ContentTypes DW_type = ContentTypes.NON;
                            if (type == "1")
                            {
                                btype[0] = 1;
                                content_code = conf_reader.ReadLine();
                                switch (content_code)
                                {
                                    case "DLL":
                                        DW_type = ContentTypes.DLL;
                                        bcontent = BitConverter.GetBytes((Int32)ContentTypes.DLL);
                                        break;
                                    case "IMG":
                                        DW_type = ContentTypes.IMG;
                                        bcontent = BitConverter.GetBytes((Int32)ContentTypes.IMG);
                                        break;
                                    case "TXT":
                                        DW_type = ContentTypes.TXT;
                                        bcontent = BitConverter.GetBytes((Int32)ContentTypes.TXT);
                                        break;
                                    case "SND":
                                        DW_type = ContentTypes.SND;
                                        bcontent = BitConverter.GetBytes((Int32)ContentTypes.SND);
                                        break;
                                    case "BIN":
                                        DW_type = ContentTypes.BIN;
                                        bcontent = BitConverter.GetBytes((Int32)ContentTypes.BIN);
                                        break;
                                }
                            }

                            config_list.Add(btype);
                            config_list.Add(bcontent);
                            config_list.Add(Encoding.UTF8.GetBytes(fname));
                            config_list.Add(bnull);

                            files.Add(new DWFileInfo("D:\\DataWall\\" +
                            id_software + " " + soft.name + "\\" + fname, DW_type));
                        }

                        byte[] eof = { 255 };
                        config_list.Add(eof);

                        byte[][] tmp = config_list.ToArray();

                        byte[] answer = tmp.SelectMany(x=>x).ToArray();
                        SendMessage(answer);

                        foreach (DWFileInfo file in files)
                        {
                            BinaryReader file_reader = new BinaryReader(File.OpenRead(file.name));
                            FileInfo info = new FileInfo(file.name);
                            byte[] file_data = file_reader.ReadBytes((int)info.Length);
                            file_reader.Close();
                            if (file.type == ContentTypes.NON)
                                SendMessage(file_data);
                            else
                            {
                                string tmp_name = "D:\\DataWall\\temp\\" + 
                                    user_code + "_" + file.name.GetHashCode() + ".pak";
                                DataWallEngine.PackInContainer(
                                    file_data,
                                    (int)info.Length,
                                    (int)file.type,
                                    key,
                                    Encoding.UTF8.GetBytes(tmp_name));
                                BinaryReader encr_reader = new BinaryReader(File.OpenRead(tmp_name));
                                FileInfo encr_info = new FileInfo(tmp_name);
                                byte[] encr_data = encr_reader.ReadBytes((int)encr_info.Length);
                                encr_reader.Close();
                                SendMessage(encr_data);
                                File.Delete(tmp_name);
                            }
                        }

                        byte[] row_hash = GetMessage();
                        string soft_hash = Encoding.UTF8.GetString(row_hash);
                        log.msg("Soft hash: " + soft_hash);

                        if (!db.WriteNewSoftHash(id, id_software, soft_hash))
                            throw new Exception("Coudn't write soft hash in db");
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        byte[] err_mes = GenerateMessage(255,
                                "Unexpected error");
                        SendMessage(err_mes);
                        alive = false;
                        SetInactive();
                        return;
                    }
                }

                if (data[0] == 130) //check hash
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

                    try
                    {
                        byte[] id_soft = GetMessage();
                        string id_software = Encoding.UTF8.GetString(id_soft);

                        byte[] row_hash = GetMessage();
                        string hash = Encoding.UTF8.GetString(row_hash);

                        byte[] message;
                        if (db.CheckSoftHash(id, id_software, hash))
                        {
                            message = GenerateMessage(200, "OK");
                            log.msg("User '" + id + "' check hash passed");
                        }
                        else
                        {
                            message = GenerateMessage(255, "Wrong hash");
                            log.msg("User '" + id + "' check hash failed");
                        }

                        if (!SendMessage(message))
                            throw new Exception("Error send message");
                    }
                    catch (Exception exp)
                    {
                        log.msg("Error at user '" + id + "' " + exp.Message);
                        byte[] err_mes = GenerateMessage(255,
                                "Unexpected error");
                        SendMessage(err_mes);
                        alive = false;
                        SetInactive();
                        return;
                    }
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
