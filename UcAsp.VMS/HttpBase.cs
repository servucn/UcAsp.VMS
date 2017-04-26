/***************************************************
*创建人:TecD02
*创建时间:2017/1/4 14:46:25
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
namespace UcAsp.VMS
{
    public class HttpBase : IDisposable
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(HttpBase));
        public TcpListener _server;
        public string _httpversion;
        public string _url = string.Empty;
        public string _mimetype = string.Empty;
        public const int buffersize = 1024;
        public int WarehouseId = 0;
        public string Url;
        public string Http;
        public string ConfigPath = string.Empty;
        private bool stop = false;

        public string LastMethod = string.Empty;
        public DateTime LastRunTime;
        public string LastError = string.Empty;
        public string LastParam = string.Empty;

        public Dictionary<string, HttpSession> Sessions = new Dictionary<string, HttpSession>();
        public Dictionary<string, string> Header = new Dictionary<string, string>();
        public Dictionary<string, string> Request = new Dictionary<string, string>();
        private int SessionTimeOut = 20;
        private Timer timer;
        public void StartListen(int port)
        {

            _url = string.Format("{0}:{1}/", Url, port);
            Http = _url;
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start(3000);
            ThreadPool.QueueUserWorkItem(AcceptSocket, null);
            timer = new Timer(new TimerCallback(timer_Callback), null, 30000, 300000);
        }
        public virtual void timer_Callback(object sender)
        {
            CleanupSession();
        }
        public void AcceptSocket(object obj)
        {
            while (true)
            {
                try
                {
                    if (stop)
                        break;
                    Socket client = _server.AcceptSocket();
                    Listen(client);
                    client.Close();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    break;
                }
                finally
                {

                }

            }
        }
        public virtual void Listen(object obj)
        {
            string sDirName;
            String OutMessage = string.Empty;

            Socket socket = (Socket)obj;
            socket.ReceiveTimeout = 10000;
            socket.SendTimeout = 10000;
            string content = string.Empty;
            if (socket.Connected)
            {
                try
                {
                    string strbuffer = string.Empty;
                    LastRunTime = DateTime.Now;
                    while (true)
                    {

                        Byte[] bReceive = new Byte[buffersize];
                        int i = socket.Receive(bReceive, bReceive.Length, 0);
                        //转换成字符串类型
                        strbuffer = strbuffer + Encoding.UTF8.GetString(bReceive).Substring(0, i);
                        Console.WriteLine("read:");
                        if (i - buffersize < 0)
                        {
                            break;
                        }
                    }
                    _log.Error(strbuffer);
                    if (string.IsNullOrEmpty(strbuffer))
                    {
                        HttpRespone.SendError(_httpversion, ref socket);
                        return;
                    }
                    Header = HttpRespone.RequestHeader(strbuffer).Item1;
                    Request = HttpRespone.RequestHeader(strbuffer).Item2;
                    AddSession();
                    _url = Header["Host"];

                    _httpversion = Request["version"];
                    // 得到请求类型和文件目录文件名
                    sDirName = Request["path"];
                    if (!sDirName.EndsWith("/")) sDirName = sDirName + "/";

                    string[] Route = sDirName.Split('/');

                    LastMethod = sDirName;
                    Console.WriteLine(sDirName);

                    Regex r = new Regex("\r\n\r\n");
                    string[] Code = r.Split(strbuffer);
                    content = Code[1];
                    LastParam = content;
                    if (Header.ContainsKey("Content-Length"))
                    {

                        int len = int.Parse(Header["Content-Length"]);
                        while (content.Length < len)
                        {
                            Byte[] bReceive = new Byte[len];
                            int i = socket.Receive(bReceive, bReceive.Length, 0);
                            strbuffer = strbuffer + Encoding.UTF8.GetString(bReceive).Substring(0, i);
                            LastParam = content = strbuffer;
                            _log.Error("Content-Length：" + len);
                            if (Encoding.UTF8.GetBytes(content).Length - len == 0)
                            { break; }
                        }

                    }
                    HttpRespone.RequestForm(content);
                    if (HttpRespone.Cookies.ContainsKey("UcAsp.Net_SessionId"))
                    {
                        string SessionId = HttpRespone.Cookies["UcAsp.Net_SessionId"];
                        if (Sessions.ContainsKey(SessionId))
                        {
                            Sessions[SessionId].RefreshLastTime();
                        }
                    }
                    Action(socket, content, Route);
                }
                catch (Exception ex)
                {
                    SendJson(socket, "false");
                    _log.Error(ex);
                }
                finally
                {
                    // socket.Close();


                }

            }
        }

        public virtual void Action(Socket socket, string content, string[] Route)
        {

        }
        public virtual void SendCode(Socket socket, string code)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(code);
            HttpRespone.SendHeader(_httpversion, "text/html", buffer.Length, " 200 OK", ref socket);
            HttpRespone.SendToBrowser(buffer, ref socket);
        }

        public virtual void SendJson(Socket socket, string code)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(code);
            HttpRespone.SendHeader(_httpversion, "text/html", buffer.Length, " 200 OK", ref socket);
            HttpRespone.SendToBrowser(buffer, ref socket);
        }

        public virtual void AddSession()
        {
            if (!string.IsNullOrEmpty(HttpRespone.SessionId))
            {
                if (!Sessions.ContainsKey(HttpRespone.SessionId))
                {
                    if (HttpRespone.Session == null)
                    {
                        HttpRespone.Session = new HttpSession();
                        HttpRespone.Session.SessionId = HttpRespone.SessionId;

                    }
                    Sessions.Add(HttpRespone.SessionId, HttpRespone.Session);

                }
                else
                {
                    Sessions[HttpRespone.SessionId].RefreshLastTime();
                }
            }
        }
        public virtual void CleanupSession()
        {
            List<string> ids = new List<string>();
            foreach (KeyValuePair<string, HttpSession> session in Sessions)
            {
                if (session.Value == null)
                {
                    ids.Add(session.Value.SessionId);
                }
                else
                {
                    if (session.Value.ElapsedMinutes > SessionTimeOut)
                    {
                        ids.Add(session.Value.SessionId);
                    }
                }
            }
            if (ids.Count > 0)
            {
                foreach (string id in ids)
                {
                    Sessions.Remove(id);
                }
            }
        }
        protected internal class HttpRespone
        {
            public static string SessionId = "";
            public static HttpSession Session;
            private readonly static ILog _log = LogManager.GetLogger(typeof(HttpRespone));
            public static void SendHeader(string sHttpVersion, string sMIMEHeader, long iTotBytes, string sStatusCode, ref Socket mySocket)
            {

                String sBuffer = "";
                if (sMIMEHeader.Length == 0)
                {
                    sMIMEHeader = "text/html"; // 默认 text/html
                }
                sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
                sBuffer = sBuffer + "Author: Rixiang Yu \r\n";
                sBuffer = sBuffer + "Server: UcAsp.Net \r\n";
                sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
                sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
                sBuffer = sBuffer + "Accept-Encoding: gzip,deflate\r\n";
                if (!Cookies.ContainsKey("UcAsp.Net_SessionId"))
                {
                    Session = new HttpSession();
                    SessionId = Session.SessionId;
                    sBuffer = sBuffer + "Set-Cookie: UcAsp.Net_SessionId=" + Session.SessionId + "\r\n";
                }
                else
                {
                    SessionId = Cookies["UcAsp.Net_SessionId"];
                }
                if (iTotBytes != 0)
                {
                    sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
                }
                SendToBrowser(sBuffer, ref mySocket);

            }
            public static void SendToBrowser(string data, ref Socket socket)
            {
                try
                {
                    if (socket.Connected)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(data);

                        socket.Send(buffer, buffer.Length, 0);
                    }

                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            public static void SendToBrowser(byte[] data, ref Socket socket)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Send(data, 0, data.Length, SocketFlags.None);
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            public static void SendError(string sHttpVersion, ref Socket mySocket)
            {
                string OutMessage = "<H2>Error!! 404 Not Found</H2><Br>";
                SendHeader(sHttpVersion, "", OutMessage.Length, " 404 Not Found", ref mySocket);
                SendToBrowser(OutMessage, ref mySocket);
            }
            public static void SendError(string sHttpVersion, string errorMsg, ref Socket mySocket)
            {
                string OutMessage = "<H2>Error!! 404 Not Found</H2><Br>" + errorMsg;
                SendHeader(sHttpVersion, "", OutMessage.Length, " 404 Not Found", ref mySocket);
                SendToBrowser(OutMessage, ref mySocket);
            }
            public static void SendExpect(ref Socket mySocket)
            {

                SendToBrowser("HTTP/1.1 100 Continue", ref mySocket);

            }
            private static Dictionary<string, string> Header(string request)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string[] r = Regex.Split(request, " ");
                dic.Add("method", r[0]);
                dic.Add("path", r[1]);
                dic.Add("version", r[2]);
                return dic;
            }
            private static Dictionary<string, string> cookies = new Dictionary<string, string>();
            public static Dictionary<string, string> Cookies

            {
                get
                {
                    return cookies;
                }
            }
            public static Tuple<Dictionary<string, string>, Dictionary<string, string>> RequestHeader(string header)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                string[] h = Regex.Split(header, "\r\n");
                Dictionary<string, string> r = Header(h[0]);
                for (int i = 1; i < h.Length; i++)
                {
                    string[] d = Regex.Split(h[i], ": ");
                    if (d.Length == 2)
                    {
                        dic.Add(d[0], d[1]);
                        #region Cookies
                        if (d[0] == "Cookie")
                        {
                            string[] rsp = d[1].Split(';');

                            for (int j = 0; j < rsp.Length; j++)
                            {
                                string[] ck = rsp[j].Split('=');
                                if (ck.Length == 2)
                                {
                                    if (!cookies.ContainsKey(ck[0]))
                                    {
                                        cookies.Add(ck[0], ck[1]);
                                    }
                                }
                            }

                        }
                        #endregion
                    }
                }
                return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(dic, r);
            }

            public static string Form(string key)
            {
                if (_form.ContainsKey(key))
                {
                    return _form[key];
                }
                else
                    return null;
            }
            private static Dictionary<string, string> _form = new Dictionary<string, string>();
            public static Dictionary<string, string> RequestForm(string content)
            {
                Dictionary<string, string> form = new Dictionary<string, string>();
                string[] c = Regex.Split(content, "&");
                for (int i = 0; i < c.Length; i++)
                {
                    string[] s = Regex.Split(c[i], "=");
                    if (s.Length == 2)
                    {
                        form.Add(s[0].ToLower().Trim(), s[1]);
                    }
                }
                _form = form;
                return form;
            }
        }


        protected internal class HtmlTemp
        {
            public static string Html(string code, int WarehouseId, params string[] param)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<!DOCTYPE html>" + Environment.NewLine);
                sb.Append(@"<html lang = ""en"">" + Environment.NewLine);
                sb.Append(@"<head>" + Environment.NewLine);
                sb.Append(@"<title>WWWarehouse 网仓设备 服务控制管理中心</title>" + Environment.NewLine);

                sb.Append(@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />" + Environment.NewLine);
                sb.Append(@"<link href=""//echarts.baidu.com/echarts2/doc/asset/css/bootstrap.css"" rel=""stylesheet"" />" + Environment.NewLine);
                sb.Append(@" </head><body>" + Environment.NewLine);
                sb.Append(@"<div class=""container-fluid"">" + Environment.NewLine);
                sb.Append(code);
                sb.Append("</div>" + Environment.NewLine);

                sb.Append(@"<script src =""//cdn.bootcss.com/jquery/3.1.1/jquery.min.js""></script>" + Environment.NewLine);
                sb.Append(@"<script src=""//echarts.baidu.com/echarts2/doc/asset/js/bootstrap.min.js""></script>" + Environment.NewLine);
                for (int i = 0; i < param.Length; i++)
                {
                    sb.Append(param[i]);
                }
                sb.Append("</body></html>" + Environment.NewLine);
                return sb.ToString();
            }
        }
        public void Dispose()
        {
            stop = true;
            timer.Dispose();
            _server.Stop();
        }


    }
}
