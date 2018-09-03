using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using UFIDA.U8.UAP.Services.ReportDataIRuntimeMyReport;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices.QueryServices;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportData.ReportQueryXml;
using UFIDA.U8.UAP.Services.ReportData.Altova;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportElements;
using System.Collections.Generic;


namespace UFIDA.U8.UAP.Services.ReportData
{
    [ClassInterface(ClassInterfaceType.None)]
    [Serializable]
    public class ReportDataFacade
    {
        public U8LoginInfor _U8LoginInfor = null;
        private ReportListService _reportListService = null;

        internal static bool _IsUpgradeCustomReport = false;
        internal static string _DllDirectoryNotGAC = string.Empty;

        internal const string _loggerName = "ReportData";
        internal const string _VirtualAuthIdForCustomReportToPortalMenuSystem = "U870CUSTOMREPORT";
        internal const string _VirtualAuthIdForStaticReportToPortalMenuSystem = "U870STATICREPORT";
        internal const string _VirtualSysIdForMyReportToPortalMenuSystem = "RE";


        private RowAuthFacade _rowAuthFacade = null;

        #region Constructor

        public ReportDataFacade()
        {
        }

        public ReportDataFacade(object U8login)
        {
            _U8LoginInfor = new U8LoginInfor(U8login);
            this._reportListService = new ReportListService(this._U8LoginInfor);
        }

        public ReportDataFacade(U8LoginInfor logininfo)
        {
            if (logininfo == null)
                _U8LoginInfor = new U8LoginInfor();
            else
                _U8LoginInfor = logininfo;
            this._reportListService = new ReportListService(this._U8LoginInfor);
        }

        #endregion

        #region Exposed Interfeces

        #region Property

        public U8LoginInfor LoginInfor
        {
            get { return _U8LoginInfor; }
        }

        #endregion

        #region Public Method

        #region About ReportMeta

        public string GetRuntimeFormatXml(string viewId)
        {
            string sql = string.Format(
                "SELECT RuntimeFormat FROM UAP_ReportView WHERE ID=N'{0}'",
                viewId);
            object xml = SqlHelper.ExecuteScalar(this._U8LoginInfor.UfMetaCnnString, sql);
            if (xml != null)
                return xml.ToString();
            return null;
        }

        public void UpdateFilterCondition(
            string reportID,
            string filterID,
            string filterClass)
        {
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            ReportDefinition.UpdateFilterCondition(reportID, filterID, filterClass);
        }

        public void UpdateDataSourceCondition(
            string reportID,
            string className,
            string bVB,
            string functionName)
        {
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            ReportDefinition.UpdateDataSourceCondition(reportID, className, bVB, functionName);
        }

        public void SaveReport(ReportDefinition reportDefinition)
        {
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            ReportDefinition.cAccId = this._U8LoginInfor.cAccId;
            ReportDefinition.cYear = this._U8LoginInfor.cYear;
            reportDefinition.UserID = this._U8LoginInfor.UserID;
            reportDefinition.Login = this._U8LoginInfor;//ɾ��������ҪData�����Ӵ�����������������
            reportDefinition.Save();
        }

        public void SaveReportName(
            string subProjectId,
            string reportId,
            string newName,
            string localeId)
        {
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            ReportDefinition.Rename(reportId, newName, localeId, subProjectId, this._U8LoginInfor.cAccId, this._U8LoginInfor.cYear);
        }

        public void SaveAfterPropertyForm(
            ReportDefinition rd,
            SimpleView simpleView)
        {
            simpleView.cYear = this._U8LoginInfor.cYear;
            simpleView.UserID = this._U8LoginInfor.UserID;
            simpleView.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            simpleView.DataCnn = _U8LoginInfor.AccDbName;
            simpleView.ReportID = rd.ID;
            simpleView.ReportName = rd.Name;
            simpleView.cAccId = this._U8LoginInfor.cAccId;
            simpleView.Save();
        }

        public bool IsExistedReportName(
            string subProjectId,
            string reportId,
            string newName)
        {
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            return ReportDefinition.IsExistedReportName(reportId, newName, subProjectId);
        }

        public bool RetrieveReport(
            string reportId,
            out ReportDefinition reportDefinition)
        {
            ColumnCollection columns = null;
            return RetrieveReport(reportId, this._U8LoginInfor.LocaleID, out reportDefinition, out columns);
        }

        public bool RetrieveReport(
            string reportId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReport(reportId, this._U8LoginInfor.LocaleID, out reportDefinition, out columns);
        }

        public bool RetrieveReport(
            string reportId,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReportMeta(false, reportId, localeId, out reportDefinition, out columns);
        }

        public bool RetrieveReport(
            string reportId,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns,
            bool IsDesigningTime)
        {
            return RetrieveReportMeta(
                false,
                reportId,
                localeId,
                out reportDefinition,
                out columns,
                IsDesigningTime);
        }

        public bool RetrieveReportByView(
            string viewId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReportByView(viewId, this._U8LoginInfor.LocaleID, out reportDefinition, out columns);
        }

        public bool RetrieveReportByView(
            string viewId,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReportMeta(true, viewId, localeId, out reportDefinition, out columns);
        }

        public bool RetrieveReportByView(
            string viewId,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns,
            bool IsDesigningTime)
        {
            return RetrieveReportMeta(
                true,
                viewId,
                localeId,
                out reportDefinition,
                out columns,
                IsDesigningTime);
        }

        public bool RetrieveReportByViewForStaticReport(
            string viewId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReportMeta(
                true,
                viewId,
                this._U8LoginInfor.LocaleID,
                out reportDefinition,
                out columns,
                false,
                false);
        }

