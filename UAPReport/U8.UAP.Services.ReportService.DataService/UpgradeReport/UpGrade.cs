//using System;
//using System.IO;
//using System.Xml;
//using System.Data;
//using System.Diagnostics;
//using System.Data.SqlClient;
//using System.Collections;
//using UFIDA.U8.UAP.Services.BizDAE.Elements;

//namespace UFIDA.U8.UAP.Services.ReportData
//{
//    internal class UpgradeEngine
//    {
//        #region Parameter

//        private const int _SuperLabelInforFieldCount = 8;

//        private const string _InValidatedID = "InValidatedID";
//        private const string _ReportFunction = "ReportFunction";

//        private const string _ViewNameZHCN = "ԭU861��ͼ";
//        private const string _ViewNameZHTW = "ԭU861ҕ�D";
//        private const string _ViewNameENUS = "View of U861";

//        // �����б�
//        // ע�⣺��һ���������Ա�����"zh-CN"
//        private string[] LanguageIDList = new string[] { "zh-CN", "zh-TW", "en-US" };
//        private string[] ColumnInfoForSQLQueristConfig;	//	��������Ϣ���Ժ󽫻�д��SQLQueristConfig
//        private string[] CrossDetailItems;

//        private bool IsCrossReport;
//        private bool _IsAllTypeBeGridLabel = false;
//        private bool IsSuperLabelDataArrayListEmpty;

//        private string UfDataConnString;
//        private string UfMetaConnString;
//        private string ReportName;
//        private string ReportTitle;
//        private string ReportID;
//        private string SubID;
//        private string SectionName;
//        private string LocaleId;
//        private string ParentColumnName;
//        private string QueristStringForDataEngine;
//        private string SQLQueristScript;
//        private string ClassName;
//        private string FilterClass;
//        private string sCurrentElementName;
//        private string CrossColumnHeaderItem;
//        private string _BaseTableReportID;				//  ���ı�����Ҫ��Դ���id����ȡGetValue��GetName����Ϣ
//        private string _LevelExpand;

//        private int CurrentColumnLevelTop;			//	��¼GetParentColumnCation��Ҫ�����ĸ�����
//        private int iMaxColumnLevelCount;			//	���ı������
//        private int ReportHeadBottom;				//	���������ĵ׶�
//        private int ReportPageTitleBottom;			//	�б������ĵ׶�
//        private int PreviousColumnX;				//	��һ�е�X
//        private int CurrentColumnX;					//	��ǰ�е�X
//        private int PreviousColumnLeftExIndex;		//	���ݱ�����LeftEx��ʶ�е�λ��˳�򣬴˱�ʶ��Ϊ��һ�е�λ��

//        private XmlTextWriter FormatDatasXmlWriter;
//        private XmlTextWriter FormatDatasLangXmlWriter;

//        private ArrayList SuperLabelDataArrayList;		//	�ݴ�SuperLabel��һЩ���ݣ��ȵ�����ȷ�����ȵ�ʱ����һ��д��
//        private Hashtable ColumnNameArrayList;			//	�洢�����Թ���ȡ������ʱ���в�ѯ
//        private ArrayList _SchemaItemNameArrayList;
//        private ArrayList _SchemaItemLangArrayList;

//        private SortedList _DataSourceNoLangInfo;			// �����������涨������Դ����Ҫ������������Դ����Ϣ����д��һ��XML��
//        private SortedList _DataSourceLangInfo;

//        private SqlConnection cnn;
//        private SqlCommand cmd;
//        private SqlDataReader reader;

//        private UpgradeReport _upgradeReport;
//        private MendWhenUpgrading _MendWhenUpgrading;
//        private ReportDefinition _BaseReportDefinition;	//	���ı�����Զ��屨��ʹ��Դ��������Ϣ	

//        #endregion

//        #region Constructor

//        public UpgradeEngine(
//            string ufDataConnString,
//            string ufMetaConnString)
//        {
//            UfDataConnString = ufDataConnString;
//            UfMetaConnString = ufMetaConnString;
//            ColumnInfoForSQLQueristConfig = new string[3];
//        }

//        #endregion

//        #region Public Methods

//        public UpgradeReport GetUpgradeData(string reportID)
//        {
//            try
//            {
//                if (IsExportReport(reportID))
//                {
//                    UpgradeExportReport upgradeExportReport = new UpgradeExportReport(
//                        reportID,
//                        this.UfDataConnString,
//                        this.UfMetaConnString);
//                    return upgradeExportReport;
//                }

//                CheckReportID(reportID);

//                _BaseReportDefinition = null;
//                _BaseTableReportID = string.Empty;
//                _LevelExpand = string.Empty;

//                IsCrossReport = false;
//                SQLQueristScript = string.Empty;

//                SuperLabelDataArrayList = null;
//                ColumnNameArrayList = null;
//                _SchemaItemNameArrayList = null;
//                _SchemaItemLangArrayList = null;

//                _DataSourceNoLangInfo = null;
//                _DataSourceLangInfo = null;

//                _DataSourceNoLangInfo = new SortedList();
//                _DataSourceLangInfo = new SortedList();

//                PrepareDatas();

//                _upgradeReport.ReportViews[0].GroupSchemas = GetGroupSchemasXml();
//                _upgradeReport.ReportViews[0].LevelExpand = _LevelExpand;
//                SetDataSourceInfor();

//                return _upgradeReport;
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.Upgrade", e);
//                return null;
//            }
//        }

//        #endregion

//        #region Private Method

//        private bool IsExportReport(string reportID)
//        {
//            if (reportID.Substring(2, 4) != "[__]")
//            {
//                DataSet ds = SqlHelper.ExecuteDataSet(
//                    this.UfDataConnString,
//                    UpgradeSqlProducer.GetSqlExportReportReportProperty(reportID));
//                if (ds.Tables[0].Rows.Count > 0)
//                    return true;
//            }
//            return false;
//        }

//        private void CheckReportID(string reportID)
//        {
//            if (reportID.Length <= 6
//                || reportID.Substring(2, 4) != "[__]")
//            {
//                System.Exception e = new System.Exception(
//                    string.Format("��������:����ı����ʶ\"{0}\"����ȷ.\r\n��ȷ�ı����ʶ����ʽ����: **[__]******", reportID));
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.CheckReportID", e);
//            }

//            SubID = reportID.Substring(0, 2);
//            ReportName = reportID.Substring(6, reportID.Length - 6);
//        }

//        private void PrepareDatas()
//        {
//            // ��ε��û�ȡ���ݽӿ�ʱ��Ҫ����ϴ�������Ϣ
//            if (_upgradeReport != null)
//            {
//                _upgradeReport.ReportLocaleInfos.Clear();
//                _upgradeReport.ReportViews[0].ViewLocaleInfos.Clear();
//            }

//            if (cnn == null)
//            {
//                if (OpenConnection())
//                    for (int i = 0; i < LanguageIDList.Length; i++)
//                        WriteReportData(LanguageIDList[i]);
//            }
//            else
//            {
//                if (cnn.State != System.Data.ConnectionState.Open)
//                    cnn.Open();
//                for (int i = 0; i < LanguageIDList.Length; i++)
//                    WriteReportData(LanguageIDList[i]);
//            }

//            if (cnn.State == System.Data.ConnectionState.Open)
//                cnn.Close();
//        }


//        private string GetStringFromSream(System.IO.MemoryStream MS)
//        {
//            string sTemp = string.Empty;
//            if (MS != null && MS.CanSeek && MS.CanRead)
//            {
//                System.IO.StreamReader sr = new System.IO.StreamReader(MS);
//                MS.Seek(0, System.IO.SeekOrigin.Begin);
//                sTemp = sr.ReadToEnd();
//                sr.Close();
//            }

//            return sTemp;
//        }

//        private void FormatXmlWriteStartElement(string StartElementName)
//        {
//            sCurrentElementName = StartElementName;
//            if (LocaleId.ToLower() == "zh-cn")
//                FormatDatasXmlWriter.WriteStartElement(StartElementName);

//            if (StartElementName.ToLower() != "algorithm"
//                || StartElementName.ToLower() != "prepaintevent")
//                FormatDatasLangXmlWriter.WriteStartElement(StartElementName);
//        }

//        private void FormatXmlWriteEndElement()
//        {
//            if (LocaleId.ToLower() == "zh-cn")
//                FormatDatasXmlWriter.WriteEndElement();

//            if (sCurrentElementName.ToLower() != "algorithm"
//                || sCurrentElementName.ToLower() != "prepaintevent")
//                FormatDatasLangXmlWriter.WriteEndElement();
//            else
//                // ����������Ҫ����Ϊ������Ҫ�ų�FormatDatasLangXmlWriter��
//                // Ԫ��Algorithm��Ӧ��EndElement�������EndElementӦ��ͨ��
//                sCurrentElementName = "";
//        }

//        private void FormatXmlWriteAttribute(string Name, string Value)
//        {
//            if (LocaleId.ToLower() == "zh-cn"
//                && (!IsLanguageOrSectionAttribute(Name) || (sCurrentElementName == "Report")))
//                FormatDatasXmlWriter.WriteAttributeString(Name, Value);

//            if (sCurrentElementName == "Section"
//                || IsLanguageOrSectionAttribute(Name)
//                || Name == "Name")
//                FormatDatasLangXmlWriter.WriteAttributeString(Name, Value);
//        }

//        private bool IsLanguageOrSectionAttribute(string AttributeName)
//        {
//            if (AttributeName == "Type"
//                || AttributeName == "X"
//                || AttributeName == "Y"
//                || AttributeName == "Width"
//                || AttributeName == "Height"
//                || AttributeName == "Caption")
//                return true;
//            return false;
//        }

//        private void CloseXmlWriter()
//        {
//            if (!reader.IsClosed)
//                reader.Close();
//            if (FormatDatasXmlWriter.WriteState != System.Xml.WriteState.Closed)
//                FormatDatasXmlWriter.Close();
//            if (FormatDatasLangXmlWriter.WriteState != System.Xml.WriteState.Closed)
//                FormatDatasLangXmlWriter.Close();
//        }

//        private bool OpenConnection()
//        {
//            try
//            {
//                cnn = new System.Data.SqlClient.SqlConnection();
//                cnn.ConnectionString = UfDataConnString;
//                cnn.Open();
//                cmd = new System.Data.SqlClient.SqlCommand();
//                cmd.Connection = cnn;
//                return true;
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.OpenConnection", e);
//                return false;
//            }
//        }

//        private void WriteReportData(string sLocaleId)
//        {
//            LocaleId = sLocaleId;
//            ReportID = _InValidatedID;

//            MemoryStream CommonFormatMS = new MemoryStream();
//            MemoryStream LanguageFormatMS = new MemoryStream();

//            //FileStream CommonFormatMS = new FileStream( @"F:\ldq.xml" , System.IO.FileMode.OpenOrCreate );

//            FormatDatasXmlWriter = new XmlTextWriter(CommonFormatMS, null);
//            FormatDatasLangXmlWriter = new XmlTextWriter(LanguageFormatMS, null);

//            FormatDatasXmlWriter.Formatting = Formatting.Indented;
//            FormatDatasLangXmlWriter.Formatting = Formatting.Indented;

//            bool IsGoOn = WriteReportBeginInfor();
//            if (!IsGoOn)
//                return;

//            WriteSections();
//            WriteGroupSchemas();
//            WriteReportEndInfor();

//            FormatDatasXmlWriter.Flush();
//            FormatDatasLangXmlWriter.Flush();

//            if (LocaleId.ToUpper().Trim() == "ZH-CN")
//                FillCommonDatas(CommonFormatMS);
//            FillLangDatas(LanguageFormatMS);

//            CloseXmlWriter();
//        }

//        private void SetIsAllTypeBeGridLabel()
//        {
//            string sql = UpgradeSqlProducer.GetSqlIsAllTypeBeGridLabel(ReportID);
//            DataSet ds = SqlHelper.ExecuteDataSet(UfDataConnString, sql);
//            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
//            if (dr == null)
//            {
//                _IsAllTypeBeGridLabel = true;
//                return;
//            }

//            if (ds.Tables[0].Rows.Count == 1
//                && (SqlHelper.GetStringFrom(dr[0]) == "0"
//                    || dr[0] == DBNull.Value))
//            {
//                _IsAllTypeBeGridLabel = true;
//            }
//            else
//            {
//                _IsAllTypeBeGridLabel = false;
//            }
//        }

//        // �������ı�����������Դ����Sql�ű�ʱ, 
//        // ��ʹ���丸������Զ�������Դ,����870
//        // ��׼������861����ͼʱҲ��������������Դ
//        private bool IsUseExistDataSource()
//        {
//            if ((_BaseReportDefinition != null && QueristStringForDataEngine != "SQLQuerist")
//                || IsUpdateU861View())
//            {
//                return true;
//            }

//            return false;
//        }

