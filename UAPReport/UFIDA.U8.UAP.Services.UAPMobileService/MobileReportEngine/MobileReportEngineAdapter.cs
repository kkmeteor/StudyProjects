using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
//using UFIDA.U8.Report.EngineAdpter;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.UAPMobileService;

namespace UFIDA.U8.UAP.Services.UAPMobileService
{
    public class MobileReportEngineAdapter
    {
        private U8LoginInfor _login;
        SemiRowsContainerOnServer _datacontainer;
        private MobileReport _mobileReport = new MobileReport();
        RemoteDataHelper _rdh = null;

        //缓存视图类型，减少数据库交互
        private static Dictionary<string, int> viewIdVsTypeDic = new Dictionary<string, int>();

        public MobileReport GetReport()
        {
            return this._mobileReport;
        }

        public MobileReport GetAllReport()
        {
            return null;
        }

        /// <summary>
        /// 参照BS读数据方法，循环调用GetMessage方法
        /// 第一次获取报表report对象，分批次读取semirows
        /// </summary>
        /// <returns></returns>
        public object GetMessage()
        {
            this._mobileReport.ReportDataEnd = false;
            object message = _datacontainer.GetObject();
            if (message == null)
                return null;
            if (message is Report)
            {
                var report = message as Report;
                this._mobileReport.Report = report;
            }
            else if (message is SemiRows)
            {
                var semirows = message as SemiRows;
                if (this._mobileReport.SemiRows == null)
                    this._mobileReport.SemiRows = new SemiRows();
                foreach (SemiRow semiRow in semirows)
                {
                    this._mobileReport.SemiRows.Add(semiRow);
                }
            }
            else if (message.ToString() == "")
            {
                this._mobileReport.PageEnd = true;
                this._mobileReport.ReportDataEnd = true;
            }
            else if (message.ToString() == "WAIT")
            {
            }
            return this._mobileReport;
        }

        public MobileReportEngineAdapter(U8LoginInfor login)
        {
            this._login = login;
            _datacontainer = new SemiRowsContainerOnServer();
            _rdh = new RemoteDataHelper();
        }