        #endregion

        #region About DataEngine

        public ReportDataSourceTypeEnum RetrieveDataSourceType(
            string dataSourceId,
            string functionName,
            string localeId)
        {
            Trace.WriteLine("RetrieveDataSourceType-- dataSourceId:" + dataSourceId + " functionName:" + functionName);

            ConfigureServiceProxy proxy = new ConfigureServiceProxy(
                this._U8LoginInfor.AppServer,
                this._U8LoginInfor.UfMetaCnnString);
            BusinessObject bo = proxy.GetBusinessObject(dataSourceId);
            proxy.LanguageId = localeId;
            QuerySetting qs = ((QueryFunction)bo.Functions[functionName]).QuerySettings[0];

            if (qs is SQLQuerySetting)
            {
                if ((qs as SQLQuerySetting).DataSourceType == DataSourceTypeEnum.Procedure)
                    return ReportDataSourceTypeEnum.StoreProc;
                else if ((qs as SQLQuerySetting).DataSourceType == DataSourceTypeEnum.Script)
                    return ReportDataSourceTypeEnum.SQL;
            }
            else if (qs is ERQuerySetting)
                return ReportDataSourceTypeEnum.ER;

            return ReportDataSourceTypeEnum.Custom;
        }

        public ColumnCollection RetrieveDataSource(
            string dataSourceId,
            string functionName,
            string localeId)
        {
            QuerySetting qs = this.GetQuerySetting(dataSourceId, functionName, localeId);
            return qs.QueryResultTable.Columns;
            //if (qs is ERQuerySetting)
            //{
            //    foreach (ERQueryResultColumn column in qs.QueryResultTable.Columns)
            //    {
            //       EntityRef entityRef = (EntityRef)((ERQuerySetting)qs).BaseSetting.GetDataSourceByAlias(column.DataSourceAlias);
            //       if (entityRef.bChildQuery)
            //       {

            //       }
            //       else
            //       {

            //       }
            //    }
            //}
        }

        public QuerySetting GetQuerySetting(
            string dataSourceId,
            string functionName,
            string localeId)
        {
            try
            {
                Trace.WriteLine("����ReportDataFacade.GetQuerySetting()");
                Trace.WriteLine("dataSourceId:" + dataSourceId + " functionName:" + functionName);
                Trace.WriteLine("Appserver:" + this._U8LoginInfor.AppServer);
                ConfigureServiceProxy proxy = new ConfigureServiceProxy(this._U8LoginInfor.AppServer, this._U8LoginInfor.UfMetaCnnString);
                Trace.WriteLine("begin create businessobject");
                BusinessObject bo = proxy.GetBusinessObject(dataSourceId);
                Trace.WriteLine("end get bo");
                proxy.LanguageId = localeId;
                return ((QueryFunction)bo.Functions[functionName]).QuerySettings[0];
            }
            catch (Exception e)
            {
                throw new Exception("��ȡ����Դ��Ϣʱ����:\r\nDataSourceID:" + dataSourceId + "\r\n������Ϣ:" + e.Message);
            }
        }

        public FilterConditionCollection RetrieveFilterParas(
            string dataSourceId,
            string functionName,
            string localeId)
        {
            QuerySetting qs = this.GetQuerySetting(dataSourceId, functionName, localeId);
            return qs.FilterConditions;
        }

        #endregion

        #region DesignTimePublish

        public bool Publish(string reportID, ReportPublicPosition location)
        {
            DesignTimePublishEngine.DbCnnString = this._U8LoginInfor.UfMetaCnnString;
            if (location == ReportPublicPosition.CS || location == ReportPublicPosition.CSAndBS)
            {
                DesignTimePublishEngine.Publish(reportID, _U8LoginInfor.cAccId, _U8LoginInfor.cYear, _U8LoginInfor.UserID, location);
            }
            if (location == ReportPublicPosition.BS || location == ReportPublicPosition.CSAndBS)
            {
                DesignTimePublishEngine.PublishBS(reportID, _U8LoginInfor.cAccId, _U8LoginInfor.cYear, _U8LoginInfor.UserID, this._U8LoginInfor);
            }
            return true;
        }

        public bool IsPublished(string reportID)
        {
            DesignTimePublishEngine.DbCnnString = this._U8LoginInfor.UfMetaCnnString;
            return DesignTimePublishEngine.IsPublished(reportID);
        }

        #endregion

        #region RunTime

        #region ����ԴͳһΪ��ʱ��

        /// <summary>
        /// ����������ʽ������Դ(sql�ʹ洢����)��䵽��ʱ��.
        /// ���в���dataSourceId,functionName��customDataSource
        /// ������Ϊ��
        /// </summary>
        /// <param name="dataSourceId">��������������Դid</param>
        /// <param name="functionName">���������в�ѯ���ݺ���</param>
        /// <param name="filterSrv">����������������</param>
        /// <param name="customDataSource">������ʱ����Ϣ�Ķ���</param>
        public void MoveData2TempDB4DataEngine(
            string dataSourceIdExtended,
            string dataSourceId,
            string functionName,
            FilterSrv filterSrv,
            CustomDataSource customDataSource,
            string extendedFilterstring)
        {
            if (filterSrv == null)
                Trace.Write("!!����:����ReportDataFacade.MoveData2TempDB������FilterSrvΪnull");
            if (string.IsNullOrEmpty(dataSourceId)
                || string.IsNullOrEmpty(functionName)
                || customDataSource == null)
            {
                throw new TempDBServiceException(
                    "�������������͵�����Դͳһ����ʱ��ʱ��������д�",
                    "����dataSourceId,functionName��customdatasource������Ϊ��");
            }

            try
            {

                U8DataEngine2TempTable u8de2tt = new U8DataEngine2TempTable(
                    this,
                    customDataSource,
                    dataSourceId,
                    functionName,
                    filterSrv,
                    string.Empty);
                u8de2tt.ToTempTable();

                this.ExtendDataSourceIfNecessary(
                    dataSourceIdExtended,
                    functionName,
                    filterSrv,
                    customDataSource,
                    extendedFilterstring);
            }
            catch (Exception e)
            {
                this.HandleError(e);
            }
        }

