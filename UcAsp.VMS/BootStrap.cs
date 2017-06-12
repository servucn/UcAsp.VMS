/***************************************************
*创建人:TecD02
*创建时间:2016/12/30 19:47:45
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config = UcAsp.VMS.Config;
using System.Threading;
using System.IO;
using log4net;
namespace UcAsp.VMS
{
    public class BootStrap : IDisposable
    {
        public static ILog _log = LogManager.GetLogger("BootStrap");
        string _config = AppDomain.CurrentDomain.BaseDirectory + "iscs.config";
        private List<NVRInfo> NVRInfos;
        private HttpServer _server;
        public IDevice Device;
        public string Temp = AppDomain.CurrentDomain.BaseDirectory;
        public BootStrap()
        {
            _config = AppDomain.CurrentDomain.BaseDirectory + "iscs.config";
        }
        public DateTime LastRunTime()
        {
            if (_server != null)
            {
                return _server.LastRunTime;
            }
            else
            {
                return DateTime.Parse("0001/01/01 00:00");
            }
        }
        public string LastError()
        {
            if (_server != null)
            {
                return _server.LastError;
            }
            else
            {
                return "服务不存在";
            }
        }

        public string LastMethod()
        {
            if (_server != null)
            {
                return _server.LastMethod;
            }
            else
            {
                return "服务不存在";
            }

        }
        public string LastParam()
        {

            if (_server != null)
            {
                return _server.LastParam;
            }
            else
            {
                return "服务不存在";
            }
        }
        public void Start(string configpath)
        {

            _config = configpath;
            Device = new HikVision();
            Device.LastRunTime = DateTime.Now.Ticks / 10000;
            Device.Initialilize();
            NVRInfos = Device.NVRInfos;
            Config setting = new Config(_config) { GroupName = "app" };
            int port = setting.GetValue("http", "port", 6001);
            ///生成录像包装路径
            string dir = setting.GetValue("http", "dir", AppDomain.CurrentDomain.BaseDirectory);
            ///vms 下载录像零时保存地址
            string temp = setting.GetValue("video", "temp", AppDomain.CurrentDomain.BaseDirectory);
            ///包装录像包装地址
            string path = setting.GetValue("video", "path", AppDomain.CurrentDomain.BaseDirectory);
            string url = setting.GetValue("http", "url", "http://127.0.0.1");
            _server = new HttpServer();
            _server.TempPath = Temp = temp;
            _server.Url = url;           
            _server.NVRInfos = NVRInfos;
            _server.Strap = this;
            _server.Path = path;
            _server.VirtualDirectory = dir;
            _server.StartListen(port);

        }

        public bool RequestGobal(List<Channel> channels, string fileName)
        {

            Dictionary<int, NVRDownloadTime> status = new Dictionary<int, NVRDownloadTime>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < channels.Count; i++)
            {
                NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == channels[i].NvrIp);
                if (nvr == null)
                    return false;
                channels[i].NvrIp = nvr.Host.DVRIPAddress;
                channels[i].UserId = nvr.Host.LUserId;
                channels[i].No = i;

                IPC ipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == channels[i].ChannelIp);
                if (ipc != null)
                {
                    channels[i].Index = ipc.ChannelIndex;
                    RunParam param = new RunParam { FileName = fileName, Channel = channels[i], SavePath = Temp.Replace("\\", "\\\\") };
                    if (ipc.Status == 1)
                    {
                        NVRDownloadTime down = new NVRDownloadTime();
                        Device.LastRunTime = down.LastRunTime;
                        status.Add(i, down);
                        down.DownloadTime(param);
                        // Thread t = new Thread(new ParameterizedThreadStart(down.DownloadTime));
                        // t.Start(param);
                    }
                }
                Thread.Sleep(10);

            }
            int times = 0;
            int error = 0;
            bool result = false;
            while (true)
            {

                bool flag = true;
                foreach (KeyValuePair<int, NVRDownloadTime> kv in status)
                {

                    Device.LastRunTime = kv.Value.LastRunTime;
                    if (kv.Value.GetDownloadPos() < 100 && kv.Value.Error == 0)
                    {
                        flag = false;
                    }
                    if (kv.Value.GetDownloadPos() == 100 && !kv.Value.Write)
                    {
                        kv.Value.Write = true;

                    }
                    Console.WriteLine(kv.Key + "." + kv.Value.GetDownloadPos() + "." + kv.Value.Error);
                    Thread.Sleep(20);
                }
                if (times > 10000)
                {
                    foreach (KeyValuePair<int, NVRDownloadTime> kv in status)
                    {
                        kv.Value.CancelDownload();

                    }
                    result = false;
                    break;
                }
                if (flag)
                {
                    result = true;
                    break;
                }
                Thread.Sleep(30);
                times++;
            }
            if (result)
            {
                foreach (KeyValuePair<int, NVRDownloadTime> kv in status)
                {
                    if (kv.Value.Write)
                    {

                        sb.AppendLine("file " + Temp.Replace("\\", "\\\\") + fileName + "." + kv.Value.GetChannel.No + ".mp4" + "");
                    }
                }
                System.IO.File.WriteAllText(Temp + fileName + "_gobal.txt", sb.ToString(), Encoding.ASCII);
            }
            return result;
        }


        public bool RequestLocal(long PackageUkid)
        {
            Config setting = new Config(_config) { GroupName = "app" };
            string path = _server.Path;
            StringBuilder sb = new StringBuilder();
            List<string> fileName = new List<string>();
            string files = PackageUkid.ToString();
            if (files.Length > 8)
            {
                files = files.Substring(0, 8);
            }
            fileName.Add(path + "\\\\" + files + "\\\\" + PackageUkid + ".avi");
            _log.Info(path + "\\" + files + "\\" + PackageUkid + ".avi");
            if (File.Exists(path + "\\" + files + "\\" + PackageUkid + ".avi"))
            {
                foreach (string file in fileName)
                {
                    sb.AppendLine("file " + file + "");
                    if (!System.IO.File.Exists(file))
                    {
                        return false;
                    }
                }

                System.IO.File.WriteAllText(Temp + PackageUkid + "_local.txt", sb.ToString(), Encoding.ASCII);
                return true;
            }
            else
            { return false; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用


        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            if (!disposedValue)
            {
                _server.Dispose();
                Device.Dispose();
            }
        }
        #endregion


    }
}
