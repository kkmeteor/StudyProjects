using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Runtime.Remoting.Lifetime;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.ReportResource;
using System.Runtime.Remoting;
using System.Collections.Generic;
using U8Login;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public partial class ReportEngine : MarshalByRefObject
    {
        #region fields
        private Report _report;
        private ReportStates _reportstate;
        private U8LoginInfor _login;
        private DataHelper _datahelper;
        private ReportDataSource _datasource;
        private ReportHandler _handler;
        protected SimpleHashtable _expandcolumns;

        private bool _bcanceled = false;
        private SemiRowsContainerOnServer _semirowcontainer;
        private AutoResetEvent _disposeevent;
        private AutoResetEvent _cancelevent;
        private string _monitorkey = null;
        private Guid _monitorguid = Guid.Empty;
        private System.Timers.Timer _timer;
        private INIClass _iniClass;

        #endregion

        #region constructor
        public ReportEngine(U8LoginInfor login, ReportStates reportstate)
        {
            if (reportstate == ReportStates.WebBrowse)
                _login = new U8LoginInfor(ReportLoginManager.GetLogin(login.UserToken, login.TaskID, login.SubID));
            else
                _login = login;
            _reportstate = reportstate;
            if (_login != null)
                _datahelper = new DataHelper(_login, reportstate);
            _semirowcontainer = new SemiRowsContainerOnServer();
            _disposeevent = new AutoResetEvent(true);
            _cancelevent = new AutoResetEvent(true);
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
        }
        /// <summary>
        /// 该方法只为Silverlight服务端适配器调用，其他的勿调用
        /// 增加containerServer为了把数据会传给silverlight客户端
        /// </summary>
        /// <param name="login"></param>
        /// <param name="reportstate"></param>
        /// <param name="containerServer">从Silverlight服务段适配器传入的，非内部创建的</param>
        public ReportEngine(U8LoginInfor login, ReportStates reportstate, SemiRowsContainerOnServer containerServer)
        {
            if (reportstate == ReportStates.WebBrowse)
                _login = new U8LoginInfor(ReportLoginManager.GetLogin(login.UserToken, login.TaskID, login.SubID));
            else
                _login = login;
            _reportstate = reportstate;
            if (_login != null)
                _datahelper = new DataHelper(_login, reportstate);
            _semirowcontainer = new SemiRowsContainerOnServer();
            _disposeevent = new AutoResetEvent(true);
            _cancelevent = new AutoResetEvent(true);
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _semirowcontainer = containerServer;
        }


        public override object InitializeLifetimeService()
        {
            return null;
        }

        protected void SetLocaleID()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_login.LocaleID);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_login.LocaleID);
        }
        protected void SetLocaleID(string localeID)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(localeID);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(localeID);
        }
        private void CheckCanceled()
        {
            if (_bcanceled)
                throw new CanceledException();
        }

        private void NewReportHandlerOfEvent(byte[] assemblybytes, PageInfos pageinfos)
        {
            switch (_report.Type)
            {
                case ReportType.FreeReport:
                    _handler = new FreeReportHandler(_report, _login, _datahelper, _datasource, _semirowcontainer, assemblybytes, pageinfos);
                    break;
                case ReportType.IndicatorReport:
                    _handler = new IndicatorReportHandler(_report, _login, _datahelper, _datasource, _semirowcontainer, assemblybytes, pageinfos);
                    break;
                default:
                    _handler = new GridReportHandler(_report, _login, _datahelper, _datasource, _semirowcontainer, assemblybytes, pageinfos);
                    break;
            }
            _handler.AfterCross += new AfterCrossHandler(AfterCrossResponse);
            _handler.CheckCanceled += new CheckCanceledHandler(CheckCanceled);
        }
        #endregion

        #region getobject
        public object GetObject()
        {
            object o = _semirowcontainer.GetObject();
            if (o != null && o is string && o.ToString() == "")
                OnDispose();
            return o;
        }


        #region !debug
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
#if !DEBUG
            Canceled();
