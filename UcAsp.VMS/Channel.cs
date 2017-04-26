/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 17:03:15
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
    public class Channel
    {
        private string Program_Id = "778e1b46-30a1-4749-8c6a-8a22bb90fac4";

        public int No { get; set; }
        public string NvrIp { get; set; }
        public string ChannelIp { get; set; }
        public int UserId { get; set; }

        public int Index { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}
