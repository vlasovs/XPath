using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;

namespace WindowsFormsApp1
{
    class Page { 
        public String url
        {
            get;
            set;            
        }
        public String code
        {
            get;
            set;            
        }

    }
    class Downloader
    {
        public Downloader(){
            resp = "";
        }
        public virtual String MyRequest(String site, out bool fe) {
            fe = true;
            WebClient wc = new WebClient();
            try
            {
                Stream str = wc.OpenRead(site);
                StreamReader sr = new StreamReader(str);
                resp = sr.ReadToEnd();
                if (resp != "")
                {
                    fe = false;
                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp = site + " Error: " + ex.Message;
                return "";
            }
            return "";
        }       
        public String GetResp()
        {
            return resp;
        }
       
        protected String resp;

    }
    class DownloaderZPrice: Downloader
    {
        public DownloaderZPrice():base() {}
        private String MyWaitAnswer(Int64 id)
        {
            string url = "http://dm.z-price.com/XPathTest/GetResult?taskId=" + id;
            string result = "";
            bool ok = false;
            string status = "";
            Int64 state = 0;
            string html = "";
            while (!ok)
            {
                Thread.Sleep(100);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resp = streamReader.ReadToEnd();
                    JsonTextReader reader = new JsonTextReader(new StringReader(resp));
                    int count = 0;

                    while (reader.Read())
                    {
                        string num = count.ToString(CultureInfo.InvariantCulture);
                        if (count == 2)
                        {
                            status = (string)reader.Value;
                        }
                        if (count == 23)
                        {
                            html = (string)reader.Value;
                        }
                        if (count == 25)
                        {
                            state = (Int64)reader.Value;
                        }

                        if (reader.Value != null)
                        {
                            result += "Token " + num + ": " + reader.TokenType + " Value: " + reader.Value;
                        }
                        else
                        {
                            result += "Token " + num + ": " + reader.TokenType + " = NULL";
                        }
                        count++;
                        result += "\n";
                    }

                }
                if (state == 2)
                {
                    ok = true;
                }

                if (status != "success")
                {
                    ok = true;
                    html = "Error";
                }
                //ok = true;
            }
            result += "\n";
            result += html;

            return result;
        }

