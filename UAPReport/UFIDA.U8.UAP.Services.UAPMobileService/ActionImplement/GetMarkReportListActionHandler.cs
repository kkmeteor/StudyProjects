using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.MA.SchemaResolve;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 获取关注列表
    /// </summary>
    class GetMarkReportListActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;
        private string _solutionId;
        public GetMarkReportListActionHandler()
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
            this.Init(token, actionType, parameters, ref responseXml);
            this.GetMarkReportList(token, ref result);
            return result;
        }

        private void GetMarkReportList(string token, ref ActionResult result)
        {
            var ds = new DataSet();
            try
            {
                ds = this.GetDataFromDb();
            }
            catch (Exception ex)
            {
                result.Description = "获取模块信息失败:" + ex.Message;
                throw;
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = "ReportListInfo";
            }
            const string templateId = "getMarkReportList";
            var service = new SchemaServiceForNet();
            result.ResultData = service.MakeSchema(token, templateId, ds, null);
            result.Flag = 0;
            result.Description = "查询报表列表信息成功！";
        }

        private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        {
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            this._solutionId = TokenTransfer.GetInformationByKey("solutionId", parameters);
        }
        /// <summary>
        /// 从数据库中读取列表
        /// </summary>
        /// <param name="is4Web"></param>
        /// <param name="is4Ref"></param>
        /// <returns></returns>
        private DataSet GetDataFromDb()
        {
            var cmd = new SqlCommand("UAP_Report_GetMarkedMobileReports") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._loginInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this._loginInfo.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this._loginInfo.cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleId", SqlDbType.NVarChar, 10, this._loginInfo.LocaleID));
            return SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, cmd);
        }
    }
}
