/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 10:15:15
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
    public class IPC
    {
        private string Program_Id = "4eec08e8-321e-4315-ba10-96caaac0a1c1";

        public string ChannelNumber { get; set; }

        public string ChannelName { get; set; }

        public int ChannelIndex { get; set; }

        public string ChannelIp { get; set; }

        public string AdminPort { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Status { get; set; }

        public string ErrorMsg { get; set; }


    }
}
