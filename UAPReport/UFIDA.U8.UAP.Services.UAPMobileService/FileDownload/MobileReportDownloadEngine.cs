using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using UFIDA.U8.UAP.Services.ReportData;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    /// <summary>
    /// 移动报表后台计算引擎
    /// </summary>
    public class MobileReportDownloadEngine
    {
        private U8LoginInfor _loginInfo;// u8登陆对象
        public bool Busy = false;//下载后台计算状态
        private MobileReport _mobileReport;
        private MobileReportDownloadTask _task;


        /// <summary>
        /// 创建下载计算引擎并开始下载任务
        /// </summary>
        /// <param name="loginInfor">u8登陆对象</param>
        public void CreatDownLoadEngine(U8LoginInfor loginInfor)
        {
            Busy = true;
            _loginInfo = loginInfor;
            BeginTask();
        }

        /// <summary>
        /// 获取报表下载任务并开始进行下载数据准备
        /// </summary>
        /// <param name="loginInfor">U8登陆对象</param>
        public void BeginTask(U8LoginInfor loginInfor = null)
        {
            try
            {
                this._task = GetDownloadTask(_loginInfo);
                if (_task != null)
                {
                    if (GoTask(_task))
                        Busy = false;
                }
                else
                {
                    Busy = false;
                }
            }
            catch (Exception)
            {
                Busy = false;
            }
        }

        /// <summary>
        /// 按时间次序获取一个下载任务
        /// </summary>
        /// <returns>一个下载任务</returns>
        public static MobileReportDownloadTask GetDownloadTask(U8LoginInfor loginInfo)
        {
            string sql = string.Format("SELECT TOP 1 * FROM dbo.UAP_MobileReportTask WHERE Status in( 0,1) ORDER BY CreatedOn");
            DataSet ds = SqlHelper.ExecuteDataSet(loginInfo.UfDataCnnString, sql);
            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }
            DataRow dr = ds.Tables[0].Rows[0];
            var task = new MobileReportDownloadTask()
            {
                TaskId = dr["TaskID"].ToString(),
                Status = int.Parse(dr["Status"].ToString()),
                SolutionId = dr["SolutionID"].ToString(),
                Condition = dr["Condition"].ToString(),
                UserId = dr["UserID"].ToString(),
                Url = dr["Url"].ToString(),
                CreateOn = (DateTime)dr["CreatedOn"],
                CompletedOn = (DateTime)dr["CompletedOn"],
                Token = dr["Token"].ToString(),
                SubId = dr["subid"].ToString(),
                Pass = dr["pass"].ToString(),
                AppServer = dr["appserver"].ToString(),
                AccId = dr["accid"].ToString(),
                DataSource = dr["datasource"].ToString(),
                Syear = dr["syear"].ToString(),
                Sdate = dr["sdate"].ToString()
            };
            sql =
                string.Format("UPDATE dbo.UAP_MobileReportTask SET Status = '{0}' WHERE TaskID = '{1}'", "1", task.TaskId);
            SqlHelper.ExecuteNonQuery(loginInfo.UfDataCnnString, sql);
            return task;
        }

        /// <summary>
        /// 启动定时任务，执行报表计算任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool GoTask(MobileReportDownloadTask task)
        {
            this._loginInfo = ConstructLogin(task);
            // 1.查询报表
            try
            {
                PrepairReportData(task);
            // 2.生成文件
            string fileName = string.Empty;
            MakeFile(ref fileName);
            // 3.更新数据库状态及URL
            UpdateDb(fileName);
            }
            catch (Exception)
            {
                // 如果在准备数据时发生异常，则将报表下载任务状态置为6，等待前端删除或者重新下载。
                PrepairReportDataError(task, this._loginInfo);
                throw;
            }
            return true;
        }

        /// <summary>
        /// 通过task中存储的用户登陆信息构建用户登陆对象
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private U8LoginInfor ConstructLogin(MobileReportDownloadTask task)
        {
            return TokenTransfer.DoLogin(task.AccId, task.Syear, task.AppServer, task.DataSource, task.Sdate, task.UserId, task.Pass, "zh-CN");
        }

        /// <summary>
        /// 如果在准备数据时发生异常，则将报表下载任务状态置为6，等待前端删除或者重新下载
        /// </summary>
        /// <param name="task">报表下载任务</param>
        private static void PrepairReportDataError(MobileReportDownloadTask task,U8LoginInfor login)
        {
            if (task != null && login != null)
            {
                string sql =
                    string.Format("UPDATE dbo.UAP_MobileReportTask SET Status = 6 WHERE TaskID = '{0}'", task.TaskId);
                SqlHelper.ExecuteNonQuery(login.UfDataCnnString, sql);
            }
        }

        /// <summary>
        /// 更新DB状态
        /// </summary>
        private void UpdateDb(string fileName)
        {
            const int status = 2; //报表数据完成状态
            // 示例http://10.1.43.36/U8AuditWebSite/MobileReport/ReportData(7b11e4e9-d7de-4a3c-aa8c-d0b73e528aaa).db
            string hostname = Dns.GetHostName();//得到本机名
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList.Length > 1 ? localhost.AddressList[1] : localhost.AddressList[0];
            IPAddress[] arrIpAddresses = Dns.GetHostAddresses(hostname);
            foreach ( IPAddress ip in arrIpAddresses )
            {
                if ( ip.AddressFamily.Equals ( AddressFamily.InterNetwork ) )
                {
                    localaddr = ip;
                }
            }
            string url = "http://" + localaddr + "/U8AuditWebSite/MobileReport/" + fileName;
            string dt = DateTime.Now.ToString();//文件完成时间，取服务器时间
            string sql =
                string.Format("UPDATE dbo.UAP_MobileReportTask SET Status = '{0}',Url = '{1}',CompletedOn = '{2}' WHERE TaskID = '{3}'", status, url, dt, _task.TaskId);
            SqlHelper.ExecuteNonQuery(_loginInfo.UfDataCnnString, sql);
        }

        /// <summary>
        /// 准备报表数据文件
        /// </summary>
        /// <param name="url"></param>
        private void MakeFile(ref string url)
        {
            DBOperator.SaveMobileReport(_mobileReport, ref url);
        }

        /// <summary>
        /// 准备报表数据
        /// </summary>
        private void PrepairReportData(MobileReportDownloadTask task)
        {
            //_loginInfo = TokenTransfer.GetLoginInfo(task.Token);
            MobileReportEngine engine = new MobileReportEngine(task, _loginInfo);
            _mobileReport = engine.GetAllReport(null, task.Condition);
        }

        /// <summary>
        /// 销毁当前任务
        /// </summary>
        internal void DestroyTask()
        {
            PrepairReportDataError(this._task, this._loginInfo);
            this._loginInfo = null;
            this._mobileReport = null;
            this._task = null;
            this.Busy = false;
        }
    }
}
