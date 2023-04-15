using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Data.SqlClient;

namespace DLL
{
    internal class Connexion
    {
        static IDbConnection con = null;
        static IDbCommand cmd = null;
        public static IDbConnection connect(string connexionString, string dbt = "MySql")
        {
            if (dbt == "MySql")
            {
                if (con == null)
                {
                    con = new MySqlConnection(connexionString);
                    cmd = new MySqlCommand();
                }
                if (con.State.ToString() == "Closed")
                {
                    con.Open();
                    cmd.Connection = (MySqlConnection)con;
                }
                return con;
            }
            else if (dbt == "SqlServer")
            {
                if (con == null)
                {
                    con = new SqlConnection(connexionString);
                    cmd = new SqlCommand();
                }
                if (con.State.ToString() == "Closed")
                {
                    con.Open();
                    cmd.Connection = (SqlConnection)con;
                }
                return con;
            }

            if (connexionString is null)
            {
                throw new ArgumentNullException(nameof(connexionString));
            }

            return con;
        }



        public static int IUD(string req)
        {
            cmd.CommandText = req;
            return cmd.ExecuteNonQuery();
        }

        public static IDataReader select(string req)
        {
            cmd.CommandText = req;
            return cmd.ExecuteReader();
        }
        public static Dictionary<string, string> getChamps_table(string table)
        {
            Dictionary<string, string> champs = new Dictionary<string, string>();
            string sql = "desc " + table;
            cmd.CommandText = sql;
            IDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                switch (dr.GetString(1))
                {
                    case "varchar(255)":
                        champs.Add(dr.GetString(0), "string");
                        break;

                    default:
                        champs.Add(dr.GetString(0), dr.GetString(1));
                        break;
                }
            }
            return champs;
        }
    }
}