//        // �����ǰƴ�ӳɵ�id�Ѿ�������UAP_Report��,
//        // ��ǰ��������870��׼������861����ͼ
//        private bool IsUpdateU861View()
//        {
//            string id = string.Format("{0}[__]{1}", this.SubID, this.ReportName);
//            string sql = UpgradeSqlProducer.GetSqlNewReportInfor(id);
//            DataSet ds = SqlHelper.ExecuteDataSet(UfMetaConnString, sql);
//            if (SqlHelper.GetDataRowFrom(0, ds) != null)
//                return true;
//            return false;
//        }

//        private void FillCommonDatas(MemoryStream CommonFormatMS)
//        {
//            if (_upgradeReport == null)
//            {
//                _upgradeReport = new UpgradeReport();
//                _upgradeReport.ReportViews.Add(new UpgradeReportView(_upgradeReport));
//            }

//            //	������: �ž��� [mailto:zjt@ufida.com.cn] 
//            //	����ʱ��: 2006-05-11 10:31
//            //	�ռ���: 'Seaven�����ĺ���'
//            //	����: �¹���ID
//            //	rsSql("systemID") & "[__]" & rsSql("name")
//            _upgradeReport.ID = string.Format("{0}[__]{1}", this.SubID, this.ReportName);
//            _upgradeReport.SubProjectID = this.SubID;
//            _upgradeReport.FunctionName = _ReportFunction;

//            // �Զ��屨��ʹ�õ����Ѵ��ڵ�BO����
//            if (IsUseExistDataSource())
//            {
//                if (_BaseReportDefinition != null)
//                {
//                    _upgradeReport.FilterID = _BaseReportDefinition.FilterID;
//                    _upgradeReport.DataSourceID = _BaseReportDefinition.DataSourceID;
//                    _upgradeReport.ClassName = _BaseReportDefinition.ClassName;
//                    _upgradeReport.FilterClass = _BaseReportDefinition.FilterClass;
//                }

//                _upgradeReport.bSystem = false;
//                _upgradeReport.Creator = "Customer's Report Administrator";
//                _upgradeReport.ReportViews[0].bSystem = false;
//            }
//            else
//            {
//                _upgradeReport.FilterID = GetFilterID();
//                _upgradeReport.DataSourceID = Guid.NewGuid().ToString();
//                _upgradeReport.ClassName = ClassName;
//                _upgradeReport.FilterClass = FilterClass;
//            }

//            _upgradeReport.bSystem = false;
//            _upgradeReport.ReportViews[0].bSystem = false;
//            _upgradeReport.ReportViews[0].ID = Guid.NewGuid().ToString();
//            _upgradeReport.ReportViews[0].CommonFormat = GetStringFromSream(CommonFormatMS);

//            if (this.IsCrossReport)
//                _upgradeReport.ReportViews[0].ViewType = 3;
//        }

//        private string GetFilterID()
//        {
//            string sql = string.Format(
//                "select * from rpt_fltdef_base where id=N'{0}'",
//                ReportID);
//            DataSet ds = SqlHelper.ExecuteDataSet(UfDataConnString, sql);
//            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
//            if (dr != null)
//                return _upgradeReport.ID;
//            return string.Empty;
//        }

//        private void FillLangDatas(MemoryStream LanguageFormatMS)
//        {
//            if (_upgradeReport != null)
//            {
//                ReportLocaleInfo reportLocaleInfo;
//                reportLocaleInfo = new ReportLocaleInfo();
//                reportLocaleInfo.Name = ReportTitle;
//                reportLocaleInfo.LocaleID = LocaleId;

//                ReportViewLocaleInfo reportViewLocaleInfo;
//                reportViewLocaleInfo = new ReportViewLocaleInfo();
//                reportViewLocaleInfo.Name = GetDefaultViewName();
//                reportViewLocaleInfo.LocaleID = LocaleId;
//                reportViewLocaleInfo.LocaleFormat = GetStringFromSream(LanguageFormatMS);

//                _upgradeReport.ReportLocaleInfos.Add(reportLocaleInfo);
//                _upgradeReport.ReportViews[0].ViewLocaleInfos.Add(reportViewLocaleInfo);
//            }
//        }

//        private string GetDefaultViewName()
//        {
//            switch (LocaleId.ToLower())
//            {
//                case "zh-cn":
//                    return _ViewNameZHCN;
//                case "zh-tw":
//                    return _ViewNameZHTW;
//                case "en-us":
//                    return _ViewNameENUS;
//                default:
//                    return "No View Name";
//            }
//        }

//        private void SetDataForUserDefineReport()
//        {
//            if (LocaleId.ToUpper() != "ZH-CN")
//                return;

//            string note = MendWhenUpgrading.GetFieldNote(UfDataConnString, ReportID);

//            // ��������±��������Զ��屨��
//            if (note != string.Empty && note != ReportName)
//            {
//                string baseTableNewReportId = string.Format("{0}[__]{1}", SubID.ToUpper(), note);
//                Trace.Write("baseTableNewReportId:" + baseTableNewReportId);
//                RaiseExceptionIfSpecialID(baseTableNewReportId);

//                // ���ܴ���_BaseTableDataSourceIDΪ�յ����,Ҫ����Ӧ�Ĵ���
//                _BaseReportDefinition = MendWhenUpgrading.GetSqlBaseReportDefinition(UfMetaConnString, baseTableNewReportId);
//                _BaseTableReportID = MendWhenUpgrading.GetSqlBaseTableOldId(UfDataConnString, ReportID, SubID);
//            }

//            if (_BaseTableReportID == string.Empty)
//                _BaseTableReportID = ReportID;
//        }

//        private void RaiseExceptionIfSpecialID(string reportId)
//        {
//            if (reportId.IndexOf("SA[__]�������") != -1
//                || reportId.IndexOf("ST[__]ҵ�����ͻ��ܱ�") != -1
//                || reportId.IndexOf("ST[__]�շ�����ܱ�") != -1
//                || reportId.IndexOf("ST[__]������λ�շ�����ܱ�") != -1)
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Close();
//                throw new Exception("�˱���Ϊ���ⱨ�����ܽ����������ؼ��֣�" + reportId);
//            }
//        }

//        private bool WriteReportBeginInfor()
//        {
//            try
//            {
//                cmd.CommandText = UpgradeSqlProducer.GetSqlOldReportInfor(ReportName, SubID, LocaleId);
//                reader = cmd.ExecuteReader();
//                if (reader.Read())
//                {
//                    ReportID = reader["ID"].ToString().Trim();
//                    ReportTitle = reader["Title"].ToString().Trim();
//                    SubID = reader["SystemID"].ToString().Trim().ToUpper();

//                    _IsAllTypeBeGridLabel = false;
//                    SetIsAllTypeBeGridLabel();

//                    SetDataForUserDefineReport();
//                    SetLevelExpandXml();

//                    // ׼����ȷ����Դ����
//                    _MendWhenUpgrading = new MendWhenUpgrading(ReportID, LocaleId, UfDataConnString);

//                    FormatXmlWriteStartElement("Report");
//                    FormatXmlWriteAttribute("Name", ReportName);
//                    FormatXmlWriteAttribute("Type", "GridReport");

//                    // �Զ��屨��ClassName��FilterClass��Ϊ��
//                    //ClassName	= reader[ "ClassName" ].ToString().Trim();
//                    //FilterClass	= reader[ "FilterClass" ].ToString().Trim();
//                    ClassName = string.Empty;
//                    FilterClass = string.Empty;

//                    if (reader["IsBaseTable"] != DBNull.Value
//                        && Convert.ToBoolean(reader["IsBaseTable"]))
//                    {
//                        QueristStringForDataEngine = "CustomQuerist";
//                    }
//                    else
//                        QueristStringForDataEngine = "SQLQuerist";
//                }
//                else
//                {
//                    reader.Close();
//                    if (LocaleId.ToUpper() == "ZH-CN")
//                    {
//                        throw new Exception(
//                           string.Format("��������:����ϵͳ{0}�в����ڱ���\"{1}\"", SubID, ReportName));
//                    }

//                    return false;
//                }

//                reader.Close();

//                if (QueristStringForDataEngine == "SQLQuerist" && SQLQueristScript == string.Empty)
//                    GetSQLQueristScript();

//                return true;
//            }
//            catch (Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteReportBeginInfor", e);
//                return false;
//            }
//        }

//        private void GetSQLQueristScript()
//        {
//            cmd.CommandText = string.Format(
//                "SELECT Expression FROM rpt_flddef_base WHERE id=N'{0}' AND ModeEx=24 AND LocaleId=N'zh-CN' ORDER BY ID_Field",
//                this.ReportID);

//            reader = cmd.ExecuteReader();
//            while (reader.Read())
//            {
//                SQLQueristScript += reader["Expression"].ToString();
//            }
//            reader.Close();

//            // ֻ��Ҫ����һ�ν��汨������������
//            if (this.CrossDetailItems == null || this.CrossColumnHeaderItem == string.Empty)
//                HandleIfCrossReport(ref SQLQueristScript);

//            // ȥ��SELECT * FROM ({0}) A ģʽ
//            //if( SQLQueristScript != string.Empty )
//            //    SQLQueristScript = string.Format( "SELECT * FROM ({0}) A" , SQLQueristScript );
//        }

//        // "TRANSFORM ���,����,����  PIVOT Ҫ������ SELECT Pm_PubElementVouchs.cElementCode AS Ҫ�ر���,..."
//        // Ҫ������"���,����,����"��"Ҫ������",�Լ������Sql�ű�
//        private void HandleIfCrossReport(ref string script)
//        {
//            int indexTRANSFORM = script.IndexOf("TRANSFORM");
//            int indexPIVOT = script.IndexOf("PIVOT");
//            int indexSELECT = script.IndexOf("SELECT");

//            string sTemp = string.Empty;
//            if (indexTRANSFORM != -1
//                && indexPIVOT != -1)
//            {
//                this.IsCrossReport = true;

//                sTemp = script.Substring(indexTRANSFORM + 9, indexPIVOT - indexTRANSFORM - 9).Trim();
//                this.CrossDetailItems = sTemp.Split(',');
//                this.CrossColumnHeaderItem = script.Substring(indexPIVOT + 5, indexSELECT - indexPIVOT - 5).Trim();
//                script = script.Substring(indexSELECT, script.Length - indexSELECT).Trim();
//            }
//        }

//        private void WriteReportEndInfor()
//        {
//            if (ReportID != _InValidatedID)
//                FormatXmlWriteEndElement();
//        }

//        private void WriteSections()
//        {
//            //	���ݱ�rpt_FldDef_base�е��ֶ�ModeEx=25��ʾ�������������ݣ�
//            //	��ʱ���е��ֶ�OrderEx��ʾ������������

//            int GroupIndex = 0;
//            ReportHeadBottom = 0;
//            ReportPageTitleBottom = 0;

//            try
//            {
//                for (int OrderEx = 1; ReportID != _InValidatedID; OrderEx++)
//                {
//                    //	�������ݿ�Ĵ���һЩ��ǩ��ModeEx�ֶ�Ҳ����25����Ҫ�ų�
//                    //	����ʱnameForeign���벻�ܵ��ڱ�������,ҳ��������,ҳ������,��ע����
//                    cmd.CommandText = "select * from rpt_flddef_base where id='"
//                        + ReportID
//                        + "' and LocaleId=N'"
//                        + LocaleId
//                        + "' and ModeEx=25 and OrderEx="
//                        + OrderEx.ToString()
//                        + " and ISNULL(nameForeign,'')<>N'��������' "
//                        + "and ISNULL(nameForeign,'')<>N'���}�^��' "
//                        + "and ISNULL(nameForeign,'')<>N'Header region' "
//                        + "and ISNULL(nameForeign,'')<>N'ҳ��������' "
//                        + "and ISNULL(nameForeign,'')<>N'퓘��}�^��' "
//                        + "and ISNULL(nameForeign,'')<>N'Page title area' "
//                        + "and ISNULL(nameForeign,'')<>N'ҳ��ע����' "
//                        + "and ISNULL(nameForeign,'')<>N'��]�_�^��' "
//                        + "and ISNULL(nameForeign,'')<>N'ҳ������' "
//                        + "and ISNULL(nameForeign,'')<>N'��_�^��' "
//                        + "and ISNULL(nameForeign,'')<>N'Page footnote area' "
//                        + "and ISNULL(nameForeign,'')<>N'Page footer region' "
//                        + "and ISNULL(nameForeign,'')<>N'��ע����' "
//                        + "and ISNULL(nameForeign,'')<>N'�]�_�^��' "
//                        + "and ISNULL(nameForeign,'')<>N'Footnote area'";

//                    reader = cmd.ExecuteReader();
//                    if (reader.Read())
//                    {
//                        if (reader["FormatEx"].ToString().Trim() != "����")
//                        {
//                            SectionName = reader["Name"].ToString().Trim();

