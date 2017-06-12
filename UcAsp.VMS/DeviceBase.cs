/***************************************************
*创建人:rixiang.yu
*创建时间:2017/5/25 10:10:07
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UcAsp.VMS
{
    public  abstract class DeviceBase:IDevice
    {
        private string Program_Id = "7b0a4ef4-ab75-4b05-8a5a-eab627820d29";

        public string ErrorMsg
        {
            get;

            set;
        }

        public long LastRunTime
        {
            get;

            set;
        }

        public List<NVRInfo> NVRInfos
        {
            get;

            set;
        }

        public abstract bool CheckState();

        public abstract void Dispose();

        public abstract byte[] GetImageByte(string nvrip, string ipcip);

        public virtual bool Initialilize()
        {
            throw new NotImplementedException();
        } 

        public abstract bool IPCOSD(string nvrip, string ipcip, string osd);

        public abstract bool SaveImage(string nvrip, string ipcip, string filename);

        public abstract int SaveVideo(string nvrip, string ipcip, long packageukid);
        public abstract bool StopVideo(int id);
    }
}