        internal void OpenReport(FilterArgs filter, string filterFlag, string[] dynamicAddColumns, byte[] rf, string cacheId = null, string rawTable = null)
        {
            string groupId = null;
            string crossId = null;
            GetGroupAndCrossId(filterFlag, ref  groupId, ref crossId);

            #region 动态列处理的

            RuntimeFormat runtimeFormat = GetRunTimeFormatByDynamicColumn(dynamicAddColumns);

            if (rf != null)
            {
                if (runtimeFormat == null)
                    runtimeFormat = new RuntimeFormat();
                //var rff = SLElements.BinarySerializHelper.ObjectFromBinary<SLElements.RuntimeFormat>(rf);
                //if (rff.SummaryCols != null && rff.SummaryCols.Count > 0)
                //{
                //    IConfigXmlItem cols = runtimeFormat.GetSubItem("SummaryCols");
                //    if (cols == null)
                //    {
                //        cols = new RuntimeFormatInfo();
                //        cols.SetProperty(ConfigXmlContext.XmlKeyId, "SummaryCols");
                //        runtimeFormat.SetSubItem(cols);
                //    }
                //    foreach (var pair in rff.SummaryCols)
                //    {
                //        IConfigXmlItem config = new RuntimeFormatInfo();
                //        config.SetProperty(ConfigXmlContext.XmlKeyId, pair.Key);
                //        config.SetProperty(RuntimeFormatServerContext.XmlKeyOperatorType, pair.Value);
                //        cols.SetSubItem(config);
                //    }
                //}
            }

            #endregion

            //加载报表的样式
            //_reportEngine.LoadFormat(cacheId, slFilterArgs.ViewID, fileterFlag, groupId, null, rawtable, runtimeFormat);
            //添加FilterArgs的一些属性
            filter.Args.Add("NetDataConnString", _login.UfDataCnnString);
            filter.Args.Add("netmetaconnstring", _login.UfMetaCnnString);
            filter.Args.Add("curdate", _login.Date);
            filter.Args.Add("ciyear", _login.cYear);
            filter.AccID = _login.cAccId;
            filter.LangID = _login.LocaleID;
            if (string.IsNullOrEmpty(rawTable))
                filter = GetSql(filter);
            else
            {
                filter.DataSource.SQL = rawTable;
            }
            int viewType = GetViewTypeByViewId(filter.ViewID);
            if (viewType == 3)//交叉视图
            {
                InnerOpenCrossReport(cacheId, filter.ViewID, filterFlag, groupId, rawTable, runtimeFormat, filter, crossId);
            }
            else
            {
                InnerOpenReport(cacheId, filter.ViewID, filterFlag, groupId, rawTable, runtimeFormat, filter, crossId);
            }
        }
        private FilterArgs GetSql(FilterArgs args)
        {
            #region CS
            //RemoteDataHelper rdh = null;
            //if (args != null && !string.IsNullOrEmpty(args.ClassName.Trim()))
            //{
            //    if (args.DataSource.Type == CustomDataSourceTypeEnum.TemplateTable)
            //        args.DataSource.SQL = CustomDataSource.GetATableName();
            //    if (_context.ReportState == ReportStates.WebBrowse)
            //    {
            //        rdh = DefaultConfigs.GetRemoteHelper();
            //        args = rdh.GetSql(_context.UserToken, _context.TaskID, _context.SubID, _context.FilterArgs);
            //    }
            //    else
            //    {
            //        rdh = new RemoteDataHelper();
            //        ClientReportContext.Login = _login;
            //        rdh.GetSql(args);
            //    }
            //}
            #endregion

            var rdHelper = new RemoteDataHelper();
            //return rdHelper.GetSql(_login.UserToken, _login.TaskID, _login.SubID, args);
            if (args.DataSource.Type == CustomDataSourceTypeEnum.TemplateTable)
            {
                //args.DataSource.SQL = CustomDataSource.GetATableNameWithTaskId(_login.TaskID);
                args.DataSource.SQL = CustomDataSource.GetATableName();
            }
            ClientReportContext.Login = _login;
            rdHelper.GetSql(args);
            return args;
        }
        /// <summary>
        /// 打开的crosstable，rawtable为NULL,
        /// basetable传入的是rawtable
        /// </summary>
        private void InnerOpenCrossReport(string cacheId, string viewId, string fileterFlag, string groupId, string rawtable, RuntimeFormat runtimeFormat, FilterArgs args, string crossId)
        {
            ReportEngine reportEngine = new ReportEngine(_login, ReportStates.Browse, _datacontainer);
            reportEngine.CrossLoadFormat(cacheId, viewId, runtimeFormat);
            string colAuthString = GetColAuthString(viewId);
            reportEngine.CreateCrossReport(true, true, fileterFlag, colAuthString, groupId, null, args, null, null, rawtable, 0, ShowStyle.Normal, crossId, false);
        }

