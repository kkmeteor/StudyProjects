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

//        private const string _ViewNameZHCN = "原U861视图";
//        private const string _ViewNameZHTW = "原U861視圖";
//        private const string _ViewNameENUS = "View of U861";

//        // 语言列表
//        // 注意：第一个区域语言必须是"zh-CN"
//        private string[] LanguageIDList = new string[] { "zh-CN", "zh-TW", "en-US" };
//        private string[] ColumnInfoForSQLQueristConfig;	//	保存列信息，以后将会写入SQLQueristConfig
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
//        private string _BaseTableReportID;				//  另存的报表需要用源表的id来获取GetValue和GetName的信息
//        private string _LevelExpand;

//        private int CurrentColumnLevelTop;			//	纪录GetParentColumnCation中要搜索的父列数
//        private int iMaxColumnLevelCount;			//	最大的标题层数
//        private int ReportHeadBottom;				//	标题区填充的底端
//        private int ReportPageTitleBottom;			//	列标题区的底端
//        private int PreviousColumnX;				//	上一列的X
//        private int CurrentColumnX;					//	当前列的X
//        private int PreviousColumnLeftExIndex;		//	数据表里以LeftEx标识列的位置顺序，此标识符为上一列的位置

//        private XmlTextWriter FormatDatasXmlWriter;
//        private XmlTextWriter FormatDatasLangXmlWriter;

//        private ArrayList SuperLabelDataArrayList;		//	暂存SuperLabel的一些数据，等到可以确定其宽度的时候再一起写入
//        private Hashtable ColumnNameArrayList;			//	存储列名以供获取分组列时进行查询
//        private ArrayList _SchemaItemNameArrayList;
//        private ArrayList _SchemaItemLangArrayList;

//        private SortedList _DataSourceNoLangInfo;			// 由于数据引擎定义数据源的需要，把三个数据源的信息集中写入一个XML串
//        private SortedList _DataSourceLangInfo;

//        private SqlConnection cnn;
//        private SqlCommand cmd;
//        private SqlDataReader reader;

//        private UpgradeReport _upgradeReport;
//        private MendWhenUpgrading _MendWhenUpgrading;
//        private ReportDefinition _BaseReportDefinition;	//	另存的报表和自定义报表使用源表的相关信息	

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
//                    string.Format("升级错误:传入的报表标识\"{0}\"不正确.\r\n正确的报表标识的形式类似: **[__]******", reportID));
//                ExceptionHandler("U8.UAP.Services.UpGradeService.UpGradeEngine.CheckReportID", e);
//            }

//            SubID = reportID.Substring(0, 2);
//            ReportName = reportID.Substring(6, reportID.Length - 6);
//        }

//        private void PrepareDatas()
//        {
//            // 多次调用获取数据接口时需要清空上次升级信息
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
//                // 这个处理很重要，因为仅仅需要排除FormatDatasLangXmlWriter中
//                // 元素Algorithm对应的EndElement，其余的EndElement应该通过
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

//        // 当是另存的报表且其数据源不是Sql脚本时, 
//        // 将使用其父报表的自定义数据源,升级870
//        // 标准报表中861的视图时也不重新升级数据源
//        private bool IsUseExistDataSource()
//        {
//            if ((_BaseReportDefinition != null && QueristStringForDataEngine != "SQLQuerist")
//                || IsUpdateU861View())
//            {
//                return true;
//            }

//            return false;
//        }

//        // 如果当前拼接成的id已经存在于UAP_Report中,
//        // 则当前是在升级870标准报表中861的视图
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

//            //	发件人: 张劲涛 [mailto:zjt@ufida.com.cn] 
//            //	发送时间: 2006-05-11 10:31
//            //	收件人: 'Seaven（陈文海）'
//            //	主题: 新过滤ID
//            //	rsSql("systemID") & "[__]" & rsSql("name")
//            _upgradeReport.ID = string.Format("{0}[__]{1}", this.SubID, this.ReportName);
//            _upgradeReport.SubProjectID = this.SubID;
//            _upgradeReport.FunctionName = _ReportFunction;

