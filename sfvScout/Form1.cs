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

namespace WHA_avac
{
    public partial class Form1 : Form
    {
        Thread gAlarm = null;
        string gnrnodeGUID = "";
        string gViewstate = "";
        string gViewStateGenerator = "";
        CookieCollection gCookieContainer = null;
        string user = "dudeea";
        string password = "Dd123456";
        string gVACity = "30",     //  30=guangzhou;29=shanghai;28=beijing
               gTitle = "MR.",
               gContactNumber = "",
               gEmail = "33333%40qq.com",//replace @ with %40 
               gFname = "ZHANG",
               gLastName = "XIAOMING",
               gMobile = "13900000000",
               gPassport = "55555555",
               gSTDCode = "0533",
               gDays = "5721"           //5721 means 2015.08.31, the number of days since 2000.01.01
               ;


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
            if (File.Exists(System.Environment.CurrentDirectory + "\\" + "urlList"))
            {
                string[] lines = File.ReadAllLines(System.Environment.CurrentDirectory + "\\" + "urlList");
                foreach (string line in lines)
                {
                    urlList.Items.Add(line);
                }
            }
        }

        public void alarm()
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(WHA_avac.Properties.Resources.mtl);
            player.Load();
            player.PlayLooping();
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
            FileStream aFile = new FileStream(file, FileMode.Create);
            StreamWriter sw = new StreamWriter(aFile);
            sw.Write(content);
            sw.Close();
        }

        public int downloadHtml(string url, string html)
        {
            string lastSection = "";
            string P = @"(?<=\/)[^\/]+?(?=$|\/$|\?)";
            Match found = (new Regex(P)).Match(url);
            if (found.Success)
            {
                lastSection = found.Groups[0].Value;
            }
            string fileName = lastSection + System.DateTime.Now.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo) + ".txt";
            writeFile(System.Environment.CurrentDirectory + "\\" + fileName, "URL:" + url + Environment.NewLine + "HTML:" + Environment.NewLine + html);
            return 1;
        }
        public int HtmlHandler(HttpWebResponse resp)
        {            
            string url = resp.ResponseUri.ToString();
            string html = resp2html(resp);
            if (html.Equals(""))
            {
                return -1;
            }
            string validHtml = "";
            string lastSection = "";
            bool have_APM_DO_NOT_TOUCH = false;
            string P = @"(?<=\/)[^\/]+?(?=$|\/$|\?)";
            Match found = (new Regex(P)).Match(url);
            if (found.Success)
            {
                lastSection = found.Groups[0].Value;
            }
            if (html.Contains("APM_DO_NOT_TOUCH"))//得到的是带JS乱码的页
            {
                have_APM_DO_NOT_TOUCH = true;
                P = @"(?<=</APM_DO_NOT_TOUCH>)[\s\S]+(?=$)";
                found = (new Regex(P)).Match(html);
                if (found.Success)
                {
                    validHtml = found.Groups[0].Value;
                }
            }
            else
            {
                validHtml = html;
            }
            validHtml = Regex.Replace(validHtml, @"<div id=""dateTime"">.+?<\/div>", "");
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            FileInfo[] allFile = dir.GetFiles();
            bool isNewContent = true;
            bool isNewURL = true;
            foreach (FileInfo fi in allFile)
            {                
                if (!fi.Name.Contains(lastSection))
                {
                    continue;
                }
                else
                {
                    string fileContent = System.IO.File.ReadAllText(fi.FullName);
                    string urlInFile = "";
                    P = @"(?<=URL:).+?(?=\r\n)";
                    found = (new Regex(P)).Match(fileContent);
                    if (found.Success)
                    {
                        urlInFile = found.Groups[0].Value;
                    }
                    if (!urlInFile.Equals(url))
                    {
                        continue;
                    }
                    else//找到url相同的文件
                    {
                        isNewURL = false;
                        string validHtmlInFile = "";
                        if (have_APM_DO_NOT_TOUCH)
                        {
                            P = @"(?<=</APM_DO_NOT_TOUCH>)[\s\S]+(?=$)";
                            found = (new Regex(P)).Match(fileContent);
                            if (found.Success)
                            {
                                validHtmlInFile = found.Groups[0].Value;
                            }
                        }
                        else
                        {
                            P = @"(?<=\r\nHTML:\r\n)[\s\S]+(?=$)";
                            found = (new Regex(P)).Match(fileContent);
                            if (found.Success)
                            {
                                validHtmlInFile = found.Groups[0].Value;
                            }
                        }
                        validHtmlInFile = Regex.Replace(validHtmlInFile, @"<div id=""dateTime"">.+?<\/div>", "");
                        if (validHtmlInFile.Equals(validHtml))//有效内容也相同，则认为页面无变更
                        {
                            isNewContent = false;
                            break;
                        }
                        else //有效内容不同，有可能与早期文件内容不同，与新文件相同，继续遍历
                        {
                            continue;
                        }
                    }
                }
            }
            if (isNewURL)//认为是新增的地址，进行第一次下载
            {
                downloadHtml(url, html);
                setLogT("new url: " + url );
                setLogT("page saved successfully");
                return 1;
            }
            if (isNewContent)//旧URL，且与所有文件内容均不同，下载文件，拉响警报！
            {
                downloadHtml(url, html);
                if (gAlarm != null)
                {
                    //gAlarm.Abort();                   
                }
                else
                {
                    Thread t = new Thread(alarm);
                    t.Start();
                    gAlarm = t;
                }
                setLogtRed("Attention! Page modified on " + url);
                return 2;
            }
            else
            {
                setLogT(url + " is unchanged");
                return 3;
            }
        }

        public void setRequest(HttpWebRequest req)
        {
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //req.Accept = "*/*";
            //req.Connection = "keep-alive";
            req.KeepAlive = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0";
            //req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E";
            req.Headers["Accept-Encoding"] = "gzip, deflate";
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Host = "www.immigration.govt.nz";
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
            req.ContentLength = postBytes.Length;
            Stream postDataStream = null;
            try
            {
                postDataStream = req.GetRequestStream();
                
            }
            catch (WebException webEx)
            {
                setLogT("GetRequestStream," + webEx.Status.ToString());
                return -1;
            }
            postDataStream.Write(postBytes, 0, postBytes.Length);
            //postDataStream.Close();
            return 1;
        }

        public string resp2html(HttpWebResponse resp)
        {
            string respHtml = "";
            char[] cbuffer = new char[256];
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
            return respHtml;
        }


        /* 
         * return success or not
         */
        public int weLoveMuYue(string url, string method, string referer, bool allowAutoRedirect, string postData)
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
            while (true)
            {
                if (method.Equals("POST"))
                {
                    writePostData(req, postData);
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
                setTestLog(req, respHtml);
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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = method;
            req.Referer = referer;
            if (allowAutoRedirect)
            {
                req.AllowAutoRedirect = true;
            }
            while (true)
            {
                if (method.Equals("POST"))
                {
                    writePostData(req, postData);
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
                    gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
                    setTestLog(req, respHtml);
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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            setRequest(req);
            req.Method = method;
            req.Referer = referer;
            if (allowAutoRedirect)
            {
                req.AllowAutoRedirect = true;
            }
            while (true)
            {
                if (method.Equals("POST"))
                {
                    writePostData(req, postData);
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
        getLoginHtml:
            setLogT("login..");
			weLoveYue("https://www.facebook.com/login.php?login_attempt=1&lwv=110\n",
				"POST",
				"??REFERER",
				false,
				"data?");
			//daiyyr


			string LoginUrl = "https://www.facebook.com/login.php?login_attempt=1&lwv=110\n";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(LoginUrl);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            string respHtml = "";
            req.Referer = "https://www.immigration.govt.nz/secure/Login+Working+Holiday.htm";

            if (
                writePostData(req, "TS0120d49b_76=0"
                + "&TS0120d49b_86=0"
                + "&TS0120d49b_cr=08eba48ebbab280005c3feeb3387f42e86443e509e659dd53e4f5f2d0d2c4ae01e32903a43b4077b598b8abd49931b42087c8733fa894800e5c1c4071f1e6ad87a6f25bc8ea65effbf3eb2f42056fe16b0b02dfbc94b881e12d829c30547783fa7788c4cac4afdd01c2fd58baa1fcb77ae19353d2c154926c5bb674877292c83"
                + "&TS0120d49b_ct=0"
                + "&TS0120d49b_id=3"
                + "&TS0120d49b_md=1"
                + "&TS0120d49b_pd=0"
                + "&TS0120d49b_rf=0"
                )
              < 0)
            {
                return -3;
            }

            
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("lgoin1 Timeout..");
                    goto getLoginHtml;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("lgoin1 NameResolutionFailure..");
                    goto getLoginHtml;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto getLoginHtml;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto getLoginHtml;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto getLoginHtml;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -3;
                }
            }

            if (resp != null)
            {
                respHtml = resp2html(resp);
                if (respHtml.Equals(""))
                {
                    goto getLoginHtml;
                }
            }
            else
            {
                goto getLoginHtml;
            }
            string tokenValP = @"(?<=;NRNODEGUID=).*?(?=&amp;)";
            Match foundTokenVal = (new Regex(tokenValP)).Match(respHtml);
            if (foundTokenVal.Success)
            {
                gnrnodeGUID = foundTokenVal.Groups[0].Value;
                setLogT("got token");
            }

            tokenValP = @"(?<=name=""__VIEWSTATE"" value="").*?(?="" />)";//after@, transfer" by""
            foundTokenVal = (new Regex(tokenValP)).Match(respHtml);
            if (foundTokenVal.Success)
            {
                gViewstate = foundTokenVal.Groups[0].Value;
            }

            tokenValP = @"(?<=__VIEWSTATEGENERATOR"" value="").*?(?="" />)";//after@, transfer" by""
            foundTokenVal = (new Regex(tokenValP)).Match(respHtml);
            if (foundTokenVal.Success)
            {
                gViewStateGenerator = foundTokenVal.Groups[0].Value;
            }

            resp.Close();

        post1:
            //1st post
            setLogT("login2..");
            string postUrl = "https://www.immigration.govt.nz/Templates/Secure/Login.aspx?NRMODE=Published&NRNODEGUID="+ gnrnodeGUID +"&NRORIGINALURL=%2fsecure%2fLogin%2bWorking%2bHoliday%2ehtm&NRCACHEHINT=Guest";
            HttpWebRequest req2 = (HttpWebRequest)WebRequest.Create(postUrl);
            setRequest(req2);
            req2.CookieContainer = req.CookieContainer;
            req2.Method = "POST";
            req2.Referer = "https://www.immigration.govt.nz/secure/Login+Working+Holiday.htm";
            if (
                writePostData(req2, "HeaderCommunityHomepage%3ASearchControl%3AtxtSearchString="
                + "&OnlineServicesLoginStealth%3AVisaLoginControl%3AloginImageButton.x=52"
                + "&OnlineServicesLoginStealth%3AVisaLoginControl%3AloginImageButton.y=15"
                + "&OnlineServicesLoginStealth%3AVisaLoginControl%3AuserNameTextBox="
                + user
                + "&OnlineServicesLoginStealth%3AVisaLoginControl%3ApasswordTextBox="
                + password
                + "&VisaDropDown=%2Fsecure%2FLogin%2BWorking%2BHoliday.htm"
                + "&__EVENTARGUMENT=&__EVENTTARGET="
                + "&__VIEWSTATE="
                + ToUrlEncode(gViewstate)
                + "&__VIEWSTATEGENERATOR="
                + gViewStateGenerator)
                < 0) 
            {
                return -3;
            }
        
            try
            {
                resp = (HttpWebResponse)req2.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("login2 Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("login2 NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -3;
                }
            }
            if (resp != null)
            {
                respHtml = resp2html(resp);
                if (respHtml.Equals(""))
                {
                    goto post1;
                }
            }
            else
            {
                goto post1;
            }

            if (respHtml.Contains("incorrect user name or password"))
            {
                setLogT("username/password error!");
                return -1;
            }
            else
            {
                setLogT("password verification OK");
            }

            if (req2.CookieContainer.GetCookies(req2.RequestUri)["ImmigrationAuth"] != null)
            {
                setLogT("got ImmigrationAuth");
            }
            else
            {
                setLogT("got ImmigrationAuth err");
                return -2;
            }
            gCookieContainer = req2.CookieContainer.GetCookies(req2.RequestUri);
            resp.Close();
            setLogT("login succeed");
            return 1;
        }

        public int probe(string url)
        {
            string lastSection="";
            string P = @"(?<=\/)[^\/]+?(?=$|\/$|\?)";
            Match found = (new Regex(P)).Match(url);
            if (found.Success)
            {
                lastSection = found.Groups[0].Value;
            }
            setLogT("post "+ lastSection +"..");
        post1:
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.immigration.govt.nz/SilverFern/";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }

            if (
                writePostData(req, "TS0120d49b_76=0"
                + "&TS0120d49b_86=0"
                + "&TS0120d49b_cr=08eba48ebbab2800e443817b4ffa4b06b983fe9ef7704e54a006aedadf1ecdb6d52731a0895708329e5c6c247eb39b7c08fe389b03894800809f0e86c6d5b2ada4bb53965d3352a581e9186cbb7669fe51daefec752eef50b4fb649cf368df61b0872e03ebc2cbd87197735b6f2ce8cc47157f9998644844e2ae018981e37a89"
                + "&TS0120d49b_ct=0"
                + "&TS0120d49b_id=3"
                + "&TS0120d49b_md=1"
                + "&TS0120d49b_pd=0"
                + "&TS0120d49b_rf=0"
                )
                < 0)
            {
                return -2;
            }
            string respHtml = "";
        
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                } 
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.Redirect)
            {
                setLogT("session expired!");
                return -1;
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                HtmlHandler(resp);
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }

        /*
         * Schedule an Appointment
         */

        public int create()
        {            
            setLogT("post create..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppWelcome.aspx?p=Gta39GFZnstZVCxNVy83zTlkvzrXE95fkjmft28XjNg%3d";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppWelcome.aspx?p=Gta39GFZnstZVCxNVy83zTlkvzrXE95fkjmft28XjNg";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req,
                "__EVENTTARGET=ctl00%24plhMain%24lnkSchApp&__EVENTARGUMENT=&__VIEWSTATE=y4H3v3Vf%2FKZIy3YqiTkuKz7SPKXXlEpmuZ6GTMaXlw"
                +"%2FHKbSmzf7waL3xHoK95pzY2hGK3rQDoi0q42%2FCvrGonzU4V1igrr2pJTSqGmr3ZQ2FwazRCivxeWczISS4s8cW2oncbOVUDW"
                +"iKT2sNb81jqPJmJE8JM5q7FHALaX6u15kv63n1leTZCklLZEAmHtv3lfm421fOeGjgcyRXH2ZgQ33nptPQGy62wi4gLMCwtrZoUehF"
                +"%2BVdo%2FxRXQdIoGl4fWucqgyMPdm3Dt3OzWBkr1ITCjCCNf%2B5Q5VpQzGY%2FeF0I4cTDQYu2Fk7Sy%2Fv8DL%2B30q3joC3mY"
                +"%2BmIuYz7atZ9oPYaonhVsC8%2F0X0CxEB2%2BMI4WlifqjcMuFQbd9Bn81oFWAF0oeYDqYbtcod5TC98aBzx%2FI5yviLBP6%2BUsQ09s5NtLuwCF33GfnbeqS91t6YrUacR1ZcQEYzXMrgzbt2JtIR8SP"
                +"%2Bmje%2F9xQfALETYlRPiejibFjMfwVrlGc2%2BSWg1zahT3Sn8gdQ%3D&____Ticket=1&__EVENTVALIDATION=2VOscnt3wUrYVEmbEBdVVV"
                +"%2BecrUfaQrd3%2FYGJ0r9LAEFBQ%2F%2BIAGAtGVyE4iBprD%2Bio9a0iKK8SGy0TT0hF2vJQ%3D%3D")
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }


        public int selectLocation()
        {
            setLogT("post selectLocation..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppScheduling.aspx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppWelcome.aspx?p=Gta39GFZnstZVCxNVy83zTlkvzrXE95fkjmft28XjNg";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req, "__VIEWSTATE=U8AJVYNixXRfz4bH8v8%2F0vyB2azTOxRhlu62TVP4Amy7PraT6FvK3uGzIJqpRnwHPLQBDjit0Tjqobj9c3TrNCXUsyOncX0WxstNd60kTj8"
+ "%2Bd2aNdNAHhWwFQbihaPgQt5lqYnaTge7vlpLbWpGs1joqc1zDofYD9mVpEFI%2FO2z%2Bek3MI8aSix%2FDSg5erl%2B8uRJ1JwBoHBwR2so02sjNNZGjkrCqF8m6WqbVdzjMAnEEhrSuy7sSn"
+ "%2Fpfy54zWWFpQpBwD1OXAtltLg1C%2FT5KV5tpWKQHxmuq4JXjIQ4EPdT%2BSZFl9taV2DiZDT3X0kkl%2FyxDYRbAo0OU88hhWUeJf"
+ "%2BMdRSC7C6y3pzBtisn2c10P9Dk6t%2FZxewskMJAIsnN5a7cAQr3%2BVEWgDVMdZBow0Ylr7q6CFokZaUVebCMLBTFOnvnI9Zjbxg"
+ "%3D%3D&ctl00%24plhMain%24cbo"
+ "VAC=" + gVACity
+ "&ctl00%24plhMain%24btnSubmit=%E6%8F%90%E4%BA%A4&ctl00%24plhMain%24hdnValidation1"
+ "=%E8%AF%B7%E9%80%89%E6%8B%A9%EF%BC%9A&ctl00%24plhMain%24hdnValidation2=%E7%AD%BE%E8%AF%81%E7%94%B3%E8"
+ "%AF%B7%E4%B8%AD%E5%BF%83&ctl00%24plhMain%24hdnValidation3=%E5%B1%85%E4%BD%8F%E5%9B%BD&____Ticket=2&__EVENTVALIDATION"
+ "=Vl39F6qwZOICJpTa9PcH0gV%2FyeRGOfg5uQqROs1LRKtDZsxgCLg9LzkK%2F0bKajvKLYu88iUHiiiQQjV%2FynffMgplscVinn0GMf5vgACt66c"
+ "%3D")
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }

        public int selectType()
        {
            setLogT("post selectType..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingGetInfo.aspx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingGetInfo.aspx";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req, "__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=U8AJVYNixXRUwcXUTmqrf7zAU4hEbOgeUh39xR6xe%2Ft7RBN"
+"%2BatU43OaTXZpQSjNY8%2BVbFRnhLbL8GKP7XABq1YaErm%2B4SpFT2KZuLwVM4nPGsea8q%2FeCfvd%2F7REf2AN0qrIWP41Q7Ci2GJ5km7B"
+"%2BTj0Of4uPtiTFviciojgqAaiJk6QUz7SjB0Kf1Pwbaq58A0R5YHTMEFGrPA70l3sudHH3yb5T3jbsRpBBoXMYtCNz1lgy4fzmXo1xTMEbdFEfRAH"
+"%2BGqY4utRjTpf3hhCxLTVCNM1F9Cf%2FUZhFqxtmjtBUgrSJEs4QrWBEgaMhgI29WAsm1DfqrBAhzeVuaxHpyq4l7pN49umb07dVPZLmSLk"
+"%2FtujFJ91uOkLTdzXDPoxtFCqaMUdfM0x9iEDYwDko47wf9xiTl3JRFGYcpVDRFzFtlu9VekNLQzGtiUal%2F%2FfwUevuu4WrCLvQnFSR5b85"
+"%2Fx8WoEobIp1j9ur40h8ldh%2FKxfovLNtWAl%2Fg5rZlHeOd6vU9aEnxn2EZv2ROEONsNDGd6bN6VhEkBoLW1zAjG%2B1t5SelnIeFCq0IERuNZFbcDWf7"
+"%2BG3h8Wdkqkm5wAcdvj%2FdIp7UkCcWdSMImqiXky%2FKdQgsr4bwLJnz30kK2ugxKp7xmuTRxciPgUon5Yb0rEdkYhGax9YQayZi0XG7ZtJzpZrK3fHVu4xjMcwxx6qmv7ltoTbWPIyq2TU5ElQz0zJbstGo2msU4lmBwC6CcLuD3zkJJtadJ65waQ8V2"
+"%2FiBlGHGlIhe1%2FofgahynsoVroQ6xs2ixsNxY%2BqDEUZMeWkzqbmV7urhUncKVO7A%2F3I6%2FVoKudxB32%2FRGgWn0JDZksn3T"
+"%2FLjcEdjXbfeuZTXNsY4RX1KkaaWexAlria6D0ougaRkLzZ%2BNuLViQS5579XUwbe11ZhW52%2FvnRS%2FMejWh172fWzGvl0hVyUiPSkx"
+"%2B4380uD5na5%2F2pOVlsfq%2BS4oqsQgiQfDehJcaZ7bco29SR2vGHIpusT1lNInltj8mYIlta6eWIQpjfthTnQCO%2B9Id5f4yCpf7lV7vQ9s9BrbkGPm2c"
+"%2BllCs6HuH28TZ61w8K5UIk9W18cdLT8fd6EKaiigwTLuMlYJz644I1ckubvqz3Io34l%2Fs8YW2o4XP790vToplHFBzShu6fJ%2BDc6UC6EnQ"
+"%2BdbRwxp7T5RYdp3pV4chRa7u74B0m5cBdXWw1QR4pn4szRnLn%2FQ0S%2BxrQlMfJCg0nOkazcJxJw0myYMBzqjMz5LqPyntix5qbCU"
+"%2BJptjClwe%2F45yFogTAu9RInEiX%2FVwyoZ9WB05nOdWAoPXQfi%2BrTzXv%2BoTRw%3D%3D&ctl00%24plhMain%24tbxNumOfApplicants"
+"=1&ctl00%24plhMain%24cboVisaCategory=13&ctl00%24plhMain%24btnSubmit=%E6%8F%90%E4%BA%A4&ctl00%24plhMain"
+"%24hdnValidation1=%E8%AF%B7%E8%BE%93%E5%85%A5%EF%BC%9A&ctl00%24plhMain%24hdnValidation2=%E6%9C%89%E6"
+"%95%88%E4%BA%BA%E6%95%B0%E3%80%82&ctl00%24plhMain%24hdnValidation3=%E6%8A%A5%E5%90%8D%E4%BA%BA%E6%95"
+"%B0%E5%BF%85%E9%A1%BB%E4%BB%8B%E4%BA%8E1%E5%92%8C++&ctl00%24plhMain%24hdnValidation4=%E7%AD%BE%E8%AF"
+"%81%E7%B1%BB%E5%88%AB&____Ticket=8&__EVENTVALIDATION=LgGliIkkUxxirzupetnKohQ9X2Ck3WdINu939id2MnTg091fU"
+"%2F6UXnEZnwn63l2JoJ4tIPqrdbsBNufKc6PbfYIhfNKAdbB1%2BwfWRGYwopZfspcAb4K04xbR0Uk0tEMx")
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }


        public int fillInDetails()
        {
            setLogT("post fillInDetails..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingVisaCategory.aspx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingGetInfo.aspx";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req, "__VIEWSTATE=WeAT97QH0avIEHaB5uNheUxvgfsGBJeySDR%2BhmFKuZ2qnOYUkFBX%2B96Q8wMpOhOSdrvAfS65N3kIscxaAqNozuKasxwdnKIQi7FJZHD1IjkljVI9t1AmYDfOu"
+ "%2FXygUero2%2FfblS4TK1N%2FAGIVoWnkSWmXgr8RAEGTius35BLfCOsc27Tu%2BYrpxjhM9sPG2OWv%2Bo2T8szuBum8wkWrB%2BQ1UXi"
+ "%2Ft7Py8qOXStxE2jQ79IiGfoKc%2FTZ%2FVdFNtwgybKTLyEINle8KsJb7XkjeaVvkYXPA2nwErgv5L8zJ%2BlIx9xssAai3qgooQxB1uxoG6JOpANA"
+ "%2Flo%2BTk38yAoNg04%2BqDHosynFp1TsaY7VpszHnZX3QCceB4h2TYr94uEpYtMibWoTPUvpKtcD8va2a6SAau5nkMdUDq%2FtmEwDMHvA0Qo0oBtjWCjc6kKvD3m86v15l9L8IE4ZUaykUJtbF3wEpaCTHEB56K"
+ "%2Bh4SvaU%2B9H3DmAsiLkG%2BeXm6wGxt9NhamIWm1N5ASb0NyhTaYXcxtQq%2FBbptsBMcpFaz8NsapfoVxHPHy%2BU40dsY6L8PC2d07OD3ulzlqmc"
+ "%2BUJV4nBX5td1VH4ha63CUfJz47CLEveg2oAqY52C5wA5y2WMefNCdRYa9PMV0O2aSe6UagqXzfPj04zTFWeP08%2BfOg8tZR7hdA0DdNseogNaaCAIVJVIi"
+ "%2BamjFmJCPxxe9BOh3Dwfc9zLeCNW%2FHsk0RfEHOD8usvDdJpXdw9ys8HY8%2BQLs%2BkitAlDzLD9VMpGSH%2BCgOjgVnSegb1wdXprg0BiNBziuo"
+ "%2FNi0VdRf8H8KyGmXTKnV44BLh6vrKvZjnDrZpvVrB%2FKxU8b4ixNUho4sgvwgACwTjLmBFlyV5aXyZEvvgq%2B8cXWL%2Fj6fGm5C"
+ "%2BNvedysqZOqCBxmmSRuyhW0lWY7rxH4sZoEhdOK5MY3d%2FYkJuhp4%2FzLwCo8sB%2BlIVHwT3WXh348InV1UQ14SeiYLz86LmHmf2vjkPgdJ11BpMyCcNRMCrzaJ"
+ "&ctl00%24plhMain%24repAppVisaDetails%24ctl01%24tbxPassportNo="
+ gPassport
+  "&ctl00%24plhMain%24repAppVisaDetails"
+ "%24ctl01%24cboTitle=" 
+ gTitle
+ "&ctl00%24plhMain%24repAppVisaDetails%24ctl01%24tbxFName="
+ gFname
+ "&ctl00%24plhMain"
+ "%24repAppVisaDetails%24ctl01%24tbxLName="
+ gLastName
+ "&ctl00%24plhMain%24repAppVisaDetails%24ctl01%24tbxSTDCode="
+ gSTDCode
+ "&ctl00%24plhMain%24repAppVisaDetails%24ctl01%24tbxContactNumber="
+ gContactNumber
+ "&ctl00%24plhMain%24repAppVisaDetails"
+ "%24ctl01%24tbxMobileNumber="
+ gMobile
+ "&ctl00%24plhMain%24repAppVisaDetails%24ctl01%24tbxEmailAddress="
+ gEmail
+ "&ctl00%24plhMain%24btnSubmit=%E6%8F%90%E4%BA%A4&ctl00%24plhMain%24hdnValidation1=%E8%AF%B7%E8"
+ "%BE%93%E5%85%A5%E7%94%B3%E8%AF%B7%E4%BA%BA%E7%9A%84%E6%8A%A4%E7%85%A7%E5%8F%B7%E7%A0%81%E3%80%82&ctl00"
+ "%24plhMain%24hdnValidation2=%E8%AF%B7%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E9%80%89%E6%8B%A9"
+ "%E6%A0%87%E9%A2%98%E3%80%82&ctl00%24plhMain%24hdnValidation3=%E8%AF%B7%E8%BE%93%E5%85%A5%E7%BB%99%E5"
+ "%AE%9A%E5%90%8D%E7%A7%B0%E7%9A%84%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3%80%82&ctl00%24plhMain"
+ "%24hdnValidation4=%E8%AF%B7%E8%BE%93%E5%85%A5%E5%A7%93%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3"
+ "%80%82&ctl00%24plhMain%24hdnValidation5=%E8%AF%B7%E8%BE%93%E5%85%A5%E6%89%8B%E6%9C%BA%E5%8F%B7%E7%A0"
+ "%81%E3%80%82%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3%80%82&ctl00%24plhMain%24hdnValidation6="
+ "%E8%AF%B7%E8%BE%93%E5%85%A5%E6%9C%89%E6%95%88%E7%9A%84%E7%9A%84STD%E4%BB%A3%E7%A0%81%E4%B8%BA%E7%94%B3"
+ "%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3%80%82&ctl00%24plhMain%24hdnValidation7=%E8%AF%B7%E8%BE%93%E5"
+ "%85%A5%E6%9C%89%E6%95%88%E7%9A%84%E7%94%B5%E5%AD%90%E9%82%AE%E4%BB%B6%E5%9C%B0%E5%9D%80%EF%BC%8C%E7%94"
+ "%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3%80%82&ctl00%24plhMain%24hdnValidation8=%E8%AF%B7%E5%AF%B9"
+ "%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E8%BE%93%E5%85%A5%E6%9C%89%E6%95%88%E7%9A%84GWFNo%E7%9A"
+ "%84%E3%80%82&ctl00%24plhMain%24hdnValidation9=%E8%AF%B7%E9%80%89%E6%8B%A9%E7%AD%BE%E8%AF%81%E7%B1%BB"
+ "%E5%88%AB%E7%9A%84%E7%94%B3%E8%AF%B7%E4%BA%BA%E6%B2%A1%E6%9C%89%E3%80%82&____Ticket=5&__EVENTVALIDATION"
+ "=IRo%2B19B8GWQ0OsjXOo0KEnDN30QgiISDxFhl%2B5xNoJsaPrL5SnVxA71owC9Ut3NCv8sN8DQyGEJvoj5zZZBUfvm6DJ4TjRHEgxWcpAhD09ilgZzdNg0AZcbPXYvWZBlkTYkBvOnDCR9d8zC"
+ "%2FHCuFZY5%2BLho4j256oqb2KcKZpHUqK95b2bfsbS8uK%2FM0fsrtm9ocbEzVS3uK9HwxuQccwA%3D%3D")
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }

        public int pickDate()
        {
            setLogT("post pickDate..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingInterviewDate.aspx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingVisaCategory.aspx";
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req, "__EVENTTARGET=ctl00%24plhMain%24cldAppointment&__EVENTARGUMENT="
+ gDays
+ "&__VIEWSTATE=U8AJVYNixXSq9VLDbRaZ8nMA"
+ "%2BMPSDsEGivqjUsubuqrcpK53JEiQp99OoAyRN33bd%2Fmkv6DPLEOZox163uZqP6i1E4ngRhMmu7EYUfGh7SW19Ih1rZ86kozkMPBIkYfjqEdgOriF4ijbK63KhfrSkKRUhfQUQyUUuC9JDQsdmcdO0Y7"
+ "%2BAUppCiZZlbGgdvCkbaWCUQGjwaNrorzaOXsKt6naeLs3YOVYHs14cg7P5SCCi0aHmpO8pMcI0bvNVOwtV333bmzvfpr3Kcv866D56yEAuErEQq43C08Swqmx754IBnqw0Z4tMMaojg8PN9w"
+ "%2BpuMySi%2B9l3oGMQDOs%2FoGFtQOt7QFtPeEWCCK0hnU8YaKln6K37yL%2BzFkLyq5EL8RNlVGTZgz7e2v%2Fo9sYQz8x%2FVOQjjZ3jtWtcF9bJQyCU38iKjPabJa0FNOJVI64XS08P8h2MeuqqBdqFagMGorkOBsDxIY5rPF1EEnKgSloH"
+ "%2Fn8FurXHDR1fI7BypOaAHscFSGVE%2FM5p3QTbsnkrvVV1suHPCzkB8vdkS6jPW8RH3Shr7puQTWhODtg6QeLMec%2BsKfqSaUoF0KnP4Rxyt6bdoi"
+ "%2FrVm4SuWRqjcfkAunmXPh1nk%2BnaLMjDMKOVZsuklOCv6pjkgdmpvLRDfepzqHVpde0E9Xd5wT6r6nnzHebTnQBSEVpi%2BTBkGvtbNhibIOXKbAQEaRyx8tHH4ZV2Fs6FZ7MHhwzrR4cFc4AHiA"
+ "%2Fv%2BFsFF2JwxIwf5wUC46goo9YMiM%2B7w4eWKFnvOXOeyMNqSUN2VqaqoQ5AAYgmQ%2B1VvFjlBpc%2FTYfy76W89%2FFt3traK9pFszwzkL1GUCL0qToKFRlFMuRtEgA3515dPcKkSpkv3bqqEj1jws"
+ "%2BJYdF8w3ENSh2rNm%2FGFzTxN0QMlnfXU4E60kA8ctXf9twe9cvWszNZxSZaD8H0jqkDafd4%2BcBCd9P%2F2MWF9X6u6mQBFILjdegtPa1udgvEMQ"
+ "%2Fq3ORcRxa0O0mQ7RwYvpV8E%2B%2Bp5Qm9LhwTwpIXMfhmlh%2FAYRXjtz7PtaXmKnSv%2FSYv9m655pXWGKdULeC%2Bpl31NJyK0BvexdQ3Jw2ARueXSKkfiaeQ"
+ "%2B3bd59wZnqA3N5ghq5PBSlzD0%2Fexjtyn0zZx%2Fa1OblwHiQ2TtLndSutRxC%2FlMTO5Ms3nxkEkVtWvjF0CBjI4GYqOoquehX0X1N37htIbMYdzwQB9"
+ "%2FqIN%2Bo5dDZ04T4Kg0KLeHlWZbkgKQMva2e1hxhD9Nin5BST6RwiJpk7WyWOyaCSAIFrvMvQONTr74ummW3KX8CWjF8wkTyh7Cf5yt53xqvHsxnHculsA5zFDfW5RN3jYEQSRM55Q3Uurh7Qx1l5wuTl60HjfyQHkNDadlhNeM"
+ "%2BH0RaRfKX2KGDrMKO1bZJ2tIjDEaL%2FPDH6%2BpZfg48btx6IgaRzLdtIql44JqNH98L2QrDaySRzYqPmCM4Kqg3Lv7LB2UuX"
+ "frpUGniYYIqObxFeLT6bcDtYmwYov5t1yhmS2hTGOPuJyW5UIEYaovfGNj6qluQbEo6iszcj2AsYngPunNX68em9Nl9feehoVEgerTD6XTzA4j0Tm9f5NHuW37j9pGRoJuXedjV6YuT"
+ "%2BeXZ%2F0SKG9lyTYxA1%2BwXlxURYQL7rqrXyNr7wx2BSDcolBFd3TDHfEaDtJ%2F2LoDGqeGtqrnEmRu56tVKqKLqMw%2FOhZ11pBxvyGgq4ohxIH3YUZttTKVR9ppC8dRP2L"
+ "%2Fiuqnr2fyNg1JP0M2ch%2FiSsupxAN%2FvZohNnCsFqRd9IpT5zrGBisXdAcgTxWU6%2BWHEfLjuL9B66NmlRXpA%2FlcKWhbh9vkgPs0FbJj5ey6c9PAx90o68vbLiDipMHVNZLPJcv0Oc"
+ "%2F4Ejwzfm%2FmnLS3WkMXm67vsA%3D%3D&____Ticket=6&__EVENTVALIDATION=5u6H2uzLeMjuSNA%2FL1X%2BYh9C%2BpKtOLIEP3GLFHkoPO65OWB"
+ "%2FexoIoD6a%2BmOOgbpKHCVQOf2ttLi%2FdaB61bgwKo5mPKjQw3lejTdS39kZuoSRGuTRvh8e1Jm%2F89Dsn7yFIDU0MXA1IlqVZWU4j6BbNiFuh2C"
+ "%2F%2BE0hF0WECyWfYuZw7Zb%2BxyEt3571jSWeS1JzjkHovW7D9FOgw3Xk%2FnGr%2FlvUwjVlSYZVHBwhywU2xlXjCdQxCC7bHE7WXzrIDe0VgRZJxwLxXwAnwhtyBUCVVaqK4"
+ "%2BKfm11XcSAhRHKQKrwpdooVj%2F9lxgKefXwobAgfKd71vrmXHyx1OOV2jxUSsbJ%2Fn4J4tN%2FJ%2BwTjobYWnjcLdsin5Cj"
+ "s8ryyOyGxtAjCw15hDBn7ZgkoioJq0vAV0z0k9WMfsd1ZeaZACLkwCjWJypHjXnDzFRAGie9yTFfR")
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }

        public int pickTime()
        {
            setLogT("post pickTime..");
        post1:
            string url = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingInterviewDate.aspx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            setRequest(req);
            req.Method = "POST";
            req.Referer = "https://www.visaservices.org.in/DIAC-China-Appointment/AppScheduling/AppSchedulingInterviewDate.aspx";                           
            if (gCookieContainer != null)
            {
                req.CookieContainer.Add(gCookieContainer);
            }
            if (
                writePostData(req, "")//dyyr; To be continued
                < 0)
            {
                return -2;
            }
            string respHtml = "";

            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    setLogT("post Timeout..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    setLogT("post NameResolutionFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.UnknownError)
                {
                    setLogT("post UnknownError..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    setLogT("post ConnectFailure..");
                    goto post1;
                }
                if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                {
                    setLogT("post ConnectionClosed..");
                    goto post1;
                }
                else
                {
                    setLogT("other WebExceptions..");
                    return -2;
                }
            }
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string html = resp2html(resp);//dyyr
            }
            else
            {
                goto post1;
            }
            //gCookieContainer = req.CookieContainer.GetCookies(req.RequestUri);
            resp.Close();
            return 1;
        }

        public void loginT()
        {
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
            loginT();
            while (true)
            {
                for (int i = 0; i < urlList.Items.Count; i++)
                {
                    if (probe(urlList.GetItemText(urlList.Items[i])) == -1)
                    {
                        loginT();
                    }
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
                }
            }
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
        public void addURL()
        {
            //string P = @"^http(s)?:\/\/([\w-]+\.)+[\w-]+$";//无法匹配下级页面
            string P = @"^(https?|ftp|file)://[-a-zA-Z0-9+&@#/%?=~_|!:,.;]*[-a-zA-Z0-9+&@#/%=~_|]";
            Match M = (new Regex(P)).Match(inputT.Text);
            if (M.Success)
            {
            }else{
                MessageBox.Show("invalid url!");
                return;
            }            
            if (urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    urlList.Items.Add(inputT.Text);
                    inputT.Text = "";
                });
                urlList.Invoke(sl);
            }
            else
            {
                urlList.Items.Add(inputT.Text);
                inputT.Text = "";
            }
            string strCollected = string.Empty;
            for (int i = 0; i < urlList.Items.Count; i++)
            {
                if (strCollected == string.Empty)
                {
                    strCollected = urlList.GetItemText(urlList.Items[i]);
                }
                else
                {
                    strCollected += "\n" + urlList.GetItemText(urlList.Items[i]) ;
                }
            }
            writeFile(System.Environment.CurrentDirectory + "\\" + "urlList", strCollected);
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
        }

        private void addB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(addURL);
            t.Start();
        }

        private void deleteB_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(deleteURL);
            t.Start();
        }
    }
}

