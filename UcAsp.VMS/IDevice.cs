/***************************************************
*创建人:余日祥
*创建时间:2017/2/4 9:52:35
*功能说明:<Function>
*版权所有:<Copyright>
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UcAsp.VMS
{
    public interface IDevice:IDisposable
    {
        string ErrorMsg { get; set; }
        List<NVRInfo> NVRInfos { get; set; }
        long LastRunTime { get; set; }

        bool Initialilize();

        bool IPCOSD(string nvrip, string ipcip, string osd);

        int SaveVideo(string nvrip, string ipcip, long packageukid);

        bool StopVideo(int id);

        bool SaveImage(string nvrip, string ipcip, string filename);
        byte[] GetImageByte(string nvrip, string ipcip);

        bool CheckState();
        

    }
}
