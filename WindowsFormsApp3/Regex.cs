using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp3
{
    class Paginator
    {
        public Paginator(String domen)
        {
            Domen = domen;
            errors = new List<String>();
        }
        private bool has_numbers(String s)
        {
            Regex regex = new Regex(@"[\d]"); //цифры в тексте
            return regex.IsMatch(s);
        }

        private bool is_number(String s)
        {
            int res;
            return int.TryParse(s, out res);
        }

        public List<HtmlNode> Find(String html)
        {
            //public String Find(String html)       
            List<HtmlNode> tmp = new List<HtmlNode>();

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
            var dn = document.DocumentNode;
            foreach (var node in dn.DescendantNodes())
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == "a")
                    {
                        if (has_numbers(node.InnerText))
                            tmp.Add(node);
                    }
                }
            }
            int i = 2;
            int n = 2;

            List<HtmlNode> p = new List<HtmlNode>();
            List<HtmlNode> pages = new List<HtmlNode>();

            foreach (var node in tmp)
            {
                String t = node.InnerText;
                bool is_num = false;
                if (is_number(t) && (int.Parse(t) == i))
                    is_num = true;
                if (is_num)
                {
                    i += 1;
                    p.Add(node);
                    if (i > n)
                    {
                        n = i;
                        pages.Clear();
                        pages.AddRange(p);
                    }
                }
                else
                {
                    i = 2;
                    p.Clear();
                }
            }
            return pages;
        }
        private String compareClasses(String class1, String class2)
        {
            int i = 0;
            while (i < class1.Length && i < class2.Length)
            {
                if (!(class1[i] == class2[i]))
                    break;
                i++;
            }
            return class1.Substring(0, i);
        }

        public String extract_re(List<HtmlNode> page_items)
        {
            String root_class = "";
            String root_href = "";
            String root_query = "";
            bool first = true;
            bool has_query = true;
            foreach (var tag_class in page_items)
            {
                String full_class = "";
                String href1 = "";
                String query1 = "";
                String str = "";
                if (tag_class.Attributes["class"] != null)
                {
                    full_class = tag_class.Attributes["class"].Value;
                }
                str = tag_class.OuterHtml;
                Regex regex = new Regex("(?<=href=[\"'])\\S+?(?=[\"'])");
                if (regex.IsMatch(str))
                    str = regex.Match(str).Value;
                else
                    str = "";

                href1 = str;

                Regex regex2 = new Regex("(?<=\\?)\\S+?$");
                if (regex2.IsMatch(str))
                {
                    str = regex2.Match(str).Value;
                    query1 = str;
                    Regex regex3 = new Regex("\\S+?(?=\\?)");
                    if (regex3.IsMatch(href1))
                    {
                        href1 = regex3.Match(href1).Value;
                    }
                }
                else
                {
                    str = "";
                    has_query = false;
                }
                if (first)
                {
                    first = false;
                    root_class = full_class;
                    root_href = href1;
                    root_query = query1;
                }
                else
                {
                    root_class = compareClasses(root_class, full_class);
                    root_href = compareClasses(root_href, href1);
                    root_query = compareClasses(root_query, query1);
                }
            }

            if (root_class == "" && ((root_href == "" || root_href == "/") && root_query == ""))
                return "";
            String result = "";
            if (root_class == "")
                if (!has_query)
                    result = "(?<=<a .*?href=[\"\'])" + root_href + "\\S*?(?=[\"\'].*?>)";
                else
                    result = "(?<=<a .*?href=[\"\'])" + root_href + "\\S*?\\?" + root_query + "\\S*?(?=[\"\'].*?>)";
            else
                if ((root_href == "" || root_href == "/") && root_query == "")
                result = "(?<=<a .*?class=[\"\']?" + root_class + ".*?[\"\']? .*?href=[\"\'])\\S+?(?=[\"\'].*?>)|(?<=<a .*?href=[\"\'])\\S+?(?=[\"\'].*? class=[\"\']?" + root_class + ".*?[\"\']?.*?>)";
            else
                if (!has_query)
                result = "(?<=<a .*?class=[\"\']?" + root_class + ".*?[\"\']? .*?href=[\"\'])" + root_href + "\\S*?(?=[\"\'].*?>)|(?<=<a .*?href=[\"\'])" + root_href + "\\S*?(?=[\"\'].*? class=[\"\']?" + root_class + ".*?[\"\']?.*?>)";
            else
                result = "(?<=<a .*?class=[\"\']?" + root_class + ".*?[\"\']? .*?href=[\"\'])" + root_href + "\\S*?\\?" + root_query + "\\S*?(?=[\"\'].*?>)|(?<=<a .*?href=[\"\'])" + root_href + "\\S*?\\?" + root_query + "\\S*?(?=[\"\'].*? class=[\"\']?" + root_class + ".*?[\"\']?.*?>)";

            return result;
        }
        public void Start()
        {
            Load();

            List<HtmlNode> page_items = new List<HtmlNode>();

            foreach (var page in data)
            {
                //String url = page[1];
                var res = Find(page[2]);
                if (res.Count > 1)
                    page_items.AddRange(res);
            }

            if (page_items.Count > 0)
                Reg = extract_re(page_items);
            else
                Reg = "";
            AddError(UpdateRegex(ID, Reg));

        }
        public void ClearErrors()
        {
            errors.Clear();
        }
        public void AddError(String Error)
        {
            if (Error != "")
            {
                errors.Add(Error);
            }
        }
        public List<String> GetErrors()
        {
            return errors;
        }
        protected List<String> errors;
        public virtual String UpdateRegex(int ID, String Regex)
        {
            return "";
        }
        public virtual void Load()
        {            
        }
        public String Domen { set; get; }
        public int ID { set; get; }
        public String Reg { set; get; }

        public List<String[]> data;

    }

    class Paginator2 : Paginator
    {
        public Paginator2(String domen) : base(domen)
        {
            su = new SiteUtils();
        }

        public override void Load()
        {
            MySqlCommand cmd = new MySqlCommand();

            cmd.CommandText = "SELECT id FROM `sites` WHERE url=@url;";
            cmd.Parameters.Add("@url", MySqlDbType.Text).Value = Domen;

            List<String[]> ans = new List<String[]>();

            su.ReadFromDB(cmd, ref ans, 1);

            ID = int.Parse(ans[0][0]);

            MySqlCommand cmd2 = new MySqlCommand();

            cmd2.CommandText = "SELECT id,url,code FROM `pages` WHERE sid=@sid;";
            cmd2.Parameters.Add("@sid", MySqlDbType.Int32).Value = ID;

            data = new List<String[]>();

            su.ReadFromDB(cmd2, ref data, 3);
        }
        public override String UpdateRegex(int ID, String Regex)
        {
            return su.UpdateRegex(ID, Regex);
        }
        private SiteUtils su;
    }
}
