using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using U8Login;
using UFIDA.U8.MERP.MerpContext;
using UFIDA.U8.UAP.Services.ReportData;
using UFSoft.U8.Framework.LoginContext;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public static class TokenTransfer
    {
        /// <summary>
        ///     通过移动端传递过来的token解析本地登录对象
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static U8LoginInfor GetLoginInfo(string token)
        {
            string userToken = ContextManager.SingletonInstance.GetCurrentUserToken(token);
            var vblogin = new clsLogin();
            vblogin.ConstructLogin(userToken);
            string taksId = token;
            vblogin.set_TaskId(ref taksId);
            return new U8LoginInfor(vblogin);
        }

        /// <summary>
        ///     获取参数信息
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetInformationByKey(string p, Dictionary<string, string> parameters)
        {
            if (parameters.Keys.Contains(p))
                return parameters[p];
            return string.Empty;
        }

        public static U8LoginInfor DoLogin(object login)
        {
            var _u8LoginCls = login as clsLogin;
            var loginnet = new UFSoft.U8.Framework.Login.UI.clsLogin();
            var _userdata = loginnet.GetLoginInfo(_u8LoginCls.userToken);
            return new U8LoginInfor(login);
        }

        public static U8LoginInfor DoLogin(string cAccId, string cYear, string appServer, string dataSource,
            string sdate, string userid, string pass, string curlangid)
        {
            bool bmerp = false;
            string subid = "AS";
            if (userid.Trim().Length == 0)
            {
                userid = "ASUSER";
                pass = "asuser";
            }
            else
                bmerp = true;
            string serial = "";
            string accid = dataSource + "@" + cAccId;
            string year = cYear;
            string appserver = appServer;
            if (sdate.Trim().Length == 0)
                sdate = DateTime.Now.ToString("yyyy-MM-dd");
            var login = new UFSoft.U8.Framework.Login.UI.clsLogin();
            if (curlangid.Length > 0)
                login.LanguageID = curlangid;
            if (login.login(subid, userid, pass, appserver, sdate, accid, serial))
            {
                #region 将login缓存到MERP中
                var token = "";
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(login.userToken);
                var doc = xmlDocument.DocumentElement;
                var tokennode = doc.SelectSingleNode("/ufsoft/data/SignedToken");
                if (tokennode != null)
                {
                    token = tokennode.Attributes["id"].Value;
                }
                ContextObj context = new ContextObj();
                context.Login = login;
                ContextManager.SingletonInstance.Add(token, context);
                #endregion
                clsLogin _u8LoginCls = new clsLoginClass();
                _u8LoginCls.ConstructLogin(login.userToken);
                return new U8LoginInfor(_u8LoginCls);
                //m_LoginCollection[hash] = login.userToken;
                //logger.Info(m_LoginCollection[hash].GetType().FullName);
                //_userdata = login.GetLoginInfo();
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("登陆失败,错误信息:");
                sb.AppendLine("subid:" + subid);
                sb.AppendLine("accid:" + accid);
                sb.AppendLine("year:" + year);
                sb.AppendLine("userid:" + userid);
                sb.AppendLine("pass:" + pass);
                sb.AppendLine("sdate:" + sdate);
                sb.AppendLine("appserver:" + appserver);
                sb.AppendLine("serial:" + serial);
                sb.AppendLine("login.ErrDescript:" + login.ErrDescript);
                throw new Exception(sb.ToString());
            }
        }
    }
}
