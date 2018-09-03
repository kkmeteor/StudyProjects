using System;
using System.Collections.Generic;
using System.Text;
using U8Login;
using System.Diagnostics;

namespace UFIDA.U8.UAP.Services.ReportData
{
    public class ReportLoginManager
    {
        public static object GetLogin( string userToken,string taskId,string subid )
        {
            string accountId = string.Empty;
            string yearId = string.Empty;
            string userId = string.Empty;
            string password = string.Empty;
            string date = string.Empty;
            string server = string.Empty;
            string serial = string.Empty;
            Trace.WriteLine("begin create object : U8Login._clsLogin");
            U8Login._clsLogin login = new U8Login.clsLoginClass();
            login.ConstructLogin(userToken);
            login.set_TaskId(ref taskId);
            if (!login.Login(ref subid, ref accountId, ref yearId, ref userId, ref password, ref date, ref server, ref serial))
            {
                System.Diagnostics.Trace.WriteLine("UserToken:  " + userToken);
                System.Diagnostics.Trace.WriteLine(login.ShareString);
                throw new Exception(login.ShareString);
            }
            Trace.WriteLine("ReportLoginManager::GetLogin()½áÊø");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(login.LanguageRegion );
            return login;
        }
    }

    
}
