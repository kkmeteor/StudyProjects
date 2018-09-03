using System;
using System.Drawing; 
using System.Data;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Windows.Forms;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// Rect 的摘要说明。
    /// </summary>
    [Serializable]
    public abstract class Rect : DisplyTextCustomTypeDescriptor, Cell, ISerializable, ICloneable, IDisposable
    {
        #region Fields
        protected int _x, _y, _w, _h;
        protected const int VSIZE = 24;
        protected const int HSIZE = 96;
        protected string _type = "";
        protected string _caption = "";
        protected string _name = "";
        protected ContentAlignment _captionalign = ContentAlignment.MiddleLeft;
        protected bool _visible = true;
        protected bool _bhidden = false;
        protected bool _bsolid = false;
        protected int _zorder = 0;
        protected Section _parent;
        protected string _prepaintevent = "";
        //protected Font _font = new Font(new FontFamily("宋体"), 9, FontStyle.Regular, GraphicsUnit.Point);
        protected Font _clientfont = null;
        protected ServerFont _serverfont = new ServerFont();
        protected Color _backcolor = Color.White;
        protected Color _forecolor = SystemColors.ControlText;
        protected BorderSide _borderside = new BorderSide(true, true, true, true);
        protected int _borderwidth = 1;
        protected Color _bordercolor = Color.Empty;//DefaultConfigs.LineColor;
        protected int _relativey;
        protected int _runtimeheight;
        protected bool _keeppos = false;
        protected bool _bactive = false;
        protected bool _binactiverow = false;
        protected bool _bcontrolauth = true;
        protected int _crossindex = -1;
        protected Report _inreport;
        //		protected const int SCHEMA = 1;
        protected ReportStates _understate = ReportStates.Designtime;

        private SuperLabel _super;
        protected object _tag;
        protected DrillData  _drilltag;
        protected string _scriptname = null;
        protected int _realy;
        protected int _realx;
        protected string _unformatvalue;
        protected string _identitycaption = "";
        protected bool _bexpand=true;
        protected int _visibleposition = -1;
        protected string _oldcaption;
        protected string _oldname;
        protected bool _bsupportlocate = true;
        protected string _twcaption ;
        protected string _encaption;
        protected string _cncaption;
        protected CrossColumnType _crosscolumntype = CrossColumnType.None;
        protected string _scriptid;
        protected bool _bshowonx=true;
        #endregion

        #region Constructor
        public Rect()
        {
            _relativey = DefaultConfigs.SECTIONHEADERHEIGHT + 10;
            //_x = 10; _y = 20; //_relativey = 20;
            _w = HSIZE; _h = VSIZE;
            SetType();
            SetDefault();
            SetSolid();
        }

        public Rect(int x, int y)
            : this()
        {
            _x = x; _y = y;
        }

        public Rect(int x, int y, int width, int height)
            : this(x, y)
        {
            _w = width; _h = height;
        }

        public Rect(Rect rect)
        {
            _x = rect.X;
            _y = rect.Y;
            _relativey = rect.RelativeY;
            _runtimeheight = rect.RuntimeHeight;
            _w = rect.Width;
            _h = rect.Height;
            _caption = rect.Caption;
            _keeppos = rect.KeepPos;
            _name = rect.Name;
            _captionalign = rect.CaptionAlign;
            _visible = rect.Visible;
            _zorder = rect.Z_Order;
            _parent = rect.Parent;
            _prepaintevent = rect.PrepaintEvent;
            _serverfont = rect.ServerFont;
            _backcolor = rect.BackColor;
            _forecolor = rect.ForeColor;
            _borderside = (BorderSide)((ICloneable)rect.Border).Clone();
            _borderwidth = rect.BorderWidth;
            _bordercolor = rect.BorderColor;
            _bcontrolauth = rect.bControlAuth;
            _crossindex = rect.CrossIndex;
            _understate = rect.UnderState;
            _tag = rect.Tag;
            _identitycaption = rect.GetIdentityCaption();
            _bexpand = rect.bExpand;
            _visibleposition = rect.VisiblePosition;
            _oldname = rect.OldName;
            _bhidden = rect.bHidden;
            _bsupportlocate = rect.bSupportLocate;
            _crosscolumntype = rect.CrossColumnType;
            _scriptid = rect.ScriptIDOnly;
            _unformatvalue = rect.UnFormatValue;
            _bshowonx = rect.bShowOnX;
            SetType();
            SetSolid();
        }

        protected int _version = 1;
        protected Rect(SerializationInfo info, StreamingContext context)
        {
            try
            {
                _version = info.GetInt32("Version");
            }
            catch
            {
            }
            SetLocation((Point)info.GetValue("Location", typeof(Point)));
            SetSize((Size)info.GetValue("Size", typeof(Size)));
            _y = _relativey;
            _type = info.GetString("Type");

            _name = info.GetString("Name");
            _caption = info.GetString("Caption");
            _captionalign = (ContentAlignment)info.GetValue("CaptionAlign", typeof(ContentAlignment));
            _visible = info.GetBoolean("Visible");
            _bhidden  = info.GetBoolean("bHidden");
            _understate = (ReportStates)info.GetValue("UnderState", typeof(ReportStates));
            _keeppos = info.GetBoolean("KeepPos");
            _borderside = (BorderSide)info.GetValue("BorderSide", typeof(BorderSide));
            _borderwidth = info.GetInt32("BorderWidth");
            _backcolor = Color.FromArgb(info.GetInt32("BackColor"));
            _forecolor = Color.FromArgb(info.GetInt32("ForeColor"));
            _bordercolor = Color.FromArgb(info.GetInt32("BorderColor"));
            _zorder = info.GetInt32("ZOrder");
            _prepaintevent = info.GetString("PrepaintEvent");
            _bcontrolauth = info.GetBoolean("bControlAuth");
            _crossindex = info.GetInt32("CrossIndex");
            _identitycaption = info.GetString("IdentityCaption");
            _oldname = info.GetString("OldName");
            _bsupportlocate = info.GetBoolean("bSupportLocate");
            

            if (_version > 1)
            {
                _serverfont = (ServerFont)info.GetValue("ServerFont", typeof(ServerFont));
                _twcaption = info.GetString("TWCaption");
                _encaption = info.GetString("ENCaption");
                _crosscolumntype = (CrossColumnType )info.GetValue("CrossColumnType", typeof(CrossColumnType));
            }
            if (_version > 20)
            {
                _cncaption = info.GetString("CNCaption");
            }
            
            if (_version >= 91)
                _visibleposition = info.GetInt32("VisiblePosition");
            if (_version >= 93)
                _scriptid = info.GetString("ScriptID");
            if (_version >= 100)
                _unformatvalue = info.GetString("UnFormatValue");
            if (_version >= 102)
                _bshowonx = info.GetBoolean("bShowOnX");

            if (!ClientReportContext.bInServerProcess)
                InerpretCaption();
        }

        protected virtual void InerpretCaption()
        {
            if (this._caption.IndexOf("U8.") == 0)
            {
                int levelExpand = this._caption.IndexOf("$$");
                if (levelExpand > 0)
                {
                    string level = _caption.Substring(levelExpand + 2, _caption.Length - levelExpand - 2);
                    _caption = _caption.Substring(0, levelExpand);
                    _caption = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString(_caption, ClientReportContext.LocaleID) + level;
                }
                else
                    _caption = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString(this._caption, ClientReportContext.LocaleID);
            }
            
        }
        #endregion

        #region Drawing 成员
        [Browsable(false)]
        public virtual bool bShowOnX
        {
            get
            {
                return _bshowonx;
            }
            set
            {
                _bshowonx = value;
            }
        }

        public void SetY(int y)
        {
            _y = y;
            _relativey = y;
        }

        public void DefaultHeight()
        {
            _h = VSIZE;
        }

        [Browsable(false)]
        public bool bScriptIDEmpty
        {
            get
            {
                return string.IsNullOrEmpty(_scriptid);
            }
        }

        [Browsable(false)]
        public string ScriptIDOnly
        {
            get
            {
                return _scriptid;
            }
        }

        [Browsable(false)]
        public string ScriptID
        {
            get
            {
                return string.IsNullOrEmpty(_scriptid) ? _name : _scriptid;
            }
            set
            {
                _scriptid = value;
            }
        }

        [Browsable(false)]
        public DrillData  DrillTag
        {
            get
            {
                return _drilltag;
            }
            set
            {
                _drilltag = value;
            }
        }

        [Browsable(false)]
        public CrossColumnType CrossColumnType
        {
            get
            {
                return _crosscolumntype;
            }
            set
            {
                _crosscolumntype = value;
            }
        }

        [Browsable(false)]
        public bool bCrossDynamicColumn
        {
            get
            {
                return _crosscolumntype != CrossColumnType.None;
            }
        }

        [Browsable(false)]
        public virtual  int  VisiblePosition
        {
            get
            {
                return _visibleposition ;
            }
            set
            {
                _visibleposition  = value;
            }
        }

        [Browsable(false)]
        public string UnFormatValue
        {
            get
            {
                return _unformatvalue;
            }
            set
            {
                _unformatvalue = value;
            }
        }
        [Browsable(false)]
        public int RealX
        {
            get
            {
                return _realx;
            }
        }

        [Browsable(false)]
        public int RealY
        {
            get
            {
                return _realy;
            }
        }


        [Browsable(false)]
        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

        [Browsable(false)]
        public virtual ReportStates UnderState
        {
            get
            {
                return _understate;
            }
            set
            {
                _understate = value;
            }
        }

        [Browsable(false)]
        public Report Report
        {
            get
            {
                GetReport();
                return _inreport;
            }
            set
            {
                _inreport = value;
            }
        }
        public event EventHandler GetReportEvent;
        public void GetReport()
        {
            if (GetReportEvent != null)
                GetReportEvent(this, null);
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis1")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis1")]
        public virtual System.Drawing.Point Location
        {
            get
            {
                return new System.Drawing.Point(_x, _relativey);
            }
            set
            {
                if (BeforeLocationChanged != null)
                    BeforeLocationChanged(this, null);
                if (value.X != _x || value.Y != _relativey)
                {
                    _x = value.X;
                    _relativey = value.Y;
                    _y = _parent.Y + _relativey;
                    if (AfterLocationChanged != null)
                        AfterLocationChanged(this, null);
                }
            }
        }

        [Browsable(true)]
        [DisplayText("UFIDA.U8.UAP.Services.ReportElements.Rect.是否控制权限")]
        [LocalizeDescription("UFIDA.U8.UAP.Services.ReportElements.Rect.是否控制权限")]
        public virtual bool bControlAuth
        {
            get
            {
                return _bcontrolauth;
            }
            set
            {
                if (_bcontrolauth != value)
                {
                    _bcontrolauth = value;
                    OnOtherChanged(new PropertyChangeArgs( PropertyType.ControlAuth));
                }
            }
        }

        public void SetControlAuth(bool b)
        {
            _bcontrolauth = b;
        }

        [Browsable(false)]
        public int CrossIndex
        {
            get
            {
                return _crossindex;
            }
            set
            {
                _crossindex = value;
            }
        }

        [Browsable(false)]
        public bool bExpand
        {
            get
            {
                return _bexpand ;
            }
            set
            {
                _bexpand = value;
            }
        }

        [Browsable(false)]
        public virtual SuperLabel Super
        {
            get
            {
                return _super;
            }
            set
            {
                _super = value;
            }
        }

        [Browsable(false)]
        public virtual int RelativeY
        {
            get
            {
                return _relativey;
            }
            set
            {
                _relativey = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis2")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis2")]
        public virtual System.Drawing.Size Size
        {
            get
            {
                return new System.Drawing.Size(_w, _h);
            }
            set
            {
                if (value.Width > 2000 || value.Height > 1000)
                    return;
                if (BeforeSizeChanged != null)
                    BeforeSizeChanged(this, null);
                if (value.Width != _w || value.Height != _h)
                {
                    _w = value.Width;
                    if(!(this is IWithSizable))
                        _h = value.Height;
                    if (AfterSizeChanged != null)
                        AfterSizeChanged(this, null);
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis3")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis3")]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (BeforeNameChanged != null)
                    BeforeNameChanged(this, null);
                if (_name.ToLower() != value.ToLower())
                {
                    _name = value;
                    if (AfterNameChanged != null)
                        AfterNameChanged(this, null);
                }
            }
        }

        [Browsable(false)]
        public string Type
        {
            get
            {
                return _type;
            }
        }

        public virtual string GetStateType()
        {
            return Type;
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.ShowVisible")]
        [LocalizeDescription("U8.UAP.Services.Report.ShowVisible")]
        public virtual bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    OnOtherChanged(new PropertyChangeArgs(PropertyType.Visible ));
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.SupportLocateSearch")]
        [LocalizeDescription("U8.UAP.Services.Report.SupportLocateSearch")]
        public virtual bool bSupportLocate
        {
            get
            {
                return _bsupportlocate;
            }
            set
            {
                _bsupportlocate = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.Report.SolidItem")]
        [LocalizeDescription("U8.UAP.Services.Report.SolidItem")]
        public virtual bool bSolid
        {
            get
            {
                return _bsolid ;
            }
            set
            {
                _bsolid = value;
                if (_bsolid )
                    Visible = true;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.AssistedItem")]
        [LocalizeDescription("U8.UAP.Services.Report.AssistedItem")]
        public virtual bool bHidden
        {
            get
            {
                return _bhidden ;
            }
            set
            {
                _bhidden = value;
                if (_bhidden)
                    Visible = false;
            }
        }

        [Browsable(false)]
        public virtual int VisibleWidth
        {
            get
            {
                if (_visible)
                    return _w;
                return 0;
            }
        }

        [Browsable(false)]
        public virtual Section Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    if (ParentChanged != null)
                    {
                        ParentChanged(null, null);
                    }
                }
            }
        }

        [Browsable(false)]
        public  string OldCaption
        {
            get
            {
                return _oldcaption;
            }
            set
            {
                _oldcaption = value;
            }
        }

        [Browsable(false)]
        public string OldName
        {
            get
            {
                return _oldname ;
            }
            set
            {
                _oldname  = value;
            }
        }

        [Browsable(false)]
        public string Value
        {
            get
            {
                return _caption ;
            }
            set
            {
                _caption  = value;
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis4")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis4")]
        public virtual bool KeepPos
        {
            get
            {
                return _keeppos;
            }
            set
            {
                if (_keeppos != value)
                {
                    _keeppos = value;
                    OnOtherChanged(new PropertyChangeArgs(PropertyType.KeepPos ));
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis5")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis5")]
        public virtual string Caption
        {
            get
            {
                if (_caption != null)
                {
                    if (this._caption.IndexOf("U8.") == 0)
                    {
                        int levelExpand = this._caption.IndexOf("$$");
                        if (levelExpand > 0)
                        {
                            string level = _caption.Substring(levelExpand + 2, _caption.Length - levelExpand - 2);
                            _caption = _caption.Substring(0, levelExpand);
                            _caption = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString(_caption, ClientReportContext.LocaleID) + level;
                        }
                        else
                            _caption = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString(this._caption, ClientReportContext.LocaleID);

                    }
                    //LocaleCaption();
                }
                return _caption;
            }
            set
            {
                if (BeforeCaptionChanged != null)
                    BeforeCaptionChanged(this, null);
                if (_caption.ToLower() != value.ToLower())
                {
                    _caption = value;
                    if (AfterCaptionChanged != null)
                        AfterCaptionChanged(this, null);
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.IndentifierTitle")]
        [LocalizeDescription("U8.UAP.Services.Report.IndentifierTitle")]
        public virtual string IdentityCaption
        {
            get
            {
                if (_identitycaption == "")
                    return _caption;
                return _identitycaption;
            }
            set
            {
                if (_identitycaption.ToLower() != value.ToLower())
                {
                    _identitycaption = value;
                    OnOtherChanged(new PropertyChangeArgs(PropertyType.IdentityCaption ));
                }
            }
        }

        public string GetIdentityCaption()
        {
            return _identitycaption;
        }

        [Browsable(false)]
        public virtual int Z_Order
        {
            get
            {
                return _zorder;
            }
            set
            {
                if (_zorder != value)
                {
                    _zorder = value;
                    if (Z_OrderChanged != null)
                    {
                        Z_OrderChanged(this, null);
                    }
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis6")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis6")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual string PrepaintEvent
        {
            get
            {
                return _prepaintevent;
            }
            set
            {
                if (_prepaintevent != value)
                {
                    _prepaintevent = value;
                }
            }
        }
        
        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis7")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis7")]
        public virtual System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return _captionalign;
            }
            set
            {
                if (BeforeCaptionAlignChanged != null)
                    BeforeCaptionAlignChanged(this, null);
                if (_captionalign != value)
                {
                    _captionalign = value;
                    if (AfterCaptionAlignChanged != null)
                        AfterCaptionAlignChanged(this, null);
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis8")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis8")]
        public virtual Color BackColor
        {
            get
            {
                return _backcolor;
            }
            set
            {
                if (BeforeBackColorChanged != null)
                    BeforeBackColorChanged(this, null);
                if (_backcolor != value)
                {
                    _backcolor = value;
                    if (AfterBackColorChanged != null)
                        AfterBackColorChanged(this, null);
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis9")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis9")]
        public virtual Color ForeColor
        {
            get
            {
                return _forecolor;
            }
            set
            {
                if (BeforeForeColorChanged != null)
                    BeforeForeColorChanged(this, null);
                if (_forecolor != value)
                {
                    _forecolor = value;
                    if (AfterForeColorChanged != null)
                        AfterForeColorChanged(this, null);
                }
            }
        }

        [Browsable(false)]
        public ServerFont ServerFont
        {
            get
            {
                return _serverfont;
            }
            set
            {
                _serverfont = value;
                if (_clientfont != null)
                {
                    _clientfont.Dispose();
                    _clientfont = null;
                }
            }
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis10")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis10")]
        public virtual Font ClientFont
        {
            get
            {
                if (_clientfont == null)
                    _clientfont=GetClientFont(_serverfont );
                return _clientfont;
            }
            set
            {
                if (BeforeFontChanged != null)
                    BeforeFontChanged(this, null);
                if (_clientfont != value)
                {
                    _clientfont = value;
                    _serverfont.FromFont(_clientfont);
                    if (AfterFontChanged != null)
                        AfterFontChanged(this, null);
                }
            }
        }

        protected virtual   Font  GetClientFont(ServerFont sf)
        {
            return new Font(sf.FontName,
                sf.FontSize,
                sf.FontStyle,
                sf.FontUnit,
                sf.GdiCharSet,
                sf.GdiVerticalFont);
        }

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis11")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis11")]
        public virtual BorderSide Border
        {
            get
            {
                _borderside.BeforeBorderChanged -= new EventHandler(BorderSide_BeforeBorderChanged);
                _borderside.BeforeBorderChanged += new EventHandler(BorderSide_BeforeBorderChanged);
                _borderside.AfterBorderChanged -= new EventHandler(BorderSide_AfterBorderChanged);
                _borderside.AfterBorderChanged += new EventHandler(BorderSide_AfterBorderChanged);
                return _borderside;
            }
            set
            {
                _borderside = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis12")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis12")]
        public virtual int BorderWidth
        {
            get
            {
                return _borderwidth;
            }
            set
            {
                if (value < 0 || value > 10)
                    return;
                if (BeforeBorderWidthChanged != null)
                    BeforeBorderWidthChanged(this, null);
                if (_borderwidth != value)
                {
                    _borderwidth = value;
                    if (AfterBorderWidthChanged != null)
                        AfterBorderWidthChanged(this, null);
                }
            }
        }

        [Browsable(true)]
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
                if (BeforeBorderColorChanged != null)
                    BeforeBorderColorChanged(this, null);
                if (_bordercolor != value)
                {
                    _bordercolor = value;
                    if (AfterBorderColorChanged != null)
                        AfterBorderColorChanged(this, null);
                }
            }
        }

        public void SetBackColor(Color backcolor)
        {
            _backcolor = backcolor;
        }

        public void SetForeColor(Color forecolor)
        {
            _forecolor = forecolor;
        }

        public void SetClientFont(Font font)
        {
            _clientfont = font;
            _serverfont.FromFont(_clientfont);
        }

        public void SetBorderWidth(int borderwidth)
        {
            _borderwidth = borderwidth;
        }

        public void SetBorderColor(Color bordercolor)
        {
            _bordercolor = bordercolor;
        }

        public BorderSide getBorder()
        {
            _borderside.BeforeBorderChanged -= new EventHandler(BorderSide_BeforeBorderChanged);
            _borderside.AfterBorderChanged -= new EventHandler(BorderSide_AfterBorderChanged);
            return _borderside;
        }

        public event System.EventHandler BeforeBackColorChanged;
        public event System.EventHandler BeforeForeColorChanged;
        public event System.EventHandler BeforeFontChanged;
        public event System.EventHandler BeforeBorderColorChanged;
        public event System.EventHandler BeforeBorderWidthChanged;
        public event System.EventHandler BeforeBorderChanged;
        public event System.EventHandler BeforeNameChanged;
        public event System.EventHandler BeforeLocationChanged;
        public event System.EventHandler BeforeSizeChanged;
        public event System.EventHandler BeforeCaptionChanged;
        public event System.EventHandler BeforeCaptionAlignChanged;

        public event EventHandler ParentChanged;
        public event System.EventHandler Z_OrderChanged;
        public event System.EventHandler OtherPropertyChanged;

        public event System.EventHandler AfterBackColorChanged;
        public event System.EventHandler AfterForeColorChanged;
        public event System.EventHandler AfterFontChanged;
        public event System.EventHandler AfterBorderColorChanged;
        public event System.EventHandler AfterBorderWidthChanged;
        public event System.EventHandler AfterBorderChanged;
        public event System.EventHandler AfterNameChanged;
        public event System.EventHandler AfterLocationChanged;
        public event System.EventHandler AfterSizeChanged;
        public event System.EventHandler AfterCaptionChanged;
        public event System.EventHandler AfterCaptionAlignChanged;

        public virtual void SetName(string name)
        {
            _name = name;
        }

        public virtual void SetVisible(bool visible)
        {
            _visible = visible;
        }

        public virtual void SetCaption(string caption)
        {
            _caption = caption;
        }

        public virtual void SetCaptionAlign(ContentAlignment ca)
        {
            _captionalign = ca;
        }

        public virtual void SetZOrder(int zorder)
        {
            _zorder = zorder;
        }

        public virtual void SetLocation(Point pt)
        {
            _x = pt.X;
            _relativey = pt.Y;
            if (_parent != null)
                _y = _relativey + _parent.Y;
        }

        public virtual void SetSize(Size size)
        {
            _w = size.Width;
            _h = size.Height;
        }

        [Browsable(false)]
        public virtual   bool bActive
        {
            get
            {
                return _bactive;
            }
            set
            {
                _bactive = value;
            }
        }

        [Browsable(false)]
        public virtual   bool bInActiveRow
        {
            get
            {
                return _binactiverow;
            }
            set
            {
                _binactiverow = value;
            }
        }

        [Browsable(false)]
        public virtual int X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        [Browsable(false)]
        public virtual int Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                if (_parent != null)
                    _relativey = _y - _parent.Y;
            }
        }

        [Browsable(false)]
        public virtual int Width
        {
            get
            {
                return _w;
            }
            set
            {
                _w = value;
            }
        }

        [Browsable(false)]
        public virtual int Height
        {
            get
            {
                return _h;
            }
            set
            {
                _h = value;
            }
        }

        public void SetRuntimeHeight(int height)
        {
            _runtimeheight = height;
        }

        [Browsable(false)]
        public virtual int RuntimeHeight
        {
            get
            {
                if (_runtimeheight == 0)
                    return _h;
                return _runtimeheight;
            }
        }

        [Browsable(false)]
        public string CNCaption
        {
            get
            {
                return _cncaption;
            }
            set
            {
                _cncaption = value;
            }
        }

        [Browsable(false)]
        public string ENCaption
        {
            get
            {
                return _encaption;
            }
            set
            {
                _encaption = value;
            }
        }

        [Browsable(false)]
        public string TWCaption
        {
            get
            {
                return _twcaption ;
            }
            set
            {
                _twcaption = value;
            }
        }


        [Browsable(false)]
        public virtual int ExpandHeight
        {
            get
            {
                return this.RuntimeHeight ;
            }
        }

        [Browsable(false)]
        public virtual int MetaHeight
        {
            get
            {
                return _h;
            }
        }

        public virtual System.Drawing.Rectangle getRects()
        {
            return AdjustRect(_x, _y, _w, _h);
        }

        public void AppendSuperCaption()
        {
            Cell super = this.Super;
            while (super != null)
            {
                _caption = super.Caption + " - " + _caption;
                super = super.Super;
            }
        }

        private bool _bomit;

        [Browsable(false)]
        public bool bOmit
        {
            get
            {
                return _bomit;
            }
        }

        protected System.Drawing.Image AnalysisIndicator(CompareValue cv)
        {
            System.Drawing.Image image = null;
            if (cv != null && !string.IsNullOrEmpty(cv.Expression1))
            {
                double thisvalue = 0;
                if (!string.IsNullOrEmpty(this._caption))
                    thisvalue = Convert.ToDouble(_caption);

                double comparevalue1 = Convert.ToDouble(cv.Expression1);

                double comparevalue2 = 0;
                bool hasvalue2 = false;
                if (!string.IsNullOrEmpty(cv.Expression2))
                {
                    comparevalue2 = Convert.ToDouble(cv.Expression2);
                    hasvalue2 = true;
                }

                if (hasvalue2)
                {
                    double tmp1 = Math.Min(comparevalue1, comparevalue2);
                    double tmp2 = Math.Max(comparevalue1, comparevalue2);
                    comparevalue1 = tmp1;
                    comparevalue2 = tmp2;

                    if (thisvalue < comparevalue1)
                    {
                        if (cv.ViewStyle == IndicatorViewType.BackColor)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter || cv.Performance== IndicatorPerformance.BothSidesBetter )
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _backcolor = Color.Green;
                                    _forecolor = Color.White;
                                }
                            }
                            else
                            {
                                _backcolor = Color.Red;
                                _forecolor = Color.White;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.FontColor)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _forecolor = Color.Green;
                                }
                            }
                            else
                            {
                                _forecolor = Color.Red;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.RGBLight)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("green.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("red.ico");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.SmileCry)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("smile.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("cry.png");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.UpDown)
                        {
                            //if (cv.Performance == IndicatorPerformance.LessBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            //{
                            //    image = String4Report.GetImage("up.gif");
                            //}
                            //else
                            //{
                                image = String4Report.GetImage("down.ico");
                            //}
                        }  
                    }
                    else if (thisvalue >= comparevalue1 && thisvalue < comparevalue2)
                    {
                        if (cv.ViewStyle == IndicatorViewType.BackColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance== IndicatorPerformance.LessBetter )
                            {
                                _backcolor = Color.Yellow ;
                                _forecolor = Color.Blue ;
                            }
                            else if (cv.Performance == IndicatorPerformance.AmidBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _backcolor = Color.Green;
                                    _forecolor = Color.White;
                                }
                            }
                            else
                            {
                                _backcolor = Color.Red;
                                _forecolor = Color.White;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.FontColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.LessBetter)
                            {
                                _forecolor  = Color.Yellow;
                            }
                            else if (cv.Performance == IndicatorPerformance.AmidBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    _forecolor = Color.Green;
                            }
                            else
                            {
                                _forecolor = Color.Red;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.RGBLight)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.LessBetter)
                            {
                                image = String4Report.GetImage("yellow.ico");
                            }
                            else if (cv.Performance == IndicatorPerformance.AmidBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("green.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("red.ico");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.SmileCry)
                        {
                            if (cv.Performance == IndicatorPerformance.AmidBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("smile.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("cry.png");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.UpDown)
                        {
                            //if (cv.Performance == IndicatorPerformance.AmidBetter)
                            //{
                            if (!cv.FlagOnBadOnly)
                                image = String4Report.GetImage("up.gif");
                            //}
                            //else
                            //{
                            //    image = String4Report.GetImage("down.ico");
                            //}
                        }  
                    }
                    else if(thisvalue>=comparevalue2)
                    {
                        if (cv.ViewStyle == IndicatorViewType.BackColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _backcolor = Color.Green;
                                    _forecolor = Color.White;
                                }
                            }
                            else
                            {
                                _backcolor = Color.Red;
                                _forecolor = Color.White;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.FontColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    _forecolor = Color.Green;
                            }
                            else
                            {
                                _forecolor = Color.Red;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.RGBLight)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("green.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("red.ico");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.SmileCry)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("smile.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("cry.png");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.UpDown)
                        {
                            //if (cv.Performance == IndicatorPerformance.GreaterBetter || cv.Performance == IndicatorPerformance.BothSidesBetter)
                            //{
                            if (!cv.FlagOnBadOnly)
                                image = String4Report.GetImage("up.gif");
                            //}
                            //else
                            //{
                            //    image = String4Report.GetImage("down.ico");
                            //}
                        } 
                    }
                }
                else
                {
                    if (thisvalue >= comparevalue1)
                    {
                        if (cv.ViewStyle == IndicatorViewType.BackColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _backcolor = Color.Green;
                                    _forecolor = Color.White;
                                }
                            }
                            else
                            {
                                _backcolor = Color.Red;
                                _forecolor = Color.White;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.FontColor)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    _forecolor = Color.Green;
                            }
                            else
                            {
                                _forecolor = Color.Red;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.RGBLight)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("green.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("red.ico");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.SmileCry)
                        {
                            if (cv.Performance == IndicatorPerformance.GreaterBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("smile.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("cry.png");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.UpDown)
                        {
                            //if (cv.Performance == IndicatorPerformance.GreaterBetter)
                            //{
                            if (!cv.FlagOnBadOnly)
                                image = String4Report.GetImage("up.gif");
                            //}
                            //else
                            //{
                            //    image = String4Report.GetImage("down.ico");
                            //}
                        }                        
                    }
                    else if (thisvalue < comparevalue1)
                    {
                        if (cv.ViewStyle == IndicatorViewType.BackColor)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter )
                            {
                                if (!cv.FlagOnBadOnly)
                                {
                                    _backcolor = Color.Green;
                                    _forecolor = Color.White;
                                }
                            }
                            else
                            {
                                _backcolor = Color.Red;
                                _forecolor = Color.White;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.FontColor)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    _forecolor = Color.Green;
                            }
                            else
                            {
                                _forecolor = Color.Red;
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.RGBLight)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("green.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("red.ico");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.SmileCry)
                        {
                            if (cv.Performance == IndicatorPerformance.LessBetter)
                            {
                                if (!cv.FlagOnBadOnly)
                                    image = String4Report.GetImage("smile.ico");
                            }
                            else
                            {
                                image = String4Report.GetImage("cry.png");
                            }
                        }
                        else if (cv.ViewStyle == IndicatorViewType.UpDown)
                        {
                            //if (cv.Performance == IndicatorPerformance.LessBetter)
                            //{
                            //    image = String4Report.GetImage("up.gif");
                            //}
                            //else
                            //{
                                image = String4Report.GetImage("down.ico");
                            //}
                        }  
                    }
                }
            }
            return image;
        }

        protected void DrawCompareImage(Graphics g,System.Drawing.Image image)
        {
            Rectangle rect = new Rectangle(_x + 1, _y + 1, 16, 16);//VSIZE - 2, VSIZE - 2);
            g.DrawImage(image, rect);
        }

        protected virtual void draw(System.Drawing.Graphics g)
        {
            if (_understate == ReportStates.Designtime)
                SetRuntimeHeight(g,this._caption);
            StringFormat style =HandleStringFormat();
            if (_backcolor != Color.FromArgb(235, 243, 252))
            {
                DrawBackGround(g);
            }
            DrawBorder(g);
            DrawShade(g, style);
            DrawRect(g, style);
            DrawFocus(g);
        }

        protected void DrawFocus(System.Drawing.Graphics g)
        {
            if (_bactive)
                ControlPaint.DrawFocusRectangle(g, new Rectangle(_x, _y, _w, _h), Color.Yellow, Color.Red);
        }

        private StringFormat HandleStringFormat()
        {
            _bomit = false;
            StringFormat style = getStringFormat();
            if (_h < _runtimeheight)
            {
                style.FormatFlags = StringFormatFlags.NoWrap;
                _bomit = true;
            }
            else
                style.Trimming = StringTrimming.None;
            return style;
        }
        protected  void DrawBackGround(System.Drawing.Graphics g)
        {
            Color backcolor = _backcolor;
            if (!Visible)
                backcolor = Color.DarkGray;
            else if (_bactive ||_binactiverow )
                backcolor = Color.Blue;
            //using (SolidBrush sbback = new SolidBrush(backcolor ))
            //{
                //if (!_binactiverow)
                    //g.FillRectangle(sbback, _x , _y , _w , _h );
            //}
            Rectangle rect = new Rectangle(_x, _y, _w, _h);
            DrawBackGround(g, rect, backcolor);
        }

        protected void DrawBackGround(Graphics g, Rectangle rect,Color backcolor)
        {
            if (rect.Width > 0 && rect.Height > 0)
            {
                using (GraphicsPath path = new GraphicsPath())
                using (LinearGradientBrush filler = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.White, backcolor, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    path.AddRectangle(rect);
                    g.FillPath(filler, path);
                }
            }
        }

        protected void DrawBorder(System.Drawing.Graphics g)
        {
            #region draw border
            //Color backlight = ControlPaint.Light(_bordercolor);
            //Color backlightlight = Color.White; //ControlPaint.LightLight(bordercolor);
            //Color backdark = _bordercolor;
            //Color backdarkdark = ControlPaint.Dark(_bordercolor);

            ////Color backlight = ControlPaint.Light(_bordercolor);
            ////Color backlightlight = Color.White; //ControlPaint.LightLight(bordercolor);
            ////Color backdark = ControlPaint.Dark(_bordercolor);
            ////Color backdarkdark = ControlPaint.DarkDark(_bordercolor);

            //using (Brush brushlight = new SolidBrush(backlight))
            //using (Brush brushdark = new SolidBrush(backdark))
            //using (Pen penlight = new Pen(brushlight, 1))
            //using (Pen pendark = new Pen(brushdark, 1))
            //using (Brush brushlightlight = new SolidBrush(backlightlight))
            //using (Brush brushdarkdark = new SolidBrush(backdarkdark))
            //using (Pen penlightlight = new Pen(brushlightlight, 1))
            //using (Pen pendarkdark = new Pen(brushdarkdark, 1))
            //{
            //    //left
            //    if (_borderside.Left)
            //    {
            //        penlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        penlightlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        g.DrawLine(penlight, _x, _y, _x, _y + _h);
            //        g.DrawLine(penlightlight, _x + 1, _borderside.Top?(_y + 1):_y, _x + 1, _y + _h - 1);
            //    }
            //    else if (_understate == ReportStates.Designtime)
            //    {
            //        penlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom ;
            //        penlightlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom ;
            //        penlight.DashPattern = new float[] { 1, 1 };
            //        penlightlight.DashPattern = new float[] { 1, 1 };
            //        g.DrawLine(penlight, _x, _y, _x, _y + _h);
            //        g.DrawLine(penlightlight, _x + 1, _borderside.Top ? (_y + 1) : _y, _x + 1, _y + _h - 1);
            //    }

            //    //top
            //    if (_borderside.Top)
            //    {
            //        penlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        penlightlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        g.DrawLine(penlight, _x, _y, _x + _w, _y);
            //        g.DrawLine(penlightlight, _borderside.Left ?(_x + 1):_x, _y + 1, _x + _w - 1, _y + 1);
            //    }
            //    else if (_understate == ReportStates.Designtime)
            //    {
            //        penlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom ;
            //        penlightlight.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom ;
            //        penlight.DashPattern = new float[] { 1, 1 };
            //        penlightlight.DashPattern = new float[] { 1, 1 };
            //        g.DrawLine(penlight, _x, _y, _x + _w, _y);
            //        g.DrawLine(penlightlight, _borderside.Left ? (_x + 1) : _x, _y + 1, _x + _w - 1, _y + 1);
            //    }

            //    //right
            //    if (_borderside.Right)
            //    {
            //        pendark.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        pendarkdark.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        g.DrawLine(pendark, _x + _w - 1, _borderside.Top ? (_y + 1) : _y, _x + _w - 1, _y + _h - 1);
            //        g.DrawLine(pendarkdark, _x + _w, _y, _x + _w, _y + _h);
            //    }
            //    else if (_understate == ReportStates.Designtime)
            //    {
            //        pendark.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            //        pendarkdark.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            //        pendark.DashPattern = new float[] { 1, 1 };
            //        pendarkdark.DashPattern = new float[] { 1, 1 };
            //        g.DrawLine(pendark, _x + _w - 1, _borderside.Top ? (_y + 1) : _y, _x + _w - 1, _y + _h - 1 );
            //        g.DrawLine(pendarkdark, _x + _w, _y, _x + _w, _y + _h +1);
            //    }

            //    //bottom
            //    if (_borderside.Bottom)
            //    {
            //        pendark.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        pendarkdark.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            //        g.DrawLine(pendark, _borderside.Left ?(_x + 1):_x, _y + _h - 1, _x + _w - 1, _y + _h - 1);
            //        g.DrawLine(pendarkdark, _x, _y + _h, _x + _w, _y + _h);
            //    }
            //    else if (_understate == ReportStates.Designtime)
            //    {
            //        pendark.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            //        pendarkdark.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            //        pendark.DashPattern = new float[] { 1, 1 };
            //        pendarkdark.DashPattern = new float[] { 1, 1 };
            //        g.DrawLine(pendark, _borderside.Left ? (_x + 1) : _x, _y + _h - 1, _x + _w - 1, _y + _h - 1);
            //        g.DrawLine(pendarkdark, _x, _y + _h, _x + _w +1, _y + _h);
            //    }
            //}
            #endregion

            Color bordercolor = _bordercolor;
            if (_bordercolor.ToArgb() == Color.Empty.ToArgb())
                bordercolor = Color.Black;
            #region draw border
            using (Brush brush = new SolidBrush(bordercolor))
            using (Pen pen = new Pen(brush, 1))
            {
                //left
                if (_borderside.Left)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(pen, _x, _y, _x, _y + _h);
                }
                else if (_understate == ReportStates.Designtime)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[] { 1, 1 };
                    g.DrawLine(pen, _x, _y, _x, _y + _h);
                }

                //top
                if (_borderside.Top)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(pen, _x, _y, _x + _w, _y);
                }
                else if (_understate == ReportStates.Designtime)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[] { 1, 1 };
                    g.DrawLine(pen, _x, _y, _x + _w, _y);
                }

                //right
                if (_borderside.Right)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(pen, _x + _w, _y, _x + _w, _y + _h);
                }
                else if (_understate == ReportStates.Designtime)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[] { 1, 1 };
                    g.DrawLine(pen, _x + _w, _y, _x + _w, _y + _h );
                }

                //bottom
                if (_borderside.Bottom)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(pen, _x, _y + _h, _x + _w, _y + _h);
                }
                else if (_understate == ReportStates.Designtime)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[] { 1, 1 };
                    g.DrawLine(pen, _x, _y + _h, _x + _w , _y + _h);
                }
            }
            #endregion
        }

        protected void DrawShade(Graphics g, StringFormat style)
        {
            //using (SolidBrush sbfore = new SolidBrush(Color.LightGray ))
            //{
            //    g.DrawString(this._caption, this.ClientFont, sbfore, new Rectangle( _x +2 +1 ,  _y +2 +1 , _w -2 , _h - 2 ), style);
            //}
        }

        protected void DrawRect(Graphics g, StringFormat style)
        {
            Color forecolor = GetInfomationForeColr();            
            if (_binactiverow || _bactive)
                forecolor = Color.White;
            using (SolidBrush sbfore = new SolidBrush(forecolor))
            {
                g.DrawString(this._caption, this.ClientFont, sbfore, new Rectangle(_x + 2 , _y + 2 , _w - 2 , _h  ), style);
            }
        }

        protected virtual Color GetInfomationForeColr()
        {
            return _forecolor;
        }

        public void draw(Graphics g, int xcoordinate, int ycoordinate, int height)
        {
            int ox = _x;
            int oy = _y;
            int oh = _h;
            int ow = _w;
            _x = xcoordinate;
            _y = ycoordinate;
            _realy = _y;
            _realx = _x;
            _h = height;
            draw(g);
            _x = ox;
            _y = oy;
            _h = oh;
            _w = ow;
        }

        public virtual bool contains(Point pt)
        {
            return getRects().Contains(pt);
        }

        public void SetAlternativeStyle(IAlternativeStyle ias)
        {
            SetBackColor(ias.BackColor2);
            //SetBorderColor(ias.BorderColor2);
            SetForeColor(ias.ForeColor2);
            //ServerFont = ias.ServerFont2;
        }

        public virtual bool bContainedBy(Drawing d)
        {
            return d.getRects().Contains(this.getRects());
        }

        public virtual void SetRuntimeHeight(Graphics g,string s)
        {
            StringFormat style = getStringFormat();
            SizeF height;
            height = g.MeasureString(s, this.ClientFont, _w - 2 -1 , style);
            _runtimeheight = Math.Max(_h, Convert.ToInt32(height.Height));
        }        

        public void MoveX(int dx, int sx)
        {
            _x += dx - sx;
        }
        public void MoveY(int dy, int sy)
        {
            _y += dy - sy;
            _relativey = _y - _parent.Y;
        }
        public void ResizeX(int dx)
        {
            if (dx < 0)
                dx = 0;
            _w = _x + _w - dx;
            _x = dx;
        }
        public void ResizeY(int dy)
        {
            _h = _y + _h - dy;
            _y = dy;
            _relativey = _y - _parent.Y;
        }
        public void ResizeWidth(int dx)
        {
            _w = dx - _x;
        }
        public virtual void ResizeHeight(int dy)
        {
            _h = dy - _y;
        }

        public virtual bool bUnder(Cell cell)
        {
            if (_y == cell.Y && _x == cell.X && _x + _w == cell.X + cell.Width)
                return false;
            else
                return _y > cell.Y && _x >= cell.X && _x + _w <= cell.X + cell.Width;
        }

        #endregion

        #region protected
        public abstract void SetType();
        public virtual void SetDefault()
        {
        }
        public virtual void SetSolid()
        {
        }
        public virtual void AdjustSelf()
        {
            if (_w < 0)
            {
                _x += _w;
                _w = Math.Abs(_w);
            }
            if (_h < 0)
            {
                _y += _h;
                _h = Math.Abs(_h);
            }
        }

        private Rectangle AdjustRect(int x, int y, int w, int h)
        {
            if (w < 0)
            {
                x += w;
                w = Math.Abs(w);
            }
            if (h < 0)
            {
                y += h;
                h = Math.Abs(h);
            }
            return new Rectangle(x, y, w, h);
        }

        protected StringFormat getStringFormat()
        {
            StringFormat style = new StringFormat();
            switch (_captionalign)
            {
                case ContentAlignment.MiddleLeft:
                    style.Alignment = StringAlignment.Near;
                    style.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    style.Alignment = StringAlignment.Far;
                    style.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleCenter:
                    style.Alignment = StringAlignment.Center;
                    style.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    style.Alignment = StringAlignment.Near;
                    style.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomRight:
                    style.Alignment = StringAlignment.Far;
                    style.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    style.Alignment = StringAlignment.Center;
                    style.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.TopLeft:
                    style.Alignment = StringAlignment.Near;
                    style.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    style.Alignment = StringAlignment.Far;
                    style.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    style.Alignment = StringAlignment.Center;
                    style.LineAlignment = StringAlignment.Near;
                    break;
            }
            style.Trimming = StringTrimming.EllipsisCharacter;
            return style;
        }
        
        protected void OnOtherChanged(PropertyChangeArgs e)
        {
            if (OtherPropertyChanged != null)
                OtherPropertyChanged(this, e);
        }
        protected void BorderSide_BeforeBorderChanged(object sender, EventArgs e)
        {
            if (BeforeBorderChanged != null)
                BeforeBorderChanged(this, null);
        }
        protected void BorderSide_AfterBorderChanged(object sender, EventArgs e)
        {
            if (AfterBorderChanged != null)
                AfterBorderChanged(this, null);
        }
        #endregion

        #region ISerializable 成员

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", 102);//21--872;90--890
            info.AddValue("Location", this.Location);
            info.AddValue("Size", this.Size);
            info.AddValue("Type", _type);
            info.AddValue("Caption", _caption);
            info.AddValue("Name", _name);
            info.AddValue("CaptionAlign", _captionalign);
            info.AddValue("Visible", _visible);
            info.AddValue("bHidden", _bhidden );
            info.AddValue("UnderState", _understate);
            info.AddValue("BorderSide", _borderside);
            info.AddValue("BorderWidth", _borderwidth);
            info.AddValue("KeepPos", _keeppos);

            if (_clientfont != null)
                _serverfont.FromFont(_clientfont);
            
            info.AddValue("ServerFont", _serverfont);

            info.AddValue("BackColor", _backcolor.ToArgb());
            info.AddValue("ForeColor", _forecolor.ToArgb());
            info.AddValue("BorderColor", _bordercolor.ToArgb());
            info.AddValue("ZOrder", _zorder);
            info.AddValue("PrepaintEvent", _prepaintevent);
            info.AddValue("bControlAuth", _bcontrolauth);
            info.AddValue("CrossIndex", _crossindex);
            info.AddValue("IdentityCaption", _identitycaption);
            info.AddValue("OldName", _oldname);
            info.AddValue("bSupportLocate", _bsupportlocate);
            info.AddValue("TWCaption", _twcaption);
            info.AddValue("ENCaption", _encaption);
            info.AddValue("CNCaption", _cncaption);
            info.AddValue("CrossColumnType", _crosscolumntype);
            info.AddValue("VisiblePosition", _visibleposition);
            info.AddValue("ScriptID", _scriptid);
            info.AddValue("UnFormatValue", _unformatvalue);
            info.AddValue("bShowOnX", _bshowonx);
        }

        #endregion

        #region ICloneable 成员

        public abstract object Clone();

        #endregion

        #region IDisposable 成员

        public virtual void Dispose()
        {
            if (_clientfont != null)
            {
                _clientfont.Dispose();
                _clientfont = null;
            }

            _serverfont = null;
            _borderside = null;
            _parent = null;
            _inreport = null;
            _tag = null;
            _drilltag = null;

            _type = null;
            _caption = null;
            _name = null;
            _identitycaption = null;
            _scriptname = null;
            _unformatvalue = null;
            _oldcaption = null;
            _super = null;
        }

        #endregion
    }
}
