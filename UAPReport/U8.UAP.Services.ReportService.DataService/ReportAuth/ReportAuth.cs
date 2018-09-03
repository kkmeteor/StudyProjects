using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Xml;
using U8ColAuthsvr;
using U8Login;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
    /// <summary>
    /// ReportAuth 的摘要说明。
    /// </summary>
    public class ReportAuth
    {

        public ReportAuth()
        {
        }

        //专为字段权限使用
        public ReportAuth(U8LoginInfor login)
        {
            init(login);
        }

        #region 功能权限

        /// <summary>
        /// 检查功能权限.
        /// ldq:2007.10.10重构
        /// </summary>
        /// <param name="u8login">vb的login对象</param>
        /// <param name="reportIdOrViewId">
        /// 报表id或视图id:operation=OperationEnum.Publish时,此参数为
        /// 报表id,其他情况此参数为视图id
        /// </param>
        /// <param name="operation">权限类型</param>
        /// <returns></returns>
        public bool AuthCheck(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation)
        {
            return this.AuthCheck(u8login, reportIdOrViewId, operation, null);
        }

        /// <summary>
        /// 检查功能权限.
        /// ldq:2007.10.10重构
        /// </summary>
        /// <param name="u8login">vb的login对象</param>
        /// <param name="reportIdOrViewId">
        /// 报表id或视图id:operation=OperationEnum.Publish时,此参数为
        /// 报表id,其他情况此参数为视图id
        /// </param>
        /// <param name="operation">
        /// 权限类型,改为string类型是因为有些报表需要特殊的权限,
        /// 如数据变更日志需要清除数据的权限
        /// </param>
        /// <returns></returns>
        public bool AuthCheck(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
		{
            return AuthCheckIfRecordLog(u8login, reportIdOrViewId, operation, otherAuthString, true);
		}

        public bool AuthCheckNoLog(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
        {
            return AuthCheckIfRecordLog(u8login, reportIdOrViewId, operation, otherAuthString, false);
        }
  
        //2012-12-25 新增是否记录日志功能
        private bool AuthCheckIfRecordLog(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString,bool bLog)
        {
            Trace.WriteLine("进入AuthCheck");
            init(u8login);

            // 子报表都不控制权限
            if (!this.IsReportApplyAuthControl(reportIdOrViewId))
                return true;

            //未发布的报表不控制权限
            if (!this.IsReportPublished(reportIdOrViewId))
                return true;

            string reportAuthId = this.GetReportAuthId(reportIdOrViewId);
            string viewAuthId = this.GetViewAuthId(reportIdOrViewId);
            if (string.IsNullOrEmpty(reportAuthId))
                throw new Exception("传入的Id没有对应的ReportAuthId前缀,Id:" + reportIdOrViewId);
            reportAuthId += this.GetOperateCodeBy(operation, otherAuthString);


            bool isAuthOk = false;
            if (bLog)
            {
                isAuthOk = _u8login.TaskExec(reportAuthId, -1, short.Parse(_u8login.cIYear));
            }
            else
            {
                isAuthOk = _u8login.TaskExec(reportAuthId, 1, short.Parse(_u8login.cIYear));
            }       
     

            // 检查运行发布权限不需要有某个视图的权限
            // 其他权限需要同时具有报表的相应权限和某个视图的权限
            if (isAuthOk && operation != OperationEnum.Publish)
                isAuthOk = this.CheckAuth(u8login, viewAuthId);
            if (!isAuthOk)
                //    throw new Exception(U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportElements.RemoteDataEngine.没有权限"));// _u8login.ShareString );
                throw new Exception(_u8login.ShareString.Length == 0 ? U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportElements.RemoteDataEngine.没有权限") : _u8login.ShareString);
            return true;
        }


        /// <summary>
        /// 检查报表特定的功能权限
        /// </summary>
        /// <param name="u8login">vb的login对象</param>
        /// <param name="report">报表ID</param>
        /// <param name="operation">操作类型</param>
        /// <returns></returns>
        public bool AuthCheckSpecifiedOperation(
            object u8login,
            string reportID,
            OperationEnum operation)
        {
            Trace.WriteLine("进入AuthCheckSpecifiedOperation");
            init(u8login);

            string reportAuthId = this.GetReportAuthIdByReportID(reportID);
            if (string.IsNullOrEmpty(reportAuthId))
                throw new Exception("传入的reportID没有对应的ReportAuthId,reportID:" + reportID);
            reportAuthId += this.GetOperateCodeBy(operation, null);
            bool isAuthOk = _u8login.TaskExec(reportAuthId, -1, short.Parse(_u8login.cIYear));
            if (!isAuthOk)
            {
                Logger log = Logger.GetLogger("KKMETEOR");
                log.Info(_u8login.ShareString);
                throw new Exception(_u8login.ShareString);
            }
            return true;
        }

        private string GetOperateCodeBy(
            OperationEnum opType,
            string otherAuthString)
        {
            switch (opType)
            {
                case OperationEnum.Other:
                    return otherAuthString;
                case OperationEnum.Publish:
                    return "_05";
                case OperationEnum.Print:
                    return "_03";
                case OperationEnum.Setting:
                    return "_02";
                case OperationEnum.Output:
                    return "_04";
                default:
                case OperationEnum.Query:
                    return "_01";
            }
        }

        /// <summary>
        /// 释放功能权限.
        /// ldq:2007.10.10重构
        /// </summary>
        /// <param name="u8login">vb的login对象</param>
        /// <param name="reportIdOrViewId">
        /// 报表id或视图id:operation=OperationEnum.Publish时,此参数为
        /// 报表id,其他情况此参数为视图id
        /// </param>
        /// <param name="operation">权限类型</param>
        /// <returns></returns>
        public bool ReleaseAuth(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation)
        {
            return this.ReleaseAuth(u8login, reportIdOrViewId, operation, null);
        }

        public bool ReleaseAuth(
            object u8login,
            string reportIdOrViewId,
            OperationEnum operation,
            string otherAuthString)
        {
            Trace.WriteLine("开始释放功能权限,Id:" + reportIdOrViewId);
            init(u8login);
            string reportAuthId = this.GetReportAuthId(reportIdOrViewId);
            if (!string.IsNullOrEmpty(reportAuthId))
            {
                reportAuthId += this.GetOperateCodeBy(operation, otherAuthString);
                //Logger logger = Logger.GetLogger("zxy");
                //logger.Info("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                //logger.Info("reportAuthId = " + reportAuthId);
                //logger.Info("_u8login.cIYear = " + _u8login.cIYear);
                bool result = _u8login.TaskExec(reportAuthId, 0, short.Parse(_u8login.cIYear));
                //logger.Info("_u8login.TaskID = " + _u8login.get_TaskId());
                //logger.Info("result = " + result.ToString());
                //logger.Close();
                return result;
            }
            return false;
        }

        public AuthCollection AuthCollectionCheck(object u8login, string viewguid)
        {
            init(u8login);
            AuthCollection ac = new AuthCollection();

            string procedurename = "uap_reportview_getauthcollection";
            SqlCommand cmd = new SqlCommand(procedurename);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, viewguid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._u8login.get_cAcc_Id()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, _u8login.cBeginYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cUser", SqlDbType.NVarChar, 20, this._u8login.cUserId));

            DataSet ds = SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                string cauth_id = ds.Tables[0].Rows[i]["cAuth_Id"].ToString();
                if (String.Compare(cauth_id.Substring(cauth_id.Length - 3, 3), "_01") == 0)
                {
                    ac.Add(OperationEnum.Query);
                }
                else if (String.Compare(cauth_id.Substring(cauth_id.Length - 3, 3), "_02") == 0)
                {
                    ac.Add(OperationEnum.Setting);
                }
                else if (String.Compare(cauth_id.Substring(cauth_id.Length - 3, 3), "_03") == 0)
                {
                    ac.Add(OperationEnum.Print);
                }
                else if (String.Compare(cauth_id.Substring(cauth_id.Length - 3, 3), "_04") == 0)
                {
                    ac.Add(OperationEnum.Output);
                }
                else if (String.Compare(cauth_id.Substring(cauth_id.Length - 3, 3), "_05") == 0)
                {
                    ac.Add(OperationEnum.Publish);
                }
            }

            return ac;

        }


        public bool ReportDelAuthCheck(object u8login, string reportguid)
        {
            init(u8login);

            string sql = string.Format("select ReportId+ViewIndex as ViewId,ReportId from UAP_ReportView_AuthId where ReportGuid =  '{0}'", reportguid);

            SqlCommand cmd = new SqlCommand(sql);
            cmd.CommandType = CommandType.Text;

            DataSet ds = SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);
            if (ds.Tables[0].Rows.Count > 0)
            {
                int cnt = 0;

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (CheckAuth(u8login, ds.Tables[0].Rows[i]["ViewId"].ToString())) cnt++;
                }
                if (cnt == ds.Tables[0].Rows.Count && CheckAuth(u8login, ds.Tables[0].Rows[0]["ReportId"].ToString() + "_02"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


        public ReportCollection GetReportsByUser(object u8login, OperationEnum op)
        {
            init(u8login);
            string strop = string.Empty;
            if (op == OperationEnum.Query)
            {
                strop = "_01";
            }
            else if (op == OperationEnum.Setting)
            {
                strop = "_02";
            }
            else if (op == OperationEnum.Print)
            {
                strop = "_03";
            }
            else if (op == OperationEnum.Output)
            {
                strop = "_04";
            }
            else if (op == OperationEnum.Publish)
            {
                strop = "_05";
            }

            ReportCollection rc = new ReportCollection();

            string procedurename = "uap_reportview_getreportsbyuser";

            SqlCommand cmd = new SqlCommand(procedurename);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._u8login.get_cAcc_Id()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, _u8login.cBeginYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cUser", SqlDbType.NVarChar, 20, this._u8login.cUserId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@op", SqlDbType.Char, 3, strop));

            DataSet ds = SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                rc.Add(ds.Tables[0].Rows[i]["ReportGuid"].ToString());
            }

            return rc;
        }

        public ViewCollection GetViewsByReport(object u8login, string reportguid)
        {
            init(u8login);
            ViewCollection vc = new ViewCollection();

            string procedurename = "uap_reportview_getviewid";

            SqlCommand cmd = new SqlCommand(procedurename);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, reportguid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._u8login.get_cAcc_Id()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, _u8login.cBeginYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cUser", SqlDbType.NVarChar, 20, this._u8login.cUserId));

            DataSet ds = SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                ViewAuth va = new ViewAuth();
                va.ID = ds.Tables[0].Rows[i]["ViewGuid"].ToString();
                vc.Add(va);
            }

            return vc;

        }

        public ViewCollection GetViewsByReport(object u8login, string reportguid, string localeId, OperationEnum op)
        {
            init(u8login);
            string strop = string.Empty;
            if (op == OperationEnum.Query)
            {
                strop = "_01";
            }
            else if (op == OperationEnum.Setting)
            {
                strop = "_02";
            }
            else if (op == OperationEnum.Print)
            {
                strop = "_03";
            }
            else if (op == OperationEnum.Output)
            {
                strop = "_04";
            }
            else if (op == OperationEnum.Publish)
            {
                strop = "_05";
            }

            ViewCollection vc = new ViewCollection();

            string procedurename = "uap_reportview_getviewsbyreport";

            SqlCommand cmd = new SqlCommand(procedurename);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, reportguid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._u8login.get_cAcc_Id()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, _u8login.cBeginYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cUser", SqlDbType.NVarChar, 20, this._u8login.cUserId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@op", SqlDbType.Char, 3, strop));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, localeId));

            DataSet ds = SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                ViewAuth va = new ViewAuth();
                va.ID = ds.Tables[0].Rows[i]["ViewGuid"].ToString();
                va.Name = ds.Tables[0].Rows[i]["Name"].ToString();
                va.ViewType = Convert.ToInt32(ds.Tables[0].Rows[i]["ViewType"].ToString());
                //va.AuthCollection = AuthCollectionCheck( va.ID );
                vc.Add(va);
            }

            return vc;
        }

        public ViewCollection GetViewsByView(object u8login, string viewguid, string localeId, OperationEnum op)
        {
            init(u8login);

            string strSQL = string.Format("SELECT ReportID FROM UAP_ReportView  WHERE ID  ='{0}'", viewguid);
            SqlCommand cmd = new SqlCommand(strSQL);
            cmd.CommandType = CommandType.Text;

            DataSet ds = SqlHelper.ExecuteDataSet(_ufMetaConnString, cmd);
            string reportguid = ds.Tables[0].Rows[0]["ReportID"].ToString();

            return GetViewsByReport(u8login, reportguid, localeId, op);
        }
        #endregion

        #region 字段权限
        #region save col auth
        private void init(U8LoginInfor u8info)
        {
            if (_ufDataConnString == string.Empty)
            {
                _ufDataConnString = u8info.UfDataCnnString;
                _ufMetaConnString = u8info.UfMetaCnnString;
                _ufSystConnString = u8info.UfSystCnnString;
                datadb = u8info.AccDbName;
                metadatadb = "UFMeta_" + u8info.cAccId;

                _cnn = new SqlConnection(_ufMetaConnString);
                _cnn.Open();
                _tran = _cnn.BeginTransaction();
            }
        }

        public void ColumnsAuth(
            string viewguid,
            string projectid,
            ArrayList data)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                this.AddDeleteColAuth(sb, _tran, viewguid);
                this.AddInsertViewCol2Database(sb, _tran, viewguid, projectid, data);
                SqlHelper.ExecuteNonQuery(_tran, sb.ToString());
                _tran.Commit();
            }
            catch (Exception exc)
            {
                _tran.Rollback();
                throw exc;
            }
            finally
            {
                CloseConn();
            }
        }

        private void AddDeleteColAuth(
            StringBuilder sb,
            SqlTransaction tran,
            string viewguid)
        {
            sb.AppendFormat(
                @"delete from {0}..AA_ColumnDic_Base where cKey=N'{1}'",
                metadatadb,
                viewguid);
            sb.AppendLine();
            sb.AppendFormat(
                @"delete from {0}..aa_busobject_base where cbusobid=N'{1}'",
                datadb,
                viewguid);
            sb.AppendLine();
        }

        private void AddInsertViewCol2Database(
            StringBuilder sb,
            SqlTransaction tran,
            string viewguid,
            string projectid,
            ArrayList data)
        {
            this.AddInsertView2Busobject(sb, tran, viewguid);
            foreach (Hashtable args in data)
                this.AddInsertACol2ColumnDic(sb, tran, viewguid, projectid, args);
        }

        private void AddInsertView2Busobject(
            StringBuilder sb,
            SqlTransaction tran,
            string viewguid)
        {
            string strSQL = string.Format(
                @"select A.Name AS ReportName, B.SubID, C.Name AS ViewName, A.LocaleId
				from UAP_Report_Lang A INNER JOIN UAP_Report B
				ON A.ReportId=B.Id
				INNER JOIN UAP_ReportView D ON B.Id=D.ReportId
				INNER JOIN UAP_ReportView_Lang C ON D.Id=C.ViewId and A.LocaleID=C.LocaleID
				where D.ID='{0}'",
                viewguid);
            DataSet ds = SqlHelper.ExecuteDataSet(tran, strSQL);
            DataTable dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                this._subid = SqlHelper.GetStringFrom(dr["SubID"]);
                sb.AppendFormat(
                    @"if not exists(select cbusobid from {0}..aa_busobject_base where cbusobid=N'{1}' and langid=N'{5}')
					begin
						insert into {0}..aa_busobject_base
						(cbusobid,cbusobname,iauthtype,bauthcontrol,csub_id,ifunctype,langid)
						values(N'{1}', N'{2}({3})', N'1', 0, N'{4}', 2, N'{5}')
					end",
                    datadb,
                    viewguid,
                    SqlHelper.GetStringFrom(dr["ReportName"]).Replace("'", "''"),
                    SqlHelper.GetStringFrom(dr["ViewName"]).Replace("'", "''"),
                    this._subid,
                    SqlHelper.GetStringFrom(dr["LocaleId"]));
                sb.AppendLine();
            }
        }

        private void AddInsertACol2ColumnDic(
            StringBuilder sb,
            SqlTransaction tran,
            string viewguid,
            string projectid,
            Hashtable args)
        {
            sb.AppendFormat(
                @"if not exists(
					select cKey from {0}..AA_ColumnDic_Base 
					where cKey=N'{1}' and cFld=N'{2}' and LocaleID=N'{4}'
				)
				begin
					insert into {0}..AA_ColumnDic_Base 
					(cKey,cFld,iColPos,cCaption,LocaleID,cProjectNO,cSubID)
					values
					(N'{1}', N'{2}', 1, N'{3}', N'{4}', N'{5}', N'{6}')
				end
				else
				begin
					update {0}..AA_ColumnDic_Base 
					set cCaption=N'{3}', 
						cProjectNO=N'{5}',
						cSubID=N'{6}'
					where cKey=N'{1}' and cFld=N'{2}' and LocaleID=N'{4}'
				end",
                metadatadb,
                viewguid,
                args[ReportAuth.ArgKeyAuthKey].ToString().Replace("'", "''"),
                args[ReportAuth.ArgKeyAuthCaption].ToString().Replace("'", "''"),
                args[ReportAuth.ArgKeyLocaleId].ToString(),
                projectid,
                _subid);
            sb.AppendLine();
        }

        public Hashtable GetViewLoacleFormat(string viewguid)
        {
            Hashtable formats = new Hashtable();
            string strSQL = string.Format(
                @"select Format,LocaleId from uap_reportview_lang
                where viewid='{0}'",
                viewguid);
            DataSet ds = SqlHelper.ExecuteDataSet(_tran, strSQL);
            DataTable dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                formats.Add(
                       SqlHelper.GetStringFrom(dr["LocaleId"]).ToUpper(),
                       SqlHelper.GetStringFrom(dr["Format"]));
            }
            return formats;
        }

        public void DeleteViewColFromDatabase(string datacnn, SqlTransaction tran, string viewguid)
        {
            string strSQL = string.Format("delete from {0}..AA_ColumnAuth where cKey = '{1}'      " +
                                          "delete from {0}..AA_ColumnAuthMapping where cBusObId = '{1}'  " +
                                          "delete from {2}..AA_ColumnDic_Base where cKey ='{1}'   " +
                                          "delete from {0}..aa_busobject_base where cbusobid ='{1}' ", datacnn, viewguid, metadatadb);
            SqlHelper.ExecuteNonQuery(tran, strSQL);
        }

        public void UpdateViewNameToBusobject(string cnnstring, string datacnn, SqlTransaction tran, string viewguid)
        {
            string viewname_CN = string.Empty;
            string viewname_TW = string.Empty;
            string viewname_US = string.Empty;
            string subid = string.Empty;
            string datadbstring = datacnn;

            string strSQL = string.Format("select A.Name,B.SubID,C.Name as VName from UAP_Report_Lang A,UAP_Report B,UAP_ReportView_Lang C,UAP_ReportView D  " +
                "where D.ID='{0}' and D.ReportID = A.ReportID and D.ReportID = B.ID and D.ID = C.ViewID and A.LocaleID = C.LocaleID order by A.LocaleID ", viewguid);
            DataSet ds = SqlHelper.ExecuteDataSet(tran, strSQL);
            if (ds.Tables[0].Rows.Count != 3) throw new Exception();
            viewname_US = ds.Tables[0].Rows[0]["Name"].ToString() + "(" + ds.Tables[0].Rows[0]["VName"].ToString() + ")";
            viewname_CN = ds.Tables[0].Rows[1]["Name"].ToString() + "(" + ds.Tables[0].Rows[1]["VName"].ToString() + ")";
            viewname_TW = ds.Tables[0].Rows[2]["Name"].ToString() + "(" + ds.Tables[0].Rows[2]["VName"].ToString() + ")";

            viewname_US = viewname_US.Replace("'", "''");
            viewname_CN = viewname_CN.Replace("'", "''");
            viewname_TW = viewname_TW.Replace("'", "''");
            strSQL = string.Format("update {0}..aa_busobject_base set cbusobname = '{2}' where cbusobid = '{1}' and langid = 'zh-CN'  " +
                                    "update {0}..aa_busobject_base set cbusobname = '{3}' where cbusobid = '{1}' and langid = 'zh-TW'  " +
                                    "update {0}..aa_busobject_base set cbusobname = '{4}' where cbusobid = '{1}' and langid = 'en-US'  ", datadbstring, viewguid, viewname_CN, viewname_TW, viewname_US);

            SqlHelper.ExecuteNonQuery(tran, strSQL);

        }

        private void DeleteColsFromDatabase(SqlTransaction tran, string viewguid, string delcolumns)
        {
            string[] temp = delcolumns.Split(';');
            if (temp[0] != "")
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    DeleteAColFromColumnDic(tran, viewguid, temp[i]);
                }
            }
        }

        private void DeleteViewColFromDatabase(SqlTransaction tran, string viewguid)
        {
            string strSQL = string.Format("delete from {0}..AA_ColumnAuth where cKey = '{1}'      " +
                                           "delete from {0}..AA_ColumnAuthMapping where cBusObId = '{1}'  " +
                                           "delete from {2}..AA_ColumnDic_Base where cKey ='{1}'   " +
                                           "delete from {0}..aa_busobject_base where cbusobid ='{1}' ", datadb, viewguid, metadatadb);
            SqlHelper.ExecuteNonQuery(tran, strSQL);
        }

        private void DeleteAColFromColumnDic(SqlTransaction tran, string viewguid, string col)
        {
            string strSQL = string.Format("delete from {0}..AA_ColumnAuth where cKey = '{1}' and cFld = '{2}' " +
                                           "delete from {0}..AA_ColumnAuthMapping where cBusObId = '{1}' and cFld = '{2}' " +
                                           "delete from {3}..AA_ColumnDic_Base where cKey ='{1}' and cfld='{2}'" +
                                           "if not exists(select * from {3}..AA_ColumnDic_Base where cKey ='{1}')  " +
                                           "delete from {0}..aa_busobject_base where cbusobid ='{1}'  ", datadb, viewguid, col, metadatadb);
            SqlHelper.ExecuteNonQuery(tran, strSQL);
        }

        #endregion

        #region read col auth string
        public string GetViewColAuthString(object u8login, string guid)
        {
            init(u8login);
            System.Diagnostics.Trace.WriteLine("Begin create object: U8ColAuthsvr.clsColAuthClass");
            U8ColAuthsvr.clsColAuthClass cac = new clsColAuthClass();
            cac.Init(_u8login.UfDbName, _u8login.cUserId);
            string result = cac.GetAuthString("<u8ColAuth cBusObId='" + guid + "' cFuncId='N'/>");
            return result;
        }
        #endregion

        #endregion

        #region insert delete update

        public void AddAuthToDatabase(string viewguid, SqlTransaction tran)
        {
            try
            {
                string procedurename = "uap_reportview_addauth";
                SqlCommand cmd = new SqlCommand(procedurename);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, viewguid));

                SqlHelper.ExecuteNonQuery(tran, cmd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void DeleteViewAuthFromDatabase(string viewguid, SqlTransaction tran)
        {
            try
            {
                U8LoginInfor login = new U8LoginInfor(_u8login);
                string procedurename = "uap_reportview_deleteviewauth";
                SqlCommand cmd = new SqlCommand(procedurename);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, viewguid));
                cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, login.cAccId));
                cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, login.cYear));
                SqlHelper.ExecuteNonQuery(tran, cmd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void UpdateViewAuthToDatabase(string viewguid, string localeid, SqlTransaction tran)
        {
            try
            {
                string procedurename = "uap_reportview_updateviewauth";
                SqlCommand cmd = new SqlCommand(procedurename);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, viewguid));
                cmd.Parameters.Add(SqlHelper.GetParameter("@localeid", SqlDbType.NVarChar, 10, localeid));

                SqlHelper.ExecuteNonQuery(tran, cmd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpdateReportNameToDatabase(string reportguid, string localeid, SqlTransaction tran)
        {
            try
            {
                string procedurename = "uap_reportview_updatereportname";

                SqlCommand cmd = new SqlCommand(procedurename);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(SqlHelper.GetParameter("@guid", SqlDbType.NVarChar, 100, reportguid));
                cmd.Parameters.Add(SqlHelper.GetParameter("@localeid", SqlDbType.NVarChar, 10, localeid));
                SqlHelper.ExecuteNonQuery(tran, cmd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpgradeReport(string guid, SqlTransaction tran)
        {
            try
            {
                string procedurename = "uap_reportview_upgradereport";

                SqlCommand cmd = new SqlCommand(procedurename);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(SqlHelper.GetParameter("@viewguid", SqlDbType.NVarChar, 100, guid));
                SqlHelper.ExecuteNonQuery(tran, cmd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region private
        private bool CheckAuth(object u8login, string authid)
        {
            if (this._u8login.cUserId.ToUpper().Equals("ASUSER"))
                return true;
            string procedurename = "uap_reportview_checkauth";

            SqlCommand cmd = new SqlCommand(procedurename);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(SqlHelper.GetParameter("@authid", SqlDbType.NVarChar, 100, authid));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.Char, 3, this._u8login.get_cAcc_Id()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.Char, 4, _u8login.cBeginYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cUser", SqlDbType.NVarChar, 20, this._u8login.cUserId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@result", SqlDbType.Bit));

            SqlHelper.ExecuteDataSet(this._ufMetaConnString, cmd);

            if (Convert.ToBoolean(cmd.Parameters["@result"].Value)) return true;
            else return false;
        }

        /// <summary>
        /// ldq:2007.10.10重构
        /// </summary>
        public  string GetReportAuthId(string reportIdOrViewId)
        {
            DataRow dr = this.GetDataRowBy(reportIdOrViewId);
            if (dr != null)
                return dr["ReportId"].ToString().Trim();
            return null;
        }

        private   string GetReportAuthIdByReportID(string reportId)
        {
            string sql = string.Format(
                @"SELECT TOP 1 * 
				FROM UAP_ReportView_AuthId  
				WHERE ReportGuid=N'{0}'",
                reportId);
            DataSet ds = SqlHelper.ExecuteDataSet(_ufMetaConnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);

            //可能是861报表
            if (dr == null)
            {
                sql = string.Format(
                    @"SELECT TOP 1 WhereEx AS ReportId 
					FROM Rpt_GlbDef_Base  
					WHERE (SystemID+N'[__]'+Name)=N'{0}'",
                    reportId);
                ds = SqlHelper.ExecuteDataSet(this._ufDataConnString, sql);
                dr = SqlHelper.GetDataRowFrom(0, ds);
            }

            if (dr != null)
                return SqlHelper.GetStringFrom(dr["ReportId"]);
            return null;
        }

        /// <summary>
        /// ldq:2007.10.10重构
        /// </summary>
        private  string GetViewAuthId(string reportIdOrViewId)
        {
            DataRow dr = this.GetDataRowBy(reportIdOrViewId);
            if (dr != null)
                return dr["ReportId"].ToString().Trim() + dr["ViewIndex"].ToString().Trim();
            return null;
        }

        private DataRow GetDataRowBy(string reportIdOrViewId)
        {
            string sql = string.Format(
                @"SELECT TOP 1 * 
				FROM UAP_ReportView_AuthId  
				WHERE {0}=N'{1}'",
                "{0}",
                reportIdOrViewId);

            //            string checkedField = "ViewGuid";
            //            if( operation == OperationEnum.Publish )
            //                checkedField = "ReportGuid";
            //            string sql = string.Format( 
            //                @"SELECT TOP 1 * 
            //				FROM UAP_ReportView_AuthId  
            //				WHERE {0}=N'{1}'",
            //                checkedField,
            //                reportIdOrViewId );

            DataSet ds = SqlHelper.ExecuteDataSet(_ufMetaConnString, string.Format(sql, "ViewGuid"));
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr == null)
            {
                ds = SqlHelper.ExecuteDataSet(_ufMetaConnString, string.Format(sql, "ReportGuid"));
                dr = SqlHelper.GetDataRowFrom(0, ds);
            }
            //可能是861报表
            if (dr == null)
            {
                sql = string.Format(
                    @"SELECT TOP 1 WhereEx AS ReportId 
					FROM Rpt_GlbDef_Base  
					WHERE (SystemID+N'[__]'+Name)=N'{0}'",
                    reportIdOrViewId);
                ds = SqlHelper.ExecuteDataSet(this._ufDataConnString, sql);
                dr = SqlHelper.GetDataRowFrom(0, ds);
            }
            return dr;
        }


        private void init(object u8login)
        {
            if (_ufDataConnString == string.Empty)
            {
                _u8login = (clsLogin)u8login;
                _objectu8login = u8login;
                U8LoginInfor u8info = new U8LoginInfor(u8login);
                //Logger logger = Logger.GetLogger("zxy");
                //logger.Info("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                //logger.Info("u8info.TaskID = " + u8info.TaskID);
                //logger.Close();
                _ufDataConnString = u8info.UfDataCnnString;
                _ufMetaConnString = u8info.UfMetaCnnString;
                _ufSystConnString = u8info.UfSystCnnString;
                _subid = u8info.SubID;
                datadb = u8info.AccDbName;
                metadatadb = "UFMeta_" + this._u8login.get_cAcc_Id();
            }
        }

        private bool IsReportApplyAuthControl(string viewId)
        {
            string sql = string.Format(
                @"SELECT * FROM UAP_ReportView_AuthID 
				WHERE ViewGuid=N'{0}' AND ReportGuid IN
				(SELECT ReportGuid FROM UAP_ReportView_AuthID WHERE IsReportApplyAuthControl=0)",
                viewId);
            DataSet ds = SqlHelper.ExecuteDataSet(_ufMetaConnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr != null)
                return false;
            return true;
        }
        private bool IsReportPublished(string viewId)
        {
            string sql = string.Format(
                @"select bPublished from UAP_Report 
                where ID in 
                (select ReportID from UAP_ReportView where ID='{0}') ",
                    viewId);
            DataTable dt = SqlHelper.ExecuteDataSet(_ufMetaConnString, sql).Tables[0];
            //if (Convert.ToBoolean(dt.Rows[0][0]))
            if (Convert.ToBoolean(Convert.ToInt32(dt.Rows[0][0])&1))//modify by yanghx
                return true;
            return false;
        }
        private void CloseConn()
        {
            _cnn.Close();
        }

        public const string ArgKeyAuthKey = "ArgKeyAuthKey";
        public const string ArgKeyLocaleId = "ArgKeyLocaleId";
        public const string ArgKeyAuthCaption = "ArgKeyAuthCaption";

        private clsLogin _u8login;
        private object _objectu8login;
        private string _ufMetaConnString = string.Empty;
        private string _ufSystConnString = string.Empty;
        private string _ufDataConnString = string.Empty;
        private string datadb = string.Empty;
        private string metadatadb = string.Empty;
        private string _subid = string.Empty;
        private SqlTransaction _tran;
        private SqlConnection _cnn;

        #endregion

    }
}
