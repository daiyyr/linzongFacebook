using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Net.Cache;

namespace facebookWorm
{
    public partial class Form1 : Form
    {

        public static bool debug = true;
        public static bool gForceToStop = false;
        public static CookieCollection gCookieContainer = null;
        public static bool gLoginOkFlag = false;
        static DateTime expireDate = new DateTime(2015, 10, 12);
        static string rgx;
        static Match myMatch;

        //Thread gAlarm = null;
       // string gnrnodeGUID = "";
      //  string gViewstate = "";
      //  string gViewStateGenerator = "";
       

        public delegate void setLog(string str1);
        public void setLogT(string s)
        {
            if (logT.InvokeRequired)
            {
                // 实例一个委托，匿名方法，
                setLog sl = new setLog(delegate(string text)
                {
                    logT.AppendText(DateTime.Now.ToString() + " " + text + Environment.NewLine);
                });
                // 把调用权交给创建控件的线程，带上参数
                logT.Invoke(sl, s);
            }
            else
            {
                logT.AppendText(DateTime.Now.ToString() + " " + s + Environment.NewLine);
            }
        }

        public void setLogtRed(string s)
        {
            if (logT.InvokeRequired)
            {
                setLog sl = new setLog(delegate(string text)
                {
                    logT.AppendText(DateTime.Now.ToString() + " " + text + Environment.NewLine);
                    int i = logT.Text.LastIndexOf("\n", logT.Text.Length - 2);
                    if (i > 1)
                    {
                        logT.Select(i, logT.Text.Length);
                        logT.SelectionColor = Color.Red;
                        logT.Select(i, logT.Text.Length);
                        logT.SelectionFont = new Font(logT.Font, FontStyle.Bold);
                    }
                });
                logT.Invoke(sl, s);
            }
            else
            {
                logT.AppendText(DateTime.Now.ToString() + " " + s + Environment.NewLine);
                int i = logT.Text.LastIndexOf("\n", logT.Text.Length - 2);
                if (i > 1)
                {
                    logT.Select(i, logT.Text.Length);
                    logT.SelectionColor = Color.Red;
                    logT.Select(i, logT.Text.Length);
                    logT.SelectionFont = new Font(logT.Font, FontStyle.Bold);
                }
            }
        }

        public delegate void DSetTestLog(HttpWebRequest req, string respHtml);
        public void setTestLog(HttpWebRequest req, string respHtml)
        {
            if (testLog.InvokeRequired)
            {
                DSetTestLog sl = new DSetTestLog(delegate(HttpWebRequest req1, string text)
                {
                    testLog.Text = Environment.NewLine + "返回的HTML源码：";
                    testLog.Text += Environment.NewLine + text;
                });
                testLog.Invoke(sl, req, respHtml);
            }
            else
            {
                testLog.Text = Environment.NewLine + "返回的HTML源码：";
                testLog.Text += Environment.NewLine + respHtml;
            }
        }

        public Form1()
        {
            InitializeComponent();
            label6.Text = "expire date: " + expireDate.ToString("yyyy-MM-dd");
            if (debug)
            {
                button1.Visible = true;
                testLog.Visible = true;
                this.ClientSize = new System.Drawing.Size(931, 760);
            }
            else
            {
                DateTime t = GetNistTime(this);
                if (t == DateTime.MinValue)
                {
                    setLogT("请连接互联网后重新启动程序");
                    autoB.Visible = false;
                }
                else
                {
                    if ((t - expireDate).Days > 0)
                    {
                        setLogT("程序已过期，请联系作者");
                        autoB.Visible = false;
                    }
                }
            }

            

            //if (File.Exists(System.Environment.CurrentDirectory + "\\" + "urlList"))
            //{
            //    string[] lines = File.ReadAllLines(System.Environment.CurrentDirectory + "\\" + "urlList");
            //    foreach (string line in lines)
            //    {
            //        urlList.Items.Add(line);
            //    }
            //}
        }
        /*
        public void alarm()
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(WHA_avac.Properties.Resources.mtl);
            player.Load();
            player.PlayLooping();
        }
        */

        public static DateTime GetNistTime(Form1 form1)
        {
            DateTime dateTime = DateTime.MinValue;

//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.stdtime.gov.tw/chinese/home.aspx");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException webEx)
            {
                form1.setLogT("WebException: " + webEx.Status.ToString());
                return dateTime;
            }
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(response.GetResponseStream());
//                string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
//                string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
//                double milliseconds = Convert.ToInt64(time) / 1000.0;
//                dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();

                string html = stream.ReadToEnd();//id="TimeTag" style="display: none;">2015/09/28 20:39:42</span>
                string time = Regex.Match(html, @"(?<=id=""TimeTag"" style=""display: none;"">)[^ ]*").Value;
                dateTime = DateTime.ParseExact(time, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
            }
            return dateTime;
        }

