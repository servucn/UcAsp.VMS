/***************************************************
*创建人:TecD02
*创建时间:2016/12/30 19:48:47
*功能说明:<Function>
*版权所有:<Copyright>
*Frameworkversion:4.0
*CLR版本：4.0.30319.42000
***************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using log4net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace UcAsp.VMS
{
    public static class FFmpeg
    {
        public static ILog _log = LogManager.GetLogger("FFmpeg");
        private static string _ffmpeg = AppDomain.CurrentDomain.BaseDirectory + @"ffmpeg\ffmpeg.exe";
        public static string TempPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string Path = AppDomain.CurrentDomain.BaseDirectory;
        public static string FileSavePath = AppDomain.CurrentDomain.BaseDirectory + "\\Video\\";
        public static string Http = "http://127.0.0.1:8888/";

        public static MergeResult MergeVideo(string filename)
        {
            string error = string.Empty;
            if (!Directory.Exists(FileSavePath))
            {
                Directory.CreateDirectory(FileSavePath);
                _log.Info("创建视频保存录像");
            }
            if (File.Exists(FileSavePath + filename + ".mp4"))
            {
                _log.Info(filename + "已经存在");
                return new MergeResult() { File = Http + filename + ".mp4" };
            }
            _log.Info("\"" + _ffmpeg + "\"");
            _log.Info(@" -f  concat -safe 0 -i " + TempPath + filename + ".txt -c copy " + FileSavePath + filename + ".mp4");
            ProcessStartInfo oInfo = new ProcessStartInfo("\"" + _ffmpeg + "\"", @" -f  concat -safe 0 -i " + TempPath + filename + ".txt -c copy " + FileSavePath + filename + ".mp4");
            string output = string.Empty;
            RunProcess(@" -f  concat -safe 0 -i " + TempPath + filename + ".txt -c copy " + FileSavePath + filename + ".mp4", out output, true);

            if (string.IsNullOrEmpty(output))
            {
                return new MergeResult() { Result = false, ErrMsg = error };
            }
            else
            {
                string[] linestr = Regex.Split(output, "\r\n");
                string lastline = linestr[linestr.Length - 2];
                _log.Info(lastline);
                if (lastline.IndexOf("No such file or directory") > 0)
                {
                    return new MergeResult() { Result = false, ErrMsg = "文件不存在" };
                }
                else
                {
                    string preline = linestr[linestr.Length - 3];
                    _log.Info(preline);
                    string[] video = Regex.Split(lastline, ":");
                    if (video[0] == "video")
                    {
                        return new MergeResult() { Result = true, File = Http + "video/" + filename + ".mp4" };
                    }
                    else
                    {
                        return new MergeResult() { Result = false, ErrMsg = lastline };
                    }
                }
            }
        }

        public static bool RecordImage(NVRHostHiKv hikv, int channelindex, string filename)
        {
            string msg = string.Empty;

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            int id = RunProcess(@"-i rtsp://" + hikv.DVRUserName + ":" + hikv.DVRPassword + "@" + hikv.DVRIPAddress + "/h264/ch" + channelindex + @"/main/av_stream  -f image2 -ss 5 -vframes 1 -s 1920*1080 " + filename + " ", out msg, true);
            ColseProcess(id);
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static int RecordVideo(NVRHostHiKv hikv, int channelindex, long packageukid)
        {
            string msg = string.Empty;
            string filepath = packageukid.ToString();
            if (filepath.Length > 8)
            {
                filepath = filepath.Substring(0, 8);
            }
            if (!Directory.Exists(Path + "\\" + filepath + "\\"))
            {
                Directory.CreateDirectory(Path + "\\" + filepath + "\\");
            }

            int id = RunProcess(@"-i rtsp://" + hikv.DVRUserName + ":" + hikv.DVRPassword + "@" + hikv.DVRIPAddress + "/h264/ch" + channelindex + @"/main/av_stream  -vcodec copy " + Path + "\\" + filepath + "\\" + packageukid + ".mp4  ", out msg, false);


            return id;
        }

        private static int RunProcess(string para, out string msg, bool exit)
        {

            ProcessStartInfo oInfo = new ProcessStartInfo(_ffmpeg, para);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;

            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;
            oInfo.RedirectStandardInput = true;

            msg = string.Empty;
            StreamReader srOutput = null;
            int id = 0;

            try
            {
                Process proc = System.Diagnostics.Process.Start(oInfo);
                proc.ErrorDataReceived += Proc_ErrorDataReceived;
                id = proc.Id;
                if (exit)
                {
                    srOutput = proc.StandardError;
                    msg = srOutput.ReadToEnd();
                    _log.Info(msg);
                    proc.WaitForExit(60 * 1000 * 5);
                    proc.Close();
                }
            }
            catch (Exception ex)
            {
                msg = string.Empty;
            }
            finally
            {
                if (srOutput != null)
                {
                    srOutput.Close();
                    srOutput.Dispose();
                }


            }
            return (int)id;
        }

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        public delegate bool ConsoleCtrlDelegate(int dwCtrlType);
        public static int ColseProcess(int id)
        {

            ConsoleCtrl ctrl = new ConsoleCtrl(id);
            return ctrl.ErrorMsg;

        }
    }
}
