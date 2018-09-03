using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using UFGeneralFilter;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportFilterService;
using FilterSrv = UFGeneralFilter.FilterSrv;
using UFGeneralFilterSrv;
using UFIDA.U8.MERP.MerpContext;
using U8Login;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class MobileReportEngine
    {
        private U8LoginInfor _loginInfo;
        private string _solutionId;
        private string _className;
        private string _filterClass;
        private string _filterId;
        private string _reportId;
        private string _viewId;
        private string _baseTableName;
        private FilterArgs _filter;
        private string _customFilterString;
        private MobileReport _mobileReport;
        // 和VIEW相关
        // 写方法获得
        private string _filterFlag;
        private string _groupId;
        /// <summary>
        /// 每页显示行数
        /// </summary>
        private int _pageRowsCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="token"></param>
        /// <param name="actionType"></param>
        /// <param name="parameters"></param>
        /// <param name="responseXml"></param>
        public MobileReportEngine(string token, string actionType, Dictionary<string, string> parameters,
                                  ref string responseXml)
        {
            this.Init(token, actionType, parameters, ref responseXml);
        }

        public MobileReportEngine(U8LoginInfor login, string actionType, Dictionary<string, string> parameters,
                                  ref string responseXml)
        {
            this._loginInfo = login;
            this._solutionId = this.GetInformationByKey("solutionId", parameters);
            this.Init2();
        }

        /// <summary>
        /// 移动报表下载调用
        /// </summary>
        /// <param name="task">移动报表下载任务</param>
        public MobileReportEngine(MobileReportDownloadTask task, U8LoginInfor loginInfor = null)
        {
            this._loginInfo = loginInfor;
            this.Init(task);
        }
        /// <summary>
        /// 通过传入参数初始化查询报表需要参数
        /// </summary>
        private void Init(string token, string actionType, Dictionary<string, string> parameters, ref string responseXml)
        {
            this._loginInfo = TokenTransfer.GetLoginInfo(token);
            ClientReportContext.Login = this._loginInfo;
            this._solutionId = this.GetInformationByKey("solutionId", parameters);
            this.Init2();
        }


        /// <summary>
        /// 移动报表下载使用，通过用户名密码初始化登陆对象
        /// </summary>
        /// <param name="task"></param>
        private void Init(MobileReportDownloadTask task)
        {
            this._solutionId = task.SolutionId;
            this.Init2();
        }

        /// <summary>
        /// 初始化参数2，获取除login和solutionId之外的参数
        /// </summary>
        private void Init2()
        {
            string commandText = string.Format(@"SELECT * FROM flt_Solution WHERE ID = '{0}'", _solutionId);
            object objSolution = SqlHelper.ExecuteDataSet(_loginInfo.UfDataCnnString, commandText);
            var ds = objSolution as DataSet;
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                this._groupId = dt.Rows[0]["GroupId"].ToString();
                this._pageRowsCount = Convert.ToInt32(dt.Rows[0]["PageRowCount"].ToString());
                this._viewId = dt.Rows[0]["ViewId"].ToString();
            }
            object result = SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString,
                                                    string.Format(@"SELECT  a.ID AS ReportId,b.ID AS ReportViewId FROM UAP_Report a 
                                                        LEFT JOIN UAP_ReportView b ON a.ID = b.ReportID WHERE
                                                        b.ID = '{0}'", _viewId));
            var dataSet = result as DataSet;
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                this._reportId = dt.Rows[0]["ReportId"].ToString();
                this._viewId = dt.Rows[0]["ReportViewId"].ToString();
            }
            string root = this.GetU8Path();
            DefaultConfigs.CachePath = Path.Combine(root, @"SLReportEngine\Cache");
            //若用户未购买BS报表，则可能不存在该path这里生成一下,代码走查点修改
            if (!Directory.Exists(DefaultConfigs.CachePath))
            {
                Directory.CreateDirectory(DefaultConfigs.CachePath);
            }
        }
        /// <summary>
        /// 下载时使用，获取全部报表数据
        /// </summary>
        /// <param name="o"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public MobileReport GetAllReport(Dictionary<string, string> parameters = null, string condition = null)
        {
            MobileReport report = this.OpenReport(parameters, condition, true);
            return report;
        }

        /// <summary>
        /// 打开一张报表
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filterString">下载时使用，直接传入过滤字符串</param>
        /// <param name="isGetAllDate">是否获取所有报表数据，为true则为被下载时调用</param>
        /// <returns></returns>
        public MobileReport OpenReport(Dictionary<string, string> parameters = null, string filterString = null, bool isGetAllDate = false)
        {
            var time = DateTime.Now;
            #region 1.LoadFormat 读取报表结构
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->LoadFormat TaskID: " + _loginInfo.TaskID + "  Start:" + time.ToString());
            var reportEngine = new ReportEngine(this._loginInfo, ReportStates.Browse);
            //reportEngine.LoadFormat(null, this._viewId, null, null, null, null, null);
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->LoadFormat TaskID: " + _loginInfo.TaskID + "  End:" + System.DateTime.Now.ToString());
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->LoadFormat TaskID: " + _loginInfo.TaskID + "  Use Time " + (DateTime.Now - time).ToString());

            #endregion 1.LoadFormat 读取报表结构


            #region 2.GetSql 与习文过滤交互，获取过滤对象

            time = System.DateTime.Now;
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetSql TaskID: " + _loginInfo.TaskID + "  Start:" + time.ToString());
            // 调用习文的FilterSrv方法，构建FilterArgs的RawFilter部分
            FilterSrv objfilter = new FilterSrvClass();
            object err = null;
            objfilter.InitSolutionID = _solutionId;
            // 构建Filter对象_filter
            _filter = new FilterArgs(this._reportId, this._filterClass, this._loginInfo);
            this.InitFilter(ref _filter, this._loginInfo);
            //if (!isGetAllDate)
            //    this.InitFilter2(ref _filter, this._loginInfo);
            _filter.RawFilter = objfilter;
            var filter1 = _filter.RawFilter as FilterSrv;
            // 创建业务组的自定义行为对象，传给过滤控件
            if (!string.IsNullOrEmpty(_filter.FilterClass))
            {
                var oComObj = Activator.CreateInstance(
                        Type.GetTypeFromProgID(_filter.FilterClass));
                filter1.BehaviorObject = oComObj;
            }
            filter1.FilterArgs.Add(_filter.Args["ufreportarg"], "ufreportarg");
            bool flag = filter1.OpenFilter(this._loginInfo.U8Login, _filterId, null, null, ref err, true);
            if (flag)
            {
                UFGeneralFilter.FilterItemsClass filterItems = filter1.FilterList as FilterItemsClass;
                for (int i = 1; i < filterItems.Count + 1; i++)
                {
                    ReportFilterService.FilterItem item = new ReportFilterService.FilterItem(filterItems[i].Key, filterItems[i].varValue);
                    _filter.FltDAESrv.Add(item);
                    _filter.FltSrv.Add(item);
                    if (!String.IsNullOrEmpty(filterItems[i].DAEexpression1))
                    {
                        ReportFilterService.FilterItem item1 = new ReportFilterService.FilterItem(filterItems[i].DAEexpression1, filterItems[i].varValue);
                        _filter.FltDAESrv.Add(item1);
                        _filter.FltSrv.Add(item);
                    }
                }
                //1.初始化一个reportEngine对象
                if (parameters == null) //下载时逻辑，直接传入
                    _filter.DataSource.FilterString = this.FillCustomFilterString(objfilter, filterString);
                else //界面打开时
                {
                    _filter.ViewID = this._viewId;
                    _filter.DataSource.FilterString = this.FillCustomFilterString(objfilter, parameters);
                    //_filter.DataSource.FilterString = JsonTransfer.VTransfer(_filter.DataSource.FilterString);
                    //_filter.DataSource.FilterString = "1=1";
                }
            }
            else
            {
                string errorInfo = err.ToString();
                throw new InvalidEnumArgumentException("该报表有必输过滤项目没有设置默认值，请设置默认值重新查询！" + errorInfo);
            }

            // 从习文的过滤对象中取得值赋值给报表过滤对象
            this.InitFilterArgsAfterOpenFilter(ref _filter, filter1.InitSolutionID);
            #endregion 2.GetSql 与习文过滤交互，获取过滤对象
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetSql TaskID: " + _loginInfo.TaskID + "  End:" + System.DateTime.Now.ToString());
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->GetSql TaskID: " + _loginInfo.TaskID + "  Use Time " + (DateTime.Now - time).ToString());

            #region 3.OpenReport 调用业务组getSql方法获取数据源，打开一张报表
            time = System.DateTime.Now;
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->OpenReport TaskID: " + _loginInfo.TaskID + "  Start:" + time.ToString());
            var engineAdapter = new MobileReportEngineAdapter(this._loginInfo);
            //这里有个GetSql（）方法。
            this._filterFlag = this.GetfilterFlag();

            engineAdapter.OpenReport(_filter, _filterFlag, null, null, null, _baseTableName);

            // 参照BS的GetMessage,循环调用拉取数据
            this._mobileReport = engineAdapter.GetReport();

            // 如果是第一次打开新的报表

            while (!this._mobileReport.PageEnd)
            {
                engineAdapter.GetMessage();
            }
            this._mobileReport = engineAdapter.GetReport();

            if (isGetAllDate)// 如果是下载全部报表数据,这里采用pageTo方法获取所有报表数据.
            {
                this._mobileReport.SemiRows = new SemiRows();
                var mobileReport = this.PageTo(this._mobileReport.Report.CacheID, -1, -1);
                this._mobileReport.SemiRows = mobileReport.SemiRows;
            }
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->OpenReport TaskID: " + _loginInfo.TaskID + "  End:" + System.DateTime.Now.ToString());
            System.Diagnostics.Trace.WriteLine("<<<<<<<<<<MobileReportTest>>>>>>>>>>OpenReport-->OpenReport TaskID: " + _loginInfo.TaskID + "  Use Time " + (DateTime.Now - time).ToString());
            return this._mobileReport;

            #endregion 3.OpenReport 调用业务组getSql方法获取数据源，打开一张报表
        }

        /// <summary>
        /// 从数据库中获取习文存储的业务组预置参数信息
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="solutionId"></param>
        /// <returns></returns>
        private bool InitFilterArgsAfterOpenFilter(ref FilterArgs filter, string solutionId)
        {
            try
            {
                string sql = string.Format("select RunData FROM dbo.flt_Solution where id = '{0}'", solutionId);
                var filterString = SqlHelper.ExecuteScalar(this._loginInfo.UfDataCnnString, sql).ToString();
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(filterString);
                var doc = xmlDocument.DocumentElement;
                var nodelist = doc.SelectNodes("/rundata/args/arg");
                if (nodelist != null)
                {
                    foreach (XmlNode node in nodelist)
                        _filter.Args.Add(node.Attributes["key"].Value, node.Attributes["value"].Value);
                    }
                nodelist = doc.SelectNodes("/rundata/items/item");
                if (nodelist != null)
                {
                    foreach (XmlNode node in nodelist)
                        _filter.Args.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        /// <summary>
        /// CS端有界面打开习文过滤时需要添加以下参数，这里先加上，可能没用。
        /// </summary>
        /// <param name="filterargs"></param>
        /// <param name="login"></param>
        private void InitFilter2(ref FilterArgs filterargs, U8LoginInfor login)
        {
            //if (login.Year.ToString() != null && login.Year.ToString() != "")
            //    filterargs.Args.Add("CIYear", login.Year.ToString());
            //if (!string.IsNullOrEmpty(login.Date))
            //    filterargs.Args.Add("CurDate", login.Date);
            if (!filterargs.Args.Contains("ufreportarg"))
            {
                #region init filterargs
                string metastring = null;
                string datastring = null;
                string tempstring = null;
                //RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
                var rdh = new RemoteDataHelper();
                ReportRelateInfor rri = rdh.GetReportInfor(ClientReportContext.Login, filterargs.ReportID, ref metastring, ref datastring, ref tempstring);

                if (rri.Views.Count == 0)
                    throw new ResourceReportException("U8.UAP.Services.ReportElements.RemoteDataEngine.没有权限");

                ClientReportContext.Login.UfMetaCnnString = metastring;
                ClientReportContext.Login.UfDataCnnString = datastring;
                ClientReportContext.Login.TempDBCnnString = tempstring;
                var SimpleViews = rri.Views;
                filterargs.InitClass(rri.RootReportId, rri.FilterId, rri.FilterClass, rri.ClassName);
                //_rootReportId = rri.RootReportId;

                filterargs.Args.Add("filterxml", rri.FilterXML);
                StringBuilder sb = new StringBuilder();
                SimpleView sv = null;
                if (string.IsNullOrEmpty(rri.FilterId) && string.IsNullOrEmpty(rri.FilterClass))
                {
                    string vid = filterargs.ViewID ?? SimpleViews.DefaultView.ID;
                    //string gid = _context.Report != null ? _context.Report.CurrentSchemaID : "";
                    string gid = string.Empty;
                    sb.Append("<Data");
                    sb.Append(" ViewID=\"");
                    sb.Append(vid);
                    sb.Append("\" GroupID=\"");
                    sb.Append(gid ?? "");
                    sb.Append("\" dispdetail=\"");
                    sb.Append("1");
                    sb.Append("\">");
                }
                else
                    sb.Append("<Data>");
                sb.Append("<Views>");
                for (int i = 0; i < SimpleViews.Count; i++)
                {
                    sv = SimpleViews[i];
                    sb.Append("<View id=\"");
                    sb.Append(sv.ID);
                    sb.Append("\" ViewType=\"");
                    sb.Append(sv.ViewType);
                    sb.Append("\" Name=\"");
                    sb.Append(getXmlStr(sv.Name));
                    sb.Append("\" ViewClass=\"");
                    sb.Append(sv.ViewClass);
                    sb.Append("\" bShowDetail=\"");
                    sb.Append(sv.bShowDetail ? "True" : "False");
                    sb.Append("\" RowsCount=\"");
                    sb.Append(sv.RowsCount.ToString());
                    sb.Append("\" bDefault=\"");
                    sb.Append(sv.IsDefault ? "True" : "False");
                    sb.Append("\">");
                    sb.Append("</View>");
                }
                sb.Append("</Views>");
                sb.Append("</Data>");

                filterargs.Args.Add("ufreportarg", sb.ToString());

                #endregion
            }
        }
        private string getXmlStr(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\"", "&quot;");
            str = str.Replace("'", "&apos;");
            return str;
        }

        /// <summary>
        /// 翻页操作
        /// </summary>
        /// <param name="cacheid"></param>
        /// <param name="pageindex">从0开始</param>
        /// <param name="lastindex">=-1，自己处理</param>
        public MobileReport PageTo(string cacheid, int pageindex, int lastindex)
        {
            try
            {
                var enginAdapter = new MobileReportEngineAdapter(this._loginInfo);
                enginAdapter.PageTo(cacheid, pageindex, lastindex);
                while (!enginAdapter.GetReport().ReportDataEnd)
                {
                    enginAdapter.GetMessage();
                }
                return enginAdapter.GetReport();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #region 获取U8安装路径
        /// <summary>
        /// 通过注册表获取U8安装路径
        /// </summary>
        /// <returns>U8安装路径</returns>
        private string GetU8Path()
        {
            string subKey = @"Software\UFSoft\WF\V8.700\Install\CurrentInstPath", sValue = "";
            try
            {
                using (var key = Registry.LocalMachine)
                {
                    using (RegistryKey key2 = key.OpenSubKey(subKey))
                    {
                        if ((key2 != null) && (key2.GetValue(sValue) != null))
                        {
                            return (string)key2.GetValue(sValue);
                        }
                    }
                }
                return GetInstallPath();
            }
            catch
            {
                return GetInstallPath();
            }

        }
        private string GetInstallPath()
        {
            string referencepath = Path.GetDirectoryName(Application.ExecutablePath);
            if (referencepath != null && !referencepath.EndsWith(@"\"))
                referencepath += @"\";

            if (referencepath.ToLower().EndsWith(@"\uap\"))
            {
                //
                // 如果是从uap目录下执行 去掉路径中的"uap\"
                //
                referencepath.Remove(referencepath.Length - 4);
            }
            return referencepath;
        }
        #endregion 获取U8安装路径

        private void InitFilter(ref FilterArgs filterargs, U8LoginInfor login)
        {
            filterargs.Login = login != null ? login.U8Login : null;
            filterargs.IsWebFilter = false;
            filterargs.ViewID = this._viewId;
            if (login != null)
            {
                filterargs.UserID = login.UserID;
                filterargs.LangID = login.LocaleID;
                filterargs.AccID = login.cAccId;
                filterargs.UserToken = login.UserToken;
            }
            filterargs.DataSource.FilterString = this._customFilterString;
            object result = SqlHelper.ExecuteDataSet(login.UfMetaCnnString,
                                                    string.Format(
                                                        "SELECT * FROM uap_report  WHERE ID = '{0}'",
                                                        this._reportId));
            var dataSet = result as DataSet;
            if (dataSet != null)
            {
                var dt = dataSet.Tables[0];
                if (dt != null)
                {
                    _className = dt.Rows[0]["ClassName"].ToString();
                    _filterClass = dt.Rows[0]["FilterClass"].ToString();
                    _filterId = dt.Rows[0]["FilterID"].ToString();
                    filterargs.InitClass(null, _filterId, _filterClass, _className);
                }
            }
        }

        /// <summary>
        /// 将客户端传递过来的jason串转换为习文过滤对象需要的字符串
        /// </summary>
        /// <param name="objfilter">习文过滤对象</param>
        /// <param name="parameters">客户端传入的jason串</param>
        /// <returns>习文过滤对象返回的结果</returns>
        private string FillCustomFilterString(FilterSrv objfilter, Dictionary<string, string> parameters)
        {
            string filterString = string.Empty;
            if (parameters.Keys.Contains("queryParams"))
            {
                filterString = parameters["queryParams"];
                if (string.IsNullOrEmpty(filterString))
                    return objfilter.GetSQLWhere();
            }
            else
            {
                return objfilter.GetSQLWhere();
            }
            return this.FillCustomFilterString(objfilter, filterString);
        }

        /// <summary>
        /// 重载方法，用以解析下载时传入的参数
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private string FillCustomFilterString(FilterSrv objfilter, string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return string.Empty;
            }
            string parameter;
            try
            {
                var doc1 = JsonTransfer.Json2Xml(filterString);
                parameter = doc1.SelectSingleNode("root").OuterXml;
            }
            catch
            {
                parameter = "<root>" + filterString + "</root>";
            }
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(parameter);
            // 这里需要将前台传入的字符串赋值到习文过滤控件中
            var doc = xmlDocument.DocumentElement;
            if (doc == null)
            {
                return string.Empty;
            }
            foreach (XmlNode node in doc.ChildNodes)
            {
                try
                {
                    if (!(node.HasChildNodes && node.ChildNodes.Count > 1))
                    {
                        string nodeName = this.ConvertStr(node.Name);
                        nodeName = JsonTransfer.VTransfer(nodeName);
                        objfilter.FilterList[nodeName].varValue = node.InnerText;
                        ReportFilterService.FilterItem item = new ReportFilterService.FilterItem(nodeName, node.InnerText);
                        _filter.FltDAESrv.Add(item);
                        _filter.FltSrv.Add(item);
                    }
                    else
                    {
                        foreach (XmlNode childnode in node.ChildNodes)
                        {
                            string varValue1 = "", varValue2 = "";
                            string nodeName = this.ConvertStr(node.Name);
                            nodeName = JsonTransfer.VTransfer(nodeName);
                            if (childnode.Name == "start")
                            {
                                objfilter.FilterList[nodeName].varValue = childnode.InnerText;
                                varValue1 = childnode.InnerText;
                            }
                            else if (childnode.Name == "end")
                            {
                                objfilter.FilterList[nodeName].varValue2 = childnode.InnerText;
                                varValue2 = childnode.InnerText;
                            }
                            ReportFilterService.FilterItem item = new ReportFilterService.FilterItem(nodeName, varValue1, varValue2);
                            _filter.FltSrv.Add(item);
                            _filter.FltDAESrv.Add(item);
                        }
                    }
                }
                catch
                {
                }
            }
            return objfilter.GetSQLWhere();
        }

        /// <summary>
        /// 对前台传递过来的包含专业字符的nodename进行反转义
        /// </summary>
        /// <param name="p">前台传递过来的包含转义字符的字符串</param>
        /// <returns>实际值</returns>
        private string ConvertStr(string str)
        {
            str = str.Replace("SDOT", ".");
            str = str.Replace("SLBRACKET", "[");
            str = str.Replace("SRBRACKET", "]");
            str = str.Replace("SNODE", "");
            str = str.Replace("SSPACE", " ");
            return str;
        }
        /// <summary>
        /// 获取filterFlag
        /// </summary>
        /// <returns></returns>
        private string GetfilterFlag()
        {
            string commandText = string.Format("select ViewType,GroupSchemas,PreservedField  from UAP_ReportView  where ID=@ViewID");
            SqlCommand command = new SqlCommand(commandText) { CommandType = CommandType.Text };
            command.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, this._viewId));
            var ds = SqlHelper.ExecuteDataSet(this._loginInfo.UfMetaCnnString, command);
            int type = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            string groupSchemas = ds.Tables[0].Rows[0]["GroupSchemas"].ToString();
            string preservedField = ds.Tables[0].Rows[0]["PreservedField"].ToString();
            var groupSchema = GroupSchemas.GetSchemas(groupSchemas, this._loginInfo.LocaleID);
            var crossSchema = GroupSchemas.GetSchemas(preservedField, this._loginInfo.LocaleID);
            var data = new XElement("Data");
            data.Add(new XAttribute("ViewID", this._viewId));
            data.Add(new XAttribute("ViewType", type));
            if (groupSchema.Contains(this._groupId))
            {
                data.Add(new XAttribute("GroupID", this._groupId));
            }
            if (crossSchema.Contains(this._groupId))
            {
                data.Add(new XAttribute("CrossID", this._groupId));
            }
            data.Add(new XAttribute("reportpagerows", this._pageRowsCount));
            return data.ToString();
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

        public int GetReportPageRowCount()
        {
            return this._pageRowsCount;
        }

        public bool SetReportPageRowCount(int value)
        {
            this._pageRowsCount = value;
            return true;
        }

        public ChartSchemas InitMobileReportChartSchema(Report report)
        {
            string _chartstrings = "";
            ChartSchemas _chartschemas = null;

            if (!ClientReportContext.bInServerProcess && string.IsNullOrEmpty(report.ChartStrings))
            {
                RemoteDataHelper rdh = new RemoteDataHelper();

                _chartstrings = rdh.LoadChartStrings(this._loginInfo.UfMetaCnnString, report.ViewID);
            }
            if (_chartstrings != "")
                _chartschemas = ChartSchemas.FromStrings(_chartstrings);
            else
                _chartschemas = new ChartSchemas();

            string id = null;
            var bFree = report.Type == ReportType.FreeReport || report.Type == ReportType.IndicatorReport || report.Type == ReportType.MetrixReport;
            if (bFree)
            {
                id = "freeid";
                if (!_chartschemas.Contains(id))
                {
                    id = report.CurrentSchemaID;
                    if (string.IsNullOrEmpty(id))
                        id = "freeid";
                }
            }
            else
            {
                id = report.CurrentSchemaID;
                if (string.IsNullOrEmpty(id))
                    id = "freeid";
            }

            _chartschemas.SetCurrentGroupChart(id, report.GroupLevels);
            report.ChartSchemas = _chartschemas;
            return _chartschemas;
        }
    }
}

