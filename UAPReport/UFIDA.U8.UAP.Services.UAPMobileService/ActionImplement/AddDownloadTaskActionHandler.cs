using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.MERP.MerpContext;
using UFIDA.U8.UAP.Services.ReportData;
using UFSoft.U8.Framework.LoginContext;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    class AddDownloadTaskActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;
        private string _solutionId;
        private string _taskId;
        private string _queryParams;

        public AddDownloadTaskActionHandler()
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
            if (string.IsNullOrEmpty(this._solutionId))//如果方案ID为空则不执行插入操作直接抛给手机端
            {
                result.Flag = 1;
                result.Description = "调用失败,报表方案ID不能为空!";
                return result;
            }
            try
            {
                this.AddDownloadTask(token);
                //MobileReportDownloadTask task = MobileReportDownloadEngine.GetDownloadTask(this._loginInfo);
                //task.LoginInfo = _loginInfo;
                //TimerTask.Engine.Tasks.Add(task);
                if (TimerTask.Status == 0)
                {
                    TimerTask.Time = 1000;
                    TimerTask.CreatTimerTask(token);
                }
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
        /// 增加下载任务
        /// </summary>
        /// <returns></returns>
        private void AddDownloadTask(string token)
        {
            var login = ContextManager.SingletonInstance.GetCurrentLogin(token);
            UserData userData = login.GetLoginInfo();
            string password = userData.Password;
            string appserver = userData.AppServer;
            string accid = userData.AccID;
            string subid = userData.cSubID;
            string datasource = userData.DataSource;
            var year = userData.iYear;
            var sdate = userData.operDate;
            string sql =
                   string.Format(@"IF NOT EXISTS (SELECT 1 FROM UAP_MobileReportTask  WHERE TaskID = @TaskId AND UserID = @UserID) 
                       insert into UAP_MobileReportTask ([TaskID],[Status],[SolutionID],[Condition],[UserID],[Url],[CreatedOn],[CompletedOn],[Token],[pass],[appserver],[accid],[subid],[datasource],[syear],[sdate]) 
                        values (@TaskID,@Status,@SolutionID,@Condition,@UserID,@Url,@CreatedOn,@CompletedOn,@Token,@pass,@appserver,@accid,@subid,@datasource,@syear,@sdate)");
            var cmd = new SqlCommand(sql) { CommandType = CommandType.Text };
            cmd.Parameters.Add(SqlHelper.GetParameter("@TaskId", SqlDbType.NVarChar, 100, this._taskId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 50, this._loginInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Status", SqlDbType.Int, 2, 0));
            cmd.Parameters.Add(SqlHelper.GetParameter("@SolutionID", SqlDbType.NVarChar, 50, this._solutionId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Condition", SqlDbType.NVarChar, 4000, this._queryParams));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Url", SqlDbType.NVarChar, 1000, ""));
            cmd.Parameters.Add(SqlHelper.GetParameter("@CompletedOn", SqlDbType.NVarChar, 100, ""));
            cmd.Parameters.Add(SqlHelper.GetParameter("@CreatedOn", SqlDbType.NVarChar, 100, DateTime.Now.ToString()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Token", SqlDbType.VarChar, 50, token));
            cmd.Parameters.Add(SqlHelper.GetParameter("@pass", SqlDbType.VarChar, 50, password));
            cmd.Parameters.Add(SqlHelper.GetParameter("@appserver", SqlDbType.VarChar, 50, appserver));
            cmd.Parameters.Add(SqlHelper.GetParameter("@accid", SqlDbType.VarChar, 50, accid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@subid", SqlDbType.VarChar, 50, subid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@datasource", SqlDbType.VarChar, 50, datasource));
            cmd.Parameters.Add(SqlHelper.GetParameter("@syear", SqlDbType.VarChar, 4, year));
            cmd.Parameters.Add(SqlHelper.GetParameter("@sdate", SqlDbType.VarChar, 10, sdate));
            SqlHelper.ExecuteNonQuery(this._loginInfo.UfDataCnnString, cmd);
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
            if (string.IsNullOrEmpty(this._taskId))
                this._taskId = Guid.NewGuid().ToString();
            this._solutionId = TokenTransfer.GetInformationByKey("solutionId", parameters);
            this._queryParams = TokenTransfer.GetInformationByKey("queryParams", parameters);
        }
    }
}
