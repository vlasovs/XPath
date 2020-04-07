using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;

namespace Tutorial.MySiteUtils
{
    class SiteUtils
    {        
        public String NewSite(String domen, int count, out int id)
        {
            id = 0;
            String Error = "";
            MySqlConnection conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO `sites` (count, url, download, analyzis) values(@count,@url,'0 %','0 %');";
                cmd.Parameters.Add("@count", MySqlDbType.Int32).Value = count;
                cmd.Parameters.Add("@url", MySqlDbType.Text).Value = domen;
                cmd.ExecuteNonQuery();

                MySqlCommand cmd2 = conn.CreateCommand();
                cmd2.CommandText = "SELECT id FROM `sites` WHERE url=@url;";
                cmd2.Parameters.Add("@url", MySqlDbType.Text).Value = domen;
                using (DbDataReader reader = cmd2.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        id = reader.GetInt32(0);
                    }
                }
            }
            catch (Exception exc)
            {
                Error = "New " + domen + " - Error: " + exc.Message;
            }
            conn.Close();
            return Error;
        }
        public String DeleteSite(String domen)
        {
            String Error = "";
            MySqlConnection conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id FROM `sites` WHERE url=@url;";
                cmd.Parameters.Add("@url", MySqlDbType.Text).Value = domen;
                int id = 0;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        id = reader.GetInt32(0);
                    }
                }

                MySqlCommand cmd2 = conn.CreateCommand();
                cmd2.CommandText = "DELETE FROM `pages` WHERE sid=@sid;";
                cmd2.Parameters.Add("@sid", MySqlDbType.Int32).Value = id;
                cmd2.ExecuteNonQuery();

                MySqlCommand cmd3 = conn.CreateCommand();
                cmd3.CommandText = "DELETE FROM `sites` WHERE id=@id;";
                cmd3.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                cmd3.ExecuteNonQuery();

            }
            catch (Exception exc)
            {
                Error = "Delete " + domen + " - Error: " + exc.Message;
            }
            conn.Close();
            return Error;
        }

        public String CheckSite(String domen, out int count)
        {
            String Error = "";
            count = 0;
            MySqlConnection conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(id) FROM `sites` WHERE url=@url;";
                cmd.Parameters.Add("@url", MySqlDbType.Text).Value = domen;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        count = reader.GetInt32(0);
                    }
                }
            }
            catch (Exception exc)
            {
                Error = "Check " + domen + " - Error: " + exc.Message;
            }
            conn.Close();
            return Error;
        }
        public String SavePage(int id, String url, String resp, String percent)
        {
            List<MySqlCommand> lc = new List<MySqlCommand>();

            MySqlCommand cmd1 = new MySqlCommand();
            cmd1.CommandText = "INSERT INTO `pages` (sid, url, code) VALUES (@sid, @url, @code);";

            cmd1.Parameters.Add("@sid", MySqlDbType.Int32).Value = id;
            cmd1.Parameters.Add("@url", MySqlDbType.Text).Value = url;
            cmd1.Parameters.Add("@code", MySqlDbType.LongText).Value = resp;

            MySqlCommand cmd2 = new MySqlCommand();

            cmd2.CommandText = "UPDATE `sites` SET download = @percent WHERE id = @id;";
            cmd2.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
            cmd2.Parameters.Add("@percent", MySqlDbType.VarChar).Value = percent;

            lc.Add(cmd1);
            lc.Add(cmd2);

            return Execute(lc);
        }
        public String UpdateSite(int id, String percent)
        {
            List<MySqlCommand> lc = new List<MySqlCommand>();

            MySqlCommand cmd1 = new MySqlCommand();            

            cmd1.CommandText = "UPDATE `sites` SET analyzis = @percent WHERE id = @id;";
            cmd1.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
            cmd1.Parameters.Add("@percent", MySqlDbType.VarChar).Value = percent;

            lc.Add(cmd1);

            return Execute(lc);
        }
        public String UpdatePages(List<String[]> m,bool x)
        {
            List<MySqlCommand> lc = new List<MySqlCommand>();
            foreach (var p in m)
            {
                MySqlCommand cmd1 = new MySqlCommand();
                if (x)
                    cmd1.CommandText = "UPDATE `pages` SET xamount = @amount WHERE id = @id;";
                else
                    cmd1.CommandText = "UPDATE `pages` SET amount = @amount WHERE id = @id;";

                cmd1.Parameters.Add("@id", MySqlDbType.Int32).Value = int.Parse(p[0]);
                cmd1.Parameters.Add("@amount", MySqlDbType.Int32).Value = int.Parse(p[1]);

                lc.Add(cmd1);
            }

            return Execute(lc);
        }
        public String SaveXPath(int ID, String xpath, int pid, String Code)
        {
            List<MySqlCommand> lc = new List<MySqlCommand>();

            MySqlCommand cmd1 = new MySqlCommand();

            cmd1.CommandText = "UPDATE `sites` SET xpath=@xpath,page=@page,code=@code WHERE id = @id;";
            cmd1.Parameters.Add("@id", MySqlDbType.Int32).Value = ID;
            cmd1.Parameters.Add("@xpath", MySqlDbType.Text).Value = xpath;
            cmd1.Parameters.Add("@page", MySqlDbType.Int32).Value = pid;
            cmd1.Parameters.Add("@code", MySqlDbType.LongText).Value = Code;

            lc.Add(cmd1);

            return Execute(lc);
        }
        public String UpdateRegex(int ID, String Regex)
        {
            List<MySqlCommand> lc = new List<MySqlCommand>();

            MySqlCommand cmd1 = new MySqlCommand();

            cmd1.CommandText = "UPDATE `sites` SET regex=@regex WHERE id = @id;";
            cmd1.Parameters.Add("@id", MySqlDbType.Int32).Value = ID;
            cmd1.Parameters.Add("@regex", MySqlDbType.Text).Value = Regex;

            lc.Add(cmd1);

            return Execute(lc);
        }
        public String Execute(List<MySqlCommand> qs)
        {
            String Error = "";
            MySqlConnection conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();
                foreach (var q in qs)
                {
                    q.Connection = conn;
                    q.ExecuteNonQuery();
                }
            }
            catch (Exception exc)
            {
                Error = "Execute: " + exc.Message;
            }
            conn.Close();
            return Error;
        }
        public String ReadFromDB(MySqlCommand q, ref List<String[]> list, int Columns)
        {
            list.Clear();
            String Error = "";
            MySqlConnection conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();
                q.Connection = conn;
                using (DbDataReader reader = q.ExecuteReader())
                {
                    if (reader.HasRows)
                    {                       
                        while (reader.Read())                     
                        {
                            List<String> x = new List<String>();
                            for (int i = 0; i < Columns; i++)
                            {                                
                                x.Add(reader.GetString(i));                                
                            }
                            list.Add(x.ToArray());
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Error = "ReadFromDB " + q.CommandText + " - Error: " + exc.Message;
            }
            conn.Close();
            return Error;
        }
    }    
}
