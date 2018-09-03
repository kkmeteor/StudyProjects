using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class OpenReportChartActionHandler
    {
        public string Execute(ReportData.U8LoginInfor u8login, string p, Dictionary<string, string> param, ref string response)
        {
            OpenReportActionHandler handler = new OpenReportActionHandler();
            if (!param.ContainsKey("is4chart"))
                param.Add("is4chart", "true");
            return handler.Execute(u8login, p, param, ref response);
        }
    }
}
