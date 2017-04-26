/***************************************************
*创建人:TecD02
*创建时间:2017/1/4 18:59:32
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
    public class ReuestInfo
    {
        public string PackageUkid { get; set; }

        public string RequestId { get; set; }
        public List<Channel> Channels { get; set; }

        public string Hash { get; set; }
    }
}
