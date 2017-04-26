/***************************************************
*创建人:TecD02
*创建时间:2017/1/19 20:16:23
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text;
using log4net;
namespace UcAsp.VMS
{
    public class HashCode
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(HashCode));
        public static string GetHash(string code)
        {
            _log.Info(code);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(Encoding.UTF8.GetBytes(code));
            string byte2String = null;
            for (int i = 0; i < output.Length; i++)
            {
                byte2String += output[i].ToString("x");
            }
            return byte2String;
        }
    }
}
