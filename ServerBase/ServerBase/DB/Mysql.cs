using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ServerBase
{
    public class Mysql
    {
        private string _strConn;

        public Mysql()
        {
            string host = ConfigManager.Instance.GetConfigString("TestDB", "HOST", "localhost");
            int port = ConfigManager.Instance.GetConfigInt("TestDB", "Port", 3306);
            string id = ConfigManager.Instance.GetConfigString("TestDB", "ID");
            string pw = ConfigManager.Instance.GetConfigString("TestDB", "PW");
            string dbName = ConfigManager.Instance.GetConfigString("TestDB", "Catalog");

            _strConn = $"Server={host};Port={port};Database={dbName};Uid={id};Pwd={pw};";
        }

        public void TestInsert()
        {
            using (MySqlConnection conn = new MySqlConnection(_strConn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO test VALUES (2, 1)", conn);
                cmd.ExecuteNonQuery();
            }
        }

        public void TestSelectUsingReader()
        {
            using (MySqlConnection conn = new MySqlConnection(_strConn))
            {
                conn.Open();
                string sql = "SELECT * FROM test WHERE id = 1";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Log.log.Debug($"{rdr["id"]}: {rdr["value"]}");
                }
                rdr.Close();
            }
        }

    }
}