        public override String MyRequest(String site, out bool fe)
        {
            fe = true;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://dm.z-price.com/XPathTest/AddTask");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            String result = "";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"href\":\"" + site + "\"}";
                streamWriter.Write(json);
            }
            string status = "Error";
            string html = "Error";
            //Int64 id = 0;
            resp = "";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                resp = streamReader.ReadToEnd();
                JsonTextReader reader = new JsonTextReader(new StringReader(resp));
                int count = 0;
                while (reader.Read())
                {
                    if (count == 2)
                    {
                        status = (string)reader.Value;
                    }
                    if (count == 6)
                    {
                        html = (string)reader.Value;
                    }
                    /*
                    if (count == 4)
                    {
                        id = (Int64)reader.Value;
                    }
                    string num=count.ToString(CultureInfo.InvariantCulture);

                    if (reader.Value != null)
                    {
                        result+="Token "+ num + ": " + reader.TokenType+" Value: "+reader.Value;
                    }
                    else
                    {
                        result += "Token " + num + ": " + reader.TokenType + " = NULL";
                    }
                    */
                    count++;
                    //result += "\n";
                }
            }
            if (status == "success")
            {
                //result += MyWaitAnswer(id);
                if (html != "")
                {
                    result = html;
                    fe = false;
                }
                else {
                    resp = site + " - Error";
                }
            }
            return result;
        }        
    }    

    class Checker
    {
        public void Load(string path) {
            Extensions = new List<string>();
            Keywords = new List<string>();
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs, Encoding.UTF8);
            int sw = 0;
            String[] spearator = {" "};
            Int32 count = 100;
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                
                String[] strlist = str.Split(spearator, count,
                StringSplitOptions.RemoveEmptyEntries);

                foreach (String s in strlist)
                {
                    if (s == "[files]") sw = 1;
                    else if (s == "[keywords]") sw = 2;
                    else
                    {
                        if (sw == 1) Extensions.Add(s);
                        else if (sw == 2) Keywords.Add(s);
                    }
                }
            }
        }
        private List<String> Extensions;
        private List<String> Keywords;
        public String CheckURL(String domen, String tmp, out bool ok)
        {
            ok = false;
            Uri siteUri = new Uri(domen);
            String host = siteUri.Authority;
            Uri newUri;
            String ans = "";

            try
            {
                newUri = new Uri(tmp);
                if (host == newUri.Host)
                {
                    ans = tmp;
                }
                else
                {
                    return ans;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (tmp[0] == '/')
                    {
                        newUri = new Uri(siteUri.Scheme + "://" + siteUri.Host + tmp);
                    }
                    else
                    {
                        newUri = new Uri(siteUri.Scheme + "://" + siteUri.Host + "/" + tmp);
                    }
                    ans = newUri.OriginalString;
                }
                catch (Exception ex1)
                {
                    return "Error: " + ex.Message + " , " + ex1.Message;
                }            
            }
            ok = true;            
            int ld = ans.LastIndexOf('.');
            if (ld != -1) {
                String s = ans.Substring(ld + 1);
                s = s.ToLower();
                foreach (String ext in Extensions) {
                    if (s == ext) {
                        ok = false;
                        break;
                    }
                }
            }

            if (ok) foreach (String key in Keywords)
                {
                    int k = newUri.PathAndQuery.IndexOf(key);
                    if (k != -1)
                    {
                        ok = false;
                        break;
                    }
                }
            if (ok) return ans;            
            else return ans += " Ignore!!!";
        }

        public List<String> FindHrefs(String domen,String html)
        {
            List<String> ans=new List<String>();
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
            var nodes = document.DocumentNode.SelectNodes("//a");
            if (nodes!=null) foreach (var node in nodes)
            {
                var ca = node.GetAttributeValue("href", string.Empty);
                bool ok;
                string tmp = CheckURL(domen, ca, out ok);
                if (tmp != "" && ok)
                {
                    ans.Add(tmp);                    
                }
            }
            return ans;
        }
    }
    
    class SiteDownloader
    {
        public SiteDownloader(Downloader d)
        {
            Down = d;
            errors = new List<String>();
            ch1 = new Checker();
            ch1.Load("ignore.cnf");
            Pages = new List<Page>();
        }
        protected virtual String NewSite(String domen, int count, out int id)
        {
            id = 0;
            String Error = "";            
            return Error;
        }

        protected virtual String DeleteSite(String domen)
        {
            String Error = "";            
            return Error;
        }

        protected virtual String CheckSite(String domen, out int count)
        {
            count = 0;
            String Error = "";            
            return Error;
        }
        protected virtual String SavePage(int id, String url, String resp, String percent)
        {
            Page p = new Page
            {
                url = url,
                code = resp
            };
            Pages.Add(p);
            return "";
        }
        private void AddError(String Error) {
            lock (xerrors)
            {
                errors.Add(Error);
            }
        }
        public void DownloadSite()
        {
            Stop = false;
            var rand = new Random();
            //errors.Clear();
            String Error;
            int count_site;
            Error = CheckSite(Domen, out count_site);
            if (Error != "") { AddError(Error); return; }
            if (count_site > 0)
            {
                Error = DeleteSite(Domen);
                if (Error != "") { AddError(Error); return; }
            }

            Error = NewSite(Domen, Maxcount, out int id);
            if (Error != "") { AddError(Error); return; }
            lock (xpercent)
            {
                percent = "0 %";
            }

            //pages = new List<Page>();

            var urlset = new HashSet<String>();
            var urls = new Queue<String>();
            urlset.Add(Domen);
            urls.Enqueue(Domen);
            int count = 0;
            while (urls.Count > 0 && (!Stop))
            {
                var cur = urls.Dequeue();
                bool me;
                String resp;
                try
                {
                    resp = Down.MyRequest(cur, out me);
                }
                catch (Exception ex)
                {
                    AddError(cur + " - Error: " + ex.Message);                
                    continue;
                }
                if (!me)
                {
                    count++;
                    int p = (count * 100) / Maxcount;
                    lock (xpercent)
                    {
                        percent = p + " %";
                    }

                    Error = SavePage(id, cur, resp, percent);
                    if (Error != "") AddError(cur + ": " + Error);                        

                    if (count >= Maxcount) break;
                    var result = ch1.FindHrefs(Domen, resp);
                    foreach (var x in result)
                        if (!urlset.Contains(x))
                        {
                            urlset.Add(x);
                            urls.Enqueue(x);
                        }
                }
                else {                    
                    AddError(Down.GetResp());
                }
                int rr = rand.Next(101);
                Thread.Sleep(100 + rr);
            }
        }
        protected Checker ch1;
        protected Downloader Down;
        protected List<String> errors;
        public String Domen { get; set; }
        public int Maxcount { get; set; }
        public String GetText()
        {
            lock (xpercent)            
            {
                return Domen + " " + percent;
            }
        }
        public void ClearErrors()
        {
            lock (xerrors)
            {
                errors.Clear();
            }
        }
        public List<String> GetErrors()
        {
            var e = new List<String>();
            lock (xerrors) {
                e = errors;
            }
            return e;
        }
        public List<Page> GetPages() {
            return Pages;
        }
        public String GetPercent()
        {
            String s = "";
            lock (xpercent)
            {
                s = percent;
            }

            return s;
        }
        public bool Stop { set; get; }
        private List<Page> Pages;
        private String percent;
        private readonly object xpercent = new object();
        private readonly object xerrors = new object();
    }
    
    class DownloadAndSave: SiteDownloader
    {
        public DownloadAndSave(Downloader d):base(d) {
            su = new SiteUtils();
        }
        protected override String NewSite(String domen, int count, out int id)
        {
            String Error = su.NewSite(domen,count,out id);            
            return Error;
        }

        protected override String DeleteSite(String domen)
        {
            String Error = su.DeleteSite(domen);            
            return Error;
        }

        protected override String CheckSite(String domen, out int count)
        {
            String Error = su.CheckSite(domen,out count);            
            return Error;
        }
        protected override String SavePage(int id, String url, String resp, String percent)
        {
            String Error = su.SavePage(id, url, resp, percent);            
            return Error;
        }
        private SiteUtils su;
    }
}
