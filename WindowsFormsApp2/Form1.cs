using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Data.Common;
using System.Threading;

namespace WindowsFormsApp2
{	
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }		

        private void button1_Click(object sender, EventArgs e)
        {		     
			
			SiteUtils su = new SiteUtils();
            
            MySqlCommand cmd = new MySqlCommand();

            cmd.CommandText = "SELECT id,url,count FROM `sites`;";

            List<String[]> ans=new List<String[]>();
            su.ReadFromDB(cmd, ref ans, 3);

            listBox1.Items.Clear();
            for (int i=0;i<ans.Count;i++) {
                listBox1.Items.Add(ans[i][1]);
            }            
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            stop = true;
        }
        private bool stop;

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            stop = false;
            richTextBox1.Clear();
            try
            {
                const int mms = 20;
                int taskcount = int.Parse(numericUpDown1.Text);                

                Forest[] sites = { };
                Array.Resize(ref sites, taskcount);
                for (int i = 0; i < taskcount; i++)
                {
                    //sites[i] = new SiteDownloader(new Downloader());
                    //sites[i] = new SiteDownloader(new DownloaderZPrice());
                    sites[i] = null;
                }
                Task[] tasks = new Task[taskcount];
                int count = 0;
                String[] urls= { };
                Array.Resize(ref urls, listBox1.Items.Count);
                for(int i = 0; i < listBox1.Items.Count; i++)                
                    urls[i] = listBox1.Items[i].ToString();                
                
                bool next = true;
                while (next && (!stop))
                {
                    String text = "";
                    next = false;
                    for (int i = 0; i < taskcount; i++)
                    {
                        if (tasks[i] != null)
                            if (tasks[i].IsCompleted)
                            {
                                foreach (var str in sites[i].GetErrors())
                                    richTextBox2.AppendText(str + "\n");
                                richTextBox2.AppendText(sites[i].Domen + " " + sites[i].GetPercent() + "\n");
                                tasks[i].Dispose();
                                tasks[i] = null;
                            }
                            else next = true;

                        if (tasks[i] == null)
                        {
                            if (count < urls.Length)
                            {
                                sites[i] = new Forest2(urls[count]);                                
                                tasks[i] = Task.Run(sites[i].Start);
                                next = true;
                                count++;
                            }
                        }
                        else text += sites[i].Domen + " " + sites[i].GetPercent() + "\n";
                    }
                    richTextBox1.Text = text;
                    for (int ms = 0; ms < mms; ms++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }

                for (int i = 0; i < taskcount; i++)
                {
                    if (tasks[i] != null)
                        sites[i].Stop = true;
                    else tasks[i] = Task.Run(() => { /*nothing */ });
                }
                Task.WaitAll(tasks);
                for (int i = 0; i < taskcount; i++)
                {
                    tasks[i].Dispose();
                }
            }
            catch (Exception ex)
            {
                richTextBox1.Text = "Error:\n" + ex.Message;
            }
            button2.Enabled = true;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Array a=Array.CreateInstance(typeof(object), listBox1.SelectedItems.Count);
                listBox1.SelectedItems.CopyTo(a, 0);
                foreach (var si in a)
                {
                    listBox1.Items.Remove(si);
                }
                
            }
        }
    }	
}
