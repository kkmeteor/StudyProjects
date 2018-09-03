using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportData;
using System.Runtime.InteropServices;
using System.IO;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class SemiRowsContainer
    {
        private Queue<SemiRows> _semirows;
        private ReportEngine _engine;
        private ManualResetEvent _cancelevent;
        private ManualResetEvent _completeevent;
        private Semaphore _newobjectsemaphone;
        private WaitHandle[] _exitwaithandles;
        private WaitHandle[] _getobjectwaithandles;

        protected DrillData _drilltag = null;
        protected SimpleHashtable _cellnametocolumnname = null;

        public event ReportComingHandler ReportComing;
        public event SemiRowsComingHandler SemiRowsComing;
        public event EventHandler EndAll;
        public event ErrorHandler ErrorOccured;
        public event EventHandler Waiting;

        protected SemiRowsContainer()
        {
        }

        public SemiRowsContainer(ReportEngine engine)
        {
            _semirows = new Queue<SemiRows>();
            _cancelevent = new ManualResetEvent(false);
            _completeevent = new ManualResetEvent(false);
            _newobjectsemaphone = new Semaphore(0, Int32.MaxValue);
            _exitwaithandles = new WaitHandle[] { _completeevent, _cancelevent };
            _getobjectwaithandles = new WaitHandle[] { _newobjectsemaphone, _completeevent, _cancelevent };

            _engine = engine;
            Thread t = new Thread(GetObject);
            t.Start();

            t = new Thread(SendFlag);
            t.Start();
        }

        public class GetObjectException : Exception
        {
        }

        private void GetObject()
        {
            try
            {
                int flag = 2;
                while (flag != 0 && flag != 1)
                {
                    object o = null;
                    try
                    {
                        o = _engine.GetObject();
                    }
                    catch (Exception ex)
                    {
                        Logger logger = Logger.GetLogger("SemiRowsContainer");
                        logger.Info("GetObject Catch Exception ex:" + ex.Message);
                        logger.Info("GetObject Catch InnerException ex:" + ex.InnerException.Message);
                        logger.Close();
                        System.Diagnostics.Trace.WriteLine("GetObject Catch Exception ex:" + ex.Message);
                        throw ex;
                        //throw new GetObjectException();
                    }

                    if (o is Report)
                        OnReportComing(o as Report);
                    else if (o is SemiRows)
                        OnSemiRowsComing(o as SemiRows);
                    else if (o != null && o.ToString() == "")
                        OnEndAll();
                    else if (o != null && o.ToString() == "WAIT")
                        OnWaiting();
                    else//error on server or canceled by client
                    {
                        InnerCanceled();
                    }
                    flag = WaitHandle.WaitAny(_exitwaithandles, 0, false);
                }
                //System.Diagnostics.Trace.WriteLine("Report client: End get object from server");
            }
            catch (GetObjectException)
            {
                _engine = null;
                InnerCanceled();
            }
            catch (CodeException e)
            {
                OnError(e.Code, e.Message);
            }
            catch (Exception e)
            {
                OnError(null, e.Message);
                System.Diagnostics.Trace.WriteLine("Error on client get object");
            }
        }

        private void OnReportComing(Report r)
        {
            if (ReportComing != null)
                ReportComing(r);
        }
        private void OnSemiRowsComing(SemiRows srs)
        {
            if (SemiRowsComing != null)
                SemiRowsComing(srs);
        }
        private void OnEndAll()
        {
            try
            {
                _completeevent.Set();
                if (EndAll != null)
                    EndAll(null, null);
            }
            finally
            {
                _engine = null;
            }
        }

        private void SendFlag()
        {
            AutoResetEvent flag = new AutoResetEvent(false);
            while (!flag.WaitOne(2000, false))
            {
                if (_engine == null)
                    flag.Set();
                else
                {
                    try
                    {
                        _engine.RequireFlag();
                    }
                    catch
                    {
                        flag.Set();
                    }
                }
            }
        }

        private void OnWaiting()
        {
            if (Waiting != null)
                Waiting(null, null);
        }

        private void OnError(string code, string msg)
        {
            if (ErrorOccured != null)
                ErrorOccured(code, msg);
        }

        public virtual bool b4Matrix
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public void SetDrillTag(DrillData dd, SimpleHashtable cellnametocolumnname)
        {
            _drilltag = dd;
            _cellnametocolumnname = cellnametocolumnname;
        }

        public void RefreshDrillTag(Row row)
        {
            foreach (Cell cell in row.Cells)
            {
                if (_cellnametocolumnname.Contains(cell.Name))
                    _drilltag.Add(_cellnametocolumnname[cell.Name].ToString(), cell.Caption);
            }
        }

        public DrillData DrillTag
        {
            get
            {
                return _drilltag;
            }
        }

        public virtual void AddSemiRows(SemiRows semirows)
        {
            lock ((_semirows as ICollection).SyncRoot)
            {
                _semirows.Enqueue(semirows);
            }
            //Thread.Sleep(100);
            _newobjectsemaphone.Release();
        }

        public virtual SemiRows GetABlockWait()
        {
            SemiRows semirows = null;
            int r = WaitHandle.WaitAny(_getobjectwaithandles);
            System.Diagnostics.Trace.WriteLine("GetABlockWait........." + r.ToString());
            if (r == 0)
            {
                lock ((_semirows as ICollection).SyncRoot)
                {
                    semirows = _semirows.Dequeue();
                }
            }
            return semirows;
        }

        public virtual SemiRows GetABlock()
        {
            SemiRows semirows = null;
            if (_newobjectsemaphone.WaitOne(0, false))
            {
                lock ((_semirows as ICollection).SyncRoot)
                {
                    semirows = _semirows.Dequeue();
                }
            }
            return semirows;
        }

        public virtual bool bComplete
        {
            get
            {
                bool bcanceled = _cancelevent.WaitOne(0, false);
                bool bcompleted = _completeevent.WaitOne(0, false);
                bool allobjectremoved = false;
                lock ((_semirows as ICollection).SyncRoot)
                {
                    allobjectremoved = (_semirows.Count == 0);
                }
                return bcanceled || (bcompleted && allobjectremoved);
            }
        }

        public virtual bool bCanceled
        {
            get
            {
                return _cancelevent.WaitOne(0, false);
            }
        }

        private void InnerCanceled()
        {
            if (_cancelevent != null && !_cancelevent.WaitOne(0, false))
            {
                _cancelevent.Set();
                while (_newobjectsemaphone.WaitOne(0, false))
                {
                    lock ((_semirows as ICollection).SyncRoot)
                    {
                        _semirows.Dequeue();
                    }
                }
            }
        }
        public virtual void Canceled()
        {
            InnerCanceled();
            CancelEngine();
        }

        private void CancelEngine()
        {
            if (_engine != null)
            {
                Thread t = new Thread(delegate()
                {
                    try
                    {
                        _engine.Canceled();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _engine = null;
                    }
                });
                t.Start();
            }
        }
    }

    public class SemiRowsContainerPerhaps4Matrix : SemiRowsContainer
    {
        private SemiRows _semirows4matrix;
        private int _pageindex = -1;
        private int _pagesize = Int32.MaxValue;
        private bool _really4matrix = false;

        protected SemiRowsContainerPerhaps4Matrix()
        {
            _semirows4matrix = new SemiRows();
        }

        public SemiRowsContainerPerhaps4Matrix(ReportEngine engine)
            : base(engine)
        {
            _semirows4matrix = new SemiRows();
        }

        public SemiRowsContainerPerhaps4Matrix(int pagesize)
            : this()
        {
            _pageindex = 0;
            _pagesize = pagesize;
            if (_pagesize > 100 || _pagesize == 0)
                _pagesize = 100;
            _really4matrix = true;
        }

        public override bool b4Matrix
        {
            get
            {
                return _really4matrix;
            }
            set
            {
                _really4matrix = value;
            }
        }

        public SemiRows SemiRows
        {
            get
            {
                return _semirows4matrix;
            }
        }

        //public void Be4Matrix()
        //{
        //    _really4matrix = true;
        //}

        public int RowsCount
        {
            get
            {
                return _semirows4matrix.Count;
            }
        }

        public void SetPage(int pageindex)
        {
            _pageindex = pageindex;
        }

        public override SemiRows GetABlock()
        {
            if (!_really4matrix)
                return base.GetABlock();
            if (_pageindex == -1)
                return _semirows4matrix;

            int beginindex = _pageindex * _pagesize;
            SemiRows srs = new SemiRows();
            int endindex = Math.Min(_semirows4matrix.Count, beginindex + _pagesize);
            for (int i = beginindex; i < endindex; i++)
                srs.Add(_semirows4matrix[i]);
            return srs;
        }

        public override void AddSemiRows(SemiRows semirows)
        {
            if (!_really4matrix)
            {
                base.AddSemiRows(semirows);
                return;
            }
            if (semirows == null)
                return;
            foreach (SemiRow sr in semirows)
                _semirows4matrix.Add(sr);
        }

        public override bool bCanceled
        {
            get
            {
                if (!_really4matrix)
                    return base.bCanceled;
                return false;
            }
        }
        public override bool bComplete
        {
            get
            {
                if (!_really4matrix)
                    return base.bComplete;
                return true;
            }
        }
        public override SemiRows GetABlockWait()
        {
            if (!_really4matrix)
                return base.GetABlockWait();
            return GetABlock();
        }
        public override void Canceled()
        {
            base.Canceled();
            if (_semirows4matrix != null)
            {
                _semirows4matrix.Clear();
                _semirows4matrix = null;
            }
        }
    }

    public class SemiRowsContainerOnServer
    {
        private Queue<object> _semirows;
        private ManualResetEvent _cancelevent;
        private Semaphore _newobjectsemaphore;
        private WaitHandle[] _getobjectwaithandles;
        //由于输出时随着时间增加会造成内存溢出，这里做一个记数，如果多于10W行之后，每次数据进入队列先使线程sleep一下。
        private int i = 0;
        private int time = 500;
        private INIClass _iniClass;

        public SemiRowsContainerOnServer()
        {
            string iniPath = AppDomain.CurrentDomain.BaseDirectory + "UAPReportConfig.ini";
            this._iniClass = new INIClass(iniPath);
            int.TryParse(this._iniClass.IniReadValue("SemiRowsContainerOnServer", "Sleep"),out time);
            _semirows = new Queue<object>();
            _cancelevent = new ManualResetEvent(false);
            _newobjectsemaphore = new Semaphore(0, Int32.MaxValue);
            _getobjectwaithandles = new WaitHandle[] { _newobjectsemaphore, _cancelevent };
        }

        public void AddReport(Report report)
        {
            lock ((_semirows as ICollection).SyncRoot)
            {
                _semirows.Enqueue(report);
            }
            _newobjectsemaphore.Release();
        }

        public void AddSemiRows(SemiRows semirows)
        {
            lock ((_semirows as ICollection).SyncRoot)
            {
                _semirows.Enqueue(semirows);
            }
            if (i > 1000)
                Thread.Sleep(time);
            while (_semirows.Count > 10)
            {
                System.Diagnostics.Trace.WriteLine("警告：semirows中已有semirow数量为" + _semirows.Count);
                Thread.Sleep(time);
            }
            i++;
            _newobjectsemaphore.Release();
        }

        public void EndAll()
        {
            lock ((_semirows as ICollection).SyncRoot)
            {
                _semirows.Enqueue("");
            }
            _newobjectsemaphore.Release();
        }

        public void Wait()
        {
            lock ((_semirows as ICollection).SyncRoot)
            {
                _semirows.Enqueue("WAIT");
            }
            _newobjectsemaphore.Release();
        }

        //public void OnError(string msg)
        //{
        //    while (_newobjectsemaphore.WaitOne(0, false))
        //    {
        //        lock ((_semirows as ICollection).SyncRoot)
        //        {
        //            _semirows.Dequeue();
        //        }
        //    }
        //    if (!_cancelevent.WaitOne(0, false))
        //    {
        //        lock ((_semirows as ICollection).SyncRoot)
        //        {
        //            _semirows.Enqueue(msg);
        //        }
        //        _newobjectsemaphore.Release();
        //    }
        //    _cancelevent.Set();
        //}

        public object GetObject()
        {
            object o = null;
            if (WaitHandle.WaitAny(_getobjectwaithandles) != 1)
            {
                lock ((_semirows as ICollection).SyncRoot)
                {
                    o = _semirows.Dequeue();
                }
            }
            return o;
        }

        public void Canceled()
        {
            _cancelevent.Set();
            while (_newobjectsemaphore.WaitOne(0, false))
            {
                lock ((_semirows as ICollection).SyncRoot)
                {
                    _semirows.Dequeue();
                }
            }
        }
    }
    public class INIClass
    {
        public string Inipath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
                                                          int size, string filePath);

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="iniPath">文件路径</param>
        public INIClass(string iniPath)
        {
            Inipath = iniPath;
        }

        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">项目名称(如 [TypeName] )</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void IniWriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.Inipath);
        }

        /// <summary>
        /// 读出INI文件
        /// </summary>
        /// <param name="section">项目名称(如 [TypeName] )</param>
        /// <param name="key">键</param>
        public string IniReadValue(string section, string key)
        {
            if (this.ExistIniFile())
            {
                StringBuilder temp = new StringBuilder(500);
                int i = GetPrivateProfileString(section, key, "", temp, 500, this.Inipath);
                return temp.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 验证文件是否存在
        /// </summary>
        /// <returns>布尔值</returns>
        public bool ExistIniFile()
        {
            return File.Exists(Inipath);
        }
    }

    public delegate void ShowUIDelegate(object param);
    public delegate void ErrorHandler(string code, string msg);
    public delegate bool HasDrillDefined(string viewid, string matrixname);

    public class EngineHelper
    {
        protected Control _control;
        protected ClientReportContext _context;
        protected SemiRowsContainer _semirowscontainer;
        protected ReportEngine _engine;

        protected AutoResetEvent _headerevent;
        protected AutoResetEvent _outterevent;

        public event ShowUIDelegate ShowUIEvent;
        public event EventHandler EndAllEvent;
        public event EventHandler RowsComing;
        public event ErrorHandler ErrorEvent;
        public event EventHandler StaticEvent;
        public event EventHandler PreLoadEvent;
        public event EventHandler WaitingEvent;
        public event ReportComingHandler BindingMetrixSyleEvent;
        public event HasDrillDefined HasDrillDefinedEvent;

        public EngineHelper(Control control, ClientReportContext context)
        {
            _context = context;
            _control = control;
            int handle = 0;
            if (_control != null)
                handle = _control.Handle.ToInt32();

            _engine = DefaultConfigs.GetRemoteEngine(ClientReportContext.Login, _context == null ? ReportStates.Print : _context.ReportState);
            CreateSemiRowsContainer();
            _semirowscontainer.ReportComing += new ReportComingHandler(_semirowscontainer_ReportComing);
            _semirowscontainer.SemiRowsComing += new SemiRowsComingHandler(_semirowscontainer_SemiRowsComing);
            _semirowscontainer.EndAll += new EventHandler(_semirowscontainer_EndAll);
            _semirowscontainer.ErrorOccured += new ErrorHandler(_semirowscontainer_ErrorOccured);
            _semirowscontainer.Waiting += new EventHandler(_semirowscontainer_Waiting);

            _headerevent = new AutoResetEvent(true);
            _outterevent = new AutoResetEvent(false);
        }

        protected virtual void CreateSemiRowsContainer()
        {
            _semirowscontainer = new SemiRowsContainer(_engine);
        }


        public SemiRowsContainer Container
        {
            get
            {
                return _semirowscontainer;
            }
        }

        public ReportEngine Engine
        {
            get
            {
                return _engine;
            }
        }

        private void InvokeEvent(Delegate e, object[] o)
        {
            if (_control == null)
                return;
            if (_control.IsDisposed)
                return;
            if (_control.InvokeRequired)
            {
                _control.BeginInvoke(e, o);
            }
            else
            {
                _control.Invoke(e, o);
            }
        }

        private void InvokeEvent(Delegate e)
        {
            if (_control == null)
                return;
            if (_control.IsDisposed)
                return;
            if (_control.InvokeRequired)
            {
                _control.BeginInvoke(e);
            }
            else
            {
                _control.Invoke(e);
            }
        }

        private void _semirowscontainer_ErrorOccured(string code, string msg)
        {
            OnError(code, msg);
        }

        private void _semirowscontainer_Waiting(object sender, EventArgs e)
        {
            if (WaitingEvent != null)
                InvokeEvent(WaitingEvent, new object[] { _context.ReportID, null });//null
        }

        private void OnError(string code, string msg)
        {
            //System.Diagnostics.Trace.WriteLine("Catch server error....");
            if (ErrorEvent != null)
            {
                if (msg == "StaticReportNotExist")
                {
                    code = msg;
                    msg = U8ResService.GetResStringEx("U8.UAP.Report.监控数据未生成");
                }
                InvokeEvent(ErrorEvent, new object[] { code, msg });
            }
        }

        protected virtual void _semirowscontainer_EndAll(object sender, EventArgs e)
        {
            if (EndAllEvent != null)
            {
                InvokeEvent(EndAllEvent, new object[] { _context.ReportID, null });//null
            }
        }

        protected virtual void _semirowscontainer_SemiRowsComing(SemiRows semirows)
        {
            _semirowscontainer.AddSemiRows(semirows);
            if (RowsComing != null)
            {
                InvokeEvent(RowsComing, new object[] { null, null });
            }
        }

        protected virtual void _semirowscontainer_ReportComing(Report report)
        {
            if (report != null)
            {
                _context.FillFrom(report);
                ShowUI(null);
                OnStaticEvent();
            }
        }

        protected void OnBindingMatrixStyle(Report report)
        {
            if (BindingMetrixSyleEvent != null)
                BindingMetrixSyleEvent(report);
        }

        protected bool OnDrillDefined(string viewid, string matrixname)
        {
            if (HasDrillDefinedEvent != null)
                return HasDrillDefinedEvent(viewid, matrixname);
            return false;
        }

        private void OnStaticEvent()
        {
            if (StaticEvent != null)
                StaticEvent.BeginInvoke(null, null, null, null);
        }

        protected void ShowUI(object param)
        {
            //System.Diagnostics.Trace.WriteLine("Begin ShowUI......");
            if (ShowUIEvent != null)
                InvokeEvent(ShowUIEvent, new object[] { param });
        }

        public void Wait()
        {
            _outterevent.WaitOne();
        }

        public void EndWait()
        {
            _outterevent.Set();
        }

        public void ParallelTest(string id)
        {
            try
            {
                _context.ReportID = id;
                _engine.ParallelTest(id);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        public void CreateReport(object o)
        {
            CreateArgs e = o as CreateArgs;
            try
            {
                _engine.TryOpen(_context.ReportID);
                if (e.bgetsql)
                {
                    #region Get Sql
                    GetSql();
                    #endregion
                }

                if (_control.IsDisposed)
                    return;

                #region create report
                if (_context.Type == ReportType.CrossReport)
                    _engine.CreateCrossReport(e.bfilter || e.bgetsql, e.bcross, e.filterflag, e.colauthstring, e.gsid, e.rle, _context.FilterArgs
                        , e.crosstable, e.rawtable, e.basetable, e.levels, e.style, e.csid, e.showall);
                else
                    _engine.CreateReport(e.bfilter || e.bgetsql, _context.FilterArgs, e.rle
                        , e.crosstable, e.rawtable, e.basetable, e.levels, e.colauthstring, e.style, e.csid, e.showall);
                #endregion
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        public void PreLoad(object o)
        {
            ArgsBase e = o as ArgsBase;
            try
            {
                _engine.PreLoad();
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        public void QuickSortReport(object o)
        {
            SortArgs e = o as SortArgs;
            try
            {
                _engine.QuickSortReport(e.cacheid, e.sortschema, e.style);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        public void PageTo(object o)
        {
            PageArgs e = o as PageArgs;
            try
            {
                _engine.PageTo(e.cacheid, e.pageindex, e.lastindex, e.style);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }
        public void ChangePageSize(object o)
        {
            PageSizeArgs e = o as PageSizeArgs;
            try
            {
                _engine.ChangePaseSize(e.cacheid, e.pageSize, e.pageindex, e.lastindex, e.style);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }
        public void OpenStatic(object o)
        {
            StaticArgs e = o as StaticArgs;
            try
            {
                string allcolumns = _engine.GetStaticReportAllColumns(e.staticid);
                RowAuthFacade raf = new RowAuthFacade();
                string rowauth = raf.GetRowAuthFromAllColumnsWithStaticID(e.staticid, allcolumns, ClientReportContext.Login, false);
                _engine.OpenStaticReport(e.eventfilter, e.uifilter, e.pageindex, e.showall, rowauth);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        public void FilterAgain(object o)
        {
            FltArgs e = o as FltArgs;
            try
            {
                _engine.FilterAgain(e.cacheid, e.solidfilter, e.style);
            }
            catch (Exception ex)
            {
                OnError(null, ex.Message);
            }
        }

        private void GetSql()
        {
            if (_context.ReportState == ReportStates.OutU8)
                return;
            RemoteDataHelper rdh = null;
            if (_context.FilterArgs != null && !string.IsNullOrEmpty(_context.FilterArgs.ClassName.Trim()))
            {
                if (_context.FilterArgs.DataSource.Type == CustomDataSourceTypeEnum.TemplateTable)
                    //_context.FilterArgs.DataSource.SQL = CustomDataSource.GetATableName();
                    _context.FilterArgs.DataSource.SQL = CustomDataSource.GetATableNameWithTaskId(_context.TaskID);
                if (_context.ReportState == ReportStates.WebBrowse)
                {
                    rdh = DefaultConfigs.GetRemoteHelper();
                    _context.FilterArgs = rdh.GetSql(_context.UserToken, _context.TaskID, _context.SubID, _context.FilterArgs);
                }
                else
                {
                    rdh = new RemoteDataHelper();
                    rdh.GetSql(_context.FilterArgs);
                }
            }
        }
    }

    public class IndicatorEngineHelper : EngineHelper
    {
        protected IndicatorBuilder _detailbuilder;
        protected SemiRowsContainerPerhaps4Matrix _semirowscontainer4matrix;

        public IndicatorEngineHelper(Control control, ClientReportContext context)
            : base(control, context)
        {

        }

        public new SemiRowsContainerPerhaps4Matrix Container
        {
            get
            {
                return _semirowscontainer4matrix;
            }
        }

        protected override void CreateSemiRowsContainer()
        {
            _semirowscontainer4matrix = new SemiRowsContainerPerhaps4Matrix(_engine);
            _semirowscontainer = _semirowscontainer4matrix;
        }

        protected override void _semirowscontainer_EndAll(object sender, EventArgs e)
        {
            _detailbuilder.EndBuild();
            SetVisibleWidth();

            ShowUI(_detailbuilder.BuildResult);
        }

        protected virtual void SetVisibleWidth()
        {
            //if (!_semirowscontainer4matrix.b4Matrix)
            _context.Report.SetVisibleWidth(_context.Report.Sections[SectionType.Detail].Cells.Right);
        }

        protected override void _semirowscontainer_SemiRowsComing(SemiRows semirows)
        {
            _semirowscontainer.AddSemiRows(semirows);
            _detailbuilder.BuildRows();
        }

        protected override void _semirowscontainer_ReportComing(Report report)
        {
            if (_detailbuilder == null)
            {
                IndicatorDetail indicatordetail = (IndicatorDetail)report.Sections[SectionType.IndicatorDetail];
                if (_context.MatrixOrChart == MatrixOrChart.None)
                {
                    if (indicatordetail.Cells.Count == 2)
                    {
                        Cell cell1 = indicatordetail.Cells[0];
                        Cell cell2 = indicatordetail.Cells[1];
                        if ((cell1.Visible && !cell2.Visible) || (!cell1.Visible && cell2.Visible))
                        {
                            if (cell1 is IndicatorMetrix && cell2 is Chart)
                            {
                                if (cell1.Visible)
                                {
                                    _context.MatrixOrChart = MatrixOrChart.Matrix;
                                    indicatordetail.Cells.Remove(cell2);
                                }
                                else
                                    _context.MatrixOrChart = MatrixOrChart.Chart;
                            }
                            else if (cell1 is Chart && cell2 is IndicatorMetrix)
                            {
                                if (cell1.Visible)
                                    _context.MatrixOrChart = MatrixOrChart.Chart;
                                else
                                {
                                    _context.MatrixOrChart = MatrixOrChart.Matrix;
                                    indicatordetail.Cells.Remove(cell1);
                                }
                            }
                        }
                    }
                }
                else if (_context.MatrixOrChart == MatrixOrChart.Chart)
                {
                    Cell cell1 = indicatordetail.Cells[0];
                    Cell cell2 = indicatordetail.Cells[1];
                    if (cell1 is IndicatorMetrix)
                    {
                        cell1.Visible = false;
                        cell2.Visible = true;
                    }
                    else
                    {
                        cell1.Visible = true;
                        cell2.Visible = false;
                    }
                }
                else//MatrixOrChart.Matrix
                {
                    Cell cell1 = indicatordetail.Cells[0];
                    Cell cell2 = indicatordetail.Cells[1];
                    if (cell1 is IndicatorMetrix)
                    {
                        cell1.Visible = true;
                        indicatordetail.Cells.Remove(cell2);
                    }
                    else
                    {
                        cell2.Visible = true;
                        indicatordetail.Cells.Remove(cell1);
                    }
                }

                //if (indicatordetail.Cells.Count == 1 && indicatordetail.Cells[0] is IndicatorMetrix)
                //{
                //    _detailbuilder = CreateIndicatorBuilder(report.ViewID, indicatordetail.Cells[0] as IndicatorMetrix, _semirowscontainer4matrix);
                //}
                //else
                //{
                Detail detail = new Detail();
                report.Sections.Remove(indicatordetail);
                report.Sections.Add(detail);
                _detailbuilder = CreateIndicatorDetailBuilder(report.ViewID, indicatordetail, detail, _semirowscontainer);
                _context.FillFrom(report);
                //}
            }
            else
            {
                OnBindingMatrixStyle(report);

                //if (_semirowscontainer4matrix.b4Matrix )
                //{
                //    report.Type = ReportType.MetrixReport;
                //    report.FreeViewStyle = FreeViewStyle.MergeCell;
                //    _context.FillFrom(report);
                //    _context.ViewID = _detailbuilder.ViewID;
                //    report.SetVisibleWidth(report.Sections[SectionType.PageTitle].VisibleWidth);
                //    //_detailbuilder.BuildIndicatorMetrix(report, false);
                //}
                _context.Report.BaseTable = report.BaseTable;

                bool hasdrilldefined = OnDrillDefined(_context.ViewID, report.ViewID);
                _detailbuilder.BuildIndicatorMetrix(report, hasdrilldefined);

            }
        }

        protected virtual IndicatorBuilder CreateIndicatorBuilder(string viewid, IndicatorMetrix matrix, SemiRowsContainerPerhaps4Matrix semirowscontainer)
        {
            return new IndicatorBuilder(viewid, matrix, semirowscontainer);
        }

        protected virtual IndicatorBuilder CreateIndicatorDetailBuilder(string viewid, IndicatorDetail idetail, Detail detail, SemiRowsContainer semirowcontainer)
        {
            return new IndicatorDetailBuilder(viewid, idetail, detail, _semirowscontainer);
        }
    }



    public class CreateArgs
    {
        public bool bgetsql;
        public bool bfilter;
        public bool bcross;
        public string filterflag;
        public string colauthstring;
        public string gsid;
        public ReportLevelExpand rle;
        public string crosstable;
        public string rawtable;
        public string basetable;
        public int levels;
        public ShowStyle style = ShowStyle.None;
        public string csid;
        public bool showall = false;
    }

    public class ArgsBase
    {
    }

    public class StaticArgs : ArgsBase
    {
        public string staticid;
        public string eventfilter;
        public string uifilter;
        public int pageindex = 0;
        public bool showall = false;
    }

    public class SortArgs : ArgsBase
    {
        public string cacheid;
        public QuickSortSchema sortschema;
        public ShowStyle style;
    }

    public class PageArgs : ArgsBase
    {
        public string cacheid;
        public int pageindex;
        public int lastindex = -1;
        public ShowStyle style;
    }
    public class PageSizeArgs : PageArgs
    {
        public int pageSize;
    }
    public class FltArgs : ArgsBase
    {
        public string cacheid;
        public string solidfilter;
        public ShowStyle style;
    }

}