        private void InnerOpenReport(string cacheId, string viewId, string fileterFlag, string groupId, string rawtable, RuntimeFormat runtimeFormat, FilterArgs args, string crossId)
        {
            //加载报表的样式
            ReportEngine reportEngine = new ReportEngine(_login, ReportStates.Browse, _datacontainer);
            string allcolumns = reportEngine.LoadFormat(cacheId, viewId, fileterFlag, groupId, null, rawtable, runtimeFormat);

            if (args != null)
            {
                if (!string.IsNullOrEmpty(allcolumns))
                {
                    //if (_context.Type == ReportType.IndicatorReport)
                    if (string.IsNullOrEmpty(args.ClassName))// &&! _context.FilterArgs.bAutoSource )
                    {
                        RowAuthFacade raf = new RowAuthFacade();
                        string rowauth = raf.GetRowAuthFromAllColumns(viewId, allcolumns, ClientReportContext.Login, true);
                        args.Args.Add("RowAuthString", rowauth);
                    }
                    string[] columns = Regex.Split(allcolumns, "@;@");//allcolumns.Split(new char[] { '@', ';', '@' });
                    //if( _context.FilterArgs.Args.Contains( "DataAccordingGroup" ) )
                    //{
                    if (columns.Length > 1)
                    {
                        string[] items = columns[1].Split(',');
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(items[i]))
                                args.GroupItems.Add(items[i]);
                        }
                    }
                    if (columns.Length > 2)
                    {
                        string[] items = columns[2].Split(',');
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(items[i]))
                                args.SumItems.Add(items[i]);
                        }
                    }
                    //}
                    args.DataSource.SelectString = string.IsNullOrEmpty(columns[0]) ? "" : columns[0];
                }
                else
                    args.DataSource.SelectString = "";

                args.Args.Add("columns", args.DataSource.SelectString);
            }

            string colAuthString = GetColAuthString(viewId);
            //运行报表
            reportEngine.CreateReport(true, args, null, null, null, rawtable, 0, colAuthString, ShowStyle.Normal, crossId, false);
        }
        private string GetColAuthString(string viewId)
        {
            RemoteDataHelper rdh = new RemoteDataHelper();
            string colauthstring;
            try //U易联报表获取不到taskID这里需要做一下处理
            {
                colauthstring = rdh.GetColAuthString(
                ClientReportContext.Login.UserToken,
                ClientReportContext.Login.TaskID,
                ClientReportContext.Login.SubID, viewId);
            }
            catch
            {
                colauthstring = "";
            }
            //colauthstring = rdh.GetColAuthString(ClientReportContext.Login.U8Login, viewId);
            //colauthstring = rdh.GetColAuthString(ClientReportContext.Login, viewId);
            return colauthstring;
        }

        /// <summary>
        /// 通过视图ID获取视图类型
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        private int GetViewTypeByViewId(string viewId)
        {
            lock (viewIdVsTypeDic)
            {
                if (viewIdVsTypeDic.ContainsKey(viewId))
                {
                    return viewIdVsTypeDic[viewId];
                }
                else
                {
                    string commandText = string.Format("select ViewType  from UAP_ReportView  where ID='{0}'", viewId);
                    int type = Convert.ToInt32(SqlHelper.ExecuteScalar(_login.UfMetaCnnString, commandText));
                    viewIdVsTypeDic.Add(viewId, type);
                    return type;
                }
            }
        }
        /// <summary>
        /// 根据fileterFlag，获取groupId和crossId
        /// </summary>
        /// <param name="fileterFlag"></param>
        /// <param name="groupId"></param>
        /// <param name="crossId"></param>
        private void GetGroupAndCrossId(string fileterFlag, ref string groupId, ref string crossId)
        {
            if (string.IsNullOrEmpty(fileterFlag))
            {
                return;
            }
            TextReader tr = new StringReader(fileterFlag);
            XElement elment = XElement.Load(tr);
            XAttribute groupAt = elment.Attribute("GroupID");
            if (groupAt != null)
            {
                groupId = groupAt.Value;
            }
            XAttribute crossAt = elment.Attribute("CrossID");
            if (crossAt != null)
            {
                crossId = crossAt.Value;
            }
        }

        /// <summary>
        /// 获取动态添加列
        /// </summary>
        /// <param name="dynamicAddColumns"></param>
        /// <returns></returns>
        private RuntimeFormat GetRunTimeFormatByDynamicColumn(string[] dynamicAddColumns)
        {
            RuntimeFormat runtimeFormat = null;
            if (dynamicAddColumns != null && dynamicAddColumns.Length > 0)
            {
                runtimeFormat = new RuntimeFormat();
                IConfigXmlItem addedCos = new RuntimeFormatInfo();
                addedCos.SetProperty(ConfigXmlContext.XmlKeyId, RuntimeFormatServerContext.ArgKeyDymanicAddedCols);
                runtimeFormat.SetSubItem(addedCos);
                foreach (string key in dynamicAddColumns)
                {
                    RuntimeFormatInfo rfi = new RuntimeFormatInfo();
                    rfi.SetProperty(ConfigXmlContext.XmlKeyId, key);
                    addedCos.SetSubItem(rfi);
                }
            }
            return runtimeFormat;
        }


        public void PageTo(string cacheid, int pageindex, int lastindex)
        {
            ReportEngine reportEngine = new ReportEngine(_login, ReportStates.Browse, _datacontainer);
            reportEngine.PageTo(cacheid, pageindex, lastindex, ShowStyle.NoGroupHeader);
        }
    }
}
