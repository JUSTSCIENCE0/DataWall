using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading;

namespace DataWallServer
{
    enum ActivityType
    {
        ALL_USERS,
        ACTIVE_ONLY,
        INACTIVE_ONLY
    }

    struct DBUser //user
    {
        public UInt64 id_user;
        public string nickname;
        public string passwd_hash;
        public UInt64 user_code;
        public bool active;
    }

    struct DBUnit //library_unit
    {
        public UInt64 id_unit;
        public UInt64 id_user;
        public string product;
        public UInt64 product_code;
        public bool active;
    }

    struct DBDevice //devices
    {
        public UInt64 device_id;
        public UInt64 id_user;
        public string cpu;
        public string gpu;
        public string motherboard;
        public bool active;
    }

    class DBActions
    {
        private MySqlConnection conn = null;
        private Logger log;
        private Mutex mtx;

        public DBActions(ref Logger logger)
        {
            log = logger;
            mtx = new Mutex();
        }

        public bool InitConnection(
            string host, 
            string db, 
            string port, 
            string username, 
            string password)
        {
            String connString = "Server=" + host + ";Database=" + db
               + ";port=" + port + ";User Id=" + username + ";password=" + password;
            conn = new MySqlConnection(connString);
            log.msg("Connect with server " + host + ":" + port + 
                " database - " + db);

            try
            {
                mtx.WaitOne();
                conn.Open();
                log.msg("Connection with database successful!");
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Connection error - " + exp.Message);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                mtx.WaitOne();
                conn.Close();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Не удалось отключиться от текущего соединения: " + exp.Message);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public List<DBUser> LoadAllUsersData(ActivityType type)
        {
            List<DBUser> result = new List<DBUser>();

            string sql = "SELECT * FROM user";
            if (type == ActivityType.ACTIVE_ONLY)
                sql += " WHERE active = 1";
            if (type == ActivityType.INACTIVE_ONLY)
                sql += " WHERE active = 0";

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DBUser
                    {
                        id_user = Convert.ToUInt64(reader["id_user"]),
                        nickname = reader["nickname"].ToString(),
                        passwd_hash = reader["passwd_hash"].ToString(),
                        user_code = Convert.ToUInt64(reader["user_code"]),
                        active = Convert.ToBoolean(reader["active"])
                    });
                }
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return result;
            }

            return result;
        }

        public DBUser LoadUserData(UInt64 id_user, string nickname)
        {
            string sql = "SELECT * FROM user WHERE id_user = " + id_user.ToString() +
                " AND nickname = '" + nickname + "'";

            DBUser result = new DBUser();
            result.id_user = 0;

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    return result;
                }

                result.id_user = id_user;
                result.nickname = nickname;
                result.passwd_hash = reader["passwd_hash"].ToString();
                result.user_code = Convert.ToUInt64(reader["user_code"]);
                result.active = Convert.ToBoolean(reader["active"]);

                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return result;
            }

            return result;
        }

        public List<DBUnit> LoadUserLibrary(UInt64 id_user)
        {
            List<DBUnit> result = new List<DBUnit>();

            try
            {
                mtx.WaitOne();
                string sql = "SELECT * FROM library_unit" +
                " WHERE id_user = " + id_user.ToString();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DBUnit
                    {
                        id_unit = Convert.ToUInt64(reader["id_unit"]),
                        product = reader["product"].ToString(),
                        product_code = Convert.ToUInt64(reader["product_code"]),
                        active = Convert.ToBoolean(reader["active"])
                    });
                }
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message);
                mtx.ReleaseMutex();
                return result;
            }

            return result;
        }

        public List<DBDevice> LoadUserDevices(UInt64 id_user)
        {
            List<DBDevice> result = new List<DBDevice>();

            string sql = "SELECT * FROM devices" +
                " WHERE id_user = " + id_user.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DBDevice
                    {
                        device_id = Convert.ToUInt64(reader["device_id"]),
                        cpu = reader["cpu"].ToString(),
                        motherboard = reader["motherboard"].ToString(),
                        gpu = reader["gpu"].ToString(),
                        active = Convert.ToBoolean(reader["active"])
                    });
                }
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return result;
            }

            return result;
        }

        public bool SetUserActive(UInt64 id_user, bool state)
        {
            int istate;
            if (state)
                istate = 1;
            else
                istate = 0;

            string sql = "UPDATE user SET active = " + istate.ToString() +
                " WHERE id_user = " + id_user.ToString();
            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool SetDeviceActive(UInt64 device_id, bool state)
        {
            int istate;
            if (state)
                istate = 1;
            else
                istate = 0;

            string sql = "UPDATE devices SET active = " + istate.ToString() +
                " WHERE device_id = " + device_id.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool SetUnitActive(UInt64 id_unit, bool state)
        {
            int istate;
            if (state)
                istate = 1;
            else
                istate = 0;

            string sql = "UPDATE library_unit SET active = " + istate.ToString() +
                " WHERE id_unit = " + id_unit.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool CheckUserLibrary(UInt64 id_user)
        {
            string sql = "SELECT * FROM user, library_unit" +
                " WHERE id_user = " + id_user.ToString() +
                " AND library_unit.active = 1";

            bool result = false;

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                result = reader.Read();
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }

            return result;
        }

        public UInt64 GetMaxUserID()
        {
            string sql = "SELECT max(id_user) as mx FROM user";

            UInt64 result = 0;

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    result = Convert.ToUInt64(reader["mx"]);
                }
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return result;
            }

            return result;
        }

        public bool RegisterNewUser(DBUser user)
        {
            int active = 0;
            if (user.active)
                active = 1;

            string sql = "INSERT INTO user SET" +
                " id_user = " + user.id_user.ToString() +
                ", nickname = '" + user.nickname +
                "', password_hash = '" + user.passwd_hash +
                "', user_code = " + user.user_code +
                ", active = " + active.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool AddNewDevice(DBDevice device)
        {
            int active = 0;
            if (device.active)
                active = 1;

            string sql = "INSERT INTO devices SET" +
                " id_user = " + device.id_user +
                ", cpu = '" + device.cpu +
                "', motherboard = '" + device.motherboard +
                "', gpu = '" + device.gpu +
                "', active = " + active.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool AddNewUnit(DBUnit unit)
        {
            int active = 0;
            if (unit.active)
                active = 1;

            string sql = "INSERT INTO library_unit SET" +
                " id_user = " + unit.id_user.ToString() +
                ", product = '" + unit.product +
                "', product_code = " + unit.product_code.ToString() +
                ", active = " + active.ToString();

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
                mtx.ReleaseMutex();
                return true;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }
    }
}
