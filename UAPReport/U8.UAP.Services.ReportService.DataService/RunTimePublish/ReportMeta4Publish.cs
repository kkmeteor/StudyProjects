/*
 * 作者:卢达其
 * 时间:2009.2.11
 * 
 * 890重构报表发布功能
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// 发布需要报表的一些信息
    /// </summary>
    public class ReportMeta4Publish
    {
        private string _viewId = null;
        private string _reportId = null;
        private U8LoginInfor _loginInfo = null;
        private List<ViewInfo4Publish> _viewInfos = new List<ViewInfo4Publish>();
        private ViewInfo4Publish _currentViewInfo = null;
        private PublishData _target = null;
        private ReportDataPublishSettingType _publishSettingType = ReportDataPublishSettingType.New;

        public string ReportId
        {
            get { return this._reportId; }
            set { this._reportId = value; }
        }

        public string ViewId
        {
            get { return this._viewId; }
            set { this._viewId = value; }
        }

        public ReportDataPublishSettingType PublishSettingType
        {
            get { return this._publishSettingType; }
            set { this._publishSettingType = value; }
        }

        public PublishData Target
        {
            get { return this._target; }
            set { this._target = value; }
        }

        /// <summary>
        /// 发布窗体中显示的当前的view
        /// </summary>
        public ViewInfo4Publish CurrentViewInfo
        {
            get { return this._currentViewInfo; }
            set { this._currentViewInfo = value; }
        }

        public U8LoginInfor LogInfo
        {
            get { return this._loginInfo; }
            set { this._loginInfo = value; }
        }

        public List<ViewInfo4Publish> ViewInfos
        {
            get { return this._viewInfos; }
            set { this._viewInfos = value; }
        }

        public void Retrieve(bool is4portal, List<U8User> users, string currentViewId)
        {
            DataSet ds = null;
            int defaultPublishSettingBeginIndex = 4;
            if (is4portal)
            {
                defaultPublishSettingBeginIndex = 1;
                ds = this.RetrieveData4Portal();
            }
            else
                ds = this.RetrieveData(currentViewId);

            // 获取默认设置
            DataTable[] dts = new DataTable[]{
				ds.Tables[defaultPublishSettingBeginIndex],
				ds.Tables[defaultPublishSettingBeginIndex +1],
				ds.Tables[defaultPublishSettingBeginIndex +2],
				ds.Tables[defaultPublishSettingBeginIndex +3],
			};
            this.Target = new PublishData(null);
            this.Target.LogInfo = this.LogInfo;
            this.Target.RetrieveFrom(dts);
            //this.Target.EmailTitle = null;
            //this.Target.EmailContext = null;
            this.Target.Name = null;//置为空，让ReportPublishFacade.InitTargetNew生成默认名称

            PublishDataService.FillPublishUserInfo(users, ds.Tables[defaultPublishSettingBeginIndex + 4]);
        }

        private DataSet RetrieveData(string currentViewId)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ReportMeta4PublishRetrieve");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ID", SqlDbType.NVarChar, 100, this.ReportId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, this.LogInfo.LocaleID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this.LogInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 3, this.LogInfo.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 4, this.LogInfo.cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@currentViewId", SqlDbType.NVarChar, 100, currentViewId));
            DataSet ds = SqlHelper.ExecuteDataSet(this.LogInfo.UfMetaCnnString, cmd);
            this.FillViewInfos(ds.Tables[0], false);
            this.FillPublishInfo(ds.Tables[1]);
            this.FillDefaultView(ds.Tables[2]);
            this.FillReportInfo(ds.Tables[3]);
            return ds;
        }

        private DataSet RetrieveData4Portal()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ReportMeta4PublishRetrieve4Portal");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, this.ViewId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, this.LogInfo.LocaleID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this.LogInfo.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 3, this.LogInfo.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 4, this.LogInfo.cYear));
            DataSet ds = SqlHelper.ExecuteDataSet(this.LogInfo.UfMetaCnnString, cmd);
            if (ds.Tables[0].Rows.Count <= 0)
                throw new Exception(String4Report.GetString("UAP报表系统中不存在任何监控报表或操作员没有监控报表的发布权限"));
            this.FillViewInfos(ds.Tables[0], true);
            this.CurrentViewInfo = this.ViewInfos[0];
            return ds;
        }

        public void FillViewInfos(DataTable dt, bool is4portal)
        {
            foreach (DataRow dr in dt.Rows)
            {
                ViewInfo4Publish vi = new ViewInfo4Publish();
                vi.FillData(dr);
                if (is4portal)// 为门户获取视图列表时需要作一些特殊处理
                {
                    this.FillReportData2View(vi, dr);
                    vi.Name = string.Format("{0}({1})", vi.Name, vi.ReportName);
                }
                this._viewInfos.Add(vi);
            }
        }

        public void FillPublishInfo(DataTable dt)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string reportViewID = SqlHelper.GetStringFrom(dr["ReportViewID"]);
                ViewInfo4Publish vi = this.GetViewInfo(reportViewID);
                if (vi != null)
                {
                    PublishData pd = new PublishData(null);
                    pd.ID = SqlHelper.GetStringFrom(dr["ID"]);
                    pd.Name = SqlHelper.GetStringFrom(dr["Name"]);
                    pd.PublisherID = SqlHelper.GetStringFrom(dr["PublisherId"]);
                    pd.ReportViewID = reportViewID;
                    pd.LocaleID = this.LogInfo.LocaleID;
                    vi.Publishes.Add(pd);
                    if (vi.CurrentPublishData == null)
                        vi.CurrentPublishData = pd;
                }
            }
        }

        public void FillDefaultView(DataTable dt)
        {
            try
            {
                ViewInfo4Publish defaultView = this._viewInfos[0];
                if (dt.Rows.Count > 0)
                {
                    string viewId = SqlHelper.GetStringFrom(dt.Rows[0]["ViewID"]);
                    defaultView = this.GetViewInfo(viewId);
                }
                if (defaultView != null)
                {
                    defaultView.IsDefault = true;
                    this.CurrentViewInfo = defaultView;
                }
                else
                    this.CurrentViewInfo = this._viewInfos[0];
            }
            catch { throw new Exception(String4Report.GetString("当前操作的目标对象已被删除或操作员没有权限!")); }
        }

        public ViewInfo4Publish GetViewInfo(string viewId)
        {
            foreach (ViewInfo4Publish vi in this._viewInfos)
                if (vi.ID == viewId)
                    return vi;
            return null;
        }

        public void FillReportInfo(DataTable dt)
        {
            foreach (ViewInfo4Publish vi in this._viewInfos)
                this.FillReportData2View(vi, dt.Rows[0]);
        }

        private void FillReportData2View(ViewInfo4Publish vi, DataRow dr)
        {
            vi.ReportId = SqlHelper.GetStringFrom(dr["ReportId"]);
            vi.RootReportId = SqlHelper.GetStringFrom(dr["RootReportId"]);
            vi.ReportName = SqlHelper.GetStringFrom(dr["ReportName"]);
            vi.FilterId = SqlHelper.GetStringFrom(dr["FilterID"]);
            vi.SubId = SqlHelper.GetStringFrom(dr["SubID"]);
            vi.FilterClass = SqlHelper.GetStringFrom(dr["FilterClass"]);
            vi.ClassName = SqlHelper.GetStringFrom(dr["ClassName"]);
        }
    }
}
