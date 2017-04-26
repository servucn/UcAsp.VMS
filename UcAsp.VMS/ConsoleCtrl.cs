/***************************************************
*创建人:TecD02
*创建时间:2017/2/3 16:40:35
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
using System.Runtime.InteropServices;
using log4net;
using System.Threading;
namespace UcAsp.VMS
{
    public class ConsoleCtrl
    {
        public static ILog _log = LogManager.GetLogger("ConsoleCtrl");
        #region Console 
        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(int handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern int GetLastError();
        public int ErrorMsg { get; set; }
        #endregion
        public ConsoleCtrl(int id)
        {
            FreeConsole();
            bool result = AttachConsole(id);
            bool set = SetConsoleCtrlHandler(0, true);
            bool ctrl = GenerateConsoleCtrlEvent(0, 0);
            int ErrorMsg = GetLastError();
            _log.Error(ErrorMsg);
            Thread.Sleep(200);
            SetConsoleCtrlHandler(0, false);
            AttachConsole(-1);
            FreeConsole();
            GC.Collect();
        }

    }
}
