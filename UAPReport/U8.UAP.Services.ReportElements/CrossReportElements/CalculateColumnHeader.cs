using System;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// CalculateColumnHeader 的摘要说明。
	/// </summary>
	[Serializable]
    public class CalculateColumnHeader : Rect, ISort, ICalculateColumn, IMultiHeader, ISerializable, ICloneable, IDisposable, ICalculateSequence, IWithSizable
	{
		#region fields
		protected string _expression="";
		protected SortOption _sortoption ;
		protected Columns _columns=new Columns();
        protected int _calculateindex;
        protected DataSource _sortsource = new DataSource("EmptyColumn");
        protected string _formatstring = "";
		#endregion

		#region constructor
		public CalculateColumnHeader():base()
		{
		}

		public CalculateColumnHeader(int x,int y):base(x,y)
		{
		}

		public CalculateColumnHeader(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public CalculateColumnHeader(string name,string expression):base()
		{
			_name=name;
			Expression=expression;
		}

		public CalculateColumnHeader(int x,int y,string name,string expression):base(x,y)
		{
			_name=name;
			Expression=expression;
		}

		public CalculateColumnHeader(int x,int y,int width,int height,string name,string expression):base(x,y,width,height)
		{
			_name=name;
			Expression=expression;
		}

		public CalculateColumnHeader(CalculateColumnHeader groupobject):base(groupobject)
		{
			Expression=groupobject.Expression;
			_sortoption =groupobject.SortOption ;
            _calculateindex = groupobject.CalculateIndex;
            _sortsource = groupobject.SortSource;
            _formatstring = groupobject.FormatString;
		}

		protected CalculateColumnHeader( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _sortoption = new SortOption();
			_expression=info.GetString("Expression");
			_sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            _calculateindex = info.GetInt32("CalculateIndex");
            _sortsource = (DataSource)info.GetValue("SortSource", typeof(DataSource));
            _formatstring = info.GetString("FormatString");
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
        }

        public CalculateColumnHeader(IDateTimeDimensionLevel groupobject,string accid)
            : base(groupobject as Rect)
        {
            ColumnHeader ch = groupobject as ColumnHeader;
            _name = ch.Name + "_Dimension";
            _scriptid = ch.ScriptID;
            _sortoption  = ch.SortOption;
            _sortsource = ch.SortSource;
            _formatstring = ch.FormatString;
            if (string.IsNullOrEmpty(_prepaintevent))
                Expression = DateTimeDimensionHelper.GetExpressionAll(groupobject.DDLevel,groupobject.ShowYear ,groupobject.ShowWeekRange, ch.DataSource.Name,accid);
            else
                Expression = DateTimeDimensionHelper.GetExpressionOnly(groupobject.DDLevel,groupobject.ShowYear , ch.DataSource.Name,accid);            
        }

		#endregion

		#region override
		public override void SetType()
		{
			_type="CalculateColumnHeader";
		}

        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = System.Drawing.ContentAlignment.MiddleCenter;
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        [Browsable(false )]
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
        public override System.Drawing.Color BorderColor
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
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }
		#endregion

		#region property	
        [Browsable(false)]
        public override bool bControlAuth
        {
            get
            {
                return _bcontrolauth;
            }
            set
            {
                _bcontrolauth = value;
            }
        }
		#endregion

		#region ISort 成员

        [Browsable(true)]
        [DisplayText("U8.Report.SortOption")]
        [LocalizeDescription("U8.Report.SortOption")]
        public SortOption SortOption
        {
            get
            {
                if (_sortoption == null)
                    _sortoption = new SortOption();
                return _sortoption;
            }
            set
            {
                _sortoption = value;
            }
        }

		#endregion

		#region ICalculateColumn 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis14")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis14")]
		[Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string Expression
		{
			get
			{
				return _expression;
			}
			set
			{
				_expression=value;
			}
		}
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Expression",_expression);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
            info.AddValue("CalculateIndex", _calculateindex);
            info.AddValue("SortSource", _sortsource);
            info.AddValue("FormatString", _formatstring);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new CalculateColumnHeader(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _sortsource = null;
            _columns.Clear();
            _columns = null;
            _sortoption = null;
            base.Dispose();
		}

		#endregion

		#region IMultiHeader 成员

		[Browsable(false)]
		public Columns Columns
		{
			get
			{
				return _columns;
			}
		}

        [DisplayText("排序关键字")]
        [LocalizeDescription("排序关键字")]
        public DataSource SortSource
        {
            get
            {
                return _sortsource;
            }
            set
            {
                _sortsource = value;
            }
        }
		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _name;
			}
		}
		#endregion

        #region IFormat 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis22")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis22")]
        public string FormatString
        {
            get
            {
                return _formatstring;
            }
            set
            {
                _formatstring = value;
            }
        }

        #endregion

        #region ICalculateSequence 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis18")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis18")]
        public int CalculateIndex
        {
            get
            {
                return _calculateindex;
            }
            set
            {
                _calculateindex = value;
            }
        }

        #endregion
    }
}
