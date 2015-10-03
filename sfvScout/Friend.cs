using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Net.Cache;
using System.Text.RegularExpressions;

namespace facebookWorm
{
    public class Friend
    {
        static string rgx;
        static Match myMatch;

        public List<string> gFriends = new List<string>();

        string gFileName = null;
        string collection_token = "";
        string cursor = "";
        string profile_id = "";
        string user_id = "";
        string revision = "";
        int succeed = 0;
        int failed = 0;
        int successInOneProbe = 0;
        int SUMsuccessInOneProbe;
        Form1 form1;

        public Friend(Form1 f) {
            form1 = f;
        }

        public int writeResult(string content)
        {
            if (gFriends.Count > 0)
            {
                if (gFileName == null)
                {
                    gFileName = "save_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", DateTimeFormatInfo.InvariantInfo) + ".txt";
                    form1.setLogT("Create file: " + System.Environment.CurrentDirectory + "\\" + gFileName);
                }
                Form1.writeFile(System.Environment.CurrentDirectory + "\\" + gFileName, content);

            }
            return 1;
        }

        public int probe(string userId)
        {
            form1.setLogT("probe " + userId + "..");

            string respHtml = Form1.weLoveYue(
                form1,
                //         "https://www.facebook.com/" + userId + "?v=friends",
                "https://www.facebook.com/" + userId + "/friends",
                "GET", "", false, "");

            if (respHtml.Equals("wrong address"))
            {
                form1.setLogtRed("用户id错误");
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
                form1.setLogT("session expired!");
                return -1;
            }

            if (respHtml.Length < 200001)
            {
                form1.setLogtRed("无权限访问该用户好友列表");
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

            respHtml = respHtml.Substring(60000, respHtml.Length - 60000);
            rgx = @"(?<=friends_all"" aria-controls=""pagelet_timeline_app_collection_).*?(?="")";
            myMatch = (new Regex(rgx)).Match(respHtml);
            if (myMatch.Success)
            {
                collection_token = myMatch.Groups[0].Value;
            }
            collection_token = Form1.ToUrlEncode(collection_token);

            rgx = @"^\d+(?=\%)";
            myMatch = (new Regex(rgx)).Match(collection_token);
            if (myMatch.Success)
            {
                profile_id = myMatch.Groups[0].Value;
            }

            respHtml = respHtml.Substring(140000, respHtml.Length - 140000);
            rgx = @"(?<=https:\/\/www\.facebook\.com\/)(\w|\.)+?(?=\?fref=pb&amp;hc_location=\w+?"" tabindex=)";
            myMatch = (new Regex(rgx)).Match(respHtml);
            while (myMatch.Success)
            {
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
            else //just on one page
            {
                return 1;
            }
            cursor = Form1.ToUrlEncode(cursor);

            //page by page
            while (true)
            {
                if (Form1.gForceToStop)
                {
                    break;
                }
                respHtml = Form1.weLoveYue(
                    form1,
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
                cursor = Form1.ToUrlEncode(cursor);
            }
            return 1;
        }


        public delegate void delegate2();


        public void startProbe()
        {
            if (form1.urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    form1.deleteB.Enabled = false;
                });
                form1.urlList.Invoke(sl);
            }
            else
            {
                form1.deleteB.Enabled = false;
            }

            if (form1.urlList.Items.Count == 0)
            {
                form1.setLogT("empty user list! please import a userID file");
                if (form1.urlList.InvokeRequired)
                {
                    delegate2 sl = new delegate2(delegate()
                    {
                        form1.deleteB.Enabled = true;
                    });
                    form1.urlList.Invoke(sl);
                }
                else
                {
                    form1.deleteB.Enabled = true;
                }
                Form1.gForceToStop = false;
                return;
            }
            Login login = new Login(form1) { };

            if (!Form1.gLoginOkFlag)
            {
                login.loginT();
                if (!Form1.gLoginOkFlag)
                {
                    if (form1.urlList.InvokeRequired)
                    {
                        delegate2 sl = new delegate2(delegate()
                        {
                            form1.deleteB.Enabled = true;
                        });
                        form1.urlList.Invoke(sl);
                    }
                    else
                    {
                        form1.deleteB.Enabled = true;
                    }
                    Form1.gForceToStop = false;
                    return;
                }
            }

            form1.setLogT("开始扫描..");
            while (true)
            {
                for (int i = 0; i < form1.urlList.Items.Count; i++)
                {
                    int r1 = 0;
                    while ((r1 = this.probe(form1.urlList.GetItemText(form1.urlList.Items[i]))) == -1)
                    {
                        Form1.gLoginOkFlag = false;
                        login.loginT();
                        if (!Form1.gLoginOkFlag)
                        {
                            if (form1.urlList.InvokeRequired)
                            {
                                delegate2 sl = new delegate2(delegate()
                                {
                                    form1.deleteB.Enabled = true;
                                });
                                form1.urlList.Invoke(sl);
                            }
                            else
                            {
                                form1.deleteB.Enabled = true;
                            }
                            Form1.gForceToStop = false;
                            return;
                        }
                    }
                    if (form1.urlList.InvokeRequired)
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
                                form1.urlList.SetItemChecked(i, true);
                                succeed++;
                                form1.setLogT(" got from " + form1.urlList.GetItemText(form1.urlList.Items[i]) + ": " + successInOneProbe);
                                SUMsuccessInOneProbe += successInOneProbe;
                                successInOneProbe = 0;
                            }
                        });
                        form1.urlList.Invoke(sl);
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
                            form1.urlList.SetItemChecked(i, true);
                            succeed++;
                        }
                    }


                    if (form1.rate.Text.Equals(""))
                    {
                        Thread.Sleep(100);
                    }
                    else if (Convert.ToInt32(form1.rate.Text) > 0)
                    {
                        Thread.Sleep(Convert.ToInt32(form1.rate.Text));
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                    if (Form1.gForceToStop)
                    {
                        break;
                    }
                }//end of 'for' for checklistbox
                break;//just proce once.
            }
            form1.setLogT( "列表扫描结束，成功列表项: " + succeed + ", 失败列表项: " + failed + ", 共收集好友: " + SUMsuccessInOneProbe);
            succeed = 0;
            failed = 0;
            SUMsuccessInOneProbe = 0;
            if (gFileName != null)
            {
                form1.setLogT("Result in " + System.Environment.CurrentDirectory + "\\" + gFileName);

            }
            gFriends.Clear();
            if (form1.urlList.InvokeRequired)
            {
                delegate2 sl = new delegate2(delegate()
                {
                    form1.deleteB.Enabled = true;
                });
                form1.urlList.Invoke(sl);
            }
            else
            {
                form1.deleteB.Enabled = true;
            }
            Form1.gForceToStop = false;
            return;
        }


    }
}
