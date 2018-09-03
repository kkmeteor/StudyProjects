using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UFIDA.U8.MA.Component.Framework;
using UFIDA.U8.UAP.Services.ReportData;
using System.Text;
using UFIDA.U8.MERP.MerpContext;
using U8Login;
using UFIDA.U8.UAP.Services.ReportElements;
using Infragistics.Win.UltraWinChart;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class OpenReportActionHandler : MAActionHandler
    {
        private U8LoginInfor _loginInfo;
        #region ULink使用字段
        private string _currentPage;
        private string _totolPage;
        private string _columnsStr;
        private string _cacheid;
        #endregion 待删ULink除的字段
        private MobileReport _mobileReport;
        /// <summary>
        /// 每页显示行数
        /// </summary>
        private int _pageRowsCount;
        private string _responseXml;

        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public OpenReportActionHandler()
        {
        }
        /// <summary>
        /// 新增供U易联调用的接口
        /// </summary>
        /// <param name="login"></param>
        /// <param name="actionType"></param>
        /// <param name="parameters"></param>
        /// <param name="responseXml"></param>
        /// <returns></returns>
        public string Execute(U8LoginInfor login, string actionType, Dictionary<string, string> parameters,
                                             ref string responseXml)
        {
            string result = "";
            bool is4chart = false;
            if (parameters.ContainsKey("is4chart"))
                is4chart = Boolean.Parse(parameters["is4chart"]);
            this._loginInfo = login;
            this._responseXml = responseXml;
            if (!this.CheckReportExist(parameters))
            {
                result = "该查询方案不存在，可能已经取消发布到移动端，请确认！";
                throw new Exception("该查询方案不存在，可能已经取消发布到移动端，请确认！");
            }
            try
            {
                MobileReportEngine engine = new MobileReportEngine(this._loginInfo, actionType, parameters, ref responseXml);
                int startLine = Convert.ToInt32(this.GetInformationByKey("startline", parameters));
                this._columnsStr = this.GetInformationByKey("columnsstring", parameters);
                int pageRowCount = 25;
                if (this.GetInformationByKey("count", parameters) != null)
                    pageRowCount = Convert.ToInt32(this.GetInformationByKey("count", parameters));

                engine.SetReportPageRowCount(pageRowCount);
                if (startLine == 1) //请求第一页数据
                {
                    this._mobileReport = engine.OpenReport(parameters);
                    this._pageRowsCount = engine.GetReportPageRowCount();
                    //this._mobileReport = this.OpenReport(this._loginInfo, parameters);
                    this._mobileReport.Page = 0;
                    int totalpage = this._mobileReport.Report.RowsCount / this._pageRowsCount + 1;
                    string reportCacheString = RetrieveRowData2Json.GetColumnsInfoString(this._mobileReport);
                    this.SetCacheInfoIntoSession(ref responseXml, "currentpage", "1");
                    this.SetCacheInfoIntoSession(ref responseXml, "totalpage", totalpage.ToString());
                    byte[] bytes = Encoding.Default.GetBytes(reportCacheString);
                    reportCacheString = Convert.ToBase64String(bytes);
                    this.SetCacheInfoIntoSession(ref responseXml, "columnsString", reportCacheString);
                    this.SetCacheInfoIntoSession(ref responseXml, "cacheid", this._mobileReport.Report.CacheID);

                }
                else
                {
                    int pageIndex = Convert.ToInt32(this.GetCacheInfoFromSession(responseXml, "currentpage"));
                    if (pageIndex == 0)
                    {
                        pageIndex = Convert.ToInt32(this.GetInformationByKey("currentpage", parameters));
                        //pageIndex = Convert.ToInt32(parameters["currentpage"].ToString());
                    }
                    int totalpage = Convert.ToInt32(this.GetCacheInfoFromSession(responseXml, "totalpage"));
                    if (totalpage == 0)
                    {
                        totalpage = Convert.ToInt32(this.GetInformationByKey("totalpage", parameters));
                        this._totolPage = totalpage.ToString();
                    }
                    if (pageIndex + 1 > totalpage)
                    {
                        result = null;
                        return result;
                    }
                    else
                    {
                        string cacheId = GetCacheInfoFromSession(responseXml, "cacheid");
                        if (string.IsNullOrEmpty(cacheId))
                        {
                            cacheId = this.GetInformationByKey("cacheid", parameters);
                            this._cacheid = cacheId;
                        }
                        int lastIndex = -1;
                        //this._mobileReport = this.PageTo(cacheId, pageIndex, lastIndex);
                        this._mobileReport = engine.PageTo(cacheId, pageIndex, lastIndex);
                        this.SetCacheInfoIntoSession(ref responseXml, "currentpage", (pageIndex + 1).ToString());
                        this._mobileReport.Page = pageIndex + 1;
                    }
                }

                if (_mobileReport != null)
                {
                    int pageIndex = Convert.ToInt32(this.GetCacheInfoFromSession(responseXml, "currentpage"));
                    if (pageIndex == 0)
                    {
                        if (parameters.ContainsKey("currentpage"))
                            pageIndex = Convert.ToInt32(this.GetInformationByKey("currentpage", parameters));
                    }
                    // 由于CS端每页都需要显示总计行，这里需要将不是最后一页的总计行删除掉
                    //if (!bLastPage(pageIndex, _mobileReport.Report) && this._mobileReport.Report.Sections[SectionType.ReportSummary] != null)
                    //{
                    //    _mobileReport.SemiRows.RemoveAt(_mobileReport.SemiRows.Count - 1);
                    //}
                    // 如果是请求图表数据
                    if (is4chart)
                    {
                        var report = _mobileReport.Report;
                        engine.InitMobileReportChartSchema(report);
                        ChartService chartservice = new ChartService(report);
                        ChartSchemas css = report.ChartSchemas;
                        if (css.CurrentGroupChart.Items != null && css.CurrentGroupChart.Items.Count > 0)
                        {
                            ChartSchemaItem groupCharts = css.CurrentGroupChart.Items[1] as ChartSchemaItem;
                            if (groupCharts != null)
                            {
                                foreach (ChartSchemaItemAmong among in groupCharts.Items.Values)
                                {
                                    UltraChart designChart = MobileChartHelper.CreateAchartByDesignTime(among);
                                    var dataTable1 = chartservice.GetDataSource(groupCharts.Level, among.ID, null, designChart.ChartType);
                                    MobileChart mobileChart = new MobileChart(designChart.ChartType, dataTable1);
                                    string result1 = MobileChartHelper.TransferDataTableToString(mobileChart);
                                    MobileChartHelper.Desrialize<MobileChart>(mobileChart, result1);
                                    return result1;
                                }
                            }
                        }
                    }
                    System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ParseReport TaskID: " + _loginInfo.TaskID + "  Start:" + System.DateTime.Now.ToString());
                    result = ULinkParse(this._mobileReport);
                    System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ParseReport TaskID: " + _loginInfo.TaskID + "  End:" + System.DateTime.Now.ToString());
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("无法将类型为“U8Login.clsLoginClass”的 COM 对象强制转换为接口类型“U8Login._clsLogin”") && !parameters.ContainsKey("AAA"))
                {
                    parameters.Add("AAA", "1");
                    return Execute(this._loginInfo, actionType, parameters, ref responseXml);
                }
                else
                {
                    throw ex;
                }
            }
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ExecuteOpenReport TaskID: " + this._loginInfo.UserToken + "  End:" + System.DateTime.Now.ToString());
            return result;
        }
        private bool bLastPage(int pageindex, Report _report)
        {
            if (pageindex == -1)
                return true;
            if (_report.bPageByGroup)
                return (pageindex == (_report.RowsCount == 0 ? 1 : _report.RowsCount) - 1);
            else
                return pageindex == ((_report.RowsCount / _report.PageRecords + (_report.RowsCount % _report.PageRecords == 0 ? 0 : 1)) - 1);
        }

        public override ActionResult Execute(string token, string actionType, Dictionary<string, string> parameters,
                                             ref string responseXml)
        {
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ExecuteOpenReport TaskID: " + token + "  Start:" + System.DateTime.Now.ToString());
            this.Init(token, actionType, parameters, ref responseXml);
            this._responseXml = responseXml;
            var result = new ActionResult()
            {
                Action = actionType,
                Flag = 1,
                Description = "调用失败",
                ResultData = null
            };
            if (!this.CheckReportExist(parameters))
            {
                result.Description = "该查询方案不存在，可能已经取消发布到移动端，请确认！";
                return result;
            }
            try
            {
                var engine = new MobileReportEngine(token, actionType, parameters, ref responseXml);
                int startLine = Convert.ToInt32(this.GetInformationByKey("startline", parameters));
                if (startLine == 1) //请求第一页数据
                {
                    this._mobileReport = engine.OpenReport(parameters);
                    this._pageRowsCount = engine.GetReportPageRowCount();
                    //this._mobileReport = this.OpenReport(this._loginInfo, parameters);
                    this._mobileReport.Page = 0;
                    int totalpage = this._mobileReport.Report.RowsCount / this._pageRowsCount + 1;
                    string reportCacheString = RetrieveRowData.GetColumnsInfoString(this._mobileReport);
                    this.SetCacheInfoIntoSession(ref responseXml, "currentpage", "1");
                    this.SetCacheInfoIntoSession(ref responseXml, "totalpage", totalpage.ToString());
                    byte[] bytes = Encoding.Default.GetBytes(reportCacheString);
                    reportCacheString = Convert.ToBase64String(bytes);
                    this.SetCacheInfoIntoSession(ref responseXml, "columnsString", reportCacheString);
                    this.SetCacheInfoIntoSession(ref responseXml, "cacheid", this._mobileReport.Report.CacheID);

                }
                else
                {
                    int pageIndex = Convert.ToInt32(this.GetCacheInfoFromSession(responseXml, "currentpage"));
                    if (pageIndex == 0)
                    {
                        pageIndex = Convert.ToInt32(parameters["currentpage"].ToString());
                    }
                    int totalpage = Convert.ToInt32(this.GetCacheInfoFromSession(responseXml, "totalpage"));
                    if (totalpage == 0)
                    {
                        totalpage = Convert.ToInt32(parameters["totalpage"].ToString());
                        this._totolPage = totalpage.ToString();
                    }
                    if (pageIndex + 1 > totalpage)
                    {
                        result.ResultData = null;
                        result.Flag = 0;
                        return result;
                    }
                    else
                    {
                        string cacheId = GetCacheInfoFromSession(responseXml, "cacheid");
                        if (string.IsNullOrEmpty(cacheId))
                        {
                            cacheId = parameters["cacheid"].ToString();
                            this._cacheid = cacheId;
                        }
                        int lastIndex = -1;
                        //this._mobileReport = this.PageTo(cacheId, pageIndex, lastIndex);
                        this._mobileReport = engine.PageTo(cacheId, pageIndex, lastIndex);
                        this.SetCacheInfoIntoSession(ref responseXml, "currentpage", (pageIndex + 1).ToString());
                        this._mobileReport.Page = pageIndex + 1;
                    }
                }

                if (_mobileReport != null)
                {

                    System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ParseReport TaskID: " + _loginInfo.TaskID + "  Start:" + System.DateTime.Now.ToString());
                    result.ResultData = Parse(this._mobileReport);
                    System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ParseReport TaskID: " + _loginInfo.TaskID + "  End:" + System.DateTime.Now.ToString());
                    result.Flag = 0;
                    result.Description = "获取报表成功";
                }
            }
            catch (Exception ex)
            {
                result.Flag = 1;
                result.Description = "获取报表数据失败:" + ex.Message;

                if (ex.Message.Contains("无法将类型为“U8Login.clsLoginClass”的 COM 对象强制转换为接口类型“U8Login._clsLogin”") && !parameters.ContainsKey("AAA"))
                {
                    parameters.Add("AAA", "1");
                    return Execute(token, actionType, parameters, ref responseXml);
                }
            }
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->ExecuteOpenReport TaskID: " + token + "  End:" + System.DateTime.Now.ToString());
            return result;
        }

        private bool CheckReportExist(Dictionary<string, string> parameters)
        {
            //return true;
            var solutionId = this.GetInformationByKey("solutionId", parameters);
            // 由于财务也需要通过此种方式来获取报表数据，为了保证不冲突，这里先放开检测。
            //string commandText = string.Format(@"SELECT 1 FROM flt_Solution WHERE ID = '{0}' AND IsMobile = 1", solutionId);
            string commandText = string.Format(@"SELECT 1 FROM flt_Solution WHERE ID = '{0}'", solutionId);
            var result = SqlHelper.ExecuteScalar(this._loginInfo.UfDataCnnString, commandText);
            return result != null;
        }

        /// <summary>
        /// 将报表CacheId放入缓存
        /// </summary>
        /// <param name="responseXml"></param>
        private void SetCacheInfoIntoSession(ref string responseXml, string key, string value)
        {
            if (!string.IsNullOrEmpty(responseXml))
            {
                var doc = new XmlDocument();
                doc.LoadXml(responseXml);
                XmlNode contextstructNode = doc.SelectSingleNode("response/contextstruct");
                XmlNode cacheNode = doc.SelectSingleNode(string.Format("response/contextstruct/{0}", key));
                if (cacheNode == null)
                {
                    cacheNode = doc.CreateElement(key);
                    cacheNode.InnerText = value;
                    if (contextstructNode != null)
                    {
                        contextstructNode.AppendChild(cacheNode);
                    }
                }
                else
                {
                    cacheNode.InnerText = value;
                }
                responseXml = doc.InnerXml;
            }
            else// ULIKN报表不使用此参数
            {
                switch (key)
                {
                    case "currentpage":
                        this._currentPage = value;
                        break;
                    case "totalpage":
                        this._totolPage = value;
                        break;
                    case "columnsString":
                        byte[] bytes = Convert.FromBase64String(value);
                        this._columnsStr = Encoding.Default.GetString(bytes); ;
                        break;
                    case "cacheid":
                        this._cacheid = value;
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// 将报表对象转换为DSL串
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private string Parse(MobileReport report)
        {
            string tableSchema = string.Empty;
            string reportStr;
            string rowData;
            if (report.Page == 0) //第一页
            {
                tableSchema = DslTransfer.TransferToTableSchema(report);
                //Logger logger = Logger.GetLogger("DSLTransfer");
                //logger.Info(tableSchema);
                //logger.Close();
                byte[] bytes = Encoding.Default.GetBytes(tableSchema);
                tableSchema = Convert.ToBase64String(bytes);
                rowData = RetrieveRowData.TransferToRowData(report);
                reportStr = string.Format("<report><tableSchema>{0}</tableSchema>{1}</report>", tableSchema, rowData);
            }
            else
            {
                if (report.SemiRows != null)
                {
                    string columnstr = this.GetCacheInfoFromSession(this._responseXml, "columnsString");
                    byte[] bytes = Convert.FromBase64String(columnstr);
                    columnstr = Encoding.Default.GetString(bytes);
                    rowData = RetrieveRowData.TransferToRowData(columnstr, report);
                    reportStr = string.Format("<report>{0}</report>", rowData);
                }
                else
                {
                    reportStr = null;
                }
            }
            return reportStr;
        }

        private string ULinkParse(MobileReport report)
        {

            string tableSchema = string.Empty;
            string reportStr;
            string rowData;
            if (report.Page == 0) //第一页
            {
                //tableSchema = JsonTransfer.TransferToTableSchema(report);
                //byte[] bytes = Encoding.Default.GetBytes(tableSchema);
                //tableSchema = Convert.ToBase64String(bytes);
                rowData = RetrieveRowData2Json.TransferToRowData(report);
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"resresult\":{");
                sb.Append("\"flag\": \"0\",");

                sb.Append("\"currentpage\": \"");
                sb.Append(string.IsNullOrEmpty(this._currentPage) ? "" : this._currentPage + "\",");

                sb.Append("\"totalpage\": \"");
                sb.Append(string.IsNullOrEmpty(this._totolPage) ? "" : this._totolPage + "\",");

                sb.Append("\"columnsstring\": \"");
                sb.Append(string.IsNullOrEmpty(this._columnsStr) ? "" : this._columnsStr + "\",");

                sb.Append("\"cacheid\": \"");
                sb.Append(string.IsNullOrEmpty(this._cacheid) ? "" : this._cacheid + "\",");

                sb.Append("\"desc\": \"查询报表详情成功！\",");
                sb.Append("\"resdata\": ");
                sb.Append(string.Format("{0}", rowData));
                sb.Append("}}");
                reportStr = sb.ToString();
                //reportStr = string.Format("<report><tableSchema>{0}</tableSchema>{1}</report>", tableSchema, rowData);
            }
            else
            {
                if (report.SemiRows != null)
                {
                    string columnstr = this.GetCacheInfoFromSession(this._responseXml, "columnsString");
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(columnstr);
                        columnstr = Encoding.Default.GetString(bytes);
                    }
                    catch
                    {
                        columnstr = this._columnsStr;
                    }
                    rowData = RetrieveRowData2Json.TransferToRowDataJson(columnstr, report);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append("\"resresult\":{");
                    sb.Append("\"flag\": \"0\",");

                    sb.Append("\"currentpage\": \"");
                    sb.Append(string.IsNullOrEmpty(this._currentPage) ? "" : this._currentPage + "\",");

                    sb.Append("\"totalpage\": \"");
                    sb.Append(string.IsNullOrEmpty(this._totolPage) ? "" : this._totolPage + "\",");

                    sb.Append("\"columnsstring\": \"");
                    sb.Append(string.IsNullOrEmpty(this._columnsStr) ? "" : this._columnsStr + "\",");

                    sb.Append("\"cacheid\": \"");
                    sb.Append(string.IsNullOrEmpty(this._cacheid) ? "" : this._cacheid + "\",");

                    sb.Append("\"desc\": \"查询报表详情成功！\",");
                    sb.Append("\"resdata\":{");
                    sb.Append(string.Format("{0}", rowData));
                    sb.Append("}}");
                    reportStr = sb.ToString();
                    //reportStr = string.Format("<report>{0}</report>", rowData);
                }
                else
                {
                    reportStr = null;
                }
            }
            return reportStr;
        }

        /// <summary>
        /// 从responseXml中获取SessionID
        /// </summary>
        /// <param name="responseXml"></param>
        /// <returns></returns>
        private string GetCacheInfoFromSession(string responseXml, string key)
        {
            string result = null;

            if (!string.IsNullOrEmpty(responseXml))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseXml);
                //这个结构：businesssession/sessionguid，大家可以自己定义。
                //必须保证设置和取的路径一直即可。
                XmlNode node = doc.SelectSingleNode(string.Format("response/contextstruct/{0}", key));
                if (node != null)
                {
                    result = node.InnerText;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetInformationByKey(string p, Dictionary<string, string> parameters)
        {
            if (parameters.Keys.Contains(p))
                return parameters[p];
            return string.Empty;
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
        }

        #region 准备删除的测试方法

        ///// <summary>
        ///// 通过传入参数初始化查询报表需要参数
        ///// </summary>
        //  private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        //  {
        //      this._loginInfo = TokenTransfer.GetLoginInfo(token);
        //      ClientReportContext.Login = this._loginInfo;
        //      this._solutionId = this.GetInformationByKey("solutionId", parameters);

        //      string commandText = string.Format(@"SELECT * FROM flt_Solution WHERE ID = '{0}' AND IsMobile = 1", _solutionId);
        //      object objSolution = SqlHelper.ExecuteDataSet(_loginInfo.UfDataCnnString, commandText);
        //      var ds = objSolution as DataSet;
        //      if (ds != null && ds.Tables.Count > 0)
        //      {
        //          DataTable dt = ds.Tables[0];
        //          this._groupId = dt.Rows[0]["GroupId"].ToString();
        //          this._pageRowsCount = Convert.ToInt32(dt.Rows[0]["PageRowCount"].ToString());
        //          //this._pageRowsCount = 200;
        //          this._viewId = dt.Rows[0]["ViewId"].ToString();
        //      }
        //      object result = SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString,
        //                                              string.Format(@"SELECT  a.ID AS ReportId,b.ID AS ReportViewId FROM UAP_Report a 
        //                                                  LEFT JOIN UAP_ReportView b ON a.ID = b.ReportID WHERE
        //                                                  b.ID = '{0}'", _viewId));
        //      var dataSet = result as DataSet;
        //      if (dataSet != null && dataSet.Tables.Count > 0)
        //      {
        //          DataTable dt = dataSet.Tables[0];
        //          this._reportId = dt.Rows[0]["ReportId"].ToString();
        //          this._viewId = dt.Rows[0]["ReportViewId"].ToString();
        //      }
        //      string root = this.GetU8Path();
        //      DefaultConfigs.CachePath = Path.Combine(root, @"SLReportEngine\Cache");
        //      //若用户未购买BS报表，则可能不存在该path这里生成一下,代码走查点修改
        //      if (!Directory.Exists(DefaultConfigs.CachePath))
        //      {
        //          Directory.CreateDirectory(DefaultConfigs.CachePath);
        //      }
        //  }
        ///// <summary>
        ///// 通过注册表获取U8安装路径
        ///// </summary>
        ///// <returns>U8安装路径</returns>
        //private string GetU8Path()
        //{
        //    string subKey = @"Software\UFSoft\WF\V8.700\Install\CurrentInstPath", sValue = "";
        //    try
        //    {
        //        using (var key = Registry.LocalMachine)
        //        {
        //            using (RegistryKey key2 = key.OpenSubKey(subKey))
        //            {
        //                if ((key2 != null) && (key2.GetValue(sValue) != null))
        //                {
        //                    return (string)key2.GetValue(sValue);
        //                }
        //            }
        //        }
        //        return GetInstallPath();
        //    }
        //    catch
        //    {
        //        return GetInstallPath();
        //    }

        //}
        //private string GetInstallPath()
        //{
        //    string referencepath = Path.GetDirectoryName(Application.ExecutablePath);
        //    if (referencepath != null && !referencepath.EndsWith(@"\"))
        //        referencepath += @"\";

        //    if (referencepath.ToLower().EndsWith(@"\uap\"))
        //    {
        //        //
        //        // 如果是从uap目录下执行 去掉路径中的"uap\"
        //        //
        //        referencepath.Remove(referencepath.Length - 4);
        //    }
        //    return referencepath;
        //}

        //private void Copy(MobileReport mobileReport)
        //{
        //    while (mobileReport.SemiRows.Count < 50000)
        //    {
        //        mobileReport.SemiRows.Add(semiRow);
        //    }
        //}

        //private RuntimeFormat GetRunTimeFormatByDynamicColumn(string[] dynamicAddColumns)
        //{
        //    RuntimeFormat runtimeFormat = null;
        //    if (dynamicAddColumns != null && dynamicAddColumns.Length > 0)
        //    {
        //        runtimeFormat = new RuntimeFormat();
        //        IConfigXmlItem addedCos = new RuntimeFormatInfo();
        //        addedCos.SetProperty(ConfigXmlContext.XmlKeyId, RuntimeFormatServerContext.ArgKeyDymanicAddedCols);
        //        runtimeFormat.SetSubItem(addedCos);
        //        foreach (string key in dynamicAddColumns)
        //        {
        //            RuntimeFormatInfo rfi = new RuntimeFormatInfo();
        //            rfi.SetProperty(ConfigXmlContext.XmlKeyId, key);
        //            addedCos.SetSubItem(rfi);
        //        }
        //    }
        //    return runtimeFormat;
        //}

        ///// <summary>
        ///// 打开一张报表
        ///// 三个动作
        ///// 1.LoadFormat 读取报表结构
        ///// 2.GetSql 获取数据读取SQL
        ///// 3.OpenReport 打开一张报表
        ///// </summary>
        ///// <param name="login">U8登陆对象</param>
        ///// <param name="parameters"></param>
        //public MobileReport OpenReport(U8LoginInfor login, Dictionary<string, string> parameters)
        //{
        //    #region 1.LoadFormat 读取报表结构

        //    var reportEngine = new ReportEngine(login, ReportStates.Browse);
        //    reportEngine.LoadFormat(null, this._viewId, null, null, null, null, null);

        //    #endregion 1.LoadFormat 读取报表结构


        //    #region 2.GetSql 获取数据读取SQL

        //    // 调用习文的FilterSrv方法，构建FilterArgs的RawFilter部分
        //    FilterSrv objfilter = new FilterSrvClass();
        //    object err = null;
        //    objfilter.InitSolutionID = _solutionId;
        //    // 构建Filter对象
        //    _filter = new FilterArgs(this._reportId, this._filterClass, login);
        //    this.InitFilter(_filter, login);
        //    bool flag = objfilter.OpenFilter(login.U8Login, _filterId, null, null, err, true);
        //    if (flag)
        //    {
        //        //1.初始化一个reportEngine对象
        //        _filter.DataSource.FilterString = this.FillCustomFilterString(objfilter, parameters);
        //        _filter.RawFilter = objfilter;
        //        _filter.ViewID = this._viewId;
        //    }

        //    #endregion 2.GetSql 获取数据读取SQL


        //    #region 3.OpenReport 打开一张报表
        //    var engineAdapter = new MobileReportEngineAdapter(login);
        //    //这里有个GetSql（）方法。
        //    this._filterFlag = this.GetfilterFlag();
        //    engineAdapter.OpenReport(_filter, _filterFlag, null, null, null, _baseTableName);
        //    // 参照BS的GetMessage,循环调用拉取数据
        //    this._mobileReport = engineAdapter.GetReport();

        //    // 如果是第一次打开新的报表

        //    while (!this._mobileReport.PageEnd)
        //    {
        //        engineAdapter.GetMessage();
        //    }
        //    this._mobileReport = engineAdapter.GetReport();
        //    return this._mobileReport;

        //    #endregion 3.OpenReport 打开一张报表
        //}

        ///// <summary>
        ///// 使用U8Login对象初始化FilterArgs
        ///// </summary>
        ///// <param name="filterargs"></param>
        ///// <param name="login"></param>
        //private void InitFilter(FilterArgs filterargs, U8LoginInfor login)
        //{
        //    filterargs.Login = login != null ? login.U8Login : null;
        //    filterargs.IsWebFilter = false;
        //    filterargs.ViewID = this._viewId;
        //    if (login != null)
        //    {
        //        filterargs.UserID = login.UserID;
        //        filterargs.LangID = login.LocaleID;
        //        filterargs.AccID = login.cAccId;
        //        filterargs.UserToken = login.UserToken;
        //    }
        //    filterargs.DataSource.FilterString = this._customFilterString;
        //    object result = SqlHelper.ExecuteDataSet(ClientReportContext.Login.UfMetaCnnString,
        //                                            string.Format(
        //                                                "SELECT * FROM uap_report  WHERE ID = '{0}'",
        //                                                this._reportId));
        //    var dataSet = result as DataSet;
        //    if (dataSet != null)
        //    {
        //        var dt = dataSet.Tables[0];
        //        if (dt != null)
        //        {
        //            _className = dt.Rows[0]["ClassName"].ToString();
        //            _filterClass = dt.Rows[0]["FilterClass"].ToString();
        //            _filterId = dt.Rows[0]["FilterID"].ToString();
        //            filterargs.InitClass(null, _filterId, _filterClass, _className);
        //        }
        //    }
        //    //filterargs.DataSource.FilterString = " 1=1   And ((dDate >= N'2013-11-01') And (dDate <= N'2013-11-30'))";
        //    //"select top 1 name from tempdb..sysobjects where name='{0}' and xtype='U'", e.DataSource.SQL.Replace("tempdb..", "")))};
        //    //if (authstring != null)
        //    //    _filterargs.Args.Add("AuthString", authstring);
        //    //if (ciyear != null && ciyear != "")
        //    //    _filterargs.Args.Add("CIYear", ciyear);
        //    //if (!string.IsNullOrEmpty(curdate))
        //    //    _filterargs.Args.Add("CurDate", curdate);
        //    //if (_reportstate == ReportStates.Preview || bWebOrOutU8)//ReportStates.WebBrowse
        //    //    _filterargs.Args.Add("previeworweb", "1");
        //}

        ///// <summary>
        ///// 获取filterFlag
        ///// </summary>
        ///// <returns></returns>
        //private string GetfilterFlag()
        //{
        //    string commandText = string.Format("select ViewType  from UAP_ReportView  where ID=@ViewID", this._viewId);
        //    SqlCommand command = new SqlCommand(commandText) { CommandType = CommandType.Text };
        //    command.Parameters.Add("@ViewID", SqlDbType.NVarChar, 100, this._viewId);
        //    SqlDataReader sdr = SqlHelper.ExecuteReader(this._loginInfo.UfMetaCnnString, command);
        //    int type = Convert.ToInt32(sdr.ToString());
        //    var data = new XElement("Data");
        //    data.Add(new XAttribute("ViewID", this._viewId));
        //    data.Add(new XAttribute("ViewType", type));
        //    //data.Add(new XAttribute("Name", filterView.ViewName));
        //    data.Add(new XAttribute("GroupID", this._groupId));
        //    data.Add(new XAttribute("reportpagerows", this._pageRowsCount));
        //    return data.ToString();
        //}
        ///// <summary>
        ///// 将客户端传递过来的jason串转换为习文过滤对象需要的字符串
        ///// </summary>
        ///// <param name="objfilter">习文过滤对象</param>
        ///// <param name="parameters">客户端传入的jason串</param>
        ///// <returns>习文过滤对象返回的结果</returns>
        //private string FillCustomFilterString(FilterSrv objfilter, Dictionary<string, string> parameters)
        //{
        //    string parameter = @"<root></root>";
        //    if (parameters.Keys.Contains("queryParams"))
        //    {
        //        parameter = "<root>" + parameters["queryParams"] + "</root>";
        //    }
        //    var xmlDocument = new XmlDocument();
        //    xmlDocument.LoadXml(parameter);
        //    // 这里需要将前台传入的字符串赋值到习文过滤控件中
        //    var doc = xmlDocument.DocumentElement;
        //    if (doc == null)
        //    {
        //        return string.Empty;
        //    }
        //    foreach (XmlNode node in doc.ChildNodes)
        //    {
        //        try
        //        {
        //            if (!(node.HasChildNodes && node.ChildNodes.Count > 1))
        //            {
        //                string nodeName = this.ConvertStr(node.Name);
        //                objfilter.FilterList[nodeName].varValue = node.InnerText;
        //            }
        //            else
        //            {
        //                foreach (XmlNode childnode in node.ChildNodes)
        //                {
        //                    string nodeName = this.ConvertStr(node.Name);
        //                    if (childnode.Name == "start")
        //                    {
        //                        objfilter.FilterList[nodeName].varValue = childnode.InnerText;
        //                    }
        //                    else if (childnode.Name == "end")
        //                    {
        //                        objfilter.FilterList[nodeName].varValue2 = childnode.InnerText;
        //                    }
        //                }
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return objfilter.GetSQLWhere();
        //}

        ///// <summary>
        ///// 对前台传递过来的包含专业字符的nodename进行反转义
        ///// </summary>
        ///// <param name="p">前台传递过来的包含转义字符的字符串</param>
        ///// <returns>实际值</returns>
        //private string ConvertStr(string str)
        //{
        //    str = str.Replace("SDOT", ".");
        //    str = str.Replace("SLBRACKET", "[");
        //    str = str.Replace("SRBRACKET", "]");
        //    return str;
        //}

        #endregion
    }
}
