﻿using System;
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
        public string login;
        public string passwd_hash;
        public bool active;
        public UInt64 user_code;

        public DBUser(string _login, string _passwd_hash, bool _active, UInt64 _user_code)
        {
            login = _login;
            passwd_hash = _passwd_hash;
            active = _active;
            user_code = _user_code;
        }
    }

    struct DBUnit //software
    {
        public UInt64 id_software;
        public string name;
        public UInt64 product_code;
        public bool active;
    }

    struct DBDevice //computer
    {
        public UInt64 id_computer;
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
                        login = reader["login"].ToString(),
                        passwd_hash = reader["passwd_hash"].ToString(),
                        user_code = Convert.ToUInt64(reader["code"]),
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

        public DBUser LoadUserData(string login)
        {
            string sql = "SELECT * FROM user WHERE " +
                "nickname = '" + login + "'";

            DBUser result = new DBUser(login, "", false, 0);

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

                result.passwd_hash = reader["passwd_hash"].ToString();
                result.user_code = Convert.ToUInt64(reader["code"]);
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

        public List<DBUnit> LoadUserLibrary(string login)
        {
            List<DBUnit> result = new List<DBUnit>();

            try
            {
                mtx.WaitOne();
                string sql = "SELECT * FROM user_soft, software" +
                " WHERE id_user = '" + login + "' AND id_soft = id_software";
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DBUnit
                    {
                        id_software = Convert.ToUInt64(reader["id_software"]),
                        name = reader["name"].ToString(),
                        product_code = Convert.ToUInt64(reader["code"]),
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

        public List<DBDevice> LoadUserDevices(string login)
        {
            List<DBDevice> result = new List<DBDevice>();

            string sql = "SELECT * FROM user_comp, computer" +
                " WHERE id_user = '" + login + "' AND " +
                "user_comp.id_computer=computer.id_computer";

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new DBDevice
                    {
                        id_computer = Convert.ToUInt64(reader["computer.id_computer"]),
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

        public bool SetUserActive(string login, bool state)
        {
            int istate;
            if (state)
                istate = 1;
            else
                istate = 0;

            string sql = "UPDATE user SET active = " + istate.ToString() +
                " WHERE login = '" + login + "'";
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

            string sql = "UPDATE computer SET active = " + istate.ToString() +
                " WHERE id_computer = " + device_id.ToString();

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

            string sql = "UPDATE software SET active = " + istate.ToString() +
                " WHERE id_software = " + id_unit.ToString();

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

        public bool CheckUserLibrary(string login)
        {
            string sql = "SELECT * FROM user_soft, software" +
                " WHERE id_user = '" + login +
                "' AND software.active = 1";

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

        public bool AuthentificateUser(string nickname, string pass_hash)
        {
            string sql = "SELECT * FROM user WHERE" +
                " login = '" + nickname + "' AND" +
                " passwd_hash = '" + pass_hash + "'";

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                bool result = reader.HasRows;
                reader.Close();
                mtx.ReleaseMutex();
                return result;
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return false;
            }
        }

        public bool RegisterNewUser(DBUser user)
        {
            //int active = 0;
            //if (user.active)
            //    active = 1;

            return false;

            //string sql = "INSERT INTO user SET" +
            //    " id_user = " + user.id_user.ToString() +
            //    ", nickname = '" + user.nickname +
            //    "', password_hash = '" + user.passwd_hash +
            //    "', user_code = " + user.user_code +
            //    ", active = " + active.ToString();

            //try
            //{
            //    mtx.WaitOne();
            //    MySqlCommand command = new MySqlCommand(sql, conn);
            //    command.ExecuteNonQuery();
            //    mtx.ReleaseMutex();
            //    return true;
            //}
            //catch (Exception exp)
            //{
            //    log.msg("Database error - " + exp.Message + " for query :" + sql);
            //    mtx.ReleaseMutex();
            //    return false;
            //}
        }

        public Int64 LoadDevice(string mb, string cpu, string gpu)
        {
            string sql = "SELECT id_computer FROM computer " +
                "WHERE motherboard = '" + mb + "' AND cpu = '" + cpu +
                "' AND gpu = '" + gpu + "'";

            Int64 result = -1;

            try
            {
                mtx.WaitOne();
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    result = Convert.ToInt64(reader["id_computer"]);
                reader.Close();
                mtx.ReleaseMutex();
            }
            catch (Exception exp)
            {
                log.msg("Database error - " + exp.Message + " for query :" + sql);
                mtx.ReleaseMutex();
                return -1;
            }

            return result;
        }

        public bool AddNewDevice(DBDevice device)
        {
            return false;
            //int active = 0;
            //if (device.active)
            //    active = 1;

            //string sql = "INSERT INTO devices SET" +
            //    " id_user = " + device.id_user +
            //    ", cpu = '" + device.cpu +
            //    "', motherboard = '" + device.motherboard +
            //    "', gpu = '" + device.gpu +
            //    "', active = " + active.ToString();

            //try
            //{
            //    mtx.WaitOne();
            //    MySqlCommand command = new MySqlCommand(sql, conn);
            //    command.ExecuteNonQuery();
            //    mtx.ReleaseMutex();
            //    return true;
            //}
            //catch (Exception exp)
            //{
            //    log.msg("Database error - " + exp.Message + " for query :" + sql);
            //    mtx.ReleaseMutex();
            //    return false;
            //}
        }

        public bool AddNewUnit(DBUnit unit)
        {
            return false;
            //int active = 0;
            //if (unit.active)
            //    active = 1;

            //string sql = "INSERT INTO library_unit SET" +
            //    " id_user = " + unit.id_user.ToString() +
            //    ", product = '" + unit.product +
            //    "', product_code = " + unit.product_code.ToString() +
            //    ", active = " + active.ToString();

            //try
            //{
            //    mtx.WaitOne();
            //    MySqlCommand command = new MySqlCommand(sql, conn);
            //    command.ExecuteNonQuery();
            //    mtx.ReleaseMutex();
            //    return true;
            //}
            //catch (Exception exp)
            //{
            //    log.msg("Database error - " + exp.Message + " for query :" + sql);
            //    mtx.ReleaseMutex();
            //    return false;
            //}
        }
    }
}
