using System;
using System.Collections.Generic;
using System.Text;


using UFIDA.U8.Portal.Proxy.editors;
using UFIDA.U8.Portal.Proxy.supports;
using UFIDA.U8.Portal.Proxy.Accessory;


namespace UFIDA.U8.Portal.ReportFacade
{
    public class ReportFormInput : NetFormInput
    {
        public ReportFormInput(string key, Command command)
                    : base(key, command)
        {

        }

        public object GetU8Login()
        {
            return base.Command.Args.ExtProperties["U8Login"]; ;
        }

        public string GetReportId()
        {
            string reportId = base.Command.Args.ExtProperties["ReportId"].ToString();
            //reportId = "SA[__]15946f1d-d56d-447b-9e0f-5191be9837f0";
            //reportId = "SA[__]f761304b-70e7-4aa9-8bf7-7ca0d5672a4a";
            //System.Diagnostics.Trace.WriteLine("Back from portal :..............."+ reportId);

            return reportId;

        }

        public object GetRawFilter()
        {
            return base.Command.Args.ExtProperties["RawFilter"];
        }

        public bool IsShowUI()
        {
            return (bool)base.Command.Args.ExtProperties["IsShowUI"];
        }

        public int GetViewType()
        {
            return (int)base.Command.Args.ExtProperties["ViewType"];
        }
        
        public bool IsStaticReport()
        {
            if (base.Command.Args.ExtProperties.Contains("StaticReport"))
                return true;

            return false;
        }

        public void FireActionsChange(ButtonChangeType changeType)
        {
            base.OnActionsChange(changeType, base.Actions);
        }

       
    }
}
