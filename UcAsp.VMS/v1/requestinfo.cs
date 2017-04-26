/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 11:01:32
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
    public class requestinfo
    {
        public string nvr_ip { get; set; }

        public List<channels> channels { get; set; }
    }
}
