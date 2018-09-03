using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 删除下载任务
    /// </summary>
    class DeleteDownloadTaskActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;
        private string _taskId;

        public DeleteDownloadTaskActionHandler()
        {

        }

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
            try
            {
                this.DeleteDownloadTask();
            }
            catch (Exception)
            {
                result.Flag = 1;
                result.Description = "调用失败";
                throw;
            }
            return result;
        }
        /// <summary>
        /// 初始化关注报表需要的参数信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="actionType"></param>
        /// <param name="parameters"></param>
        /// <param name="responseXml"></param>
        private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        {
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            this._taskId = TokenTransfer.GetInformationByKey("taskId", parameters);
        }

        /// <summary>
        /// 删除下载任务
        /// </summary>
        /// <returns></returns>
        private void DeleteDownloadTask()
        {
            string sql =
                   string.Format(
                       "DELETE FROM UAP_MobileReportTask WHERE taskId = @TaskID and userId = @UserID");
            SqlCommand cmd = new SqlCommand(sql) { CommandType = CommandType.Text };
            cmd.Parameters.Add(SqlHelper.GetParameter("@TaskID", SqlDbType.NVarChar, 100, this._taskId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._loginInfo.UserID));
            SqlHelper.ExecuteNonQuery(this._loginInfo.UfDataCnnString, cmd);
        }
    }
}
