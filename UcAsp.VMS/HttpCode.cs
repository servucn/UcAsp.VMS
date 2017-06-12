/***************************************************
*创建人:TecD02
*创建时间:2017/1/4 15:49:51
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UcAsp.VMS;
namespace UcAsp.VMS
{
    public class HttpCode
    {
        public string AppCode(List<NVRInfo> nvrs)
        {
            if (nvrs == null)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<div class=""panel panel-primary""><div class=""panel-heading""><h3>UCASP VMS管理中心</h3></div>" + Environment.NewLine);
            sb.Append(@"<table class=""table table-responsive"">" + Environment.NewLine);

            foreach (NVRInfo nvr in nvrs)
            {
                sb.Append(@"<tr class=""active"">" + Environment.NewLine);
                sb.Append(@"<td>" + Environment.NewLine);
                sb.Append(@"</td>" + Environment.NewLine);
                sb.Append(@"<td>" + Environment.NewLine);
                sb.Append(nvr.Host.DVRIPAddress + Environment.NewLine);
                sb.Append(@"</td>" + Environment.NewLine);
                sb.Append(@"<td>" + Environment.NewLine);
                sb.Append(nvr.Host.DVRPort + Environment.NewLine);
                sb.Append(@"</td>" + Environment.NewLine);
                sb.Append(@"<td>" + Environment.NewLine);

                sb.Append(@"</td>" + Environment.NewLine);
                sb.Append(@"<td>" + Environment.NewLine);
                if (nvr.Host.LUserId > -1)
                {
                    sb.Append(@"Online" + Environment.NewLine);
                }
                else
                {
                    sb.Append(@"Offline(" + nvr.Host.ErrorMsg + ")" + Environment.NewLine);
                }
                sb.Append(@"</td>" + Environment.NewLine);
                sb.Append(@"</tr>" + Environment.NewLine);
                foreach (IPC ipc in nvr.IPCs)
                {
                    if (ipc.Status == 0)
                    {
                        sb.Append(@"<tr class=""danger"">" + Environment.NewLine);

                    }
                    else
                    {
                        sb.Append(@"<tr>" + Environment.NewLine);
                    }
                    sb.Append(@"<td>" + Environment.NewLine);
                    sb.Append(ipc.ChannelIndex + Environment.NewLine);
                    sb.Append(@"</td>" + Environment.NewLine);
                    sb.Append(@"<td>" + Environment.NewLine);
                    sb.Append(ipc.ChannelName + Environment.NewLine);
                    sb.Append(@"</td>" + Environment.NewLine);
                    sb.Append(@"<td>" + Environment.NewLine);
                    sb.Append(ipc.ChannelNumber + Environment.NewLine);
                    sb.Append(@"</td>" + Environment.NewLine);
                    sb.Append(@"<td>" + Environment.NewLine);
                    sb.Append(ipc.ChannelIp + Environment.NewLine);
                    sb.Append(@"</td>" + Environment.NewLine);
                    sb.Append(@"<td>" + Environment.NewLine);
                    sb.Append((Status)ipc.Status + Environment.NewLine);
                    sb.Append(@"</td>" + Environment.NewLine);
                    sb.Append(@"</tr>" + Environment.NewLine);
                }
            }
            sb.Append("</table></div>" + Environment.NewLine);
            return sb.ToString();
        }
 
    }
}
