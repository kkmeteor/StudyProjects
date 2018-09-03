using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class MobileReportDownloadTask
    {
        public string TaskId { get; set; }
        public int Status { get; set; }
        public string SolutionId { get; set; }
        public string Condition { get; set; }
        public string UserId { get; set; }
        public string Url { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime CompletedOn { get; set; }

        public string Token { get; set; }
        public string SubId { get; set; }
        public string Pass { get; set; }
        public string AppServer { get; set; }
        public string AccId { get; set; }
        public string DataSource { get; set; }
        public string Syear { get; set; }
        public string Sdate { get; set; }
    }
}