        /// <summary>
        /// ����������ά����չ,������ʱ��׼���ú��������Դ��չ
        /// </summary>
        private void ExtendDataSourceIfNecessary(
            string dataSourceIdExtended,
            string functionName,
            FilterSrv filterSrv,
            CustomDataSource customDataSource,
            string extendedFilterstring)
        {
            if (!string.IsNullOrEmpty(dataSourceIdExtended))
            {
                customDataSource.FilterString = extendedFilterstring;
                System.Diagnostics.Stopwatch watch = new Stopwatch();
                watch.Start();
                U8DataEngine2TempTable u8de2tt = new U8DataEngine2TempTable(
                    this,
                    customDataSource,
                    dataSourceIdExtended,
                    functionName,
                    filterSrv,
                    customDataSource.SQL);
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("Create U8DataEngine2TempTable Use Time:" + watch.ElapsedMilliseconds);
                watch.Start();
                u8de2tt.ToTempTable();
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("ToTempTable Use Time:" + watch.ElapsedMilliseconds);
            }
        }

        private void HandleError(Exception e)
        {
            Logger logger = Logger.GetLogger(ReportDataFacade._loggerName);
            logger.Error(e);
            logger.Close();
            if (e is TempDBServiceException)
            {
                TempDBServiceException te = e as TempDBServiceException;
                Exception root = te.RootException;
                if (root != null)
                    throw root;
            }
            throw e;
        }

        /// <summary>
        /// �Զ������͵�����Դ������ݵ���ʱ��
        /// </summary>
        /// <param name="customDataSource">
        /// ��������Դ��Ϣ�Լ�������ʱ����Ϣ�Ķ���
        /// </param>
        public void MoveData2TempDB4Custom(
            string dataSourceIdExtended,
            string dataSourceId,
            string functionName,
            FilterSrv filterSrv,
            CustomDataSource customDataSource,
            string extendedFilterstring)
        {
            if (customDataSource == null)
            {
                throw new TempDBServiceException(
                    "�������Զ�������Դͳһ����ʱ��ʱ��������д�",
                    "����customdatasource������Ϊ��");
            }

            try
            {

                CustomDataSource2TempTable cds2tt = new CustomDataSource2TempTable(
                    this, customDataSource);
                cds2tt.ToTempTable();

                this.ExtendDataSourceIfNecessary(
                    dataSourceIdExtended,
                    functionName,
                    filterSrv,
                    customDataSource,
                    extendedFilterstring);
            }
            catch (Exception e)
            {
                this.HandleError(e);
            }
        }

        /// <summary>
        /// ������չ������Ϣ��䵽��ʱ��
        /// </summary>
        /// <param name="columnInfo">
        /// Դ�����к�Ŀ�������е���Ϣ,��ϣ��ĽṹΪ��
        /// 1.��ΪĿ����������;
        /// 2.ֵΪӳ�䵽Դ�����ֶ�����.
        /// �������ֶ�����Ϊ�´�����Ŀ����е��ֶ�
        /// </param>
        /// <param name="sourceTableName">Դ���ݱ�����,����ʽΪtempdb..*</param>
        /// <param name="destTableName">Ŀ�����ݱ�����,����ʽΪtempdb..*</param>
        /// <param name="exteralService">���㼶��չ����Ϣ�ķ������</param>
        public void LevelExpandInfo2TempDB(
            Hashtable columnInfo,
            string sourceTableName,
            string destTableName,
            ILevelExpandTempDBGetDataService exteralService)
        {
            try
            {
                LevelExpandTempDBManager letdbm = new LevelExpandTempDBManager(
                    this,
                    columnInfo,
                    sourceTableName,
                    destTableName,
                    exteralService);
                letdbm.CacheExpandData();
            }
            catch (Exception e)
            {
                this.HandleError(e);
            }
        }

        #endregion

        #region Exhibition

        /// <summary>
        /// �ж�����ʱ�򿪵���ͼ��̬�����Ƿ񻹴���
        /// </summary>
        /// <param name="id">��ͼ��̬�����ID</param>
        public bool IsRunTimeItemExists(string id, RunItemType type)
        {
            if (type == RunItemType.ReportView)
                return this.IsViewExist(id);
            if (type == RunItemType.StaticReport)
                return this.IsStaticReportExist(id);
            if (type == RunItemType.Report)
                return this.IsDynamicReportExist(id);
            if (type == RunItemType.PublishData)
                return this.IsPublishDataExist(id);
            return true;
        }

        public bool IsViewExist(string id)
        {
            return IsItemExist(id, "UAP_ReportView");
        }

        private bool IsItemExist(string id, string tableName)
        {
            string sql = string.Format("SELECT ID FROM {0} WHERE ID=N'{1}'", tableName, id);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr != null)
                return true;
            return false;
        }

