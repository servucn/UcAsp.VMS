/***************************************************
*创建人:TecD02
*创建时间:2016/12/30 20:35:38
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
    public class NVRHostHiKv
    {
        private string Program_Id = "6ad19648-1334-4378-8fe1-bbf3fe3249da";

        public int LUserId = -1;
        public string DVRIPAddress { get; set; }
        public int DVRPort { get; set; }

        public string DVRUserName { get; set; }

        public string DVRPassword { get; set; }

        public string ErrorMsg { get; set; }




    }
}
