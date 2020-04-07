using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Tutorial.SqlConn;
using Tutorial.MySiteUtils;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Web.Script.Serialization;

namespace WebApplication2.Controllers
{
    public class Work {
        public List<String[]> Get(String start, String length,int choose, out String m)
        {
            MySqlCommand cmd = new MySqlCommand();
            int count = choose == 0 ? 11 : 6;
            if (choose==0)
                cmd.CommandText = "SELECT id,count,url,download,analyzis,COALESCE(code,'null'),COALESCE(xpath,'null'),COALESCE(page,'null'),COALESCE(regex,'null'),date,COALESCE(tag,'null') FROM `sites` LIMIT @start,@length;";
            else
                cmd.CommandText = "SELECT id,sid,url,code,COALESCE(amount,'null'),COALESCE(xamount,'null') FROM `pages` LIMIT @start,@length;";
            cmd.Parameters.Add("@start", MySqlDbType.Int32).Value = int.Parse(start);
            cmd.Parameters.Add("@length", MySqlDbType.Int32).Value = int.Parse(length);
            SiteUtils su = new SiteUtils();
            var list = new List<String[]>();            
            m = su.ReadFromDB(cmd, ref list, count);            
            return list;
        }      
    }
    public class DataController : Controller
    {
        // GET: Data
        private String[] Names1 ={"id",
            "count" ,
            "url" ,
            "download",
            "analyzis",
            "code" ,
            "xpath",
            "page" ,
            "regex",
            "date" ,
            "tag" };
        private String[] Names2 ={
            "id" ,
            "sid" ,
            "url" ,
            "code",
            "amount",
            "xamount"};         
        private String ListToJson(List<String[]> data, String[] names)
        {
            List<String> buf = new List<String>();
            foreach (var r in data)
            {
                String s = "";
                for (int i = 0; i < r.Count(); i++)
                {
                    String encoded = r[i];
                    if (names[i]=="regex")
                        encoded = System.Security.SecurityElement.Escape(r[i]);
                    if (i > 0) s += ",\n";
                    s += JsonConvert.ToString(names[i]) + ": " + JsonConvert.ToString(encoded);                   
                }
                buf.Add(s);
            }
            String o = "[\n{";
            o += String.Join("},\n{", buf);
            o += "}\n]";
            return o;
        }
        public JsonResult DataRequest1()
        {
            string start = this.Request.QueryString["start"];
            string length = this.Request.QueryString["length"];
            String o;
            List<String[]> list = new Work().Get(start, length, 0, out o);
            ViewBag.Message = o;

            string jsonData ="{\"data\":"+ListToJson(list,Names1)+"}";
            
            JavaScriptSerializer j = new JavaScriptSerializer();
            object obj = j.Deserialize(jsonData, typeof(object));
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DataRequest2()
        {
            string start = this.Request.QueryString["start"];
            string length = this.Request.QueryString["length"];
            String o;
            List<String[]> list = new Work().Get(start, length, 1, out o);
            ViewBag.Message = o;

            string jsonData = "{\"data\":" + ListToJson(list, Names2) + "}";
            
            JavaScriptSerializer j = new JavaScriptSerializer();
            object obj = j.Deserialize(jsonData, typeof(object));
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}
