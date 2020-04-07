using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tutorial.SqlConn;
using Tutorial.MySiteUtils;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.IO;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Sites()
        {
            /*
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT id,count,url,download,analyzis,COALESCE(code,'null'),COALESCE(xpath,'null'),COALESCE(page,'null'),COALESCE(regex,'null'),date,COALESCE(tag,'null') FROM `sites`;";
            SiteUtils su = new SiteUtils();
            var list = new List<String[]>();
            ViewBag.Message = su.ReadFromDB(cmd,ref list, 11);
            ViewBag.Data = list;
            */            
            return View();
        }
        public ActionResult Pages()
        {
            /*
            MySqlCommand cmd = new MySqlCommand();
            
            cmd.CommandText = "SELECT id,sid,url,code,COALESCE(amount,'null'),COALESCE(xamount,'null') FROM `pages`;";
            SiteUtils su = new SiteUtils();
            var list = new List<String[]>();
            ViewBag.Message = su.ReadFromDB(cmd, ref list, 11);
            ViewBag.Data = list;
            */
            return View();
        }
        public ActionResult Index()
        {            
            return View();
        }        
    }
}