//            // 自定义报表使用的是已存在的BO对象
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

//            // 这种情况下表明其是自定义报表
//            if (note != string.Empty && note != ReportName)
//            {
//                string baseTableNewReportId = string.Format("{0}[__]{1}", SubID.ToUpper(), note);
//                Trace.Write("baseTableNewReportId:" + baseTableNewReportId);
//                RaiseExceptionIfSpecialID(baseTableNewReportId);

//                // 可能存在_BaseTableDataSourceID为空的情况,要做相应的处理
//                _BaseReportDefinition = MendWhenUpgrading.GetSqlBaseReportDefinition(UfMetaConnString, baseTableNewReportId);
//                _BaseTableReportID = MendWhenUpgrading.GetSqlBaseTableOldId(UfDataConnString, ReportID, SubID);
//            }

//            if (_BaseTableReportID == string.Empty)
//                _BaseTableReportID = ReportID;
//        }

//        private void RaiseExceptionIfSpecialID(string reportId)
//        {
//            if (reportId.IndexOf("SA[__]货龄分析") != -1
//                || reportId.IndexOf("ST[__]业务类型汇总表") != -1
//                || reportId.IndexOf("ST[__]收发存汇总表") != -1
//                || reportId.IndexOf("ST[__]供货单位收发存汇总表") != -1)
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Close();
//                throw new Exception("此报表为特殊报表，不能进行升级，关键字：" + reportId);
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

//                    // 准备正确数据源数据
//                    _MendWhenUpgrading = new MendWhenUpgrading(ReportID, LocaleId, UfDataConnString);

//                    FormatXmlWriteStartElement("Report");
//                    FormatXmlWriteAttribute("Name", ReportName);
//                    FormatXmlWriteAttribute("Type", "GridReport");

//                    // 自定义报表ClassName和FilterClass都为空
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
//                           string.Format("升级错误:在子系统{0}中不存在报表\"{1}\"", SubID, ReportName));
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

//            // 只需要解析一次交叉报表交叉区的数据
//            if (this.CrossDetailItems == null || this.CrossColumnHeaderItem == string.Empty)
//                HandleIfCrossReport(ref SQLQueristScript);

//            // 去掉SELECT * FROM ({0}) A 模式
//            //if( SQLQueristScript != string.Empty )
//            //    SQLQueristScript = string.Format( "SELECT * FROM ({0}) A" , SQLQueristScript );
//        }

//        // "TRANSFORM 金额,数量,单价  PIVOT 要素名称 SELECT Pm_PubElementVouchs.cElementCode AS 要素编码,..."
//        // 要解析出"金额,数量,单价"和"要素名称",以及后面的Sql脚本
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
//            //	数据表rpt_FldDef_base中的字段ModeEx=25表示该行是区域数据，
//            //	此时表中的字段OrderEx表示区域上下排序

//            int GroupIndex = 0;
//            ReportHeadBottom = 0;
//            ReportPageTitleBottom = 0;

//            try
//            {
//                for (int OrderEx = 1; ReportID != _InValidatedID; OrderEx++)
//                {
//                    //	由于数据库的错误，一些标签的ModeEx字段也等于25，需要排除
//                    //	即此时nameForeign必须不能等于标题区域,页标题区域,页脚区域,脚注区域
//                    cmd.CommandText = "select * from rpt_flddef_base where id='"
//                        + ReportID
//                        + "' and LocaleId=N'"
//                        + LocaleId
//                        + "' and ModeEx=25 and OrderEx="
//                        + OrderEx.ToString()
//                        + " and ISNULL(nameForeign,'')<>N'标题区域' "
//                        + "and ISNULL(nameForeign,'')<>N'標題區域' "
//                        + "and ISNULL(nameForeign,'')<>N'Header region' "
//                        + "and ISNULL(nameForeign,'')<>N'页标题区域' "
//                        + "and ISNULL(nameForeign,'')<>N'頁標題區域' "
//                        + "and ISNULL(nameForeign,'')<>N'Page title area' "
//                        + "and ISNULL(nameForeign,'')<>N'页脚注区域' "
//                        + "and ISNULL(nameForeign,'')<>N'頁註腳區域' "
//                        + "and ISNULL(nameForeign,'')<>N'页脚区域' "
//                        + "and ISNULL(nameForeign,'')<>N'頁腳區域' "
//                        + "and ISNULL(nameForeign,'')<>N'Page footnote area' "
//                        + "and ISNULL(nameForeign,'')<>N'Page footer region' "
//                        + "and ISNULL(nameForeign,'')<>N'脚注区域' "
//                        + "and ISNULL(nameForeign,'')<>N'註腳區域' "
//                        + "and ISNULL(nameForeign,'')<>N'Footnote area'";