        public bool IsSystemReport(string id)
        {
            string sql = string.Format(
                @"SELECT ID FROM UAP_Report 
				WHERE ID=N'{0}' 
				AND (ProjectId=N'U870' OR bSystem=1)",
                id);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr != null)
                return true;
            return false;
        }

        public void UpdateDatasourceIdExtended(
            string datasourceIdExtended,
            string reportId)
        {
            ReportDataFacade.UpdateDatasourceIdExtended(
                datasourceIdExtended,
                reportId,
                this._U8LoginInfor.UfMetaCnnString);
        }

        public static void UpdateDatasourceIdExtended(
            string datasourceIdExtended,
            string reportId,
            string ufMetaCnnString)
        {
            string sql = string.Format(
                @"update UAP_Report set DatasourceIdExtended=N'{0}' where ID=N'{1}'",
                datasourceIdExtended,
                reportId);
            SqlHelper.ExecuteNonQuery(ufMetaCnnString, sql);
        }

        /// <summary>
        /// ���Ԫ����(�������ͼ)
        /// </summary>
        /// <param name="login">��װ��login��Ϣ</param>
        /// <param name="sourceId">����ԴId(����id����ͼId)</param>
        /// <param name="savaAsName">����������</param>
        /// <param name="saveAsType">�������</param>
        /// <returns>���ɹ�,����true;�Ѵ���Ҫ��������,����false</returns>
        public bool SaveAs(
            string sourceId,
            string savaAsName,
            string reportSubId,
            SaveAsType saveAsType,
            string runtimeForamtXml,
            string colorstyleid,
            string currentViewId)
        {
            RunTimeSaveAsService service = new RunTimeSaveAsService();
            return service.SaveAs(
                this._U8LoginInfor,
                sourceId,
                savaAsName,
                reportSubId,
                saveAsType,
                runtimeForamtXml,
                colorstyleid,
                currentViewId);
        }

        public void GetReportDatasByReportID(
            string reportID,
            ReportRelateInfor reportRelateInfor)
        {
            UpgradeIfNeeded(reportID);
            FillReportRelateInfor(reportID, reportRelateInfor);
        }

        public void GetReportDatasByReportIDOutU8(
            string reportID,
            ReportRelateInfor reportRelateInfor)
        {
            UpgradeIfNeededOutU8(reportID, reportRelateInfor.FilterXML, reportRelateInfor.ClassName);
            reportRelateInfor.FilterXML = null;
            reportRelateInfor.ClassName = null;
            FillReportRelateInfor(reportID, reportRelateInfor);
        }

        private void UpgradeIfNeededOutU8(string reportID, string filterstring, string classname)
        {
            object o = SqlHelper.ExecuteScalar(_U8LoginInfor.UfMetaCnnString, "select isnull(bhadupgradedfrom861,0) from uap_report where id='" + reportID + "'");
            if (o == null || Convert.ToInt32(o) == 0)
            {
                Upgrade872ControllerOutU8 uc = new Upgrade872ControllerOutU8(this._U8LoginInfor);
                uc.Upgrade(reportID, filterstring, classname);
            }
        }

        public void GetReportDatasByViewID(
            string viewID,
            ReportRelateInfor reportRelateInfor)
        {
            FillReportRelateInfor(viewID, reportRelateInfor);
        }

        public void SaveLevelExpend(string viewID, string levelExpend)
        {
            ComplexView cv = new ComplexView();
            cv.ID = viewID;
            cv.LevelExpend = levelExpend;
            cv.DBConnString = this._U8LoginInfor.UfMetaCnnString;
            cv.SaveLevelExpand();
        }

        //add by xn 2006-7-10
        public XmlDocument GetGroupSchemas(string viewID)
        {
            string sql = string.Format("SELECT GroupSchemas FROM UAP_ReportView  WHERE ID=N'{0}'", viewID);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            string gs = ds.Tables[0].Rows[0]["GroupSchemas"].ToString();
            XmlDocument xml = new XmlDocument();
            if (!gs.Equals(string.Empty))
                xml.LoadXml(gs);

            return xml;
        }

        public XmlDocument GetCrossSchemas(string viewID)
        {
            string sql = string.Format("SELECT PreservedField FROM UAP_ReportView  WHERE ID=N'{0}'", viewID);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            string gs = ds.Tables[0].Rows[0]["PreservedField"].ToString();
            XmlDocument xml = new XmlDocument();
            if (!gs.Equals(string.Empty))
                xml.LoadXml(gs);

            return xml;
        }

        /// <summary>
        /// groupSchemas:ÿ�α���ʱֻ��һ������������version�ж��ǲ������µģ�����state�ж�����ɾ�ġ�
        /// </summary>
        /// <param name="viewID"></param>
        /// <param name="groupSchemas"></param>
        public void SaveGroupSchemas(string viewID, string groupSchemas)
        {
            ComplexView cv = new ComplexView();
            cv.ID = viewID;
            cv.GroupSchemas = groupSchemas;
            cv.DBConnString = this._U8LoginInfor.UfMetaCnnString;
            cv.SaveGroupSchemas();
        }




        public void SaveCrossSchemas(string viewID, string crossSchemas)
        {
            ComplexView cv = new ComplexView();
            cv.ID = viewID;
            cv.CrossSchemas = crossSchemas;
            cv.DBConnString = this._U8LoginInfor.UfMetaCnnString;
            cv.SaveCrossSchemas();
        }

        public void SavePrintSetting(
            string viewID,
            bool blandScape,
            string pageMargins,
            string papername,
            string pagesetting)
        {
            ComplexView cv = new ComplexView();
            cv.ID = viewID;
            cv.BlandScape = blandScape;
            cv.PageMargins = pageMargins;
            cv.Columns = papername;
            cv.PageSetting = pagesetting;
            cv.DBConnString = this._U8LoginInfor.UfMetaCnnString;
            cv.SavePrintSetting();
        }

        #endregion

        #region RunTimePublishment

        public bool IsPublishDataExist(string id)
        {
            return IsItemExist(id, "UAP_ReportPublish");
        }

        #endregion

        #region MyReport

        public bool IsDynamicReportExist(string id)
        {
            bool exist = IsItemExist(id, "UAP_Report");

            //������861����
            if (!exist)
            {
                string sql = string.Format("SELECT ID FROM Rpt_Glbdef_Base WHERE (SystemID+'[__]'+Name)=N'{0}'", id);
                DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfDataCnnString, sql);
                DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
                if (dr != null)
                    return true;
                return false;
            }
            return exist;
        }

        public DataSet GetReceiveData()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportGetReceiveData");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._U8LoginInfor.UserID));
            return SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public DataSet GetMyPublishmentData()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportGetMyPublishmentData");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._U8LoginInfor.UserID));
            return SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public bool GetMitInforEnable()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportGetMitInforEnable");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._U8LoginInfor.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@IsEnabled", SqlDbType.Bit));
            SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfMetaCnnString, cmd);
            return Convert.ToBoolean(cmd.Parameters["@IsEnabled"].Value);
        }

        #region Static Report

        public bool IsStaticReportExist(string id)
        {
            return IsItemExist(id, "UAP_ReportStaticRpt");
        }

        // ����ǵ�һ���������Ż������С��ҵı�����
        // ��Ҫͨ���˷�������ȡȫ���ľ�̬����
        public Reports GetStaticReportList()
        {
            return this._reportListService.GetList4MyReportStatic();
        }

        #endregion

        #region Custom Report

        public Reports GetCustomReportList(object u8login)
        {
            this._U8LoginInfor = new U8LoginInfor(u8login);
            this._reportListService.Login = this._U8LoginInfor;
            return this.GetCustomReportList();
        }

        public Reports GetCustomReportList()
        {
            return this._reportListService.GetList4MyReportCustom();
        }

        #endregion

        #region Standard Report

        /// <summary>
        /// �ж�ָ��id�Ƿ���UAP�����id
        /// </summary>
        public bool IsUapReport(string reportId)
        {
            return this._reportListService.IsUapReport(reportId);
        }

        public Reports GetStandardReportList()
        {
            return this._reportListService.GetList4MyReportSystem();
        }

        #endregion

        #endregion

        #endregion

        #region Orthers

        public bool HasDefaultFormat(string viewId)
        {
            string sql = string.Format(
                "select ID from UAP_ReportView_DefaultBak where ID=N'{0}'",
                viewId);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr == null)
                return false;
            return true;
        }
        public bool HasFactoryFormat(string viewId)
        {
            string sql = string.Format(
                "select ID from UAP_ReportView_FactoryBak where reportid in (select reportid from uap_reportview where id=N'{0}') and viewtype in (select viewtype from uap_reportview where id=N'{0}')",
                viewId);
            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql);
            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
            if (dr == null)
                return false;
            return true;
        }

        public void ResumeViewDefaultFormat(string viewId)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ViewDefaultFormatResume");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@viewId", SqlDbType.NVarChar, 100, viewId));

            SqlHelper.ExecuteDataSet(
                this._U8LoginInfor.UfMetaCnnString,
                cmd);
        }

        public void ResumeViewFactoryFormat(string viewId, string staticViewId)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ViewFactoryFormatResume");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@viewId", SqlDbType.NVarChar, 100, viewId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@staticViewId", SqlDbType.NVarChar, 100, staticViewId));

            SqlHelper.ExecuteDataSet(
                this._U8LoginInfor.UfMetaCnnString,
                cmd);
        }
        public Hashtable FactoryViewCollection(string viewId, string localId)
        {
            Hashtable ht = new Hashtable();
            //"select b.name,a.id from UAP_ReportView_FactoryBak a "+
            //" left join uap_reportview_lang_factoryBak b on a.id=b.viewid "+
            //" where b.localeid='{0}' and a.reportid in(select top 1 reportid from uap_reportview where id='{1}' )",
            string sql = string.Format(@"select b.name,a.id from UAP_ReportView_FactoryBak a 
             left join uap_reportview_lang_factoryBak b on a.id=b.viewid 
            left join uap_reportview on a.reportid=uap_reportview.reportid
             where b.localeid='{0}' and uap_reportview.id='{1}'",
               localId, viewId);

            SqlDataReader reader = SqlHelper.ExecuteReader(this._U8LoginInfor.UfMetaCnnString, sql);
            while (reader.Read())
            {
                if (reader["id"].ToString() == viewId)
                {
                    ht.Add(reader["name"].ToString(), reader["id"].ToString());
                }
            }
            return ht;
        }

        #region ServiceForBPM

        public void PublishReport(
            string ReportUrl,
            string Creator,
            DateTime createdTime,
            ReportLocaleInfoCollection ReportLocaleInfos)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportRptPublishmentForBPM");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportUrl", SqlDbType.NVarChar, 100, ReportUrl));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Creator", SqlDbType.NVarChar, 100, Creator));
            cmd.Parameters.Add(SqlHelper.GetParameter("@CreatedTime", SqlDbType.DateTime, createdTime));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportNameCN", SqlDbType.NVarChar, 100, ReportLocaleInfos["zh-CN"].Name));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportNameTW", SqlDbType.NVarChar, 100, ReportLocaleInfos["zh-TW"].Name));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportNameUS", SqlDbType.NVarChar, 100, ReportLocaleInfos["en-US"].Name));

            SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public void DeleteBPMReport(string ReportUrl)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportRptDeleteForBPM");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportUrl", SqlDbType.NVarChar, 100, ReportUrl));
            SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public ReportInfoList GetReportInfos(string localeId)
        {
            return GetReportInfos(RunTimeReportType.StaticReport, localeId);
        }

        // ����BPM��������Ҫ(�Զ��屨��ͱ�׼����)��ʱ��
        // �᷵�ؾ�̬����,����ReportType.StaticReport
        // ʱ,��ȡ�Զ��屨��ͱ�׼����
        private ReportInfoList GetReportInfos(
            RunTimeReportType reportType,
            string localeId)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_MyReportGetReportListForBPM");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@UserID", SqlDbType.NVarChar, 100, this._U8LoginInfor.UserID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, localeId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportType", SqlDbType.Int, (object)Convert.ToInt32(reportType)));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 10, this._U8LoginInfor.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, this._U8LoginInfor.cYear));

            Trace.Write("cAccId:" + this._U8LoginInfor.cAccId);
            Trace.Write("cYear:" + this._U8LoginInfor.cYear);

            DataSet ds = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, cmd);
            ReportInfoList reportInfoList = null;
            if (ds != null && ds.Tables.Count > 0)
            {
                reportInfoList = new ReportInfoList();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string reportID = ds.Tables[0].Rows[i]["ID"].ToString();
                    string reportName = ds.Tables[0].Rows[i]["Name"].ToString();
                    string subID = ds.Tables[0].Rows[i]["SubID"].ToString();

                    bool bSystem =
                        (ds.Tables[0].Rows[i]["bSystem"] == DBNull.Value) ?
                        false
                        : Convert.ToBoolean(ds.Tables[0].Rows[i]["bSystem"]);

                    RunTimeReportType ptType = bSystem ? RunTimeReportType.StandardReport : RunTimeReportType.CustomReport;
                    ReportInfo reportInfo = new ReportInfo(reportID, reportName, subID, ptType);

                    reportInfoList.Add(reportInfo);
                }

                return reportInfoList;
            }

            return null;
        }

        #endregion

        public void UpgradeU861CustomReport(string dllDirectoryNotGAC)
        {
        }

        public DataSet RetrieveReferReportViews(string viewid, int viewType, string dataSourceID, string localeID)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_OthersRetrieveReferReportViews");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, viewid));//add by xn
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewType", SqlDbType.Int, (object)viewType));
            cmd.Parameters.Add(SqlHelper.GetParameter("@DataSourceID", SqlDbType.NVarChar, 100, dataSourceID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleID", SqlDbType.NVarChar, 10, localeID));

            return SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public void SavaRuntimeFormat(
            string viewId,
            string runtimeForamtXml,
            string colorstyleid)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_RuntimeSaveRuntimeFormat");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, viewId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@GroupSchemas", SqlDbType.NText, ""));
            cmd.Parameters.Add(SqlHelper.GetParameter("@RuntimeFormat", SqlDbType.NText, runtimeForamtXml));
            cmd.Parameters.Add(SqlHelper.GetParameter("@colorstyleid", SqlDbType.NVarChar, 100, colorstyleid));
            SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, cmd);
        }

        public List<ReportPublicInfo> GetReportsToPublish(
             string projectId,
             string subProjectId,
             string localeId)
        {
            return this._reportListService.GetList4Uap2Publish(projectId, subProjectId, localeId);
        }

        public Hashtable GetReports(
            string projectId,
            string subProjectId,
            string localeId)
        {
            return this._reportListService.GetList4UapAllReports(projectId, subProjectId, localeId);
        }

        #endregion

        #region �����û�״̬�������ȡ
        /// <summary>
        /// ������ṹ���״̬����
        /// </summary>
        /// <param name="uiState"></param>
        public void SaveReportUserStateNew(Hashtable uiState)
        {
            try
            {
                if (uiState == null || uiState.Count == 0)
                    return;
                StringBuilder sb = new StringBuilder();
                string save = "IF  EXISTS ( select reportid from UAP_ReportUserStateNew where userId=@userId and reportid=@ReportId and name='{0}')" +
               " update UAP_ReportUserStateNew set value={1} " +
               " where userId=@userId and reportid=@ReportId and name='{0}' " +
               " else insert into UAP_ReportUserStateNew(userId,ReportId,name,value)" +
               " values(@userId,@ReportId,'{0}',{1});";

                string reportId = "";
                string userId = "";
                foreach (DictionaryEntry kv in uiState)
                {
                    if (reportId == "" && kv.Key.ToString() == "ReportId")
                    {
                        reportId = kv.Value.ToString();
                        continue;
                    }
                    if (userId == "" && kv.Key.ToString() == "userId")
                    {
                        userId = kv.Value.ToString();
                        continue;
                    }
                    sb.Append(string.Format(save, kv.Key.ToString(), kv.Value.ToString()));
                }
                SqlCommand cmd = new SqlCommand(sb.ToString());

                cmd.Parameters.Add(SqlHelper.GetParameter("@userId", SqlDbType.NVarChar, 20, uiState["userId"].ToString()));
                cmd.Parameters.Add(SqlHelper.GetParameter("@ReportId", SqlDbType.NVarChar, 64, uiState["ReportId"].ToString()));

                SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfDataCnnString, cmd);
            }
            catch (Exception e)
            {

            }
        }
        /// <summary>
        ///  ������ṹ���״̬��ȡ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public Hashtable GetReportUserStateNew(string userId, string reportId)
        {
            Hashtable reportState = new Hashtable();
            reportState.Add("userId", userId);
            reportState.Add("ReportId", reportId);
            string sel = " select userId,ReportId,name,value" +
                          " from UAP_ReportUserStateNew where userId=@userId and reportid=@ReportId";
            SqlCommand cmd = new SqlCommand(sel);
            cmd.Parameters.Add(SqlHelper.GetParameter("@userId", SqlDbType.NVarChar, 20, userId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportId", SqlDbType.NVarChar, 64, reportId));
            try
            {
                DataTable dt = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfDataCnnString, cmd).Tables[0];
                //���ݿ����м�¼��������������ʾ
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        reportState.Add(SqlHelper.GetStringFrom(dr["name"]), SqlHelper.GetStringFrom(dr["value"]));
                    }
                }
                //���ݿ���û�м�¼��������Ĭ��ֵ
                else
                {
                    //Ĭ����ʾ��ѯ����
                    reportState.Add("ConditionVisible", "1");
                    //Ĭ����ʾ�������
                    reportState.Add("FilterFlag", "1");
                    //Ĭ����ʾ��ѯ����
                    reportState.Add("SolutionVisible", "1");
                    reportState.Add("FilterInfo", "0");
                    reportState.Add("IncludeBackColor", "0");
                    reportState.Add("Layout", "0");
                    reportState.Add("LayoutIsSetted", "0");
                    reportState.Add("printDefault", "0");
                    reportState.Add("PrintSet", "0");
                    reportState.Add("QuickFilterLayoutFlag", "0");
                }
            }
            catch
            {
                //Ĭ����ʾ��ѯ����
                reportState.Add("ConditionVisible", "1");
                //Ĭ����ʾ�������
                reportState.Add("FilterFlag", "1");
                //Ĭ����ʾ��ѯ����
                reportState.Add("SolutionVisible", "1");
                reportState.Add("FilterInfo", "0");
                reportState.Add("IncludeBackColor", "0");
                reportState.Add("Layout", "0");
                reportState.Add("LayoutIsSetted", "0");
                reportState.Add("printDefault", "0");
                reportState.Add("PrintSet", "0");
                reportState.Add("QuickFilterLayoutFlag", "0");
            }
            return reportState;
        }

        public void SaveReportUserState(Hashtable uiState)
        {
            string save =
            "IF  EXISTS ( select reportid from UAP_ReportUserState where userId=@userId and reportid=@ReportId)" +
            " update UAP_ReportUserState set PrintSet=@PrintSet,printDefault=@printDefault,FilterInfo=@FilterInfo,ExtendState=@ExtendState," +
            " IncludeBackColor=@IncludeBackColor,Layout=@Layout where userId=@userId and reportid=@ReportId" +
            " else insert into UAP_ReportUserState(userId,ReportId,PrintSet,printDefault,FilterInfo,ExtendState,IncludeBackColor,Layout)" +
            " values(@userId,@ReportId,@PrintSet,@printDefault,@FilterInfo,@ExtendState,@IncludeBackColor,@Layout)";
            SqlCommand cmd = new SqlCommand(save);
            cmd.Parameters.Add(SqlHelper.GetParameter("@userId", SqlDbType.NVarChar, 20, uiState["userId"].ToString()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportId", SqlDbType.NVarChar, 64, uiState["ReportId"].ToString()));
            cmd.Parameters.Add(SqlHelper.GetParameter("@PrintSet", SqlDbType.SmallInt, uiState["PrintSet"]));
            cmd.Parameters.Add(SqlHelper.GetParameter("@printDefault", SqlDbType.SmallInt, uiState["printDefault"]));
            cmd.Parameters.Add(SqlHelper.GetParameter("@FilterInfo", SqlDbType.SmallInt, uiState["FilterInfo"]));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ExtendState", SqlDbType.SmallInt, uiState["ExtendState"]));
            cmd.Parameters.Add(SqlHelper.GetParameter("@IncludeBackColor", SqlDbType.Bit, Convert.ToBoolean(uiState["IncludeBackColor"])));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Layout", SqlDbType.SmallInt, uiState["Layout"]));
            SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfDataCnnString, cmd);
        }

        public Hashtable GetReportUserState(string userId, string reportId)
        {
            Hashtable reportState = new Hashtable();
            reportState.Add("userId", userId);
            reportState.Add("ReportId", reportId);
            string sel = " select userId,ReportId,PrintSet,printDefault,FilterInfo,ExtendState,IncludeBackColor,Layout" +
                          " from UAP_ReportUserState where userId=@userId and reportid=@ReportId";
            SqlCommand cmd = new SqlCommand(sel);
            cmd.Parameters.Add(SqlHelper.GetParameter("@userId", SqlDbType.NVarChar, 20, userId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportId", SqlDbType.NVarChar, 64, reportId));
            try
            {
                DataTable dt = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfDataCnnString, cmd).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    reportState.Add("PrintSet", SqlHelper.GetIntFrom(dr["PrintSet"]));
                    reportState.Add("printDefault", SqlHelper.GetIntFrom(dr["printDefault"]));
                    reportState.Add("FilterInfo", SqlHelper.GetIntFrom(dr["FilterInfo"]));
                    reportState.Add("ExtendState", SqlHelper.GetIntFrom(dr["ExtendState"]));
                    reportState.Add("IncludeBackColor", SqlHelper.GetBooleanFrom(dr["IncludeBackColor"], false));
                    reportState.Add("Layout", SqlHelper.GetIntFrom(dr["Layout"]));
                }
                else
                {
                    reportState.Add("PrintSet", 1);
                    reportState.Add("printDefault", 1);
                    reportState.Add("FilterInfo", 1);
                    reportState.Add("ExtendState", null);
                    reportState.Add("IncludeBackColor", false);
                    reportState.Add("Layout", 1);
                }
            }
            catch
            {
                //�½ű�û�ύ����ʱ�쳣���׳�
                reportState.Add("PrintSet", 1);
                reportState.Add("printDefault", 1);
                reportState.Add("FilterInfo", 1);
                reportState.Add("ExtendState", null);
                reportState.Add("IncludeBackColor", false);
                reportState.Add("Layout", 1);
            }
            return reportState;
        }

        #endregion

        #endregion

        #endregion

        #region Private Method

        private void UpgradeCustomReportAuth()
        {
            Trace.Write("��ʼ:UpgradeCustomReportAuth()...");

            SqlCommand cmd = new SqlCommand("uap_reportview_updatecustomreport");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@accid", SqlDbType.NVarChar, 10, this._U8LoginInfor.cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@year", SqlDbType.NVarChar, 10, this._U8LoginInfor.cYear));
            SqlHelper.ExecuteNonQuery(this._U8LoginInfor.UfMetaCnnString, cmd);

            Trace.Write("����:UpgradeCustomReportAuth()");
        }

        /// <summary>
        /// 872�������������
        /// ����GetReportDatasByReportID(����ʱ)
        /// ��RetrieveReportMeta(UAP���ʱ)����
        /// </summary>
        public void UpgradeIfNeeded(string reportID)
        {
            Upgrade872Controller uc = new Upgrade872Controller(this._U8LoginInfor);
            int i = uc.Is861Report(reportID);
            if (i == 1)
                uc.Upgrade(reportID);
            else if (i == 2)
                uc.UpgradeBO(reportID);
            i = uc.IsNeedFactoryBak(reportID);
            if (i == 1)
                uc.UpgradeFactoryBak(reportID);
            i = uc.IsNeedExpandLanguage(reportID);
            if (i == 1)
                uc.UpgradeLang(reportID);
        }

        private void FillReportRelateInfor(
            string ID,
            ReportRelateInfor reportRelateInfor)
        {
            reportRelateInfor.InputID = ID;
            reportRelateInfor.cAccId = this._U8LoginInfor.cAccId;
            reportRelateInfor.cYear = this._U8LoginInfor.cYear;
            reportRelateInfor.UserID = this._U8LoginInfor.UserID;
            reportRelateInfor.LocaleID = this._U8LoginInfor.LocaleID;
            reportRelateInfor.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            reportRelateInfor.Retrieve();
        }

        private bool RetrieveReportMeta(
            bool IsByViewId,
            string Id,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns)
        {
            return RetrieveReportMeta(IsByViewId, Id, localeId, out reportDefinition, out columns, false);
        }

        private bool RetrieveReportMeta(
            bool isByViewId,
            string id,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns,
            bool isDesigningTime)
        {
            return this.RetrieveReportMeta(
                isByViewId,
                id,
                localeId,
                out reportDefinition,
                out columns,
                isDesigningTime,
                true);
        }

        private bool RetrieveReportMeta(
            bool isByViewId,
            string id,
            string localeId,
            out ReportDefinition reportDefinition,
            out ColumnCollection columns,
            bool isDesigningTime,
            bool isApplyAuthControl)
        {
            Trace.WriteLine("����ReportData.RetrieveReportMeta()");
            Trace.WriteLine("begin retrieve meta data.....");
            if (!isByViewId)
            {
                UpgradeIfNeeded(id);
            }
            else
            {
                string sql = string.Format(@"select reportid from uap_reportview where id ='{0}'", id);
                DataTable dt = SqlHelper.ExecuteDataSet(this._U8LoginInfor.UfMetaCnnString, sql).Tables[0];
                if (dt.Rows.Count != 0)
                    UpgradeIfNeeded(dt.Rows[0][0].ToString());
            }

            reportDefinition = new ReportDefinition();
            ReportDefinition.DbConnString = this._U8LoginInfor.UfMetaCnnString;
            ReportDefinition.cAccId = this._U8LoginInfor.cAccId;
            ReportDefinition.cYear = this._U8LoginInfor.cYear;
            reportDefinition.UserID = this._U8LoginInfor.UserID;
            reportDefinition.ID = id;
            reportDefinition.LocaleID = localeId;
            bool isOK = reportDefinition.Retrieve(
                isByViewId,
                isDesigningTime,
                !isApplyAuthControl);
            if (isOK)
            {
                columns = null;
                if (_U8LoginInfor.SubID != "OutU8")
                    columns = this.RetrieveDataSource(reportDefinition.GetActualDataSourceId(), reportDefinition.FunctionName, localeId);
            }
            else
            {
                reportDefinition = null;
                columns = null;
                return false;
            }
            Trace.WriteLine("end retrieve meta data ");
            return true;
        }

        #endregion

        /// <summary>
        /// ��ȡ�й��˵�where������
        /// </summary>
        /// <param name="fieldString">������</param>
        /// <param name="bWeb">�Ƿ�web����</param>
        /// <returns>���˵�where������</returns>
        protected string GetRowAuth(string subid, string fieldString, bool bWeb)
        {
            if (this._U8LoginInfor == null)
                throw new Exception("U8LoginInforû�г�ʼ��");

            if (this._rowAuthFacade == null)
                this._rowAuthFacade = new RowAuthFacade();
            return this._rowAuthFacade.GetRowAuth(subid, "", fieldString, this._U8LoginInfor, bWeb);
        }


    }
}