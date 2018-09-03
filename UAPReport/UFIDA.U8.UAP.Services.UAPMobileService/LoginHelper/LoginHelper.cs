using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UFIDA.U8.UAP.Services.UAPMobileService.LoginHelper
{
    public class LoginHelper
    {
        public static string LoginU8()
        {
            string cAccId = "999";
            string cYear = "2015";
            string appServer = "localhost";
            string dataSource = "(default)";
            string sdata = "2015/08/14";
            string userid = "ASUSER";
            string pass = "asuser";
            string curlangid = "zh-CN";
            var login = TokenTransfer.DoLogin(cAccId, cYear, appServer, dataSource, sdata, userid, pass, curlangid);
            var userToken = login.UserToken;
            return userToken;
        }
        public static string LoginU8(string cAccId, string cYear, string appServer, string dataSource, string sdata, string userid, string pass, string curlangid)
        {
            var login = TokenTransfer.DoLogin(cAccId, cYear, appServer, dataSource, sdata, userid, pass, curlangid);
            var userToken = login.UserToken;
            return userToken;
        }
    }
}