//                            // ����ǽ��汨����Ҫ�������CrossDetail
//                            // ��CrossColumnHeader����

//                            if (this.IsCrossReport)
//                                WriteCrossDetailCrossColumnHeaderIfCrossReport(SectionName);

//                            FormatXmlWriteStartElement("Section");
//                            WriteSectionAtrrNameType(SectionName, ref GroupIndex);

//                            CalculateTwoBottom(SectionName);

//                            //	��ʼд��������֮ǰҪ�ȹر�reader��ʹ��
//                            //	��������cnn����������reader
//                            reader.Close();
//                            WriterLabelsData(SectionName);

//                            FormatXmlWriteEndElement();
//                        }
//                    }
//                    else
//                    {
//                        break;
//                    }
//                    if (!reader.IsClosed)
//                    {
//                        reader.Close();
//                    }
//                }
//                if (!reader.IsClosed)
//                {
//                    reader.Close();
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteSections", e);
//            }
//        }

//        private void WriteCrossDetailCrossColumnHeaderIfCrossReport(string SectionName)
//        {
//            // ��������֮ǰд������������
//            if (SectionName == "��������"
//                || SectionName == "���ą^��"
//                || SectionName == "Body region")
//            {
//                FormatXmlWriteStartElement("Section");
//                FormatXmlWriteAttribute("Name", "CrossColumnHeader");
//                FormatXmlWriteAttribute("Type", "CrossColumnHeader");
//                WriteCrossColumnHeaderDetailItem(this.CrossColumnHeaderItem, "ColumnHeader", 0);
//                FormatXmlWriteEndElement();

//                FormatXmlWriteStartElement("Section");
//                FormatXmlWriteAttribute("Name", "CrossDetail");
//                FormatXmlWriteAttribute("Type", "CrossDetail");
//                if (this.CrossDetailItems != null)
//                {
//                    for (int i = 0; i < this.CrossDetailItems.Length; i++)
//                        WriteCrossColumnHeaderDetailItem(this.CrossDetailItems[i], "GridDecimal", i);
//                }

//                FormatXmlWriteEndElement();
//            }
//        }

//        // itemIndex���������ǩ�����λ��
//        private void WriteCrossColumnHeaderDetailItem(
//            string caption,
//            string type,
//            int itemIndex)
//        {
//            FormatXmlWriteStartElement("Control");
//            FormatXmlWriteAttribute("Name", "Column" + itemIndex.ToString());
//            FormatXmlWriteAttribute("Type", type);
//            FormatXmlWriteAttribute("X", System.Convert.ToString(40 + (itemIndex * 96)));
//            FormatXmlWriteAttribute("Y", "48");
//            FormatXmlWriteAttribute("Width", "96");
//            FormatXmlWriteAttribute("Height", "24");
//            FormatXmlWriteAttribute("Caption", caption);
//            FormatXmlWriteAttribute("Visible", "true");
//            WriteCaptionAlignAttribute(null);
//            FormatXmlWriteAttribute("bSummary", "true");
//            FormatXmlWriteAttribute("DataSource", caption);
//            FormatXmlWriteEndElement();
//        }

//        private void AddCrossDetailDataSourceColumn()
//        {
//            AddSourceColumn(this.CrossColumnHeaderItem, "String");
//            for (int i = 0; i < this.CrossDetailItems.Length; i++)
//                AddSourceColumn(this.CrossDetailItems[i], "Decimal");
//        }

//        // ������б��ʽ�����汨���������Ҫ��������Դ��Ϣ
//        private void AddSourceColumn(
//            string name,
//            string dataType)
//        {
//            string[] columnDataSourceInfo = new string[3];
//            columnDataSourceInfo[0] = name;
//            columnDataSourceInfo[1] = dataType;
//            columnDataSourceInfo[2] = name;

//            KeepDataSourceInfo(columnDataSourceInfo, "zh-CN");
//            KeepDataSourceInfo(columnDataSourceInfo, "zh-TW");
//            KeepDataSourceInfo(columnDataSourceInfo, "en-US");
//        }

//        private void WriteSectionAtrrNameType(string SectionName, ref int GroupIndex)
//        {
//            try
//            {
//                switch (SectionName)
//                {
//                    case "��������":
//                    case "���}�^��":
//                    case "Header region":
//                        FormatXmlWriteAttribute("Name", "ReportHeader");
//                        FormatXmlWriteAttribute("Type", "ReportHeader");
//                        break;
//                    case "ҳ��������":
//                    case "퓘��}�^��":
//                    case "Page title area":
//                        FormatXmlWriteAttribute("Name", "PageHeader");
//                        FormatXmlWriteAttribute("Type", "PageHeader");
//                        break;
//                    case "��������":
//                    case "���ą^��":
//                    case "Body region":
//                        if (!this.IsCrossReport)
//                        {
//                            FormatXmlWriteAttribute("Name", "GridDetail");
//                            FormatXmlWriteAttribute("Type", "GridDetail");
//                        }
//                        else
//                        {
//                            FormatXmlWriteAttribute("Name", "CrossRowHeader");
//                            FormatXmlWriteAttribute("Type", "CrossRowHeader");
//                        }

//                        break;
//                    case "ҳ��ע����":
//                    case "��]�_�^��":
//                    case "Page footnote area":
//                        FormatXmlWriteAttribute("Name", "PageFooter");
//                        FormatXmlWriteAttribute("Type", "PageFooter");
//                        break;

//                    //	�ɱ���Ľ�ע����ת��Ϊ�±����ҳ��ע����
//                    case "��ע����":
//                    case "�]�_�^��":
//                    case "Footnote area":
//                        FormatXmlWriteAttribute("Name", "PageFooter");
//                        FormatXmlWriteAttribute("Type", "PageFooter");
//                        break;
//                    default:
//                        GroupIndex++;
//                        FormatXmlWriteAttribute("Name", "GroupHeader" + GroupIndex.ToString());
//                        FormatXmlWriteAttribute("Type", "GroupHeader");
//                        break;
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteSectionAtrrNameType", e);
//            }
//        }

//        private void CalculateTwoBottom(string SectionName)
//        {
//            try
//            {
//                if (SectionName == "��������"
//                    || SectionName == "���}�^��"
//                    || SectionName == "Header region")
//                {
//                    ReportHeadBottom = (int)reader["TopEx"] + (int)reader["Height"];
//                    ReportHeadBottom = ReportHeadBottom / 15;
//                }
//                else if (SectionName == "��������"
//                    || SectionName == "���ą^��"
//                    || SectionName == "Body region")
//                {
//                    ReportPageTitleBottom = (int)reader["TopEx"] + (int)reader["Height"];
//                    ReportPageTitleBottom = ReportPageTitleBottom / 15;
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.CalculateTwoBottom", e);
//            }
//        }

//        private string ConvertVBTiwpToPixel(object DBFieldValue)
//        {
//            try
//            {
//                return System.Convert.ToString((int)DBFieldValue / 15);
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.ConvertVBTiwpToPixel", e);
//                return string.Empty;
//            }
//        }

//        private void WriterLabelsData(string SectionName)
//        {
//            if (SectionName == "��������"
//                || SectionName == "���ą^��"
//                || SectionName == "Body region")
//            {
//                WriterLabelsInBodyRegion();
//            }
//            else
//            {
//                WriterLabelsNotInBodyRegion(SectionName);
//            }
//        }

//        private void WriterLabelsInBodyRegion()
//        {
//            try
//            {
//                CurrentColumnX = 7;	//	�б������������뱨���Ե7����
//                PreviousColumnLeftExIndex = 0;
//                ParentColumnName = "";
//                ColumnInfoForSQLQueristConfig[0] = "";
//                ColumnInfoForSQLQueristConfig[1] = "";
//                ColumnInfoForSQLQueristConfig[2] = "";
//                SuperLabelDataArrayList = new ArrayList();
//                ColumnNameArrayList = new Hashtable();
//                IsSuperLabelDataArrayListEmpty = true;

//                //	Ԥ�����ı��������ΪһЩû����Ӧ���б�����Ϣ�������ñ������
//                SetMaxColumnLevelCount();

//                cmd.CommandText = UpgradeSqlProducer.GetSqlColumnsInforInDetailSection(
//                    this.ReportID,
//                    this.LocaleId);
//                reader = cmd.ExecuteReader();

//                //	Ϊ���ModeEx=0ʱ����ͬ��OrderEx��������¼������
//                //	�ȼ�¼�Ѿ�������У��ٲ��ҳ�����ͬ��OrderEx����
//                //	�����Щ�л�û�м��룬����Ҫ���
//                Hashtable columnsAdded = new Hashtable();
//                while (reader.Read())
//                {
//                    bool isShouldHandle = false;
//                    string name = SqlHelper.GetStringFrom(reader["Column_Name"]);
//                    string expression = SqlHelper.GetStringFrom(reader["Column_Expression"]);
//                    if (expression != string.Empty)
//                    {
//                        if (columnsAdded[expression] == null)
//                        {
//                            columnsAdded.Add(expression, name);
//                            isShouldHandle = true;
//                        }

//                        // ˫���ͷ���ϲ��ͷΪSuperLable,sql��ѯ��
//                        // ��ָʾ������ԴΪǰ��ĳ���Ѿ����������Դ
//                        else if (GetColumnDataType() == "SuperLable")
//                            isShouldHandle = true;
//                    }
//                    else if (columnsAdded[name] == null)
//                    {
//                        columnsAdded.Add(name, name);
//                        isShouldHandle = true;
//                    }

//                    if (isShouldHandle)
//                        HandleAColumn();
//                }

//                //	����һ������SuperLabel���е���Ϣд��SQLQueristConfig
//                WriteColumnSQLQueristConfig();
//                reader.Close();

//                //	�����������ͬ��OrderEx��������¼����©����
//                HandleTowOrderExEqual(columnsAdded);
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriterLabelsInBodyRegion", e);
//            }
//        }

//        private void HandleTowOrderExEqual(Hashtable columnsAdded)
//        {
//            if (columnsAdded != null && columnsAdded.Count > 0)
//            {
//                cmd.CommandText = UpgradeSqlProducer.GetSqlAllColumnInfor(this.ReportID, this.LocaleId);
//                reader = cmd.ExecuteReader();
//                while (reader.Read())
//                {
//                    string name = SqlHelper.GetStringFrom(reader["Column_Name"]);
//                    string expression = SqlHelper.GetStringFrom(reader["Column_Expression"]);

//                    if (expression != string.Empty)
//                    {
//                        if (columnsAdded[expression] == null)
//                            HandleAColumn();
//                    }
//                    else if (columnsAdded[name] == null)
//                    {
//                        HandleAColumn();
//                    }
//                }

//                //	����һ������SuperLabel���е���Ϣд��SQLQueristConfig
//                WriteColumnSQLQueristConfig();
//                reader.Close();
//            }
//        }

//        private void HandleAColumn()
//        {
//            // �����ǰ�����ǽ��汨��,���ڽ��汨��CrossDetail
//            // ������оͲ����ڽ��汨��rossRowHeader������
//            if (this.IsCrossReport
//                && this.IsACrossDetailColumn(reader["Column_Expression"]))
//            {
//                return;
//            }

//            string sCaption;
//            string sColumnDataType = GetColumnDataType();

//            //	�����ǰ������������SuperLabel������������ݴ�����
//            if (sColumnDataType == "SuperLable")
//            {
//                SaveSuperLabelDataToArrayList();

//                //	ΪSQLQueristConfig��¼������Ϣ
//                ParentColumnName = GetParentColumnCation();
//            }
//            else
//            {
//                //	�Ȱ�ǰһ������SuperLabel���е���Ϣд��SQLQueristConfig
//                WriteColumnSQLQueristConfig();

//                sCaption = GetCaption(reader["Title_nameForeign"], reader["Title_Name"]);
//                FormatXmlWriteStartElement("Control");
//                string cellName = GetCellName(sColumnDataType);
//                FormatXmlWriteAttribute("Name", cellName);
//                FormatXmlWriteAttribute("Type", sColumnDataType);
//                FormatXmlWriteAttribute("X", GetX(reader["Title_LeftEx"], reader["Column_iColSize"]));
//                FormatXmlWriteAttribute("Y", GetY(reader["Title_TopEx"], reader["Column_Height"]));
//                FormatXmlWriteAttribute("Width", GetWidth(reader["Title_Width"], reader["Column_iColSize"]));
//                FormatXmlWriteAttribute("Height", GetHeight(reader["Title_Height"], reader["Column_Height"]));
//                FormatXmlWriteAttribute("Caption", sCaption);
//                FormatXmlWriteAttribute("Visible", GetVisible(reader["Column_Visible"]));
//                WritePrecisionOrFormatStringAttribute(reader["Column_FormatEx"]);
//                WriteCaptionAlignAttribute(reader["Column_iAlignStyle"]);
//                WriteDataSourceAttribute(reader, sColumnDataType, sCaption, cellName);
//                WriteUserDefineItemAttributeIfNeeded(reader);
//                FormatXmlWriteEndElement();

