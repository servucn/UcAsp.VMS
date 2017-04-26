/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 13:39:17
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
    public class ReturnResult
    {
        public string Json(MergeResult nvr, MergeResult cifs, string requestid)
        {

            string flag = (nvr.Result || cifs.Result) ? "ok" : "fail";
            string nvrflag = (nvr.Result) ? "Done" : "None";
            string cifsflag = (cifs.Result) ? "Done" : "None";
            return "{\"result\":\"" + flag + "\",\"package-id\":\"" + nvr.PackageUkid + "\",\"request-id\":\"" + requestid + "\",\"urls\":[{\"status\":\"" + nvrflag + "\",\"nvr-video-url\":\"" + nvr.File + "\"},{\"cifs-video-url\":\"" + cifs.File + "\",\"status\":\"" + cifsflag + "\"}]}";
        }
    }
}