//                    reader = cmd.ExecuteReader();
//                    if (reader.Read())
//                    {
//                        if (reader["FormatEx"].ToString().Trim() != "隐含")
//                        {
//                            SectionName = reader["Name"].ToString().Trim();

//                            // 如果是交叉报表则要另外添加CrossDetail
//                            // 和CrossColumnHeader区域

//                            if (this.IsCrossReport)
//                                WriteCrossDetailCrossColumnHeaderIfCrossReport(SectionName);

//                            FormatXmlWriteStartElement("Section");
//                            WriteSectionAtrrNameType(SectionName, ref GroupIndex);

//                            CalculateTwoBottom(SectionName);

//                            //	开始写其他数据之前要先关闭reader，使得
//                            //	可以利用cnn来打开其它的reader
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
//            // 在正文区之前写入这两个区域
//            if (SectionName == "正文区域"
//                || SectionName == "正文區域"
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

//        // itemIndex用来计算标签的左端位置
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

//        // 特殊的列表达式、交叉报表的升级需要补充数据源信息
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
//                    case "标题区域":
//                    case "標題區域":
//                    case "Header region":
//                        FormatXmlWriteAttribute("Name", "ReportHeader");
//                        FormatXmlWriteAttribute("Type", "ReportHeader");
//                        break;
//                    case "页标题区域":
//                    case "頁標題區域":
//                    case "Page title area":
//                        FormatXmlWriteAttribute("Name", "PageHeader");
//                        FormatXmlWriteAttribute("Type", "PageHeader");
//                        break;
//                    case "正文区域":
//                    case "正文區域":
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
//                    case "页脚注区域":
//                    case "頁註腳區域":
//                    case "Page footnote area":
//                        FormatXmlWriteAttribute("Name", "PageFooter");
//                        FormatXmlWriteAttribute("Type", "PageFooter");
//                        break;

//                    //	旧报表的脚注区域转换为新报表的页脚注区域
//                    case "脚注区域":
//                    case "註腳區域":
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
//                if (SectionName == "标题区域"
//                    || SectionName == "標題區域"
//                    || SectionName == "Header region")
//                {
//                    ReportHeadBottom = (int)reader["TopEx"] + (int)reader["Height"];
//                    ReportHeadBottom = ReportHeadBottom / 15;
//                }
//                else if (SectionName == "正文区域"
//                    || SectionName == "正文區域"
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
//            if (SectionName == "正文区域"
//                || SectionName == "正文區域"
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
//                CurrentColumnX = 7;	//	列标题区域的左端离报表边缘7像素
//                PreviousColumnLeftExIndex = 0;
//                ParentColumnName = "";
//                ColumnInfoForSQLQueristConfig[0] = "";
//                ColumnInfoForSQLQueristConfig[1] = "";
//                ColumnInfoForSQLQueristConfig[2] = "";
//                SuperLabelDataArrayList = new ArrayList();
//                ColumnNameArrayList = new Hashtable();
//                IsSuperLabelDataArrayListEmpty = true;

//                //	预置最大的标题层数，为一些没有相应的列标题信息的列设置标题层数
//                SetMaxColumnLevelCount();