//                //	ΪSQLQueristConfig��¼������Ϣ
//                ParentColumnName = GetParentColumnCation();

//                //	���SuperLabelDataArrayList��Ϊ�գ������Ƿ���SuperLabel
//                //	�Ѿ�����ȷ����ȣ�����У���Ҫ��ʱд��������
//                if (!IsSuperLabelDataArrayListEmpty)
//                {
//                    HandleArrayList(reader["Title_LeftEx"]);
//                }
//            }
//        }

//        private string GetCellName(string columnDataType)
//        {
//            string cellName = string.Empty;

//            // ��Ϊ������ʱ��ȡName�ֶ�ֵΪcellName
//            if (columnDataType == "GridDecimalAlgorithmColumn"
//                || columnDataType == "GridCalculateColumn")
//            {
//                if (this.LocaleId.ToUpper() != "ZH-CN")
//                {
//                    string orderEx = SqlHelper.GetStringFrom(reader["Column_OrderEx"]);
//                    string topEx = SqlHelper.GetStringFrom(reader["Column_TopEx"]);
//                    cellName = _MendWhenUpgrading.GetCaculateColumnName(ReportID, orderEx, topEx);

//                    Trace.Write(string.Format("{0}:{1}", this.LocaleId, cellName));
//                    Trace.Write(string.Format(
//                        "ReportID:{0};OrderEx:{1};TopEx:{2}",
//                        ReportID,
//                        orderEx,
//                        topEx));
//                }
//                else
//                {
//                    cellName = SqlHelper.GetStringFrom(reader["Column_Expression"]);

//                    // ��Щ���������������ExpressionΪ��
//                    // ��ʱȡName�ֶ�
//                    if (cellName == string.Empty)
//                        cellName = SqlHelper.GetStringFrom(reader["Column_Name"]);
//                }
//            }
//            else
//            {
//                cellName = "Column" + GetIDField("0");
//            }

//            ReplaceInvalidChar(ref cellName);
//            return cellName;
//        }

//        private string GetIDField(string modeEx)
//        {
//            string id_Field = SqlHelper.GetStringFrom(reader["Title_ID_Field"]);

//            // ModeEx="5"ʱ,��ʾ˫���ǩ�Ķ����ǩ��id_field
//            // ��ȡ��ϸ�����ݵ�SQL���������BAK_ID_Field�洢
//            if (modeEx == "5")
//                id_Field = SqlHelper.GetStringFrom(reader["BAK_ID_Field"]);

//            if (this.LocaleId.ToUpper() == "ZH-CN")
//                return id_Field;

//            string sql = UpgradeSqlProducer.GetSqlIDFieldInZHCN(this.ReportID, modeEx, id_Field);
//            DataSet ds = SqlHelper.ExecuteDataSet(this.UfDataConnString, sql);
//            DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
//            if (dr != null)
//                return SqlHelper.GetStringFrom(dr[0]);
//            else
//            {
//                throw new System.Exception(
//                    string.Format("��������û�в��ҵ�ID_Field���ؼ��֣�{0};{1};{2}",
//                    id_Field,
//                    this.LocaleId,
//                    this.ReportID));
//            }
//        }

//        private void ReplaceInvalidChar(ref string name)
//        {
//            string charForReplace = "_";
//            name = name.Replace("%", charForReplace);
//            name = name.Replace(".", charForReplace);
//            name = name.Replace("[", charForReplace);
//            name = name.Replace("]", charForReplace);
//            name = name.Replace("(", charForReplace);
//            name = name.Replace(")", charForReplace);
//            name = name.Replace("<", charForReplace);
//            name = name.Replace(">", charForReplace);
//            name = name.Replace("{", charForReplace);
//            name = name.Replace("}", charForReplace);
//        }

//        private bool IsACrossDetailColumn(object DBFieldCondition)
//        {
//            string dataSourceName = GetConvertedExpressionString(DBFieldCondition);
//            if (IsInCrossDetailItems(dataSourceName))
//                return true;

//            return false;
//        }

//        private bool IsInCrossDetailItems(string dataSourceName)
//        {
//            if (this.CrossDetailItems != null)
//            {
//                foreach (string s in this.CrossDetailItems)
//                    if (dataSourceName == s)
//                        return true;
//            }

//            return false;
//        }

//        private void SetMaxColumnLevelCount()
//        {
//            try
//            {
//                iMaxColumnLevelCount = 1;
//                string sql = UpgradeSqlProducer.GetSqlMaxColumnLevelCount(ReportID);
//                DataSet ds = SqlHelper.ExecuteDataSet(UfDataConnString, sql);
//                DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
//                if (dr != null)
//                    iMaxColumnLevelCount = SqlHelper.GetIntFrom(dr[0], 1);
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.SetMaxColumnLevelCount", e);
//            }
//        }

//        private void HandleArrayList(object DBFieldLeftEx)
//        {
//            int iTemp;
//            bool bRunOnce = true;
//            string[] sSuperLabelDatas;

//            try
//            {
//                string sLeftEx = DBFieldLeftEx.ToString().Trim();
//                for (int i = 0; i < SuperLabelDataArrayList.Count; i++)
//                {
//                    sSuperLabelDatas = (string[])SuperLabelDataArrayList[i];
//                    if (sSuperLabelDatas[0] == sLeftEx)
//                    {
//                        //	ΪSQLQueristConfig��¼������Ϣ
//                        if (bRunOnce)
//                        {
//                            bRunOnce = false;
//                            ParentColumnName = GetParentColumnCation();
//                        }

//                        //	�����SuperLabel�Ŀ��
//                        iTemp = System.Convert.ToInt32(sSuperLabelDatas[4]);
//                        sSuperLabelDatas[4] = System.Convert.ToString(CurrentColumnX - iTemp);

//                        FormatXmlWriteStartElement("Control");
//                        FormatXmlWriteAttribute("Name", "Column" + sSuperLabelDatas[7]);
//                        FormatXmlWriteAttribute("Type", sSuperLabelDatas[1]);
//                        FormatXmlWriteAttribute("X", sSuperLabelDatas[2]);
//                        FormatXmlWriteAttribute("Y", sSuperLabelDatas[3]);
//                        FormatXmlWriteAttribute("Width", sSuperLabelDatas[4]);
//                        FormatXmlWriteAttribute("Height", sSuperLabelDatas[5]);
//                        FormatXmlWriteAttribute("Caption", sSuperLabelDatas[6]);
//                        FormatXmlWriteEndElement();

//                        SuperLabelDataArrayList.RemoveAt(i);
//                        if (SuperLabelDataArrayList.Count == 0)
//                        {
//                            IsSuperLabelDataArrayListEmpty = true;
//                            break;
//                        }
//                        else
//                            i = -1;		//	���¿�ʼHandleArrayList
//                    }
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.HandleArrayList", e);
//            }
//        }

//        private void SaveSuperLabelDataToArrayList()
//        {
//            string[] sSuperLabelDatas;
//            sSuperLabelDatas = new string[_SuperLabelInforFieldCount];

//            try
//            {
//                //	��¼��Title_LeftExΪ��ֵʱ����ȷ����SuperLabel�Ŀ��
//                sSuperLabelDatas[0] =
//                    System.Convert.ToString((int)reader["Title_LeftEx"] + (int)reader["Title_Width"] - 1);
//                sSuperLabelDatas[1] = "SuperLabel";
//                sSuperLabelDatas[2] = GetX(reader["Title_LeftEx"], reader["Column_iColSize"]);
//                sSuperLabelDatas[3] = GetY(reader["Title_Topex"], reader["Column_Height"]);
//                sSuperLabelDatas[4] = sSuperLabelDatas[2];
//                sSuperLabelDatas[5] = GetHeight(reader["Title_Height"], reader["Column_Height"]);
//                sSuperLabelDatas[6] = GetCaption(reader["Title_NameForeign"], reader["Title_Name"]);
//                sSuperLabelDatas[7] = GetIDField("5");
//                SuperLabelDataArrayList.Add(sSuperLabelDatas);
//                IsSuperLabelDataArrayListEmpty = false;
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.SaveSuperLabelDataToArrayList", e);
//            }
//        }

//        private void WriteColumnSQLQueristConfig()
//        {
//            // Ϊ����ò����滻�б��ʽ��������Դ��Name������Description�ķ�����
//            // �˴�����Ϊ�ж���ǩ����ı�ǩ�Ӹ��еı�ʶ����C(A.B)
//            //if( ParentColumnName != "" )
//            //{	
//            //    ParentColumnName					= "(" + ParentColumnName + ")";
//            //    ColumnInfoForSQLQueristConfig[2]	= ColumnInfoForSQLQueristConfig[2] + ParentColumnName;
//            //}

//            //	��ֹ��һ����SuperLabel���г���ʱ�ͱ�����Ϣ
//            //	��Ϊ��ʱ��û�д洢�κ�����Ϣ
//            if (ColumnInfoForSQLQueristConfig[0] != "")
//                KeepDataSourceInfo(ColumnInfoForSQLQueristConfig);
//        }

//        private class ColumnLangInfoNode
//        {
//            public string LocaleId;
//            public string InfoValue;

//            public ColumnLangInfoNode() { }
//        }

//        private void KeepDataSourceInfo(string[] DataSourceInfo)
//        {
//            KeepDataSourceInfo(DataSourceInfo, LocaleId);
//        }

//        // ����������Դ��Ϣ�����������������������֮������֯��XML��
//        private void KeepDataSourceInfo(string[] DataSourceInfo, string localeId)
//        {
//            if (_DataSourceNoLangInfo[DataSourceInfo[0]] == null)
//            {
//                string[] NoLangInfo = new string[2];
//                NoLangInfo[0] = DataSourceInfo[0];
//                NoLangInfo[1] = DataSourceInfo[1];
//                _DataSourceNoLangInfo.Add(DataSourceInfo[0], NoLangInfo);

//                ColumnLangInfoNode[] LangInfo = new ColumnLangInfoNode[1];
//                LangInfo[0] = new ColumnLangInfoNode();
//                LangInfo[0].InfoValue = DataSourceInfo[2];
//                LangInfo[0].LocaleId = localeId;
//                _DataSourceLangInfo.Add(DataSourceInfo[0], LangInfo);
//            }
//            else
//            {
//                // ��ԭ��������Ϣ�б���¼���һ��������Ϣ
//                ColumnLangInfoNode[] LangInfo = _DataSourceLangInfo[DataSourceInfo[0]] as ColumnLangInfoNode[];
//                if (LangInfo != null)
//                {
//                    ColumnLangInfoNode[] NewLangInfo = new ColumnLangInfoNode[LangInfo.Length + 1];

//                    // �ȴ洢ԭ�����Ѿ����ڵ���Ϣ
//                    for (int i = 0; i < LangInfo.Length; i++)
//                    {
//                        // ����Ѿ���Ҫ��ӵ����Ե���Ϣ����
//                        // �Ե�ǰ����Ϣ���¸������Ե���Ϣ
//                        // ��ʱ��������µ�������Ϣ
//                        if (LangInfo[i].LocaleId.ToUpper() == localeId.ToUpper())
//                        {
//                            // ����������нű�ʱ���÷���AddDataSourceKey
//                            // ������Դ��name��description��ͬ
//                            // ��ʱӦ��ֹ�˲����޸�ԭ�е�������Ϣ
//                            if (DataSourceInfo[2] != DataSourceInfo[0])
//                                LangInfo[i].InfoValue = DataSourceInfo[2];
//                            return;
//                        }

//                        NewLangInfo[i] = LangInfo[i];
//                    }

//                    // ����µ�������Ϣ
//                    NewLangInfo[LangInfo.Length] = new ColumnLangInfoNode();
//                    NewLangInfo[LangInfo.Length].InfoValue = DataSourceInfo[2];
//                    NewLangInfo[LangInfo.Length].LocaleId = localeId;
//                    _DataSourceLangInfo[DataSourceInfo[0]] = NewLangInfo;
//                }
//            }
//        }

//        private void SetDataSourceInfor()
//        {
//            // �Զ��屨��ʱʹ�õ����Ѵ��ڵ�BO����
//            if (IsUseExistDataSource())
//            {
//                _upgradeReport.DataSourceInfo.IsShouldSave = false;
//                return;
//            }

//            // �����ʱ�������ǽ��汨��,���������б�����������
//            // Դ���ܲ���ԭ����ModeEx=0�Ķ�����,������Ҫ���
//            if (this.IsCrossReport)
//                AddCrossDetailDataSourceColumn();

//            //			System.IO.FileStream DataSourceXmlMS = new System.IO.FileStream( @"F:\ldq.xml" , System.IO.FileMode.OpenOrCreate );
//            MemoryStream DataSourceXmlMS = new MemoryStream();
//            XmlTextWriter writer = new XmlTextWriter(DataSourceXmlMS, null);
//            writer.Formatting = Formatting.Indented;

