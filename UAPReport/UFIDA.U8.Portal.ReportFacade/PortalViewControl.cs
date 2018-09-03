using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Reflection;
using UFIDA.U8.UAP.Services.ReportElements;
using UFIDA.U8.UAP.Services.ReportExhibition;
using UFIDA.U8.UAP.Services.MVCFramework;
using UFIDA.U8.UAP.Services.ColorAssigner;
using UFIDA.U8.UAP.Services.ReportData;
using UFIDA.U8.UAP.Services.ReportTimer;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.Portal.EventBroker;

namespace UFIDA.U8.Portal.ReportFacade
{
    public partial class PortalViewControl : UserControl
    {
        private ClientReportContext _context;
        private SemiRowsContainerPerhaps4Matrix _semirowscontainer;
        private string _publishid;
        private string _staticid;
        private DateTime _datatime;
        private string _title="";
        private string _fresherr;
        private string _eventfilter;
        private bool _brealtime=false;
        private string _currentduration = null;
        private Dictionary<int,string> _durations=null;
        private ManualResetEvent _resizeevent;
        private AutoResetEvent _releaseevent;
        private BarPageHelper _barpagehelper;
        private object _message;
        public event DefaultSettingHandler SettingDefault;
        public event EventHandler ActionsUnabling;
        public event EventHandler ActionsEnabling;

        public PortalViewControl()
        {
            InitializeComponent();
            this.tlbprint.Text = U8ResService.GetResStringEx("U8.UAP.Report.打印");
            this.tlbprintview.Text = U8ResService.GetResStringEx("U8.UAP.Report.打印预览");
            this.tlbrefresh.Text = U8ResService.GetResStringEx("UFIDA.U8.UAP.Services.ReportManagement.刷新");

            this.tlbsetting.Text = U8ResService.GetResStringEx("U8.UAP.Report.Setting");
            this.tlbchart.Text = U8ResService.GetResStringEx("U8.UAP.Services.ReportDesigner.GroupPabel.图表");
            this.tlbmatrix.Text = U8ResService.GetResStringEx("U8.UAP.Report.Data");
            this.tlbquery.Text = U8ResService.GetResStringEx("U8.UAP.Report.Query");

            this.reportcontrol.SendInformation += new SendInformationHandler(reportcontrol_SendInformation);
            this.reportcontrol.Resize += new EventHandler(reportcontrol_Resize);
            _resizeevent = new ManualResetEvent(false);
            _barpagehelper = new BarPageHelper(this.toolStrip1);
            _barpagehelper.PageIndexClick += new EventHandler(_barpagehelper_PageIndexClick);
            _releaseevent = new AutoResetEvent(true);
            this.Disposed += new EventHandler(PortalViewControl_Disposed);
            //UnableToolBarAll();

            PleaseChangeToSilverlight();
        }

        private void PortalViewControl_Disposed(object sender, EventArgs e)
        {
            Release();
        }

        public void Release()
        {
            if (_releaseevent.WaitOne(0, false))
            {
                try
                {
                    if (_semirowscontainer != null)
                        _semirowscontainer.Canceled();

                    #region release auth
                    //UICommonHelper.ReleaseAuth(_context, _context.ViewID, OperationEnum.Query);
                    #endregion

                    #region release cache & db
                    if (_context.Report != null)
                    {
                        RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
                        rdh.DisposeDbAndCache(ClientReportContext.Login.TempDBCnnString, _context.Report.CacheID, _context.TablenameCollection, _context.LevelCollection);
                    }
                    #endregion

                    #region normal release
                    if (_context.Report != null)
                    {
                        _context.Report.Dispose();
                        _context.Report = null;
                    }
                    #endregion

                    GC.Collect();
                }
                catch
                {
                }
            }
        }

        protected  void SetFirstRow()
        {
            reportcontrol.SetFirstRow();
        }

        public void SetFirstRowAndReDraw()
        {
            reportcontrol.SetFirstRowAndReDraw();
        }