//                cmd.CommandText = UpgradeSqlProducer.GetSqlColumnsInforInDetailSection(
//                    this.ReportID,
//                    this.LocaleId);
//                reader = cmd.ExecuteReader();

//                //	为解决ModeEx=0时有相同的OrderEx的两条纪录的问题
//                //	先纪录已经加入的列，再查找出有相同的OrderEx的列
//                //	如果这些列还没有加入，则需要添加
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

//                        // 双层表头的上层表头为SuperLable,sql查询可
//                        // 能指示其数据源为前面某个已经处理的数据源
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

//                //	将最一个不是SuperLabel的列的信息写入SQLQueristConfig
//                WriteColumnSQLQueristConfig();
//                reader.Close();

//                //	添加由于有相同的OrderEx的两条纪录而遗漏的列
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

//                //	将最一个不是SuperLabel的列的信息写入SQLQueristConfig
//                WriteColumnSQLQueristConfig();
//                reader.Close();
//            }
//        }

//        private void HandleAColumn()
//        {
//            // 如果当前报表是交叉报表,则在交叉报表CrossDetail
//            // 区域的列就不能在交叉报表rossRowHeader区域中
//            if (this.IsCrossReport
//                && this.IsACrossDetailColumn(reader["Column_Expression"]))
//            {
//                return;
//            }

//            string sCaption;
//            string sColumnDataType = GetColumnDataType();

//            //	如果当前的列类型属于SuperLabel，则把其数据暂存起来
//            if (sColumnDataType == "SuperLable")
//            {
//                SaveSuperLabelDataToArrayList();

//                //	为SQLQueristConfig纪录父列信息
//                ParentColumnName = GetParentColumnCation();
//            }
//            else
//            {
//                //	先把前一个不是SuperLabel的列的信息写入SQLQueristConfig
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

//                //	为SQLQueristConfig纪录父列信息
//                ParentColumnName = GetParentColumnCation();

//                //	如果SuperLabelDataArrayList不为空，则检查是否有SuperLabel
//                //	已经可以确定宽度，如果有，则要这时写入其数据
//                if (!IsSuperLabelDataArrayListEmpty)
//                {
//                    HandleArrayList(reader["Title_LeftEx"]);
//                }
//            }
//        }

//        private string GetCellName(string columnDataType)
//        {
//            string cellName = string.Empty;

//            // 当为计算列时，取Name字段值为cellName
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

//                    // 有些报表在这种情况下Expression为空
//                    // 此时取Name字段
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

//            // ModeEx="5"时,表示双层标签的顶层标签的id_field
//            // 在取明细区数据的SQL语句内其用BAK_ID_Field存储
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
//                    string.Format("升级错误：没有查找到ID_Field，关键字：{0};{1};{2}",
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
//                        //	为SQLQueristConfig纪录父列信息
//                        if (bRunOnce)
//                        {
//                            bRunOnce = false;
//                            ParentColumnName = GetParentColumnCation();
//                        }

//                        //	计算此SuperLabel的宽度
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
//                            i = -1;		//	重新开始HandleArrayList
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
//                //	纪录当Title_LeftEx为何值时可以确定此SuperLabel的宽度
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
//            // 为解决用补丁替换列表达式中用数据源的Name来代替Description的方案，
//            // 此处不再为有多层标签报表的标签加父列的标识，如C(A.B)
//            //if( ParentColumnName != "" )
//            //{	
//            //    ParentColumnName					= "(" + ParentColumnName + ")";
//            //    ColumnInfoForSQLQueristConfig[2]	= ColumnInfoForSQLQueristConfig[2] + ParentColumnName;
//            //}

//            //	防止第一个非SuperLabel的列出现时就保存信息
//            //	因为此时还没有存储任何列信息
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

