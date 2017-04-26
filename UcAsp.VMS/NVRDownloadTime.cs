/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 16:07:31
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.IO;
using log4net;
namespace UcAsp.VMS
{
    public class NVRDownloadTime
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(NVRDownloadTime));
        private int _userId { get; set; }
        private int _download { get; set; }
        private bool _finsh { get; set; }

        private int _downloadprogress { get; set; }
        private Channel _channel = new Channel();
        public Channel GetChannel { get { return this._channel; } }
        public bool Write { get; set; }
        private object _obj;
        private int trytimes = 0;
        public long LastRunTime { get; set; }
        public int Error
        {
            get
            {
                if (_finsh)
                {
                    return 0;
                }
                else
                {
                    int ierr = (int)CHCNetSDK.NET_DVR_GetLastError();
                    if (ierr != 0)
                    {
                        Console.WriteLine(this._download + "." + ierr);
                        if (trytimes < 10)
                        {
                            //  DownloadTime(_obj);
                        }
                        trytimes++;
                    }
                    return ierr;
                }
            }
        }
        public int GetDownloadPos()
        {
            LastRunTime = DateTime.Now.Ticks / 10000;
            if (_downloadprogress == 100)
            {
                if (!_finsh)
                {
                    _finsh = true;
                    Console.WriteLine(this._download + "完成" + "." + CHCNetSDK.NET_DVR_GetLastError());

                    while (!CHCNetSDK.NET_DVR_StopGetFile(this._download))
                    {
                        Console.WriteLine("停止下载");
                        _log.Info("停止下载");
                        Thread.Sleep(100);
                    }

                    GC.Collect();

                }
                return _downloadprogress;
            }
            else
            {
                _downloadprogress = CHCNetSDK.NET_DVR_GetDownloadPos(this._download);
            }

            return _downloadprogress;
        }

        public void CancelDownload()
        {
            LastRunTime = DateTime.Now.Ticks / 10000;
            CHCNetSDK.NET_DVR_StopGetFile(this._download);
            GC.Collect();
            Console.WriteLine(this._download + "取消");
        }
        public void DownloadTime(object req)
        {
            LastRunTime = DateTime.Now.Ticks / 10000;
            _obj = req;
            _finsh = false;
            RunParam parm = (RunParam)req;
            Channel request = parm.Channel;
            _channel = request;
            int userId = request.UserId; int index = request.Index; DateTime start = request.StartTime; DateTime end = request.EndTime;
            //  Console.WriteLine("下载" + userId + "." + index);
            _userId = userId;
            CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
            struDownPara.dwChannel = (uint)index;
            //设置下载的开始时间 Set the starting time
            struDownPara.struStartTime.dwYear = (uint)start.Year;
            struDownPara.struStartTime.dwMonth = (uint)start.Month;
            struDownPara.struStartTime.dwDay = (uint)start.Day;
            struDownPara.struStartTime.dwHour = (uint)start.Hour;
            struDownPara.struStartTime.dwMinute = (uint)start.Minute;
            struDownPara.struStartTime.dwSecond = (uint)start.Second;

            //设置下载的结束时间 Set the stopping time
            struDownPara.struStopTime.dwYear = (uint)end.Year;
            struDownPara.struStopTime.dwMonth = (uint)end.Month;
            struDownPara.struStopTime.dwDay = (uint)end.Day;
            struDownPara.struStopTime.dwHour = (uint)end.Hour;
            struDownPara.struStopTime.dwMinute = (uint)end.Minute;
            struDownPara.struStopTime.dwSecond = (uint)end.Second;
            string sVideoFileName;  //录像文件保存路径和文件名 the path and file name to save      
            sVideoFileName = parm.SavePath + "\\" + parm.FileName + "." + request.No + ".mp4";
            _log.Error(sVideoFileName);
            if (File.Exists(sVideoFileName))
            {
                _log.Info(sVideoFileName + "文件已存在");
                _downloadprogress = 100;
                _finsh = true;
            }
            else
            {
                //按时间下载 Download by time
                this._download = CHCNetSDK.NET_DVR_GetFileByTime_V40(userId, sVideoFileName, ref struDownPara);
                int t = 0;
                while (this._download < 0 && t < 10)
                {
                    Console.WriteLine(_channel.ChannelIp + "." + _download + "：重试" + CHCNetSDK.NET_DVR_GetLastError());
                    _log.Info(_channel.ChannelIp + "." + _download + "：重试" + CHCNetSDK.NET_DVR_GetLastError());
                    _download = CHCNetSDK.NET_DVR_GetFileByTime_V40(userId, sVideoFileName, ref struDownPara);
                    t++;
                    Thread.Sleep(300);
                }
                uint iOutValue = 0;
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(_download, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    _log.Info(_download + "." + CHCNetSDK.NET_DVR_GetLastError());
                    Console.WriteLine(CHCNetSDK.NET_DVR_GetLastError());
                }
            }
        }

    }
}