//            WriteStartSqlQueristConfig(writer);
//            WriteDataSourceColumnInfo(writer);
//            WriteEndSqlQueristConfig(writer);

//            writer.Flush();
//            string tempString = GetStringFromSream(DataSourceXmlMS);
//            writer.Close();

//            // ����861�Զ��屨��ʱ����������Դ��ֱ��д���ݱ�BD_BusinessObjects��
//            // ��������������������ݷ���
//            if (ReportDataFacade._IsUpgradeCustomReport)
//            {
//                _upgradeReport.DataSourceInfo.MetaID = _upgradeReport.DataSourceID;
//                _upgradeReport.DataSourceInfo.ProjectNo = "U8CUSTDEF";
//                _upgradeReport.DataSourceInfo.SubNo = _upgradeReport.SubProjectID;
//                _upgradeReport.DataSourceInfo.MetaInfo = tempString;
//            }
//            else
//            {
//                BusinessObject bo = new BusinessObject();
//                bo.MetaID = _upgradeReport.DataSourceID;
//                bo.SubNo = _upgradeReport.SubProjectID;
//                bo.ProjectNo = "U8CUSTDEF";
//                bo.Deserialize(tempString);
//                _upgradeReport.DataSourceInfo.DataSourceBO = bo;
//            }
//        }

//        private void WriteStartSqlQueristConfig(XmlTextWriter writer)
//        {
//            writer.WriteStartDocument();

//            string boName = string.Format("{0}({1})BO", ReportName, this.SubID);
//            writer.WriteStartElement("BusinessObject");
//            writer.WriteAttributeString("name", boName);
//            _upgradeReport.DataSourceInfo.Name = boName;

//            // wangzq�涨description��󳤶�Ϊ50
//            string description = ReportName + UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.UpGrade.����Դ����");
//            if (description.Length > 50)
//                description = description.Substring(0, 50);
//            writer.WriteAttributeString("description", description);
//            _upgradeReport.DataSourceInfo.Description = description;

//            writer.WriteStartElement("Function");
//            writer.WriteAttributeString("type", "Query");
//            writer.WriteAttributeString("name", _ReportFunction);
//            writer.WriteAttributeString("description", "ComeFromU870ReportUpdateEngine");

//            writer.WriteStartElement("DataQueries");

//            writer.WriteStartElement("Query");
//            writer.WriteAttributeString("name", ReportName);
//            writer.WriteAttributeString("querist", QueristStringForDataEngine);
//            writer.WriteAttributeString("description", "ComeFromU870ReportUpdateEngine");

//            writer.WriteStartElement("ResultTable");
//            writer.WriteAttributeString("name", ReportName);
//            writer.WriteAttributeString("description", "ComeFromU870ReportUpdateEngine");
//        }

//        private void WriteEndSqlQueristConfig(XmlTextWriter writer)
//        {
//            writer.WriteEndElement();

//            // ��Ԫ�ء�ResultTable��֮��д���Զ���sql�ű����Զ�������Ϣ
//            if (QueristStringForDataEngine == "SQLQuerist")
//                WriteSQLQueristInfor(writer);
//            else if (QueristStringForDataEngine == "CustomQuerist")
//                WriteCustomQueristInfor(writer);

//            writer.WriteEndElement();
//            writer.WriteEndElement();
//            writer.WriteEndElement();
//            writer.WriteEndElement();
//        }

//        private void WriteSQLQueristInfor(XmlTextWriter writer)
//        {
//            if (SQLQueristScript == string.Empty)
//                return;

//            DispartSqlSelect dispartSqlSelect = new DispartSqlSelect();
//            dispartSqlSelect.Dispart(SQLQueristScript);

//            writer.WriteStartElement("Content");
//            writer.WriteAttributeString("type", "Script");

//            writer.WriteStartElement("Select");
//            writer.WriteString(dispartSqlSelect.SelectPart);
//            writer.WriteEndElement();

//            writer.WriteStartElement("Where");
//            writer.WriteString(dispartSqlSelect.WherePart);
//            writer.WriteEndElement();

//            writer.WriteStartElement("OrderBy");
//            writer.WriteString(dispartSqlSelect.OrderByPart);
//            writer.WriteEndElement();

//            writer.WriteStartElement("GroupBy");
//            writer.WriteString(dispartSqlSelect.GroupByPart);
//            writer.WriteEndElement();

//            writer.WriteStartElement("Having");
//            writer.WriteString(dispartSqlSelect.HavingPart);
//            writer.WriteEndElement();

//            writer.WriteEndElement();
//        }

//        private void WriteCustomQueristInfor(XmlTextWriter writer)
//        {
//            writer.WriteStartElement("Content");
//            writer.WriteAttributeString("dllType", "Com");
//            writer.WriteAttributeString("typeInfo", this._upgradeReport.ClassName);
//            writer.WriteEndElement();
//        }

//        private void WriteDataSourceColumnInfo(System.Xml.XmlTextWriter writer)
//        {
//            if (_DataSourceNoLangInfo == null || _DataSourceLangInfo == null)
//                return;

//            foreach (string[] NoLangInfo in _DataSourceNoLangInfo.Values)
//                if (NoLangInfo != null && NoLangInfo.Length == 2)
//                {
//                    ColumnLangInfoNode[] LangInfo = _DataSourceLangInfo[NoLangInfo[0]] as ColumnLangInfoNode[];
//                    if (LangInfo != null && LangInfo.Length > 0)
//                    {
//                        writer.WriteStartElement("Column");
//                        writer.WriteAttributeString("name", NoLangInfo[0]);
//                        writer.WriteAttributeString("dataType", NoLangInfo[1]);
//                        writer.WriteAttributeString("description", LangInfo[0].InfoValue);

//                        writer.WriteStartElement("MulitLangInfo");
//                        for (int i = 0; i < LangInfo.Length; i++)
//                        {
//                            writer.WriteStartElement("Lang");
//                            writer.WriteAttributeString("id", LangInfo[i].LocaleId);
//                            writer.WriteAttributeString("value", LangInfo[i].InfoValue);
//                            writer.WriteEndElement();
//                        }

//                        writer.WriteEndElement();
//                        writer.WriteEndElement();
//                    }
//                }
//        }

//        //	��ȡ���е�����
//        private string GetColumnDataType()
//        {
//            if (reader["Title_Width"] != System.DBNull.Value
//                && (int)reader["Title_Width"] > 1)
//            {
//                return "SuperLable";
//            }
//            else
//            {
//                // ����֮�������û��Զ��屨��,�����������:
//                // iSize�ֶζ�Ϊ0ʱ���Ͷ�����ΪGridLabel
//                //if( _IsAllTypeBeGridLabel )
//                //    return "GridLabel";

//                string sCondition = SqlHelper.GetStringFrom(reader["Column_CONditiON"]);
//                string sColumnKey = SqlHelper.GetStringFrom(reader["Column_Expression"]);
//                if (sColumnKey == string.Empty)
//                    sColumnKey = SqlHelper.GetStringFrom(reader["Column_Name"]);

//                //	�б��ʽ
//                if (sCondition != string.Empty)
//                {
//                    //	�б��ʽ��������һ�е�ĳЩ�ֶ�
//                    if (sCondition.IndexOf("PREV_", StringComparison.CurrentCultureIgnoreCase) != -1
//                        || sCondition.IndexOf("IIF(", StringComparison.CurrentCultureIgnoreCase) != -1
//                        || _MendWhenUpgrading.IsPercentColumn(sColumnKey)
//                        || _MendWhenUpgrading.IsPercentGroupColumn(sColumnKey))
//                    {
//                        return "GridDecimalAlgorithmColumn";
//                    }
//                    else
//                    {
//                        return "GridCalculateColumn";
//                    }
//                }
//                else
//                {
//                    //'�������ͳ�������(861ϵͳ����ϵͳ)
//                    //Public Const cDatatypeNumber = 0                    '��ֵ����
//                    //Public Const cDatatypeString = 1                    '�ַ�������
//                    //Public Const cDatatypeDate = 2                      '��������
//                    //Public Const cDatatypeBoolean = 3                   '�߼�����ֵ
//                    int iSize;
//                    if (reader["Column_iSize"] == System.DBNull.Value)
//                        iSize = 1;
//                    else
//                        iSize = (int)reader["Column_iSize"];
//                    switch (iSize)
//                    {
//                        case 0:
//                            return "GridDecimal";
//                        case 1:
//                            return "GridLabel";
//                        case 2:
//                            return "GridDateTime";
//                        //case 3:
//                        //return "GridBoolean";
//                        default:
//                            return "GridLabel";		//	Ĭ������
//                    }
//                }
//            }
//        }

//        private string GetX(object DBFieldLeftEx, object DBFieldColumniColSize)
//        {
//            //	Ĭ�ϵ�λ����Ϊ67����
//            int iColumniColSize = 67;

//            if (DBFieldColumniColSize != System.DBNull.Value
//                && (int)DBFieldColumniColSize != 0)
//            {
//                iColumniColSize = (int)DBFieldColumniColSize / 15;
//            }

//            try
//            {
//                // 2006-06-16
//                // ��һЩ�Զ�����Ĵ���(��Щ�Զ�������ֶ�LeftEx��Ϊ0)
//                if (DBFieldLeftEx == System.DBNull.Value
//                    || (int)DBFieldLeftEx == 0)
//                {
//                    PreviousColumnLeftExIndex++;
//                    PreviousColumnX = CurrentColumnX;
//                    CurrentColumnX = CurrentColumnX + iColumniColSize;
//                    return System.Convert.ToString(PreviousColumnX);
//                }

//                //	��ǰ��LeftEx����һ��LeftEx��ͬʱ����ǰΪSuperLabel
//                //	������ȷ�����ȣ����Ա���״ֵ̬PreviousColumnLeftExIndex��
//                //	PreviousColumnX��CurrentColumnX����
//                else if (PreviousColumnLeftExIndex == (int)DBFieldLeftEx)
//                {
//                    return Convert.ToString(PreviousColumnX);
//                }
//                else
//                {
//                    PreviousColumnLeftExIndex = (int)DBFieldLeftEx;
//                    PreviousColumnX = CurrentColumnX;
//                    CurrentColumnX = CurrentColumnX + iColumniColSize;
//                    return System.Convert.ToString(PreviousColumnX);
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.GetX", e);
//                return string.Empty;
//            }
//        }

//        private string GetY(object DBFieldTopEx, object DBFieldColumnHeight)
//        {
//            //	�����б���Ĭ����ʼ��Ϊ1��Ĭ�ϸ߶�Ϊ17����
//            int iTopEx = 1;
//            int iColumnTitleHeight = 17;

//            if (DBFieldTopEx != System.DBNull.Value
//                && (int)DBFieldTopEx != 0)
//            {
//                iTopEx = (int)DBFieldTopEx;
//            }
//            if (DBFieldColumnHeight != System.DBNull.Value
//                && (int)DBFieldColumnHeight != 0)
//            {
//                iColumnTitleHeight = (int)DBFieldColumnHeight / 15;
//            }
//            return System.Convert.ToString(ReportHeadBottom + (iTopEx - 1) * iColumnTitleHeight);
//        }

//        private string GetWidth(object DBFieldWidth, object DBFieldColumniColSize)
//        {
//            //	�����б���Ĭ�Ͽ�Ȳ���Ϊ1��Ĭ�ϵ�λ����Ϊ67����
//            int iLevel = 1;
//            int iColumnTitleWidth = 67;

//            if (DBFieldColumniColSize != System.DBNull.Value
//                && (int)DBFieldColumniColSize != 0)
//            {
//                iColumnTitleWidth = (int)DBFieldColumniColSize / 15;
//            }
//            if (DBFieldWidth != System.DBNull.Value
//                && (int)DBFieldWidth != 0)
//            {
//                iLevel = (int)DBFieldWidth;
//            }
//            return System.Convert.ToString(iLevel * iColumnTitleWidth);
//        }

//        private string GetHeight(object DBFieldHeight, object DBFieldColumnHeight)
//        {
//            //	��Щû�б�����Ϣ������Ӧ��û�и߶Ȳ�����Ϣ,��ʱ����������Ϊ�����������
//            //	�����б���Ĭ�ϸ߶Ȳ���Ϊ1��Ĭ�ϵ�λ��߶�Ϊ17����
//            int iLevel = 1;
//            int iColumnTitleHeight = 17;

//            if (DBFieldColumnHeight != System.DBNull.Value
//                && (int)DBFieldColumnHeight != 0)
//            {
//                iColumnTitleHeight = (int)DBFieldColumnHeight / 15;
//            }
//            if (DBFieldHeight != System.DBNull.Value
//                && (int)DBFieldHeight != 0)
//            {
//                iLevel = (int)DBFieldHeight;
//            }
//            if (iLevel > iMaxColumnLevelCount)
//                iLevel = iMaxColumnLevelCount;
//            return System.Convert.ToString(iLevel * iColumnTitleHeight);
//        }