//        // 把所有数据源信息都保存起来，以在升级完成之后再组织成XML串
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
//                // 在原先语言信息列表后新加入一种语言信息
//                ColumnLangInfoNode[] LangInfo = _DataSourceLangInfo[DataSourceInfo[0]] as ColumnLangInfoNode[];
//                if (LangInfo != null)
//                {
//                    ColumnLangInfoNode[] NewLangInfo = new ColumnLangInfoNode[LangInfo.Length + 1];

//                    // 先存储原来就已经存在的信息
//                    for (int i = 0; i < LangInfo.Length; i++)
//                    {
//                        // 如果已经有要添加的语言的信息，则
//                        // 以当前的信息更新该种语言的信息
//                        // 此时无需添加新的语言信息
//                        if (LangInfo[i].LocaleId.ToUpper() == localeId.ToUpper())
//                        {
//                            // 如果是升级列脚本时调用方法AddDataSourceKey
//                            // 中数据源的name和description相同
//                            // 此时应防止此操作修改原有的语言信息
//                            if (DataSourceInfo[2] != DataSourceInfo[0])
//                                LangInfo[i].InfoValue = DataSourceInfo[2];
//                            return;
//                        }

//                        NewLangInfo[i] = LangInfo[i];
//                    }

//                    // 添加新的语言信息
//                    NewLangInfo[LangInfo.Length] = new ColumnLangInfoNode();
//                    NewLangInfo[LangInfo.Length].InfoValue = DataSourceInfo[2];
//                    NewLangInfo[LangInfo.Length].LocaleId = localeId;
//                    _DataSourceLangInfo[DataSourceInfo[0]] = NewLangInfo;
//                }
//            }
//        }

//        private void SetDataSourceInfor()
//        {
//            // 自定义报表时使用的是已存在的BO对象
//            if (IsUseExistDataSource())
//            {
//                _upgradeReport.DataSourceInfo.IsShouldSave = false;
//                return;
//            }

//            // 如果此时升级的是交叉报表,交叉区和列标题区的数据
//            // 源可能不在原来的ModeEx=0的定义中,所以需要添加
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

//            // 升级861自定义报表时，保存数据源将直接写数据表“BD_BusinessObjects”
//            // 而不是走数据引擎的数据服务
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

//            // wangzq规定description最大长度为50
//            string description = ReportName + UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.UpGrade.数据源定义");
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

//            // 在元素“ResultTable”之后写入自定义sql脚本或自定义类信息
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

//        //	获取该列的类型
//        private string GetColumnDataType()
//        {
//            if (reader["Title_Width"] != System.DBNull.Value
//                && (int)reader["Title_Width"] > 1)
//            {
//                return "SuperLable";
//            }
//            else
//            {
//                // 发版之后升级用户自定义报表,屏蔽这个策略:
//                // iSize字段都为0时类型都升级为GridLabel
//                //if( _IsAllTypeBeGridLabel )
//                //    return "GridLabel";

//                string sCondition = SqlHelper.GetStringFrom(reader["Column_CONditiON"]);
//                string sColumnKey = SqlHelper.GetStringFrom(reader["Column_Expression"]);
//                if (sColumnKey == string.Empty)
//                    sColumnKey = SqlHelper.GetStringFrom(reader["Column_Name"]);

//                //	列表达式
//                if (sCondition != string.Empty)
//                {
//                    //	列表达式中引用上一列的某些字段
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
//                    //'数据类型常量定义(861系统报表系统)
//                    //Public Const cDatatypeNumber = 0                    '数值类型
//                    //Public Const cDatatypeString = 1                    '字符串类型
//                    //Public Const cDatatypeDate = 2                      '日期类型
//                    //Public Const cDatatypeBoolean = 3                   '逻辑型数值
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
//                            return "GridLabel";		//	默认类型
//                    }
//                }
//            }
//        }

//        private string GetX(object DBFieldLeftEx, object DBFieldColumniColSize)
//        {
//            //	默认单位层宽度为67像素
//            int iColumniColSize = 67;

//            if (DBFieldColumniColSize != System.DBNull.Value
//                && (int)DBFieldColumniColSize != 0)
//            {
//                iColumniColSize = (int)DBFieldColumniColSize / 15;
//            }

