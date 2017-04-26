/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 14:14:01
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
using System.Runtime.InteropServices;
using log4net;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
namespace UcAsp.VMS
{

    public class HikVision : IDevice
    {
        public static ILog _log = LogManager.GetLogger("HikVision");
        string configpath = AppDomain.CurrentDomain.BaseDirectory + "iscs.config";
        public string ErrorMsg { get; set; }

        private System.Timers.Timer heartbeat = new System.Timers.Timer();
        private Dictionary<int, long> process = new Dictionary<int, long>();
        private CHCNetSDK.NET_DVR_IPPARACFG_V40 struIpParaCfgV40;

        private CHCNetSDK.NET_DVR_PICCFG_V40 struPicCfgV40;
        private CHCNetSDK.NET_DVR_SHOWSTRING_V30 m_struShowStrCfg;
        public List<NVRInfo> NVRInfos { get; set; }
        public long LastRunTime { get; set; }
        public bool Initialilize()
        {
            LastRunTime = DateTime.Now.Ticks / 10000;
            struIpParaCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40();
            struPicCfgV40 = new CHCNetSDK.NET_DVR_PICCFG_V40();
            m_struShowStrCfg = new CHCNetSDK.NET_DVR_SHOWSTRING_V30();
            Console.WriteLine("开始初始化");
            heartbeat.Interval = 60 * 1000 * 3;
            heartbeat.Elapsed -= Heartbeat_Elapsed;
            heartbeat.Elapsed += Heartbeat_Elapsed;
            heartbeat.Start();
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "SdkLog\\"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "SdkLog\\");
            }
            Config config = new Config(configpath) { GroupName = "service" };
            string[] host = config.GetSectionValues();
            if (NVRInfos == null)
            {
                CHCNetSDK.NET_DVR_SetLogToFile(0, AppDomain.CurrentDomain.BaseDirectory + "SdkLog\\", true);
                bool init = CHCNetSDK.NET_DVR_Init();
                if (!init)
                {
                    _log.Error("初始化失败" + CHCNetSDK.NET_DVR_GetLastError());
                    ErrorMsg = "初始化失败";
                    return false;
                }

                bool retime = CHCNetSDK.NET_DVR_SetConnectTime(3000, 500);
                if (!retime)
                {
                    _log.Error("连接超时设置" + CHCNetSDK.NET_DVR_GetLastError());
                    ErrorMsg = "连接超时设置";
                }
                bool re = CHCNetSDK.NET_DVR_SetReconnect(3000, 1);
                if (!re)
                {
                    _log.Error("重连设置" + CHCNetSDK.NET_DVR_GetLastError());
                    ErrorMsg = "重连设置";
                }

                NVRInfos = new List<NVRInfo>();
            }
            for (int x = 0; x < host.Length; x++)
            {
                string name = host[x];
                config.Section = name;
                string ip = config.GetValue("host", "ip", "192.168.2.1");
                int port = config.GetValue("host", "port", 8000);
                string username = config.GetValue("host", "username", "admin");
                string password = config.GetValue("host", "password", "ywwy2016");
                _log.Error(name + "." + x);
                NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == ip);

                if (nvr == null)
                {
                    nvr = new NVRInfo() { Host = new NVRHostHiKv() { DVRIPAddress = ip, DVRPassword = password, DVRPort = port, DVRUserName = username, LUserId = -1 }, IPCs = new List<IPC>() };

                }
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                int _login = -1;
                if (nvr.Host.LUserId > -1)
                {
                    _login = nvr.Host.LUserId;
                }
                else
                {
                    _login = CHCNetSDK.NET_DVR_Login_V30(ip, port, username, password, ref DeviceInfo);
                    Console.WriteLine(x + "登录" + _login);
                }

                if (_login >= 0)
                {
                    nvr.DeviceInfo = DeviceInfo;
                    nvr.Host.LUserId = _login;
                    uint dwSize = (uint)Marshal.SizeOf(struIpParaCfgV40);

                    IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struIpParaCfgV40, ptrIpParaCfgV40, true);

