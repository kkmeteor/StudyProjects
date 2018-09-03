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
    /// 获取下载任务集合
    /// </summary>
    class GetDownloadTaskListActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;

        public override ActionResult Execute(string token, string actionType, Dictionary<string, string> parameters,
                                             ref string responseXml)
        {
            var result = new ActionResult()
            {
                Action = actionType,
                Flag = 0,
                Description = "调用成功",
                ResultData = null
            };
            this.Init(token, actionType, parameters, ref responseXml);
            this.GetDownloadTaskList(token, ref result);
            return result;
        }

        private void GetDownloadTaskList(string token, ref ActionResult result)
        {
            var ds = new DataSet();
            try
            {
                string sql = string.Format("SELECT * FROM dbo.UAP_MobileReportTask WHERE UserID = @UserID");
                var command = new SqlCommand(sql) { CommandType = CommandType.Text };
                command.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 50, this._loginInfo.UserID));
                ds = SqlHelper.ExecuteDataSet(this._loginInfo.UfDataCnnString, command);
            }
            catch (Exception ex)
            {
                result.Description = "获取下载列表失败:" + ex.Message;
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = "DownloadTaskListInfo";
            }
            const string templateId = "getDownloadTaskList";
            var service = new SchemaServiceForNet();
            result.ResultData = service.MakeSchema(token, templateId, ds, null);
            result.Flag = 0;
            result.Description = "获取下载列表成功！";
        }
        private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        {
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            TokenTransfer.GetInformationByKey("taskId", parameters);
        }
    }
}
