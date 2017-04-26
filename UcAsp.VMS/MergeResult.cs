/***************************************************
*创建人:TecD02
*创建时间:2016/12/30 19:51:43
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
    public class MergeResult
    {
        /// <summary>
        /// 错误编码 0,正常
        /// </summary>
        public int Error { get; set; }

        public long PackageUkid { get; set; }
        public string ErrMsg { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 帧数量
        /// </summary>
        public long Frame { get; set; }

        /// <summary>
        /// 视频时长
        /// </summary>
        public string Time { get; set; }

        public string File { get; set; }

        public bool Result { get; set; }
    }
}