        private void SetInfos(string instanceid)
        {
            _staticid = null;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(ClientReportContext.Login.UfMetaCnnString, "select id,createtime from UAP_ReportStaticRpt where PublishID='" + instanceid + "'"))
            {
                if (reader.Read())
                {
                    _staticid = reader["id"].ToString();
                    _datatime = Convert.ToDateTime(reader["CreateTime"]);
                }
            }
            object t=SqlHelper.ExecuteScalar(ClientReportContext.Login.UfMetaCnnString, "select name from uap_reportpublish_lang where localeid='" + ClientReportContext.LocaleID + "' and publishid='" + instanceid + "'");
            if(t!=null)
                _title =t.ToString();
        }

        public string OpenView(ClientReportContext context,string instanceid)
        {
            try
            {
                _context = context;
                _context.bPortalView = true;
                OpenView(instanceid);

                #region waste
                //_context.ViewID = "43866c9f-f43c-4f60-be38-5a6abfb847f5";

                //EngineHelper eh = CreateEngine();
                //CreateArgs e = new CreateArgs();
                //e.bgetsql = true;
                //e.filterflag = "";
                //eh.Engine.LoadFormat(null, _context.ViewID, e.filterflag, e.gsid, e.rawtable, e.basetable, null);
                //ParameterizedThreadStart ts = new ParameterizedThreadStart(eh.CreateReport);
                //Thread t = new Thread(ts);
                //t.Start(e);
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);                
            }
            return _title;
        }

        public void OpenView(ClientReportContext context,string viewid, Dictionary<string, object> message)
        {
            if (message.ContainsKey("cdimcode") && string.IsNullOrEmpty((string)message["cdimcode"]))
                return;
            PleaseChangeToSilverlight();
            return;
            _context = context;
            _context.bPortalView = true;
            string reportid = null;
            string filterid = null;
            string filterclass = null;
            string classname = null;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(ClientReportContext.Login.UfMetaCnnString, "select uap_report.filterid,uap_report.filterclass,uap_report.classname,uap_report.id as reportid from uap_report inner join uap_reportview on uap_report.id=uap_reportview.reportid where uap_reportview.id='"+viewid+"'"))
            {
                if (reader.Read())
                {
                    reportid = reader["reportid"].ToString();
                    filterid = reader["filterid"].ToString();
                    filterclass = reader["filterclass"].ToString(); 
                    classname  = reader["classname"].ToString();
                }
            }
            _context.Initialize(reportid, ReportStates.Browse , null, false);
            _context.FilterArgs.InitClass(null, filterid, filterclass , classname );
            _context.ViewID = viewid;

            RefreshView(true,message);
        }

        public void RefreshView(Dictionary<string, object> message)
        {
            PleaseChangeToSilverlight();
            return;
            RefreshView(false, message);
        }

        private void RefreshView(bool applyauth,Dictionary<string, object> message)
        {
            _message = message;
            InitFilterArgs(message);
            ShowFilter(false);
            RealTimeQuery(applyauth);
        }

        private void RealTimeQuery(bool applyauth)
        {
            try
            {
                UnableToolBarAll();                

                //if(applyauth)
                //    UICommonHelper.ApplyAuth(_context, _context.ViewID, OperationEnum.Query);

                CreateArgs e = new CreateArgs();
                //authstring
                RemoteDataHelper rdh = new RemoteDataHelper();
                e.colauthstring = rdh.GetColAuthString(ClientReportContext.Login.U8Login, _context.ViewID);

                EngineHelper eh = CreateEngine();
                string allcolumns = eh.Engine.LoadFormat(null, _context.ViewID, null, null, null, null, null);
                if (!string.IsNullOrEmpty(allcolumns))
                {
                    string columns = System.Text.RegularExpressions.Regex.Split(allcolumns, "@;@")[0].Replace("[", "").Replace("]", "");
                    RowAuthFacade raf = new RowAuthFacade();
                    string rowauth = raf.GetRowAuth(_context.ViewID,"",columns, ClientReportContext.Login, false);
                    _context.FilterArgs.Args.Add("RowAuthString", rowauth);
                }

                e.bfilter = false;
                e.bgetsql = true;
                ParameterizedThreadStart ts = new ParameterizedThreadStart(eh.CreateReport);
                Thread t = new Thread(ts);
                t.Start(e);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void InitFilterArgs(Dictionary<string, object> message)
        {
            FilterArgs fa = _context.FilterArgs;
            fa.ViewID = _context.ViewID;
            fa.Login = ClientReportContext.Login.U8Login;

            if (message != null)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xe = doc.CreateElement("data");
                doc.AppendChild(xe);
                foreach (string key in message.Keys)
                {
                    XmlElement xei = doc.CreateElement("item");
                    xei.SetAttribute("name", key);

                    object o = message[key];
                    if (!(o is string[]))
                    {
                        xei.SetAttribute("value", o==null?"":o.ToString());
                    }
                    else
                    {
                        string[] os = o as string[];
                        xei.SetAttribute("value", os[0]);
                        xei.SetAttribute("value2", os[1]);
                    }
                    xe.AppendChild(xei);
                }
                fa.Args.Add("filteritems", doc.InnerXml);
            }
        }

        private bool ShowFilter(bool bshow)
        {
            NativeMethods.ReleaseCapture();
            _context.FilterArgs.Canceled = false;
            _context.FilterArgs.bShowUI = bshow;
            object oComObj = null;
            object[] oParams = null;
            try
            {
                oParams = new object[] { _context.FilterArgs };
                oComObj = Activator.CreateInstance(
                    Type.GetTypeFromProgID("ReportService.clsNetService"));
                oComObj.GetType().InvokeMember("Filter",
                    BindingFlags.InvokeMethod,
                    null,
                    oComObj,
                    oParams);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (oComObj != null)
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oComObj);
                oParams = null;
            }
            if (_context.FilterArgs.Canceled)
                return false;
            return true;
        }

        private void OpenView(string instanceid)
        {
            //context.Initialize("SA[__]844b64bd-d26a-4da7-b6df-1b1225549b75", ReportStates.Browse,null,true);
            OpenView(instanceid, null);
        }

        private bool CheckRealTimeQuery(string instanceid)
        {
            _brealtime = false;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(ClientReportContext.Login.UfMetaCnnString, "select GivenTimeType,reportviewid from uap_reportpublish where id='" + instanceid + "'"))
            {
                if (reader.Read())
                {
                    _brealtime = reader["GivenTimeType"].ToString()=="3";
                    _context.ViewID=reader["reportviewid"].ToString();
                }
            }
            return _brealtime;
        }

        private void OpenView(string instanceid,StaticArgs e)
        {
            if (instanceid!=null &&  instanceid.Trim().ToUpper() == "E13B77F9-4122-4BF1-8C38-97313111C36D")
            {
                PortalView();
            }
            else
            {
                _publishid = instanceid;
                if (CheckRealTimeQuery(instanceid))
                {
                    _brealtime = true;
                    PublishService ps = new PublishService(ClientReportContext.Login.U8Login);
                    _context.FilterArgs = ps.GetFilterArgs(_publishid);

                    if (!string.IsNullOrEmpty(_currentduration))
                    {
                        if (_context.FilterArgs.FltSrv.Contains("CurrentDuration"))
                            _context.FilterArgs.FltSrv["CurrentDuration"].Value1 = _currentduration;
                        else if (_context.FilterArgs.FltSrv.Contains("AutoCurrentDuration"))
                            _context.FilterArgs.FltSrv["AutoCurrentDuration"].Value1 = _currentduration;
                    }
                    RealTimeQuery(true);
                }
                else
                {
                    SetInfos(instanceid);
                    if (!string.IsNullOrEmpty(_staticid))
                    {
                        UnableToolBarAll();
                        tlbtime.Text = U8ResService.GetResStringEx("U8.UAP.Report.数据生成时间") + _datatime.ToLongDateString() + " " + _datatime.ToLongTimeString();
                        tlbtime.ToolTipText = tlbtime.Text;
                        _context.Initialize(_staticid, ReportStates.Static, ReportType.IndicatorReport);

                        if (e == null)
                            e = new StaticArgs();
                        e.staticid = _context.StaticID;
                        e.eventfilter = _eventfilter;
                        RunWithCache(e);
                    }
                    else
                    {
                        EmptyView();
                    }
                }
            }
        }

        private void PleaseChangeToSilverlight()
        {
            UnableToolBarAll();
            picwait.SetTipType(TipType.Info);
            picwait.SetInfo("请将该视图迁移到监控台，本场景将不再支持监控视图");
            picwait.Visible = true;
            //_title = U8ResService.GetResStringEx("U8.UAP.Report.ViewViewer");
        }

        private void PortalView()
        {
            UnableToolBarAll();
            picwait.SetTipType(TipType.Info);
            picwait.SetInfo(U8ResService.GetResStringEx("U8.UAP.Report.ChooseView"));
            picwait.Visible = true;
            _title = U8ResService.GetResStringEx("U8.UAP.Report.ViewViewer");
        }

        private void EmptyView()
        {
            tlbrefresh.Enabled = true;
            picwait.SetTipType(TipType.Info);
            picwait.SetInfo(U8ResService.GetResStringEx("U8.UAP.Report.监控数据未生成"));
            picwait.Visible = true;
            UnableToolBarButRefresh();
        }

        private void UnableToolBarButRefresh()
        {
            tlbtime.Text = "";
            tlbtime.ToolTipText = "";
            tlbprint.Enabled = false;
            tlbprintview.Enabled = false;
            //this.toolStripSeparator1.Visible = false;
            this.tlcfilter.Visible = false;
            this.tlcdimension.Visible = false;
            tlbmatrix.Visible = false;
            tlbchart.Visible = false;
            _barpagehelper.RemovePages();
            tlbsetting.Visible = false;
            tlbquery.Visible = false;
            UnableActions();
        }

        protected virtual void UnableActions()
        {
            if (ActionsUnabling != null)
                ActionsUnabling(this, null);
        }

        protected virtual void EnableActions()
        {
            if (ActionsEnabling != null)
                ActionsEnabling(this, null);
        }

        private void UnableToolBarAll()
        {
            UnableToolBarButRefresh();
            tlbrefresh.Enabled = false;
            picwait.SetTipType(TipType.Waiting);
            picwait.SetInfo("");
            picwait.Visible = true;
        }

        private void EnableToolBar()
        {
            tlbprint.Enabled = true;
            tlbprintview.Enabled = true;
            tlbrefresh.Enabled = true;
            if (_message != null)
                tlbsetting.Visible = true;

            EnableFilter();
            EnableDimension();
            EnableQuery();
            picwait.Visible = false;
            if (_context.MatrixOrChart == MatrixOrChart.Chart)
            {
                tlbchart.Visible = false;
                tlbmatrix.Visible = true;
            }
            else if (_context.MatrixOrChart == MatrixOrChart.Matrix)
            {
                tlbchart.Visible = true;
                tlbmatrix.Visible = false;
            }
            EnableActions();
        }

        private void EnableFilter()
        {
            //if (!_context.Report.FilterSource.IsEmpty)
            //{
            //    //this.toolStripSeparator1.Visible = true;
            //    string text = string.Format(U8ResService.GetResStringEx("U8.UAP.Report.FilterOn"), _context.Report.FilterSource.Caption);
            //    this.tlcfilter.ToolTipText =text;

            //    if (tlcfilter.Tag != null)
            //        text = tlcfilter.Tag.ToString();

            //    this.tlcfilter.SelectedIndexChanged-=new EventHandler(tlcfilter_SelectedIndexChanged);
            //    this.tlcfilter.Items.Clear();
            //    this.tlcfilter.Items.Add(text);                
            //    this.tlcfilter.Visible = true;
            //    this.tlcfilter.SelectedIndex = 0;
            //    this.tlcfilter.SelectedIndexChanged += new EventHandler(tlcfilter_SelectedIndexChanged);

            //}
        }

        private void EnableQuery()
        {
            if (_brealtime && _context.FilterArgs!=null && !string.IsNullOrEmpty( _context.FilterArgs.FilterID))
                tlbquery.Visible = true;
        }

        //@dayduration,@weekduration,@monthduration,@quarterduration,@currentduration
        private void EnableDimension()
        {
            if (_context.FilterArgs != null && (_context.FilterArgs.FltSrv.Contains("CurrentDuration") || _context.FilterArgs.FltSrv.Contains("AutoCurrentDuration")))
            {
                if (_context.FilterArgs.FltSrv.Contains("CurrentDuration"))
                    _currentduration = _context.FilterArgs.FltSrv["CurrentDuration"].Value1;//Day,Week,Month,quarter,Year
                else
                    _currentduration = _context.FilterArgs.FltSrv["AutoCurrentDuration"].Value1;

                if (_durations == null)
                {
                    _durations = new Dictionary<int, string>();
                }
                else
                    _durations.Clear();

                this.tlcdimension.SelectedIndexChanged -= new EventHandler(tlcdimension_SelectedIndexChanged);
                this.tlcdimension.Items.Clear();
                int selectedindex = 0;
                using (SqlDataReader reader = SqlHelper.ExecuteReader(ClientReportContext.Login.UfDataCnnString, "select enumindex, enumcode,enumname from aa_enum where enumtype='U8.UAP.Report.DateDimension' and localeid='" + ClientReportContext.LocaleID + "'"))
                {
                    while (reader.Read())
                    {
                        int index=Convert.ToInt32(reader["enumindex"]);
                        string code=reader["enumcode"].ToString();                        
                        string name = reader["enumname"].ToString();
                        _durations.Add(index,code );
                        this.tlcdimension.Items.Add(name);
                        if (code.ToLower() == _currentduration.ToLower())
                            selectedindex = index;
                    }
                }

                this.tlcdimension.SelectedIndex = selectedindex;
                
                this.tlcdimension.Visible = true;
                this.tlcdimension.SelectedIndexChanged += new EventHandler(tlcdimension_SelectedIndexChanged);
            }
            else
            {
                this.tlcdimension.Visible = false;
            }
        }

        private void tlcdimension_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_durations != null)
            {
                _currentduration = _durations[tlcdimension.SelectedIndex];
                Refresh();
            }
        }

        private void RunWithCache(StaticArgs  e)
        {
            EngineHelper eh = CreateEngine();
            ParameterizedThreadStart ts = new ParameterizedThreadStart(eh.OpenStatic);
            Thread t = new Thread(ts);
            t.Start(e);
        }

        private EngineHelper CreateEngine()
        {
            if (_semirowscontainer != null)
                _semirowscontainer.Canceled();

            IndicatorEngineHelper ceh = new IndicatorEngineHelper(this, _context);
            ceh.ShowUIEvent += new ShowUIDelegate(ceh_ShowUIEvent);
            ceh.ErrorEvent += new ErrorHandler(ceh_ErrorEvent);
            ceh.BindingMetrixSyleEvent += new ReportComingHandler(ceh_BindingMetrixSyleEvent);
            ceh.HasDrillDefinedEvent += new HasDrillDefined(ceh_HasDrillDefinedEvent);
            _semirowscontainer = ceh.Container;
            return ceh;
        }

        private bool ceh_HasDrillDefinedEvent(string viewid, string matrixname)
        {
            DrillDownHelper dd = new DrillDownHelper();
            return dd.GetReportsToDrill(viewid, matrixname).Count !=0 ;
        }

        private void ceh_BindingMetrixSyleEvent(Report report)
        {
            ColorStyleHandler csh = new ColorStyleHandler();
            IColorStyle cs = ReportFontColorHelper.GetColorStyle(ClientReportContext.Login.UfMetaCnnString, report.ColorStyleID);
            csh.ApplyColorStyle(report, cs);
        }

        private void ceh_ErrorEvent(string code,string msg)
        {
            ShowErrorMessage(msg);
        }

        private void ShowErrorMessage(string msg)
        {
            UnableToolBarAll();
            picwait.Visible = true;
            picwait.SetTipType(TipType.Error);
            picwait.SetInfo(msg);
        }

        private void ceh_ShowUIEvent(object param)
        {
            _resizeevent.Set();
            EnableToolBar();
            if (_context.FilterArgs != null)
                _context.FilterArgs.Args.Remove("RefreshInvoked");
            
            (this.reportcontrol as ControlView).initView(new object[] { _context, _semirowscontainer, this });
            SetFirstRow();
            reportcontrol_Resize(null, null);
        }

        private void _barpagehelper_PageIndexClick(object sender, EventArgs e)
        {
            _semirowscontainer.SetPage(Convert.ToInt32(sender));
            (this.reportcontrol as IReportControl).ReDraw(_semirowscontainer);
        }

        private void reportcontrol_Resize(object sender, EventArgs e)
        {
            if (_semirowscontainer ==null || !_semirowscontainer.b4Matrix || !_resizeevent.WaitOne(0,false))
                return;
            int pagerows = 1;
            int headerheight = 24;
            foreach (Cell cell in _context.Report.Sections[SectionType.PageTitle].Cells)
            {
                if (cell is SuperLabel)
                    headerheight = 48;
            }
            if (this.reportcontrol.Height >= headerheight+16+24)
                pagerows = (this.reportcontrol.Height - headerheight - 16) / 24;
                
            int pages = (_semirowscontainer.RowsCount % pagerows == 0) ? (_semirowscontainer.RowsCount / pagerows) : (_semirowscontainer.RowsCount / pagerows + 1);
            //_semirowscontainer.SetPageSize(pagerows);
            //AddPageButtons(pages);
            //_semirowscontainer.SetPage(0);
            //if (this.toolStrip1.Items.ContainsKey("1"))
            //    this.toolStrip1.Items["1"].ForeColor = Color.Red;
            //(this.reportcontrol as IReportControl).ReDraw(_semirowscontainer);
            _barpagehelper.SetPages(pages);
        }        

        private void tlbprint_Click(object sender, EventArgs e)
        {
            try
            {
                UICommonHelper ch = null;
                if (_semirowscontainer.b4Matrix)
                    ch = new UICommonHelper(_context, _semirowscontainer);
                else
                    ch = new UICommonHelper(_context, this);
                ch.Print();
            }
            catch (Exception ex)
            {
                ReportViewControl.ShowInformationMessageBox(ex.Message);
            }
        }

        private void tlbprintview_Click(object sender, EventArgs e)
        {
            try
            {
                UICommonHelper ch = null;
                if (_semirowscontainer.b4Matrix)
                    ch = new UICommonHelper(_context, _semirowscontainer);
                else
                    ch = new UICommonHelper(_context, this);
                ch.PrintView();
            }
            catch (Exception ex)
            {
                ReportViewControl.ShowInformationMessageBox(ex.Message);
            }
        }
        
        private void tlbrefresh_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(U8ResService.GetResStringEx("U8.UAP.Report.RefreshConfirm"), String4Report.GetString("Report"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (_context.FilterArgs != null)
                    _context.FilterArgs.Args.Add("RefreshInvoked", 1);
                Refresh();
            }
        }

        private void Refresh()
        {
            _fresherr = null;
            UnableToolBarAll();

            if (_message != null)
            {
                RefreshView(_message as Dictionary<string, object>);
            }
            else
            {
                Thread t = new Thread(delegate()
                {
                    try
                    {
                        PublishService ps = new PublishService(ClientReportContext.Login.U8Login);
                        ps.RunTask(_publishid);
                    }
                    catch (Exception fex)
                    {
                        _fresherr = fex.Message;
                    }
                    this.BeginInvoke(new FreshDelegate(Fresh));
                }
                     );
                t.Start();
            }
        }

        private delegate void FreshDelegate();
        private void Fresh()
        {
            try
            {
                tlcfilter.Tag = null;
                if (!string.IsNullOrEmpty(_fresherr))
                    throw new Exception(_fresherr);
                OpenView(_publishid);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void RefreshNow()
        {
            if (_message != null)
                RefreshView(_message as Dictionary<string,object>);
            else
                OpenView(_publishid);
        }

        private void tlbquery_Click(object sender, EventArgs e)
        {
            object rawfilter = _context.FilterArgs.RawFilter;
            if(rawfilter !=null)
                InitFilterArgs(rawfilter as UFGeneralFilter.FilterSrv);

            _context.FilterArgs.RawFilter = null;
            _context.FilterArgs.Args.Remove("ufreportarg");
            if (ShowFilter(true))
                RealTimeQuery(true);
            else
                _context.FilterArgs.RawFilter = rawfilter;
        }

        private void InitFilterArgs(UFGeneralFilter.FilterSrv fltsrv)
        {
            FilterArgs fa = _context.FilterArgs;
            XmlDocument doc = new XmlDocument();
            XmlElement xe = doc.CreateElement("data");
            doc.AppendChild(xe);
            for (int i = 1; i <= fltsrv.FilterList.Count;i++ )
            {
                XmlElement xei = doc.CreateElement("item");
                xei.SetAttribute("name", fltsrv.FilterList[i].Key);
                xei.SetAttribute("value", fltsrv.FilterList[i].varValue);
                xei.SetAttribute("value2", fltsrv.FilterList[i].varValue2);                 
                xe.AppendChild(xei);
            }
            fa.Args.Add("filteritems", doc.InnerXml);
        }

        private void tlbsetting_Click(object sender, EventArgs e)
        {
            if (SettingDefault != null)
                SettingDefault();
        }        

        private void tlbmatrix_Click(object sender, EventArgs e)
        {
            _context.MatrixOrChart = MatrixOrChart.Matrix;
            RefreshNow();
        }

        private void tlbchart_Click(object sender, EventArgs e)
        {
            _context.MatrixOrChart = MatrixOrChart.Chart;
            RefreshNow();
        }          

        private void tlcfilter_DropDown(object sender, EventArgs e)
        {
            try
            {
                this.tlcfilter.Items.Clear();
                this.tlcfilter.Items.Add(U8ResService.GetResStringEx("U8.UAP.Report.All"));
                RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
                DataTable dt = rdh.GetIndicatorFilter(_context.Report.FilterSource.Name, _context.Report.BaseTable,_eventfilter , ClientReportContext.Login.UfDataCnnString);
                foreach (DataRow dr in dt.Rows)
                {
                    this.tlcfilter.Items.Add(dr[0].ToString());
                }                
            }
            catch (Exception ex)
            {
                ReportViewControl.ShowInformationMessageBox(ex.Message);
            }
        }

        private void tlcfilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            tlcfilter.Tag = tlcfilter.SelectedItem.ToString();
            StaticArgs args = new StaticArgs();
            if (tlcfilter.SelectedIndex > 0)
            {
                args.uifilter = _context.Report.FilterSource.Name + "='" + tlcfilter.SelectedItem.ToString() + "'";
            }
            OpenView(_publishid, args );
        }      

        private void reportcontrol_SendInformation(string informationid, string information)
        {
            if (SendReportInformation != null )
            {
                InformationData infor = new InformationData(informationid ,information );
                infor.Tag = _message;
                SendReportInformation(this, new DataEventArgs<InformationData>(infor));
            }
        }

        [EventPublication("topic://u8/portal/MonitorReportInformation")]
        public event EventHandler<DataEventArgs<InformationData>> SendReportInformation;

        [EventSubscription("topic://u8/portal/MonitorViewList")]
        public void refreshMonitorViewHandler(object sender, DataEventArgs<string> args)
        {
            tlcfilter.Tag = null;
            OpenView(args.Data);
        }

        [EventSubscription("topic://u8/portal/MonitorReportInformation")]
        public void refreshMonitorReportInformationHandler(object sender, DataEventArgs<InformationData> args)
        {
            if (_context!=null && _context.Report!=null && _context.Report.Informations.Contains(args.Data.ID ))
            {
                tlcfilter.Tag = null;
                _context.Report.SetInformationString(args.Data.ID  , args.Data.Value );
                _eventfilter = _context.Report.InformationString;
                OpenView(_publishid);
            }
        }        
    }
}
