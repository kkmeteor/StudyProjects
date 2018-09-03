using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.MA.SchemaResolve;
using UFIDA.U8.MERP.MerpContext;
using UFIDA.U8.UAP.Services.ReportData;
using UFSoft.U8.Framework.Login.UI;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 获取报表列表
    /// </summary>
    public class GetReportListActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;

        public GetReportListActionHandler()
        {

        }
        public override ActionResult Execute(string token, string actionType, Dictionary<string, string> parameters,
                                             ref string responseXml)
        {
            var result = new ActionResult()
            {
                Action = actionType,
                Flag = 1,
                Description = "调用失败",
                ResultData = null
            };
            var time = System.DateTime.Now;
            System.Diagnostics.Trace.Write("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList TaskID: " + token + " Start:" + time.ToString());
            var ds = new DataSet();

            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            System.Diagnostics.Trace.Write("<<<<<<<<<<MobileReportTest>>>>>>>>>>TokenTransfer处理之后-->token=: " + token);
            System.Diagnostics.Trace.Write("<<<<<<<<<<MobileReportTest>>>>>>>>>>Login.UfMetaCnnString-->" + this._loginInfo.UfMetaCnnString);
            //TimerTask timer = new TimerTask(1000, this._loginInfo);
            try
            {
                ds = this.GetDataFromDb(false, false);
            }
            catch (Exception ex)
            {
                result.Description = "获取模块信息失败:" + ex.Message;
                return result;//代码走查点修改
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = "ReportListInfo";
            }
            const string templateId = "getReportList";
            var service = new SchemaServiceForNet();
            result.ResultData = service.MakeSchema(token, templateId, ds, null);
            result.Flag = 0;
            result.Description = "查询报表列表信息成功！";
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList  TaskID: " + token + " End:" + System.DateTime.Now.ToString());
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetReportList TaskID: " + token + "  Use Time " + (DateTime.Now - time).ToString());
            return result;
        }

        public string GetReportList(string token)
        {
            var result = new ActionResult()
            {
                Action = "",
                Flag = 1,
                Description = "调用失败",
                ResultData = null
            };
            var time = System.DateTime.Now;
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList TaskID: " + token + " Start:" + time.ToString());
            var ds = new DataSet();
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            //TimerTask timer = new TimerTask(1000, this._loginInfo);
            try
            {
                ds = this.GetDataFromDb(false, false);
            }
            catch (Exception ex)
            {
                result.Description = "获取模块信息失败:" + ex.Message;
                return result.Description;//代码走查点修改
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = "ReportListInfo";
            }
            const string templateId = "getReportList";
            #region 将login缓存到MERP中
            var token1 = "";
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(token);
            var doc = xmlDocument.DocumentElement;
            var tokennode = doc.SelectSingleNode("/ufsoft/data/SignedToken");
            if (tokennode != null)
            {
                token1 = tokennode.Attributes["id"].Value;
            }
            var login = new UFSoft.U8.Framework.Login.UI.clsLogin();

            var userData = login.GetLoginInfo(token);
            login.login(userData.cSubID, userData.UserId, userData.Password, userData.AppServer, userData.operDate, userData.DataSource, userData.WorkStationSerial, false);
            ContextObj context = new ContextObj();
            context.Login = login;
            //context.Login = _u8LoginCls as UFSoft.U8.Framework.Login.UI.clsLogin;
            ContextManager.SingletonInstance.Add(token1, context);
            #endregion
            var service = new SchemaServiceForNet();
            result.ResultData = service.MakeSchema(token1, templateId, ds, null);
            result.Flag = 0;
            result.Description = "查询报表列表信息成功！";
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList  TaskID: " + token1 + " End:" + System.DateTime.Now.ToString());
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetReportList TaskID: " + token1 + "  Use Time " + (DateTime.Now - time).ToString());
            return result.ResultData;
        }
        /// <summary>
        /// 从数据库中读取列表
        /// </summary>
        /// <param name="is4Web"></param>
        /// <param name="is4Ref"></param>
        /// <returns></returns>
        private DataSet GetDataFromDb(bool is4Web, bool is4Ref)
        {
            var cmd = new SqlCommand("UAP_Report_GetMobileReports") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._loginInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this._loginInfo.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this._loginInfo.cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleId", SqlDbType.NVarChar, 10, this._loginInfo.LocaleID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Is4Web", SqlDbType.Bit, is4Web));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Is4Ref", SqlDbType.Bit, is4Ref));
            return SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, cmd);
        }

        public string GetReportList(U8LoginInfor login)
        {
            var result = new ActionResult()
            {
                Action = "",
                Flag = 1,
                Description = "调用失败",
                ResultData = null
            };
            var time = System.DateTime.Now;
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList TaskID: " + login.TaskID + " Start:" + time.ToString());
            var ds = new DataSet();
            this._loginInfo = login;
            //TimerTask timer = new TimerTask(1000, this._loginInfo);
            try
            {
                ds = this.GetDataFromDb(false, false);
            }
            catch (Exception ex)
            {
                result.Description = "获取模块信息失败:" + ex.Message;
                return result.Description;//代码走查点修改
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = "ReportListInfo";
            }
            const string templateId = "getReportList";
            var token = "";
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(login.UserToken);
            var doc = xmlDocument.DocumentElement;
            var tokennode = doc.SelectSingleNode("/ufsoft/data/SignedToken");
            if (tokennode != null)
            {
                token = tokennode.Attributes["id"].Value;
            }

            var service = new SchemaServiceForNet();
            result.ResultData = service.MakeSchema(token, templateId, ds, null);
            result.Flag = 0;
            result.Description = "查询报表列表信息成功！";
            //System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>GetReportList-->GetReportList  TaskID: " + token + " End:" + System.DateTime.Now.ToString());
            //System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetReportList TaskID: " + token + "  Use Time " + (DateTime.Now - time).ToString());
            var xmlResult = result.ResultData;
            var doc1 = new XmlDocument();
            doc1.LoadXml(xmlResult);
            var dataXML = doc1.SelectSingleNode("/struct/reportList");
            string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(dataXML);
            result.ResultData = json;
            string finalResult = JsonTransfer.ReportListToJson(result);
            return finalResult;
        }
    }
}
