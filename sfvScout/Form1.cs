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

namespace WHA_avac
{
    public partial class Form1 : Form
    {

        bool debug = false;
        string username = "";           
        string password = "";
   //     string username = "54739633%40qq.com";
   //     string password = "dyyr0125";

        DateTime expireDate = new DateTime(2015, 10, 12);

        //Thread gAlarm = null;
       // string gnrnodeGUID = "";
      //  string gViewstate = "";
      //  string gViewStateGenerator = "";
       
        CookieCollection gCookieContainer = null;
		string rgx;
		Match myMatch;
        bool gLoginOkFlag = false;
        List<string> gFriends = new List<string>();
        string collection_token = "";
        string cursor = "";
        string profile_id= "";
        string user_id="";
        string revision = "";
        int succeed = 0;
        int failed = 0;
        string gFileName = null;
        FileStream aFile ;
        StreamWriter sw;
        int successInOneProbe = 0;
        bool gForceToStop = false;
        int SUMsuccessInOneProbe;
        int iii;

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
                        logT.SelectionFont = new Font(Font, FontStyle.Bold);
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
                    logT.SelectionFont = new Font(Font, FontStyle.Bold);
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

            DateTime t = GetNistTime();
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

        public DateTime GetNistTime()
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
                setLogT("WebException: "+ webEx.Status.ToString() );
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

        public void writeFile(string file, string content)
        {
            aFile = new FileStream(file, FileMode.Append);
            sw = new StreamWriter(aFile);
            sw.Write(content);
            sw.Close();
        }

        public int writeResult(string content)
        {
            if (gFriends.Count>0){
                if (gFileName == null)
                {
                    gFileName = "save_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", DateTimeFormatInfo.InvariantInfo) + ".txt";
                    setLogT("Create file: " + System.Environment.CurrentDirectory + "\\" + gFileName);
                }
                writeFile(System.Environment.CurrentDirectory + "\\" + gFileName, content);

            }
                 
            return 1;
        }


        public void setRequest(HttpWebRequest req)
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

