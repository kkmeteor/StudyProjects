using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using UFIDA.U8.UAP.Services.ReportFilterService;
using System.Security.Cryptography;
namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// Report 的摘要说明。
    /// </summary>
    [Serializable]
    public partial class Report : DisplyTextCustomTypeDescriptor, ISerializable, ICloneable, IDisposable, IRowFilter, IDrill
    {
        #region fields
        protected GridBorderStyle _borderstyle = GridBorderStyle.VerticalAndHorizontal;
        protected Color _bordercolor = Color.Silver;
        protected DataSource _filtersource;
        protected Informations _informations;
        protected Sections _sections;
        protected PagingCriterion _pagingcriterion = PagingCriterion.ByCount;
        protected string _pagername = "A4";
        protected PageMargins _pagemargins;
        protected bool _blandscape;
        protected string _viewid;
        protected string _reportid;
        protected ReportType _type;
        protected int _pagerecords = 500;
        protected int _designwidth;
        protected string _name = "Report";
        protected System.Drawing.Image _backimage;
        protected string _filterid;
        protected GroupSchemas _groupschemas;
        protected string _groupschemaid;
        protected GlobalVarients _varients;
        protected SelfActions _selfactions;
        protected GroupSchemas _crossschemas;
        protected string _crossschemaid;

        protected ReportStates _understate = ReportStates.Designtime;

        protected HelpSetting _helpinfo;

        protected RowFilter _rowfilter;
        protected string _initevent = "";
        protected string _groupevent = "";

        protected int _visiblewidth;
        protected bool _bshowdetail = false;
        protected string _projectid = "";
        protected string _subId = "OutU8";
        protected bool _bshowsummary = true;
        protected bool _bshowsubtotal = true;

        protected bool _badjustprintwidth = true;

        protected LevelExpandSchema _expandschema;

        protected int _rowsCount;
        protected int _groupscount = -1;

        protected string _cacheid;
        protected string _viewclass = "";
        protected bool _reportmergecell = false;
        protected string _reportcolorset = "";
        protected DataSources _datasources;
        protected FilterSrv _fltsrv;
        protected AgileArgs _args;
        protected string _basetable;
        protected string _rawtable;
        protected string _crosstable;
        protected bool _bshowwhenzero = true;

        protected string _dynamicscript;

        //----------------------don't serialize to client
        //name
        protected SimpleArrayList _scriptcalcultors;//脚本汇总

        //mapname
        protected SimpleArrayList _complexcolumns;
        protected SimpleArrayList _accumulatecolumns;
        protected SimpleArrayList _balancecolumns;
        protected SimpleArrayList _rowbalancecolumns;
        protected SequenceMaps _scriptcolumns;//复杂列,移出了展开分组列及其他复杂分组列
        //-----------------------

        protected Hashtable _groupstrings;
        protected string _detailstring;
        protected string _minoraggregatestring;
        protected string _upperaggregatestring;

        protected QuickSortSchema _sortschema;
        protected Hashtable _groupstructure;
        protected ReportSummaryData _summarydata;

        protected string _crossfltargstring;
        protected string _crossauthstring;
        protected string _baseid = "baseid";

        private bool _mustshowdetail = false;
        private bool _bstoreprocandtemptable = false;

        protected Hashtable _groupbystrings;
        protected string _solidfilter;
        protected string _chartstrings = null;
        protected ChartSchemas _chartschemas;

        protected FreeViewStyle _freeviewstyle = FreeViewStyle.Normal;
        protected string _solidgroup = "";
        protected bool _solidsort = false;

        protected PrintOption _printoption;
        protected GroupOption _groupoption;
        protected string _colorstyleid;
        protected string _freecolorstyleid;
        protected bool _bsupportdynamiccolumn = false;
        protected bool _bdynamiccolumnvisible = false;
        protected bool _allowgroup = true;
        protected bool _bfromcache = false;

        protected bool _allowsubtotal = true;
        protected bool _allowtotal = true;
        protected bool _allowcross = true;

        protected string _datasourceid;
        protected string _extendeddatasourceid;

        protected int _state = 0;
        public event EventHandler WidthChanged;

        private bool _canSaveAs = true;
        protected AutoResetEvent _serializeevent;

        //返回所有的设计时字段
        protected Cells _gridDetailCells;

        private ReportType _realViewType;

        /// <summary>
        /// 真正的view类型
        /// </summary>
        [Browsable(false)]
        public ReportType RealViewType
        {
            get { return _realViewType; }
            set { _realViewType = value; }
        }

        [Browsable(false)]
        public Cells GridDetailCells
        {
            get { return _gridDetailCells; }
            set { _gridDetailCells = value; }
        }

        /// <summary>
        /// 运行时格式元数据
        /// </summary>
        protected RuntimeFormat _runtimeFormatMeta = null;
        protected Hashtable _bodyAlign = null;
        protected string _pagesetting;
        private string _informationstring;

        private string _sourcefilter = null;
        protected int _version = -1;

        #endregion

        #region constructor
        public Report()
        {
            _designwidth = _blandscape ? Size.Height : Size.Width;

            _sections = new Sections();
            _pagemargins = new PageMargins();
            _groupschemas = new GroupSchemas();
            _sortschema = new QuickSortSchema();
            _groupstructure = new Hashtable();
            _varients = new GlobalVarients();
            _selfactions = new SelfActions();
            _helpinfo = new HelpSetting();
            _rowfilter = new RowFilter();
            _expandschema = new LevelExpandSchema();
            _args = new AgileArgs();
            _printoption = new PrintOption();
            _groupoption = new GroupOption();
            _serializeevent = new AutoResetEvent(true);
            _informations = new Informations();
            _filtersource = new DataSource("EmptyColumn");
            _crossschemas = new GroupSchemas(true);
            InitOther();
        }

        public void InitOther()
        {
            _cacheid = Guid.NewGuid().ToString();

            _scriptcalcultors = new SimpleArrayList();
            _complexcolumns = new SimpleArrayList();
            _accumulatecolumns = new SimpleArrayList();
            _balancecolumns = new SimpleArrayList();
            _rowbalancecolumns = new SimpleArrayList();
            _scriptcolumns = new SequenceMaps();
            _groupstrings = new Hashtable();
            _groupbystrings = new Hashtable();

            _detailstring = "";
            _minoraggregatestring = "";
            _upperaggregatestring = "";
        }

        internal Report(Report report, bool designtime)
        {
            _sections = report.Sections;
            CopyProperty(report);
        }

        public Report(Report report)
        {
            _sections = report.Sections.Clone() as Sections;
            CopyProperty(report);
            SetVisibleWidth(report.Width);
        }

        private void CopyProperty(Report report)
        {
            _pagername = report.PaperName;
            _pagemargins = report.PageMargins;
            _blandscape = report.bLandScape;
            _printoption = report.PrintOption;
            _viewid = report.ViewID;
            _reportid = report.ReportID;
            _type = report.Type;
            _pagerecords = report.PageRecords;
            _groupschemas = report.GroupSchemas;
            _groupschemaid = report.CurrentSchemaID;
            _crossschemas = report.CrossSchemas;
            _crossschemaid = report.CurrentCrossID;
            _understate = report.UnderState;
            _bshowdetail = report.bShowDetail;
            _expandschema = report.ExpandSchema;
            _projectid = report.ProjectID;
            _subId = report.SubId;
            _name = report.Name;
            _badjustprintwidth = report.bAdjustPrintWidth;
            _reportcolorset = report.ReportColorSet;
            _cacheid = report.CacheID;
            _viewclass = report.ViewClass;
            _reportmergecell = report.ReportMergeCell;
            _groupstructure = report.GroupStructure;
            _chartstrings = report.ChartStrings;
            _mustshowdetail = report.MustShowDetail;
            _bstoreprocandtemptable = report.bStoreProcAndTempTable;
            _groupoption = report.GroupOption;
            _solidsort = report.SolidSort;
            _colorstyleid = report.ColorStyleID;
            _freecolorstyleid = report.FreeColorStyleID;
            _rowfilter = report.RowFilter;
            _varients = report.Varients;
            _selfactions = report.SelfActions;
            _helpinfo = report.HelpInfo;
            _initevent = report.InitEvent;
            _designwidth = report.DesignWidth;
            _freeviewstyle = report.FreeViewStyle;
            _bsupportdynamiccolumn = report.bSupportDynamicColumn;
            _bdynamiccolumnvisible = report.bDynamicColumnVisible;
            _datasources = report.DataSources;
            _basetable = report.BaseTable;
            _allowgroup = report.AllowGroup;
            _allowcross = report.AllowCross;
            _bshowwhenzero = report.bShowWhenZero;
            _informations = report.Informations;
            _filtersource = report.FilterSource;
            _borderstyle = report.BorderStyle;
            _bordercolor = report.BorderColor;
            _dynamicscript = report.DynamicScript;
            _sourcefilter = report.SourceFilter;

            _bshowsummary = report.bShowSummary;
            _bshowsubtotal = report.bShowSubTotal;
            _bodyAlign = report.BodyAlign;
        }

        //state=0 design; report =1 runtime to client; =2  runtime cache
        public Report(SerializationInfo info, StreamingContext context)//:this()
        {
            int version = 1;
            try
            {
                version = info.GetInt32("Version");
            }
            catch
            {
            }
            _version = version;
            _state = info.GetInt32("State");
            _serializeevent = new AutoResetEvent(true);
            if (_state == 0)
            {
                _initevent = info.GetString("InitEvent");
                if (version > 1)
                    _groupevent = info.GetString("GroupFilter");
                _rowfilter = (RowFilter)info.GetValue("RowFilter", typeof(RowFilter));
                _varients = (GlobalVarients)info.GetValue("Varients", typeof(GlobalVarients));
                _filterid = info.GetString("FilterID");
                _viewclass = info.GetString("ViewClass");
                _reportmergecell = info.GetBoolean("ReportMergeCell");
                if (version < 9)
                    _chartstrings = info.GetString("ChartStrings");
            }
            else if (_state == 1)
            {
                _rowsCount = info.GetInt32("RowsCount");
                _cacheid = info.GetString("CacheID");
                _crosstable = info.GetString("CrossTable");
                _rawtable = info.GetString("RawTable");
                _basetable = info.GetString("BaseTable");
                _baseid = info.GetString("BaseID");
                _sortschema = (QuickSortSchema)info.GetValue("SortSchema", typeof(QuickSortSchema));
                _groupstructure = (Hashtable)info.GetValue("GroupStructure", typeof(Hashtable));
                _reportmergecell = info.GetBoolean("ReportMergeCell");
                _bstoreprocandtemptable = info.GetBoolean("bStoreProcAndTempTable");
                if (version > 1)
                {
                    _detailstring = info.GetString("DetailString");
                    _bfromcache = info.GetBoolean("FromCache");
                }
            }
            else//==2
            {
                _initevent = info.GetString("InitEvent");
                _rowfilter = (RowFilter)info.GetValue("RowFilter", typeof(RowFilter));
                _varients = (GlobalVarients)info.GetValue("Varients", typeof(GlobalVarients));
                _filterid = info.GetString("FilterID");
                _viewclass = info.GetString("ViewClass");
                try
                {
                    _reportmergecell = info.GetBoolean("ReportMergeCell");
                }
                catch
                {
                    ;
                }

                _rowsCount = info.GetInt32("RowsCount");
                _cacheid = info.GetString("CacheID");
                _crosstable = info.GetString("CrossTable");
                _rawtable = info.GetString("RawTable");
                _basetable = info.GetString("BaseTable");
                _baseid = info.GetString("BaseID");

                _sortschema = (QuickSortSchema)info.GetValue("SortSchema", typeof(QuickSortSchema));
                _detailstring = info.GetString("DetailString");
                _groupstrings = (Hashtable)info.GetValue("GroupStrings", typeof(Hashtable));
                _groupbystrings = (Hashtable)info.GetValue("GroupByStrings", typeof(Hashtable));
                _groupstructure = (Hashtable)info.GetValue("GroupStructure", typeof(Hashtable));
                _datasources = (DataSources)info.GetValue("DataSources", typeof(DataSources));
                _fltsrv = (FilterSrv)info.GetValue("FilterSrv", typeof(FilterSrv));
                _args = (AgileArgs)info.GetValue("Args", typeof(AgileArgs));
                _scriptcalcultors = (SimpleArrayList)info.GetValue("ScirptCalculators", typeof(SimpleArrayList));
                _accumulatecolumns = (SimpleArrayList)info.GetValue("AccumulateColumns", typeof(SimpleArrayList));
                _balancecolumns = (SimpleArrayList)info.GetValue("BalanceColumns", typeof(SimpleArrayList));
                _rowbalancecolumns = (SimpleArrayList)info.GetValue("RowBalanceColumns", typeof(SimpleArrayList));
                _scriptcolumns = (SequenceMaps)info.GetValue("ScriptColumns", typeof(SequenceMaps));
                _minoraggregatestring = info.GetString("Minoraggregatestring");
                _upperaggregatestring = info.GetString("Upperaggregatestring");
                _summarydata = (ReportSummaryData)info.GetValue("SummaryData", typeof(ReportSummaryData));

                _crossauthstring = info.GetString("CrossAuthString");
                _crossfltargstring = info.GetString("CrossFltArgString");
                _solidfilter = info.GetString("SolidFilter");
                _bstoreprocandtemptable = info.GetBoolean("bStoreProcAndTempTable");

                _complexcolumns = new SimpleArrayList();
                if (version > 1)
                {
                    _groupevent = info.GetString("GroupFilter");
                    _complexcolumns = (SimpleArrayList)info.GetValue("ComplexColumns", typeof(SimpleArrayList));
                }
            }
            _datasources = (DataSources)info.GetValue("DataSources", typeof(DataSources));

            _printoption = new PrintOption();
            _groupoption = new GroupOption();

            _pagername = info.GetString("PaperName");
            _pagemargins = (PageMargins)info.GetValue("PageMargins", typeof(PageMargins));
            _blandscape = info.GetBoolean("bLandScape");
            _groupoption.PageByGroup = info.GetBoolean("bPageByGroup");
            _viewid = info.GetString("ViewID");
            _reportid = info.GetString("ReportID");
            _groupschemas = (GroupSchemas)info.GetValue("GroupSchemas", typeof(GroupSchemas));
            _groupschemaid = info.GetString("GroupSchemaID");
            _selfactions = (SelfActions)info.GetValue("SelfActions", typeof(SelfActions));
            _helpinfo = (HelpSetting)info.GetValue("HelpInfo", typeof(HelpSetting));
            _expandschema = (LevelExpandSchema)info.GetValue("ExpandSchema", typeof(LevelExpandSchema));
            _projectid = info.GetString("ProjectID");
            _subId = info.GetString("SubId");
            _name = info.GetString("Name");
            _badjustprintwidth = info.GetBoolean("bAdjustPrintWidth");
           
            _sections = (Sections)info.GetValue("Sections", typeof(Sections));
            _type = (ReportType)info.GetValue("Type", typeof(ReportType));
            _pagerecords = info.GetInt32("PageRecords");
            //_backimage=(System.Drawing.Image)info.GetValue("BackImage",typeof(System.Drawing.Image));
            _understate = (ReportStates)info.GetValue("UnderState", typeof(ReportStates));
            _bshowdetail = info.GetBoolean("bShowDetail");
            _mustshowdetail = info.GetBoolean("MustShowDetail");

            _freeviewstyle = (FreeViewStyle)info.GetValue("FreeViewStyle", typeof(FreeViewStyle));
            _runtimeFormatMeta = new RuntimeFormat();
            if (version > 1)
            {
                _printoption.HeaderPrintOption = (ReportHeaderPrintOption)info.GetValue("ReportHeaderOption", typeof(ReportHeaderPrintOption));
                _printoption.PrintProvider = (PrintProvider)info.GetValue("PrintProvider", typeof(PrintProvider));
                _printoption.CanSelectProvider = info.GetBoolean("CanSelectProvider");
                _printoption.FixedRowsPerPage = info.GetInt32("FixedRowsPerPage");
                _runtimeFormatMeta = (RuntimeFormat)info.GetValue("RuntimeFormatMeta", typeof(RuntimeFormat));
                _groupoption.SolidGroup = info.GetString("SolidGroup");
                _groupoption.SolidGroupStyle = (SolidGroupStyle)info.GetValue("SolidGroupStyle", typeof(SolidGroupStyle));
                _solidsort = info.GetBoolean("SolidSort");
                _colorstyleid = info.GetString("ColorStyleID");
                _freecolorstyleid = info.GetString("FreeColorStyleID");
                _bsupportdynamiccolumn = info.GetBoolean("SupportDynamicColumn");
                _canSaveAs = info.GetBoolean("CanSaveAs");
                _allowgroup = info.GetBoolean("AllowGroup");
            }
            if (version >= 9)
            {
                _datasourceid = info.GetString("DataSourceID");
                _extendeddatasourceid = info.GetString("ExtendedDataSourceID");
                _chartstrings = info.GetString("ChartStrings");
                _bshowwhenzero = info.GetBoolean("bShowWhenZero");
                _informations = (Informations)info.GetValue("Informations", typeof(Informations));
                _filtersource = (DataSource)info.GetValue("FilterSource", typeof(DataSource));
                _borderstyle = (GridBorderStyle)info.GetValue("BorderStyle", typeof(GridBorderStyle));
                _bordercolor = (Color)info.GetValue("BorderColor", typeof(Color));
            }
            if (version >= 10)//891
            {
                _dynamicscript = info.GetString("DynamicScript");
                _crossschemas = (GroupSchemas)info.GetValue("CrossSchemas", typeof(GroupSchemas));
                _crossschemaid = info.GetString("CrossSchemaID");
                _groupscount = info.GetInt32("GroupsCount");
            }
            if (version >= 12)
                _bdynamiccolumnvisible = info.GetBoolean("DynamicColumnVisible");
            if (version >= 13)
            {
                _allowsubtotal = info.GetBoolean("AllowSubTotal");
                _allowtotal = info.GetBoolean("AllowTotal");
                _bshowsubtotal = info.GetBoolean("bShowSubTotal");
                _bshowsummary = info.GetBoolean("bShowSummary");
            }
            if (version >= 14)
            {
                _allowcross = info.GetBoolean("AllowCross");
            }
            //由于新加的，所以以前的已保存的静态报表没有该字段，增加异常处理
           if (version >= 14)
            {
                _allowcross = info.GetBoolean("AllowCross");
            }
            if (version >= 15)
            {
                _gridDetailCells = (Cells)info.GetValue("GridDetailCells", typeof(Cells));
                _reportcolorset = info.GetString("ReportColorSet");
                //
                // pengzhzh
                //
                _realViewType = (ReportType)info.GetValue("RealViewType", typeof(ReportType));
            }
           
        }

        #endregion

        #region property
        [Browsable(false)]
        public bool AllowSubTotal
        {
            get
            {
                return _allowsubtotal;
            }
            set
            {
                _allowsubtotal = value;
            }
        }
        [Browsable(false)]
        public bool AllowTotal
        {
            get
            {
                return _allowtotal;
            }
            set
            {
                _allowtotal = value;
            }
        }
        [Browsable(false)]
        public bool bShowSubTotal
        {
            get
            {
                if (!this.bNoneCross && this.CurrentCrossSchema!=null)
                {
                    if (this.CurrentCrossSchema.CrossRowGroup != null &&
                        this.CurrentCrossSchema.CrossRowGroup.ShowStyle == ShowStyle.NoGroupSummary)
                        return true;
                }
                if (this.CurrentSchema.ShowStyle == ShowStyle.NoGroupSummary)
                    return true;

              
                //else if (this.CurrentSchema != null && !this.CurrentSchema.bNoneGroup)
                //{
                //    return this.CurrentSchema.bShowSubTotal;
                //}
                return _bshowsubtotal;
            }
            set
            {
                _bshowsubtotal = value;
            }
        }

        //[DisplayText("U8.UAP.Services.Report.ShowReportSummary")]
        //[LocalizeDescription("U8.UAP.Services.Report.ShowReportSummary")]
        [Browsable(false)]
        public virtual bool bShowSummary
        {
            get
            {
                return _bshowsummary;
            }
            set
            {
                _bshowsummary = value;
            }
        }

        [Browsable(false)]
        public int Version
        {
            get
            {
                return _version;
            }
        }

        [Browsable(false)]
        public string SourceFilter
        {
            get
            {
                return _sourcefilter;
            }
            set
            {
                _sourcefilter = value;
            }
        }

        [Browsable(false)]
        public bool bDynamicFormat
        {
            get
            {
                return _viewid != null && _viewid.EndsWith("_DynamicFormat");
            }
        }

        [Browsable(false)]
        public bool bNoneCross
        {
            get
            {
                return this.CurrentCrossSchema == null || this.CurrentCrossSchema.bNoneGroup;
            }
        }
        [Browsable(false)]
        public int GroupsCount
        {
            get
            {
                return _groupscount;
            }
            set
            {
                _groupscount = value;
            }
        }

        [Browsable(false)]
        public string CurrentCrossID
        {
            get
            {
                return _crossschemaid;
            }
            set
            {
                _crossschemaid = value;
            }
        }
        [Browsable(false)]
        public GroupSchema CurrentCrossSchema
        {
            get
            {
                if (string.IsNullOrEmpty(_crossschemaid) || !_crossschemas.Contains(_crossschemaid))
                {
                    GroupSchema gs = new GroupSchema();
                    gs.ID = "00000000-0000-0000-0000-000000000001";
                    return gs;
                }
                return _crossschemas[_crossschemaid];
            }
        }
        [Browsable(false)]
        public GroupSchemas CrossSchemas
        {
            get
            {
                return _crossschemas;
            }
            set
            {
                _crossschemas = value;
            }
        }

        [Browsable(false)]
        public string DynamicScript
        {
            get
            {
                return _dynamicscript;
            }
            set
            {
                _dynamicscript = value;
            }
        }

        [Browsable(false)]
        public bool bWebOrOutU8
        {
            get
            {
                return _understate == ReportStates.WebBrowse || _understate == ReportStates.OutU8;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis13")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis13")]
        public virtual Color BorderColor
        {
            get
            {
                return _bordercolor;
            }
            set
            {
                if (_bordercolor.ToArgb() != value.ToArgb())
                {
                    _bordercolor = value;
                    if (BorderStyleChanged != null)
                        BorderStyleChanged(this, null);
                }
            }
        }

        public event EventHandler BorderStyleChanged;
        [DisplayText("U8.UAP.Report.BorderStyle")]
        [LocalizeDescription("U8.UAP.Report.BorderStyle")]
        public virtual GridBorderStyle BorderStyle
        {
            get
            {
                return _borderstyle;
            }
            set
            {
                if (_borderstyle != value)
                {
                    _borderstyle = value;
                    if (BorderStyleChanged != null)
                        BorderStyleChanged(this, null);
                }
            }
        }
        [Browsable(false)]
        public virtual DataSource FilterSource
        {
            get
            {
                return _filtersource;
            }
            set
            {
                _filtersource = value;
            }
        }

        [Browsable(false)]
        public virtual Informations Informations
        {
            get
            {
                return _informations;
            }
            set
            {
                _informations = value;
            }
        }


        public string SetInformationString(string informationid, string information)
        {
            _informationstring = _informations.InstanceInformation(informationid, information);
            return _informationstring;
        }
        [Browsable(false)]
        public string InformationString
        {
            get
            {
                return _informationstring;
            }
            set
            {
                _informationstring = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis21")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis21")]
        public bool bShowWhenZero
        {
            get
            {
                return _bshowwhenzero;
            }
            set
            {
                _bshowwhenzero = value;
            }
        }

        [Browsable(false)]
        public string PageSetting
        {
            get
            {
                return _pagesetting;
            }
            set
            {
                _pagesetting = value;
            }
        }

        [Browsable(false)]
        public string DataSourceID
        {
            get
            {
                return _datasourceid;
            }
            set
            {
                _datasourceid = value;
            }
        }

        [Browsable(false)]
        public string ExtendedDataSourceID
        {
            get
            {
                return _extendeddatasourceid;
            }
            set
            {
                _extendeddatasourceid = value;
            }
        }
        [Browsable(false)]
        public bool bGrid
        {
            get
            {
                return (_type == ReportType.GridReport || _type == ReportType.CrossReport || _type == ReportType.MetrixReport);
            }
        }

        [Browsable(false)]
        public bool bFree
        {
            get
            {
                return (_type == ReportType.FreeReport || _type == ReportType.IndicatorReport || _type == ReportType.MetrixReport);
            }
        }

        [Browsable(false)]
        public bool bIndicator
        {
            get
            {
                return (_type == ReportType.IndicatorReport || _type == ReportType.MetrixReport);
            }
        }

        [Browsable(false)]
        public bool AllowCross
        {
            get
            {
                return _allowcross;
            }
            set
            {
                _allowcross = value;
            }
        }

        [Browsable(false)]
        public bool AllowGroup
        {
            get
            {
                return _allowgroup;
            }
            set
            {
                _allowgroup = value;
            }
        }

        [Browsable(false)]
        public bool bFromCache
        {
            get
            {
                return _bfromcache;
            }
            set
            {
                _bfromcache = value;
            }
        }

        /// <summary>
        /// 获取或设置运行时格式元数据
        /// </summary>
        [Browsable(false)]
        public RuntimeFormat RuntimeFormatMeta
        {
            get { return this._runtimeFormatMeta; }
            set { this._runtimeFormatMeta = value; }
        }
        //打印表体对齐方式
        [Browsable(false)]
        public Hashtable BodyAlign
        {
            get { return this._bodyAlign; }
            set { this._bodyAlign = value; }
        }
        /// <summary>
        /// 获取或设置运行时格式元数据
        /// </summary>
        [Browsable(false)]
        public bool CanSaveAs
        {
            get { return this._canSaveAs; }
            set { this._canSaveAs = value; }
        }

        [Browsable(false)]
        public bool bStoreProcAndTempTable
        {
            get
            {
                return _bstoreprocandtemptable;
            }
            set
            {
                _bstoreprocandtemptable = value;
            }
        }
        [Browsable(false)]
        public string ColorStyleID
        {
            get
            {
                return _colorstyleid;
            }
            set
            {
                _colorstyleid = value;
            }
        }

        [Browsable(false)]
        public string FreeColorStyleID
        {
            get
            {
                return _freecolorstyleid;
            }
            set
            {
                _freecolorstyleid = value;
            }
        }
        [Browsable(false)]
        public string BaseID
        {
            get
            {
                return _baseid;
            }
            set
            {
                _baseid = value;
            }
        }
        [Browsable(false)]
        public ChartSchemas ChartSchemas
        {
            get
            {
                if (_chartschemas == null)
                {
                    if (!ClientReportContext.bInServerProcess && string.IsNullOrEmpty(ChartStrings))
                    {
                        RemoteDataHelper rdh = DefaultConfigs.GetRemoteHelper();
                        _chartstrings = rdh.LoadChartStrings(ClientReportContext.Login.UfMetaCnnString, _viewid);
                    }
                    if (_chartstrings != "")
                        _chartschemas = ChartSchemas.FromStrings(_chartstrings);
                    else
                        _chartschemas = new ChartSchemas();
                }
                string id = null;
                if (bFree)
                {
                    id = "freeid";
                    if (!_chartschemas.Contains(id))
                    {
                        id = CurrentSchemaID;
                        if (string.IsNullOrEmpty(id))
                            id = "freeid";
                    }
                }
                else
                {
                    id = CurrentSchemaID;
                    if (string.IsNullOrEmpty(id))
                        id = "freeid";
                }

                _chartschemas.SetCurrentGroupChart(id, GroupLevels);
                return _chartschemas;
            }
	        set { _chartschemas = value; }
        }

        [Browsable(false)]
        public string ChartStrings
        {
            get
            {
                if (_chartschemas != null)
                    return _chartschemas.ToStrings();
                return _chartstrings;
            }
            set
            {
                _chartstrings = value;
                if (value == null)
                    _chartschemas = null;
            }
        }

        [Browsable(false)]
        public string SolidFilter
        {
            get
            {
                return _solidfilter;
            }
            set
            {
                _solidfilter = value;
            }
        }
        [Browsable(false)]
        public string CrossAuthString
        {
            get
            {
                return _crossauthstring;
            }
            set
            {
                _crossauthstring = value;
            }
        }

        [Browsable(false)]
        public string CrossFltArgString
        {
            get
            {
                return _crossfltargstring;
            }
            set
            {
                _crossfltargstring = value;
            }
        }

        [Browsable(false)]
        public string BaseTableNoneHeader
        {
            get
            {
                string tmptable = BaseTableInTemp;
                int index = tmptable.IndexOf("..");
                if (index == -1)
                    return tmptable;
                else
                    return tmptable.Substring(index + 2);
            }
        }

        [Browsable(false)]
        public string BaseTableInTemp
        {
            get
            {
                if (!_basetable.Contains(".."))
                    //return _basetable.Replace("UFTmpTable", "tempdb..UFTmpTable" + _cacheid.Replace("-", "_"));
                    return _basetable.Replace("UFTmpTable", "tempdb..UFTmpTable") + _cacheid.Replace("-", "_");

                //return _basetable.Replace("UFReport..UFTmpTable", "tempdb..UFTmpTable" + _cacheid.Replace("-", "_"));
                return _basetable;
            }
        }

        [Browsable(false)]
        public int State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                if (_state == 1)
                    _serializeevent.Reset();
            }
        }

        [Browsable(false)]
        public ReportSummaryData SummaryData
        {
            get
            {
                return _summarydata;
            }
            set
            {
                _summarydata = value;
            }
        }

        [Browsable(false)]
        public Hashtable GroupStructure
        {
            get
            {
                return _groupstructure;
            }
            set
            {
                _groupstructure = value;
            }
        }

        [Browsable(false)]
        public string CrossTable
        {
            get
            {
                return _crosstable;
            }
            set
            {
                _crosstable = value;
            }
        }

        [Browsable(false)]
        public string RawTable
        {
            get
            {
                return _rawtable;
            }
            set
            {
                _rawtable = value;
            }
        }

        [Browsable(false)]
        public string BaseTable
        {
            get
            {
                return _basetable;
            }
            set
            {
                _basetable = value;
            }
        }

        [Browsable(false)]
        public AgileArgs Args
        {
            get
            {
                return _args;
            }
            set
            {
                _args = value;
            }
        }

        [Browsable(false)]
        public FilterSrv FltSrv
        {
            get
            {
                return _fltsrv;
            }
            set
            {
                _fltsrv = value;
            }
        }

        [Browsable(false)]
        public QuickSortSchema SortSchema
        {
            get
            {
                return _sortschema;
            }
            set
            {
                _sortschema = value;
            }
        }

        [Browsable(false)]
        public string UpperAggregateString
        {
            get
            {
                return _upperaggregatestring;
            }
            set
            {
                _upperaggregatestring = value;
            }
        }

        [Browsable(false)]
        public string MinorAggregateString
        {
            get
            {
                return _minoraggregatestring;
            }
            set
            {
                _minoraggregatestring = value;
            }
        }

        [Browsable(false)]
        public string DetailString
        {
            get
            {
                return _detailstring;
            }
            set
            {
                _detailstring = value;
            }
        }

        [Browsable(false)]
        public Hashtable GroupByStrings
        {
            get
            {
                return _groupbystrings;
            }
            set
            {
                _groupbystrings = value;
            }
        }

        [Browsable(false)]
        public Hashtable GroupStrings
        {
            get
            {
                return _groupstrings;
            }
            set
            {
                _groupstrings = value;
            }
        }

        [Browsable(false)]
        public SimpleArrayList ScriptCalculators
        {
            get
            {
                return _scriptcalcultors;
            }
            set
            {
                _scriptcalcultors = value;
            }
        }

        [Browsable(false)]
        public SimpleArrayList ComplexColumns
        {
            get
            {
                return _complexcolumns;
            }
            set
            {
                _complexcolumns = value;
            }
        }

        [Browsable(false)]
        public SimpleArrayList AccumulateColumns
        {
            get
            {
                return _accumulatecolumns;
            }
            set
            {
                _accumulatecolumns = value;
            }
        }

        [Browsable(false)]
        public SimpleArrayList BalanceColumns
        {
            get
            {
                return _balancecolumns;
            }
            set
            {
                _balancecolumns = value;
            }
        }

        [Browsable(false)]
        public SimpleArrayList RowBalanceColumns
        {
            get
            {
                return _rowbalancecolumns;
            }
            set
            {
                _rowbalancecolumns = value;
            }
        }

        [Browsable(false)]
        public SequenceMaps ScriptColumns
        {
            get
            {
                return _scriptcolumns;
            }
            set
            {
                _scriptcolumns = value;
            }
        }

        [Browsable(false)]
        public string CacheID
        {
            get
            {
                return _cacheid;
            }
            set
            {
                _cacheid = value;
            }
        }

        [Browsable(false)]
        public DataSources DataSources
        {
            get
            {
                return _datasources;
            }
            set
            {
                _datasources = value;
            }
        }

        [Browsable(false)]
        public string ProjectID
        {
            get
            {
                return _projectid;
            }
            set
            {
                _projectid = value;
            }
        }


        [Browsable(false)]
        public string SubId
        {
            get
            {
                return _subId;
            }
            set
            {
                _subId = value;
            }
        }

        [Browsable(false)]
        [DisplayText("固定排序")]
        [LocalizeDescription("固定排序")]
        public bool SolidSort
        {
            get
            {
                return _solidsort;
            }
            set
            {
                _solidsort = value;
            }
        }


        [DisplayText("U8.UAP.Report.DynamicColumnVisible")]
        [LocalizeDescription("U8.UAP.Report.DynamicColumnVisible")]
        public virtual bool bDynamicColumnVisible
        {
            get
            {
                return _bdynamiccolumnvisible;
            }
            set
            {
                _bdynamiccolumnvisible = value;
            }
        }

        [DisplayText("U8.Report.SupportDynamicColumn")]
        [LocalizeDescription("U8.Report.SupportDynamicColumn")]
        public virtual bool bSupportDynamicColumn
        {
            get
            {
                return _bsupportdynamiccolumn;
            }
            set
            {
                _bsupportdynamiccolumn = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.Report.PrintOption")]
        [LocalizeDescription("U8.Report.PrintOption")]
        public virtual PrintOption PrintOption
        {
            get
            {
                return _printoption;
            }
            set
            {
                _printoption = value;
            }
        }

        [Browsable(false)]
        public PrintProvider PrintProvider
        {
            get
            {
                return _printoption.PrintProvider;
            }
            set
            {
                _printoption.PrintProvider = value;
            }
        }

        [Browsable(false)]
        public virtual int FixedRowsPerPage
        {
            get
            {
                return _printoption.FixedRowsPerPage;
            }
            set
            {
                _printoption.FixedRowsPerPage = value;
            }
        }

        [Browsable(false)]
        public bool CanSelectProvider
        {
            get
            {
                return _printoption.CanSelectProvider;
            }
            set
            {
                _printoption.CanSelectProvider = value;
            }
        }

        [Browsable(false)]
        public virtual ReportHeaderPrintOption ReportHeaderOption
        {
            get
            {
                return _printoption.HeaderPrintOption;
            }
            set
            {
                _printoption.HeaderPrintOption = value;
            }
        }

        [DisplayText("U8.UAP.Services.Report.ViewClass")]
        [LocalizeDescription("U8.UAP.Services.Report.ViewClass")]
        public string ViewClass
        {
            get
            {
                return _viewclass;
            }
            set
            {
                _viewclass = value;
            }
        }
        [Editor(typeof(ColorSetEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.UAP.Services.Report.ReportColorSet")]
        [LocalizeDescription("U8.UAP.Services.Report.ReportColorSet")]
        public string ReportColorSet
        {
            get
            {
                return _reportcolorset;
            }
            set
            {
                _reportcolorset = value;
            }
        }
        [Browsable(false)]
        [DisplayText("U8.UAP.Services.Report.ReportMergeCell")]
        [LocalizeDescription("U8.UAP.Services.Report.ReportMergeCell")]
        public bool ReportMergeCell
        {
            get
            {
                return _reportmergecell;
            }
            set
            {
                _reportmergecell = value;
            }
        }
        [DisplayText("U8.UAP.Services.Report.MustShowDetail")]
        [LocalizeDescription("U8.UAP.Services.Report.MustShowDetail")]
        public virtual bool MustShowDetail
        {
            get
            {
                return _mustshowdetail;
            }
            set
            {
                _mustshowdetail = value;
            }
        }

        [Browsable(false)]
        public virtual FreeViewStyle FreeViewStyle
        {
            get
            {
                if (this.Type == ReportType.IndicatorReport)
                    return FreeViewStyle.Normal;
                else if (this.Type == ReportType.MetrixReport)
                    return FreeViewStyle.MergeCell;
                else if (this.bFree)
                    return _freeviewstyle;
                else
                    return FreeViewStyle.Normal;
            }
            set
            {
                _freeviewstyle = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Report.强制打印一致性")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Report.只有在打印预览和显示不一致的情况下")]
        public bool bAdjustPrintWidth
        {
            get
            {
                if (_type != ReportType.FreeReport && _understate != ReportStates.Designtime && _badjustprintwidth)
                {
                    foreach (Section section in _sections)
                    {
                        if (section is IAutoSequence)
                            (section as IAutoSequence).bAutoSequence = true;
                    }
                }
                return _badjustprintwidth;
            }
            set
            {
                //_badjustprintwidth = value;
            }
        }
        [Browsable(false)]
        public ReportLevelExpand CurrentExpand
        {
            get
            {
                return _expandschema.CurrentReportLevelExpand;
            }
            set
            {
                _expandschema.CurrentReportLevelExpand = value;
            }
        }
        [Browsable(false)]
        public LevelExpandSchema ExpandSchema
        {
            get
            {
                return _expandschema;
            }
            set
            {
                _expandschema = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Report.是否显示明细")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Report.是否显示明细")]
        public bool bShowDetail
        {
            get
            {
                return _bshowdetail;
            }
            set
            {
                _bshowdetail = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.Report.GroupEvent")]
        [LocalizeDescription("U8.UAP.Services.Report.GroupEvent")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string GroupFilter
        {
            get
            {
                return _groupevent;
            }
            set
            {
                _groupevent = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Report.初始化事件")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Report.初始化事件")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string InitEvent
        {
            get
            {
                return _initevent;
            }
            set
            {
                _initevent = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Report.行过滤器")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Report.行过滤器")]
        public RowFilter RowFilter
        {
            get
            {
                return _rowfilter;
            }
            set
            {
                _rowfilter = value;
                _rowfilter.Parent = this;
            }
        }

        [Browsable(false)]
        public ReportStates UnderState
        {
            get
            {
                return _understate;
            }
            set
            {
                _understate = value;
                _sections.UnderState = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis3")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis3")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis28")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis28")]
        public GlobalVarients Varients
        {
            get
            {
                return _varients;
            }
            set
            {
                _varients = value;
            }
        }

        [DisplayText("U8.IA.chhssql.frmOptDetail.tbrOptDetail.Button10_10.ToolTipText")]
        [LocalizeDescription("U8.IA.chhssql.frmOptDetail.tbrOptDetail.Button10_10.ToolTipText")]
        public HelpSetting HelpInfo
        {
            get
            {
                return _helpinfo;
            }
            set
            {
                _helpinfo = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis29")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis29")]
        public System.Drawing.Image BackImage
        {
            get
            {
                return _backimage;
            }
            set
            {
                _backimage = value;
            }
        }

        [Browsable(false)]
        public string ViewID
        {
            get
            {
                return _viewid;
            }
            set
            {
                _viewid = value;
            }
        }

        [Browsable(false)]
        public string FilterID
        {
            get
            {
                return _filterid;
            }
            set
            {
                _filterid = value;
            }
        }

        [Browsable(false)]
        public string ReportID
        {
            get
            {
                return _reportid;
            }
            set
            {
                _reportid = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis32")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis32")]
        public int PageRecords
        {
            get
            {
                return _pagerecords;
            }
            set
            {
                _pagerecords = value;
            }
        }

        [Browsable(false)]
        public Sections Sections
        {
            get
            {
                return _sections;
            }
            set
            {
                _sections = value;
            }
        }

        [Browsable(false)]
        public int GroupLevels
        {
            get
            {
                return _sections.GroupLevels;
            }
        }

        [Browsable(false)]
        //[DisplayText("U8.UAP.Services.ReportElements.Dis34")]
        //[LocalizeDescription("U8.UAP.Services.ReportElements.Dis34")]
        //[RefreshProperties(RefreshProperties.All)]
        public string PaperName
        {
            get
            {
                return _pagername;
            }
            set
            {
                _pagername = value;
            }
        }

        [Browsable(false)]
        //[DisplayText("U8.UAP.Services.ReportElements.Dis35")]
        //[LocalizeDescription("U8.UAP.Services.ReportElements.Dis35")]
        public PageMargins PageMargins
        {
            get
            {
                return _pagemargins;
            }
            set
            {
                _pagemargins = value;
            }
        }

        [Browsable(false)]
        //[DisplayText("U8.UAP.Services.ReportElements.Dis36")]
        //[LocalizeDescription("U8.UAP.Services.ReportElements.Dis36")]
        public bool bLandScape
        {
            get
            {
                return _blandscape;
            }
            set
            {
                _blandscape = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.Report.GroupOption")]
        [LocalizeDescription("U8.Report.GroupOption")]
        public virtual GroupOption GroupOption
        {
            get
            {
                return _groupoption;
            }
            set
            {
                _groupoption = value;
            }
        }

        [Browsable(false)]
        public virtual bool bPageByGroup
        {
            get
            {
                return _groupoption.PageByGroup;
            }
            set
            {
                _groupoption.PageByGroup = value;
            }
        }

        [Browsable(false)]
        public string SolidGroup
        {
            get
            {
                return _groupoption.SolidGroup;
            }
            set
            {
                _groupoption.SolidGroup = value;
            }
        }

        [Browsable(false)]
        public SolidGroupStyle SolidGroupStyle
        {
            get
            {
                return _groupoption.SolidGroupStyle;
            }
            set
            {
                _groupoption.SolidGroupStyle = value;
            }
        }

        [Browsable(false)]
        public Size Size
        {
            get
            {
                return new Size(1000, 800);//getPaperSize(_paperkind);
            }
        }

        [Browsable(false)]
        public int Height
        {
            get
            {
                return _sections.TotalHeight;
            }
        }

        [Browsable(false)]
        public int Length
        {
            get
            {
                return Math.Abs(X) + Width;
            }
        }

        [Browsable(false)]
        public int X
        {
            get
            {
                if (bFree || _understate == ReportStates.Designtime)
                    return _sections.X;
                else
                    return DefaultConfigs.ReportLeft;
            }
        }

        public ArrayList GetAuthCols(string authstring)
        {
            if (authstring == null)
                authstring = "";
            string[] authstrings = authstring.Split(',');
            ArrayList colsauth = new ArrayList();
            for (int i = 0; i < authstrings.Length; i++)
            {
                if (authstrings[i].Trim() != "")
                    colsauth.Add(authstrings[i].Trim().ToLower());
            }
            return colsauth;
        }

        public bool NoAuthOrNotValidCusItem(string name, ArrayList colsauth, DataHelper datahelper)
        {
            return (!AuthCheck(name, colsauth)
                //|| !datahelper.bNotCusNameOrHasCusName(name)
                            );
        }

        public bool NoAuthOrNotValidCusItem(Cell cell, ArrayList colsauth, DataHelper datahelper)
        {
            return (!AuthCheck(cell.Name, colsauth)
                || (cell is IDataSource && !AuthCheck((cell as IDataSource).DataSource.Name, colsauth))
                //|| (cell is IDataSource && !datahelper.bNotCusNameOrHasCusName((cell as IDataSource).DataSource.Name))
                            );
        }

        private bool AuthCheck(string name, ArrayList colsauth)
        {
            return !colsauth.Contains(name.ToLower());
        }

        public void CheckGroup(string authstring, DataHelper datahelper, ReportStates state)
        {
            #region checkgroup
            if (!this.bFree) //(this.Type != ReportType.FreeReport)
            {
                Section section = this.Sections[SectionType.CrossRowHeader];
                if (section == null)
                    section = this.Sections[SectionType.GridDetail];

                Cells cells = section.Cells;
                if (this.AllowGroup)
                    CheckGroup(this.CurrentSchema, cells, authstring, datahelper, state);
                if (this.CurrentSchema.SchemaItems.Count == 0)
                {
                    this.bPageByGroup = false;
                    this.bShowDetail = true;
                    this.MustShowDetail = true;
                }
            }
            #endregion
        }

        //runtime : state=designtime
        //designtime : state=_report.understate
        public void CheckGroup(GroupSchema gs, Cells cells, string authstring, DataHelper datahelper, ReportStates state)
        {
            if (cells == null)
                return;

            ArrayList colsauth = this.GetAuthCols(authstring);

            if (gs != null)
            {
                #region solidgroup handler
                if (this.SolidGroup.Trim() == "" && gs.bNoneGroup)
                    gs.SchemaItems.Clear();
                else
                {
                    string[] sgs = this.SolidGroup.Split(',');
                    for (int i = 0; i < sgs.Length; i++)
                    {
                        if (sgs[i].Trim() == "")
                            continue;
                        RemoveFromCurrentSchema(gs, sgs[i].Trim(), cells);
                    }
                    GroupSchemaItem soliditem = null;
                    for (int i = 0; i < sgs.Length; i++)
                    {
                        if (sgs[i].Trim() == "")
                            continue;
                        if (soliditem == null)
                            soliditem = new GroupSchemaItem();
                        soliditem.Items.Add(sgs[i].Trim());
                    }
                    if (soliditem != null)
                    {
                        soliditem.bSolid = true;
                        if (this.SolidGroupStyle == SolidGroupStyle.FixOnLastGroup)
                            gs.SchemaItems.Add(soliditem);
                        else
                            gs.SchemaItems.Insert(0, soliditem);
                    }
                }
                #endregion
                int gcount = gs.SchemaItems.Count - 1;
                SimpleArrayList alds = new SimpleArrayList();
                while (gcount >= 0)
                {
                    GroupSchemaItem gsi = gs.SchemaItems[gcount];
                    int icount = gsi.Items.Count - 1;
                    while (icount >= 0)
                    {
                        Cell gcell = cells.GetByGroupKeyWhenCheckGroup(gsi.Items[icount]);
                        if (gcell == null)
                        {
                            if (!this.DataSources.Contains(gsi.Items[icount]) || this.DataSources[gsi.Items[icount]].Caption.Trim() == "")
                            {
                                gsi.Items.RemoveAt(icount);
                                gcell = null;
                            }
                            else
                            {
                                gcell = new GridLabel(this.DataSources[gsi.Items[icount]]);
                                gcell.X = -10000 * (icount + 1);
                                cells.Insert(0, gcell);
                            }
                        }
                        else if (gcell is IDataSource && !this.DataSources.Contains((gcell as IDataSource).DataSource.Name))
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }
                        else if (gcell is IDataSource && ((gcell as IDataSource).DataSource.Type == DataType.Text || (gcell as IDataSource).DataSource.Type == DataType.Boolean))
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }
                        else if (gcell is IAlgorithm)
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }
                        else if (gcell.CrossColumnType != CrossColumnType.None)
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }
                        else if (gcell.bHidden || gcell is SuperLabel)
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }

                        if (gcell != null && this.UnderState != state
                                && NoAuthOrNotValidCusItem(gcell, colsauth, datahelper))
                        {
                            gsi.Items.RemoveAt(icount);
                            gcell = null;
                        }

                        if (gcell != null && gcell is IMapName)
                        {
                            if (gcell.Name.ToLower() == this.BaseID.ToLower() || (gcell as IMapName).MapName.ToLower() == this.BaseID.ToLower())
                            {
                                gsi.Items.RemoveAt(icount);
                                gcell = null;
                            }
                            else if (alds.Contains((gcell as IMapName).MapName))
                            {
                                gsi.Items.RemoveAt(icount);
                                gcell = null;
                            }
                            else
                                alds.Add((gcell as IMapName).MapName);
                        }

                        if (gcell != null && gsi.Items[icount].ToLower() != gcell.Name.ToLower())
                        {
                            gsi.Items.RemoveAt(icount);
                            gsi.Items.Insert(icount, gcell.Name);
                        }
                        if (gcell != null && gcell is IDecimal && gcell is ICalculateColumn)
                            (gcell as ICalculateColumn).Expression = "Convert(nvarchar(100), " + (gcell as ICalculateColumn).Expression + ")";
                        icount--;
                    }
                    if (gsi.Items.Count == 0)
                        gs.SchemaItems.RemoveAt(gcount);
                    gcount--;
                }
                gcount = 1;
                foreach (GroupSchemaItem gsi in gs.SchemaItems)
                {
                    gsi.Level = gcount;
                    gcount++;
                }
            }
        }

        private void RemoveFromCurrentSchema(GroupSchema gs, string mapname, Cells cells)
        {
            int gcount = gs.SchemaItems.Count - 1;
            while (gcount >= 0)
            {
                GroupSchemaItem gsi = gs.SchemaItems[gcount];
                int icount = gsi.Items.Count - 1;
                while (icount >= 0)
                {
                    Cell gcell = cells.GetByGroupKey(gsi.Items[icount]);
                    if ((gcell != null && (gcell as IMapName).MapName.ToLower() == mapname.ToLower())
                        || gsi.Items[icount].ToLower() == mapname.ToLower())
                        gsi.Items.RemoveAt(icount);
                    icount--;
                }
                gcount--;
            }
        }

        [Browsable(false)]
        public bool HasGroupRelateExpression
        {
            get
            {
                Section section = Sections[SectionType.PrintPageTitle];
                if (section != null)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is Expression && (cell as Expression).bGroupRelate)
                            return true;
                    }
                }
                section = Sections[SectionType.PrintPageSummary];
                if (section != null)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is Expression && (cell as Expression).bGroupRelate)
                            return true;
                    }
                }
                return false;
            }
        }
        public void SetVisibleWidth(int width)
        {
            _visiblewidth = width;
        }

        public void SetVisibleWidth()
        {
            _visiblewidth = 0;
            Detail detail = _sections[SectionType.Detail] as Detail;
            PageTitle pagetitle = _sections[SectionType.PageTitle] as PageTitle;
            ReportSummary summary = _sections[SectionType.ReportSummary] as ReportSummary;
            if (this.bFree && !this.bShowDetail)
            {
                int grouplevels = this.GroupLevels;
                if (grouplevels == 0)
                {
                    if (pagetitle != null && pagetitle.bAutoSequence)
                        pagetitle.AutoLeft = DefaultConfigs.ReportLeft;
                    if (detail != null && detail.bAutoSequence)
                        detail.AutoLeft = DefaultConfigs.ReportLeft;
                    if (summary != null && summary.bAutoSequence)
                        detail.AutoLeft = DefaultConfigs.ReportLeft;
                }
                else
                {
                    int[] autoleft = new int[grouplevels];
                    for (int i = 1; i <= grouplevels; i++)
                    {
                        GroupHeader gh = _sections.GetGroupHeader(i);
                        autoleft[i - 1] = gh.VisibleWidth;
                    }
                    if (detail != null && detail.bAutoSequence)
                        detail.AutoLeft = autoleft[grouplevels - 1];
                    if (summary != null && summary.bAutoSequence)
                        summary.AutoLeft = autoleft[0];
                    for (int i = 1; i <= grouplevels; i++)
                    {
                        GroupSummary gs = _sections.GetGroupSummary(i);
                        if (gs != null && gs.bAutoSequence)
                            gs.AutoLeft = autoleft[i - 1];
                    }
                }
            }

            if (detail != null)
            {
                _visiblewidth = detail.VisibleWidth;
            }

            if (summary != null)
            {
                summary.SetVisibleWidth(_visiblewidth);
            }
            foreach (Section section in _sections)
            {
                if (section.SectionType == SectionType.PageHeader || section.SectionType == SectionType.PageFooter || section.SectionType == SectionType.ReportHeader)
                    continue;
                int vw = section.VisibleWidth;
                if (vw > _visiblewidth)
                    _visiblewidth = vw;
            }
            ReportHeader header = _sections[SectionType.ReportHeader] as ReportHeader;
            if (header != null && _visiblewidth == 0)
            {
                _visiblewidth = header.VisibleWidth;
            }
        }

        [Browsable(false)]
        public int Width
        {
            get
            {
                if (_understate == ReportStates.Designtime)
                {
                    if (_designwidth < Math.Max(_sections.Right, _blandscape ? Size.Height : Size.Width))
                        _designwidth = Math.Max(_sections.Right, _blandscape ? Size.Height : Size.Width);
                    return _designwidth;
                }
                //					return Math.Max(Math.Abs(_sections.X)+_sections.Width,_blandscape?Size.Height:Size.Width);
                else if (bFree)
                    return _visiblewidth;
                else
                    return _visiblewidth - this.X;// _sections.Width;
            }
        }

        [Browsable(false)]
        public ReportType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        [Browsable(false)]
        public GroupSchemas GroupSchemas
        {
            get
            {
                return _groupschemas;
            }
            set
            {
                _groupschemas = value;
            }
        }

        [Browsable(false)]
        public GroupSchema CurrentSchema
        {
            get
            {
                if (string.IsNullOrEmpty(_groupschemaid) || !_groupschemas.Contains(_groupschemaid))
                {
                    GroupSchema gs = new GroupSchema();
                    gs.ID = "00000000-0000-0000-0000-000000000001";
                    return gs;
                }
                return _groupschemas[_groupschemaid];
                //if (string.IsNullOrEmpty(_groupschemaid))
                //{
                //    GroupSchema gs = new GroupSchema();
                //    gs.ID = "00000000-0000-0000-0000-000000000001";
                //    return gs;
                //}
                //else if(_groupschemas.Contains(_groupschemaid))
                //    return _groupschemas[_groupschemaid];
                //else 
                //return _crossschemas[_groupschemaid];
            }
            //set
            //{
            //    _groupschema=value;
            //}
        }

        [Browsable(false)]
        public string CurrentSchemaID
        {
            get
            {
                return _groupschemaid;
            }
            set
            {
                _groupschemaid = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis40")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis40")]
        public int DesignWidth
        {
            get
            {
                return _designwidth;
            }
            set
            {
                int width = Math.Max(_sections.Right, _blandscape ? Size.Height : Size.Width);
                if (value > width)
                {
                    _designwidth = value;
                    if (WidthChanged != null)
                        WidthChanged(this, EventArgs.Empty);
                }
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.Dis41")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis41")]
        public virtual SelfActions SelfActions
        {
            get
            {
                return _selfactions;
            }
            set
            {
                _selfactions = value;
            }
        }

        [Browsable(false)]
        public int RowsCount
        {
            get
            {
                return _rowsCount;
            }
            set
            {
                _rowsCount = value;
            }
        }

        public Cell RuntimeGetGroupOrDetailCell(string name)
        {
            Cell cell = null;
            int grouplevels = GroupLevels;
            for (int i = 1; i <= grouplevels; i++)
            {
                Section groupheader = _sections.GetGroupHeader(i);
                cell = groupheader.Cells[name];
                if (cell != null)
                    return cell;
            }
            Section detail = _sections[SectionType.Detail];
            cell = detail.Cells[name];
            return cell;
        }

        public Hashtable DesigntimeGetPrintRefers(bool bdecimal)
        {
            Hashtable ht = new Hashtable();
            foreach (Section section in this.Sections)
            {
                if (section.SectionType == SectionType.CrossRowHeader ||
                    section.SectionType == SectionType.Detail ||
                    section.SectionType == SectionType.GridDetail ||
                    section.SectionType == SectionType.GroupHeader)
                {
                    foreach (Cell cell in section.Cells)
                    {
                        if (cell is IMapName || cell is IAlgorithm)
                        {
                            if (!bdecimal || cell is IDecimal)
                            {
                                if (!ht.Contains(cell.Name) && cell.Caption.ToString().Trim() != "")
                                    ht.Add(cell.Name, cell.Caption);
                            }
                        }
                    }
                }
            }
            return ht;
        }

        public Cells DesigntimeGetPrepairCells()
        {
            SectionType st = SectionType.GridDetail;
            if (this.Type == ReportType.CrossReport)
                st = SectionType.CrossRowHeader;
            Cells cells = new Cells();
            Section section = _sections[st];
            SimpleArrayList dsexclude = new SimpleArrayList();
            if (section != null)
            {
                section.Cells.SetRawSuper();
                foreach (Cell c in section.Cells)
                {
                    dsexclude.Add(c.Name);
                    if (c is GridLabel ||
                        c is GridColumnExpression ||
                        c is GridDateTime ||
                        c is GridCalculateColumn ||
                        c is GridDecimal
                        )
                    {
                        if (c.bHidden)
                            continue;
                        if (c.Super != null)
                        {
                            SuperLabel sl = c.Super;
                            Cell ctemp = c.Clone() as Cell;
                            ctemp.Super = sl;
                            ctemp.AppendSuperCaption();
                            if (ctemp.Caption.Trim() != "")
                                cells.AddDirectly(ctemp);
                        }
                        else if (c.Caption.Trim() != "")
                            cells.AddDirectly(c);
                        //}
                    }
                }
            }
            foreach (string key in this.DataSources.Keys)
            {
                if (dsexclude.Contains(key))
                    continue;
                DataSource ds = this.DataSources[key];
                if (ds.Type == DataType.Boolean || ds.Type == DataType.Text)
                    continue;
                Cell cell = cells.GetByGroupKey(key);
                if (cell == null && ds.Caption.Trim() != "")
                    cells.AddDirectly(new GridLabel(ds));
            }
            return cells;
        }

        public Cells GetGroupCells()
        {
            Cells cells = new Cells();
            Section section = this.Sections[SectionType.PageTitle];
            SimpleArrayList dsexclude = new SimpleArrayList();
            this.GridDetailCells.SetRawSuper();
            if (section != null)
            {
                Cells tempcells = new Cells();
                //foreach (Cell cell in section.Cells)
                foreach (Cell cell in this.GridDetailCells)
                {
                    if (cell is SuperLabel)
                        AddFromSuper(cell as SuperLabel, tempcells);
                    else
                        tempcells.AddDirectly(cell);
                }
                foreach (Cell tempc in tempcells)
                {
                    dsexclude.Add(tempc.Name);
                    if (tempc is SuperLabel)
                        continue;
                    Cell c = GetDataCellByName(tempc.Name);
                    if (c == null || c.CrossColumnType != CrossColumnType.None || c.bHidden)
                        continue;
                    //if (c.CrossColumnType == CrossColumnType.CrossTotal)
                    //{
                    //    DataSource ds = this.DataSources[c.Name];
                    //    if(ds!=null)
                    //        ds.Caption = "";
                    //    continue;
                    //}
                    //else if (c.CrossColumnType != CrossColumnType.None)
                    //    continue;
                    c = c.Clone() as Cell;
                    c.Caption = tempc.Caption;
                    if (c is DBText ||
                        c is ColumnExpression ||
                        c is DBDateTime ||
                        c is CalculateColumn ||
                        c is DBDecimal ||
                        c is GroupObject ||
                        c is CalculateGroupObject ||
                        c is GridLabel ||
                        c is GridColumnExpression ||
                        c is GridDateTime ||
                        c is GridCalculateColumn ||
                        c is GridDecimal)
                    {
                        if (tempc.Super != null)
                        {
                            SuperLabel sl = tempc.Super;
                            while (sl != null)
                            {
                                c.Caption = sl.Caption + " - " + c.Caption;
                                sl = sl.Super;
                            }
                        }
                        if (c.Caption.Trim() != "")
                            cells.AddDirectly(c);
                    }
                }
            }
            foreach (string key in this.DataSources.Keys)
            {
                if (dsexclude.Contains(key))
                    continue;
                DataSource ds = this.DataSources[key];
                if (ds.Type == DataType.Boolean || ds.Type == DataType.Text)
                    continue;
                Cell cell = cells.GetByGroupKey(key);
                //if (cell == null && (ds.Type == DataType.DateTime || ds.Type == DataType.String) && ds.Caption != "")
                //    cells.AddDirectly(new GridLabel(ds));
                if (cell == null && ds.Caption.Trim() != "")
                {
                    if (ds.Type == DataType.DateTime)
                        cells.AddDirectly(new GridDateTime(ds));
                    else
                        cells.AddDirectly(new GridLabel(ds));
                }
            }
            return cells;
        }

        private void AddFromSuper(SuperLabel sl, Cells cells)
        {
            if (sl.Labels.Count > 0)
            {
                foreach (ReportElements.Label l in sl.Labels)
                {
                    if (l is SuperLabel)
                        AddFromSuper(l as SuperLabel, cells);
                    else
                        cells.AddDirectly(l);
                }
            }
            cells.AddDirectly(sl);
        }

        private Cell GetDataCellByName(string name)
        {
            Cell cell = null;
            int grouplevels = this.GroupLevels;
            for (int i = 1; i <= grouplevels; i++)
            {
                Section groupheader = this.Sections.GetGroupHeader(i);
                cell = groupheader.Cells[name];
                if (cell != null)
                    return cell;
            }
            //Section detail = this.Sections[SectionType.Detail];
            //cell = detail.Cells[name];
            cell = this.GridDetailCells[name];
            return cell;
        }

        public Cell RuntimeGetSummaryCell(string name)
        {
            Cell cell = null;
            int grouplevels = GroupLevels;
            if (grouplevels == 0)
            {
                Section s = _sections[SectionType.ReportSummary];
                if (s != null)
                {
                    cell = s.Cells[name];
                    if (cell != null)
                        return cell;
                }
            }
            for (int i = 1; i <= grouplevels; i++)
            {
                Section groupsummary = _sections.GetGroupSummary(i);
                if (groupsummary != null)
                {
                    cell = groupsummary.Cells[name];
                    if (cell != null)
                        return cell;
                }
            }
            return cell;
        }

        public void HandleGridCellBorder(Cell cell)
        {
            if (_borderstyle == GridBorderStyle.None)
                cell.Border.NoneBorder();
            else if (_borderstyle == GridBorderStyle.VerticalAndHorizontal)
                cell.Border.AllBorder();
            else
            {
                cell.Border.Top = true;
                cell.Border.Bottom = true;
                cell.Border.Left = false;
                cell.Border.Right = false;
            }
            cell.SetBorderColor(_bordercolor);
        }
        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            // TODO:  添加 Report.Clone 实现
            return new Report(this);
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            _initevent = null;
            _groupevent = null;
            if (_rowfilter != null)
            {
                _rowfilter.Dispose();
                _rowfilter = null;
            }
            if (_varients != null)
            {
                _varients.Dispose();
                _varients = null;
            }
            _filterid = null;

            _cacheid = null;
            _viewclass = null;
            _reportmergecell = false;
            _crosstable = null;
            _rawtable = null;
            _basetable = null;

            if (_sortschema != null)
            {
                _sortschema.Clear();
                _sortschema = null;
            }
            _detailstring = null;
            if (_groupstrings != null)
            {
                _groupstrings.Clear();
                _groupstrings = null;
            }
            if (_groupbystrings != null)
            {
                _groupbystrings.Clear();
                _groupbystrings = null;
            }
            if (_groupstructure != null)
            {
                _groupstructure.Clear();
                _groupstructure = null;
            }
            if (_datasources != null)
            {
                _datasources.Dispose();
                _datasources = null;
            }
            if (_fltsrv != null)
            {
                _fltsrv.Clear();
                _fltsrv = null;
            }
            if (_args != null)
            {
                _args.Clear();
                _args = null;
            }
            if (_scriptcalcultors != null)
            {
                _scriptcalcultors.Clear();
                _scriptcalcultors = null;
            }
            if (_complexcolumns != null)
            {
                _complexcolumns.Clear();
                _complexcolumns = null;
            }
            if (_accumulatecolumns != null)
            {
                _accumulatecolumns.Clear();
                _accumulatecolumns = null;
            }
            if (_balancecolumns != null)
            {
                _balancecolumns.Clear();
                _balancecolumns = null;
            }
            if (_rowbalancecolumns != null)
            {
                _rowbalancecolumns.Clear();
                _rowbalancecolumns = null;
            }
            if (_scriptcolumns != null)
            {
                _scriptcolumns.Clear();
                _scriptcolumns = null;
            }
            _minoraggregatestring = null;
            _upperaggregatestring = null;
            if (_summarydata != null)
            {
                _summarydata.Dispose();
                _summarydata = null;
            }

            _viewid = null;
            _reportid = null;

            _groupschemaid = null;
            if (_groupschemas != null)
            {
                _groupschemas.Dispose();
                _groupschemas = null;
            }

            if (_selfactions != null)
            {
                _selfactions.Clear();
                _selfactions = null;
            }
            if (_helpinfo != null)
            {
                _helpinfo.Dispose();
                _helpinfo = null;
            }
            if (_expandschema != null)
            {
                _expandschema.Dispose();
                _expandschema = null;
            }
            _projectid = null;
            _subId = null;
            _name = null;
            _pagesetting = null;

            if (_sections != null)
            {
                _sections.Dispose();
                _sections = null;
            }
        }

        #endregion

        #region private
        private Size getPaperSize(PaperKind paperKind)//   1/100 inch
        {
            switch (paperKind)
            {
                case PaperKind.A2:
                    return GetSizeFromMM(420, 594);
                case PaperKind.A3:
                    return GetSizeFromMM(297, 420);
                case PaperKind.A3Extra:
                    return GetSizeFromMM(322, 445);
                case PaperKind.A3ExtraTransverse:
                    return GetSizeFromMM(322, 445);
                case PaperKind.A3Rotated:
                    return GetSizeFromMM(420, 297);
                case PaperKind.A3Transverse:
                    return GetSizeFromMM(297, 420);
                case PaperKind.A4:
                    return GetSizeFromMM(210, 297);
                case PaperKind.A4Extra:
                    return GetSizeFromMM(236, 322);
                case PaperKind.A4Plus:
                    return GetSizeFromMM(210, 330);
                case PaperKind.A4Rotated:
                    return GetSizeFromMM(297, 210);
                case PaperKind.A4Small:
                    return GetSizeFromMM(210, 297);
                case PaperKind.A4Transverse:
                    return GetSizeFromMM(210, 297);
                case PaperKind.A5:
                    return GetSizeFromMM(148, 210);
                case PaperKind.A5Extra:
                    return GetSizeFromMM(174, 235);
                case PaperKind.A5Rotated:
                    return GetSizeFromMM(210, 148);
                case PaperKind.A5Transverse:
                    return GetSizeFromMM(148, 210);
                case PaperKind.A6:
                    return GetSizeFromMM(105, 148);
                case PaperKind.A6Rotated:
                    return GetSizeFromMM(148, 105);
                case PaperKind.APlus:
                    return GetSizeFromMM(227, 356);
                case PaperKind.B4:
                    return GetSizeFromMM(250, 353);
                case PaperKind.B4Envelope:
                    return GetSizeFromMM(250, 353);
                case PaperKind.B4JisRotated:
                    return GetSizeFromMM(364, 257);
                case PaperKind.B5:
                    return GetSizeFromMM(176, 250);
                case PaperKind.B5Envelope:
                    return GetSizeFromMM(176, 250);
                case PaperKind.B5Extra:
                    return GetSizeFromMM(201, 276);
                case PaperKind.B5Transverse:
                    return GetSizeFromMM(257, 182);
                case PaperKind.B5JisRotated:
                    return GetSizeFromMM(182, 257);
                case PaperKind.B6Envelope:
                    return GetSizeFromMM(176, 125);
                case PaperKind.B6Jis:
                    return GetSizeFromMM(128, 182);
                case PaperKind.B6JisRotated:
                    return GetSizeFromMM(182, 128);
                case PaperKind.BPlus:
                    return GetSizeFromMM(305, 487);
                case PaperKind.C3Envelope:
                    return GetSizeFromMM(324, 458);
                case PaperKind.C4Envelope:
                    return GetSizeFromMM(229, 324);
                case PaperKind.C5Envelope:
                    return GetSizeFromMM(162, 229);
                case PaperKind.C65Envelope:
                    return GetSizeFromMM(114, 229);
                case PaperKind.C6Envelope:
                    return GetSizeFromMM(114, 162);
                case PaperKind.CSheet:
                    return GetSizeFromMM(432, 559);
                case PaperKind.DLEnvelope:
                    return GetSizeFromMM(110, 220);
                case PaperKind.DSheet:
                    return GetSizeFromMM(559, 864);
                case PaperKind.ESheet:
                    return GetSizeFromMM(864, 1118);
                case PaperKind.Executive:
                    return GetSizeFromMM(184, 267);
                case PaperKind.Folio:
                    return GetSizeFromMM(216, 330);
                case PaperKind.GermanLegalFanfold:
                    return GetSizeFromMM(216, 330);
                case PaperKind.GermanStandardFanfold:
                    return GetSizeFromMM(216, 305);
                case PaperKind.InviteEnvelope:
                    return GetSizeFromMM(220, 220);
                case PaperKind.IsoB4:
                    return GetSizeFromMM(250, 353);
                case PaperKind.ItalyEnvelope:
                    return GetSizeFromMM(110, 230);
                case PaperKind.JapaneseDoublePostcard:
                    return GetSizeFromMM(200, 148);
                case PaperKind.JapaneseDoublePostcardRotated:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeChouNumber3://
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeChouNumber3Rotated:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeChouNumber4:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeChouNumber4Rotated:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeKakuNumber2:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeKakuNumber2Rotated:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeKakuNumber3:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeKakuNumber3Rotated:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeYouNumber4:
                    return GetSizeFromMM(148, 200);
                case PaperKind.JapaneseEnvelopeYouNumber4Rotated:
                    return GetSizeFromMM(148, 200);//
                case PaperKind.JapanesePostcard:
                    return GetSizeFromMM(100, 148);
                case PaperKind.JapanesePostcardRotated:
                    return GetSizeFromMM(148, 100);
                case PaperKind.Ledger:
                    return GetSizeFromMM(432, 279);
                case PaperKind.Legal:
                    return GetSizeFromMM(216, 356);
                case PaperKind.LegalExtra:
                    return GetSizeFromMM(236, 381);
                case PaperKind.Letter:
                    return GetSizeFromMM(216, 279);
                case PaperKind.LetterExtra:
                    return GetSizeFromMM(236, 304);
                case PaperKind.LetterExtraTransverse:
                    return GetSizeFromMM(236, 305);
                case PaperKind.LetterPlus:
                    return GetSizeFromMM(216, 322);
                case PaperKind.LetterRotated:
                    return GetSizeFromMM(279, 216);
                case PaperKind.LetterSmall:
                    return GetSizeFromMM(216, 279);
                case PaperKind.LetterTransverse:
                    return GetSizeFromMM(210, 279);
                case PaperKind.MonarchEnvelope:
                    return GetSizeFromMM(98, 191);
                case PaperKind.Note:
                    return GetSizeFromMM(216, 279);
                case PaperKind.Number10Envelope:
                    return GetSizeFromMM(105, 241);
                case PaperKind.Number11Envelope:
                    return new Size(Convert.ToInt32(4.5 * 75), Convert.ToInt32(10.375 * 75));
                case PaperKind.Number12Envelope:
                    return new Size(Convert.ToInt32(4.75 * 75), Convert.ToInt32(11 * 75));
                case PaperKind.Number14Envelope:
                    return new Size(Convert.ToInt32(5 * 75), Convert.ToInt32(11.5 * 75));
                case PaperKind.Number9Envelope:
                    return new Size(Convert.ToInt32(3.875 * 75), Convert.ToInt32(8.875 * 75));
                case PaperKind.PersonalEnvelope:
                    return GetSizeFromMM(92, 165);
                case PaperKind.Prc16K:
                    return GetSizeFromMM(146, 215);
                case PaperKind.Prc16KRotated:
                    return GetSizeFromMM(146, 215);
                case PaperKind.Prc32K:
                    return GetSizeFromMM(97, 151);
                case PaperKind.Prc32KBig:
                    return GetSizeFromMM(97, 151);
                case PaperKind.Prc32KBigRotated:
                    return GetSizeFromMM(97, 151);
                case PaperKind.Prc32KRotated:
                    return GetSizeFromMM(97, 151);
                case PaperKind.PrcEnvelopeNumber1:
                    return GetSizeFromMM(102, 165);
                case PaperKind.PrcEnvelopeNumber10:
                    return GetSizeFromMM(324, 458);
                case PaperKind.PrcEnvelopeNumber10Rotated:
                    return GetSizeFromMM(458, 324);
                case PaperKind.PrcEnvelopeNumber1Rotated:
                    return GetSizeFromMM(165, 102);
                case PaperKind.PrcEnvelopeNumber2:
                    return GetSizeFromMM(102, 176);
                case PaperKind.PrcEnvelopeNumber2Rotated:
                    return GetSizeFromMM(176, 102);
                case PaperKind.PrcEnvelopeNumber3:
                    return GetSizeFromMM(125, 176);
                case PaperKind.PrcEnvelopeNumber3Rotated:
                    return GetSizeFromMM(176, 125);
                case PaperKind.PrcEnvelopeNumber4:
                    return GetSizeFromMM(110, 208);
                case PaperKind.PrcEnvelopeNumber4Rotated:
                    return GetSizeFromMM(208, 110);
                case PaperKind.PrcEnvelopeNumber5:
                    return GetSizeFromMM(110, 220);
                case PaperKind.PrcEnvelopeNumber5Rotated:
                    return GetSizeFromMM(220, 110);
                case PaperKind.PrcEnvelopeNumber6:
                    return GetSizeFromMM(120, 230);
                case PaperKind.PrcEnvelopeNumber6Rotated:
                    return GetSizeFromMM(230, 120);
                case PaperKind.PrcEnvelopeNumber7:
                    return GetSizeFromMM(160, 230);
                case PaperKind.PrcEnvelopeNumber7Rotated:
                    return GetSizeFromMM(230, 160);
                case PaperKind.PrcEnvelopeNumber8:
                    return GetSizeFromMM(120, 309);
                case PaperKind.PrcEnvelopeNumber8Rotated:
                    return GetSizeFromMM(309, 120);
                case PaperKind.PrcEnvelopeNumber9:
                    return GetSizeFromMM(229, 324);
                case PaperKind.PrcEnvelopeNumber9Rotated:
                    return GetSizeFromMM(229, 324);
                case PaperKind.Quarto:
                    return GetSizeFromMM(215, 275);
                case PaperKind.Standard10x11:
                    return GetSizeFromMM(254, 279);
                case PaperKind.Standard10x14:
                    return GetSizeFromMM(254, 356);
                case PaperKind.Standard11x17:
                    return GetSizeFromMM(279, 432);
                case PaperKind.Standard12x11:
                    return GetSizeFromMM(305, 279);
                case PaperKind.Standard15x11:
                    return GetSizeFromMM(381, 279);
                case PaperKind.Standard9x11:
                    return GetSizeFromMM(229, 279);
                case PaperKind.Statement:
                    return GetSizeFromMM(140, 216);
                case PaperKind.Tabloid:
                    return GetSizeFromMM(279, 432);
                case PaperKind.TabloidExtra:
                    return GetSizeFromMM(297, 457);
                case PaperKind.USStandardFanfold:
                    return GetSizeFromMM(378, 279);
                default: //PaperKind.Custom://a4
                    return GetSizeFromMM(210, 297);
            }
        }

        private Size GetSizeFromMM(int w, int h)
        {
            return new Size(Convert.ToInt32(w * 3.937), Convert.ToInt32(h * 3.937));
        }
        #endregion

        #region ISerializable 成员

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 15);//1--871;2--872;9--890,10--810 15-11.0
            info.AddValue("State", _state);
            if (_state == 0)
            {
                info.AddValue("InitEvent", _initevent);
                info.AddValue("GroupFilter", _groupevent);
                info.AddValue("RowFilter", _rowfilter);
                info.AddValue("Varients", _varients);
                info.AddValue("FilterID", _filterid);
                info.AddValue("ViewClass", _viewclass);
                //info.AddValue("ChartStrings", _chartstrings);
            }
            else if (_state == 1)
            {
                info.AddValue("RowsCount", this._rowsCount);
                info.AddValue("CacheID", this._cacheid);
                info.AddValue("CrossTable", _crosstable);
                info.AddValue("RawTable", _rawtable);
                info.AddValue("BaseTable", _basetable);
                info.AddValue("BaseID", _baseid);
                info.AddValue("SortSchema", _sortschema);
                info.AddValue("GroupStructure", _groupstructure);
                info.AddValue("DetailString", _detailstring);
                info.AddValue("FromCache", _bfromcache);
                _serializeevent.Set();
            }
            else
            {
                info.AddValue("InitEvent", _initevent);
                info.AddValue("GroupFilter", _groupevent);
                info.AddValue("RowFilter", _rowfilter);
                info.AddValue("Varients", _varients);
                info.AddValue("FilterID", _filterid);
                info.AddValue("CacheID", this._cacheid);

                info.AddValue("RowsCount", this._rowsCount);
                info.AddValue("ViewClass", _viewclass);
                info.AddValue("CrossTable", _crosstable);
                info.AddValue("RawTable", _rawtable);
                info.AddValue("BaseTable", _basetable);
                info.AddValue("BaseID", _baseid);

                info.AddValue("SortSchema", _sortschema);
                info.AddValue("DetailString", _detailstring);
                info.AddValue("GroupStrings", _groupstrings);
                info.AddValue("GroupByStrings", _groupbystrings);
                info.AddValue("GroupStructure", _groupstructure);
                //info.AddValue("DataSources", _datasources);
                info.AddValue("FilterSrv", _fltsrv);
                info.AddValue("Args", _args);
                info.AddValue("ScirptCalculators", _scriptcalcultors);
                info.AddValue("ComplexColumns", _complexcolumns);
                info.AddValue("AccumulateColumns", _accumulatecolumns);
                info.AddValue("BalanceColumns", _balancecolumns);
                info.AddValue("RowBalanceColumns", _rowbalancecolumns);
                info.AddValue("ScriptColumns", _scriptcolumns);
                info.AddValue("Minoraggregatestring", _minoraggregatestring);
                info.AddValue("Upperaggregatestring", _upperaggregatestring);
                info.AddValue("SummaryData", _summarydata);

                info.AddValue("CrossAuthString", _crossauthstring);
                info.AddValue("CrossFltArgString", _crossfltargstring);
                info.AddValue("SolidFilter", _solidfilter);
            }
            info.AddValue("PaperName", _pagername);
            info.AddValue("PageMargins", _pagemargins);
            info.AddValue("bLandScape", _blandscape);
            info.AddValue("bPageByGroup", _groupoption.PageByGroup);
            info.AddValue("SolidGroup", _groupoption.SolidGroup);
            info.AddValue("SolidGroupStyle", _groupoption.SolidGroupStyle);
            info.AddValue("ViewID", _viewid);
            info.AddValue("ReportID", _reportid);
            info.AddValue("SelfActions", _selfactions);
            info.AddValue("HelpInfo", _helpinfo);
            info.AddValue("ProjectID", _projectid);
            info.AddValue("SubId", _subId);
            info.AddValue("Name", _name);
            info.AddValue("bAdjustPrintWidth", _badjustprintwidth);
            info.AddValue("GroupSchemas", _groupschemas);
            info.AddValue("GroupSchemaID", _groupschemaid);
            info.AddValue("ExpandSchema", _expandschema);

            info.AddValue("Sections", _sections);
            info.AddValue("Type", _type);
            info.AddValue("PageRecords", _pagerecords);
            info.AddValue("UnderState", _understate);
            info.AddValue("bShowDetail", _bshowdetail);
            //info.AddValue("BackImage",_backimage);	
            info.AddValue("MustShowDetail", _mustshowdetail);
            info.AddValue("FreeViewStyle", _freeviewstyle);
            info.AddValue("bStoreProcAndTempTable", _bstoreprocandtemptable);
            info.AddValue("ReportHeaderOption", _printoption.HeaderPrintOption);
            info.AddValue("PrintProvider", _printoption.PrintProvider);
            info.AddValue("CanSelectProvider", _printoption.CanSelectProvider);
            info.AddValue("FixedRowsPerPage", _printoption.FixedRowsPerPage);
            info.AddValue("RuntimeFormatMeta", this._runtimeFormatMeta);
            info.AddValue("DataSources", _datasources);
            info.AddValue("SolidSort", _solidsort);
            info.AddValue("ColorStyleID", _colorstyleid);
            info.AddValue("FreeColorStyleID", _freecolorstyleid);
            info.AddValue("SupportDynamicColumn", _bsupportdynamiccolumn);
            info.AddValue("DynamicColumnVisible", _bdynamiccolumnvisible);
            info.AddValue("CanSaveAs", _canSaveAs);
            info.AddValue("AllowGroup", _allowgroup);
            info.AddValue("DataSourceID", _datasourceid);
            info.AddValue("ExtendedDataSourceID", _extendeddatasourceid);
            info.AddValue("ChartStrings", this.ChartStrings);
            info.AddValue("bShowWhenZero", _bshowwhenzero);
            info.AddValue("Informations", _informations);
            info.AddValue("FilterSource", _filtersource);
            info.AddValue("BorderStyle", _borderstyle);
            info.AddValue("BorderColor", _bordercolor);
            info.AddValue("DynamicScript", _dynamicscript);
            info.AddValue("CrossSchemas", _crossschemas);
            info.AddValue("CrossSchemaID", _crossschemaid);
            info.AddValue("GroupsCount", _groupscount);
            info.AddValue("AllowSubTotal", _allowsubtotal);
            info.AddValue("AllowTotal", _allowtotal);
            info.AddValue("bShowSubTotal", _bshowsubtotal);
            info.AddValue("bShowSummary", _bshowsummary);
            info.AddValue("AllowCross", _allowcross);
            info.AddValue("ReportMergeCell", _reportmergecell);
            info.AddValue("GridDetailCells", _gridDetailCells);
            info.AddValue("ReportColorSet", _reportcolorset);
            info.AddValue("RealViewType", _realViewType);
        }

        #endregion

        #region bytes
        public byte[] ToBytesForCache()
        {
            //if (this.State == 1)
            //    _serializeevent.WaitOne(180000,false );
            //    _serializeevent.WaitOne(500, false);
            return ToBytes();
        }

        public byte[] ToBytes()
        {
            this.State = 2;
            MemoryStream fs = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return EnclopeByte.Encrypto(fs.ToArray());
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

        public static Report FromBytes(Byte[] bs)
        {
            MemoryStream ms = new MemoryStream(EnclopeByte.Decrypto(bs));
            try
            {
                Report report;
                BinaryFormatter formatter = new BinaryFormatter();
                report = (Report)formatter.Deserialize(ms);
                return report;
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }
        #endregion

        #region IDrill Members
        [DisplayText("U8.UAP.Report.DrillToReport")]
        [LocalizeDescription("U8.UAP.Report.DrillToReport")]
        [TypeConverter(typeof(DrillDownTypeConverter))]
        [Editor(typeof(DrillDownEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual string DrillToReport
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }
        [DisplayText("U8.UAP.Report.DrillToUAPVouch")]
        [LocalizeDescription("U8.UAP.Report.DrillToUAPVouch")]
        [TypeConverter(typeof(DrillDownTypeConverter))]
        [Editor(typeof(DrillDownEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual string DrillToVouch
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public event EventHandler ReportDrillDesigning;

        public event EventHandler VouchDrillDesigning;

        public void DesignReportDrill()
        {
            if (ReportDrillDesigning != null)
                ReportDrillDesigning(this, null);
        }

        public void DesignVouchDrill()
        {
            if (VouchDrillDesigning != null)
                VouchDrillDesigning(this, null);
        }

        #endregion
    }

    [Serializable]
    public class FreeReport : Report
    {
        public FreeReport(Report report)
            : base(report, true)
        {
        }

        public FreeReport(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [Browsable(false)]
        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override GridBorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        [Browsable(false)]
        public override bool MustShowDetail
        {
            get
            {
                return base.MustShowDetail;
            }
            set
            {
                base.MustShowDetail = value;
            }
        }

        [Browsable(false)]
        public override bool bSupportDynamicColumn
        {
            get
            {
                return base.bSupportDynamicColumn;
            }
            set
            {
                base.bSupportDynamicColumn = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.StaticShowStyle")]
        [LocalizeDescription("U8.UAP.Services.Report.StaticShowStyle")]
        public override FreeViewStyle FreeViewStyle
        {
            get
            {
                return base.FreeViewStyle;
            }
            set
            {
                base.FreeViewStyle = value;
            }
        }

        [Browsable(false)]
        public override GroupOption GroupOption
        {
            get
            {
                return base.GroupOption;
            }
            set
            {
                base.GroupOption = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis37")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis37")]
        public override bool bPageByGroup
        {
            get
            {
                return base.bPageByGroup;
            }
            set
            {
                base.bPageByGroup = value;
            }
        }

        [Browsable(false)]
        public override PrintOption PrintOption
        {
            get
            {
                return base.PrintOption;
            }
            set
            {
                base.PrintOption = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.Report.HeaderPrintOption")]
        [LocalizeDescription("U8.Report.HeaderPrintOption")]
        public override ReportHeaderPrintOption ReportHeaderOption
        {
            get
            {
                return base.ReportHeaderOption;
            }
            set
            {
                base.ReportHeaderOption = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.Report.FixedRowsPerPage")]
        [LocalizeDescription("U8.Report.FixedRowsPerPage")]
        public override int FixedRowsPerPage
        {
            get
            {
                return base.FixedRowsPerPage;
            }
            set
            {
                base.FixedRowsPerPage = value;
            }
        }
    }

    [Serializable]
    public class IndicatorReport : FreeReport
    {
        public IndicatorReport(Report report)
            : base(report)
        {
        }

        public IndicatorReport(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [Browsable(false)]
        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }
        [Browsable(false)]
        public override GridBorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Report.FilterSource")]
        [LocalizeDescription("U8.UAP.Report.FilterSource")]
        public override DataSource FilterSource
        {
            get
            {
                return _filtersource;
            }
            set
            {
                _filtersource = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Report.InformationSubscription")]
        [LocalizeDescription("U8.UAP.Report.InformationSubscription")]
        public override Informations Informations
        {
            get
            {
                return base.Informations;
            }
            set
            {
                base.Informations = value;
            }
        }

        [Browsable(false)]
        public override SelfActions SelfActions
        {
            get
            {
                return base.SelfActions;
            }
            set
            {
                base.SelfActions = value;
            }
        }
        [Browsable(false)]
        public override string DrillToReport
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }
        [Browsable(false)]
        public override string DrillToVouch
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }
    }
}
public class EnclopeByte
{
    private static SymmetricAlgorithm mobjCryptoService = new RijndaelManaged();
    private static string Key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87jH7";
    /// <summary>
    /// 获得密钥
    /// </summary>
    /// <returns>密钥</returns>
    private static byte[] GetLegalKey()
    {
        string sTemp = Key;
        mobjCryptoService.GenerateKey();
        byte[] bytTemp = mobjCryptoService.Key;
        int KeyLength = bytTemp.Length;
        if (sTemp.Length > KeyLength)
            sTemp = sTemp.Substring(0, KeyLength);
        else if (sTemp.Length < KeyLength)
            sTemp = sTemp.PadRight(KeyLength, ' ');
        return ASCIIEncoding.ASCII.GetBytes(sTemp);
    }
    /// <summary>
    /// 获得初始向量IV
    /// </summary>
    /// <returns>初试向量IV</returns>
    private static byte[] GetLegalIV()
    {
        string sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb#er57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
        mobjCryptoService.GenerateIV();
        byte[] bytTemp = mobjCryptoService.IV;
        int IVLength = bytTemp.Length;
        if (sTemp.Length > IVLength)
            sTemp = sTemp.Substring(0, IVLength);
        else if (sTemp.Length < IVLength)
            sTemp = sTemp.PadRight(IVLength, ' ');
        return ASCIIEncoding.ASCII.GetBytes(sTemp);
    }
    /// <summary>
    /// 加密方法
    /// </summary>
    /// <param name="Source">待加密的串</param>
    /// <returns>经过加密的串</returns>
    public static byte[] Encrypto(byte[] bytIn)
    {
        MemoryStream ms = new MemoryStream();
        mobjCryptoService.Key = GetLegalKey();
        mobjCryptoService.IV = GetLegalIV();
        ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();
        CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
        cs.Write(bytIn, 0, bytIn.Length);
        cs.FlushFinalBlock();
        byte[] b = ms.ToArray();
        ms.Close();
        return b;
    }
    /// <summary>
    /// 解密方法
    /// </summary>
    /// <param name="Source">待解密的串</param>
    /// <returns>经过解密的串</returns>
    public static byte[] Decrypto(byte[] bytIn)
    {
        try
        {
            MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
            mobjCryptoService.Key = GetLegalKey();
            mobjCryptoService.IV = GetLegalIV();
            ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            byte[] b = new byte[ms.Length];
            cs.Read(b, 0, (int)ms.Length);
            ms.Close();
            return b;
        }
        catch (Exception e)
        {
            return bytIn;
        }

        //return sr.ReadToEnd();
    }

}