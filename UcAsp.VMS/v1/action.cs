/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 11:26:24
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
using Newtonsoft.Json;
using System.Globalization;
namespace UcAsp.VMS.v1
{
    public class action
    {

        public Tuple<string, UcAsp.VMS.ReuestInfo> requestnvrplaybackvideourl(string content)
        {


            Dictionary<string, string> request = new Dictionary<string, string>();
            string[] _r = Regex.Split(content, "&");
            for (int s = 0; s < _r.Length; s++)
            {
                string[] _s = Regex.Split(_r[s], "=");
                if (_s.Length == 2)
                {
                    request.Add(_s[0], _s[1]);
                }
            }
            if (!request.ContainsKey("request-device-list"))
            {
                return new Tuple<string, UcAsp.VMS.ReuestInfo>("Missing 'request-device-list'", null);
            }
            List<v1.requestinfo> param2 = JsonConvert.DeserializeObject<List<v1.requestinfo>>(request["request-device-list"]);
            if (!request.ContainsKey("package-id"))
            {
                return new Tuple<string, UcAsp.VMS.ReuestInfo>("Missing 'package-id'", null);
            }

            ReuestInfo param = new ReuestInfo();
            param.PackageUkid = request["package-id"];
            param.Channels = new List<Channel>();
            int i = 0;
            CultureInfo culture = new CultureInfo("zh-CN");
            foreach (v1.requestinfo info in param2)
            {
                foreach (v1.channels c in info.channels)
                {
                    Channel cl = new Channel();
                    cl.NvrIp = info.nvr_ip;
                    cl.No = i;
                    cl.ChannelIp = c.channel_ip;
                    cl.StartTime = DateTime.ParseExact(c.start_time, "ddMMyyyyHHmmss", culture, DateTimeStyles.AdjustToUniversal).AddHours(8);
                    cl.EndTime = DateTime.ParseExact(c.end_time, "ddMMyyyyHHmmss", culture, DateTimeStyles.AdjustToUniversal).AddHours(8);
                    i++;
                    param.Channels.Add(cl);
                }

            }
            return new Tuple<string, UcAsp.VMS.ReuestInfo>("", param);
        }
    }
}