//        private string GetCaption(object DBFieldNameForeign, object DBFieldName)
//        {
//            if (DBFieldNameForeign.ToString().Trim() == "")
//            {
//                return DBFieldName.ToString().Trim();
//            }
//            else
//            {
//                return DBFieldNameForeign.ToString().Trim();
//            }
//        }

//        private string GetVisible(object DBFieldVisible)
//        {
//            if (DBFieldVisible != System.DBNull.Value
//                && (int)DBFieldVisible != 0)
//            {
//                return "true";
//            }

//            return "false";
//        }

//        //public enum PrecisionType
//        //{
//        //    None			=0,
//        //    Money			=1,				//���
//        //    Weight		=2,				//����
//        //    Volume		=3,				//���
//        //    Rate			=4,				//����
//        //    Quantity		=5,				//����
//        //    PieceNum		=6,				//����
//        //    ExchangeRate	=7,				//������
//        //    TaxRate		=8,				//˰��
//        //    BillPrice		=9,				//��Ʊ����
//        //    CostMoney		=10,			//�ɱ����
//        //    CostQuantity	=11				//�ɱ�����
//        //}

//        private void WritePrecisionOrFormatStringAttribute(object DBFieldFormatEx)
//        {
//            if (DBFieldFormatEx != System.DBNull.Value)
//            {
//                int iType;
//                string sType = DBFieldFormatEx.ToString().Trim().ToUpper();
//                switch (sType.ToUpper())
//                {
//                    case "MONEY":
//                        iType = 9;
//                        break;
//                    case "QUANTITY":
//                        iType = 5;
//                        break;
//                    case "NUM":
//                        iType = 6;
//                        break;
//                    case "PERCENT":
//                        iType = -1;
//                        sType = "P";
//                        break;
//                    case "PERCENTGROUP":
//                        iType = -1;
//                        sType = "P";
//                        break;
//                    default:
//                        iType = -1;
//                        break;
//                }
//                if (iType != -1)
//                    FormatXmlWriteAttribute("Precision", iType.ToString());
//                else if (sType != "")
//                {
//                    sType = sType.Replace("yyyy-mm-dd", "yyyy-MM-dd");
//                    sType = sType.Replace("YYYY-MM-DD", "yyyy-MM-dd");
//                    FormatXmlWriteAttribute("FormatString", sType);
//                }
//            }
//        }

//        private void WriteCaptionAlignAttribute(object DBFieldiAlignStyle)
//        {
//            //			�����ݵĶ����Ӧ��ϵ����
//            //			enum stdalign                        
//            //				alignnormal = 0
//            //				alignleft	= 1
//            //				aligncenter	= 2
//            //				alignright	= 3
//            //			end enum

//            //.Net�еĶ���ö��
//            //BottomCenter		= 0x200
//            //BottomLeft		= 0x100
//            //BottomRight		= 0x400
//            //MiddleCenter		= 0x20
//            //MiddleLeft		= 0x10
//            //MiddleRight		= 0x40
//            //TopCenter			= 2
//            //TopLeft			= 1
//            //TopRight			= 4

//            int iAlign;
//            if (DBFieldiAlignStyle != null
//                && DBFieldiAlignStyle != System.DBNull.Value)
//            {
//                iAlign = (int)DBFieldiAlignStyle;
//            }
//            else
//                iAlign = 0;

//            switch (iAlign)
//            {
//                case 0:
//                case 3:
//                    iAlign = 0x40;
//                    break;
//                case 1:
//                    iAlign = 0x10;
//                    break;
//                case 2:
//                    iAlign = 0x20;
//                    break;
//                default:
//                    iAlign = 0x20;
//                    break;
//            }
//            FormatXmlWriteAttribute("CaptionAlign", iAlign.ToString());
//        }

//        // �������޹�
//        private void WriteUserDefineItemAttributeIfNeeded(SqlDataReader Reader)
//        {
//            if (LocaleId.ToLower() != "zh-cn")
//                return;

//            string columnDataSourceExpression = SqlHelper.GetStringFrom(Reader["Column_Expression"]);
//            string userDefineCaption = GetUserDefineCaption(columnDataSourceExpression);
//            if (userDefineCaption != string.Empty)
//            {
//                FormatXmlWriteAttribute("EventType", "0");
//                FormatXmlWriteStartElement("PrepaintEvent");
//                FormatDatasXmlWriter.WriteCData(userDefineCaption);
//                FormatXmlWriteEndElement();
//            }
//        }

//        private string GetUserDefineCaption(string columnDataSourceExpression)
//        {
//            int index = GetUserDefineIndex(columnDataSourceExpression);
//            if (index == -1)
//                return string.Empty;

//            string fieldName = columnDataSourceExpression.ToUpper();
//            string userDefineKey = string.Empty;

//            // �ж��Ƿ��ǹ�Ӧ��
//            if (fieldName.IndexOf("CVENDEFINE") != -1)
//                userDefineKey = "@��Ӧ��.�Զ�����" + index.ToString();

//            // �ж��Ƿ��Ǵ���Զ�����
//            else if (fieldName.IndexOf("CINVDEFINE") != -1)
//                userDefineKey = "@���.�Զ�����" + index.ToString();

//            // �ж��Ƿ��Ǵ��������
//            else if (fieldName.IndexOf("CFREE") != -1)
//                userDefineKey = "@���.������" + index.ToString();

//            // �ж��Ƿ�����ϵ��
//            else if (fieldName.IndexOf("CCONDEFINE") != -1)
//                userDefineKey = "@��ϵ��.�Զ�����" + index.ToString();

//            // �ж��Ƿ������к�����
//            else if (fieldName.IndexOf("CSNDEFINE") != -1)
//                userDefineKey = "@���к�.����" + index.ToString();

//            // �ж��Ƿ��ǵ���ͷ�򵥾���
//            else if (fieldName.IndexOf("DDEFINE_") != -1
//                || fieldName.IndexOf("DEFINE_") != -1
//                || fieldName.IndexOf("CDEFINE") != -1)
//            {
//                if (index > 21)
//                {
//                    index -= 21;
//                    userDefineKey = "@������.�Զ�����" + index.ToString();
//                }
//                else
//                    userDefineKey = "@����ͷ.�Զ�����" + index.ToString();
//            }

//            // �ж��Ƿ����Զ�����
//            else if (fieldName.IndexOf("INVDEFINE_") != -1
//                || fieldName.IndexOf("DINVDEFINE_") != -1)
//            {
//                userDefineKey = "@���.�Զ�����" + index.ToString();
//            }

//            // �ж��Ƿ���������
//            else if (fieldName.IndexOf("INVFREE_") != -1
//                || fieldName.IndexOf("DINVFREE_") != -1)
//            {
//                userDefineKey = "@���.�Զ�����" + index.ToString();
//            }

//            if (userDefineKey != string.Empty)
//                return string.Format("cell.Caption=datahelper.CusDefineInfo(\"{0}\");", userDefineKey);

//            return string.Empty;
//        }

//        private int GetUserDefineIndex(string expression)
//        {
//            try
//            {
//                if (expression.Length >= 2
//                    && char.IsNumber(expression, expression.Length - 1)
//                    && char.IsNumber(expression, expression.Length - 2))
//                {
//                    return System.Convert.ToInt32(expression.Substring(expression.Length - 2));
//                }
//                else if (expression.Length >= 1
//                    && char.IsNumber(expression, expression.Length - 1))
//                {
//                    return System.Convert.ToInt32(expression.Substring(expression.Length - 1));
//                }
//            }
//            catch (System.Exception e)
//            {
//                throw (new System.Exception(
//                    string.Format("������������GetUserDefineIndex(),����ldq��ϵ.\r\n����ؼ��ַ�����\r\n{0}\r\n����ԭ��\r\n{1}",
//                    expression,
//                    e.Message)));
//            }

//            return -1;
//        }

//        private void WriteDataSourceAttribute(
//            System.Data.SqlClient.SqlDataReader Reader,
//            string DataType,
//            string Caption,
//            string cellName)
//        {
//            string sColumn_Name = GetConvertedExpressionString(Reader["Column_CONditiON"]);

//            //	������Ƿ���ԺϼƵ���Ϣ
//            string name = SqlHelper.GetStringFrom(Reader["Column_Name"]);

//            if (_MendWhenUpgrading.IsSummaryColumn(name))
//            {
//                FormatXmlWriteAttribute("bSummary", "true");
//                FormatXmlWriteAttribute("Operator", _MendWhenUpgrading.GetOperatorType(name));
//            }
//            else
//                FormatXmlWriteAttribute("bSummary", "false");

//            if (DataType == "GridDecimalAlgorithmColumn")
//            {
//                FormatXmlWriteStartElement("Algorithm");
//                FormatXmlWriteAttribute("Language", "1");

//                // �������޹ص���Ϣ�����������������
//                // ����������������ִ��
//                if (LocaleId.ToLower() == "zh-cn")
//                    FormatDatasXmlWriter.WriteCData(sColumn_Name);

//                FormatXmlWriteEndElement();
//            }
//            else if (DataType == "GridCalculateColumn")
//                FormatXmlWriteAttribute("Expression", sColumn_Name);
//            else
//            {
//                sColumn_Name = Reader["Column_Expression"].ToString().Trim();

//                if (LocaleId.ToUpper() == "ZH-CN"
//                    && ColumnNameArrayList[sColumn_Name] == null)
//                {
//                    ColumnNameArrayList.Add(sColumn_Name, cellName);
//                }

//                FormatXmlWriteAttribute("DataSource", sColumn_Name);
//            }

//            //	��������Դ���е������ݴ��������Ա����Ժ�д��SQLQueristConfig
//            //	��Ϊ��GridDecimalAlgorithmColumn����ҲҪ��Ϊ����Դ,���ܶิ�Ӽ����е�Expression
//            //	�ֶζ�Ϊ��,��ʱȡName�ֶ�;����ͬ�����¸��Ӽ����е�Nameֵ�᲻һ��,����ֻ����������
//            //	���Ӽ����е���Ϣ
//            string dataSourceName = SqlHelper.GetStringFrom(Reader["Column_Expression"]);
//            if (dataSourceName == string.Empty && this.LocaleId.ToUpper() == "ZH-CN")
//                dataSourceName = SqlHelper.GetStringFrom(Reader["Column_Name"]);

//            string dataSourceDecription = SqlHelper.GetStringFrom(Reader["Title_Name"], Caption);
//            if (dataSourceName != string.Empty)
//            {
//                SaveColumnInfoForQueristConfig(
//                    dataSourceName,
//                    DataType,
//                    dataSourceDecription);
//            }
//        }

//        private void SaveColumnInfoForQueristConfig(
//            string name,
//            string dataType,
//            string description)
//        {
//            string sDataType = dataType;
//            string sDescription = description;
//            switch (sDataType)
//            {
//                case "GridDecimal":
//                case "GridDecimalAlgorithmColumn":
//                case "GridCalculateColumn":
//                    sDataType = "Decimal";
//                    break;
//                case "GridLabel":
//                    sDataType = "String";
//                    break;
//                case "GridDateTime":
//                    sDataType = "DateTime";
//                    break;
//                //case "GridBoolean":
//                //    sDataType = "Boolean";
//                //    break;
//                default:
//                    sDataType = "";
//                    break;
//            }
//            if (sDataType != "")
//            {
//                ColumnInfoForSQLQueristConfig[0] = name;
//                ColumnInfoForSQLQueristConfig[1] = sDataType;
//                ColumnInfoForSQLQueristConfig[2] = sDescription;

//                //	���ݵ�ǰ���е�Title_TopExȷ��GetParentColumnCation��Ҫ�����ĸ�����
//                if (reader["Title_TopEx"] != System.DBNull.Value
//                    && (int)reader["Title_TopEx"] != 0)
//                    CurrentColumnLevelTop = (int)reader["Title_TopEx"];
//            }
//        }

//        //	�����ǰ�д��ڸ��У���ӵ�ǰ��SuperLabelDataArrayList��ƴ�ճ��������ƴ���
//        //	�������ƴ��ĸ�ʽ�磺A.B
//        private string GetParentColumnCation()
//        {
//            int iPreTopEx = System.Int32.MinValue;		//	��һ�㸸�е�Y����
//            int iCurTopEx;									//	Ŀǰ����һ����ӽ��е�Y����
//            string[] sSuperLabelDatas;
//            string sParentColumnCation = "";
//            string sTemp = "";

