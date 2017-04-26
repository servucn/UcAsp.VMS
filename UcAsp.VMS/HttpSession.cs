/***************************************************
*创建人:rixiang.yu
*创建时间:2017/3/22 11:53:35
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
    public class HttpSession
    {
        public HttpSession()
        {
            this.SessionId = this.CreateSessionId();
            this.RefreshLastTime();
        }

        private String CreateSessionId()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
        /// <summary>
        /// Id
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// 是否已验证通过
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// 用户唯一标识符
        /// </summary>
        public string UserId { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// 最后请求访问时间
        /// </summary>
        public DateTime LastTime { get; set; }

        public void RefreshLastTime()
        {
            this.LastTime = DateTime.Now;
        }

        public int ElapsedMinutes
        {
            get
            {
                TimeSpan ts = DateTime.Now - LastTime;
                return Convert.ToInt32(ts.TotalMinutes);
            }
        }
    }

}
