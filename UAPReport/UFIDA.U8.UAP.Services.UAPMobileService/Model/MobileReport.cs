using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 移动报表对象
    /// </summary>
    [Serializable]
    public class MobileReport
    {
        /// <summary>
        /// 当前一页结束
        /// </summary>
        public bool PageEnd { get; set; }
        /// <summary>
        /// report对象
        /// </summary>
        public Report Report { get; set; }
        /// <summary>
        /// report数据
        /// </summary>
        public SemiRows SemiRows { get; set; }
        /// <summary>
        /// report结束标志
        /// </summary>
        public bool ReportDataEnd { get; set; }
        /// <summary>
        /// 已经加载的分页
        /// 0为首页
        /// </summary>
        public int Page { get; set; }
    }
}
