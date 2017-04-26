/***************************************************
*创建人:TecD02
*创建时间:2017/1/3 20:05:47
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
    /// <summary>
    /// 请求参数
    /// </summary>
    public class RunParam
    {

        public Channel Channel { get; set; }

        public string FileName { get; set; }

        public string SavePath { get; set; }


    }
}
