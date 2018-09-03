using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Data;
using System.Windows.Forms;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.ReportData;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Drawing;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ClientReportContext 的摘要说明。
	/// </summary>
	public class ClientReportContext
	{		
		private int _pages;
		private int _pageindex=0;
		private PageMargins _pms;
		private ReportStates _reportstate;
		private bool _bautoheight=false;//autosize
		private bool _bpagebygroup=false;
		private int _pagesize=10;
		private int _grouplevels=0;
		private bool _bshowdetail=true;
		private SimpleViewCollection _simpleviews;
		private string _viewid="";
		private string _reportid;
		private FilterArgs _filterargs;
		private ReportType _type;
		private string _datafilter;
		private string _staticid;
        private string _usertoken;
        private Report _report;
        private string _taskid;
        private string _subid;
        private static string _localeid = "zh-CN";
        private static string _protocal;
        private static string _port;

        private string _userId;
        //private string _ufDataDbString;
        //private string _ufMetaDbString;
        private string _appServer;
        private string _accYear;
        private string _accId;

        //private object _tag;
        private bool _bcomplete = false;
        private object _rawlogin;

        private SimpleArrayList _tablenamecollection = new SimpleArrayList();
        private SimpleHashtable _levelcollection = new SimpleHashtable();

        private bool _dynamiccontentonheader = false;

		private static  U8LoginInfor _login;
        private static bool _binserverside = false;

        private ShowStyle _showstyle= ShowStyle.None ;
        private string _colorid;
        private MatrixOrChart _matrixorchart= MatrixOrChart.None ;
        private bool _bportalview = false;
        private ReportUIUserStateInfo _reportUIUserStateInfo;


        public ClientReportContext()
        {
            InitBarCodeLicense();
        }

        public static void InitBarCodeLicense()
        {
            //Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional.LicenseOwner = "UFIDA Software Co Ltd-Standard Edition-Developer License";
            //Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional.LicenseKey = "GGJNNV2RGNEPKV3QJFLUYWSEWT9W24W7NUFV34LH7E2E4QNMVZDQ";
            Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional.LicenseOwner = "UFIDA Software Co. Ltd-Ultimate Edition-OEM Developer License";
            Neodynamic.WinControls.BarcodeProfessional.BarcodeProfessional.LicenseKey = "YBKCEFV82U8UM78CBB3ZRGCT9C3JZH984LPRK29G8T5TSK9LVMYQ";
        }

        public static bool bInServerProcess
        {
            get
            {
                return _binserverside;
            }
            set
            {
                _binserverside = value;
            }
        }

        public string ColorStyleID
        {
            get
            {
                return _colorid;
            }
            set
            {
                _colorid = value;
            }
        }

        public ShowStyle ShowStyle
        {
            get
            {
                return _showstyle;
            }
            set
            {
                _showstyle = value;
            }
        }

        public ClientReportContext(object vblogin):this()
        {
            _rawlogin = vblogin;
            //if (_login == null)
            _login = new U8LoginInfor(vblogin);

            _usertoken = _login.UserToken;
            _localeid = _login.LocaleID;
            _taskid = _login.TaskID;
            _subid = _login.SubID;
            //_ufDataDbString = _login.UfDataCnnString;
            //_ufMetaDbString = _login.UfMetaCnnString;
            _appServer = _login.AppServer;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_localeid);
        }

        public Report Report
        {
            get
            {
                return _report;
            }
            set
            {
                _report = value;
            }
        }

        public bool DynamicContentInHeader
        {
            get
            {
                return _dynamiccontentonheader ;
            }
        }

        /// <summary>
        /// 根据用户保存报表的布局信息
        /// </summary>
        public ReportUIUserStateInfo ReportUIUserStateInfo
        {
            get { return _reportUIUserStateInfo; }
            set { _reportUIUserStateInfo = value; }
        }

        public void FillFrom(Report report)
        {
            if (_report != null)
                _report.BaseTable = report.BaseTable;
            _grouplevels = report.GroupLevels;    
            if (!string.IsNullOrEmpty(report.BaseTable))
            {
                _tablenamecollection.Add(report.BaseTable);
                if (!_levelcollection.Contains(report.BaseTable) || _grouplevels > (int)_levelcollection[report.BaseTable])
                    _levelcollection.Add(report.BaseTable, _grouplevels);
            }
            if (!string.IsNullOrEmpty(report.RawTable))
                _tablenamecollection.Add(report.RawTable);
            if (!string.IsNullOrEmpty(report.CrossTable))
                _tablenamecollection.Add(report.CrossTable);

            if (report.bFromCache)
            {
                if (report.bPageByGroup)
                    _pages = report.RowsCount == 0 ? 1 : report.RowsCount;
                else
                    _pages = report.PageRecords == 0 ? 1 : (report.RowsCount / report.PageRecords + (report.RowsCount % report.PageRecords == 0 ? 0 : 1));
                _report.RowsCount = report.RowsCount;
                _report.GroupsCount = report.GroupsCount;
                return;

            }

            if (_report != null)
            {
                _report.Dispose();
                _report = null;
            }
            _report = report;
            _type = _report.Type;

            if(string.IsNullOrEmpty(_viewid))
                _viewid = _report.ViewID;
            if (!string.IsNullOrEmpty(_colorid))
                _report.ColorStyleID = _colorid ;
            if (_showstyle == ShowStyle.None)
            {
                if (!_report.bNoneCross && _report.CurrentCrossSchema != null)
                {
                    if (_report.CurrentCrossSchema.CrossRowGroup != null)
                        _showstyle = _report.CurrentCrossSchema.CrossRowGroup.ShowStyle;
                    else _showstyle = _report.CurrentCrossSchema.ShowStyle;//为了升级兼容
                }
                else
                {
                    _showstyle = _report.CurrentSchema.ShowStyle;
                }
            }

            //自由报表合并单元格--2
            //if (_type == ReportType.FreeReport)
            //    _report.CurrentSchema.ShowStyle = (_report.FreeViewStyle == FreeViewStyle.MergeCell ? ShowStyle.NoGroupHeader : ShowStyle.Normal);

            _bshowdetail = report.bShowDetail;

            _pageindex = 0;
            if (_report.bPageByGroup)
                _pages = _report.RowsCount == 0 ? 1 : _report.RowsCount;
            else
                _pages = _report.PageRecords==0?1:( _report.RowsCount / _report.PageRecords + (_report.RowsCount % _report.PageRecords == 0 ? 0 : 1));

            if (_report.bIndicator )
                return;            

            using (Graphics g = Row.CreateGraphics())
            {
                _report.TransformFromInchOfHandredToPixel(g);
            }

            foreach (Section section in report.Sections)
                section.Cells.SetSuper();

            #region RedrawReportHeader
            Section header = _report.Sections[SectionType.ReportHeader];
            if (header != null)
            {
                foreach (Cell cell in header.Cells)
                {
                    if (cell is Expression && cell.Visible) 
                    {
                        if ((cell as Expression).Formula.FormulaExpression == "Page()"
                        || (cell as Expression).Formula.FormulaExpression == "Pages()")
                        {
                            _dynamiccontentonheader  = true;
                            if (_report.ReportHeaderOption != ReportHeaderPrintOption.NotPint)
                                _report.ReportHeaderOption = ReportHeaderPrintOption.EveryPage;
                        }
                    }
                }
            }
            #endregion
        }

        #region initialize
        //prewivew / static
        public void Initialize(string id,ReportStates reportstate,ReportType type)
		{			
			_reportstate=reportstate;
            if (reportstate == ReportStates.Preview)
                _viewid = id;
            else
                _staticid = id;
            _type = type;
		}

        //web
		public void Initialize(string reportid,ReportStates reportstate,string usertoken,string taskid,string subid,string url,string userid,string langid,string accid,string filterclass,string authstring,string ciyear,string curdate)
		{
            _usertoken = usertoken;
            _taskid = taskid;
            _subid = subid;
            _login = new U8LoginInfor();
            _login.UserToken = _usertoken;
            _login.TaskID = _taskid;
            _login.SubID = _subid;
            _login.LocaleID = langid;
			Initialize(reportid,reportstate,null,true,url,userid,langid,accid,filterclass ,authstring ,ciyear,curdate );
		}

        //browse
		public void Initialize(string reportid,ReportStates reportstate,object rawfilter,bool bshowui)
		{
            Initialize(reportid, reportstate, rawfilter, bshowui , "", _login.UserID, _login.LocaleID, _login.cAccId,null,null,_login.Year.ToString() ,_login.Date );
		}

		private void Initialize(string reportid,ReportStates reportstate,object rawfilter,bool bshowui,string url,string userid,string langid,string accid,string filterclass,string authstring,string ciyear,string curdate)
		{
            try
            {
                _reportid = reportid;
                _reportstate = reportstate;
                _localeid = langid;
                this._accYear = ciyear;
                this._accId = accid ;
                this._userId = userid;
                _reportstate = reportstate;

                _filterargs = new FilterArgs(_reportid,filterclass , url.Length > 0 ? null : _login.U8Login);
                _filterargs.Login = _login != null ? _login.U8Login : null;
                _filterargs.bShowUI = bshowui;
                _filterargs.RawFilter = rawfilter;
                _filterargs.UFBaseLibUrl = url;
                _filterargs.IsWebFilter = url.Length > 0;
                _filterargs.UserID = userid;
                _filterargs.LangID = langid;
                _filterargs.AccID = accid;
                _filterargs.UserToken = _usertoken;

                if (authstring != null)
                    _filterargs.Args.Add("AuthString", authstring);
                if (ciyear != null && ciyear != "")
                    _filterargs.Args.Add("CIYear", ciyear);
                if (!string.IsNullOrEmpty(curdate))
                    _filterargs.Args.Add("CurDate", curdate );

                if (_reportstate == ReportStates.Preview || bWebOrOutU8)//ReportStates.WebBrowse
                    _filterargs.Args.Add("previeworweb", "1");
            }
            catch (TargetInvocationException ex)
            {
                throw new ReportException(ex.InnerException.Message);
            }
        }

        #endregion

        public bool bNoneCross
        {
            get
            {
                return _report.bNoneCross;
            }
        }
        public SimpleHashtable LevelCollection
        {
            get
            {
                return _levelcollection;
            }
        }
        public SimpleArrayList TablenameCollection
        {
            get
            {
                return _tablenamecollection;
            }
        }

        public bool bComplete
        {
            get
            {
                return _bcomplete;
            }
            set
            {
                _bcomplete = value;
            }
        }

        //public object Tag
        //{
        //    get
        //    {
        //        return _tag;
        //    }
        //    set
        //    {
        //        _tag  = value;
        //    }
        //}

        public string TaskID
        {
            get
            {
                return _taskid;
            }
            set
            {
                _taskid = value;
            }
        }

        public string SubID
        {
            get
            {
                return _subid;
            }
            set
            {
                _subid = value;
            }
        }

        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
            }
        }


        //public string UfMetaDbString
        //{
        //    get
        //    {
        //        return _ufMetaDbString;
        //    }
        //    set
        //    {
        //        _ufMetaDbString = value;
        //    }
        //}


        //public string UfDataDbString
        //{
        //    get
        //    {
        //        return _ufDataDbString;
        //    }
        //    set
        //    {
        //        _ufDataDbString = value;
        //    }
        //}

        public static string Protocal
        {
            get
            {
                //if (string.IsNullOrEmpty(_protocal))
                //    return UFSoft.U8.Framework.Login.UI.Cryptography.LoginInfo.ProtocolPort["RePr"].ToString();
                //else
                    return _protocal;
            }
            set
            {
                _protocal=value;
            }
        }

        public static string Port
        {
            get
            {
                //if (string.IsNullOrEmpty(_port ))
                //    return UFSoft.U8.Framework.Login.UI.Cryptography.LoginInfo.ProtocolPort["RePt"].ToString();
                //else
                    return _port ;
            }
            set
            {
                _port  = value;
            }
        }

        public string AccountId
        {
            get
            {
                return _accId;
            }
            set
            {
                _accId = value;
            }
        }

        public string AccountYear
        {
            get
            {
                return _accYear;
            }
            set
            {
                _accYear = value;
            }
        }

        public string AppServer
        {
            get
            {
                return _appServer;
            }
            set
            {
                _appServer = value;
            }
        }



        public static string LocaleID
        {
            get
            {
                return _localeid;
            }
            set
            {
                _localeid = value;
            }
        }

        public string UserToken
        {
            get
            {
                return _usertoken;
            }
            set
            {
                _usertoken = value;
            }
        }

		public ReportStates ReportState
		{
			get
			{
				return _reportstate;
			}
			set
			{
				_reportstate=value;
			}
		}

		public string DataFilter
		{
			get
			{
				return _datafilter;
			}
			set
			{
				_datafilter=value;
			}
		}
		public int Pages
		{
			get
			{
				return _pages;
			}
			set
			{
				_pages=value;
			}
		}

		public int PageIndex
		{
			get
			{
				return _pageindex;
			}
			set
			{
				_pageindex=value;
			}
		}
		public static U8LoginInfor  Login
		{
			get
			{
				return _login;
			}
            set
            {
                _login = value;
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(value.LocaleID);
            }
		}

        public object RawLogin
        {
            get
            {
                return _rawlogin;
            }
        }

		public string ViewID
		{
			get
			{
				return _viewid;
			}
			set
			{
				_viewid=value;
			}
		}

		public FilterArgs FilterArgs
		{
			get
			{
				return _filterargs;
			}
			set
			{
				_filterargs=value;
			}
		}

		public string ReportID
		{
			get
			{
				return _reportid;
			}
			set
			{
				_reportid=value;
			}
		}
		public PageMargins PMS
		{
			get
			{
				return _pms;
			}
			set
			{
				_pms=value;
			}
		}

		public bool bAutoHeight
		{
			get
			{
				return _bautoheight;
			}
			set
			{
				_bautoheight=value;
			}
		}

		public bool bPageByGroup
		{
			get
			{
				return _bpagebygroup;
			}
			set
			{
				_bpagebygroup=value;
			}
		}
		public int PageSize
		{
			get
			{
				return _pagesize;
			}
			set
			{
				_pagesize=value;
			}
		}
		public int GroupLevels
		{
			get
			{
				return _grouplevels;
			}
			set
			{
				_grouplevels=value;
			}
		}

		public bool bShowDetail
		{
			get
			{
				return _bshowdetail;
			}
			set
			{
				_bshowdetail=value;
			}
		}
		
		public SimpleViewCollection SimpleViews
		{
			get
			{
				return _simpleviews;
			}
			set
			{
				_simpleviews=value;
			}
		}
		public ReportType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type=value;
			}
		}

		public string StaticID
		{
			get
			{
				return _staticid;
			}
			set
			{
				_staticid=value;
			}
		}

        public MatrixOrChart MatrixOrChart
        {
            get
            {
                return _matrixorchart;
            }
            set
            {
                _matrixorchart = value;
            }
        }

        public bool bPortalView
        {
            get
            {
                return _bportalview;
            }
            set
            {
                _bportalview = value;
            }
        }

        public bool bWebOrOutU8
        {
            get
            {
                return _reportstate == ReportStates.WebBrowse || _reportstate == ReportStates.OutU8;
            }
        }

        public string GetReportId()
        {
            if(!string.IsNullOrEmpty(this._staticid))
            {
                return _staticid.ToLower();
            }
            else return _reportid.ToLower();
        }
	}

    public enum MatrixOrChart
    {
        Matrix,
        Chart,
        None
    }
}