//            if (SuperLabelDataArrayList.Count > 0)
//            {
//                for (int i = 0; i < CurrentColumnLevelTop - 1; i++)
//                {
//                    iCurTopEx = System.Int32.MaxValue;
//                    for (int j = 0; j < SuperLabelDataArrayList.Count; j++)
//                    {
//                        sSuperLabelDatas = (string[])SuperLabelDataArrayList[j];
//                        int iTemp = System.Convert.ToInt32(sSuperLabelDatas[3]);
//                        if (iTemp > iPreTopEx && iTemp < iCurTopEx)
//                        {
//                            iCurTopEx = iTemp;
//                            sTemp = sSuperLabelDatas[6];
//                        }
//                    }
//                    iPreTopEx = iCurTopEx;
//                    sParentColumnCation = sParentColumnCation + sTemp + ".";
//                }

//                // ��������ΪһЩ�����������д��´˴������쳣
//                // ���������һЩ��¼��ModeEx=0ʱ�������OrderEx
//                // ���Ƕ������������һЩ��������û��Ҳ�����쳣��δ�ҵ�ԭ��
//                if (sParentColumnCation.Length - 1 > 0)
//                    sParentColumnCation = sParentColumnCation.Remove(sParentColumnCation.Length - 1, 1);
//            }

//            return sParentColumnCation;
//        }

//        private string GetConvertedExpressionString(object DBFieldConditionOrExpression)
//        {
//            string sReturn = DBFieldConditionOrExpression.ToString().Trim();
//            _MendWhenUpgrading.ReplaceNameDescription(ref sReturn);
//            if (sReturn.IndexOf("PREV_", StringComparison.CurrentCultureIgnoreCase) != -1)
//            {
//                sReturn = GetExpressionAmongTowColumn(sReturn);
//            }
//            else if (sReturn.IndexOf("IIF(", StringComparison.CurrentCultureIgnoreCase) != -1)
//            {
//                sReturn = GetScriptFromIIF(sReturn);
//            }
//            else
//            {
//                sReturn = sReturn.Replace("[", "");
//                sReturn = sReturn.Replace("]", "");

//                string sColumnKey = SqlHelper.GetStringFrom(reader["Column_Name"]);
//                if (_MendWhenUpgrading.IsPercentColumn(sColumnKey))
//                    sReturn = _MendWhenUpgrading.GetPercentExpression(sReturn);
//                else if (_MendWhenUpgrading.IsPercentGroupColumn(sColumnKey))
//                    sReturn = _MendWhenUpgrading.GetPercentGroupExpression(sReturn);
//            }

//            return sReturn;
//        }

//        //	ȡ�ÿ��м���ʽ
//        private string GetExpressionAmongTowColumn(string expression)
//        {
//            string value = string.Empty;
//            if (expression.IndexOf("[") != -1)
//            {
//                if (this.LocaleId.ToUpper() == "zh-CN")
//                    AddDataSourceKey(GetDataSourceKeyWords(expression));

//                string StringWhenIndexZero = GetReturnStringWhenIndexZero(expression);
//                string StringWhenIndexNotZero = GetReturnStringWhenIndexNotZero(expression);
//                string currentValue = "current." + reader["Column_Name"].ToString();
//                value = string.Format(
//                    "if( currentindex == 0 )\r\n{0}\r\n\t{2} = {3};\r\n{1}\r\nelse\r\n{0}\r\n\t{2} = {4};\r\n{1}\r\n\r\nreturn {2};",
//                    "{",
//                    "}",
//                    currentValue,
//                    StringWhenIndexZero,
//                    StringWhenIndexNotZero);

//                value = value.Replace("�ڳ�ֵLdqAddForRepaceLater", currentValue);
//            }
//            //	���ܴ�����������������ʽ�к��С�PREV_��������ȴ�����С�[����]��
//            //	��Ԥ��������Ϊ��������Ҫ�������������λ��

//            return value;
//        }

//        // ����Ϊ�ű���ʱ����Ҫ�������ű��к��е�����Դ
//        // �Է�ֹ�ű�����Щ����Դ�ؼ��ֲ���������Դ
//        // �硰������Ʒ��̨�ˡ����С�JYQuantity��
//        private void AddDataSourceKey(string[] keys)
//        {
//            if (keys != null)
//            {
//                foreach (string s in keys)
//                {
//                    if (s != string.Empty)
//                        AddSourceColumn(s, "Decimal");
//                }
//            }
//        }

//        private string[] GetDataSourceKeyWords(string expression)
//        {
//            string temp = expression.Replace("PREV_", string.Empty);
//            return temp.Split(new char[] { '[', ']', '(', ')', '+', '-', '*', '/' });
//        }

//        // ת��IIF���ʽΪC#�ű�
//        private string GetScriptFromIIF(string expression)
//        {
//            string value = expression;
//            value = value.Replace("IIF(", "");
//            value = value.Replace("iif(", "");
//            value = value.Replace(")", "");
//            value = value.Replace("[", "");
//            value = value.Replace("]", "");
//            value = value.Replace(" OR ", " || ");
//            value = value.Replace(" or ", " || ");
//            value = value.Replace(" AND ", " && ");
//            value = value.Replace(" and ", " && ");
//            value = value.Replace("<>", "!=");

//            // �ȴ��溬��"="�������
//            value = value.Replace("<=", "<<");
//            value = value.Replace(">=", ">>");
//            value = value.Replace("!=", "!!");

//            value = value.Replace("=", "==");

//            // ���Ѿ����溬��"="�������������
//            value = value.Replace("<<", "<=");
//            value = value.Replace(">>", ">=");
//            value = value.Replace("!!", "!=");

//            string[] keys = value.Split(',');
//            if (keys == null || keys.Length != 3)
//                throw new Exception(string.Format("��ת�����ʽ{0}ʱ���ִ���", expression));

//            for (int i = 0; i < keys.Length; i++)
//                if (!IsStringNumber(keys[i].Trim()))
//                    keys[i] = "current." + keys[i].Trim();

//            return string.Format(
//                "return {0} ? {1} : {2};",
//                keys[0].Trim(),
//                keys[1].Trim(),
//                keys[2].Trim());
//        }

//        private bool IsStringNumber(string s)
//        {
//            foreach (char c in s)
//                if (!char.IsDigit(c))
//                    return false;

//            return true;
//        }

//        private string GetReturnStringWhenIndexZero(string SourceString)
//        {
//            int BeginIndex = SourceString.IndexOf("[PREV_");

//            while (BeginIndex != -1)
//            {
//                int EndIndex = SourceString.IndexOf(']', BeginIndex);
//                string SubString = SourceString.Substring(BeginIndex, EndIndex - BeginIndex + 1);
//                SourceString = SourceString.Replace(SubString, "�ڳ�ֵLdqAddForRepaceLater");
//                BeginIndex = SourceString.IndexOf("[PREV_");
//            }

//            SourceString = SourceString.Replace("[", "current.");
//            SourceString = SourceString.Replace("]", "");

//            return SourceString;
//        }

//        private string GetReturnStringWhenIndexNotZero(string SourceString)
//        {
//            SourceString = SourceString.Replace("[PREV_", "rows&bracketLcurrentindex-1&bracketR.");
//            SourceString = SourceString.Replace("[", "current.");
//            SourceString = SourceString.Replace("]", "");
//            SourceString = SourceString.Replace("&bracketL", "[");
//            SourceString = SourceString.Replace("&bracketR", "]");

//            return SourceString;
//        }

//        private void WriterLabelsNotInBodyRegion(string SectionName)
//        {
//            try
//            {
//                bool bHasUFSoftString;			//	�Ƿ����ַ�������������������������ӱ�ǩ���
//                cmd.CommandText = UpgradeSqlProducer.GetSqlLabelsInforNotInDetailSection(
//                    ReportID,
//                    LocaleId,
//                    SectionName);

//                reader = cmd.ExecuteReader();
//                while (reader.Read())
//                {
//                    string sDrawingIndex = GetDrawingIndex();
//                    bHasUFSoftString = false;

//                    FormatXmlWriteStartElement("Control");
//                    FormatXmlWriteAttribute("Name", sDrawingIndex);
//                    WriteTypeCaptionFormatStringAttributes(ref bHasUFSoftString);
//                    WriteCaptionAlignAttribute(reader["iAlignStyle"]);
//                    WriteAboutPositionAttributes(bHasUFSoftString);
//                    WriteAboutFontAttributes(reader["Condition"]);
//                    FormatXmlWriteAttribute("Visible", "true");	//	��������ı�ǩ����Ϊȫ���ɼ�
//                    FormatXmlWriteAttribute("KeepPos", "true");

//                    // ����ϸ���ڵı�ǩ����Ϊ�ޱ߿�
//                    FormatXmlWriteAttribute("BorderLeft", "false");
//                    FormatXmlWriteAttribute("BorderTop", "false");
//                    FormatXmlWriteAttribute("BorderRight", "false");
//                    FormatXmlWriteAttribute("BorderBottom", "false");

//                    FormatXmlWriteEndElement();
//                }
//                reader.Close();
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriterLabelsNotInBodyRegion", e);
//            }
//        }

//        private string GetDrawingIndex()
//        {
//            return "Drawing" + GetIDField("26");
//        }

//        //	���ݱ�rpt_flddef_base���ֶ�Note�����硰��ʽ;YYYY-MM-DD;ϵͳ���ù�ʽ��
//        //	����ʾLabel�����͵���Ϣ,���ֶ�Expression���ʾ��Ӧ���͵�ֵ

//        //  ���ö��

//        //		public enum FormulaType
//        //		{
//        //			Common,   //0
//        //			Filter,   //1
//        //			Business,
//        //			UserDefine
//        //		}

//        private void WriteTypeCaptionFormatStringAttributes(ref bool bHasUFSoftString)
//        {
//            try
//            {
//                string[] sTemp = reader["Note"].ToString().Split(';');
//                string sExpression = reader["Expression"].ToString().Trim();

//                //	���ı��ת��ΪӢ�ı�㣬��ֹLabel���ַ���
//                sExpression = sExpression.Replace("��", ":");
//                sExpression = sExpression.Replace("��", ",");

//                string sType = "";
//                string sFormatString = "";
//                string FormulaTypeSource = "";
//                if (sTemp.Length >= 3)
//                {
//                    sType = sTemp[0].Trim();
//                    sFormatString = sTemp[1].Trim();
//                    FormulaTypeSource = sTemp[2].Trim();

//                    // �����ݿ��д����"YYYY-MM-DD"��Ϊ"yyyy-MM-dd"
//                    sFormatString = sFormatString.Replace("yyyy-mm-dd", "yyyy-MM-dd");
//                    sFormatString = sFormatString.Replace("YYYY-MM-DD", "yyyy-MM-dd");
//                }

//                //	Expression��FormulaType������Ϣ
//                string iFormulaType = "";

//                if (sExpression.IndexOf("�����������") != -1)
//                    bHasUFSoftString = true;

//                switch (sType)
//                {
//                    case "�ı�":
//                        sType = "CommonLabel";

//                        // Expression�ֶ�Ϊ��ʱ��ȡName�ֶ�ΪCaption��ֵ
//                        if (sExpression != string.Empty)
//                            FormatXmlWriteAttribute("Caption", sExpression);
//                        else
//                            FormatXmlWriteAttribute("Caption", reader["Name"].ToString().Trim());
//                        break;
//                    case "SQL��ѯ":
//                        sType = "Expression";
//                        iFormulaType = "3";
//                        break;
//                    case "��ʽ":

//                        //	��ϵͳ���ù�ʽ��GetReportNameת��Ϊ�ı�
//                        if (sExpression == "GetReportName()")
//                        {
//                            sType = "CommonLabel";
//                            FormatXmlWriteAttribute("Caption", ReportTitle);
//                        }
//                        else
//                        {
//                            sType = "Expression";

//                            //	��ϵͳ���ù�ʽ��GetFilterValue��FormulaTypΪ��Filter��ö��
//                            //	���ࡰϵͳ���ù�ʽ���ǡ�Common��ö��
//                            //	���û��Զ��幫ʽ���ǡ�UserDefine��ö��
//                            //	�����ǡ�Business��ö��
//                            if (sExpression.IndexOf("GetFilterValue") != -1)
//                            {
//                                iFormulaType = "1";
//                                sExpression = ConvertToNewFilterFuction(sExpression);
//                            }
//                            else
//                            {
//                                if (FormulaTypeSource == "ϵͳ���ù�ʽ")
//                                    iFormulaType = "0";
//                                else if (FormulaTypeSource == "�Զ��幫ʽ")
//                                    iFormulaType = "3";
//                                else
//                                    iFormulaType = "2";
//                            }
//                        }
//                        break;
//                    case "���ʽ":
//                        sType = "Expression";
//                        break;
//                    default:
//                        sType = "CommonLabel";
//                        break;
//                }

//                FormatXmlWriteAttribute("Type", sType);
//                if (sFormatString != "")
//                    FormatXmlWriteAttribute("FormatString", sFormatString);
//                if (sType == "Expression")
//                {
//                    sExpression = sExpression.Replace("[", "");
//                    sExpression = sExpression.Replace("]", "");

