using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.Data.Common;
using MySqlConnector;

namespace DLL
{
    public partial class Model
    {
        public int saveProcedureMySql(string procedure)
        {
            try
            {
                DbCommand cmd = new MySqlCommand(procedure, (MySqlConnection)con);
                cmd.CommandType = CommandType.StoredProcedure;
                if (this.Id == 0)
                {

                    PropertyInfo[] properties = (this).GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        string name = property.Name.ToLower();
                        Console.WriteLine(name);
                        if (name == "id")
                            break;
                        MySqlParameter p1 = new MySqlParameter(property.Name, property.GetValue(this));
                        cmd.Parameters.Add(p1);
                    }
                }
                else
                {
                    PropertyInfo[] properties = (this).GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        string name = property.Name.ToLower();
                        Console.WriteLine(name);
                        MySqlParameter p1 = new MySqlParameter(property.Name, property.GetValue(this));
                        cmd.Parameters.Add(p1);
                    }
                }
                return cmd.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.ToString());
                return this.save();
            }
        }

        public int saveProcedureSql(string procedure)
        {
            try
            {
                DbCommand cmd = new SqlCommand(procedure, (SqlConnection)con);
                cmd.CommandType = CommandType.StoredProcedure;
                if (this.Id == 0)
                {

                    PropertyInfo[] properties = (this).GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        string name = property.Name.ToLower();
                        Console.WriteLine(name);
                        if (name == "id")
                            break;
                        SqlParameter p1 = new SqlParameter(property.Name, property.GetValue(this));
                        cmd.Parameters.Add(p1);
                    }
                }
                else
                {
                    PropertyInfo[] properties = (this).GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        string name = property.Name.ToLower();
                        Console.WriteLine(name);
                        SqlParameter p1 = new SqlParameter(property.Name, property.GetValue(this));
                        cmd.Parameters.Add(p1);
                    }
                }
                return cmd.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.ToString());
                return this.save();
            }
        }

        private int deleteProcedure(string procedure)
        {
            try
            {
                if (this.con is MySqlConnection)
                {
                    MySqlCommand cmd = new MySqlCommand(procedure, (MySqlConnection)con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    MySqlParameter p1 = new MySqlParameter("id", this.Id);
                    cmd.Parameters.Add(p1);
                    return cmd.ExecuteNonQuery();
                }
                else if (this.con is SqlConnection)
                {
                    SqlCommand cmd = new SqlCommand(procedure, (SqlConnection)con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter p1 = new SqlParameter("id", this.Id);
                    cmd.Parameters.Add(p1);
                    return cmd.ExecuteNonQuery();
                }
                return this.delete();
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.ToString());
                return this.delete();
            }
        }
    }
}
