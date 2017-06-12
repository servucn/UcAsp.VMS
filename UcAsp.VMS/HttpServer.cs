/***************************************************
*创建人:TecD02
*创建时间:2017/1/4 15:16:22
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using UcAsp.VMS;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace UcAsp.VMS
{
    public class HttpServer : HttpBase
    {
        private string Program_Id = "1a10a5c6-0529-4e40-b763-3ca67a37812b";
        private readonly static ILog _log = LogManager.GetLogger(typeof(HttpServer));
        public List<NVRInfo> NVRInfos = new List<NVRInfo>();
        public BootStrap Strap = new BootStrap();
        public string VirtualDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string TempPath = AppDomain.CurrentDomain.BaseDirectory;
        public Dictionary<string, string> Result = new Dictionary<string, string>();
        public Dictionary<string, string> Progress = new Dictionary<string, string>();

        public string Path = AppDomain.CurrentDomain.BaseDirectory;
        public override void Listen(object obj)
        {
            FFmpeg.TempPath = this.TempPath;
            FFmpeg.Http = Http;
            FFmpeg.FileSavePath = VirtualDirectory + "\\video\\";
            FFmpeg.Path = Path;
            base.Listen(obj);

        }


        public override void Action(TcpClient socket, string content, string[] Route)
        {



            if (Header.ContainsKey("Host"))
            {
                FFmpeg.Http = Http = "http://" + Header["Host"] + "/";
            }

            if (Route[1].ToLower() == "webapi")
            {

                #region 版本V2
                if (Route[2].ToLower() == "iscs.wcs.vms" || string.IsNullOrEmpty(Route[2].ToLower()))
                {
                    MergeResult NvrVideo = new MergeResult();
                    MergeResult CifsVideo = new MergeResult();
                    bool resgobal = false;
                    bool reslocal = false;
                    #region 合并
                    if (Route[3].ToLower() == "mergevideo")
                    {
                        List<object> para = JsonConvert.DeserializeObject<List<object>>(content);
                        ReuestInfo param = JsonConvert.DeserializeObject<ReuestInfo>(para[0].ToString());

                        if (param == null)
                        {
                            param = new ReuestInfo();
                        }
                        else
                        {

                            param.Hash = HashCode.GetHash(content);
                            resgobal = Strap.RequestGobal(param.Channels, param.PackageUkid + "_" + param.Hash);
                            NvrVideo = FFmpeg.MergeVideo(param.PackageUkid + "_" + param.Hash + "_gobal");

                            reslocal = Strap.RequestLocal(long.Parse(param.PackageUkid));
                            CifsVideo = FFmpeg.MergeVideo(param.PackageUkid.ToString() + "_local");
                        }
                        NvrVideo.PackageUkid = long.Parse(param.PackageUkid);
                        CifsVideo.PackageUkid = long.Parse(param.PackageUkid);
                        string id = Guid.NewGuid().ToString().Replace("-", "");
                        string result = new ReturnResult().Json(NvrVideo, CifsVideo, id);
                        SendCode(socket, result);

                    }
                    #endregion
                    #region 设置OSD
                    else if (Route[3].ToLower() == "osd")
                    {
                        // HttpRespone.RequestForm(content);
                        List<object> param = JsonConvert.DeserializeObject<List<object>>(content);
                        _log.Error(param[0] + "." + param[1] + "." + param[2]);
                        if (Strap.Device != null)
                        {
                            bool result = Strap.Device.IPCOSD(param[0].ToString(), param[1].ToString(), param[2].ToString());
                            if (result)
                            {

                                SendJson(socket, "{\"result\":\"ok\"}");
                            }
                            else
                            {
                                _log.Error(Strap.Device.ErrorMsg);
                                SendJson(socket, "{\"result\":\"fail\",\"msg\":\"" + Strap.Device.ErrorMsg + "\"}");
                            }
                        }
                        else
                        {
                            SendJson(socket, "{\"result\":\"ok\"}");
                        }

                    }
                    #endregion
                    #region 保存图片
                    else if (Route[3].ToLower() == "saveimage")
                    {
                        // HttpRespone.RequestForm(content);
                        string name = DateTime.Now.Ticks + ".jpg";
                        if (!Directory.Exists(FFmpeg.FileSavePath))
                        {
                            Directory.CreateDirectory(FFmpeg.FileSavePath);
                        }
                        string filename = FFmpeg.FileSavePath + name;
                        List<object> param = JsonConvert.DeserializeObject<List<object>>(content);
                        bool re = Strap.Device.SaveImage(param[0].ToString(), param[1].ToString(), filename);
                        _log.Debug(filename);
                        if (re)
                        {
                            SendJson(socket, "{\"result\":\"ok\",\"url\":\"http://" + _url + "/jpg/" + name + "\"}");
                        }
                        else
                        {
                            SendJson(socket, "{\"result\":\"fail\",\"msg\":\"" + Strap.Device.ErrorMsg + "\"}");
                        }
                    }
                    #endregion
                    #region 获取摄像头
                    else if (Route[3].ToLower() == "camera")
                    {
                        SendJson(socket, JsonConvert.SerializeObject(NVRInfos));
                    }
                    #endregion
                    #region  开始录像
                    else if (Route[3].ToLower() == "savevideo")
                    {
                        List<object> param = JsonConvert.DeserializeObject<List<object>>(content);
                        _log.Error(param[0] + "." + param[1] + "." + param[2]);
                        int id = Strap.Device.SaveVideo(param[0].ToString(), param[1].ToString(), long.Parse(param[2].ToString()));
                        if (id != 0)
                        {
                            SendJson(socket, "{\"result\":\"ok\",\"msg\":" + id + "}");
                        }
                        else
                        {
                            SendJson(socket, "{\"result\":\"fail\",\"msg\":" + Strap.Device.ErrorMsg + "}");
                        }
                    }
                    #endregion
                    #region 停止录像
                    else if (Route[3].ToLower() == "stopvideo")
                    {
                        List<object> param = JsonConvert.DeserializeObject<List<object>>(content);
                        bool result = Strap.Device.StopVideo(int.Parse(param[0].ToString()));

                        if (result)
                        {
                            SendJson(socket, "{\"result\":\"ok\"}");
                        }
                        else
                        {
                            SendJson(socket, "{\"result\":\"ok\",\"msg\":\"" + Strap.Device.ErrorMsg + "\"}");
                        }
                    }
                    #endregion


                }
                #endregion

            }
            else if (Route[1].ToLower() == "video")
            {
                string localfile = VirtualDirectory + "video\\" + Route[2].ToLower();
                if (File.Exists(localfile))
                {
                    FileStream fs = new FileStream(localfile, FileMode.Open, FileAccess.Read);
                    BufferedStream bs2 = new BufferedStream(fs);

                    HttpRespone.SendHeader(_httpversion, "application/octet-stream", bs2.Length, " 200 OK", ref socket);
                    byte[] bytes = new byte[4096];
                    int read;
                    while ((read = bs2.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        HttpRespone.SendToBrowser(bytes, ref socket);
                    }
                    bs2.Close();
                }
                else
                {
                    HttpRespone.SendError(_httpversion, "文件存在", ref socket);
                }
            }
            else if (Route[1].ToLower() == "jpg")
            {
                string localfile = VirtualDirectory + "video\\" + Route[2].ToLower();
                if (File.Exists(localfile))
                {
                    FileStream fs = new FileStream(localfile, FileMode.Open, FileAccess.Read);
                    BufferedStream bs2 = new BufferedStream(fs);

                    HttpRespone.SendHeader(_httpversion, "image/jpeg", bs2.Length, " 200 OK", ref socket);
                    byte[] bytes = new byte[4096];
                    int read;
                    while ((read = bs2.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        HttpRespone.SendToBrowser(bytes, ref socket);
                    }
                    bs2.Close();
                }
                else
                {
                    HttpRespone.SendError(_httpversion, "文件存在", ref socket);
                }
            }
            else
            {
                SendCode(socket, HtmlTemp.Html(new HttpCode().AppCode(Strap.Device.NVRInfos), 1));
            }






        }

        private void SaveVideo(string code)
        { }
    }
}