//                    if (sExpression != "")
//                    {
//                        FormatXmlWriteAttribute("Caption", sExpression);
//                        FormatXmlWriteAttribute("Formula", sExpression);
//                    }
//                    if (iFormulaType != "")
//                        FormatXmlWriteAttribute("FormulaType", iFormulaType);
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteTypeCaptionFormatStringAttributes", e);
//            }
//        }

//        private string ConvertToNewFilterFuction(string sExpression)
//        {
//            string sReturnVal = string.Empty;

//            //	GetFilterValue(*,0,0)ת��ΪGetValue1("*")
//            //	GetFilterValue(*,0,1)ת��ΪGetValue2("*")
//            //	GetFilterValue(*,2,0)ת��ΪGetName1("*")
//            //	GetFilterValue(*,2,1)ת��ΪGetName2("*")
//            string[] sTemp1 = sExpression.Split('(');
//            if (sTemp1.Length >= 2)
//            {
//                string[] sTemp2 = sTemp1[1].Split(',');
//                if (sTemp2.Length >= 3)
//                    sReturnVal = GetFilterFunction(sTemp2);
//            }
//            return sReturnVal;
//        }

//        //	GetFilterValue(*,0,0)ת��ΪGetValue1("*")
//        //	GetFilterValue(*,0,1)ת��ΪGetValue2("*")
//        //	GetFilterValue(*,2,0)ת��ΪGetName1("*")
//        //	GetFilterValue(*,2,1)ת��ΪGetName2("*")
//        private string GetFilterFunction(string[] sTemp2)
//        {
//            // ��ֹ����"1  )"�����
//            string functionIndex = "1";
//            bool bTemp1 = (sTemp2[2] == "1)");
//            bool bTemp2 = (sTemp2[2].IndexOf("1") != -1);
//            bool bTemp3 = (sTemp2[2].IndexOf(")") != -1);
//            if (bTemp1 || (bTemp2 && bTemp3))
//                functionIndex = "2";
//            else
//                functionIndex = "1";

//            string functionName = string.Empty;
//            if (sTemp2[1].Trim() == "2")
//                functionName = "GetName";
//            else
//                functionName = "GetValue";

//            // Expression��Formula���Խ������ĵ�Ϊ׼
//            // ��Captionȡ��������صı�ʾ��ʽ
//            string filterValueString = string.Empty;
//            if (this.LocaleId.ToUpper() != "ZH-CN")
//                filterValueString = sTemp2[0];
//            else
//                filterValueString = GetFilterValueString(sTemp2[0]);

//            return string.Format("{0}{1}(\"{2}\")", functionName, functionIndex, filterValueString);
//        }

//        private string GetFilterValueString(string note)
//        {
//            try
//            {
//                note = note.Replace("[", string.Empty);
//                note = note.Replace("]", string.Empty);
//                string sql = string.Format(
//                    "select Name from Rpt_FltDEF_Base where ID=N'{0}' and Note=N'{1}' and localeid=N'zh-CN'",
//                    _BaseTableReportID,
//                    note);
//                DataSet ds = SqlHelper.ExecuteDataSet(this.UfDataConnString, sql);
//                DataRow dr = SqlHelper.GetDataRowFrom(0, ds);
//                if (dr != null)
//                    return SqlHelper.GetStringFrom(dr[0]);
//                return string.Empty;
//            }
//            catch
//            {
//                return string.Empty;
//            }
//        }

//        private void WriteAboutPositionAttributes(bool bHasUFSoftString)
//        {
//            try
//            {
//                if (bHasUFSoftString)
//                {
//                    FormatXmlWriteAttribute("X", System.Convert.ToString((int)reader["LeftEx"] / 15 - 6));
//                    FormatXmlWriteAttribute("Y", GetNormalLabelY(reader["TopEx"]));
//                    FormatXmlWriteAttribute("Width", System.Convert.ToString((int)reader["Width"] / 15 + 6));
//                }
//                else
//                {
//                    FormatXmlWriteAttribute("X", ConvertVBTiwpToPixel(reader["LeftEx"]));
//                    FormatXmlWriteAttribute("Y", GetNormalLabelY(reader["TopEx"]));
//                    FormatXmlWriteAttribute("Width", ConvertVBTiwpToPixel(reader["Width"]));
//                }
//                FormatXmlWriteAttribute("Height", ConvertVBTiwpToPixel(reader["Height"]));
//                FormatXmlWriteAttribute("bAutoHeight", "true");
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteAboutPositionAttributes", e);
//            }
//        }

//        private string GetNormalLabelY(object DBFieldTopEx)
//        {
//            try
//            {
//                return System.Convert.ToString(ReportPageTitleBottom + (int)DBFieldTopEx / 15);
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.GetNormalLabelY", e);
//                return string.Empty;
//            }
//        }

//        private void WriteAboutFontAttributes(object DBFieldCondition)
//        {
//            string[] sCondition = DBFieldCondition.ToString().Split(',');

//            //	�ֶΡ�Condition�����������ơ�True,134,False,����,15.75,False,False,700��
//            //	������ֵ�ŶԸ�Label����������Ϣ
//            if (sCondition.Length > 7)
//            {
//                FormatXmlWriteAttribute("FontBold", sCondition[0].Trim());
//                FormatXmlWriteAttribute("FontGdiCharSet", sCondition[1].Trim());
//                FormatXmlWriteAttribute("FontItalic", sCondition[2].Trim());
//                FormatXmlWriteAttribute("FontName", sCondition[3].Trim());
//                FormatXmlWriteAttribute("FontSize", sCondition[4].Trim());
//                FormatXmlWriteAttribute("FontStrikethout", sCondition[5].Trim());
//                FormatXmlWriteAttribute("FontUnderline", sCondition[6].Trim());
//            }
//        }

//        private void WriteGroupSchemas()
//        {
//            if (ReportID == _InValidatedID)
//                return;

//            string sColumnName;
//            bool RunOnce = true;

//            if (_SchemaItemNameArrayList == null)
//            {
//                _SchemaItemNameArrayList = new System.Collections.ArrayList();
//                _SchemaItemLangArrayList = new System.Collections.ArrayList();
//            }

//            try
//            {
//                cmd.CommandText = UpgradeSqlProducer.GetSqlGroupSchemasItems(ReportID);
//                reader = cmd.ExecuteReader();
//                while (reader.Read())
//                {
//                    if (RunOnce)
//                    {
//                        ColumnLangInfoNode LangInfoNode = new ColumnLangInfoNode();
//                        LangInfoNode.LocaleId = LocaleId;
//                        LangInfoNode.InfoValue = GetSchemeName();
//                        _SchemaItemLangArrayList.Add(LangInfoNode);
//                        RunOnce = false;
//                    }

//                    sColumnName = reader["Expression"].ToString().Trim();
//                    if (ColumnNameArrayList != null
//                        && ColumnNameArrayList[sColumnName] != null)
//                    {
//                        // �Զ������ԣ�SchemaItem��д��һ��
//                        if (LocaleId.ToLower().Trim() == "zh-cn")
//                            _SchemaItemNameArrayList.Add(ColumnNameArrayList[sColumnName].ToString());
//                    }
//                }
//            }
//            catch (System.Exception e)
//            {
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.WriteGroupSchemas", e);
//            }
//        }

//        private string GetGroupSchemasXml()
//        {
//            if (_SchemaItemNameArrayList == null || _SchemaItemNameArrayList.Count <= 0)
//                return string.Empty;

//            System.IO.MemoryStream GroupSchemasXmlMS = new System.IO.MemoryStream();
//            //			System.IO.FileStream GroupSchemasXmlMS	= new System.IO.FileStream( "F:\\ldq.xml" , System.IO.FileMode.OpenOrCreate );
//            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(GroupSchemasXmlMS, null);
//            writer.Formatting = System.Xml.Formatting.Indented;

//            WriteStartGroupSchemas(writer);
//            WriteGroupSchemasColumn(writer);
//            WriteEndGroupSchemas(writer);

//            writer.Flush();
//            string tempString = GetStringFromSream(GroupSchemasXmlMS);
//            writer.Close();
//            return tempString;
//        }

//        private void SetLevelExpandXml()
//        {
//            if (LocaleId.ToUpper() == "ZH-CN")
//            {
//                ViewLevelExpandSrv vles = new ViewLevelExpandSrv(ReportID, UfDataConnString);
//                _LevelExpand = vles.GetLevelExpand();
//            }
//        }

//        private void WriteStartGroupSchemas(System.Xml.XmlTextWriter writer)
//        {
//            string guid = System.Guid.NewGuid().ToString();
//            writer.WriteStartElement("GroupSchemas");
//            writer.WriteStartElement("GroupSchema");
//            writer.WriteAttributeString("ID", guid);
//            writer.WriteAttributeString("bDefault", "true");
//        }

//        private void WriteGroupSchemasColumn(System.Xml.XmlTextWriter writer)
//        {
//            if (_SchemaItemLangArrayList == null || _SchemaItemNameArrayList == null)
//                return;

//            writer.WriteStartElement("MulitLangInfo");
//            for (int i = 0; i < _SchemaItemLangArrayList.Count; i++)
//            {
//                ColumnLangInfoNode LangInfo = _SchemaItemLangArrayList[i] as ColumnLangInfoNode;
//                if (LangInfo != null)
//                {
//                    writer.WriteStartElement("Lang");
//                    writer.WriteAttributeString("id", LangInfo.LocaleId);
//                    writer.WriteAttributeString("Name", LangInfo.InfoValue);
//                    writer.WriteEndElement();
//                }
//            }
//            writer.WriteEndElement();

//            for (int i = 0; i < _SchemaItemNameArrayList.Count; i++)
//            {
//                string sColumnName = _SchemaItemNameArrayList[i] as string;
//                if (sColumnName != string.Empty)
//                {
//                    writer.WriteStartElement("SchemaItem");
//                    writer.WriteStartElement("Item");
//                    writer.WriteAttributeString("Name", sColumnName);
//                    writer.WriteEndElement();
//                    writer.WriteEndElement();
//                }
//            }
//        }

//        private void WriteEndGroupSchemas(System.Xml.XmlTextWriter writer)
//        {
//            writer.WriteEndElement();
//            writer.WriteEndElement();
//        }

//        //private bool IsColumnNameFoundInColumnNameArrayList( ref string sColumnName )
//        //{
//        //    for( int i = 0 ; i < ColumnNameArrayList.Count ; i++ )
//        //    {
//        //        if( sColumnName == ColumnNameArrayList[i].ToString() )
//        //        {
//        //            sColumnName = "Column" + System.Convert.ToString( i+1 );
//        //            return true;
//        //        }
//        //    }
//        //    return false;
//        //}

//        private string GetSchemeName()
//        {
//            switch (LocaleId.ToLower())
//            {
//                case "zh-cn":
//                    return "����1";
//                case "zh-tw":
//                    return "����1";
//                case "en-us":
//                    return "Scheme1";
//                default:
//                    return "����1";
//            }
//        }

//        private void ExceptionHandler(string Source, System.Exception e)
//        {
//            //            try
//            //            {
//            //                Logger logger = Logger.GetLogger();
//            //                logger.Error( string.Format( "������������:�����ʶ=={0}", ReportID ));
//            //                logger.Error( e );
//            //                logger.Dispose();
//            //#if UpGradeTool
//            //                AppendExceptionInfor( Source , e );
//            //#endif
//            //            }
//            //            catch( System.Exception EHe )
//            //            {
//            //                throw new System.Exception( "IO Operations(StreamWriter) Exception In \"ExceptionHandler\":\n"+ EHe.Message );
//            //            }
//        }

//#if UpGradeTool
//        public int ExceptionCount = 0;
//        private string _ExceptionInfor = string.Empty;

//        public string ExceptionInfor
//        {
//            get
//            {
//                string temp = "�쳣������" + ExceptionCount.ToString() + "\r\n";
//                return temp + _ExceptionInfor;
//            }
//            set { _ExceptionInfor = value; }
//        }

//        private void AppendExceptionInfor(string Source, System.Exception e)
//        {
//            ExceptionCount++;
//            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

//            stringBuilder.Append("\r\n-------------------------------\r\n");
//            stringBuilder.Append("\r\nOccur Time: " + System.DateTime.Now.ToString());
//            stringBuilder.Append("\r\nReport Name: " + ReportName);
//            stringBuilder.Append("\r\nReport ID: " + ReportID);
//            stringBuilder.Append("\r\nError Source: " + Source);
//            stringBuilder.Append("\r\nError Message:\r\n\t" + e.Message);
//            stringBuilder.Append("\r\nError String:\r\n\t" + e.StackTrace);
//            stringBuilder.Append("\r\n\r\n-------------------------------");
//            stringBuilder.Append("\r\n");

//            _ExceptionInfor += stringBuilder.ToString();
//        }
//#endif
//        #endregion
//    }
//}
