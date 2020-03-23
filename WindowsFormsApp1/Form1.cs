using System;
using System.Data;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //richTextBox1.Parent.DoubleBuffered = true;
        }         
        
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            stop = false;
            richTextBox1.Clear();
            try
            {
                const int mms = 20;
                int taskcount = int.Parse(numericUpDown1.Text);
                int pagecount = int.Parse(numericUpDown2.Text);

                SiteDownloader[] sites = { };
                Array.Resize(ref sites, taskcount);
                for (int i = 0; i < taskcount; i++)
                {
                    //sites[i] = new SiteDownloader(new Downloader());
                    //sites[i] = new SiteDownloader(new DownloaderZPrice());
                    sites[i] = new DownloadAndSave(new DownloaderZPrice());
                }
                Task[] tasks = new Task[taskcount];
                int count = 0;
                String[] urls = richTextBox2.Lines;

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
                                    richTextBox1.AppendText(str + "\n");
                                richTextBox1.AppendText(sites[i].GetText() + "\n");
                                tasks[i].Dispose();
                                tasks[i] = null;
                            }
                            else next = true;

                        if (tasks[i] == null)
                        {
                            if (count < urls.Length)
                            {
                                sites[i].ClearErrors();
                                sites[i].Domen = urls[count];
                                sites[i].Maxcount = pagecount;
                                tasks[i] = Task.Run(sites[i].DownloadSite);
                                next = true;
                                count++;
                            }
                        }
                        else text += sites[i].GetText() + "\n";
                    }
                    richTextBox3.Text = text;
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
            button1.Enabled = true;
        }       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                button1_Click(null, null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private bool stop;
    }
}
