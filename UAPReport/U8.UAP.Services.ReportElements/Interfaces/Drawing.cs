using System;
using System.Drawing;
using System.Data;
using System.Xml;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Drawing 的摘要说明。
	/// </summary>
	public interface Drawing
	{
		Point Location{get;set;}
		Size Size{get;set;}
		string Name{get;set;}
		string Type{get;}
        string GetStateType();
		bool Visible{get;set;}
        bool bHidden { get;set;}
        bool bSolid { get;set;}
        bool bSupportLocate { get;set;}
		Section Parent{get;set;}
		string Caption{get;set;}
        string IdentityCaption { get;set;}
		int Z_Order{get;set;}
        string PrepaintEvent { get; set; }
        
		ContentAlignment CaptionAlign{get;set;}

		Color BackColor{get;set;}
		Color ForeColor{get;set;}
        Font ClientFont { get;set;}
        ServerFont ServerFont { get;set;}

		BorderSide Border{get;set;}
		int BorderWidth{get;set;}
		Color BorderColor{get;set;}

		event EventHandler BeforeBackColorChanged;
		event EventHandler BeforeForeColorChanged;
		event EventHandler BeforeFontChanged;
		event EventHandler BeforeBorderColorChanged;
		event EventHandler BeforeBorderWidthChanged;
		event EventHandler BeforeBorderChanged;
		event EventHandler BeforeNameChanged;
		event EventHandler BeforeLocationChanged;
		event EventHandler BeforeSizeChanged;
		event EventHandler OtherPropertyChanged;
		event EventHandler BeforeCaptionChanged;
		event EventHandler Z_OrderChanged;
		event EventHandler BeforeCaptionAlignChanged;	

		event EventHandler ParentChanged;

		event EventHandler AfterBackColorChanged;
		event EventHandler AfterForeColorChanged;
		event EventHandler AfterFontChanged;
		event EventHandler AfterBorderColorChanged;
		event EventHandler AfterBorderWidthChanged;
		event EventHandler AfterBorderChanged;
		event EventHandler AfterNameChanged;
		event EventHandler AfterLocationChanged;
		event EventHandler AfterSizeChanged;
		event EventHandler AfterCaptionChanged;
		event EventHandler AfterCaptionAlignChanged;	

		void SetName(string name);
		void SetVisible(bool visible);
		void SetCaption(string caption);
		void SetCaptionAlign(ContentAlignment ca);
		void SetZOrder(int zorder);
		void SetBackColor(Color backcolor);
		void SetForeColor(Color forecolor);
        void SetClientFont(Font font);
		void SetBorderWidth(int borderwidth);
		void SetBorderColor(Color bordercolor);
		void SetLocation(Point pt);
		void SetSize(Size size);
		void SetType();
        void SetControlAuth(bool b);
		int X{get;set;}
		int Y{get;set;}
		int Width{get;set;}
		int Height{get;set;}
		Rectangle getRects();
		BorderSide getBorder();
		int RelativeY{get;set;}
		bool KeepPos{get;set;}
		string OldCaption{get;set;}
        string OldName { get;set;}
        string Value { get;set;}
        string UnFormatValue { get;set;}
        int VisiblePosition { get;set;}

        //void draw(Graphics g);
//		void draw(Graphics g,int xcoordinate,int ycoordinate);
		void draw(Graphics g,int xcoordinate,int ycoordinate,int height);
		bool contains(Point pt);	
		bool bContainedBy(Drawing d);
		void SetRuntimeHeight(Graphics g,string s);
        void SetRuntimeHeight(int height);
		void MoveX(int dx,int sx);
		void MoveY(int dy,int sy);
		void ResizeX(int dx);
		void ResizeY(int dy);
		void ResizeWidth(int dx);
		void ResizeHeight(int dy);
		void AdjustSelf();
		bool bUnder(Cell cell);
		int MetaHeight{get;}
		int ExpandHeight{get;}
		int RuntimeHeight{get;}
		bool bOmit{get;}
		bool bActive{get;set;}
        bool bInActiveRow { get;set;}
		object Clone();
		SuperLabel Super{get;set;}
        bool bExpand { get;set;}

		bool bControlAuth{get;set;}
		int CrossIndex{get;set;}
		Report Report{get;set;}
		event EventHandler GetReportEvent;
		void GetReport();
        ReportStates UnderState { get;set;}

        int RealY { get;}
        int RealX { get;}
        void DefaultHeight();
        int VisibleWidth { get;}

        DrillData  DrillTag { get;set;}
        object Tag { get;set;}
        string TWCaption { get;set;}
        string ENCaption { get;set;}
        string CNCaption { get;set;}
        void AppendSuperCaption();
        void SetY(int y);
        void SetAlternativeStyle(IAlternativeStyle ias);

        CrossColumnType CrossColumnType { get;set;}
        bool bCrossDynamicColumn { get;}
        string ScriptID { get;set;}
        string ScriptIDOnly { get; }
        bool bScriptIDEmpty { get; }
        bool bShowOnX { get; set; }
	}

    public enum GridBorderStyle
    {
        VerticalAndHorizontal,
        Horizontal,
        None
    }

    public enum CrossColumnType
    {
        None,
        CrossDetail,
        CrossSubTotal,
        CrossTotal
    }

	public enum Resize_Border
	{
		RB_NONE = 0,
		RB_TOP = 1,
		RB_RIGHT = 2,
		RB_BOTTOM = 3,
		RB_LEFT = 4,
		RB_LTRB = 5,
		RB_TRLB = 6,
		RB_RBLT =7,
		RB_LBTR=8,
		RB_SECTION=9
	}

	public enum PagingCriterion
	{
		None,
		BySize,
		ByCount
	}

    public enum LabelType
    {
        SummaryLabel,
        DetailLabel,
        GroupLabel,
        OtherLabel
    }

    public enum States
    {
        Arrow,
        AlgorithmCalculator,
        CalculateColumn,
        Calculator,
        Chart,
        ColumnExpression,
        DBBoolean,
        DBDateTime,
        DBDecimal,
        DBExchangeRate,
        DBImage,
        DBText,
        DecimalAlgorithmCalculator,
        CommonExpression,
        FilterExpression,
        PrintExpression,
        GroupObject,
        CalculateGroupObject,
        AlgorithmGroupObject,
        Image,
        Label,
        CommonLabel,
        Line,
        SubReport,
        SuperLabel,
        GridCalculateColumn,
        GridColumnExpression,
        GridBoolean,
        GridDateTime,
        GridDecimal,
        GridExchangeRate,
        GridImage,
        GridDecimalAlgorithmColumn,
        GridAlgorithmColumn,
        GridLabel,
        GridDetail,
        PageHeader,
        PrintPageTitle,
        PageTitle,
        ReportHeader,
        GroupHeader,
        Detail,
        GroupSummary,
        PrintPageSummary,
        ReportSummary,
        PageFooter,
        CrossRowHeader,
        CrossColumnHeader,
        CrossDetail,
        ColumnHeader,
        CalculateColumnHeader,
        AlgorithmColumnHeader,
        DecimalAlgorithmColumn,
        AlgorithmColumn,
        BarCode,
        IndicatorMetrix,
        Gauge,
        Indicator,
        CalculateIndicator,
        GroupDimension,
        CalculateGroupDimension,
        CrossDimension,
        CalculateCrossDimension,
        CalculatorIndicator,
        None,
        GridProportionDecimal,
        GridProportionDecimalIndicator 
    }

    public enum GridViewStyle
    {
        Plat,
        Collapse,
        MergeCell
    }

    public enum FreeViewStyle
    {
        MergeCell,
        Normal
    }

	//public delegate void YHandler(object sender,YEventArgs e);
	public class YEventArgs:EventArgs
	{
		private int _y;
		public YEventArgs(int y)
		{
			_y=y;
		}

		public int Y
		{
			get
			{
				return _y;
			}
		}
	}

    [Serializable]
    public class ServerFont
    {
        private string _fontname = "宋体";
        private float _fontsize = 9;
        private System.Drawing.GraphicsUnit _fontunit = GraphicsUnit.Point;
        private byte _gdicharset = 0;
        private bool _gdiverticalfont = false;
        private bool _bold = false;
        private bool _italic = false;
        private bool _strikethout = false;
        private bool _underline = false;

        public ServerFont()
        {
        }

        public void From(ServerFont font)
        {
            if (font != null)
            {
                _fontname = font.FontName ;
                _fontsize = font.FontSize ;
                _fontunit = font.FontUnit ;
                _gdicharset = font.GdiCharSet;
                _gdiverticalfont = font.GdiVerticalFont;
                _bold = font.Bold;
                _italic = font.Italic;
                _strikethout = font.StrikethOut ;
                _underline = font.UnderLine ;
            }
        }

        public void FromFont(Font font)
        {
            if (font != null)
            {
                _fontname = font.Name;
                _fontsize = font.Size;
                _fontunit = font.Unit;
                _gdicharset = font.GdiCharSet;
                _gdiverticalfont = font.GdiVerticalFont;
                _bold = font.Bold;
                _italic = font.Italic;
                _strikethout = font.Strikeout;
                _underline = font.Underline;
            }
        }

        public string FontName
        {
            get
            {
                return _fontname;
            }
            set
            {
                _fontname = value;
            }
        }
        public float FontSize
        {
            get
            {
                return _fontsize;
            }
            set
            {
                _fontsize = value;
            }
        }
        public GraphicsUnit FontUnit
        {
            get
            {
                return _fontunit;
            }
            set
            {
                _fontunit = value;
            }
        }
        public byte GdiCharSet
        {
            get
            {
                return _gdicharset;
            }
            set
            {
                _gdicharset = value;
            }
        }
        public bool GdiVerticalFont
        {
            get
            {
                return _gdiverticalfont;
            }
            set
            {
                _gdiverticalfont = value;
            }
        }
        public bool Bold
        {
            get
            {
                return _bold;
            }
            set
            {
                _bold = value;
            }
        }
        public bool Italic
        {
            get
            {
                return _italic;
            }
            set
            {
                _italic = value;
            }
        }
        public bool StrikethOut
        {
            get
            {
                return _strikethout;
            }
            set
            {
                _strikethout = value;
            }
        }
        public bool UnderLine
        {
            get
            {
                return _underline;
            }
            set
            {
                _underline = value;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                System.Drawing.FontStyle fontstyle = FontStyle.Regular;
                if (_bold)
                    fontstyle = fontstyle | FontStyle.Bold;
                if (_italic)
                    fontstyle = fontstyle | FontStyle.Italic;
                if (_strikethout)
                    fontstyle = fontstyle | FontStyle.Strikeout;
                if (_underline)
                    fontstyle = fontstyle | FontStyle.Underline;
                return fontstyle;
            }
        }
    }
}