        public static string ToUrlEncode(string strCode)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(strCode); //默认是System.Text.Encoding.Default.GetBytes(str)  
            System.Text.RegularExpressions.Regex regKey = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9]+$");
            for (int i = 0; i < byStr.Length; i++)
            {
                string strBy = Convert.ToChar(byStr[i]).ToString();
                if (regKey.IsMatch(strBy))
                {
                    //是字母或者数字则不进行转换    
                    sb.Append(strBy);
                }
                else
                {
                    sb.Append(@"%" + Convert.ToString(byStr[i], 16));
                }
            }
            return (sb.ToString());
        }

        public static void writeFile(string file, string content)
        {
            FileStream aFile;
            StreamWriter sw;
            aFile = new FileStream(file, FileMode.Append);
            sw = new StreamWriter(aFile);
            sw.Write(content);
            sw.Close();
        }




        public static void setRequest(HttpWebRequest req)
        {
            //req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //req.Accept = "*/*";
            //req.Connection = "keep-alive";
            //req.KeepAlive = true;
            //req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E";
            //req.Headers["Accept-Encoding"] = "gzip, deflate";
            //req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            req.Host = "www.facebook.com";

            req.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.10; rv:40.0) Gecko/20100101 Firefox/40.0";
            req.AllowAutoRedirect = false;
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.PerDomainCapacity = 40;
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            req.ContentType = "application/x-www-form-urlencoded";
        }

        public static int writePostData(Form1 form1, HttpWebRequest req, string data)
        {
            byte[] postBytes = Encoding.UTF8.GetBytes(data);
            //req.ContentLength = postBytes.Length;  // cause InvalidOperationException: 写入开始后不能设置此属性。
            Stream postDataStream = null;
            try
            {
                postDataStream = req.GetRequestStream();
                postDataStream.Write(postBytes, 0, postBytes.Length);
            }
            catch (WebException webEx)
            {
                form1.setLogT("While writing post data," + webEx.Status.ToString());
                return -1;
            }
            
            postDataStream.Close();
            return 1;
        }

        public static string resp2html(HttpWebResponse resp)
        {
            
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(resp.GetResponseStream());
                return stream.ReadToEnd();
            }
            else
            {
                return resp.StatusDescription;
            }

            /*
             char[] cbuffer = new char[256];
             string respHtml = "";
            Stream respStream = resp.GetResponseStream();
            StreamReader respStreamReader = new StreamReader(respStream);//respStream,Encoding.UTF8
            int byteRead = 0;
            try
            {
                byteRead = respStreamReader.Read(cbuffer, 0, 256);
            }
            catch (WebException webEx)
            {
                setLogT("respStreamReader, " + webEx.Status.ToString());
                return "";
            }
            while (byteRead != 0)
            {
                string strResp = new string(cbuffer, 0, byteRead);
                respHtml = respHtml + strResp;
                try
                {
                    byteRead = respStreamReader.Read(cbuffer, 0, 256);
                }
                catch (WebException webEx)
                {
                    setLogT("respStreamReader, " + webEx.Status.ToString());
                    return "";
                }

            }
            respStreamReader.Close();
            respStream.Close();
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                return respHtml;
            }
            else
            {
                return resp.StatusDescription;
            }
             */
        }


        /* 
         * return success or not
         */
        public static int weLoveMuYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData)
        {
            while (true)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
					if (writePostData (form1, req, postData) < 0) {
						continue;
					}
                }
                string respHtml = "";
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("respStreamReader, " + webEx.Status.ToString());
                    continue;
                }
                if (resp != null)
                {
                    respHtml = resp2html(resp);
                    if (respHtml.Equals(""))
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                if (debug)
                {
                    form1.setTestLog(req, respHtml);
                }
                gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
                resp.Close();
                break;
            }
            return 1;
        }

        /* 
         * return responsive HTML
         */
        public static string weLoveYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData)
        {
            while (true)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
					if (writePostData (form1, req, postData) < 0) {
						continue;
					}
                }
                string respHtml = "";
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("respStreamReader, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        return "wrong address"; //user id错误
                    }
                    continue;
                }
                if (resp != null)
                {
                    respHtml = resp2html(resp);
                    if (respHtml.Equals(""))
                    {
                        continue;
                    }
                    gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
                    if (debug)
                    {
                        form1.setTestLog(req, respHtml);
                    }
                    resp.Close();
                    return respHtml;
                }
                else
                {
                    continue;
                }
            }
        }

        /*
         * do not handle the response
         */
        public static HttpWebResponse weLoveYueer(Form1 form1, HttpWebResponse resp, string url, string method, string referer, bool allowAutoRedirect, string postData)
        {
            while (true)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                setRequest(req);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
						continue;
					}
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("respStreamReader, " + webEx.Status.ToString());
                    continue;
                }
                if (resp != null)
                {
                    gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
                    return resp;
                }
                else
                {
                    continue;
                }
            }
        }



        

        private void loginB_Click(object sender, EventArgs e)
        {
            Login login = new Login(this);
            Thread t = new Thread(login.loginT);
            t.Start();
        }

        private void autoB_Click(object sender, EventArgs e)
        {
            Friend friend = new Friend(this);
            Thread t = new Thread(friend.startProbe);
            t.Start();
        }

        private void rate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void logT_TextChanged(object sender, EventArgs e)
        {
            logT.SelectionStart = logT.Text.Length;
            logT.ScrollToCaret();
        }

        public delegate void delegate2();

        public void addIds()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(*.txt)|*.txt|(*.html)|*.html";

            if (urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    //打开对话框, 判断用户是否正确的选择了文件
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {

                        //获取用户选择文件的后缀名
                        //    string extension = Path.GetExtension(fileDialog.FileName);
                        //声明允许的后缀名
                        //    string[] str = new string[] { ".txt", ".html" };
                        //    if (!str.Contains(extension))
                        //    {
                        //        MessageBox.Show("仅能上传txt,html格式的文件！");
                        //    }
                        //}

                        //获取用户选择的文件，并判断文件大小不能超过20K，fileInfo.Length是以字节为单位的
                        FileInfo fileInfo = new FileInfo(fileDialog.FileName);
                        if (fileInfo.Length > 204800)
                        {
                            MessageBox.Show("上传的文件不能大于200K");
                        }
                        else
                        {
                            //在这里就可以写获取到正确文件后的代码了
                            string[] lines = File.ReadAllLines(fileDialog.FileName);
                            foreach (string line in lines)
                            {
                                if (line.Length == 0)
                                {
                                    continue;
                                }
                                if ((line.Length>0 && line.Length < 4) ||!line.Substring(0, 4).Equals("1-1-"))
                                {
                                    MessageBox.Show("文件格式错误,导入中止!");
                                    break;
                                }
                                else
                                {
                                    urlList.Items.Add(line.Substring(4, line.Length - 4));
                                }
                            }
                        }
                    }
                });
                urlList.Invoke(sl);
            }
            else //do not use delegate
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FileInfo fileInfo = new FileInfo(fileDialog.FileName);
                        if (fileInfo.Length > 204800)
                        {
                            MessageBox.Show("上传的文件不能大于200K");
                        }
                        else
                        {
                            string[] lines = File.ReadAllLines(fileDialog.SafeFileName);
                            foreach (string line in lines)
                            {
                                if (!line.Substring(0, 4).Equals("1-1-"))
                                {
                                    MessageBox.Show("文件格式错误!");
                                    break;
                                }
                                else
                                {
                                    urlList.Items.Add(line.Substring(4, line.Length - 4));
                                }
                            }
                        }
                    }
                }
        }

        public void deleteURL()
        {
            if (urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    for (int i = urlList.CheckedItems.Count - 1; i >= 0; i--)
                    {
                        urlList.Items.Remove(urlList.CheckedItems[i]);
                    }
                });
                urlList.Invoke(sl);
            }
            else
            {
                for (int i = urlList.CheckedItems.Count - 1; i >= 0; i--)
                {
                    urlList.Items.Remove(urlList.CheckedItems[i]);
                }
            }
            /*
            string strCollected = string.Empty;
            for (int i = 0; i < urlList.Items.Count; i++)
            {
                if (strCollected == string.Empty)
                {
                    strCollected = urlList.GetItemText(urlList.Items[i]);
                }
                else
                {
                    strCollected += "\n" + urlList.GetItemText(urlList.Items[i]);
                }
            }
            writeFile(System.Environment.CurrentDirectory + "\\" + "urlList", strCollected);
             * */
        }

        private void addB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(addIds);
            t.Start();
        }

        private void deleteB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(deleteURL);
            t.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pattern = @"^";
            string replacement = "1-1-";
            string result = Regex.Replace("12345", pattern, replacement);
            setLogT(result);

            rgx = @"(?<=aa).*?(?=aa)";
            myMatch = (new Regex(rgx)).Match("qqqqqaaqwdsfaafferaafe222aa2222444aa444444222faaloveaa");
            while (myMatch.Success)
            {
                setLogT(myMatch.Groups[0].Value);
                myMatch = myMatch.NextMatch();
            }

            string message = "4344.34334.23.24.";
            Regex rex = new Regex(@"^(\.|\d)+$");
            if (rex.IsMatch(message))
            {
                //float result2 = float.Parse(message);
                setLogT("match");
            }
            else
                setLogT("not match");

            int aa;
            if ((aa = 4) == 4)
            {
                setLogT(aa.ToString());
            }
        }

        private void textBox1_keyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                autoB.PerformClick();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gForceToStop = true;
            setLogT("stop probe");
        }
    }
}
