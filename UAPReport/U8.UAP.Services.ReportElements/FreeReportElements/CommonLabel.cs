using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Label 的摘要说明。
	/// </summary>
	[Serializable]
	public class CommonLabel:Rect,ISerializable,ICloneable,IDisposable,ILabelType ,IUserDefine,IApplyColorStyle,IInformationSender ,ICenterAlign
	{
		#region constructor
        protected LabelType _labeltype = LabelType.OtherLabel  ;
        protected string _userdefineitem = "";
        protected bool _bapplycolorstyle = false;
        protected string _informationid = "";
        protected bool _bcenteralign = false;
		public CommonLabel():base()
		{
		}
		public CommonLabel(int x,int y):base(x,y)
		{
		}

		public CommonLabel(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public CommonLabel(string caption):base()
		{
			_caption=caption;
		}
		public CommonLabel(int x,int y,string caption):base(x,y)
		{
			_caption=caption;
		}

		public CommonLabel(int x,int y,int width,int height,string caption):base(x,y,width,height)
		{
			_caption=caption;
		}

		public CommonLabel(CommonLabel label):base(label)
		{
            _labeltype = label.LabelType;
            _bapplycolorstyle = label.bApplyColorStyle;
            _informationid = label.InformationID;
            _bcenteralign = label.CenterAlign;
            _userdefineitem = label.UserDefineItem;
		}

		protected CommonLabel( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _labeltype = (LabelType)info.GetValue("LabelType", typeof(LabelType));
            if (_version > 1)
            {
                _userdefineitem = info.GetString("UserDefineItem");
                _bapplycolorstyle = info.GetBoolean("bApplyColorStyle");
            }
            if(_version>=90)
            {
                _informationid=info.GetString("InformationID");
            }
            if (_version >= 101)
                _bcenteralign = info.GetBoolean("CenterAlign");
		}

		public CommonLabel(Cell cell):base(cell as Rect )
		{
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="CommonLabel";			
		}
         
		public override void SetDefault()
		{
			_borderside.NoneBorder();
            _bcontrolauth = false;
		}

        protected override Font GetClientFont(ServerFont sf)
        {
            if (!string.IsNullOrEmpty(_informationid))
                sf.UnderLine = true;
            else
                sf.UnderLine = false;
            return base.GetClientFont(sf);
        }

        protected override Color GetInfomationForeColr()
        {
            Color c = _forecolor;
            if (!string.IsNullOrEmpty(_informationid))
                c = Color.Blue;
            return c;
        }

        [Browsable(false)]
        public override bool bControlAuth
        {
            get
            {
                return base.bControlAuth;
            }
            set
            {
                //base.bControlAuth = value;
            }
        }

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("LabelType", _labeltype);
            info.AddValue("UserDefineItem", _userdefineitem);
            info.AddValue("bApplyColorStyle", _bapplycolorstyle);
            info.AddValue("InformationID",_informationid);
            info.AddValue("CenterAlign", _bcenteralign);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new CommonLabel(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            base.Dispose();
		}

		#endregion

        #region ILabelType 成员
        [DisplayText("U8.Report.UserDefineItem")]
        [LocalizeDescription("U8.Report.UserDefineItem")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string UserDefineItem
        {
            get
            {
                return _userdefineitem;
            }
            set
            {
                _userdefineitem = value;
            }
        }

        [DisplayText("U8.UAP.Services.Report.LabelType")]
        [LocalizeDescription("U8.UAP.Services.Report.LabelType")]
        public virtual LabelType LabelType
        {
            get
            {
                return _labeltype;
            }
            set
            {
                if (_labeltype != value)
                {
                    _labeltype = value;
                    OnOtherChanged(null);
                }
            }
        }

        #endregion

        #region IApplyColorStyle 成员
        [DisplayText("U8.Report.ApplyColorStyle")]
        [LocalizeDescription("U8.Report.ApplyColorStyle")]
        public bool bApplyColorStyle
        {
            get
            {
                return _bapplycolorstyle;
            }
            set
            {
                _bapplycolorstyle = value;
                if (_bapplycolorstyle)
                {
                    if (this.Parent != null)
                    {
                        this.SetBackColor(this.Parent.BackColor);
                        //this.SetBorderColor(this.Parent.BorderColor);
                        this.ServerFont=this.Parent.ServerFont ;
                        this.ForeColor = this.Parent.ForeColor;
                    }
                }
            }
        }

        #endregion

        #region IInformationSender Members

        [Browsable(false)]
        public string InformationID
        {
            get
            {
                return _informationid;
            }
            set
            {
                _informationid=value;
            }
        }

        #endregion
        
        [Browsable(false)]
        public bool CenterAlign
        {
            get
            {
                return _bcenteralign;
            }
            set
            {
                _bcenteralign = value;                
            }
        }

        [DisplayText("U8.UAP.Report.ReportName")]
        [LocalizeDescription("U8.UAP.Report.ReportName")]
        public bool CenterAlignDesign
        {
            get
            {
                return CenterAlign;
            }
            set
            {
                CenterAlign = value;
                if (value)
                {
                    base.ClientFont = new Font(new FontFamily("黑体"), 15);
                }
            }
        }
    }
}