                    uint dwReturn = 0;
                    int iGroupNo = 0;
                    #region IPC通道
                    if (DeviceInfo.byIPChanNum > 0)
                    {

                        if (!CHCNetSDK.NET_DVR_GetDVRConfig(_login, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
                        {
                            ErrorMsg = "获取IPC失败." + CHCNetSDK.NET_DVR_GetLastError();
                            Console.WriteLine(ErrorMsg);
                            _log.Error(ErrorMsg);
                            Marshal.FreeHGlobal(ptrIpParaCfgV40);
                        }
                        else
                        {
                            struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
                            CHCNetSDK.NET_DVR_IPDEVINFO_V31[] ipdevinfos = struIpParaCfgV40.struIPDevInfo;
                            #region　获取通道信息
                            for (int m = 0; m < ipdevinfos.Length; m++)
                            {
                                CHCNetSDK.NET_DVR_GET_STREAM_UNION unionGetStream = struIpParaCfgV40.struStreamMode[m].uGetStream;
                                uint unSize = (uint)Marshal.SizeOf(unionGetStream);
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)unSize);
                                Marshal.StructureToPtr(unionGetStream, ptrChanInfo, true);
                                CHCNetSDK.NET_DVR_IPCHANINFO struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));
                                IPC ipc = new IPC() { AdminPort = ipdevinfos[m].wDVRPort.ToString(), ChannelIndex = m + (int)struIpParaCfgV40.dwStartDChan, ChannelIp = ipdevinfos[m].struIP.sIpV4, ChannelName = "IPCamera" + m, Status = ipdevinfos[m].byEnable, UserName = Encoding.UTF8.GetString(ipdevinfos[m].sUserName).Trim('\0') };
                                #region 获取通道名称
                                if (ipc.Status == 1)
                                {
                                    Int32 nSize = Marshal.SizeOf(struPicCfgV40);
                                    IntPtr ptrPicCfg = Marshal.AllocHGlobal(nSize);
                                    Marshal.StructureToPtr(struPicCfgV40, ptrPicCfg, true);
                                    if (!CHCNetSDK.NET_DVR_GetDVRConfig(_login, CHCNetSDK.NET_DVR_GET_PICCFG_V40, ipc.ChannelIndex, ptrPicCfg, (UInt32)nSize, ref dwReturn))
                                    {
                                        ipc.ErrorMsg = CHCNetSDK.NET_DVR_GetLastError().ToString();
                                        ipc.Status = 0;
                                        IPC oldipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipc.ChannelIp);
                                        if (oldipc != null)
                                        {
                                            nvr.IPCs.Remove(oldipc);
                                        }
                                        nvr.IPCs.Add(ipc);
                                    }
                                    else
                                    {
                                        struPicCfgV40 = (CHCNetSDK.NET_DVR_PICCFG_V40)Marshal.PtrToStructure(ptrPicCfg, typeof(CHCNetSDK.NET_DVR_PICCFG_V40));
                                        if (struPicCfgV40.sChanName != null)
                                        {
                                            ipc.ChannelNumber = System.Text.Encoding.GetEncoding("GBK").GetString(struPicCfgV40.sChanName).Trim('\0');
                                        }
                                        ipc.Status = struChanInfo.byEnable;
                                        _log.Info("2:" + ipc.ChannelIndex + "." + ipc.ChannelIp + "." + struChanInfo.byEnable + "." + ipc.ChannelNumber);
                                        Console.WriteLine(ipc.ChannelIndex + "." + ipc.ChannelIp + "." + struChanInfo.byEnable + "." + ipc.ChannelNumber);
                                        IPC oldipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipc.ChannelIp);
                                        if (oldipc != null)
                                        {
                                            nvr.IPCs.Remove(oldipc);
                                        }
                                        nvr.IPCs.Add(ipc);
                                    }
                                    Marshal.FreeHGlobal(ptrPicCfg);
                                }
                                #endregion
                                Marshal.FreeHGlobal(ptrChanInfo);
                            }
                            #endregion

                        }

                        NVRInfo oldnvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == ip);
                        if (nvr != null)
                        {
                            NVRInfos.Remove(oldnvr);
                        }
                        NVRInfos.Add(nvr);
                    }
                    #endregion
                    #region 其他通道
                    else
                    {
                        for (int i = 0; i < DeviceInfo.byChanNum; i++)
                        {
                            dwReturn = 0;

                            Int32 nSize = Marshal.SizeOf(struPicCfgV40);
                            IntPtr ptrPicCfg = Marshal.AllocHGlobal(nSize);
                            Marshal.StructureToPtr(struPicCfgV40, ptrPicCfg, true);
                            int chancelid = i + DeviceInfo.byStartChan;
                            if (!CHCNetSDK.NET_DVR_GetDVRConfig(_login, CHCNetSDK.NET_DVR_GET_PICCFG_V40, chancelid, ptrPicCfg, (UInt32)nSize, ref dwReturn))
                            {
                                ErrorMsg = CHCNetSDK.NET_DVR_GetLastError().ToString();
                            }
                            else
                            {
                                struPicCfgV40 = (CHCNetSDK.NET_DVR_PICCFG_V40)Marshal.PtrToStructure(ptrPicCfg, typeof(CHCNetSDK.NET_DVR_PICCFG_V40));
                                IPC ipc = new IPC() { AdminPort = "", ChannelIndex = chancelid, ChannelIp = nvr.Host.DVRIPAddress, ChannelName = "IPCamera" + i, Status = 1, ChannelNumber = System.Text.Encoding.GetEncoding("GBK").GetString(struPicCfgV40.sChanName).Trim('\0') };

                                IPC oldipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipc.ChannelIp);
                                if (oldipc != null)
                                {
                                    nvr.IPCs.Remove(oldipc);
                                }
                                nvr.IPCs.Add(ipc);
                            }
                            NVRInfo oldnvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == ip);
                            if (nvr != null)
                            {
                                NVRInfos.Remove(oldnvr);
                            }
                            NVRInfos.Add(nvr);
                            Marshal.FreeHGlobal(ptrPicCfg);
                        }

                    }
                    #endregion

                    Marshal.FreeHGlobal(ptrIpParaCfgV40);
                }
                else
                {
                    NVRInfo nvrerr = new NVRInfo() { Host = new NVRHostHiKv() { DVRIPAddress = ip, DVRPassword = password, DVRPort = port, DVRUserName = username, LUserId = -1 }, IPCs = new List<IPC>() };

                    ErrorMsg = "登录失败." + CHCNetSDK.NET_DVR_GetLastError();
                    nvrerr.Host.ErrorMsg = ErrorMsg;
                    Console.WriteLine(ErrorMsg);
                    _log.Error(ErrorMsg);
                    NVRInfo oldnvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == ip);
                    if (nvr != null)
                    {
                        NVRInfos.Remove(oldnvr);
                    }
                    NVRInfos.Add(nvrerr);
                }


            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("完成初始化");

            return true;
        }

        private void Heartbeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                heartbeat.Stop();
                if (((DateTime.Now.Ticks / 10000) - LastRunTime) > 10 * 1000 * 60)
                {
                    CheckState();
                }

                var listprocess = process.ToList();
                for (int i = listprocess.Count - 1; i >= 0; i--)
                {
                    long kv = process[listprocess[i].Key];
                    if (((DateTime.Now.Ticks / 10000) - kv) > 15 * 1000 * 60)
                    {
                        StopVideo(listprocess[i].Key);
                    }
                }




            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                heartbeat.Stop();
                heartbeat.Start();
            }
        }

        public bool IPCOSD(string nvrip, string ipcip, string osd)
        {
            List<OSD> text = JsonConvert.DeserializeObject<List<OSD>>(osd).Take(8).ToList();
            NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == nvrip);
            if (nvr == null)
            {
                ErrorMsg = "NVR不存在";
                _log.Error(ErrorMsg);
                return false;
            }
            int userId = nvr.Host.LUserId;
            IPC ipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipcip);
            if (ipc == null)
            {
                ErrorMsg = "IPC不存在";
                _log.Error(ErrorMsg);
                return false;
            }
            int channel = ipc.ChannelIndex;
            UInt32 dwReturn = 0;
            Int32 nSize = Marshal.SizeOf(m_struShowStrCfg);
            IntPtr ptrShowStrCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struShowStrCfg, ptrShowStrCfg, true);

            _log.Debug(userId + "." + channel);
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(userId, CHCNetSDK.NET_DVR_GET_SHOWSTRING_V30, channel, ptrShowStrCfg, (UInt32)nSize, ref dwReturn))
            {

                ErrorMsg = "获取设置信息失败" + CHCNetSDK.NET_DVR_GetLastError();
                _log.Error(ErrorMsg);
                Marshal.FreeHGlobal(ptrShowStrCfg);
                return false;

            }
            else
            {
                m_struShowStrCfg = (CHCNetSDK.NET_DVR_SHOWSTRING_V30)Marshal.PtrToStructure(ptrShowStrCfg, typeof(CHCNetSDK.NET_DVR_SHOWSTRING_V30));
                Marshal.FreeHGlobal(ptrShowStrCfg);
            }
            int i = 0;
            foreach (OSD txt in text)
            {
                m_struShowStrCfg.struStringInfo[i].wShowString = (ushort)(txt.Show == true ? 1 : 0);
                m_struShowStrCfg.struStringInfo[i].sString = txt.Text;
                m_struShowStrCfg.struStringInfo[i].wStringSize = (ushort)Encoding.UTF8.GetBytes(txt.Text).Length;
                m_struShowStrCfg.struStringInfo[i].wShowStringTopLeftX = UInt16.Parse(txt.X.ToString());
                m_struShowStrCfg.struStringInfo[i].wShowStringTopLeftY = UInt16.Parse(txt.Y.ToString());
                i++;
            }
            nSize = Marshal.SizeOf(m_struShowStrCfg);
            ptrShowStrCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struShowStrCfg, ptrShowStrCfg, true);
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(userId, CHCNetSDK.NET_DVR_SET_SHOWSTRING_V30, channel, ptrShowStrCfg, (UInt32)nSize))
            {
                ErrorMsg = "设置信息失败" + CHCNetSDK.NET_DVR_GetLastError();
                _log.Error(ErrorMsg);
                Marshal.FreeHGlobal(ptrShowStrCfg);
                return false;
            }
            else
            {
                Marshal.FreeHGlobal(ptrShowStrCfg);
                ErrorMsg = "";
                return true;
            }
        }

        public bool CheckState()
        {
            _log.Debug("CheckState");
            //  Process.GetCurrentProcess().MinWorkingSet = new System.IntPtr(100);
            Initialilize();

            return true;
        }

        public int SaveVideo(string nvrip, string ipcip, long packageukid)
        {
            NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == nvrip);
            if (nvr == null)
            {
                ErrorMsg = "NVR不存在";
                _log.Error(ErrorMsg);
                return 0;
            }
            IPC ipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipcip);
            if (ipc == null)
            {
                ErrorMsg = "IPC不存在";
                _log.Error(ErrorMsg);
                return 0;
            }
            int id = FFmpeg.RecordVideo(nvr.Host, ipc.ChannelIndex, packageukid);
            if (!process.ContainsKey(id))
            {
                process.Add(id, DateTime.Now.Ticks / 10000);
            }
            return id;
        }

        public bool StopVideo(int id)
        {
            int result = FFmpeg.ColseProcess(id);
            if (process.ContainsKey(id))
            {
                process.Remove(id);
            }
            if (result != 0)
            {
                ErrorMsg = "失败:" + result;
                return false;
            }
            return true;
        }
        public bool SaveImage(string nvrip, string ipcip, string filename)
        {
            NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == nvrip);
            if (nvr == null)
            {
                ErrorMsg = "NVR不存在";
                _log.Error(ErrorMsg);
                return false;
            }
            int userId = nvr.Host.LUserId;
            IPC ipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipcip);
            if (ipc == null)
            {
                ErrorMsg = "IPC不存在";
                _log.Error(ErrorMsg);
                return false;
            }
            int channel = ipc.ChannelIndex;
            CHCNetSDK.NET_DVR_JPEGPARA jpg = new CHCNetSDK.NET_DVR_JPEGPARA() { wPicQuality = 0, wPicSize = 2 };

            bool re = CHCNetSDK.NET_DVR_CaptureJPEGPicture(userId, channel, ref jpg, filename);
            if (!re)
            {
                _log.Error("保存图片错误" + CHCNetSDK.NET_DVR_GetLastError());
                ErrorMsg = "保存图片错误" + CHCNetSDK.NET_DVR_GetLastError();
            }
            return re;
        }

        public byte[] GetImageByte(string nvrip, string ipcip)
        {
            NVRInfo nvr = NVRInfos.FirstOrDefault(o => o.Host.DVRIPAddress == nvrip);
            if (nvr == null)
            {
                ErrorMsg = "NVR不存在";
                _log.Error(ErrorMsg);
            }
            int userId = nvr.Host.LUserId;
            IPC ipc = nvr.IPCs.FirstOrDefault(o => o.ChannelIp == ipcip);
            if (ipc == null)
            {
                ErrorMsg = "IPC不存在";
                _log.Error(ErrorMsg);
            }
            int channel = ipc.ChannelIndex;
            CHCNetSDK.NET_DVR_JPEGPARA jpg = new CHCNetSDK.NET_DVR_JPEGPARA() { wPicQuality = 0, wPicSize = 2 };
            byte[] buffer = new byte[1024 * 1000];
            uint dwPicSize = 1024 * 1000;
            uint size = 0;
            bool re = CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(userId, channel, ref jpg, buffer, dwPicSize, ref size);
            if (!re)
            {
                ErrorMsg = "获取失败" + CHCNetSDK.NET_DVR_GetLastError();
            }

            byte[] by = buffer.ToArray().Take((int)size).ToArray();
            return by;
        }
        ~HikVision()
        {
            Dispose();
        }
        public void Dispose()
        {
            foreach (NVRInfo nvr in NVRInfos)
            {
                if (nvr.Host.LUserId > -1)
                {
                    CHCNetSDK.NET_DVR_Logout(nvr.Host.LUserId);
                }
            }
            CHCNetSDK.NET_DVR_Cleanup();
            foreach (KeyValuePair<int, long> kv in process)
            {

                StopVideo(kv.Key);
                try
                {
                    Process p = Process.GetProcessById(kv.Key);
                    if (p != null)
                        p.Kill();
                }
                catch { }

            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
