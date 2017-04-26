/***************************************************
*创建人:TecD02
*创建时间:2017/1/7 18:46:27
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
namespace UcAsp.VMS
{
    public class CoreThread
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(CoreThread));
        public HttpServer Server { get; set; }
        // public BootStrap Strap { get; set; }
        public void Task(object requestinfo)
        {
            try
            {
                ReuestInfo param = (ReuestInfo)requestinfo;
                if (param == null)
                {
                    param = new ReuestInfo();
                }
                MergeResult NvrVideo = new MergeResult();
                MergeResult CifsVideo = new MergeResult();
                string nvrstatus = "In Progress";
                string nvrmsg = "In Progress";
                string cifstatus = string.Empty;
                string cifmsg = string.Empty;
                string m = "{\"result\": \"ok\",\"package-id\": \"" + param.PackageUkid + "\",\"request-id\": \"" + param.RequestId + "\",\"urls\": [{\"status\": \"" + nvrstatus + "\",\"msg\":\"" + nvrmsg + "\"},{\"status\":\"" + cifstatus + "\",\"msg\":\"" + cifmsg + "\"}]}";
                if (Server.Progress.ContainsKey(param.RequestId))
                {

                    Server.Progress[param.RequestId] = m;
                }
                else
                {
                    Server.Progress.Add(param.RequestId, m);

                }


                nvrmsg = "Downloading";
                Server.Progress[param.RequestId] = m;
                Server.Strap.RequestGobal(param.Channels, param.PackageUkid + "_" + param.Hash);

                nvrmsg = "Downloaded";
                Server.Progress[param.RequestId] = m;

                NvrVideo = FFmpeg.MergeVideo(param.PackageUkid + "_" + param.Hash + "_gobal");

                Server.Progress[param.RequestId] = m;

                nvrstatus = "ok";
                cifmsg = "Downloaded";

                Server.Strap.RequestLocal(long.Parse(param.PackageUkid));
                Server.Progress[param.RequestId] = m;
                _log.Info("NvrVideo" + NvrVideo.Result);

                CifsVideo = FFmpeg.MergeVideo(param.PackageUkid.ToString() + "_local");
                NvrVideo.PackageUkid = long.Parse(param.PackageUkid);
                CifsVideo.PackageUkid = long.Parse(param.PackageUkid);
                cifmsg = "Merged";
                Server.Progress[param.RequestId] = m;
                _log.Info("CifsVideo" + CifsVideo.Result);

                string result = new ReturnResult().Json(NvrVideo, CifsVideo, param.RequestId);
                _log.Info(param.RequestId + "." + result);

                if (Server.Result.ContainsKey(param.RequestId))
                {
                    Server.Result[param.RequestId] = result;
                    _log.Info("Modify:" + param.RequestId);
                }
                else
                {
                    Server.Result.Add(param.RequestId, result);
                    _log.Info("Add:" + param.RequestId);
                }
                _log.Info("Progress:" + Server.Progress[param.RequestId]);
                // Server.Progress[param.RequestId] = string.Format(m, param.PackageUkid, param.RequestId, "ok", "Downloaded", "ok", "Merged");

                _log.Info("Progress:" + Server.Progress[param.RequestId]);

            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }


        }
    }
}
