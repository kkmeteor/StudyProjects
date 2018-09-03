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
	public class Label:Rect,ISerializable,ICloneable,IDisposable,ILabelType,IUserDefine 
	{
        protected LabelType _labeltype=LabelType.DetailLabel ;
        protected string _userdefineitem = "";
        protected int _runtimey;
        protected string _designcaption = "";
        protected string _designname = "";

		#region constructor
		public Label():base()
		{
		}
		public Label(int x,int y):base(x,y)
		{
		}

		public Label(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public Label(string caption):base()
		{
			_caption=caption;
		}
		public Label(int x,int y,string caption):base(x,y)
		{
			_caption=caption;
		}

		public Label(int x,int y,int width,int height,string caption):base(x,y,width,height)
		{
			_caption=caption;
		}

		public Label(Label label):base(label)
		{
            _labeltype = label.LabelType;
            _userdefineitem = label.UserDefineItem;
		}

		protected Label( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _labeltype = (LabelType)info.GetValue("LabelType", typeof(LabelType));
            if (_version > 1)
                _userdefineitem = info.GetString("UserDefineItem");
            if (_version >= 90)
            {
                _designcaption = info.GetString("DesignCaption");
                _designname = info.GetString("DesignName");
            }
		}

		public Label(Cell cell):base(cell as Rect )
        {
			SetDefault();
        }

        public void Update(Cell cell)
        {
            _backcolor = cell.BackColor;
            _caption  = cell.Caption.ToString();
            _forecolor  = cell.ForeColor;
        }
		#endregion

		#region override
		public override void SetType()
		{
			_type="Label";				
		}
        
		public override void SetDefault()
		{
			_backcolor=DefaultConfigs.DefaultTitleBackColor;
            _captionalign = ContentAlignment.MiddleCenter;
		}
        
        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis8")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis8")]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public string DesignCaption
        {
            get
            {
                return _designcaption;
            }
            set
            {
                _designcaption = value;
            }
        }

        [Browsable(false)]
        public string DesignName
        {
            get
            {
                return _designname ;
            }
            set
            {
                _designname  = value;
            }
        }

        [Browsable(false)]
        public int AutoHeightY
        {
            get
            {
                return _runtimey;
            }
            set
            {
                _runtimey = value;
            }
        }


        public override bool bUnder(Cell cell)
        {
            return _x >= cell.X && _x + _w <= cell.X + cell.Width;
        }

        public void AdjustRuntimeHeight(int height)
        {
            if (height > this.RuntimeHeight )
                _runtimeheight = height;
        }
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("LabelType", _labeltype);
            info.AddValue("UserDefineItem", _userdefineitem);
            info.AddValue("DesignCaption", _designcaption);
            info.AddValue("DesignName", _designname);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new Label(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            base.Dispose();
            _userdefineitem = null;
		}

		#endregion

        #region ILabelType 成员

        [DisplayText("U8.Report.UserDefineItem")]
        [LocalizeDescription("U8.Report.UserDefineItem")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual string UserDefineItem
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
    }
}
