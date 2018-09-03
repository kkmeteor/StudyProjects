/*
 * 作者:卢达其
 * 时间:2007.8.7
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// 整合系统中使用到的所有报表列表服务
    /// </summary>
    partial class ReportListService
    {
        private U8LoginInfor _login = null;

        public ReportListService(U8LoginInfor login)
        {
            this._login = login;
        }

        public U8LoginInfor Login
        {
            set { this._login = value; }
        }

        /// <summary>
        /// 获取"我的报表"中的预置报表
        /// </summary>
        public Reports GetList4MyReportSystem()
        {
            //StandardReport.DbConnString = this._login.UfMetaCnnString;
            //StandardReport.UserID = this._login.UserID;
            //StandardReport.LocaleID = this._login.LocaleID;
            //StandardReport.cAccId = this._login.cAccId;
            //StandardReport.cYear = this._login.cYear;
            //return StandardReport.GetList();
            return null;
        }

        /// <summary>
        /// 获取"我的报表"中的自定义报表
        /// </summary>
        public Reports GetList4MyReportCustom()
        {
            //CustomReport.DbConnString = this._login.UfMetaCnnString;
            //CustomReport.UserID = this._login.UserID;
            //CustomReport.LocaleID = this._login.LocaleID;
            //CustomReport.cAccId = this._login.cAccId;
            //CustomReport.cYear = this._login.cYear;
            //return CustomReport.GetList();
            return null;
        }

        /// <summary>
        /// 获取"我的报表"中的已发布报表
        /// </summary>
        public Reports GetList4MyReportStatic()
        {
            //StaticReport.DbConnString = this._login.UfMetaCnnString;
            //StaticReport.UserID = this._login.UserID;
            //StaticReport.LocaleID = this._login.LocaleID;
            //return StaticReport.GetStaticReportList();
            return null;
        }

        /// <summary>
        /// 获取UAP项目树中所有的报表
        /// </summary>
        public Hashtable GetList4UapAllReports(
            string projectId,
            string subProjectId,
            string localeId)
        {
            return this.GetList4Uap(projectId, subProjectId, localeId, false);
        }

        /// <summary>
        /// 获取UAP中所有可发布的报表
        /// </summary>
        //public Hashtable GetList4Uap2Publish(
        //    string projectId,
        //    string subProjectId,
        //    string localeId)
        //{
        //    return this.GetList4Uap(projectId, subProjectId, localeId, true);
        //}

        public List<ReportPublicInfo> GetList4Uap2Publish(
            string projectId,
            string subProjectId,
            string localeId)
        {
            //return this.GetList4Uap(projectId, subProjectId, localeId, true);
            List<ReportPublicInfo> reports = new List<ReportPublicInfo>();
            SqlCommand cmd = new SqlCommand("UAP_Report_GetAllReportPubInfo");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ProjectID", SqlDbType.NVarChar, 100, projectId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@SubID", SqlDbType.NVarChar, 50, subProjectId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, localeId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._login.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._login.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, this._login.cYear));
            DataSet ds = SqlHelper.ExecuteDataSet(this._login.UfMetaCnnString, cmd);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = SqlHelper.GetDataRowFrom(i, ds);
                if (dr != null)
                {
                    string id = SqlHelper.GetStringFrom(dr["ReportID"]);
                    string className = SqlHelper.GetStringFrom(dr["ClassName"]);
                    if (this.IsUapReport(id))
                    {
                        bool isCom = false;
                        ReportPublicInfo report = new ReportPublicInfo();
                        report.Id = id;
                        report.Name = SqlHelper.GetStringFrom(dr["Name"]);
                        report.HavPublicPosition = (ReportPublicPosition)SqlHelper.GetIntFrom(dr["bPublished"]);
                        if (string.IsNullOrEmpty(className) || className.Contains(","))//非com类型的数据源
                            report.CanPublicsPosition = (ReportPublicPosition)SqlHelper.GetIntFrom(dr["viewtype"]);
                        else
                        {
                            report.CanPublicsPosition = ReportPublicPosition.CS;
                            isCom = true;
                        }
                        if (report.HavPublicPosition == ReportPublicPosition.CS && isCom == true)
                        {
                            //不添加
                        }
                        else
                        {
                            reports.Add(report);
                        }
                    }
                }
            }
            return reports;
        }

        private Hashtable GetList4Uap(
            string projectId,
            string subProjectId,
            string localeId,
            bool isForPublished)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_OthersReportListByProject");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ProjectID", SqlDbType.NVarChar, 100, projectId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@SubID", SqlDbType.NVarChar, 50, subProjectId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, localeId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._login.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._login.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, this._login.cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@IsForPublish", SqlDbType.Bit, isForPublished));
            DataSet ds = SqlHelper.ExecuteDataSet(this._login.UfMetaCnnString, cmd);
            Hashtable htReports = new Hashtable();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = SqlHelper.GetDataRowFrom(i, ds);
                if (dr != null)
                {
                    string id = SqlHelper.GetStringFrom(dr["ReportID"]);
                    if (this.IsUapReport(id))
                        htReports.Add(id, SqlHelper.GetStringFrom(dr["Name"]));
                }
            }
            return htReports;
        }
    }

    public enum ReportPublicPosition
    {
        none = 0,
        CS = 1,
        BS = 2,
        CSAndBS = 3
    }

    public class ReportPublicInfo
    {
        string id;
        string name;
        ReportPublicPosition havPublicPosition;
        ReportPublicPosition canPublicsPosition;

        /// <summary>
        /// 报表ID
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// 报表名字
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// 已经发布位置
        /// </summary>
        public ReportPublicPosition HavPublicPosition
        {
            get { return havPublicPosition; }
            set { havPublicPosition = value; }
        }

        /// <summary>
        /// 可拥有的发布位置权限
        /// </summary>
        public ReportPublicPosition CanPublicsPosition
        {
            get { return canPublicsPosition; }
            set { canPublicsPosition = value; }
        }
    }
}
