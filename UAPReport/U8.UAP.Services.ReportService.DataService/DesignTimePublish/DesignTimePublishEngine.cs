using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
    internal class DesignTimePublishEngine
    {
        #region Parameter

        private static string _DbCnnString = string.Empty;

        #endregion

        #region Constructor

        private DesignTimePublishEngine() { }

        #endregion

        #region Exposed Interfeces

        #region Property

        public static string DbCnnString
        {
            set { _DbCnnString = value; }
        }

        #endregion

        #region Public Method

        public static bool Publish(string ReportID, string cAccId, string cYear, string userId, ReportPublicPosition location)
        {
            CheckCnnString();
            AddRptAndViewAuth(_DbCnnString, ReportID, cAccId, cYear, userId);
            SqlHelper.ExecuteNonQuery(_DbCnnString, GetCmdPublish(ReportID, ReportPublicPosition.CS));//总是只设置Cs为了事务一直行
            return true;
        }
        /// <summary>
        /// 发布到BS
        /// </summary>
        /// <param name="reportID"></param>
        /// <param name="cAccId"></param>
        /// <param name="cYear"></param>
        /// <param name="userId"></param>
        /// <param name="conString"></param>
        /// <returns></returns>
        public static bool PublishBS(string reportID, string cAccId, string cYear, string userId, U8LoginInfor u8LoginInfor)
        {
            CheckCnnString();
            
            //string menuId = reportID.GetHashCode().ToString();
            string menuId = reportID;
            string reportName = string.Empty;
            string twReportName = string.Empty;
            string enReportName = string.Empty;
            string filterid = string.Empty;
            string subId = string.Empty;
            string authId = string.Empty;
            GetReportSimpleInfo(reportID, out reportName, out twReportName, out enReportName, out filterid, out subId);
            ReportAuth auth = new ReportAuth(u8LoginInfor);
            authId = auth.GetReportAuthId(reportID) + "_01";//01是查询权限
            //authId = GetAuthIdByReport(reportID);
            StringBuilder sb = new StringBuilder();
            string parameter = string.Format("<property type='Report' reportid='{0}'></property>", reportID);
            //菜单
            sb.AppendFormat("insert into ua_menu(cMenu_id,cMenu_name,cmenu_Eng,cSub_id,iGrade,cSupMenu_id,bEndGrade,cAuth_id,iOrder,iImgIndex,Paramters,  Depends,Flag,isWebFlag,cimagename,cmenutype)" +
                                         "values('{0}'   ,'{1}'      ,null     ,'{2}'  ,'1'   ,'{3}'     ,1        ,'{4}'    ,1     ,1        ,@parameter    ,null   ,null,1,   'report_default.png','report');",
            menuId, reportName, "UA", subId, authId);
            //语言
            sb.AppendFormat("INSERT INTO UFMENU_BUSINESS_LANG(MenuId,Caption,Localeid) values('{0}','{1}','zh-cn');", menuId, reportName);
            sb.AppendFormat("INSERT INTO UFMENU_BUSINESS_LANG(MenuId,Caption,Localeid) values('{0}','{1}','zh-tw');", menuId, twReportName);
            sb.AppendFormat("INSERT INTO UFMENU_BUSINESS_LANG(MenuId,Caption,Localeid) values('{0}','{1}','en-us');", menuId, enReportName);
            
            //把菜单置为isweb
            sb.AppendFormat(" update ufsystem..ua_Menu set iswebflag=2 where cmenu_id= '{0}';", subId);
            sb.AppendFormat("update  UFMeta_{0}..uap_report set MappingBsMenuId='{1}'where id='{2}'", cAccId, menuId, reportID);

            //菜单对应的程序集
            //sb.AppendFormat("Insert into ua_idt_ex (id,assembly,catalogtype,type,class,entrypoint,parameter,reserved)" +
            //"values('{0}','SLReport/UFIDA.U8.Report.SLReportView.dll',1,0,'UFIDA.U8.Report.SLReportView.ReportLoginable','{0}','','UFIDA.U8.Portal.Proxy.Supports.NetModule');",
            //menuId);
            SqlParameter reportParameter = new SqlParameter("parameter", parameter);
            SqlCommand comand = new SqlCommand(sb.ToString());
            comand.Parameters.Add(reportParameter);
            SqlHelper.ExecuteNonQuery(u8LoginInfor.UfDataCnnString, comand);
            
            //插入程序集
            //sb = new StringBuilder();
            //sb.AppendFormat("Insert into ua_idt (id,assembly,catalogtype,type,class,entrypoint,parameter,reserved)" +
            //"values('{0}','SLReport/UFIDA.U8.Report.SLReportView.dll',1,0,'UFIDA.U8.Report.SLReportView.ReportLoginable','{0}','','UFIDA.U8.Portal.Proxy.Supports.NetModule');",
            //menuId);
            //SqlHelper.ExecuteScalar(u8LoginInfor.UfSystCnnString, sb.ToString());

            comand.CommandType = CommandType.StoredProcedure;
            comand.CommandText = "dbo.sp_CalculateSupAuthByAuthID";
            comand.Parameters.Clear();
            comand.Parameters.Add(SqlHelper.GetParameter("@AuthID", SqlDbType.NVarChar, 100, authId));
            comand.Parameters.Add(SqlHelper.GetParameter("@isDelete", SqlDbType.Bit,"false"));
            SqlHelper.ExecuteNonQuery(u8LoginInfor.UfSystCnnString, comand);
            //更改状态 _DbCnnString是meta库
            SqlHelper.ExecuteNonQuery(_DbCnnString, GetCmdPublish(reportID, ReportPublicPosition.CSAndBS));
            return true;
        }

        private static string GetAuthIdByReport(string reportID)
        {
            ReportAuth auth = new ReportAuth();

            string sql = string.Format("select ReportId from UAP_ReportView_AuthId  where ReportGuid='{0}'", reportID);
            return SqlHelper.ExecuteScalar(_DbCnnString, sql).ToString();
        }

        private static void GetReportSimpleInfo(string reportId, out string reportName, out string twReportName, out string enReportName, out string filterid, out string subId)
        {
            reportName = twReportName = enReportName = filterid = subId = string.Empty;
            string sql = string.Format("select FilterID, SubID,b.LocaleID, b.Name from UAP_Report a left join UAP_Report_Lang b on a.ID=b.ReportID where A.ID='{0}'", reportId);
            DataSet ds = SqlHelper.ExecuteDataSet(_DbCnnString, sql);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (string.IsNullOrEmpty(filterid))
                {
                    filterid = SqlHelper.GetStringFrom(dr["FilterID"], string.Empty);
                }
                if (string.IsNullOrEmpty(subId))
                {
                    subId = SqlHelper.GetStringFrom(dr["SubID"], string.Empty);
                }
                if (SqlHelper.GetStringFrom(dr["LocaleID"]) == "zh-CN")
                {
                    reportName = SqlHelper.GetStringFrom(dr["Name"]);
                }
                if (SqlHelper.GetStringFrom(dr["LocaleID"]) == "en-US")
                {
                    enReportName = SqlHelper.GetStringFrom(dr["Name"]);
                }
                if (SqlHelper.GetStringFrom(dr["LocaleID"]) == "zh-TW")
                {
                    twReportName = SqlHelper.GetStringFrom(dr["Name"]);
                }
            }
        }
        public static bool IsPublished(string ReportID)
        {
            CheckCnnString();
            DataSet ds = SqlHelper.ExecuteDataSet(_DbCnnString, GetCmdIsPublished(ReportID));
            return Convert.ToBoolean(Convert.ToInt32(ds.Tables[0].Rows[0]["bPublished"])
                                      & 1);//modify by yanghx
        }

        #endregion
        #endregion

        #region Private Method

        private static void CheckCnnString()
        {
            if (_DbCnnString == null || _DbCnnString.Trim() == string.Empty)
                ReportDataException.ThrowCnnStringEmptyException("DesignTimePublishEngine.CheckCnnString()");
        }

        private static SqlCommand GetCmdPublish(string ReportID, ReportPublicPosition location)
        {
            SqlCommand cmd = new SqlCommand(
                //string.Format("UPDATE UAP_Report SET bPublished=1 WHERE ID=N'{0}'", ReportID));
                string.Format("UPDATE UAP_Report SET bPublished={0} WHERE ID=N'{1}'", Convert.ToInt32(location), ReportID));
            cmd.CommandType = CommandType.Text;//modify by yanghx

            return cmd;
        }

        private static SqlCommand GetCmdIsPublished(string ReportID)
        {
            SqlCommand cmd = new SqlCommand(
                string.Format("SELECT ISNULL(bPublished,0) AS bPublished FROM UAP_Report WHERE ID=N'{0}'", ReportID));
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        private static void AddRptAndViewAuth(string _DbCnnString, string ReportID, string cAccId, string cYear, string userId)
        {
            string projectId = string.Empty;
            bool isdevelopmode = true;
            SqlDataReader drProject = SqlHelper.ExecuteReader(_DbCnnString, GetCmdProject(ReportID));
            while (drProject.Read())
            {
                projectId = drProject.GetValue(0).ToString();
                if (projectId == "U870")
                    isdevelopmode = false;
            }
            SqlCommand cmd = new SqlCommand("uap_report_AddAuthAfterReportPublish");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@reportId", SqlDbType.NVarChar, 100, ReportID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@caccid", SqlDbType.NVarChar, 10, cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@isdevelopmode", SqlDbType.Bit, isdevelopmode));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserId", SqlDbType.NVarChar, 10, userId));
            SqlHelper.ExecuteNonQuery(_DbCnnString, cmd);
        }
        private static SqlCommand GetCmdProject(string ReportID)
        {
            SqlCommand cmd = new SqlCommand(
                    string.Format("SELECT projectID FROM UAP_Report WHERE ID=N'{0}'", ReportID));
            cmd.CommandType = CommandType.Text;

            return cmd;
        }
        #endregion
    }
}