#endif
        }

        public void RequireFlag()
        {
            _timer.Stop();
            _timer.Start();
        }
        #endregion
        #endregion

        #region open
        //public void CreateCrossReport(bool bfilter, bool bcross, string filterflag, string authstring, string groupschemaid, ReportLevelExpand levelexpand
        //    , FilterArgs filterargs, string crosstable, string rawtable, string basetable, int levels, ShowStyle showstyle)
        //{
        //    CreateCrossReport(bfilter, bcross, filterflag, authstring, groupschemaid, levelexpand, filterargs, crosstable, rawtable, basetable, levels, showstyle, null);
        //}
        private bool bsaexpand = false;
        public void CreateCrossReport(bool bfilter, bool bcross, string filterflag, string authstring, string groupschemaid, ReportLevelExpand levelexpand
            , FilterArgs filterargs, string crosstable, string rawtable, string basetable, int levels, ShowStyle showstyle, string csid, bool showall)
        {
            try
            {
                SetLocaleID();
                //if (!bcross)
                //{
                //    if (!string.IsNullOrEmpty(crosstable))
                //        _report.CrossTable = crosstable;
                //    if (!string.IsNullOrEmpty(rawtable))
                //        _report.BaseTable = rawtable;
                //    else if (!string.IsNullOrEmpty(basetable))
                //        _report.BaseTable = basetable;
                //}
                //else
                //{
                if (!string.IsNullOrEmpty(rawtable))
                    _report.BaseTable = rawtable;
                else if (!string.IsNullOrEmpty(crosstable))
                    _report.BaseTable = crosstable;
                else if (!string.IsNullOrEmpty(basetable))
                    _report.BaseTable = basetable;
                //}
                //交叉忽略分组汇总过滤
                RemoveGroupfilter(filterargs);
                InitReportFromFilterArgs(filterargs);

                if ((bfilter && bcross) || _report.UnderState == ReportStates.Preview)
                    CreateBaseTable(filterargs, crosstable, rawtable, basetable, levels);
                else
                {
                    //RemoteDataHelper rdh = new RemoteDataHelper();
                    //if (bcross && !string.IsNullOrEmpty(basetable ))
                    //{
                    //    rdh.DropFromDB(_login.TempDBCnnString, rdh.BaseString(basetable, _report.CacheID));
                    //}
                    //if (levelexpand != null && !string.IsNullOrEmpty(rawtable))
                    //{
                    //    #region drop basetable & expand
                    //    rdh.DropFromDB(_login.TempDBCnnString, rdh.ExpandTableAndBaseString(basetable,_report.CacheID ));
                    //    #endregion
                    //}
                    //#region drop index & levels
                    //rdh.DropFromDB(_login.TempDBCnnString, rdh.IndexAndLevelsString(basetable, levels,_report.CacheID ));
                    //#endregion
                }

                AfterBaseTableCreated(_report.BaseTable, filterargs);
                InitExpand(levelexpand);
                InitGroup(filterflag, groupschemaid);

                if (CheckCrossSchema(csid))
                {
                    GenerateFormatWhenCross(_report.CurrentCrossSchema);
                    bcross = true;
                }
                //sourcefilter

                HandleSourceFilter(filterargs);
                if (showall)
                    _report.PageRecords = 0;
                _handler.OpenCrossReport(true, authstring, levelexpand, showstyle);

            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }


        //public void CreateReport(bool bfilter, FilterArgs filterargs, ReportLevelExpand levelexpand
        //    , string crosstable, string rawtable, string basetable, int levels, string authstring, ShowStyle showstyle)
        //{
        //    CreateReport(bfilter, filterargs, levelexpand, crosstable, rawtable, basetable, levels, authstring, showstyle, null);
        //}

        public void CreateReport(bool bfilter, FilterArgs filterargs, ReportLevelExpand levelexpand
            , string crosstable, string rawtable, string basetable, int levels, string authstring, ShowStyle showstyle, string csid, bool showall)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            try
            {
                //System.Threading.Thread.Sleep(10000);

                SetLocaleID();
                if (!string.IsNullOrEmpty(rawtable))
                    _report.BaseTable = rawtable;
                else if (!string.IsNullOrEmpty(crosstable))
                    _report.BaseTable = crosstable;
                else if (!string.IsNullOrEmpty(basetable))
                    _report.BaseTable = basetable;

                InitReportFromFilterArgs(filterargs);
                watch.Stop();
                System.Diagnostics.Debug.WriteLine("InitReportFromFilterArgs耗时:" + (watch.ElapsedMilliseconds));
                Console.WriteLine("InitReportFromFilterArgs耗时:" + (watch.ElapsedMilliseconds));//输出时间 毫秒
                watch.Reset();
                watch.Start();
                if (bfilter || _report.UnderState == ReportStates.Preview || _report.UnderState == ReportStates.OutU8)
                    CreateBaseTable(filterargs, null, rawtable, basetable, levels);
                else
                {
                    //#region drop index & levels
                    //RemoteDataHelper rdh = new RemoteDataHelper();
                    //rdh.DropFromDB(_login.TempDBCnnString, rdh.IndexAndLevelsString(basetable, levels,_report.CacheID ));
                    //#endregion
                }
                watch.Stop();
                System.Diagnostics.Debug.WriteLine("CreateBaseTable耗时:" + (watch.ElapsedMilliseconds));
                watch.Reset();
                watch.Start();
                AfterBaseTableCreated(_report.BaseTable, filterargs);
                watch.Stop();
                System.Diagnostics.Debug.WriteLine("AfterBaseTableCreated耗时:" + (watch.ElapsedMilliseconds));
                watch.Reset();
                watch.Start();
                InitExpand(levelexpand);
                watch.Stop();
                System.Diagnostics.Debug.WriteLine("InitExpand耗时:" + (watch.ElapsedMilliseconds));
                watch.Reset();
                watch.Start();
                if (showall)
                    _report.PageRecords = 0;
                if (CheckCrossSchema(csid))
                {
                    RemoveGroupfilter(filterargs);
                    GenerateFormatWhenCross(_report.CurrentCrossSchema);
                    HandleSourceFilter(filterargs);
                    _handler.OpenCrossReport(true, authstring, levelexpand, showstyle);
                }
                else
                {
                    //sourcefilter
                    HandleSourceFilter(filterargs);
                    //AddSortSchema();
                    _handler.OpenReport(authstring, levelexpand, showstyle);
                }
                watch.Stop();
                System.Diagnostics.Debug.WriteLine("CreateBaseTable耗时:" + (watch.ElapsedMilliseconds));
                
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
        private void RemoveGroupfilter(FilterArgs filterargs)
        {//交叉忽略分组汇总过滤
            if (filterargs != null && filterargs.Args != null)
            {
                if (filterargs.Args.Contains("groupfilter"))
                {
                    filterargs.Args.Remove("groupfilter");
                    _report.GroupFilter = string.Empty;
                }
            }
        }
        public string GetStaticReportAllColumns(string staticid)
        {
            try
            {
                SetLocaleID();
                #region read format
                ReadStaticFormat(staticid);
                #endregion
                return GenerateAllColumns(false);
            }
            catch (Exception e)
            {
                HandleError(e);
                return null;
            }
        }
        private void CheckReportQueryAuth()
        {
            clsLogin login = new clsLogin();
            login.ConstructLogin(_login.UserToken);
            login.set_TaskId(_login.TaskID);
            if (_report == null || _report.ViewID == null || string.IsNullOrEmpty(_login.TaskID))
                return;
            if (login.IsAdmin)
                return;
            RemoteDataHelper rdh = new RemoteDataHelper();
            rdh.AuthCheck(login, _report.ViewID, OperationEnum.Query);
        }
        public void OpenStaticReport(string eventfilter, string uifilter, int pageindex, bool showall, string rowauthstring)
        {
            try
            {
                CheckReportQueryAuth();//检查权限问题
                ShowStyle ss = _report.CurrentSchema.ShowStyle;
                if (pageindex == -2)
                {
                    pageindex = -1;
                    if (ss == ShowStyle.NoGroupSummary
                   && _report.CurrentSchema.SchemaItems.Count > 0)//分组折叠展现
                    {
                        ss = ShowStyle.NoGroupSummary;

                    }
                    else
                    {
                        ss = ShowStyle.NoGroupHeader;
                    }

                }
                HandleSourceFilter(rowauthstring);
                InitInformationString(eventfilter, uifilter);
                if (showall)
                    _report.PageRecords = 0;
                _handler.SetDefaultPageIndex(pageindex);
                if (_report.Type == ReportType.CrossReport)
                {
                    InitGroup(_report.CrossFltArgString, null);
                    ss = _report.CurrentSchema.ShowStyle;
                    if (ss == ShowStyle.NoGroupSummary
                       && _report.CurrentSchema.SchemaItems.Count > 0)//分组折叠展现
                    {
                        ss = ShowStyle.NoGroupSummary;

                    }
                    else
                    {
                        ss = ShowStyle.NoGroupHeader;
                    }
                    _handler.OpenCrossReport(true, _report.CrossAuthString, null, ss);
                }
                else
                    _handler.OpenReport(_report.CrossAuthString, null, ss);

            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        //public void OpenStaticReport(string staticid, string eventfilter, string uifilter)
        //{
        //    OpenStaticReport(staticid, eventfilter, uifilter, 0,false);
        //}

        //public void OpenStaticReport(string staticid)
        //{
        //    OpenStaticReport(staticid, null, null);
        //}
        #endregion

        #region loadformat
        public string StaticCrossLoadFormat(string cacheid, string viewid, string filterflag, string authstring)
        {
            SetLocaleID();
            string allcolumns = CrossLoadFormat(cacheid, viewid, null);
            _report.CrossAuthString = authstring;
            _report.CrossFltArgString = filterflag;
            return allcolumns;
        }
        public string CrossLoadFormat(string cacheid, string viewid, RuntimeFormat columnsettings)
        {
            try
            {
                SetLocaleID();
                Load(cacheid, viewid, columnsettings);
                return GenerateAllColumns(false);
            }
            catch (CanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                HandleError(e);
                return null;
            }
        }
        public string StaticLoadFormat(string viewid, string filterflag, string authstring)
        {
            SetLocaleID();
            string allcolumns = LoadFormat(null, viewid, filterflag, null, null, null, null);
            InitGroup(filterflag, null);

            _report.CrossFltArgString = filterflag;
            _report.CrossAuthString = authstring;
            return allcolumns;
        }
        public string LoadFormat(string cacheid, string viewid, string filterflag, string groupschemaid
            , string rawtable, string basetable, RuntimeFormat columnsettings)
        {

            try
            {

                SetLocaleID();
                //if (levelexpand != null && !string.IsNullOrEmpty(rawtable))
                //{
                //    #region drop basetable & expand
                //    RemoteDataHelper rdh = new RemoteDataHelper();
                //    rdh.DropFromDB(_login.TempDBCnnString, rdh.ExpandTableAndBaseString(basetable,cacheid ));
                //    #endregion
                //}

                Load(cacheid, viewid, columnsettings);
                InitGroup(filterflag, groupschemaid);

                #region return columns
                return GenerateAllColumns(true);
                #endregion
            }
            catch (CanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                HandleError(e);
                return null;
            }
        }

        private string GenerateAllColumns(bool handlesum)
        {
            SimpleArrayList allcolumns = new SimpleArrayList();
            SimpleArrayList sumcolumns = new SimpleArrayList();
            foreach (Section section in _report.Sections)
            {
                foreach (Cell cell in section.Cells)
                {
                    SetColumns(cell, allcolumns);
                    if (cell is IGridCollect)
                    {
                        if ((cell as IGridCollect).bSummary)
                            sumcolumns.Add((cell as IMapName).MapName);
                    }
                    else if (cell is ICalculator)
                        sumcolumns.Add((cell as IMapName).MapName);
                }
            }

            if (!string.IsNullOrEmpty(_report.RowFilter.FilterString))
            {
                SetScriptKey(null, null, _report.RowFilter.FilterString, allcolumns);
            }
            foreach (string key in _report.DataSources.Keys)
                allcolumns.Add(key);

            StringBuilder sb = new StringBuilder();
            //#region expandcolumns
            //if (_expandcolumns != null)
            //{
            //    foreach (string key in _expandcolumns.Keys)
            //    {
            //        string column = _expandcolumns[key].ToString();
            //        allcolumns.Add(column);
            //    }
            //}
            //#endregion
            StringBuilder sbg = new StringBuilder();
            if (handlesum)
            {
                #region group region
                int grouplevels = 0;
                if (_report.bFree)
                    grouplevels = _report.GroupLevels;
                else
                    grouplevels = _report.CurrentSchema.SchemaItems.Count;
                for (int i = 1; i <= grouplevels; i++)
                {
                    Section sec = null;
                    if (_report.Type == ReportType.FreeReport)
                    {
                        sec = _report.Sections.GetGroupHeader(i);
                        foreach (Cell cell in sec.Cells)
                        {
                            if (cell is IGroup)
                            {
                                if (sbg.Length > 0)
                                    sbg.Append(",");
                                sbg.Append((cell as IMapName).MapName);
                            }
                        }
                    }
                    else
                    {
                        sec = _report.Sections[SectionType.GridDetail];
                        if (sec != null && _report.CurrentSchema != null)
                        {
                            foreach (string cell in _report.CurrentSchema.SchemaItems[i - 1].Items)
                            {
                                Cell c = sec.Cells[cell];
                                if (c != null && c is IMapName)
                                {
                                    if (sbg.Length > 0)
                                        sbg.Append(",");
                                    sbg.Append((c as IMapName).MapName);
                                }
                                else if (_report.DataSources.Contains(cell))
                                {
                                    if (sbg.Length > 0)
                                        sbg.Append(",");
                                    sbg.Append(cell);
                                    allcolumns.Add(cell);
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            foreach (string key in allcolumns)
            {
                //if (_expandcolumns != null && _expandcolumns.Contains(key.Trim()))
                //    continue;
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append("[");
                sb.Append(key);
                sb.Append("]");
            }
            if (handlesum)
            {
                sb.Append("@;@");

                sb.Append(sbg.ToString());

                sb.Append("@;@");
                StringBuilder sbs = new StringBuilder();
                foreach (string key in sumcolumns)
                {
                    if (sbs.Length > 0)
                        sbs.Append(",");
                    sbs.Append(key);
                }
                sb.Append(sbs.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 以前选择无分组，则走默认的交叉方案
        /// 现在选择无分组就是走无分组
        /// 默认选择手工设置的默认交叉方案，如果没有，则走手工设置的分组方案
        /// </summary>
        /// <param name="csid"></param>
        /// <returns></returns>
        private bool CheckCrossSchema(string csid)
        {
            //if (string.IsNullOrEmpty(csid) && !_report.CurrentSchema.bNoneGroup)
            //    return false;
            //if (string.IsNullOrEmpty(csid) && _report.CrossSchemas.Default!=null)
            //{
            //    csid = _report.CrossSchemas.Default.ID;                
            //}
            //if (!string.IsNullOrEmpty(csid) && csid != "00000000-0000-0000-0000-000000000001")
            //{
            //    _report.CurrentCrossID = csid;
            //    return true;
            //}
            if (string.IsNullOrEmpty(csid))
                return false;
            else if (csid != "00000000-0000-0000-0000-000000000001" &&
                (_report.CrossSchemas != null && _report.CrossSchemas.Contains(csid)))//运行时恢复出厂格式会调用的
            {
                _report.CurrentCrossID = csid;
                return true;
            }
            return false;
        }

        private void GenerateFormatWhenCross(GroupSchema crossschema)
        {
            Section rowheader = null;
            Section columnheader = null;
            Section crossdetail = null;
            Cells cells = null;
            if (_report.Type == ReportType.CrossReport)
            {
                rowheader = _report.Sections[SectionType.CrossRowHeader];
                cells = rowheader.Cells;
                _report.Sections.Remove(rowheader);
                _report.Sections.Remove(_report.Sections[SectionType.CrossColumnHeader]);
                _report.Sections.Remove(_report.Sections[SectionType.CrossDetail]);
            }
            else if (_report.Type == ReportType.GridReport)
            {
                rowheader = _report.Sections[SectionType.GridDetail];
                cells = rowheader.Cells;
                _report.Sections.Remove(rowheader);
            }

            rowheader = new CrossRowHeader();
            columnheader = new CrossColumnHeader();
            crossdetail = new CrossDetail();
            _report.Sections.Add(rowheader);
            _report.Sections.Add(columnheader);
            _report.Sections.Add(crossdetail);
            _report.Type = ReportType.CrossReport;

            //_report.GroupSchemas = _report.CrossSchemas;
            _report.CurrentSchemaID = crossschema.ID;

            GroupSchemaItems gsis = crossschema.SchemaItems;
            crossschema.SchemaItems = new GroupSchemaItems();
            if (crossschema.FromCrossItem(gsis[0]) &&
                string.IsNullOrEmpty(crossschema.GuidVersion))
                crossschema.CrossRowGroup = crossschema.Clone() as GroupSchema;

            //_report.MustShowDetail = true;
            //if (crossschema.SchemaItems.Count > 0)
            //{
            //    GroupSchemaItem gsi = crossschema.SchemaItems[crossschema.SchemaItems.Count - 1];
            //    crossschema.SchemaItems.Remove(gsi);
            //}

            AddCrossCells(gsis[0].Items, rowheader, cells, crossschema.DateDimensions, crossschema.SwitchItem);
            AddCrossCells(gsis[1].Items, columnheader, cells, crossschema.DateDimensions, crossschema.SwitchItem);
            bool bshowdetail = AddCrossCells(gsis[2].Items, crossdetail, cells, null, null);

            crossschema.bShowDetail = bshowdetail;
            _report.bShowDetail = bshowdetail;
            crossschema.SchemaItems = gsis;
        }


        private bool AddCrossCells(SimpleArrayList sal, Section section, Cells cells, Dictionary<string, int> datedimensions, string switchitem)
        {
            bool bshowdetail = false;
            foreach (String s in sal)
            {
                string[] ss = System.Text.RegularExpressions.Regex.Split(s, "____");
                Cell cell = cells[ss[1]];
                Cell celltmp = null;
                if (cell != null)
                {
                    if (section is CrossColumnHeader)
                    {
                        if (cell is IDateTimeDimensionLevel && (cell as IDateTimeDimensionLevel).DDLevel != DateTimeDimensionLevel.时间 && (datedimensions == null || !datedimensions.ContainsKey(cell.Name.ToLower())))
                        {
                            celltmp = new CalculateColumnHeader(cell.Name + "__DayDimension", DateTimeDimensionHelper.GetExpressionAll((cell as IDateTimeDimensionLevel).DDLevel, (cell as IDateTimeDimensionLevel).ShowYear, (cell as IDateTimeDimensionLevel).ShowWeekRange, (cell as IMapName).MapName, _login.cAccId));
                        }
                        else if (cell is IDataSource)
                        {
                            celltmp = new ColumnHeader((cell as IDataSource).DataSource);
                            celltmp.Name = cell.Name;
                            if (cell is IDateTime)
                                (celltmp as ColumnHeader).bDateTime = true;
                        }
                        else if (cell is ICalculateColumn)
                        {
                            celltmp = new CalculateColumnHeader(cell.Name, (cell as ICalculateColumn).Expression);
                        }
                        section.Cells.AddDirectly(celltmp);
                    }
                    else
                    {
                        cell.Visible = true;
                        section.Cells.AddDirectly(cell);
                        celltmp = cell;
                    }
                }
                else
                {
                    DataSource ds = _datasource.DataSources[ss[1]];
                    if (ds != null && !ds.IsEmpty)
                    {
                        cell = section.GetDefaultRect(ds);
                        if (cell != null)
                        {
                            section.Cells.AddDirectly(cell);
                            celltmp = cell;
                        }
                    }
                }

                if (section is CrossRowHeader && cell != null && cell is IGridCollect)
                    (cell as IGridCollect).bSummary = false;

                if (datedimensions == null && cell != null && !(cell is IDecimal))
                    bshowdetail = true;

                if (section is CrossDetail && cell is IGridCollect)
                    (cell as IGridCollect).bSummary = true;

                if (datedimensions != null && celltmp != null && datedimensions.ContainsKey(celltmp.Name.ToLower()))
                {
                    if (celltmp is IBDateTime)
                        (celltmp as IBDateTime).bDateTime = true;
                    if (switchitem != null)
                        (celltmp as IDateTimeDimensionLevel).SupportSwitch = celltmp.Name.ToLower() == switchitem.ToLower();
                    DateTimeDimensionHelper.SetDateDimensionLevel(celltmp as IDateTimeDimensionLevel, datedimensions[celltmp.Name.ToLower()]);
                    //if ((celltmp as IDateTimeDimensionLevel).DDLevel != DateTimeDimensionLevel.Day)
                    //{
                    //    section.Cells.Remove(celltmp);
                    //    if(section is CrossColumnHeader)
                    //        section.Cells.AddDirectly(new CalculateColumnHeader(celltmp as IDateTimeDimensionLevel));
                    //    else
                    //        section.Cells.AddDirectly(new GridColumnExpression(celltmp as IDateTimeDimensionLevel));
                    //}
                }
            }
            return bshowdetail;
        }

        private void ReadStaticFormat(string staticid)
        {
            byte[] bs = LoadFromStatic(staticid);
            if (_report == null)
                throw new ReportException("StaticReportNotExist");
            _report.CacheID = Guid.NewGuid().ToString();
            //"UFReport.." +
            _report.BaseTable = staticid;
            _report.CrossTable = null;
            _report.RawTable = null;
            _datasource = new ReportDataSource(_report.DataSources);
            NewReportHandlerOfEvent(bs, null);
        }
        private byte[] LoadFromStatic(string staticid)
        {
            byte[] bs = null;
            //UFReport..
            using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, "select report,dynamicassembly from UAP_Report_StaticReport where staticid='" + staticid + "'"))
            {

                long retval;
                int bufferSize = 100;
                byte[] outByte = new byte[bufferSize];

                int colindex = 0;  //0--report  1---assembly
                while (reader.Read())
                {
                    while (colindex < 2)
                    {
                        if (!reader.IsDBNull(colindex))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            using (BinaryWriter writer = new BinaryWriter(ms))
                            {
                                long startIndex = 0;

                                retval = reader.GetBytes(colindex, startIndex, outByte, 0, bufferSize);
                                while (retval == bufferSize)
                                {
                                    writer.Write(outByte);
                                    writer.Flush();

                                    startIndex += bufferSize;
                                    retval = reader.GetBytes(colindex, startIndex, outByte, 0, bufferSize);
                                }

                                writer.Write(outByte, 0, (int)retval);
                                writer.Flush();

                                if (ms.Length > 0)
                                {
                                    switch (colindex)
                                    {
                                        case 0:
                                            _report = Report.FromBytes(ms.ToArray());
                                            break;
                                        case 1:
                                            bs = ms.ToArray();
                                            break;
                                    }
                                }
                                ms.Close();
                                writer.Close();
                            }
                        }
                        colindex++;
                    }
                }
                reader.Close();
            }
            return bs;
        }

        private void Load(string cacheid, string viewid, RuntimeFormat columnsettings)
        {
            _report = new Report();
            if (!string.IsNullOrEmpty(cacheid))
            {
                _report.CacheID = cacheid;
            }
            _report.UnderState = _reportstate;
            _report.ViewID = viewid;
            XmlDocument commonformat = new XmlDocument();
            XmlDocument localeformat = new XmlDocument();
            XmlDocument schema = new XmlDocument();

            ReportDefinition rd = null;
            ColumnCollection ds = null;

            ReportDataFacade rdf = new ReportDataFacade(_login);
            rdf.RetrieveReportByView(viewid, out rd, out ds);
            //rdf.RetrieveReportByViewForStaticReport(
            //viewid, out rd, out ds);

            if (_report.bDynamicFormat && columnsettings != null && !string.IsNullOrEmpty(columnsettings.OutU8FormatClass))
            {
                string[] classtocreate = columnsettings.OutU8FormatClass.Split(',');
                object oh = Activator.CreateInstance(classtocreate[1], classtocreate[0]).Unwrap();
                Hashtable ht = oh.GetType().InvokeMember("GetFormat",
                            BindingFlags.InvokeMethod,
                            null,
                            oh,
                            new object[] { _login.UserToken, columnsettings.OutU8FilterString }) as Hashtable;

                rd.ComplexView.CommonFormat = ht["CommonFormat"].ToString();
                rd.ComplexView.LocaleFormat = ht["LocaleFormat"].ToString();
                rd.ComplexView.AssemblyString = null;
            }

            _report.Varients.DataHelper = _datahelper;

            if (rd != null && rd.ComplexView != null)
            {
                _report.Name = rd.Name;
                _report.SubId = rd.SubProjectID;
                _report.ProjectID = rd.ProjectID;
                if (ds != null)
                {
                    _datasource = new ReportDataSource(ds);
                    //_datasource.DataSourceID = rd.DataSourceID;
                    _datasource.FunctionName = rd.FunctionName;
                }
                if (rd.HelpFileName != "" && rd.HelpFileName != null)
                {
                    _report.HelpInfo.FileName = rd.HelpFileName;
                    _report.HelpInfo.KeyIndex = rd.HelpIndex;
                    _report.HelpInfo.KeyWord = rd.HelpKeyWord;
                }
                _report.DataSourceID = rd.DataSourceID;
                _report.ExtendedDataSourceID = rd.DataSourceIDExtended;

                LoadFromComplexView(rd.ComplexView, commonformat, localeformat, schema);

                #region load assembly
                byte[] bs = null;
                if (!string.IsNullOrEmpty(rd.ComplexView.AssemblyString))
                    bs = Convert.FromBase64String(rd.ComplexView.AssemblyString);
                #endregion
                NewReportHandlerOfEvent(bs, null);
            }
            else
            {
                throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex15");
            }

            InterpretXmlFormat(commonformat, localeformat, schema);

            if (columnsettings == null || columnsettings.bEmpty)
            {
                this.InitRuntimeFormat(rd.ComplexView);
                columnsettings = _report.RuntimeFormatMeta;
            }
            else
                _report.RuntimeFormatMeta = columnsettings;

            AdaptFormatAccordingToColumnSettings(columnsettings);
        }

        //design time load
        protected void LoadFormat(ComplexView cv, ColumnCollection cc, string projectId)
        {
            _report = new Report();
            _report.UnderState = _reportstate;

            XmlDocument commonformat = new XmlDocument();
            XmlDocument localeformat = new XmlDocument();
            XmlDocument schema = new XmlDocument();

            _report.Varients.DataHelper = _datahelper;

            if (cv != null)
            {
                _report.ViewID = cv.ID;
                _report.ProjectID = projectId;
                _report.ChartStrings = cv.ChartString;
                _report.ReportMergeCell = cv.ReportMergeCell;
                if (cc != null)
                    _datasource = new ReportDataSource(cc);

                LoadFromComplexView(cv, commonformat, localeformat, schema);
            }
            else
            {
                throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex15");
            }
            InterpretXmlFormat(commonformat, localeformat, schema);
        }

        private void InterpretXmlFormat(XmlDocument commonformat, XmlDocument localeformat, XmlDocument schema)
        {
            BaseInterpretor interpretor = null;
            switch (_report.Type)
            {
                case ReportType.FreeReport:
                    interpretor = new FreeInterpretor(_report, _datahelper);
                    break;
                case ReportType.GridReport:
                    interpretor = new GridInterpretor(_report, _datahelper);
                    break;
                case ReportType.CrossReport:
                    interpretor = new CrossInterpretor(_report, _datahelper);
                    break;
                case ReportType.IndicatorReport:
                    interpretor = new IndicatorIntepretor(_report, _datahelper);
                    break;
            }
            interpretor.Interprete(localeformat, commonformat);
            GroupSchemas.Initialize(_report, schema, interpretor.DetailType, _login.LocaleID);
            SetGridDetailCells();
        }

        //返回设计时的所有GridDetail字段
        private void SetGridDetailCells()
        {
            foreach (Section s in _report.Sections)
            {
                if (s.SectionType == SectionType.GridDetail)
                {
                    _report.GridDetailCells = s.Cells.Clone() as Cells;
                    _report.GridDetailCells.SetRawSuper();
                    return;
                }
                if (s.SectionType == SectionType.CrossRowHeader)
                {
                    _report.GridDetailCells = s.Cells.Clone() as Cells;
                    _report.GridDetailCells.SetRawSuper();
                    return;
                }
            }
        }

        private void LoadFromComplexView(ComplexView cv, XmlDocument commonformat, XmlDocument localeformat, XmlDocument schema)
        {
            _report.ReportID = cv.ReportID;
            _report.Type = (ReportType)cv.ViewType;
            _report.RealViewType = (ReportType)cv.ViewType;//11.0新增加
            _report.CanSaveAs = cv.CanSaveAs;
            _report.ReportMergeCell = cv.ReportMergeCell;
            if (cv.ReportID == "AP[__]应付总账表" || cv.ReportID == "AR[__]应收总账表")
                _report.AllowGroup = false;

            if (cv.CommonFormat != null && cv.CommonFormat != "" &&
                cv.LocaleFormat != null && cv.LocaleFormat != "")
            {
                commonformat.LoadXml(cv.CommonFormat);
                localeformat.LoadXml(cv.LocaleFormat);
            }

            if (cv.GroupSchemas != null && cv.GroupSchemas != "")
            {
                //schema = new XmlDocument();
                schema.LoadXml(cv.GroupSchemas);
            }

            //crossschema
            XmlDocument crossschemas = null;
            if (!string.IsNullOrEmpty(cv.CrossSchemas))
            {
                crossschemas = new XmlDocument();
                crossschemas.LoadXml(cv.CrossSchemas);
            }
            GroupSchemas.InitializeCross(_report, crossschemas, _login.LocaleID);

            LevelExpandSchema les = new LevelExpandSchema();
            les.LoadFromString(cv.LevelExpend);
            _report.ExpandSchema = les;

            if (_datasource == null && commonformat != null && localeformat != null)
            {
                XmlNode dscommon = commonformat.DocumentElement.SelectSingleNode("DataSource");
                XmlNode dslocale = localeformat.DocumentElement.SelectSingleNode("DataSource");
                _datasource = new ReportDataSource(dscommon, dslocale);
            }
            if (_datasource != null)
            {
                _datasource.UnderState = _report.UnderState;
                _report.DataSources = _datasource.DataSources;
            }

            _report.bLandScape = cv.BlandScape;
            _report.PageMargins = PageMargins.ConvertToPageMargins(cv.PageMargins);
            _report.PrintOption.PrintProvider = cv.bShowDetail ? PrintProvider.U8PrintComponent : PrintProvider.UAPReportPrintComponent;
            _report.PrintOption.CanSelectProvider = cv.bMustShowDetail;
            string[] columns = cv.GetColumns();
            if (columns.Length > 0)
                _report.PaperName = columns[0];
            if (columns.Length > 1)
                _report.PrintOption.FixedRowsPerPage = Convert.ToInt32(columns[1]);

            _report.ColorStyleID = cv.FontColorStyleId;
        }

        private void ReadCache(string cacheid)
        {
            string filename = CacheFileName(cacheid);
            byte[] b = null;
            PageInfos pis = null;
            using (System.IO.FileStream fs = new System.IO.FileStream(filename + "rt.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                b = new Byte[fs.Length];
                fs.Read(b, 0, b.Length);
                fs.Close();
            }
            if (b != null)
                _report = Report.FromBytes(b);

            _datasource = new ReportDataSource(_report.DataSources);

            if (File.Exists(filename + "pi.rc"))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filename + "pi.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    b = new Byte[fs.Length];
                    fs.Read(b, 0, b.Length);
                    fs.Close();
                }
                if (b != null)
                {
                    pis = PageInfos.FromBytes(b);
                }
            }
            b = null;
            if (File.Exists(filename + "as.rc"))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filename + "as.rc", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    b = new Byte[fs.Length];
                    fs.Read(b, 0, b.Length);
                    fs.Close();
                }
            }
            NewReportHandlerOfEvent(b, pis);
        }

        private void AdaptFormatAccordingToColumnSettings(RuntimeFormat columnsettings)
        {
            if (_report.Type == ReportType.FreeReport || columnsettings == null)
                return;
            Hashtable htdata = columnsettings.GetData();
            Section detail = null;
            Section cross = null;
            string temptable = _report.BaseTable;
            if (_report.Type == ReportType.GridReport)
                detail = _report.Sections[SectionType.GridDetail];
            else if (_report.Type == ReportType.CrossReport)
            {
                detail = _report.Sections[SectionType.CrossRowHeader];
                cross = _report.Sections[SectionType.CrossDetail];
            }
            if (detail != null)
            {
                #region dynamiccolumns
                ArrayList dynamiccolumns = htdata[RuntimeFormatServerContext.ArgKeyDymanicAddedCols] as ArrayList;
                foreach (string name in dynamiccolumns)
                {
                    Cell cell = detail.Cells.GetByGroupKey(name);
                    if (cell != null)
                    {
                        cell.Visible = true;
                        continue;
                    }
                    AddDynamicCell(detail, name, _report.DataSources[name], DataType.String, true);
                }
                #endregion
                #region dynamicsummarys
                Hashtable htsummarys = htdata[RuntimeFormatServerContext.ArgKeySummaryCols] as Hashtable;
                foreach (string key in htsummarys.Keys)
                {
                    string cellkey = key;
                    if (key.Contains("__"))
                    {
                        int index_ = key.IndexOf("__");
                        cellkey = key.Substring(0, index_);
                    }
                    Cell cell = detail.Cells.GetByGroupKey(cellkey);
                    if (cell == null)
                    {
                        if (cross != null)
                            cell = cross.Cells.GetByGroupKey(cellkey);
                        if (cell == null)
                            continue;
                    }
                    cell.Visible = true;
                    if (cell is IGridCollect)
                    {
                        int op = Convert.ToInt32(htsummarys[key]);
                        if (op == -1)
                            (cell as IGridCollect).bSummary = false;
                        else
                        {
                            (cell as IGridCollect).bSummary = true;
                            (cell as IGridCollect).Operator = (OperatorType)op;
                            if ((OperatorType)op == OperatorType.AccumulateSUM ||
                                (OperatorType)op == OperatorType.ComplexSUM ||
                                (OperatorType)op == OperatorType.BalanceSUM)
                                _report.MustShowDetail = true;
                        }
                    }
                }
                #endregion
            }
        }

        protected void InitInformationString(string eventfilter, string uifilter)
        {
            if (_report.Informations != null && string.IsNullOrEmpty(eventfilter))
            {
                if (_report.Informations.Contains("currentuser"))
                    eventfilter = _report.SetInformationString("currentuser", _login.UserID);
                else if (_report.Informations.Contains("currentmonth"))
                    eventfilter = _report.SetInformationString("currentmonth", _login.Month.ToString());
                else if (_report.Informations.Contains("currentdate"))
                    eventfilter = _report.SetInformationString("currentdate", _login.Date);
            }
            _report.InformationString = (string.IsNullOrEmpty(eventfilter) ? "" : eventfilter) + ((!string.IsNullOrEmpty(eventfilter) && !string.IsNullOrEmpty(uifilter)) ? " and " : "") + (string.IsNullOrEmpty(uifilter) ? "" : uifilter);
        }
        protected void InitReportFromFilterArgs(FilterArgs filterargs)
        {
            if (filterargs != null)
            {
                if (filterargs.Args.Contains("SolidGroup"))
                    _report.SolidGroup = filterargs.Args["SolidGroup"].ToString();
                if (filterargs.Args.Contains("SolidSort"))
                    _report.SolidSort = Boolean.Parse(filterargs.Args["SolidSort"].ToString());
                if (filterargs.Args.Contains("GroupFilter") && filterargs.Args["GroupFilter"] != null)
                    _report.GroupFilter = filterargs.Args["GroupFilter"].ToString();
                if (filterargs.Args.Contains("RowFilter") && filterargs.Args["RowFilter"].ToString() != "")
                    _report.RowFilter.FilterString = string.IsNullOrEmpty(_report.RowFilter.FilterString) ? filterargs.Args["RowFilter"].ToString() : ("(" + _report.RowFilter.FilterString + ") and (" + filterargs.Args["RowFilter"].ToString() + ")");
                if (filterargs.Args.Contains("EventFilter") && filterargs.Args["EventFilter"].ToString() != "")
                    InitInformationString(filterargs.Args["EventFilter"].ToString(), null);

                #region outu8 filterargs handler
                if (_report.UnderState == ReportStates.OutU8)
                {
                    try
                    {
                        string[] classtocreate = filterargs.ClassName.Split(',');
                        object oh = Activator.CreateInstance(classtocreate[1], classtocreate[0]).Unwrap();
                        Hashtable ht = oh.GetType().InvokeMember("GetFilterArgs",
                                    BindingFlags.InvokeMethod,
                                    null,
                                    oh,
                                    new object[] { filterargs.DataSource.FilterString }) as Hashtable;

                        foreach (string key in ht.Keys)
                        {
                            filterargs.Args.Add(key, ht[key]);
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion


                foreach (string key in filterargs.FltDAESrv.Keys)
                {
                    if (key.ToLower() != "@filterstring")
                    {
                        string newkey = key.Substring(1);
                        if (!filterargs.FltSrv.Contains(newkey))
                        {
                            FilterItem fi = filterargs.FltDAESrv[key];
                            fi.Key = newkey;
                            filterargs.FltSrv.Add(fi);
                        }
                    }
                }
                _report.FltSrv = filterargs.FltSrv;
                _report.Args = filterargs.Args;

                if (!filterargs.FltSrv.Contains("CurrentUser"))
                    filterargs.FltSrv.Add(new FilterItem("CurrentUser", _login.UserID));
                if (!filterargs.FltSrv.Contains("CurrentDate"))
                    filterargs.FltSrv.Add(new FilterItem("CurrentDate", _login.Date));
                if (!filterargs.FltSrv.Contains("CurrentYear"))
                    filterargs.FltSrv.Add(new FilterItem("CurrentYear", Convert.ToString(_login.Year)));
            }
        }

        protected void SetColumns(Cell cell, SimpleArrayList allcolumns)
        {
            string keystring = null;
            if (cell is IMultiHeader)
            {
                allcolumns.Add((cell as IMultiHeader).SortSource.Name);
            }
            if (cell is IDataSource)
            {
                allcolumns.Add((cell as IDataSource).DataSource.Name);
            }
            else if (cell is ICalculateColumn)
            {
                string[] expressions = ExpressionService.SplitExpression((cell as ICalculateColumn).Expression.ToLower());
                for (int i = 0; i < expressions.Length; i++)
                {
                    if (expressions[i].Trim() != "" && _datasource.DataSources.Contains(expressions[i].Trim()))
                        allcolumns.Add(expressions[i].Trim());
                }
            }
            else if (cell is IAlgorithm)
            {
                keystring = (cell as IAlgorithm).Algorithm;
            }
            SetScriptKey(cell, keystring, cell.PrepaintEvent, allcolumns);
        }

        private void SetScriptKey(Cell cell, string keystring, string script, SimpleArrayList allcolumns)
        {
            if (string.IsNullOrEmpty(keystring) && string.IsNullOrEmpty(script))
                return;
            foreach (string key in _datasource.DataSources.Keys)
            {
                if (keystring != null && keystring.ToLower().Contains(key.ToLower()))
                {
                    allcolumns.Add(key);
                }
                if (script != null && script.ToLower().Contains(key.ToLower()))
                {
                    allcolumns.Add(key);
                }
            }
        }

        public void InitGroup(string ufreportarg, string groupschemaid1)
        {
            if (_report.AllowGroup)
            {
                #region fltargstring
                string groupschemaid2 = null;
                string dispdetail = null;
                if (!string.IsNullOrEmpty(ufreportarg))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(ufreportarg);
                    XmlElement root = doc.DocumentElement;
                    if (root.HasAttribute("ViewID") && root.GetAttribute("ViewID").Trim() != "")
                    {
                        if (root.HasAttribute("reportpagerows"))
                            _report.PageRecords = Convert.ToInt32(root.GetAttribute("reportpagerows"));

                        if (root.HasAttribute("dispdetail"))
                            dispdetail = root.GetAttribute("dispdetail");

                        if (root.HasAttribute("GroupID"))
                            groupschemaid2 = root.GetAttribute("GroupID");
                    }
                    //else //  liancha qie huan xianshimingxi 
                    //{
                    //    XmlNode xn = doc.SelectNodes(@"Data/Views/View")[0];
                    //    if (xn.Attributes["bShowDetail"] != null && xn.Attributes["bShowDetail"].Value == "True")
                    //        dispdetail = "S";
                    //    if (xn.Attributes["bShowDetail"] != null && xn.Attributes["bShowDetail"].Value == "False")
                    //        dispdetail = "N";
                    //}
                }

                #endregion

                #region set groupschema
                if (!string.IsNullOrEmpty(groupschemaid1) && _report.GroupSchemas.Contains(groupschemaid1))
                    _report.CurrentSchemaID = groupschemaid1;
                else if (!string.IsNullOrEmpty(groupschemaid2) && _report.GroupSchemas.Contains(groupschemaid2))
                    _report.CurrentSchemaID = groupschemaid2;
                else if (_report.GroupSchemas.Count > 0)
                    _report.CurrentSchemaID = _report.GroupSchemas.Default.ID;

                #endregion

                #region showdetail & pagebygroup
                if (dispdetail != null && dispdetail.StartsWith("S"))
                    _report.bShowDetail = true;
                else if (dispdetail != null && dispdetail.StartsWith("N"))
                    _report.bShowDetail = false;
                else
                    _report.bShowDetail = _report.CurrentSchema.bShowDetail;

                if (_report.Type == ReportType.FreeReport)
                    _report.bShowDetail = true;

                if (_report.bPageByGroup)
                    _report.MustShowDetail = true;

                if (_report.MustShowDetail)
                    _report.bShowDetail = true;

                if ((_report.Type != ReportType.FreeReport && _report.CurrentSchema.SchemaItems.Count == 0)
                    || (_report.Type == ReportType.FreeReport && _report.GroupLevels == 0))
                {
                    _report.bPageByGroup = false;
                    _report.bShowDetail = true;
                    _report.MustShowDetail = true;
                }

                if (_report.CurrentSchema.SchemaItems.Count == 1 && !_report.bShowDetail)
                    _report.bPageByGroup = false;
                #endregion
            }
            else
                _report.GroupSchemas.Clear();
        }

        #endregion

        #region data created
        private void CreateBaseTable(FilterArgs filterargs, string crosstable, string rawtable, string basetable, int levels)
        {
            System.Diagnostics.Trace.WriteLine("Begin create base table");
            //#region drop base table
            //if(!string.IsNullOrEmpty(_report.BaseTable))
            //{
            //    RemoteDataHelper rdh = new RemoteDataHelper();
            //    rdh.DropFromDB(_login.TempDBCnnString, rdh.AllString(crosstable, rawtable, basetable, levels,_report.CacheID ));
            //}
            //#endregion
            if (_report.UnderState == ReportStates.Preview)
            {
                #region preview
                // _report.BaseTable = CustomDataSource.GetATableName();
                _report.BaseTable = CustomDataSource.GetATableNameWithTaskId(this._login.TaskID);
                //generate preview table
                #region datatable
                DataTable dt = new DataTable();
                DataColumn dc;
                DataSources dss = _datasource.DataSources;
                if (!dss.Contains("baseid"))
                {
                    dc = new DataColumn("baseid");
                    dc.DataType = typeof(int);
                    dt.Columns.Add(dc);
                }
                foreach (string key in dss.Keys)
                {
                    DataSource ds = dss[key];
                    if (ds.bAppend)
                        continue;
                    dc = new DataColumn(ds.Name);
                    switch (ds.Type)
                    {
                        case DataType.Decimal:
                        case DataType.Currency:
                            dc.DataType = typeof(double);
                            break;
                        case DataType.Int:
                            dc.DataType = typeof(int);
                            break;
                        case DataType.DateTime:
                            dc.DataType = typeof(DateTime);
                            break;
                        case DataType.Boolean:
                            dc.DataType = typeof(bool);
                            break;
                        case DataType.Image:
                            dc.DataType = typeof(byte[]);
                            break;
                        default:
                            dc.DataType = typeof(string);
                            break;
                    }
                    dt.Columns.Add(dc);
                }
                Random random = new Random();
                for (int j = 0; j < 10; j++)
                {
                    DataRow dr = dt.NewRow();
                    dr["baseid"] = j;
                    foreach (string key in dss.Keys)
                    {
                        DataSource ds = dss[key];
                        if (ds.bAppend)
                            continue;
                        int seed = (random.Next()) % 5;
                        object o = DBNull.Value;
                        switch (ds.Type)
                        {
                            case DataType.Decimal:
                            case DataType.Currency:
                                o = CurrentValue.Decimal(seed);
                                break;
                            case DataType.Int:
                                o = CurrentValue.Int(seed);
                                break;
                            case DataType.DateTime:
                                o = CurrentValue.DateTime(seed).ToShortDateString();
                                break;
                            case DataType.Boolean:
                                o = true;
                                break;
                            case DataType.Image:
                                o = DBNull.Value;
                                break;
                            default:
                                o = CurrentValue.String(seed);
                                break;
                        }
                        dr[ds.Name] = o;
                    }
                    dt.Rows.Add(dr);
                }
                #endregion
                #region write to tempdb
                CheckCanceled();
                CreateTempTable(_report.BaseTableNoneHeader.Trim(), dt);
                CheckCanceled();
                using (SqlBulkCopy bcp = new SqlBulkCopy(_login.TempDBCnnString))
                {
                    bcp.BulkCopyTimeout = 0;
                    bcp.DestinationTableName = _report.BaseTableNoneHeader.Trim();
                    CheckCanceled();
                    bcp.WriteToServer(dt);
                }
                #endregion
                #endregion
            }
            else
            {
                if (_report.UnderState == ReportStates.OutU8)
                {
                    filterargs.DataSource.Type = CustomDataSourceTypeEnum.TemplateTable;
                    //string errstring = null;
                    //object oh = Activator.CreateInstance("U8DRP_GeneralQueryC", "UFIDA.U8.DRP.Lib.Business.U8DRP_GeneralQueryC.Query").Unwrap();
                    //oh.GetType().InvokeMember("QueryBI",
                    //            BindingFlags.InvokeMethod,
                    //            null,
                    //            oh,
                    //            new object[] { _login.UserToken, filterargs.DataSource.FilterString, "", "", filterargs.DataSource.SQL, errstring });
                    //if (!string.IsNullOrEmpty(errstring))
                    //    throw new Exception(errstring);

                    string[] classtocreate = filterargs.ClassName.Split(',');
                    object oh = Activator.CreateInstance(classtocreate[1], classtocreate[0]).Unwrap();
                    oh.GetType().InvokeMember("FillData",
                                BindingFlags.InvokeMethod,
                                null,
                                oh,
                                new object[] { _login.UserToken, filterargs.DataSource.FilterString, filterargs.DataSource.SQL });
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Begin call reportdata ");
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    #region call reportdata
                    CheckCanceled();
                    //if (_report.SelfActions != null && _report.SelfActions.Count > 0)
                    filterargs.DataSource.SelectString = "";
                    ReportDataFacade rdf = new ReportDataFacade(_login);
                    System.Diagnostics.Trace.WriteLine("t1:" + watch.ElapsedMilliseconds);

                    string extendedfilterstring = "";
                    if (filterargs.Args.Contains("extendedfilterstring"))
                    {
                        object arg = filterargs.Args["extendedfilterstring"];
                        if (arg != null)
                            extendedfilterstring = arg.ToString();
                    }

                    if (!filterargs.bAutoSource){
                        rdf.MoveData2TempDB4Custom(_report.ExtendedDataSourceID, _report.DataSourceID, _datasource.FunctionName, filterargs.FltSrv, filterargs.DataSource, extendedfilterstring);
                        System.Diagnostics.Trace.WriteLine("t21:" + watch.ElapsedMilliseconds);
                    }
                       
                    else
                    {
                        rdf.MoveData2TempDB4DataEngine(_report.ExtendedDataSourceID, _report.DataSourceID, _datasource.FunctionName, filterargs.FltSrv, filterargs.DataSource, extendedfilterstring);
                        if (filterargs.DataSource.Type == CustomDataSourceTypeEnum.StoreProc)
                            _report.bStoreProcAndTempTable = true;
                        System.Diagnostics.Trace.WriteLine("t22:" + watch.ElapsedMilliseconds);
                    }
                    System.Diagnostics.Trace.WriteLine("t3:" + watch.ElapsedMilliseconds);
                    #endregion
                }
                CheckCanceled();
                _report.BaseTable = filterargs.DataSource.SQL;
            }
            System.Diagnostics.Trace.WriteLine("End create base table");
        }

        private void AfterBaseTableCreated(string temptable, FilterArgs filterargs)
        {
            bool bdeleteadundentdatasource = (!_report.bFree && (_report.bSupportDynamicColumn || !string.IsNullOrEmpty(_report.ExtendedDataSourceID)) && CanApplyDynamicColumn(filterargs));

            ArrayList keys = new ArrayList();
            Hashtable ht = new Hashtable();
            AdaptDataSource(keys, ht, temptable);

            if (bdeleteadundentdatasource)
            {
                Section detail = null;
                Section crosspoint = null;
                Section crosscolumnheader = null;
                if (_report.Type == ReportType.GridReport)
                    detail = _report.Sections[SectionType.GridDetail];
                else
                {
                    detail = _report.Sections[SectionType.CrossRowHeader];
                    crosspoint = _report.Sections[SectionType.CrossDetail];
                    crosscolumnheader = _report.Sections[SectionType.CrossColumnHeader];
                }
                if (detail != null)
                {
                    foreach (Cell cell in detail.Cells)
                    {
                        string dynamicscript = cell.Name.ToLower();
                        if (dynamicscript == "dynamicscriptt" || dynamicscript == "dynamicscriptc" || dynamicscript == "dynamicscripts" || dynamicscript == "dynamicscriptcs" || dynamicscript == "dynamicscripta")
                        {
                            _report.DynamicScript = cell.Name;
                            break;
                        }
                    }
                }
                int left = 1000000;
                int left2 = 100000;
                int width = 0;
                SuperLabel sl = null;

                //SA[__]货龄分析(按发票)发货未结价税合计
                string flag = null;
                if (_report.ReportID == "SA[__]货龄分析(按发票)")
                {
                    object o = SqlHelper.ExecuteScalar(_login.UfDataCnnString, "select enumname from v_aa_enum where enumtype='SA.HLMoneyPer' and enumcode='0'");
                    if (o != null)
                    {
                        string s = o.ToString();
                        int index = s.IndexOf("_");
                        flag = s.Substring(index + 1, s.Length - index - 2);
                    }
                }

                foreach (string name in keys)
                {
                    if (!ht.Contains(name.ToLower()) || name.ToLower() == _report.BaseID.ToLower())
                        continue;
                    if (detail != null)
                    {
                        Cell cell = detail.Cells.GetBySource(name);
                        if (cell != null)
                            continue;
                        if (crosscolumnheader != null)
                        {
                            cell = crosscolumnheader.Cells.GetBySource(name);
                            if (cell != null)
                                continue;
                        }
                        if (crosspoint != null)
                        {
                            cell = crosspoint.Cells.GetBySource(name);
                            if (cell != null)
                                continue;
                        }

                        DataSource ds = _report.DataSources[name];
                        cell = AddDynamicCell(detail, name, ds, GetDataType(ht[name.ToLower()].ToString()), _report.bDynamicColumnVisible || (ds != null && ds.bDimension) ? true : false);
                        if (cell != null)
                        {
                            if (flag != null && name.EndsWith(flag) && cell is IGridEvent)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("double d=Convert.ToDouble(columntodata[\"发货未结价税合计\"]);");
                                sb.AppendLine("if(d!=0)");
                                sb.AppendLine("     cell.Caption=Convert.ToDouble(Convert.ToDouble(cell.Caption)/d).ToString(\"#0.00%\");");
                                sb.AppendLine("else");
                                sb.AppendLine("     cell.Caption=\"\";");
                                cell.PrepaintEvent = sb.ToString();
                                (cell as IGridEvent).EventType = EventType.BothContentAndSummary;
                                _handler.NeedReCompile();
                            }
                            int _index = cell.Caption.IndexOf('_');
                            if (_index > 0)
                            {
                                string sc = cell.Caption.Substring(0, _index).Trim();
                                if (sl == null || sl.Caption.ToLower() != sc.ToLower())
                                {
                                    if (sl != null)
                                    {
                                        sl.Width = width;
                                        width = 0;
                                    }
                                    sl = new SuperLabel();
                                    sl.SetY(cell.Y - 24);
                                    sl.Caption = sc;
                                    sl.Name = sl.Caption;
                                    sl.X = left;
                                    detail.Cells.AddDirectly(sl);
                                }
                                cell.X = left;
                                cell.Visible = true;
                                cell.Caption = cell.Caption.Substring(_index + 1).Trim();
                                left += cell.Width;
                                width += cell.Width;
                            }
                            else
                            {
                                cell.X = left2;
                                left2 += cell.Width;
                            }
                        }
                    }
                }
                if (sl != null)
                {
                    sl.Width = width;
                }
            }
        }

        private Cell AddDynamicCell(Section detail, string name, DataSource ds, DataType dt, bool bvisible)
        {
            if (_login.SubID == "OutU8")
                bvisible = true;
            if (ds != null)
                name = ds.Name;
            string userdefinekey = _datahelper.GetUserDefineCaption(name);
            if (ds == null)
            {
                ds = new DataSource();
                ds.Name = name;
                ds.Caption = _datahelper.CusDefineInfo(name, userdefinekey);
                ds.Type = dt;
                _report.DataSources.Add(ds);
            }
            Cell cell = detail.GetDefaultRect(ds);
            if (cell != null)
            {
                detail.Cells.AddDirectly(cell);
                //if (_datahelper.bCusName(ds.Name))
                (cell as IUserDefine).UserDefineItem = userdefinekey;
            }
            cell.Visible = bvisible;

            if (cell is IGridEvent && !string.IsNullOrEmpty(_report.DynamicScript))
            {
                cell.ScriptID = _report.DynamicScript;
                switch (_report.DynamicScript.ToLower())
                {
                    case "dynamicscriptt":
                        (cell as IGridEvent).EventType = EventType.OnTitle;
                        break;
                    case "dynamicscriptc":
                        (cell as IGridEvent).EventType = EventType.OnContent;
                        break;
                    case "dynamicscripts":
                        (cell as IGridEvent).EventType = EventType.OnSummary;
                        break;
                    case "dynamicscriptcs":
                        (cell as IGridEvent).EventType = EventType.BothContentAndSummary;
                        break;
                    case "dynamicscripta":
                        (cell as IGridEvent).EventType = EventType.OnAll;
                        break;
                }
            }
            if (ds != null && ds.Type == DataType.Int && cell is GridDecimal)
                (cell as GridDecimal).bSummary = false;


            if (cell is GridDecimal)
            {
                string iniPath = AppDomain.CurrentDomain.BaseDirectory + "UAPReportConfig.ini";
                this._iniClass = new INIClass(iniPath);
                int length = 2;
                int.TryParse(this._iniClass.IniReadValue("DecimalLength", "Length"), out length);
                string formatString = "#,##0.00";
                if (length > 2)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = length - 2; i > 0; i--)
                    {
                        sb.Append("0");
                    }
                    formatString = formatString + sb.ToString();
                }
                (cell as GridDecimal).FormatString = formatString;
            }
            return cell;
        }

        private void AdaptDataSource(ArrayList keys, Hashtable ht, string temptable)
        {
            if (keys == null)
                keys = new ArrayList();
            if (ht == null)
                ht = new Hashtable();
            DataSet dset = SqlHelper.ExecuteDataSet(_login.TempDBCnnString, GetTempTableSchemaSQL(temptable));
            DataTable dt = dset.Tables[1];
            if (dt.Rows.Count > 0)
                _report.BaseID = dt.Rows[0][0].ToString();

            dt = dset.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                string key = dr["Column_name"].ToString();
                keys.Add(key);
                ht.Add(key.ToLower(), dr["Type"].ToString());
            }
            int index = 0;
            while (index < _report.DataSources.Count)
            {
                DataSource ds = _report.DataSources[index];
                index++;

                if (!ht.Contains(ds.Name.ToLower()))
                {
                    _report.DataSources.Remove(ds.Name);
                    index--;
                }
                else
                {
                    string dbtype = ht[ds.Name.ToLower()].ToString();
                    if (!string.IsNullOrEmpty(dbtype))
                    {
                        DataType tmptype = GetDataType(dbtype);
                        if (ds.Type == DataType.Currency || ds.Type == DataType.Decimal || ds.Type == DataType.Int)
                        {
                            if (tmptype != DataType.Currency && tmptype != DataType.Decimal && tmptype != DataType.Int)
                                ds.Type = tmptype;
                        }
                        else if (ds.Type == DataType.DateTime)
                        {
                            if (tmptype != DataType.String)
                                ds.Type = tmptype;
                        }
                        else
                            ds.Type = tmptype;
                    }
                    if (!ds.bDimension)
                        ht.Remove(ds.Name.ToLower());

                    string userdefinekey = _datahelper.GetUserDefineCaption(ds.Name);
                    ds.Caption = _datahelper.CusDefineInfo(ds.Caption, userdefinekey);
                }
            }
        }

        #region expand handle
        public void InitExpand(ReportLevelExpand levelexpand)
        {
            #region levelexpand
            if (levelexpand != null)
                _report.CurrentExpand = levelexpand;
            if (_report.CurrentExpand != null)
            {
                Section section = _report.Sections[SectionType.GridDetail];
                if (section == null)
                    section = _report.Sections[SectionType.CrossRowHeader];
                if (section != null)
                {
                    SetExpandCells(section);
                    ExpandHandle();
                }
            }
            #endregion
        }

        private void ExpandHandle()
        {
            #region write back to base
            if (_expandcolumns != null && _expandcolumns.Count > 0)
            {
                _report.RawTable = _report.BaseTable;
                //_report.BaseTable = CustomDataSource.GetATableName();
                _report.BaseTable = CustomDataSource.GetATableNameWithTaskId(_login.TaskID);
                ReportDataFacade rdf = new ReportDataFacade(_login);
                rdf.LevelExpandInfo2TempDB(_expandcolumns.InnerHash, _report.RawTable, _report.BaseTable + "_expand",
                    new RuntimeLevelExpandSrv(_report.CurrentExpand, _datahelper));
                #region generate base table
                StringBuilder sbb = new StringBuilder();
                StringBuilder sbon = new StringBuilder();
                SimpleArrayList altmp = new SimpleArrayList();
                foreach (string key in _expandcolumns.Keys)
                {
                    sbb.Append(",B.[");
                    sbb.Append(key);
                    sbb.Append("]");
                    if (!altmp.Contains(_expandcolumns[key].ToString()))
                    {
                        altmp.Add(_expandcolumns[key].ToString());
                        if (sbon.Length > 0)
                            sbon.Append(" and ");
                        sbon.Append("isnull(convert(nvarchar(256),A.[");
                        sbon.Append(_expandcolumns[key].ToString());
                        sbon.Append("]),'')=isnull(B.[");
                        sbon.Append(_expandcolumns[key].ToString());
                        sbon.Append("],'')");
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("select A.*");
                sb.Append(sbb.ToString());
                sb.Append(" into ");
                sb.Append(_report.BaseTable);
                sb.Append(" from ");
                sb.Append(_report.RawTable);
                sb.Append(" A inner join ");
                sb.Append(_report.BaseTable);
                sb.Append("_expand B on ");
                sb.Append(sbon.ToString());
                SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sb.ToString());
                #endregion
            }
            #endregion
        }

        private void SetExpandCells(Section section)
        {
            if (_report.UnderState == ReportStates.Designtime ||
                _report.UnderState == ReportStates.Static)
                return;
            int x = DefaultConfigs.ReportLeft;
            int y = DefaultConfigs.SECTIONHEADERHEIGHT;
            if (section.Cells.Count > 0)
            {
                Cell cell = section.Cells[section.Cells.Count - 1];
                x = cell.X + cell.Width;
                y = cell.Y;
            }
            foreach (LevelExpandItem item in _report.CurrentExpand.LevelExpandItems)
            {
                if (_expandcolumns == null)
                    _expandcolumns = new SimpleHashtable();
                int depth = _datahelper.ExpandDepth(item);
                bool bok = false;
                foreach (Cell cell in section.Cells)
                {
                    if (cell is IMapName && (cell as IMapName).MapName.ToLower() == item.ColumnName.ToLower())
                    {
                        bok = true;
                        break;
                    }
                }
                if (!bok)
                    continue;
                for (int i = 1; i <= depth; i++)
                {
                    GridLabel column = new GridLabel();
                    column.Name = "Expand" + item.ColumnName + i.ToString();
                    string caption = _datahelper.ExpandCaption(item.ExpandType) + "$$" + i.ToString();
                    if (Convert.ToInt32(item.ExpandType) >= 11)
                        caption = _datahelper.ExpandCaption(item.ExpandType) + i.ToString();
                    column.SetCaption(caption);
                    column.DataSource = new DataSource(item.ColumnName + "_" + i.ToString(), DataType.String);
                    column.DataSource.bAppend = true;
                    _datasource.DataSources.Add(column.DataSource);
                    column.X = x;
                    column.Y = y;
                    x += column.Width;
                    //column.bShowAtReal = true;
                    #region oldcode
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append(" LevelExpandItem item=cell.Tag as LevelExpandItem;\r\n");
                    //sb.Append(" int depth=cell.CrossIndex;\r\n");
                    //sb.Append(" string colname=item.ColumnName;\r\n");
                    //sb.Append(" string colvalue=current[colname].ToString();\r\n");
                    //sb.Append(" return datahelper.ExpandData(colvalue,item,depth);");
                    //column.Algorithm = sb.ToString();
                    #endregion
                    section.Cells.Add(column);
                    AppendToRawDetail(column);
                    _expandcolumns.Add(column.DataSource.Name, item.ColumnName);
                }
            }
            if (_expandcolumns != null && _expandcolumns.Count == 0)
                _expandcolumns = null;
        }

        protected void AppendToRawDetail(Cell cell)
        {
            try
            {
                if (_report.GridDetailCells != null)
                {
                    _report.GridDetailCells.AddDirectly(cell.Clone() as Cell);
                }
            }
            catch
            {
            }
        }
        #endregion

        private string GetTempTableSchemaSQL(string tablename)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("declare @objid int ");
            sb.Append("select @objid = id from sysobjects where id = object_id(N'");
            sb.Append(tablename.ToLower().Replace("tempdb..", ""));
            sb.Append("')	");
            sb.AppendLine("select 'Column_name'= name,'Type'= type_name(xusertype) ");
            sb.AppendLine("from syscolumns where id = @objid and number = 0 ");// order by name
            sb.AppendLine("select name from syscolumns where id = @objid and colstat & 1 = 1");
            return sb.ToString();
        }

        //N'tinyint,smallint,decimal,int,real,money,float,numeric,smallmoney' image,datetime
        private DataType GetDataType(string type)
        {
            switch (type)
            {
                case "tinyint":
                case "smallint":
                case "int":
                    return DataType.Int;
                case "decimal":
                case "real":
                case "float":
                case "numeric":
                    return DataType.Decimal;
                case "money":
                case "smallmoney":
                    return DataType.Currency;
                case "datetime":
                    return DataType.DateTime;
                case "image":
                    return DataType.Image;
                case "bit":
                    return DataType.Boolean;
                case "text":
                case "ntext":
                    return DataType.Text;
            }
            return DataType.String;
        }

        private void CreateTempTable(string tablename, DataTable dtschema)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"CREATE TABLE [");
            sb.Append(tablename);
            sb.Append("] (");
            foreach (DataColumn dc in dtschema.Columns)
            {
                sb.Append("[");
                sb.Append(dc.ColumnName);
                sb.Append("] ");
                sb.Append(TypeName(dc.DataType));
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(@") ON [PRIMARY] ");
            SqlHelper.ExecuteNoneQueryNoneTransaction(_login.TempDBCnnString, sb.ToString());
        }

        private string TypeName(Type type)
        {
            if (type == typeof(DateTime))
                return "[datetime]";
            else if (type == typeof(double) || type == typeof(int))
                return "[int]";
            else
                return "[nvarchar] (50)";
        }

        private bool CanApplyDynamicColumn(FilterArgs filterargs)
        {
            if (filterargs == null)
                return false;
            //return (_report.bStoreProcAndTempTable ||
            //    (!filterargs.bAutoSource && filterargs.DataSource.Type == CustomDataSourceTypeEnum.TemplateTable));
            return true;
        }
        #endregion

        #region datedimension
        private void HandleSourceFilter(string rowauthstring)
        {
            _report.SourceFilter = rowauthstring;
            DoSourceFilter();
        }
        private void HandleSourceFilter(FilterArgs filterargs)
        {
            string dimensionsource = HandleDimensionSource(filterargs);
            _report.SourceFilter = HandleRowAuthStringAndDateDimensionFilter(filterargs, dimensionsource);
            DoSourceFilter();
        }

        private void DoSourceFilter()
        {
            if (!string.IsNullOrEmpty(_report.SourceFilter))
            {
                string oldtablename = _report.BaseTable;
                //string newtablename = CustomDataSource.GetATableName();
                string newtablename = CustomDataSource.GetATableNameWithTaskId(this._login.TaskID);

                StringBuilder sbsql = new StringBuilder();
                sbsql.Append("select * into ");
                sbsql.Append(newtablename);
                sbsql.Append(" from ");
                sbsql.Append(oldtablename);
                sbsql.Append(" where ");
                sbsql.Append(_report.SourceFilter);

                SqlHelper.ExecuteNoneQueryNoneTransaction(_login.UfDataCnnString, sbsql.ToString());

                _report.BaseTable = newtablename;

                DropTmpTableHandler(_login.TempDBCnnString, oldtablename);
            }
        }

        private string HandleDimensionSource(FilterArgs filterargs)
        {
            if (filterargs == null)
                return null;
            string dimensionsource = null;
            string currentdimension = null;
            if (filterargs.FltSrv.Contains("AutoCurrentDuration"))
                currentdimension = filterargs.FltSrv["AutoCurrentDuration"].Value1;
            else if (filterargs.FltSrv.Contains("CurrentDuration"))
                currentdimension = filterargs.FltSrv["CurrentDuration"].Value1;

            Cell dimensioncell = null;
            ArrayList dimensioncells = new ArrayList();
            foreach (Section section in _report.Sections)
            {
                int i = 0;
                while (i < section.Cells.Count)
                {
                    Cell cell = section.Cells[i];
                    if (cell is IDateTimeDimensionLevel)
                    {
                        if (!(cell is IBDateTime) || (cell as IBDateTime).bDateTime)
                        {
                            dimensionsource = HandleDateDimensionLevel(section, cell, dimensionsource, ref dimensioncell);
                            cell.Parent = section;
                            dimensioncells.Add(cell);
                        }
                        //ChangeTypeForDateDimension(cell, section);
                        //if ((cell as IDateTimeDimensionLevel).SupportSwitch)
                        //    //&&(cell is CrossDimension ||
                        //    //cell is ColumnHeader ||
                        //    //cell is GridDateTime ||
                        //    //cell is GroupObject ||
                        //    //cell is GroupDimension))

                        //else
                        //{

                        //}
                    }
                    i++;
                }
            }
            if (dimensioncell != null)
                SetDimensionLevel(dimensioncell as IDateTimeDimensionLevel, currentdimension);

            foreach (Cell cell in dimensioncells)
                ChangeTypeForDateDimension(cell, cell.Parent);

            return dimensionsource;
        }

        private void ChangeTypeForDateDimension(Cell dimensioncell, Section dimensionsection)
        {
            if ((dimensioncell as IDateTimeDimensionLevel).DDLevel != DateTimeDimensionLevel.时间)
            {
                int index = dimensionsection.Cells.IndexOf(dimensioncell);
                dimensionsection.Cells.Remove(dimensioncell);
                Cell newcell = GetCalculateDimensionCell(dimensioncell as IDateTimeDimensionLevel);
                dimensionsection.Cells.Insert(index, newcell);
                if (dimensioncell.Name != newcell.Name)
                    UpdateGroupCellName(dimensioncell.Name, newcell.Name);
            }
        }

        private void UpdateGroupCellName(string oldname, string newname)
        {
            GroupSchema gs = _report.CurrentSchema;
            if (gs != null)
            {
                foreach (GroupSchemaItem gsi in gs.SchemaItems)
                {
                    int index = gsi.Items.IndexOf(oldname);
                    if (index != -1)
                    {
                        gsi.Items.RemoveAt(index);
                        gsi.Items.Insert(index, newname);
                        break;
                    }
                }
            }
        }

        private Cell GetCalculateDimensionCell(IDateTimeDimensionLevel cell)
        {
            if (cell is CrossDimension)
                return new CalculateCrossDimension(cell, _login.cAccId);
            else if (cell is ColumnHeader)
                return new CalculateColumnHeader(cell, _login.cAccId);
            else if (cell is GridDateTime)
                return new GridColumnExpression(cell, _login.cAccId);
            else if (cell is GroupObject)
                return new CalculateGroupObject(cell, _login.cAccId);
            else//GroupDimension
                return new CalculateGroupDimension(cell, _login.cAccId);
        }

        private string HandleDateDimensionLevel(Section section, Cell cell, string dimensionsource, ref Cell dimensioncell)
        {
            if (string.IsNullOrEmpty(dimensionsource) || (cell as IDateTimeDimensionLevel).SupportSwitch)
            {
                dimensioncell = cell;
                dimensionsource = (cell as IDataSource).DataSource.Name;
            }
            return dimensionsource;
        }

        private void SetDimensionLevel(IDateTimeDimensionLevel cell, string datedimension)
        {
            if (!string.IsNullOrEmpty(datedimension))
            {
                cell.DDLevel = GetDateDimensionLevel(datedimension);
            }
        }
        private DateTimeDimensionLevel GetDateDimensionLevel(string datedimension)
        {
            switch (datedimension.ToLower())
            {
                case "datetime":
                    return DateTimeDimensionLevel.时间;
                case "date":
                case "day":
                    return DateTimeDimensionLevel.日;
                case "week":
                    return DateTimeDimensionLevel.周;
                case "month":
                    return DateTimeDimensionLevel.月;
                case "accountmonth":
                    return DateTimeDimensionLevel.会计月;
                case "quarter":
                    return DateTimeDimensionLevel.季;
                case "xun":
                    return DateTimeDimensionLevel.旬;
                default://year
                    return DateTimeDimensionLevel.年;
            }
        }

        private string HandleRowAuthStringAndDateDimensionFilter(FilterArgs filterargs, string dimensionsource)
        {
            if (filterargs == null)
                return "";
            if (bsaexpand)
                return "";
            StringBuilder sb = new StringBuilder();
            if (filterargs.FltSrv.Contains("AutoCurrentDuration") && !string.IsNullOrEmpty(dimensionsource))
            {
                string datepart = filterargs.FltSrv["AutoCurrentDuration"].Value1.ToLower();
                string direction = null;// = "Forward";//向前
                if (filterargs.FltSrv.Contains("DateDirection"))
                    direction = filterargs.FltSrv["DateDirection"].Value1;
                string curduration = "";
                switch (datepart)
                {
                    case "week":
                        if (filterargs.FltSrv.Contains("WeekDuration"))
                            curduration = "WeekDuration";
                        break;
                    case "month":
                        if (filterargs.FltSrv.Contains("MonthDuration"))
                            curduration = "MonthDuration";
                        break;
                    case "accountmonth":
                        datepart = "month";
                        //if (filterargs.FltSrv.Contains("AccountMonthDuration"))
                        //    curduration = "AccountMonthDuration";
                        if (filterargs.FltSrv.Contains("MonthDuration"))
                            curduration = "MonthDuration";
                        break;
                    case "quarter":
                        if (filterargs.FltSrv.Contains("QuarterDuration"))
                            curduration = "QuarterDuration";
                        break;
                    case "year"://year
                        if (filterargs.FltSrv.Contains("YearDuration"))
                            curduration = "YearDuration";
                        break;
                    default:// "day":
                        datepart = "day";
                        if (filterargs.FltSrv.Contains("DayDuration"))
                            curduration = "DayDuration";
                        break;
                }
                double dduration = 10000000;
                if (curduration != "" && !string.IsNullOrEmpty(filterargs.FltSrv[curduration].Value1))
                {
                    if (direction == null)
                        direction = "Forward";
                    dduration = Math.Abs(Convert.ToDouble(filterargs.FltSrv[curduration].Value1));
                }


                if (direction == "Forward")
                {
                    sb.Append("DATEDIFF(");
                    sb.Append(datepart);
                    sb.Append(" ,");
                    sb.Append(dimensionsource);
                    sb.Append(",'");
                    sb.Append(_login.Date);
                    sb.Append("')");
                    sb.Append(">=0");
                    sb.Append(" and ");
                    sb.Append("DATEDIFF(");
                    sb.Append(datepart);
                    sb.Append(" ,");
                    sb.Append(dimensionsource);
                    sb.Append(",'");
                    sb.Append(_login.Date);
                    sb.Append("')");
                    sb.Append("<");
                    sb.Append(dduration);
                }
                else if (direction != null)
                {
                    sb.Append("DATEDIFF(");
                    sb.Append(datepart);
                    sb.Append(" ,");
                    sb.Append(dimensionsource);
                    sb.Append(",'");
                    sb.Append(_login.Date);
                    sb.Append("')");
                    sb.Append("<=0");
                    sb.Append(" and ");
                    sb.Append("DATEDIFF(");
                    sb.Append(datepart);
                    sb.Append(" ,");
                    sb.Append(dimensionsource);
                    sb.Append(",'");
                    sb.Append(_login.Date);
                    sb.Append("')");
                    sb.Append(">-");
                    sb.Append(dduration);
                }


            }

            if (filterargs.Args.Contains("RowAuthString"))
            {
                string authstring = filterargs.Args["RowAuthString"].ToString();
                if (!string.IsNullOrEmpty(authstring))
                {
                    if (sb.Length > 0)
                        sb.Append(" AND ");
                    sb.Append(authstring);
                }
            }

            return sb.ToString();
        }

        #endregion
        //--------------------------------------------
        #region static
        private static int getCharCount(string s, char e)
        {
            string[] a = s.Split(e);
            return a.Length;
        }
        internal static string HandleExpression(DataSources dss, string expression, string prefix, bool bhandlenull)
        {

            expression = expression.ToLower().Trim();
            //交叉后的列
            if (ExpressionService.bExpressionAfterCross(expression))
            {
                int leftc = getCharCount(expression, '[');
                int rightc = getCharCount(expression, ']');

                //if (leftc != rightc)
                //{
                int k = expression.LastIndexOf("]");
                int l = expression.Length;
                string expression1 = expression.Substring(0, k);
                string expression2 = expression.Substring(k, l - k);
                if (expression1.IndexOf("]") > 0)
                {
                    //expression1 = expression1.Replace("]", "]]");
                    //isnull([iamo@@@材料费用@@@半成品],0) +isnull([iamo@@@材料费用@@@材料],0) 不替换
                    //要替换的 isnull([iamo[@@@[材料费用@@@材料]],0) +
                    if (expression1.IndexOf("]") > 0)
                    {
                        expression1 = expression1.Replace("],", "&&|||&&");
                        expression1 = expression1.Replace("]", "]]");
                        expression1 = expression1.Replace("&&|||&&", "],");
                    }
                }
                expression = expression1 + expression2;
                //}
                if (prefix != "" && !prefix.Contains("(") && expression.Contains("isnull(["))
                    return expression.Replace("isnull([", "isnull(" + prefix + "[");
                else if (prefix.Contains("("))
                    return prefix + expression + ")";
                else if (expression.EndsWith("+''"))
                {
                    string s = expression.Substring(0, expression.Length - 3);
                    s = "Convert(nvarchar(300)," + prefix + s + ")" + "+''";
                    return s;
                }
                else
                    return prefix + expression;
            }
            expression = ReplaceAppend(dss, expression, 0);
            string[] expressions = ExpressionService.SplitExpression(expression);
            DataSources dsf = new DataSources();
            SimpleHashtable ht = new SimpleHashtable();
            bool dsrelate = false;
            for (int i = 0; i < expressions.Length; i++)
            {
                if (expressions[i].Trim() != "")
                {
                    //if (dss.RawContains(expressions[i].Trim()))
                    if (dss.Contains(expressions[i].Trim()))
                    {
                        dsrelate = true;
                        string fuc = "!!!@" + i.ToString() + "@!!!";
                        dsf.Add(expressions[i].Trim(), fuc);
                        if (prefix.Contains("("))
                            //ht.Add(fuc, bhandlenull ? (prefix + "isnull([" + expressions[i].Trim() + "],0))") : (prefix + "[" + expressions[i].Trim() + "])"));
                            ht.Add(fuc, bhandlenull ? ("isnull([" + expressions[i].Trim() + "],0)") : ("[" + expressions[i].Trim() + "]"));
                        else
                            ht.Add(fuc, (bhandlenull ? "isnull(" : "") + prefix + "[" + expressions[i].Trim() + "]" + (bhandlenull ? ",0)" : ""));//A.
                    }
                    else
                    {
                        if (expressions[i].Trim() == "1.000" && prefix == "SUM(")
                        {
                            dsrelate = true;
                            string fuc = "!!!@" + i.ToString() + "@!!!";
                            dsf.Add("1CoNsT1", fuc);
                            if (prefix.Contains("("))
                                //ht.Add(fuc, bhandlenull ? (prefix + "isnull(" + expressions[i].Trim() + ",0))") : (prefix + "" + expressions[i].Trim() + ")"));
                                ht.Add(fuc, bhandlenull ? ("isnull(" + expressions[i].Trim() + ",0)") : ("" + expressions[i].Trim()));
                            else
                                ht.Add(fuc, (bhandlenull ? "isnull(" : "") + prefix + "" + expressions[i].Trim() + "" + (bhandlenull ? ",0)" : ""));//A.
                        }
                    }

                    //else if (dss.Contains(expressions[i].Trim()))
                    //{
                    //    expressions[i] = HandleExpression(dss,dss[expressions[i].Trim()].Tag , prefix);
                    //    string fuc = "!!!@" + i.ToString() + "@!!!";
                    //    dsf.Add(expressions[i], fuc);
                    //    ht.Add(fuc, expressions[i].Trim() );//A.
                    //}
                }
            }

            if (!dsrelate && prefix != "" && prefix.Contains("("))
                return prefix + expression + ")";

            #region divide
            expression = ExpressionService.HandleDivide(expression, dsf);
            #endregion

            foreach (string key in dsf.Keys)
                if (key == "1CoNsT1")
                    expression = expression.Replace("(1.000+0)", dsf.GetString(key));
                else
                    expression = expression.Replace(key, dsf.GetString(key));

            foreach (string key in ht.Keys)
                expression = expression.Replace(key, ht[key].ToString());

            if (prefix.Contains("("))
            {
                expression = prefix + "" + expression + ")";
            }
            return expression;
        }
        //循环引用，A=B+c; B=A/d
        internal static string ReplaceAppend(DataSources dss, string expression, int iterativecount)
        {
            expression = expression.ToLower().Trim();
            if (iterativecount > 100)
                throw new Exception("表达式定义中存在循环定义，请检查。");//need resource
            string[] expressions = ExpressionService.SplitExpression(expression);
            DataSources dsf = new DataSources();
            SimpleHashtable ht = new SimpleHashtable();
            for (int i = 0; i < expressions.Length; i++)
            {
                if (expressions[i].Trim() != "")
                {
                    DataSource ds = dss[expressions[i].Trim()];
                    if (ds != null && ds.bAppend)
                    {
                        string old = expressions[i].Trim();
                        string fuc = "!!!@" + i.ToString() + "@!!!";
                        dsf.Add(old, fuc);
                        if (ds.Tag != null)
                        {
                            expressions[i] = ReplaceAppend(dss, ds.Tag, iterativecount + 1);
                            ht.Add(fuc, "(" + expressions[i].Trim() + ")");
                        }
                        else
                            ht.Add(fuc, expressions[i].Trim());
                    }
                }
            }

            foreach (string key in dsf.Keys)
                expression = expression.Replace(key, dsf.GetString(key));

            foreach (string key in ht.Keys)
                expression = expression.Replace(key, ht[key].ToString());

            return expression;
        }

        internal static string CacheFileName(string cacheid)
        {
            string filename = DefaultConfigs.CachePath;
            if (string.IsNullOrEmpty(filename))
                filename = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!filename.EndsWith(@"\"))
                filename += @"\";
            filename += cacheid + @"\";
            if (!Directory.Exists(filename))
                Directory.CreateDirectory(filename);
            return filename;
        }
        #endregion
        //--------------------------------------------
        #region filter again
        //二次过滤
        public void FilterAgain(string cacheid, string solidfilter, ShowStyle style)
        {
            try
            {
                SetLocaleID();
                #region read cache
                ReadCache(cacheid);
                _report.bFromCache = true;
                #endregion

                //#region drop index & levels
                //RemoteDataHelper rdh = new RemoteDataHelper();
                //rdh.DropFromDB(_login.TempDBCnnString, rdh.IndexAndLevelsString(_report.BaseTable, _report.GroupLevels,cacheid ));
                //#endregion

                _report.SolidFilter = solidfilter;

                _handler.SortReport(false, style);

            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
        #endregion

        #region quick sort
        //排序
        public void QuickSortReport(string cacheid, QuickSortSchema sortschema, ShowStyle style)
        {
            try
            {
                SetLocaleID();
                #region read cache
                ReadCache(cacheid);
                if (sortschema.PageSize > 0)
                {
                    _report.PageRecords = sortschema.PageSize;
                }
                _report.bFromCache = true;
                #endregion

                //#region drop index
                //RemoteDataHelper rdh = new RemoteDataHelper();
                //rdh.DropFromDB(_login.TempDBCnnString, rdh.IndexString(_report.BaseTable,cacheid ));
                //#endregion

                _report.SortSchema = sortschema;
                //ClearLevel1GroupSortItems(_report.SortSchema);
                //for (int i = 0; i < sortschema.QuickSortItems.Count; i++)
                //    _report.SortSchema.AddAfterGroup(sortschema.QuickSortItems[i]);
                //    _report.SortSchema.QuickSortItems.Add(sortschema.QuickSortItems[i]);
                _handler.SortReport(true, style);

            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        /// <summary>
        /// 一级分组及无分组进行清空
        /// </summary>
        /// <param name="sortSchema"></param>
        private void ClearLevel1GroupSortItems(QuickSortSchema sortSchema)
        {
            if (_report.bShowDetail)
                return;
            bool isLevel1Group = true;
            foreach (QuickSortItem sort in sortSchema.QuickSortItems)
            {
                if (sort.Level > 1)
                    isLevel1Group = false;
            }
            if (isLevel1Group == true)
            {
                sortSchema.QuickSortItems.Clear();
            }
        }

        #endregion

        #region PageTo
        public void PageTo(string cacheid, int pageindex, int lastindex, ShowStyle style)
        {
            try
            {
                SetLocaleID();
                #region read cache
                ReadCache(cacheid);
                #endregion
                if (lastindex == -1)
                    lastindex = _report.RowsCount - 1;
                _handler.InnerPageTo(pageindex, lastindex, style);

            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
        public void ChangePaseSize(string cacheid, int newPageSize, int pageindex, int lastindex, ShowStyle style)
        {
            try
            {
                SetLocaleID();
                #region read cache
                ReadCache(cacheid);
                _report.PageRecords = newPageSize;
                #endregion
                if (lastindex == -1)
                    lastindex = _report.RowsCount - 1;
                _handler.InnerPageTo(pageindex, lastindex, style);
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
        #endregion

        #region publish
        public string publish(FilterArgs filterargs, bool bAlert)
        {
            string tb = null;
            try
            {
                SetLocaleID();
                InitReportFromFilterArgs(filterargs);

                CreateBaseTable(filterargs, null, null, null, 0);
                tb = _report.BaseTableNoneHeader;
                if (bAlert)
                {
                    int count = (int)SqlHelper.ExecuteScalar(_login.TempDBCnnString, "select count(*) from " + _report.BaseTable);
                    if (count == 0)
                        return null;
                }
                AfterBaseTableCreated(_report.BaseTable, filterargs);

                //sourcefilter
                HandleSourceFilter(filterargs);

                RemoteDataHelper rdh = new RemoteDataHelper();
                _report.ChartStrings = rdh.LoadChartStrings(_login.UfMetaCnnString, _report.ViewID);

                #region move to reportdb
                StringBuilder sb = new StringBuilder();

                if (SqlHelper.ExecuteScalar(_login.TempDBCnnString,
                    " SELECT name FROM dbo.syscolumns WHERE id = OBJECT_ID(N'[" + tb + "]') AND NAME=N'" + _report.BaseID + "'") != null)
                {
                    sb.Append("select * into ");
                }
                else
                {
                    sb.Append("select identity(int ,0,1) as baseid, * into ");
                }

                //sb.Append("UFReport..");
                sb.Append(tb);
                sb.Append(" from ");
                sb.Append(_report.BaseTable);
                sb.Append(";");

                SqlParameter param = null;
                if (_handler.AssemblyBytes != null)
                {
                    //UFReport..
                    sb.Append(" insert into UAP_Report_StaticReport (staticid,report,dynamicassembly) values (@staticid,@report,@dynamicassembly);");
                    param = new SqlParameter("@dynamicassembly", SqlDbType.Image);
                    param.Value = _handler.AssemblyBytes;
                }
                else
                {
                    //UFReport..
                    sb.Append(" insert into UAP_Report_StaticReport (staticid,report) values (@staticid,@report);");
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    if (param != null)
                        cmd.Parameters.Add(param);
                    cmd.CommandText = sb.ToString();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@staticid", tb));
                    param = new SqlParameter("@report", SqlDbType.Image);
                    param.Value = _report.ToBytes();
                    cmd.Parameters.Add(param);

                    SqlHelper.ExecuteNonQuery(_login.UfDataCnnString, cmd);
                }
                #endregion

                return tb;
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
            finally
            {
                DropTmpTableHandler(_login.TempDBCnnString, tb);
                OnDispose();
            }
            return null;
        }

        private void DropTmpTableHandler(string cnnstring, string tablename)
        {
            if (string.IsNullOrEmpty(tablename))
                return;
            tablename = ReportHandler.TableNameNoneHeader(tablename);
            Thread t = new Thread(delegate()
                      {
                          try
                          {
                              StringBuilder sb = new StringBuilder();
                              sb.Append(" if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[");
                              sb.Append(tablename);
                              sb.Append("]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)");
                              sb.Append(" drop table ");
                              sb.Append(tablename);
                              SqlHelper.ExecuteNonQuery(cnnstring, sb.ToString());
                          }
                          catch
                          {
                          }
                      }
                     );
            t.Start();
        }
        #endregion

        #region ComplexChartData
        public DataTable ComplexChartDataByOtherGroup(string cacheid, SimpleHashtable simplesource, SimpleHashtable complexsource, int level, string wherestring, string gouplevle1Str, GroupSchema dependGroup, ArrayList alname)
        {
            try
            {
                #region read cache
                ReadCache(cacheid);
                #endregion

                #region CreateTable
                DataTable dt = new DataTable();
                dt.Columns.Add("x");
                foreach (string key in simplesource.Keys)
                {
                    if (alname.Contains(key))
                    {
                        dt.Columns.Add(new DataColumn(key, typeof(double)));
                    }
                }
                //foreach(string key in alname)
                //    dt.Columns.Add(new DataColumn(key, typeof(double)));
                foreach (string key in complexsource.Keys)
                {
                    string cs = complexsource[key].ToString();
                    if (!cs.StartsWith("D___"))
                        dt.Columns.Add(new DataColumn(cs, typeof(double)));
                    else
                        dt.Columns.Add(new DataColumn(cs.Substring(4), typeof(DateTime)));
                }
                #endregion

                #region filldata
                GroupHeader gh = _report.Sections.GetGroupHeader(level);
                GroupSummary gs = _report.Sections.GetGroupSummary(level);
                StringBuilder sb = new StringBuilder();
                sb.Append("select (");

                SimpleArrayList altmp = dependGroup.SchemaItems[0].Items;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                sb.Append(") as x, * from ");
                sb.Append("  (");
                sb.Append(gouplevle1Str);
                sb.Append(" ) as group_1");
                //sb.Append(_report.BaseTableInTemp);
                //sb.Append("_");
                //sb.Append(level.ToString());
                sb.Append(wherestring);
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString()))
                {
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        Group group = new Group(level, reader);
                        dr["x"] = group["x"];
                        foreach (string key in simplesource.Keys)
                        {
                            if (alname.Contains(key))
                            {
                                object o = group[key];
                                dr[simplesource[key].ToString()] = (o == DBNull.Value ? 0 : o);
                            }
                        }
                        foreach (string key in complexsource.Keys)
                        {
                            string cs = complexsource[key].ToString();
                            if (cs.StartsWith("D___"))
                                cs = cs.Substring(4);
                            Cell cell = null;
                            //cell = gh.Cells[key];
                            //if (cell == null)
                            //    cell = gs.Cells[key];
                            cell = _report.GridDetailCells[key];

                            if (cell != null)
                            {
                                //cell.OldCaption = cell.Caption;
                                //try
                                //{
                                if (cell is GridDecimal)
                                {
                                    cell = new Calculator(cell as GridDecimal);
                                }
                                else if (cell is GridCalculateColumn)
                                {
                                    cell = new Calculator(cell as GridCalculateColumn);
                                }
                                if (cell.PrepaintEvent.Trim().Contains("if(currentgroup==null)".Trim()))
                                    cell.PrepaintEvent = "";
                                if (!string.IsNullOrEmpty(cell.PrepaintEvent))
                                    _handler.PreEvent(cell, group, null);
                                else if (cell is Calculator || cell is IAlgorithm)
                                    _handler.ScriptCalculator(cell, group, null);
                                dr[cs] = (string.IsNullOrEmpty(cell.Caption) ? "0" : cell.Caption);
                                //}
                                //catch
                                //{
                                //    if (cell is Calculator || cell is IAlgorithm)
                                //        _handler.ScriptCalculator(cell, group, null);
                                //    dr[cs] = (string.IsNullOrEmpty(cell.Caption) ? "0" : cell.Caption);
                                //}
                                //cell.Caption = cell.OldCaption;
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                    reader.Close();
                }
                #endregion

                return dt;
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
            finally
            {
                OnDispose();
            }
            return null;
        }
        public DataTable ComplexChartData(string cacheid, SimpleHashtable simplesource, SimpleHashtable complexsource, int level, string wherestring)
        {
            try
            {
                #region read cache
                ReadCache(cacheid);
                #endregion

                #region CreateTable
                DataTable dt = new DataTable();
                dt.Columns.Add("x");
                foreach (string key in simplesource.Keys)
                    dt.Columns.Add(new DataColumn(simplesource[key].ToString(), typeof(double)));
                foreach (string key in complexsource.Keys)
                {
                    string cs = complexsource[key].ToString();
                    if (!cs.StartsWith("D___"))
                        dt.Columns.Add(new DataColumn(cs, typeof(double)));
                    else
                        dt.Columns.Add(new DataColumn(cs.Substring(4), typeof(DateTime)));
                }
                #endregion

                #region filldata
                GroupHeader gh = _report.Sections.GetGroupHeader(level);
                GroupSummary gs = _report.Sections.GetGroupSummary(level);
                StringBuilder sb = new StringBuilder();
                sb.Append("select (");
                ArrayList altmp = _report.GroupStructure[level] as ArrayList;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                sb.Append(") as x, * from ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_");
                sb.Append(level.ToString());
                sb.Append(wherestring);
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString()))
                {
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        Group group = new Group(level, reader);
                        dr["x"] = group["x"];
                        foreach (string key in simplesource.Keys)
                        {
                            object o = group[key];
                            dr[simplesource[key].ToString()] = (o == DBNull.Value ? 0 : o);
                        }
                        foreach (string key in complexsource.Keys)
                        {
                            string cs = complexsource[key].ToString();
                            if (cs.StartsWith("D___"))
                                cs = cs.Substring(4);
                            Cell cell = null;
                            cell = gh.Cells[key];
                            if (cell == null)
                                cell = gs.Cells[key];
                            if (cell != null)
                            {
                                //cell.OldCaption = cell.Caption;
                                //try
                                //{
                                if (cell.PrepaintEvent.Trim().Contains("if(currentgroup==null)".Trim()))
                                    cell.PrepaintEvent = "";
                                if (!string.IsNullOrEmpty(cell.PrepaintEvent))
                                    _handler.PreEvent(cell, group, null);
                                else if (cell is Calculator || cell is IAlgorithm)
                                    _handler.ScriptCalculator(cell, group, null);
                                dr[cs] = (string.IsNullOrEmpty(cell.Caption) ? "0" : cell.Caption);
                                //}
                                //catch
                                //{
                                //    if (cell is Calculator || cell is IAlgorithm)
                                //        _handler.ScriptCalculator(cell, group, null);
                                //    dr[cs] = (string.IsNullOrEmpty(cell.Caption) ? "0" : cell.Caption);
                                //}
                                //cell.Caption = cell.OldCaption;
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                    reader.Close();
                }
                #endregion

                return dt;
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
            finally
            {
                OnDispose();
            }
            return null;
        }

        public DataTable ComplexChartDataWithDependId(string cacheid, SimpleHashtable simplesource, SimpleHashtable complexsource, int level, string wherestring, string dependGroupId)
        {
            try
            {
                #region read cache
                ReadCache(cacheid);
                #endregion

                #region CreateTable
                DataTable dt = new DataTable();
                dt.Columns.Add("x");
                foreach (string key in simplesource.Keys)
                    dt.Columns.Add(new DataColumn(simplesource[key].ToString(), typeof(double)));
                foreach (string key in complexsource.Keys)
                {
                    string cs = complexsource[key].ToString();
                    if (!cs.StartsWith("D___"))
                        dt.Columns.Add(new DataColumn(cs, typeof(double)));
                    else
                        dt.Columns.Add(new DataColumn(cs.Substring(4), typeof(DateTime)));
                }
                #endregion

                #region filldata
                GroupHeader gh = _report.Sections.GetGroupHeader(level);
                GroupSummary gs = _report.Sections.GetGroupSummary(level);
                StringBuilder sb = new StringBuilder();
                sb.Append("select (");
                //ArrayList altmp = _report.GroupStructure[level] as ArrayList;
                SimpleArrayList altmp = _report.GroupSchemas[dependGroupId].SchemaItems[0].Items;
                foreach (string key in altmp)
                {
                    if (sb.Length > 7)
                        sb.Append("+ N' ' + ");

                    int bconvert = -1;
                    DataSource ds = _report.DataSources[key];
                    if (ds != null && ds.Type == DataType.DateTime)
                    {
                        sb.Append("convert(nvarchar(10),");
                        bconvert = 1;
                    }
                    else if (ds != null && ds.Type != DataType.String)
                    {
                        sb.Append("convert(nvarchar(100),");
                        bconvert = 2;
                    }

                    sb.Append("[");
                    sb.Append(key);
                    sb.Append("]");

                    if (bconvert == 1)
                        sb.Append(",120)");
                    else if (bconvert == 2)
                        sb.Append(")");
                }
                sb.Append(") as x, * from ");
                sb.Append(_report.BaseTableInTemp);
                sb.Append("_");
                sb.Append(level.ToString());
                sb.Append(wherestring);
                using (SqlDataReader reader = SqlHelper.ExecuteReader(_login.UfDataCnnString, sb.ToString()))
                {
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        Group group = new Group(level, reader);
                        dr["x"] = group["x"];
                        foreach (string key in simplesource.Keys)
                        {
                            object o = group[key];
                            dr[simplesource[key].ToString()] = (o == DBNull.Value ? 0 : o);
                        }
                        foreach (string key in complexsource.Keys)
                        {
                            string cs = complexsource[key].ToString();
                            if (cs.StartsWith("D___"))
                                cs = cs.Substring(4);
                            Cell cell = null;
                            cell = gh.Cells[key];
                            if (cell == null)
                                cell = gs.Cells[key];
                            if (cell != null)
                            {
                                if (cell.PrepaintEvent.Trim().Contains("if(currentgroup==null)".Trim()))
                                    cell.PrepaintEvent = "";
                                if (!string.IsNullOrEmpty(cell.PrepaintEvent))
                                    _handler.PreEvent(cell, group, null);
                                else if (cell is Calculator || cell is IAlgorithm)
                                    _handler.ScriptCalculator(cell, group, null);
                                dr[cs] = (string.IsNullOrEmpty(cell.Caption) ? "0" : cell.Caption);
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                    reader.Close();
                }
                #endregion

                return dt;
            }
            catch (CanceledException)
            {
            }
            catch (Exception e)
            {
                throw new ReportException(e.Message);
            }
            finally
            {
                OnDispose();
            }
            return null;
        }
        #endregion

        #region designtime
        #region save
        private bool bSimpleSum(OperatorType optype)
        {
            return optype == OperatorType.SUM || optype == OperatorType.MAX || optype == OperatorType.MIN || optype == OperatorType.AVG || optype == OperatorType.ExpressionSUM;
        }
        public void Save(Report report, DataSources dss, ref ComplexView complexview)
        {
            try
            {
                SetLocaleID();
                _report = report;
                _report.InitOther();
                _report.DataSources = dss;
                _datasource = new ReportDataSource(dss);

                XmlDocument doccommon = new XmlDocument();
                XmlDocument doclocale = new XmlDocument();
                SaveSelfActions();
                WriteFormat(report, doccommon, doclocale);

                if (!_report.bShowDetail)
                    _report.bShowDetail = true;

                foreach (Section section in report.Sections)
                {
                    bool hasgroup = false;
                    foreach (Cell cell in section.Cells)
                    {
                        #region normal validate
                        if (cell is IGroup)
                        {
                            hasgroup = true;
                            if (cell is IDataSource && (cell as IDataSource).DataSource.IsEmpty)
                                throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex11", cell.Name);
                        }
                        if (cell is GridDecimal && (cell as GridDecimal).bSummary && (cell as IDataSource).DataSource.IsEmpty)
                            throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex11", cell.Name);
                        if (cell is IAlgorithm && (cell as IAlgorithm).Algorithm.Length == 0)
                            throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex10", cell.Name);
                        if (cell is IExpression && (cell as IExpression).Formula.FormulaExpression.Length == 0)
                            throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex12", cell.Name);
                        if (cell is ICalculateColumn && (cell as ICalculateColumn).Expression.Length == 0)
                            throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex13", cell.Name);

                        if (cell is IMapName)
                        {
                            DataSource ds = _datasource.DataSources[(cell as IMapName).MapName];
                            if (ds != null)
                            {
                                DataType dt = ds.Type;
                                if ((cell is IDateTime || (cell is IBDateTime && (cell as IBDateTime).bDateTime)) && dt != DataType.DateTime)
                                    throw new ResourceReportException("U8.UAP.Services.ReportElements.ReportEngine.必须是日期类型的数据源", cell.Name);
                                if (cell is IDecimal && dt != DataType.Currency && dt != DataType.Decimal && dt != DataType.Int)
                                    throw new ResourceReportException("U8.UAP.Services.ReportElements.ReportEngine.必须是数值类型的数据源", cell.Name);
                                if (cell is IImage && dt != DataType.Image)
                                    throw new ResourceReportException("U8.UAP.Services.ReportElements.ReportEngine.必须是图片类型的数据源", cell.Name);
                            }
                        }
                        #endregion
                    }
                    if (section.SectionType == SectionType.GroupHeader && !hasgroup)
                        throw new ResourceReportException("U8.UAP.Report.MustContainsAGroupItemAtLeast", new string[] { section.Level.ToString() });
                }

                if (report.Type == ReportType.GridReport)
                {
                    #region Grid check
                    GridDetail detail = report.Sections[SectionType.GridDetail] as GridDetail;
                    foreach (Cell cell in detail.Cells)
                    {
                        if (!(cell is SuperLabel))
                            continue;
                        foreach (Cell celltmp in detail.Cells)
                        {
                            if (celltmp == cell || !(celltmp is SuperLabel))
                                continue;
                            if (cell.bUnder(celltmp) || celltmp.bUnder(cell))
                                throw new ResourceReportException("U8.UAP.Report.GridMoreThanTwoLayer", new string[] { cell.Name, celltmp.Name });
                        }
                    }
                    #endregion
                }
                else if (report.Type == ReportType.FreeReport)
                {
                    if (string.IsNullOrEmpty(_report.ChartStrings))
                    {
                        RemoteDataHelper rdh = new RemoteDataHelper();
                        _report.ChartStrings = rdh.LoadChartStrings(_login.UfMetaCnnString, report.ViewID);
                    }
                    #region chart check
                    SimpleArrayList al = new SimpleArrayList();
                    foreach (Section sec in report.Sections)
                    {
                        foreach (Cell cell in sec.Cells)
                        {
                            if (cell is Chart)
                            {
                                if (!report.ChartSchemas.CurrentGroupChart.Contains((cell as Chart).Level))
                                    throw new ResourceReportException("U8.Report.ChartNotDefine", new string[] { (cell as Chart).Level.ToString() });
                            }
                            if (cell is IGroup)
                            {
                                if (al.Contains((cell as IMapName).MapName))
                                    throw new ResourceReportException("U8.Report.DoubleGroupItem", new string[] { (cell as IMapName).MapName });
                                al.Add((cell as IMapName).MapName);
                            }
                        }
                    }
                    foreach (Section sec in report.Sections)
                    {
                        foreach (Cell cell in sec.Cells)
                        {
                            if (cell is ICalculator && al.Contains((cell as IMapName).MapName))
                            {
                                throw new ResourceReportException("U8.Report.GroupItemCalculator", new string[] { (cell as IMapName).MapName, cell.Name });
                            }
                        }
                    }
                    #endregion
                }
                else if (report.Type == ReportType.CrossReport)
                {
                    #region cross check
                    Cells reportheadercells = null;
                    Section reportheader = report.Sections[SectionType.ReportHeader];
                    if (reportheader != null)
                        reportheadercells = reportheader.Cells;
                    Cells rowheadercells = report.Sections[SectionType.CrossRowHeader].Cells;
                    Cells colheadercells = report.Sections[SectionType.CrossColumnHeader].Cells;
                    Cells crosscells = report.Sections[SectionType.CrossDetail].Cells;
                    int chcount = colheadercells.Count;
                    int cdcount = crosscells.Count;
                    if (rowheadercells.Count == 0)
                        throw new ResourceReportException("U8.UAP.Services.ReportElements.CrossReportEngine.至少必须要有一个行标题");
                    if (chcount == 0)
                        throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex4");
                    if (cdcount == 0)
                        throw new ResourceReportException("U8.UAP.Services.ReportElements.Ex5");
                    if (cdcount == 1 && chcount > 2)
                        throw new ResourceReportException("U8.UAP.Report.CrossMoreThanTwo");
                    if (cdcount > 1 && chcount > 1)
                        throw new ResourceReportException("U8.UAP.Report.CrossMoreThanTwo");

                    foreach (Cell cell1 in colheadercells)
                    {
                        foreach (Cell cell2 in colheadercells)
                        {
                            if (cell1 != cell2)
                                CheckCrossComponent(cell1, cell2);
                        }
                        foreach (Cell cell2 in rowheadercells)
                            CheckCrossComponent(cell1, cell2);
                        foreach (Cell cell2 in crosscells)
                            CheckCrossComponent(cell1, cell2);
                        if (reportheadercells != null)
                        {
                            foreach (Cell cell2 in reportheadercells)
                                CheckCrossComponent(cell1, cell2);
                        }
                    }
                    foreach (Cell cell1 in crosscells)
                    {
                        foreach (Cell cell2 in crosscells)
                        {
                            if (cell1 != cell2)
                            {
                                CheckCrossComponent(cell1, cell2);
                                CheckCrossExpression(cell1, cell2);
                            }
                        }
                        foreach (Cell cell2 in rowheadercells)
                            CheckCrossComponent(cell1, cell2);
                        if (reportheadercells != null)
                        {
                            foreach (Cell cell2 in reportheadercells)
                                CheckCrossComponent(cell1, cell2);
                        }
                    }
                    foreach (Cell cell1 in rowheadercells)
                    {
                        foreach (Cell cell2 in rowheadercells)
                        {
                            if (cell1 != cell2)
                                CheckCrossComponent(cell1, cell2);
                        }
                        if (reportheadercells != null)
                        {
                            foreach (Cell cell2 in reportheadercells)
                                CheckCrossComponent(cell1, cell2);
                        }
                    }
                    #endregion
                }
                else if (report.Type == ReportType.IndicatorReport)
                {
                    if (string.IsNullOrEmpty(_report.ChartStrings))
                    {
                        RemoteDataHelper rdh = new RemoteDataHelper();
                        _report.ChartStrings = rdh.LoadChartStrings(_login.UfMetaCnnString, report.ViewID);
                    }
                    Section detail = report.Sections[SectionType.IndicatorDetail];
                    int metrixcount = 0;
                    int chartcount = 0;
                    foreach (Cell cell in detail.Cells)
                    {
                        if (cell is IIndicatorMetrix)// && (string.IsNullOrEmpty((cell as IIndicatorMetrix).Groups) || string.IsNullOrEmpty((cell as IIndicatorMetrix).Indicators)))
                        {
                            metrixcount++;
                            if (string.IsNullOrEmpty((cell as IIndicatorMetrix).Groups))
                                throw new ResourceReportException("U8.UAP.Report.非法指标矩阵");
                            if (!string.IsNullOrEmpty((cell as IIndicatorMetrix).Cross))
                            {
                                if (string.IsNullOrEmpty((cell as IIndicatorMetrix).Indicators))
                                    throw new ResourceReportException("U8.UAP.Report.非法指标矩阵");

                                (cell as IMetrix).InitParts(detail.Cells);
                                foreach (Cell cell1 in (cell as IIndicatorMetrix).GroupParts)
                                {
                                    foreach (Cell cell2 in (cell as IIndicatorMetrix).GroupParts)
                                    {
                                        if (cell1 != cell2)
                                        {
                                            CheckCrossComponent(cell1, cell2);
                                            CheckCrossExpression(cell1, cell2);
                                        }
                                    }
                                    CheckCrossComponent(cell1, (cell as IIndicatorMetrix).CrossPart);
                                    foreach (Cell cell2 in (cell as IIndicatorMetrix).IndicatorParts)
                                    {
                                        CheckCrossComponent(cell1, cell2);
                                    }
                                }
                                foreach (Cell cell1 in (cell as IIndicatorMetrix).IndicatorParts)
                                {
                                    CheckCrossComponent(cell1, (cell as IIndicatorMetrix).CrossPart);
                                    foreach (Cell cell2 in (cell as IIndicatorMetrix).IndicatorParts)
                                    {
                                        if (cell1 != cell2)
                                        {
                                            CheckCrossComponent(cell1, cell2);
                                            CheckCrossExpression(cell1, cell2);
                                        }
                                    }
                                }
                            }

                        }
                        if (metrixcount > 1)
                            throw new ResourceReportException("U8.UAP.Report.Toomoremetrix");
                        if (cell is Chart)
                        {
                            chartcount++;
                            if (string.IsNullOrEmpty((cell as Chart).DataSource))
                                throw new ResourceReportException("U8.UAP.Report.未定义图表数据源", new string[] { cell.Name });
                            Cell cs = detail.Cells[(cell as Chart).DataSource];
                            if (cs == null || !(cs is IIndicatorMetrix))
                                throw new ResourceReportException("U8.UAP.Report.非法图表数据源", new string[] { cell.Name });
                            if (!report.ChartSchemas.CurrentGroupChart.Contains((cell as Chart).Level))
                                throw new ResourceReportException("U8.UAP.Report.图表未进行详细设定", new string[] { cell.Name });
                        }
                        if (chartcount > 1)
                            throw new ResourceReportException("U8.UAP.Report.Toomorechart");
                        if (cell is Gauge)
                        {
                            if (string.IsNullOrEmpty((cell as Gauge).IndicatorName))
                                throw new ResourceReportException("U8.UAP.Report.未定义仪表盘数据源", new string[] { cell.Name });
                            Cell cs = detail.Cells[(cell as Gauge).IndicatorName];
                            if (cs == null || !(cs is CalculatorIndicator))
                                throw new ResourceReportException("U8.UAP.Report.非法仪表盘数据源", new string[] { cell.Name });
                        }
                    }
                }

                #region runtime check(compile)
                InitGroup(null, null);
                NewReportHandlerOfEvent(null, null);
                _handler.SaveCheck();
                #endregion

                #region complexview
                XmlDocument docgroupschemas = null;
                if (report.GroupSchemas != null)
                {
                    docgroupschemas = report.GroupSchemas.ToXml();
                }
                if (complexview != null)
                {
                    complexview.CommonFormat = doccommon.InnerXml;
                    complexview.LocaleFormat = doclocale.InnerXml;
                    if (docgroupschemas != null)
                        complexview.GroupSchemas = docgroupschemas.InnerXml;
                    complexview.BlandScape = report.bLandScape;
                    complexview.PageMargins = report.PageMargins.ConvertToString();
                    complexview.Columns = report.PaperName + "@#$" + report.PrintOption.FixedRowsPerPage.ToString();
                    complexview.bShowDetail = report.PrintOption.PrintProvider == PrintProvider.U8PrintComponent ? true : false;
                    complexview.bMustShowDetail = report.PrintOption.CanSelectProvider;
                    complexview.ViewClass = report.ViewClass;
                    complexview.RowsCount = report.PageRecords;

                    #region get assemblystring
                    try
                    {
                        if (!string.IsNullOrEmpty(_handler.OutputAssembly) && File.Exists(_handler.OutputAssembly))
                        {
                            byte[] bs = File.ReadAllBytes(_handler.OutputAssembly);
                            complexview.AssemblyString = Convert.ToBase64String(bs);
                            Directory.Delete(CacheFileName(_report.CacheID), true);
                        }
                        else
                        {
                            complexview.AssemblyString = "";
                        }
                    }
                    catch
                    {
                    }
                    #endregion
                }
                #endregion
            }
            catch (ReportException re)
            {
                throw re;
            }
            catch (ResourceReportException rre)
            {
                throw rre;
            }
            catch (Exception ex)
            {
                //LogSaveException(ex);
                throw new ReportException(ex.Message);
            }
        }

        /// <summary>
        /// add 12.0 matfb
        /// 保存用户自定义事件到数据库中以便升级
        /// </summary>
        private void SaveSelfActions()
        {
            DeleteOldSelfActions();
            SelfActions selfActions = _report.SelfActions;
            foreach (var selfAction in selfActions)
            {
                var action = selfAction as SelfAction;
                if (action != null)
                    SetSelfActionIntoDb(action);
            }
        }

        /// <summary>
        /// 先删除旧的，再统一执行插入操作
        /// </summary>
        private void DeleteOldSelfActions()
        {
            string delSql = string.Format("delete FROM uap_report_selfaction WHERE viewID = '{0}'", _report.ViewID);
            _datahelper.ExecuteScalarFromMeta(delSql);
        }

        /// <summary>
        /// 将相应报表的用户自定义事件插入DB中
        /// </summary>
        /// <param name="tmpsa"></param>
        private void SetSelfActionIntoDb(SelfAction tmpsa)
        {
            string sql = string.Format(
           @"IF NOT EXISTS (SELECT 1 FROM dbo.UAP_Report_SelfAction  WHERE viewID = N'{13}' AND actionclass = N'{5}' AND name = N'{0}') 
           INSERT INTO dbo.UAP_Report_SelfAction 
           (name ,
            caption ,
            encaption ,
            twcaption ,
            imagestring ,
            actionclass ,
            tooltip ,
            entooltip ,
            twtooltip ,
            bdoubleclickaction ,
            bshowcaption ,
            bneedcontext ,
            ProjectID ,
            viewID,
            cSub_Id)
           VALUES  ( N'{0}' , 
             N'{1}' , 
             N'{2}' ,  
             N'{3}' ,  
             N'{4}' ,  
             N'{5}' ,  
             N'{6}' ,  
             N'{7}' ,  
             N'{8}' ,  
             N'{9}' ,  
             N'{10}' , 
             N'{11}' , 
             N'{12}' , 
             N'{13}' ,
             N'{14}'
        )", tmpsa.Name, tmpsa.Caption, tmpsa.EnCaption,
         tmpsa.TwCaption, tmpsa.ImageString, tmpsa.ActionClass,
         tmpsa.ToolTip, tmpsa.EnTip, tmpsa.TwTip,
         tmpsa.bDoubleClickAction ? 1 : 0,
         tmpsa.bShowCaptionOnToolBar ? 1 : 0,
         tmpsa.bNeedContext ? 1 : 0,
         _report.ProjectID, _report.ViewID,
         _report.SubId.ToLower() == "outu8" ? _report.ViewID.Substring(0, 2) : _report.SubId// 如果是升级上来的老报表这里取不到正确的subId这里需要从viewId取得。
         );
            _datahelper.ExecuteScalarFromMeta(sql);
        }

        private void CheckCrossComponent(Cell cell1, Cell cell2)
        {
            if (cell1 is IMapName && cell2 is IMapName && (cell1 as IMapName).MapName.ToLower() == (cell2 as IMapName).MapName.ToLower())
                throw new ResourceReportException("U8.UAP.Report.CrossComponentInvalid", new string[] { cell1.Name, cell2.Name });
        }

        private void CheckCrossExpression(Cell cell1, Cell cell2)
        {
            if (cell1 is ICalculateColumn && cell2 is ICalculateColumn && (cell1 as ICalculateColumn).Expression.ToLower() == (cell2 as ICalculateColumn).Expression.ToLower())
                throw new ResourceReportException("U8.UAP.Report.CrossComponentInvalid", new string[] { cell1.Name, cell2.Name });
        }

        private void LogSaveException(Exception ex)
        {
            try
            {
                if (!string.IsNullOrEmpty(DefaultConfigs.CachePath))
                {
                    string directory = Path.GetDirectoryName(DefaultConfigs.CachePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    string logfile = DefaultConfigs.CachePath + "reportsave" + Guid.NewGuid().ToString() + ".log";
                    using (FileStream fs = new FileStream(logfile, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                        {
                            sw.WriteLine(System.DateTime.Now.ToString() + " error when save format on server: " + ex.Message);
                            sw.WriteLine(ex.StackTrace);
                            sw.WriteLine("------------------------------------");
                            sw.Flush();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void UpgradeSave(Report report, DataSources dss, ref string commonxml, ref string znxml, ref string twxml, ref string enxml)
        {
            SetLocaleID();
            _report = report;
            _report.InitOther();
            _report.DataSources = dss;
            _datasource = new ReportDataSource(dss);
            XmlDocument doccommon = new XmlDocument();
            XmlDocument doclocale = new XmlDocument();
            XmlDocument doctw = new XmlDocument();
            XmlDocument docen = new XmlDocument();
            //SaveSelfActions();
            WriteFormat(report, doccommon, doclocale, doctw, docen);
            commonxml = doccommon.InnerXml;
            znxml = doclocale.InnerXml;
            twxml = doctw.InnerXml;
            enxml = docen.InnerXml;
        }
        #endregion

        #region design
        public Report DesignReport(ComplexView cv, ColumnCollection cc, string projectId)
        {
            try
            {
                //SetLocaleID(cv.LocaleID);
                SetLocaleID();
                LoadFormat(cv, cc, projectId);
                AdjustLayOut();

                foreach (Section section in _report.Sections)
                    section.AutoLayOutAtDesigntime();

                return _report;
            }
            catch (Exception ex)
            {
                throw new ReportException(ex.Message);
            }
        }

        /// <summary>
        /// 从xml初始化运行时格式元数据
        /// </summary>
        private void InitRuntimeFormat(ComplexView cv)
        {
            _report.RuntimeFormatMeta = new RuntimeFormat();
            if (!string.IsNullOrEmpty(cv.RuntimeFormat))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(cv.RuntimeFormat);
                _report.RuntimeFormatMeta.FromXml(doc.DocumentElement);
            }
        }

        protected void AdjustLayOut()
        {
            foreach (Section section in _report.Sections)
                section.AdjustHeight();
            _report.Sections.calcPosition();
        }
        #endregion

        #region private
        protected void WriteFormat(Report report, XmlDocument doccommon, XmlDocument doclocale)
        {
            WriteFormat(report, doccommon, doclocale, null, null);
        }
        protected void WriteFormat(Report report, XmlDocument doccommon, XmlDocument doclocale, XmlDocument doctw, XmlDocument docen)
        {
            bool bupdate = false;
            XmlElement xecommon = doccommon.CreateElement("Report");
            XmlElement xelocale = doclocale.CreateElement("Report");
            XmlElement xetw = null;
            XmlElement xeen = null;
            if (doctw != null)
            {
                xetw = doctw.CreateElement("Report");
                bupdate = true;
            }
            if (docen != null)
                xeen = docen.CreateElement("Report");

            //common
            xecommon.SetAttribute("Name", report.Name);
            //xecommon.SetAttribute("Version", "");
            xecommon.SetAttribute("Type", Convert.ToInt32(report.Type).ToString());

            xecommon.SetAttribute("bPageByGroup", report.bPageByGroup.ToString());
            xecommon.SetAttribute("SolidGroup", report.SolidGroup);
            xecommon.SetAttribute("SolidGroupStyle", Convert.ToInt32(report.SolidGroupStyle).ToString());

            xecommon.SetAttribute("DesignWidth", report.DesignWidth.ToString());
            xecommon.SetAttribute("PageRecords", report.PageRecords.ToString());
            if (report.Varients.Count > 0)
                xecommon.SetAttribute("GlobalVarients", report.Varients.ToString());
            xecommon.SetAttribute("ShowDetail", report.bShowDetail.ToString());
            xecommon.SetAttribute("MustShowDetail", report.MustShowDetail.ToString());
            xecommon.SetAttribute("ViewClass", report.ViewClass);
            xecommon.SetAttribute("FreeViewStyle", Convert.ToInt32(report.FreeViewStyle).ToString());
            xecommon.SetAttribute("ReportHeaderOption", Convert.ToInt32(report.ReportHeaderOption).ToString());
            xecommon.SetAttribute("PrintProvider", Convert.ToInt32(report.PrintProvider).ToString());
            xecommon.SetAttribute("CanSelectProvider", report.CanSelectProvider.ToString());
            xecommon.SetAttribute("FixedRowsPerPage", report.FixedRowsPerPage.ToString());
            xecommon.SetAttribute("SolidGroup", report.SolidGroup);
            xecommon.SetAttribute("SolidSort", report.SolidSort.ToString());
            xecommon.SetAttribute("FreeColorStyleID", report.FreeColorStyleID);
            xecommon.SetAttribute("SupportDynamicColumn", report.bSupportDynamicColumn.ToString());
            xecommon.SetAttribute("DynamicColumnVisible", report.bDynamicColumnVisible.ToString());
            xecommon.SetAttribute("AllowGroup", report.AllowGroup.ToString());
            xecommon.SetAttribute("AllowCross", report.AllowCross.ToString());

            xecommon.SetAttribute("bShowWhenZero", report.bShowWhenZero.ToString());
            xecommon.SetAttribute("BorderStyle", Convert.ToInt32(report.BorderStyle).ToString());
            xecommon.SetAttribute("BorderColor", report.BorderColor.ToArgb().ToString());

            //if (report.SelfActions.Count > 0)
            //add 12.0 matfb
            //将用户自定义事件写入数据库中保存以便升级，这里注释掉不再保存到format字段中
            //由于有老报表升级需要走此流程，因此还需要继续向reportview表中插入该自定义行为
            //xecommon.SetAttribute("SelfActions", "");

            ////locale
            if (report.SelfActions.Count > 0)
            {
                xelocale.SetAttribute("SelfActions", report.SelfActions.ToString());
            }

            //common
            xecommon.SetAttribute("HelpName", report.HelpInfo.FileName);
            xecommon.SetAttribute("KeyIndex", report.HelpInfo.KeyIndex);
            xecommon.SetAttribute("KeyWord", report.HelpInfo.KeyWord);

            xecommon.SetAttribute("RowFilter", report.RowFilter.ToString());
            xecommon.SetAttribute("bAdjustPrintWidth", report.bAdjustPrintWidth.ToString());
            xecommon.SetAttribute("ReportColorSet", report.ReportColorSet);
            xecommon.SetAttribute("FilterSource", report.FilterSource.Name);

            if (report.InitEvent != null && report.InitEvent != "")
            {
                XmlCDataSection CData;
                CData = doccommon.CreateCDataSection(report.InitEvent);

                XmlElement xalgorithm = doccommon.CreateElement("InitEvent");
                xalgorithm.AppendChild(CData);
                xecommon.AppendChild(xalgorithm);
            }

            if (report.Informations.Count > 0)
            {
                XmlElement xeinformations = doccommon.CreateElement("Informations");
                foreach (Information infor in report.Informations)
                {
                    XmlElement xeinformation = doccommon.CreateElement("Information");
                    xeinformation.SetAttribute("Name", infor.Name);
                    xeinformation.SetAttribute("Handler", infor.InformationHandler);
                    xeinformations.AppendChild(xeinformation);
                }
                xecommon.AppendChild(xeinformations);
            }

            foreach (Section section in report.Sections)
            {
                XmlElement xecommonsection = doccommon.CreateElement("Section");
                XmlElement xelocalesection = doclocale.CreateElement("Section");
                XmlElement xetwsection = null;
                XmlElement xeensection = null;
                if (doctw != null)
                    xetwsection = doctw.CreateElement("Section");
                if (docen != null)
                    xeensection = docen.CreateElement("Section");

                ConvertTo(doccommon, xecommonsection, section);
                ConvertToLocale(xelocalesection, section, bupdate);
                if (xetwsection != null)
                    ConvertToTw(xetwsection, section);
                if (xeensection != null)
                    ConvertToEn(xeensection, section);

                foreach (Cell cell in section.Cells)
                {
                    XmlElement xecommoncell = doccommon.CreateElement("Control");
                    XmlElement xelocalecell = doclocale.CreateElement("Control");

                    XmlElement xetwcell = null;
                    XmlElement xeencell = null;
                    if (doctw != null)
                        xetwcell = doctw.CreateElement("Control");
                    if (docen != null)
                        xeencell = docen.CreateElement("Control");

                    ConvertTo(doccommon, xecommoncell, cell);
                    ConvertToLocale(xelocalecell, cell, bupdate);
                    if (xetwcell != null)
                        ConvertToTw(xetwcell, cell);
                    if (xeencell != null)
                        ConvertToEn(xeencell, cell);

                    xecommonsection.AppendChild(xecommoncell);
                    xelocalesection.AppendChild(xelocalecell);

                    if (xetwsection != null)
                        xetwsection.AppendChild(xetwcell);
                    if (xeensection != null)
                        xeensection.AppendChild(xeencell);
                }

                xecommon.AppendChild(xecommonsection);
                xelocale.AppendChild(xelocalesection);
                if (xetw != null)
                    xetw.AppendChild(xetwsection);
                if (xeen != null)
                    xeen.AppendChild(xeensection);
            }

            if (_report.SubId == "OutU8")
            {
                XmlElement xecommondss = doccommon.CreateElement("DataSource");
                XmlElement xelocaledss = doclocale.CreateElement("DataSource");
                foreach (string key in _report.DataSources.Keys)
                {
                    DataSource ds = _report.DataSources[key];
                    XmlElement xecommonds = doccommon.CreateElement("Column");
                    XmlElement xelocaleds = doclocale.CreateElement("Column");
                    xecommonds.SetAttribute("Name", ds.Name);
                    xecommonds.SetAttribute("Type", ds.Type.ToString());

                    xelocaleds.SetAttribute("Name", ds.Name);
                    xelocaleds.SetAttribute("Caption", ds.Caption);

                    xecommondss.AppendChild(xecommonds);
                    xelocaledss.AppendChild(xelocaleds);
                }

                xecommon.AppendChild(xecommondss);
                xelocale.AppendChild(xelocaledss);
            }

            doccommon.AppendChild(xecommon);
            doclocale.AppendChild(xelocale);

            if (doctw != null)
                doctw.AppendChild(xetw);
            if (docen != null)
                docen.AppendChild(xeen);
        }

        #region covertto
        protected virtual void ConvertToLocale(XmlElement xe, Cell sender, bool bupdate)
        {
            if (!xe.HasAttribute("Name"))
            {
                xe.SetAttribute("Name", sender.Name);
                xe.SetAttribute("Type", sender.Type);
            }
            if (!(sender is Section))
            {
                xe.SetAttribute("X", sender.X.ToString());
                xe.SetAttribute("Y", sender.RelativeY.ToString());
                xe.SetAttribute("Width", sender.Width.ToString());
            }
            xe.SetAttribute("Height", sender.Height.ToString());
            xe.SetAttribute("Caption", sender.Caption);//bupdate?sender.CNCaption:sender.Caption);
            xe.SetAttribute("IdentityCaption", (sender as Rect).GetIdentityCaption());
        }

        protected virtual void ConvertToTw(XmlElement xe, Cell sender)
        {
            if (!xe.HasAttribute("Name"))
            {
                xe.SetAttribute("Name", sender.Name);
                xe.SetAttribute("Type", sender.Type);
            }
            if (!(sender is Section))
            {
                xe.SetAttribute("X", sender.X.ToString());
                xe.SetAttribute("Y", sender.RelativeY.ToString());
                xe.SetAttribute("Width", sender.Width.ToString());
            }
            xe.SetAttribute("Height", sender.Height.ToString());
            xe.SetAttribute("Caption", sender.TWCaption);
            xe.SetAttribute("IdentityCaption", sender.TWCaption);
        }

        protected virtual void ConvertToEn(XmlElement xe, Cell sender)
        {
            if (!xe.HasAttribute("Name"))
            {
                xe.SetAttribute("Name", sender.Name);
                xe.SetAttribute("Type", sender.Type);
            }
            if (!(sender is Section))
            {
                xe.SetAttribute("X", sender.X.ToString());
                xe.SetAttribute("Y", sender.RelativeY.ToString());
                xe.SetAttribute("Width", sender.Width.ToString());
            }
            xe.SetAttribute("Height", sender.Height.ToString());
            xe.SetAttribute("Caption", sender.ENCaption);
            xe.SetAttribute("IdentityCaption", sender.ENCaption);
        }

        protected virtual void ConvertTo(XmlDocument doc, XmlElement xe, Cell sender)
        {
            //ConvertToLocale(xe, sender,false);
            CellTo(doc, xe, sender);
            if (sender is IBoolean)
                IBooleanTo(xe, sender as IBoolean);
            if (sender is ICalculateColumn)
                ICalculateColumnTo(xe, sender as ICalculateColumn);
            if (sender is ICalculateSequence)
                ICalculateSequenceTo(xe, sender as ICalculateSequence);
            if (sender is ICalculator)
                ICalculatorTo(xe, sender as ICalculator);
            if (sender is IDataSource)
                IDataSourceTo(xe, sender as IDataSource);
            if (sender is IDateTime)
                IDateTimeTo(xe, sender as IDateTime);
            if (sender is IDecimal)
                IDecimalTo(xe, sender as IDecimal);
            if (sender is IExchangeRate)
                IExchangeRateTo(xe, sender as IExchangeRate);
            if (sender is IExpression)
                IExpressonTo(xe, sender as IExpression);
            if (sender is IFormat)
                IFormatTo(xe, sender as IFormat);
            if (sender is IImage)
                IImageTo(xe, sender as IImage);
            if (sender is ISection)
                ISectionTo(xe, sender as ISection);
            if (sender is ISort)
                ISortTo(xe, sender as ISort);
            if (sender is IBDateTime)
                IBDateTimeTo(xe, sender as IBDateTime);
            if (sender is IAlgorithm)
                IAlgorithmTo(doc, xe, sender as IAlgorithm);
            if (sender is IAddWhenDesign)
                IAddWhenDesignTo(xe, sender as IAddWhenDesign);
            if (sender is ILabelType)
                ILabelTypeTo(xe, sender as ILabelType);
            if (sender is IGridCollect)
                IGridCollectTo(xe, sender as IGridCollect);
            if (sender is IGridEvent)
                IGridEventTo(xe, sender as IGridEvent);
            if (sender is IMultiHeader)
                IMultiHeaderTo(xe, sender as IMultiHeader);
            if (sender is Chart)
                IChartTo(xe, sender as Chart);
            if (sender is IAutoSequence)
                IAutoSequenceTo(xe, sender as IAutoSequence);
            if (sender is IDateTimeDimensionLevel)
                IDateTimeDimensionLevelTo(xe, sender as IDateTimeDimensionLevel);
            if (sender is IUserDefine)
                IUserDefineTo(xe, sender as IUserDefine);
            if (sender is IGroupHeader)
                IGroupHeaderTo(xe, sender as IGroupHeader);
            if (sender is IAlternativeStyle)
                IAlternativeStyleTo(xe, sender as IAlternativeStyle);
            if (sender is IApplyColorStyle)
                IApplyColorStyleTo(xe, sender as IApplyColorStyle);
            if (sender is IBarCode)
                IBarCodeTo(xe, sender as IBarCode);
            if (sender is IIndicatorMetrix)
                IIndicatorMetrixTo(xe, sender as IIndicatorMetrix);
            if (sender is IIndicator)
                IIndicatorTo(doc, xe, sender as IIndicator);
            if (sender is Gauge)
                IGaugeTo(xe, sender as Gauge);
            if (sender is IGap)
                IGapTo(xe, sender as IGap);
            if (sender is IInformationSender)
                IInformationSenderTo(xe, sender as IInformationSender);
            if (sender is IGroupDimensionStyle)
                IGroupDimensionStyleTo(xe, sender as IGroupDimensionStyle);
            if (sender is ICenterAlign)
                ICenterAlignTo(xe, sender as ICenterAlign);
            if (sender is IMergeStyle)
                IMergeSyleTo(xe, sender as IMergeStyle);
        }
        protected void CellTo(XmlDocument doc, XmlElement xe, Cell sender)
        {
            if (!xe.HasAttribute("Name"))
            {
                xe.SetAttribute("Name", sender.Name);
                xe.SetAttribute("Type", sender.Type);
            }
            xe.SetAttribute("CaptionAlign", Convert.ToInt32(sender.CaptionAlign).ToString());
            xe.SetAttribute("Visible", sender.Visible.ToString());
            xe.SetAttribute("bHidden", sender.bHidden.ToString());
            xe.SetAttribute("KeepPos", sender.KeepPos.ToString());
            xe.SetAttribute("Z_Order", sender.Z_Order.ToString());

            xe.SetAttribute("FontName", sender.ServerFont.FontName);
            xe.SetAttribute("FontSize", sender.ServerFont.FontSize.ToString());
            xe.SetAttribute("FontUnit", Convert.ToInt32(sender.ServerFont.FontUnit).ToString());
            xe.SetAttribute("FontGdiCharSet", sender.ServerFont.GdiCharSet.ToString());
            xe.SetAttribute("FontGdiVerticalFont", sender.ServerFont.GdiVerticalFont.ToString());
            xe.SetAttribute("FontBold", sender.ServerFont.Bold.ToString());
            xe.SetAttribute("FontItalic", sender.ServerFont.Italic.ToString());
            xe.SetAttribute("FontStrikethout", sender.ServerFont.StrikethOut.ToString());
            xe.SetAttribute("FontUnderline", sender.ServerFont.UnderLine.ToString());

            xe.SetAttribute("BackColor", sender.BackColor.ToArgb().ToString());
            xe.SetAttribute("ForeColor", sender.ForeColor.ToArgb().ToString());
            xe.SetAttribute("BorderLeft", sender.Border.Left.ToString());
            xe.SetAttribute("BorderTop", sender.Border.Top.ToString());
            xe.SetAttribute("BorderRight", sender.Border.Right.ToString());
            xe.SetAttribute("BorderBottom", sender.Border.Bottom.ToString());
            xe.SetAttribute("BorderWidth", sender.BorderWidth.ToString());
            xe.SetAttribute("BorderColor", sender.BorderColor.ToArgb().ToString());
            xe.SetAttribute("bControlAuth", sender.bControlAuth.ToString());
            xe.SetAttribute("bSupportLocate", sender.bSupportLocate.ToString());
            xe.SetAttribute("bShowOnX", sender.bShowOnX.ToString());

            if (sender.PrepaintEvent != null && sender.PrepaintEvent != "")
            {
                XmlCDataSection CData;
                CData = doc.CreateCDataSection(sender.PrepaintEvent);

                XmlElement xevent = doc.CreateElement("PrepaintEvent");
                xevent.AppendChild(CData);
                xe.AppendChild(xevent);
            }
        }

        protected void IAutoSequenceTo(XmlElement xe, IAutoSequence sender)
        {
            xe.SetAttribute("bAutoSequence", sender.bAutoSequence.ToString());
        }

        protected void IAlternativeStyleTo(XmlElement xe, IAlternativeStyle sender)
        {
            xe.SetAttribute("BackColor2", sender.BackColor2.ToArgb().ToString());
            xe.SetAttribute("BorderColor2", sender.BorderColor2.ToArgb().ToString());
            xe.SetAttribute("ForeColor2", sender.ForeColor2.ToArgb().ToString());

            xe.SetAttribute("FontName2", sender.ServerFont2.FontName);
            xe.SetAttribute("FontSize2", sender.ServerFont2.FontSize.ToString());
            xe.SetAttribute("FontUnit2", Convert.ToInt32(sender.ServerFont2.FontUnit).ToString());
            xe.SetAttribute("FontGdiCharSet2", sender.ServerFont2.GdiCharSet.ToString());
            xe.SetAttribute("FontGdiVerticalFont2", sender.ServerFont2.GdiVerticalFont.ToString());
            xe.SetAttribute("FontBold2", sender.ServerFont2.Bold.ToString());
            xe.SetAttribute("FontItalic2", sender.ServerFont2.Italic.ToString());
            xe.SetAttribute("FontStrikethout2", sender.ServerFont2.StrikethOut.ToString());
            xe.SetAttribute("FontUnderline2", sender.ServerFont2.UnderLine.ToString());

            xe.SetAttribute("bApplyAlternative", sender.bApplyAlternative.ToString());
        }

        protected void IChartTo(XmlElement xe, Chart sender)
        {
            xe.SetAttribute("Level", sender.Level.ToString());
            xe.SetAttribute("DataSource", sender.DataSource);
        }

        protected void ILabelTypeTo(XmlElement xe, ILabelType sender)
        {
            xe.SetAttribute("LabelType", Convert.ToInt32(sender.LabelType).ToString());
        }

        protected void IBarCodeTo(XmlElement xe, IBarCode sender)
        {
            xe.SetAttribute("BarCodeType", Convert.ToInt32(sender.Symbology).ToString());
        }

        protected void IDateTimeDimensionLevelTo(XmlElement xe, IDateTimeDimensionLevel sender)
        {
            xe.SetAttribute("DDLevel", Convert.ToInt32(sender.DDLevel).ToString());
            xe.SetAttribute("ShowYear", sender.ShowYear.ToString());
            xe.SetAttribute("ShowWeekRange", sender.ShowWeekRange.ToString());
            xe.SetAttribute("SupportSwitch", sender.SupportSwitch.ToString());
        }

        protected void IBooleanTo(XmlElement xe, IBoolean sender)
        {
            xe.SetAttribute("Checked", sender.Checked.ToString());
        }

        protected void IApplyColorStyleTo(XmlElement xe, IApplyColorStyle sender)
        {
            xe.SetAttribute("bApplyColorStyle", sender.bApplyColorStyle.ToString());
        }

        protected void IAddWhenDesignTo(XmlElement xe, IAddWhenDesign sender)
        {
            xe.SetAttribute("bAddWhenDesign", sender.bAddWhenDesign.ToString());
        }

        protected void ICalculateColumnTo(XmlElement xe, ICalculateColumn sender)
        {
            xe.SetAttribute("Expression", sender.Expression);
        }

        protected void ICalculatorTo(XmlElement xe, ICalculator sender)
        {
            xe.SetAttribute("Operator", Convert.ToInt32(sender.Operator).ToString());
            xe.SetAttribute("Unit", sender.Unit.Name);
        }
        protected void ICalculateSequenceTo(XmlElement xe, ICalculateSequence sender)
        {
            xe.SetAttribute("CalculateIndex", sender.CalculateIndex.ToString());
        }
        protected void IDataSourceTo(XmlElement xe, IDataSource sender)
        {
            xe.SetAttribute("DataSource", sender.DataSource.Name);
        }
        protected void IDateTimeTo(XmlElement xe, IDateTime sender)
        {
        }
        protected void IDecimalTo(XmlElement xe, IDecimal sender)
        {
            xe.SetAttribute("Precision", Convert.ToInt32(sender.Precision).ToString());
            xe.SetAttribute("bShowWhenZero", Convert.ToInt32(sender.bShowWhenZero).ToString());
        }
        protected void IExchangeRateTo(XmlElement xe, IExchangeRate sender)
        {
            xe.SetAttribute("ExchangeCode", sender.ExchangeCode);
        }
        protected void IExpressonTo(XmlElement xe, IExpression sender)
        {
            xe.SetAttribute("Formula", sender.Formula.FormulaExpression);
            xe.SetAttribute("FormulaType", Convert.ToInt32(sender.Formula.Type).ToString());
            xe.SetAttribute("Precision", Convert.ToInt32(sender.Precision).ToString());
            xe.SetAttribute("FormatString", sender.FormatString);
            xe.SetAttribute("bDateTime", sender.bDate.ToString());
        }
        protected void IFormatTo(XmlElement xe, IFormat sender)
        {
            xe.SetAttribute("FormatString", sender.FormatString);
        }
        protected void IImageTo(XmlElement xe, IImage sender)
        {
            xe.SetAttribute("ImageString", sender.ImageString);
            xe.SetAttribute("SizeMode", Convert.ToInt32(sender.SizeMode).ToString());
        }
        protected void ISectionTo(XmlElement xe, ISection sender)
        {
            xe.SetAttribute("Level", sender.Level.ToString());
        }
        protected void ISortTo(XmlElement xe, ISort sender)
        {
            xe.SetAttribute("Direction", Convert.ToInt32(sender.SortOption.SortDirection).ToString());
            xe.SetAttribute("Priority", sender.SortOption.Priority.ToString());
        }

        protected void IBDateTimeTo(XmlElement xe, IBDateTime sender)
        {
            xe.SetAttribute("bDateTime", sender.bDateTime.ToString());
        }

        protected void IAlgorithmTo(XmlDocument doc, XmlElement xe, IAlgorithm sender)
        {
            if (sender.Algorithm != null && sender.Algorithm != "")
            {
                XmlCDataSection CData;
                CData = doc.CreateCDataSection(sender.Algorithm);

                XmlElement xalgorithm = doc.CreateElement("Algorithm");
                xalgorithm.AppendChild(CData);
                xe.AppendChild(xalgorithm);
            }
        }

        protected void IGridCollectTo(XmlElement xe, IGridCollect sender)
        {
            xe.SetAttribute("bSummary", sender.bSummary.ToString());
            xe.SetAttribute("bClue", sender.bClue.ToString());
            xe.SetAttribute("bColumnSummary", sender.bColumnSummary.ToString());
            xe.SetAttribute("bCalcAfterCross", sender.bCalcAfterCross.ToString());
        }

        protected void IGridEventTo(XmlElement xe, IGridEvent sender)
        {
            xe.SetAttribute("EventType", Convert.ToInt32(sender.EventType).ToString());
            xe.SetAttribute("bShowAtReal", sender.bShowAtReal.ToString());
        }

        protected void IMultiHeaderTo(XmlElement xe, IMultiHeader sender)
        {
            xe.SetAttribute("SortSource", sender.SortSource.Name);
        }

        protected void IGroupHeaderTo(XmlElement xe, IGroupHeader sender)
        {
            xe.SetAttribute("bShowNullGroup", sender.bShowNullGroup.ToString());
            xe.SetAttribute("bAloneLine", sender.bAloneLine.ToString());
        }

        protected void IUserDefineTo(XmlElement xe, IUserDefine sender)
        {
            if (sender is IDataSource && !(sender as IDataSource).DataSource.IsEmpty)// && _datahelper.bCusName((sender as IDataSource).DataSource.Name))
                sender.UserDefineItem = _datahelper.GetUserDefineCaption((sender as IDataSource).DataSource.Name);

            xe.SetAttribute("UserDefineItem", sender.UserDefineItem);
        }

        protected void IIndicatorTo(XmlDocument doc, XmlElement xe, IIndicator sender)
        {
            if (sender.DetailCompare != null)
                WriteACompareValue("Detail", doc, xe, sender.DetailCompare);
            if (sender.TotalCompare != null)
                WriteACompareValue("Total", doc, xe, sender.TotalCompare);
            if (sender.SummaryCompare != null)
                WriteACompareValue("Summary", doc, xe, sender.SummaryCompare);
        }

        protected void WriteACompareValue(string pre, XmlDocument doc, XmlElement xe, CompareValue cv)
        {
            xe.SetAttribute(pre + "ScriptID", cv.ScriptID);
            xe.SetAttribute(pre + "ViewStyle", Convert.ToInt32(cv.ViewStyle).ToString());
            xe.SetAttribute(pre + "Performance", Convert.ToInt32(cv.Performance).ToString());
            xe.SetAttribute(pre + "FlagOnBadOnly", cv.FlagOnBadOnly.ToString());
            if (!string.IsNullOrEmpty(cv.Expression1))
            {
                XmlCDataSection CData;
                CData = doc.CreateCDataSection(cv.Expression1);

                XmlElement xalgorithm = doc.CreateElement(pre + "Expression1");
                xalgorithm.AppendChild(CData);
                xe.AppendChild(xalgorithm);
            }
            if (!string.IsNullOrEmpty(cv.Expression2))
            {
                XmlCDataSection CData;
                CData = doc.CreateCDataSection(cv.Expression2);

                XmlElement xalgorithm = doc.CreateElement(pre + "Expression2");
                xalgorithm.AppendChild(CData);
                xe.AppendChild(xalgorithm);
            }
        }

        protected void IIndicatorMetrixTo(XmlElement xe, IIndicatorMetrix sender)
        {
            xe.SetAttribute("Groups", sender.Groups);
            xe.SetAttribute("Cross", sender.Cross);
            xe.SetAttribute("Indicators", sender.Indicators);
            xe.SetAttribute("ShowSummary", sender.ShowSummary.ToString());
            xe.SetAttribute("StyleID", sender.StyleID);
            xe.SetAttribute("PageSize", sender.PageSize.ToString());
            xe.SetAttribute("BorderStyle", Convert.ToInt32(sender.BorderStyle).ToString());
        }

        protected void IGaugeTo(XmlElement xe, Gauge sender)
        {
            xe.SetAttribute("GaugeType", Convert.ToInt32(sender.GaugeType).ToString());
            xe.SetAttribute("TemplateIndex", sender.TemplateIndex.ToString());
            xe.SetAttribute("IndicatorName", sender.IndicatorName);
            xe.SetAttribute("NeedleColor", sender.NeedleColor.ToArgb().ToString());
            xe.SetAttribute("NeedleLength", sender.NeedleLength.ToString());
            xe.SetAttribute("TickFontColor", sender.FontColor.ToArgb().ToString());
            xe.SetAttribute("LineColor", sender.LineColor.ToArgb().ToString());
            xe.SetAttribute("TickColor", sender.TickColor.ToArgb().ToString());
            xe.SetAttribute("SectionStart", sender.SectionStart.ToString());
            xe.SetAttribute("SectionEnd", sender.SectionEnd.ToString());
            xe.SetAttribute("TickStart", sender.TickStart.ToString());
            xe.SetAttribute("TickEnd", sender.TickEnd.ToString());
            xe.SetAttribute("TextLoc", sender.TextLoc.ToString());
            xe.SetAttribute("MaxTick", sender.MaxTick.ToString());
            xe.SetAttribute("MinTick", sender.MinTick.ToString());
            xe.SetAttribute("GaugeColor", sender.GaugeColor.ToArgb().ToString());
            xe.SetAttribute("SemiCircle", sender.bSemiCircle.ToString());
        }

        protected void IGapTo(XmlElement xe, IGap sender)
        {
            xe.SetAttribute("GapHeight", sender.GapHeight.ToString());
        }

        protected void ICenterAlignTo(XmlElement xe, ICenterAlign sender)
        {
            xe.SetAttribute("CenterAlign", sender.CenterAlign.ToString());
        }
        protected void IMergeSyleTo(XmlElement xe, IMergeStyle sender)
        {
            xe.SetAttribute("bMergeCell", sender.bMergeCell.ToString());
        }
        protected void IInformationSenderTo(XmlElement xe, IInformationSender sender)
        {
            xe.SetAttribute("InformationID", sender.InformationID);
        }

        protected void IGroupDimensionStyleTo(XmlElement xe, IGroupDimensionStyle sender)
        {
            xe.SetAttribute("UseColumnStyle", sender.UseColumnStyle.ToString());
        }
        #endregion

        #endregion
        #endregion

        #region IDisposable 成员
        public void Canceled()
        {
            if (_cancelevent.WaitOne(0, false))
            {
                _bcanceled = true;
                OnDispose();
            }
        }

        private void CancelLease()
        {
            ILease lease = (ILease)this.GetLifetimeService();
            if (lease != null)
                lease.Renew(TimeSpan.FromSeconds(2));
        }

        //exit
        private void Dispose()
        {

            try
            {
                _semirowcontainer = null;
                #region dispose memory
                if (_report != null)
                {
                    _report.Dispose();
                    _report = null;
                }
                if (_datahelper != null)
                {
                    _datahelper.Dispose();
                    _datahelper = null;
                }
                if (_datasource != null)
                {
                    _datasource.Dispose();
                    _datasource = null;
                }
                _login = null;
                //if (_scripthelper != null)
                //{
                //    _scripthelper.Dispose();
                //    _scripthelper = null;
                //}                    
                //_assembly = null;
                //_assemblybytes = null;
                //if (_tmprows != null)
                //{
                //    _tmprows = null;
                //}
                //if (_datatable != null)
                //{
                //    _datatable.Dispose();
                //    _datatable = null;
                //}
                ////_pageinfos;
                ////_pageinfo;
                //if (_sumcalculators != null)
                //{
                //    _sumcalculators.Clear();
                //    _sumcalculators = null;
                //}
                //if (_avgcalculators != null)
                //{
                //    _avgcalculators.Clear();
                //    _avgcalculators = null;
                //}
                //if (_maxcalculators != null)
                //{
                //    _maxcalculators.Clear();
                //    _maxcalculators = null;
                //}
                //if (_mincalculators != null)
                //{
                //    _mincalculators.Clear();
                //    _mincalculators = null;
                //}
                //if (_normalcolumns != null)
                //{
                //    _normalcolumns.Clear();
                //    _normalcolumns = null;
                //}
                //if (_expandcolumns != null)
                //{
                //    _expandcolumns.Clear();
                //    _expandcolumns = null;
                //}
                //if (_precisions  != null)
                //{
                //    _precisions.Clear();
                //    _precisions  = null;
                //}
                _semirowcontainer = null;
                #endregion

                System.Runtime.Remoting.RemotingServices.Disconnect(this);
                CancelLease();
            }
            catch (Exception)
            {
            }
        }

        private void OnDispose()
        {
            if (_disposeevent.WaitOne(0, false))
            {
                _cancelevent.Reset();
                CommitOpen();
                if (_semirowcontainer != null)
                    _semirowcontainer.Canceled();
                Thread t = new Thread(delegate()
                      {
                          System.Threading.Thread.Sleep(1000);
                          Dispose();
                      }
                     );
                t.Start();
            }
        }

        #endregion

        #region error handle
        private void HandleError(Exception e)
        {
            if (_cancelevent.WaitOne(0, false))
            {
                LogException(e.InnerException == null ? e : e.InnerException);
                OnDispose();
                throw new ReportException(e.InnerException == null ? e.Message : e.InnerException.Message);
            }
        }

        private void LogException(Exception e)
        {
            SqlHelper.WriteLog(e);
            //System.Diagnostics.Trace.WriteLine("Error on server side :" + e.Message + "---" + e.StackTrace);            
        }
        #endregion

        #region preload
        public void PreLoad()
        {
            //try
            //{
            //    _report = new Report();
            //    _report.Type = ReportType.GridReport;
            //    PageTitle pt = new PageTitle();
            //    Label label = new Label(10, 10, " ");
            //    label.Name = "NoCaption";
            //    pt.Cells.Add(label);
            //    _report.Sections.Add(pt);
            //    Detail detail = new Detail();
            //    detail.Cells.Add(new DBText(label));
            //    _report.Sections.Add(detail);
            //    _report.ViewID = "preloadview";
            //    _report.DataSources = new DataSources();
            //    _report.GroupSchemas = new GroupSchemas();
            //    GroupSchema gs = new GroupSchema();
            //    gs.ID = "preloadgsid";
            //    gs.CurrentLocaleId = "zh-CN";
            //    _report.GroupSchemas.Add(gs);
            //    _report.CurrentSchemaID = "preloadgsid";
            //    OnReportGoing(_report);
            //    OnEndAll(null);
            //}
            //catch
            //{
            //}
        }
        #endregion

        #region monitor
        public void ParallelTest(string id)
        {
            try
            {
                TryOpen(id);
                System.Threading.Thread.Sleep(10000);
                if (id == "5" || id == "15" || id == "30")
                    throw new Exception(id);
                _semirowcontainer.EndAll();
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void TryOpen(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;
            _monitorkey = id;
            Request r = ParallelCenter.SubmitARequest(id);
            if (r != null)
            {
                _monitorguid = r.Guid;
                _semirowcontainer.Wait();
                r.Wait();
            }
        }

        private void CommitOpen()
        {
            if (!string.IsNullOrEmpty(_monitorkey))
                ParallelCenter.CommitARequest(_monitorkey, _monitorguid);
        }
        #endregion

        #region event response
        private void AfterCrossResponse()
        {
            AdaptDataSource(null, null, _report.BaseTable);
        }
        #endregion
    }

    internal class PrecisionHelper
    {
        public bool bGroupItem;
        public string GroupName;
        public string OperType;
        public string AsName;
        public string Expression;
        public bool bDecimal;
        public bool bSingleColumn;
        public PrecisionHelper(bool bgroupitem, string groupname, string opertype, string asname, string expression, bool bsingle, bool bdecimal)
        {
            bGroupItem = bgroupitem;
            GroupName = groupname;
            OperType = opertype;
            AsName = asname;
            Expression = expression;
            bDecimal = bdecimal;
            bSingleColumn = bsingle;
        }
    }

    public delegate void SemiRowsComingHandler(SemiRows semirows);
    public delegate void ReportComingHandler(Report report);

    public delegate void AfterCrossHandler();
    public delegate void CheckCanceledHandler();

    public class ServerEventWrapper : MarshalByRefObject
    {
        public event SemiRowsComingHandler SemiRowsComingEvent;
        public event SemiRowsComingHandler EndAllEvent;
        public event ReportComingHandler ReportComingEvent;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void SemiRowsComing(SemiRows semirows)
        {
            if (SemiRowsComingEvent != null)
                SemiRowsComingEvent(semirows);
        }

        public void EndAll(SemiRows semirows)
        {
            if (EndAllEvent != null)
                EndAllEvent(semirows);
        }

        public void ReportComing(Report report)
        {
            if (ReportComingEvent != null)
                ReportComingEvent(report);
        }
    }
}
