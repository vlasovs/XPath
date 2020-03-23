using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace WindowsFormsApp2
{
	class Record
	{
		public Record(long hash, int points, int level, String path)
		{
			Count = 1;
			Hash = hash;
			Points = points;
			Path = path;
			Level = level;
		}
		public long Hash { get; set; }
		public int Count { get; set; }
		public int Points { get; set; }
		public int Level { get; set; }
		public String Path { get; set; }

	}
	class Google
	{
		public bool Check(List<String[]> m)
		{
			bool have = false;
			foreach (String[] rec in m)
			{
				HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
				document.LoadHtml(rec[2]);
				var nodes = document.DocumentNode.SelectSingleNode("//div[@itemtype ='http://schema.org/Product']");
				have = (nodes != null);
				if (have) break;
			}
			return have;
		}
	}


	class TreeHash
	{
		private static long hashkof1 = 99999941;
		private static long hashkof2 = 99999959;
		private static long hashkof3 = 99999971;
		private static long hashkof4 = 99999989;

		private long process_tags(String path, HtmlNode node, int level, ref int child_count, ref bool noindex1, ref bool ref1, ref bool info1, ref bool price1)
		{

			HtmlNode sibling = node;
			long hashCode = 0;
			while (sibling != null)
			{
				if (sibling.NodeType == HtmlNodeType.Element)
				{

					String element = sibling.Name;
					String full_class = "";

					if (sibling.Attributes["class"] != null)
					{
						full_class = sibling.Attributes["class"].Value;
					}
					String new_path = path + element + ':' + full_class + "/";

					long hash_child = 0;
					child_count++;

					bool noindex2 = false;
					bool ref2 = false;
					bool info2 = false;
					bool price2 = false;

					if ("noindex" == element.ToLower()) noindex2 = true;
					if ("a" == element.ToLower()) ref2 = true;
					if ("img" == element.ToLower()) info2 = true;

					Regex regex = new Regex(@"[\d]"); //цифры в тексте
					if (regex.IsMatch(sibling.InnerText.ToLower()))
					{
						price2 = true;
					}
					String first_class = full_class.Split(' ')[0];

					long name_hash = element.GetHashCode();
					long class_hash = first_class.GetHashCode();
					//long class_hash = full_class.GetHashCode();

					int current_child_count = 0;

					if (sibling.ChildNodes.Count > 0)
						hash_child = process_tags(new_path, sibling.FirstChild, level + 1, ref current_child_count, ref noindex2, ref ref2, ref info2, ref price2);
					child_count += current_child_count;
					long hash = (name_hash * hashkof2 + class_hash * hashkof3 + hash_child * hashkof4);
					if (hash < 0) hash = -hash;
					if (hash % 2 == 0) hash += 1;

					if (current_child_count > 4)
					{
						int points = 0;
						if (ref2) points += 1;
						if (info2) points += 1;
						if (price2) points += 1;
						if (noindex2) points = 0;
						if (path.ToLower().IndexOf("slide") == -1)
						{

							DoSomthing(new Record(hash, points, level, new_path));
														
						}
					}

					noindex1 |= noindex2;
					ref1 |= ref2;
					info1 |= info2;
					price1 |= price2;

					hashCode = (hashCode * hashkof1 + hash);

				}
				sibling = sibling.NextSibling;
			}
			return hashCode;
		}

		public void tags_content(String html)
		{

			HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
			document.LoadHtml(html);
			int child_count = 0;
			bool noindex1 = false, ref1 = false, info1 = false, price1 = false;
			process_tags("", document.DocumentNode.FirstChild, 0, ref child_count, ref noindex1, ref ref1, ref info1, ref price1);
		}

		protected virtual void DoSomthing(Record r) {
			//Nothing
		}

	}

	class SearchTree: TreeHash
	{
		public SearchTree() {
			hh = new Dictionary<long, Record>();
		}	

		private Dictionary<long, Record> hh;
		protected override void DoSomthing(Record r)
		{
			if (!hh.ContainsKey(r.Hash))
			{
				hh[r.Hash] = r;
			}
			else
			{
				if (hh[r.Hash].Points > r.Points)
					hh[r.Hash].Points = r.Points;
				hh[r.Hash].Count++;
			}
		}
		public Dictionary<long, Record> GetDict(){
			return hh;		
		}
		
	}

	class FindPathes : TreeHash
	{
		public FindPathes()
		{
			hh = new HashSet<String>();
			count = 0;
		}

		private HashSet<String> hh;

		private int count;

		protected override void DoSomthing(Record r)
		{
			if (Hash == r.Hash)
			{
				hh.Add(r.Path);
				count++;
			}
		}
		public int GetCount()
		{
			return count;
		}
		public HashSet<String> GetSet()
		{
			return hh;
		}
		public long Hash { set; get; }
	}

	class XPathFinder
	{

		private List<String[]> string2struct(String s)
		{
			List<String[]> res=new List<String[]>();
			String[] tags = s.Split('/');
			tags.Reverse();
			foreach (var tag in tags) 
				if ((!(tag=="") && (tag.IndexOf("tbody:")==-1)) )				
					res.Add(tag.Split(':'));
			return res;
		}

		private String compareclasses(String class1, String class2)
		{
			if (class1 == class2)
				return class1;
			else	
				return "";
		}

		private String cutxpath(String xpath)
		{
			String str1 = "";			
			int index = xpath.LastIndexOf('/');
			if (index > 1)
			{
				str1 = xpath.Substring(0, index);
			}			
			return str1;
		}

		private String shortxpath(String xpath) {

			int start = xpath.LastIndexOf("@class");
			if (start == -1)
				start = xpath.Length - 1;
			int index = 0;
			int i = 0;
			while (i < start && !(i == -1))
			{
				index = i;
				i = xpath.IndexOf("/", i + 1);
			}
			String str1 = "/" + xpath.Substring(index,xpath.Length-index);			
			return str1;
		}
		public bool FindXPath(String[] pathes)
		{
			String result = "";
			List<List<String[]>> st = new List<List<String[]>>();
			int count_tag = -1;
			foreach (var i in pathes) {
				List<String[]> str1 = string2struct(cutxpath(i));
				st.Add(str1);
				if (count_tag == -1)
					count_tag = str1.Count;
				else
					if (count_tag > str1.Count)
					count_tag = str1.Count;
			}

			List<String> res = new List<String>();

			for (int i = 0; i < count_tag; i++)
			{
				bool is_tag1 = true;
				bool is_class1 = false;
				String tag1 = null;
				String class1 = null;

				foreach (var str1 in st)
				{
					String tag2 = str1[i][0];
					String class2 = str1[i][1];
					if (tag1 == null)
						tag1 = tag2;
					else
						if (!(tag1 == tag2))
						is_tag1 = false;
					if (class1 == null)
					{
						class1 = class2;
						is_class1 = true;
					}
					else
						class1 = compareclasses(class1, class2);
					if (class1 == "")
						is_class1 = false;
				}
				if (is_tag1)
					if (is_class1 && !(class1 == ""))
					{
						String y = tag1 + "[@class=\"" + class1 + "\"]";
						res.Add(y);
					}
					else
						res.Add(tag1);
				else
					res.Add("*");
			}			
			int index = 0;

			while (res[index] == "*")
				index ++;

			if (index > 0)
				result = "/";
			else
				result = "";

			for (int i = 0; i < count_tag - index; i++) {
				result += "/" + res[i + index];
			}

			//XPath = result;
			XPath = shortxpath(result);	

			return true;
		}		
		public String XPath { set; get; }	
		
	}	

	class Forest
	{
		public Forest(String domen)
		{
			Domen = domen;
			errors = new List<String>();
			data = new List<String[]>();
		}
		private int RecLess(Record r1, Record r2)
		{
			if (r1.Points == r2.Points)
				if (r1.Count == r2.Count)
					return r1.Level - r2.Level;
				else return r2.Count - r1.Count;
			else return r2.Points - r1.Points;
		}
		protected virtual void Load() {			
		}
		protected virtual String Update()
		{
			return "";
		}

		public Record FindBestTree(List<String[]> m)
		{
			lock (xpercent)
			{
				percent = "0 %";
			}
			SearchTree tree = new SearchTree();
			for (int i = 0; i < m.Count; i++)
			{
				if (Stop) return new Record(0,0,0,"");
				lock (xpercent)
				{
					int p = (i * 33 / m.Count);
					percent = p.ToString() + " %";
				}
				AddError(Update());
				tree.tags_content(m[i][2]);
			}			
			List<Record> rs = new List<Record>();
			foreach (var p in tree.GetDict()) rs.Add(p.Value);
			rs.Sort(RecLess);
			return rs[0];
		}
		public String[] BestNodesAmount(Record rec, List<String[]> m)
		{
			lock (xpercent)
			{
				percent = "33 %";
			}
			List<String[]> ans=new List<String[]>();
			FindPathes tree = new FindPathes();
			tree.Hash = rec.Hash;
			int count = 0;
			MaxCount = 0;
			for (int i = 0; i < m.Count; i++)
			{				 
				tree.tags_content(m[i][2]);
				int current = tree.GetCount();
				int c = (current - count);
				if (MaxCount < c) {
					MaxCount = c;
					PageID = int.Parse(m[i][0]);
				}
				String[] x = { m[i][0], c.ToString() };
				count = current;
				ans.Add(x);								
				lock (xpercent)
				{
					int p = (i * 33 / m.Count) + 33;
					percent = p.ToString() + " %";
				}
				AddError(Update());

				if (Stop) break;
			}
			AddError(UpdatePage(ans));
			return tree.GetSet().ToArray();
		}

		public void XPathAmount(String XPath, List<String[]> m)
		{
			lock (xpercent)
			{
				percent = "66 %";
			}
			List<String[]> ans = new List<String[]>();
			for (int i = 0; i < m.Count; i++)
			{				
				HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
				document.LoadHtml(m[i][2]);
				var sn = document.DocumentNode.SelectNodes(XPath);
				int c = 0;
				if (sn != null)
					c = sn.Count;
				String[] x = { m[i][0], c.ToString() };				
				ans.Add(x);
				lock (xpercent)
				{
					int p = (i * 33 / m.Count) + 66;
					percent = p.ToString() + " %";
				}
				AddError(Update());
				if (Stop) break;
			}
			AddError(UpdatePage2(ans));
		}
		public void SaveCode(String XPath, List<String[]> m)
		{		
			
			for (int i = 0; i < m.Count; i++)
			{
				if (m[i][0] == PageID.ToString())
				{
					HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
					document.LoadHtml(m[i][2]);
					var sn = document.DocumentNode.SelectSingleNode(XPath);
					int c = 0;
					if (sn != null)
					{
						AddError(SaveXPath(ID, XPath, PageID, sn.OuterHtml));
					}
				}				
			}
			
		}
		public void Start() {
			Load();
			Google g = new Google();
			XPath = "";
			/*
			if (g.Check(data))
			{
				XPath = "//div[@itemtype =\"http://schema.org/Product\"]";
			}
			else*/
			{
				var p = FindBestTree(data);
				if (!Stop)
				{
					String[] pathes = BestNodesAmount(p, data);
					if (!Stop)
					{
						XPathFinder x = new XPathFinder();
						x.FindXPath(pathes);
						XPath = x.XPath;
					}
				}
			}
			if (!Stop) XPathAmount(XPath, data);			
			if (!Stop) SaveCode(XPath, data);

			lock (xpercent)
			{
				percent = "100 %";
			}
			if (!Stop) AddError(Update());			
		}		
		protected virtual String SaveXPath(int ID, String xpath, int pid, String Code)
		{
			return "";
		}
		
		public void ClearErrors()
		{
			errors.Clear();
		}
		public void AddError(String Error) {
			if (Error != "") {
				errors.Add(Error);
			}
		}
		public List<String> GetErrors()
		{
			return errors;
		}
		protected virtual String UpdatePage(List<String[]> m) {
			return "";
		}
		protected virtual String UpdatePage2(List<String[]> m)
		{
			return "";
		}
		public String XPath { set; get; }
		public String Domen { set; get; }
		public int PageID { set; get; }
		public int MaxCount { set; get; }
		public int ID { set; get; }
		public bool Stop { set; get; }
		protected List<String> errors;
		protected List<String[]> data;
		protected String percent;
		private readonly object xpercent = new object();
		public String GetPercent()
		{
			String s = "";
			lock (xpercent)
			{
				s = percent;
			}
			return s;
		}		
	}

	class Forest2 : Forest
	{
		public Forest2(String domen):base(domen) 
		{				
			su = new SiteUtils();			
		}
		protected override void Load()
		{
			MySqlCommand cmd = new MySqlCommand();

			cmd.CommandText = "SELECT id FROM `sites` WHERE url=@url;";
			cmd.Parameters.Add("@url", MySqlDbType.Text).Value = Domen;

			List<String[]> ans = new List<String[]>();
			AddError(su.ReadFromDB(cmd, ref ans, 1));

			ID = int.Parse(ans[0][0]);

			MySqlCommand cmd2 = new MySqlCommand();

			cmd2.CommandText = "SELECT id,url,code FROM `pages` WHERE sid=@sid;";
			cmd2.Parameters.Add("@sid", MySqlDbType.Int32).Value = ID;
						
			AddError(su.ReadFromDB(cmd2, ref data, 3));
			
		}
		protected override String SaveXPath(int ID, String xpath, int pid, String Code)
		{
			return su.SaveXPath(ID, xpath, pid, Code); ;
		}

		protected override String Update()
		{
			return su.UpdateSite(ID, percent);
		}
		protected override String UpdatePage(List<String[]> m)
		{
			return su.UpdatePages(m,false); 
		}
		protected override String UpdatePage2(List<String[]> m)
		{
			return su.UpdatePages(m,true);
		}
		private readonly SiteUtils su;
	}
}

