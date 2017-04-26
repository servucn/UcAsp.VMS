using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
namespace UcAsp.VMS
{
    class Program
    {
        static void Main(string[] args)
        {
            // DeviceInitialize device = new DeviceInitialize();
            //  device.Initialilize();
            // List<NVRInfo> list = device.NVRInfos;
            
            //List<Channel> channels = new List<Channel>();
            //Channel request1 = new Channel { Nvr_Ip = "192.168.2.2", PackageUkid = "999", End_Time = DateTime.Now.AddMinutes(-5), Start_Time = DateTime.Now.AddMinutes(-11), Channel_Ip = "192.168.1.9" };
            //channels.Add(request1);
            //Channel request4 = new Channel { Nvr_Ip = "192.168.2.2", PackageUkid = "999", End_Time = DateTime.Now.AddMinutes(-5), Start_Time = DateTime.Now.AddMinutes(-10), Channel_Ip = "192.168.1.99" };
            //channels.Add(request4);
            //Channel request2 = new Channel { Nvr_Ip = "192.168.2.1", PackageUkid = "999", End_Time = DateTime.Now.AddMinutes(-20), Start_Time = DateTime.Now.AddMinutes(-25), Channel_Ip = "192.168.1.38" };
            //channels.Add(request2);
            //Channel request3 = new Channel { Nvr_Ip = "192.168.2.1", PackageUkid = "999", End_Time = DateTime.Now.AddMinutes(-21), Start_Time = DateTime.Now.AddMinutes(-27), Channel_Ip = "192.168.1.35" };
            //channels.Add(request3);
             string _config = AppDomain.CurrentDomain.BaseDirectory + "iscs.config";
            BootStrap strap = new BootStrap();
            strap.Start(_config);
            //string fileName = DateTime.Now.Ticks.ToString();
            //bool resgobal = strap.RequestGobal(channels, fileName);
            //FFmpeg.MergeVideo(fileName+"_gobal");
            //List<string> localfile = new List<string>();
            //localfile.Add(@"F:\\video\\71918290\\719182900000106.AVI");
            //localfile.Add(@"F:\\video\\71918290\\719182900000103.AVI");
            //bool reslocal = strap.RequestLocal(719182900000106, localfile);
            //FFmpeg.MergeVideo("719182900000106_local");
            //Console.WriteLine(resgobal);


            Console.ReadKey();
        }
    }
}
