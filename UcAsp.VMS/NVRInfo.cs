/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 13:49:14
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
namespace UcAsp.VMS
{
    public class NVRInfo
    {
        private string Program_Id = "d240e83f-63e4-4258-9010-4a8b9cd7c4cc";
        public NVRHostHiKv Host { get; set; }
        [JsonIgnore]
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo { get; set; }
        public List<IPC> IPCs { get; set; }
    }
}
