using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Xml;

namespace DLL
{
    public abstract partial class Model
    {
        private long id = 0;
        private string sql = "";
        private IDbConnection con;
        private readonly string connectionString;
        private readonly string databaseType;
        public long Id { get => id; set => id = value; }
        public Model()
        {
            var configuration = XDocument.Load("appsettings.xml");
            connectionString = configuration.Descendants("add")
                .Where(x => x.Attribute("key").Value == "ConnectionString")
                .Select(x => x.Attribute("value").Value)
                .FirstOrDefault();

            databaseType = configuration.Descendants("add")
                .Where(x => x.Attribute("key").Value == "DatabaseType")
                .Select(x => x.Attribute("value").Value)
                .FirstOrDefault();

            this.con = Connexion.connect(connectionString, databaseType);
        }

        public Dictionary<string, T> ToDictionary<T>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dico = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
            return dico;
        }

        public dynamic DictionaryToObject(Dictionary<string, object> dico)
        {
            var model = Activator.CreateInstance((this).GetType());
            PropertyInfo[] properties = (this).GetType().GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].SetValue(model, dico[properties[i].Name]);
            }
            return model;
        }
        public static dynamic DictionaryToObject<T>(Dictionary<string, object> dico)
        {
            var model = Activator.CreateInstance(typeof(T));
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].SetValue(model, dico[properties[i].Name]);
            }
            return model;
        }

        public int save(string procedure = null)
        {
            if (procedure == null)
            {
                if (this.Id == 0)
                {
                    Dictionary<string, object> dico = new Dictionary<string, object>();
                    dico = ToDictionary<object>(this);
                    sql = "insert into " + this.GetType().Name + "(";
                    foreach (var entry in dico)
                    {
                        if (!entry.Key.Equals("Id"))
                            sql += entry.Key + ",";
                    }
                    sql = sql.Remove(sql.Length - 1, 1);
                    sql += ") values(";
                    foreach (var entry in dico)
                    {
                        if (!entry.Key.Equals("Id"))
                        {
                            if (entry.Key.GetType().Name.Equals("String"))
                            {
                                double tmp;
                                if (double.TryParse(entry.Value.ToString(), out tmp))
                                {
                                    string str = "";
                                    str += tmp;
                                    str = str.Replace(',', '.');
                                    sql += str + ",";
                                }
                                else
                                    sql += "'" + entry.Value + "',";
                            }
                            else
                                sql += entry.Value + ",";
                        }
                    }
                    sql = sql.Remove(sql.Length - 1, 1);
                    sql += ")";
                }
                else
                {
                    Dictionary<string, object> dico = new Dictionary<string, object>();
                    dico = ToDictionary<object>(this);
                    sql = "update " + this.GetType().Name + " set ";


                    foreach (var entry in dico)
                    {
                        if (!entry.Key.Equals("Id") && entry.Value != null)
                        {
                            if (entry.Key.GetType().Name.Equals("String"))
                            {
                                double tmp;
                                if (double.TryParse(entry.Value.ToString(), out tmp))
                                {
                                    string str = "";
                                    str += tmp;
                                    str = str.Replace(',', '.');
                                    sql += entry.Key + "=" + "" + str + ",";
                                }
                                else
                                    sql += entry.Key + "=" + "'" + entry.Value + "',";
                            }

                            else
                                sql += entry.Key + "=" + "" + entry.Value + ",";
                        }
                    }
                    sql = sql.Remove(sql.Length - 1, 1);
                    sql += " where id = " + Id;
                }
                Console.WriteLine(sql);
                return Connexion.IUD(sql);
            }
            else
            {
                Console.WriteLine(sql);
                if (con is MySqlConnection)
                    this.saveProcedureMySql(procedure);
                else if (con is SqlConnection)
                    this.saveProcedureSql(procedure);
                return save();
            }
        }

        public dynamic find()
        {
            Dictionary<string, object> dico = new Dictionary<string, object>();
            Dictionary<string, string> ch = new Dictionary<string, string>();
            sql = "select * from " + this.GetType().Name + " where id=" + Id;
            Console.WriteLine (sql);
            IDataReader dr = Connexion.select(sql);

            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    dico.Add(name, dr.GetValue(i));
                }
            }
            dr.Close();
            return DictionaryToObject(dico);
        }

        public dynamic find<T>(int id)
        {
            Dictionary<string, object> dico = new Dictionary<string, object>();
            sql = "select * from " + typeof(T).Name + " where id=" + id;
            IDataReader idr = Connexion.select(sql);
            if (idr.Read())
            {
                for (int i = 0; i < idr.FieldCount; i++)
                {
                    string name = idr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    dico.Add(name, idr.GetValue(i));
                }
            }
            return DictionaryToObject<T>(dico);
        }


        public List<dynamic> All()
        {
            List<dynamic> res = new List<dynamic>();
            Dictionary<string, object> dico = new Dictionary<string, object>();
            sql = "select * from " + this.GetType().Name;
            IDataReader dr = Connexion.select(sql);
            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    dico.Add(name, dr.GetValue(i));
                }

                res.Add(DictionaryToObject(dico));
                dico.Clear();
            }
            dr.Close();
            return res;
        }


        public static List<dynamic> all<T>()
        {
            List<dynamic> res = new List<dynamic>();
            Dictionary<string, object> dico = new Dictionary<string, object>();
            Dictionary<string, string> ch = new Dictionary<string, string>();
            string sql = "select * from " + (typeof(T)).Name + ";";
            Console.WriteLine(sql);
            IDataReader dr = Connexion.select(sql);
            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    //Console.WriteLine(name+" ..-.-.- "+ dr.GetValue(i));
                    dico.Add(name, dr.GetValue(i));

                    
                }

                res.Add(DictionaryToObject<T>(dico));
                dico.Clear();
            }
            dr.Close();

            return res;
        }

        public List<dynamic> Select(Dictionary<string, object> dico)
        {
            List<dynamic> res = new List<dynamic>();
            Dictionary<string, object> dico1 = new Dictionary<string, object>();
            sql = "select * from " + this.GetType().Name + " where ";
            foreach (var entry in dico)
            {
                if (entry.Key.GetType().Name.Equals("String"))
                    sql += entry.Key + " = " + "'" + entry.Value.ToString() + "'" + " and ";
                else
                    sql += entry.Key + " = " + entry.Value.ToString() + " and ";
            }

            for (int i = 0; i < 4; i++)
            {
                sql = sql.Remove(sql.Length - 1, 1);
            }
            Console.WriteLine(sql);

            IDataReader dr = Connexion.select(sql);

            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    dico1.Add(name, dr.GetValue(i));
                }

                res.Add(DictionaryToObject(dico1));
                dico1.Clear();
            }
            dr.Close();
            return res;
        }


        public static List<dynamic>  Select<T>(Dictionary<string, object> dico)
        {
            List<dynamic> res = new List<dynamic>();
            Dictionary<string, object> dico1 = new Dictionary<string, object>();
            string sql = "select * from " + (typeof(T)).Name + " where ";
            foreach (var entry in dico)
            {
                if (entry.Key.GetType().Name.Equals("String"))
                    sql += entry.Key + " = " + "'" + entry.Value.ToString() + "'" + " and ";
                else
                    sql += entry.Key + " = " + entry.Value.ToString() + " and ";
            }

            for (int i = 0; i < 4; i++)
            {
                sql = sql.Remove(sql.Length - 1, 1);
            }

            IDataReader dr = Connexion.select(sql);

            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    dico1.Add(name, dr.GetValue(i));


                }

                res.Add(DictionaryToObject<T>(dico1));
                dico1.Clear();
            }
            dr.Close();
            return res;
        }
        public int delete()
        {
            string sql = "delete from "+ this.GetType().Name+" where id="+id;
            return Connexion.IUD(sql);
        }

        public static int update<T>(int id, Dictionary<string, object> changedFields)
        {
            string sql = "update " + typeof(T).Name + " set ";
            foreach (var entry in changedFields)
            {
                if (entry.Value.GetType().Name.Equals("String"))
                    sql += entry.Key + " = " + "'" + entry.Value.ToString() + "'" + ", ";
                else
                    sql += entry.Key + " = " + entry.Value.ToString() + ", ";
            }
            sql = sql.Remove(sql.Length - 2, 2);
            sql += " where id = " + id.ToString();
            return Connexion.IUD(sql);
        }

        public static List<int> findId<T>(Dictionary<string, object> dico)
        {
            List<int> list = new List<int>();
            string sql = "select * from " + (typeof(T)).Name + " where ";
            foreach (var entry in dico)
            {
                if (entry.Key.GetType().Name.Equals("String"))
                    sql += entry.Key + " = " + "'" + entry.Value.ToString() + "'" + " and ";
                else
                    sql += entry.Key + " = " + entry.Value.ToString() + " and ";
            }
            for (int i = 0; i < 4; i++)
            {
                sql = sql.Remove(sql.Length - 1, 1);
            }
            IDataReader dr = Connexion.select(sql);
            while (dr.Read())
            {
                list.Add(dr.GetInt32(0));
            }
            dr.Close();
            return list;
        }

    }
}