//            try
//            {
//                // 2006-06-16
//                // 对一些自定义项的处理(这些自定义项的字段LeftEx都为0)
//                if (DBFieldLeftEx == System.DBNull.Value
//                    || (int)DBFieldLeftEx == 0)
//                {
//                    PreviousColumnLeftExIndex++;
//                    PreviousColumnX = CurrentColumnX;
//                    CurrentColumnX = CurrentColumnX + iColumniColSize;
//                    return System.Convert.ToString(PreviousColumnX);
//                }

//                //	当前的LeftEx和上一个LeftEx相同时，则当前为SuperLabel
//                //	还不能确定其宽度，所以保持状态值PreviousColumnLeftExIndex、
//                //	PreviousColumnX、CurrentColumnX不变
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
//            //	单层列标题默认起始层为1，默认高度为17像素
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
//            //	单层列标题默认宽度层数为1，默认单位层宽度为67像素
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
//            //	有些没有标题信息的列相应的没有高度层数信息,此时将层数其置为标题的最大层数
//            //	单层列标题默认高度层数为1，默认单位层高度为17像素
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
//        //    Money			=1,				//金额
//        //    Weight		=2,				//重量
//        //    Volume		=3,				//体积
//        //    Rate			=4,				//比率
//        //    Quantity		=5,				//数量
//        //    PieceNum		=6,				//件数
//        //    ExchangeRate	=7,				//换算率
//        //    TaxRate		=8,				//税率
//        //    BillPrice		=9,				//开票单价
//        //    CostMoney		=10,			//成本金额
//        //    CostQuantity	=11				//成本数量
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
//            //			旧数据的对齐对应关系定义
//            //			enum stdalign                        
//            //				alignnormal = 0
//            //				alignleft	= 1
//            //				aligncenter	= 2
//            //				alignright	= 3
//            //			end enum

//            //.Net中的对齐枚举
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

//        // 与语言无关
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

//            // 判断是否是供应商
//            if (fieldName.IndexOf("CVENDEFINE") != -1)
//                userDefineKey = "@供应商.自定义项" + index.ToString();

//            // 判断是否是存货自定义项
//            else if (fieldName.IndexOf("CINVDEFINE") != -1)
//                userDefineKey = "@存货.自定义项" + index.ToString();

//            // 判断是否是存货自由项
//            else if (fieldName.IndexOf("CFREE") != -1)
//                userDefineKey = "@存货.自由项" + index.ToString();

//            // 判断是否是联系人
//            else if (fieldName.IndexOf("CCONDEFINE") != -1)
//                userDefineKey = "@联系人.自定义项" + index.ToString();

//            // 判断是否是序列号属性
//            else if (fieldName.IndexOf("CSNDEFINE") != -1)
//                userDefineKey = "@序列号.属性" + index.ToString();

//            // 判断是否是单据头或单据体
//            else if (fieldName.IndexOf("DDEFINE_") != -1
//                || fieldName.IndexOf("DEFINE_") != -1
//                || fieldName.IndexOf("CDEFINE") != -1)
//            {
//                if (index > 21)
//                {
//                    index -= 21;
//                    userDefineKey = "@单据体.自定义项" + index.ToString();
//                }
//                else
//                    userDefineKey = "@单据头.自定义项" + index.ToString();
//            }

//            // 判断是否存货自定义项
//            else if (fieldName.IndexOf("INVDEFINE_") != -1
//                || fieldName.IndexOf("DINVDEFINE_") != -1)
//            {
//                userDefineKey = "@存货.自定义项" + index.ToString();
//            }

//            // 判断是否存货自由项
//            else if (fieldName.IndexOf("INVFREE_") != -1
//                || fieldName.IndexOf("DINVFREE_") != -1)
//            {
//                userDefineKey = "@存货.自定义项" + index.ToString();
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
//                    string.Format("升级错误发生在GetUserDefineIndex(),请与ldq联系.\r\n错误关键字符串：\r\n{0}\r\n错误原因：\r\n{1}",
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

