using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// LogPusher by LB0MG 
// 2015

namespace LogPusher
{
    class WebConnect
    {
        private string password;
        private string username;
        private string adifpath;
        private string bookid;
        private string book;

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }

        public string Adifpath
        {
            get
            {
                return adifpath;
            }

            set
            {
                adifpath = value;
            }
        }

        public string Bookid
        {
            get
            {
                return bookid;
            }

            set
            {
                bookid = value;
            }
        }

        public string Book
        {
            get
            {
                return book;
            }

            set
            {
                book = value;
            }
        }

        public string ConnectToService()
        {

            using (var client = new WebClientEx())
            {
                var values = new NameValueCollection
                { 
                    { "username", username },
                    { "password", password },
                };
                // Authenticate (must be https on this one)
                byte[] retval;
                try
                {
                    retval = client.UploadValues("https://www.qrz.com/login", values);
                }
                catch (Exception)
                {
                    Form1.Instance.addEventText("Could not connect to qrz.com...");
                    return "-1";
                }
                Form1.Instance.addEventText("Connected to qrz.com...");
                string retstring = Encoding.UTF8.GetString(retval);

                /*
                var direct = new NameValueCollection
                {
                    {"op","book_opts" }
                };
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] result = client.UploadValues("http://logbook.qrz.com/logbook", "POST", direct);
                string ResultAuthTicket = Encoding.UTF8.GetString(result);
                */
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("op", "upfile");
                nvc.Add("book", book);
                nvc.Add("bid", bookid);
                //nvc.Add("upload_file", adifpath);
                string rstring = HttpUploadFile("http://logbook.qrz.com/adif",adifpath, "upload_file", "application/octet-stream", nvc, client.CookieContainer);
                if (rstring.Contains("successfully"))
                {
                    Form1.Instance.addEventText("Log uploaded to qrz.com...");
                    return "Upload success";
                }
                else
                {
                    Form1.Instance.addEventText("Upload failed...");
                    return "Upload failed";
                }

            }

        }

        public static string HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc, CookieContainer cookies)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            wr.CookieContainer = cookies;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                return reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
                
            }
            return "NULL";
        }

    }


    /// <summary>
    /// A custom WebClient featuring a cookie container
    /// </summary>
    public class WebClientEx : WebClient
    {
        public CookieContainer CookieContainer { get; private set; }

        public WebClientEx()
        {
            CookieContainer = new CookieContainer();
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = CookieContainer;
            }
            return request;
        }
    }

}
