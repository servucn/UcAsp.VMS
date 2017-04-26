/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 14:24:26
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace UcAsp.VMS
{
    public class HttpHeader
    {
        public Dictionary<string, string> Header(string content)
        {
            Dictionary<string, string> _r = new Dictionary<string, string>();
            string[] _h = Regex.Split(content, "\r\n");
            for (int i = 1; i < _h.Length; i++)
            {
                string[] _v = Regex.Split(_h[i], ": ");
                if (_v.Length == 2)
                {
                    _r.Add(_v[0], _v[1]);
                }
            }
            return _r;
        }
    }
}
