using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// TODO:
// 1: Keep Settings even when the version changes (Now the version number is fixed at 1.1.1.1)
// 2:

/*
The MIT License(MIT)

Copyright(c) 2015
By Joakim Flathagen, LB0MG

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace LogPusher
{
    public partial class Form1 : Form
    {
        static WebConnect webconnect = new WebConnect();
        public static Form1 Instance;
        static DateTime last;
        public Form1()
        {
            InitializeComponent();
            Instance = this;
            last = DateTime.Now;
        }

        public void addEventText(string Text)
        {
            DateTime now = DateTime.Now;
            string timestring = now.ToLongTimeString();
            //string name = "";
            if (txtEvents.InvokeRequired)
            {
                txtEvents.Invoke(new MethodInvoker(delegate { txtEvents.Text += timestring + " : " + Text + "\r\n"; }));
            }
            else {
                txtEvents.Text += timestring + " : " + Text + "\r\n";
            }
        }
        

        static public void uploadLogfile()
        {
            // Must be at least 30 seconds between each upload
            // To avoid multiple uploads to QRZ if the file event handler is called twice.
            if((DateTime.Now - last).TotalSeconds > 5)
            {
                last = DateTime.Now;
                Form1.Instance.addEventText("File changed, uploading file...");
                webconnect.ConnectToService();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            UpdateUploader();
            uploadLogfile();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly()
                                           .GetName()
                                           .Version
                                           .ToString();

            addEventText("Starting LogPusher " + version);
            addEventText("Loading settings...");
            try
            {
                txtBook.Text = Properties.Settings.Default["Book"].ToString();
                txtBookId.Text = Properties.Settings.Default["BookId"].ToString();
                txtUsername.Text = Properties.Settings.Default["UserName"].ToString();
                txtPassword.Text = Properties.Settings.Default["Password"].ToString();
                txtAdifPath.Text = Properties.Settings.Default["AdilFile"].ToString();

                if (Properties.Settings.Default["Autoupload"].Equals(true))
                {
                    rdoAuto.Checked = true;
                    rdoManual.Checked = false;
                }
                else
                {
                    rdoAuto.Checked = false;
                    rdoManual.Checked = true;
                }

            }
            catch (Exception)
            {

                MessageBox.Show("You need to enter settings");
            }

            UpdateUploader();

            // If log file exists, add watcher
            if(File.Exists(txtAdifPath.Text) )
            {
                addEventText("Watching file " + txtAdifPath.Text);
                CreateFileWatcher(txtAdifPath.Text);
            }

            
        }

        private void UpdateUploader()
        {
            webconnect.Username = txtUsername.Text;
            webconnect.Password = txtPassword.Text;
            webconnect.Adifpath = txtAdifPath.Text;
            webconnect.Book = txtBook.Text;
            webconnect.Bookid = txtBookId.Text;
         
        }

        private void btnFileChoose_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string filename = openFileDialog1.FileName;
            txtAdifPath.Text = filename;

      
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["Book"] = txtBook.Text;
            Properties.Settings.Default["BookId"] = txtBookId.Text;
            Properties.Settings.Default["UserName"] = txtUsername.Text;
            Properties.Settings.Default["Password"] = txtPassword.Text;
            Properties.Settings.Default["AdilFile"] = txtAdifPath.Text;
            Properties.Settings.Default["Autoupload"] = rdoAuto.Checked;
            Properties.Settings.Default.Save();

        }

        public void CreateFileWatcher(string path)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(path);
            watcher.Filter = Path.GetFileName(path);

            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);

            // Begin watching.s
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
           
            if(Properties.Settings.Default["Autoupload"].Equals(true)) uploadLogfile();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }


}