//            //	输出列是否可以合计的信息
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

//                // 与语言无关的信息采用中文情况的数据
//                // 下述语句中文情况下执行
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

//            //	具有数据源的列的数据暂存起来，以便在以后写入SQLQueristConfig
//            //	因为是GridDecimalAlgorithmColumn的列也要做为数据源,而很多复杂计算列的Expression
//            //	字段都为空,此时取Name字段;而不同语言下复杂计算列的Name值会不一样,所以只采用中文下
//            //	复杂计算列的信息
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

//                //	根据当前的列的Title_TopEx确定GetParentColumnCation中要搜索的父列数
//                if (reader["Title_TopEx"] != System.DBNull.Value
//                    && (int)reader["Title_TopEx"] != 0)
//                    CurrentColumnLevelTop = (int)reader["Title_TopEx"];
//            }
//        }

//        //	如果当前列存在父列，则从当前的SuperLabelDataArrayList中拼凑出父列名称串，
//        //	父列名称串的格式如：A.B
//        private string GetParentColumnCation()
//        {
//            int iPreTopEx = System.Int32.MinValue;		//	上一层父列的Y坐标
//            int iCurTopEx;									//	目前与上一层最接近列的Y坐标
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

//                // 曾经有因为一些报表中数据有错导致此处引发异常
//                // 错误可能是一些纪录在ModeEx=0时具有相等OrderEx
//                // 但是对于质量管理的一些报表数据没错，也出现异常，未找到原因
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

//        //	取得夸列间表达式
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

//                value = value.Replace("期初值LdqAddForRepaceLater", currentValue);
//            }
//            //	可能存在这样的情况：表达式中含有“PREV_”，但是却不含有“[”或“]”
//            //	现预留这里作为将来可能要处理这种情况的位置

//            return value;
//        }

//        // 升级为脚本的时候需要特殊加入脚本中含有的数据源
//        // 以防止脚本中有些数据源关键字并非是数据源
//        // 如“代销商品款台账”中列“JYQuantity”
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

//        // 转换IIF表达式为C#脚本
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

//            // 先代替含有"="的运算符
//            value = value.Replace("<=", "<<");
//            value = value.Replace(">=", ">>");
//            value = value.Replace("!=", "!!");

//            value = value.Replace("=", "==");

//            // 把已经代替含有"="的运算符换回来
//            value = value.Replace("<<", "<=");
//            value = value.Replace(">>", ">=");
//            value = value.Replace("!!", "!=");

//            string[] keys = value.Split(',');
//            if (keys == null || keys.Length != 3)
//                throw new Exception(string.Format("在转换表达式{0}时出现错误", expression));

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
//                SourceString = SourceString.Replace(SubString, "期初值LdqAddForRepaceLater");
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
//                bool bHasUFSoftString;			//	是否含有字符串“【用友软件】”，以增加标签宽度
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
//                    FormatXmlWriteAttribute("Visible", "true");	//	正文区外的标签设置为全部可见
//                    FormatXmlWriteAttribute("KeepPos", "true");

//                    // 非明细区内的标签升级为无边框
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

//        //	数据表rpt_flddef_base的字段Note用型如“公式;YYYY-MM-DD;系统内置公式”
//        //	来表示Label的类型等信息,而字段Expression则表示相应类型的值

//        //  相关枚举

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

//                //	中文标点转换为英文标点，防止Label出现分行
//                sExpression = sExpression.Replace("：", ":");
//                sExpression = sExpression.Replace("，", ",");

//                string sType = "";
//                string sFormatString = "";
//                string FormulaTypeSource = "";
//                if (sTemp.Length >= 3)
//                {
//                    sType = sTemp[0].Trim();
//                    sFormatString = sTemp[1].Trim();
//                    FormulaTypeSource = sTemp[2].Trim();

