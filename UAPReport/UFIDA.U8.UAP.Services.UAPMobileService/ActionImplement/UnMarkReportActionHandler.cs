using System;
using System.Collections.Generic;
using System.Data;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 取消关注一张报表
    /// </summary>
    class UnMarkReportActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;
        private string _solutionId;
        public UnMarkReportActionHandler()
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
                this.UnMarkMobileReport();
            }
            catch (Exception ex)
            {
                result.Flag = 1;
                result.Description = "调用失败:" + ex.Message;
                throw;
            }
            return result;
        }

        private void UnMarkMobileReport()
        {
            if (!this.CheckData(this._solutionId))
            {
                throw new Exception("系统不存在该查询方案！");
            }
            string sql =
                string.Format(
                    "delete from UAP_Report_MarkedMobileReport where SolutionID = '{0}' and UserId = '{1}'",
                    this._solutionId, this._loginInfo.UserID);
            SqlHelper.ExecuteNonQuery(this._loginInfo.UfDataCnnString, sql);
        }
        /// <summary>
        /// 校验系统是否存在此方案
        /// </summary>
        /// <param name="solutionId"></param>
        /// <returns></returns>
        private bool CheckData(string solutionId)
        {
            string sql = string.Format("SELECT 1 FROM dbo.flt_Solution WHERE ID = '{0}'", solutionId);
            DataSet ds = SqlHelper.ExecuteDataSet(this._loginInfo.UfDataCnnString, sql);
            if (ds.Tables[0] != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 初始化取消关注报表需要的参数信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="actionType"></param>
        /// <param name="parameters"></param>
        /// <param name="responseXml"></param>
        private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        {
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            this._solutionId = TokenTransfer.GetInformationByKey("solutionId", parameters);
        }
    }
}