        public int writePostData(HttpWebRequest req, string data)
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
                setLogT("While writing post data," + webEx.Status.ToString());
                return -1;
            }
            
            postDataStream.Close();
            return 1;
        }

        public string resp2html(HttpWebResponse resp)
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
        public int weLoveMuYue(string url, string method, string referer, bool allowAutoRedirect, string postData)
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
					if (writePostData (req, postData) < 0) {
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
                    setLogT("respStreamReader, " + webEx.Status.ToString());
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
                    setTestLog(req, respHtml);
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
        public string weLoveYue(string url, string method, string referer, bool allowAutoRedirect, string postData)
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
					if (writePostData (req, postData) < 0) {
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
                    setLogT("respStreamReader, " + webEx.Status.ToString());
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
                        setTestLog(req, respHtml);
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
        public HttpWebResponse weLoveYueer(HttpWebResponse resp, string url, string method, string referer, bool allowAutoRedirect, string postData)
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
					if (writePostData (req, postData) < 0) {
						continue;
					}
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    setLogT("respStreamReader, " + webEx.Status.ToString());
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



        public int loginF()
        {
            setLogT("login..");
			string respHtml = "";

			respHtml = weLoveYue("https://www.facebook.com/",
				"GET",
				"",
				false,
				"");

			string lgnrnd = "";
			string token = "";
            string datr = "";

            if (respHtml.Equals("Found"))
            {
                setLogT("getting login page failed!");
                return -1;
            }

            rgx = @"(?<=name=""reg_instance"" value="").+?(?="")";
            myMatch = (new Regex(rgx)).Match(respHtml);
            if (myMatch.Success)
            {
                datr = myMatch.Groups[0].Value;
            }
            else
            {
                setLogT("getting login page failed!");
                return -1;
            }

			rgx = @"(?<=input type=""hidden"" name=""lgnrnd"" value="").*?(?="" />)";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				lgnrnd = myMatch.Groups[0].Value;
			}

			rgx = @"(?<=type=""hidden"" name=""lsd"" value="").*?(?="" autocomplete=""off"" />)";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				token = myMatch.Groups[0].Value;
			}

            gCookieContainer.Add(new Cookie("_js_datr", datr) { Domain = "www.facebook.com" });

            //to get cookies: datr
            // weLoveMuYue("https://www.facebook.com/hellocdn/results?data=%7B%22results%22%3A%5B%7B%22loading_time%22%3A0%2C%22platform%22%3A%22www%22%2C%22cdn%22%3A%22ak%22%2C%22resource_timing%22%3A%7B%22name%22%3A%22https%3A%2F%2Ffbcdn-photos-b-a.akamaihd.net%2Fhphotos-ak-prn1%2Ftest-80KB.jpg%22%2C%22entryType%22%3A%22resource%22%2C%22startTime%22%3A307.131182%2C%22duration%22%3A422.90310700000003%2C%22initiatorType%22%3A%22xmlhttprequest%22%2C%22redirectStart%22%3A0%2C%22redirectEnd%22%3A0%2C%22fetchStart%22%3A307.131182%2C%22domainLookupStart%22%3A307.131182%2C%22domainLookupEnd%22%3A307.131182%2C%22connectStart%22%3A307.131182%2C%22connectEnd%22%3A307.131182%2C%22secureConnectionStart%22%3A0%2C%22requestStart%22%3A308.652099%2C%22responseStart%22%3A398.19683299999997%2C%22responseEnd%22%3A730.0342890000001%7D%2C%22url%22%3A%22https%3A%2F%2Ffbcdn-photos-b-a.akamaihd.net%2Fhphotos-ak-prn1%2Ftest-80KB.jpg%22%2C%22headers%22%3A%22Access-Control-Allow-Origin%3A%20*%5Cr%5CnCache-Control%3A%20no-transform%2C%20max-age%3D8%5Cr%5CnContent-Length%3A%2079957%5Cr%5CnContent-Type%3A%20image%2Fjpeg%5Cr%5CnDate%3A%20Sat%2C%2019%20Sep%202015%2003%3A55%3A59%20GMT%5Cr%5CnExpires%3A%20Sat%2C%2019%20Sep%202015%2003%3A56%3A07%20GMT%5Cr%5CnLast-Modified%3A%20Fri%2C%2012%20Dec%202014%2000%3A53%3A28%20GMT%5Cr%5CnServer%3A%20proxygen%5Cr%5CnTiming-Allow-Origin%3A%20*%5Cr%5Cnx-akamai-session-info%3A%20name%3DBEGIN_CLOCK%3B%20value%3D1435761000%2C%20name%3DCLOCK_DURATION%3B%20value%3D6873959%2C%20name%3DFB_DISABLE_FULL_HTTPS%3B%20value%3Dtrue%2C%20name%3DFB_DISABLE_FULL_LOGGING%3B%20value%3Dtrue%2C%20name%3DFB_LOGGING_URL_SAMPLE%3B%20value%3Dtrue%2C%20name%3DFULL_PATH_KEY%3B%20value%3Dfalse%2C%20name%3DHSAFSERIAL%3B%20value%3D842%2C%20name%3DNOW_CLOCK%3B%20value%3D1442634959%2C%20name%3DORIGIN%3B%20value%3Dhphotos-ak-prn1%2C%20name%3DOVERRIDE_HTTPS_IE_CACHE_BUST%3B%20value%3Dall%2C%20name%3DSERIALNEXT%3B%20value%3D1791%2C%20name%3DSINGLE_TIER%3B%20value%3Dtrue%2C%20name%3DSINGLE_TIER_HVAL%3B%20value%3D789613%2C%20name%3DVALIDORIGIN%3B%20value%3Dtrue%3B%20full_location_id%3Dmetadata%5Cr%5Cnx-akamai-ssl-client-sid%3A%20B2VGSAuDC%2BONy6lq7deAkQ%3D%3D%2C%20jXyFJAJ1swG8eI9JJLO85A%3D%3D%2C%20eHGuYL6ebCGxtQ%2FFzTWoqQ%3D%3D%2C%20xTmDZCNM%2BaM1EFSiyU%2B5PQ%3D%3D%2C%2000ViDZZBGURh2d4RBXYqtA%3D%3D%2C%20ldw%2F1r4y03Q8umDIRzyoDw%3D%3D%2C%20QoOhGh3xuf88M%2BjTOOnWfg%3D%3D%2C%20Z8kyY5MKFQLQt3zz2YkPsQ%3D%3D%2C%20slblWhmVC8ViR3qetpM4dw%3D%3D%2C%20xrYGqTI4Hs1DdfyZ5Yx27w%3D%3D%2C%20VjZkcoaZbgPN8byHaDILuA%3D%3D%2C%20p4Jq2SVzcMwCfYiMGWuigg%3D%3D%2C%20rMFYfXpdb3PLXvjNNOBgrw%3D%3D%5Cr%5CnX-Cache%3A%20TCP_MISS%20from%20a119-224-129-198.deploy.akamaitechnologies.com%20(AkamaiGHost%2F7.3.2.2-15906379)%20(-)%5Cr%5Cnx-cache-key%3A%20S%2FL%2F1791%2F98030%2F14d%2Fphoto.facebook.com%2Ftest-80KB.jpg%5Cr%5Cnx-cache-remote%3A%20TCP_HIT%20from%20a119-224-129-207.deploy.akamaitechnologies.com%20(AkamaiGHost%2F7.3.2.2-15906379)%20(-)%5Cr%5Cnx-check-cacheable%3A%20YES%5Cr%5Cnx-serial%3A%201791%5Cr%5Cnx-true-cache-key%3A%20%2FL%2Fphoto.facebook.com%2Ftest-80KB.jpg%5Cr%5CnX-Firefox-Spdy%3A%203.1%5Cr%5Cn%22%2C%22status%22%3A200%7D%5D%7D",
             //   "GET",
            //    "",
            //    false,
             //   "");


            respHtml = weLoveYue("https://www.facebook.com/login.php?login_attempt=1&lwv=110",
                "POST",
                "https://www.facebook.com/",
                false,
                "lsd=" + token +
                "&email=" + username +
                "&pass=" + password + 
                "&default_persistent=0&timezone=-720" +
                "&lgndim=eyJ3IjoxNDQwLCJoIjo5MDAsImF3IjoxNDQwLCJhaCI6ODA1LCJjIjoyNH0%3D" +
                "&lgnrnd=" + lgnrnd +
                "&lgnjs=1442408093&locale=en_US&qsstamp=W1tbMjAsMjMsMzAsMzEsOTYsMTE3LDEyNiwxMjgsMTM4LDE1MywxODUsMTg2LDE4NywyMTIsMjIyLDI0MywyNDcsMjY5LDI3NywyODQsMjg3LDMxMiwzMTMsMzUzLDM4MCwzOTQsNDE2LDQzOSw0NjAsNDY4LDQ5MCw0OTEsNDk4LDUxMyw1MjEsNTQzLDU0OCw1NjMsNTg1LDYwNSw2MjcsODg5XV0sIkFabW9wclU0QTBjdXFFWWdNYlFPQ19hRklCdHNfWWZXMjA4MFRTSVIyNF9lcUVsa3k3aE04YUx1WmpsZFAxUTNPZWl6LU5tZEpUcXNuRHZaN0lXU2hwc05Ba0VYZXNnN0NRRXdNdGZ4Yl9NQy0wNVg3aThybDhSTUNubTRPaWVBbWZrUmRqeUlXZzNMRGhPd0oxazBwWlNkZnhBSENwdllUd3RGTGlDUUNRMDBGUlVNSTNndTVfOEJyZ1cwTE51dWJCV2pRVFpkdFlxWTJjekVOSHFjUi0zRlJCQTk3UmczRjdKQWRWUXJYREhPZ2pZVjNWdHkyNzRUWm5tMTM3QWN5R0EiXQ%3D%3D");


            if (respHtml.Equals("Found"))
            {
                setLogT("login succeed");
                gLoginOkFlag = true;
                return 1;

            }
            if (respHtml.Contains("Incorrect email or phone number")
                || respHtml.Contains("It looks like you entered a slight misspelling of your email or username")
				|| respHtml.Contains("The email you entered does not belong to any account")
			)
            {
                setLogT("username error!");
                return -1;
            }
			else if(respHtml.Contains("The password you entered is incorrect"))
			{
				setLogT("password error!");
				return -1;
			}
            else if (respHtml.Contains("Please enable cookies in your browser preferences to continue"))
			{
                setLogT("cookies error!");
				return -1;
			}

            return -2;
        }

        public int probe(string userId)
        {
            setLogT("probe " + userId + "..");

            string respHtml = weLoveYue(
       //         "https://www.facebook.com/" + userId + "?v=friends",
                "https://www.facebook.com/" + userId + "/friends",
                "GET", "", false, "");

            if (respHtml.Equals("wrong address"))
            {
                setLogtRed("用户id错误");
                return -2;
            }

            if (
                respHtml.Equals("Found")
                ||
                (respHtml.Length < 100000 && respHtml.Contains("class=\"uiHeaderTitle\">Favorites</h4>"))
                ||
                (respHtml.Length < 100000 && respHtml.Contains("Please enter the text below"))
                )
            {
                setLogT("session expired!");
                return -1;
            }

            if (respHtml.Length < 200001)
            {
                setLogtRed("无权限访问该用户好友列表");
                return -2;
            }

            rgx = @"(?<=""USER_ID"":"")\d+?(?="")";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				user_id = myMatch.Groups[0].Value;
			}

            rgx = @"(?<=\[\]\,{""revision"":)\d+?(?=\,)";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				revision = myMatch.Groups[0].Value;
			}

            respHtml = respHtml.Substring(60000, respHtml.Length-60000);
            rgx = @"(?<=friends_all"" aria-controls=""pagelet_timeline_app_collection_).*?(?="")";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				collection_token = myMatch.Groups[0].Value;
			}
            collection_token = ToUrlEncode(collection_token);

            rgx = @"^\d+(?=\%)";
			myMatch = (new Regex(rgx)).Match(collection_token);
			if (myMatch.Success)
			{
				profile_id = myMatch.Groups[0].Value;
			}

            respHtml = respHtml.Substring(140000, respHtml.Length-140000);
            rgx = @"(?<=https:\/\/www\.facebook\.com\/)(\w|\.)+?(?=\?fref=pb&amp;hc_location=\w+?"" tabindex=)";
            myMatch = (new Regex(rgx)).Match(respHtml);
            while (myMatch.Success)
            {
                iii++;

                //ignore all digital id
                Regex rex = new Regex(@"^(\.|\d)+$");
                if (rex.IsMatch(myMatch.Groups[0].Value))
                {
                    continue;
                }
                if ((gFriends.Count == 0) || (!gFriends.Contains(myMatch.Groups[0].Value)))
                {
                    gFriends.Add(myMatch.Groups[0].Value);
                    string pattern = @"^";
                    string replacement = "1-1-";
                    writeResult(Regex.Replace(myMatch.Groups[0].Value, pattern, replacement) + Environment.NewLine);
                    successInOneProbe++;
                }
                myMatch = myMatch.NextMatch();
            }

            rgx = @"(?<=pagelet_timeline_app_collection_" + collection_token.Replace("%3a",":") + @""",{""__m"":""\w+_\w+_\w+""},"").*?(?="")";
			myMatch = (new Regex(rgx)).Match(respHtml);
			if (myMatch.Success)
			{
				cursor = myMatch.Groups[0].Value;
			}
            else //just on one page
            {
                return 1;
            }
            cursor = ToUrlEncode(cursor);

            //page by page
            while(true){
                if (gForceToStop)
                {
                    break;
                }
                respHtml = weLoveYue(
                    "https://www.facebook.com/ajax/pagelet/generic.php/AllFriendsAppCollectionPagelet?data=%7B%22collection_token%22%3A%22"
                    + collection_token + "%22%2C%22cursor%22%3A%22" + cursor + "%22%2C%22tab_key%22%3A%22friends%22%2C%22profile_id%22%3A"
                    + profile_id + "%2C%22overview%22%3Afalse%2C%22ftid%22%3Anull%2C%22order%22%3Anull%2C%22sk%22%3A%22friends%22%2C%22importer_state%22%3Anull%7D&__user=" + user_id
                    + "&__a=1&__dyn=7AmajEyl2qm2d2u6aEB191qeCwKyWgyi8zQC-K26m6oKezob4q68K5Uc-dy88axbxjx27W88ybx-qCEWfybDGcCxC2e78"
                    + "&__req=g&__rev=" + revision
                    ,
                    "GET", "", false, "");

                //rgx = @"(?<=https:\/\/www\.facebook\.com\/)(\w|\.)+?(?=\?fref=pb&amp;hc_location=\w+?"" tabindex=)";
                rgx = @"(?<=facebook\.com\\\/)(\w|\.)+?(?=\?fref=pb\&amp\;hc_location=\w+?\\\"" tabindex=)";
                
                myMatch = (new Regex(rgx)).Match(respHtml);
                while (myMatch.Success)
                {
                    iii++;
                    //ignore all digital id
                    Regex rex = new Regex(@"^(\.|\d)+$");
                    if (rex.IsMatch(myMatch.Groups[0].Value))
                    {
                        continue;
                    }
                    if ((gFriends.Count == 0) || (!gFriends.Contains(myMatch.Groups[0].Value)))
                    {
                        gFriends.Add(myMatch.Groups[0].Value);
                        string pattern = @"^";
                        string replacement = "1-1-";
                        writeResult(Regex.Replace(myMatch.Groups[0].Value, pattern, replacement) + Environment.NewLine);
                        successInOneProbe++;
                    }
                    myMatch = myMatch.NextMatch();
                }

                rgx = @"(?<=pagelet_timeline_app_collection_" + collection_token.Replace("%3a", ":") + @""",{""__m"":""\w+_\w+_\w+""},"").*?(?="")";
                myMatch = (new Regex(rgx)).Match(respHtml);
                if (myMatch.Success)
                {
                    cursor = myMatch.Groups[0].Value;
                }
                else //to the end of friends list
                {
                    return 1;
                }
                cursor = ToUrlEncode(cursor);
            }
            return 1;
        }


        public void loginT()
        {
            if (!debug)
            {
                username = inputT.Text.Replace("@","%40");
                password = textBox1.Text;
                if (inputT.Text.Equals("") || textBox1.Text.Equals(""))
                {
                    setLogT("please enter username and password");
                    return;
                }
            }
            
            while (true)
            {
                if (rate.Text.Equals(""))
                {
                    Thread.Sleep(500);
                }
                else if (Convert.ToInt32(rate.Text) > 0)
                {
                    Thread.Sleep(Convert.ToInt32(rate.Text));
                }
                else
                {
                    Thread.Sleep(500);
                }

                int r = loginF();
                if (r == -3)
                {
                    continue;
                }
                if (r != -2)
                {
                    break;
                }
                
            }
        }

        public void autoT()
        {
            if (urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    deleteB.Enabled = false;
                });
                urlList.Invoke(sl);
            }
            else
            {
                deleteB.Enabled = false;
            }

            if (urlList.Items.Count == 0)
            {
                setLogT("empty user list! please import a userID file");
                if (urlList.InvokeRequired)
                {
                    delegate2 sl = new delegate2(delegate()
                    {
                        deleteB.Enabled = true;
                    });
                    urlList.Invoke(sl);
                }
                else
                {
                    deleteB.Enabled = true;
                }
                gForceToStop = false;
                return;
            }
            if (!gLoginOkFlag)
            {
                loginT();
                if (!gLoginOkFlag)
                {
                    if (urlList.InvokeRequired)
                    {
                        delegate2 sl = new delegate2(delegate()
                        {
                            deleteB.Enabled = true;
                        });
                        urlList.Invoke(sl);
                    }
                    else
                    {
                        deleteB.Enabled = true;
                    }
                    gForceToStop = false;
                    return;
                }
            }

            setLogT("开始扫描..");
            while (true)
            {
                for (int i = 0; i < urlList.Items.Count; i++)
                {
                    int r1 = 0;
                    while ((r1 = probe(urlList.GetItemText(urlList.Items[i]))) == -1)
                    {
                        gLoginOkFlag = false;
                        loginT();
                        if (!gLoginOkFlag)
                        {
                            if (urlList.InvokeRequired)
                            {
                                delegate2 sl = new delegate2(delegate()
                                {
                                    deleteB.Enabled = true;
                                });
                                urlList.Invoke(sl);
                            }
                            else
                            {
                                deleteB.Enabled = true;
                            }
                            gForceToStop = false;
                            return;
                        }
                    }
                    if (urlList.InvokeRequired)
                    {
                        delegate2 sl = new delegate2(delegate()
                        {
                            if (r1 == -2)
                            {
                                //red daiyyr
                                failed++;
                            }
                            else
                            {
                                urlList.SetItemChecked(i, true);
                                succeed++;
                                setLogT(" got from " + urlList.GetItemText(urlList.Items[i]) + ": " + successInOneProbe);
                                SUMsuccessInOneProbe += successInOneProbe;
                                successInOneProbe = 0;
                            }
                        });
                        urlList.Invoke(sl);
                    }
                    else
                    {
                        if (r1 == -2)
                        {
                            //red
                            failed++;
                        }
                        else
                        {
                            urlList.SetItemChecked(i, true);
                            succeed++;
                        }
                    }
                    

                    if (rate.Text.Equals(""))
                    {
                        Thread.Sleep(100);
                    }
                    else if (Convert.ToInt32(rate.Text) > 0)
                    {
                        Thread.Sleep(Convert.ToInt32(rate.Text));
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                    if (gForceToStop)
                    {
                        break;
                    }
                }//end of 'for' for checklistbox
                break;//just proce once.
            }
            setLogT(iii+"列表扫描结束，成功列表项: " + succeed + ", 失败列表项: " + failed + ", 共收集好友: " + SUMsuccessInOneProbe);
            succeed=0;
            failed = 0;
            SUMsuccessInOneProbe = 0;
            if (gFileName != null)
            {
                setLogT("Result in " + System.Environment.CurrentDirectory + "\\" + gFileName);

            }
            gFriends.Clear();
            if (urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    deleteB.Enabled = true;
                });
                urlList.Invoke(sl);
            }
            else
            {
                deleteB.Enabled = true;
            }
            gForceToStop = false;
            return;
        }

        private void loginB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(loginT);
            t.Start();
        }

        private void autoB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(autoT);
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
