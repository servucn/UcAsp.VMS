/***************************************************
*创建人:rixiang.yu
*创建时间:2017/5/25 14:15:47
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
    public class Axis:DeviceBase
    {
        private string Program_Id = "b0c07917-765d-4d8f-a650-2712b7826767";

        public override bool Initialilize()
        {
            return base.Initialilize();
        }
        public override bool CheckState()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override byte[] GetImageByte(string nvrip, string ipcip)
        {
            throw new NotImplementedException();
        }

        public override bool IPCOSD(string nvrip, string ipcip, string osd)
        {
            throw new NotImplementedException();
        }

        public override bool SaveImage(string nvrip, string ipcip, string filename)
        {
            throw new NotImplementedException();
        }

        public override int SaveVideo(string nvrip, string ipcip, long packageukid)
        {
            throw new NotImplementedException();
        }

        public override bool StopVideo(int id)
        {
            throw new NotImplementedException();
        }
    }
}