//                    // 把数据库中错误的"YYYY-MM-DD"改为"yyyy-MM-dd"
//                    sFormatString = sFormatString.Replace("yyyy-mm-dd", "yyyy-MM-dd");
//                    sFormatString = sFormatString.Replace("YYYY-MM-DD", "yyyy-MM-dd");
//                }

//                //	Expression的FormulaType属性信息
//                string iFormulaType = "";

//                if (sExpression.IndexOf("【用友软件】") != -1)
//                    bHasUFSoftString = true;

//                switch (sType)
//                {
//                    case "文本":
//                        sType = "CommonLabel";

//                        // Expression字段为空时，取Name字段为Caption的值
//                        if (sExpression != string.Empty)
//                            FormatXmlWriteAttribute("Caption", sExpression);
//                        else
//                            FormatXmlWriteAttribute("Caption", reader["Name"].ToString().Trim());
//                        break;
//                    case "SQL查询":
//                        sType = "Expression";
//                        iFormulaType = "3";
//                        break;
//                    case "公式":

//                        //	“系统内置公式”GetReportName转换为文本
//                        if (sExpression == "GetReportName()")
//                        {
//                            sType = "CommonLabel";
//                            FormatXmlWriteAttribute("Caption", ReportTitle);
//                        }
//                        else
//                        {
//                            sType = "Expression";

//                            //	“系统内置公式”GetFilterValue的FormulaTyp为“Filter”枚举
//                            //	其余“系统内置公式”是“Common”枚举
//                            //	“用户自定义公式”是“UserDefine”枚举
//                            //	其它是“Business”枚举
//                            if (sExpression.IndexOf("GetFilterValue") != -1)
//                            {
//                                iFormulaType = "1";
//                                sExpression = ConvertToNewFilterFuction(sExpression);
//                            }
//                            else
//                            {
//                                if (FormulaTypeSource == "系统内置公式")
//                                    iFormulaType = "0";
//                                else if (FormulaTypeSource == "自定义公式")
//                                    iFormulaType = "3";
//                                else
//                                    iFormulaType = "2";
//                            }
//                        }
//                        break;
//                    case "表达式":
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

//            //	GetFilterValue(*,0,0)转换为GetValue1("*")
//            //	GetFilterValue(*,0,1)转换为GetValue2("*")
//            //	GetFilterValue(*,2,0)转换为GetName1("*")
//            //	GetFilterValue(*,2,1)转换为GetName2("*")
//            string[] sTemp1 = sExpression.Split('(');
//            if (sTemp1.Length >= 2)
//            {
//                string[] sTemp2 = sTemp1[1].Split(',');
//                if (sTemp2.Length >= 3)
//                    sReturnVal = GetFilterFunction(sTemp2);
//            }
//            return sReturnVal;
//        }

//        //	GetFilterValue(*,0,0)转换为GetValue1("*")
//        //	GetFilterValue(*,0,1)转换为GetValue2("*")
//        //	GetFilterValue(*,2,0)转换为GetName1("*")
//        //	GetFilterValue(*,2,1)转换为GetName2("*")
//        private string GetFilterFunction(string[] sTemp2)
//        {
//            // 防止类似"1  )"的情况
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

//            // Expression的Formula属性仅以中文的为准
//            // 而Caption取与语言相关的表示方式
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

//            //	字段“Condition”必须有类似“True,134,False,宋体,15.75,False,False,700”
//            //	这样的值才对该Label设置字体信息
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
//                        // 对多种语言，SchemaItem仅写入一次
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
//                    return "方案1";
//                case "zh-tw":
//                    return "方案1";
//                case "en-us":
//                    return "Scheme1";
//                default:
//                    return "方案1";
//            }
//        }

//        private void ExceptionHandler(string Source, System.Exception e)
//        {
//            //            try
//            //            {
//            //                Logger logger = Logger.GetLogger();
//            //                logger.Error( string.Format( "报表升级出错:报表标识=={0}", ReportID ));
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
//                string temp = "异常个数：" + ExceptionCount.ToString() + "\r\n";
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
