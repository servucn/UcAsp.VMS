/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 11:00:18
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UcAsp.VMS.v1
{
    public class channels
    {
        private string Program_Id = "bc1fa4ec-6f78-48f4-bda9-312eb540b457";
        public string channel_ip { get; set; }

        public string start_time { get; set; }

        public string end_time { get; set; }
    }
}
